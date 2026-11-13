#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace Fish;

// This scene-local registry is a snapshot of the two owner FAF manifests. It never
// installs the expansion into the generated Pond or the Ocean.
public sealed record PikeCoverAsset(string Id, string Family, string Source, string SourceSha256,
    string Glb, string GlbSha256, string? Wrapper, int Seed, float[] Size, int Triangles,
    int Materials, int CollisionShapes);
public sealed record PikeCoverCatalog(PikeCoverAsset[] Assets);
public sealed record PikeCoverPlacement(string Id, string Zone, Vector3 Position, float Yaw,
    float Scale, float BedY, float TipY);

public static class PikeCoverFixture
{
    public const int Seed = 5202;
    public const float WaterY = 3.5f;
    public const string CatalogPath = "res://assets/faf/pond/p5_cover_catalog.json";

    // Locations are authored around one continuous bed, with the central x=0
    // crossing and two outside lanes kept open. Each source variant has a role.
    private static readonly (string id, float x, float z, float yaw)[] Anchors =
    {
        ("FAF-PLANT-010A", -7.2f, 23.0f, .15f),
        ("FAF-PLANT-010B", -8.5f, 19.6f, -.3f),
        ("FAF-PLANT-010C", -8.7f, 15.5f, .32f),
        ("FAF-PLANT-011A", -8.2f, 5.1f, .3f),
        ("FAF-PLANT-011B", 7.7f, 3.4f, -.4f),
        ("FAF-PLANT-011C", -7.4f, -2.4f, .1f),
        ("FAF-PLANT-012A", -6.9f, 13.0f, .5f),
        ("FAF-PLANT-012B", 8.4f, -2.3f, -.2f),
        ("FAF-PLANT-012C", -9.2f, -5.4f, .28f),
        ("FAF-PLANT-013A", 9.1f, 22.5f, .45f),
        ("FAF-PLANT-013B", 7.5f, 12.6f, -.15f),
        ("FAF-PLANT-014A", -8.5f, 0.5f, -.35f),
        ("FAF-PLANT-014B", 9.8f, -5.0f, .25f),
        ("FAF-WOOD-005A", 11.3f, 19.0f, 0f),
        ("FAF-WOOD-005B", -14.1f, 19.2f, 0f),
        ("FAF-WOOD-006A", -8.1f, 2.6f, 0f),
        ("FAF-WOOD-006B", 9.1f, 3.2f, 0f),
        ("FAF-WOOD-007A", 18.5f, .0f, .24f),
        ("FAF-WOOD-007B", -18.5f, .0f, -.24f),
        ("FAF-WOOD-008A", -8.0f, -17.8f, .08f),
        ("FAF-WOOD-008B", 8.2f, -18.2f, -.08f),
        ("FAF-WOOD-009A", 1.0f, -24.5f, .05f),
        ("FAF-WOOD-009B", 12.8f, -25.0f, -.2f),
    };

    public static PikeCoverCatalog LoadCatalog()
    {
        var catalog = JsonSerializer.Deserialize<PikeCoverCatalog>(File.ReadAllBytes(
            ProjectSettings.GlobalizePath(CatalogPath))) ?? throw new InvalidDataException("P5 cover catalog empty");
        if (catalog.Assets.Length != 23 || catalog.Assets.Select(a => a.Id).Distinct().Count() != 23 ||
            catalog.Assets.Count(a => a.Id.StartsWith("FAF-PLANT-", StringComparison.Ordinal)) != 13 ||
            catalog.Assets.Count(a => a.Id.StartsWith("FAF-WOOD-", StringComparison.Ordinal)) != 10)
            throw new InvalidDataException("P5 cover requires 13 plants and 10 wood variants");
        foreach (var asset in catalog.Assets)
        {
            if (asset.Size.Length != 3 || asset.Size.Any(n => n <= 0 || !float.IsFinite(n)) ||
                asset.Triangles <= 0 || asset.Materials <= 0 || asset.Seed <= 0 ||
                !asset.Glb.StartsWith("res://assets/faf/pond/", StringComparison.Ordinal) ||
                (asset.Id.StartsWith("FAF-WOOD-", StringComparison.Ordinal) != (asset.Wrapper is not null)))
                throw new InvalidDataException("P5 cover metadata invalid: " + asset.Id);
            CheckHash(asset.Source, asset.SourceSha256, asset.Id);
            CheckHash(asset.Glb, asset.GlbSha256, asset.Id);
            if (ResourceLoader.Load<PackedScene>(asset.Glb) is null ||
                (asset.Wrapper is not null && ResourceLoader.Load<PackedScene>(asset.Wrapper) is null))
                throw new InvalidDataException("P5 cover import missing: " + asset.Id);
        }
        var grass = catalog.Assets.Where(a => a.Family == "ribbon_grass")
            .OrderBy(a => a.Size[2]).ToArray();
        if (grass.Length != 3 || grass[0].Size[2] is < .5f or > 1f ||
            grass[1].Size[2] is < 1f or > 2f || grass[2].Size[2] < 3.5f)
            throw new InvalidDataException("P5 ribbon grass height classes missing");
        return catalog;
    }

    private static void CheckHash(string path, string expected, string id)
    {
        var absolute = path.StartsWith("res://", StringComparison.Ordinal)
            ? ProjectSettings.GlobalizePath(path)
            : Path.GetFullPath(Path.Combine(ProjectSettings.GlobalizePath("res://"), "..", path));
        if (!File.Exists(absolute) || !Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(absolute)))
                .Equals(expected, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("P5 cover source/export changed: " + id + " " + path);
    }

    public static float Bed(float x, float z)
    {
        // Broad, connected terrace transitions plus small deterministic mud relief.
        var mid = Mathf.SmoothStep(0f, 1f, Mathf.Clamp((12f - z) / 7f, 0f, 1f));
        var deep = Mathf.SmoothStep(0f, 1f, Mathf.Clamp((-7f - z) / 8f, 0f, 1f));
        var bank = Mathf.Pow(Mathf.Clamp((Mathf.Abs(x) - 14f) / 9f, 0f, 1f), 2f) * 1.9f;
        return -.24f - 1.95f * mid - 2.55f * deep + bank +
            .09f * Mathf.Sin(.28f * x + .19f * z) * Mathf.Cos(.21f * z);
    }

    public static IReadOnlyList<PikeCoverPlacement> Plan(PikeCoverCatalog catalog)
    {
        var byId = catalog.Assets.ToDictionary(a => a.Id);
        if (Anchors.Any(a => !byId.ContainsKey(a.id))) throw new InvalidDataException("P5 anchor asset missing");
        var placed = new List<PikeCoverPlacement>();
        var random = new Random(Seed);
        foreach (var (id, x, z, yaw) in Anchors)
        {
            Add(id, x, z, yaw, 1f);
            if (!id.StartsWith("FAF-PLANT-", StringComparison.Ordinal)) continue;
            // Separate irregular clumps, with a clear central crossing and no
            // repeated grid. Tall grass is restricted to the level shallow bed.
            var extra = id is "FAF-PLANT-010C" or "FAF-PLANT-014B" ? 1 : 2;
            for (var i = 0; i < extra; i++)
            {
                var side = x < 0 ? -1f : 1f;
                var px = x + side * (1.7f + (float)random.NextDouble() * 1.9f);
                var pz = z + (float)(random.NextDouble() * 3.5 - 1.75);
                if (id == "FAF-PLANT-010C") pz = Mathf.Max(pz, 15f);
                Add(id, px, pz, yaw + (float)(random.NextDouble() - .5) * .9f,
                    .93f + (float)random.NextDouble() * .13f);
            }
        }
        return placed;

        void Add(string id, float x, float z, float yaw, float scale)
        {
            var asset = byId[id];
            var bed = Bed(x, z);
            var tip = bed + asset.Size[2] * scale;
            if (tip > WaterY + .12f && id.StartsWith("FAF-PLANT-", StringComparison.Ordinal))
                throw new InvalidDataException("P5 foliage above waterline: " + id);
            placed.Add(new PikeCoverPlacement(id, z > 9f ? "nursery" : z > -10f ? "refuge" : "maze",
                new Vector3(x, bed - .035f, z), yaw, scale, bed, tip));
        }
    }

    public static void Build(Node3D scene, PikeCoverCatalog catalog, IReadOnlyList<PikeCoverPlacement> plan)
    {
        var atmosphere = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.Color,
            BackgroundColor = new Color(.13f, .21f, .16f),
            AmbientLightSource = Godot.Environment.AmbientSource.Color,
            AmbientLightColor = new Color(.57f, .65f, .46f),
            AmbientLightEnergy = .55f,
            FogEnabled = true,
            FogLightColor = new Color(.2f, .32f, .23f),
            FogDensity = .008f,
        };
        scene.AddChild(new WorldEnvironment { Name = "PondColumnLight", Environment = atmosphere });
        var water = new MeshInstance3D { Name = "Waterline",
            Mesh = new PlaneMesh { Size = new Vector2(80, 80) },
            Position = new Vector3(0, WaterY, 0) };
        water.MaterialOverride = new StandardMaterial3D
        {
            AlbedoColor = new Color(.19f, .37f, .29f, .12f),
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
        };
        scene.AddChild(water);
        var ground = new StaticBody3D { Name = "ConnectedMudBed", CollisionLayer = 1, CollisionMask = 0 };
        scene.AddChild(ground);
        var surface = new SurfaceTool();
        surface.Begin(Mesh.PrimitiveType.Triangles);
        for (var z = -38; z < 38; z++)
            for (var x = -38; x < 38; x++)
            {
                var a = new Vector3(x, Bed(x, z), z);
                var b = new Vector3(x + 1, Bed(x + 1, z), z);
                var c = new Vector3(x + 1, Bed(x + 1, z + 1), z + 1);
                var d = new Vector3(x, Bed(x, z + 1), z + 1);
                AddMudVertex(a); AddMudVertex(d); AddMudVertex(b);
                AddMudVertex(b); AddMudVertex(d); AddMudVertex(c);
            }
        surface.GenerateNormals();
        var mesh = surface.Commit();
        var mud = new StandardMaterial3D
        {
            AlbedoColor = Colors.White, Roughness = .96f,
            VertexColorUseAsAlbedo = true,
        };
        var visual = new MeshInstance3D { Name = "Mud", Mesh = mesh, MaterialOverride = mud };
        ground.AddChild(visual);
        ground.AddChild(new CollisionShape3D { Name = "MudCollision", Shape = mesh.CreateTrimeshShape() });
        var collection = new Node3D { Name = "Cover" };
        scene.AddChild(collection);
        var byId = catalog.Assets.ToDictionary(a => a.Id);
        foreach (var placement in plan)
        {
            var asset = byId[placement.Id];
            var path = asset.Wrapper ?? asset.Glb;
            var node = ResourceLoader.Load<PackedScene>(path)!.Instantiate<Node3D>();
            node.Name = placement.Id.Replace("-", "_") + "_" + collection.GetChildCount();
            collection.AddChild(node);
            node.Position = placement.Position;
            node.Rotation = new Vector3(0, placement.Yaw, 0);
            node.Scale = Vector3.One * placement.Scale;
            node.SetMeta("faf_id", placement.Id);
            node.SetMeta("zone", placement.Zone);
        }

        void AddMudVertex(Vector3 point)
        {
            var depth = Mathf.Clamp((WaterY - point.Y - 3.7f) / 5f, 0f, 1f);
            var shore = Mathf.Clamp((Mathf.Abs(point.X) - 13f) / 12f, 0f, 1f);
            var color = new Color(.30f, .285f, .20f).Lerp(
                new Color(.13f, .18f, .135f), depth);
            surface.SetColor(color.Lerp(new Color(.33f, .29f, .19f), shore * .35f));
            surface.AddVertex(point);
        }
    }
}
