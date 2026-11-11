#nullable enable
using System;
using Godot;

namespace Fish;

/// <summary>Seeded, cheap open-seabed query shared by mesh, collision and later placement stages.</summary>
public sealed class GeneratedBasin
{
    public const string AlgorithmVersion = "c3-basin-1";
    public const ulong DefaultSeed = 42;
    public const float HalfExtent = 48f;
    public const float CellSize = 1f;
    public const int CellsPerSide = 96;
    private readonly float[] _phase = new float[9];

    public ulong Seed { get; }
    public Vector2 DeepCore { get; }
    public float DeepFloorY { get; }
    public float MinPerimeterRadius { get; }
    public float MaxPerimeterRadius { get; }
    public float MeanPerimeterRadius { get; }

    public GeneratedBasin(ulong seed)
    {
        Seed = seed;
        var random = new WorldCompileContext(new WorldGenerationSettings(seed)).Stream("basin", AlgorithmVersion);
        for (var i = 0; i < _phase.Length; i++)
            _phase[i] = (float)((random.Next() >> 11) * (Math.Tau / 9007199254740992.0));

        var minimum = float.PositiveInfinity;
        var core = Vector2.Zero;
        // Metadata is computed from the same analytic field as the vertices, not from shelf markers.
        for (var z = -16; z <= 16; z++)
        for (var x = -16; x <= 16; x++)
        {
            var y = HeightAt(x * 0.5f, z * 0.5f);
            if (y >= minimum) continue;
            minimum = y;
            core = new Vector2(x * 0.5f, z * 0.5f);
        }
        DeepCore = core;
        DeepFloorY = minimum;
        var low = float.PositiveInfinity;
        var high = 0f;
        var sum = 0f;
        for (var i = 0; i < 360; i++)
        {
            var angle = i * Mathf.Tau / 360f;
            var radius = PerimeterRadius(angle);
            low = Mathf.Min(low, radius);
            high = Mathf.Max(high, radius);
            sum += radius;
        }
        MinPerimeterRadius = low;
        MaxPerimeterRadius = high;
        MeanPerimeterRadius = sum / 360f;
    }

    public float PerimeterRadius(float angle) => 35f
        + 2.35f * Mathf.Sin(3f * angle + _phase[0])
        + 1.55f * Mathf.Sin(5f * angle + _phase[1])
        + 0.85f * Mathf.Sin(7f * angle + _phase[2]);

    public float HeightAt(float x, float z)
    {
        // Smooth domain warp and angular radius prevent axis-aligned contours.
        var wx = x + 1.5f * Mathf.Sin(z * 0.095f + _phase[3])
            + 0.7f * Mathf.Sin((x + z) * 0.055f + _phase[4]);
        var wz = z + 1.4f * Mathf.Sin(x * 0.085f + _phase[5])
            + 0.6f * Mathf.Sin((z - x) * 0.07f + _phase[6]);
        var angle = Mathf.Atan2(wz, wx);
        var normalized = Mathf.Sqrt(wx * wx + wz * wz) / PerimeterRadius(angle);
        var bowl = -10.3f + 9.1f * Mathf.Pow(Mathf.Min(normalized, 1.3f), 1.8f);
        var bank = 44f * Smooth(0.65f, 1.15f, normalized);
        var fade = 1f - Smooth(0.7f, 1.05f, normalized);
        var folds = fade * (1.1f * Mathf.Sin(wx * 0.115f + wz * 0.075f + _phase[7])
            + 0.8f * Mathf.Sin(wx * 0.075f - wz * 0.14f + _phase[8])
            + 0.35f * Mathf.Sin(wx * 0.29f + wz * 0.23f + _phase[4]));
        // A broad offset channel and shoulder break the bowl without moving its useful minimum outside the core.
        var channel = -1.1f * Gaussian(x, z, 10f, -13f, 9f, 12f);
        var shoulder = 1.4f * Gaussian(x, z, -17f, 7f, 10f, 8f);
        return bowl + bank + folds + channel + shoulder;
    }

    public Vector3 NormalAt(float x, float z)
    {
        const float step = 0.2f;
        var dx = (HeightAt(x + step, z) - HeightAt(x - step, z)) / (2f * step);
        var dz = (HeightAt(x, z + step) - HeightAt(x, z - step)) / (2f * step);
        return new Vector3(-dx, 1f, -dz).Normalized();
    }

    private static float Gaussian(float x, float z, float cx, float cz, float rx, float rz)
    {
        var dx = (x - cx) / rx;
        var dz = (z - cz) / rz;
        return Mathf.Exp(-0.5f * (dx * dx + dz * dz));
    }

    private static float Smooth(float start, float end, float value)
    {
        var t = Mathf.Clamp((value - start) / (end - start), 0f, 1f);
        return t * t * (3f - 2f * t);
    }
}
