#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

/// <summary>Registered Bass growth, rigs and collision-based stage transition probe.</summary>
public static class GrowthChecks
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
            var camera = fish.GetNode<FollowCamera>("CameraRig");
            var identity = scene.Growth.Identity;
            Need(DemoSelection.CharacterId == PondGrowthState.SpeciesId &&
                scene.Growth.Stage == "juvenile" &&
                Near(fish.BodyRadius, scene.Growth.ColliderRadius), "juvenile identity or collider");
            var juvenileCollision = fish.GetNode<CollisionShape3D>("Collision");
            Need(juvenileCollision.Shape is CapsuleShape3D { } juvenileCapsule &&
                Near(juvenileCapsule.Height, scene.Growth.ColliderLength) &&
                Near(juvenileCapsule.Radius, scene.Growth.ColliderRadius) &&
                Near(fish.BodyForwardReach, scene.Growth.ColliderLength * .42f),
                "juvenile Bass physical body does not follow registered dimensions");
            CheckRig();
            var larva = scene.PreyActors[4];
            var open = new Vector3(0, PikeCoverFixture.Bed(0, 20) + 1.2f, 20);
            fish.GlobalPosition = open;
            fish.GlobalBasis = Basis.Identity;
            larva.GlobalPosition = open + new Vector3(0, 0, -.8f);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            camera.ResetFollow();
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            var juvenileCameraDistance = camera.GlobalPosition.DistanceTo(fish.GlobalPosition);
            Need(juvenileCameraDistance > fish.BodyRadius + .1f &&
                CameraClear(scene, fish, camera), "juvenile camera clearance");
            var stageBeforeCamera = scene.Growth.Stage;
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(scene.Growth.Stage == stageBeforeCamera && scene.Feeding.Nutrition == 0,
                "camera update mutated growth");
            Need(FeedingLedger.IsEligiblePrey(larva.SpeciesId, larva.Stage, larva.Length,
                    scene.Growth.Stage, scene.Growth.Length),
                "registered Bluegill larva should be edible from juvenile Bass");
            var wall = new StaticBody3D { Name = "GrowthTestWall", CollisionLayer = 1 };
            scene.AddChild(wall);
            wall.GlobalPosition = fish.GlobalPosition + new Vector3(.16f, 0, 0);
            wall.AddChild(new CollisionShape3D { Shape = new BoxShape3D
                { Size = new Vector3(.08f, 1f, 1f) } });
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(scene.TryBite(larva).Consumed && scene.Feeding.Nutrition == 4 &&
                scene.Growth.Stage == "juvenile" && !scene.TryAdvanceGrowth(),
                "near-wall stage should defer after real Bluegill bite");
            wall.QueueFree();
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(scene.TryAdvanceGrowth() && scene.Growth.Stage == "subadult" &&
                Near(fish.BodyRadius, scene.Growth.ColliderRadius) &&
                scene.Growth.Identity == identity && scene.Feeding.Nutrition == 4,
                "subadult stage did not apply after clearance");
            CheckRig();
            tree.Paused = true;
            Need(!scene.TryBite(larva).Consumed && !scene.TryAdvanceGrowth() &&
                scene.Feeding.Nutrition == 4, "paused scene advanced feeding/growth");
            tree.Paused = false;
            foreach (var prey in scene.PreyActors.Skip(5).Take(2))
            {
                fish.GlobalPosition = open;
                fish.GlobalBasis = Basis.Identity;
                prey.GlobalPosition = open + new Vector3(0, 0, -.95f);
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                Need(scene.TryBite(prey).Consumed, "subadult Bluegill larva bite failed");
                Need(!scene.TryBite(prey).Consumed, "duplicate Bluegill bite awarded");
            }
            Need(scene.Growth.Stage == "adult" && scene.Feeding.Nutrition == 12 &&
                scene.Feeding.Events.Count == 3 && scene.Growth.Identity == identity &&
                DemoSelection.CharacterId == PondGrowthState.SpeciesId &&
                Near(fish.BodyRadius, PondGrowthState.AdultColliderRadius),
                "adult stage, one-time awards or identity changed");
            CheckRig();
            var juvenileBluegill = scene.PreyActors[0];
            fish.GlobalPosition = open;
            fish.GlobalBasis = Basis.Identity;
            juvenileBluegill.GlobalPosition = open + new Vector3(0, 0, -.95f);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(scene.TryBite(juvenileBluegill).Consumed &&
                !scene.TryBite(juvenileBluegill).Consumed && scene.Feeding.Events.Count == 4,
                "adult Bass did not gain a one-time Bluegill-juvenile hunt");
            Need(camera.GetNode<Camera3D>("Camera3D").Current &&
                Near(fish.BodyScale, 1f), "camera/body scale lost at adult");
            fish.GlobalPosition = open;
            camera.ResetFollow();
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            var adultCameraDistance = camera.GlobalPosition.DistanceTo(fish.GlobalPosition);
            Need(adultCameraDistance > juvenileCameraDistance * 1.5f &&
                CameraClear(scene, fish, camera) && !scene.TryAdvanceGrowth(),
                "adult camera framing/clearance or spurious growth");
            ClearanceSurvey(scene, fish);
            GD.Print($"GROWTH_CHECK_OK seed={PikeCoverFixture.Seed} fixture=pike identity={identity} stage={scene.Growth.Stage} nutrition={scene.Feeding.Nutrition} events={scene.Feeding.Events.Count} radius={fish.BodyRadius:F3} camera_juvenile={juvenileCameraDistance:F2} camera_adult={adultCameraDistance:F2}");
            tree.Quit();

            void CheckRig()
            {
                var asset = scene.Growth.Asset;
                var visual = fish.GetNode<Node3D>("PlayerVisual");
                var player = visual.FindChild("AnimationPlayer", true, false) as AnimationPlayer;
                var motion = visual.GetNode<CatalogFishVisualMotion>("SwimPresentation");
                var adultBass = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "adult")!;
                Need(asset.Id == PikeEncounterAssets.BassId &&
                    Near(visual.Scale.X, 1f / adultBass.Dimensions[1]) &&
                    Near(fish.BodyRadius, asset.Dimensions[0] /
                        (2f * adultBass.Dimensions[1])) &&
                    fish.GetNode<CollisionShape3D>("Collision").Shape is CapsuleShape3D body &&
                    Near(body.Height, asset.Dimensions[1] / adultBass.Dimensions[1]) &&
                    PondCreatureCatalog.ExportMatches(asset) &&
                    player is not null && player.HasAnimation(asset.Clip("Glide")) &&
                    player.HasAnimation(asset.Clip("Swim")) &&
                    player.HasAnimation(asset.Clip("SwimFast")) &&
                    motion is not null && visual.GetChildCount() > 0,
                    "registered rig or clips missing at " + scene.Growth.Stage);
            }
        }
        catch (Exception error)
        {
            GD.PushError("GROWTH_CHECK_FAILED " + error);
            tree.Quit(1);
        }
    }

    private static void ClearanceSurvey(PikeEncounter scene, FishController fish)
    {
        var catalog = PikeCoverFixture.LoadCatalog();
        var juvenileRadius = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "juvenile")!.Dimensions[0] /
            (2f * PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "adult")!.Dimensions[1]);
        var exclude = new Godot.Collections.Array<Rid> { fish.GetRid(),
            scene.GetNode<PikeActor>("NorthernPike").GetRid() };
        foreach (var prey in scene.PreyActors) exclude.Add(prey.GetRid());
        foreach (var id in new[] { "FAF-WOOD-006A", "FAF-WOOD-007A", "FAF-WOOD-008A", "FAF-WOOD-009A" })
        {
            var placement = scene.CoverPlan.Single(p => p.Id == id);
            var asset = catalog.Assets.Single(a => a.Id == id);
            var basis = new Basis(Vector3.Up, placement.Yaw);
            var smallOnly = 0;
            var small = 0;
            var large = 0;
            var reachableShelter = 0;
            for (var iy = 0; iy < 7; iy++)
            {
                var juvenileCells = new bool[17, 17];
                var adultCells = new bool[17, 17];
                for (var ix = 0; ix < 17; ix++)
                    for (var iz = 0; iz < 17; iz++)
                    {
                        var local = new Vector3((ix - 8) * .3f, .55f + iy * .35f, (iz - 8) * .3f);
                        var at = placement.Position + basis * local;
                        if (at.Y > PikeCoverFixture.WaterY - .65f * PondGrowthState.AdultColliderRadius) continue;
                        var juvenile = Clear(juvenileRadius, at);
                        var adult = Clear(PondGrowthState.AdultColliderRadius, at);
                        juvenileCells[ix, iz] = juvenile;
                        adultCells[ix, iz] = adult;
                        if (juvenile) small++;
                        if (adult) large++;
                        if (juvenile && !adult) smallOnly++;
                    }
                var seen = new bool[17, 17];
                var queue = new Queue<(int x, int z)>();
                for (var ix = 0; ix < 17; ix++)
                    for (var iz = 0; iz < 17; iz++)
                        if ((ix is 0 or 16 || iz is 0 or 16) && juvenileCells[ix, iz])
                        { seen[ix, iz] = true; queue.Enqueue((ix, iz)); }
                while (queue.Count > 0)
                {
                    var (x, z) = queue.Dequeue();
                    if (!adultCells[x, z] && Math.Abs(x - 8) <= 5 && Math.Abs(z - 8) <= 5)
                        reachableShelter++;
                    foreach (var (dx, dz) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                    {
                        var xx = x + dx; var zz = z + dz;
                        if (xx < 0 || xx >= 17 || zz < 0 || zz >= 17 ||
                            seen[xx, zz] || !juvenileCells[xx, zz]) continue;
                        // Clear endpoints can straddle a thin wrapper. Check the
                        // moving juvenile sphere along each step as well.
                        var from = placement.Position + basis * new Vector3(
                            (x - 8) * .3f, .55f + iy * .35f, (z - 8) * .3f);
                        var to = placement.Position + basis * new Vector3(
                            (xx - 8) * .3f, .55f + iy * .35f, (zz - 8) * .3f);
                        if (!Clear(juvenileRadius, from.Lerp(to, .25f)) ||
                            !Clear(juvenileRadius, from.Lerp(to, .5f)) ||
                            !Clear(juvenileRadius, from.Lerp(to, .75f))) continue;
                        seen[xx, zz] = true;
                        queue.Enqueue((xx, zz));
                    }
                }
            }
            GD.Print($"GROWTH_GAP_SURVEY id={id} family={asset.Family} juvenile_clear={small} adult_clear={large} juvenile_only={smallOnly} reachable_small_only_core={reachableShelter}");
            Need(reachableShelter > 0 && smallOnly > 0 && small > large,
                "no connected juvenile-only real cover pocket " + id);
        }
        var plants = PikeCoverFixture.VisibleSoftCover(scene.GetNode<Node3D>("Cover"));
        foreach (var family in new[] { "ribbon_grass", "coontail" })
        {
            var ids = catalog.Assets.Where(a => a.Family == family).Select(a => a.Id).ToHashSet();
            var found = false;
            foreach (var plant in plants.Where(p => ids.Contains(p.FafId)))
            {
                var center = plant.VisibleBounds.GetCenter();
                var observer = center + Vector3.Right * 2f;
                var through = SoftCoverTransmission.Along(observer,
                    center - Vector3.Right * 2f, plants, .2f);
                if (through >= .98f) continue;
                var sensor = new WorldSightSensor(scene.GetWorld3D().DirectSpaceState, plants,
                    scene.GetNode<PikeActor>("NorthernPike").GetRid(), fish.GetRid(),
                    range: 42f, acuity: 2.5f);
                var juvenile = sensor.Observe(observer, -Vector3.Right,
                    new SightTarget(center, juvenileRadius, 1f), scene.SightWater!, 1, false);
                sensor.Reset();
                var adult = sensor.Observe(observer, -Vector3.Right,
                    new SightTarget(center, PondGrowthState.AdultColliderRadius, 1f),
                    scene.SightWater!, 2, false);
                if (juvenile.ClearRays == 0 || adult.ClearRays == 0 ||
                    adult.Strength <= juvenile.Strength) continue;
                GD.Print($"GROWTH_SOFT_COVER family={family} transmission={through:F3} juvenile_sight={juvenile.Strength:F3} adult_sight={adult.Strength:F3}");
                found = true;
                break;
            }
            Need(found, "no visible growth exposure tradeoff in real " + family);
        }

        bool Clear(float radius, Vector3 at)
        {
            using var query = new PhysicsShapeQueryParameters3D
            {
                Shape = new SphereShape3D { Radius = radius },
                Transform = new Transform3D(Basis.Identity, at), CollisionMask = 1, Exclude = exclude,
            };
            return scene.GetWorld3D().DirectSpaceState.IntersectShape(query, 1).Count == 0;
        }
    }

    private static bool Near(float a, float b) => Mathf.Abs(a - b) < .003f;
    private static bool CameraClear(PikeEncounter scene, FishController fish, FollowCamera camera)
    {
        using var query = new PhysicsShapeQueryParameters3D
        {
            Shape = new SphereShape3D { Radius = .22f * Mathf.Max(.32f, fish.BodyScale) },
            Transform = new Transform3D(Basis.Identity, camera.GlobalPosition),
            CollisionMask = 1,
            Exclude = new Godot.Collections.Array<Rid> { fish.GetRid(),
                scene.GetNode<PikeActor>("NorthernPike").GetRid() },
        };
        return scene.GetWorld3D().DirectSpaceState.IntersectShape(query, 1).Count == 0;
    }
    private static void Need(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
