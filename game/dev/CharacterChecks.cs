using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fish;

/// <summary>Focused loaded-world integration check for every catalog entry.</summary>
public static class CharacterChecks
{
    public static async void Run(CharacterSelect selection)
    {
        var tree = selection.GetTree();
        try
        {
            var root = tree.Root;
            var originalCount = root.GetChildCount();
            await Frame(tree);
            var expectedNames = new[] { "Yellow Tang", "Ocellaris Clownfish", "Blue-Green Chromis",
                "Reef Butterflyfish", "Blacktip Reef Shark" };
            var marine = SpeciesCompatibility.Available(WorldCatalog.OceanId, SpeciesRole.Selectable);
            Require(marine.Count == 5 &&
                marine.Select(entry => entry.Id)
                    .SequenceEqual(Enumerable.Range(1, 5).Select(index => $"FAF-FISH-{index:000}")) &&
                marine.Select(entry => entry.Name).SequenceEqual(expectedNames),
                "selection catalog identity");
            selection._Input(new InputEventKey { Keycode = Key.Right, Pressed = true });
            Require((selection.FindChild("Character_1", true, false) as Button)?.Text.Contains("SELECTED") == true,
                "keyboard next did not change selection");
            selection._Input(new InputEventKey { Keycode = Key.Left, Pressed = true });
            Require((selection.FindChild("Character_0", true, false) as Button)?.Text.Contains("SELECTED") == true,
                "keyboard previous did not change selection");
            for (var index = 0; index < marine.Count; index++)
            {
                var entry = marine[index];
                var tile = selection.FindChild("Character_" + index, true, false) as Button;
                Require(tile is not null, $"{entry.Id} tile missing");
                tile!.EmitSignal(Button.SignalName.Pressed);
                var preview = selection.FindChild("PreviewModel", true, false) as Node3D;
                var previewPlayer = preview?.FindChildren("*", "AnimationPlayer", true, false)
                    .OfType<AnimationPlayer>().FirstOrDefault();
                Require(preview is not null && previewPlayer is not null &&
                    tile.Text.Contains("SELECTED") && previewPlayer.IsPlaying() &&
                    previewPlayer.CurrentAnimation == entry.GlideClip,
                    $"{entry.Id} animated selection preview");
                var previewTransform = preview!.Transform;
                await Frames(tree, 3);
                Require(preview.Transform.IsEqualApprox(previewTransform),
                    $"{entry.Id} preview clip translated model root");
                Require(CharacterCatalog.TryInstantiate(entry, out var probe, out _, out var diagnostic),
                    $"{entry.Id} unavailable: {diagnostic}");
                probe!.Free();
                DemoSelection.CharacterId = entry.Id;
                var world = GD.Load<PackedScene>("res://scenes/main.tscn").Instantiate<Main>();
                root.AddChild(world);
                await Frame(tree);
                var fish = world.GetNode<FishController>("Fish");
                fish.SetActivePreset(ControlPreset.Builtins()[0]);
                var visual = fish.GetNodeOrNull<Node3D>("PlayerVisual");
                var camera = fish.GetNodeOrNull<Camera3D>("CameraRig/Camera3D");
                var collider = fish.GetNode<CollisionShape3D>("Collision");
                Require(visual is not null && camera is not null && camera.Current &&
                    fish.Tuning.ResourcePath == "res://resources/swim_tuning.tres" &&
                    fish.ActivePresetId == ControlPreset.KeyboardId &&
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
                await Frames(tree, 8);
                Require(anim!.IsPlaying(), $"{entry.Id} animation stopped at rest");
                SendKey(fish, Key.W, true);
                await Frames(tree, 12);
                Require(fish.PresentationEffort > .8f && fish.PresentationSpeed > .05f,
                    $"{entry.Id} could not swim effort={fish.PresentationEffort} speed={fish.PresentationSpeed} armed={fish.GetInputArmed()} held={fish.GetHeldInputCount()} preset={fish.ActivePresetId} paused={selection.GetTree().Paused}");
                SendKey(fish, Key.A, true);
                SendKey(fish, Key.Up, true);
                await Frames(tree, 6);
                Require(fish.LastYaw < 0 && fish.LastPitch > 0,
                    $"{entry.Id} could not turn/climb");
                SendKey(fish, Key.A, false);
                SendKey(fish, Key.Up, false);
                SendKey(fish, Key.D, true);
                SendKey(fish, Key.Down, true);
                await Frames(tree, 6);
                Require(fish.LastYaw > 0 && fish.LastPitch < 0,
                    $"{entry.Id} could not turn/dive");
                SendKey(fish, Key.W, false);
                SendKey(fish, Key.D, false);
                SendKey(fish, Key.Down, false);
                SendKey(fish, Key.S, true);
                await Frames(tree, 6);
                Require(fish.LastSlow > 0 && fish.PresentationEffort == 0,
                    $"{entry.Id} could not slow");
                SendKey(fish, Key.S, false);
                world.Free();
                Require(root.GetChildCount() == originalCount, $"{entry.Id} leaked world nodes");
                GD.Print($"CHARACTER_OK id={entry.Id}");
            }
            DemoSelection.Clear();
            Input.MouseMode = Input.MouseModeEnum.Visible;
            await CheckRouting(selection, root);
            GD.Print($"CHARACTER_CHECK_OK count={marine.Count} route=select-play-return-select-play");
            tree.Quit();
        }
        catch (Exception error)
        {
            GD.PushError($"CHARACTER_CHECK_FAILED {error}");
            tree.Quit(1);
        }
    }

    private static async Task CheckRouting(CharacterSelect selection, Window root)
    {
        var tree = selection.GetTree();
        var first = selection.FindChild("Character_0", true, false) as Button;
        var enter = selection.FindChild("EnterWorldButton", true, false) as Button;
        Require(first is not null && enter is not null, "route entry controls missing");
        first!.EmitSignal(Button.SignalName.Pressed);
        enter!.EmitSignal(Button.SignalName.Pressed);
        await tree.ToSignal(tree, SceneTree.SignalName.SceneChanged);
        var world = tree.CurrentScene as Main;
        Require(world is not null && DemoSelection.CharacterId == "FAF-FISH-001",
            "first start did not enter selected world");
        var fish = world!.GetNode<FishController>("Fish");
        Require(fish.GetNodeOrNull<Node3D>("PlayerVisual")?.Scale.IsEqualApprox(
            Vector3.One * CharacterCatalog.All[0].WorldScale) == true, "first player visual");
        fish.SetActivePreset(ControlPreset.Builtins()[1]);
        await Frames(tree, 2);
        Require(fish.UsesMouseInput && fish.CanUseMouseCamera,
            "logical mouse capture was not active before return probe");
        fish._Input(new InputEventMouseMotion { Relative = new Vector2(20, 8) });
        var shell = world.GetNode<PlayShell>("PlayShell");
        var options = shell.FindChild("OptionsButton", true, false) as Button;
        var leave = shell.FindChild("WorldSelectButton", true, false) as Button;
        Require(options is not null && leave is not null, "route pause controls missing");
        options!.EmitSignal(Button.SignalName.Pressed);
        Require(tree.Paused && !fish.GetInputArmed() && fish.GetHeldInputCount() == 0 &&
            Input.MouseMode == Input.MouseModeEnum.Visible, "pause did not clear input/capture");
        leave!.EmitSignal(Button.SignalName.Pressed);
        await tree.ToSignal(tree, SceneTree.SignalName.SceneChanged);
        var returned = tree.CurrentScene as WorldSelect;
        Require(returned is not null && !tree.Paused &&
            Input.MouseMode == Input.MouseModeEnum.Visible &&
            DemoSelection.CharacterId is null && !GodotObject.IsInstanceValid(world) &&
            WorldSession.ActiveHabitat is null && WorldSession.ActiveManifest is null &&
            root.GetChildren().OfType<Main>().Count() == 0,
            "return retained gameplay session, selection or capture");
        var choose = returned!.FindChild("ChooseWorldButton", true, false) as Button;
        Require(choose is not null, "world choice missing after return");
        choose!.EmitSignal(Button.SignalName.Pressed);
        await tree.ToSignal(tree, SceneTree.SignalName.SceneChanged);
        var secondPage = tree.CurrentScene as CharacterSelect;
        var second = secondPage?.FindChild("Character_4", true, false) as Button;
        var start = secondPage?.FindChild("EnterWorldButton", true, false) as Button;
        Require(second is not null && start is not null, "second start controls missing");
        second!.EmitSignal(Button.SignalName.Pressed);
        start!.EmitSignal(Button.SignalName.Pressed);
        await tree.ToSignal(tree, SceneTree.SignalName.SceneChanged);
        var next = tree.CurrentScene as Main;
        Require(next is not null && DemoSelection.CharacterId == "FAF-FISH-005" &&
            next.GetNode<FishController>("Fish").GetNodeOrNull<Node3D>("PlayerVisual")?.Scale
                .IsEqualApprox(Vector3.One * CharacterCatalog.All[4].WorldScale) == true &&
            next.GetNode<FishController>("Fish").GetNode<CollisionShape3D>("Collision").Shape
                is SphereShape3D { Radius: 0.48f },
            "second start did not create fresh shared-authority shark world");
        GD.Print("CHARACTER_ROUTE_OK first=FAF-FISH-001 second=FAF-FISH-005 input=cleared session=replaced persistence=untouched");
    }

    private static async Task Frame(SceneTree tree) => await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
    private static async Task Frames(SceneTree tree, int count)
    {
        for (var i = 0; i < count; i++) await Frame(tree);
    }
    private static void SendKey(FishController fish, Key key, bool pressed) =>
        fish._Input(new InputEventKey { Keycode = key, PhysicalKeycode = key, Pressed = pressed });
    private static void Require(bool valid, string message)
    {
        if (!valid) throw new Exception(message);
    }
}
