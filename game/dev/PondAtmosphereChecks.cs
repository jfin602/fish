#nullable enable
using Fish;
using Godot;
using System;
using System.IO;
using System.Linq;

public static class PondAtmosphereChecks
{
    public static void Run(Main main)
    {
        var tree = main.GetTree();
        var environmentClock = main.GetNode<EnvironmentClock>("EnvironmentClock");
        var camera = main.GetNode<Camera3D>("Fish/CameraRig/Camera3D");
        var fish = main.GetNode<FishController>("Fish");
        var fishPosition = fish.GlobalPosition;
        var fishVelocity = fish.Velocity;
        var cameraTransform = camera.GlobalTransform;
        var oceanFile = ProjectSettings.GlobalizePath($"user://p5-ocean-{Guid.NewGuid():N}.json");
        var pondFile = ProjectSettings.GlobalizePath($"user://p5-pond-{Guid.NewGuid():N}.json");
        try
        {
            var oceanStore = new LightingPresetStore(oceanFile);
            var pondStore = new LightingPresetStore(pondFile, WorldEnvironmentProfile.Pond);
            Require(oceanStore.CurrentId == LightingPresetStore.DefaultId &&
                pondStore.CurrentId == LightingPresetStore.PondCloudyId &&
                oceanStore.Current.Settings.VisibilityDistanceMeters == 35f &&
                pondStore.Current.Settings.VisibilityDistanceMeters == 18f,
                "world lighting bases are not independent");
            var brighter = pondStore.List();
            var shallowOption = brighter.Single(p => p.Id == LightingPresetStore.PondShallowsId);
            Require(brighter.Count == 2 &&
                shallowOption.Settings.SunEnergy > pondStore.Current.Settings.SunEnergy &&
                shallowOption.Settings.VisibilityDistanceMeters > pondStore.Current.Settings.VisibilityDistanceMeters,
                "brighter Pond option missing");
            var custom = pondStore.Current.Settings.Copy();
            custom.SunEnergy = 0.9f;
            pondStore.SaveAs("Pond test", custom);
            var pondReload = new LightingPresetStore(pondFile, WorldEnvironmentProfile.Pond);
            pondReload.Load();
            Require(pondReload.Current.Settings.SunEnergy == 0.9f &&
                oceanStore.Current.Settings.SunEnergy == 1.25f, "Pond preset leaked into Ocean");
            pondStore.Choose(LightingPresetStore.PondCloudyId);

            // No freshwater player artwork is installed until P8. The visual fixture
            // loads the actual baked habitat without creating a production fish.
            LoadWorld(main, WorldCatalog.PondId);
            var atmosphere = main.GetNode<UnderwaterAtmosphere>("UnderwaterAtmosphere");
            tree.Paused = true;
            atmosphere.ConfigureProfile(WorldEnvironmentProfile.Pond);
            atmosphere.Initialize(main);
            Require(atmosphere.EnvironmentTime == environmentClock &&
                atmosphere.Solar == environmentClock.Solar,
                "Pond presentation did not bind shared clock");
            atmosphere.ApplyLighting(pondStore.Current.Settings);
            CheckPond(main, atmosphere, camera);
            CheckSpatialField(main, atmosphere, camera);
#if DEBUG
            var firstNoon = CheckSolarWater(main, atmosphere, environmentClock, 12);
            var firstNight = CheckSolarWater(main, atmosphere, environmentClock, 0);
            Require(firstNoon.Surface > firstNight.Surface &&
                firstNoon.Shaft > firstNight.Shaft && firstNoon.Caustic > firstNight.Caustic,
                "Pond water did not lose sunlight at night");
            CheckSolarWater(main, atmosphere, environmentClock, 12);
#endif
            var pausedRange = atmosphere.EffectiveUsefulRange;
            var uiLayer = new CanvasLayer { Layer = 10 };
            main.AddChild(uiLayer);
            var panel = new GraphicsPanel(pondStore, () => atmosphere, () => { });
            uiLayer.AddChild(panel);
            panel.Open();
            Require(panel.Visible && main.GetViewport().GuiGetFocusOwner()?.Name == "LightingPresetChoice" &&
                atmosphere.ProfileId == WorldEnvironmentProfile.Pond &&
                Mathf.Abs(atmosphere.EffectiveUsefulRange - pausedRange) < 0.001f &&
                panel.Material is null, "paused Pond graphics focus changed water or UI material");
            panel.RequestClose();
            main.RemoveChild(uiLayer);
            uiLayer.Free();
            tree.Paused = false;

            LoadWorld(main, WorldCatalog.OceanId);
            atmosphere = ReplaceAtmosphere(main);
            atmosphere.ConfigureProfile(WorldEnvironmentProfile.Ocean);
            atmosphere.Initialize(main);
            Require(atmosphere.EnvironmentTime == environmentClock &&
                atmosphere.Solar == environmentClock.Solar,
                "Ocean presentation did not retain shared clock");
            atmosphere.ApplyLighting(oceanStore.Current.Settings);
            Require(atmosphere.ProfileId == WorldEnvironmentProfile.Ocean &&
                main.GetNodeOrNull<FogVolume>("UnderwaterAtmosphere/PondTurbidity") is null &&
                atmosphere.BackgroundWater == WaterVisualProfile.Ocean.Background &&
                WaterVisualProfile.Ocean.SurfaceExtinctionPerMeter == 0f &&
                atmosphere.DistanceFogColor == WaterVisualProfile.Ocean.Fog &&
                Mathf.Abs(atmosphere.WaterEnvironment!.FogDensity - .96f) < .001f &&
                Mathf.Abs(atmosphere.WaterEnvironment!.FogDepthBegin - 5f) < .001f &&
                atmosphere.CaptureLighting().VisibilityDistanceMeters == 35f &&
                main.GetNode<WorldEnvironment>("WorldEnvironment").Environment == atmosphere.WaterEnvironment,
                "Ocean profile did not reset after Pond disposal");
            var oceanSky = (ShaderMaterial)atmosphere.WaterEnvironment!.Sky.SkyMaterial;
            var oceanSurface = (ShaderMaterial)main.GetNode<MeshInstance3D>(
                "UnderwaterAtmosphere/WaterEffects/OverheadSurface").MaterialOverride;
            Require((float)oceanSky.GetShaderParameter("overcast") == 0f &&
                (float)oceanSurface.GetShaderParameter("overcast") == 0f &&
                (float)oceanSurface.GetShaderParameter("surface_extinction") == 0f &&
                (float)oceanSurface.GetShaderParameter("reflection_strength") ==
                    WaterVisualProfile.Ocean.SurfaceReflectionStrength &&
                (float)oceanSurface.GetShaderParameter("air_surface_enabled") == 1f,
                "Pond material parameters leaked into Ocean");
#if DEBUG
            var oceanNoon = CheckSolarWater(main, atmosphere, environmentClock, 12);
            var oceanNight = CheckSolarWater(main, atmosphere, environmentClock, 0);
            Require(oceanNoon.Surface > oceanNight.Surface &&
                oceanNoon.Horizon != firstNoon.Horizon &&
                oceanNoon.Daylight != firstNoon.Daylight,
                "Ocean water kept Pond solar response");
            CheckSolarWater(main, atmosphere, environmentClock, 12);
#endif

            LoadWorld(main, WorldCatalog.PondId);
            atmosphere = ReplaceAtmosphere(main);
            atmosphere.ConfigureProfile(WorldEnvironmentProfile.Pond);
            atmosphere.Initialize(main);
            Require(atmosphere.EnvironmentTime == environmentClock &&
                atmosphere.Solar == environmentClock.Solar,
                "Pond reload did not retain shared clock");
            atmosphere.ApplyLighting(pondStore.Current.Settings);
            CheckPond(main, atmosphere, camera);
            CheckSpatialField(main, atmosphere, camera);
#if DEBUG
            var returnNoon = CheckSolarWater(main, atmosphere, environmentClock, 12);
            var returnNight = CheckSolarWater(main, atmosphere, environmentClock, 0);
            Require(returnNoon == firstNoon && returnNight == firstNight,
                "Pond solar water response changed across Pond-Ocean-Pond");
#endif
            Require(fish.GlobalPosition == fishPosition && fish.Velocity == fishVelocity &&
                camera.GetInstanceId() != 0, "presentation changed fish state");
            GD.Print($"POND_ATMOSPHERE_CHECK_OK profile=pond ocean_reset=ok pond_reload=ok " +
                $"base={atmosphere.EffectiveBaseVisibilityRange:F1} deep={atmosphere.EffectiveUsefulRange:F1} " +
                $"fish=unchanged presets=isolated");
        }
        finally
        {
            tree.Paused = false;
            camera.GlobalTransform = cameraTransform;
            foreach (var file in new[] { oceanFile, pondFile })
                if (File.Exists(file)) File.Delete(file);
        }
    }

#if DEBUG
    private readonly record struct SolarWaterSnapshot(float Surface, float Daylight,
        Vector3 Horizon, float Shaft, float Caustic);

    private static SolarWaterSnapshot CheckSolarWater(Main main, UnderwaterAtmosphere atmosphere,
        EnvironmentClock clock, double hour)
    {
        clock.SetTimeForDebug(hour);
        atmosphere.RefreshSolar(true);
        atmosphere.RefreshDepth(true, 2f);
        var solar = clock.Solar;
        var expectedLight = SolarLightingState.From(solar, atmosphere.ProfileId);
        var expectedWater = SolarWaterState.From(solar, expectedLight,
            WaterVisualProfile.For(atmosphere.ProfileId), atmosphere.SunEnergy);
        var surface = (ShaderMaterial)main.GetNode<MeshInstance3D>(
            "UnderwaterAtmosphere/WaterEffects/OverheadSurface").MaterialOverride;
        var shaft = (ShaderMaterial)main.GetNode<MeshInstance3D>(
            "UnderwaterAtmosphere/WaterEffects/SunShafts").MaterialOverride;
        var decal = main.GetNode<Decal>("UnderwaterAtmosphere/WaterEffects/CausticProjection1");
        var sun = main.GetNode<DirectionalLight3D>("Sun");
        var horizon = (Vector3)surface.GetShaderParameter("reflected_sky_horizon");
        var daylight = (float)surface.GetShaderParameter("surface_daylight");
        var illumination = (float)surface.GetShaderParameter("surface_illumination");
        var shaftEnergy = (float)shaft.GetShaderParameter("shaft_brightness");
        Require(horizon.DistanceTo(new Vector3(expectedLight.AirHorizon.R,
                expectedLight.AirHorizon.G, expectedLight.AirHorizon.B)) < .0001f &&
            Mathf.Abs(daylight - expectedWater.SurfaceDaylight) < .0001f &&
            Mathf.Abs(illumination - expectedWater.SurfaceIllumination) < .0001f &&
            Mathf.Abs(shaftEnergy - atmosphere.ShaftBrightness * atmosphere.CameraImmersion01 *
                expectedWater.ShaftEnergy) < .0001f &&
            Mathf.Abs(decal.EmissionEnergy - atmosphere.CausticEnergy * atmosphere.CameraImmersion01 *
                expectedWater.CausticEnergy) < .0001f,
            "profile solar water values diverged from the shared clock");
        Require(sun.ShadowEnabled == (atmosphere.ProfileId == WorldEnvironmentProfile.Pond &&
                solar.DirectLight > .08f) &&
            (atmosphere.ProfileId != WorldEnvironmentProfile.Pond ||
                sun.DirectionalShadowMode == DirectionalLight3D.ShadowMode.Orthogonal),
            "Pond-only solar shadows did not follow the shared clock");
        if (atmosphere.ProfileId == WorldEnvironmentProfile.Pond)
        {
            var basin = (ShaderMaterial)main.GetNode<MeshInstance3D>("Habitat/Seabed/Mesh").MaterialOverride;
            Require(Mathf.Abs((float)basin.GetShaderParameter("sunlight_energy") -
                    expectedLight.DirectEnergyFactor) < .0001f &&
                ((Vector3)basin.GetShaderParameter("sun_direction")).DistanceTo(
                    sun.GlobalTransform.Basis.Z.Normalized()) < .0001f,
                "Pond basin light pools diverged from the shared sun");
        }
        return new SolarWaterSnapshot(illumination, daylight, horizon, shaftEnergy, decal.EmissionEnergy);
    }
#endif

    private static void LoadWorld(Main main, string id)
    {
        WorldSession.Select(id);
        var old = main.GetNode<Node3D>("Habitat");
        main.RemoveChild(old);
        old.Free();
        var habitat = WorldCatalog.Resolve(id).LoadHabitatScene().Instantiate<Node3D>();
        habitat.Name = "Habitat";
        main.AddChild(habitat);
        WorldSession.Bind(habitat, WorldCatalog.Load(id, habitat));
    }

    private static UnderwaterAtmosphere ReplaceAtmosphere(Main main)
    {
        var old = main.GetNode<UnderwaterAtmosphere>("UnderwaterAtmosphere");
        main.RemoveChild(old);
        old.Free();
        Require(main.GetNode<WorldEnvironment>("WorldEnvironment").Environment is null,
            "disposed atmosphere kept a world environment");
        var next = new UnderwaterAtmosphere { Name = "UnderwaterAtmosphere" };
        main.AddChild(next);
        return next;
    }

    private static void CheckPond(Main main, UnderwaterAtmosphere atmosphere, Camera3D camera)
    {
        var env = atmosphere.WaterEnvironment ?? throw new Exception("Pond environment missing");
        var surface = main.GetNode<MeshInstance3D>("UnderwaterAtmosphere/WaterEffects/OverheadSurface");
        var surfaceMaterial = (ShaderMaterial)surface.MaterialOverride;
        var sky = (ShaderMaterial)env.Sky.SkyMaterial;
        var shafts = main.GetNode<MeshInstance3D>("UnderwaterAtmosphere/WaterEffects/SunShafts");
        var shaftMaterial = (ShaderMaterial)shafts.MaterialOverride;
        var screen = atmosphere.ScreenLayer ?? throw new Exception("water screen missing");
        var rect = screen.GetNode<ColorRect>("ScreenTreatment");
        var original = camera.GlobalPosition;
        camera.GlobalPosition = new Vector3(original.X, atmosphere.SurfaceHeight - 2f, original.Z);
        atmosphere.RefreshDepth(true, 2f);
        Require(atmosphere.ProfileId == WorldEnvironmentProfile.Pond &&
            atmosphere.ParametersAreFinite() && env.FogEnabled && env.VolumetricFogEnabled &&
            (float)surfaceMaterial.GetShaderParameter("surface_extinction") ==
                WaterVisualProfile.Pond.SurfaceExtinctionPerMeter &&
            (float)surfaceMaterial.GetShaderParameter("reflection_strength") ==
                WaterVisualProfile.Pond.SurfaceReflectionStrength &&
            (float)surfaceMaterial.GetShaderParameter("reflection_distortion") ==
                WaterVisualProfile.Pond.SurfaceReflectionDistortion &&
            Mathf.Abs((float)surfaceMaterial.GetShaderParameter("surface_daylight") -
                Mathf.Clamp(atmosphere.SunEnergy / 1.35f, 0.6f, 1.3f)) < .001f &&
            atmosphere.BackgroundWater == WaterVisualProfile.Pond.Background &&
            Mathf.Abs(env.FogLightColor.R - WaterVisualProfile.Pond.Fog.Lerp(
                WaterVisualProfile.Pond.DeepFog, atmosphere.EffectiveDepth01).R) < .001f &&
            (float)sky.GetShaderParameter("overcast") == 1f &&
            (float)surfaceMaterial.GetShaderParameter("overcast") == 1f &&
            surface.CastShadow == GeometryInstance3D.ShadowCastingSetting.Off &&
            (Vector3)shaftMaterial.GetShaderParameter("shaft_color") == new Vector3(
                WaterVisualProfile.Pond.ShaftColor.R, WaterVisualProfile.Pond.ShaftColor.G,
                WaterVisualProfile.Pond.ShaftColor.B), "Pond material/palette switch failed");
        Require(screen.Layer == 0 && rect.MouseFilter == Control.MouseFilterEnum.Ignore &&
            rect.Material is ShaderMaterial &&
            main.GetNode<CanvasLayer>("PrototypeHelp").Layer > screen.Layer,
            "camera treatment leaked into UI layer");
        var x = camera.GlobalPosition.X; var z = camera.GlobalPosition.Z;
        var surfaceY = atmosphere.SurfaceHeight;
        var deepY = atmosphere.DeepReferenceHeight;
        float At(float y)
        {
            camera.GlobalPosition = new Vector3(x, y, z);
            atmosphere.RefreshDepth(true);
            return env.FogDepthEnd;
        }
        var shallow = At(surfaceY);
        var middle = At((surfaceY + deepY) * 0.5f);
        var deep = At(deepY);
        var extinction = env.FogDensity;
        Require(shallow > middle && middle > deep && deep > env.FogDepthBegin + 7f &&
            Mathf.Abs(env.FogDepthBegin - 4.5f) < .001f &&
            extinction > .77f && extinction < .79f &&
            UnderwaterAtmosphere.EstimateDistanceRetention(5f, deep, extinction, env.FogDepthBegin) > .85f &&
            UnderwaterAtmosphere.EstimateDistanceRetention(12f, shallow, extinction, env.FogDepthBegin) >
                UnderwaterAtmosphere.EstimateDistanceRetention(12f, deep, extinction, env.FogDepthBegin) &&
            UnderwaterAtmosphere.EstimateDistanceRetention(12f, deep, extinction, env.FogDepthBegin) < .5f,
            "Pond depth/distance extinction lost near-to-far progression");
        Require(Mathf.Abs(At(surfaceY + 0.001f) - At(surfaceY - 0.001f)) < 0.01f &&
            Mathf.Abs(At(deepY + 0.001f) - At(deepY - 0.001f)) < 0.01f,
            "Pond depth endpoint is discontinuous");
        At(deepY);
    }

    private static void CheckSpatialField(Main main, UnderwaterAtmosphere atmosphere, Camera3D camera)
    {
        var basin = main.GetNode<SeabedSurface>("Habitat/Seabed").Basin;
        var a = new PondTurbidityField(basin, atmosphere.SurfaceHeight);
        var b = new PondTurbidityField(basin, atmosphere.SurfaceHeight);
        Require(a.Center == b.Center && a.Size == b.Size && a.Size.X < 200f &&
            a.Size.Z < 200f && a.Size.Y < 30f, "Pond turbidity bounds changed");
        var minimum = 1f;
        var maximum = 0f;
        var count = 0;
        var sites = new System.Collections.Generic.List<(float Depth, float Density, Vector2 Position)>();
        for (var z = -55; z <= 55; z += 5)
        for (var x = -55; x <= 55; x += 5)
        {
            if (basin.DistanceToShore(x, z) < 5f ||
                atmosphere.SurfaceHeight - basin.HeightAt(x, z) < 4f) continue;
            var point = new Vector3(x, atmosphere.SurfaceHeight - 2f, z);
            var sample = a.Sample(point);
            Require(float.IsFinite(sample) && sample >= 0f && sample <= .95f &&
                Mathf.Abs(sample - b.Sample(point)) < .000001f &&
                Mathf.Abs(sample - a.Sample(point + new Vector3(.25f, 0, 0))) < .08f &&
                Mathf.Abs(sample - a.Sample(point + new Vector3(0, 0, .25f))) < .08f,
                "Pond turbidity is unstable, unbounded, or discontinuous");
            minimum = Mathf.Min(minimum, sample);
            maximum = Mathf.Max(maximum, sample);
            sites.Add((atmosphere.SurfaceHeight - basin.HeightAt(x, z), sample, new Vector2(x, z)));
            count++;
        }
        Require(count > 100 && maximum - minimum > .20f,
            $"Pond spatial variation too weak: count={count} min={minimum:F3} max={maximum:F3}");
        var sameDepthGap = 0f;
        foreach (var first in sites)
        foreach (var second in sites)
        {
            if (Mathf.Abs(first.Depth - second.Depth) > .5f ||
                first.Position.DistanceTo(second.Position) < 12f) continue;
            sameDepthGap = Mathf.Max(sameDepthGap, Mathf.Abs(first.Density - second.Density));
        }
        Require(sameDepthGap > .4f,
            $"Pond clarity lacks comparable-depth clear and silty sites: gap={sameDepthGap:F3}");
        var core = basin.DeepCore;
        var coreBed = basin.HeightAt(core.X, core.Y);
        var nearBed = a.Sample(new Vector3(core.X, coreBed + .6f, core.Y));
        var aboveBed = a.Sample(new Vector3(core.X, coreBed + 3f, core.Y));
        Require(nearBed > aboveBed + .06f,
            "Pond mud proximity did not raise sediment independently of illumination");
        var volume = main.GetNode<FogVolume>("UnderwaterAtmosphere/PondTurbidity");
        var material = (FogMaterial)volume.Material;
        var fixedPosition = volume.GlobalPosition;
        camera.GlobalPosition = new Vector3(0, atmosphere.SurfaceHeight - 2f, 0);
        atmosphere.RefreshDepth(true, 2f);
        Require(material.Density > .04f && material.DensityTexture is ImageTexture3D,
            "Pond fog field did not reach Forward+ rendering");
        camera.GlobalPosition += new Vector3(8f, 0, 5f);
        atmosphere.RefreshDepth(true, 2f);
        Require(volume.GlobalPosition == fixedPosition, "camera movement shifted Pond turbidity");
        atmosphere.SetPondClarityDiagnosticEnabled(false);
        Require(material.Density == 0f && volume.GlobalPosition == fixedPosition,
            "Pond diagnostic off moved or retained fog");
        atmosphere.SetPondClarityDiagnosticEnabled(true);
        Require(material.Density > .04f, "Pond diagnostic on did not restore density");
        GD.Print($"POND_TURBIDITY_CHECK_OK samples={count} min={minimum:F3} max={maximum:F3} same_depth_gap={sameDepthGap:F3} texture=64x16x64 camera_anchor=fixed");
    }

    private static void Require(bool condition, string reason)
    {
        if (!condition) throw new Exception(reason);
    }
}
