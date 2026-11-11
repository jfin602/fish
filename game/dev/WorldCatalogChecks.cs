#nullable enable
using Fish;
using Godot;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

public static class WorldCatalogChecks
{
    public static void Run()
    {
        var stages = WorldGenerationAudit.StageInvocations;
        var fish = DemoSelection.CharacterId;
        if (WorldCatalog.All.Count != 2 || WorldCatalog.All.Select(w => w.Id).Distinct().Count() != 2)
            throw new InvalidDataException("World IDs are not distinct");
        var ocean = WorldCatalog.Resolve(WorldCatalog.OceanId);
        var pond = WorldCatalog.Resolve(WorldCatalog.PondId);
        if (ocean.Habitat != WorldHabitat.Marine || pond.Habitat != WorldHabitat.Freshwater ||
            ocean.AssetCatalog == pond.AssetCatalog || ocean.EnvironmentProfile == pond.EnvironmentProfile ||
            ocean.Bake is null || pond.Bake is not null || pond.HabitatScenePath is not null ||
            ocean.SelectedManifestId == pond.SelectedManifestId ||
            pond.SelectedManifestId != "pond:not-installed")
            throw new InvalidDataException("World catalog routing is incomplete");
        var manifest = WorldCatalog.VerifyInstalledManifest(ocean);
        if (manifest.WorldId != "world-6cba9a67e6275297" ||
            manifest.ContentSha256 != "ba397da2b4ff2d2982898a3c3519b0e5cd0443222e9d31650197d8bcb10e3879")
            throw new InvalidDataException("Accepted Ocean identity changed");
        Reject(() => WorldCatalog.Resolve("lake"), "unknown ID");
        Reject(() => WorldCatalog.VerifyInstalledManifest(pond), "uninstalled Pond");
        WorldSession.Select(WorldCatalog.PondId);
        var missingHabitat = new Node3D();
        try { Reject(() => WorldCatalog.Load(WorldSession.SelectedWorldId, missingHabitat), "Pond load"); }
        finally { missingHabitat.Free(); }
        if (WorldSession.SelectedWorldId != WorldCatalog.PondId || DemoSelection.CharacterId != fish)
            throw new InvalidDataException("World selection changed fish or fell back");
        WorldSession.Select(WorldCatalog.OceanId);

        var temporary = Path.Combine(ProjectSettings.GlobalizePath("user://"),
            "world-catalog-check-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporary);
        try
        {
            var source = ProjectSettings.GlobalizePath(ocean.Bake.Directory + "/" + FullWorldPackage.ManifestFile);
            var target = Path.Combine(temporary, FullWorldPackage.ManifestFile);
            File.Copy(source, target);
            var altered = ocean with { Bake = ocean.Bake with { Directory = temporary } };
            var incompleteHabitat = ocean.LoadHabitatScene().Instantiate<Node3D>();
            try { Reject(() => WorldCatalog.Load(altered, incompleteHabitat), "missing package resources"); }
            finally { incompleteHabitat.Free(); }
            var changed = manifest with { SettingsVersion = 2 };
            File.WriteAllText(target, JsonSerializer.Serialize(changed));
            Reject(() => WorldCatalog.VerifyInstalledManifest(altered), "changed manifest");
            var wrongVersion = altered with { Bake = altered.Bake! with {
                ManifestSha256 = BakedWorldContract.Hash(File.ReadAllBytes(target)) } };
            Reject(() => WorldCatalog.VerifyInstalledManifest(wrongVersion), "version mismatch");
            File.WriteAllText(target, "{");
            Reject(() => WorldCatalog.VerifyInstalledManifest(altered), "corrupt manifest");
        }
        finally { Directory.Delete(temporary, true); }

        var scene = ocean.LoadHabitatScene();
        for (var attempt = 0; attempt < 2; attempt++)
        {
            var habitat = scene.Instantiate<Node3D>();
            try
            {
                var loaded = WorldCatalog.Load(ocean.Id, habitat);
                if (loaded.WorldId != manifest.WorldId || loaded.ContentSha256 != manifest.ContentSha256)
                    throw new InvalidDataException("Repeat load changed world identity");
                WorldSession.Bind(habitat, loaded);
                if (WorldSession.ActiveHabitat != habitat) throw new InvalidDataException("Active habitat not bound");
            }
            finally
            {
                WorldSession.Unload();
                habitat.Free();
            }
            if (WorldSession.ActiveHabitat is not null || WorldSession.ActiveManifest is not null)
                throw new InvalidDataException("Unload retained cached world references");
        }
        if (WorldGenerationAudit.StageInvocations != stages)
            throw new InvalidDataException("Generator ran during world catalog checks");
        GD.Print($"WORLD_CATALOG_CHECK_OK ocean={manifest.WorldId} manifest={ocean.Bake.ManifestSha256} content={manifest.ContentSha256} pond=not-installed repeats=2 generator_invocations=0");
    }

    private static void Reject(Action action, string reason)
    {
        try { action(); }
        catch (InvalidDataException) { return; }
        throw new InvalidDataException($"Expected failure was not observed: {reason}");
    }
}
