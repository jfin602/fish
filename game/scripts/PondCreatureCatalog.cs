#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace Fish;

// Intake snapshot of the owner FAF manifests. Life stages retain the species ID.
public sealed record PondCreatureAsset(string Id, string Name, string? Stage, string? Variant,
    string Source, string ModelPath, string Sha256, float[] Dimensions, string[] Bones,
    string[] Actions, string[] LoopRoles, string? Breathing, string Habitat, string Role,
    string TechnicalReview, string VisualReview)
{
    public string Clip(string role) => Actions.Single(action => action.EndsWith("__" + role, StringComparison.Ordinal));
    // The egg has no swimming direction. All four swimming stages import nose-first +Z.
    public float FacingDegrees => Stage == "egg" ? 0f : 180f;
}
public sealed record PondSourceManifest(string Path, string Sha256);
public sealed record PondCreatureRegistry(IReadOnlyList<PondSourceManifest> SourceManifests,
    IReadOnlyList<PondCreatureAsset> FishStages, IReadOnlyList<PondCreatureAsset> Ambient);

public static class PondCreatureCatalog
{
    private const string Path = "res://assets/faf/pond/pond_creature_catalog.json";
    private static readonly Lazy<PondCreatureRegistry> Registry = new(Load);
    public static IReadOnlyList<PondCreatureAsset> FishStages => Registry.Value.FishStages;
    public static IReadOnlyList<PondCreatureAsset> Ambient => Registry.Value.Ambient;

    public static PondCreatureAsset? Stage(string id, string stage) =>
        FishStages.SingleOrDefault(asset => asset.Id == id && asset.Stage == stage);

    public static bool ExportMatches(PondCreatureAsset asset)
    {
        if (!Godot.FileAccess.FileExists(asset.ModelPath)) return false;
        try
        {
            var bytes = File.ReadAllBytes(ProjectSettings.GlobalizePath(asset.ModelPath));
            return Convert.ToHexString(SHA256.HashData(bytes))
                .Equals(asset.Sha256, StringComparison.OrdinalIgnoreCase);
        }
        catch (IOException) { return false; }
    }

    private static PondCreatureRegistry Load()
    {
        var registry = JsonSerializer.Deserialize<PondCreatureRegistry>(
            File.ReadAllBytes(ProjectSettings.GlobalizePath(Path)))
            ?? throw new InvalidDataException("POND_CREATURE_CATALOG_EMPTY");
        var stages = new[] { "egg", "larva", "juvenile", "subadult", "adult" };
        var ids = Enumerable.Range(6, 5).Select(i => $"FAF-FISH-{i:000}").ToArray();
        if (registry.FishStages is null || registry.Ambient is null ||
            registry.SourceManifests is null || registry.FishStages.Count != 25 ||
            registry.Ambient.Count != 7 || registry.SourceManifests.Count != 3 ||
            ids.Any(id => stages.Any(stage => registry.FishStages.Count(a => a.Id == id && a.Stage == stage) != 1)) ||
            registry.Ambient.Select(a => a.Id).Distinct().Count() != 7 ||
            registry.FishStages.Any(a => a.Role != "fish_stage") ||
            registry.Ambient.Any(a => a.Role != "ambient" || a.Stage is not null))
            throw new InvalidDataException("POND_CREATURE_CATALOG_INCOMPLETE");
        foreach (var asset in registry.FishStages.Concat(registry.Ambient))
        {
            if (asset.Habitat != WorldCatalog.PondId ||
                string.IsNullOrWhiteSpace(asset.ModelPath) ||
                !asset.ModelPath.StartsWith("res://assets/faf/pond/", StringComparison.Ordinal) ||
                asset.Dimensions is not { Length: 3 } ||
                asset.Dimensions.Any(v => !float.IsFinite(v) || v <= 0) ||
                asset.Bones is null || asset.Bones.Length < 4 || asset.Actions is null ||
                asset.Actions.Length == 0 || asset.LoopRoles is null ||
                asset.Sha256 is null || asset.Sha256.Length != 64 ||
                (asset.Stage is null ? asset.Role != "ambient" : asset.Role != "fish_stage"))
                throw new InvalidDataException($"POND_CREATURE_CATALOG_INVALID id={asset.Id}");
            if (asset.Stage is "egg")
            {
                if (asset.Breathing != "not_applicable_egg" || asset.Bones.Length != 4 ||
                    asset.Actions.Length != 2 || !asset.Actions.Any(a => a.EndsWith("__Hatch")) ||
                    asset.Actions.Any(a => a.Contains("Swim", StringComparison.Ordinal)))
                    throw new InvalidDataException($"POND_EGG_RIG_INVALID id={asset.Id}");
            }
            else if (asset.Stage is not null &&
                (asset.Breathing != "operculate_bony" ||
                 !asset.Bones.Contains("gill_L") || !asset.Bones.Contains("gill_R") ||
                 !new[] { "Idle", "Glide", "SwimSlow", "Swim", "SwimFast", "TurnLeft", "TurnRight" }
                    .All(role => asset.Actions.Any(a => a.EndsWith("__" + role, StringComparison.Ordinal)))))
                throw new InvalidDataException($"POND_SWIM_RIG_INVALID id={asset.Id} stage={asset.Stage}");
        }
        return registry;
    }

    public static IReadOnlyList<CharacterEntry> AdultChoices() => FishStages
        .Where(asset => asset.Stage == "adult")
        .Select(asset => new CharacterEntry(asset.Id, asset.Name, "Freshwater fish",
            "Pond native fish · adult demo visual", asset.ModelPath,
            // Pond GLBs import nose-first toward +Z; FishController swims along -Z.
            1f / asset.Dimensions[1], asset.FacingDegrees, 30, asset.Clip("Glide"),
            asset.Clip("Swim"), asset.Clip("SwimFast"), WorldHabitat.Freshwater,
            true, true, true))
        .ToArray();
}
