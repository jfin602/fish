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
        var camera = main.GetNode<Camera3D>("Fish/CameraRig/Camera3D");
        var fish = main.GetNode<FishController>("Fish");
        var fishPosition = fish.GlobalPosition;
        var fishVelocity = fish.Velocity;
        var cameraTransform = camera.GlobalTransform;
        var skyId = sky.GetInstanceId();
        var materialId = material.GetInstanceId();
        var environmentId = env.GetInstanceId();
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
            SolarLightingState? previous = null;
            for (var tick = 0; tick <= 1440; tick++)
            {
                var mapped = SolarLightingState.From(SolarState.At(tick / 1440.0), profile);
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
            }
            Require(noon.SunEnergy > morning.SunEnergy && morning.SunEnergy > sunset.SunEnergy &&
                afternoon.SunEnergy > sunset.SunEnergy && night.SunEnergy < .0001f &&
                noon.AmbientEnergy > twilight.AmbientEnergy &&
                twilight.AmbientEnergy > night.AmbientEnergy && night.AmbientEnergy > .01f,
                "day, twilight and night energies are not ordered");
            Require(Luminance(noon.Horizon) > 3f * Luminance(night.Horizon) &&
                Luminance(noon.Zenith) > 3f * Luminance(night.Zenith) &&
                noon.Zenith.Z > noon.Zenith.X &&
                dawn.Warm > noon.Warm && sunset.Warm > noon.Warm &&
                twilight.Warm > night.Warm &&
                noon.Horizon.DistanceTo(noon.Zenith) > .08f &&
                night.Horizon.DistanceTo(night.Zenith) > .01f,
                "sky lacks daylight, localized warmth or vertical variation");
            foreach (var anchor in new[] { 0.0, 6.0, 18.0 })
            {
                var left = Sample(anchor - .0001);
                var right = Sample(anchor + .0001);
                Require(left.Direction.DistanceTo(right.Direction) < .0001f &&
                    left.Horizon.DistanceTo(right.Horizon) < .001f &&
                    left.Zenith.DistanceTo(right.Zenith) < .001f &&
                    Math.Abs(left.SunEnergy - right.SunEnergy) < .001f &&
                    Math.Abs(left.AmbientEnergy - right.AmbientEnergy) < .001f,
                    "solar presentation jumps across a time boundary");
            }
            Require(env.GetInstanceId() == environmentId && env.Sky.GetInstanceId() == skyId &&
                ((ShaderMaterial)env.Sky.SkyMaterial).GetInstanceId() == materialId &&
                main.GetNode<WorldEnvironment>("WorldEnvironment").Environment == env &&
                fish.GlobalPosition == fishPosition && fish.Velocity == fishVelocity,
                "solar sweep recreated resources or changed fish state");
            GD.Print($"SOLAR_LIGHTING_CHECK_OK habitat={profile.ToString().ToLowerInvariant()} samples=1441 boundaries=3 sky=stable sun=shared fish=unchanged noon_sun={noon.SunEnergy:F3} night_sun={night.SunEnergy:F3} noon_ambient={noon.AmbientEnergy:F3} night_ambient={night.AmbientEnergy:F3}");
        }
        finally { camera.GlobalTransform = cameraTransform; }

        SampleResult Sample(double hour)
        {
            clock.SetTimeForDebug(hour);
            atmosphere.RefreshSolar(true);
            var state = clock.Solar;
            var expected = SolarLightingState.From(state, profile);
            var direction = sun.GlobalTransform.Basis.Z.Normalized();
            var horizon = (Vector3)material.GetShaderParameter("air_horizon");
            var zenith = (Vector3)material.GetShaderParameter("air_zenith");
            var warm = (float)material.GetShaderParameter("warm_strength");
            var glow = (float)material.GetShaderParameter("sun_glow");
            var ambient = env.AmbientLightEnergy;
            Require(direction.IsFinite() && direction.DistanceTo(state.DirectionToSun) < .0001f &&
                ((Vector3)material.GetShaderParameter("sun_direction")).DistanceTo(direction) < .0001f &&
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
            if (profile == WorldEnvironmentProfile.Pond)
                Require(Math.Abs(ambient - expected.AirAmbientEnergy) < .001f &&
                    ToVector(env.AmbientLightColor).DistanceTo(ToVector(expected.AirAmbient)) < .001f &&
                    ToVector(sun.LightColor).DistanceTo(ToVector(expected.DirectColor)) < .001f,
                    "Pond air lighting does not match the solar baseline");
            return new SampleResult(direction, horizon, zenith, warm, sun.LightEnergy, ambient);
        }
    }

    private readonly record struct SampleResult(Vector3 Direction, Vector3 Horizon,
        Vector3 Zenith, float Warm, float SunEnergy, float AmbientEnergy);
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
