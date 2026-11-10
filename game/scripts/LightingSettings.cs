#nullable enable
using System;

namespace Fish;

/// <summary>Bounded presentation values only; no simulation or camera state.</summary>
public sealed class LightingSettings
{
    public float SurfaceWaveHeight { get; set; } = 1f;
    public float SurfaceWaveSpeed { get; set; } = 1f;
    public float SurfaceWaveDetail { get; set; } = 0.8f;
    public float CausticEnergy { get; set; } = 0.32f;
    public float CausticDrift { get; set; } = 0.22f;
    public float CausticTravel { get; set; } = 0.8f;
    public int ShaftCount { get; set; } = 6;
    public float ShaftBrightness { get; set; } = 1f;
    public float SunEnergy { get; set; } = 1.25f;
    public float AmbientEnergy { get; set; } = 0.42f;
    public float VolumetricDensity { get; set; } = 0.024f;
    public float DistanceFogDensity { get; set; } = 0.0035f;
    public int MoteCount { get; set; } = 128;
    public float ScreenWarpPixels { get; set; } = 0.10f;

    public LightingSettings Copy() => (LightingSettings)MemberwiseClone();

    public bool IsValid() =>
        Bounded(SurfaceWaveHeight, 0.25f, 1.2f) && Bounded(SurfaceWaveSpeed, 0.25f, 2f) &&
        Bounded(SurfaceWaveDetail, 0f, 1f) && Bounded(CausticEnergy, 0f, 0.8f) &&
        Bounded(CausticDrift, 0f, 0.5f) && Bounded(CausticTravel, 0f, 1.5f) &&
        ShaftCount is >= 0 and <= 8 && Bounded(ShaftBrightness, 0f, 2f) &&
        Bounded(SunEnergy, 0f, 3f) && Bounded(AmbientEnergy, 0f, 1f) &&
        Bounded(VolumetricDensity, 0.001f, 0.06f) && Bounded(DistanceFogDensity, 0f, 0.02f) &&
        MoteCount is >= 0 and <= 160 && Bounded(ScreenWarpPixels, 0f, 0.2f);

    private static bool Bounded(float value, float min, float max) =>
        float.IsFinite(value) && value >= min && value <= max;
}
