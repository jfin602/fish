#nullable enable
using Fish;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class PondWorldChecks
{
    public static void Run(Main main, string directory)
    {
        var habitat = main.GetNode<Node3D>("Habitat");
        var fish = main.GetNode<FishController>("Fish");
        var before = WorldGenerationAudit.StageInvocations;
        var (manifest, data) = FullWorldPackage.Load(directory, habitat, WorldCatalog.PondId);
        Require(WorldGenerationAudit.StageInvocations == before, "generator ran on baked load");
        var seabed = habitat.GetNode<SeabedSurface>("Seabed");
        var basin = seabed.Basin;
        Require(basin.IsPond && manifest.BasinAlgorithm == basin.AlgorithmIdentity,
            "wrong basin algorithm");
        Require(seabed.GeneratedMesh?.GetSurfaceCount() == 1 &&
            seabed.GeneratedCollision is { Data.Length: > 110000 and < 120000 },
            "simplified terrain collision missing");
        var collisionVertices = seabed.GeneratedCollision!.Data;
        foreach (var vertex in collisionVertices)
            Require(Mathf.Abs(vertex.Y - basin.HeightAt(vertex.X, vertex.Z)) < .0002f,
                "collision does not follow terrain height field");
        var maxSimplificationError = 0f;
        var step = GeneratedBasin.HalfExtent * 2f / 138f;
        for (var iz = 0; iz < 138; iz++)
        for (var ix = 0; ix < 138; ix++)
        {
            var x = -GeneratedBasin.HalfExtent + ix * step;
            var z = -GeneratedBasin.HalfExtent + iz * step;
            var middleX = x + step * .5f; var middleZ = z + step * .5f;
            if (basin.DistanceToShore(middleX, middleZ) < 0f) continue;
            var physical = .5f * (basin.HeightAt(x, z + step) + basin.HeightAt(x + step, z));
            maxSimplificationError = Mathf.Max(maxSimplificationError,
                Mathf.Abs(physical - basin.HeightAt(middleX, middleZ)));
        }
        Require(maxSimplificationError < .7f,
            $"visual/collision terrain gap {maxSimplificationError:F3} m");
        var required = new[] { "spawn", "shallow", "middle", "deep", "return", "alternate" };
        Require(required.All(id => data.Nodes.Count(n => n.Id == id) == 1), "route landmarks missing");
        Require(habitat.GetNode<Node3D>("PondLandmarks").GetChildCount() == 6 &&
            habitat.GetNode<Marker3D>("PondLandmarks/BankApproach").Position.Y >
                GeneratedBasin.WaterSurfaceY,
            "inspectable markers missing");
        var nodes = data.Nodes.ToDictionary(n => n.Id, StringComparer.Ordinal);
        var shallow = Depth("shallow");
        var middle = Depth("middle");
        var deep = Depth("deep");
        Require(shallow < middle && middle < deep && deep > 8f,
            "shore-to-core depth sequence invalid");
        Require(basin.DistanceToShore(nodes["spawn"].Position[0], nodes["spawn"].Position[2]) > 5f,
            "spawn too close to bank");
        Require(basin.HeightAt(basin.ShoreMax.X + 25f, 0f) > GeneratedBasin.WaterSurfaceY,
            "bank does not rise above water");
        var graph = data.Edges.GroupBy(e => e.From)
            .ToDictionary(g => g.Key, g => g.Select(e => e.To).ToArray());
        Require(Reach("spawn", "shallow", "middle", "deep", "return", "shallow") &&
            Reach("shallow", "middle", "alternate", "shallow"), "required loop/alternate missing");
        using var planner = new WorldRoutePlanner(habitat.GetWorld3D().DirectSpaceState,
            fish.GetRid(), basin, Array.Empty<GeologicalRegion>());
        var probes = 0;
        foreach (var edge in data.Edges)
        {
            Require(edge.Points.Length >= 2 && nodes.ContainsKey(edge.From) && nodes.ContainsKey(edge.To),
                "route topology invalid");
            for (var segment = 1; segment < edge.Points.Length; segment++)
            {
                var a = Vec(edge.Points[segment - 1]);
                var b = Vec(edge.Points[segment]);
                var steps = Mathf.Max(1, Mathf.CeilToInt(a.DistanceTo(b) / .3f));
                for (var i = 0; i <= steps; i++)
                {
                    var point = a.Lerp(b, i / (float)steps);
                    Require(planner.PointClear(point), $"blocked {edge.Id} at {point}");
                    probes++;
                }
            }
        }
        Require(data.Environment.All(row => row.All(float.IsFinite)), "nonfinite environment field");
        GD.Print($"POND_WORLD_CHECK_OK id={manifest.WorldId} seed={manifest.Seed} content={manifest.ContentSha256} shallow={shallow:F2} middle={middle:F2} deep={deep:F2} probes={probes} collision_vertices={collisionVertices.Length} max_gap={maxSimplificationError:F3} generator_invocations=0");

        float Depth(string id) => GeneratedBasin.WaterSurfaceY - basin.HeightAt(
            nodes[id].Position[0], nodes[id].Position[2]);
        bool Reach(params string[] path)
        {
            for (var i = 1; i < path.Length; i++)
                if (!graph.TryGetValue(path[i - 1], out var destinations) ||
                    !destinations.Contains(path[i], StringComparer.Ordinal)) return false;
            return true;
        }
    }

    private static Vector3 Vec(float[] p) => new(p[0], p[1], p[2]);
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidDataException(message);
    }
}
