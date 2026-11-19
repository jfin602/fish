#nullable enable
using Godot;
using System;
using System.Diagnostics;
using System.Linq;

namespace Fish;

/// <summary>Seeded, player-distant real Pond actors and one-prey claim checks.</summary>
public static class P6EcologyChecks
{
    public static async void Run(PikeEncounter scene)
    {
        var tree = scene.GetTree();
        try
        {
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            var fish = scene.GetNode<FishController>("Fish");
            var bed = scene.GetNode<SeabedSurface>("Habitat/Seabed");
            var frog = scene.Frog!;
            var snake = scene.Snake!;
            var pike = scene.GetNode<PikeActor>("NorthernPike");
            var worldId = WorldCatalog.Resolve(WorldCatalog.PondId).Bake!.WorldId;
            Need(scene.PreyActors.Count == 16 && scene.BassRivals.Count == 3 &&
                scene.BluegillSchools.SelectMany(s => s.Members).Distinct().Count() == 16 &&
                frog.StableId != snake.StableId &&
                scene.PreyActors.All(p => p.StableId.StartsWith(worldId + ":bluegill-",
                    StringComparison.Ordinal)) &&
                pike.StableId == worldId + ":pike:deep:0" &&
                WorldCatalog.Resolve(WorldCatalog.PondId).Bake!.Seed == 5266,
                "seeded ordinary Pond actor roster invalid");
            Need(!PikeEncounterAssets.TryValidate(out _, out _, out var missing,
                    scenePath: "res://scenes/missing-ecology.tscn") &&
                missing.StartsWith("PIKE_SCENE_MISSING", StringComparison.Ordinal) &&
                !SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.OceanId,
                    PondFrogActor.SpeciesId, null, SpeciesRole.Ecological, out _, out _) &&
                !SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.OceanId,
                    PondSnakeActor.SpeciesId, null, SpeciesRole.Ecological, out _, out _),
                "missing asset or Ocean species isolation failed");
            var oldLife = scene.Growth.Identity;
            scene.CompleteHatch();
            for (var i = 0; i < 300 && scene.IntroActive; i++)
                await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            Need(!scene.IntroActive, "hatch did not finish");
            scene.SetPhysicsProcess(false);
            fish.SetPhysicsProcess(false);
            fish.GlobalPosition = new Vector3(55, 2, 55);
            var larvae = scene.PreyActors.Where(p => p.Stage == "larva").ToArray();
            var frogPrey = larvae[0];
            var snakePrey = larvae[1];
            var bluegillPrey = larvae[2];
            var frogSite = frog.Route[2];
            var snakeSite = snake.Route[2];
            frogSite.Y = fish.PondSurfaceHeightAt(frogSite.X, frogSite.Z) - .055f;
            snakeSite.Y = fish.PondSurfaceHeightAt(snakeSite.X, snakeSite.Z) - .055f;
            var adultBluegill = scene.PreyActors.Last(p => p.Stage == "adult");
            var bluegillSite = adultBluegill.GlobalPosition - adultBluegill.GlobalBasis.Z * .55f;
            bluegillSite.Y = adultBluegill.GlobalPosition.Y;
            var adultBass = scene.BassRivals.Last();
            var juvenileBluegill = scene.PreyActors.First(p => p.Stage == "juvenile");
            var bassSite = adultBass.GlobalPosition - adultBass.GlobalBasis.Z * .65f;
            bassSite.Y = adultBass.GlobalPosition.Y;
            var pikePrey = scene.PreyActors.Where(p => p.Stage == "juvenile").Skip(1).First();
            var pikeSite = pike.GlobalPosition - pike.GlobalBasis.Z * 2.4f;
            pikeSite.Y = pike.GlobalPosition.Y;
            var watch = Stopwatch.StartNew();
            const float step = 1f / 60f;
            for (var frame = 0; frame < 1500; frame++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                if (frogPrey.IsAlive) frogPrey.GlobalPosition = frogSite;
                if (snakePrey.IsAlive) snakePrey.GlobalPosition = snakeSite;
                if (bluegillPrey.IsAlive) bluegillPrey.GlobalPosition = bluegillSite;
                if (juvenileBluegill.IsAlive) juvenileBluegill.GlobalPosition = bassSite;
                if (pikePrey.IsAlive) pikePrey.GlobalPosition = pikeSite;
                scene._PhysicsProcess(step);
                if (frame % 300 == 0)
                {
                    var d = scene.EcologyDiagnostics;
                    GD.Print($"P6_ECOLOGY_TRACE frame={frame} seconds={scene.GameSeconds:F2} " +
                        $"actors={d.ActiveActors} events={d.Interactions} queries={d.Queries} " +
                        $"checks={d.CandidateChecks} samples={d.SightSamples} search={d.SearchStates}");
                }
                if (scene.PredationEvents.Select(e => e.PredatorId).Distinct().Count() >= 3)
                    break;
            }
            // Keep the frog's eligible fry in its actual shore lane while the
            // other hunters are held, then let the frog's own sight and motor work.
            var frogVictim = scene.PreyActors.First(p => p.IsAlive && p.Stage == "larva");
            frog.GlobalPosition = frog.Route[1];
            frog.LookAt(frog.Route[2], Vector3.Up);
            var frogSeconds = Math.Max(scene.GameSeconds, 12.0);
            for (var frame = 0; frame < 1100 && frogVictim.IsAlive; frame++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                frogVictim.GlobalPosition = frogSite;
                frogSeconds += step;
                scene.TickFrogForEcologyCheck(frogSeconds, step);
            }
            Need(!frogVictim.IsAlive && scene.PredationEvents.Any(e =>
                e.PredatorId == frog.StableId && e.TargetId == frogVictim.StableId),
                $"frog failed physical NPC shore catch mode={frog.Mode} target={frog.TargetId}");

            // Pike's close strike is driven by real world sight, committed mind,
            // and the same physical mouth check used in normal play.
            var pikeVictim = scene.PreyActors.First(p => p.IsAlive && p.Stage == "juvenile");
            pikeVictim.GlobalPosition = pike.MouthPosition;
            var pikeSeconds = frogSeconds + 1;
            var sensor = new WorldSightSensor(scene.GetWorld3D().DirectSpaceState,
                Array.Empty<SoftSightCover>(), pike.GetRid(), pikeVictim.GetRid(), 30f, 2.5f);
            var seen = sensor.Observe(pike.GlobalPosition, -pike.GlobalBasis.Z,
                new SightTarget(pikeVictim.GlobalPosition, .03f, 1f),
                scene.SightWater!, pikeSeconds, false);
            Need(seen.Detected && seen.ClearRays > 0, "Pike close NPC sight unavailable");
            pike.Mind.Retarget(pikeVictim.StableId, false);
            pike.Mind.Step(pike.GlobalPosition, seen, default, pikeSeconds, false);
            seen = sensor.Observe(pike.GlobalPosition, -pike.GlobalBasis.Z,
                new SightTarget(pikeVictim.GlobalPosition, .03f, 1f),
                scene.SightWater!, pikeSeconds + .65, false);
            pike.Mind.Step(pike.GlobalPosition, seen, default, pikeSeconds + .65, false);
            Need(pike.Mind.StrikeReady &&
                scene.TryPikeConsumePrey(pikeVictim, pikeSeconds + .65) &&
                !pikeVictim.IsAlive && !scene.TryPikeConsumePrey(pikeVictim, pikeSeconds + .65),
                "Pike physical NPC catch or duplicate guard failed");
            watch.Stop();
            var npcEvents = scene.PredationEvents.Where(e => e.TargetId != oldLife).ToArray();
            var diagnostics = scene.EcologyDiagnostics;
            GD.Print($"P6_ECOLOGY_OUTCOME events={string.Join(';', npcEvents.Select(e => e.Id))} " +
                $"frog={frog.Mode}/{frog.TargetId} snake={snake.Mode}/{snake.TargetId} " +
                $"pike={pike.Mind.Mode}/{pike.Mind.TargetId} " +
                $"actors={diagnostics.ActiveActors} queries={diagnostics.Queries} " +
                $"candidate_checks={diagnostics.CandidateChecks} samples={diagnostics.SightSamples} " +
                $"search={diagnostics.SearchStates} wall_ms={watch.ElapsedMilliseconds}");
            Need(npcEvents.Length >= 5 &&
                npcEvents.Select(e => e.PredatorId).Distinct().Count() >= 5 &&
                npcEvents.Select(e => e.TargetId).Distinct().Count() == npcEvents.Length &&
                npcEvents.All(e => e.TargetId != oldLife) &&
                npcEvents.All(e => e.Contact.DistanceTo(fish.GlobalPosition) > 20f) &&
                !scene.GetNode<FishController>("Fish").IsQueuedForDeletion() &&
                diagnostics.CandidateChecks <= diagnostics.Queries * PondEcology.MaxActors &&
                diagnostics.ActiveActors <= PondEcology.MaxActors,
                "player-absent multi-species predation or bounded work failed");
            var remaining = scene.PreyActors.First(p => p.IsAlive && p.Stage == "larva");
            var claims = 0;
            scene.Ecology.Refresh(new[] { new PondEcologyCandidate(remaining.StableId,
                remaining.SpeciesId, remaining.Stage, remaining.Length, .006f,
                remaining.GetRid(), remaining, false, true) }, 100);
            Need(!scene.Ecology.TryClaimNpc(remaining.StableId, remaining.GetRid(), () => false) &&
                scene.Ecology.TryClaimNpc(remaining.StableId, remaining.GetRid(), () =>
                    { claims++; remaining.Consume(); return true; }) &&
                !scene.Ecology.TryClaimNpc(remaining.StableId, remaining.GetRid(), () =>
                    { claims++; return true; }) && claims == 1 && !remaining.IsAlive &&
                remaining.CollisionLayer == 0 && !remaining.Visible &&
                scene.Ecology.Nearby(remaining.GlobalPosition, 1, "other", _ => true).Count == 0,
                "contested prey was claimed twice or remained targetable");
            var beforePause = scene.EcologyDiagnostics;
            var clockBefore = scene.GameSeconds;
            scene._Notification((int)Node.NotificationApplicationFocusOut);
            Need(tree.Paused, "focus loss did not pause the scene");
            scene._PhysicsProcess(step);
            Need(scene.EcologyDiagnostics == beforePause &&
                scene.GameSeconds == clockBefore,
                "paused ecology or game clock advanced");
            tree.Paused = false;
            var oldScene = scene;
            Need(tree.ChangeSceneToFile("res://scenes/world_select.tscn") == Error.Ok,
                "scene exit failed");
            for (var i = 0; i < 3; i++)
                await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            Need(tree.CurrentScene is WorldSelect && !GodotObject.IsInstanceValid(oldScene),
                "scene-owned ecology survived exit");
            GD.Print($"P6_ECOLOGY_CHECK_OK seed=5266 events={npcEvents.Length} " +
                $"predators={npcEvents.Select(e => e.PredatorId).Distinct().Count()} " +
                $"claims={diagnostics.ClaimedPrey} actor_cap={PondEcology.MaxActors} " +
                $"nearby_cap={PondEcology.MaxNearby} wall_ms={watch.ElapsedMilliseconds} " +
                "focus=pause contested=one exit=clear ocean=isolated missing_asset=rejected");
            tree.Quit();
        }
        catch (Exception error)
        {
            tree.Paused = false;
            GD.PushError("P6_ECOLOGY_CHECK_FAILED " + error);
            tree.Quit(1);
        }
    }

    private static void Need(bool condition, string reason)
    { if (!condition) throw new InvalidOperationException(reason); }
}
