using Godot;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fish;

/// <summary>Focused P4 menu and scene-lifecycle integration check.</summary>
public static class WorldSelectChecks
{
    public static async void Run(WorldSelect initial)
    {
        var tree = initial.GetTree();
        try
        {
            Require(tree.CurrentScene == initial && !tree.Paused &&
                Input.MouseMode == Input.MouseModeEnum.Visible, "startup did not open world selection");
            var ocean = FindButton(initial, "World_ocean");
            var pond = FindButton(initial, "World_pond");
            var choose = FindButton(initial, "ChooseWorldButton");
            Require(ocean.Text.Contains("Ocean") && pond.Text.Contains("Freshwater Pond") &&
                pond.Text.Contains("Preview art pending") && ocean.Text.Contains("SELECTED"), "cards or preview state");
            initial._Input(KeyEvent(Key.Right));
            Require(pond.Text.Contains("SELECTED"), "keyboard world browsing");
            ocean.EmitSignal(Control.SignalName.MouseEntered);
            Require(ocean.Text.Contains("SELECTED"), "pointer world browsing");
            pond.GrabFocus();
            Require(pond.HasFocus() && pond.Text.Contains("SELECTED"), "pointer/keyboard focus state");
            choose.EmitSignal(Button.SignalName.Pressed);
            await Scene(tree);
            var pondCharacters = tree.CurrentScene as CharacterSelect;
            Require(pondCharacters is not null && WorldSession.SelectedWorldId == WorldCatalog.PondId &&
                Enumerable.Range(0, 5).All(index =>
                    pondCharacters.FindChild($"Character_{index}", true, false) is Button) &&
                pondCharacters.FindChild("EnterWorldButton", true, false) is Button { Disabled: false },
                "Pond should offer five native fish after P8");
            FindButton(pondCharacters!, "BackToWorldButton").EmitSignal(Button.SignalName.Pressed);
            await Scene(tree);
            initial = tree.CurrentScene as WorldSelect ?? throw new Exception("Pond Back did not return to World Select");
            ocean = FindButton(initial, "World_ocean");
            choose = FindButton(initial, "ChooseWorldButton");
            try { WorldSession.Select("not-a-world"); throw new Exception("unknown world accepted"); }
            catch (InvalidDataException) { }
            Require(WorldSession.SelectedWorldId == WorldCatalog.PondId, "invalid world changed session");

            ocean.EmitSignal(Button.SignalName.Pressed);
            choose.EmitSignal(Button.SignalName.Pressed);
            await Scene(tree);
            var characters = tree.CurrentScene as CharacterSelect;
            Require(characters is not null && !GodotObject.IsInstanceValid(initial) &&
                WorldSession.SelectedWorldId == WorldCatalog.OceanId &&
                Input.MouseMode == Input.MouseModeEnum.Visible, "Ocean did not enter character selection");
            FindButton(characters!, "BackToWorldButton").EmitSignal(Button.SignalName.Pressed);
            await Scene(tree);
            var returned = tree.CurrentScene as WorldSelect;
            Require(returned is not null && !GodotObject.IsInstanceValid(characters) &&
                DemoSelection.CharacterId is null && WorldSession.ActiveHabitat is null,
                "Back did not release character scene/session");
            FindButton(returned!, "ChooseWorldButton").EmitSignal(Button.SignalName.Pressed);
            await Scene(tree);
            characters = tree.CurrentScene as CharacterSelect;
            Require(characters is not null, "Ocean reentry failed");
            FindButton(characters!, "EnterWorldButton").EmitSignal(Button.SignalName.Pressed);
            await Scene(tree);
            var world = tree.CurrentScene as Main;
            Require(world is not null && WorldSession.ActiveHabitat is not null &&
                WorldSession.ActiveManifest?.WorldId == WorldCatalog.Resolve(WorldCatalog.OceanId).SelectedManifestId,
                "Ocean play identity failed");
            var fish = world!.GetNode<FishController>("Fish");
            var shell = world.GetNode<PlayShell>("PlayShell");
            fish.SetActivePreset(ControlPreset.Builtins()[1]);
            fish._Input(KeyEvent(Key.W));
            fish._Input(KeyEvent(Key.S));
            await Frames(tree, 2);
            shell.Notification((int)Node.NotificationApplicationFocusOut);
            Require(tree.Paused && !fish.GetInputArmed() && fish.GetHeldInputCount() == 0 &&
                Input.MouseMode == Input.MouseModeEnum.Visible, "focus loss retained thrust/brake/capture");
            shell.Notification((int)Node.NotificationApplicationFocusIn);
            Require(tree.Paused, "focus return resumed play without explicit rearm");
            FindButton(shell, "WorldSelectButton").EmitSignal(Button.SignalName.Pressed);
            await Scene(tree);
            returned = tree.CurrentScene as WorldSelect;
            Require(returned is not null && !tree.Paused && !GodotObject.IsInstanceValid(world) &&
                WorldSession.ActiveHabitat is null && WorldSession.ActiveManifest is null &&
                DemoSelection.CharacterId is null && Input.MouseMode == Input.MouseModeEnum.Visible &&
                tree.Root.GetChildren().OfType<Main>().Count() == 0,
                "Return leaked world, held input, pause or session");
            FindButton(returned!, "ChooseWorldButton").EmitSignal(Button.SignalName.Pressed);
            await Scene(tree);
            characters = tree.CurrentScene as CharacterSelect;
            FindButton(characters!, "Character_4").EmitSignal(Button.SignalName.Pressed);
            FindButton(characters, "EnterWorldButton").EmitSignal(Button.SignalName.Pressed);
            await Scene(tree);
            world = tree.CurrentScene as Main;
            Require(world is not null && DemoSelection.CharacterId == "FAF-FISH-005" &&
                WorldSession.ActiveManifest?.WorldId == WorldCatalog.Resolve(WorldCatalog.OceanId).SelectedManifestId &&
                world.GetNode<FishController>("Fish").GetHeldInputCount() == 0,
                "repeated Ocean selection leaked prior input or lost fish choice");
            shell = world.GetNode<PlayShell>("PlayShell");
            FindButton(shell, "OptionsButton").EmitSignal(Button.SignalName.Pressed);
            FindButton(shell, "WorldSelectButton").EmitSignal(Button.SignalName.Pressed);
            await Scene(tree);
            returned = tree.CurrentScene as WorldSelect;
            Require(returned is not null && FindButton(returned, "WorldBackButton").Text.Contains("Quit demo"),
                "World Select Back action missing");
            GD.Print("WORLD_SELECT_CHECK_OK pond=five-native-species ocean=repeated back=clean return=clean");
            FindButton(returned!, "WorldBackButton").EmitSignal(Button.SignalName.Pressed);
        }
        catch (Exception error)
        {
            GD.PushError($"WORLD_SELECT_CHECK_FAILED {error}");
            tree.Quit(1);
        }
    }

    private static Button FindButton(Node root, string name) => root.FindChild(name, true, false) as Button
        ?? throw new Exception($"missing {name}");
    private static string Status(Node root) => (root.FindChild("WorldStatus", true, false) as Label)?.Text ?? "";
    private static InputEventKey KeyEvent(Key key) => new() { Keycode = key, PhysicalKeycode = key, Pressed = true };
    private static async Task Scene(SceneTree tree) => await tree.ToSignal(tree, SceneTree.SignalName.SceneChanged);
    private static async Task Frames(SceneTree tree, int count)
    {
        for (var i = 0; i < count; i++) await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
    }
    private static void Require(bool valid, string message)
    {
        if (!valid) throw new Exception(message);
    }
}
