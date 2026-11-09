using Godot;
using System;

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
        if (OS.GetCmdlineUserArgs().Contains("--control-preset-check"))
        {
            try { ControlPresetChecks.Run(); GetTree().Quit(); }
            catch (Exception error) { GD.PushError($"CONTROL_PRESET_CHECK_FAILED {error}"); GetTree().Quit(1); }
            return;
        }
        var fish = GetNodeOrNull<FishController>("Fish");
        var camera = GetNodeOrNull<Camera3D>("Fish/CameraRig/Camera3D");
        var habitat = GetNodeOrNull<StaticBody3D>("Habitat/WestWall");
        if (fish is null || camera is null || habitat is null || !camera.Current)
        {
            GD.PushError("FISH_SMOKE_FAILED required 3D scene nodes missing");
            if (OS.GetCmdlineUserArgs().Contains("--smoke")) GetTree().Quit(1);
            return;
        }

        GD.Print("FISH_FOUNDATION_READY");

        AddChild(new PlayShell(fish, GetNode<Label>("PrototypeHelp/Help"), _preferences)
        {
            Name = "PlayShell",
        });

        if (!StarterWorld.TryPopulate(this, fish, out var assetDiagnostic))
        {
            GD.PushError(assetDiagnostic);
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

        if (OS.GetCmdlineUserArgs().Contains("--smoke"))
            CallDeferred(MethodName.VerifySmoke);
    }

    private void VerifySmoke()
    {
        var fish = GetNode<FishController>("Fish");
        if (!fish.IsInsideTree() || fish.GlobalPosition.Y < -3.0f || fish.GlobalPosition.Y > 3.0f)
        {
            GD.PushError("FISH_SMOKE_FAILED C# fish was not initialized within habitat");
            GetTree().Quit(1);
            return;
        }
        var skeleton = GetNodeOrNull<Skeleton3D>("Fish/PlayerVisual/Armature/Skeleton3D");
        var animation = GetNodeOrNull<AnimationPlayer>("Fish/PlayerVisual/AnimationPlayer");
        var ambient = GetNodeOrNull<Node3D>("Habitat/AmbientFish");
        var decor = GetNodeOrNull<Node3D>("Habitat/StarterDecor");
        var course = GetNodeOrNull<Node3D>("Habitat/MovementCourse");
        var floor = GetNode<MeshInstance3D>("Habitat/Floor/Mesh");
        if (fish.Tuning.ResourcePath != "res://resources/swim_tuning.tres" ||
            skeleton is null || skeleton.GetBoneCount() == 0 ||
            animation is null || !animation.HasAnimation("Armature|Swim") ||
            ambient is null || ambient.GetChildCount() < 3 ||
            decor is null || decor.GetChildCount() != 4 || course is null ||
            floor.MaterialOverride is not StandardMaterial3D sand || sand.AlbedoTexture is null)
        {
            GD.PushError("FISH_SMOKE_FAILED starter resources did not instantiate fully");
            GetTree().Quit(1);
            return;
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
