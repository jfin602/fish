#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Fish;

public sealed record WorldRouteNode(string Id, Vector3 Position, string Role, float FishRadius,
    float CameraRadius, float CameraOffsetY);
public sealed record WorldRouteEdge(string Id, string From, string To, string Kind,
    Vector3[] Points, float ProtectedRadius, float CameraProtectedRadius);

/// <summary>Immutable, named routes compiled after the basin and geological shells.</summary>
public sealed class GeneratedWorldRoutes
{
    public const float FishRadius = .48f;
    public const float CameraRadius = .22f;
    public const float CameraOffsetY = 2f;
    public const float OpenExclusionRadius = 2.2f;
    public const float EnclosedExclusionRadius = 1.0f;
    public const float CameraExclusionRadius = .65f;
    private readonly Dictionary<string, WorldRouteNode> _nodes = new(StringComparer.Ordinal);
    private readonly List<WorldRouteEdge> _edges = new();
    public IReadOnlyDictionary<string, WorldRouteNode> Nodes => _nodes;
    public IReadOnlyList<WorldRouteEdge> Edges => _edges;

    public int RepairOpenConnectors(PhysicsDirectSpaceState3D space, Rid fishRid,
        GeneratedBasin basin, IReadOnlyList<GeologicalRegion> regions)
    {
        WorldGenerationAudit.MarkStage();
        using var planner = new WorldRoutePlanner(space, fishRid, basin, regions);
        var repaired = 0;
        for (var i = 0; i < _edges.Count; i++)
        {
            var edge = _edges[i];
            if (edge.Kind is "cave" or "tunnel" or "arch" or "canyon") continue;
            if (planner.LineClear(edge.Points)) continue;
            var points = planner.Find(edge.Points[0], edge.Points[^1]);
            if (points is null)
                throw new InvalidOperationException($"No clear connector for route {edge.Id} from={edge.Points[0]} to={edge.Points[^1]} start_clear={planner.PointClear(edge.Points[0])} goal_clear={planner.PointClear(edge.Points[^1])}");
            _edges[i] = edge with { Points = points };
            repaired++;
        }
        return repaired;
    }

    public GeneratedWorldRoutes(GeneratedBasin basin, IReadOnlyList<GeologicalRegion> regions,
        Vector3 spawn)
    {
        AddNode("spawn", spawn, "safe-spawn");
        AddNode("middle", new Vector3(8, -2f, -3), "middle");
        AddNode("shallow", new Vector3(0, 4.5f, 24), "shallow");
        AddOpen("deep", basin.DeepCore.X, basin.DeepCore.Y, "deep");
        AddOpen("west-open", -23, 0, "open-water");
        AddOpen("east-open", 24, 0, "open-water");
        AddNode("west-return", new Vector3(-10, 6.5f, 21), "return");
        AddNode("east-return", new Vector3(10, 6.5f, 21), "alternate");
        AddOpen("deep-west", -13, -18, "landmark-approach");
        AddNode("shallow-east", new Vector3(12, 6.5f, 24), "landmark-approach");
        Add("spawn-middle", "spawn", "middle", "open-water");
        Add("middle-shallow", "middle", "shallow", "shallow-middle");
        Add("middle-deep", "middle", "deep", "middle-deep");
        AddHigh("middle-west", "middle", "west-open", "open-water");
        AddHigh("middle-east", "middle", "east-open", "open-water");
        Add("west-return", "west-open", "west-return", "return");
        Add("return-shallow", "west-return", "shallow", "return");
        Add("east-return", "east-open", "east-return", "alternate");
        Add("alternate-shallow", "east-return", "shallow", "alternate");
        Add("deep-landmark", "deep", "deep-west", "landmark-approach");
        Add("shallow-landmark", "shallow", "shallow-east", "landmark-approach");
        foreach (var region in regions.OrderBy(r => r.Id, StringComparer.Ordinal))
        {
            var line = region.Centerline;
            var entry = region.Id + ":entry";
            var exit = region.Id + ":exit";
            AddNode(entry, line[0], region.Kind + "-entrance");
            AddNode(exit, line[^1], region.Kind + "-entrance");
            _edges.Add(new WorldRouteEdge(region.Id, entry, exit, region.Kind,
                (Vector3[])line.Clone(), EnclosedExclusionRadius, CameraExclusionRadius));
            if (region.Kind == "canyon")
            {
                Add(region.Id + ":link-in", "shallow", entry, "open-water");
                Add(region.Id + ":link-out", exit, "shallow", "open-water");
                continue;
            }
            var before = line[0] + (line[0] - line[1]).Normalized() * 4f;
            var after = line[^1] + (line[^1] - line[^2]).Normalized() * 4f;
            var approach = region.Id + ":approach";
            var departure = region.Id + ":departure";
            AddNode(approach, before, "entrance-approach");
            Add(region.Id + ":in", approach, entry, "entrance");
            var hub = region.Id switch
            {
                "west-swim-through" => "west-open",
                "east-swim-through" => "east-open",
                "canyon-ridge" => "shallow",
                "stone-arch" => "middle",
                _ => "shallow",
            };
            Add(region.Id + ":link-in", hub, approach, "open-water");
            // The generated basin still seals the east tunnel's far mouth. Keep
            // its traversable centreline as an out-and-back branch for this seed.
            if (region.Id != "east-swim-through")
            {
                AddNode(departure, after, "entrance-approach");
                Add(region.Id + ":out", exit, departure, "entrance");
                Add(region.Id + ":link-out", departure, hub, "open-water");
            }
        }

        void AddOpen(string id, float x, float z, string role)
        {
            var y = Mathf.Clamp(basin.HeightAt(x, z) + 3f, -8f, 6.5f);
            AddNode(id, new Vector3(x, y, z), role);
        }
    }

    private void AddNode(string id, Vector3 position, string role) =>
        _nodes.Add(id, new WorldRouteNode(id, position, role, FishRadius, CameraRadius, CameraOffsetY));

    private void Add(string id, string from, string to, string kind) =>
        _edges.Add(new WorldRouteEdge(id, from, to, kind,
            new[] { _nodes[from].Position, _nodes[to].Position }, OpenExclusionRadius,
            CameraExclusionRadius));

    private void AddHigh(string id, string from, string to, string kind)
    {
        var a = _nodes[from].Position;
        var b = _nodes[to].Position;
        _edges.Add(new WorldRouteEdge(id, from, to, kind,
            new[] { a, new Vector3(a.X, 5.5f, a.Z), new Vector3(b.X, 5.5f, b.Z), b },
            OpenExclusionRadius, CameraExclusionRadius));
    }

    public float DistanceToRoute(Vector3 position)
    {
        var best = float.PositiveInfinity;
        foreach (var edge in _edges)
        for (var i = 1; i < edge.Points.Length; i++)
        {
            var a = edge.Points[i - 1];
            var b = edge.Points[i];
            var delta = b - a;
            var t = Mathf.Clamp((position - a).Dot(delta) / Mathf.Max(delta.LengthSquared(), .0001f), 0f, 1f);
            best = Mathf.Min(best, position.DistanceTo(a + delta * t));
        }
        return best;
    }

    public bool IsProtected(Vector3 position, float objectRadius = 0f)
    {
        foreach (var edge in _edges)
        for (var i = 1; i < edge.Points.Length; i++)
        {
            var a = edge.Points[i - 1];
            var delta = edge.Points[i] - a;
            var t = Mathf.Clamp((position - a).Dot(delta) / Mathf.Max(delta.LengthSquared(), .0001f), 0f, 1f);
            if (position.DistanceTo(a + delta * t) < edge.ProtectedRadius + objectRadius) return true;
            var cameraA = a + Vector3.Up * CameraOffsetY;
            var cameraT = Mathf.Clamp((position - cameraA).Dot(delta) /
                Mathf.Max(delta.LengthSquared(), .0001f), 0f, 1f);
            if (position.DistanceTo(cameraA + delta * cameraT) <
                edge.CameraProtectedRadius + objectRadius) return true;
        }
        return false;
    }
}

internal sealed class WorldRoutePlanner : IDisposable
{
    private const float Step = 1.75f;
    private readonly PhysicsDirectSpaceState3D _space;
    private readonly SphereShape3D _shape = new();
    private readonly PhysicsShapeQueryParameters3D _query;
    private readonly GeneratedBasin _basin;
    private readonly List<StaticTriangle> _geometry = new();
    private readonly Dictionary<Vector3I, bool> _clearCache = new();
    private readonly record struct Cell(int X, int Y, int Z);
    private readonly record struct StaticTriangle(Vector3 A, Vector3 B, Vector3 C,
        Vector3 Normal, Vector3 Min, Vector3 Max);
    private static readonly Cell[] Directions = BuildDirections();

    public WorldRoutePlanner(PhysicsDirectSpaceState3D space, Rid fishRid,
        GeneratedBasin basin, IReadOnlyList<GeologicalRegion> regions)
    {
        _space = space;
        _basin = basin;
        _query = new PhysicsShapeQueryParameters3D { Shape = _shape, CollisionMask = 1,
            Exclude = new Godot.Collections.Array<Rid> { fishRid } };
        foreach (var region in regions)
        {
            var vertices = region.Collision.Data;
            for (var i = 0; i < vertices.Length; i += 3)
            {
                var a = vertices[i]; var b = vertices[i + 1]; var c = vertices[i + 2];
                _geometry.Add(new StaticTriangle(a, b, c, (b - a).Cross(c - a).Normalized(),
                    new Vector3(Mathf.Min(a.X, Mathf.Min(b.X, c.X)),
                        Mathf.Min(a.Y, Mathf.Min(b.Y, c.Y)), Mathf.Min(a.Z, Mathf.Min(b.Z, c.Z))),
                    new Vector3(Mathf.Max(a.X, Mathf.Max(b.X, c.X)),
                        Mathf.Max(a.Y, Mathf.Max(b.Y, c.Y)), Mathf.Max(a.Z, Mathf.Max(b.Z, c.Z)))));
            }
        }
    }

    public void Dispose() { _query.Dispose(); _shape.Dispose(); }

    public bool LineClear(Vector3[] points)
    {
        for (var i = 1; i < points.Length; i++)
            if (!LineClear(points[i - 1], points[i])) return false;
        return true;
    }

    public bool PointClear(Vector3 point) => Clear(point);

    private bool LineClear(Vector3 a, Vector3 b)
    {
        var count = Mathf.Max(1, Mathf.CeilToInt(a.DistanceTo(b) / .7f));
        for (var i = 0; i <= count; i++)
            if (!Clear(a.Lerp(b, i / (float)count))) return false;
        return true;
    }

    private bool Clear(Vector3 point)
    {
        if (Mathf.Abs(point.X) > 35f || Mathf.Abs(point.Z) > 35f ||
            point.Y < -11.5f || point.Y > 7.15f) return false;
        var key = new Vector3I(Mathf.RoundToInt(point.X * 100),
            Mathf.RoundToInt(point.Y * 100), Mathf.RoundToInt(point.Z * 100));
        if (_clearCache.TryGetValue(key, out var cached)) return cached;
        var result = StaticClear(point, GeneratedWorldRoutes.FishRadius) &&
            StaticClear(point + Vector3.Up * GeneratedWorldRoutes.CameraOffsetY,
                GeneratedWorldRoutes.CameraRadius) &&
            Probe(point, GeneratedWorldRoutes.FishRadius) &&
            Probe(point + Vector3.Up * GeneratedWorldRoutes.CameraOffsetY,
                GeneratedWorldRoutes.CameraRadius);
        _clearCache.Add(key, result);
        return result;
    }

    private bool StaticClear(Vector3 point, float radius)
    {
        if (point.Y < _basin.HeightAt(point.X, point.Z) + radius + .08f) return false;
        var limit = radius * radius;
        foreach (var t in _geometry)
        {
            if (point.X < t.Min.X - radius || point.X > t.Max.X + radius ||
                point.Y < t.Min.Y - radius || point.Y > t.Max.Y + radius ||
                point.Z < t.Min.Z - radius || point.Z > t.Max.Z + radius) continue;
            var distance = (point - t.A).Dot(t.Normal);
            var projected = point - distance * t.Normal;
            var inside = (t.B - t.A).Cross(projected - t.A).Dot(t.Normal) >= -.0001f &&
                (t.C - t.B).Cross(projected - t.B).Dot(t.Normal) >= -.0001f &&
                (t.A - t.C).Cross(projected - t.C).Dot(t.Normal) >= -.0001f;
            if (inside && distance * distance < limit) return false;
            var ab = Geometry3D.GetClosestPointToSegment(point, t.A, t.B);
            var bc = Geometry3D.GetClosestPointToSegment(point, t.B, t.C);
            var ca = Geometry3D.GetClosestPointToSegment(point, t.C, t.A);
            if (point.DistanceSquaredTo(ab) < limit || point.DistanceSquaredTo(bc) < limit ||
                point.DistanceSquaredTo(ca) < limit) return false;
        }
        return true;
    }

    private bool Probe(Vector3 point, float radius)
    {
        _shape.Radius = radius;
        _query.Transform = new Transform3D(Basis.Identity, point);
        return _space.IntersectShape(_query, 1).Count == 0;
    }

    public Vector3[]? Find(Vector3 start, Vector3 goal)
    {
        if (!Clear(start) || !Clear(goal)) return null;
        var origin = start;
        var first = new Cell(0, 0, 0);
        var queue = new PriorityQueue<Cell, float>();
        var cost = new Dictionary<Cell, float> { [first] = 0f };
        var parent = new Dictionary<Cell, Cell>();
        queue.Enqueue(first, start.DistanceTo(goal));
        var expansions = 0;
        while (queue.Count > 0 && expansions++ < 5500)
        {
            var current = queue.Dequeue();
            var here = Position(current);
            if (here.DistanceTo(goal) < 3.5f && LineClear(here, goal))
            {
                var reverse = new List<Vector3> { goal, here };
                while (parent.TryGetValue(current, out var previous))
                {
                    current = previous;
                    reverse.Add(Position(current));
                }
                reverse.Reverse();
                var smooth = new List<Vector3> { reverse[0] };
                for (var k = 1; k < reverse.Count;)
                {
                    var next = reverse.Count - 1;
                    while (next > k && !LineClear(smooth[^1], reverse[next])) next--;
                    smooth.Add(reverse[next]);
                    k = next + 1;
                }
                return smooth.ToArray();
            }
            foreach (var direction in Directions)
            {
                var next = new Cell(current.X + direction.X, current.Y + direction.Y,
                    current.Z + direction.Z);
                var at = Position(next);
                if (!Clear(at) || !LineClear(here, at)) continue;
                var candidate = cost[current] + here.DistanceTo(at);
                if (cost.TryGetValue(next, out var old) && candidate >= old - .0001f) continue;
                cost[next] = candidate;
                parent[next] = current;
                queue.Enqueue(next, candidate + at.DistanceTo(goal));
            }
        }
        return null;

        Vector3 Position(Cell c) => origin + new Vector3(c.X * Step, c.Y * Step, c.Z * Step);
    }

    private static Cell[] BuildDirections()
    {
        var result = new List<Cell>();
        for (var y = -1; y <= 1; y++)
        for (var z = -1; z <= 1; z++)
        for (var x = -1; x <= 1; x++)
            if (x != 0 || y != 0 || z != 0) result.Add(new Cell(x, y, z));
        return result.ToArray();
    }
}
