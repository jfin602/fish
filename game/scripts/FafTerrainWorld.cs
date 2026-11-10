#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace Fish;

/// <summary>Authored FAF cover draped over the unchanged P2 collision/depth surface.</summary>
public partial class FafTerrainWorld : Node3D
{
    private readonly record struct Placement(char Variant, float X, float Z, float SX, float SZ,
        float Yaw, float Relief, string Zone);

    // Unequal overlapping sand fields continue beyond the fish's reachable +/-29 m area.
    // Relief is deliberately below a fish radius on paths; geology gets its own collision in P3.
    private static readonly Placement[] Layout =
    {
        new('A', -30, -29, 1.70f, 1.67f, -.31f, .12f, "deep-west"),
        new('A', -15, -30, 1.76f, 1.73f, .18f, .12f, "deep-west"),
        new('A', 1, -30, 1.83f, 1.65f, -.13f, .12f, "deep-lane"),
        new('A', 17, -29, 1.74f, 1.80f, .27f, .12f, "deep-lane"),
        new('A', 31, -30, 1.64f, 1.73f, -.20f, .12f, "deep-east"),
        new('A', -28, -15, 1.84f, 1.69f, .21f, .12f, "lower-west"),
        new('A', -13, -16, 1.75f, 1.81f, -.28f, .12f, "tunnel-approach"),
        new('A', 2, -15, 1.82f, 1.77f, .15f, .12f, "lower-lane"),
        new('A', 17, -16, 1.78f, 1.68f, -.25f, .12f, "shelter-approach"),
        new('A', 31, -14, 1.69f, 1.78f, .34f, .12f, "lower-east"),
        new('A', -31, 0, 1.68f, 1.77f, -.16f, .12f, "middle-west"),
        new('A', -16, -1, 1.81f, 1.69f, .32f, .12f, "middle-west"),
        new('A', 0, 0, 1.82f, 1.84f, -.09f, .12f, "spawn-lane"),
        new('A', 16, 1, 1.72f, 1.80f, .23f, .12f, "middle-east"),
        new('A', 30, -1, 1.78f, 1.73f, -.37f, .12f, "east-channel"),
        new('A', -29, 15, 1.78f, 1.71f, .28f, .12f, "upper-west"),
        new('A', -14, 14, 1.76f, 1.84f, -.19f, .12f, "upper-west"),
        new('A', 2, 15, 1.88f, 1.71f, .31f, .12f, "upper-lane"),
        new('A', 17, 14, 1.79f, 1.79f, -.27f, .12f, "upper-east"),
        new('A', 31, 16, 1.70f, 1.83f, .18f, .12f, "upper-east"),
        new('A', -30, 30, 1.75f, 1.67f, -.26f, .12f, "shallow-west"),
        new('A', -15, 29, 1.82f, 1.76f, .13f, .12f, "shallow-west"),
        new('A', 1, 30, 1.81f, 1.78f, -.32f, .12f, "shallow-lane"),
        new('A', 16, 29, 1.76f, 1.83f, .24f, .12f, "shallow-east"),
        new('A', 31, 30, 1.71f, 1.71f, -.11f, .12f, "shallow-east"),
        new('B', -22, 24, 2.05f, 1.71f, .52f, .24f, "shallow-west"),
        new('B', 11, 25, 1.77f, 1.92f, -.46f, .24f, "shallow-east"),
        new('B', -20, 3, 2.12f, 1.82f, -.38f, .24f, "middle-west"),
        new('B', 10, -3, 1.86f, 2.04f, .63f, .24f, "middle-east"),
        new('B', -19, -25, 1.88f, 1.72f, .34f, .24f, "deep-west"),
        new('C', -23, 14, 2.02f, 1.76f, -.60f, .39f, "upper-west-shelf"),
        new('C', 27, 17, 1.86f, 2.03f, .41f, .39f, "upper-east-shelf"),
        new('C', -25, -6, 1.95f, 1.87f, .71f, .39f, "middle-west-shelf"),
        new('C', 27, -8, 2.08f, 1.78f, -.48f, .39f, "east-channel-shelf"),
        new('C', -20, -19, 1.72f, 1.92f, .37f, .39f, "deep-west-shelf"),
        new('D', -12, 18, 2.18f, 1.90f, .62f, .36f, "shallow-upper-ramp"),
        new('D', 12, 7, 2.03f, 2.05f, -.44f, .36f, "upper-middle-ramp"),
        new('D', -22, -2, 2.12f, 1.83f, .28f, .36f, "middle-lower-ramp"),
        new('D', 14, -12, 1.91f, 2.16f, -.67f, .36f, "lower-deep-ramp"),
        new('D', -5, -25, 2.09f, 1.88f, .50f, .36f, "deep-return-ramp"),
        new('E', -26, 8, 2.04f, 1.78f, .77f, .37f, "west-break"),
        new('E', 27, 5, 1.83f, 2.14f, -.58f, .37f, "east-break"),
        new('E', -24, -13, 1.90f, 2.05f, -.32f, .37f, "tunnel-break"),
        new('E', 27, -20, 2.01f, 1.84f, .69f, .37f, "deep-east-break"),
        new('E', -10, -28, 1.78f, 1.95f, -.73f, .37f, "deep-west-break"),
        new('F', -24, 22, 2.08f, 1.92f, -.46f, .31f, "shallow-rock"),
        new('F', 27, 10, 1.94f, 2.07f, .55f, .31f, "upper-rock"),
        new('F', -28, -3, 2.14f, 1.80f, -.28f, .31f, "middle-rock"),
        new('F', 25, -15, 1.87f, 2.15f, .38f, .31f, "lower-rock"),
        new('F', -24, -27, 2.11f, 1.91f, -.61f, .31f, "deep-rock"),
        new('F', 13, -27, 1.95f, 2.01f, .72f, .31f, "deep-rock"),
    };

    private readonly List<(Vector3 A, Vector3 B, Vector3 C)> _faces = new();
    public int ChunkCount => Layout.Length;

    public bool TryBuild(SeabedSurface seabed, out string diagnostic)
    {
        if (GetNodeOrNull<Node3D>("Visuals") is not { } visuals || visuals.GetChildCount() != 0)
        {
            diagnostic = "FAF_TERRAIN_INVALID action=check-empty-visual-root";
            return false;
        }
        for (var index = 0; index < Layout.Length; index++)
        {
            var place = Layout[index];
            var id = $"FAF-TERRAIN-001{place.Variant}";
            var wrapper = FafRuntimeCatalog.GetEnvironment(id).CreateVisual();
            wrapper.Name = $"Chunk{index:00}_{place.Variant}";
            wrapper.Position = new Vector3(place.X, 0, place.Z);
            wrapper.Rotation = new Vector3(0, place.Yaw, 0);
            wrapper.Scale = new Vector3(place.SX, 1, place.SZ);
            wrapper.SetMeta("seam_zone", place.Zone);
            visuals.AddChild(wrapper);
            if (FindMesh(wrapper) is not { } mesh || mesh.Mesh is null)
            {
                diagnostic = $"FAF_TERRAIN_INVALID id={id} action=missing-imported-mesh";
                return false;
            }
            if (!Drape(mesh, seabed, place.Relief))
            {
                diagnostic = $"FAF_TERRAIN_INVALID id={id} action=invalid-imported-surface";
                return false;
            }
        }
        foreach (var child in GetNode<Node3D>("SeamZones").GetChildren())
        {
            if (child is not Marker3D marker) continue;
            marker.Position = new Vector3(marker.Position.X,
                seabed.GetHeightAt(marker.Position.X, marker.Position.Z), marker.Position.Z);
        }
        diagnostic = "";
        GD.Print($"FAF_TERRAIN_READY chunks={ChunkCount} ids=A,B,C,D,E,F collision=P2-Seabed seam_zones=authored");
        return true;
    }

    // The original imported mesh/material is read, then a private draped mesh is assigned
    // to this visual instance. Its zero-height outer skirt sits below the P2 surface.
    private bool Drape(MeshInstance3D meshNode, SeabedSurface seabed, float relief)
    {
        var source = meshNode.Mesh;
        if (source is null || source.GetSurfaceCount() == 0) return false;
        var positiveHeight = Mathf.Max(source.GetAabb().End.Y, 0.01f);
        var burial = relief <= 0.12f ? 0.02f : 0.075f;
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
                var original = new Vector3[3];
                var uv = new Vector2[3];
                for (var corner = 0; corner < 3; corner++)
                {
                    var vertexIndex = indices.Length == 0 ? i + corner : indices[i + corner];
                    original[corner] = vertices[vertexIndex];
                    uv[corner] = uvs.Length == vertices.Length ? uvs[vertexIndex] : Vector2.Zero;
                }
                // Original FAF facets are broad. Three substeps let the visual
                // follow the curved P2 collider without a half-metre chord gap.
                const int subdivisions = 3;
                for (var row = 0; row < subdivisions; row++)
                for (var column = 0; column < subdivisions - row; column++)
                {
                    var p0 = Barycentric(original, row, column, subdivisions);
                    var p1 = Barycentric(original, row + 1, column, subdivisions);
                    var p2 = Barycentric(original, row, column + 1, subdivisions);
                    var t0 = Barycentric(uv, row, column, subdivisions);
                    var t1 = Barycentric(uv, row + 1, column, subdivisions);
                    var t2 = Barycentric(uv, row, column + 1, subdivisions);
                    Emit(tool, meshNode, seabed, positiveHeight, relief, burial,
                        p0, p1, p2, t0, t1, t2);
                    if (row + column >= subdivisions - 1) continue;
                    var p3 = Barycentric(original, row + 1, column + 1, subdivisions);
                    var t3 = Barycentric(uv, row + 1, column + 1, subdivisions);
                    Emit(tool, meshNode, seabed, positiveHeight, relief, burial,
                        p1, p3, p2, t1, t3, t2);
                }
            }
            tool.Commit(result);
        }
        meshNode.Mesh = result;
        return true;
    }

    private static Vector3 Barycentric(Vector3[] points, int row, int column, int divisions)
        => points[0] + (points[1] - points[0]) * (row / (float)divisions)
            + (points[2] - points[0]) * (column / (float)divisions);

    private static Vector2 Barycentric(Vector2[] points, int row, int column, int divisions)
        => points[0] + (points[1] - points[0]) * (row / (float)divisions)
            + (points[2] - points[0]) * (column / (float)divisions);

    private void Emit(SurfaceTool tool, MeshInstance3D mesh, SeabedSurface seabed,
        float positiveHeight, float relief, float burial,
        Vector3 p0, Vector3 p1, Vector3 p2, Vector2 t0, Vector2 t1, Vector2 t2)
    {
        var a = Warp(p0, mesh, seabed, positiveHeight, relief, burial);
        var b = Warp(p1, mesh, seabed, positiveHeight, relief, burial);
        var c = Warp(p2, mesh, seabed, positiveHeight, relief, burial);
        var normal = (b - a).Cross(c - a).Normalized();
        tool.SetNormal(normal); tool.SetUV(t0); tool.AddVertex(a);
        tool.SetNormal(normal); tool.SetUV(t1); tool.AddVertex(b);
        tool.SetNormal(normal); tool.SetUV(t2); tool.AddVertex(c);
        _faces.Add((mesh.ToGlobal(a), mesh.ToGlobal(b), mesh.ToGlobal(c)));
    }

    private static Vector3 Warp(Vector3 original, MeshInstance3D mesh, SeabedSurface seabed,
        float positiveHeight, float relief, float burial)
    {
        var world = mesh.ToGlobal(original);
        var height = seabed.GetHeightAt(world.X, world.Z)
            + original.Y / positiveHeight * relief - burial;
        return mesh.ToLocal(new Vector3(world.X, height, world.Z));
    }

    // Test/query aid: top visible surface at a route point, including the seamless underlay.
    public float GetVisibleHeightAt(float x, float z, SeabedSurface seabed)
    {
        var top = seabed.GetHeightAt(x, z);
        foreach (var (a, b, c) in _faces)
        {
            var denominator = (b.Z - c.Z) * (a.X - c.X) + (c.X - b.X) * (a.Z - c.Z);
            if (Mathf.Abs(denominator) < 0.00001f) continue;
            var u = ((b.Z - c.Z) * (x - c.X) + (c.X - b.X) * (z - c.Z)) / denominator;
            var v = ((c.Z - a.Z) * (x - c.X) + (a.X - c.X) * (z - c.Z)) / denominator;
            if (u < -0.0001f || v < -0.0001f || u + v > 1.0001f) continue;
            top = Mathf.Max(top, u * a.Y + v * b.Y + (1 - u - v) * c.Y);
        }
        return top;
    }

    private static MeshInstance3D? FindMesh(Node node)
    {
        if (node is MeshInstance3D mesh) return mesh;
        foreach (var child in node.GetChildren())
            if (FindMesh(child) is { } found) return found;
        return null;
    }
}
