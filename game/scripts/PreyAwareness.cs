#nullable enable
using Godot;
using System;

namespace Fish;

public enum PreyMode { Forage, Alert, Flee, Settle }

/// <summary>Fixture prey remembers only recent world-space sensory evidence.</summary>
public sealed class PreyAwareness
{
    private const double MemorySeconds = 2.8;
    private Vector3? _threat;
    private double _lastEvidenceAt = double.NegativeInfinity;
    private double _modeAt;

    public PreyMode Mode { get; private set; } = PreyMode.Forage;
    public Vector3? LastSensedPosition => _threat;
    public double LastSensedAt => _lastEvidenceAt;

    public void Reset()
    {
        Mode = PreyMode.Forage;
        _threat = null;
        _lastEvidenceAt = double.NegativeInfinity;
        _modeAt = 0;
    }

    public void Step(SightEvidence sight, DisturbanceEvidence disturbance,
        double seconds, bool paused)
    {
        if (paused || !double.IsFinite(seconds)) return;
        if (sight.Detected && sight.SensedPosition is { } seen && Finite(seen) &&
            double.IsFinite(sight.SensedAt) && sight.SensedAt <= seconds &&
            seconds - sight.SensedAt <= WorldSightSensor.SampleIntervalSeconds + .05)
        {
            _threat = seen;
            _lastEvidenceAt = sight.SensedAt;
            Enter(PreyMode.Flee, seconds);
            return;
        }
        if (disturbance.Strength >= .035f && disturbance.Position is { } cue && Finite(cue) &&
            double.IsFinite(disturbance.SensedAt) && disturbance.SensedAt <= seconds &&
            seconds - disturbance.SensedAt <= MovementDisturbanceField.LifetimeSeconds &&
            disturbance.SensedAt > _lastEvidenceAt)
        {
            _threat = cue;
            _lastEvidenceAt = disturbance.SensedAt;
            if (Mode is PreyMode.Forage or PreyMode.Settle) Enter(PreyMode.Alert, seconds);
        }
        if (_threat is not null && seconds - _lastEvidenceAt > MemorySeconds)
        {
            _threat = null;
            Enter(PreyMode.Settle, seconds);
        }
        else if (Mode == PreyMode.Alert && seconds - _modeAt > .35)
            Enter(PreyMode.Flee, seconds);
        else if (Mode == PreyMode.Settle && seconds - _modeAt > 1.2)
            Enter(PreyMode.Forage, seconds);
    }

    private void Enter(PreyMode mode, double seconds)
    {
        if (Mode == mode) return;
        Mode = mode;
        _modeAt = seconds;
    }

    private static bool Finite(Vector3 p) => float.IsFinite(p.X) && float.IsFinite(p.Y) && float.IsFinite(p.Z);
}
