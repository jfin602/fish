using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fish;

/// <summary>Headless route, resource refusal and fixture lifecycle checks.</summary>
public static class PikeEncounterChecks
{
    public static bool Started { get; private set; }

    public static async void Run(WorldSelect initial)
    {
        Started = true;
        var tree = initial.GetTree();
        try
        {
            Require(WorldCatalog.All.Count == 2 && WorldDestinations.All.Count == 3 &&
                WorldCatalog.All.All(w => w.Id != WorldDestinations.PikeId), "encounter became a habitat");
            Require(PikeEncounterAssets.TryValidate(out var bluegill, out var pike, out var diagnostic) &&
                bluegill?.Id == PikeEncounterAssets.BluegillId && pike?.Id == PikeEncounterAssets.PikeId,
                "real Pond actor validation " + diagnostic);
            Require(!PikeEncounterAssets.TryValidate(out _, out _, out diagnostic,
                "res://scenes/missing-pike.tscn") && diagnostic.Contains("SCENE_MISSING"),
                "missing scene accepted");
            Require(!PikeEncounterAssets.TryValidate(out _, out _, out diagnostic,
                pikeId: "FAF-FISH-001") && diagnostic.Contains("INELIGIBLE"),
                "wrong Pike accepted");
            var missingModel = CharacterCatalog.All.Select(entry => entry.Id == PikeEncounterAssets.BluegillId
                ? entry with { ModelPath = "res://assets/faf/pond/missing-bluegill.glb" } : entry).ToArray();
            Require(!PikeEncounterAssets.TryValidate(out _, out _, out diagnostic,
                catalog: missingModel) && diagnostic.Contains("MODEL_MISSING"), "missing Bluegill model accepted");
            var ineligible = CharacterCatalog.All.Select(entry => entry.Id == PikeEncounterAssets.PikeId
                ? entry with { Ecological = false } : entry).ToArray();
            Require(!PikeEncounterAssets.TryValidate(out _, out _, out diagnostic,
                catalog: ineligible) && diagnostic.Contains("ROLE_INELIGIBLE"), "ineligible Pike accepted");

            var pikeCard = Button(initial, "World_pike-encounter");
            Require(pikeCard.FocusMode == Control.FocusModeEnum.All &&
                pikeCard.Text.Contains("Pike Encounter"), "third card missing focus or label");
            initial._Input(new InputEventKey { Keycode = Key.Left, Pressed = true });
            Require(pikeCard.Text.Contains("SELECTED"), "keyboard did not select encounter");
            pikeCard.EmitSignal(Control.SignalName.MouseEntered);
            initial.EncounterScenePath = "res://scenes/missing-pike.tscn";
            Button(initial, "ChooseWorldButton").EmitSignal(BaseButton.SignalName.Pressed);
            await Frames(tree, 2);
            Require(tree.CurrentScene == initial && Status(initial).Contains("PIKE_SCENE_MISSING") &&
                !Button(initial, "ChooseWorldButton").Disabled, "missing scene was not shown in menu");
            initial.EncounterScenePath = PikeEncounterAssets.ScenePath;
            initial.EncounterCatalog = missingModel;
            Button(initial, "ChooseWorldButton").EmitSignal(BaseButton.SignalName.Pressed);
            await Frames(tree, 2);
            Require(tree.CurrentScene == initial && Status(initial).Contains("SPECIES_MODEL_MISSING"),
                "missing model was not shown in menu");
            initial.EncounterCatalog = ineligible;
            Button(initial, "ChooseWorldButton").EmitSignal(BaseButton.SignalName.Pressed);
            await Frames(tree, 2);
            Require(tree.CurrentScene == initial && Status(initial).Contains("SPECIES_ROLE_INELIGIBLE"),
                "ineligible Pike was not shown in menu");
            initial.EncounterCatalog = null;
            Button(initial, "ChooseWorldButton").EmitSignal(BaseButton.SignalName.Pressed);
            await Scene(tree);
            for (var visit = 0; visit < 3; visit++)
            {
                var encounter = tree.CurrentScene as PikeEncounter;
                Require(encounter is not null && !tree.Paused &&
                    WorldSession.ActiveHabitat is null && WorldSession.ActiveManifest is null &&
                    DemoSelection.CharacterId == PikeEncounterAssets.BluegillId &&
                    encounter.GetNode<FishController>("Fish").GetNodeOrNull<Node3D>("PlayerVisual") is not null &&
                    encounter.GetNode<Node3D>("NorthernPike").GetNodeOrNull<Node3D>("PikeVisual") is not null &&
                    encounter.FindChild("CharacterSelect", true, false) is null &&
                    encounter.FindChild("AmbientLife", true, false) is null,
                    "direct fixture entry or isolation failed");
                var fish = encounter!.GetNode<FishController>("Fish");
                Require(fish.GetHeldInputCount() == 0, "held control leaked into fixture");
                encounter._Input(new InputEventKey { Keycode = Key.Escape, Pressed = true });
                Require(tree.Paused && Input.MouseMode == Input.MouseModeEnum.Visible &&
                    encounter.FindChild("EncounterMenu", true, false) is Control { Visible: true },
                    "fixture pause failed");
                Button(encounter, "RetryEncounter").EmitSignal(BaseButton.SignalName.Pressed);
                await Scene(tree);
                Require(!GodotObject.IsInstanceValid(encounter) && !tree.Paused,
                    "retry retained old actor or pause");
            }
            var final = tree.CurrentScene as PikeEncounter;
            Require(final is not null, "fixture lost after retries");
            final!._Input(new InputEventKey { Keycode = Key.Escape, Pressed = true });
            Button(final, "ReturnToWorldSelect").EmitSignal(BaseButton.SignalName.Pressed);
            await Scene(tree);
            var menu = tree.CurrentScene as WorldSelect;
            Require(menu is not null && !tree.Paused && Input.MouseMode == Input.MouseModeEnum.Visible &&
                DemoSelection.CharacterId is null && WorldSession.ActiveHabitat is null &&
                WorldSession.ActiveManifest is null && WorldSession.SelectedWorldId == WorldCatalog.OceanId &&
                !GodotObject.IsInstanceValid(final), "return leaked fixture state");
            foreach (var worldId in new[] { WorldCatalog.OceanId, WorldCatalog.PondId })
            {
                Button(menu!, "World_" + worldId).EmitSignal(BaseButton.SignalName.Pressed);
                Button(menu, "ChooseWorldButton").EmitSignal(BaseButton.SignalName.Pressed);
                await Scene(tree);
                Require(tree.CurrentScene is CharacterSelect && WorldSession.SelectedWorldId == worldId &&
                    DemoSelection.CharacterId is null, "normal world route changed: " + worldId);
                Button(tree.CurrentScene, "BackToWorldButton").EmitSignal(BaseButton.SignalName.Pressed);
                await Scene(tree);
                menu = tree.CurrentScene as WorldSelect;
            }
            Button(menu!, "World_pond").EmitSignal(BaseButton.SignalName.Pressed);
            Button(menu, "ChooseWorldButton").EmitSignal(BaseButton.SignalName.Pressed);
            await Scene(tree);
            Button(tree.CurrentScene, "EnterWorldButton").EmitSignal(BaseButton.SignalName.Pressed);
            await Scene(tree);
            var ordinary = tree.CurrentScene as Main;
            Require(ordinary is not null && WorldSession.ActiveHabitat is not null &&
                ordinary.FindChild("PondAmbientLife", true, false) is not null,
                "ordinary Pond did not load ambient actors for leakage check");
            var shell = ordinary!.GetNode<PlayShell>("PlayShell");
            Button(shell, "OptionsButton").EmitSignal(BaseButton.SignalName.Pressed);
            Button(shell, "WorldSelectButton").EmitSignal(BaseButton.SignalName.Pressed);
            await Scene(tree);
            menu = tree.CurrentScene as WorldSelect;
            Require(menu is not null && !GodotObject.IsInstanceValid(ordinary),
                "ordinary Pond remained after return");
            Button(menu!, "World_pike-encounter").EmitSignal(BaseButton.SignalName.Pressed);
            Button(menu, "ChooseWorldButton").EmitSignal(BaseButton.SignalName.Pressed);
            await Scene(tree);
            var isolated = tree.CurrentScene as PikeEncounter;
            Require(isolated is not null && WorldSession.ActiveHabitat is null &&
                WorldSession.ActiveManifest is null &&
                tree.Root.GetChildren().OfType<Main>().Count() == 0 &&
                isolated.FindChild("PondAmbientLife", true, false) is null &&
                isolated.GetNode<SeabedSurface>("Habitat/Seabed").Basin.Seed == PikeCoverFixture.Seed,
                "ordinary Pond actors leaked into encounter");
            Button(isolated!, "ReturnToWorldSelect").EmitSignal(BaseButton.SignalName.Pressed);
            await Scene(tree);
            GD.Print("PIKE_ENCOUNTER_CHECK_OK routes=3 habitats=2 retries=3 errors=4 visible_errors=3 pond_to_fixture=clean isolated=true");
            tree.Quit();
        }
        catch (Exception error)
        {
            GD.PushError("PIKE_ENCOUNTER_CHECK_FAILED " + error);
            tree.Quit(1);
        }
    }

    private static Button Button(Node root, string name) => root.FindChild(name, true, false) as Button
        ?? throw new Exception("missing button " + name);
    private static async Task Scene(SceneTree tree) => await tree.ToSignal(tree, SceneTree.SignalName.SceneChanged);
    private static async Task Frames(SceneTree tree, int count)
    {
        for (var i = 0; i < count; i++) await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
    }
    private static string Status(Node root) => (root.FindChild("WorldStatus", true, false) as Label)?.Text ?? "";
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
