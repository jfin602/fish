#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

/// <summary>Installs the deterministic generated FAF plan after terrain, routes and fields exist.</summary>
public partial class FafReefDressing : Node3D
{
    private GeneratedFafPlacement? _plan;
    public IReadOnlyList<GeneratedFafInstance> Placements => _plan?.Instances ?? Array.Empty<GeneratedFafInstance>();
    public WorldPlacementScore? WorldScore => _plan?.Score;
    public IReadOnlyDictionary<HabitatRole, float> HabitatArea =>
        _plan?.HabitatArea ?? new Dictionary<HabitatRole, float>();
    public IReadOnlyDictionary<HabitatRole, int> HabitatPatches =>
        _plan?.HabitatPatches ?? new Dictionary<HabitatRole, int>();
    public long HabitatPlanningMilliseconds => _plan?.PlanningMilliseconds ?? 0;
    public long DensePlacementMilliseconds => _plan?.PlacementMilliseconds ?? 0;
    public int PlacementCount => Placements.Count;
    public int ClusterCount => _plan?.AnchorCount ?? 0;
    public string GetPlacementId(int index) => Placements[index].Asset.Id;
    public string GetPlacementZone(int index) => Placements[index].DepthBand;
    public string GetPlacementCluster(int index) => Placements[index].Role.ToString();
    public Vector3 GetPlacementPosition(int index) => Placements[index].Position;
    public float GetPlacementRadius(int index) => Placements[index].Radius;

    public bool TryBuild(WorldEnvironmentAnalysis environment, GeneratedWorldRoutes routes,
        ulong seed, out string diagnostic)
    {
        var visuals = GetNodeOrNull<Node3D>("Visuals");
        if (visuals is null || visuals.GetChildCount() != 0 || _plan is not null)
        { diagnostic = "FAF_DRESSING_INVALID action=check-empty-visual-root"; return false; }
        var plan = new GeneratedFafPlacement(environment, routes, seed);
        if (!plan.Build(out diagnostic)) return false;
        var batches = new Dictionary<string, List<Transform3D>>(StringComparer.Ordinal);
        foreach (var placement in plan.Instances)
        {
            var asset = placement.Asset;
            if (asset.Family is FafEnvironmentFamily.Terrain or FafEnvironmentFamily.Rock)
            {
                var wrapper = asset.CreateVisual();
                wrapper.Name = asset.Id.Replace('-', '_') + "_" + visuals.GetChildCount();
                wrapper.Transform = placement.Transform;
                wrapper.SetMeta("habitat_role", placement.Role.ToString());
                wrapper.SetMeta("support_delta", placement.FootprintDelta);
                visuals.AddChild(wrapper);
                if (asset.Family == FafEnvironmentFamily.Terrain)
                {
                    var terrainMesh = FindMesh(wrapper);
                    if (terrainMesh is null || !DrapeTerrain(terrainMesh, environment, placement))
                    { diagnostic = $"FAF_DRESSING_INVALID id={asset.Id} action=drape-generated-surface"; return false; }
                    var drapedShape = terrainMesh.Mesh!.CreateTrimeshShape();
                    var drapedBody = new StaticBody3D { Name = "DrapedSolid", CollisionLayer = 1, CollisionMask = 0 };
                    drapedBody.AddChild(new CollisionShape3D { Name = "Shape", Shape = drapedShape });
                    terrainMesh.AddChild(drapedBody);
                    continue;
                }
                var shapePath = $"res://assets/faf/collision/{asset.Id.ToLowerInvariant().Replace('-', '_')}.tres";
                var shape = ResourceLoader.Load<Shape3D>(shapePath);
                if (shape is null)
                { diagnostic = $"FAF_DRESSING_INVALID id={asset.Id} action=missing-collision"; return false; }
                var body = new StaticBody3D { Name = "Solid", CollisionLayer = 1, CollisionMask = 0 };
                body.AddChild(new CollisionShape3D { Name = "Shape", Shape = shape });
                wrapper.AddChild(body);
                continue;
            }
            if (!batches.TryGetValue(asset.Id, out var transforms))
                batches[asset.Id] = transforms = new List<Transform3D>();
            transforms.Add(placement.Transform);
        }
        foreach (var (id, transforms) in batches.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            var asset = FafRuntimeCatalog.GetEnvironment(id);
            var source = asset.LoadScene().Instantiate<Node3D>();
            var mesh = FindMesh(source)?.Mesh;
            if (mesh is null)
            { source.Free(); diagnostic = $"FAF_DRESSING_INVALID id={id} action=missing-imported-mesh"; return false; }
            var batch = new MultiMesh { TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
                Mesh = mesh, InstanceCount = transforms.Count, VisibleInstanceCount = transforms.Count };
            for (var i = 0; i < transforms.Count; i++) batch.SetInstanceTransform(i, transforms[i]);
            var node = new MultiMeshInstance3D { Name = id.Replace('-', '_'), Multimesh = batch };
            node.SetMeta("faf_id", id);
            node.SetMeta("placement_count", transforms.Count);
            visuals.AddChild(node);
            source.Free();
        }
        _plan = plan;
        diagnostic = "";
        GD.Print($"FAF_DRESSING_READY seed={seed} habitat_roles={plan.AnchorCount} habitat_patches={plan.HabitatPatches.Values.Sum()} {plan.Score}");
        return true;
    }

    private static MeshInstance3D? FindMesh(Node node)
    {
        if (node is MeshInstance3D mesh) return mesh;
        foreach (var child in node.GetChildren())
            if (FindMesh(child) is { } found) return found;
        return null;
    }

    private static bool DrapeTerrain(MeshInstance3D meshNode, WorldEnvironmentAnalysis environment,
        GeneratedFafInstance placement)
    {
        var source = meshNode.Mesh;
        if (source is null || source.GetSurfaceCount() == 0) return false;
        var result = new ArrayMesh();
        for (var surface = 0; surface < source.GetSurfaceCount(); surface++)
        {
            var arrays = source.SurfaceGetArrays(surface);
            var vertices = arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
            var indices = arrays[(int)Mesh.ArrayType.Index].AsInt32Array();
            var uvs = arrays[(int)Mesh.ArrayType.TexUV].AsVector2Array();
            if (vertices.Length == 0 || (indices.Length == 0 && vertices.Length % 3 != 0) ||
                (indices.Length > 0 && indices.Length % 3 != 0)) return false;
            using var tool = new SurfaceTool();
            tool.Begin(Mesh.PrimitiveType.Triangles);
            tool.SetMaterial(source.SurfaceGetMaterial(surface));
            var count = indices.Length == 0 ? vertices.Length : indices.Length;
            for (var i = 0; i < count; i += 3)
            {
                var corners = new Vector3[3];
                var tex = new Vector2[3];
                for (var j = 0; j < 3; j++)
                {
                    var index = indices.Length == 0 ? i + j : indices[i + j];
                    corners[j] = vertices[index];
                    tex[j] = uvs.Length == vertices.Length ? uvs[index] : Vector2.Zero;
                }
                const int subdivisions = 3;
                for (var row = 0; row < subdivisions; row++)
                for (var column = 0; column < subdivisions - row; column++)
                {
                    var a = Barycentric(corners, row, column, subdivisions);
                    var b = Barycentric(corners, row + 1, column, subdivisions);
                    var c = Barycentric(corners, row, column + 1, subdivisions);
                    var ta = Barycentric(tex, row, column, subdivisions);
                    var tb = Barycentric(tex, row + 1, column, subdivisions);
                    var tc = Barycentric(tex, row, column + 1, subdivisions);
                    Emit(tool, meshNode, environment, placement, a, b, c, ta, tb, tc);
                    if (row + column >= subdivisions - 1) continue;
                    var d = Barycentric(corners, row + 1, column + 1, subdivisions);
                    var td = Barycentric(tex, row + 1, column + 1, subdivisions);
                    Emit(tool, meshNode, environment, placement, b, d, c, tb, td, tc);
                }
            }
            tool.Commit(result);
        }
        meshNode.Mesh = result;
        return true;
    }

    private static Vector3 Barycentric(Vector3[] points, int row, int column, int divisions) =>
        points[0] + (points[1] - points[0]) * (row / (float)divisions) +
        (points[2] - points[0]) * (column / (float)divisions);
    private static Vector2 Barycentric(Vector2[] points, int row, int column, int divisions) =>
        points[0] + (points[1] - points[0]) * (row / (float)divisions) +
        (points[2] - points[0]) * (column / (float)divisions);

    private static void Emit(SurfaceTool tool, MeshInstance3D mesh,
        WorldEnvironmentAnalysis environment, GeneratedFafInstance placement,
        Vector3 a, Vector3 b, Vector3 c, Vector2 ta, Vector2 tb, Vector2 tc)
    {
        var wa = Warp(a); var wb = Warp(b); var wc = Warp(c);
        var normal = (wb - wa).Cross(wc - wa).Normalized();
        tool.SetNormal(normal); tool.SetUV(ta); tool.AddVertex(wa);
        tool.SetNormal(normal); tool.SetUV(tb); tool.AddVertex(wb);
        tool.SetNormal(normal); tool.SetUV(tc); tool.AddVertex(wc);

        Vector3 Warp(Vector3 vertex)
        {
            var world = mesh.ToGlobal(vertex);
            var floor = environment.ProjectDown(new Vector3(world.X, 45f, world.Z)).Position.Y;
            return mesh.ToLocal(new Vector3(world.X, floor - .08f + vertex.Y * placement.Scale, world.Z));
        }
    }
}
