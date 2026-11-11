#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace Fish;

/// <summary>One water mesh clipped to the baked basin's signed shore field.</summary>
public static class PondSurfaceGeometry
{
    public static ArrayMesh Build(GeneratedBasin basin)
    {
        if (!basin.IsPond) throw new ArgumentException("Pond basin required", nameof(basin));
        using var tool = new SurfaceTool();
        tool.Begin(Mesh.PrimitiveType.Triangles);
        var minX = Mathf.FloorToInt((basin.ShoreMin.X - 2f) / 2f) * 2;
        var maxX = Mathf.CeilToInt((basin.ShoreMax.X + 2f) / 2f) * 2;
        var minZ = Mathf.FloorToInt((basin.ShoreMin.Y - 2f) / 2f) * 2;
        var maxZ = Mathf.CeilToInt((basin.ShoreMax.Y + 2f) / 2f) * 2;
        for (var z = minZ; z < maxZ; z += 2)
        for (var x = minX; x < maxX; x += 2)
        {
            var a = new Vector2(x, z);
            var b = new Vector2(x + 2, z);
            var c = new Vector2(x, z + 2);
            var d = new Vector2(x + 2, z + 2);
            // X/Z positive winding faces downward in 3D. Match PlaneMesh's
            // upward front face so FRONT_FACING selects the air treatment.
            EmitTriangle(a, c, b);
            EmitTriangle(b, c, d);
        }
        var mesh = new ArrayMesh();
        tool.Commit(mesh);
        return mesh;

        void EmitTriangle(Vector2 a, Vector2 b, Vector2 c)
        {
            var polygon = new List<(Vector2 point, float distance)>
            {
                (a, basin.DistanceToShore(a.X, a.Y)),
                (b, basin.DistanceToShore(b.X, b.Y)),
                (c, basin.DistanceToShore(c.X, c.Y)),
            };
            var clipped = new List<(Vector2 point, float distance)>();
            for (var i = 0; i < polygon.Count; i++)
            {
                var start = polygon[i];
                var end = polygon[(i + 1) % polygon.Count];
                if (start.distance >= 0f) clipped.Add(start);
                if ((start.distance >= 0f) == (end.distance >= 0f)) continue;
                var ratio = start.distance / (start.distance - end.distance);
                clipped.Add((start.point.Lerp(end.point, ratio), 0f));
            }
            for (var i = 1; i + 1 < clipped.Count; i++)
            {
                Add(clipped[0]); Add(clipped[i]); Add(clipped[i + 1]);
            }
        }
        void Add((Vector2 point, float distance) vertex)
        {
            tool.SetNormal(Vector3.Up);
            tool.SetUV(vertex.point * .1f);
            tool.SetColor(new Color(Mathf.Clamp(vertex.distance / 1.6f, 0f, 1f), 1, 1));
            tool.AddVertex(new Vector3(vertex.point.X, 0, vertex.point.Y));
        }
    }
}
