#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace Fish;

/// <summary>One fixture fish identity with nutrition-gated, registered swimming stages.</summary>
public sealed class PondGrowthState
{
    public const string SpeciesId = PikeEncounterAssets.BluegillId;
    public const float AdultColliderRadius = .48f;
    public static readonly IReadOnlyDictionary<string, int> Thresholds =
        new Dictionary<string, int> { ["juvenile"] = 0, ["subadult"] = 3, ["adult"] = 11 };
    private static readonly string[] Stages = { "juvenile", "subadult", "adult" };
    private readonly PondCreatureAsset[] _assets = new PondCreatureAsset[3];
    private int _index;

    public string Stage => Stages[_index];
    public PondCreatureAsset Asset => _assets[_index];
    public float Length => Asset.Dimensions[1];
    public float ColliderRadius => RadiusFor(_index);
    public string Identity { get; }
    public int NextThreshold => _index + 1 < Stages.Length ? Thresholds[Stages[_index + 1]] : int.MaxValue;

    public PondGrowthState(string identity)
    {
        if (string.IsNullOrWhiteSpace(identity)) throw new ArgumentException("identity required", nameof(identity));
        Identity = identity;
        for (var i = 0; i < Stages.Length; i++)
        {
            if (!SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.PondId, SpeciesId,
                    Stages[i], SpeciesRole.Ecological, out var asset, out var diagnostic) || asset is null)
                throw new InvalidOperationException("BLUEGILL_GROWTH_STAGE_UNAVAILABLE " + diagnostic);
            _assets[i] = asset;
            if (i > 0 && _assets[i].Dimensions[1] <= _assets[i - 1].Dimensions[1])
                throw new InvalidOperationException("BLUEGILL_GROWTH_SIZE_ORDER_INVALID");
        }
    }

    public PondCreatureAsset? Next(int nutrition) => nutrition >= NextThreshold && _index + 1 < Stages.Length
        ? _assets[_index + 1] : null;

    public float RadiusForNext() => RadiusFor(_index + 1);
    private float RadiusFor(int index) => AdultColliderRadius *
        _assets[index].Dimensions[1] / _assets[^1].Dimensions[1];

    public void Commit(PondCreatureAsset next)
    {
        if (_index + 1 >= Stages.Length || !ReferenceEquals(_assets[_index + 1], next))
            throw new InvalidOperationException("BLUEGILL_GROWTH_OUT_OF_ORDER");
        _index++;
    }

    public CharacterEntry Entry(PondCreatureAsset asset) => new(SpeciesId, asset.Name,
        "Pond growth fixture", "Registered Bluegill stage", asset.ModelPath,
        1f / _assets[^1].Dimensions[1], asset.FacingDegrees, 30f,
        asset.Clip("Glide"), asset.Clip("Swim"), asset.Clip("SwimFast"),
        WorldHabitat.Freshwater, true, false, true, true);
}
