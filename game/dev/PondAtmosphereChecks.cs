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
                pondStore.Current.Settings.VisibilityDistanceMeters == 25f,
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
            atmosphere.ApplyLighting(pondStore.Current.Settings);
            CheckPond(main, atmosphere, camera);
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
            atmosphere.ApplyLighting(oceanStore.Current.Settings);
            Require(atmosphere.ProfileId == WorldEnvironmentProfile.Ocean &&
                atmosphere.BackgroundWater == WaterVisualProfile.Ocean.Background &&
                WaterVisualProfile.Ocean.SurfaceExtinctionPerMeter == 0f &&
                atmosphere.DistanceFogColor == WaterVisualProfile.Ocean.Fog &&
                atmosphere.CaptureLighting().VisibilityDistanceMeters == 35f &&
                main.GetNode<WorldEnvironment>("WorldEnvironment").Environment == atmosphere.WaterEnvironment,
                "Ocean profile did not reset after Pond disposal");
            var oceanSky = (ShaderMaterial)atmosphere.WaterEnvironment!.Sky.SkyMaterial;
            var oceanSurface = (ShaderMaterial)main.GetNode<MeshInstance3D>(
                "UnderwaterAtmosphere/WaterEffects/OverheadSurface").MaterialOverride;
            Require((float)oceanSky.GetShaderParameter("overcast") == 0f &&
                oceanSky.Shader.ResourcePath == "res://shaders/underwater_sky.gdshader" &&
                oceanSurface.Shader.ResourcePath == "res://shaders/water_surface.gdshader" &&
                (float)oceanSurface.GetShaderParameter("overcast") == 0f &&
                (float)oceanSurface.GetShaderParameter("surface_extinction") == 0f &&
                (float)oceanSurface.GetShaderParameter("air_surface_enabled") == 0f,
                "Pond material parameters leaked into Ocean");

            LoadWorld(main, WorldCatalog.PondId);
            atmosphere = ReplaceAtmosphere(main);
            atmosphere.ConfigureProfile(WorldEnvironmentProfile.Pond);
            atmosphere.Initialize(main);
            atmosphere.ApplyLighting(pondStore.Current.Settings);
            CheckPond(main, atmosphere, camera);
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
            sky.Shader.ResourcePath == "res://shaders/pond_sky.gdshader" &&
            surfaceMaterial.Shader.ResourcePath == "res://shaders/pond_water_surface.gdshader" &&
            atmosphere.ParametersAreFinite() && env.FogEnabled && env.VolumetricFogEnabled &&
            (float)surfaceMaterial.GetShaderParameter("surface_extinction") ==
                WaterVisualProfile.Pond.SurfaceExtinctionPerMeter &&
            Mathf.Abs((float)surfaceMaterial.GetShaderParameter("surface_daylight") -
                Mathf.Clamp(atmosphere.SunEnergy / 1.35f, 0.6f, 1.3f)) < .001f &&
            atmosphere.BackgroundWater == WaterVisualProfile.Pond.Background &&
            Mathf.Abs(env.FogLightColor.R - WaterVisualProfile.Pond.Fog.Lerp(
                WaterVisualProfile.Pond.DeepFog, atmosphere.EffectiveDepth01).R) < .001f &&
            (float)sky.GetShaderParameter("overcast") == 1f &&
            (float)surfaceMaterial.GetShaderParameter("overcast") == 1f &&
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
        Require(shallow > middle && middle > deep && deep > env.FogDepthBegin + 8f &&
            extinction > .15f && extinction < .30f &&
            UnderwaterAtmosphere.EstimateDistanceRetention(5f, deep, extinction) > 0.95f &&
            UnderwaterAtmosphere.EstimateDistanceRetention(12f, shallow, extinction) >
                UnderwaterAtmosphere.EstimateDistanceRetention(12f, deep, extinction) &&
            UnderwaterAtmosphere.EstimateDistanceRetention(25f, deep, extinction) < .85f,
            "Pond depth/distance extinction lost near-to-far progression");
        Require(Mathf.Abs(At(surfaceY + 0.001f) - At(surfaceY - 0.001f)) < 0.01f &&
            Mathf.Abs(At(deepY + 0.001f) - At(deepY - 0.001f)) < 0.01f,
            "Pond depth endpoint is discontinuous");
        At(deepY);
    }

    private static void Require(bool condition, string reason)
    {
        if (!condition) throw new Exception(reason);
    }
}
