#nullable enable
using Godot;

namespace Fish;

/// <summary>Water presentation derived from the shared sun and a scene-local optical profile.</summary>
public readonly record struct SolarWaterState(
    float SurfaceDaylight, float SurfaceIllumination, float UnderwaterAmbient,
    float UnderwaterSky, float ShaftEnergy, float CausticEnergy,
    float ReflectionSunEnergy, float ShaftLean, Color CausticTint)
{
    public static SolarWaterState From(SolarState solar, SolarLightingState lighting,
        WaterVisualProfile profile, float presetSunEnergy)
    {
        var pond = profile.Id == WorldEnvironmentProfile.Pond;
        var daylight = Mathf.Sqrt(solar.Daylight);
        var lowSun = Mathf.Clamp(lighting.WarmStrength, 0f, 1f);
        var surface = Mathf.Clamp(presetSunEnergy / 1.35f *
            ((pond ? 0.78f : 0.86f) + (pond ? 0.22f : 0.14f) * daylight), 0.6f, 1.3f);
        // Keep a low local ambient floor so close geometry remains navigable when
        // direct sunlight is gone. Distance information is reduced by the dark
        // fog-light palette, never by changing optical density.
        var ambient = Mathf.Min(1f, (pond ? 0.52f : 0.18f) +
            (pond ? 0.48f : 0.82f) * solar.AmbientLight + 0.08f * solar.Twilight);
        var direct = Mathf.Pow(solar.DirectLight, 0.65f);
        var lean = Mathf.Clamp(solar.DirectionToSun.Y, 0f, 1f);
        return new SolarWaterState(surface,
            (pond ? 0.12f : 0.07f) + (pond ? 0.88f : 0.93f) *
                (0.85f * solar.Daylight + 0.15f * solar.Twilight),
            ambient, (pond ? 0.15f : 0.06f) + (pond ? 0.85f : 0.94f) *
                (0.90f * solar.Daylight + 0.10f * solar.Twilight),
            direct, direct * (pond ? 0.90f : 1f),
            solar.Daylight * (0.55f + 0.45f * lean) + solar.Twilight * 0.12f +
                0.035f * SolarLightingState.MoonVisibility(solar),
            Mathf.Clamp((lean - 0.08f) / 0.32f, 0f, 1f),
            Colors.White.Lerp(new Color(1f, 0.70f, 0.42f), 0.55f * lowSun));
    }
}
