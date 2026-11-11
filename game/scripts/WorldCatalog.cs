#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace Fish;

public enum WorldHabitat { Marine, Freshwater }
public enum WorldEnvironmentProfile { Ocean, Pond }
public enum WorldAssetCatalog { MarineFaf, FreshwaterFaf }

public sealed record WorldPreview(string Description, Color Accent);
public sealed record WorldSpawn(string RouteNodeId, string RouteRole);
public sealed record BakedWorldIdentity(string Directory, string WorldId, ulong Seed,
    int SettingsVersion, string GeneratorVersion, string CatalogSha256,
    string ContentSha256, string ManifestSha256);

/// <summary>A stable world identity. An absent bake is an unavailable world, never a fallback.</summary>
public sealed record WorldDefinition(string Id, string DisplayName, WorldHabitat Habitat,
    WorldPreview Preview, WorldEnvironmentProfile EnvironmentProfile,
    WorldSpawn Spawn, WorldAssetCatalog AssetCatalog, string? HabitatScenePath,
    BakedWorldIdentity? Bake)
{
    public string SelectedManifestId => Bake?.WorldId ?? $"{Id}:not-installed";

    public PackedScene LoadHabitatScene() => HabitatScenePath is not null
        ? ResourceLoader.Load<PackedScene>(HabitatScenePath)
            ?? throw new InvalidDataException($"WORLD_SCENE_MISSING world={Id} scene={HabitatScenePath} action=restore-world-scene")
        : throw new InvalidDataException($"WORLD_SCENE_UNAVAILABLE world={Id} action=install-world-scene");
}

public static class WorldCatalog
{
    public const string OceanId = "ocean";
    public const string PondId = "pond";

    public static IReadOnlyList<WorldDefinition> All { get; } = new[]
    {
        new WorldDefinition(OceanId, "Ocean", WorldHabitat.Marine,
            new WorldPreview("Open reef and deep ocean", new Color(0.05f, 0.38f, 0.55f)),
            WorldEnvironmentProfile.Ocean, new WorldSpawn("spawn", "safe-spawn"),
            WorldAssetCatalog.MarineFaf, "res://scenes/p2_habitat.tscn",
            new BakedWorldIdentity("res://worlds/base/ocean-m4-42", "world-6cba9a67e6275297",
                42, 1, "c3-full-12",
                "d7f9e29007443eb53b09a780028f2176f23fcc01774fbec178cce80d1581cf9a",
                "ba397da2b4ff2d2982898a3c3519b0e5cd0443222e9d31650197d8bcb10e3879",
                "b21b4dd4517b4f66a732bf8834218223912bca5886ffc5c6a511c7187f107865")),
        new WorldDefinition(PondId, "Freshwater Pond", WorldHabitat.Freshwater,
            new WorldPreview("Overgrown freshwater banks", new Color(0.32f, 0.36f, 0.13f)),
            WorldEnvironmentProfile.Pond, new WorldSpawn("spawn", "safe-spawn"),
            WorldAssetCatalog.FreshwaterFaf, "res://scenes/pond_habitat.tscn",
            new BakedWorldIdentity("res://worlds/base/pond-p4-71", "world-5e3b4fee759acbe3",
                71, 2, "pond-full-1",
                "d7f9e29007443eb53b09a780028f2176f23fcc01774fbec178cce80d1581cf9a",
                "d76eabbb5b6fff2f91418b49b2de92dd090541d609b46d0725ad9fc85cbda347",
                "966a70f784c5398e0071ca098ec07c3765d70fc03c2e0bae4e8c82f3bd2f2173")),
    };

    public static WorldDefinition Resolve(string? id)
    {
        foreach (var world in All)
            if (string.Equals(world.Id, id, StringComparison.Ordinal)) return world;
        throw new InvalidDataException($"WORLD_ID_UNKNOWN id={id ?? "<null>"} action=choose-ocean-or-pond");
    }

    public static FullWorldManifest VerifyInstalledManifest(WorldDefinition world)
    {
        var bake = world.Bake ?? throw new InvalidDataException(
            $"WORLD_BAKE_NOT_INSTALLED world={world.Id} action=complete-p4-pond-bake");
        var manifestPath = ProjectSettings.GlobalizePath(bake.Directory + "/" + FullWorldPackage.ManifestFile);
        if (!File.Exists(manifestPath)) throw new InvalidDataException(
            $"WORLD_MANIFEST_MISSING world={world.Id} path={bake.Directory} action=restore-baked-package");
        if (BakedWorldContract.Hash(File.ReadAllBytes(manifestPath)) != bake.ManifestSha256)
            throw new InvalidDataException($"WORLD_MANIFEST_CHANGED world={world.Id} action=restore-or-requalify-baked-package");
        FullWorldManifest manifest;
        try { manifest = FullWorldPackage.ReadManifest(bake.Directory); }
        catch (Exception error) when (error is IOException or System.Text.Json.JsonException)
        {
            throw new InvalidDataException($"WORLD_MANIFEST_INVALID world={world.Id} action=restore-baked-package", error);
        }
        if (manifest.WorldId != bake.WorldId || manifest.Seed != bake.Seed ||
            (manifest.HabitatId is not null && manifest.HabitatId != world.Id) ||
            manifest.SettingsVersion != bake.SettingsVersion ||
            manifest.GeneratorVersion != bake.GeneratorVersion ||
            manifest.CatalogSha256 != bake.CatalogSha256 ||
            manifest.ContentSha256 != bake.ContentSha256)
            throw new InvalidDataException($"WORLD_MANIFEST_IDENTITY_MISMATCH world={world.Id} action=restore-or-requalify-baked-package");
        return manifest;
    }

    public static FullWorldManifest Load(string worldId, Node3D habitat)
        => Load(Resolve(worldId), habitat);

    internal static FullWorldManifest Load(WorldDefinition world, Node3D habitat)
    {
        VerifyInstalledManifest(world);
        try { return FullWorldPackage.Load(world.Bake!.Directory, habitat, world.Id,
            world.Bake.WorldId).Manifest; }
        catch (Exception error) when (error is IOException or InvalidDataException or System.Text.Json.JsonException)
        {
            throw new InvalidDataException($"WORLD_BAKE_INVALID world={world.Id} detail={error.Message} action=restore-or-requalify-baked-package", error);
        }
    }
}

/// <summary>Transient selection and loaded scene references; independent of fish and future run state.</summary>
public static class WorldSession
{
    public static string SelectedWorldId { get; private set; } = WorldCatalog.OceanId;
    public static Node3D? ActiveHabitat { get; private set; }
    public static FullWorldManifest? ActiveManifest { get; private set; }

    public static void Select(string worldId)
    {
        WorldCatalog.Resolve(worldId);
        if (ActiveHabitat is { } habitat && worldId != SelectedWorldId)
        {
            habitat.GetNodeOrNull<FafAmbientReefLife>(
                "Composition/ReefComposition/AmbientLife")?.ClearPopulation();
            habitat.GetNodeOrNull<FafAmbientPondLife>("PondAmbientLife")?.ClearPopulation();
        }
        Unload();
        SelectedWorldId = worldId;
    }

    public static void Bind(Node3D habitat, FullWorldManifest manifest)
    {
        Unload();
        ActiveHabitat = habitat;
        ActiveManifest = manifest;
    }

    public static void Unload()
    {
        ActiveHabitat = null;
        ActiveManifest = null;
    }
}
