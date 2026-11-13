#nullable enable
using Godot;
using System;

namespace Fish;

public enum PikeMode { Patrol, Suspicious, Investigate, Chase, Commit, Search, Disengage, Caught }

/// <summary>Fixture-local decisions. Positions enter only through confirmed sight or spatial cues.</summary>
public sealed class PikeAwareness
{
    private const double SearchSeconds = 6.5;
    private const double CueMemorySeconds = 3.0;
    private Vector3? _memory;
    private double _memoryAt;
    private double _lastSightAt = double.NegativeInfinity;
    private double _modeAt;
    private double _lastCueAt = double.NegativeInfinity;
    private double _nextCommitAt;
    private bool _confirmed;
    private int _searchIndex;

    public PikeMode Mode { get; private set; } = PikeMode.Patrol;
    public Vector3? LastSensedPosition => _memory;
    public double LastSensedAt => _memoryAt;
    public bool HadVerifiedChase => _confirmed;
    public bool StrikeReady { get; private set; }
    public bool Escaped { get; private set; }

    public void Reset()
    {
        _memory = null;
        _memoryAt = 0;
        _lastSightAt = double.NegativeInfinity;
        _lastCueAt = double.NegativeInfinity;
        _nextCommitAt = 0;
        _modeAt = 0;
        _confirmed = false;
        _searchIndex = 0;
        StrikeReady = false;
        Escaped = false;
        Mode = PikeMode.Patrol;
    }

    public void Step(Vector3 pikePosition, SightEvidence sight,
        DisturbanceEvidence disturbance, double seconds, bool paused)
    {
        if (paused || Mode == PikeMode.Caught || !double.IsFinite(seconds)) return;
        StrikeReady = false;
        if (sight.Detected && sight.SensedPosition is { } seen && Finite(seen) &&
            double.IsFinite(sight.SensedAt) && sight.SensedAt <= seconds &&
            seconds - sight.SensedAt <= WorldSightSensor.SampleIntervalSeconds + .05)
        {
            _memory = seen;
            _memoryAt = sight.SensedAt;
            _lastSightAt = sight.SensedAt;
            _confirmed = true;
            if (Mode == PikeMode.Commit && seconds - _modeAt > 1.0)
            {
                _nextCommitAt = seconds + 1.0;
                Enter(PikeMode.Chase, seconds);
            }
            if (Mode != PikeMode.Commit)
                Enter(pikePosition.DistanceTo(seen) <= 1.7f && seconds >= _nextCommitAt
                    ? PikeMode.Commit : PikeMode.Chase, seconds);
            if (Mode == PikeMode.Commit)
            {
                if (pikePosition.DistanceTo(seen) > 3.2f) Enter(PikeMode.Chase, seconds);
                else StrikeReady = seconds - _modeAt >= .55;
            }
            return;
        }
        // A cue carries its own source coordinate. Do not substitute the fish's current transform.
        if (disturbance.Position is { } cue && Finite(cue) &&
            disturbance.Strength >= .035f && disturbance.SensedAt > _lastCueAt)
        {
            _lastCueAt = disturbance.SensedAt;
            _memory = cue;
            _memoryAt = seconds;
            if (Mode is PikeMode.Patrol or PikeMode.Disengage or PikeMode.Suspicious)
                Enter(PikeMode.Suspicious, seconds);
        }
        switch (Mode)
        {
            case PikeMode.Patrol:
                if (_memory is not null) Enter(PikeMode.Suspicious, seconds);
                break;
            case PikeMode.Suspicious:
                if (seconds - _memoryAt > CueMemorySeconds) Enter(PikeMode.Disengage, seconds);
                else if (seconds - _modeAt >= .35) Enter(PikeMode.Investigate, seconds);
                break;
            case PikeMode.Investigate:
                if (seconds - _memoryAt > CueMemorySeconds ||
                    (_memory is { } investigation && pikePosition.DistanceTo(investigation) < 1.1f))
                    Enter(PikeMode.Disengage, seconds);
                break;
            case PikeMode.Chase:
            case PikeMode.Commit:
                if (seconds - _lastSightAt > .32) Enter(PikeMode.Search, seconds);
                break;
            case PikeMode.Search:
                if (seconds - _lastSightAt > SearchSeconds)
                {
                    Escaped = _confirmed;
                    Enter(PikeMode.Disengage, seconds);
                }
                else if (_memory is { } last && pikePosition.DistanceTo(SearchPoint(last)) < 1.2f)
                    _searchIndex = Math.Min(_searchIndex + 1, 4);
                break;
            case PikeMode.Disengage:
                if (seconds - _modeAt > 1.2)
                {
                    _memory = null;
                    Enter(PikeMode.Patrol, seconds);
                }
                break;
        }
    }

    public Vector3 Destination(double seconds, Vector3 origin)
    {
        if (_memory is { } last)
        {
            if (Mode == PikeMode.Search) return SearchPoint(last);
            if (Mode is PikeMode.Suspicious or PikeMode.Investigate or PikeMode.Chase or PikeMode.Commit)
                return last;
        }
        // Small ordinary patrol near the starting corridor, independent of the player.
        var phase = (float)(seconds * .22);
        return origin + new Vector3(Mathf.Sin(phase) * 3f, Mathf.Sin(phase * .7f) * .35f,
            Mathf.Cos(phase) * 3f);
    }

    public float DesiredSpeed => Mode switch
    {
        PikeMode.Chase => 5.2f,
        PikeMode.Commit => StrikeReady ? 6.0f : .8f,
        PikeMode.Search => 2.1f,
        PikeMode.Investigate => 2.4f,
        PikeMode.Suspicious => 1.2f,
        PikeMode.Caught => 0f,
        _ => 1.1f,
    };

    public void Catch(double seconds)
    {
        if (Mode != PikeMode.Commit || !StrikeReady) return;
        Enter(PikeMode.Caught, seconds);
    }

    private Vector3 SearchPoint(Vector3 last) => _searchIndex switch
    {
        0 => last,
        1 => last + new Vector3(2.5f, 0, 0),
        2 => last + new Vector3(0, 1.7f, -2.5f),
        3 => last + new Vector3(-2.5f, 0, 0),
        _ => last + new Vector3(0, -.8f, 2.5f),
    };

    private void Enter(PikeMode mode, double seconds)
    {
        if (Mode == mode) return;
        Mode = mode;
        _modeAt = seconds;
        if (mode == PikeMode.Search) _searchIndex = 0;
    }

    private static bool Finite(Vector3 v) => float.IsFinite(v.X) &&
        float.IsFinite(v.Y) && float.IsFinite(v.Z);
}
