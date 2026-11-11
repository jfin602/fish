#nullable enable
using System;
using System.Collections.Generic;
using Godot;

namespace Fish;

public readonly record struct WorldSurfaceHit(Vector3 Position, Vector3 Normal, string SurfaceId,
    float Distance);
public readonly record struct WorldProjectedSample(WorldSurfaceHit Hit, WorldEnvironmentSample Fields);
public readonly record struct WorldEnvironmentSample(Vector3 Surface, Vector3 Normal, float Depth,
    float Slope, float Concavity, float PerimeterDistance, float DeepCoreDistance,
    float RouteDistance, float CaveDistance, float EntranceDistance, float Shelter,
    float Exposure, float GeologyDistance);

/// <summary>Two metre placement grid plus exact on-demand ray queries of generated mesh triangles.</summary>
public sealed class WorldEnvironmentAnalysis
{
    public const float CellSize = 2f;
    public const int CellsPerSide = 41;
    public const float GridMin = -40f;
    public const float WaterSurfaceY = 10.39f;
    private readonly GeneratedBasin _basin;
    private readonly GeneratedWorldRoutes _routes;
    private readonly IReadOnlyList<GeologicalRegion> _regions;
    private readonly List<Triangle> _triangles = new();
    private readonly WorldEnvironmentSample[] _grid = new WorldEnvironmentSample[CellsPerSide * CellsPerSide];
    private readonly float[] _heights = new float[CellsPerSide * CellsPerSide];
    private readonly Dictionary<int, List<int>> _verticalBuckets = new();
    private readonly record struct Triangle(Vector3 A, Vector3 B, Vector3 C, Vector3 Normal,
        string Id, string Kind, Vector3 Min, Vector3 Max);
    public int CachedSampleCount => _grid.Length;
    public int TriangleCount => _triangles.Count;

    public WorldEnvironmentAnalysis(GeneratedBasin basin, IReadOnlyList<GeologicalRegion> regions,
        GeneratedWorldRoutes routes)
    {
        WorldGenerationAudit.MarkStage();
        _basin = basin;
        _regions = regions;
        _routes = routes;
        foreach (var region in regions)
        {
            var vertices = region.Collision.Data;
            for (var i = 0; i < vertices.Length; i += 3)
            {
                var a = vertices[i]; var b = vertices[i + 1]; var c = vertices[i + 2];
                var normal = (b - a).Cross(c - a).Normalized();
                var index = _triangles.Count;
                var lower = new Vector3(Mathf.Min(a.X, Mathf.Min(b.X, c.X)),
                    Mathf.Min(a.Y, Mathf.Min(b.Y, c.Y)), Mathf.Min(a.Z, Mathf.Min(b.Z, c.Z)));
                var upper = new Vector3(Mathf.Max(a.X, Mathf.Max(b.X, c.X)),
                    Mathf.Max(a.Y, Mathf.Max(b.Y, c.Y)), Mathf.Max(a.Z, Mathf.Max(b.Z, c.Z)));
                _triangles.Add(new Triangle(a, b, c, normal, region.Id, region.Kind, lower, upper));
                var minX = Mathf.Clamp(Mathf.FloorToInt((Mathf.Min(a.X, Mathf.Min(b.X, c.X)) - GridMin) / CellSize), 0, CellsPerSide - 1);
                var maxX = Mathf.Clamp(Mathf.CeilToInt((Mathf.Max(a.X, Mathf.Max(b.X, c.X)) - GridMin) / CellSize), 0, CellsPerSide - 1);
                var minZ = Mathf.Clamp(Mathf.FloorToInt((Mathf.Min(a.Z, Mathf.Min(b.Z, c.Z)) - GridMin) / CellSize), 0, CellsPerSide - 1);
                var maxZ = Mathf.Clamp(Mathf.CeilToInt((Mathf.Max(a.Z, Mathf.Max(b.Z, c.Z)) - GridMin) / CellSize), 0, CellsPerSide - 1);
                for (var z = minZ; z <= maxZ; z++)
                for (var x = minX; x <= maxX; x++)
                {
                    var key = z * CellsPerSide + x;
                    if (!_verticalBuckets.TryGetValue(key, out var bucket))
                        _verticalBuckets[key] = bucket = new List<int>();
                    bucket.Add(index);
                }
            }
        }
        for (var z = 0; z < CellsPerSide; z++)
        for (var x = 0; x < CellsPerSide; x++)
        {
            var p = new Vector3(GridMin + x * CellSize, 60f, GridMin + z * CellSize);
            _heights[z * CellsPerSide + x] = ProjectDown(p).Position.Y;
        }
        for (var z = 0; z < CellsPerSide; z++)
        for (var x = 0; x < CellsPerSide; x++)
            _grid[z * CellsPerSide + x] = Compute(x, z);
    }

    public WorldEnvironmentSample SampleGrid(int x, int z) =>
        _grid[Mathf.Clamp(z, 0, CellsPerSide - 1) * CellsPerSide + Mathf.Clamp(x, 0, CellsPerSide - 1)];

    public WorldEnvironmentSample SampleNearest(float x, float z) => SampleGrid(
        Mathf.RoundToInt((x - GridMin) / CellSize), Mathf.RoundToInt((z - GridMin) / CellSize));

    /// <summary>Project onto a specific 3D surface, then evaluate placement fields there.
    /// Use this for cave floors, walls and ceilings, where X/Z alone is ambiguous.</summary>
    public WorldProjectedSample SampleProjected(Vector3 origin, Vector3 direction, float maxDistance)
    {
        var hit = RaycastSurface(origin, direction, maxDistance);
        if (!float.IsFinite(hit.Distance))
            throw new InvalidOperationException("No generated surface intersects the placement ray");
        return new WorldProjectedSample(hit, FieldsAt(hit, SurfaceConcavity(hit)));
    }

    /// <summary>Exact ray projection on arbitrary generated cave, wall or overhang triangles.
    /// Open seabed uses the analytic height field only for downward projection.</summary>
    public WorldSurfaceHit RaycastSurface(Vector3 origin, Vector3 direction, float maxDistance)
    {
        if (direction.LengthSquared() < .0001f || maxDistance <= 0f)
            throw new ArgumentOutOfRangeException(nameof(direction));
        direction = direction.Normalized();
        var best = maxDistance;
        var found = new WorldSurfaceHit(default, default, "", float.PositiveInfinity);
        IEnumerable<int> candidates;
        if (Mathf.Abs(direction.X) < .00001f && Mathf.Abs(direction.Z) < .00001f)
        {
            var x = Mathf.Clamp(Mathf.FloorToInt((origin.X - GridMin) / CellSize), 0, CellsPerSide - 1);
            var z = Mathf.Clamp(Mathf.FloorToInt((origin.Z - GridMin) / CellSize), 0, CellsPerSide - 1);
            candidates = _verticalBuckets.TryGetValue(z * CellsPerSide + x, out var bucket)
                ? bucket : Array.Empty<int>();
        }
        else candidates = AllTriangleIndices();
        foreach (var index in candidates)
        {
            var triangle = _triangles[index];
            if (!RayTriangle(origin, direction, triangle, out var distance) || distance >= best) continue;
            best = distance;
            found = new WorldSurfaceHit(origin + direction * distance, triangle.Normal,
                triangle.Id, distance);
        }
        if (direction.Dot(Vector3.Down) > .999f)
        {
            // The continuous open seabed has one height at X/Z. Bisection is only used there.
            var floorY = _basin.HeightAt(origin.X, origin.Z);
            var distance = origin.Y - floorY;
            if (distance >= 0f && distance < best)
                found = new WorldSurfaceHit(new Vector3(origin.X, floorY, origin.Z),
                    _basin.NormalAt(origin.X, origin.Z), "open-seabed", distance);
        }
        return found;
    }

    public WorldSurfaceHit ProjectDown(Vector3 origin) => RaycastSurface(origin, Vector3.Down, 100f);

    private IEnumerable<int> AllTriangleIndices()
    {
        for (var i = 0; i < _triangles.Count; i++) yield return i;
    }

    private static bool RayTriangle(Vector3 origin, Vector3 direction, Triangle triangle, out float distance)
    {
        var e1 = triangle.B - triangle.A;
        var e2 = triangle.C - triangle.A;
        var p = direction.Cross(e2);
        var determinant = e1.Dot(p);
        if (Mathf.Abs(determinant) < .000001f) { distance = 0; return false; }
        var inverse = 1f / determinant;
        var t = origin - triangle.A;
        var u = t.Dot(p) * inverse;
        var q = t.Cross(e1);
        var v = direction.Dot(q) * inverse;
        distance = e2.Dot(q) * inverse;
        return u >= -0.00001f && v >= -0.00001f && u + v <= 1.00001f && distance >= 0f;
    }

    private WorldEnvironmentSample Compute(int x, int z)
    {
        var position = new Vector3(GridMin + x * CellSize, 60f, GridMin + z * CellSize);
        var hit = ProjectDown(position);
        var h = _heights[z * CellsPerSide + x];
        var left = _heights[z * CellsPerSide + Mathf.Max(0, x - 2)];
        var right = _heights[z * CellsPerSide + Mathf.Min(CellsPerSide - 1, x + 2)];
        var near = _heights[Mathf.Max(0, z - 2) * CellsPerSide + x];
        var far = _heights[Mathf.Min(CellsPerSide - 1, z + 2) * CellsPerSide + x];
        var concavity = Mathf.Clamp(((left + right + near + far) * .25f - h) / 8f, -1f, 1f);
        return FieldsAt(hit, concavity);
    }

    private float SurfaceConcavity(WorldSurfaceHit hit)
    {
        var normal = hit.Normal;
        var tangent = normal.Cross(Mathf.Abs(normal.Y) < .9f ? Vector3.Up : Vector3.Right).Normalized();
        var bitangent = normal.Cross(tangent).Normalized();
        var total = 0f;
        var count = 0;
        foreach (var offset in new[] { tangent * 4f, -tangent * 4f, bitangent * 4f, -bitangent * 4f })
        {
            var origin = hit.Position + offset + normal * 3f;
            var nearby = RaycastSurface(origin, -normal, 8f);
            if (!float.IsFinite(nearby.Distance) || nearby.SurfaceId != hit.SurfaceId) continue;
            total += (nearby.Position - hit.Position).Dot(normal);
            count++;
        }
        return count == 0 ? 0f : Mathf.Clamp(total / count / 8f, -1f, 1f);
    }

    private WorldEnvironmentSample FieldsAt(WorldSurfaceHit hit, float concavity)
    {
        var surface = hit.Position;
        var radius = new Vector2(surface.X, surface.Z).Length();
        var angle = Mathf.Atan2(surface.Z, surface.X);
        var perimeter = Mathf.Max(0f, _basin.PerimeterRadius(angle) - radius);
        var core = new Vector2(surface.X, surface.Z).DistanceTo(_basin.DeepCore);
        var entrance = float.PositiveInfinity;
        foreach (var region in _regions)
        {
            var line = region.Centerline;
            entrance = Mathf.Min(entrance, Mathf.Min(surface.DistanceTo(line[0]), surface.DistanceTo(line[^1])));
        }
        var geology = NearestMeshDistance(surface, false);
        var cave = NearestMeshDistance(surface, true);
        var roof = RaycastSurface(surface + Vector3.Up * .15f, Vector3.Up, 10f);
        var cover = float.IsFinite(roof.Distance) ? 1f : 0f;
        var shelter = Mathf.Clamp(.55f * cover + .25f * Mathf.Max(0f, concavity) +
            .2f * (1f - Mathf.Clamp(geology / 12f, 0f, 1f)), 0f, 1f);
        var normal = hit.Normal;
        return new WorldEnvironmentSample(surface, normal,
            Mathf.Max(0f, WaterSurfaceY - surface.Y),
            Mathf.Acos(Mathf.Clamp(Mathf.Abs(normal.Y), 0f, 1f)), concavity,
            perimeter, core, _routes.DistanceToRoute(surface), cave, entrance,
            shelter, 1f - shelter, geology);
    }

    private float NearestMeshDistance(Vector3 point, bool cavesOnly)
    {
        const float cap = 12f;
        var centerX = Mathf.Clamp(Mathf.FloorToInt((point.X - GridMin) / CellSize), 0, CellsPerSide - 1);
        var centerZ = Mathf.Clamp(Mathf.FloorToInt((point.Z - GridMin) / CellSize), 0, CellsPerSide - 1);
        var checkedTriangles = new HashSet<int>();
        var best = cap * cap;
        for (var z = Mathf.Max(0, centerZ - 6); z <= Mathf.Min(CellsPerSide - 1, centerZ + 6); z++)
        for (var x = Mathf.Max(0, centerX - 6); x <= Mathf.Min(CellsPerSide - 1, centerX + 6); x++)
        {
            if (!_verticalBuckets.TryGetValue(z * CellsPerSide + x, out var bucket)) continue;
            foreach (var index in bucket)
            {
                if (!checkedTriangles.Add(index)) continue;
                var triangle = _triangles[index];
                if (cavesOnly && triangle.Kind is not ("cave" or "tunnel")) continue;
                var closestBox = new Vector3(Mathf.Clamp(point.X, triangle.Min.X, triangle.Max.X),
                    Mathf.Clamp(point.Y, triangle.Min.Y, triangle.Max.Y),
                    Mathf.Clamp(point.Z, triangle.Min.Z, triangle.Max.Z));
                if (point.DistanceSquaredTo(closestBox) >= best) continue;
                var distance = (point - triangle.A).Dot(triangle.Normal);
                var projected = point - distance * triangle.Normal;
                var inside = (triangle.B - triangle.A).Cross(projected - triangle.A).Dot(triangle.Normal) >= -.0001f &&
                    (triangle.C - triangle.B).Cross(projected - triangle.B).Dot(triangle.Normal) >= -.0001f &&
                    (triangle.A - triangle.C).Cross(projected - triangle.C).Dot(triangle.Normal) >= -.0001f;
                if (inside) best = Mathf.Min(best, distance * distance);
                else
                {
                    best = Mathf.Min(best, point.DistanceSquaredTo(Geometry3D.GetClosestPointToSegment(point, triangle.A, triangle.B)));
                    best = Mathf.Min(best, point.DistanceSquaredTo(Geometry3D.GetClosestPointToSegment(point, triangle.B, triangle.C)));
                    best = Mathf.Min(best, point.DistanceSquaredTo(Geometry3D.GetClosestPointToSegment(point, triangle.C, triangle.A)));
                }
            }
        }
        return Mathf.Sqrt(best);
    }
}
