#nullable enable
using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fish;

/// <summary>Deterministic seed-5202 decision and placed-cover probe; no native fairness claim.</summary>
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
                    actor.GlobalPosition = from;
                    actor.GlobalBasis = Basis.LookingAt((to - from).Normalized(), Vector3.Up);
                    actor.ResetActor();
                    if (OverlapName() != "") continue;
                    blocked = true;
                    Need(actor.TestMove(actor.GlobalTransform, to - from),
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
                            facing.AngleTo(-actor.GlobalBasis.Z) <= Mathf.DegToRad(126f) / 60f + .002f,
                            id + " Pike exceeded speed or turn clearance");
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
                    mind.Reset();
                    checkedWood++;
                }
                Need(blocked, id + " lacks a solid sight blocker");
            }
            // Fixed-pose body probe: collision is a real sweep, including vertical
            // and lateral alternatives around the placed wood, not direct translation.
            var start = new Vector3(0, 1, 8);
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
            actor.GlobalPosition = new Vector3(0, 1, 5);
            actor.GlobalBasis = Basis.LookingAt(Vector3.Forward, Vector3.Up);
            actor.ResetActor();
            var patrolStart = actor.GlobalPosition;
            for (var tick = 0; tick < 60; tick++)
            {
                actor.Tick(default, default, tick / 60d, step, false);
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            }
            Need(actor.Mind.Mode == PikeMode.Patrol && actor.Mind.LastSensedPosition is null &&
                actor.GlobalPosition.DistanceTo(patrolStart) > .15f && actor.ActualSpeed <= 1.2f,
                "absent-target ordinary patrol did not swim at bounded speed");
            // Run the actual fixture for an exposed, stationary Bluegill. The
            // temporary result must follow sight, windup, movement and bite range.
            actor.GlobalPosition = new Vector3(0, 1, 5);
            actor.GlobalBasis = Basis.LookingAt(Vector3.Forward, Vector3.Up);
            actor.ResetActor();
            fish.GlobalPosition = new Vector3(0, 1, 2);
            fish.ClearAndDisarmInput();
            scene.PikeSight!.Reset();
            scene.MovementCues!.Reset();
            scene.SetPhysicsProcess(true);
            for (var frame = 0; frame < 360 && scene.Outcome == "active"; frame++)
                await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            Need(scene.Outcome == "caught" && actor.Mind.Mode == PikeMode.Caught && tree.Paused,
                $"exposed fixture did not produce a physical temporary catch outcome={scene.Outcome} mode={actor.Mind.Mode} ready={actor.Mind.StrikeReady} time={scene.MovementCues.SimulationSeconds:F2} distance={actor.GlobalPosition.DistanceTo(fish.GlobalPosition):F2} at={actor.GlobalPosition}");
            GD.Print($"PIKE_AI_CHECK_OK seed={PikeCoverFixture.Seed} wood={checkedWood} states=8 open_catch=true hidden_escape=true pause=true restart=2");
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
            var hits = scene.GetWorld3D().DirectSpaceState.IntersectShape(
                new PhysicsShapeQueryParameters3D
                {
                    Shape = new CapsuleShape3D { Radius = .18f, Height = .95f },
                    Transform = collision.GlobalTransform,
                    CollisionMask = 1,
                    Margin = 0,
                    Exclude = new Godot.Collections.Array<Rid> { body.GetRid(), fish.GetRid() },
                }, 1);
            return hits.Count == 0 ? "" :
                (hits[0]["collider"].AsGodotObject() as Node)?.GetPath().ToString() ?? "unknown";
        }
    }

    private static void Need(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
