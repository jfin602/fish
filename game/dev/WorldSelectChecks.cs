using Godot;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fish;

/// <summary>Current two-world menu and direct-play lifecycle check.</summary>
public static class WorldSelectChecks
{
    public static async void Run(WorldSelect initial)
    {
        var tree = initial.GetTree();
        try
        {
            Require(WorldDestinations.All.Count == 2 && WorldCatalog.All.Count == 2,
                "world choice count");
            Require(initial.FindChild("World_pike-encounter", true, false) is null &&
                initial.FindChild("CharacterSelect", true, false) is null,
                "obsolete menu choice appeared");
            var ocean = Button(initial, "World_ocean");
            var pond = Button(initial, "World_pond");
            Require(ocean.Text.Contains("Ocean") && pond.Text.Contains("Freshwater Pond") &&
                ocean.Text.Contains("SELECTED"), "world cards");
            initial._Input(new InputEventKey { Keycode = Key.Right, Pressed = true });
            Require(pond.Text.Contains("SELECTED"), "keyboard Pond selection");
            Button(initial, "ChooseWorldButton").EmitSignal(BaseButton.SignalName.Pressed);
            await Scene(tree);
            var pondScene = tree.CurrentScene as PikeEncounter;
            var bake = WorldCatalog.Resolve(WorldCatalog.PondId).Bake!;
            Require(pondScene is not null && WorldSession.SelectedWorldId == WorldCatalog.PondId &&
                WorldSession.ActiveManifest?.WorldId == bake.WorldId &&
                WorldSession.ActiveManifest.Seed == PikeCoverFixture.Seed &&
                WorldSession.ActiveHabitat == pondScene.GetNode<Node3D>("Habitat") &&
                DemoSelection.CharacterId == PikeEncounterAssets.BassId &&
                pondScene.Frog is not null && pondScene.Snake is not null &&
                pondScene.FoodActors.Count == PondFoodPatches.MaximumActors &&
                pondScene.PreyActors.Count == 16 &&
                pondScene.GetNodeOrNull<UnderwaterAtmosphere>("UnderwaterAtmosphere") is not null,
                "Pond did not launch the integrated seed-5266 scene");
            Require(pondScene.FindChild("CharacterSelect", true, false) is null,
                "character screen appeared in Pond");
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            CheckPondSolidsAndFood(pondScene);
            var frogId = pondScene.Frog!.StableId;
            var snakeId = pondScene.Snake!.StableId;
            Button(pondScene, "ReturnToWorldSelect").EmitSignal(BaseButton.SignalName.Pressed);
            await Scene(tree);
            var returned = tree.CurrentScene as WorldSelect;
            Require(returned is not null && WorldSession.ActiveHabitat is null &&
                WorldSession.ActiveManifest is null, "Pond return leaked session");
            try { WorldSession.Select("unknown"); throw new Exception("unknown world accepted"); }
            catch (InvalidDataException) { }
            Button(returned, "ChooseWorldButton").EmitSignal(BaseButton.SignalName.Pressed);
            await Scene(tree);
            var marine = tree.CurrentScene as Main;
            Require(marine is not null && WorldSession.SelectedWorldId == WorldCatalog.OceanId &&
                DemoSelection.CharacterId == CharacterCatalog.Default.Id &&
                WorldSession.ActiveManifest?.WorldId ==
                    WorldCatalog.Resolve(WorldCatalog.OceanId).SelectedManifestId &&
                marine.FindChild("CharacterSelect", true, false) is null,
                "Ocean did not launch its existing default fish directly");
            GD.Print($"WORLD_SELECT_CHECK_OK entries=2 pond={bake.WorldId} seed={bake.Seed} " +
                $"frog={frogId} snake={snakeId} ocean_default={CharacterCatalog.Default.Id}");
            tree.Quit();
        }
        catch (Exception error)
        {
            GD.PushError($"WORLD_SELECT_CHECK_FAILED {error}");
            tree.Quit(1);
        }
    }

    private static Button Button(Node root, string name) =>
        root.FindChild(name, true, false) as Button ?? throw new Exception($"missing {name}");
    private static void CheckPondSolidsAndFood(PikeEncounter scene)
    {
        var dressing = scene.GetNode<PondFafHabitat>("Habitat/PondFafEnvironment");
        var stones = dressing.GetChildren().OfType<StaticBody3D>()
            .Where(body => body.HasMeta("faf_id") &&
                body.GetMeta("faf_id").AsString() is "FAF-DETAIL-005A" or "FAF-DETAIL-005B" or
                    "FAF-DETAIL-006A" or "FAF-DETAIL-006B").ToArray();
        var expected = dressing.Placements.Count(p => p.Zone is "algae_stone" or "pebbles");
        Require(stones.Length == expected && stones.All(body =>
            body.CollisionLayer == 1 && body.GetChildren().OfType<CollisionShape3D>()
                .Any(shape => shape.Shape is ConcavePolygonShape3D)),
            "visible Pond stone or pebble lacks a physical surface");
        var space = scene.GetWorld3D().DirectSpaceState;
        foreach (var id in new[] { "FAF-DETAIL-005A", "FAF-DETAIL-005B" })
        {
            var body = stones.First(item => item.GetMeta("faf_id").AsString() == id);
            using var ray = PhysicsRayQueryParameters3D.Create(
                body.GlobalPosition + Vector3.Up, body.GlobalPosition + Vector3.Down * .4f, 1);
            var hit = space.IntersectRay(ray);
            Require(hit.Count > 0 && ReferenceEquals(hit["collider"].AsGodotObject(), body),
                "stone is missing at its displayed position: " + id);
        }
        using var probe = new SphereShape3D { Radius = .05f };
        foreach (var food in scene.FoodActors)
        {
            using var query = new PhysicsShapeQueryParameters3D {
                Shape = probe,
                Transform = new Transform3D(Basis.Identity, food.GlobalPosition),
                CollisionMask = 1,
            };
            Require(space.IntersectShape(query, 1).Count == 0,
                "food intersects a solid: " + food.StableId);
        }
        GD.Print($"POND_SOLID_FOOD_CHECK_OK solids={stones.Length} food={scene.FoodActors.Count}");
    }
    private static async Task Scene(SceneTree tree) =>
        await tree.ToSignal(tree, SceneTree.SignalName.SceneChanged);
    private static void Require(bool valid, string message)
    {
        if (!valid) throw new Exception(message);
    }
}
