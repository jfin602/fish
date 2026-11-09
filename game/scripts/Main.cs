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

        AddChild(new PlayShell(fish, GetNode<Label>("PrototypeHelp/Help"), _preferences));

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
        GD.Print("FISH_SMOKE_OK main scene and C# scripts executed");
        GetTree().Quit();
    }
}
