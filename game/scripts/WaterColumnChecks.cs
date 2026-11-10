#nullable enable
using Godot;
using System;
using System.IO;

namespace Fish;

/// <summary>Focused headless assertions for camera-depth presentation and preset isolation.</summary>
public static class WaterColumnChecks
{
    private readonly record struct Sample(float Depth, float Fog, float Volume, float Range,
        float Sun, float Ambient, Color FogColor, Color Background, Color AmbientColor);

    public static void Run(Main main)
    {
        var atmosphere = main.GetNode<UnderwaterAtmosphere>("UnderwaterAtmosphere");
        var env = atmosphere.WaterEnvironment ?? throw new Exception("missing water environment");
        var sun = main.GetNode<DirectionalLight3D>("Sun");
        var camera = main.GetNode<Camera3D>("Fish/CameraRig/Camera3D");
        var fish = main.GetNode<FishController>("Fish");
        var seabed = main.GetNode<SeabedSurface>("Habitat/Seabed");
        var upper = main.GetNode<CollisionShape3D>("Habitat/HiddenLimits/Upper");
        var upperBox = (BoxShape3D)upper.Shape;
        var surface = main.GetNode<MeshInstance3D>("UnderwaterAtmosphere/WaterEffects/OverheadSurface");
        var surfaceY = upper.GlobalPosition.Y + upperBox.Size.Y * 0.5f + 0.34f;
        Require(Mathf.Abs(surfaceY - atmosphere.SurfaceHeight) < 0.001f &&
            Mathf.Abs(surfaceY - surface.GlobalPosition.Y) < 0.001f, "surface depth zero drifted");
        var deepY = seabed.GetHeightAt(0, SeabedSurface.DeepShelfReferenceZ) + 2f;
        Require(Mathf.Abs(deepY - atmosphere.DeepReferenceHeight) < 0.001f &&
            deepY < surfaceY - 10f, "deep shelf reference drifted");
        Require(Mathf.Abs(env.FogHeight - (surfaceY - 0.6f * (surfaceY - deepY) + atmosphere.LowerFogStartHeight)) < 0.001f,
            "height fog not anchored to water column");

        var cameraId = camera.GetInstanceId();
        var fishId = fish.GetInstanceId();
        var tuning = fish.Tuning;
        var fishPosition = fish.GlobalPosition;
        var fishVelocity = fish.Velocity;
        var cameraTransform = camera.GlobalTransform;
        var baseSettings = atmosphere.CaptureLighting();
        var file = ProjectSettings.GlobalizePath($"user://depth-check-{Guid.NewGuid():N}.json");
        try
        {
            var store = new LightingPresetStore(file);
            var shallow = At(surfaceY, atmosphere, camera, env, sun);
            var middle = At((surfaceY + deepY) * 0.5f, atmosphere, camera, env, sun);
            var deep = At(deepY, atmosphere, camera, env, sun);
            Require(shallow.Depth < 0.001f && Mathf.Abs(middle.Depth - 0.5f) < 0.001f &&
                deep.Depth > 0.999f, "depth normalization/easing failed");
            Require(shallow.Fog < middle.Fog && middle.Fog < deep.Fog &&
                shallow.Volume < middle.Volume && middle.Volume < deep.Volume &&
                shallow.Range > middle.Range && middle.Range > deep.Range &&
                shallow.Sun > middle.Sun && middle.Sun > deep.Sun &&
                shallow.Ambient > middle.Ambient && middle.Ambient > deep.Ambient,
                "extinction not monotonic");
            Require(Mathf.Abs(shallow.Fog - baseSettings.DistanceFogDensity) < 0.00001f &&
                Mathf.Abs(shallow.Sun - baseSettings.SunEnergy) < 0.0001f,
                "shallow lighting changed from c2 base");
            Bounded(shallow); Bounded(middle); Bounded(deep);
            var sky = (ShaderMaterial)env.Sky.SkyMaterial;
            Require(Mathf.Abs((float)sky.GetShaderParameter("water_depth") - 1f) < 0.001f,
                "visible sky background did not receive deep water value");
            Require(deep.FogColor.B / deep.FogColor.R > shallow.FogColor.B / shallow.FogColor.R &&
                deep.Background.B / deep.Background.R > shallow.Background.B / shallow.Background.R &&
                deep.AmbientColor.B / deep.AmbientColor.R > shallow.AmbientColor.B / shallow.AmbientColor.R,
                "deep water did not cool relative to shallow water");
            Require(At(surfaceY + 5f, atmosphere, camera, env, sun).Depth == 0f &&
                At(deepY - 5f, atmosphere, camera, env, sun).Depth == 1f,
                "depth escaped column bounds");
            atmosphere.SetEnabled(false);
            atmosphere.SetEnabled(true);
            Require(Mathf.Abs(env.FogDensity - deep.Fog) < 0.00001f &&
                Mathf.Abs(sun.LightEnergy - deep.Sun) < 0.0001f,
                "toggle did not restore deep effective lighting");

            var customBase = baseSettings.Copy();
            customBase.SunEnergy = 2f;
            customBase.AmbientEnergy = 0.7f;
            customBase.DistanceFogDensity = 0.005f;
            var preset = store.SaveAs("Depth isolation", customBase);
            main.GetTree().Paused = true;
            atmosphere.ApplyLighting(preset.Settings);
            main.GetTree().Paused = false;
            Require(env.FogDensity > customBase.DistanceFogDensity && sun.LightEnergy < customBase.SunEnergy &&
                Mathf.Abs(atmosphere.CaptureLighting().SunEnergy - customBase.SunEnergy) < 0.0001f,
                "deep preset application wrote effective values into base");
            store.Choose(LightingPresetStore.DefaultId);
            atmosphere.ApplyLighting(store.Current.Settings);
            Require(Mathf.Abs(env.FogDensity - deep.Fog) < 0.00001f &&
                Mathf.Abs(sun.LightEnergy - deep.Sun) < 0.0001f,
                "preset switch at depth did not recompute effective values");
            store.Choose(preset.Id);
            var reloaded = new LightingPresetStore(file);
            reloaded.Load();
            Require(Mathf.Abs(reloaded.Current.Settings.SunEnergy - customBase.SunEnergy) < 0.0001f &&
                Mathf.Abs(reloaded.Current.Settings.AmbientEnergy - customBase.AmbientEnergy) < 0.0001f &&
                Mathf.Abs(reloaded.Current.Settings.DistanceFogDensity - customBase.DistanceFogDensity) < 0.00001f,
                "saved preset contains depth modulation");
            atmosphere.ApplyLighting(baseSettings);
            camera.GlobalTransform = cameraTransform;
            atmosphere.RefreshDepth(true);
            Require(camera.GetInstanceId() == cameraId && fish.GetInstanceId() == fishId &&
                ReferenceEquals(fish.Tuning, tuning) && fish.GlobalPosition == fishPosition &&
                fish.Velocity == fishVelocity && camera.GlobalTransform == cameraTransform,
                "depth presentation changed fish or camera identity/state");
            GD.Print($"DEPTH_CHECK_OK surface={surfaceY:F2} deep={deepY:F2} " +
                $"shallow=({shallow.Fog:F4},{shallow.Volume:F3},{shallow.Range:F1},{shallow.Sun:F2},{shallow.Ambient:F2}) " +
                $"middle=({middle.Fog:F4},{middle.Volume:F3},{middle.Range:F1},{middle.Sun:F2},{middle.Ambient:F2}) " +
                $"deep=({deep.Fog:F4},{deep.Volume:F3},{deep.Range:F1},{deep.Sun:F2},{deep.Ambient:F2}) presets=isolated state=unchanged");
        }
        finally { if (File.Exists(file)) File.Delete(file); }
    }

    private static Sample At(float y, UnderwaterAtmosphere atmosphere, Camera3D camera,
        Godot.Environment env, DirectionalLight3D sun)
    {
        var position = camera.GlobalPosition;
        camera.GlobalPosition = new Vector3(position.X, y, position.Z);
        atmosphere.RefreshDepth(true);
        return new Sample(atmosphere.EffectiveDepth01, env.FogDensity, env.VolumetricFogDensity,
            atmosphere.EffectiveUsefulRange, sun.LightEnergy, env.AmbientLightEnergy,
            env.FogLightColor, env.BackgroundColor, env.AmbientLightColor);
    }

    private static void Bounded(Sample sample)
    {
        foreach (var color in new[] { sample.FogColor, sample.Background, sample.AmbientColor })
            Require(color.R is >= 0 and <= 1 && color.G is >= 0 and <= 1 &&
                color.B is >= 0 and <= 1, "effective water color left gamut");
    }

    private static void Require(bool condition, string reason)
    {
        if (!condition) throw new Exception(reason);
    }
}
