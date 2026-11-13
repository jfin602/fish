#nullable enable
using Godot;
using System;
using System.Linq;

namespace Fish;

/// <summary>Fixed-pose prey transactions, physical reach and spawn checks; not native play.</summary>
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
            foreach (var actor in scene.PreyActors) actor.SetPhysicsProcess(false);
            var larvae = scene.PreyActors.Where(p => p.Stage == "larva").ToArray();
            var juveniles = scene.PreyActors.Where(p => p.Stage == "juvenile").ToArray();
            var larvaSpawns = larvae.Select(p => p.GlobalPosition).ToArray();
            var ledger = scene.Feeding;
            Need(scene.Growth.Stage == "juvenile" && ledger.Nutrition == 0,
                "fixture did not start juvenile and hungry");
            Need(larvae.Length == 6 && juveniles.Length == 4 &&
                scene.PreyActors.Select(p => p.StableId).Distinct().Count() == 10,
                "registered Bass spawn identities changed");
            Need(larvae[0].GlobalPosition.DistanceTo(fish.GlobalPosition) < 4f &&
                larvae[1].GlobalPosition.DistanceTo(fish.GlobalPosition) < 6f &&
                larvaSpawns.All(p => !InsideWood(p)),
                "first Bass larvae are distant or inside solid wood");
            Need(larvae.All(p => p.GetNode<CollisionShape3D>("Collision").Shape is SphereShape3D { Radius: .07f } &&
                p.GetNode<Node3D>("PreyVisual").Scale.X > 3f),
                "Bass larvae lack visible registered rigs or physical bodies");

            var bluegillJuvenile = PondCreatureCatalog.Stage("FAF-FISH-006", "juvenile")!;
            var bluegillAdult = PondCreatureCatalog.Stage("FAF-FISH-006", "adult")!;
            var bassLarva = PondCreatureCatalog.Stage("FAF-FISH-007", "larva")!;
            var bassJuvenile = PondCreatureCatalog.Stage("FAF-FISH-007", "juvenile")!;
            var pike = PondCreatureCatalog.Stage("FAF-FISH-010", "adult")!;
            Need(FeedingLedger.IsEligiblePrey(bassLarva.Id, "larva", bassLarva.Dimensions[1],
                    "juvenile", bluegillJuvenile.Dimensions[1]) &&
                !FeedingLedger.IsEligiblePrey(bassJuvenile.Id, "juvenile", bassJuvenile.Dimensions[1],
                    "juvenile", bluegillJuvenile.Dimensions[1]) &&
                FeedingLedger.IsEligiblePrey(bassJuvenile.Id, "juvenile", bassJuvenile.Dimensions[1],
                    "adult", bluegillAdult.Dimensions[1]) &&
                !FeedingLedger.IsEligiblePrey(pike.Id, "adult", pike.Dimensions[1],
                    "adult", bluegillAdult.Dimensions[1]),
                "registered prey-size policy changed");

            var baseTarget = new FeedCandidate("test:prey", bassLarva.Id, "larva",
                WorldCatalog.PondId, WorldCatalog.PondId, bassLarva.Dimensions[1],
                "juvenile", bluegillJuvenile.Dimensions[1], true);
            var policy = new FeedingLedger();
            Need(!policy.TryConsume(baseTarget with { SpeciesId = pike.Id,
                Length = pike.Dimensions[1] }, true, 1).Consumed, "Pike edible");
            Need(!policy.TryConsume(baseTarget with { PlayerHabitat = WorldCatalog.OceanId },
                true, 1).Consumed, "cross-habitat prey edible");
            Need(!policy.TryConsume(baseTarget with { Stage = "juvenile",
                Length = bassJuvenile.Dimensions[1] }, true, 1).Consumed,
                "juvenile Bass edible before adult Bluegill");
            Need(!policy.TryConsume(baseTarget with { Alive = false }, true, 1).Consumed,
                "dead prey edible");
            Need(!policy.TryConsume(baseTarget, false, 1).Consumed,
                "inaccessible prey edible");
            Need(policy.Nutrition == 0 && policy.Events.Count == 0,
                "invalid attempt mutated ledger");
            Need(policy.TryConsume(baseTarget, true, 1).Consumed &&
                !policy.TryConsume(baseTarget, true, 1).Consumed &&
                policy.Nutrition == 4 && policy.Events.Count == 1,
                "prey transaction awarded twice");

            var open = new Vector3(0, PikeCoverFixture.Bed(0, 20) + 1.2f, 20);
            fish.GlobalPosition = open;
            fish.GlobalBasis = Basis.Identity;
            PutAhead(larvae[0], .8f);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            scene._Input(new InputEventKey { Pressed = true, PhysicalKeycode = Key.Space });
            Need(!larvae[0].IsAlive && scene.Growth.Stage == "subadult" &&
                ledger.Nutrition == 4 && ledger.Events.Count == 1 &&
                !scene.TryBite(larvae[0]).Consumed,
                "first Bass larva bite, growth or duplicate refusal failed");
            Need(!scene.TryBite(juveniles[0]).Consumed,
                "subadult Bluegill ate oversized Bass juvenile");
            Need(!scene.TryBite(larvae[1]).Consumed,
                "distant Bass larva rewarded");
            larvae[1].GlobalPosition = open + new Vector3(0, 0, .8f);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(!scene.TryBite(larvae[1]).Consumed,
                "rear Bass larva rewarded");

            // A real wall between mouth and larva rejects an otherwise reachable bite.
            var wall = new StaticBody3D { Name = "FeedingTestWall", CollisionLayer = 1 };
            scene.AddChild(wall);
            wall.GlobalPosition = open + new Vector3(0, 0, -.65f);
            wall.AddChild(new CollisionShape3D { Shape = new BoxShape3D
                { Size = new Vector3(1.5f, 1.5f, .12f) } });
            PutAhead(larvae[1], .95f);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            using (var wallRay = PhysicsRayQueryParameters3D.Create(
                open + new Vector3(0, 0, -fish.BodyRadius), larvae[1].GlobalPosition, 1,
                new Godot.Collections.Array<Rid> { fish.GetRid(), larvae[1].GetRid() }))
                Need(scene.GetWorld3D().DirectSpaceState.IntersectRay(wallRay)
                    .TryGetValue("collider", out var hit) && hit.AsGodotObject() == wall,
                    "test wall did not physically block bite ray");
            Need(!scene.TryBite(larvae[1]).Consumed && ledger.Nutrition == 4,
                "bite passed through solid wall");
            wall.QueueFree();
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);

            PutAhead(larvae[1], .95f);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(scene.TryBite(larvae[1]).Consumed && ledger.Nutrition == 8 &&
                scene.Growth.Stage == "subadult", "second Bass larva award failed");
            PutAhead(larvae[2], .95f);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(scene.TryBite(larvae[2]).Consumed && ledger.Nutrition == 12 &&
                scene.Growth.Stage == "adult", "third Bass larva did not grow adult");
            PutAhead(juveniles[0], .95f);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(scene.TryBite(juveniles[0]).Consumed && ledger.Nutrition == 16 &&
                !scene.TryBite(juveniles[0]).Consumed && ledger.Events.Count == 4,
                "adult Bass-juvenile food-chain change or duplicate refusal failed");
            GD.Print($"FEEDING_CHECK_OK seed={PikeCoverFixture.Seed} fixture=pike prey=FAF-FISH-007 nutrition={ledger.Nutrition} events={ledger.Events.Count} larvae={larvae.Length} wall=blocked");
            tree.Quit();

            void PutAhead(PreyActor prey, float distance)
            {
                fish.GlobalPosition = open;
                fish.GlobalBasis = Basis.Identity;
                prey.GlobalPosition = open + new Vector3(0, 0, -distance);
            }
            bool InsideWood(Vector3 point)
            {
                var exclude = new Godot.Collections.Array<Rid>
                { fish.GetRid(), scene.GetNode<PikeActor>("NorthernPike").GetRid() };
                foreach (var actor in scene.PreyActors) exclude.Add(actor.GetRid());
                return scene.GetWorld3D().DirectSpaceState.IntersectShape(
                    new PhysicsShapeQueryParameters3D
                    {
                        Shape = new SphereShape3D { Radius = .08f },
                        Transform = new Transform3D(Basis.Identity, point),
                        CollisionMask = 1,
                        Exclude = exclude,
                    }, 1).Count > 0;
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
