#nullable enable
using Godot;
using System;
using System.Collections.Generic;

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
    public string TargetId { get; private set; } = "";
    public bool TargetIsBass { get; private set; } = true;

    public void Retarget(string id, bool bass)
    {
        if (TargetId == id) return;
        Reset();
        TargetId = id;
        TargetIsBass = bass;
    }

    public bool ConsumeEscape()
    {
        if (!Escaped) return false;
        Escaped = false;
        return true;
    }

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
        TargetId = "";
        TargetIsBass = true;
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
                Enter(pikePosition.DistanceTo(seen) <= 3.4f && seconds >= _nextCommitAt
                    ? PikeMode.Commit : PikeMode.Chase, seconds);
            if (Mode == PikeMode.Commit)
            {
                if (pikePosition.DistanceTo(seen) > 5.0f) Enter(PikeMode.Chase, seconds);
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
                    Escaped = _confirmed && TargetIsBass;
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
        // Sweep the connected nursery, refuge and deep cover, independent of the player.
        var phase = (float)(seconds * .10);
        return origin + new Vector3(Mathf.Sin(phase) * 14f,
            Mathf.Sin(phase * .7f) * .5f, -Mathf.Cos(phase) * 18f);
    }

    public float DesiredSpeed => Mode switch
    {
        PikeMode.Chase => 5.2f,
        PikeMode.Commit => StrikeReady ? 6.0f : .8f,
        PikeMode.Search => 2.1f,
        PikeMode.Investigate => 2.4f,
        PikeMode.Suspicious => 1.2f,
        PikeMode.Caught => 0f,
        _ => 2.2f,
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

/// <summary>One living fish, with evidence made by that fish's own sensors.</summary>
public readonly record struct PikeCandidate(string Id, bool IsBass, bool Alive, float Length,
    SightEvidence Sight, DisturbanceEvidence Disturbance);

/// <summary>Bounded choice over actual fish; no position enters from an unsensed transform.</summary>
public sealed class PikeTargetSelector
{
    public const int MaxCandidates = 17;
    private string _targetId = "";
    private double _lastSwitchAt = double.NegativeInfinity;
    private readonly Dictionary<string, (Vector3 Position, double Time)> _memory = new();
    private float _maxEdibleLength = 1.3f;
    public string TargetId => _targetId;
    public int Evaluated { get; private set; }

    public void SetPhysicalEdibility(float pikeColliderLength)
    {
        if (!float.IsFinite(pikeColliderLength) || pikeColliderLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(pikeColliderLength));
        _maxEdibleLength = pikeColliderLength * .5f;
    }

    public void Reset()
    {
        _targetId = "";
        _lastSwitchAt = double.NegativeInfinity;
        Evaluated = 0;
        _memory.Clear();
    }

    public Vector3? LastKnown(string id, double seconds) =>
        _memory.TryGetValue(id, out var seen) && seconds - seen.Time is >= 0 and <= 7.0
            ? seen.Position : null;

    public PikeCandidate? Select(IReadOnlyList<PikeCandidate> candidates, Vector3 pike,
        double seconds, bool paused)
    {
        if (paused || !double.IsFinite(seconds)) return null;
        if (candidates.Count > MaxCandidates) throw new ArgumentOutOfRangeException(nameof(candidates));
        Evaluated = 0;
        PikeCandidate? current = null, best = null;
        var currentScore = 0f;
        var bestScore = 0f;
        foreach (var candidate in candidates)
        {
            Evaluated++;
            if (!candidate.Alive) _memory.Remove(candidate.Id);
            else if (candidate.Sight.Detected && candidate.Sight.SensedPosition is { } seen &&
                Finite(seen) && seconds - candidate.Sight.SensedAt is >= 0 and <= .26)
                _memory[candidate.Id] = (seen, candidate.Sight.SensedAt);
            else if (candidate.Disturbance.Position is { } cue && Finite(cue) &&
                candidate.Disturbance.Strength >= .035f &&
                seconds - candidate.Disturbance.SensedAt is >= 0 and <= MovementDisturbanceField.LifetimeSeconds)
                _memory[candidate.Id] = (cue, candidate.Disturbance.SensedAt);
            if (candidate.Id == _targetId && candidate.Alive) current = candidate;
            var score = Score(candidate, pike, seconds, _maxEdibleLength);
            if (candidate.Id == _targetId) currentScore = score;
            if (score <= bestScore) continue;
            best = candidate;
            bestScore = score;
        }
        if (current is null) _targetId = "";
        if (best is { } opportunity && opportunity.Id != _targetId &&
            seconds - _lastSwitchAt >= 1.2 &&
            bestScore > currentScore * 1.45f + .055f)
        {
            _targetId = opportunity.Id;
            _lastSwitchAt = seconds;
            return opportunity;
        }
        return current;
    }

    public void Remove(string id)
    {
        if (_targetId == id) _targetId = "";
        _memory.Remove(id);
    }

    private static float Score(PikeCandidate c, Vector3 pike, double seconds,
        float maxEdibleLength)
    {
        if (!c.Alive || string.IsNullOrWhiteSpace(c.Id) || !float.IsFinite(c.Length) ||
            c.Length <= 0 || c.Length > maxEdibleLength) return 0f;
        var score = 0f;
        if (c.Sight.Detected && c.Sight.SensedPosition is { } seen && Finite(seen) &&
            double.IsFinite(c.Sight.SensedAt) && seconds - c.Sight.SensedAt is >= 0 and <= .26)
            score = c.Sight.Strength + .24f / (1f + pike.DistanceTo(seen) * .2f);
        if (c.Disturbance.Position is { } cue && Finite(cue) &&
            c.Disturbance.Strength >= .035f && double.IsFinite(c.Disturbance.SensedAt) &&
            seconds - c.Disturbance.SensedAt is >= 0 and <= MovementDisturbanceField.LifetimeSeconds)
            score = Mathf.Max(score, c.Disturbance.Strength * .4f);
        return score;
    }

    private static bool Finite(Vector3 v) => float.IsFinite(v.X) &&
        float.IsFinite(v.Y) && float.IsFinite(v.Z);
}

/// <summary>Credits a Bass diversion only after the Pike has truly left that pursuit.</summary>
public sealed class PikeBassDiversion
{
    private double _switchedAt;
    public bool Pending { get; private set; }

    public void Reset() => Pending = false;

    public void SwitchedFromBass(double seconds, bool verifiedChase)
    {
        if (!verifiedChase || !double.IsFinite(seconds)) return;
        Pending = true;
        _switchedAt = seconds;
    }

    public bool Step(double seconds, bool bassVisible, bool bassTargeted, bool paused)
    {
        if (!Pending || paused || !double.IsFinite(seconds)) return false;
        if (bassTargeted || (bassVisible && seconds - _switchedAt > .26))
        { Pending = false; return false; }
        if (seconds - _switchedAt < 6.5) return false;
        Pending = false;
        return true;
    }
}
