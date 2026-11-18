#nullable enable
using Godot;
using System;
using System.Linq;

namespace Fish;

/// <summary>Seed-5266 Bass identity, occlusion, escape, physical catch and retry.</summary>
public static class P6BassRivalChecks
{
    private static string? _oldLife;
    private static string[]? _oldActors;
    private static string? _oldEvent;

    public static async void Run(PikeEncounter scene)
    {
        var tree = scene.GetTree();
        try
        {
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            var rivals = scene.BassRivals;
            var fish = scene.GetNode<FishController>("Fish");
            var bed = scene.GetNode<SeabedSurface>("Habitat/Seabed");
            Need(rivals.Count == 3 && rivals.Select(r => r.Stage)
                    .SequenceEqual(new[] { "juvenile", "subadult", "adult" }) &&
                rivals.Select(r => r.StableId).Distinct().Count() == 3 &&
                rivals.All(r => r.SpeciesId == PondGrowthState.SpeciesId &&
                    r.Growth.Identity == r.StableId && r.SchoolId == "" && r.IsAlive &&
                    r.GetNode<CollisionShape3D>("Collision").Shape is CapsuleShape3D) &&
                rivals.Select(r => r.StableId).Concat(scene.PreyActors.Select(p => p.StableId))
                    .Append(scene.Growth.Identity).Distinct().Count() == 20 &&
                scene.BluegillSchools.All(s => s.Members.All(p => p is not PondBassRival)),
                "solitary Bass stage, identity or school roster invalid");
            var adultAsset = PondCreatureCatalog.Stage(PondGrowthState.SpeciesId, "adult")!;
            foreach (var rival in rivals)
            {
                var collider = rival.GetNode<CollisionShape3D>("Collision");
                var shape = (CapsuleShape3D)collider.Shape;
                var visual = rival.GetNode<Node3D>("RivalVisual");
                Need(Mathf.Abs(shape.Radius - rival.Growth.Body.Radius) < .0001f &&
                    Mathf.Abs(shape.Height - rival.Growth.Body.Height) < .0001f &&
                    Mathf.Abs(visual.Scale.X - 1f / adultAsset.Dimensions[1]) < .0001f &&
                    rival.GlobalPosition.Y > bed.GetHeightAt(rival.GlobalPosition.X,
                        rival.GlobalPosition.Z) + shape.Radius &&
                    rival.GlobalPosition.Y < fish.PondSurfaceHeightAt(rival.GlobalPosition.X,
                        rival.GlobalPosition.Z) - shape.Radius &&
                    rival.GlobalPosition.DistanceTo(scene.NestPosition) > 3f,
                    "Bass body, scale or habitat site invalid: " + rival.StableId);
                using var probe = new PhysicsShapeQueryParameters3D {
                    Shape = shape, Transform = collider.GlobalTransform, CollisionMask = 1 };
                Need(scene.GetWorld3D().DirectSpaceState.IntersectShape(probe, 1).Count == 0,
                    "Bass spawned inside solid cover: " + rival.StableId);
            }
            if (_oldLife is not null)
            {
                Need(scene.Growth.Identity != _oldLife && scene.IntroActive &&
                    rivals.Select(r => r.StableId).SequenceEqual(_oldActors!) &&
                    rivals.All(r => r.IsAlive && r.TargetId is null &&
                        r.Feeding.Nutrition == 0 && r.MotorStepCount == 0) &&
                    scene.PredationEvents.Count == 0,
                    "retry retained Bass death, target or nutrition");
                GD.Print($"P6_BASS_RIVALS_CHECK_OK seed=5266 actors={string.Join(',', _oldActors!)} " +
                    $"old_life={_oldLife} new_life={scene.Growth.Identity} event={_oldEvent} retry=clear");
                tree.Quit();
                return;
            }
            var larva = PondCreatureCatalog.Stage(PondGrowthState.SpeciesId, "larva")!;
            var juvenile = PondCreatureCatalog.Stage(PondGrowthState.SpeciesId, "juvenile")!;
            var subadult = PondCreatureCatalog.Stage(PondGrowthState.SpeciesId, "subadult")!;
            Need(BassPredation.Eligible(adultAsset.Id, "adult", adultAsset.Dimensions[1],
                    juvenile.Id, "juvenile", juvenile.Dimensions[1]) &&
                BassPredation.Eligible(subadult.Id, "subadult", subadult.Dimensions[1],
                    larva.Id, "larva", larva.Dimensions[1]) &&
                !BassPredation.Eligible(adultAsset.Id, "adult", adultAsset.Dimensions[1],
                    subadult.Id, "subadult", subadult.Dimensions[1]) &&
                !BassPredation.Eligible(juvenile.Id, "juvenile", juvenile.Dimensions[1],
                    larva.Id, "larva", larva.Dimensions[1]) &&
                !BassPredation.Eligible(adultAsset.Id, "adult", adultAsset.Dimensions[1],
                    adultAsset.Id, "adult", adultAsset.Dimensions[1]) &&
                !BassPredation.Eligible(PikeEncounterAssets.BluegillId, "adult",
                    adultAsset.Dimensions[1], juvenile.Id, "juvenile", juvenile.Dimensions[1]) &&
                !BassPredation.Eligible(adultAsset.Id, "adult", adultAsset.Dimensions[1] * 1.1f,
                    juvenile.Id, "juvenile", juvenile.Dimensions[1]),
                "registered Bass diet or mouth eligibility invalid");
            var negative = new FeedingLedger();
            Need(!negative.TryConsumeBass(new FeedCandidate("wrong-diet",
                    PikeEncounterAssets.BluegillId, "juvenile", WorldCatalog.PondId,
                    WorldCatalog.PondId, juvenile.Dimensions[1], "adult",
                    adultAsset.Dimensions[1], true), true, 1).Consumed &&
                negative.Nutrition == 0 && negative.Events.Count == 0,
                "invalid diet awarded nutrition");

            scene.SetPhysicsProcess(false);
            fish.SetPhysicsProcess(false);
            scene.CompleteHatch();
            for (var i = 0; i < 300 && scene.IntroActive; i++)
                await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            Need(!scene.IntroActive && scene.Growth.Stage == "larva", "Bass hatch failed");
            var hunter = rivals[2];
            var small = rivals[0];
            var hunterStart = hunter.GlobalPosition;
            var start = Vector3.Zero;
            var found = false;
            for (var i = 0; i < 32; i++)
            {
                var angle = i * Mathf.Tau / 32f;
                var direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                var candidate = hunterStart + direction * 2.1f;
                if (candidate.Y < bed.GetHeightAt(candidate.X, candidate.Z) + .25f ||
                    candidate.Y > fish.PondSurfaceHeightAt(candidate.X, candidate.Z) - .25f ||
                    hunter.TestMove(hunter.GlobalTransform, direction * .7f)) continue;
                using var lane = PhysicsRayQueryParameters3D.Create(hunterStart, candidate, 1);
                if (scene.GetWorld3D().DirectSpaceState.IntersectRay(lane).Count > 0) continue;
                small.GlobalPosition = candidate;
                hunter.LookAt(candidate, Vector3.Up);
                small.LookAt(hunterStart, Vector3.Up);
                hunter.ResetSight();
                var seen = hunter.Observe(small.StableId, small.GetRid(), candidate,
                    small.Growth.Body.Radius, scene.SightWater!, 1);
                if (!seen.Detected) continue;
                start = candidate;
                found = true;
                break;
            }
            Need(found, "no exposed adult Bass sight lane");
            var barrier = new StaticBody3D { Name = "BassSightBarrierCheck", CollisionLayer = 1 };
            scene.AddChild(barrier);
            barrier.GlobalPosition = hunterStart.Lerp(start, .5f);
            barrier.LookAt(start, Vector3.Up);
            barrier.AddChild(new CollisionShape3D { Shape = new BoxShape3D {
                Size = new Vector3(1f, .7f, .06f) } });
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            hunter.ResetSight();
            var blocked = hunter.Observe(small.StableId, small.GetRid(), start,
                small.Growth.Body.Radius, scene.SightWater!, 1.3);
            Need(!blocked.Detected && blocked.ClearRays == 0,
                "solid cover did not block Bass sight");
            barrier.Free();
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            hunter.ResetSight();
            Need(hunter.Observe(small.StableId, small.GetRid(), start,
                small.Growth.Body.Radius, scene.SightWater!, 1.6).Detected,
                "exposed Bass sight did not recover");

            small.ResetSight();
            var initialDistance = small.GlobalPosition.DistanceTo(hunterStart);
            Need(small.Observe(hunter.StableId, hunter.GetRid(), hunterStart,
                hunter.Growth.Body.Radius, scene.SightWater!, 2).Detected,
                "small Bass did not sense adult");
            hunter.Track(small.StableId);
            for (var i = 0; i < 450; i++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                var now = 2.0 + i / 60.0;
                var threat = small.Observe(hunter.StableId, hunter.GetRid(), hunterStart,
                    hunter.Growth.Body.Radius, scene.SightWater!, now);
                small.Tick(threat, default, now, 1f / 60f, false);
                hunter.Hunt.Step(hunter.Observe(small.StableId, small.GetRid(),
                    small.GlobalPosition, small.Growth.Body.Radius, scene.SightWater!, now),
                    default, now, false);
            }
            Need(small.IsAlive && small.MotorStepCount == 450 &&
                small.GlobalPosition.DistanceTo(start) > .8f &&
                small.GlobalPosition.DistanceTo(hunterStart) > initialDistance + .8f &&
                !scene.TryBassRivalCatch(hunter, small.StableId, 9.5),
                "small Bass did not escape through real motor movement");
            hunter.Hunt.Step(default, default, 13, false);
            hunter.Hunt.Step(default, default, 16, false);
            Need(hunter.Hunt.Mode == BluegillThreatMode.Patrol &&
                hunter.Hunt.Destination is null, "Bass retained omniscient target memory");
            GD.Print($"P6_BASS_RIVAL_ESCAPE prey={small.StableId} displacement=" +
                $"{small.GlobalPosition.DistanceTo(start):F2} distance=" +
                $"{small.GlobalPosition.DistanceTo(hunterStart):F2} memory={hunter.Hunt.Mode}");

            small.GlobalPosition = start;
            small.ResetActor();
            hunter.GlobalPosition = hunterStart;
            hunter.LookAt(start, Vector3.Up);
            hunter.Track(null);
            hunter.ResetSight();
            hunter.Track(small.StableId);
            var motorStart = hunter.MotorStepCount;
            var eaten = false;
            for (var i = 0; i < 240 && !eaten; i++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                var now = 20.0 + i / 60.0;
                var sight = hunter.Observe(small.StableId, small.GetRid(),
                    small.GlobalPosition, small.Growth.Body.Radius, scene.SightWater!, now);
                hunter.Hunt.Step(sight, default, now, false);
                hunter.Tick(default, default, now, 1f / 60f, false,
                    huntGoal: hunter.Hunt.Mode == BluegillThreatMode.Chase
                        ? hunter.Hunt.Destination : null);
                if (hunter.Hunt.StrikeReady)
                    eaten = scene.TryBassRivalCatch(hunter, small.StableId, now);
            }
            Need(eaten && !small.IsAlive && hunter.MotorStepCount > motorStart &&
                hunter.GlobalPosition.DistanceTo(hunterStart) > .3f &&
                hunter.Feeding.Nutrition == 6 && hunter.Feeding.Events.Count == 1 &&
                hunter.Feeding.IsConsumed(small.StableId) && hunter.TargetId is null,
                "adult Bass did not physically consume juvenile exactly once");
            var eventId = hunter.StableId + ":ate:" + small.StableId;
            Need(scene.PredationEvents.Count(e => e.Id == eventId) == 1 &&
                !scene.TryBassRivalCatch(hunter, small.StableId, 25) &&
                hunter.Feeding.Nutrition == 6 && scene.PredationEvents.Count == 1,
                "Bass duplicate nutrition or predator event");
            tree.Paused = true;
            var pausedAt = hunter.GlobalPosition;
            var steps = hunter.MotorStepCount;
            hunter.Tick(default, default, 30, 1f, true);
            Need(hunter.GlobalPosition == pausedAt && hunter.MotorStepCount == steps,
                "paused rival moved or advanced motor");
            tree.Paused = false;
            Need(rivals.Sum(r => r.SightSamples) <= 300 &&
                rivals.All(r => r.SchoolId == ""), "Bass sensing or cohesion cost unbounded");
            hunter.GlobalPosition = hunterStart;
            hunter.ResetActor();
            hunter.LookAt(start, Vector3.Up);
            hunter.Track(null);
            hunter.ResetSight();
            fish.GlobalPosition = start;
            hunter.Track(scene.Growth.Identity);
            var caughtPlayer = false;
            for (var i = 0; i < 240 && !caughtPlayer; i++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                var now = 30.0 + i / 60.0;
                var sight = hunter.Observe(scene.Growth.Identity, fish.GetRid(),
                    fish.GlobalPosition, scene.Growth.ColliderRadius, scene.SightWater!, now);
                hunter.Hunt.Step(sight, default, now, false);
                hunter.Tick(default, default, now, 1f / 60f, false,
                    huntGoal: hunter.Hunt.Mode == BluegillThreatMode.Chase
                        ? hunter.Hunt.Destination : null);
                if (hunter.Hunt.StrikeReady)
                    caughtPlayer = scene.TryBassRivalCatch(hunter, scene.Growth.Identity, now);
            }
            var playerEvent = hunter.StableId + ":ate:" + scene.Growth.Identity;
            Need(caughtPlayer && scene.Outcome == "caught" &&
                scene.PredationEvents.Count(e => e.Id == playerEvent) == 1 &&
                hunter.Feeding.Nutrition == 8 && hunter.Feeding.Events.Count == 2 &&
                !scene.TryBassRivalCatch(hunter, scene.Growth.Identity, 35),
                "eligible player Bass did not end in one physical rival catch");
            _oldLife = scene.Growth.Identity;
            _oldActors = rivals.Select(r => r.StableId).ToArray();
            _oldEvent = eventId;
            GD.Print($"P6_BASS_RIVAL_CATCH_OK predator={hunter.StableId} prey={small.StableId} " +
                $"event={eventId} player_event={playerEvent} samples={rivals.Sum(r => r.SightSamples)}");
            (scene.FindChild("RetryEncounter", true, false) as Button)!
                .EmitSignal(BaseButton.SignalName.Pressed);
        }
        catch (Exception error)
        {
            GD.PushError("P6_BASS_RIVALS_CHECK_FAILED " + error);
            tree.Quit(1);
        }
    }

    private static void Need(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
