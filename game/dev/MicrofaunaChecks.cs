#nullable enable
using Godot;
using System;
using System.Linq;

namespace Fish;

/// <summary>Real hatch-to-first-food transaction in the isolated fixture.</summary>
public static class MicrofaunaChecks
{
    public static async void Run(PikeEncounter scene)
    {
        var tree = scene.GetTree();
        try
        {
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            var cloud = scene.FoodActors.Where(f => f.Kind == FeedingLedger.MicrofaunaId).ToArray();
            Need(scene.Growth.Stage == "egg" && cloud.Length == 8 &&
                !scene.TryBiteFood(cloud[0]).Consumed, "egg consumed food");
            scene.CompleteHatch();
            for (var i = 0; i < 240 && scene.IntroActive; i++)
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(!scene.IntroActive && scene.Growth.Stage == "larva" &&
                scene.Feeding.Nutrition == 0, "hatch did not yield unfed larva");
            GrowthChecks.CheckMeshInsideBody(scene.GetNode<FishController>("Fish"), scene.Growth);
            scene.SetPhysicsProcess(false);
            var fish = scene.GetNode<FishController>("Fish");
            Need(Mathf.IsEqualApprox(fish.Tuning.MaxSpeed, .22f), "larva speed cap invalid");
            fish.SetPhysicsProcess(false);
            var origin = scene.NestPosition + Vector3.Up * .55f;
            var first = cloud[0];
            Place(first);
            var clock = scene.MovementCues!;
            Need(!scene.TryBiteFood(first).Consumed && clock.SimulationSeconds < .1,
                "yolk interval permitted immediate feeding");
            clock.Step(origin, Vector3.Zero, 0, 0, FeedingLedger.YolkSeconds - .1,
                int.MaxValue - 2, false);
            Need(!scene.TryBiteFood(first).Consumed && scene.Feeding.Events.Count == 0,
                "yolk interval ended early");
            clock.Step(origin, Vector3.Zero, 0, 0, .2, int.MaxValue - 1, false);
            Need(clock.SimulationSeconds >= FeedingLedger.YolkSeconds,
                "game clock did not advance to first feeding");
            for (var i = 0; i < 6; i++)
            {
                Place(cloud[i]);
                Need(scene.TryBiteFood(cloud[i]).Consumed && !cloud[i].Available &&
                    !scene.TryBiteFood(cloud[i]).Consumed &&
                    scene.Feeding.Nutrition == i + 1, "microfauna reward/duplicate failed " + i);
            }
            Need(scene.Growth.Stage == "juvenile" && scene.Growth.Identity ==
                $"pike-{PikeCoverFixture.Seed}:player-bass" &&
                scene.Feeding.Events.Count == 6 &&
                scene.Feeding.Events.All(e => e.Stage == FeedingLedger.MicrofaunaId) &&
                scene.Growth.Asset.ModelPath == PondCreatureCatalog.Stage(
                    PikeEncounterAssets.BassId, "juvenile")!.ModelPath,
                "first stage did not use registered juvenile export");
            Need(Mathf.IsEqualApprox(fish.Tuning.MaxSpeed, .48f), "juvenile speed cap invalid");
            var nymphs = scene.FoodActors.Where(f => f.Kind == FeedingLedger.InsectId).Take(4).ToArray();
            foreach (var nymph in nymphs)
            {
                Place(nymph, .43f);
                Need(scene.TryBiteFood(nymph).Consumed, "juvenile did not eat aquatic nymph");
            }
            Need(scene.Growth.Stage == "subadult" && scene.Feeding.Nutrition == 14 &&
                scene.Growth.Identity == $"pike-{PikeCoverFixture.Seed}:player-bass",
                "real hatch path did not reach subadult");
            Need(Mathf.IsEqualApprox(fish.Tuning.MaxSpeed, .95f), "subadult speed cap invalid");
            foreach (var prey in scene.PreyActors.Where(p => p.Stage == "larva").Take(3))
            {
                fish.GlobalPosition = origin;
                fish.GlobalBasis = Basis.Identity;
                prey.GlobalPosition = origin + Vector3.Forward * .75f;
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                Need(scene.TryBite(prey).Consumed, "subadult did not eat size-eligible Bluegill");
            }
            Need(scene.Growth.Stage == "adult" && scene.Feeding.Nutrition == 26 &&
                scene.Feeding.Events.Count == 13 &&
                scene.Growth.Identity == $"pike-{PikeCoverFixture.Seed}:player-bass",
                "real hatch path did not reach adult under one identity");
            Need(Mathf.IsEqualApprox(fish.Tuning.MaxSpeed, 1.55f), "adult speed cap invalid");
            GD.Print($"MICROFAUNA_CHECK_OK stage={scene.Growth.Stage} nutrition={scene.Feeding.Nutrition} events={scene.Feeding.Events.Count} yolk_seconds={FeedingLedger.YolkSeconds} chain=zooplankton,nymph,bluegill");
            tree.Quit();

            void Place(PondFoodActor food, float distance = .095f)
            {
                fish.GlobalPosition = origin;
                fish.GlobalBasis = Basis.Identity;
                food.GlobalPosition = origin + Vector3.Forward * distance;
            }
        }
        catch (Exception error)
        {
            GD.PushError("MICROFAUNA_CHECK_FAILED " + error);
            tree.Quit(1);
        }
    }

    private static void Need(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
