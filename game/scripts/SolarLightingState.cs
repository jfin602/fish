#nullable enable
using Godot;

namespace Fish;

/// <summary>Presentation values derived from the shared solar state and the selected habitat.</summary>
public readonly record struct SolarLightingState(Color AirHorizon, Color AirZenith,
    Color AirAmbient, Color DirectColor, Color WarmHorizon, float WarmStrength,
    float SunGlow, float AirAmbientEnergy, float DirectEnergyFactor, float VolumetricFactor)
{
    // A single restrained night direction reuses the sun light; no second light or shadow map.
    public static float MoonVisibility(SolarState solar) => Mathf.SmoothStep(0f, 1f,
        Mathf.Clamp((-solar.DirectionToSun.Y - 0.02f) / 0.30f, 0f, 1f));

    public static SolarLightingState From(SolarState solar, WorldEnvironmentProfile habitat)
    {
        var pond = habitat == WorldEnvironmentProfile.Pond;
        var nightHorizon = new Color(0.0025f, 0.0040f, 0.0065f);
        var nightZenith = new Color(0.0015f, 0.0030f, 0.0070f);
        var dayHorizon = pond ? new Color(0.38f, 0.56f, 0.61f) :
            new Color(0.45f, 0.70f, 0.84f);
        var dayZenith = pond ? new Color(0.17f, 0.42f, 0.64f) :
            new Color(0.12f, 0.45f, 0.81f);
        var gradualDay = Mathf.Pow(solar.Daylight, 1.5f);
        var skyBlend = Mathf.Clamp(gradualDay + 0.20f * solar.Twilight, 0f, 1f);
        var zenithBlend = Mathf.Clamp(gradualDay + 0.08f * solar.Twilight, 0f, 1f);
        var lowSun = 1f - Smooth(0.12f, 0.62f, Mathf.Max(0f, solar.DirectionToSun.Y));
        var warm = Mathf.Clamp(0.72f * solar.Twilight +
            0.60f * solar.Daylight * lowSun, 0f, 1f);
        var daySun = pond ? new Color(0.94f, 0.94f, 0.86f) :
            new Color(0.97f, 0.98f, 1.00f);
        var direct = daySun.Lerp(new Color(1f, 0.49f, 0.25f), warm);
        var nightAmbient = new Color(0.10f, 0.12f, 0.15f);
        var dayAmbient = pond ? new Color(0.75f, 0.77f, 0.70f) :
            new Color(0.73f, 0.84f, 0.94f);
        return new SolarLightingState(
            nightHorizon.Lerp(dayHorizon, skyBlend),
            nightZenith.Lerp(dayZenith, zenithBlend),
            nightAmbient.Lerp(dayAmbient, skyBlend), direct,
            new Color(1f, 0.39f, 0.16f), warm,
            Mathf.Clamp(0.80f * solar.Daylight + 0.45f * solar.Twilight, 0f, 1f),
            0.05f + 0.70f * solar.AmbientLight + 0.08f * solar.Twilight,
            solar.DirectLight,
            Mathf.Sqrt(solar.DirectLight));
    }

    private static float Smooth(float start, float end, float value)
    {
        var x = Mathf.Clamp((value - start) / (end - start), 0f, 1f);
        return x * x * (3f - 2f * x);
    }
}
