#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Fish;

public static class GeneratedRouteChecks
{
    public static void Run(Main main)
    {
        var seabed = main.GetNode<SeabedSurface>("Habitat/Seabed");
        var reef = main.GetNode<ReefComposition>("Habitat/Composition/ReefComposition");
        var geology = reef.GetNode<FafGeologyWorld>("Geology");
        var routes = reef.GeneratedRoutes ?? throw new Exception("route owner missing");
        var analysis = reef.EnvironmentAnalysis ?? throw new Exception("analysis owner missing");
        Require(routes.Nodes.Count >= 20 && routes.Edges.Count >= 30, "route inventory incomplete");
        Require(routes.Nodes["spawn"].Position.DistanceTo(main.GetNode<Marker3D>("Habitat/Spawn").Position) < .001f,
            "spawn route differs from playable spawn");
        foreach (var id in new[] { "shallow", "middle", "deep", "west-return", "east-return",
            "deep-west", "shallow-east" }) Require(routes.Nodes.ContainsKey(id), "missing route node " + id);
        var seen = new HashSet<string>(StringComparer.Ordinal) { "spawn" };
        var queue = new Queue<string>(); queue.Enqueue("spawn");
        while (queue.Count > 0)
        {
            var here = queue.Dequeue();
            foreach (var edge in routes.Edges.Where(e => e.From == here || e.To == here))
            {
                var next = edge.From == here ? edge.To : edge.From;
                if (seen.Add(next)) queue.Enqueue(next);
            }
        }
        Require(seen.Count == routes.Nodes.Count, "route graph disconnected");
        Require(routes.Nodes["shallow"].Position.Y > routes.Nodes["middle"].Position.Y + 4f &&
            routes.Nodes["middle"].Position.Y > routes.Nodes["deep"].Position.Y + 4f,
            "shallow/middle/deep fish depths are unordered");
        Require(Reachable("shallow", "deep", Array.Empty<string>()) &&
            Reachable("deep", "shallow", Array.Empty<string>()), "deep route has no return");
        Require(Reachable("spawn", "shallow", new[] { "middle-shallow", "west-return" }) &&
            Reachable("spawn", "shallow", new[] { "middle-shallow", "east-return" }),
            "alternate shallow paths missing");
        foreach (var region in geology.GeneratedRegions)
        {
            var edge = routes.Edges.Single(e => e.Id == region.Id);
            Require(edge.Points.Length == region.Centerline.Length &&
                edge.Points[0].DistanceTo(region.Centerline[0]) < .001f &&
                edge.Points[^1].DistanceTo(region.Centerline[^1]) < .001f,
                "geology route differs from generated centerline " + region.Id);
            Require(routes.Edges.Any(e => e.Id == region.Id + ":link-in"),
                "missing cave approach " + region.Id);
        }
        var fish = main.GetNode<FishController>("Fish");
        var space = main.GetWorld3D().DirectSpaceState;
        var shape = new SphereShape3D();
        var query = new PhysicsShapeQueryParameters3D { Shape = shape, CollisionMask = 1 };
        query.Exclude = new Godot.Collections.Array<Rid> { fish.GetRid() };
        var probes = 0;
        var blocked = new Dictionary<string, string>();
        foreach (var edge in routes.Edges)
        for (var segment = 1; segment < edge.Points.Length; segment++)
        {
            Require(edge.ProtectedRadius >= GeneratedWorldRoutes.FishRadius &&
                edge.CameraProtectedRadius >= GeneratedWorldRoutes.CameraRadius,
                "route exclusion smaller than fish/camera envelope " + edge.Id);
            var a = edge.Points[segment - 1];
            var b = edge.Points[segment];
            var count = Mathf.Max(1, Mathf.CeilToInt(a.DistanceTo(b) / .75f));
            for (var step = 0; step <= count; step++)
            {
                var point = a.Lerp(b, step / (float)count);
                if (!Clear(point, GeneratedWorldRoutes.FishRadius, out var fishHit))
                    blocked.TryAdd(edge.Id, $"fish point={point} by={fishHit}");
                if (!Clear(point + Vector3.Up * GeneratedWorldRoutes.CameraOffsetY,
                    GeneratedWorldRoutes.CameraRadius, out var cameraHit))
                    blocked.TryAdd(edge.Id, $"camera point={point} by={cameraHit}");
                Require(routes.IsProtected(point), "route corridor is not reserved");
                Require(routes.IsProtected(point + Vector3.Up * GeneratedWorldRoutes.CameraOffsetY),
                    "camera corridor is not reserved");
                probes++;
            }
        }
        foreach (var (id, cause) in blocked) GD.Print($"ROUTE_BLOCKED edge={id} {cause}");
        Require(blocked.Count == 0, $"{blocked.Count} route edges blocked");
        var repeat = new GeneratedWorldRoutes(new GeneratedBasin(seabed.Basin.Seed),
            GeologicalRegionGenerator.Generate(new GeneratedBasin(seabed.Basin.Seed)), Vector3.Zero);
        repeat.RepairOpenConnectors(space, fish.GetRid(), new GeneratedBasin(seabed.Basin.Seed),
            GeologicalRegionGenerator.Generate(new GeneratedBasin(seabed.Basin.Seed)));
        Require(routes.Nodes.Keys.SequenceEqual(repeat.Nodes.Keys) &&
            routes.Edges.Select(e => e.Id).SequenceEqual(repeat.Edges.Select(e => e.Id)), "route order varies");
        foreach (var id in routes.Nodes.Keys)
            Require(routes.Nodes[id].Position.DistanceTo(repeat.Nodes[id].Position) < .0001f,
                "route node varies " + id);
        for (var i = 0; i < routes.Edges.Count; i++)
        {
            var a = routes.Edges[i].Points; var b = repeat.Edges[i].Points;
            Require(a.Length == b.Length, "route polyline length varies " + routes.Edges[i].Id);
            for (var j = 0; j < a.Length; j++)
                Require(a[j].DistanceTo(b[j]) < .0001f,
                    "route polyline varies " + routes.Edges[i].Id);
        }
        Require(analysis.CachedSampleCount == WorldEnvironmentAnalysis.CellsPerSide * WorldEnvironmentAnalysis.CellsPerSide &&
            analysis.TriangleCount == geology.GeneratedRegions.Sum(r => r.Collision.Data.Length / 3),
            "analysis grid or triangle cache incomplete");
        var other = new WorldEnvironmentAnalysis(new GeneratedBasin(seabed.Basin.Seed),
            GeologicalRegionGenerator.Generate(new GeneratedBasin(seabed.Basin.Seed)), repeat);
        var samples = 0;
        for (var z = 0; z < WorldEnvironmentAnalysis.CellsPerSide; z++)
        for (var x = 0; x < WorldEnvironmentAnalysis.CellsPerSide; x++)
        {
            var a = analysis.SampleGrid(x, z);
            Require(Finite(a.Surface) && Finite(a.Normal) && float.IsFinite(a.Depth) &&
                float.IsFinite(a.Slope) && float.IsFinite(a.Concavity) &&
                float.IsFinite(a.PerimeterDistance) && float.IsFinite(a.DeepCoreDistance) &&
                float.IsFinite(a.RouteDistance) && float.IsFinite(a.CaveDistance) &&
                float.IsFinite(a.EntranceDistance) && float.IsFinite(a.Shelter) &&
                float.IsFinite(a.Exposure) && float.IsFinite(a.GeologyDistance), "nonfinite analysis field");
            Require(a.Depth >= 0 && a.Slope >= 0 && a.Slope <= Mathf.Pi / 2 + .001f &&
                a.Concavity >= -1 && a.Concavity <= 1 &&
                a.PerimeterDistance >= 0 && a.DeepCoreDistance >= 0 && a.RouteDistance >= 0 &&
                a.CaveDistance >= 0 && a.EntranceDistance >= 0 && a.GeologyDistance >= 0 &&
                a.Shelter >= 0 && a.Shelter <= 1 && a.Exposure >= 0 && a.Exposure <= 1,
                "analysis field out of range");
            if (x < 4 || z < 4 || x >= WorldEnvironmentAnalysis.CellsPerSide - 4 ||
                z >= WorldEnvironmentAnalysis.CellsPerSide - 4 ||
                (x - 4) % 3 != 0 || (z - 4) % 3 != 0) continue;
            var b = other.SampleGrid(x, z);
            Require(a.Surface.DistanceTo(b.Surface) < .0001f && a.Normal.DistanceTo(b.Normal) < .0001f &&
                Mathf.Abs(a.RouteDistance - b.RouteDistance) < .0001f &&
                Mathf.Abs(a.Shelter - b.Shelter) < .0001f, "analysis varies for same seed");
            samples++;
        }
        var cave = geology.GeneratedRegions.Single(r => r.Kind == "cave");
        var center = cave.Centerline[8];
        var down = analysis.RaycastSurface(center, Vector3.Down, 8);
        var up = analysis.RaycastSurface(center, Vector3.Up, 8);
        var wall = analysis.RaycastSurface(center, Vector3.Back, 10);
        Require(down.SurfaceId == cave.Id && up.SurfaceId == cave.Id &&
            wall.SurfaceId == cave.Id &&
            down.Position.Y < center.Y && up.Position.Y > center.Y &&
            wall.Position.Z > center.Z,
            "arbitrary 3D cave projection failed");
        foreach (var direction in new[] { Vector3.Down, Vector3.Up, Vector3.Back })
        {
            var projected = analysis.SampleProjected(center, direction, 10);
            var repeated = other.SampleProjected(center, direction, 10);
            Require(projected.Hit.SurfaceId == cave.Id &&
                projected.Fields.Surface.DistanceTo(projected.Hit.Position) < .0001f &&
                Finite(projected.Fields.Normal) && float.IsFinite(projected.Fields.Depth) &&
                float.IsFinite(projected.Fields.Concavity) &&
                float.IsFinite(projected.Fields.RouteDistance) &&
                projected.Fields.Shelter is >= 0f and <= 1f &&
                projected.Hit.Position.DistanceTo(repeated.Hit.Position) < .0001f &&
                Mathf.Abs(projected.Fields.Concavity - repeated.Fields.Concavity) < .0001f,
                "3D surface fields missing or invalid");
        }
        GD.Print($"ROUTE_FIELD_CHECK_OK nodes={routes.Nodes.Count} edges={routes.Edges.Count} " +
            $"clearance_probes={probes} grid={analysis.CachedSampleCount} triangles={analysis.TriangleCount} " +
            $"sample_repeat={samples} cave_floor_ceiling=mesh");

        bool Clear(Vector3 point, float radius, out string collider)
        {
            shape.Radius = radius;
            query.Transform = new Transform3D(Basis.Identity, point);
            var hits = space.IntersectShape(query, 1);
            collider = hits.Count == 0 ? "" : (hits[0].GetValueOrDefault("collider").AsGodotObject() as Node)?.GetPath().ToString() ?? "unknown";
            return hits.Count == 0;
        }

        bool Reachable(string from, string to, IReadOnlyCollection<string> disabled)
        {
            var visited = new HashSet<string> { from };
            var pending = new Queue<string>(); pending.Enqueue(from);
            while (pending.Count > 0)
            {
                var current = pending.Dequeue();
                if (current == to) return true;
                foreach (var edge in routes.Edges)
                {
                    if (disabled.Contains(edge.Id)) continue;
                    var next = edge.From == current ? edge.To : edge.To == current ? edge.From : null;
                    if (next is not null && visited.Add(next)) pending.Enqueue(next);
                }
            }
            return false;
        }
    }

    private static bool Finite(Vector3 point) => float.IsFinite(point.X) &&
        float.IsFinite(point.Y) && float.IsFinite(point.Z);
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
