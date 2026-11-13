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
    private double _nextChoiceAt;
    private float _speed;
    private AnimationPlayer? _animation;
    private string _swim = "";
    private string _fast = "";

    public PreyAwareness Mind => _mind;
    public Vector3 CoverGoal => _coverGoal;
    public float ActualSpeed => Velocity.Length();

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

    public void ResetActor()
    {
        _mind.Reset();
        _speed = 0;
        Velocity = Vector3.Zero;
        _coverGoal = _home;
        _nextChoiceAt = 0;
        if (_animation is not null) _animation.Play(_swim);
    }

    public void Tick(SightEvidence sight, DisturbanceEvidence disturbance,
        double seconds, float delta, bool paused)
    {
        if (paused || delta <= 0) return;
        _mind.Step(sight, disturbance, seconds, false);
        if (_mind.Mode == PreyMode.Flee && _mind.LastSensedPosition is { } threat &&
            (seconds >= _nextChoiceAt || GlobalPosition.DistanceTo(_coverGoal) < .45f))
        {
            _coverGoal = ChooseCover(threat);
            _nextChoiceAt = seconds + 1.2;
        }
        var goal = _mind.Mode is PreyMode.Flee or PreyMode.Alert ? _coverGoal : _home;
        var to = goal - GlobalPosition;
        var desiredSpeed = _mind.Mode switch
        {
            PreyMode.Flee => 2.3f,
            PreyMode.Alert => .6f,
            PreyMode.Settle => .45f,
            _ => GlobalPosition.DistanceTo(_home) > 1.2f ? .45f : 0f,
        };
        if (to.Length() < .35f) desiredSpeed = 0;
        var direct = to.LengthSquared() > .01f ? to.Normalized() : -GlobalBasis.Z;
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
            center.Y = Mathf.Clamp(center.Y, PikeCoverFixture.Bed(center.X, center.Z) + .32f,
                PikeCoverFixture.WaterY - .35f);
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
            PikeCoverFixture.Bed(chosen.X, chosen.Z) + .32f, PikeCoverFixture.WaterY - .35f);
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
            if (next.Y < PikeCoverFixture.Bed(next.X, next.Z) + .25f ||
                next.Y > PikeCoverFixture.WaterY - .25f ||
                TestMove(GlobalTransform, option * .7f)) continue;
            var score = option.Dot(direct) + .25f * option.Dot(-GlobalBasis.Z);
            if (score <= best) continue;
            best = score;
            selected = option;
        }
        return selected;
    }
}
