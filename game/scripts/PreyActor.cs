#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace Fish;

/// <summary>Small physical Pond prey; chooses reachable visible foliage, not hide flags.</summary>
public partial class PreyActor : CharacterBody3D
{
    private readonly PreyAwareness _mind = new();
    private IReadOnlyList<SoftSightCover> _plants = Array.Empty<SoftSightCover>();
    private Vector3 _home;
    private Vector3 _coverGoal;
    private float _foragePhase;
    private double _nextChoiceAt;
    private float _speed;
    private AnimationPlayer? _animation;
    private string _swim = "";
    private string _fast = "";
    private Func<float, float, float> _bed = PikeCoverFixture.Bed;
    private Func<float, float, float> _water = (_, _) => PikeCoverFixture.WaterY;

    public PreyAwareness Mind => _mind;
    public Vector3 CoverGoal => _coverGoal;
    public float ActualSpeed => Velocity.Length();
    public int MotorStepCount { get; private set; }
    public string StableId { get; private set; } = "";
    public bool IsAlive { get; private set; } = true;
    public string SpeciesId { get; private set; } = "";
    public string Stage { get; private set; } = "";
    public float Length { get; private set; }
    public string SchoolId { get; internal set; } = "";

    public void SetIdentity(string id, string speciesId, string stage, float length)
    {
        StableId = id;
        SpeciesId = speciesId;
        Stage = stage;
        Length = length;
        uint hash = 2166136261;
        foreach (var symbol in id)
            hash = (hash ^ symbol) * 16777619;
        _foragePhase = hash % 6283 / 1000f;
    }

    public void Consume()
    {
        if (!IsAlive) return;
        IsAlive = false;
        Velocity = Vector3.Zero;
        CollisionLayer = 0;
        CollisionMask = 0;
        Visible = false;
    }

    public void Configure(IReadOnlyList<SoftSightCover> plants, AnimationPlayer animation,
        string swim, string fast)
    {
        _plants = plants;
        _animation = animation;
        _swim = swim;
        _fast = fast;
        _home = GlobalPosition;
        ResetActor();
    }

    public void ConfigureHabitat(Func<float, float, float> bed,
        Func<float, float, float> water)
    {
        _bed = bed;
        _water = water;
    }

    public void ResetActor()
    {
        _mind.Reset();
        _speed = 0;
        MotorStepCount = 0;
        Velocity = Vector3.Zero;
        _coverGoal = _home;
        _nextChoiceAt = 0;
        if (_animation is not null) _animation.Play(_swim);
    }

    public void Tick(SightEvidence sight, DisturbanceEvidence disturbance,
        double seconds, float delta, bool paused, SchoolSteering school = default)
    {
        if (!IsAlive || paused || delta <= 0) return;
        _mind.Step(sight, disturbance, seconds, false);
        if (school.Alarm is { } alarm && _mind.Mode is (PreyMode.Forage or PreyMode.Settle))
            _mind.Step(default, new DisturbanceEvidence(.06f, 1, false, 1f,
                alarm, seconds), seconds, false);
        if (_mind.Mode == PreyMode.Flee && _mind.LastSensedPosition is { } threat &&
            (seconds >= _nextChoiceAt || GlobalPosition.DistanceTo(_coverGoal) < .45f))
        {
            _coverGoal = ChooseCover(threat);
            _nextChoiceAt = seconds + 1.2;
        }
        var goal = _mind.Mode switch
        {
            PreyMode.Flee or PreyMode.Alert => _coverGoal,
            PreyMode.Forage => ForagePoint(seconds),
            _ => _home,
        };
        var to = goal - GlobalPosition;
        var sizeSpeed = Stage switch
        {
            "larva" => .34f,
            "juvenile" => .68f,
            "subadult" => 1.05f,
            "adult" => 1.35f,
            _ => .48f,
        };
        var desiredSpeed = _mind.Mode switch
        {
            PreyMode.Flee => sizeSpeed * 1.7f,
            PreyMode.Alert => sizeSpeed * .8f,
            PreyMode.Settle => sizeSpeed * .7f,
            _ => sizeSpeed,
        };
        if (to.Length() < .15f && school.Direction.LengthSquared() < .001f) desiredSpeed = 0;
        var direct = to.LengthSquared() > .01f ? to.Normalized() : -GlobalBasis.Z;
        if (school.Direction.LengthSquared() > .001f && _mind.Mode is (PreyMode.Forage or PreyMode.Settle))
            direct = (direct * .45f + school.Direction * .55f).Normalized();
        var direction = ClearDirection(direct);
        if (direction is null) desiredSpeed = 0;
        else
        {
            var basis = Basis.LookingAt(direction.Value,
                Mathf.Abs(direction.Value.Dot(Vector3.Up)) > .95f ? Vector3.Forward : Vector3.Up);
            var current = GlobalBasis.GetRotationQuaternion();
            var target = basis.GetRotationQuaternion();
            var angle = current.AngleTo(target);
            GlobalBasis = new Basis(current.Slerp(target,
                angle < .0001f ? 1f : Mathf.Min(1f, Mathf.DegToRad(170f) * delta / angle)));
        }
        _speed = Mathf.MoveToward(_speed, desiredSpeed, 5f * delta);
        Velocity = -GlobalBasis.Z * _speed;
        MoveAndSlide();
        MotorStepCount++;
        _speed = Mathf.Min(_speed, Velocity.Length() + .25f);
        if (_animation is not null)
        {
            var clip = _mind.Mode == PreyMode.Flee ? _fast : _swim;
            if (_animation.CurrentAnimation != clip) _animation.Play(clip, .15);
            _animation.SpeedScale = Mathf.Clamp(.8f + ActualSpeed * .2f, .8f, 1.5f);
            var source = _animation.GetAnimation(clip);
            if (source is not null && source.LoopMode == Animation.LoopModeEnum.None &&
                _animation.CurrentAnimationPosition >= source.Length - delta * _animation.SpeedScale)
                _animation.Seek(0, true);
        }
    }

    private Vector3 ForagePoint(double seconds)
    {
        var angle = (float)(seconds * .48) + _foragePhase;
        var target = _home + new Vector3(Mathf.Cos(angle) * .72f,
            Mathf.Sin(angle * .7f) * .12f, Mathf.Sin(angle) * .72f);
        target.Y = Mathf.Clamp(target.Y,
            _bed(target.X, target.Z) + .32f, _water(target.X, target.Z) - .35f);
        return target;
    }

    private Vector3 ChooseCover(Vector3 threat)
    {
        var away = (GlobalPosition - threat).Normalized();
        var best = float.NegativeInfinity;
        var chosen = GlobalPosition + away * 4f;
        foreach (var plant in _plants)
        {
            // Coontail and rooted thicket provide denser small-fish shelter than pondweed.
            var quality = plant.FafId.StartsWith("FAF-PLANT-012", StringComparison.Ordinal) ? 2f :
                plant.FafId.StartsWith("FAF-PLANT-014", StringComparison.Ordinal) ? 1.8f :
                plant.FafId.StartsWith("FAF-PLANT-010", StringComparison.Ordinal) ? 1f : .3f;
            var box = plant.VisibleBounds;
            var center = box.GetCenter();
            center.Y = Mathf.Clamp(center.Y, _bed(center.X, center.Z) + .32f,
                _water(center.X, center.Z) - .35f);
            var delta = center - GlobalPosition;
            var distance = delta.Length();
            if (distance < .5f || distance > 14f ||
                TestMove(GlobalTransform, delta.Normalized() * Mathf.Min(distance, .7f))) continue;
            var transmission = SoftCoverTransmission.Along(threat, center, _plants, .2f);
            var score = quality * 1.3f + (1f - transmission) * 2f +
                away.Dot(delta.Normalized()) * 1.5f - distance * .12f;
            if (score <= best) continue;
            best = score;
            chosen = center;
        }
        chosen.Y = Mathf.Clamp(chosen.Y,
            _bed(chosen.X, chosen.Z) + .32f, _water(chosen.X, chosen.Z) - .35f);
        return chosen;
    }

    private Vector3? ClearDirection(Vector3 direct)
    {
        var side = direct.Cross(Vector3.Up).Normalized();
        if (side.LengthSquared() < .01f) side = Vector3.Right;
        var options = new[] { direct, (direct + side * .7f).Normalized(),
            (direct - side * .7f).Normalized(), (direct + Vector3.Up * .5f).Normalized(),
            (direct - Vector3.Up * .5f).Normalized(), (direct + side * 1.6f).Normalized(),
            (direct - side * 1.6f).Normalized() };
        var best = float.NegativeInfinity;
        Vector3? selected = null;
        foreach (var option in options)
        {
            var next = GlobalPosition + option * .7f;
            if (next.Y < _bed(next.X, next.Z) + .25f ||
                next.Y > _water(next.X, next.Z) - .25f ||
                TestMove(GlobalTransform, option * .7f)) continue;
            var score = option.Dot(direct) + .25f * option.Dot(-GlobalBasis.Z);
            if (score <= best) continue;
            best = score;
            selected = option;
        }
        return selected;
    }
}
