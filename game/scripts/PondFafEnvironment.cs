#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace Fish;

// The catalog is a Godot-side intake snapshot of the three owner FAF manifests.
// It records the actual exported paths and hashes without modifying their masters.
public sealed record PondFafAsset(string Id, string Variant, string Family, string Role,
    string Source, string SourceSha256, string Glb, string? Wrapper, float[] Size,
    int Triangles, int Materials, string Habitat, string Anchor, float StemDepth,
    string Sha256, string StyleStatus);
public sealed record PondFafCatalog(IReadOnlyList<PondFafAsset> Entries);
public sealed record PondFafPlacement(string Id, Vector3 Position, float Yaw, float Scale,
    string Zone);

public static class PondFafEnvironment
{
    private const string CatalogPath = "res://assets/faf/pond/pond_environment_catalog.json";
    public static IReadOnlyList<PondFafAsset> LoadCatalog()
    {
        var catalog = JsonSerializer.Deserialize<PondFafCatalog>(File.ReadAllBytes(
            ProjectSettings.GlobalizePath(CatalogPath))) ?? throw new InvalidDataException("Pond FAF catalog empty");
        if (catalog.Entries.Count != 33 || catalog.Entries.Select(a => a.Id).Distinct().Count() != 33)
            throw new InvalidDataException("Pond FAF catalog requires 33 unique variants");
        foreach (var asset in catalog.Entries)
        {
            if (asset.Habitat != WorldCatalog.PondId || !asset.Glb.StartsWith("res://assets/faf/pond/", StringComparison.Ordinal) ||
                !asset.Id.StartsWith("FAF-", StringComparison.Ordinal) ||
                asset.Variant != asset.Id[^1..] || string.IsNullOrWhiteSpace(asset.Role) ||
                asset.Size.Length != 3 ||
                asset.Size.Any(v => !float.IsFinite(v) || v <= 0) || asset.Triangles <= 0 || asset.Materials <= 0)
                throw new InvalidDataException($"Invalid Pond FAF catalog entry: {asset.Id}");
            var bytes = File.ReadAllBytes(ProjectSettings.GlobalizePath(asset.Glb));
            if (!Convert.ToHexString(SHA256.HashData(bytes)).Equals(asset.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Pond FAF export changed: {asset.Id} {asset.Glb}");
            if (ResourceLoader.Load<PackedScene>(asset.Glb) is null ||
                (asset.Wrapper is not null && ResourceLoader.Load<PackedScene>(asset.Wrapper) is null))
                throw new InvalidDataException($"Pond FAF import unavailable: {asset.Id}");
        }
        return catalog.Entries;
    }

    // All samples were ray projected onto the final generated surface while baking.
    // The baked route-distance field protects ordinary navigation, including camera margins.
    public static IReadOnlyList<PondFafPlacement> Plan(FullWorldData data, GeneratedBasin basin,
        IReadOnlyList<PondFafAsset> assets)
    {
        var context = new WorldCompileContext(new WorldGenerationSettings(basin.Seed, 2, "pond-full-1"));
        var plan = new List<PondFafPlacement>();
        foreach (var asset in assets.OrderBy(a => a.Id == "FAF-WOOD-002A" ? 0 : 1)
            .ThenBy(a => a.Id, StringComparer.Ordinal))
        {
            var candidates = data.Environment.Where(row => Suitable(asset, row))
                .OrderBy(row => row[0]).ThenBy(row => row[2]).ToArray();
            if (candidates.Length == 0)
                throw new InvalidDataException($"Pond FAF has no supported habitat: {asset.Id}");
            var random = context.Stream("pond-faf-placement", asset.Id);
            var target = Count(asset);
            var picked = new HashSet<int>();
            for (var attempt = 0; attempt < Mathf.Max(target * 12, candidates.Length * 8) &&
                picked.Count < target; attempt++)
            {
                var index = (int)(random.Next() % (ulong)candidates.Length);
                if (picked.Contains(index)) continue;
                var row = candidates[index];
                var yaw = (float)(random.Next() % 628319) / 100000f;
                var scale = .9f + (float)(random.Next() % 2001) / 10000f;
                if (!Supported(asset, basin, row, yaw, scale)) continue;
                if (asset.Id != "FAF-WOOD-002A" && plan.Any(p => p.Id == "FAF-WOOD-002A" &&
                    new Vector2(p.Position.X, p.Position.Z).DistanceTo(new Vector2(row[0], row[2])) < 8f))
                    continue;
                picked.Add(index);
                var y = asset.Family is "lily_pad" or "floating_muck"
                    ? GeneratedBasin.WaterSurfaceY : row[1] - (asset.Family == "eroded_bank" ? .10f : .018f);
                plan.Add(new PondFafPlacement(asset.Id, new Vector3(row[0], y, row[2]), yaw, scale,
                    asset.Family));
            }
            if (picked.Count == 0) throw new InvalidDataException($"Pond FAF placement failed: {asset.Id}");
        }
        plan.AddRange(PlanSurfaceShore(basin, assets));
        return plan;
    }

    // Every anchor projects onto the baked terrain; there is no second bank mesh.
    public static IReadOnlyList<PondFafPlacement> PlanSurfaceShore(GeneratedBasin basin,
        IReadOnlyList<PondFafAsset> assets)
    {
        var plan = new List<PondFafPlacement>();
        var available = assets.Select(a => a.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var id in new[] { "FAF-PLANT-005A", "FAF-PLANT-006A", "FAF-PLANT-007B",
            "FAF-WOOD-004B", "FAF-WOOD-003A", "FAF-TERRAIN-002A", "FAF-TERRAIN-002B" })
            if (!available.Contains(id)) throw new InvalidDataException($"Pond shore asset missing: {id}");
        for (var index = 0; index < basin.ShorePointCount; index += 4)
        {
            var shore = basin.ShorePoint(index);
            var inward = new Vector2(
                basin.DistanceToShore(shore.X + 1f, shore.Y) - basin.DistanceToShore(shore.X - 1f, shore.Y),
                basin.DistanceToShore(shore.X, shore.Y + 1f) - basin.DistanceToShore(shore.X, shore.Y - 1f)).Normalized();
            if (inward.LengthSquared() < .5f) continue;
            var yaw = index * 2.3999632f;
            var wet = shore + inward * 1.25f;
            var dry = shore - inward * 2.0f;
            var wetId = index % 8 == 0 ? "FAF-PLANT-006A" : "FAF-PLANT-007B";
            plan.Add(new PondFafPlacement(wetId,
                new Vector3(wet.X, basin.HeightAt(wet.X, wet.Y) - .018f, wet.Y), yaw, .95f,
                "surface_shore"));
            plan.Add(new PondFafPlacement("FAF-PLANT-005A",
                new Vector3(dry.X, basin.HeightAt(dry.X, dry.Y) - .018f, dry.Y), yaw + .5f, 1f,
                "surface_shore"));
            if (index % 8 == 0)
                plan.Add(new PondFafPlacement("FAF-WOOD-004B",
                    new Vector3(dry.X, basin.HeightAt(dry.X, dry.Y) - .018f, dry.Y), yaw + 1f, .86f,
                    "surface_shore"));
            if (index % 16 == 0)
            {
                var trunk = shore - inward * 8f;
                plan.Add(new PondFafPlacement("FAF-WOOD-003A",
                    new Vector3(trunk.X, basin.HeightAt(trunk.X, trunk.Y) - .018f, trunk.Y), yaw, 1f,
                    "surface_shore"));
            }
            if (index % 32 == 0)
            {
                var bank = shore - inward * 1.1f;
                plan.Add(new PondFafPlacement(index % 64 == 0 ? "FAF-TERRAIN-002A" : "FAF-TERRAIN-002B",
                    new Vector3(bank.X, basin.HeightAt(bank.X, bank.Y) - .10f, bank.Y),
                    Mathf.Atan2(inward.X, inward.Y), .96f, "surface_shore"));
            }
        }
        return plan;
    }

    private static bool Supported(PondFafAsset asset, GeneratedBasin basin,
        float[] row, float yaw, float scale)
    {
        if (asset.Family is not ("hollow_trunk" or "fallen_log" or "stump" or "eroded_bank"))
            return true;
        var right = new Vector2(Mathf.Cos(yaw), -Mathf.Sin(yaw));
        var forward = new Vector2(Mathf.Sin(yaw), Mathf.Cos(yaw));
        var center = new Vector2(row[0], row[2]);
        var halfX = asset.Size[0] * scale * .35f;
        var halfZ = asset.Size[2] * scale * .35f;
        foreach (var offset in new[] { Vector2.Zero,
            right * halfX + forward * halfZ, -right * halfX + forward * halfZ,
            right * halfX - forward * halfZ, -right * halfX - forward * halfZ })
            if (Mathf.Abs(basin.HeightAt((center + offset).X, (center + offset).Y) - row[1]) >
                (asset.Family == "hollow_trunk" ? 1.5f : .8f)) return false;
        if (asset.Id == "FAF-WOOD-002A")
        {
            var fishY = row[1] - .018f + 2.52f * scale;
            for (var z = -5.4f; z <= 5.4f; z += .3f)
            {
                var at = center + forward * (z * scale);
                if (basin.HeightAt(at.X, at.Y) > fishY - GeneratedWorldRoutes.FishRadius - .35f)
                    return false;
            }
        }
        return true;
    }

    private static bool Suitable(PondFafAsset asset, float[] f)
    {
        if (f.Length != 17 || f.Any(v => !float.IsFinite(v)) ||
            (asset.Family != "tall_grass" && f[9] < 1.5f) || f[11] < 4.2f ||
            f[4] < .78f || f[1] > GeneratedBasin.WaterSurfaceY +
            (asset.Family == "tall_grass" ? 2.5f : 1.5f)) return false;
        var depth = f[6];
        return asset.Family switch
        {
            "lily_pad" => Mathf.Abs(depth - asset.StemDepth) < .6f && f[11] > 5f,
            "floating_muck" => depth is > 1.0f and < 8f && f[11] > 5f,
            "tall_grass" => depth < .2f && f[9] < 8f,
            "cattail" or "reed" => depth is > .15f and < 1.9f && f[9] < 12f,
            "submerged_algae" => depth is > 1f and < 8f,
            "hollow_trunk" => depth is > 5.5f and < 10.5f && f[4] > .96f &&
                f[11] > 6f && f[11] < 14f,
            "fallen_log" or "stump" => depth is > 1f and < 10f && f[4] > .9f && f[11] > 7f,
            "eroded_bank" => depth < 1.5f && f[9] < 10f && f[4] > .9f,
            "cluster" => depth is > .6f and < 9f && f[11] > 6f,
            _ => depth is > .2f and < 9f,
        };
    }

    private static int Count(PondFafAsset asset) => asset.Family switch
    {
        "lily_pad" => 95, "floating_muck" => 32,
        "tall_grass" or "cattail" or "reed" => 45,
        "submerged_algae" => 55,
        "hollow_trunk" => asset.Id == "FAF-WOOD-002A" ? 1 : 2,
        "fallen_log" or "stump" => 3, "cluster" => 12,
        "eroded_bank" => 4, _ => 25,
    };

    public static Node3D Install(Node3D habitat, FullWorldData data, ulong seed)
    {
        if (habitat.GetNodeOrNull<Node3D>("PondFafEnvironment") is not null)
            throw new InvalidOperationException("Pond FAF already installed");
        var assets = LoadCatalog();
        var plan = Plan(data, habitat.GetNode<SeabedSurface>("Seabed").Basin, assets);
        var root = new Node3D { Name = "PondFafEnvironment" };
        habitat.AddChild(root);
        var basinShader = ResourceLoader.Load<Shader>(
            "res://assets/faf/pond/materials/pond_basin_variation.gdshader")
            ?? throw new InvalidDataException("Pond FAF silt/mud basin shader unavailable");
        habitat.GetNode<MeshInstance3D>("Seabed/Mesh").MaterialOverride =
            new ShaderMaterial { Shader = basinShader, ResourceName = "Pond silt/mud basin" };
        var batches = new Dictionary<string, List<Transform3D>>(StringComparer.Ordinal);
        var byId = assets.ToDictionary(a => a.Id, StringComparer.Ordinal);
        var readabilityMaterials = new Dictionary<(ulong ResourceId, string Family), StandardMaterial3D>();
        foreach (var placement in plan)
        {
            var asset = byId[placement.Id];
            var transform = new Transform3D(new Basis(Vector3.Up, placement.Yaw)
                .Scaled(Vector3.One * placement.Scale), placement.Position);
            if (asset.Family is "hollow_trunk" or "fallen_log" or "stump" or "eroded_bank")
            {
                var path = asset.Wrapper ?? throw new InvalidDataException($"Missing Pond structural wrapper: {asset.Id}");
                var piece = ResourceLoader.Load<PackedScene>(path)!.Instantiate<Node3D>();
                piece.Name = asset.Id.Replace('-', '_') + "_" + root.GetChildCount();
                piece.Transform = transform;
                root.AddChild(piece);
                if (asset.Family is "hollow_trunk" or "fallen_log" or "stump")
                    foreach (var visual in FindMeshes(piece))
                        for (var surface = 0; surface < visual.Mesh!.GetSurfaceCount(); surface++)
                            if (visual.GetActiveMaterial(surface) is StandardMaterial3D original)
                                visual.SetSurfaceOverrideMaterial(surface,
                                    ReadabilityMaterial(original, asset.Family, readabilityMaterials));
            }
            else
            {
                if (!batches.TryGetValue(asset.Id, out var transforms))
                    batches[asset.Id] = transforms = new List<Transform3D>();
                transforms.Add(transform);
            }
        }
        foreach (var (id, transforms) in batches.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            var asset = byId[id];
            var source = ResourceLoader.Load<PackedScene>(asset.Glb)!.Instantiate<Node3D>();
            var meshes = FindMeshes(source).ToArray();
            if (meshes.Length == 0) throw new InvalidDataException($"Pond FAF imported mesh missing: {id}");
            for (var part = 0; part < meshes.Length; part++)
            {
                var mesh = meshes[part].Mesh ?? throw new InvalidDataException($"Pond FAF mesh missing: {id}");
                var batch = new MultiMesh { TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
                    Mesh = mesh, InstanceCount = transforms.Count, VisibleInstanceCount = transforms.Count };
                for (var i = 0; i < transforms.Count; i++)
                    batch.SetInstanceTransform(i, transforms[i] * meshes[part].Transform);
                var node = new MultiMeshInstance3D { Name = id.Replace('-', '_') + "_" + part,
                    Multimesh = batch };
                if (asset.Family is "lily_pad" or "tall_grass" or "cattail" or "reed" or
                    "submerged_algae" or "floating_muck")
                    if (meshes[part].GetActiveMaterial(0) is StandardMaterial3D original)
                        node.MaterialOverride = ReadabilityMaterial(original, asset.Family, readabilityMaterials);
                node.SetMeta("faf_id", id);
                node.SetMeta("faf_habitat", WorldCatalog.PondId);
                node.SetMeta("placement_count", transforms.Count);
                root.AddChild(node);
            }
            source.Free();
        }
        root.SetMeta("placement_count", plan.Count);
        root.SetMeta("surface_shore_count", plan.Count(p => p.Zone == "surface_shore"));
        root.SetMeta("catalog_count", assets.Count);
        GD.Print($"POND_FAF_READY assets={assets.Count} placements={plan.Count} batches={batches.Count} " +
            $"triangles_est={plan.Sum(p => byId[p.Id].Triangles)}");
        return root;
    }

    private static IEnumerable<MeshInstance3D> FindMeshes(Node node)
    {
        if (node is MeshInstance3D mesh) yield return mesh;
        foreach (var child in node.GetChildren())
            foreach (var found in FindMeshes(child)) yield return found;
    }

    // Scene-local materials preserve the owner GLBs and their named surface palette.
    // They alter neither source art nor collision and add no transparent layer.
    private static StandardMaterial3D ReadabilityMaterial(StandardMaterial3D original, string family,
        Dictionary<(ulong ResourceId, string Family), StandardMaterial3D> cache)
    {
        var key = (original.GetInstanceId(), family);
        if (cache.TryGetValue(key, out var shared)) return shared;
        var material = (StandardMaterial3D)original.Duplicate();
        var name = original.ResourceName.ToString();
        var factor = family switch
        {
            "hollow_trunk" or "fallen_log" or "stump" when name.Contains("dark_bark") => 1.22f,
            "hollow_trunk" or "fallen_log" or "stump" when name.Contains("bark_ochre") => 1.62f,
            "hollow_trunk" or "fallen_log" or "stump" when name.Contains("warm_break") => 1.44f,
            "hollow_trunk" or "fallen_log" or "stump" when name.Contains("algae_olive") => 1.30f,
            "submerged_algae" => 1.65f,
            "cattail" or "reed" => 1.32f,
            "lily_pad" => 1.17f,
            "tall_grass" => 1.15f,
            "floating_muck" => 1.10f,
            _ => 1f,
        };
        var baseColor = original.AlbedoColor;
        material.AlbedoColor = new Color(
            Mathf.Min(baseColor.R * factor, 1f),
            Mathf.Min(baseColor.G * factor, 1f),
            Mathf.Min(baseColor.B * factor, 1f), baseColor.A);
        // All parts remain opaque and matte, with just enough response for bark
        // facets and plant paddles to separate under the existing Pond Sun.
        material.Roughness = family is "hollow_trunk" or "fallen_log" or "stump"
            ? Mathf.Min(original.Roughness, .90f) : original.Roughness;
        cache[key] = material;
        return material;
    }
}
