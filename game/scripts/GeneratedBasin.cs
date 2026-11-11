#nullable enable
using System;
using System.Collections.Generic;
using Godot;

namespace Fish;

public sealed record GeologySite(string Id, string Kind, Vector2 Center, Vector2 Axis,
    float Length, float HalfWidth, float Depth, float TerrainScore, float ShoulderRise);

/// <summary>Seeded shoreline polygon, signed distance field, and shore-derived open seabed.</summary>
public sealed class GeneratedBasin
{
    public const string AlgorithmVersion = "c3-shore-depth-3";
    public const string PondAlgorithmVersion = "pond-shore-depth-1";
    public const ulong DefaultSeed = 42;
    public const float HalfExtent = 184f;
    public const float CellSize = 2f;
    public const int CellsPerSide = 184;
    public const float WaterSurfaceY = 10.39f;
    private const int ShoreCount = 256;
    private const int FieldSide = CellsPerSide + 1;
    private readonly Vector2[] _shore = new Vector2[ShoreCount];
    private readonly float[] _distance = new float[FieldSide * FieldSide];
    private readonly float[] _phase = new float[9];
    private readonly bool _legacy;
    private readonly bool _m1Baked;
    public bool IsPond { get; }
    public string AlgorithmIdentity => IsPond ? PondAlgorithmVersion : AlgorithmVersion;
    private readonly List<GeologySite> _geologySites = new();
    public IReadOnlyList<GeologySite> GeologySites => _geologySites;

    public ulong Seed { get; }
    public Vector2 DeepCore { get; }
    public float DeepFloorY { get; }
    public float MinPerimeterRadius { get; }
    public float MaxPerimeterRadius { get; }
    public float MeanPerimeterRadius { get; }
    public Vector2 ShoreMin { get; }
    public Vector2 ShoreMax { get; }
    public float MaximumInsideDistance { get; }
    public int ShorePointCount => ShoreCount;

    public GeneratedBasin(ulong seed, bool legacy = false, bool m1Baked = false, bool pond = false)
    {
        Seed = seed;
        _legacy = legacy;
        _m1Baked = m1Baked;
        IsPond = pond;
        var random = new WorldCompileContext(pond
            ? new WorldGenerationSettings(seed, 2, "pond-full-1")
            : new WorldGenerationSettings(seed)).Stream("basin",
            legacy ? "c3-basin-1" : AlgorithmIdentity);
        for (var i = 0; i < _phase.Length; i++)
            _phase[i] = (float)((random.Next() >> 11) * (Math.Tau / 9007199254740992.0));
        if (legacy)
        {
            var low = float.PositiveInfinity; var high = 0f; var sum = 0f;
            for (var i = 0; i < ShoreCount; i++)
            {
                var radius = LegacyRadius(i * Mathf.Tau / ShoreCount);
                low = Mathf.Min(low, radius); high = Mathf.Max(high, radius); sum += radius;
            }
            MinPerimeterRadius = low; MaxPerimeterRadius = high;
            MeanPerimeterRadius = sum / ShoreCount;
            ShoreMin = new(-high, -high); ShoreMax = new(high, high);
            var minY = float.PositiveInfinity; var at = Vector2.Zero;
            for (var z = -16; z <= 16; z++) for (var x = -16; x <= 16; x++)
            {
                var p = new Vector2(x * .5f, z * .5f);
                var y = LegacyHeight(p.X, p.Y);
                if (y >= minY) continue;
                minY = y; at = p;
            }
            DeepCore = at; DeepFloorY = minY;
            return;
        }

        // Construct the water domain before evaluating any elevations. Unequal
        // lobes and coves at seeded bearings avoid a regular angular starburst.
        var axis = pond ? 1.17f + .08f * Mathf.Sin(_phase[0]) : 1.04f + .11f * Mathf.Sin(_phase[0]);
        var center = pond ? new Vector2(3f * Mathf.Sin(_phase[1]), 4f * Mathf.Sin(_phase[2]))
            : new Vector2(7f * Mathf.Sin(_phase[1]), 6f * Mathf.Sin(_phase[2]));
        var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        var minR = float.PositiveInfinity; var maxR = 0f; var sumR = 0f;
        for (var i = 0; i < ShoreCount; i++)
        {
            var a = i * Mathf.Tau / ShoreCount;
            var radius = (pond ? 54f : 126f) + (pond ? 5f : 8f) * Mathf.Sin(a + _phase[0]);
            var rotation = _phase[3];
            var scale = pond ? .43f : 1f;
            radius += scale * 37f * Bump(a, rotation, .47f);
            radius += scale * 27f * Bump(a, rotation + 1.62f + .18f * Mathf.Sin(_phase[4]), .64f);
            radius += scale * 35f * Bump(a, rotation + 4.02f + .21f * Mathf.Sin(_phase[5]), .53f);
            radius -= scale * 38f * Bump(a, rotation + .88f + .13f * Mathf.Sin(_phase[6]), .37f);
            radius -= scale * 30f * Bump(a, rotation + 2.92f + .16f * Mathf.Sin(_phase[7]), .48f);
            radius -= scale * 41f * Bump(a, rotation + 5.34f + .12f * Mathf.Sin(_phase[8]), .40f);
            radius = pond ? Mathf.Clamp(radius, 32f, 73f) : Mathf.Clamp(radius, 78f, 165f);
            var p = center + new Vector2(Mathf.Cos(a) * radius * axis,
                Mathf.Sin(a) * radius / axis);
            _shore[i] = p;
            min = min.Min(p); max = max.Max(p);
            var r = p.Length();
            minR = Mathf.Min(minR, r); maxR = Mathf.Max(maxR, r); sumR += r;
        }
        ShoreMin = min; ShoreMax = max;
        MinPerimeterRadius = minR; MaxPerimeterRadius = maxR;
        MeanPerimeterRadius = sumR / ShoreCount;

        // Once per seed, cache signed distance on the mesh lattice. Height,
        // normals and analysis sample this same field with bilinear interpolation.
        var maxDistance = 0f;
        for (var z = 0; z < FieldSide; z++) for (var x = 0; x < FieldSide; x++)
        {
            var p = new Vector2(-HalfExtent + x * CellSize, -HalfExtent + z * CellSize);
            var d = ExactSignedDistance(p);
            _distance[z * FieldSide + x] = d;
            maxDistance = Mathf.Max(maxDistance, d);
        }
        MaximumInsideDistance = maxDistance;
        if (!m1Baked && !pond) SelectGeologySites();
        var deepest = float.PositiveInfinity; var deepAt = Vector2.Zero;
        for (var z = 12; z < FieldSide - 12; z++) for (var x = 12; x < FieldSide - 12; x++)
        {
            if (_distance[z * FieldSide + x] < 12f) continue;
            var p = new Vector2(-HalfExtent + x * CellSize, -HalfExtent + z * CellSize);
            var y = HeightAt(p.X, p.Y);
            if (y >= deepest) continue;
            deepest = y; deepAt = p;
        }
        DeepCore = deepAt; DeepFloorY = deepest;
    }

    public Vector2 ShorePoint(int index) => _shore[(index % ShoreCount + ShoreCount) % ShoreCount];
    public float PerimeterRadius(float angle) => _legacy ? LegacyRadius(angle) :
        ShorePoint(Mathf.PosMod(Mathf.RoundToInt(angle * ShoreCount / Mathf.Tau), ShoreCount)).Length();

    public float DistanceToShore(float x, float z)
    {
        if (_legacy) return LegacyRadius(Mathf.Atan2(z, x)) - Mathf.Sqrt(x * x + z * z);
        var gx = (x + HalfExtent) / CellSize; var gz = (z + HalfExtent) / CellSize;
        if (gx < 0f || gz < 0f || gx >= CellsPerSide || gz >= CellsPerSide)
            return ExactSignedDistance(new Vector2(x, z));
        var ix = Mathf.FloorToInt(gx); var iz = Mathf.FloorToInt(gz);
        var tx = gx - ix; var tz = gz - iz;
        var a = Mathf.Lerp(_distance[iz * FieldSide + ix], _distance[iz * FieldSide + ix + 1], tx);
        var b = Mathf.Lerp(_distance[(iz + 1) * FieldSide + ix],
            _distance[(iz + 1) * FieldSide + ix + 1], tx);
        return Mathf.Lerp(a, b, tz);
    }

    public float HeightAt(float x, float z)
    {
        if (_legacy) return LegacyHeight(x, z);
        var height = BaseHeightAt(x, z);
        if (_m1Baked) return height;
        foreach (var site in _geologySites)
        {
            var delta = new Vector2(x, z) - site.Center;
            var along = delta.Dot(site.Axis);
            var across = Mathf.Abs(delta.Cross(site.Axis));
            if (Mathf.Abs(along) >= site.Length * .5f + 7f ||
                across >= site.HalfWidth + 22f) continue;
            // Two continuous shoulders merge into the existing seabed. The middle
            // stays open for the shell floor and the fish/camera corridor.
            var longitudinal = Smooth(site.Length * .5f + 7f, site.Length * .5f - 4f,
                Mathf.Abs(along));
            var lateral = Mathf.Exp(-.5f * Mathf.Pow((across - site.HalfWidth - 3f) / 7.0f, 2f));
            height += site.ShoulderRise * lateral * longitudinal * Smooth(1f, 6f, across);
        }
        return height;
    }

    private float BaseHeightAt(float x, float z)
    {
        var shore = DistanceToShore(x, z);
        if (IsPond)
        {
            if (shore <= 0f)
                return WaterSurfaceY + 2.8f * Smooth(0f, 17f, -shore) +
                    2.2f * Smooth(15f, 47f, -shore);
            var pondDepth = .15f + 1.2f * Smooth(0f, 5f, shore) +
                2.1f * Smooth(4f, 17f, shore) +
                3.4f * Smooth(14f, 34f, shore) +
                4.3f * Smooth(28f, 53f, shore);
            var ripple = Smooth(3f, 14f, shore) *
                (.38f * Mathf.Sin(x * .14f + _phase[0]) * Mathf.Sin(z * .11f + _phase[1]) +
                 .55f * Mathf.Sin(x * .052f - z * .071f + _phase[2]));
            var core = 2.8f * Gaussian(x, z, 6f * Mathf.Sin(_phase[5]),
                7f * Mathf.Sin(_phase[6]), 18f, 21f);
            return WaterSurfaceY - Mathf.Clamp(pondDepth + ripple + core, .15f, 15f);
        }
        if (shore <= 0f) return WaterSurfaceY - .7f + 11f * Smooth(0f, 21f, -shore);
        var depth = .7f + 1.3f * Smooth(0f, 6f, shore)
            + 4f * Smooth(4f, 24f, shore)
            + 9f * Smooth(20f, 52f, shore)
            + 12f * Smooth(48f, 92f, shore);
        var fade = Smooth(7f, 25f, shore);
        var folds = 1.5f * Mathf.Sin(x * .038f + z * .021f + _phase[0])
            + 1.2f * Mathf.Sin(x * -.022f + z * .045f + _phase[1]);
        var ridge = -3.2f * Gaussian(x, z, -58f, 24f, 34f, 15f);
        var shelf = -2f * Gaussian(x, z, 43f, 54f, 32f, 25f);
        var channel = 2.8f * Gaussian(x, z, 8f, -41f, 18f, 58f);
        var pocket = 12.5f * Gaussian(x, z, 25f * Mathf.Sin(_phase[3]),
            27f * Mathf.Sin(_phase[4]), 27f, 23f);
        depth = Mathf.Clamp(depth + fade * (folds + ridge + shelf + channel + pocket), .7f, 44f);
        return WaterSurfaceY - depth;
    }

    private void SelectGeologySites()
    {
        var requests = new (string id, string kind, float target, float width, float length)[]
        {
            ("west-swim-through", "tunnel", 13f, 3.5f, 25f),
            ("east-swim-through", "tunnel", 18f, 2.9f, 23f),
            ("cave-chamber", "cave", 17f, 4.1f, 24f),
            ("stone-arch", "arch", 10f, 4.6f, 12f),
            ("canyon-ridge", "canyon", 17f, 3.2f, 25f),
        };
        var random = new WorldCompileContext(new WorldGenerationSettings(Seed))
            .Stream("geology-sites", "terrain-1");
        foreach (var request in requests)
        {
            GeologySite? best = null;
            var bestScore = float.NegativeInfinity;
            for (var z = -112; z <= 112; z += 8)
            for (var x = -112; x <= 112; x += 8)
            {
                var p = new Vector2(x, z);
                var shore = DistanceToShore(x, z);
                if (shore < 26f || p.Length() > 76f ||
                    _geologySites.Exists(s => s.Center.DistanceTo(p) < 31f)) continue;
                var depth = WaterSurfaceY - BaseHeightAt(x, z);
                if (depth < 8f || depth > 24f) continue;
                var dx = BaseHeightAt(x + 6f, z) - BaseHeightAt(x - 6f, z);
                var dz = BaseHeightAt(x, z + 6f) - BaseHeightAt(x, z - 6f);
                var gradient = new Vector2(dx, dz);
                var axis = gradient.Length() > .4f ? gradient.Normalized() :
                    new Vector2(Mathf.Cos(_phase[0]), Mathf.Sin(_phase[0]));
                // A slight seeded rotation prevents a row of parallel passages.
                var angle = ((random.Next() % 1001) / 1000f - .5f) * .5f;
                axis = axis.Rotated(angle);
                var end = axis * (request.length * .5f + 5f);
                var side = new Vector2(-axis.Y, axis.X);
                var edgeRoom = Mathf.Min(DistanceToShore((p + end).X, (p + end).Y),
                    DistanceToShore((p - end).X, (p - end).Y));
                var left = WaterSurfaceY - BaseHeightAt((p + side * 10f).X, (p + side * 10f).Y);
                var right = WaterSurfaceY - BaseHeightAt((p - side * 10f).X, (p - side * 10f).Y);
                var terrainMass = depth - (left + right) * .5f;
                if (edgeRoom < 17f) continue;
                var score = 2f * Mathf.Clamp(gradient.Length() / 3f, 0f, 1f) +
                    Mathf.Clamp(terrainMass / 2.5f, -1f, 1f) +
                    Mathf.Clamp(edgeRoom / 35f, 0f, 1f) -
                    Mathf.Abs(depth - request.target) / 6f +
                    ((random.Next() % 1000) / 1000f) * .4f;
                // Preserve M2's broad separation from the central depth landmarks.
                if (p.Length() < 35f) score -= 2f;
                if (score <= bestScore) continue;
                bestScore = score;
                best = new GeologySite(request.id, request.kind, p, axis,
                    request.length, request.width, depth, score,
                    terrainMass >= 2.5f ? 4.1f : 4.7f);
            }
            if (best is null) throw new InvalidOperationException("No water-domain geology site: " + request.id);
            _geologySites.Add(best);
        }
    }

    public Vector3 NormalAt(float x, float z)
    {
        const float step = .5f;
        var dx = (HeightAt(x + step, z) - HeightAt(x - step, z)) / (2f * step);
        var dz = (HeightAt(x, z + step) - HeightAt(x, z - step)) / (2f * step);
        return new Vector3(-dx, 1f, -dz).Normalized();
    }

    private float ExactSignedDistance(Vector2 point)
    {
        var nearest = float.PositiveInfinity; var inside = false;
        for (var i = 0; i < ShoreCount; i++)
        {
            var a = _shore[i]; var b = _shore[(i + 1) % ShoreCount];
            var edge = b - a;
            var t = Mathf.Clamp((point - a).Dot(edge) / edge.LengthSquared(), 0f, 1f);
            nearest = Mathf.Min(nearest, point.DistanceTo(a + t * edge));
            if ((a.Y > point.Y) != (b.Y > point.Y) &&
                point.X < (b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y) + a.X)
                inside = !inside;
        }
        return inside ? nearest : -nearest;
    }

    private static float Bump(float a, float center, float width)
    {
        var delta = Mathf.Atan2(Mathf.Sin(a - center), Mathf.Cos(a - center)) / width;
        return Mathf.Exp(-.5f * delta * delta);
    }
    private static float Gaussian(float x, float z, float cx, float cz, float rx, float rz)
    {
        var dx = (x - cx) / rx; var dz = (z - cz) / rz;
        return Mathf.Exp(-.5f * (dx * dx + dz * dz));
    }
    private static float Smooth(float start, float end, float value)
    {
        var t = Mathf.Clamp((value - start) / (end - start), 0f, 1f);
        return t * t * (3f - 2f * t);
    }

    // Historical baked worlds retain their original analytical terrain identity.
    private float LegacyRadius(float a) => 35f + 2.35f * Mathf.Sin(3f * a + _phase[0])
        + 1.55f * Mathf.Sin(5f * a + _phase[1]) + .85f * Mathf.Sin(7f * a + _phase[2]);
    private float LegacyHeight(float x, float z)
    {
        var wx = x + 1.5f * Mathf.Sin(z * .095f + _phase[3]) + .7f * Mathf.Sin((x + z) * .055f + _phase[4]);
        var wz = z + 1.4f * Mathf.Sin(x * .085f + _phase[5]) + .6f * Mathf.Sin((z - x) * .07f + _phase[6]);
        var normalized = Mathf.Sqrt(wx * wx + wz * wz) / LegacyRadius(Mathf.Atan2(wz, wx));
        var bowl = -10.3f + 9.1f * Mathf.Pow(Mathf.Min(normalized, 1.3f), 1.8f);
        var bank = 44f * Smooth(.65f, 1.15f, normalized);
        var fade = 1f - Smooth(.7f, 1.05f, normalized);
        var folds = fade * (1.1f * Mathf.Sin(wx * .115f + wz * .075f + _phase[7])
            + .8f * Mathf.Sin(wx * .075f - wz * .14f + _phase[8])
            + .35f * Mathf.Sin(wx * .29f + wz * .23f + _phase[4]));
        return bowl + bank + folds - 1.1f * Gaussian(x, z, 10f, -13f, 9f, 12f)
            + 1.4f * Gaussian(x, z, -17f, 7f, 10f, 8f);
    }
}
