#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

/// <summary>Registered Bass sizes and mouth capacity, shared by player and solitary NPCs.</summary>
public static class BassPredation
{
    public const float MaximumPreyLengthRatio = .38f;

    public static bool Eligible(string predatorSpecies, string predatorStage, float predatorLength,
        string preySpecies, string preyStage, float preyLength)
    {
        var predator = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, predatorStage);
        var prey = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, preyStage);
        return predatorSpecies == PikeEncounterAssets.BassId &&
            preySpecies == PikeEncounterAssets.BassId &&
            predatorStage is "subadult" or "adult" && preyStage is "larva" or "juvenile" &&
            predator is not null && prey is not null &&
            float.IsFinite(predatorLength) && float.IsFinite(preyLength) &&
            Mathf.IsEqualApprox(predatorLength, predator.Dimensions[1]) &&
            Mathf.IsEqualApprox(preyLength, prey.Dimensions[1]) &&
            preyLength <= predatorLength * MaximumPreyLengthRatio;
    }
}

/// <summary>One non-schooling Bass with its own growth profile, feeding ledger and finite hunt memory.</summary>
public partial class PondBassRival : PreyActor
{
    private readonly Dictionary<string, WorldSightSensor> _sensors = new(StringComparer.Ordinal);
    private IReadOnlyList<SoftSightCover> _cover = Array.Empty<SoftSightCover>();
    private PhysicsDirectSpaceState3D _space = null!;

    public PondGrowthState Growth { get; private set; } = null!;
    public FeedingLedger Feeding { get; } = new();
    public BluegillThreatMind Hunt { get; } = new();
    public string? TargetId { get; private set; }
    public int SightSamples => _sensors.Values.Sum(sensor => sensor.SampleCount);

    public void ConfigureRival(PondGrowthState growth, IReadOnlyList<SoftSightCover> cover,
        PhysicsDirectSpaceState3D space)
    {
        Growth = growth;
        _cover = cover;
        _space = space;
        SetIdentity(growth.Identity, PondGrowthState.SpeciesId, growth.Stage, growth.Length);
    }

    public SightEvidence Observe(string targetId, Rid targetRid, Vector3 position,
        float radius, SightEnvironment water, double seconds)
    {
        if (!_sensors.TryGetValue(targetId, out var sensor))
        {
            sensor = new WorldSightSensor(_space, _cover, GetRid(), targetRid,
                range: Stage == "adult" ? 6f : 4.8f, acuity: 2.1f);
            _sensors.Add(targetId, sensor);
        }
        return sensor.Observe(GlobalPosition - GlobalBasis.Z * Growth.Body.Radius * .45f,
            -GlobalBasis.Z, new SightTarget(position, radius, 1f), water, seconds, false);
    }

    public void ResetSight()
    {
        foreach (var sensor in _sensors.Values) sensor.Reset();
    }

    public void Track(string? id)
    {
        if (TargetId == id) return;
        TargetId = id;
        Hunt.Reset();
    }

    public new void Consume()
    {
        base.Consume();
        Track(null);
        ResetSight();
    }
}
