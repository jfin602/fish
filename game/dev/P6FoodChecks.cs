#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

/// <summary>Focused normal-Pond food identity, habitat and game-clock checks.</summary>
public static class P6FoodChecks
{
    private static string? _firstLife;

    public static async void RunRoute(WorldSelect worldSelect)
    {
        var tree = worldSelect.GetTree();
        try
        {
            (worldSelect.FindChild("World_pond", true, false) as Button)!
                .EmitSignal(BaseButton.SignalName.Pressed);
            (worldSelect.FindChild("ChooseWorldButton", true, false) as Button)!
                .EmitSignal(BaseButton.SignalName.Pressed);
            for (var i = 0; i < 3; i++) await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            var select = tree.CurrentScene as CharacterSelect;
            Need(select is not null, "Pond character page missing");
            var bass = select!.FindChildren("Character_*", "Button", true, false)
                .OfType<Button>().Single(button => button.Text.Contains("Bass", StringComparison.Ordinal));
            bass.EmitSignal(BaseButton.SignalName.Pressed);
            (select.FindChild("EnterWorldButton", true, false) as Button)!
                .EmitSignal(BaseButton.SignalName.Pressed);
        }
        catch (Exception error) { Fail(tree, error); }
    }

    public static async void Run(PondBassSession session)
    {
        var tree = session.GetTree();
        try
        {
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            if (_firstLife is not null)
            {
                Need(session.Growth.Identity != _firstLife && session.Growth.Stage == "egg" &&
                    session.GameSeconds == 0 && session.Feeding.Nutrition == 0 &&
                    session.Feeding.Events.Count == 0 &&
                    session.FoodActors.Count == PondFoodPatches.MaximumActors &&
                    session.AvailableFoodCount == PondFoodPatches.MaximumActors,
                    "retry retained depleted food or previous life state");
                GD.Print($"P6_FOOD_CHECK_OK world={WorldSession.ActiveManifest!.WorldId} " +
                    $"seed={WorldSession.ActiveManifest.Seed} patches={session.FoodPatchSites.Count} " +
                    $"actors={session.FoodActors.Count} retry=new-life");
                tree.Quit();
                return;
            }
            var main = session.GetParent<Main>();
            var fish = main.GetNode<FishController>("Fish");
            var bed = main.GetNode<SeabedSurface>("Habitat/Seabed");
            var sites = session.FoodPatchSites;
            Need(WorldSession.ActiveManifest?.Seed == 71 && sites.Count == 9 &&
                session.FoodActors.Count == PondFoodPatches.MaximumActors &&
                session.AvailableFoodCount == PondFoodPatches.MaximumActors &&
                sites.Select(site => site.Id).Distinct().Count() == sites.Count &&
                session.FoodActors.Select(actor => actor.StableId).Distinct().Count() ==
                    PondFoodPatches.MaximumActors,
                "initial seed, patch or actor identity invalid");
            var nest = session.NestPosition;
            foreach (var site in sites)
            {
                GD.Print($"P6_FOOD_PATCH region={site.Region} id={site.Id} " +
                    $"anchor={site.Anchor} nest_distance={site.Anchor.DistanceTo(nest):F2} " +
                    $"count={site.InitialCount} recovery_s={site.RecoverySeconds:F0}");
            }
            Need(sites.Single(s => s.Region == "nursery").InitialCount == 3 &&
                sites.Where(s => s.Region != "nursery").All(s => s.Anchor.DistanceTo(nest) > 1.5f),
                "nursery remains an unlimited local circle");
            var habitatAnchors = main.GetNode<PondFafHabitat>("Habitat/PondFafEnvironment").Placements;
            foreach (var region in new[] { "roots", "wood-insect" })
            {
                var site = sites.Single(s => s.Region == region);
                Need(habitatAnchors.Any(p => p.Zone is "fallen_log" or "stump" or "hollow_trunk" &&
                    p.Position.DistanceTo(site.Anchor) < .01f),
                    "food shelter anchor is not installed wood: " + region);
            }
            foreach (var actor in session.FoodActors)
            {
                var at = actor.GlobalPosition;
                var surface = fish.PondSurfaceLimitAt(at.X, at.Z);
                Need(bed.Basin.DistanceToShore(at.X, at.Z) >= 1f &&
                    at.Y > bed.GetHeightAt(at.X, at.Z) + .10f &&
                    at.Y < surface - .10f &&
                    LocalApproachClear(main, fish, at),
                    "food outside water column or blocked from a mouth approach: " + actor.StableId);
            }
            foreach (var seed in new ulong[] { 71, 72 })
            {
                var basin = new GeneratedBasin(seed, pond: true);
                var sampleNest = SampleNest(basin);
                var anchors = SampleAnchors(basin);
                var first = PondFoodPatches.PlanSites(basin, sampleNest, anchors, $"sample-{seed}");
                var second = PondFoodPatches.PlanSites(basin, sampleNest, anchors, $"sample-{seed}");
                Need(first.SequenceEqual(second) && first.Count == 9 &&
                    first.Select(s => s.Id).Distinct().Count() == 9,
                    "multi-seed patch planning unstable: " + seed);
                GD.Print($"P6_FOOD_SEED_PLAN seed={seed} patches={first.Count} " +
                    $"first={first[0].Id} last={first[^1].Id}");
            }
            session.SkipIntro();
            for (var i = 0; i < 300 && session.IntroActive; i++)
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(!session.IntroActive && session.Growth.Stage == "larva", "larval handoff failed");
            session.SetPhysicsProcess(false);
            fish.SetPhysicsProcess(false);
            var beforeProbe = fish.GlobalTransform;
            foreach (var region in new[] { "nursery", "roots" })
            {
                var target = session.FoodActors.First(a => a.StableId.Contains(":" + region + ":"));
                var at = target.GlobalPosition;
                var direction = new Vector3(at.X - nest.X, 0, at.Z - nest.Z).Normalized();
                fish.GlobalPosition = at - direction * .3f;
                fish.LookAt(at, Vector3.Up);
                fish.GlobalPosition += at - fish.MouthPosition - fish.MouthForward * .012f;
                Need(session.CanReachFoodForCheck(target) &&
                    LocalApproachClear(main, fish, fish.GlobalPosition),
                    "original food site cannot be reached by a larval mouth: " + region);
            }
            fish.GlobalTransform = beforeProbe;
            GD.Print("P6_FOOD_MOUTH_PATH_OK regions=nursery,roots wood_anchor=installed");
            session.SetHatchSecondsForCheck(12.1);
            void Place(PondFoodActor actor) =>
                actor.Relocate(fish.MouthPosition + fish.MouthForward * .012f, session.GameSeconds);
            var nursery = session.FoodActors.Where(a => a.StableId.Contains(":nursery:")).ToArray();
            foreach (var actor in nursery)
            {
                Place(actor);
                Need(session.TryConsumeFood(actor).Consumed && !session.TryConsumeFood(actor).Consumed,
                    "nursery meal was duplicated or missed");
            }
            Need(session.Feeding.Nutrition == 3 &&
                nursery.All(a => !a.Available) && session.AvailableFoodCount == 20,
                "nursery did not deplete after three meals");
            var reeds = session.FoodActors.Where(a => a.StableId.Contains(":reeds-west:")).ToArray();
            var originalReedPositions = reeds.Select(a => a.GlobalPosition).ToArray();
            foreach (var actor in reeds)
            {
                Place(actor);
                Need(session.TryConsumeFood(actor).Consumed, "alternate reed food missed");
            }
            Need(session.Feeding.Nutrition == 6 && session.Growth.Stage == "juvenile",
                "larval alternate food route cannot reach juvenile");
            var oldId = reeds[0].StableId;
            var clock = session.GameSeconds;
            var shell = main.GetNode<PlayShell>("PlayShell");
            (shell.FindChild("OptionsButton", true, false) as Button)!
                .EmitSignal(BaseButton.SignalName.Pressed);
            session.AdvanceFoodForCheck(500);
            Need(tree.Paused && session.GameSeconds == clock && session.AvailableFoodCount == 17,
                "pause advanced food recovery");
            (shell.FindChild("ResumeButton", true, false) as Button)!
                .EmitSignal(BaseButton.SignalName.Pressed);
            session.AdvanceFoodForCheck(149.9);
            Need(session.AvailableFoodCount == 17, "reed patch recovered before its deadline");
            session.AdvanceFoodForCheck(.2);
            var renewed = session.FoodActors.Single(a => a.StableId.Contains(":reeds-west:") && a.Available);
            Need(renewed.StableId != oldId &&
                originalReedPositions.All(position => renewed.GlobalPosition.DistanceTo(position) > .2f) &&
                session.FoodActors.Count <= PondFoodPatches.MaximumActors &&
                session.FoodActors.All(a => !a.StableId.Contains(":nursery:") || !a.Available) &&
                session.Feeding.IsConsumed(oldId) && session.Feeding.Nutrition == 6,
                "renewal reused identity, place, nutrition, or nursery stock");
            GD.Print($"P6_FOOD_RECOVERY old={oldId} new={renewed.StableId} " +
                $"time={session.GameSeconds:F2} position={renewed.GlobalPosition} " +
                $"available={session.AvailableFoodCount} actors={session.FoodActors.Count}");
            await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            Need(!session.TryConsumeFood(reeds[0]).Consumed,
                "disposed old actor awarded duplicate nutrition");
            var insect = session.FoodActors.Where(a => a.StableId.Contains(":reeds-insect:")).ToArray();
            foreach (var actor in insect)
            {
                Place(actor);
                Need(session.TryConsumeFood(actor).Consumed, "juvenile insect route missed");
            }
            Need(session.Feeding.Nutrition == 10 && session.Growth.Stage == "juvenile" &&
                session.FoodActors.Count <= PondFoodPatches.MaximumActors &&
                FeedingLedger.FoodAward(FeedingLedger.InsectId, "subadult", 300,
                    normalPondSurvival: true) == 0 &&
                FeedingLedger.FoodAward(FeedingLedger.InsectId, "subadult", 300) == 2,
                "insect nutrition or actor bound invalid");
            _firstLife = session.Growth.Identity;
            session.SetPhysicsProcess(true);
            fish.SetPhysicsProcess(true);
            session.Catch("p6-food-check");
            session.Retry();
        }
        catch (Exception error) { Fail(tree, error); }
    }

    private static bool LocalApproachClear(Main main, FishController fish, Vector3 at)
    {
        using var shape = new SphereShape3D { Radius = .045f };
        using var query = new PhysicsShapeQueryParameters3D {
            Shape = shape, Transform = new Transform3D(Basis.Identity, at + Vector3.Up * .10f),
            CollisionMask = 1, Exclude = new Godot.Collections.Array<Rid> { fish.GetRid() } };
        return main.GetWorld3D().DirectSpaceState.IntersectShape(query, 1).Count == 0;
    }

    private static Vector3 SampleNest(GeneratedBasin basin)
    {
        var shore = basin.ShorePoint(0);
        var inward = (basin.DeepCore - shore).Normalized();
        var at = shore + inward * 3f;
        return new Vector3(at.X, basin.HeightAt(at.X, at.Y) + .02f, at.Y);
    }

    private static IReadOnlyList<PondFafPlacement> SampleAnchors(GeneratedBasin basin)
    {
        var entries = new List<PondFafPlacement>();
        for (var i = 0; i < 16; i++)
        {
            var shore = basin.ShorePoint(i * 16);
            var inward = (basin.DeepCore - shore).Normalized();
            foreach (var (distance, family, id) in new[] {
                (2f, "reed", "FAF-PLANT-007B"),
                (8f, "fallen_log", "FAF-WOOD-001A"),
                (17f, "submerged_algae", "FAF-PLANT-008A") })
            {
                var at = shore + inward * distance;
                entries.Add(new PondFafPlacement(id,
                    new Vector3(at.X, basin.HeightAt(at.X, at.Y), at.Y), 0, 1, family));
            }
        }
        return entries;
    }

    private static void Need(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
    private static void Fail(SceneTree tree, Exception error)
    { GD.PushError("P6_FOOD_CHECK_FAILED " + error); tree.Quit(1); }
}
