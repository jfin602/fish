#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace Fish;

/// <summary>One fixture Bass identity from egg through registered swimming stages.</summary>
public sealed class PondGrowthState
{
    public const string SpeciesId = PikeEncounterAssets.BassId;
    public const float AdultColliderRadius = .19924765f;
    public static readonly IReadOnlyDictionary<string, int> Thresholds =
        new Dictionary<string, int> { ["larva"] = 0, ["juvenile"] = 1, ["subadult"] = 3, ["adult"] = 11 };
    private static readonly string[] Stages = { "egg", "larva", "juvenile", "subadult", "adult" };
    private readonly PondCreatureAsset[] _assets = new PondCreatureAsset[5];
    private int _index;

    public string Stage => Stages[_index];
    public PondCreatureAsset Asset => _assets[_index];
    public float Length => Asset.Dimensions[1];
    public float ColliderRadius => RadiusFor(_index);
    public float ColliderLength => LengthFor(_index);
    public string Identity { get; }
    public int NextThreshold => _index + 1 < Stages.Length ? Thresholds[Stages[_index + 1]] : int.MaxValue;

    public PondGrowthState(string identity, string startStage = "egg")
    {
        if (string.IsNullOrWhiteSpace(identity)) throw new ArgumentException("identity required", nameof(identity));
        Identity = identity;
        for (var i = 0; i < Stages.Length; i++)
        {
            if (!SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.PondId, SpeciesId,
                    Stages[i], SpeciesRole.Ecological, out var asset, out var diagnostic) || asset is null)
                throw new InvalidOperationException("BASS_GROWTH_STAGE_UNAVAILABLE " + diagnostic);
            _assets[i] = asset;
            if (i > 1 && _assets[i].Dimensions[1] <= _assets[i - 1].Dimensions[1])
                throw new InvalidOperationException("BASS_GROWTH_SIZE_ORDER_INVALID");
        }
        _index = Array.IndexOf(Stages, startStage);
        if (_index < 0) throw new ArgumentException("invalid Bass start stage", nameof(startStage));
    }

    public PondCreatureAsset? Next(int nutrition) => _index >= 1 && nutrition >= NextThreshold && _index + 1 < Stages.Length
        ? _assets[_index + 1] : null;

    public void Hatch()
    {
        if (_index != 0) throw new InvalidOperationException("BASS_HATCH_OUT_OF_ORDER");
        _index = 1;
    }

    public float RadiusForNext() => RadiusFor(_index + 1);
    public float LengthForNext() => LengthFor(_index + 1);
    private float LengthFor(int index) => _assets[index].Dimensions[1] / _assets[^1].Dimensions[1];
    // Bass is long and narrow. The radius follows the registered width, while
    // the capsule length follows the registered nose-to-tail dimension.
    private float RadiusFor(int index) => _assets[index].Dimensions[0] /
        (2f * _assets[^1].Dimensions[1]);

    public void Commit(PondCreatureAsset next)
    {
        if (_index + 1 >= Stages.Length || !ReferenceEquals(_assets[_index + 1], next))
            throw new InvalidOperationException("BASS_GROWTH_OUT_OF_ORDER");
        _index++;
    }

    public CharacterEntry Entry(PondCreatureAsset asset) => new(SpeciesId, asset.Name,
        "Pond growth fixture", "Registered Bass stage", asset.ModelPath,
        1f / _assets[^1].Dimensions[1], asset.FacingDegrees, 30f,
        asset.Clip("Glide"), asset.Clip("Swim"), asset.Clip("SwimFast"),
        WorldHabitat.Freshwater, true, false, true, true);
}
