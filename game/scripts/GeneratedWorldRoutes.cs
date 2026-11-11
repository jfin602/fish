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
        Func<Vector3, bool>? clear = null)
    {
        if (basin.IsPond)
        {
            BuildPond(basin, clear);
            return;
        }
        var random = new WorldCompileContext(new WorldGenerationSettings(basin.Seed))
            .Stream("route-anchors", "terrain-1");
        var angle = (random.Next() % 6283) / 1000f;
        var forward = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        var side = new Vector2(-forward.Y, forward.X);
        var chosen = new List<Vector3>();
        AddSelected("spawn", "safe-spawn", 6f, basin.DeepCore + forward * 58f);
        AddSelected("shallow", "shallow", 7f, basin.DeepCore + forward * 49f + side * 10f);
        AddSelected("middle", "middle", 14f, basin.DeepCore + forward * 34f);
        AddSelected("deep", "deep", 27f, basin.DeepCore + forward * 8f);
        AddSelected("trench", "deepest-landmark", 35f, basin.DeepCore);
        var middleAt = new Vector2(_nodes["middle"].Position.X, _nodes["middle"].Position.Z);
        var shallowAt = new Vector2(_nodes["shallow"].Position.X, _nodes["shallow"].Position.Z);
        var deepAt = new Vector2(_nodes["deep"].Position.X, _nodes["deep"].Position.Z);
        AddSelected("west-open", "open-water", 16f, middleAt - side * 20f);
        AddSelected("east-open", "open-water", 16f, middleAt + side * 20f);
        AddSelected("west-return", "return", 8f, shallowAt - side * 20f);
        AddSelected("east-return", "alternate", 8f, shallowAt + side * 20f);
        AddSelected("deep-west", "landmark-approach", 24f, deepAt - side * 15f);
        AddSelected("shallow-east", "landmark-approach", 8f, shallowAt + side * 20f);
        Add("spawn-shallow", "spawn", "shallow", "open-water");
        Add("shallow-middle", "shallow", "middle", "shallow-middle");
        Add("middle-deep", "middle", "deep", "middle-deep");
        Add("middle-west", "middle", "west-open", "open-water");
        Add("middle-east", "middle", "east-open", "open-water");
        Add("west-return", "west-open", "west-return", "return");
        Add("return-shallow", "west-return", "shallow", "return");
        Add("east-return", "east-open", "east-return", "alternate");
        Add("alternate-shallow", "east-return", "shallow", "alternate");
        Add("deep-landmark", "deep", "deep-west", "landmark-approach");
        Add("shallow-landmark", "shallow", "shallow-east", "landmark-approach");
        Add("trench-landmark", "deep", "trench", "landmark-approach");
        foreach (var region in regions.OrderBy(r => r.Id, StringComparer.Ordinal))
        {
            var line = region.Centerline;
            var entry = region.Id + ":entry";
            var exit = region.Id + ":exit";
            AddNode(entry, line[0], region.Kind + "-entrance");
            AddNode(exit, line[^1], region.Kind + "-entrance");
            _edges.Add(new WorldRouteEdge(region.Id, entry, exit, region.Kind,
                (Vector3[])line.Clone(), EnclosedExclusionRadius, CameraExclusionRadius));
            var before = line[0] + (line[0] - line[1]).Normalized() * 4f;
            var after = line[^1] + (line[^1] - line[^2]).Normalized() * 4f;
            var approach = region.Id + ":approach";
            var departure = region.Id + ":departure";
            AddNode(approach, before, "entrance-approach");
            Add(region.Id + ":in", approach, entry, "entrance");
            var hubs = new[] { "middle", "shallow", "deep", "west-open", "east-open",
                "west-return", "east-return", "deep-west", "shallow-east" };
            var hub = hubs
                .OrderBy(id => _nodes[id].Position.DistanceTo(before)).First();
            Add(region.Id + ":link-in", hub, approach, "open-water");
            AddNode(departure, after, "entrance-approach");
            Add(region.Id + ":out", exit, departure, "entrance");
            var farHub = hubs.OrderBy(id => _nodes[id].Position.DistanceTo(after) +
                (id == hub ? 18f : 0f)).First();
            Add(region.Id + ":link-out", departure, farHub, "open-water");
        }

        void AddSelected(string id, string role, float targetDepth, Vector2 target)
        {
            var best = Vector3.Zero;
            var bestScore = float.NegativeInfinity;
            for (var z = -120; z <= 120; z += 4)
            for (var x = -120; x <= 120; x += 4)
            {
                var shore = basin.DistanceToShore(x, z);
                if (shore < 13f) continue;
                var floor = basin.HeightAt(x, z);
                var depth = GeneratedBasin.WaterSurfaceY - floor;
                if (depth < targetDepth - 5f || depth > targetDepth + 5f) continue;
                var y = Mathf.Min(floor + 2.4f, 5.5f);
                if (y < floor + FishRadius + .12f || y > 6.2f) continue;
                var position = new Vector3(x, y, z);
                var score = -new Vector2(x, z).DistanceTo(target) * .16f -
                    Mathf.Abs(depth - targetDepth) * 1.2f +
                    Mathf.Min(shore, 32f) * .025f;
                if (score <= bestScore) continue;
                if (chosen.Any(p => p.DistanceTo(position) < 7f) ||
                    regions.Any(r => r.Centerline.Any(p =>
                        new Vector2(p.X - x, p.Z - z).Length() < 10f)) ||
                    (clear is not null && !clear(position))) continue;
                bestScore = score;
                best = position;
            }
            if (!float.IsFinite(bestScore))
                throw new InvalidOperationException($"No clear terrain anchor for {id} depth={targetDepth}");
            chosen.Add(best);
            AddNode(id, best, role);
        }
    }

    private void BuildPond(GeneratedBasin basin, Func<Vector3, bool>? clear)
    {
        var random = new WorldCompileContext(new WorldGenerationSettings(basin.Seed, 2, "pond-full-1"))
            .Stream("route-anchors", "pond-1");
        var angle = (random.Next() % 6283) / 1000f;
        var forward = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        var side = new Vector2(-forward.Y, forward.X);
        var core = basin.DeepCore;
        var selected = new List<Vector3>();
        Choose("spawn", "safe-spawn", 3.2f, 5.5f, core + forward * 33f);
        Choose("shallow", "bank-shallow", 2.4f, 4.8f, core + forward * 30f + side * 9f);
        Choose("middle", "middle-shelf", 5.5f, 9f, core + forward * 17f);
        Choose("deep", "deep-basin", 9f, 16f, core + forward * 4f);
        Choose("return", "return-shelf", 5f, 9f, core + forward * 15f - side * 14f);
        Choose("alternate", "alternate-safe", 4f, 8f, core + forward * 23f + side * 14f);
        Add("spawn-shallow", "spawn", "shallow", "shore-approach");
        Add("shallow-middle", "shallow", "middle", "shallow-middle");
        Add("middle-deep", "middle", "deep", "middle-deep");
        Add("deep-return", "deep", "return", "deep-return");
        Add("return-shallow", "return", "shallow", "return");
        Add("middle-alternate", "middle", "alternate", "alternate");
        Add("alternate-shallow", "alternate", "shallow", "alternate");

        void Choose(string id, string role, float minDepth, float maxDepth, Vector2 target)
        {
            var bestScore = float.NegativeInfinity;
            var best = Vector3.Zero;
            for (var z = -90; z <= 90; z += 2)
            for (var x = -90; x <= 90; x += 2)
            {
                var bank = basin.DistanceToShore(x, z);
                if (bank < 5f) continue;
                var floor = basin.HeightAt(x, z);
                var depth = GeneratedBasin.WaterSurfaceY - floor;
                if (depth < minDepth || depth > maxDepth) continue;
                var y = Mathf.Min(floor + 1.9f, GeneratedBasin.WaterSurfaceY - 2.5f);
                var point = new Vector3(x, y, z);
                if (selected.Exists(p => p.DistanceTo(point) < 6f) ||
                    (clear is not null && !clear(point))) continue;
                var score = -new Vector2(x, z).DistanceTo(target) -
                    Mathf.Abs(depth - (minDepth + maxDepth) * .5f) * .5f +
                    Mathf.Min(bank, 16f) * .03f;
                if (score <= bestScore) continue;
                best = point;
                bestScore = score;
            }
            if (!float.IsFinite(bestScore))
                throw new InvalidOperationException($"Pond route anchor impossible: {id}, seed={basin.Seed}");
            selected.Add(best);
            AddNode(id, best, role);
        }
    }

    private void AddNode(string id, Vector3 position, string role) =>
        _nodes.Add(id, new WorldRouteNode(id, position, role, FishRadius, CameraRadius, CameraOffsetY));

    private void Add(string id, string from, string to, string kind) =>
        _edges.Add(new WorldRouteEdge(id, from, to, kind,
            new[] { _nodes[from].Position, _nodes[to].Position }, OpenExclusionRadius,
            CameraExclusionRadius));

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
    private const float Step = 3.5f;
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
        var count = Mathf.Max(1, Mathf.CeilToInt(a.DistanceTo(b) / .3f));
        for (var i = 0; i <= count; i++)
            if (!Clear(a.Lerp(b, i / (float)count))) return false;
        return true;
    }

    private bool Clear(Vector3 point)
    {
        if (_basin.DistanceToShore(point.X, point.Z) < 2f ||
            point.Y < -40f || point.Y > GeneratedBasin.WaterSurfaceY -
            GeneratedWorldRoutes.CameraOffsetY - GeneratedWorldRoutes.CameraRadius - .05f)
            return false;
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
        // The seabed collider is a 2 m triangulated mesh; its linear surface can
        // sit above the analytic height between samples near a steep ridge.
        if (point.Y < _basin.HeightAt(point.X, point.Z) + radius + .7f) return false;
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
        foreach (var lift in new[] { 2f, 4f, 7f, 10f })
        {
            var curved = FindCurved(start, goal, lift);
            if (curved is not null) return curved;
        }
        foreach (var height in new[] { 5.5f, 3.5f, 1.5f, -1f, -4f })
        {
            var level = FindAtHeight(start, goal, height);
            if (level is not null) return level;
        }
        var origin = start;
        var first = new Cell(0, 0, 0);
        var queue = new PriorityQueue<Cell, float>();
        var cost = new Dictionary<Cell, float> { [first] = 0f };
        var parent = new Dictionary<Cell, Cell>();
        queue.Enqueue(first, start.DistanceTo(goal));
        var expansions = 0;
        while (queue.Count > 0 && expansions++ < 3500)
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

    private Vector3[]? FindCurved(Vector3 start, Vector3 goal, float lift)
    {
        var delta = new Vector2(goal.X - start.X, goal.Z - start.Z);
        var lengthSquared = Mathf.Max(delta.LengthSquared(), 1f);
        var maxY = GeneratedBasin.WaterSurfaceY - GeneratedWorldRoutes.CameraOffsetY -
            GeneratedWorldRoutes.CameraRadius - .05f;
        Vector3 At(float x, float z)
        {
            var t = Mathf.Clamp(new Vector2(x - start.X, z - start.Z).Dot(delta) /
                lengthSquared, 0f, 1f);
            return new Vector3(x, Mathf.Min(maxY,
                Mathf.Lerp(start.Y, goal.Y, t) + lift * Mathf.Sin(Mathf.Pi * t)), z);
        }
        var direct = new[] { start, At(Mathf.Lerp(start.X, goal.X, .25f),
            Mathf.Lerp(start.Z, goal.Z, .25f)), At(Mathf.Lerp(start.X, goal.X, .5f),
            Mathf.Lerp(start.Z, goal.Z, .5f)), At(Mathf.Lerp(start.X, goal.X, .75f),
            Mathf.Lerp(start.Z, goal.Z, .75f)), goal };
        if (LineClear(direct)) return direct;
        const float grid = 4f;
        var first = Vector2I.Zero;
        var queue = new PriorityQueue<Vector2I, float>();
        var cost = new Dictionary<Vector2I, float> { [first] = 0f };
        var parent = new Dictionary<Vector2I, Vector2I>();
        queue.Enqueue(first, start.DistanceTo(goal));
        var lowX = Mathf.Min(start.X, goal.X) - 32f;
        var highX = Mathf.Max(start.X, goal.X) + 32f;
        var lowZ = Mathf.Min(start.Z, goal.Z) - 32f;
        var highZ = Mathf.Max(start.Z, goal.Z) + 32f;
        var expansions = 0;
        while (queue.Count > 0 && expansions++ < 2400)
        {
            var cell = queue.Dequeue();
            var here = At(start.X + cell.X * grid, start.Z + cell.Y * grid);
            if (here.DistanceTo(goal) < 6f && LineClear(here, goal))
            {
                var reverse = new List<Vector3> { goal, here };
                while (parent.TryGetValue(cell, out var previous))
                {
                    cell = previous;
                    reverse.Add(At(start.X + cell.X * grid, start.Z + cell.Y * grid));
                }
                reverse.Reverse();
                var smooth = new List<Vector3> { start };
                for (var k = 1; k < reverse.Count;)
                {
                    var next = reverse.Count - 1;
                    while (next > k && !LineClear(smooth[^1], reverse[next])) next--;
                    smooth.Add(reverse[next]);
                    k = next + 1;
                }
                return smooth.ToArray();
            }
            for (var dz = -1; dz <= 1; dz++)
            for (var dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dz == 0) continue;
                var next = cell + new Vector2I(dx, dz);
                var at = At(start.X + next.X * grid, start.Z + next.Y * grid);
                if (at.X < lowX || at.X > highX || at.Z < lowZ || at.Z > highZ ||
                    !Clear(at) || !LineClear(here, at)) continue;
                var candidate = cost[cell] + here.DistanceTo(at);
                if (cost.TryGetValue(next, out var old) && candidate >= old - .0001f) continue;
                cost[next] = candidate;
                parent[next] = cell;
                queue.Enqueue(next, candidate + at.DistanceTo(goal));
            }
        }
        return null;
    }

    private Vector3[]? FindAtHeight(Vector3 start, Vector3 goal, float height)
    {
        var aboveStart = new Vector3(start.X, height, start.Z);
        var aboveGoal = new Vector3(goal.X, height, goal.Z);
        if (!LineClear(start, aboveStart) || !LineClear(aboveGoal, goal)) return null;
        if (LineClear(aboveStart, aboveGoal))
            return new[] { start, aboveStart, aboveGoal, goal };
        const float grid = 4f;
        var first = Vector2I.Zero;
        var queue = new PriorityQueue<Vector2I, float>();
        var cost = new Dictionary<Vector2I, float> { [first] = 0f };
        var parent = new Dictionary<Vector2I, Vector2I>();
        queue.Enqueue(first, aboveStart.DistanceTo(aboveGoal));
        var lowX = Mathf.Min(start.X, goal.X) - 36f;
        var highX = Mathf.Max(start.X, goal.X) + 36f;
        var lowZ = Mathf.Min(start.Z, goal.Z) - 36f;
        var highZ = Mathf.Max(start.Z, goal.Z) + 36f;
        var expansions = 0;
        while (queue.Count > 0 && expansions++ < 3500)
        {
            var cell = queue.Dequeue();
            var here = At(cell);
            if (here.DistanceTo(aboveGoal) < 6f && LineClear(here, aboveGoal))
            {
                var reverse = new List<Vector3> { aboveGoal, here };
                while (parent.TryGetValue(cell, out var previous))
                {
                    cell = previous;
                    reverse.Add(At(cell));
                }
                reverse.Reverse();
                var smooth = new List<Vector3> { start, aboveStart };
                for (var k = 1; k < reverse.Count;)
                {
                    var next = reverse.Count - 1;
                    while (next > k && !LineClear(smooth[^1], reverse[next])) next--;
                    smooth.Add(reverse[next]);
                    k = next + 1;
                }
                smooth.Add(goal);
                return smooth.ToArray();
            }
            for (var dz = -1; dz <= 1; dz++)
            for (var dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dz == 0) continue;
                var next = cell + new Vector2I(dx, dz);
                var at = At(next);
                if (at.X < lowX || at.X > highX || at.Z < lowZ || at.Z > highZ ||
                    !Clear(at) || !LineClear(here, at)) continue;
                var candidate = cost[cell] + here.DistanceTo(at);
                if (cost.TryGetValue(next, out var old) && candidate >= old - .0001f) continue;
                cost[next] = candidate;
                parent[next] = cell;
                queue.Enqueue(next, candidate + at.DistanceTo(aboveGoal));
            }
        }
        return null;

        Vector3 At(Vector2I cell) => aboveStart + new Vector3(cell.X * grid, 0f, cell.Y * grid);
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
