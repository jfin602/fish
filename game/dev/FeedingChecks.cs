#nullable enable
using Godot;
using System;
using System.Linq;

namespace Fish;

/// <summary>M3 food policy, physical bite and exactly-once scene checks; headless only.</summary>
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
            foreach (var prey in scene.PreyActors) prey.SetPhysicsProcess(false);
            var food = scene.FoodActors;
            Need(food.Count(f => f.Kind == FeedingLedger.MicrofaunaId) == 8 &&
                food.Count(f => f.Kind == FeedingLedger.InsectId) == 8 &&
                food.Select(f => f.StableId).Distinct().Count() == 16 &&
                food.All(f => f.IsInsideTree() && f.Visible), "food spawn/IDs invalid");
            var bassLarva = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "larva")!;
            var bassJuvenile = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "juvenile")!;
            var bassAdult = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "adult")!;
            var blueLarva = PondCreatureCatalog.Stage(PikeEncounterAssets.BluegillId, "larva")!;
            var blueJuvenile = PondCreatureCatalog.Stage(PikeEncounterAssets.BluegillId, "juvenile")!;
            var blueAdult = PondCreatureCatalog.Stage(PikeEncounterAssets.BluegillId, "adult")!;
            Need(!FeedingLedger.IsEligiblePrey(blueLarva.Id, "larva", blueLarva.Dimensions[1],
                    "juvenile", bassJuvenile.Dimensions[1]) &&
                FeedingLedger.IsEligiblePrey(blueLarva.Id, "larva", blueLarva.Dimensions[1],
                    "subadult", scene.Growth.Asset.Dimensions[1] * 3f) &&
                FeedingLedger.IsEligiblePrey(blueJuvenile.Id, "juvenile", blueJuvenile.Dimensions[1],
                    "adult", bassAdult.Dimensions[1]) &&
                !FeedingLedger.IsEligiblePrey(blueJuvenile.Id, "juvenile", bassAdult.Dimensions[1],
                    "adult", bassAdult.Dimensions[1]) &&
                !FeedingLedger.IsEligiblePrey(blueAdult.Id, "adult", blueAdult.Dimensions[1],
                    "adult", bassAdult.Dimensions[1]) &&
                !FeedingLedger.IsEligiblePrey(bassLarva.Id, "larva", bassLarva.Dimensions[1],
                    "adult", bassAdult.Dimensions[1]), "fish prey policy invalid");
            var policy = new FeedingLedger();
            Need(FeedingLedger.FoodAward(FeedingLedger.MicrofaunaId, "egg", 100) == 0 &&
                FeedingLedger.FoodAward(FeedingLedger.MicrofaunaId, "larva", 11.9) == 0 &&
                FeedingLedger.FoodAward(FeedingLedger.MicrofaunaId, "larva", 12) == 1 &&
                FeedingLedger.FoodAward(FeedingLedger.InsectId, "larva", 100) == 0 &&
                FeedingLedger.FoodAward(FeedingLedger.InsectId, "juvenile", 12) == 2,
                "yolk/food stage table invalid");
            Need(!policy.TryConsumeFood("m", FeedingLedger.MicrofaunaId, "egg", true, 1, 1).Consumed &&
                !policy.TryConsumeFood("m", FeedingLedger.MicrofaunaId, "larva", true, 11, 11).Consumed &&
                !policy.TryConsumeFood("m", FeedingLedger.MicrofaunaId, "larva", false, 12, 12).Consumed &&
                !policy.TryConsumeFood("m", FeedingLedger.MicrofaunaId, "larva", true, -1, 12).Consumed &&
                policy.Nutrition == 0 && policy.Events.Count == 0 &&
                policy.TryConsumeFood("m", FeedingLedger.MicrofaunaId, "larva", true, 12, 12).Consumed &&
                !policy.TryConsumeFood("m", FeedingLedger.MicrofaunaId, "larva", true, 13, 13).Consumed &&
                policy.Nutrition == 1 && policy.Events.Count == 1, "microfauna ledger invalid");
            var preyCandidate = new FeedCandidate("blue", blueLarva.Id, "larva",
                WorldCatalog.PondId, WorldCatalog.PondId, blueLarva.Dimensions[1],
                "subadult", PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "subadult")!.Dimensions[1], true);
            Need(!policy.TryConsume(preyCandidate with { PlayerHabitat = WorldCatalog.OceanId }, true, 15).Consumed &&
                !policy.TryConsume(preyCandidate with { Alive = false }, true, 15).Consumed &&
                !policy.TryConsume(preyCandidate, false, 15).Consumed &&
                policy.TryConsume(preyCandidate, true, 15).Consumed &&
                !policy.TryConsume(preyCandidate, true, 16).Consumed &&
                policy.Nutrition == 5 && policy.Events.Count == 2, "fish ledger invalid");

            var open = new Vector3(0, PikeCoverFixture.Bed(0, 20) + 1.2f, 20);
            fish.GlobalBasis = Basis.Identity;
            // This headless policy check exits immediately after feeding; keep
            // audio playback from outliving the process shutdown.
            fish.GetNode<AudioStreamPlayer3D>("SmallFeedSound").Stream = null;
            var nymphs = food.Where(f => f.Kind == FeedingLedger.InsectId).ToArray();
            PutAhead(nymphs[0], .02f);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(!scene.TryBiteFood(food[0]).Consumed && scene.Feeding.Nutrition == 0,
                "juvenile ate microfauna");
            Need(!scene.TryBite(scene.PreyActors[4]).Consumed,
                "juvenile ate Bluegill before invertebrates");
            Need(scene.TryBiteFood(nymphs[0]).Consumed && !nymphs[0].Available &&
                !scene.TryBiteFood(nymphs[0]).Consumed && scene.Feeding.Nutrition == 2,
                "nymph transaction failed");
            PutAhead(nymphs[1], .02f);
            nymphs[1].GlobalPosition = open + Vector3.Back * .9f;
            Need(!scene.TryBiteFood(nymphs[1]).Consumed, "rear nymph eaten");
            nymphs[1].GlobalPosition = open + Vector3.Forward * 2f;
            Need(!scene.TryBiteFood(nymphs[1]).Consumed, "distant nymph eaten");
            var wall = new StaticBody3D { Name = "M3FoodWall", CollisionLayer = 1 };
            scene.AddChild(wall);
            wall.GlobalPosition = open + Vector3.Forward * (fish.BodyForwardReach + .01f);
            wall.AddChild(new CollisionShape3D { Shape = new BoxShape3D
                { Size = new Vector3(1f, 1f, .004f) } });
            PutAhead(nymphs[1], .02f);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(!scene.TryBiteFood(nymphs[1]).Consumed && scene.Feeding.Nutrition == 2,
                "wall did not block nymph");
            wall.QueueFree();
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            for (var i = 1; i < 4; i++)
            {
                PutAhead(nymphs[i], .02f);
                Need(scene.TryBiteFood(nymphs[i]).Consumed, "nymph growth bite failed " + i);
            }
            Need(scene.Growth.Stage == "juvenile" && scene.Feeding.Nutrition == 8 &&
                scene.Feeding.Events.Count == 4, "juvenile advanced before threshold");
            GD.Print($"FEEDING_CHECK_OK food={food.Count} juvenile_nutrition={scene.Feeding.Nutrition} policy_events={policy.Events.Count} wall=blocked");
            tree.Quit();

            void PutAhead(PondFoodActor target, float distance)
            {
                fish.GlobalPosition = open;
                fish.GlobalBasis = Basis.Identity;
                target.GlobalPosition = open + Vector3.Forward * (fish.BodyForwardReach + distance);
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
