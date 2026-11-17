#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

/// <summary>Visible batched Pond plant bounds reconstructed from the same mesh and placement transforms.</summary>
public static class PondSoftCover
{
    public static IReadOnlyList<SoftSightCover> Near(IReadOnlyList<PondFafPlacement> placements,
        Vector3 center, float radius)
    {
        var nearby = placements.Where(p => p.Id.StartsWith("FAF-PLANT-", StringComparison.Ordinal) &&
            p.Position.DistanceTo(center) <= radius).ToArray();
        var assets = PondFafEnvironment.LoadCatalog().ToDictionary(a => a.Id);
        var boxes = new Dictionary<string, Aabb>();
        foreach (var id in nearby.Select(p => p.Id).Distinct())
        {
            var scene = ResourceLoader.Load<PackedScene>(assets[id].Glb)
                ?? throw new InvalidOperationException("POND_COVER_MODEL_MISSING " + id);
            var root = scene.Instantiate<Node3D>();
            try
            {
                var hasMesh = false;
                var bounds = new Aabb();
                foreach (var found in root.FindChildren("*", "MeshInstance3D", true, false))
                {
                    if (found is not MeshInstance3D { Mesh: not null } mesh) continue;
                    // The Pond renderer batches each mesh with its own Transform.
                    var transform = mesh.Transform;
                    var local = mesh.GetAabb();
                    for (var mask = 0; mask < 8; mask++)
                    {
                        var point = transform * (local.Position + local.Size * new Vector3(
                            (mask & 1) == 0 ? 0 : 1, (mask & 2) == 0 ? 0 : 1,
                            (mask & 4) == 0 ? 0 : 1));
                        if (!hasMesh) { bounds = new Aabb(point, Vector3.Zero); hasMesh = true; }
                        else bounds = bounds.Expand(point);
                    }
                }
                if (hasMesh) boxes.Add(id, bounds);
            }
            finally { root.Free(); }
        }
        var result = new List<SoftSightCover>(nearby.Length);
        foreach (var placement in nearby)
        {
            if (!boxes.TryGetValue(placement.Id, out var local)) continue;
            var transform = new Transform3D(new Basis(Vector3.Up, placement.Yaw)
                .Scaled(Vector3.One * placement.Scale), placement.Position);
            var first = true;
            var bounds = new Aabb();
            for (var mask = 0; mask < 8; mask++)
            {
                var point = transform * (local.Position + local.Size * new Vector3(
                    (mask & 1) == 0 ? 0 : 1, (mask & 2) == 0 ? 0 : 1,
                    (mask & 4) == 0 ? 0 : 1));
                if (first) { bounds = new Aabb(point, Vector3.Zero); first = false; }
                else bounds = bounds.Expand(point);
            }
            result.Add(new SoftSightCover(placement.Id, bounds));
        }
        return result;
    }
}
