#nullable enable
using Godot;
using System;
using System.Linq;

namespace Fish;

/// <summary>Fixed-pose transaction and collision checks; not owner native play.</summary>
public static class FeedingChecks
{
    public static async void Run(PikeEncounter scene)
    {
        var tree = scene.GetTree();
        try
        {
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            scene.SetPhysicsProcess(false);
            var fish = scene.GetNode<FishController>("Fish");
            fish.SetPhysicsProcess(false);
            var prey = scene.PreyActors[4];
            var larvaSpawns = scene.PreyActors.Skip(4).Select(p => p.GlobalPosition).ToArray();
            foreach (var actor in scene.PreyActors) actor.SetPhysicsProcess(false);
            var ledger = scene.Feeding;
            var bluegill = PondCreatureCatalog.Stage("FAF-FISH-006", "adult")!;
            var bass = PondCreatureCatalog.Stage("FAF-FISH-007", "larva")!;
            var juvenile = PondCreatureCatalog.Stage("FAF-FISH-007", "juvenile")!;
            var pike = PondCreatureCatalog.Stage("FAF-FISH-010", "adult")!;
            Need(bass.Dimensions[1] < bluegill.Dimensions[1] * .45f &&
                juvenile.Dimensions[1] > bluegill.Dimensions[1] * .45f &&
                pike.Dimensions[1] > bluegill.Dimensions[1] * .45f,
                "registered size eligibility changed");

            var baseTarget = new FeedCandidate("test:prey", FeedKind.Prey, bass.Id, "larva",
                WorldCatalog.PondId, WorldCatalog.PondId, bass.Dimensions[1],
                bluegill.Dimensions[1], true);
            var policy = new FeedingLedger();
            Need(!policy.TryConsume(baseTarget with { SpeciesId = pike.Id,
                Length = pike.Dimensions[1] }, true, 1).Consumed, "Pike edible");
            Need(!policy.TryConsume(baseTarget with { PlayerHabitat = WorldCatalog.OceanId },
                true, 1).Consumed, "cross-habitat prey edible");
            Need(!policy.TryConsume(baseTarget with { Stage = "juvenile",
                Length = juvenile.Dimensions[1] }, true, 1).Consumed,
                "juvenile Bass edible");
            Need(!policy.TryConsume(baseTarget with { Alive = false }, true, 1).Consumed,
                "dead prey edible");
            Need(!policy.TryConsume(baseTarget, false, 1).Consumed, "inaccessible prey edible");
            Need(policy.Nutrition == 0 && policy.Events.Count == 0, "invalid attempt mutated ledger");
            Need(policy.TryConsume(baseTarget, true, 1).Consumed &&
                !policy.TryConsume(baseTarget, true, 1).Consumed &&
                policy.Nutrition == 4 && policy.Events.Count == 1,
                "transaction awarded twice");

            var open = new Vector3(0, PikeCoverFixture.Bed(0, 20) + 1.2f, 20);
            fish.GlobalPosition = open;
            fish.GlobalBasis = Basis.Identity;
            prey.GlobalPosition = open + new Vector3(0, 0, -.95f);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            scene._Input(new InputEventKey { Pressed = true, PhysicalKeycode = Key.Space });
            Need(!prey.IsAlive, "Space did not dispatch reachable Bass bite");
            var reward = ledger.Nutrition;
            Need(reward == 4 && !scene.TryBite(prey).Consumed &&
                ledger.Nutrition == reward && ledger.Events.Count == 1 &&
                !prey.IsAlive && !prey.Visible, "duplicate bite rewarded or prey remained live");
            Need(!scene.TryBite(scene.PreyActors[0]).Consumed, "large juvenile prey rewarded");
            var other = scene.PreyActors[5];
            Need(!scene.TryBite(other).Consumed, "distant larva rewarded");
            other.GlobalPosition = open + new Vector3(0, 0, .8f);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(!scene.TryBite(other).Consumed, "rear bite rewarded");

            var food = scene.FoodItems[0];
            fish.GlobalPosition = food.GlobalPosition + Vector3.Up * .4f;
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(scene.TryCollectFood(food).Consumed, "reachable food failed");
            Need(!scene.TryCollectFood(food).Consumed && ledger.Nutrition == reward + 1 &&
                ledger.Events.Count == 2 && !food.IsAvailable, "duplicate food rewarded");
            Need(!scene.TryCollectFood(scene.FoodItems[4]).Consumed,
                "distant food rewarded");

            // A physical closed wall between two otherwise bite-range fish blocks the bite.
            var wall = new StaticBody3D { Name = "FeedingTestWall", CollisionLayer = 1 };
            scene.AddChild(wall);
            wall.GlobalPosition = open + new Vector3(0, 0, -.65f);
            wall.AddChild(new CollisionShape3D { Shape = new BoxShape3D
                { Size = new Vector3(1.5f, 1.5f, .12f) } });
            fish.GlobalPosition = open;
            other.GlobalPosition = open + new Vector3(0, 0, -.95f);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            using (var wallRay = PhysicsRayQueryParameters3D.Create(
                open + new Vector3(0, 0, -.48f), other.GlobalPosition, 1,
                new Godot.Collections.Array<Rid> { fish.GetRid(), other.GetRid() }))
                Need(scene.GetWorld3D().DirectSpaceState.IntersectRay(wallRay)
                    .TryGetValue("collider", out var hit) && hit.AsGodotObject() == wall,
                    "test wall did not physically block bite ray");
            Need(!scene.TryBite(other).Consumed && ledger.Nutrition == reward + 1,
                "bite passed through solid wall");
            wall.QueueFree();
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);

            // Food is placed in distinct authored transitions, outside solid wood.
            var blockedFood = scene.FoodItems.Where(f => InsideWood(f.GlobalPosition))
                .Select(f => f.StableId).ToArray();
            Need(scene.FoodItems.Count == 5 &&
                scene.FoodItems.Select(f => f.StableId).Distinct().Count() == 5 &&
                blockedFood.Length == 0,
                "food ID or exposed-lane placement invalid: " + string.Join(",", blockedFood));
            Need(larvaSpawns.All(p => !InsideWood(p)),
                "Bass larva placed inside solid wood");
            Need(scene.PreyActors.Select(p => p.StableId).Distinct().Count() == 9,
                "prey identities reused");
            foreach (var item in scene.FoodItems.Where(f => f.IsAvailable))
            {
                fish.GlobalPosition = item.GlobalPosition + Vector3.Up * .4f;
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                Need(scene.TryCollectFood(item).Consumed, "food lane not reachable: " + item.StableId);
            }
            Need(ledger.Nutrition == 9 && ledger.Events.Count == 6,
                "multi-lane food nutrition mismatch");
            GD.Print($"FEEDING_CHECK_OK seed={PikeCoverFixture.Seed} fixture=pike prey=FAF-FISH-007:larva food=non-shipping-invertebrate-proxy nutrition={ledger.Nutrition} events={ledger.Events.Count} wall=blocked");
            tree.Quit();

            bool InsideWood(Vector3 point)
            {
                var exclude = new Godot.Collections.Array<Rid>
                { fish.GetRid(), scene.GetNode<PikeActor>("NorthernPike").GetRid() };
                foreach (var actor in scene.PreyActors) exclude.Add(actor.GetRid());
                var hits = scene.GetWorld3D().DirectSpaceState.IntersectShape(
                    new PhysicsShapeQueryParameters3D
                    {
                        Shape = new SphereShape3D { Radius = .13f },
                        Transform = new Transform3D(Basis.Identity, point),
                        CollisionMask = 1,
                        Exclude = exclude,
                    }, 1);
                if (hits.Count > 0) GD.Print($"FEEDING_PLACEMENT_HIT point={point} collider={hits[0]["collider"]}");
                return hits.Count > 0;
            }
        }
        catch (Exception error)
        {
            GD.PushError("FEEDING_CHECK_FAILED " + error);
            tree.Quit(1);
        }
    }

    private static void Need(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
