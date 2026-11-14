#nullable enable
using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fish;

/// <summary>Deterministic Pond prey/cover integration; no owner gameplay claim.</summary>
public static class PreyChecks
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
            var prey = scene.GetNode<PreyActor>("BluegillPrey0");
            var school = scene.PreyActors.Where(p => p.Stage == "juvenile").ToArray();
            Need(school.Length == 4 && school.Select(p => p.GlobalPosition).Distinct().Count() == 4,
                "juvenile group missing or overlapping spawn positions");
            var pike = scene.GetNode<PikeActor>("NorthernPike");
            var plants = PikeCoverFixture.VisibleSoftCover(scene.GetNode<Node3D>("Cover"));
            Need(SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.PondId,
                PikeEncounterAssets.BluegillId, "juvenile", SpeciesRole.Ecological, out var stage, out _) &&
                stage is not null && stage.Dimensions[1] > .09f && stage.Dimensions[1] < .11f &&
                PondCreatureCatalog.Stage("FAF-FISH-007", "adult") is { } bassAdult &&
                PondCreatureCatalog.Stage("FAF-FISH-006", "adult") is { } bluegillAdult &&
                bassAdult.Dimensions[1] > bluegillAdult.Dimensions[1] &&
                !SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.OceanId,
                    PikeEncounterAssets.BluegillId, "juvenile", SpeciesRole.Ecological, out _, out _) &&
                !SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.PondId,
                    PikeEncounterAssets.BluegillId, "adult", SpeciesRole.Selectable, out _, out _),
                "Pond juvenile eligibility or size drift");
            Need(prey.SpeciesId == PikeEncounterAssets.BluegillId &&
                prey.GetNode<CollisionShape3D>("Collision").Shape is SphereShape3D { Radius: .075f } &&
                prey.GetNode<AnimationPlayer>("PreyVisual/AnimationPlayer").HasAnimation(stage!.Clip("SwimFast")),
                "prey lacks body or authentic swim rig");
            Need(plants.Any(p => p.FafId.StartsWith("FAF-PLANT-012")) &&
                plants.Any(p => p.FafId.StartsWith("FAF-PLANT-014")) &&
                plants.Any(p => p.FafId.StartsWith("FAF-PLANT-010")),
                "new FAF cover families unavailable");

            var sight = scene.PreySight!;
            var cues = scene.PreyCues!;
            var env = scene.SightWater!;
            foreach (var member in school)
            {
                var spawn = member.GlobalPosition;
                Need(spawn.DistanceTo(fish.GlobalPosition) is > 3f and < 9f &&
                    spawn.Z < fish.GlobalPosition.Z && !InsideWood(member),
                    "juvenile spawn is too far, behind player or inside solid cover: " + member.Name);
            }
            var forageTravel = 0f;
            foreach (var larvaName in new[] { "BluegillLarva0", "BluegillLarva2" })
            {
                var larva = scene.GetNode<PreyActor>(larvaName);
                var home = larva.GlobalPosition;
                var farthest = 0f;
                for (var frame = 0; frame < 180; frame++)
                {
                    larva.Tick(default, default, frame / 60d, 1f / 60f, false);
                    await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                    farthest = Mathf.Max(farthest, larva.GlobalPosition.DistanceTo(home));
                    Need(larva.GlobalPosition.DistanceTo(home) < 1.5f && !InsideWood(larva),
                        "undisturbed larva left its home cover or entered wood: " + larvaName);
                }
                Need(farthest > .25f && larva.Mind.Mode == PreyMode.Forage,
                    "undisturbed larva remained motionless: " + larvaName);
                var heldLarva = larva.GlobalPosition;
                larva.Tick(default, default, 3, 1f / 60f, true);
                Need(larva.GlobalPosition == heldLarva, "pause moved foraging larva");
                forageTravel = Mathf.Max(forageTravel, farthest);
            }
            // Retain the P7 fixed sensor comparison pose independently of the
            // nearer P8 discovery spawn.
            var from = new Vector3(-6.8f, PikeCoverFixture.Bed(-6.8f, 11f) + .55f, 11f);
            SightEvidence Query(Vector3 target, bool soft = true)
            {
                var sensor = new WorldSightSensor(scene.GetWorld3D().DirectSpaceState,
                    soft ? plants : Array.Empty<SoftSightCover>(), prey.GetRid(), fish.GetRid(), range: 24f);
                return sensor.Observe(from, target - from,
                    new SightTarget(target, .48f, 1f), env, 0, false);
            }
            var grass = scene.CoverPlan.First(p => p.Id == "FAF-PLANT-010C").Position;
            var log = scene.CoverPlan.First(p => p.Id == "FAF-WOOD-005B").Position;
            var cautious = new[]
            {
                new Vector3(grass.X, from.Y, grass.Z),
                new Vector3(log.X + 2f, from.Y, log.Z - 4f),
            };
            var exposed = new[]
            {
                from + new Vector3(0, 0, 3.5f),
                from + new Vector3(3f, 0, 3f),
                from + new Vector3(4f, 0, 1f),
            };
            var careful = cautious.Select(point => Query(point)).ToArray();
            var open = exposed.Select(point => Query(point)).ToArray();
            Need(open.Any(v => v.Detected && v.ClearRays > 0),
                "open crossing did not expose Bluegill");
            Need(careful.Any(v => !v.Detected),
                "grass/log approach did not reduce real visual evidence");
            var plantAttenuation = 0f;
            for (var x = -11; x <= -3; x++)
            for (var z = 12; z <= 21; z++)
            {
                var target = new Vector3(x, from.Y, z);
                var covered = Query(target);
                var bare = Query(target, false);
                if (covered.ClearRays > 0 && bare.ClearRays > 0)
                    plantAttenuation = Mathf.Max(plantAttenuation, bare.Strength - covered.Strength);
            }
            Need(plantAttenuation > .001f, "visible ribbon/weed meshes did not attenuate prey sight");
            var openTarget = exposed[Array.FindIndex(open, v => v.Detected)];
            var visible = Query(openTarget);
            var camera = fish.GetNode<Camera3D>("CameraRig/Camera3D");
            var savedCamera = camera.Transform;
            camera.Position += new Vector3(9, 4, -7);
            Need(visible.Equals(Query(openTarget)), "camera changed prey sight");
            camera.Transform = savedCamera;

            var mind = new PreyAwareness();
            mind.Step(default, default, 0, false);
            Need(mind.Mode == PreyMode.Forage && mind.LastSensedPosition is null,
                "prey knew an unseen player");
            mind.Step(visible, default, 0, false);
            Need(mind.Mode == PreyMode.Flee && mind.LastSensedPosition == openTarget,
                "shared sight did not trigger prey flight");
            mind.Step(default, default, 1, true);
            Need(mind.LastSensedAt == 0 && mind.Mode == PreyMode.Flee,
                "pause advanced prey memory");
            mind.Step(default, default, 3, false);
            Need(mind.Mode == PreyMode.Settle && mind.LastSensedPosition is null,
                "prey retained stale threat knowledge");
            mind.Step(default, default, 5, false);
            Need(mind.Mode == PreyMode.Forage, "prey did not return to forage");
            mind.Reset();
            Need(mind.LastSensedPosition is null && mind.Mode == PreyMode.Forage,
                "prey reset retained state");

            cues.Reset();
            cues.Step(openTarget, Vector3.Zero, 0, 0, 1d / 60d, 1, false);
            cues.Step(openTarget, new Vector3(0, 0, -6), 1, 1, 1d / 60d, 2, false);
            var fastCue = cues.Sense(from, false);
            cues.Reset();
            cues.Step(cautious[0], new Vector3(0, 0, -.2f), .1f, 0,
                1d / 60d, 1, false);
            var slowCue = cues.Sense(from, false);
            Need(fastCue.Strength > slowCue.Strength && fastCue.Events > slowCue.Events,
                $"fast/open disturbance was not stronger fast={fastCue} slow={slowCue}");
            mind.Step(default, fastCue, .04, false);
            Need(mind.Mode == PreyMode.Alert && mind.LastSensedPosition == openTarget,
                "prey ignored local disturbance position");
            mind.Step(default, default, .5, false);
            Need(mind.Mode == PreyMode.Flee, "prey did not flee after local warning");
            mind.Reset();

            prey.ResetActor();
            prey.GlobalPosition = from;
            var start = prey.GlobalPosition;
            var firstCover = Vector3.Zero;
            for (var frame = 0; frame < 180; frame++)
            {
                var sample = frame < 10 ? visible with { SensedAt = frame / 60d } : default;
                prey.Tick(sample, default, frame / 60d, 1f / 60f, false);
                if (frame == 0) firstCover = prey.CoverGoal;
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                Need(!InsideWood(), "prey entered solid wood");
            }
            Need(prey.GlobalPosition.DistanceTo(start) > 1f && prey.CoverGoal != start &&
                pike.Mind.Mode == PikeMode.Patrol, "prey did not flee independently of Pike");
            var firstFamily = plants.OrderBy(p => p.VisibleBounds.GetCenter().DistanceTo(firstCover))
                .First().FafId;
            Need(firstFamily.StartsWith("FAF-PLANT-012") ||
                firstFamily.StartsWith("FAF-PLANT-014"),
                "prey chose sparse cover over coontail/thicket: " + firstFamily);
            var held = prey.GlobalPosition;
            var chosenCover = prey.CoverGoal;
            var remembered = prey.Mind.LastSensedPosition;
            prey.Tick(visible, default, 4, 1f / 60f, true);
            Need(prey.GlobalPosition == held && prey.Mind.LastSensedPosition == remembered,
                "pause moved prey or rewrote evidence");
            var root = scene.CoverPlan.Single(p => p.Id == "FAF-WOOD-007A");
            var rootBasis = new Basis(Vector3.Up, root.Yaw);
            prey.GlobalPosition = root.Position + rootBasis * new Vector3(0, 1.4f, 6.4f);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(!prey.TestMove(prey.GlobalTransform, rootBasis * new Vector3(0, 0, -2.8f)),
                "prey cannot enter real root-ball refuge gap");
            var split = scene.CoverPlan.Single(p => p.Id == "FAF-WOOD-008A");
            var splitBasis = new Basis(Vector3.Up, split.Yaw);
            var sealedWood = false;
            foreach (var y in new[] { .8f, 1.4f, 2.3f, 3.2f })
            foreach (var z in new[] { -2f, 0f, 2f })
            {
                prey.GlobalPosition = split.Position + splitBasis * new Vector3(-4f, y, z);
                sealedWood |= prey.TestMove(prey.GlobalTransform, splitBasis * new Vector3(8f, 0, 0));
            }
            Need(sealedWood, "prey body phased through sealed split wood");
            var fork = scene.CoverPlan.Single(p => p.Id == "FAF-WOOD-006A");
            var forkBasis = new Basis(Vector3.Up, fork.Yaw);
            var forkStart = fork.Position + forkBasis * new Vector3(-5.6f, 2.3f, -2.2f);
            prey.GlobalPosition = forkStart;
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(!InsideWood(), "fork approach starts in solid wood");
            prey.ResetActor();
            var forkThreat = forkStart + forkBasis * new Vector3(-3f, 0, 0);
            for (var frame = 0; frame < 240; frame++)
            {
                var sample = frame < 10
                    ? new SightEvidence(true, .2f, "visible", 1, 1, 1, 3, 3, 7,
                        forkThreat, frame / 60d)
                    : default;
                prey.Tick(sample, default, frame / 60d, 1f / 60f, false);
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                Need(!InsideWood(), "fleeing prey penetrated fork log");
            }
            var forkEnd = forkBasis.Inverse() * (prey.GlobalPosition - fork.Position);
            Need(forkEnd.X > -3f, "prey did not steer around the forked log gap: " + forkEnd);
            prey.ResetActor();
            sight.Reset();
            cues.Reset();
            Need(prey.Mind.Mode == PreyMode.Forage && prey.Mind.LastSensedPosition is null &&
                cues.Events.Count == 0, "prey/field reset incomplete");
            GD.Print($"PREY_CHECK_OK seed={PikeCoverFixture.Seed} stage=FAF-FISH-006:juvenile forage_travel={forageTravel:F2} grass={careful[0].Reason}/{careful[0].Strength:F4} log={careful[1].Reason}/{careful[1].Strength:F4} open={open.Max(v => v.Strength):F4} plant_attenuation={plantAttenuation:F4} fast_cue={fastCue.Strength:F4} slow_cue={slowCue.Strength:F4} flee={held.DistanceTo(start):F2} first_cover={firstFamily}:{firstCover} last_cover={chosenCover} root_gap=clear fork_end={forkEnd} split_wood=solid");
            tree.Quit();

            bool InsideWood(PreyActor? candidate = null)
            {
                var actor = candidate ?? prey;
                var collision = actor.GetNode<CollisionShape3D>("Collision");
                var hits = scene.GetWorld3D().DirectSpaceState.IntersectShape(
                    new PhysicsShapeQueryParameters3D
                    {
                        Shape = new SphereShape3D { Radius = .055f },
                        Transform = collision.GlobalTransform,
                        CollisionMask = 1,
                        Margin = 0,
                        Exclude = new Godot.Collections.Array<Rid> { actor.GetRid(),
                            fish.GetRid(), pike.GetRid() },
                    }, 1);
                return hits.Count > 0;
            }
        }
        catch (Exception error)
        {
            GD.PushError("PREY_CHECK_FAILED " + error);
            tree.Quit(1);
        }
    }

    private static void Need(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
