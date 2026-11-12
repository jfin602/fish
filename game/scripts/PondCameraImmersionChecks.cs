#nullable enable
using Godot;
using System;

namespace Fish;

/// <summary>Headless camera-eye and presentation regression on the baked Pond.</summary>
public static class PondCameraImmersionChecks
{
    public static void Run(Main main)
    {
        var fish = main.GetNode<FishController>("Fish");
        var rig = fish.GetNode<FollowCamera>("CameraRig");
        var eye = rig.GetNode<Camera3D>("Camera3D");
        var atmosphere = main.GetNode<UnderwaterAtmosphere>("UnderwaterAtmosphere");
        var seabed = main.GetNode<SeabedSurface>("Habitat/Seabed");
        var env = atmosphere.WaterEnvironment ?? throw new Exception("missing environment");
        var localFog = (FogMaterial)main.GetNode<FogVolume>(
            "UnderwaterAtmosphere/PondTurbidity").Material;
        var sun = main.GetNode<DirectionalLight3D>("Sun");
        Require(seabed.TryGetPondWaterSurfaceHeight(out var water) &&
            Mathf.Abs(water - atmosphere.SurfaceHeight) < .001f &&
            atmosphere.ProfileId == WorldEnvironmentProfile.Pond,
            "Pond camera must use its generated water plane");
        Require(UnderwaterAtmosphere.TargetCameraImmersion(null, water) == 1f &&
            UnderwaterAtmosphere.TargetCameraImmersion(float.NaN, water) == 1f &&
            UnderwaterAtmosphere.TargetCameraImmersion(water, water) == .5f &&
            UnderwaterAtmosphere.TargetCameraImmersion(water, water + 1f) == 0f &&
            UnderwaterAtmosphere.TargetCameraImmersion(water, water - 1f) == 1f,
            "immersion band or absent-height fallback");
        var surfaceMaterial = (ShaderMaterial)main.GetNode<MeshInstance3D>(
            "UnderwaterAtmosphere/WaterEffects/OverheadSurface").MaterialOverride;
        Require(surfaceMaterial.Shader.Code.Contains("cull_disabled") &&
            surfaceMaterial.Shader.Code.Contains("FRONT_FACING") &&
            (float)surfaceMaterial.GetShaderParameter("air_surface_enabled") == 1f,
            "Pond surface has no translucent air-facing side");
        var originalEye = rig.GlobalTransform;
        var fishPosition = fish.GlobalPosition;
        var fishVelocity = fish.Velocity;
        var fishBasis = fish.GlobalBasis;
        var fishTransform = fish.GlobalTransform;
        var fishId = fish.GetInstanceId();
        var originalSettings = atmosphere.CaptureLighting();
        try
        {
            var core = seabed.Basin.DeepCore;
            var x = core.X; var z = core.Y;
            void At(float y, int frames = 1)
            {
                rig.GlobalPosition = new Vector3(x, y, z);
                for (var tick = 0; tick < frames; tick++)
                    atmosphere.RefreshDepth(true, 1f / 60f);
                Require(Mathf.Abs(atmosphere.CameraSignedImmersionDepth - (water - y)) < .001f,
                    "signed depth did not follow actual eye");
            }
            At(water - 2f, 120);
            Require(atmosphere.CameraImmersion01 > .999f &&
                env.FogDensity > .55f && env.FogDensity < .61f &&
                Mathf.Abs(env.FogDepthBegin - 3f) < .001f &&
                localFog.Density > .10f &&
                (float)((ShaderMaterial)env.Sky.SkyMaterial).GetShaderParameter("air_mix") < .001f,
                "submerged profile did not restore");
            var previous = atmosphere.CameraImmersion01;
            var maximumStep = 0f;
            for (var step = 0; step <= 80; step++)
            {
                At(water - 2f + step * .05f);
                var value = atmosphere.CameraImmersion01;
                maximumStep = Mathf.Max(maximumStep, Mathf.Abs(value - previous));
                Require(value <= previous + .0001f && maximumStep < .12f,
                    "frame-to-frame immersion discontinuity");
                previous = value;
            }
            At(water + 2f, 120);
            Require(atmosphere.CameraImmersion01 < .001f &&
                Mathf.Abs(env.FogDensity - WaterVisualProfile.Pond.AirFogDensity) < .001f &&
                Mathf.Abs(env.FogDepthBegin - WaterVisualProfile.Pond.AirFogBegin) < .001f &&
                Mathf.Abs(env.FogDepthEnd - WaterVisualProfile.Pond.AirFogEnd) < .001f &&
                env.VolumetricFogDensity < .001f &&
                localFog.Density < .001f &&
                Mathf.Abs(sun.LightEnergy - 1f) < .001f &&
                (float)((ShaderMaterial)env.Sky.SkyMaterial).GetShaderParameter("air_mix") > .999f,
                "air profile was not reached");
            Require(atmosphere.ScreenLayer is { Layer: 0 } &&
                main.GetNode<CanvasLayer>("PrototypeHelp").Layer > atmosphere.ScreenLayer.Layer &&
                (float)((ShaderMaterial)atmosphere.ScreenLayer.GetNode<ColorRect>("ScreenTreatment").Material)
                    .GetShaderParameter("immersion") < .001f,
                "air treatment leaked into UI");
            previous = atmosphere.CameraImmersion01;
            for (var tick = 0; tick < 180; tick++)
            {
                At(water + (tick % 2 == 0 ? .04f : -.04f));
                Require(Mathf.Abs(atmosphere.CameraImmersion01 - previous) < .12f,
                    "oscillating eye flickered");
                previous = atmosphere.CameraImmersion01;
            }
            At(water, 120);
            Require(Mathf.Abs(atmosphere.CameraImmersion01 - .5f) < .002f,
                "exact waterline midpoint");
            At(water + 1.5f, 120);
            Require(fish.GlobalPosition.Y < water && eye.GlobalPosition.Y > water,
                "camera/fish water occupancy");
            x = 0f; z = 0f;
            while (seabed.Basin.DistanceToShore(x, z) > 5f) z -= 1f;
            Require(seabed.Basin.HeightAt(x, z) > seabed.Basin.HeightAt(core.X, core.Y),
                "bank terrain probe did not rise");
            At(water + 1.5f, 120);
            Require(atmosphere.CameraImmersion01 < .001f,
                "camera waterline drifted over bank terrain");
            x = core.X; z = core.Y;
            main.GetTree().Paused = true;
            var custom = originalSettings.Copy();
            custom.SunEnergy = .9f;
            custom.ScreenWarpPixels = .08f;
            atmosphere.ApplyLighting(custom);
            Require(main.GetTree().Paused && atmosphere.CameraImmersion01 < .001f &&
                Mathf.Abs(atmosphere.CaptureLighting().SunEnergy - .9f) < .001f &&
                Mathf.Abs(sun.LightEnergy - 1f) < .001f,
                "paused air preset altered effective profile");
            main.GetTree().Paused = false;
            At(water - 2f, 120);
            Require(atmosphere.CameraImmersion01 > .999f &&
                Mathf.Abs(sun.LightEnergy - .9f * Mathf.Lerp(1f,
                    WaterVisualProfile.Pond.DeepLightFactor, atmosphere.EffectiveDepth01)) < .002f &&
                Mathf.Abs((float)((ShaderMaterial)atmosphere.ScreenLayer!.GetNode<ColorRect>("ScreenTreatment").Material)
                    .GetShaderParameter("warp_pixels") - .08f) < .001f,
                "underwater preset failed to return");
            atmosphere.SetEnabled(false);
            atmosphere.SetEnabled(true);
            Require(env.FogEnabled && atmosphere.CameraImmersion01 > .999f &&
                atmosphere.ScreenLayer!.Visible, "re-entry after effect toggle");
            Require(fish.GetInstanceId() == fishId && fish.GlobalPosition == fishPosition &&
                fish.GlobalBasis == fishBasis && fish.Velocity == fishVelocity,
                "camera-only movement changed fish state");
            // Exercise the real follow/obstruction path with a fixed fish. The
            // rig may compress near a bank, but must take finite, continuous steps.
            rig.GlobalTransform = originalEye;
            rig._PhysicsProcess(1.0 / 60.0);
            var previousPosition = rig.GlobalPosition;
            var previousForward = -rig.GlobalBasis.Z;
            var maxFollowStep = 0f;
            foreach (var preset in ControlPreset.Builtins())
            {
                fish.SetActivePreset(preset);
                for (var frame = 0; frame < 30; frame++)
                {
                    rig._PhysicsProcess(1.0 / 60.0);
                    var step = rig.GlobalPosition.DistanceTo(previousPosition);
                    maxFollowStep = Mathf.Max(maxFollowStep, step);
                    Require(rig.GlobalPosition.IsFinite() && step < 1f &&
                        (-rig.GlobalBasis.Z).AngleTo(previousForward) < .6f,
                        "preset follow position/rotation discontinuity");
                    previousPosition = rig.GlobalPosition;
                    previousForward = -rig.GlobalBasis.Z;
                }
            }
            Require(fish.GetInstanceId() == fishId && fish.GlobalPosition == fishPosition &&
                fish.GlobalBasis == fishBasis && fish.Velocity == fishVelocity,
                "follow/orbit preset moved the fish");
            // Hybrid 2 uses the real captured mouse orbit and wheel zoom path.
            // Place a fixed test fish just below the surface at the deep core.
            fish.GlobalPosition = new Vector3(core.X, water - .65f, core.Y);
            fish.SetActivePreset(ControlPreset.Builtins()[3]);
            fish._PhysicsProcess(1.0 / 60.0); // Release-only rearm tick.
            fish.ResetMouseCapture();
            rig.ResetFollow();
            rig._PhysicsProcess(1.0 / 60.0);
            var orbitFish = fish.GlobalTransform;
            var orbitVelocity = fish.Velocity;
            var lastOrbitPosition = eye.GlobalPosition;
            var lastOrbitForward = -eye.GlobalBasis.Z;
            void OrbitTick()
            {
                rig._PhysicsProcess(1.0 / 60.0);
                Require(eye.GlobalPosition.DistanceTo(lastOrbitPosition) < .5f &&
                    (-eye.GlobalBasis.Z).AngleTo(lastOrbitForward) < .5f,
                    "manual orbit/zoom frame discontinuity");
                lastOrbitPosition = eye.GlobalPosition;
                lastOrbitForward = -eye.GlobalBasis.Z;
            }
            rig._Input(new InputEventMouseMotion { Relative = Vector2.Zero });
            for (var step = 0; step < 4; step++)
            {
                rig._Input(new InputEventMouseMotion { Relative = new Vector2(0, -80) });
                OrbitTick();
            }
            for (var frame = 0; frame < 90; frame++) OrbitTick();
            Require(eye.GlobalPosition.Y < water && fish.GlobalPosition.Y < water,
                "Hybrid 2 downward orbit did not submerge eye");
            var beforeZoom = eye.GlobalPosition.DistanceTo(fish.GlobalPosition);
            rig._Input(new InputEventMouseButton { ButtonIndex = MouseButton.WheelDown, Pressed = true });
            for (var frame = 0; frame < 90; frame++) OrbitTick();
            Require(eye.GlobalPosition.DistanceTo(fish.GlobalPosition) > beforeZoom + .15f,
                "Hybrid 2 wheel zoom did not change eye distance");
            for (var step = 0; step < 5; step++)
            {
                rig._Input(new InputEventMouseMotion { Relative = new Vector2(0, 80) });
                OrbitTick();
            }
            for (var frame = 0; frame < 90; frame++) OrbitTick();
            Require(eye.GlobalPosition.Y > water && fish.GlobalTransform == orbitFish &&
                fish.Velocity == orbitVelocity,
                "Hybrid 2 upward orbit changed fish or failed to cross");
            GD.Print($"POND_CAMERA_IMMERSION_OK water={water:F3} max_blend_step={maximumStep:F4} max_follow_step={maxFollowStep:F4} presets=5 orbit_zoom=1 eye_above_fish_below=1 pause=1 reentry=1 fish_state=unchanged");
        }
        finally
        {
            main.GetTree().Paused = false;
            fish.GlobalTransform = fishTransform;
            fish.Velocity = fishVelocity;
            rig.GlobalTransform = originalEye;
            atmosphere.ApplyLighting(originalSettings);
            atmosphere.RefreshDepth(true, 2f);
        }
    }

    private static void Require(bool condition, string reason)
    {
        if (!condition) throw new Exception(reason);
    }
}
