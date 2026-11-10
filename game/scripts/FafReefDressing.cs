#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

/// <summary>Fixed, visual-only reef clusters. The accepted seabed remains the sole floor authority.</summary>
public partial class FafReefDressing : Node3D
{
    public readonly record struct Placement(string Id, string Zone, string Cluster, Vector3 Position,
        float Yaw, float Scale, float Radius);
    private readonly record struct Cluster(string Name, string Zone, float X, float Z, float Yaw,
        float Spread, int Coral, int Plant, int Detail, int Start);

    // Deliberate contact pockets on the FAF shelf/rock shoulders. Empty depth lanes,
    // cave mouths, tunnel and canyon centres are part of the composition.
    private static readonly Cluster[] Clusters =
    {
        new("WestSunShelf", "shallow", -24, 25, .4f, 1.05f, 7, 5, 4, 0),
        new("NorthFanGarden", "shallow", -15, 29, -.7f, 1.00f, 7, 5, 4, 3),
        new("ShallowRoll", "shallow", -8, 24, 1.1f, .90f, 7, 5, 4, 6),
        new("SandEdge", "shallow", -2, 30, -.3f, .78f, 7, 5, 4, 9),
        new("SunlitReturn", "shallow", 10, 27, .8f, 1.03f, 7, 5, 4, 1),
        new("EastHighGarden", "shallow", 16, 24, -1.2f, 1.00f, 7, 5, 4, 4),
        new("BeaconFoot", "shallow", 27, 26, .6f, .90f, 7, 5, 4, 7),
        new("BeaconShoulder", "shallow", 28, 20, -.5f, .82f, 7, 5, 4, 10),

        new("WestShelfSeam", "upper", -23, 12, .6f, .95f, 5, 4, 3, 2),
        new("WestShelfBack", "upper", -17, 16, -1.0f, .93f, 5, 4, 3, 5),
        new("UpperCut", "upper", -11, 10, .9f, .91f, 5, 4, 3, 8),
        new("UpperSandMargin", "upper", -3, 18, -.2f, .78f, 5, 4, 3, 11),
        new("ReturnGarden", "upper", 17, 18, .7f, .95f, 5, 4, 3, 1),
        new("EastShelfFoot", "upper", 15, 9, -.8f, .90f, 5, 4, 3, 4),
        new("EastUpperSeam", "upper", 28, 13, 1.2f, .89f, 5, 4, 3, 7),

        new("WestMiddleShelf", "middle", -27, 2, -.8f, .85f, 4, 3, 2, 0),
        new("WestFacetFoot", "middle", -20, 5, .5f, .88f, 4, 3, 2, 3),
        new("TunnelNorthRock", "middle", -12, 3, 1.0f, .82f, 4, 3, 2, 6),
        new("MiddleSandEdge", "middle", -3, 5, -.5f, .76f, 4, 3, 2, 9),
        new("EastOverhangFoot", "middle", 13, 4, .9f, .82f, 4, 3, 2, 2),
        new("ChannelRubble", "middle", 27, -3, -1.0f, .82f, 4, 3, 2, 5),

        new("WestBreakFoot", "lower", -25, -11, .5f, .80f, 2, 1, 2, 0),
        new("TunnelOuterJaw", "lower", -18, -18, -.7f, .76f, 2, 1, 2, 3),
        new("CaveOuterLip", "lower", 16, -8, .9f, .72f, 2, 1, 2, 6),
        new("EastLowerRock", "lower", 28, -13, -.4f, .75f, 2, 1, 2, 9),

        new("DeepSpurFoot", "deep", -23.5f, -23.5f, .4f, .70f, 1, 0, 2, 2),
        new("DeepBreakContact", "deep", -18, -29, -.6f, .68f, 1, 0, 2, 5),
        new("FarDeepFacet", "deep", 28, -25, .8f, .65f, 1, 0, 2, 8),
    };

    // Irregular, compact silhouettes. Detail offsets stay close to bases/contact seams.
    private static readonly Vector2[] CoralOffsets =
    { new(-.55f, .10f), new(.42f, -.28f), new(.08f, .53f), new(-.91f, -.43f),
      new(.88f, .31f), new(-.22f, -.78f), new(.32f, .91f) };
    private static readonly Vector2[] PlantOffsets =
    { new(-.89f, .35f), new(.68f, -.65f), new(-.38f, -.92f),
      new(.95f, .66f), new(.17f, -.12f) };
    private static readonly Vector2[] DetailOffsets =
    { new(-.62f, .25f), new(.49f, -.43f), new(.19f, .61f), new(-.31f, -.57f) };

    private readonly List<Placement> _placements = new();
    public IReadOnlyList<Placement> Placements => _placements;
    public int ClusterCount => Clusters.Length;
    public int PlacementCount => _placements.Count;
    public string GetPlacementId(int index) => _placements[index].Id;
    public string GetPlacementZone(int index) => _placements[index].Zone;
    public string GetPlacementCluster(int index) => _placements[index].Cluster;
    public Vector3 GetPlacementPosition(int index) => _placements[index].Position;
    public float GetPlacementRadius(int index) => _placements[index].Radius;

    public bool TryBuild(SeabedSurface seabed, FafTerrainWorld terrain, out string diagnostic)
    {
        var visuals = GetNodeOrNull<Node3D>("Visuals");
        if (visuals is null || visuals.GetChildCount() != 0 || _placements.Count != 0)
        {
            diagnostic = "FAF_DRESSING_INVALID action=check-empty-visual-root";
            return false;
        }
        var families = new[] { FafEnvironmentFamily.Coral, FafEnvironmentFamily.Plant,
            FafEnvironmentFamily.Detail };
        var entries = families.ToDictionary(f => f, f => FafRuntimeCatalog.Environment.Where(e => e.Family == f).ToArray());
        var grouped = new Dictionary<string, List<Transform3D>>();
        foreach (var cluster in Clusters)
        {
            AddFamily(cluster, entries[FafEnvironmentFamily.Coral], cluster.Coral, CoralOffsets, seabed, terrain, grouped);
            AddFamily(cluster, entries[FafEnvironmentFamily.Plant], cluster.Plant, PlantOffsets, seabed, terrain, grouped);
            AddFamily(cluster, entries[FafEnvironmentFamily.Detail], cluster.Detail, DetailOffsets, seabed, terrain, grouped);
        }
        foreach (var entry in entries.Values.SelectMany(x => x))
        {
            if (!grouped.TryGetValue(entry.Id, out var transforms) || transforms.Count == 0)
            {
                diagnostic = $"FAF_DRESSING_INVALID id={entry.Id} action=place-every-variant";
                return false;
            }
            var imported = entry.LoadScene().Instantiate<Node3D>();
            var meshNode = FindMesh(imported);
            if (meshNode?.Mesh is not { } mesh)
            {
                imported.Free();
                diagnostic = $"FAF_DRESSING_INVALID id={entry.Id} action=missing-mesh";
                return false;
            }
            var batch = new MultiMesh { TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
                Mesh = mesh, InstanceCount = transforms.Count, VisibleInstanceCount = transforms.Count };
            for (var i = 0; i < transforms.Count; i++) batch.SetInstanceTransform(i, transforms[i]);
            var node = new MultiMeshInstance3D { Name = entry.Id.Replace('-', '_'), Multimesh = batch };
            node.SetMeta("faf_id", entry.Id);
            node.SetMeta("placement_count", transforms.Count);
            visuals.AddChild(node);
            imported.Free();
        }
        diagnostic = "";
        GD.Print($"FAF_DRESSING_READY clusters={ClusterCount} placements={_placements.Count} variants={grouped.Count} batches={visuals.GetChildCount()}");
        return true;
    }

    private void AddFamily(Cluster cluster, FafEnvironmentEntry[] family, int count, Vector2[] offsets,
        SeabedSurface seabed, FafTerrainWorld terrain, Dictionary<string, List<Transform3D>> grouped)
    {
        for (var i = 0; i < count; i++)
        {
            var entry = family[(cluster.Start + i * (family.Length == 12 ? 5 : 3)) % family.Length];
            // Rotate the silhouette sequence per pocket so neighboring gardens do
            // not repeat the same coral/plant/detail stamp at a new coordinate.
            var offset = offsets[(i + cluster.Start) % offsets.Length] * cluster.Spread;
            var angle = cluster.Yaw + (i % 3 - 1) * .14f;
            var rotated = offset.Rotated(angle);
            var x = cluster.X + rotated.X;
            var z = cluster.Z + rotated.Y;
            var y = terrain.GetVisibleHeightAt(x, z, seabed) + .015f;
            var yaw = cluster.Yaw + i * 1.17f + cluster.Start * .31f
                + .19f * Mathf.Sin(cluster.X * .21f + cluster.Z * .13f + i * 2.1f);
            var scale = entry.Family == FafEnvironmentFamily.Detail
                ? 1.12f + .12f * ((i + cluster.Start) % 3)
                : .82f + .14f * ((i + cluster.Start) % 4);
            var radius = .5f * Mathf.Max(entry.SizeMeters.X, entry.SizeMeters.Z) * scale;
            var position = new Vector3(x, y, z);
            _placements.Add(new Placement(entry.Id, cluster.Zone, cluster.Name, position, yaw, scale, radius));
            if (!grouped.TryGetValue(entry.Id, out var transforms))
                grouped[entry.Id] = transforms = new List<Transform3D>();
            var width = .90f + .10f * Mathf.Abs(Mathf.Sin(cluster.X * .17f + i * 1.7f));
            var height = .88f + .12f * Mathf.Abs(Mathf.Cos(cluster.Z * .19f + i * 1.3f));
            transforms.Add(new Transform3D(new Basis(Vector3.Up, yaw)
                .Scaled(new Vector3(scale * width, scale * height, scale)), position));
        }
    }

    private static MeshInstance3D? FindMesh(Node node)
    {
        if (node is MeshInstance3D mesh) return mesh;
        foreach (var child in node.GetChildren())
            if (FindMesh(child) is { } found) return found;
        return null;
    }
}
