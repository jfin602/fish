using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Fish;

public static class RouteAnchorChecks
{
    public static void Run()
    {
        Vector3? previousSpawn = null;
        for (ulong seed = 42; seed <= 51; seed++)
        {
            var basin = new GeneratedBasin(seed);
            var regions = GeologicalRegionGenerator.Generate(basin);
            var routes = new GeneratedWorldRoutes(basin, regions);
            var repeat = new GeneratedWorldRoutes(basin, regions);
            Require(routes.Nodes.Count == 31 && routes.Edges.Count == 37,
                $"seed {seed}: route inventory");
            Require(routes.Nodes.Keys.SequenceEqual(repeat.Nodes.Keys) &&
                routes.Edges.Select(e => e.Id).SequenceEqual(repeat.Edges.Select(e => e.Id)),
                $"seed {seed}: route identity changed");
            foreach (var node in routes.Nodes.Values)
            {
                Require(node.Position == repeat.Nodes[node.Id].Position,
                    $"seed {seed}: anchor changed {node.Id}");
                Require(basin.DistanceToShore(node.Position.X, node.Position.Z) > 2f,
                    $"seed {seed}: anchor outside water {node.Id}");
            }
            foreach (var edge in routes.Edges)
            {
                Require(edge.Points[0] == routes.Nodes[edge.From].Position &&
                    edge.Points[^1] == routes.Nodes[edge.To].Position,
                    $"seed {seed}: edge endpoints {edge.Id}");
                Require(routes.IsProtected(edge.Points[0]) && routes.IsProtected(edge.Points[^1]),
                    $"seed {seed}: route protection {edge.Id}");
            }
            foreach (var region in regions)
            {
                Require(routes.Edges.Any(e => e.Id == region.Id + ":link-in") &&
                    routes.Edges.Any(e => e.Id == region.Id + ":link-out"),
                    $"seed {seed}: geology reconnection {region.Id}");
            }
            var connected = new HashSet<string> { "spawn" };
            var pending = new Queue<string>(); pending.Enqueue("spawn");
            while (pending.Count > 0)
            {
                var current = pending.Dequeue();
                foreach (var edge in routes.Edges)
                {
                    var next = edge.From == current ? edge.To : edge.To == current ? edge.From : null;
                    if (next is not null && connected.Add(next)) pending.Enqueue(next);
                }
            }
            Require(connected.Count == routes.Nodes.Count, $"seed {seed}: graph disconnected");
            var spawn = routes.Nodes["spawn"].Position;
            if (previousSpawn is { } old) Require(old.DistanceTo(spawn) > 5f,
                $"seed {seed}: spawn repeats prior seed");
            previousSpawn = spawn;
            GD.Print($"ROUTE_ANCHOR_SEED seed={seed} spawn={spawn} middle={routes.Nodes["middle"].Position} " +
                $"deep={routes.Nodes["deep"].Position} nodes={routes.Nodes.Count} edges={routes.Edges.Count}");
        }
        GD.Print("ROUTE_ANCHOR_CHECK_OK seeds=42..51 deterministic=true different=true connected=true geology=true");
    }

    private static void Require(bool condition, string reason)
    {
        if (!condition) throw new InvalidOperationException(reason);
    }
}
