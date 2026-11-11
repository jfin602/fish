using Godot;
using System;
using System.Linq;

namespace Fish;

public partial class Main : Node3D
{
    private DisplayPreferences _preferences = null!;
    private bool _startupInvalid;

    public override void _EnterTree()
    {
        _preferences = new DisplayPreferences();
        _preferences.Load();
        var requested = _preferences.SavedDisplayMode;
        var source = "saved-or-default";
        foreach (var argument in OS.GetCmdlineUserArgs())
        {
            if (!argument.StartsWith("--display-mode=", StringComparison.Ordinal)) continue;
            var value = argument["--display-mode=".Length..];
            if (!DisplayPreferences.IsSupportedDisplayMode(value))
            {
                GD.PushError($"Unsupported display mode: {value}");
                _startupInvalid = true;
                GetTree().Quit(2);
                return;
            }
            requested = value;
            source = "launch-override";
        }
        _preferences.ActiveDisplayMode = requested;
        GetWindow().Mode = DisplayPreferences.ToWindowMode(requested);
        GD.Print($"FISH_DISPLAY_MODE requested={requested} source={source} effective={GetWindow().Mode}");
    }

    public override void _Ready()
    {
        if (_startupInvalid) return;
        if (OS.GetCmdlineUserArgs().Contains("--pond-atmosphere-check"))
        {
            try { PondAtmosphereChecks.Run(this); GetTree().Quit(); }
            catch (Exception error) { GD.PushError($"POND_ATMOSPHERE_CHECK_FAILED {error}"); GetTree().Quit(1); }
            return;
        }
        if (OS.GetCmdlineUserArgs().Contains("--species-check"))
        {
            SpeciesCompatibilityChecks.Run(this);
            return;
        }
        if (OS.GetCmdlineUserArgs().Contains("--world-catalog-check"))
        {
            try { WorldCatalogChecks.Run(); GetTree().Quit(); }
            catch (Exception error) { GD.PushError($"WORLD_CATALOG_CHECK_FAILED {error}"); GetTree().Quit(1); }
            return;
        }
        foreach (var argument in OS.GetCmdlineUserArgs())
        {
            if (!argument.StartsWith("--compile-world-fixture=", StringComparison.Ordinal)) continue;
            var seedText = argument["--compile-world-fixture=".Length..];
            if (!ulong.TryParse(seedText, out var seed))
            {
                GD.PushError($"WORLD_COMPILE_FAILED invalid seed: {seedText}");
                GetTree().Quit(2);
                return;
            }
            try
            {
                var folder = System.IO.Path.Combine(WorldCompiler.CandidateRoot, $"fixture-{seed}");
                var manifest = WorldCompiler.CompileFixture(new WorldGenerationSettings(seed), folder,
                    FafRuntimeCatalog.Environment,
                    p => GD.Print($"WORLD_COMPILE_STAGE {p.Stage} {p.Completed}/{p.Total} elapsed_ms={p.ElapsedMilliseconds}"));
                GD.Print($"WORLD_COMPILE_OK id={manifest.WorldId} manifest={System.IO.Path.Combine(folder, BakedWorldContract.ManifestFileName)}");
                GetTree().Quit();
            }
            catch (Exception error) { GD.PushError($"WORLD_COMPILE_FAILED {error}"); GetTree().Quit(1); }
            return;
        }
        if (OS.GetCmdlineUserArgs().Contains("--world-compiler-check"))
        {
            try { WorldCompilerChecks.Run(); GetTree().Quit(); }
            catch (Exception error) { GD.PushError($"WORLD_COMPILER_CHECK_FAILED {error}"); GetTree().Quit(1); }
            return;
        }
        if (OS.GetCmdlineUserArgs().Contains("--shore-field-check"))
        {
            try { BasinChecks.RunStructural(); GetTree().Quit(); }
            catch (Exception error) { GD.PushError($"SHORE_FIELD_CHECK_FAILED {error}"); GetTree().Quit(1); }
            return;
        }
        if (OS.GetCmdlineUserArgs().Contains("--geology-site-check"))
        {
            try { GeologicalRegionChecks.RunSites(); GetTree().Quit(); }
            catch (Exception error) { GD.PushError($"GEOLOGY_SITE_CHECK_FAILED {error}"); GetTree().Quit(1); }
            return;
        }
        if (OS.GetCmdlineUserArgs().Contains("--route-anchor-check"))
        {
            try { RouteAnchorChecks.Run(); GetTree().Quit(); }
            catch (Exception error) { GD.PushError($"ROUTE_ANCHOR_CHECK_FAILED {error}"); GetTree().Quit(1); }
            return;
        }
        if (OS.GetCmdlineUserArgs().Contains("--control-preset-check"))
        {
            try { ControlPresetChecks.Run(); GetTree().Quit(); }
            catch (Exception error) { GD.PushError($"CONTROL_PRESET_CHECK_FAILED {error}"); GetTree().Quit(1); }
            return;
        }
        if (OS.GetCmdlineUserArgs().Contains("--faf-runtime-check"))
        {
            try { FafRuntimeChecks.Run(); GetTree().Quit(); }
            catch (Exception error) { GD.PushError($"FAF_RUNTIME_CHECK_FAILED {error}"); GetTree().Quit(1); }
            return;
        }
        var fish = GetNodeOrNull<FishController>("Fish");
        var camera = GetNodeOrNull<Camera3D>("Fish/CameraRig/Camera3D");
        var habitat = GetNodeOrNull<Node3D>("Habitat");
        var isCourseFixture = habitat?.GetNodeOrNull<Node3D>("MovementCourse") is not null;
        if (fish is null || camera is null || habitat is null || !camera.Current ||
            (!isCourseFixture && habitat.GetNodeOrNull<SeabedSurface>("Seabed") is null))
        {
            GD.PushError("FISH_SMOKE_FAILED required 3D scene nodes missing");
            if (OS.GetCmdlineUserArgs().Contains("--smoke")) GetTree().Quit(1);
            return;
        }

        GD.Print("FISH_FOUNDATION_READY");

        var pondInspect = OS.GetCmdlineUserArgs().FirstOrDefault(arg =>
            arg.StartsWith("--pond-inspect=", StringComparison.Ordinal));
        if (pondInspect is not null)
        {
            try { PondWorldChecks.Run(this, pondInspect["--pond-inspect=".Length..]); GetTree().Quit(); }
            catch (Exception error) { GD.PushError($"POND_WORLD_CHECK_FAILED {error}"); GetTree().Quit(1); }
            return;
        }

        AddChild(new PlayShell(fish, GetNode<Label>("PrototypeHelp/Help"), _preferences)
        {
            Name = "PlayShell",
        });

        var populated = isCourseFixture
            ? StarterWorld.TryPopulate(this, fish, out var assetDiagnostic)
            : P2HabitatWorld.TryPopulate(this, fish, out assetDiagnostic);
        if (!populated)
        {
            GD.PushError(assetDiagnostic);
            if (!isCourseFixture)
            {
                WorldSession.Unload();
                GetTree().Quit(1);
                return;
            }
            var label = new Label
            {
                Name = "AssetDiagnostic",
                Text = $"Starter art unavailable: {assetDiagnostic}\nSee game/assets/README.md.",
                Position = new Vector2(18, 82),
                CustomMinimumSize = new Vector2(720, 80),
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };
            GetNode<CanvasLayer>("PrototypeHelp").AddChild(label);
            if (OS.GetCmdlineUserArgs().Contains("--smoke"))
            {
                GetTree().Quit(1);
                return;
            }
        }

        if (populated && !isCourseFixture && GetParent() is not RendererComparison)
        {
            var profile = WorldCatalog.Resolve(WorldSession.SelectedWorldId).EnvironmentProfile;
            GetNode<PlayShell>("PlayShell").ConfigureLighting(profile);
            GetNode<UnderwaterAtmosphere>("UnderwaterAtmosphere").ConfigureProfile(profile);
            GetNode<UnderwaterAtmosphere>("UnderwaterAtmosphere").Initialize(this);
            GetNode<PlayShell>("PlayShell").ApplySavedLighting();
            fish.AddChild(new WaterReaction(fish));
        }

        if (OS.GetCmdlineUserArgs().Any(arg => arg.StartsWith("--world-generate=", StringComparison.Ordinal) ||
            arg.StartsWith("--pond-generate=", StringComparison.Ordinal) ||
            arg == "--world-inspect"))
        {
            GetTree().Quit(populated ? 0 : 1);
            return;
        }

        if (OS.GetCmdlineUserArgs().Contains("--smoke"))
            CallDeferred(MethodName.VerifySmoke);
        if (OS.GetCmdlineUserArgs().Contains("--depth-check"))
            CallDeferred(MethodName.VerifyDepth);
        if (OS.GetCmdlineUserArgs().Contains("--basin-check"))
            CallDeferred(MethodName.VerifyBasin);
        if (OS.GetCmdlineUserArgs().Contains("--geology-mesh-check"))
            CallDeferred(MethodName.VerifyGeologyMesh);
        if (OS.GetCmdlineUserArgs().Contains("--route-field-check"))
            CallDeferred(MethodName.VerifyRouteFields);
        if (OS.GetCmdlineUserArgs().Contains("--placement-check"))
            CallDeferred(MethodName.VerifyPlacement);
    }

    public override void _ExitTree() => WorldSession.Unload();

    private void VerifyBasin()
    {
        try { BasinChecks.Run(this); GetTree().Quit(); }
        catch (Exception error) { GD.PushError($"BASIN_CHECK_FAILED {error}"); GetTree().Quit(1); }
    }

    private void VerifyGeologyMesh()
    {
        try { GeologicalRegionChecks.Run(this); GetTree().Quit(); }
        catch (Exception error) { GD.PushError($"GEOLOGY_MESH_FAILED {error}"); GetTree().Quit(1); }
    }

    private void VerifyRouteFields()
    {
        try { GeneratedRouteChecks.Run(this); GetTree().Quit(); }
        catch (Exception error) { GD.PushError($"ROUTE_FIELD_CHECK_FAILED {error}"); GetTree().Quit(1); }
    }

    private void VerifyPlacement()
    {
        try { PlacementChecks.Run(this); GetTree().Quit(); }
        catch (Exception error) { GD.PushError($"WORLD_PLACEMENT_CHECK_FAILED {error}"); GetTree().Quit(1); }
    }

    private void VerifyDepth()
    {
        try { WaterColumnChecks.Run(this); GetTree().Quit(); }
        catch (Exception error) { GD.PushError($"DEPTH_CHECK_FAILED {error}"); GetTree().Quit(1); }
    }

    private void VerifySmoke()
    {
        var fish = GetNode<FishController>("Fish");
        var spawn = GetNodeOrNull<Marker3D>("Habitat/Spawn");
        if (!fish.IsInsideTree() || !fish.GlobalPosition.IsFinite() || spawn is null ||
            fish.GlobalPosition.DistanceTo(spawn.GlobalPosition) > 1.5f)
        {
            GD.PushError("FISH_SMOKE_FAILED C# fish was not initialized within habitat");
            GetTree().Quit(1);
            return;
        }
        var skeleton = GetNodeOrNull<Skeleton3D>("Fish/PlayerVisual/Armature/Skeleton3D");
        var animation = GetNodeOrNull<AnimationPlayer>("Fish/PlayerVisual/AnimationPlayer");
        var course = GetNodeOrNull<Node3D>("Habitat/MovementCourse");
        var seabed = GetNodeOrNull<SeabedSurface>("Habitat/Seabed");
        var water = GetNodeOrNull<WaterReaction>("Fish/WaterReaction");
        if (fish.Tuning.ResourcePath != "res://resources/swim_tuning.tres" ||
            skeleton is null || skeleton.GetBoneCount() == 0 ||
            animation is null || !animation.HasAnimation("Armature|Swim") ||
            (course is null && (water is null || water.GetChildCount() != 3 ||
                seabed?.GetNodeOrNull<MeshInstance3D>("Mesh")?.Mesh is null ||
                seabed.GetNodeOrNull<CollisionShape3D>("Collision")?.Shape is null ||
                GetNodeOrNull<StaticBody3D>("Habitat/HiddenLimits") is null ||
                GetNodeOrNull<Node3D>("Habitat/Composition") is null)))
        {
            GD.PushError("FISH_SMOKE_FAILED world resources did not instantiate fully");
            GetTree().Quit(1);
            return;
        }
        if (course is not null)
        {
            var ambient = GetNodeOrNull<Node3D>("Habitat/AmbientFish");
            var decor = GetNodeOrNull<Node3D>("Habitat/StarterDecor");
            var floor = GetNodeOrNull<MeshInstance3D>("Habitat/Floor/Mesh");
            if (ambient?.GetChildCount() != 7 || decor?.GetChildCount() != 4 ||
                floor?.MaterialOverride is not StandardMaterial3D { AlbedoTexture: not null })
            {
                GD.PushError("FISH_SMOKE_FAILED P1 course resources did not instantiate fully");
                GetTree().Quit(1);
                return;
            }
        }
        try { MotorChecks.Run(fish.Tuning); }
        catch (Exception error)
        {
            GD.PushError($"FISH_SMOKE_FAILED motor: {error.Message}");
            GetTree().Quit(1);
            return;
        }
        GD.Print("FISH_SMOKE_OK main scene and C# scripts executed");
        GetTree().Quit();
    }
}
