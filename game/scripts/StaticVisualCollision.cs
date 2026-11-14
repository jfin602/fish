using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

/// <summary>Builds static prop collision from the displayed triangle meshes.</summary>
public static class StaticVisualCollision
{
    private static readonly Dictionary<string, ConcavePolygonShape3D> Shapes = new(StringComparer.Ordinal);

    public static int CachedShapeCount => Shapes.Count;

    public static int Install(Node3D prop, StaticBody3D body, string assetId)
    {
        if (string.IsNullOrWhiteSpace(assetId) || !prop.IsInsideTree() ||
            !body.IsInsideTree() || !prop.IsAncestorOf(body))
            throw new InvalidOperationException("VISUAL_COLLISION_OWNER_INVALID " + prop.Name);
        var meshes = VisibleMeshes(prop).ToArray();
        if (meshes.Length == 0)
            throw new InvalidOperationException("VISUAL_COLLISION_MESH_MISSING " + prop.Name);
        var shapes = new List<CollisionShape3D>();
        for (var i = 0; i < meshes.Length; i++)
        {
            var mesh = meshes[i];
            var key = assetId + ":" + i;
            if (!Shapes.TryGetValue(key, out var shape))
            {
                shape = mesh.Mesh!.CreateTrimeshShape();
                if (shape.Data.Length == 0)
                    throw new InvalidOperationException("VISUAL_COLLISION_TRIANGLES_MISSING " + prop.Name);
                shape.BackfaceCollision = true;
                Shapes.Add(key, shape);
            }
            shapes.Add(new CollisionShape3D
            {
                Name = "DisplayedMeshCollision_" + i,
                Shape = shape,
                Transform = body.GlobalTransform.AffineInverse() * mesh.GlobalTransform,
            });
        }
        foreach (Node child in body.GetChildren())
            if (child is CollisionShape3D old)
            {
                body.RemoveChild(old);
                old.Free();
            }
        foreach (var shape in shapes) body.AddChild(shape);
        body.SetMeta("displayed_mesh_collision", true);
        return shapes.Count;
    }

    private static IEnumerable<MeshInstance3D> VisibleMeshes(Node node)
    {
        if (node is MeshInstance3D { Mesh: not null } mesh && mesh.IsVisibleInTree())
            yield return mesh;
        foreach (Node child in node.GetChildren())
            foreach (var visibleMesh in VisibleMeshes(child)) yield return visibleMesh;
    }
}
