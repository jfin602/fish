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
        new(0.54f, 0.79f, 0.72f), 4f / 7f, 0.35f, 0.48f, 2.25f, 0.68f);

    public static WaterVisualProfile Pond { get; } = new(
        WorldEnvironmentProfile.Pond,
        new(0.065f, 0.115f, 0.052f), new(0.43f, 0.46f, 0.28f),
        new(0.22f, 0.28f, 0.105f), new(0.32f, 0.38f, 0.15f),
        new(0.78f, 0.72f, 0.43f), new(0.76f, 0.71f, 0.34f),
        new(0.66f, 0.59f, 0.31f, 0.25f),
        new(0.035f, 0.075f, 0.040f), new(0.24f, 0.30f, 0.18f),
        new(0.105f, 0.17f, 0.065f), new(0.20f, 0.27f, 0.11f),
        new(0.15f, 0.22f, 0.095f), new(0.40f, 0.46f, 0.22f),
        new(0.055f, 0.12f, 0.060f), new(0.20f, 0.29f, 0.14f),
        new(0.13f, 0.28f, 0.12f), new(0.31f, 0.43f, 0.20f),
        new(0.72f, 0.70f, 0.38f), 0.68f, 0.42f, 0.55f, 1.65f, 0.78f);

    public static WaterVisualProfile For(WorldEnvironmentProfile id) =>
        id == WorldEnvironmentProfile.Pond ? Pond : Ocean;
}
