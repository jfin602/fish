#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

public enum PondFrogMode { Rest, Forage, Notice, Strike, Retreat, Evade }

public readonly record struct PondFrogTarget(string Id, string SpeciesId, string Stage,
    float Length, Vector3 Position, float Radius, Rid BodyRid, bool Alive, bool Player);

/// <summary>One physical shoreline frog. The session supplies game time and its local prey registry.</summary>
public partial class PondFrogActor : CharacterBody3D
{
    public const string SpeciesId = "FAF-CREATURE-001";
    public const float MaximumPreyLength = .075f;
    private readonly Dictionary<string, WorldSightSensor> _sensors = new(StringComparer.Ordinal);
    private IReadOnlyList<SoftSightCover> _cover = Array.Empty<SoftSightCover>();
    private SeabedSurface _bed = null!;
    private Func<float, float, float> _water = null!;
    private AnimationPlayer _animation = null!;
    private PondCreatureAsset _asset = null!;
    private Vector3[] _route = Array.Empty<Vector3>();
    private int _routeIndex;
    private int _routeDirection = 1;
    private double _modeAt;
    private double _restUntil = 4;
    private double _nextHuntAt = 12;
    private double _lastSeenAt;
    private Vector3 _lastSeen;
    private string? _targetId;
    private float _speed;

    public string StableId { get; private set; } = "";
    public PondFrogMode Mode { get; private set; } = PondFrogMode.Rest;
    public string ClipRole { get; private set; } = "IdleShore";
    public Vector3 MouthPosition => GlobalPosition - GlobalBasis.Z * .105f + Vector3.Up * .018f;
    public IReadOnlyList<Vector3> Route => _route;
    public string? TargetId => _targetId;
    public int MotorSteps { get; private set; }
    public int Disengagements { get; private set; }
    public int SightSamples => _sensors.Values.Sum(sensor => sensor.SampleCount);

    public void ForgetTarget(string id, double seconds)
    {
        if (_targetId != id) return;
        _targetId = null;
        _sensors.Remove(id);
        _nextHuntAt = Mathf.Max(_nextHuntAt, seconds + 2);
        Enter(PondFrogMode.Retreat, seconds);
    }

    public static PondFrogActor Create(Node3D parent, SeabedSurface bed,
        Func<float, float, float> water, string worldId, Vector3 nursery,
        IReadOnlyList<PondFafPlacement> placements)
    {
        if (!SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.PondId,
                SpeciesId, null, SpeciesRole.Ecological, out var asset, out var diagnostic) ||
            asset is null || !PondCreatureCatalog.ExportMatches(asset) ||
            !new[] { "IdleShore", "SurfaceFloat", "Swim", "TurnLeft", "TurnRight", "Hop", "Dive" }
                .All(role => asset.Actions.Contains(asset.Clip(role))))
            throw new InvalidOperationException("POND_FROG_ASSET_INVALID " + diagnostic);
        var scene = ResourceLoader.Load<PackedScene>(asset.ModelPath)
            ?? throw new InvalidOperationException("POND_FROG_MODEL_MISSING");
        var rig = scene.Instantiate<Node3D>();
        var animation = rig.FindChildren("*", "AnimationPlayer", true, false)
            .OfType<AnimationPlayer>().FirstOrDefault();
        if (animation is null || new[] { "IdleShore", "SurfaceFloat", "Swim", "TurnLeft",
                "TurnRight", "Hop", "Dive" }.Any(role => !animation.HasAnimation(asset.Clip(role))))
        { rig.Free(); throw new InvalidOperationException("POND_FROG_CLIP_MISSING"); }
        var frog = new PondFrogActor { Name = "PondLeopardFrog", _bed = bed,
            _water = water, _asset = asset, _animation = animation,
            StableId = worldId + ":frog:shore:0", CollisionLayer = 2,
            CollisionMask = 1, MotionMode = MotionModeEnum.Floating };
        // The long axis is local -Z. The collider surrounds the body and grounded feet.
        frog.AddChild(new CollisionShape3D { Name = "Collision",
            Rotation = new Vector3(Mathf.Pi * .5f, 0, 0),
            Shape = new CapsuleShape3D { Radius = .07f, Height = .24f } });
        frog.AddChild(rig);
        rig.Name = "Rig";
        // The imported frog faces +Z while the motor and mouth use local -Z.
        rig.Rotation = new Vector3(0, Mathf.Pi, 0);
        frog._route = BuildRoute(bed, water, nursery);
        frog._cover = PondSoftCover.Near(placements, frog._route[1], 8f)
            .OrderBy(p => p.VisibleBounds.GetCenter().DistanceSquaredTo(frog._route[1]))
            .Take(128).ToArray();
        parent.AddChild(frog);
        frog.GlobalPosition = frog._route[0];
        var forward = frog._route[1] - frog._route[0];
        forward.Y = 0;
        frog.LookAt(frog.GlobalPosition + forward.Normalized(), Vector3.Up);
        frog.SetMeta("faf_id", SpeciesId);
        frog.SetMeta("faf_habitat", WorldCatalog.PondId);
        frog.Play("IdleShore");
        return frog;
    }

    private static Vector3[] BuildRoute(SeabedSurface bed, Func<float, float, float> water, Vector3 nursery)
    {
        var basin = bed.Basin;
        var choices = Enumerable.Range(0, basin.ShorePointCount)
            .OrderBy(i => basin.ShorePoint(i).DistanceSquaredTo(new Vector2(nursery.X, nursery.Z)));
        foreach (var i in choices)
        {
            var shore = basin.ShorePoint(i);
            var inward = (basin.DeepCore - shore).Normalized();
            var points = new[] { -.48f, .27f, .82f }.Select(offset => shore + inward * offset).ToArray();
            var route = points.Select((p, index) => new Vector3(p.X,
                index == 0 ? bed.GetHeightAt(p.X, p.Y) + .08f : water(p.X, p.Y) - .035f,
                p.Y)).ToArray();
            if (basin.DistanceToShore(points[0].X, points[0].Y) > -.2f ||
                basin.DistanceToShore(points[1].X, points[1].Y) < .08f ||
                basin.DistanceToShore(points[2].X, points[2].Y) > 1.25f ||
                water(points[2].X, points[2].Y) - bed.GetHeightAt(points[2].X, points[2].Y) > .6f ||
                route[0].DistanceTo(route[1]) > 1.2f) continue;
            return route;
        }
        throw new InvalidOperationException("POND_FROG_SHORE_ROUTE_UNAVAILABLE");
    }

    public static bool Eligible(PondFrogTarget target) => target.Alive &&
        target.SpeciesId is PikeEncounterAssets.BassId or PikeEncounterAssets.BluegillId &&
        target.Stage == "larva" && target.Length > 0 && target.Length <= MaximumPreyLength;

    public void Tick(IReadOnlyList<PondFrogTarget> candidates, SightEnvironment environment,
        double seconds, float delta, Func<PondFrogTarget, Vector3, bool> commit)
    {
        if (delta <= 0 || GetTree().Paused) return;
        var large = candidates.Where(t => t.Alive && t.Length >= .17f &&
            t.Position.DistanceTo(GlobalPosition) < 1.35f &&
            Observe(t, environment, seconds).Detected)
            .OrderBy(t => t.Position.DistanceSquaredTo(GlobalPosition)).FirstOrDefault();
        if (large.Alive && Mode != PondFrogMode.Evade)
        { Enter(PondFrogMode.Evade, seconds); _targetId = null; }
        else if (Mode == PondFrogMode.Evade && seconds - _modeAt >= 1.2 &&
            (!large.Alive || seconds - _modeAt > 2.6)) Enter(PondFrogMode.Retreat, seconds);

        PondFrogTarget? seen = null;
        if (Mode is not (PondFrogMode.Evade or PondFrogMode.Retreat) && seconds >= _nextHuntAt)
        {
            foreach (var target in candidates.Where(Eligible)
                .OrderBy(t => t.Id == _targetId ? 0 : 1)
                .ThenBy(t => t.Position.DistanceSquaredTo(GlobalPosition)))
            {
                if (!ReachableHabitat(target.Position,
                        _water(target.Position.X, target.Position.Z)) ||
                    target.Position.DistanceTo(GlobalPosition) > 1.25f) continue;
                var sight = Observe(target, environment, seconds);
                if (!sight.Detected || sight.ClearRays == 0) continue;
                seen = target;
                _lastSeen = target.Position;
                _lastSeenAt = seconds;
                break;
            }
        }
        if (seen is { } visible)
        {
            if (_targetId != visible.Id || Mode is PondFrogMode.Rest or PondFrogMode.Forage)
            { _targetId = visible.Id; Enter(PondFrogMode.Notice, seconds); }
            else if (Mode == PondFrogMode.Notice && seconds - _modeAt >= .45)
                Enter(PondFrogMode.Strike, seconds);
        }
        else if (Mode is PondFrogMode.Notice or PondFrogMode.Strike &&
            seconds - _lastSeenAt > .45)
        { Disengagements++; _targetId = null; _nextHuntAt = seconds + 14; Enter(PondFrogMode.Retreat, seconds); }

        if (Mode == PondFrogMode.Strike && seconds - _modeAt > 2.2)
        { Disengagements++; _targetId = null; _nextHuntAt = seconds + 18; Enter(PondFrogMode.Retreat, seconds); }

        if (Mode == PondFrogMode.Rest && seconds >= _restUntil) Enter(PondFrogMode.Forage, seconds);
        if (Mode == PondFrogMode.Retreat && GlobalPosition.DistanceTo(_route[0]) < .18f)
        { _routeIndex = 0; _targetId = null; _restUntil = seconds + 3.5; Enter(PondFrogMode.Rest, seconds); }

        var goal = Mode switch
        {
            PondFrogMode.Strike => _lastSeen,
            PondFrogMode.Evade or PondFrogMode.Retreat => _route[0],
            PondFrogMode.Notice => GlobalPosition,
            PondFrogMode.Rest => _route[_routeIndex],
            _ => _route[_routeIndex],
        };
        if (Mode == PondFrogMode.Forage &&
            new Vector2(GlobalPosition.X - goal.X, GlobalPosition.Z - goal.Z).Length() < .16f)
        {
            if (_routeIndex == 2)
            {
                _routeDirection = -1;
                _routeIndex = 1;
                _restUntil = seconds + 2.2;
                Enter(PondFrogMode.Rest, seconds);
            }
            else
            {
                if (_routeIndex == 0) _routeDirection = 1;
                _routeIndex += _routeDirection;
                goal = _route[_routeIndex];
            }
        }
        MoveToward(goal, seconds, delta);
        if (Mode == PondFrogMode.Strike && seen is { } prey &&
            seconds - _modeAt >= .23 && CanContact(prey) && commit(prey, MouthPosition))
        { _targetId = null; _nextHuntAt = seconds + 20; Enter(PondFrogMode.Retreat, seconds); }
    }

    private bool ReachableHabitat(Vector3 point, float surface) =>
        _bed.Basin.DistanceToShore(point.X, point.Z) is >= .02f and <= 1.45f &&
        surface - point.Y is >= -.02f and <= .28f &&
        point.Y - _bed.GetHeightAt(point.X, point.Z) >= .07f;

    private SightEvidence Observe(PondFrogTarget target, SightEnvironment environment, double seconds)
    {
        if (!_sensors.TryGetValue(target.Id, out var sensor))
            _sensors[target.Id] = sensor = new WorldSightSensor(
                GetWorld3D().DirectSpaceState, _cover, GetRid(), target.BodyRid, 1.35f, 2.7f);
        return sensor.Observe(MouthPosition, -GlobalBasis.Z,
            new SightTarget(target.Position, target.Radius, 1f), environment, seconds, false);
    }

    public bool CanContact(PondFrogTarget target)
    {
        if (Mode != PondFrogMode.Strike || _targetId != target.Id || !Eligible(target) ||
            !ReachableHabitat(target.Position, _water(target.Position.X, target.Position.Z))) return false;
        var mouth = MouthPosition;
        var toward = target.Position - GlobalPosition;
        if (toward.LengthSquared() < .000001f ||
            (-GlobalBasis.Z).Dot(toward.Normalized()) < .7f ||
            mouth.DistanceTo(target.Position) > target.Radius + .085f) return false;
        var exclude = new Godot.Collections.Array<Rid> { GetRid(), target.BodyRid };
        using var bodyRay = PhysicsRayQueryParameters3D.Create(GlobalPosition, mouth, 1, exclude);
        if (GetWorld3D().DirectSpaceState.IntersectRay(bodyRay).Count != 0) return false;
        using var ray = PhysicsRayQueryParameters3D.Create(mouth, target.Position, 1, exclude);
        return GetWorld3D().DirectSpaceState.IntersectRay(ray).Count == 0;
    }

    private void MoveToward(Vector3 goal, double seconds, float delta)
    {
        var to = goal - GlobalPosition;
        var flat = new Vector3(to.X, 0, to.Z);
        var distance = flat.Length();
        var desired = Mode switch
        {
            PondFrogMode.Strike => Mathf.Clamp((distance - .13f) * 2.5f, 0f, .9f),
            PondFrogMode.Evade => .78f,
            PondFrogMode.Retreat => .48f,
            PondFrogMode.Forage => .29f,
            _ => 0f,
        };
        if (distance < .08f) desired = 0;
        var direction = distance > .001f ? flat / distance : -GlobalBasis.Z;
        if (desired > 0 && !PathBounded(GlobalPosition + direction * Mathf.Min(distance, .17f)))
            desired = 0;
        var oldForward = -GlobalBasis.Z;
        if (desired > 0)
        {
            var desiredBasis = Basis.LookingAt(direction, Vector3.Up);
            var current = GlobalBasis.GetRotationQuaternion();
            var target = desiredBasis.GetRotationQuaternion();
            var angle = current.AngleTo(target);
            GlobalBasis = new Basis(current.Slerp(target,
                angle < .0001f ? 1f : Mathf.Min(1f, Mathf.DegToRad(220f) * delta / angle)));
        }
        _speed = Mathf.MoveToward(_speed, desired, 2.2f * delta);
        var surface = _water(GlobalPosition.X, GlobalPosition.Z);
        var shore = _bed.Basin.DistanceToShore(GlobalPosition.X, GlobalPosition.Z);
        var desiredY = shore < 0 ? _bed.GetHeightAt(GlobalPosition.X, GlobalPosition.Z) + .08f : surface - .035f;
        Velocity = -GlobalBasis.Z * _speed + Vector3.Up * Mathf.Clamp((desiredY - GlobalPosition.Y) * 4f, -.55f, .55f);
        MoveAndSlide();
        MotorSteps++;
        var nowShore = _bed.Basin.DistanceToShore(GlobalPosition.X, GlobalPosition.Z) < 0;
        var turn = oldForward.Cross(-GlobalBasis.Z).Y;
        var clip = Mode == PondFrogMode.Notice ? (nowShore ? "IdleShore" : "SurfaceFloat") :
            Mode == PondFrogMode.Rest ? (nowShore ? "IdleShore" : "SurfaceFloat") :
            Mathf.Abs(turn) > .04f ? (turn > 0 ? "TurnRight" : "TurnLeft") :
            nowShore ? "Hop" : shore < .22f && Mode == PondFrogMode.Evade ? "Dive" : "Swim";
        Play(clip);
    }

    private bool PathBounded(Vector3 point)
    {
        var shore = _bed.Basin.DistanceToShore(point.X, point.Z);
        var depth = _water(point.X, point.Z) - _bed.GetHeightAt(point.X, point.Z);
        return shore is >= -.85f and <= 1.5f &&
            (shore < 0 || depth <= .65f) &&
            !TestMove(GlobalTransform, point - GlobalPosition);
    }

    private void Play(string role)
    {
        if (ClipRole == role && _animation.IsPlaying()) return;
        ClipRole = role;
        _animation.Play(_asset.Clip(role), .12);
    }

    private void Enter(PondFrogMode mode, double seconds)
    { Mode = mode; _modeAt = seconds; }
}
