#nullable enable
using Godot;
using System;

namespace Fish;

/// <summary>Rest-pose bounds of the actual attached player mesh, in fish coordinates.</summary>
public readonly record struct PlayerBodyGeometry(float Radius, float Height, float CenterZ,
    float Length, float TailZ, float TopReach, float SideReach,
    Vector3 LeftFin, Vector3 RightFin)
{
    public static PlayerBodyGeometry Measure(FishController fish)
    {
        var visual = fish.GetNodeOrNull<Node3D>("PlayerVisual")
            ?? throw new InvalidOperationException("PLAYER_BODY_VISUAL_MISSING");
        var radialMaximum = 0f;
        var sideMaximum = 0f;
        var minimumZ = float.PositiveInfinity;
        var maximumZ = float.NegativeInfinity;
        var maximumY = float.NegativeInfinity;
        var count = 0;
        var fins = new Vector3[2];
        var finCount = 0;
        foreach (var found in visual.FindChildren("*", "MeshInstance3D", true, false))
        {
            if (found is not MeshInstance3D { Mesh: not null } mesh) continue;
            var transform = mesh.Transform;
            var parent = mesh.GetParent();
            while (parent is Node3D node)
            {
                transform = node.Transform * transform;
                if (node == visual) break;
                parent = node.GetParent();
            }
            for (var surface = 0; surface < mesh.Mesh.GetSurfaceCount(); surface++)
            {
                var vertices = mesh.Mesh.SurfaceGetArrays(surface)
                    [(int)Mesh.ArrayType.Vertex].AsVector3Array();
                foreach (var vertex in vertices)
                {
                    var point = transform * vertex;
                    radialMaximum = Mathf.Max(radialMaximum,
                        Mathf.Sqrt(point.X * point.X + point.Y * point.Y));
                    sideMaximum = Mathf.Max(sideMaximum, Mathf.Abs(point.X));
                    minimumZ = Mathf.Min(minimumZ, point.Z);
                    maximumZ = Mathf.Max(maximumZ, point.Z);
                    maximumY = Mathf.Max(maximumY, point.Y);
                    count++;
                }
            }
            // Bass stages provide separate pectoral fin meshes. Pick the rear
            // rim in fish coordinates, after the stage's presentation transform.
            if (mesh.Name.ToString().Contains("pectoral", StringComparison.OrdinalIgnoreCase))
            {
                var rear = float.NegativeInfinity;
                var points = new System.Collections.Generic.List<Vector3>();
                for (var surface = 0; surface < mesh.Mesh.GetSurfaceCount(); surface++)
                    foreach (var vertex in mesh.Mesh.SurfaceGetArrays(surface)
                        [(int)Mesh.ArrayType.Vertex].AsVector3Array())
                    {
                        var point = transform * vertex;
                        rear = Mathf.Max(rear, point.Z);
                        points.Add(point);
                    }
                var sum = Vector3.Zero;
                var rimCount = 0;
                foreach (var point in points)
                    if (point.Z >= rear - .00001f)
                    { sum += point; rimCount++; }
                if (rimCount > 0 && finCount < 2) fins[finCount++] = sum / rimCount;
            }
        }
        if (count == 0 || !float.IsFinite(radialMaximum) || !float.IsFinite(sideMaximum) ||
            !float.IsFinite(minimumZ) || !float.IsFinite(maximumZ) || !float.IsFinite(maximumY))
            throw new InvalidOperationException("PLAYER_BODY_GEOMETRY_INVALID");
        // Allow for animation at fin tips. The longitudinal capsule encloses
        // the measured mesh without treating every species as the same sphere.
        var radius = radialMaximum * 1.08f + .0005f;
        var height = Mathf.Max(radius * 2f, maximumZ - minimumZ + radius * 2f);
        var fallbackX = Mathf.Max(sideMaximum * .62f, .002f);
        var fallbackZ = minimumZ + (maximumZ - minimumZ) * .68f;
        var leftFin = new Vector3(-fallbackX, 0f, fallbackZ);
        var rightFin = new Vector3(fallbackX, 0f, fallbackZ);
        if (finCount == 2)
        {
            leftFin = fins[0].X <= fins[1].X ? fins[0] : fins[1];
            rightFin = fins[0].X > fins[1].X ? fins[0] : fins[1];
        }
        return new PlayerBodyGeometry(radius, height, (minimumZ + maximumZ) * .5f,
            maximumZ - minimumZ, maximumZ, maximumY, sideMaximum,
            leftFin, rightFin);
    }
}
