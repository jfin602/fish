#if DEBUG
#nullable enable
using Godot;
using System;

namespace Fish;

public static class SolarLightingChecks
{
    public static void Run(Main main)
    {
        var clock = main.GetNode<EnvironmentClock>("EnvironmentClock");
        var atmosphere = main.GetNode<UnderwaterAtmosphere>("UnderwaterAtmosphere");
        var env = atmosphere.WaterEnvironment ?? throw new Exception("world environment missing");
        var sky = env.Sky ?? throw new Exception("sky resource missing");
        var material = sky.SkyMaterial as ShaderMaterial ?? throw new Exception("sky material missing");
        var sun = main.GetNode<DirectionalLight3D>("Sun");
        var surface = main.GetNode<MeshInstance3D>("UnderwaterAtmosphere/WaterEffects/OverheadSurface");
        var surfaceMaterial = surface.MaterialOverride as ShaderMaterial ?? throw new Exception("surface material missing");
        var shafts = main.GetNode<MeshInstance3D>("UnderwaterAtmosphere/WaterEffects/SunShafts");
        var shaftMaterial = shafts.MaterialOverride as ShaderMaterial ?? throw new Exception("shaft material missing");
        var caustic = main.GetNode<Decal>("UnderwaterAtmosphere/WaterEffects/CausticProjection1");
        var camera = main.GetNode<Camera3D>("Fish/CameraRig/Camera3D");
        var fish = main.GetNode<FishController>("Fish");
        var fishPosition = fish.GlobalPosition;
        var fishVelocity = fish.Velocity;
        var cameraTransform = camera.GlobalTransform;
        var skyId = sky.GetInstanceId();
        var materialId = material.GetInstanceId();
        var environmentId = env.GetInstanceId();
        var surfaceId = surfaceMaterial.GetInstanceId();
        var shaftId = shaftMaterial.GetInstanceId();
        var causticId = caustic.GetInstanceId();
        var profile = atmosphere.ProfileId;
        try
        {
            Require(atmosphere.EnvironmentTime == clock &&
                clock.ProcessPriority < atmosphere.ProcessPriority &&
                env.BackgroundMode == Godot.Environment.BGMode.Sky &&
                material.Shader.ResourcePath == "res://shaders/underwater_sky.gdshader",
                "normal play did not bind the shared procedural sky");
            if (profile == WorldEnvironmentProfile.Pond)
            {
                camera.GlobalPosition = new Vector3(camera.GlobalPosition.X,
                    atmosphere.SurfaceHeight + 2f, camera.GlobalPosition.Z);
                atmosphere.RefreshDepth(true, 2f);
                Require(atmosphere.CameraImmersion01 < .001f, "Pond air sample remained underwater");
            }
            var noon = Sample(12);
            var dawn = Sample(6);
            var morning = Sample(9);
            var afternoon = Sample(17);
            var sunset = Sample(18);
            var twilight = Sample(18.5);
            var night = Sample(0);
            PrintWaterSample(profile, 6, dawn);
            PrintWaterSample(profile, 12, noon);
            PrintWaterSample(profile, 18, sunset);
            PrintWaterSample(profile, 0, night);
            Require(noon.SurfaceLight > dawn.SurfaceLight && dawn.SurfaceLight > night.SurfaceLight &&
                noon.ReflectedLight > 3f * night.ReflectedLight &&
                noon.ShaftEnergy > afternoon.ShaftEnergy && afternoon.ShaftEnergy > sunset.ShaftEnergy &&
                noon.CausticEnergy > twilight.CausticEnergy && night.CausticEnergy < .0001f,
                "water and underwater daylight samples are not ordered");
            SolarLightingState? previous = null;
            SolarWaterState? previousWater = null;
            for (var tick = 0; tick <= 1440; tick++)
            {
                var solarSample = SolarState.At(tick / 1440.0);
                var mapped = SolarLightingState.From(solarSample, profile);
                var water = SolarWaterState.From(solarSample, mapped,
                    WaterVisualProfile.For(profile), atmosphere.SunEnergy);
                Require(ColorFinite(mapped.AirHorizon) && ColorFinite(mapped.AirZenith) &&
                    ColorFinite(mapped.AirAmbient) && ColorFinite(mapped.DirectColor) &&
                    float.IsFinite(mapped.WarmStrength) && mapped.WarmStrength is >= 0 and <= 1 &&
                    float.IsFinite(mapped.SunGlow) && mapped.SunGlow is >= 0 and <= 1 &&
                    float.IsFinite(mapped.DirectEnergyFactor) && mapped.DirectEnergyFactor is >= 0 and <= 1 &&
                    float.IsFinite(mapped.VolumetricFactor) && mapped.VolumetricFactor is >= 0 and <= 1 &&
                    float.IsFinite(mapped.AirAmbientEnergy) && mapped.AirAmbientEnergy is > 0 and <= 1,
                    "solar presentation left finite bounds");
                if (previous is { } prior)
                    Require(ToVector(mapped.AirHorizon).DistanceTo(ToVector(prior.AirHorizon)) < .06f &&
                        ToVector(mapped.AirZenith).DistanceTo(ToVector(prior.AirZenith)) < .06f &&
                        Math.Abs(mapped.DirectEnergyFactor - prior.DirectEnergyFactor) < .06f,
                        $"solar presentation jumped between adjacent samples at tick {tick}: horizon={ToVector(mapped.AirHorizon).DistanceTo(ToVector(prior.AirHorizon)):F3} zenith={ToVector(mapped.AirZenith).DistanceTo(ToVector(prior.AirZenith)):F3} energy={Math.Abs(mapped.DirectEnergyFactor - prior.DirectEnergyFactor):F3}");
                previous = mapped;
                Require(float.IsFinite(water.SurfaceDaylight) && water.SurfaceDaylight is >= .6f and <= 1.3f &&
                    InUnit(water.SurfaceIllumination) && InUnit(water.UnderwaterAmbient) &&
                    InUnit(water.UnderwaterSky) && InUnit(water.ShaftEnergy) &&
                    InUnit(water.CausticEnergy) && InUnit(water.ReflectionSunEnergy) &&
                    InUnit(water.ShaftLean) && ColorFinite(water.CausticTint),
                    "water solar response left finite bounds");
                if (previousWater is { } priorWater)
                    Require(Math.Abs(water.SurfaceDaylight - priorWater.SurfaceDaylight) < .04f &&
                        Math.Abs(water.SurfaceIllumination - priorWater.SurfaceIllumination) < .04f &&
                        Math.Abs(water.ShaftEnergy - priorWater.ShaftEnergy) < .04f &&
                        Math.Abs(water.CausticEnergy - priorWater.CausticEnergy) < .04f,
                        $"water solar response jumped at minute {tick}");
                previousWater = water;
            }
            Require(noon.SunEnergy > morning.SunEnergy && morning.SunEnergy > sunset.SunEnergy &&
                afternoon.SunEnergy > sunset.SunEnergy &&
                night.SunEnergy is > 0f and < .2f &&
                noon.AmbientEnergy > twilight.AmbientEnergy &&
                twilight.AmbientEnergy > night.AmbientEnergy && night.AmbientEnergy > .01f,
                "day, twilight and night energies are not ordered");
            Require(Luminance(noon.Horizon) > 3f * Luminance(night.Horizon) &&
                Luminance(noon.Zenith) > 3f * Luminance(night.Zenith) &&
                noon.Zenith.Z > noon.Zenith.X &&
                dawn.Warm > noon.Warm && sunset.Warm > noon.Warm &&
                twilight.Warm > night.Warm &&
                noon.Horizon.DistanceTo(noon.Zenith) > .08f &&
                night.Horizon.DistanceTo(night.Zenith) > .001f &&
                Luminance(night.Horizon) < .01f,
                "sky lacks daylight, localized warmth or vertical variation");
            if (profile == WorldEnvironmentProfile.Pond)
                Require(Luminance(noon.FogColor) > 1.5f * Luminance(sunset.FogColor) &&
                    Luminance(sunset.FogColor) > 3f * Luminance(night.FogColor) &&
                    Luminance(night.FogColor) < .03f &&
                    Math.Abs(noon.FogDensity - sunset.FogDensity) < .0001f &&
                    Math.Abs(noon.FogDensity - night.FogDensity) < .0001f,
                    "Pond air fog stayed pale at sunset or changed extinction");
            else
                Require(Luminance(noon.FogColor) > 2f * Luminance(night.FogColor) &&
                    Math.Abs(noon.FogDensity - night.FogDensity) < .0001f,
                    "Ocean night scatter did not darken or changed extinction");
            foreach (var anchor in new[] { 0.0, 6.0, 18.0 })
            {
                var left = Sample(anchor - .0001);
                var right = Sample(anchor + .0001);
                Require((anchor == 0.0 || left.SunEnergy < .001f && right.SunEnergy < .001f ||
                    left.Direction.DistanceTo(right.Direction) < .0001f) &&
                    left.Horizon.DistanceTo(right.Horizon) < .001f &&
                    left.Zenith.DistanceTo(right.Zenith) < .001f &&
                    left.FogColor.DistanceTo(right.FogColor) < .001f &&
                    Math.Abs(left.SunEnergy - right.SunEnergy) < .001f &&
                    Math.Abs(left.AmbientEnergy - right.AmbientEnergy) < .001f,
                    "solar presentation jumps across a time boundary");
            }
            if (profile == WorldEnvironmentProfile.Pond)
            {
                camera.GlobalPosition = new Vector3(camera.GlobalPosition.X,
                    atmosphere.SurfaceHeight - 2f, camera.GlobalPosition.Z);
                atmosphere.RefreshDepth(true, 2f);
                var underwaterNoon = Sample(12);
                var underwaterNight = Sample(0);
                Require(atmosphere.CameraImmersion01 > .99f &&
                    underwaterNoon.AmbientEnergy > 1.2f * underwaterNight.AmbientEnergy &&
                    underwaterNoon.SunEnergy > underwaterNight.SunEnergy &&
                    underwaterNoon.FogColor.DistanceTo(underwaterNight.FogColor) > .05f &&
                    Math.Abs(underwaterNoon.FogDensity - underwaterNight.FogDensity) < .0001f,
                    "Pond underwater illumination did not lose daylight");
            }
            var pondRepeat = SolarWaterState.From(SolarState.At(.5),
                SolarLightingState.From(SolarState.At(.5), WorldEnvironmentProfile.Pond),
                WaterVisualProfile.Pond, atmosphere.SunEnergy);
            _ = SolarWaterState.From(SolarState.At(.5),
                SolarLightingState.From(SolarState.At(.5), WorldEnvironmentProfile.Ocean),
                WaterVisualProfile.Ocean, atmosphere.SunEnergy);
            Require(pondRepeat == SolarWaterState.From(SolarState.At(.5),
                SolarLightingState.From(SolarState.At(.5), WorldEnvironmentProfile.Pond),
                WaterVisualProfile.Pond, atmosphere.SunEnergy),
                "Pond solar water state leaked across profile mapping");
            Require(env.GetInstanceId() == environmentId && env.Sky.GetInstanceId() == skyId &&
                ((ShaderMaterial)env.Sky.SkyMaterial).GetInstanceId() == materialId &&
                surfaceMaterial.GetInstanceId() == surfaceId && shaftMaterial.GetInstanceId() == shaftId &&
                caustic.GetInstanceId() == causticId &&
                main.GetNode<WorldEnvironment>("WorldEnvironment").Environment == env &&
                fish.GlobalPosition == fishPosition && fish.Velocity == fishVelocity,
                "solar sweep recreated resources or changed fish state");
            GD.Print($"SOLAR_LIGHTING_CHECK_OK habitat={profile.ToString().ToLowerInvariant()} samples=1441 boundaries=3 sky=stable water=stable sun=shared fish=unchanged noon_sun={noon.SunEnergy:F3} night_sun={night.SunEnergy:F3} noon_ambient={noon.AmbientEnergy:F3} night_ambient={night.AmbientEnergy:F3}");
            GD.Print($"SOLAR_WATER_CHECK_OK habitat={profile.ToString().ToLowerInvariant()} dawn_surface={dawn.SurfaceLight:F3} noon_surface={noon.SurfaceLight:F3} sunset_surface={sunset.SurfaceLight:F3} night_surface={night.SurfaceLight:F3} noon_reflection={noon.ReflectedLight:F3} night_reflection={night.ReflectedLight:F3} noon_shaft={noon.ShaftEnergy:F3} night_shaft={night.ShaftEnergy:F3} noon_caustic={noon.CausticEnergy:F3} night_caustic={night.CausticEnergy:F3}");
        }
        finally { camera.GlobalTransform = cameraTransform; }

        SampleResult Sample(double hour)
        {
            clock.SetTimeForDebug(hour);
            atmosphere.RefreshSolar(true);
            var state = clock.Solar;
            var expected = SolarLightingState.From(state, profile);
            var water = SolarWaterState.From(state, expected, WaterVisualProfile.For(profile), atmosphere.SunEnergy);
            var direction = sun.GlobalTransform.Basis.Z.Normalized();
            var horizon = (Vector3)material.GetShaderParameter("air_horizon");
            var zenith = (Vector3)material.GetShaderParameter("air_zenith");
            var warm = (float)material.GetShaderParameter("warm_strength");
            var glow = (float)material.GetShaderParameter("sun_glow");
            var ambient = env.AmbientLightEnergy;
            var expectedDirection = state.DirectionToSun.Y < 0f ? -state.DirectionToSun : state.DirectionToSun;
            Require(direction.IsFinite() && direction.DistanceTo(expectedDirection) < .0001f &&
                ((Vector3)material.GetShaderParameter("sun_direction")).DistanceTo(direction) < .0001f &&
                ((Vector3)material.GetShaderParameter("warm_direction")).DistanceTo(state.DirectionToSun) < .0001f &&
                Math.Abs((float)material.GetShaderParameter("moon_glow") -
                    SolarLightingState.MoonVisibility(state)) < .0001f &&
                ((Vector3)surfaceMaterial.GetShaderParameter("sun_direction")).DistanceTo(direction) < .0001f &&
                horizon.DistanceTo(ToVector(expected.AirHorizon)) < .0001f &&
                zenith.DistanceTo(ToVector(expected.AirZenith)) < .0001f &&
                Math.Abs(warm - expected.WarmStrength) < .0001f &&
                Math.Abs(glow - expected.SunGlow) < .0001f &&
                ColorFinite(sun.LightColor) && float.IsFinite(sun.LightEnergy) &&
                sun.LightEnergy >= 0 && float.IsFinite(ambient) && ambient >= 0 &&
                float.IsFinite(sun.LightVolumetricFogEnergy) &&
                sun.LightVolumetricFogEnergy >= 0 &&
                Math.Abs(sun.LightVolumetricFogEnergy -
                    0.8f * expected.VolumetricFactor * atmosphere.CameraImmersion01) < .001f &&
                env.Sky.GetInstanceId() == skyId && material.GetInstanceId() == materialId,
                "actual sky, sun or ambient state diverged from shared solar state");
            Require(((Vector3)surfaceMaterial.GetShaderParameter("reflected_sky_horizon")).DistanceTo(ToVector(expected.AirHorizon)) < .0001f &&
                ((Vector3)surfaceMaterial.GetShaderParameter("reflected_sky_zenith")).DistanceTo(ToVector(expected.AirZenith)) < .0001f &&
                ((Vector3)surfaceMaterial.GetShaderParameter("reflection_sun_color")).DistanceTo(ToVector(expected.DirectColor)) < .0001f &&
                Math.Abs((float)surfaceMaterial.GetShaderParameter("reflection_sun_energy") - water.ReflectionSunEnergy) < .0001f &&
                Math.Abs((float)surfaceMaterial.GetShaderParameter("surface_daylight") - water.SurfaceDaylight) < .0001f &&
                Math.Abs((float)surfaceMaterial.GetShaderParameter("surface_illumination") - water.SurfaceIllumination) < .0001f &&
                Math.Abs((float)material.GetShaderParameter("underwater_daylight") - water.UnderwaterSky) < .0001f &&
                ((Vector3)shaftMaterial.GetShaderParameter("sun_direction")).DistanceTo(direction) < .0001f &&
                Math.Abs((float)shaftMaterial.GetShaderParameter("shaft_lean") - water.ShaftLean) < .0001f &&
                Math.Abs((float)shaftMaterial.GetShaderParameter("shaft_brightness") -
                    atmosphere.ShaftBrightness * atmosphere.CameraImmersion01 * water.ShaftEnergy) < .0001f &&
                Math.Abs(caustic.EmissionEnergy - atmosphere.CausticEnergy *
                    atmosphere.CameraImmersion01 * water.CausticEnergy) < .0001f &&
                ToVector(caustic.Modulate).DistanceTo(ToVector(water.CausticTint)) < .0001f,
                "actual surface, shaft or caustic state diverged from shared solar state");
            if (profile == WorldEnvironmentProfile.Pond && atmosphere.CameraImmersion01 < .01f)
                Require(Math.Abs(ambient - expected.AirAmbientEnergy) < .001f &&
                    ToVector(env.AmbientLightColor).DistanceTo(ToVector(expected.AirAmbient)) < .001f &&
                    ToVector(sun.LightColor).DistanceTo(ToVector(expected.DirectColor)) < .001f,
                    "Pond air lighting does not match the solar baseline");
            return new SampleResult(direction, horizon, zenith, ToVector(env.FogLightColor),
                env.FogDensity, warm, sun.LightEnergy, ambient,
                water.SurfaceIllumination, Luminance(horizon), water.ShaftEnergy, water.CausticEnergy,
                state.ElevationRadians, water.SurfaceDaylight, water.UnderwaterAmbient);
        }
    }

    private readonly record struct SampleResult(Vector3 Direction, Vector3 Horizon,
        Vector3 Zenith, Vector3 FogColor, float FogDensity, float Warm,
        float SunEnergy, float AmbientEnergy,
        float SurfaceLight, float ReflectedLight, float ShaftEnergy, float CausticEnergy,
        float Elevation, float SurfaceDaylight, float WaterAmbient);
    private static void PrintWaterSample(WorldEnvironmentProfile profile, int hour, SampleResult sample) =>
        GD.Print($"SOLAR_WATER_STATE habitat={profile.ToString().ToLowerInvariant()} hour={hour:00} elevation={sample.Elevation:F3} sun={sample.SunEnergy:F3} sky_horizon={sample.Horizon} surface_daylight={sample.SurfaceDaylight:F3} surface_light={sample.SurfaceLight:F3} reflected_horizon={sample.ReflectedLight:F3} water_ambient={sample.WaterAmbient:F3} shafts={sample.ShaftEnergy:F3} caustics={sample.CausticEnergy:F3}");
    private static bool InUnit(float value) => float.IsFinite(value) && value is >= 0f and <= 1f;
    private static Vector3 ToVector(Color color) => new(color.R, color.G, color.B);
    private static bool ColorFinite(Color color) => float.IsFinite(color.R) &&
        float.IsFinite(color.G) && float.IsFinite(color.B) && float.IsFinite(color.A);
    private static float Luminance(Vector3 color) => .2126f * color.X + .7152f * color.Y + .0722f * color.Z;
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
#endif
