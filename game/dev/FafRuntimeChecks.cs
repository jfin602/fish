#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Fish;

/// <summary>Checks the P3 runtime boundary against tracked FAF records and imported scenes.</summary>
public static class FafRuntimeChecks
{
    private static readonly (FafEnvironmentFamily Family, string Manifest, int Count)[] Packs =
    {
        (FafEnvironmentFamily.Terrain, "terrain/terrain_001_manifest.json", 6),
        (FafEnvironmentFamily.Coral, "coral/coral_001_to_004_manifest.json", 12),
        (FafEnvironmentFamily.Plant, "plants/plant_001_to_003_manifest.json", 8),
        (FafEnvironmentFamily.Detail, "details/detail_001_to_004_manifest.json", 8),
    };

    public static void Run()
    {
        Require(FafRuntimeCatalog.Environment.Count == 38, "environment catalog count");
        Require(FafRuntimeCatalog.Fish.Count == 5, "fish catalog count");
        Require(FafRuntimeCatalog.Environment.Select(e => e.Id).Distinct().Count() == 38,
            "duplicate environment ID");
        var seen = new HashSet<string>();
        foreach (var (family, manifest, count) in Packs)
        {
            using var document = ReadManifest(manifest);
            var assets = document.RootElement.GetProperty("assets");
            Require(assets.GetArrayLength() == count, $"{family} manifest count");
            foreach (var asset in assets.EnumerateArray())
            {
                var id = asset.GetProperty("faf_id").GetString()!;
                var entry = FafRuntimeCatalog.GetEnvironment(id);
                Require(seen.Add(id) && entry.Family == family && entry.Variant == id[^1],
                    $"{id} family/variant identity");
                var palette = asset.TryGetProperty("palette", out var sourcePalette)
                    ? sourcePalette.GetString() : "";
                Require(entry.ScenePath == ScenePath(asset) &&
                    entry.Type == Type(asset) && entry.Palette == palette,
                    $"{id} source metadata");
                var dimensions = asset.GetProperty("dimensions_m");
                var expectedSize = new Vector3(Dim(dimensions, 0), Dim(dimensions, 2), Dim(dimensions, 1));
                Require(entry.SizeMeters.DistanceTo(expectedSize) < .001f, $"{id} dimension metadata");
                Require(entry.HasPlantHeightMask == (family == FafEnvironmentFamily.Plant) &&
                    entry.HeightMaskCoordinate == (family == FafEnvironmentFamily.Plant ? "UV.y" : null),
                    $"{id} plant height-mask metadata");
                CheckEnvironment(entry, asset.GetProperty("triangles").GetInt32(), MaterialNames(asset));
            }
        }
        var rockTriangles = new[] { 240, 186, 248, 222 };
        for (var index = 0; index < 4; index++)
        {
            var variant = (char)('A' + index);
            var id = $"FAF-ROCK-001{variant}";
            var entry = FafRuntimeCatalog.GetEnvironment(id);
            Require(seen.Add(id) && entry.Family == FafEnvironmentFamily.Rock &&
                entry.Variant == variant && entry.Type == "reef_boulder" &&
                entry.ScenePath == $"res://assets/faf/rocks/rock_001{char.ToLowerInvariant(variant)}_reef_boulder.glb" &&
                entry.Palette == "stone.cool_slate", $"{id} rock candidate record");
            CheckEnvironment(entry, rockTriangles[index], new[] { "mat__rock_cool_slate" });
        }
        Require(seen.Count == 38, "unmapped environment entry");
        CheckFish();
        GD.Print("FAF_RUNTIME_CHECK_OK environment=38 terrain=6 rock=4 coral=12 plant=8 detail=8 fish=5");
    }

    private static void CheckEnvironment(FafEnvironmentEntry entry, int triangles, string[] materialNames)
    {
        var root = entry.CreateVisual();
        try
        {
            Require(root.GetMeta("faf_id").AsString() == entry.Id &&
                root.GetMeta("faf_scene_path").AsString() == entry.ScenePath &&
                root.GetChildCount() == 1, $"{entry.Id} wrapper identity");
            var meshes = Nodes<MeshInstance3D>(root).ToArray();
            Require(meshes.Length == 1 && meshes[0].Mesh is not null, $"{entry.Id} mesh");
            var mesh = meshes[0].Mesh;
            Require(mesh.GetSurfaceCount() == materialNames.Length, $"{entry.Id} material count");
            var found = new HashSet<string>();
            var actualTriangles = 0;
            for (var surface = 0; surface < mesh.GetSurfaceCount(); surface++)
            {
                var material = mesh.SurfaceGetMaterial(surface);
                Require(material is not null && materialNames.Contains(material.ResourceName),
                    $"{entry.Id} material {surface}");
                found.Add(material!.ResourceName);
                actualTriangles += mesh.SurfaceGetArrays(surface)[(int)Mesh.ArrayType.Index].AsInt32Array().Length / 3;
            }
            Require(actualTriangles == triangles && found.Count == materialNames.Length,
                $"{entry.Id} triangle/material identity");
            var bounds = meshes[0].GetAabb();
            Require(bounds.Size.DistanceTo(entry.SizeMeters) < .04f &&
                Mathf.Abs(bounds.Position.Y - entry.ContactY) < .025f,
                $"{entry.Id} bounds/contact: {bounds}");
            if (entry.HasPlantHeightMask)
            {
                var uv = mesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.TexUV].AsVector2Array();
                Require(uv.Length > 0 && uv.Min(point => point.Y) < .01f &&
                    uv.Max(point => point.Y) > .99f, $"{entry.Id} UV.y mask");
            }
            GD.Print($"FAF_RUNTIME_ENV id={entry.Id} path={entry.ScenePath} triangles={actualTriangles}");
        }
        finally { root.Free(); }
    }

    private static void CheckFish()
    {
        using var document = ReadManifest("fish/reef_fish_002_to_005_manifest.json");
        var manifest = document.RootElement.GetProperty("assets").EnumerateArray()
            .ToDictionary(item => item.GetProperty("faf_id").GetString()!);
        Require(manifest.Count == 4, "reef fish manifest count");
        var lengths = new[] { .237f, .154f, .145f, .201f, 1.57f };
        var triangles = new[] { 1100, 912, 404, 896, 876 };
        var meshCounts = new[] { 8, 8, 8, 8, 12 };
        var materialCounts = new[] { 6, 3, 3, 3, 3 };
        var boneCounts = new[] { 14, 14, 13, 14, 14 };
        var names = new[] { "Yellow Tang", "Ocellaris Clownfish", "Blue-Green Chromis",
            "Reef Butterflyfish", "Blacktip Reef Shark" };
        for (var index = 0; index < 5; index++)
        {
            var entry = FafRuntimeCatalog.Fish[index];
            var id = $"FAF-FISH-{index + 1:000}";
            Require(entry.Id == id && entry.Name == names[index] &&
                entry.PlayerScenePath == entry.PreviewScenePath &&
                entry.PlayerScenePath == entry.ModelPath &&
                entry.ModelPath.StartsWith("res://assets/faf/fish/", StringComparison.Ordinal),
                $"{id} descriptor identity/path");
            if (index > 0)
            {
                var source = manifest[id];
                Require(entry.ModelPath == ScenePath(source), $"{id} manifest path");
                var actions = source.GetProperty("actions").EnumerateArray()
                    .Select(value => value.GetString()).ToArray();
                Require(actions.SequenceEqual(new[] { entry.GlideClip, entry.SwimClip, entry.FastClip }),
                    $"{id} manifest action mapping");
            }
            Require(Mathf.Abs(lengths[index] * entry.WorldScale - 1f) < .075f &&
                Mathf.Abs(entry.FacingDegrees - 180f) < .001f &&
                entry.PresentationTransform.Origin == Vector3.Zero,
                $"{id} shared visual envelope/orientation");
            Require(CharacterCatalog.TryInstantiate(entry, out var model, out var player, out var diagnostic),
                $"{id} scene: {diagnostic}");
            try
            {
                var meshes = Nodes<MeshInstance3D>(model!).ToArray();
                var skeletons = Nodes<Skeleton3D>(model!).ToArray();
                Require(meshes.Length == meshCounts[index] && skeletons.Length == 1 &&
                    skeletons[0].GetBoneCount() == boneCounts[index], $"{id} mesh/rig count");
                var actualTriangles = 0;
                var materials = new HashSet<string>();
                foreach (var part in meshes)
                {
                    Require(part.Mesh is not null, $"{id} empty mesh");
                    var mesh = part.Mesh!;
                    for (var surface = 0; surface < mesh.GetSurfaceCount(); surface++)
                    {
                        actualTriangles += mesh.SurfaceGetArrays(surface)[(int)Mesh.ArrayType.Index].AsInt32Array().Length / 3;
                        var material = mesh.SurfaceGetMaterial(surface);
                        Require(material is not null, $"{id} missing material");
                        materials.Add(material!.ResourceName);
                    }
                }
                Require(actualTriangles == triangles[index] && materials.Count == materialCounts[index],
                    $"{id} mesh/material identity");
                foreach (var clipName in new[] { entry.GlideClip, entry.SwimClip, entry.FastClip })
                {
                    Require(player!.HasAnimation(clipName), $"{id} missing {clipName}");
                    var clip = player.GetAnimation(clipName);
                    Require(clip is not null && clip.Length > 0 &&
                        clip.LoopMode == Animation.LoopModeEnum.Linear && clip.GetTrackCount() > 0,
                        $"{id} invalid {clipName}");
                }
                GD.Print($"FAF_RUNTIME_FISH id={id} path={entry.ModelPath} clips=3 scale={entry.WorldScale}");
            }
            finally { model!.Free(); }
        }
    }

    private static JsonDocument ReadManifest(string relativePath)
    {
        var path = ProjectSettings.GlobalizePath("res://../assets/faf/" + relativePath);
        return JsonDocument.Parse(System.IO.File.ReadAllText(path));
    }

    private static string ScenePath(JsonElement item) =>
        "res://" + item.GetProperty("glb").GetString()!["game/".Length..];
    private static string Type(JsonElement item) =>
        item.TryGetProperty("family", out var family) ? family.GetString()! :
        item.GetProperty("species_or_type").GetString()!;
    private static float Dim(JsonElement dimensions, int index) => dimensions[index].GetSingle();
    private static string[] MaterialNames(JsonElement item)
    {
        if (item.TryGetProperty("material_names", out var names))
            return names.EnumerateArray().Select(value => value.GetString()!).ToArray();
        if (item.TryGetProperty("material_name", out var name)) return new[] { name.GetString()! };
        var category = item.GetProperty("category").GetString();
        var swatch = item.GetProperty("palette").GetString()!.Split('.')[1];
        return new[] { $"mat__{(category == "PLANT" ? "plant" : "coral")}_{swatch}" };
    }
    private static IEnumerable<T> Nodes<T>(Node root) where T : Node
    {
        if (root is T typed) yield return typed;
        foreach (var child in root.GetChildren())
            foreach (var descendant in Nodes<T>(child)) yield return descendant;
    }
    private static void Require(bool valid, string message)
    {
        if (!valid) throw new InvalidOperationException(message);
    }
}
