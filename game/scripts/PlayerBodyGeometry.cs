#nullable enable
using Godot;
using System;

namespace Fish;

/// <summary>Rest-pose bounds of the actual attached player mesh, in fish coordinates.</summary>
public readonly record struct PlayerBodyGeometry(float Radius, float Height, float CenterZ,
    float Length, float TailZ, float TopReach)
{
    public static PlayerBodyGeometry Measure(FishController fish)
    {
        var visual = fish.GetNodeOrNull<Node3D>("PlayerVisual")
            ?? throw new InvalidOperationException("PLAYER_BODY_VISUAL_MISSING");
        var radialMaximum = 0f;
        var minimumZ = float.PositiveInfinity;
        var maximumZ = float.NegativeInfinity;
        var maximumY = float.NegativeInfinity;
        var count = 0;
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
                    minimumZ = Mathf.Min(minimumZ, point.Z);
                    maximumZ = Mathf.Max(maximumZ, point.Z);
                    maximumY = Mathf.Max(maximumY, point.Y);
                    count++;
                }
            }
        }
        if (count == 0 || !float.IsFinite(radialMaximum) ||
            !float.IsFinite(minimumZ) || !float.IsFinite(maximumZ) || !float.IsFinite(maximumY))
            throw new InvalidOperationException("PLAYER_BODY_GEOMETRY_INVALID");
        // Allow for animation at fin tips. The longitudinal capsule encloses
        // the measured mesh without treating every species as the same sphere.
        var radius = radialMaximum * 1.08f + .0005f;
        var height = Mathf.Max(radius * 2f, maximumZ - minimumZ + radius * 2f);
        return new PlayerBodyGeometry(radius, height, (minimumZ + maximumZ) * .5f,
            maximumZ - minimumZ, maximumZ, maximumY);
    }
}
