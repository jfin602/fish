#nullable enable
using Godot;
using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

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
        var surfaceMaterial = (ShaderMaterial)surface.MaterialOverride;
        Require(surfaceMaterial.Shader.Code.Contains("fog_disabled") &&
            surfaceMaterial.Shader.Code.Contains("deep_cue") &&
            surfaceMaterial.Shader.Code.Contains("edge_fade"),
            "overhead-only deep transmission is missing");
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
            Require(env.FogMode == Godot.Environment.FogModeEnum.Depth &&
                Mathf.Abs(env.FogDepthBegin - 5f) < 0.001f &&
                Mathf.Abs(env.FogDepthCurve - 1.2f) < 0.001f &&
                env.FogSkyAffect == 0f, "camera-distance fog configuration drifted");
            Require(Mathf.Abs((float)surfaceMaterial.GetShaderParameter("camera_depth")) < 0.001f,
                "shallow surface changed from its base treatment");
            var middle = At((surfaceY + deepY) * 0.5f, atmosphere, camera, env, sun);
            var deep = At(deepY, atmosphere, camera, env, sun);
            Require(Mathf.Abs((float)surfaceMaterial.GetShaderParameter("camera_depth") - 1f) < 0.001f,
                "deep overhead surface did not receive camera depth");
            Require(shallow.Depth < 0.001f && Mathf.Abs(middle.Depth - 0.5f) < 0.001f &&
                deep.Depth > 0.999f, "depth normalization/easing failed");
            Require(shallow.Volume < middle.Volume && middle.Volume < deep.Volume &&
                shallow.Range > middle.Range && middle.Range > deep.Range &&
                shallow.Sun > middle.Sun && middle.Sun > deep.Sun &&
                shallow.Ambient > middle.Ambient && middle.Ambient > deep.Ambient,
                "extinction not monotonic");
            Require(Mathf.Abs(shallow.Fog - 0.96f) < 0.00001f &&
                Mathf.Abs(shallow.Range - 35f) < 0.01f &&
                Mathf.Abs(middle.Range - 27.5f) < 0.01f &&
                Mathf.Abs(deep.Range - 20f) < 0.01f &&
                Mathf.Abs(atmosphere.CaptureLighting().VisibilityDistanceMeters - baseSettings.VisibilityDistanceMeters) < 0.00001f &&
                Mathf.Abs(shallow.Sun - baseSettings.SunEnergy) < 0.0001f,
                "shallow lighting changed from c2 base");
            foreach (var sample in new[] { shallow, middle, deep })
            {
                var near = UnderwaterAtmosphere.EstimateDistanceRetention(8f, sample.Range);
                var mid = UnderwaterAtmosphere.EstimateDistanceRetention(15f, sample.Range);
                var far = UnderwaterAtmosphere.EstimateDistanceRetention(25f, sample.Range);
                Require(near > mid && mid > far && near > 0.7f && far < 0.5f,
                    "distance retention lost progressive near-to-far extinction");
            }
            Require(UnderwaterAtmosphere.EstimateDistanceRetention(25f, middle.Range) < 0.3f &&
                UnderwaterAtmosphere.EstimateDistanceRetention(25f, deep.Range) < 0.1f,
                "middle/deep distant terrain retained too much contrast");
            Require(UnderwaterAtmosphere.EstimateDistanceRetention(15f, shallow.Range) >
                UnderwaterAtmosphere.EstimateDistanceRetention(15f, middle.Range) &&
                UnderwaterAtmosphere.EstimateDistanceRetention(15f, middle.Range) >
                UnderwaterAtmosphere.EstimateDistanceRetention(15f, deep.Range),
                "deeper camera failed to shorten useful distance");
            Bounded(shallow); Bounded(middle); Bounded(deep);
            // The eased response must join both clamped endpoints without a visible numerical step.
            foreach (var boundary in new[] { surfaceY, deepY })
            {
                var above = At(boundary + 0.001f, atmosphere, camera, env, sun);
                var below = At(boundary - 0.001f, atmosphere, camera, env, sun);
                Require(Mathf.Abs(above.Range - below.Range) < 0.01f &&
                    Mathf.Abs(above.Volume - below.Volume) < 0.001f, "water-column endpoint has a numerical jump");
            }
            At(deepY, atmosphere, camera, env, sun);
            atmosphere.SetVisibilityDiagnosticBypass(true);
            Require(env.FogEnabled && env.VolumetricFogEnabled && env.FogDensity == 0f &&
                Mathf.Abs(env.FogHeightDensity - atmosphere.LowerWaterFog) < 0.0001f,
                "visibility diagnostic changed non-distance fog");
            atmosphere.SetVisibilityDiagnosticBypass(false);
            Require(Mathf.Abs(env.FogDensity - deep.Fog) < 0.0001f,
                "visibility diagnostic failed to restore depth fog");
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
            Require(!env.FogEnabled && !env.VolumetricFogEnabled &&
                atmosphere.ScreenLayer is { Visible: false }, "atmosphere toggle did not disable fog/screen");
            atmosphere.SetEnabled(true);
            Require(env.FogEnabled && env.VolumetricFogEnabled &&
                atmosphere.ScreenLayer is { Visible: true } &&
                Mathf.Abs(env.FogDepthEnd - deep.Range) < 0.0001f &&
                Mathf.Abs(sun.LightEnergy - deep.Sun) < 0.0001f,
                "toggle did not restore deep effective lighting");

            var customBase = baseSettings.Copy();
            customBase.SunEnergy = 2f;
            customBase.AmbientEnergy = 0.7f;
            customBase.VisibilityDistanceMeters = 25f;
            var preset = store.SaveAs("Depth isolation", customBase);
            main.GetTree().Paused = true;
            atmosphere.ApplyLighting(preset.Settings);
            Require(main.GetTree().Paused && Mathf.Abs(env.FogDepthEnd -
                customBase.VisibilityDistanceMeters * 4f / 7f) < 0.001f,
                "paused deep preset preview lost effective visibility");
            main.GetTree().Paused = false;
            Require(atmosphere.EffectiveUsefulRange < deep.Range && sun.LightEnergy < customBase.SunEnergy &&
                Mathf.Abs(atmosphere.CaptureLighting().SunEnergy - customBase.SunEnergy) < 0.0001f,
                "deep preset application wrote effective values into base");
            store.Choose(LightingPresetStore.DefaultId);
            atmosphere.ApplyLighting(store.Current.Settings);
            Require(Mathf.Abs(env.FogDepthEnd - deep.Range) < 0.0001f &&
                Mathf.Abs(sun.LightEnergy - deep.Sun) < 0.0001f,
                "preset switch at depth did not recompute effective values");
            store.Choose(preset.Id);
            var reloaded = new LightingPresetStore(file);
            reloaded.Load();
            Require(Mathf.Abs(reloaded.Current.Settings.SunEnergy - customBase.SunEnergy) < 0.0001f &&
                Mathf.Abs(reloaded.Current.Settings.AmbientEnergy - customBase.AmbientEnergy) < 0.0001f &&
                Mathf.Abs(reloaded.Current.Settings.VisibilityDistanceMeters - customBase.VisibilityDistanceMeters) < 0.00001f,
                "saved preset contains depth modulation");
            atmosphere.VisibilityRangeMeters = 60f;
            atmosphere.RefreshDepth(true);
            var clearRange = atmosphere.EffectiveUsefulRange;
            atmosphere.VisibilityRangeMeters = 18f;
            atmosphere.RefreshDepth(true);
            var murkyRange = atmosphere.EffectiveUsefulRange;
            Require(float.IsFinite(clearRange) && float.IsFinite(murkyRange) &&
                clearRange > deep.Range && deep.Range > murkyRange && murkyRange > env.FogDepthBegin &&
                Mathf.Abs(env.FogDepthEnd - murkyRange) < 0.001f,
                "meter visibility no longer maps monotonically to bounded fog");
            CheckPresetCompatibility(file, baseSettings);
            atmosphere.ApplyLighting(baseSettings);
            camera.GlobalTransform = cameraTransform;
            atmosphere.RefreshDepth(true);
            Require(camera.GetInstanceId() == cameraId && fish.GetInstanceId() == fishId &&
                ReferenceEquals(fish.Tuning, tuning) && fish.GlobalPosition == fishPosition &&
                fish.Velocity == fishVelocity && camera.GlobalTransform == cameraTransform,
                "depth presentation changed fish or camera identity/state");
            GD.Print($"DEPTH_CHECK_OK surface={surfaceY:F2} deep={deepY:F2} base_visibility={atmosphere.EffectiveBaseVisibilityRange:F1} " +
                $"shallow=(end:{shallow.Range:F1},volume:{shallow.Volume:F3},t8:{UnderwaterAtmosphere.EstimateDistanceRetention(8f, shallow.Range):F2},t15:{UnderwaterAtmosphere.EstimateDistanceRetention(15f, shallow.Range):F2},t25:{UnderwaterAtmosphere.EstimateDistanceRetention(25f, shallow.Range):F2}) " +
                $"middle=(end:{middle.Range:F1},volume:{middle.Volume:F3},t8:{UnderwaterAtmosphere.EstimateDistanceRetention(8f, middle.Range):F2},t15:{UnderwaterAtmosphere.EstimateDistanceRetention(15f, middle.Range):F2},t25:{UnderwaterAtmosphere.EstimateDistanceRetention(25f, middle.Range):F2}) " +
                $"deep=(end:{deep.Range:F1},volume:{deep.Volume:F3},t8:{UnderwaterAtmosphere.EstimateDistanceRetention(8f, deep.Range):F2},t15:{UnderwaterAtmosphere.EstimateDistanceRetention(15f, deep.Range):F2},t25:{UnderwaterAtmosphere.EstimateDistanceRetention(25f, deep.Range):F2}) presets=isolated state=unchanged");
        }
        finally { if (File.Exists(file)) File.Delete(file); }
    }

    private static Sample At(float y, UnderwaterAtmosphere atmosphere, Camera3D camera,
        Godot.Environment env, DirectionalLight3D sun)
    {
        var position = camera.GlobalPosition;
        camera.GlobalPosition = new Vector3(position.X, y, position.Z);
        atmosphere.RefreshDepth(true);
        Require(Mathf.Abs(env.FogDepthEnd - atmosphere.EffectiveUsefulRange) < 0.001f,
            "reported useful range diverged from engine fog end");
        return new Sample(atmosphere.EffectiveDepth01, env.FogDensity, env.VolumetricFogDensity,
            atmosphere.EffectiveUsefulRange, sun.LightEnergy, env.AmbientLightEnergy,
            env.FogLightColor, env.BackgroundColor, env.AmbientLightColor);
    }

    private static void Bounded(Sample sample)
    {
        Require(float.IsFinite(sample.Depth) && sample.Depth is >= 0 and <= 1 &&
            float.IsFinite(sample.Fog) && sample.Fog is >= 0 and <= 1 &&
            float.IsFinite(sample.Volume) && sample.Volume is >= 0 and <= 0.06f &&
            float.IsFinite(sample.Range) && sample.Range is > 5 and <= 60,
            "effective distance fog left finite bounds");
        foreach (var color in new[] { sample.FogColor, sample.Background, sample.AmbientColor })
            Require(color.R is >= 0 and <= 1 && color.G is >= 0 and <= 1 &&
                color.B is >= 0 and <= 1, "effective water color left gamut");
    }

    private static void Require(bool condition, string reason)
    {
        if (!condition) throw new Exception(reason);
    }

    private static void CheckPresetCompatibility(string file, LightingSettings original)
    {
        Require(Mathf.Abs(LightingSettings.VisibilityFromLegacyDensity(0.0035f) - 35f) < 0.001f &&
            LightingSettings.VisibilityFromLegacyDensity(0f) == 60f &&
            LightingSettings.VisibilityFromLegacyDensity(0.02f) == 18f,
            "legacy conversion endpoints changed");
        var seed = new LightingPresetStore(file);
        var legacy = original.Copy();
        legacy.DistanceFogDensity = 0.005f;
        legacy.CausticEnergy = 0.51f;
        legacy.SurfaceWaveSpeed = 1.42f;
        legacy.ShaftCount = 3;
        var first = seed.SaveAs("Old reef", legacy);
        var second = seed.SaveAs("Old deep", original);
        seed.Choose(first.Id);
        var v1 = JsonNode.Parse(File.ReadAllText(file))!;
        v1["schemaVersion"] = 1;
        foreach (var item in v1["presets"]!.AsArray())
            item!["settings"]!.AsObject().Remove("visibilityDistanceMeters");
        // Older valid records may rely on the former default density.
        foreach (var item in v1["presets"]!.AsArray())
            if ((string)item!["id"]! == second.Id)
                item["settings"]!.AsObject().Remove("distanceFogDensity");
        File.WriteAllText(file, v1.ToJsonString());
        var loaded = new LightingPresetStore(file);
        loaded.Load();
        Require(loaded.CurrentId == first.Id && loaded.List().Count == 4 &&
            loaded.Current.Name == "Old reef" &&
            loaded.List().Any(p => p.Id == second.Id &&
                Mathf.Abs(p.Settings.VisibilityDistanceMeters - 35f) < 0.001f) &&
            Mathf.Abs(loaded.Current.Settings.VisibilityDistanceMeters -
                LightingSettings.VisibilityFromLegacyDensity(0.005f)) < 0.0001f &&
            Mathf.Abs(loaded.Current.Settings.DistanceFogDensity - 0.005f) < 0.00001f &&
            Mathf.Abs(loaded.Current.Settings.CausticEnergy - 0.51f) < 0.0001f &&
            Mathf.Abs(loaded.Current.Settings.SurfaceWaveSpeed - 1.42f) < 0.0001f &&
            loaded.Current.Settings.ShaftCount == 3, "valid v1 custom presets were not migrated intact");
        loaded.Save();
        var migrated = JsonNode.Parse(File.ReadAllText(file))!;
        Require((int)migrated["schemaVersion"]! == 2 &&
            migrated["presets"]![0]!["settings"]!["visibilityDistanceMeters"] is not null,
            "migration did not persist explicit meter visibility");
        loaded.RenameCurrent("Reef clarity");
        var changed = loaded.Current.Settings.Copy();
        changed.VisibilityDistanceMeters = 44f;
        loaded.UpdateCurrent(changed);
        var roundTrip = new LightingPresetStore(file);
        roundTrip.Load();
        Require(roundTrip.CurrentId == first.Id && roundTrip.Current.Name == "Reef clarity" &&
            Mathf.Abs(roundTrip.Current.Settings.VisibilityDistanceMeters - 44f) < 0.001f &&
            Mathf.Abs(roundTrip.Current.Settings.DistanceFogDensity - 0.005f) < 0.00001f,
            "v2 rename/save/restart lost visibility or legacy metadata");
        roundTrip.Choose(second.Id);
        roundTrip.DeleteCurrent();
        Require(roundTrip.CurrentId == LightingPresetStore.DefaultId && roundTrip.List().Count == 3,
            "choose/delete lifecycle changed");
        var invalid = changed.Copy();
        invalid.VisibilityDistanceMeters = 61f;
        Require(!invalid.IsValid(), "out-of-range meter setting accepted");
        invalid.VisibilityDistanceMeters = float.NaN;
        Require(!invalid.IsValid(), "nonfinite meter setting accepted");
        var raw = JsonNode.Parse(File.ReadAllText(file))!;
        raw["presets"]![0]!["settings"]!["visibilityDistanceMeters"] = 61;
        File.WriteAllText(file, raw.ToJsonString());
        var rejected = new LightingPresetStore(file);
        rejected.Load();
        Require(rejected.CurrentId == LightingPresetStore.DefaultId && rejected.List().Count == 2 &&
            Directory.GetFiles(Path.GetDirectoryName(file)!, Path.GetFileName(file) + ".corrupt-*.json").Length > 0,
            "out-of-range fixture did not preserve corrupt file and recover");
        File.WriteAllText(file, "{malformed");
        rejected.Load();
        Require(rejected.List().Count == 2 && rejected.CurrentId == LightingPresetStore.DefaultId,
            "malformed fixture did not recover");
        GD.Print("LIGHTING_PRESET_CHECK_OK v1=migrated v2=round-trip invalid=recovered");
    }
}
