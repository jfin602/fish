#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Fish;

/// <summary>Tech-demo art choices. Entries are data; swimming authority stays in FishController.</summary>
public sealed record CharacterEntry(
    string Id, string Name, string Category, string Description, string ModelPath,
    float WorldScale, float FacingDegrees, float PreviewYawDegrees,
    string GlideClip, string SwimClip, string FastClip,
    WorldHabitat NativeHabitat, bool Selectable, bool Ambient, bool Ecological,
    bool FixtureOnly = false)
{
    public string PlayerScenePath => ModelPath;
    public string PreviewScenePath => ModelPath;
    public Transform3D PresentationTransform => new(
        Basis.FromEuler(new Vector3(0, Mathf.DegToRad(FacingDegrees), 0)).Scaled(Vector3.One * WorldScale),
        Vector3.Zero);
}

public static class CharacterCatalog
{
    private const string Faf = "res://assets/faf/fish/";
    private static readonly CharacterEntry[] Marine =
    {
        new("FAF-FISH-001", "Yellow Tang", "Reef fish", "A bright, agile reef swimmer.", Faf + "fish_001_yellow_tang.glb", 4.3f, 180, 30, "fish_001_yellow_tang__Glide", "fish_001_yellow_tang__Swim", "fish_001_yellow_tang__SwimFast", WorldHabitat.Marine, true, true, true),
        new("FAF-FISH-002", "Ocellaris Clownfish", "Reef fish", "A striped fish at home among the reef.", Faf + "fish_002_ocellaris_clownfish.glb", 6.2f, 180, 30, "fish_002_ocellaris_clownfish__Glide", "fish_002_ocellaris_clownfish__Swim", "fish_002_ocellaris_clownfish__SwimFast", WorldHabitat.Marine, true, true, true),
        new("FAF-FISH-003", "Blue-Green Chromis", "Reef fish", "A small blue-green swimmer.", Faf + "fish_003_blue_green_chromis.glb", 6.4f, 180, 30, "fish_003_blue_green_chromis__Glide", "fish_003_blue_green_chromis__Swim", "fish_003_blue_green_chromis__SwimFast", WorldHabitat.Marine, true, true, true),
        new("FAF-FISH-004", "Reef Butterflyfish", "Reef fish", "A tall-finned reef explorer.", Faf + "fish_004_reef_butterflyfish.glb", 5.0f, 180, 30, "fish_004_reef_butterflyfish__Glide", "fish_004_reef_butterflyfish__Swim", "fish_004_reef_butterflyfish__SwimFast", WorldHabitat.Marine, true, true, true),
        new("FAF-FISH-005", "Blacktip Reef Shark", "Shark", "A sleek shark with striking dark fin tips.", Faf + "fish_005_blacktip_reef_shark.glb", .64f, 180, 30, "fish_005_blacktip_reef_shark__Glide", "fish_005_blacktip_reef_shark__Swim", "fish_005_blacktip_reef_shark__SwimFast", WorldHabitat.Marine, true, false, true),
    };
    public static string PondIntakeDiagnostic { get; private set; } = "";
    public static IReadOnlyList<CharacterEntry> All { get; } = BuildCatalog();

    private static IReadOnlyList<CharacterEntry> BuildCatalog()
    {
        try { return Marine.Concat(PondCreatureCatalog.AdultChoices()).ToArray(); }
        catch (Exception error) when (error is IOException or JsonException or InvalidDataException)
        {
            PondIntakeDiagnostic = $"POND_CREATURE_CATALOG_INVALID detail={error.Message} action=restore-owner-pack-and-import";
            return Marine;
        }
    }

    public static CharacterEntry Default => Marine[0];
    public static CharacterEntry? Find(string? id) => All.FirstOrDefault(entry => entry.Id == id);

    public static bool TryInstantiate(CharacterEntry entry, out Node3D? model,
        out AnimationPlayer? player, out string diagnostic)
    {
        model = null;
        player = null;
        if (!Godot.FileAccess.FileExists(entry.ModelPath))
        {
            diagnostic = $"FISH_ASSET_MISSING item={entry.Id} path={entry.ModelPath} action=restore-source-asset";
            return false;
        }
        if (entry.NativeHabitat == WorldHabitat.Freshwater &&
            (PondCreatureCatalog.Stage(entry.Id, "adult") is not { } pond ||
             !PondCreatureCatalog.ExportMatches(pond)))
        {
            diagnostic = $"FISH_ASSET_CHANGED item={entry.Id} action=restore-owner-export";
            return false;
        }
        var scene = ResourceLoader.Load<PackedScene>(entry.ModelPath);
        if (scene is null)
        {
            diagnostic = $"FISH_ASSET_INVALID item={entry.Id} path={entry.ModelPath} action=run-tools-dev-import";
            return false;
        }
        model = scene.Instantiate<Node3D>();
        foreach (var node in model.FindChildren("*", "MeshInstance3D", true, false))
            if (node is MeshInstance3D { Mesh: not null }) { player = FindPlayer(model); break; }
        if (player is null || !player.HasAnimation(entry.SwimClip) ||
            (entry.GlideClip.Length > 0 && !player.HasAnimation(entry.GlideClip)) ||
            (entry.FastClip.Length > 0 && !player.HasAnimation(entry.FastClip)) ||
            !AnimationStaysOnRig(player, entry))
        {
            model.Free();
            model = null;
            diagnostic = $"FISH_ASSET_INVALID item={entry.Id} path={entry.ModelPath} action=check-mesh-and-catalog-animation-mapping";
            return false;
        }
        diagnostic = "";
        return true;
    }

    private static AnimationPlayer? FindPlayer(Node3D model)
    {
        foreach (var node in model.FindChildren("*", "AnimationPlayer", true, false))
            if (node is AnimationPlayer player) return player;
        return null;
    }

    private static bool AnimationStaysOnRig(AnimationPlayer player, CharacterEntry entry)
    {
        foreach (var name in new[] { entry.GlideClip, entry.SwimClip, entry.FastClip })
        {
            if (name.Length == 0) continue;
            var clip = player.GetAnimation(name);
            if (clip is null || clip.Length <= 0 || clip.GetTrackCount() == 0) return false;
            for (var track = 0; track < clip.GetTrackCount(); track++)
                if (clip.TrackGetType(track) != Animation.TrackType.Rotation3D) return false;
        }
        return true;
    }
}

public enum SpeciesRole { Selectable, Ambient, Ecological }

/// <summary>Single fail-closed policy for menu, baked ambient and future ecological entries.</summary>
public static class SpeciesCompatibility
{
    public static bool TryResolve(string? worldId, string? speciesId, SpeciesRole role,
        out CharacterEntry? entry, out string diagnostic, string? bakedWorldId = null)
        => TryResolveFrom(CharacterCatalog.All, worldId, speciesId, role, out entry,
            out diagnostic, bakedWorldId, false);

    internal static bool TryResolveFrom(IReadOnlyList<CharacterEntry> catalog, string? worldId,
        string? speciesId, SpeciesRole role, out CharacterEntry? entry, out string diagnostic,
        string? bakedWorldId = null, bool allowFixture = false)
    {
        entry = null;
        // Diagnostics deliberately avoid echoing arbitrary external identifiers or paths.
        var world = WorldCatalog.All.FirstOrDefault(w => w.Id == worldId);
        if (world is null || !Enum.IsDefined(world.Habitat))
        { diagnostic = "SPECIES_WORLD_UNKNOWN action=choose-known-world"; return false; }
        if (bakedWorldId is not null && (world.Bake is null || world.Bake.WorldId != bakedWorldId))
        { diagnostic = "SPECIES_BAKE_INCOMPATIBLE action=load-matching-world"; return false; }
        if (string.IsNullOrWhiteSpace(speciesId) ||
            catalog.Count(e => e.Id == speciesId) != 1)
        { diagnostic = "SPECIES_ID_UNKNOWN action=use-registered-id"; return false; }
        var candidate = catalog.First(e => e.Id == speciesId);
        if (candidate.FixtureOnly && !allowFixture)
        { diagnostic = "SPECIES_FIXTURE_ONLY action=use-production-species"; return false; }
        if (!Enum.IsDefined(candidate.NativeHabitat) || candidate.NativeHabitat != world.Habitat)
        { diagnostic = "SPECIES_HABITAT_INCOMPATIBLE action=choose-native-species"; return false; }
        if (role switch { SpeciesRole.Selectable => !candidate.Selectable,
            SpeciesRole.Ambient => !candidate.Ambient,
            SpeciesRole.Ecological => !candidate.Ecological, _ => true })
        { diagnostic = "SPECIES_ROLE_INELIGIBLE action=choose-eligible-species"; return false; }
        if (string.IsNullOrWhiteSpace(candidate.ModelPath) ||
            !Godot.FileAccess.FileExists(candidate.ModelPath) ||
            ResourceLoader.Load<PackedScene>(candidate.ModelPath) is null)
        { diagnostic = "SPECIES_MODEL_MISSING action=restore-and-import-model"; return false; }
        if (candidate.NativeHabitat == WorldHabitat.Freshwater && !candidate.FixtureOnly &&
            (PondCreatureCatalog.Stage(candidate.Id, "adult") is not { } pond ||
             !PondCreatureCatalog.ExportMatches(pond)))
        { diagnostic = "SPECIES_MODEL_CHANGED action=restore-owner-export"; return false; }
        entry = candidate;
        diagnostic = "";
        return true;
    }

    /// <summary>Same habitat gate for Pond wildlife and reserved fish stages.</summary>
    public static bool TryResolvePondAsset(string? worldId, string? id, string? stage,
        SpeciesRole role, out PondCreatureAsset? asset, out string diagnostic)
    {
        asset = null;
        var candidate = stage is null
            ? PondCreatureCatalog.Ambient.SingleOrDefault(a => a.Id == id)
            : PondCreatureCatalog.Stage(id ?? "", stage);
        if (candidate is null)
        { diagnostic = "SPECIES_ID_UNKNOWN action=use-registered-id"; return false; }
        var world = WorldCatalog.All.FirstOrDefault(w => w.Id == worldId);
        if (world is null || !Enum.IsDefined(world.Habitat))
        { diagnostic = "SPECIES_WORLD_UNKNOWN action=choose-known-world"; return false; }
        if (world.Habitat != WorldHabitat.Freshwater || candidate.Habitat != WorldCatalog.PondId)
        { diagnostic = "SPECIES_HABITAT_INCOMPATIBLE action=choose-native-species"; return false; }
        if (role != SpeciesRole.Ambient && !(stage is not null && role == SpeciesRole.Ecological))
        { diagnostic = "SPECIES_ROLE_INELIGIBLE action=choose-eligible-species"; return false; }
        if (!Godot.FileAccess.FileExists(candidate.ModelPath) ||
            ResourceLoader.Load<PackedScene>(candidate.ModelPath) is null)
        { diagnostic = "SPECIES_MODEL_MISSING action=restore-and-import-model"; return false; }
        if (!PondCreatureCatalog.ExportMatches(candidate))
        { diagnostic = "SPECIES_MODEL_CHANGED action=restore-owner-export"; return false; }
        asset = candidate;
        diagnostic = "";
        return true;
    }

    public static IReadOnlyList<CharacterEntry> Available(string? worldId, SpeciesRole role)
        => CharacterCatalog.All.Where(candidate => TryResolve(worldId, candidate.Id, role,
            out _, out _)).ToArray();
}

/// <summary>Selection lives for one process and never enters a survival save.</summary>
public static class DemoSelection
{
    public static string? CharacterId { get; set; }
    public static void Clear() => CharacterId = null;
}
