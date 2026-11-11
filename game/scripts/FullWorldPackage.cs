#nullable enable
using System;
using System.Collections.Generic;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Godot;

namespace Fish;

public sealed record FullWorldResource(string Path, string Sha256);
public sealed record FullWorldManifest(int FormatVersion, string WorldId, ulong Seed,
    int SettingsVersion, string GeneratorVersion, string CatalogSha256, string ContentSha256,
    IReadOnlyList<FullWorldResource> Resources, IReadOnlyList<WorldStageTiming> Timings,
    int RouteNodes, int RouteEdges, int EnvironmentSamples, int Placements,
    int GeologyRegions, int AmbientFish);
public sealed record BakedRouteNode(string Id, float[] Position, string Role,
    float FishRadius, float CameraRadius, float CameraOffsetY);
public sealed record BakedRouteEdge(string Id, string From, string To, string Kind,
    float[][] Points, float ProtectedRadius, float CameraProtectedRadius);
public sealed record BakedPlacement(string AssetId, string Role, string DepthBand,
    float[] Position, float Radius);
public sealed record FullWorldData(IReadOnlyList<BakedRouteNode> Nodes,
    IReadOnlyList<BakedRouteEdge> Edges, IReadOnlyList<float[]> Environment,
    IReadOnlyList<BakedPlacement> Placements, FafAmbientReefLife.BakedAmbient Ambient);

public static class WorldGenerationAudit
{
    public static int StageInvocations { get; private set; }
    public static void MarkStage() => StageInvocations++;
}

/// <summary>Finite scene and analysis snapshot. Loading never calls a world generator.</summary>
public static class FullWorldPackage
{
    public const string GeneratorVersion = "c3-full-1";
    public const int SettingsVersion = 1;
    public const string MeshFile = "seabed-mesh.res";
    public const string CollisionFile = "seabed-collision.res";
    public const string GeologyFile = "geology.scn";
    public const string DressingFile = "dressing.scn";
    public const string AmbientFile = "ambient.scn";
    public const string DataFile = "world-data.json";
    public const string ManifestFile = "manifest.json";
    public const string DefaultBase = "res://worlds/base/qualification-42";
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public static FullWorldManifest Bake(Node3D habitat, ulong seed, string directory,
        IReadOnlyList<WorldStageTiming> generationTimings)
    {
        if (System.IO.Directory.Exists(ProjectSettings.GlobalizePath(directory)) &&
            System.IO.Directory.EnumerateFileSystemEntries(ProjectSettings.GlobalizePath(directory)).Any())
            throw new IOException($"World package destination is not empty: {directory}");
        System.IO.Directory.CreateDirectory(ProjectSettings.GlobalizePath(directory));
        var timer = Stopwatch.StartNew();
        var seabed = habitat.GetNode<SeabedSurface>("Seabed");
        var reef = habitat.GetNode<ReefComposition>("Composition/ReefComposition");
        var geology = reef.GetNode<FafGeologyWorld>("Geology");
        var dressing = reef.GetNode<FafReefDressing>("Dressing");
        var ambient = reef.GetNode<FafAmbientReefLife>("AmbientLife");
        var routes = reef.GeneratedRoutes ?? throw new InvalidDataException("Missing generated routes");
        var environment = reef.EnvironmentAnalysis ?? throw new InvalidDataException("Missing environment analysis");
        if (seabed.Basin.Seed != seed || geology.GeneratedRegions.Count != 5 ||
            dressing.Placements.Count < 100 || routes.Nodes.Count < 20 ||
            environment.CachedSampleCount != 1681)
            throw new InvalidDataException("Incomplete full world cannot be baked");
        var data = new FullWorldData(
            routes.Nodes.Values.OrderBy(n => n.Id, StringComparer.Ordinal)
                .Select(n => new BakedRouteNode(n.Id, Vec(n.Position), n.Role,
                    n.FishRadius, n.CameraRadius, n.CameraOffsetY)).ToArray(),
            routes.Edges.Select(e => new BakedRouteEdge(e.Id, e.From, e.To, e.Kind,
                e.Points.Select(Vec).ToArray(), e.ProtectedRadius,
                e.CameraProtectedRadius)).ToArray(),
            Enumerable.Range(0, WorldEnvironmentAnalysis.CellsPerSide * WorldEnvironmentAnalysis.CellsPerSide)
                .Select(i => Field(environment.SampleGrid(i % WorldEnvironmentAnalysis.CellsPerSide,
                    i / WorldEnvironmentAnalysis.CellsPerSide))).ToArray(),
            dressing.Placements.Select(p => new BakedPlacement(p.Asset.Id, p.Role.ToString(),
                p.DepthBand, Vec(p.Position), p.Radius)).ToArray(), ambient.ExportBaked());
        var dataBytes = JsonSerializer.SerializeToUtf8Bytes(data, Json);
        System.IO.File.WriteAllBytes(ProjectSettings.GlobalizePath(directory + "/" + DataFile), dataBytes);
        var dataMilliseconds = timer.ElapsedMilliseconds;
        GD.Print($"WORLD_STAGE data-serialization elapsed_ms={dataMilliseconds}");
        timer.Restart();
        SaveResource(seabed.GeneratedMesh ?? throw new InvalidDataException("Missing basin mesh"),
            directory + "/" + MeshFile);
        SaveResource(seabed.GeneratedCollision ?? throw new InvalidDataException("Missing basin collision"),
            directory + "/" + CollisionFile);
        SaveSubscene(geology.GetNode<Node3D>("GeneratedRegions"), directory + "/" + GeologyFile);
        SaveSubscene(dressing.GetNode<Node3D>("Visuals"), directory + "/" + DressingFile);
        SaveSubscene(ambient.GetNode<Node3D>("Visuals"), directory + "/" + AmbientFile);
        GD.Print($"WORLD_STAGE scene-serialization elapsed_ms={timer.ElapsedMilliseconds}");
        var resources = new[] { MeshFile, CollisionFile, GeologyFile, DressingFile, AmbientFile, DataFile }
            .Select(name => new FullWorldResource(name, BakedWorldContract.Hash(
                System.IO.File.ReadAllBytes(ProjectSettings.GlobalizePath(directory + "/" + name)))))
            .ToArray();
        var catalogHash = WorldCompiler.CatalogIdentity(FafRuntimeCatalog.Environment);
        var timings = generationTimings.Concat(new[] {
            new WorldStageTiming("data-serialization", dataMilliseconds),
            new WorldStageTiming("scene-serialization", timer.ElapsedMilliseconds) }).ToArray();
        var manifest = new FullWorldManifest(1,
            BakedWorldContract.WorldId(GeneratorVersion, SettingsVersion, seed, catalogHash),
            seed, SettingsVersion, GeneratorVersion, catalogHash,
            BakedWorldContract.Hash(string.Join("|", resources.Select(r => r.Sha256))),
            resources, timings, data.Nodes.Count, data.Edges.Count, data.Environment.Count,
            data.Placements.Count, geology.GeneratedRegions.Count, data.Ambient.Fish.Count);
        System.IO.File.WriteAllBytes(ProjectSettings.GlobalizePath(directory + "/" + ManifestFile),
            JsonSerializer.SerializeToUtf8Bytes(manifest, Json));
        return manifest;
    }

    public static (FullWorldManifest Manifest, FullWorldData Data) Load(string directory, Node3D habitat)
    {
        var generationBefore = WorldGenerationAudit.StageInvocations;
        var timer = Stopwatch.StartNew();
        var path = ProjectSettings.GlobalizePath(directory);
        var manifest = JsonSerializer.Deserialize<FullWorldManifest>(
            System.IO.File.ReadAllBytes(System.IO.Path.Combine(path, ManifestFile)))
            ?? throw new InvalidDataException("Empty full-world manifest");
        var catalogHash = WorldCompiler.CatalogIdentity(FafRuntimeCatalog.Environment);
        if (manifest.FormatVersion != 1 || manifest.GeneratorVersion != GeneratorVersion ||
            manifest.SettingsVersion != SettingsVersion || manifest.CatalogSha256 != catalogHash ||
            manifest.WorldId != BakedWorldContract.WorldId(GeneratorVersion, SettingsVersion,
            manifest.Seed, catalogHash) || manifest.Resources.Count != 6 ||
            !manifest.Resources.Select(r => r.Path).SequenceEqual(
                new[] { MeshFile, CollisionFile, GeologyFile, DressingFile, AmbientFile, DataFile }))
            throw new InvalidDataException("Incompatible full-world manifest");
        foreach (var resource in manifest.Resources)
        {
            var bytes = System.IO.File.ReadAllBytes(System.IO.Path.Combine(path, resource.Path));
            if (BakedWorldContract.Hash(bytes) != resource.Sha256)
                throw new InvalidDataException($"Full-world resource hash mismatch: {resource.Path}");
        }
        if (BakedWorldContract.Hash(string.Join("|", manifest.Resources.Select(r => r.Sha256))) !=
            manifest.ContentSha256) throw new InvalidDataException("Full-world content identity mismatch");
        var data = JsonSerializer.Deserialize<FullWorldData>(
            System.IO.File.ReadAllBytes(System.IO.Path.Combine(path, DataFile)))
            ?? throw new InvalidDataException("Empty full-world data");
        if (data.Nodes.Count != manifest.RouteNodes || data.Edges.Count != manifest.RouteEdges ||
            data.Environment.Count != manifest.EnvironmentSamples ||
            data.Placements.Count != manifest.Placements || data.Ambient.Fish.Count != manifest.AmbientFish ||
            manifest.GeologyRegions != 5 || manifest.EnvironmentSamples != 1681)
            throw new InvalidDataException("Full-world data summary mismatch");
        var seabed = habitat.GetNode<SeabedSurface>("Seabed");
        var mesh = ResourceLoader.Load<ArrayMesh>(directory + "/" + MeshFile)
            ?? throw new InvalidDataException("Baked basin mesh missing");
        var collision = ResourceLoader.Load<ConcavePolygonShape3D>(directory + "/" + CollisionFile)
            ?? throw new InvalidDataException("Baked basin collision missing");
        seabed.InstallBaked(mesh, collision, manifest.Seed);
        var reef = habitat.GetNode<ReefComposition>("Composition/ReefComposition");
        var geology = reef.GetNode<FafGeologyWorld>("Geology");
        var dressing = reef.GetNode<FafReefDressing>("Dressing");
        var ambient = reef.GetNode<FafAmbientReefLife>("AmbientLife");
        geology.AddChild(LoadSubscene(directory, GeologyFile));
        ReplaceVisuals(dressing, LoadSubscene(directory, DressingFile));
        ReplaceVisuals(ambient, LoadSubscene(directory, AmbientFile));
        if (seabed.GeneratedMesh is null || seabed.GeneratedCollision is null ||
            geology.GetNode<Node3D>("GeneratedRegions").GetChildCount() != 5 ||
            dressing.GetNode<Node3D>("Visuals").GetChildCount() < 30 ||
            ambient.GetNode<Node3D>("Visuals").GetChildCount() != manifest.AmbientFish)
            throw new InvalidDataException("Full-world geometry or collision is missing");
        ambient.RestoreBaked(data.Ambient, seabed);
        reef.InstallBakedData(data);
        var generationDuringLoad = WorldGenerationAudit.StageInvocations - generationBefore;
        if (generationDuringLoad != 0)
            throw new InvalidOperationException($"World generator ran during baked load: {generationDuringLoad} stages");
        GD.Print($"WORLD_BAKED_LOAD_OK id={manifest.WorldId} seed={manifest.Seed} content={manifest.ContentSha256} elapsed_ms={timer.ElapsedMilliseconds} generator_invocations={generationDuringLoad}");
        return (manifest, data);
    }

    private static void SaveResource(Resource resource, string path)
    {
        NormalizeIds(resource);
        if (ResourceSaver.Save(resource, path) != Error.Ok)
            throw new IOException($"Could not save baked resource: {path}");
    }

    private static void SaveSubscene(Node3D root, string path)
    {
        AssignMissingOwners(root, root);
        NormalizeIds(root);
        var packed = new PackedScene();
        if (packed.Pack(root) != Error.Ok)
            throw new IOException($"Could not pack baked scene: {path}");
        packed.ResourceSceneUniqueId = "root0001";
        if (ResourceSaver.Save(packed, path) != Error.Ok)
            throw new IOException($"Could not save baked scene: {path}");
        NormalizePackedNodeIds(ProjectSettings.GlobalizePath(path));
    }

    // Godot assigns random editor-only node IDs when packing a runtime tree. Their
    // serialized int32 array is the only remaining byte difference between identical
    // same-seed scenes; stable distinct IDs make the package reproducible and diffable.
    private static void NormalizePackedNodeIds(string path)
    {
        var bytes = System.IO.File.ReadAllBytes(path);
        var marker = Encoding.ASCII.GetBytes("node_ids\0");
        var found = -1;
        for (var i = 0; i <= bytes.Length - marker.Length; i++)
        {
            if (!bytes.AsSpan(i, marker.Length).SequenceEqual(marker)) continue;
            if (found >= 0) throw new InvalidDataException("Ambiguous packed-scene node IDs");
            found = i;
        }
        if (found < 0) throw new InvalidDataException("Packed scene has no node IDs");
        var header = found + marker.Length;
        if (header + 8 > bytes.Length ||
            BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(header, 4)) != 32)
            throw new InvalidDataException("Unsupported packed-scene node-ID encoding");
        var count = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(header + 4, 4));
        var start = header + 8;
        if (count <= 0 || count > 10000 || start + count * 4 > bytes.Length)
            throw new InvalidDataException("Invalid packed-scene node-ID count");
        for (var i = 0; i < count; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + i * 4, 4), i + 1);
        System.IO.File.WriteAllBytes(path, bytes);
    }

    private static Node3D LoadSubscene(string directory, string file) =>
        (ResourceLoader.Load<PackedScene>(directory + "/" + file)
            ?? throw new InvalidDataException($"Baked scene missing: {file}")).Instantiate<Node3D>();

    private static void ReplaceVisuals(Node3D parent, Node3D replacement)
    {
        var old = parent.GetNode<Node3D>("Visuals");
        parent.RemoveChild(old);
        old.Free();
        replacement.Name = "Visuals";
        parent.AddChild(replacement);
    }

    private static void AssignMissingOwners(Node node, Node root)
    {
        foreach (var child in node.GetChildren())
        {
            if (child.Owner is null) child.Owner = root;
            AssignMissingOwners(child, root);
        }
    }

    private static void NormalizeIds(GodotObject root)
    {
        var seen = new HashSet<ulong>();
        var next = 0;
        Visit(root);
        void Visit(GodotObject value)
        {
            if (!seen.Add(value.GetInstanceId())) return;
            if (value is Resource resource)
            {
                if (!resource.ResourcePath.StartsWith("res://", StringComparison.Ordinal) &&
                    !resource.ResourcePath.StartsWith("user://", StringComparison.Ordinal))
                    resource.ResourceSceneUniqueId = $"r{++next:D7}";
                else return;
            }
            foreach (var property in value.GetPropertyList())
            {
                var name = property["name"].AsString();
                var item = value.Get(name);
                VisitVariant(item);
            }
            if (value is Node node)
                foreach (var child in node.GetChildren()) Visit(child);
        }
        void VisitVariant(Variant item)
        {
            switch (item.VariantType)
            {
                case Variant.Type.Object:
                    if (item.AsGodotObject() is Resource nested) Visit(nested);
                    break;
                case Variant.Type.Array:
                    foreach (var element in item.AsGodotArray()) VisitVariant(element);
                    break;
                case Variant.Type.Dictionary:
                    foreach (var pair in item.AsGodotDictionary()) VisitVariant(pair.Value);
                    break;
            }
        }
    }
    private static float[] Vec(Vector3 p) => new[] { p.X, p.Y, p.Z };
    private static float[] Field(WorldEnvironmentSample s) => new[] {
        s.Surface.X, s.Surface.Y, s.Surface.Z, s.Normal.X, s.Normal.Y, s.Normal.Z,
        s.Depth, s.Slope, s.Concavity, s.PerimeterDistance, s.DeepCoreDistance,
        s.RouteDistance, s.CaveDistance, s.EntranceDistance, s.Shelter, s.Exposure,
        s.GeologyDistance };
}
