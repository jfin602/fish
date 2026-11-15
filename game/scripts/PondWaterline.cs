#nullable enable
using Godot;

namespace Fish;

/// <summary>CPU counterpart of water_surface.gdshader's vertex displacement.</summary>
public static class PondWaterline
{
    public static float Displacement(float x, float z, float time, float waveHeight,
        float waveSpeed, float waveDetail, float shoreBlend)
    {
        var t = time * waveSpeed;
        var a = x * .19f + z * .07f + t * .34f;
        var b = x * -.11f + z * .17f - t * .27f;
        var c = x * .31f - z * .24f + t * .47f;
        return shoreBlend * waveHeight * (.10f * Mathf.Sin(a) + .065f * Mathf.Sin(b) +
            waveDetail * .045f * Mathf.Sin(c));
    }

    // The mesh linearly interpolates vertex colors on a two-meter grid. This
    // analytic shore sample agrees in open water and converges at the bank;
    // between shoreline vertices its error is bounded by the wave amplitude.
    public static float ShoreBlend(GeneratedBasin basin, float x, float z) =>
        Mathf.Clamp(basin.DistanceToShore(x, z) / 1.6f, 0f, 1f);
}
