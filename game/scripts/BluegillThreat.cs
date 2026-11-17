#nullable enable
using Godot;
using System;

namespace Fish;

public enum BluegillThreatMode { Patrol, Notice, Chase, Search }

/// <summary>Registered Pond sizes decide whether a Bluegill can eat this Bass.</summary>
public static class BluegillPredation
{
    public const float MaximumPreyLengthRatio = .25f;

    public static bool Eligible(string predatorSpecies, string predatorStage, float predatorLength,
        string preySpecies, string preyStage, float preyLength)
    {
        var adult = PondCreatureCatalog.Stage(PikeEncounterAssets.BluegillId, "adult");
        var bass = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, preyStage);
        return predatorSpecies == PikeEncounterAssets.BluegillId && predatorStage == "adult" &&
            preySpecies == PikeEncounterAssets.BassId && preyStage != "egg" &&
            adult is not null && bass is not null &&
            float.IsFinite(predatorLength) && float.IsFinite(preyLength) &&
            Mathf.IsEqualApprox(predatorLength, adult.Dimensions[1]) &&
            Mathf.IsEqualApprox(preyLength, bass.Dimensions[1]) &&
            preyLength <= predatorLength * MaximumPreyLengthRatio;
    }
}

/// <summary>One Bluegill's memory. It never reads the camera or Pike target store.</summary>
public sealed class BluegillThreatMind
{
    private Vector3? _last;
    private double _seenAt = double.NegativeInfinity;
    private double _evidenceAt = double.NegativeInfinity;
    private double _modeAt;
    private double _lastCueAt = double.NegativeInfinity;
    public BluegillThreatMode Mode { get; private set; }
    public Vector3? Destination => Mode == BluegillThreatMode.Patrol ? null : _last;
    public bool StrikeReady { get; private set; }
    public int Disengagements { get; private set; }
    public double LastSeenAt => _seenAt;

    public void Reset()
    {
        _last = null;
        _seenAt = _evidenceAt = _lastCueAt = double.NegativeInfinity;
        _modeAt = 0;
        Mode = BluegillThreatMode.Patrol;
        StrikeReady = false;
        Disengagements = 0;
    }

    public void Step(SightEvidence sight, DisturbanceEvidence cue, double seconds, bool paused)
    {
        if (paused || !double.IsFinite(seconds)) return;
        StrikeReady = false;
        var seen = sight.Detected && sight.ClearRays > 0 && sight.SensedPosition is { } p &&
            sight.SensedAt <= seconds && seconds - sight.SensedAt <= .26;
        if (seen)
        {
            _last = sight.SensedPosition;
            _seenAt = sight.SensedAt;
            _evidenceAt = seconds;
            if (Mode is BluegillThreatMode.Patrol or BluegillThreatMode.Search)
                Enter(BluegillThreatMode.Notice, seconds);
            else if (Mode == BluegillThreatMode.Notice && seconds - _modeAt >= .45)
                Enter(BluegillThreatMode.Chase, seconds);
            StrikeReady = Mode == BluegillThreatMode.Chase && seconds - _modeAt >= .28;
            return;
        }
        if (cue.Position is { } sensed && cue.Strength >= .035f &&
            cue.SensedAt > _lastCueAt && cue.SensedAt <= seconds &&
            seconds - cue.SensedAt <= MovementDisturbanceField.LifetimeSeconds)
        {
            _last = sensed;
            _lastCueAt = cue.SensedAt;
            _evidenceAt = seconds;
            if (Mode == BluegillThreatMode.Patrol) Enter(BluegillThreatMode.Notice, seconds);
        }
        if (Mode == BluegillThreatMode.Notice && seconds - _modeAt >= .65)
            Enter(BluegillThreatMode.Search, seconds);
        else if (Mode == BluegillThreatMode.Chase && seconds - _seenAt >= .55)
            Enter(BluegillThreatMode.Search, seconds);
        else if (Mode == BluegillThreatMode.Search && seconds - _evidenceAt >= 2.8)
        {
            _last = null;
            Disengagements++;
            Enter(BluegillThreatMode.Patrol, seconds);
        }
    }

    private void Enter(BluegillThreatMode mode, double seconds)
    { Mode = mode; _modeAt = seconds; }
}
