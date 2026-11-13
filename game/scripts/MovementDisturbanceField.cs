#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace Fish;

public readonly record struct MovementCue(Vector3 Position, float Radius, float Strength,
    double CreatedAt);
public readonly record struct DisturbanceEvidence(float Strength, int Events, bool HardCover,
    float SoftCover);

/// <summary>Fixture-local movement evidence. Only completed motor ticks advance its clock.</summary>
public sealed class MovementDisturbanceField
{
    public const int Capacity = 8;
    public const double LifetimeSeconds = 1.5;
    public const double MinimumIntervalSeconds = .25;
    private readonly List<MovementCue> _events = new(Capacity);
    private readonly IReadOnlyList<SoftSightCover> _plants;
    private readonly PhysicsDirectSpaceState3D _space;
    private readonly Godot.Collections.Array<Rid> _exclude;
    private Vector3 _previousVelocity;
    private int _lastMotorStep = -1;
    private double _lastEmission = double.NegativeInfinity;

    public double SimulationSeconds { get; private set; }
    public IReadOnlyList<MovementCue> Events => _events;

    public MovementDisturbanceField(PhysicsDirectSpaceState3D space,
        IReadOnlyList<SoftSightCover> plants, Rid observer, Rid target)
    {
        if (plants.Count > 128) throw new ArgumentOutOfRangeException(nameof(plants));
        _space = space;
        _plants = plants;
        _exclude = new Godot.Collections.Array<Rid> { observer, target };
    }

    public void Reset()
    {
        _events.Clear();
        _previousVelocity = Vector3.Zero;
        _lastMotorStep = -1;
        _lastEmission = double.NegativeInfinity;
        SimulationSeconds = 0;
    }

    public void Step(Vector3 position, Vector3 velocity, float effort, float turn,
        double delta, int motorStep, bool paused)
    {
        if (paused || motorStep <= _lastMotorStep || !double.IsFinite(delta) || delta <= 0 ||
            !Finite(position) || !Finite(velocity)) return;
        var acceleration = motorStep == _lastMotorStep + 1
            ? Mathf.Max(0f, (velocity.Length() - _previousVelocity.Length()) / (float)delta)
            : 0f;
        _lastMotorStep = motorStep;
        _previousVelocity = velocity;
        SimulationSeconds += delta;
        _events.RemoveAll(cue => SimulationSeconds - cue.CreatedAt >= LifetimeSeconds);
        var speed = velocity.Length();
        var energy = Energy(speed, acceleration, effort, turn);
        if (energy < .055f || SimulationSeconds - _lastEmission < MinimumIntervalSeconds) return;
        _lastEmission = SimulationSeconds;
        if (_events.Count == Capacity) _events.RemoveAt(0);
        _events.Add(new MovementCue(position, Mathf.Clamp(.65f + energy * 7f, .65f, 8f),
            energy, SimulationSeconds));
    }

    public static float Energy(float speed, float acceleration, float effort, float turn)
    {
        speed = Mathf.Clamp(float.IsFinite(speed) ? speed / 6f : 0f, 0f, 1f);
        acceleration = Mathf.Clamp(float.IsFinite(acceleration) ? acceleration / 5f : 0f, 0f, 1f);
        effort = Mathf.Clamp(float.IsFinite(effort) ? effort : 0f, 0f, 1f);
        turn = Mathf.Clamp(float.IsFinite(turn) ? Mathf.Abs(turn) : 0f, 0f, 1f);
        return Mathf.Clamp(speed * (.12f + .42f * effort + .22f * acceleration + .24f * turn), 0f, 1f);
    }

    public DisturbanceEvidence Sense(Vector3 observer, bool paused)
    {
        if (paused || !Finite(observer)) return default;
        var strongest = 0f;
        var coverAtStrongest = 1f;
        var hardAtStrongest = false;
        foreach (var cue in _events)
        {
            var distance = observer.DistanceTo(cue.Position);
            if (distance > cue.Radius) continue;
            var age = Mathf.Clamp(1f - (float)((SimulationSeconds - cue.CreatedAt) / LifetimeSeconds), 0f, 1f);
            var falloff = Mathf.Clamp(1f - distance / cue.Radius, 0f, 1f);
            var soft = SoftCoverTransmission.Along(observer, cue.Position, _plants, .35f);
            var blocked = _space.IntersectRay(PhysicsRayQueryParameters3D.Create(
                observer, cue.Position, 1, _exclude)).Count > 0;
            // Wood damps a disturbance but cannot erase a recently sensed location.
            var strength = cue.Strength * age * falloff * soft * (blocked ? .4f : 1f);
            if (strength <= strongest) continue;
            strongest = strength;
            coverAtStrongest = soft;
            hardAtStrongest = blocked;
        }
        return new DisturbanceEvidence(strongest, _events.Count, hardAtStrongest, coverAtStrongest);
    }

    private static bool Finite(Vector3 value) => float.IsFinite(value.X) &&
        float.IsFinite(value.Y) && float.IsFinite(value.Z);
}
