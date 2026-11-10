using Godot;
using System;
using System.Threading.Tasks;

namespace Fish;

/// <summary>Focused loaded-world integration check for every catalog entry.</summary>
public static class CharacterChecks
{
    public static async void Run(CharacterSelect selection)
    {
        try
        {
            var root = selection.GetTree().Root;
            var originalCount = root.GetChildCount();
            await Frame(selection);
            foreach (var entry in CharacterCatalog.All)
            {
                Require(CharacterCatalog.TryInstantiate(entry, out var probe, out _, out var diagnostic),
                    $"{entry.Id} unavailable: {diagnostic}");
                probe!.Free();
                DemoSelection.CharacterId = entry.Id;
                var world = GD.Load<PackedScene>("res://scenes/main.tscn").Instantiate<Main>();
                root.AddChild(world);
                await Frame(selection);
                var fish = world.GetNode<FishController>("Fish");
                fish.SetActivePreset(ControlPreset.Builtins()[0]);
                var visual = fish.GetNodeOrNull<Node3D>("PlayerVisual");
                var camera = fish.GetNodeOrNull<Camera3D>("CameraRig/Camera3D");
                var collider = fish.GetNode<CollisionShape3D>("Collision");
                Require(visual is not null && camera is not null && camera.Current &&
                    collider.Shape is SphereShape3D shape &&
                    Mathf.Abs(shape.Radius - .48f) < .001f &&
                    visual.Scale.DistanceTo(Vector3.One * entry.WorldScale) < .001f,
                    $"{entry.Id} missing visual, camera or presentation");
                Require(world.GetNodeOrNull<Node3D>("Habitat/Composition") is not null &&
                    fish.GetNodeOrNull<WaterReaction>("WaterReaction") is not null,
                    $"{entry.Id} did not load complete P2 world");
                var anim = visual!.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
                Require(anim is not null && anim.HasAnimation(entry.SwimClip) &&
                    visual.GetNodeOrNull<Node>("SwimPresentation") is not null,
                    $"{entry.Id} animation not attached");
                await Frames(selection, 8);
                Require(anim!.IsPlaying(), $"{entry.Id} animation stopped at rest");
                SendKey(fish, Key.W, true);
                await Frames(selection, 12);
                Require(fish.PresentationEffort > .8f && fish.PresentationSpeed > .05f,
                    $"{entry.Id} could not swim effort={fish.PresentationEffort} speed={fish.PresentationSpeed} armed={fish.GetInputArmed()} held={fish.GetHeldInputCount()} preset={fish.ActivePresetId} paused={selection.GetTree().Paused}");
                SendKey(fish, Key.A, true);
                SendKey(fish, Key.Up, true);
                await Frames(selection, 6);
                Require(fish.LastYaw < 0 && fish.LastPitch > 0,
                    $"{entry.Id} could not turn/climb");
                SendKey(fish, Key.A, false);
                SendKey(fish, Key.Up, false);
                SendKey(fish, Key.D, true);
                SendKey(fish, Key.Down, true);
                await Frames(selection, 6);
                Require(fish.LastYaw > 0 && fish.LastPitch < 0,
                    $"{entry.Id} could not turn/dive");
                SendKey(fish, Key.W, false);
                SendKey(fish, Key.D, false);
                SendKey(fish, Key.Down, false);
                SendKey(fish, Key.S, true);
                await Frames(selection, 6);
                Require(fish.LastSlow > 0 && fish.PresentationEffort == 0,
                    $"{entry.Id} could not slow");
                SendKey(fish, Key.S, false);
                world.Free();
                Require(root.GetChildCount() == originalCount, $"{entry.Id} leaked world nodes");
                GD.Print($"CHARACTER_OK id={entry.Id}");
            }
            DemoSelection.Clear();
            Input.MouseMode = Input.MouseModeEnum.Visible;
            GD.Print($"CHARACTER_CHECK_OK count={CharacterCatalog.All.Count}");
            selection.GetTree().Quit();
        }
        catch (Exception error)
        {
            GD.PushError($"CHARACTER_CHECK_FAILED {error}");
            selection.GetTree().Quit(1);
        }
    }

    private static async Task Frame(Node node) => await node.ToSignal(node.GetTree(), SceneTree.SignalName.PhysicsFrame);
    private static async Task Frames(Node node, int count)
    {
        for (var i = 0; i < count; i++) await Frame(node);
    }
    private static void SendKey(FishController fish, Key key, bool pressed) =>
        fish._Input(new InputEventKey { Keycode = key, PhysicalKeycode = key, Pressed = pressed });
    private static void Require(bool valid, string message)
    {
        if (!valid) throw new Exception(message);
    }
}
