#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

public enum PondSnakeMode { Rest, Patrol, Pursue, Search, Recover }

public readonly record struct PondSnakeTarget(string Id, string SpeciesId, string Stage,
    float Length, Vector3 Position, float Radius, Rid BodyRid, bool Alive, bool Player);

/// <summary>A single scene-owned shore and shallow-water hunter. Game time comes from PondBassSession.</summary>
public partial class PondSnakeActor : CharacterBody3D
{
    public const string SpeciesId = "FAF-CREATURE-002";
    public const float MaximumPreyLength = .26f;
    private readonly Dictionary<string, WorldSightSensor> _sensors = new(StringComparer.Ordinal);
    private IReadOnlyList<SoftSightCover> _cover = Array.Empty<SoftSightCover>();
    private SeabedSurface _bed = null!;
    private Func<float, float, float> _water = null!;
    private AnimationPlayer _animation = null!;
    private PondCreatureAsset _asset = null!;
    private Vector3[] _route = Array.Empty<Vector3>();
    private int _routeIndex;
    private double _modeAt;
    private double _nextHuntAt = 5;
    private double _lastSeenAt;
    private Vector3 _lastSeen;
    private string? _targetId;
    private float _speed;
    private int _stuckSteps;
    private float _diveUntil;
    private float _lastDepth;

    public string StableId { get; private set; } = "";
    public PondSnakeMode Mode { get; private set; } = PondSnakeMode.Rest;
    public string ClipRole { get; private set; } = "IdleFloat";
    public Vector3 MouthPosition => GlobalPosition - GlobalBasis.Z * .43f + Vector3.Up * .012f;
    public IReadOnlyList<Vector3> Route => _route;
    public string? TargetId => _targetId;
    public int MotorSteps { get; private set; }
    public int SightSamples => _sensors.Values.Sum(sensor => sensor.SampleCount);
    public int Disengagements { get; private set; }
    public float ActualSpeed => new Vector2(Velocity.X, Velocity.Z).Length();
    public int RouteIndex => _routeIndex;
    public int StuckSteps => _stuckSteps;

    public static PondSnakeActor Create(Node3D parent, SeabedSurface bed,
        Func<float, float, float> water, string worldId, Vector3 nursery,
        IReadOnlyList<PondFafPlacement> placements)
    {
        if (!SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.PondId,
                SpeciesId, null, SpeciesRole.Ecological, out var asset, out var diagnostic) ||
            asset is null || !PondCreatureCatalog.ExportMatches(asset) ||
            !new[] { "IdleFloat", "SurfaceSwim", "UnderwaterSwim", "ShoreCrawl",
                "TurnLeft", "TurnRight", "Dive" }.All(role => asset.Actions.Contains(asset.Clip(role))))
            throw new InvalidOperationException("POND_SNAKE_ASSET_INVALID " + diagnostic);
        var scene = ResourceLoader.Load<PackedScene>(asset.ModelPath)
            ?? throw new InvalidOperationException("POND_SNAKE_MODEL_MISSING");
        var rig = scene.Instantiate<Node3D>();
        var animation = rig.FindChildren("*", "AnimationPlayer", true, false)
            .OfType<AnimationPlayer>().FirstOrDefault();
        if (animation is null || new[] { "IdleFloat", "SurfaceSwim", "UnderwaterSwim",
                "ShoreCrawl", "TurnLeft", "TurnRight", "Dive" }
            .Any(role => !animation.HasAnimation(asset.Clip(role))))
        { rig.Free(); throw new InvalidOperationException("POND_SNAKE_CLIP_MISSING"); }
        var snake = new PondSnakeActor { Name = "PondNorthernWaterSnake", _bed = bed,
            _water = water, _asset = asset, _animation = animation,
            StableId = worldId + ":snake:shore:0", CollisionLayer = 2,
            CollisionMask = 1, MotionMode = MotionModeEnum.Floating };
        // The original model's 0.973 m long axis is local -Z in Godot.
        snake.AddChild(new CollisionShape3D { Name = "Collision",
            Rotation = new Vector3(Mathf.Pi * .5f, 0, 0),
            Shape = new CapsuleShape3D { Radius = .055f, Height = .90f } });
        snake.AddChild(rig);
        rig.Name = "Rig";
        // Imported creature faces +Z; the gameplay root and mouth face -Z.
        rig.Rotation = new Vector3(0, Mathf.Pi, 0);
        snake._route = BuildRoute(bed, water, nursery, parent.GetWorld3D().DirectSpaceState);
        snake._cover = PondSoftCover.Near(placements, snake._route[1], 10f)
            .OrderBy(p => p.VisibleBounds.GetCenter().DistanceSquaredTo(snake._route[1]))
            .Take(128).ToArray();
        parent.AddChild(snake);
        snake.GlobalPosition = snake._route[0];
        var forward = snake._route[1] - snake._route[0];
        forward.Y = 0;
        snake.LookAt(snake.GlobalPosition + forward.Normalized(), Vector3.Up);
        snake.SetMeta("faf_id", SpeciesId);
        snake.SetMeta("faf_habitat", WorldCatalog.PondId);
        snake.Play("IdleFloat");
        return snake;
    }

    private static Vector3[] BuildRoute(SeabedSurface bed, Func<float, float, float> water,
        Vector3 nursery, PhysicsDirectSpaceState3D space)
    {
        var basin = bed.Basin;
        foreach (var i in Enumerable.Range(0, basin.ShorePointCount)
            .OrderBy(i => Mathf.Abs(basin.ShorePoint(i).DistanceTo(new Vector2(nursery.X, nursery.Z)) - 3f)))
        {
            var shore = basin.ShorePoint(i);
            var inward = (basin.DeepCore - shore).Normalized();
            var points = new[] { -.30f, .48f, 1.05f, 1.42f }.Select(offset => shore + inward * offset).ToArray();
            if (basin.DistanceToShore(points[0].X, points[0].Y) >= -.08f ||
                basin.DistanceToShore(points[1].X, points[1].Y) <= .1f ||
                basin.DistanceToShore(points[3].X, points[3].Y) > 1.9f ||
                water(points[3].X, points[3].Y) - bed.GetHeightAt(points[3].X, points[3].Y) > .9f)
                continue;
            var route = points.Select((p, index) => new Vector3(p.X,
                index == 0 ? bed.GetHeightAt(p.X, p.Y) + .14f :
                    water(p.X, p.Y) - (index == 3 ? .22f : .035f), p.Y)).ToArray();
            if (ClearRoute(route, bed, water, space)) return route;
        }
        throw new InvalidOperationException("POND_SNAKE_SHORE_ROUTE_UNAVAILABLE");
    }

    private static bool ClearRoute(Vector3[] route, SeabedSurface bed,
        Func<float, float, float> water, PhysicsDirectSpaceState3D space)
    {
        using var shape = new CapsuleShape3D { Radius = .055f, Height = .90f };
        for (var leg = 0; leg < route.Length - 1; leg++)
        {
            var from = route[leg];
            var to = route[leg + 1];
            var direction = new Vector3(to.X - from.X, 0, to.Z - from.Z).Normalized();
            var basis = Basis.LookingAt(direction, Vector3.Up) *
                new Basis(Vector3.Right, Mathf.Pi * .5f);
            var steps = Mathf.CeilToInt(from.DistanceTo(to) / .055f);
            for (var step = 0; step <= steps; step++)
            {
                var point = from.Lerp(to, (float)step / steps);
                var shore = bed.Basin.DistanceToShore(point.X, point.Z);
                point.Y = shore < 0 ? bed.GetHeightAt(point.X, point.Z) + .14f :
                    Mathf.Max(bed.GetHeightAt(point.X, point.Z) + .14f,
                        water(point.X, point.Z) - (leg == 2 ? .22f : .035f));
                using var query = new PhysicsShapeQueryParameters3D {
                    Shape = shape, Transform = new Transform3D(basis, point), CollisionMask = 1 };
                if (space.IntersectShape(query, 1).Count != 0) return false;
            }
        }
        return true;
    }

    public static bool Eligible(PondSnakeTarget target) => target.Alive &&
        target.SpeciesId is PikeEncounterAssets.BassId or PikeEncounterAssets.BluegillId &&
        target.Stage is "larva" or "juvenile" && target.Length > 0 &&
        target.Length <= MaximumPreyLength;

    public void Tick(IReadOnlyList<PondSnakeTarget> candidates, SightEnvironment environment,
        double seconds, float delta, Func<PondSnakeTarget, Vector3, bool> commit)
    {
        if (delta <= 0 || GetTree().Paused) return;
        PondSnakeTarget? seen = null;
        if (seconds >= _nextHuntAt && Mode != PondSnakeMode.Recover)
        {
            foreach (var target in candidates.Where(Eligible)
                .OrderBy(t => t.Position.DistanceSquaredTo(GlobalPosition)))
            {
                if (target.Position.DistanceTo(GlobalPosition) > 2.35f ||
                    !ReachableHabitat(target.Position)) continue;
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
            if (_targetId != visible.Id)
            { _targetId = visible.Id; Enter(PondSnakeMode.Pursue, seconds); }
        }
        else if (Mode == PondSnakeMode.Pursue && seconds - _lastSeenAt > .55)
            Enter(PondSnakeMode.Search, seconds);
        if (Mode is PondSnakeMode.Pursue or PondSnakeMode.Search &&
            seconds - _modeAt > (Mode == PondSnakeMode.Pursue ? 4.2 : 1.2))
            Disengage(seconds);
        if (Mode == PondSnakeMode.Recover && seconds - _modeAt > 3)
            Enter(PondSnakeMode.Patrol, seconds);
        if (Mode == PondSnakeMode.Rest && seconds - _modeAt > 2.5)
            Enter(PondSnakeMode.Patrol, seconds);

        var goal = Mode is PondSnakeMode.Pursue or PondSnakeMode.Search ? _lastSeen : _route[_routeIndex];
        if (Mode == PondSnakeMode.Patrol &&
            new Vector2(goal.X - GlobalPosition.X, goal.Z - GlobalPosition.Z).Length() < .24f &&
            Mathf.Abs(goal.Y - GlobalPosition.Y) < .12f)
        {
            var reached = _routeIndex;
            _routeIndex = (_routeIndex + 1) % _route.Length;
            if (reached == 1) Enter(PondSnakeMode.Rest, seconds);
        }
        if (Mode == PondSnakeMode.Patrol) goal = _route[_routeIndex];
        MoveToward(goal, delta);
        if (Mode == PondSnakeMode.Pursue && seen is { } prey &&
            seconds - _modeAt >= .32 && CanContact(prey) && commit(prey, MouthPosition))
        { _targetId = null; _nextHuntAt = seconds + 9; Enter(PondSnakeMode.Recover, seconds); }
    }

    private bool ReachableHabitat(Vector3 point)
    {
        var shore = _bed.Basin.DistanceToShore(point.X, point.Z);
        var surface = _water(point.X, point.Z);
        return shore is >= -.42f and <= 2.1f &&
            surface - point.Y is >= -.08f and <= .48f &&
            point.Y - _bed.GetHeightAt(point.X, point.Z) >= .04f;
    }

    private SightEvidence Observe(PondSnakeTarget target, SightEnvironment environment, double seconds)
    {
        if (!_sensors.TryGetValue(target.Id, out var sensor))
            _sensors[target.Id] = sensor = new WorldSightSensor(GetWorld3D().DirectSpaceState,
                _cover, GetRid(), target.BodyRid, 2.4f, 2.2f);
        return sensor.Observe(MouthPosition, -GlobalBasis.Z,
            new SightTarget(target.Position, target.Radius, 1f), environment, seconds, false);
    }

    public bool CanContact(PondSnakeTarget target)
    {
        if (Mode != PondSnakeMode.Pursue || _targetId != target.Id || !Eligible(target) ||
            !ReachableHabitat(target.Position) ||
            MouthPosition.DistanceTo(target.Position) > target.Radius + .075f) return false;
        var toward = target.Position - GlobalPosition;
        if (toward.LengthSquared() < .000001f ||
            (-GlobalBasis.Z).Dot(toward.Normalized()) < .72f) return false;
        var exclude = new Godot.Collections.Array<Rid> { GetRid(), target.BodyRid };
        using var bodyRay = PhysicsRayQueryParameters3D.Create(GlobalPosition, MouthPosition, 1, exclude);
        if (GetWorld3D().DirectSpaceState.IntersectRay(bodyRay).Count != 0) return false;
        using var ray = PhysicsRayQueryParameters3D.Create(MouthPosition, target.Position, 1, exclude);
        return GetWorld3D().DirectSpaceState.IntersectRay(ray).Count == 0;
    }

    private void MoveToward(Vector3 goal, float delta)
    {
        var to = goal - GlobalPosition;
        var flat = new Vector3(to.X, 0, to.Z);
        var distance = flat.Length();
        var direction = distance > .001f ? flat / distance : -GlobalBasis.Z;
        var desired = Mode switch
        {
            PondSnakeMode.Pursue => Mathf.Clamp((distance - .35f) * 1.5f, 0f, .82f),
            PondSnakeMode.Search => .28f,
            PondSnakeMode.Patrol => .32f,
            PondSnakeMode.Recover => .18f,
            _ => 0f,
        };
        // Both steering probes and MoveAndSlide use the physical capsule, so wood can halt a pursuit.
        var probe = GlobalPosition + direction * Mathf.Min(.16f, distance);
        if (desired > 0 && !PathBounded(probe))
        {
            var left = direction.Rotated(Vector3.Up, .7f);
            var right = direction.Rotated(Vector3.Up, -.7f);
            if (PathBounded(GlobalPosition + left * .14f)) direction = left;
            else if (PathBounded(GlobalPosition + right * .14f)) direction = right;
            else { desired = 0; _stuckSteps++; }
        }
        else _stuckSteps = 0;
        if (_stuckSteps > 90 && Mode is PondSnakeMode.Pursue or PondSnakeMode.Search)
            desired = 0;
        var oldForward = -GlobalBasis.Z;
        if (desired > 0)
        {
            var current = GlobalBasis.GetRotationQuaternion();
            var target = Basis.LookingAt(direction, Vector3.Up).GetRotationQuaternion();
            var angle = current.AngleTo(target);
            GlobalBasis = new Basis(current.Slerp(target,
                angle < .0001f ? 1f : Mathf.Min(1f, Mathf.DegToRad(125f) * delta / angle)));
        }
        _speed = Mathf.MoveToward(_speed, desired, 1.1f * delta);
        var surface = _water(GlobalPosition.X, GlobalPosition.Z);
        var shore = _bed.Basin.DistanceToShore(GlobalPosition.X, GlobalPosition.Z);
        var floor = _bed.GetHeightAt(GlobalPosition.X, GlobalPosition.Z);
        var requestedDepth = shore < 0 ? 0f :
            Mode == PondSnakeMode.Patrol && _routeIndex == 3 ? .27f :
            Mathf.Clamp(surface - goal.Y, .035f, .32f);
        var desiredY = shore < 0 ? floor + .14f : Mathf.Max(floor + .14f, surface - requestedDepth);
        Velocity = -GlobalBasis.Z * _speed +
            Vector3.Up * Mathf.Clamp((desiredY - GlobalPosition.Y) * 14f, -2f, 2f);
        MoveAndSlide();
        MotorSteps++;
        var depth = _water(GlobalPosition.X, GlobalPosition.Z) - GlobalPosition.Y;
        if (_lastDepth <= .14f && depth > .14f) _diveUntil = .32f;
        _lastDepth = depth;
        _diveUntil = Mathf.Max(0, _diveUntil - delta);
        var turn = oldForward.Cross(-GlobalBasis.Z).Y;
        var clip = _diveUntil > 0 ? "Dive" :
            Mathf.Abs(turn) > .025f && _speed > .05f ? (turn > 0 ? "TurnRight" : "TurnLeft") :
            shore < 0 ? "ShoreCrawl" : depth > .14f ? "UnderwaterSwim" :
            _speed < .04f ? "IdleFloat" : "SurfaceSwim";
        Play(clip);
    }

    private bool PathBounded(Vector3 point)
    {
        var shore = _bed.Basin.DistanceToShore(point.X, point.Z);
        var depth = _water(point.X, point.Z) - _bed.GetHeightAt(point.X, point.Z);
        return shore is >= -.75f and <= 2.3f && (shore < 0 || depth <= 1.0f) &&
            !TestMove(GlobalTransform, point - GlobalPosition);
    }

    private void Disengage(double seconds)
    { Disengagements++; _targetId = null; _nextHuntAt = seconds + 4; Enter(PondSnakeMode.Recover, seconds); }

    private void Enter(PondSnakeMode mode, double seconds)
    { Mode = mode; _modeAt = seconds; }

    private void Play(string role)
    {
        if (ClipRole == role && _animation.IsPlaying()) return;
        ClipRole = role;
        _animation.Play(_asset.Clip(role), .15);
    }
}
