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
    public const int Seed = 5266;
    public const float WaterY = GeneratedBasin.WaterSurfaceY;
    public const string WorldPath = "res://worlds/encounters/pike-m1-5266";
    public const string CatalogPath = "res://assets/faf/pond/p5_cover_catalog.json";
    private static readonly GeneratedBasin PondBasin = new(Seed, pond: true);
    // Repeated fixture retry reuses the same imported scene wrappers. Keeping
    // them alive also avoids a Godot/.NET finalizer race during rapid scene churn.
    private static readonly Dictionary<string, PackedScene> ImportedScenes = new();

    private static PackedScene? Scene(string path)
    {
        if (ImportedScenes.TryGetValue(path, out var scene)) return scene;
        scene = ResourceLoader.Load<PackedScene>(path);
        if (scene is not null) ImportedScenes[path] = scene;
        return scene;
    }

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
            if (Scene(asset.Glb) is null ||
                (asset.Wrapper is not null && Scene(asset.Wrapper) is null))
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
        => PondBasin.HeightAt(x, z);

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
            if (id.StartsWith("FAF-WOOD-", StringComparison.Ordinal))
            {
                var best = float.PositiveInfinity;
                var chosen = (x, z, yaw);
                for (var dz = -12; dz <= 12; dz++)
                for (var dx = -12; dx <= 12; dx++)
                for (var turn = -6; turn <= 6; turn++)
                {
                    var cx = x + dx; var cz = z + dz; var angle = yaw + turn * .15f;
                    if (PondBasin.DistanceToShore(cx, cz) < 3f ||
                        WaterY - Bed(cx, cz) < 3.5f) continue;
                    var axis = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                    var centerY = Bed(cx, cz);
                    var support = Mathf.Max(
                        Mathf.Abs(Bed(cx + axis.X * asset.Size[1] * .35f,
                            cz + axis.Y * asset.Size[1] * .35f) - centerY),
                        Mathf.Abs(Bed(cx - axis.X * asset.Size[1] * .35f,
                            cz - axis.Y * asset.Size[1] * .35f) - centerY));
                    if (support > .75f) continue;
                    var score = Mathf.Sqrt(dx * dx + dz * dz) + 2f * support + .12f * Mathf.Abs(turn);
                    if (score >= best) continue;
                    best = score;
                    chosen = (cx, cz, angle);
                }
                if (!float.IsFinite(best)) throw new InvalidDataException($"P5 wood lacks Pond terrain support: {id}");
                (x, z, yaw) = chosen;
            }
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
        scene.AddChild(new WorldEnvironment { Name = "WorldEnvironment" });
        scene.AddChild(new EnvironmentClock { Name = "EnvironmentClock" });
        var habitat = Scene("res://scenes/pond_habitat.tscn")?.Instantiate<Node3D>()
            ?? throw new InvalidDataException("Pond habitat scene missing");
        habitat.Name = "Habitat";
        scene.AddChild(habitat);
        var loaded = FullWorldPackage.Load(WorldPath, habitat, WorldCatalog.PondId);
        if (loaded.Manifest.Seed != Seed || loaded.Manifest.GeneratorVersion != FullWorldPackage.PondGeneratorVersion)
            throw new InvalidDataException("Pike fixture Pond generation identity mismatch");
        // The encounter's ten new wood families own its hard-cover gaps. Keep
        // generated Pond foliage, shore dressing and terrain, but remove the
        // ordinary Pond's older wood wrappers so they cannot fill those gaps.
        var ordinaryDressing = habitat.GetNode<Node3D>("PondFafEnvironment");
        foreach (Node child in ordinaryDressing.GetChildren())
            if (child.Name.ToString().StartsWith("FAF_WOOD_", StringComparison.Ordinal))
            { ordinaryDressing.RemoveChild(child); child.Free(); }
        var atmosphere = new UnderwaterAtmosphere { Name = "UnderwaterAtmosphere" };
        scene.AddChild(atmosphere);
        atmosphere.ConfigureProfile(WorldEnvironmentProfile.Pond);
        atmosphere.Initialize(scene);
        var collection = new Node3D { Name = "Cover" };
        scene.AddChild(collection);
        var byId = catalog.Assets.ToDictionary(a => a.Id);
        foreach (var placement in plan)
        {
            var asset = byId[placement.Id];
            var path = asset.Wrapper ?? asset.Glb;
            var node = Scene(path)!.Instantiate<Node3D>();
            node.Name = placement.Id.Replace("-", "_") + "_" + collection.GetChildCount();
            collection.AddChild(node);
            node.Position = placement.Position;
            node.Rotation = new Vector3(0, placement.Yaw, 0);
            node.Scale = Vector3.One * placement.Scale;
            node.SetMeta("faf_id", placement.Id);
            node.SetMeta("zone", placement.Zone);
        }

    }

    public static Vector3 ConcealedSpawn(IReadOnlyList<PikeCoverPlacement> plan, Vector3 pikePosition)
    {
        var log = plan.Single(p => p.Id == "FAF-WOOD-005B");
        var center = new Vector2(log.Position.X, log.Position.Z);
        var away = (center - new Vector2(pikePosition.X, pikePosition.Z)).Normalized();
        var point = center + away * 5.5f;
        return new Vector3(point.X, Bed(point.X, point.Y) + 1.35f, point.Y);
    }

    public static IReadOnlyList<SoftSightCover> VisibleSoftCover(Node3D collection)
    {
        var result = new List<SoftSightCover>();
        foreach (Node3D placement in collection.GetChildren())
        {
            var id = placement.GetMeta("faf_id").AsString();
            if (!id.StartsWith("FAF-PLANT-", StringComparison.Ordinal)) continue;
            Collect(placement, id);
        }
        return result;

        void Collect(Node node, string id)
        {
            if (node is MeshInstance3D { Mesh: not null } mesh && mesh.IsVisibleInTree())
            {
                var local = mesh.GetAabb();
                var min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
                var max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
                for (var mask = 0; mask < 8; mask++)
                {
                    var corner = local.Position + new Vector3(
                        (mask & 1) == 0 ? 0 : local.Size.X,
                        (mask & 2) == 0 ? 0 : local.Size.Y,
                        (mask & 4) == 0 ? 0 : local.Size.Z);
                    var world = mesh.GlobalTransform * corner;
                    min = min.Min(world);
                    max = max.Max(world);
                }
                result.Add(new SoftSightCover(id, new Aabb(min, max - min)));
            }
            foreach (Node child in node.GetChildren()) Collect(child, id);
        }
    }

    public static float FixtureSediment(Vector3 position)
    {
        if (position.Y >= WaterY || position.Y <= Bed(position.X, position.Z)) return 0f;
        var aboveBed = position.Y - Bed(position.X, position.Z);
        var deep = Mathf.Clamp((WaterY - position.Y - 3f) / 5f, 0f, 1f);
        return .06f + .23f * (1f - Mathf.Clamp(aboveBed / 2f, 0f, 1f)) + .10f * deep;
    }
}
