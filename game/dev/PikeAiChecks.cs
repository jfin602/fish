#nullable enable
using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fish;

/// <summary>Deterministic generated-Pond decision and placed-cover probe; no native fairness claim.</summary>
public static class PikeAiChecks
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
            var actor = scene.GetNode<PikeActor>("NorthernPike");
            var visual = actor.GetNode<Node3D>("PikeVisual");
            var shape = actor.GetNode<CollisionShape3D>("Collision").Shape as CapsuleShape3D;
            Need(PikeEncounterAssets.TryValidate(out _, out var pikeEntry, out _) &&
                pikeEntry is not null &&
                visual.Scale.IsEqualApprox(Vector3.One * (pikeEntry.WorldScale * 3f)) &&
                shape is not null && Mathf.IsEqualApprox(shape.Radius, .42f) &&
                Mathf.IsEqualApprox(shape.Height, 2.65f), "Pike visual/slim body drift");
            Need(actor.GetNode<AnimationPlayer>("PikeVisual/AnimationPlayer").CurrentAnimation ==
                pikeEntry!.SwimClip, "Pike patrol did not start its swim animation");
            var contactPosition = actor.GlobalPosition;
            var contactBasis = actor.GlobalBasis;
            actor.GlobalPosition = new Vector3(0, 4, 0);
            actor.GlobalBasis = Basis.LookingAt(Vector3.Forward, Vector3.Up);
            var fishPosition = fish.GlobalPosition;
            var fishBasis = fish.GlobalBasis;
            fish.GlobalBasis = Basis.Identity;
            fish.GlobalPosition = new Vector3(0, 4, -2.3f);
            Need(!actor.MouthCanReach(fish), "Pike bite reached across open water");
            fish.GlobalPosition = new Vector3(0, 4, -1.48f);
            Need(actor.MouthCanReach(fish), "Pike missed physical Bass contact");
            fish.GlobalPosition = new Vector3(0, 4, 1.48f);
            Need(!actor.MouthCanReach(fish), "Pike bite reached behind body");
            fish.GlobalPosition = fishPosition;
            fish.GlobalBasis = fishBasis;
            actor.GlobalPosition = contactPosition;
            actor.GlobalBasis = contactBasis;
            var strong = new SightEvidence(true, .4f, "visible", 1, 1, 1, 3, 3, 7,
                new Vector3(0, 1, 0), 0);
            var weak = new SightEvidence(false, .025f, "weak-evidence", 1, 1, 1, 3, 3, 7);
            var hidden = new SightEvidence(false, 0, "hard-cover", 0, 0, 0, 0, 3, 0);
            var mind = new PikeAwareness();
            var origin = new Vector3(0, 1, 5);
            var target = new Vector3(0, 1, 0);
            mind.Step(origin, weak, default, 0, false);
            Need(mind.Mode == PikeMode.Patrol && mind.LastSensedPosition is null,
                "insufficient silhouette created knowledge");
            mind.Step(origin, strong with { SensedPosition = null }, default, 0, false);
            Need(mind.LastSensedPosition is null, "unlocated sight created knowledge");
            mind.Step(origin, default, default, 12, false);
            Need(mind.Mode == PikeMode.Patrol && !mind.HadVerifiedChase,
                "absent target created chase");
            var cue = new DisturbanceEvidence(.07f, 1, false, 1f, target, .1);
            mind.Step(origin, weak, cue, .1, false);
            Need(mind.Mode == PikeMode.Suspicious && mind.LastSensedPosition == target &&
                !mind.HadVerifiedChase, "slow grass-edge cue did not cause suspicion");
            mind.Step(origin, hidden, default, .5, false);
            Need(mind.Mode == PikeMode.Investigate && mind.Destination(.5, origin) == target,
                "investigation did not use sensed cue position");
            mind.Step(origin, hidden, default, 3.2, false);
            Need(mind.Mode == PikeMode.Disengage && !mind.Escaped,
                "unverified cue fabricated escape");
            mind.Reset();
            Need(mind.Mode == PikeMode.Patrol && mind.LastSensedPosition is null &&
                !mind.Escaped && !mind.HadVerifiedChase, "reset retained cue memory");
            mind.Step(origin, strong, default, 1, false);
            Need(mind.Mode == PikeMode.Patrol && mind.LastSensedPosition is null,
                "stale sight sample created knowledge");

            mind.Step(origin, strong, default, 0, false);
            Need(mind.Mode == PikeMode.Chase && mind.LastSensedPosition == target,
                "exposed crossing did not start verified chase");
            mind.Step(origin, strong, default, .1, false);
            Need(mind.LastSensedPosition == target && mind.LastSensedAt == 0,
                "cached sight refreshed the hidden position or timestamp");
            var caughtAt = new Vector3(0, 1, 1.5f);
            mind.Step(caughtAt, strong with { SensedAt = 1 }, default, 1, false);
            Need(mind.Mode == PikeMode.Commit && !mind.StrikeReady,
                "strike lacked readable windup");
            mind.Step(caughtAt, strong with { SensedAt = 1.6 }, default, 1.6, false);
            Need(mind.StrikeReady, "committed attack never became ready");
            mind.Catch(1.6);
            Need(mind.Mode == PikeMode.Caught, "open-water catch did not terminate fixture threat");
            mind.Reset();
            Need(mind.Mode == PikeMode.Patrol && !mind.StrikeReady &&
                mind.LastSensedPosition is null, "caught state survived retry reset");

            var cover = scene.CoverPlan;
            var catalog = PikeCoverFixture.LoadCatalog();
            var plants = PikeCoverFixture.VisibleSoftCover(scene.GetNode<Node3D>("Cover"));
            var checkedWood = 0;
            foreach (var id in new[] { "FAF-WOOD-005A", "FAF-WOOD-006A", "FAF-WOOD-007A",
                "FAF-WOOD-008A", "FAF-WOOD-009A" })
            {
                var placement = cover.Single(p => p.Id == id);
                var asset = catalog.Assets.Single(a => a.Id == id);
                var basis = new Basis(Vector3.Up, placement.Yaw);
                var half = asset.Size[0] * .7f + 1f;
                var blocked = false;
                for (var y = .8f; y <= 3.2f && !blocked; y += .6f)
                for (var z = -2f; z <= 2f && !blocked; z += 1f)
                {
                    var from = placement.Position + basis * new Vector3(-half, y, z);
                    var to = placement.Position + basis * new Vector3(half, y, z);
                    var sensor = new WorldSightSensor(scene.GetWorld3D().DirectSpaceState,
                        plants, actor.GetRid(), fish.GetRid());
                    var sample = sensor.Observe(from, to - from,
                        new SightTarget(to, .48f, 1), scene.SightWater!, 0, false);
                    if (sample.Reason != "hard-cover") continue;
                    var physicalFrom = new Vector3(from.X,
                        Mathf.Max(from.Y, PikeCoverFixture.Bed(from.X, from.Z) + 1.8f), from.Z);
                    var physicalTo = new Vector3(to.X,
                        Mathf.Max(to.Y, PikeCoverFixture.Bed(to.X, to.Z) + 1.8f), to.Z);
                    actor.GlobalPosition = physicalFrom;
                    actor.GlobalBasis = Basis.LookingAt((to - from).Normalized(), Vector3.Up);
                    actor.ResetActor();
                    if (OverlapName() != "") continue;
                    blocked = true;
                    Need(actor.TestMove(actor.GlobalTransform, physicalTo - physicalFrom),
                        id + " hard cover does not stop physical Pike sweep");
                    for (var tick = 0; tick < 120; tick++)
                    {
                        var prior = actor.GlobalPosition;
                        var facing = -actor.GlobalBasis.Z;
                        actor.Tick(strong with { SensedPosition = to, SensedAt = tick / 60d },
                            default, tick / 60d, 1f / 60f, false);
                        await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                        var overlap = OverlapName();
                        Need(overlap == "", id + $" Pike phased into {overlap} tick={tick} at={actor.GlobalPosition}");
                        Need(actor.GlobalPosition.DistanceTo(prior) < .13f &&
                            facing.AngleTo(-actor.GlobalBasis.Z) <= Mathf.DegToRad(81f) / 60f + .002f,
                            id + $" Pike exceeded speed or turn clearance tick={tick} move={actor.GlobalPosition.DistanceTo(prior):F3} turn={facing.AngleTo(-actor.GlobalBasis.Z):F3} at={actor.GlobalPosition}");
                    }
                    var last = new Vector3(0, 1, 0);
                    mind.Step(new Vector3(0, 1, 6), strong with { SensedPosition = last },
                        default, 0, false);
                    mind.Step(from, sample, default, .4, false);
                    Need(mind.Mode == PikeMode.Search && mind.LastSensedPosition == last &&
                        mind.Destination(.4, origin) == last,
                        id + " hidden Bluegill coordinate leaked into search");
                    mind.Step(from, sample, default, 7, false);
                    Need(mind.Mode == PikeMode.Disengage && mind.Escaped,
                        id + " lost-contact search never ended");
                    Need(mind.ConsumeEscape() && !mind.ConsumeEscape() &&
                        mind.Mode == PikeMode.Disengage,
                        id + " escape was not a one-time nonterminal event");
                    mind.Reset();
                    checkedWood++;
                }
                Need(blocked, id + " lacks a solid sight blocker");
            }
            // Fixed-pose body probe: collision is a real sweep, including vertical
            // and lateral alternatives around the placed wood, not direct translation.
            var start = new Vector3(0, PikeCoverFixture.Bed(0, 8) + 2f, 8);
            actor.GlobalPosition = start;
            actor.ResetActor();
            var step = 1f / 60f;
            actor.Tick(strong, default, 0, step, false);
            Need(actor.GlobalPosition.DistanceTo(start) <= .11f && actor.ActualSpeed <= .11f,
                "Pike moved faster than acceleration/turn bound");
            var pausedPosition = actor.GlobalPosition;
            var pausedMemory = actor.Mind.LastSensedPosition;
            actor.Tick(strong with { SensedPosition = new Vector3(50, 1, 50), SensedAt = 1 },
                default, 1, step, true);
            Need(actor.GlobalPosition == pausedPosition && actor.Mind.LastSensedPosition == pausedMemory,
                "pause advanced actor or target memory");
            actor.ResetActor();
            Need(actor.Mind.LastSensedPosition is null && actor.ActualSpeed == 0 &&
                actor.Mind.Mode == PikeMode.Patrol, "actor restart retained pursuit");
            actor.ResetActor();
            Need(actor.Mind.LastSensedPosition is null, "second actor restart retained memory");
            actor.GlobalPosition = new Vector3(0, PikeCoverFixture.Bed(0, 5) + 2f, 5);
            actor.GlobalBasis = Basis.LookingAt(Vector3.Forward, Vector3.Up);
            actor.ResetActor();
            var patrolStart = actor.GlobalPosition;
            for (var tick = 0; tick < 60; tick++)
            {
                actor.Tick(default, default, tick / 60d, step, false);
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            }
            Need(actor.Mind.Mode == PikeMode.Patrol && actor.Mind.LastSensedPosition is null &&
                actor.GlobalPosition.DistanceTo(patrolStart) > .15f && actor.ActualSpeed <= 2.3f,
                "absent-target ordinary patrol did not swim at bounded speed");
            for (var tick = 60; tick < 300; tick++)
            {
                actor.Tick(default, default, tick / 60d, step, false);
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            }
            Need(actor.GlobalPosition.DistanceTo(patrolStart) > 4f,
                $"Pike did not physically patrol beyond starting corridor distance={actor.GlobalPosition.DistanceTo(patrolStart):F2}");
            Need(actor.Mind.Destination(0, patrolStart).DistanceTo(
                    actor.Mind.Destination(31.4, patrolStart)) > 18f,
                "Pike patrol did not cover the connected Pond route");
            // The same Bass must remain playable after a confirmed escape.
            actor.GlobalPosition = new Vector3(22, PikeCoverFixture.Bed(22, 22) + 2f, 22);
            actor.ResetActor();
            fish.GlobalPosition = new Vector3(0, PikeCoverFixture.Bed(0, -15) + 2f, -15);
            fish.ClearAndDisarmInput();
            var bassIdentity = scene.Growth.Identity;
            var nutritionBeforeEscape = scene.Feeding.Nutrition;
            var escapeMind = actor.Mind;
            escapeMind.Step(actor.GlobalPosition,
                strong with { SensedPosition = fish.GlobalPosition, SensedAt = -7.5 },
                default, -7.5, false);
            escapeMind.Step(actor.GlobalPosition, hidden, default, -7.1, false);
            escapeMind.Step(actor.GlobalPosition, hidden, default, 0, false);
            Need(escapeMind.Escaped && escapeMind.Mode == PikeMode.Disengage,
                "fixture escape setup did not produce a confirmed disengagement");
            scene.PikeSight!.Reset();
            scene.SetPhysicsProcess(true);
            for (var frame = 0; frame < 5; frame++)
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(scene.Outcome == "active" && scene.EscapeCount == 1 && !tree.Paused &&
                scene.Growth.Identity == bassIdentity &&
                scene.Feeding.Nutrition == nutritionBeforeEscape &&
                !escapeMind.Escaped &&
                scene.FindChild("EncounterMenu", true, false) is Control { Visible: false } &&
                scene.FindChild("EncounterStatus", true, false) is Label status &&
                status.Text.Contains("keep swimming"),
                "escape wrongly ended or paused the Bass life");
            var afterEscapeTime = scene.MovementCues!.SimulationSeconds;
            fish.SetInputMode("keyboard");
            fish.ClearAndDisarmInput();
            for (var frame = 0; frame < 3; frame++)
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            var afterEscapePosition = fish.GlobalPosition;
            Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = Key.W, Pressed = true });
            for (var frame = 0; frame < 20; frame++)
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = Key.W, Pressed = false });
            fish.ClearAndDisarmInput();
            Need(scene.Outcome == "active" && scene.EscapeCount == 1 && !tree.Paused &&
                scene.MovementCues.SimulationSeconds > afterEscapeTime &&
                fish.GlobalPosition.DistanceTo(afterEscapePosition) > .15f,
                "escaped Bass did not keep swimming with one escape event");
            scene._Input(new InputEventKey { Keycode = Key.Escape, Pressed = true });
            var resume = scene.FindChild("ResumeEncounter", true, false) as Button;
            Need(tree.Paused && resume is { Disabled: false },
                "escape disabled ordinary pause/resume");
            resume!.EmitSignal(BaseButton.SignalName.Pressed);
            Need(!tree.Paused &&
                scene.FindChild("EncounterMenu", true, false) is Control { Visible: false },
                "escaped Bass could not resume after voluntary pause");
            scene.SetPhysicsProcess(false);
            // Run the actual fixture for an exposed, stationary Bass. The
            // temporary result must follow sight, windup, movement and bite range.
            var openY = Mathf.Max(PikeCoverFixture.Bed(0, 9), PikeCoverFixture.Bed(0, 5)) + 2f;
            actor.GlobalPosition = new Vector3(0, openY, 9);
            actor.GlobalBasis = Basis.LookingAt(Vector3.Forward, Vector3.Up);
            actor.ResetActor();
            fish.GlobalPosition = new Vector3(0, openY, 7);
            fish.ClearAndDisarmInput();
            scene.PikeSight!.Reset();
            scene.MovementCues!.Reset();
            scene.SetPhysicsProcess(true);
            for (var frame = 0; frame < 360 && scene.Outcome == "active"; frame++)
                await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            Need(scene.Outcome == "caught" && actor.Mind.Mode == PikeMode.Caught && tree.Paused,
                $"exposed fixture did not produce a physical temporary catch outcome={scene.Outcome} mode={actor.Mind.Mode} ready={actor.Mind.StrikeReady} time={scene.MovementCues.SimulationSeconds:F2} distance={actor.GlobalPosition.DistanceTo(fish.GlobalPosition):F2} at={actor.GlobalPosition}");
            GD.Print($"PIKE_AI_CHECK_OK seed={PikeCoverFixture.Seed} wood={checkedWood} states=8 open_catch=true hidden_escape_continues=true pause=true restart=2");
            tree.Quit();
        }
        catch (Exception error)
        {
            GD.PushError("PIKE_AI_CHECK_FAILED " + error);
            tree.Quit(1);
        }

        string OverlapName()
        {
            var body = scene.GetNode<PikeActor>("NorthernPike");
            var fish = scene.GetNode<FishController>("Fish");
            var collision = body.GetNode<CollisionShape3D>("Collision");
            // A smaller concentric capsule distinguishes penetration from
            // normal contact tolerances at the outer collision surface.
            using var probeShape = new CapsuleShape3D { Radius = .35f, Height = 2.4f };
            using var query = new PhysicsShapeQueryParameters3D
                {
                    Shape = probeShape,
                    Transform = collision.GlobalTransform,
                    CollisionMask = 1,
                    Margin = 0,
                    Exclude = new Godot.Collections.Array<Rid> { body.GetRid(), fish.GetRid() },
                };
            var hits = scene.GetWorld3D().DirectSpaceState.IntersectShape(query, 1);
            return hits.Count == 0 ? "" :
                (hits[0]["collider"].AsGodotObject() as Node)?.GetPath().ToString() ?? "unknown";
        }
    }

    private static void Need(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
