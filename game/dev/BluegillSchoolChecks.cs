#nullable enable
using Godot;
using System;
using System.Diagnostics;
using System.Linq;

namespace Fish;

/// <summary>Headless V39 population, steering, sensory spread and cost check.</summary>
public static class BluegillSchoolChecks
{
    public static async void Run(PikeEncounter scene)
    {
        var tree = scene.GetTree();
        try
        {
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            scene.SetPhysicsProcess(false);
            var actors = scene.PreyActors;
            var schools = scene.BluegillSchools;
            Need(actors.Count == 16 && schools.Count == 3 &&
                schools.Select(s => s.Members.Count).SequenceEqual(new[] { 6, 6, 4 }) &&
                actors.Count(a => a.Stage == "larva") == 6 &&
                actors.Count(a => a.Stage == "juvenile") == 4 &&
                actors.Count(a => a.Stage == "subadult") == 4 &&
                actors.Count(a => a.Stage == "adult") == 2 &&
                actors.Select(a => a.StableId).Distinct().Count() == actors.Count &&
                actors.All(a => a.IsAlive && a.SpeciesId == PikeEncounterAssets.BluegillId),
                "school populations, stages or IDs invalid");
            foreach (var actor in actors)
            {
                var stage = PondCreatureCatalog.Stage(actor.SpeciesId, actor.Stage)!;
                var animation = actor.GetNode<AnimationPlayer>("PreyVisual/AnimationPlayer");
                var radius = ((SphereShape3D)actor.GetNode<CollisionShape3D>("Collision").Shape).Radius;
                Need(PondCreatureCatalog.ExportMatches(stage) &&
                    Mathf.Abs(actor.Length - stage.Dimensions[1]) < .00001f &&
                    animation.HasAnimation(stage.Clip("Swim")) &&
                    animation.HasAnimation(stage.Clip("SwimFast")) &&
                    actor.GetNode<Node3D>("PreyVisual").Scale == Vector3.One &&
                    radius >= stage.Dimensions[0] * .4f &&
                    actor.GlobalPosition.Y > PikeCoverFixture.Bed(actor.GlobalPosition.X, actor.GlobalPosition.Z) + radius &&
                    actor.GlobalPosition.Y < PikeCoverFixture.WaterY - radius &&
                    PikeCoverFixture.ShoreDistance(actor.GlobalPosition.X, actor.GlobalPosition.Z) > 2f &&
                    !InsideSolid(actor, radius), "stage or spawn clearance invalid " + actor.StableId);
            }
            var nursery = schools[0].Members.Average(a => PikeCoverFixture.WaterY - a.GlobalPosition.Y);
            var refuge = schools[1].Members.Average(a => PikeCoverFixture.WaterY - a.GlobalPosition.Y);
            var deep = schools[2].Members.Average(a => PikeCoverFixture.WaterY - a.GlobalPosition.Y);
            Need(nursery < refuge && refuge < deep, "age/depth preferences inverted");
            var cover = scene.CoverPlan;
            var shorePlants = scene.GetNode<Node3D>("ShoreGrowth").GetChildren()
                .OfType<Node3D>().Where(node => node.HasMeta("faf_id") &&
                    node.GetMeta("faf_id").AsString().StartsWith("FAF-PLANT-", StringComparison.Ordinal))
                .ToArray();
            var nearestRibbon = shorePlants.Where(node => node.GetMeta("faf_id").AsString()
                .StartsWith("FAF-PLANT-010", StringComparison.Ordinal))
                .Min(node => node.GlobalPosition.DistanceTo(schools[0].Center));
            var nearestCoontail = shorePlants.Where(node => node.GetMeta("faf_id").AsString()
                .StartsWith("FAF-PLANT-012", StringComparison.Ordinal))
                .Min(node => node.GlobalPosition.DistanceTo(schools[0].Center));
            Need(nearestRibbon < 6f && nearestCoontail < 8f,
                $"nursery missed real shoreline ribbon/coontail cover {nearestRibbon:F1}/{nearestCoontail:F1}");
            Need(cover.Any(p => p.Id.StartsWith("FAF-PLANT-010") &&
                    p.Position.DistanceTo(schools[0].Center) < 12f) &&
                cover.Any(p => p.Id.StartsWith("FAF-WOOD-00") &&
                    p.Position.DistanceTo(schools[1].Center) < 12f) &&
                cover.Any(p => p.Zone == "maze" &&
                    p.Position.DistanceTo(schools[2].Center) < 18f),
                "schools did not use the fixture's real cover route");

            var starts = actors.Select(a => a.GlobalPosition).ToArray();
            long updateTicks = 0;
            var maxChecks = 0;
            for (var frame = 0; frame < 180; frame++)
            {
                foreach (var school in schools) school.ResetBudget();
                var updateStart = Stopwatch.GetTimestamp();
                for (var i = 0; i < actors.Count; i++)
                    actors[i].Tick(default, default, frame / 60d, 1f / 60f, false,
                        schools.Single(s => s.Id == actors[i].SchoolId).For(actors[i], frame / 60d));
                updateTicks += Stopwatch.GetTimestamp() - updateStart;
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                maxChecks = Math.Max(maxChecks, schools.Sum(s => s.NeighborChecks));
            }
            Need(maxChecks <= 72 && maxChecks > 0 &&
                actors.Where((a, i) => a.GlobalPosition.DistanceTo(starts[i]) > .15f).Count() >= 12 &&
                actors.All(a => a.Mind.Mode == PreyMode.Forage),
                "school idle steering or bounded neighbor budget failed");
            var lead = schools[0].Members[0];
            var threat = lead.GlobalPosition + Vector3.Right;
            var evidence = new SightEvidence(true, .4f, "visible", 1, 1, 1, 3, 3, 7, threat, 3);
            lead.Tick(evidence, default, 3, 1f / 60f, false,
                schools[0].For(lead, 3));
            var neighbors = schools[0].Members.Where(a => a != lead &&
                a.GlobalPosition.DistanceTo(lead.GlobalPosition) < 2.1f).ToArray();
            Need(neighbors.Length > 0, "nursery has no local neighbors for alarm");
            foreach (var member in neighbors)
                member.Tick(default, default, 3.02, 1f / 60f, false,
                    schools[0].For(member, 3.02));
            Need(lead.Mind.Mode == PreyMode.Flee &&
                neighbors.Any(a => a.Mind.Mode == PreyMode.Alert),
                "real local alarm did not scatter neighboring fish");
            var held = lead.GlobalPosition;
            lead.Tick(default, default, 8, 1f / 60f, true,
                schools[0].For(lead, 8));
            Need(lead.GlobalPosition == held && lead.Mind.Mode == PreyMode.Flee,
                "pause advanced school member");
            for (var frame = 0; frame < 330; frame++)
            {
                lead.Tick(default, default, 3.1 + frame / 60d, 1f / 60f, false,
                    schools[0].For(lead, 3.1 + frame / 60d));
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            }
            Need(lead.Mind.Mode == PreyMode.Forage, "threat memory did not clear for regroup");
            var ledger = new FeedingLedger();
            var larva = actors.First(a => a.Stage == "larva");
            var bass = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "subadult")!;
            var candidate = new FeedCandidate(larva.StableId, larva.SpeciesId, larva.Stage,
                WorldCatalog.PondId, WorldCatalog.PondId, larva.Length,
                "subadult", bass.Dimensions[1], larva.IsAlive);
            Need(ledger.TryConsume(candidate, true, 8).Consumed &&
                !ledger.TryConsume(candidate, true, 8.1).Consumed && ledger.Events.Count == 1,
                "school member fed twice");
            larva.Consume();
            Need(!larva.IsAlive && !larva.Visible && schools[0].Members.Count == 6 &&
                schools[0].For(lead, 8.2).Neighbors <= 4,
                "consumed member remained in active steering");
            var updateMs = updateTicks * 1000.0 / Stopwatch.Frequency;
            GD.Print($"BLUEGILL_SCHOOL_CHECK_OK seed={PikeCoverFixture.Seed} schools=3 fish=16 stages=6,4,4,2 depth={nursery:F2},{refuge:F2},{deep:F2} shore_cover={nearestRibbon:F1},{nearestCoontail:F1} max_neighbor_checks={maxChecks} actor_update_180_ms={updateMs:F1} scatter=local regroup=forage pause=held feed=once");
            tree.Quit();

            bool InsideSolid(PreyActor actor, float radius)
            {
                var query = new PhysicsShapeQueryParameters3D
                {
                    Shape = new SphereShape3D { Radius = radius * .9f },
                    Transform = actor.GetNode<CollisionShape3D>("Collision").GlobalTransform,
                    CollisionMask = 1, Margin = 0,
                    Exclude = new Godot.Collections.Array<Rid> { actor.GetRid(),
                        scene.GetNode<FishController>("Fish").GetRid(),
                        scene.GetNode<PikeActor>("NorthernPike").GetRid() },
                };
                return scene.GetWorld3D().DirectSpaceState.IntersectShape(query, 1).Count > 0;
            }
        }
        catch (Exception error)
        {
            GD.PushError("BLUEGILL_SCHOOL_CHECK_FAILED " + error);
            tree.Quit(1);
        }
    }

    private static void Need(bool condition, string message)
    { if (!condition) throw new Exception(message); }
}
