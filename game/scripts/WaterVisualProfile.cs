#nullable enable
using Godot;

namespace Fish;

/// <summary>Scene-local water palette and depth response. Lighting presets only tune energy/effect amounts.</summary>
public sealed record WaterVisualProfile(
    WorldEnvironmentProfile Id, Color Background, Color Ambient, Color Fog, Color Volume,
    Color Sun, Color Caustic, Color Motes, Color DeepBackground, Color DeepAmbient,
    Color DeepFog, Color DeepVolume, Color SkyHorizon, Color SkyZenith,
    Color DeepSkyHorizon, Color DeepSkyZenith, Color SurfaceDark, Color SurfaceLight,
    Color ShaftColor, float DeepRangeFactor, float DeepLightFactor,
    float DeepAmbientFactor, float DeepVolumeFactor, float DeepFogLengthFactor)
{
    // Air and contact treatment are scene-local; graphics presets still own shared lighting knobs.
    public float AirFogBegin { get; init; }
    public float AirFogEnd { get; init; } = 100f;
    public float AirFogDensity { get; init; }
    // Air-side surface transmission only. Underwater fog remains independently tuned.
    public float SurfaceExtinctionPerMeter { get; init; }
    public Color SurfaceScatter { get; init; } = new(0.10f, 0.37f, 0.42f);
    public Color UnderwaterSurfaceScatter { get; init; } = new(0.10f, 0.37f, 0.42f);
    public float SurfaceReflectionStrength { get; init; }
    public float SurfaceReflectionDistortion { get; init; } = 1f;
    public float SurfaceRippleStrength { get; init; } = 1f;
    public float SurfaceFineNormalStrength { get; init; } = 1f;
    public float SurfaceRippleInterval { get; init; } = .18f;
    public float SurfaceWakeStrength { get; init; }
    // Illumination palette only; optical extinction and local turbidity are unchanged at night.
    public Color NightAmbient { get; init; } = new(0.45f, 0.56f, 0.66f);
    public Color NightSurfaceGlow { get; init; } = new(0.012f, 0.025f, 0.038f);
    public Color NightFog { get; init; } = new(0.040f, 0.085f, 0.125f);
    public Color NightVolume { get; init; } = new(0.08f, 0.12f, 0.18f);
    public float NightSaturation { get; init; } = 0.58f;
    public static WaterVisualProfile Ocean { get; } = new(
        WorldEnvironmentProfile.Ocean,
        new(0.035f, 0.17f, 0.22f), new(0.56f, 0.73f, 0.76f),
        new(0.10f, 0.37f, 0.42f), new(0.40f, 0.72f, 0.76f),
        new(0.78f, 0.92f, 0.90f), new(0.68f, 0.88f, 0.78f),
        new(0.73f, 0.91f, 0.82f, 0.33f),
        new(0.025f, 0.095f, 0.18f), new(0.32f, 0.52f, 0.65f),
        new(0.055f, 0.18f, 0.30f), new(0.22f, 0.43f, 0.59f),
        new(0.06f, 0.28f, 0.34f), new(0.39f, 0.77f, 0.87f),
        new(0.025f, 0.095f, 0.18f), new(0.27f, 0.57f, 0.75f),
        new(0.08f, 0.42f, 0.49f), new(0.20f, 0.66f, 0.67f),
        new(0.54f, 0.79f, 0.72f), 4f / 7f, 0.35f, 0.48f, 2.25f, 0.68f)
    {
        SurfaceReflectionStrength = 0.55f,
        NightFog = new(0.006f, 0.012f, 0.020f),
    };

    public static WaterVisualProfile Pond { get; } = new(
        WorldEnvironmentProfile.Pond,
        new(0.035f, 0.10f, 0.09f), new(0.82f, 0.82f, 0.65f),
        new(0.105f, 0.245f, 0.205f), new(0.27f, 0.40f, 0.30f),
        new(0.78f, 0.72f, 0.43f), new(0.76f, 0.71f, 0.34f),
        new(0.66f, 0.59f, 0.31f, 0.25f),
        new(0.025f, 0.06f, 0.055f), new(0.24f, 0.30f, 0.18f),
        new(0.055f, 0.115f, 0.105f), new(0.15f, 0.22f, 0.16f),
        new(0.095f, 0.26f, 0.235f), new(0.22f, 0.43f, 0.385f),
        new(0.045f, 0.11f, 0.105f), new(0.10f, 0.20f, 0.18f),
        new(0.13f, 0.28f, 0.12f), new(0.31f, 0.43f, 0.20f),
        new(0.72f, 0.70f, 0.38f), 0.68f, 0.42f, 0.55f, 1.65f, 0.78f)
    {
        AirFogBegin = 27f, AirFogEnd = 105f, AirFogDensity = .94f,
        NightAmbient = new(0.68f, 0.70f, 0.54f),
        NightSurfaceGlow = new(0.003f, 0.005f, 0.003f),
        NightFog = new(0.025f, 0.040f, 0.022f),
        NightVolume = new(0.12f, 0.15f, 0.09f),
        NightSaturation = 0.78f,
        SurfaceExtinctionPerMeter = .65f,
        SurfaceScatter = new(0.055f, 0.085f, 0.035f),
        UnderwaterSurfaceScatter = new(0.085f, 0.16f, 0.14f),
        SurfaceReflectionStrength = .95f,
        SurfaceReflectionDistortion = 1f,
        SurfaceRippleStrength = 1f,
        SurfaceFineNormalStrength = 2f,
        SurfaceRippleInterval = .19f, SurfaceWakeStrength = .72f,
    };

    public static WaterVisualProfile For(WorldEnvironmentProfile id) =>
        id == WorldEnvironmentProfile.Pond ? Pond : Ocean;
}
