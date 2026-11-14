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
            var stages = new PondGrowthState(identity, "egg");
            Need(stages.Stage == "egg" && stages.Next(100) is null &&
                stages.Asset.ModelPath == PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "egg")!.ModelPath,
                "egg asset or nonfeeding policy invalid");
            stages.Hatch();
            Need(stages.Stage == "larva" && stages.Next(5) is null &&
                stages.Next(6)?.Stage == "juvenile" &&
                stages.ColliderLength < scene.Growth.ColliderLength &&
                PondCreatureCatalog.ExportMatches(stages.Asset), "larva stage/threshold invalid");
            stages.Commit(stages.Next(6)!);
            Need(stages.Next(13) is null && stages.Next(14)?.Stage == "subadult",
                "juvenile threshold invalid");
            stages.Commit(stages.Next(14)!);
            Need(stages.Next(25) is null && stages.Next(26)?.Stage == "adult",
                "subadult threshold invalid");
            stages.Commit(stages.Next(26)!);
            Need(stages.Stage == "adult" && stages.Identity == identity &&
                PondCreatureCatalog.ExportMatches(stages.Asset), "adult stage/identity invalid");
            var baseline = fish.Tuning;
            var speeds = new[] { "larva", "juvenile", "subadult", "adult" }
                .Select(stage => new PondGrowthState(identity, stage).StageTuning(baseline).MaxSpeed).ToArray();
            Need(speeds[0] > 0f && speeds.Zip(speeds.Skip(1), (before, after) =>
                after > before && after / before < 3f).All(valid => valid) &&
                speeds[^1] < 4f,
                "Bass stage swim speed progression invalid");
            var subadultTuning = new PondGrowthState(identity, "subadult").StageTuning(baseline);
            var subadultMotor = new SwimState();
            var subadultTravel = 0f;
            for (var frame = 0; frame < 60; frame++)
            {
                subadultMotor = SwimMotor.Step(subadultMotor, new SwimIntent(1f, 0f, 0f, 0f),
                    1f / 60f, subadultTuning);
                subadultTravel += subadultMotor.Velocity.Length() / 60f;
            }
            Need(subadultMotor.Speed >= 1.35f && subadultTravel >= .95f &&
                subadultMotor.Speed < speeds[^1],
                $"subadult sustained forward swim too slow speed={subadultMotor.Speed:F2} travel={subadultTravel:F2}");
            GD.Print($"GROWTH_SUBADULT_SWIM_OK speed={subadultMotor.Speed:F2} travel_1s={subadultTravel:F2}");

            Need(scene.Growth.Stage == "juvenile" && scene.Feeding.Nutrition == 0 &&
                DemoSelection.CharacterId == PondGrowthState.SpeciesId &&
                Near(fish.BodyRadius, scene.Growth.ColliderRadius), "initial fixture state invalid");
            CheckRig();
            await CheckStageWall();
            await CheckJuvenileShoreAscent();
            var open = new Vector3(0, PikeCoverFixture.Bed(0, 20) + 1.2f, 20);
            fish.GlobalPosition = open;
            fish.GlobalBasis = Basis.Identity;
            camera.ResetFollow();
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            var juvenileCameraDistance = camera.GlobalPosition.DistanceTo(fish.GlobalPosition);
            Need(juvenileCameraDistance > fish.BodyRadius + .1f && CameraClear(scene, fish, camera),
                "juvenile camera clearance invalid");
            var nymphs = scene.FoodActors.Where(f => f.Kind == FeedingLedger.InsectId).ToArray();
            Need(nymphs.Length >= 7, "insufficient nymph food for legacy fixture start");
            var wall = new StaticBody3D { Name = "GrowthTestWall", CollisionLayer = 1 };
            scene.AddChild(wall);
            wall.GlobalPosition = open + Vector3.Right * .11f;
            wall.AddChild(new CollisionShape3D { Shape = new BoxShape3D
                { Size = new Vector3(.04f, 1f, 1f) } });
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            for (var i = 0; i < 7; i++)
            {
                fish.GlobalPosition = open;
                fish.GlobalBasis = Basis.Identity;
                nymphs[i].GlobalPosition = open + Vector3.Forward * .43f;
                Need(scene.TryBiteFood(nymphs[i]).Consumed &&
                    !scene.TryBiteFood(nymphs[i]).Consumed, "nymph duplicate/threshold failure " + i);
            }
            Need(scene.Feeding.Nutrition == 14 && scene.Feeding.Events.Count == 7 &&
                scene.Growth.Stage == "juvenile" && !scene.TryAdvanceGrowth(),
                "wall did not defer earned stage");
            wall.QueueFree();
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            fish.GlobalPosition = new Vector3(open.X,
                fish.PondWaterlineHeight - .65f * scene.Growth.RadiusForNext() + .01f,
                open.Z);
            Need(!scene.TryAdvanceGrowth() && scene.Feeding.Nutrition == 14 &&
                scene.Growth.Stage == "juvenile", "surface did not defer earned stage");
            fish.GlobalPosition = open;
            Need(scene.TryAdvanceGrowth() && scene.Growth.Stage == "subadult" &&
                scene.Feeding.Nutrition == 14 && scene.Growth.Identity == identity,
                "growth did not resume after clearance");
            CheckRig();
            await CheckStageWall();
            tree.Paused = true;
            Need(!scene.TryAdvanceGrowth() && !scene.TryBiteFood(nymphs[7]).Consumed &&
                scene.Feeding.Nutrition == 14, "pause advanced nutrition/growth");
            tree.Paused = false;
            var larvae = scene.PreyActors.Where(p => p.Stage == "larva").Take(3).ToArray();
            foreach (var prey in larvae)
            {
                fish.GlobalPosition = open;
                fish.GlobalBasis = Basis.Identity;
                prey.GlobalPosition = open + Vector3.Forward * .75f;
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                Need(scene.TryBite(prey).Consumed && !scene.TryBite(prey).Consumed,
                    "Bluegill larva bite/duplicate failed");
                Need(scene.PredationEvents.Count == scene.Feeding.Events.Count - 7 &&
                    scene.PredationEvents[^1].Id == "bass:bluegill:" + prey.StableId &&
                    scene.PredationEvents[^1].TargetId == prey.StableId &&
                    scene.Blood.SeenEventIds.Contains(scene.PredationEvents[^1].Id),
                    "Bass fish bite did not produce one visual event");
            }
            Need(scene.Growth.Stage == "adult" && scene.Feeding.Nutrition == 26 &&
                scene.Feeding.Events.Count == 10 && scene.Growth.Identity == identity &&
                DemoSelection.CharacterId == PondGrowthState.SpeciesId &&
                Near(fish.BodyRadius, scene.Growth.BodyFor("adult").Radius),
                "adult threshold/identity failed");
            CheckRig();
            await CheckStageWall();
            var juvenileBluegill = scene.PreyActors[0];
            fish.GlobalPosition = open;
            fish.GlobalBasis = Basis.Identity;
            juvenileBluegill.GlobalPosition = open + Vector3.Forward * .75f;
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(scene.TryBite(juvenileBluegill).Consumed &&
                !scene.TryBite(juvenileBluegill).Consumed && scene.Feeding.Nutrition == 32,
                "adult Bluegill-juvenile limit/transaction failed");
            Need(scene.PredationEvents.Count == 4 &&
                scene.Blood.EmissionCount == (scene.Blood.Enabled ? 4 : 0),
                "Bass predation VFX duplicated or FX-off affected nutrition");
            fish.GlobalPosition = open;
            camera.ResetFollow();
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            var adultCameraDistance = camera.GlobalPosition.DistanceTo(fish.GlobalPosition);
            Need(adultCameraDistance > juvenileCameraDistance * 1.5f &&
                CameraClear(scene, fish, camera) && !scene.TryAdvanceGrowth(),
                "adult camera clearance or spurious growth failed");
            ClearanceSurvey(scene, fish);
            ShelterCameraSurvey(scene, fish, camera);
            GD.Print($"GROWTH_CHECK_OK identity={identity} stage={scene.Growth.Stage} nutrition={scene.Feeding.Nutrition} events={scene.Feeding.Events.Count} camera_juvenile={juvenileCameraDistance:F2} camera_adult={adultCameraDistance:F2}");
            tree.Quit();

            void CheckRig()
            {
                var asset = scene.Growth.Asset;
                var visual = fish.GetNode<Node3D>("PlayerVisual");
                var player = visual.FindChild("AnimationPlayer", true, false) as AnimationPlayer;
                var motion = visual.GetNode<CatalogFishVisualMotion>("SwimPresentation");
                var adult = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "adult")!;
                var body = fish.GetNode<CollisionShape3D>("Collision").Shape as CapsuleShape3D;
                CheckMeshInsideBody(fish, scene.Growth);
                Need(asset.Id == PikeEncounterAssets.BassId &&
                    Near(visual.Scale.X, 1f / adult.Dimensions[1]) &&
                    Near(fish.BodyRadius, scene.Growth.ColliderRadius) &&
                    body is not null && Near(body.Height, scene.Growth.ColliderLength) &&
                    Near(body.Radius, scene.Growth.ColliderRadius) &&
                    Near(fish.BodyForwardReach, scene.Growth.ColliderLength * .5f -
                        scene.Growth.ColliderCenterZ) &&
                    Near(fish.GetNode<CollisionShape3D>("Collision").Position.Z,
                        scene.Growth.ColliderCenterZ) &&
                    Near(fish.BodyScale, scene.Growth.VisualLength) &&
                    Near(fish.Tuning.MaxSpeed, scene.Growth.StageTuning(baseline).MaxSpeed) &&
                    PondCreatureCatalog.ExportMatches(asset) && player is not null &&
                    player.HasAnimation(asset.Clip("Glide")) && player.HasAnimation(asset.Clip("Swim")) &&
                    player.HasAnimation(asset.Clip("SwimFast")) && motion is not null,
                    "registered stage rig/collider invalid at " + scene.Growth.Stage);
            }

            async System.Threading.Tasks.Task CheckStageWall()
            {
                fish.ClearAndDisarmInput();
                fish.SetActivePreset(ControlPreset.Builtins().Single(p => p.Id == ControlPreset.KeyboardId));
                var at = new Vector3(0, PikeCoverFixture.Bed(0, 20) + 1.2f, 20);
                fish.GlobalPosition = at;
                fish.GlobalBasis = Basis.Identity;
                var wall = new StaticBody3D { Name = "BassStageWallProbe", CollisionLayer = 1 };
                scene.AddChild(wall);
                wall.GlobalPosition = at + Vector3.Forward * (fish.BodyForwardReach + .15f);
                wall.AddChild(new CollisionShape3D { Shape = new BoxShape3D
                    { Size = new Vector3(2f, 6f, .02f) } });
                fish.SetPhysicsProcess(true);
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                fish._Input(new InputEventKey { PhysicalKeycode = Key.W, Pressed = true });
                var previous = fish.GlobalPosition;
                for (var frame = 0; frame < 120; frame++)
                {
                    await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                    Need(fish.GlobalPosition.DistanceTo(previous) <= fish.Tuning.MaxSpeed / 60f + .02f &&
                        fish.Velocity.Length() <= fish.Tuning.MaxSpeed + .02f,
                        "Bass stage motion jumped at " + scene.Growth.Stage);
                    previous = fish.GlobalPosition;
                }
                fish._Input(new InputEventKey { PhysicalKeycode = Key.W, Pressed = false });
                Need(fish.GlobalPosition.Z >= wall.GlobalPosition.Z + .01f +
                    fish.BodyRadius - .005f,
                    $"Bass stage crossed thin wall at {scene.Growth.Stage} fish={fish.GlobalPosition} wall={wall.GlobalPosition}");
                fish.SetPhysicsProcess(false);
                fish.ClearAndDisarmInput();
                wall.QueueFree();
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                GD.Print($"GROWTH_STAGE_WALL_OK stage={scene.Growth.Stage} speed_cap={fish.Tuning.MaxSpeed:F2} radius={fish.BodyRadius:F4}");
            }

            async System.Threading.Tasks.Task CheckJuvenileShoreAscent()
            {
                fish.ClearAndDisarmInput();
                fish.GlobalPosition = new Vector3(scene.NestPosition.X,
                    fish.PondWaterlineHeight - .4f, scene.NestPosition.Z);
                fish.GlobalBasis = Basis.Identity;
                fish.SetPhysicsProcess(true);
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                fish._Input(new InputEventKey { PhysicalKeycode = Key.Up, Pressed = true });
                fish._Input(new InputEventKey { PhysicalKeycode = Key.W, Pressed = true });
                for (var frame = 0; frame < 240; frame++)
                {
                    await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                    Need(fish.GlobalPosition.Y <= fish.PondSurfaceCenterLimit + .001f &&
                        fish.Velocity.Length() <= fish.Tuning.MaxSpeed + .02f,
                        "juvenile shore ascent breached surface or speed cap");
                }
                fish._Input(new InputEventKey { PhysicalKeycode = Key.Up, Pressed = false });
                fish._Input(new InputEventKey { PhysicalKeycode = Key.W, Pressed = false });
                fish.SetPhysicsProcess(false);
                fish.ClearAndDisarmInput();
                GD.Print($"GROWTH_JUVENILE_SHORE_OK y={fish.GlobalPosition.Y:F3} waterline={fish.PondWaterlineHeight:F3} speed_cap={fish.Tuning.MaxSpeed:F2}");
            }
        }
        catch (Exception error)
        {
            GD.PushError("GROWTH_CHECK_FAILED " + error);
            tree.Paused = false;
            tree.Quit(1);
        }
    }

    public static void CheckMeshInsideBody(FishController fish, PondGrowthState growth)
    {
        var visual = fish.GetNode<Node3D>("PlayerVisual");
        var profile = growth.Body;
        var vertices = 0;
        foreach (var found in visual.FindChildren("*", "MeshInstance3D", true, false))
        {
            if (found is not MeshInstance3D { Mesh: not null } mesh) continue;
            for (var surface = 0; surface < mesh.Mesh.GetSurfaceCount(); surface++)
            {
                foreach (var vertex in mesh.Mesh.SurfaceGetArrays(surface)
                    [(int)Mesh.ArrayType.Vertex].AsVector3Array())
                {
                    var point = fish.ToLocal(mesh.GlobalTransform * vertex);
                    Need(profile.Contains(point, .0001f),
                        $"Bass mesh escapes {growth.Stage} collider at {point}");
                    vertices++;
                }
            }
        }
        Need(vertices > 0, "Bass stage mesh has no measured vertices");
        GD.Print($"BASS_BODY_FIT_OK stage={growth.Stage} vertices={vertices} radius={profile.Radius:F4} height={profile.Height:F4} center_z={profile.CenterZ:F4}");
    }

    private static void ShelterCameraSurvey(PikeEncounter scene, FishController fish,
        FollowCamera camera)
    {
        foreach (var id in new[] { "FAF-WOOD-007A", "FAF-WOOD-008A" })
        {
            var placement = scene.CoverPlan.Single(p => p.Id == id);
            var passed = false;
            for (var radius = 1.2f; radius <= 3f && !passed; radius += .3f)
            for (var i = 0; i < 8 && !passed; i++)
            {
                var angle = i * Mathf.Tau / 8f;
                var x = placement.Position.X + Mathf.Cos(angle) * radius;
                var z = placement.Position.Z + Mathf.Sin(angle) * radius;
                var at = new Vector3(x, PikeCoverFixture.Bed(x, z) + .9f, z);
                if (at.Y > fish.PondSurfaceCenterLimit) continue;
                using var shape = new CapsuleShape3D
                { Radius = fish.BodyRadius, Height = scene.Growth.ColliderLength };
                using var query = new PhysicsShapeQueryParameters3D
                {
                    Shape = shape,
                    Transform = new Transform3D(fish.GlobalBasis *
                        new Basis(Vector3.Right, Mathf.Pi * .5f),
                        at + fish.GlobalBasis.Z * scene.Growth.ColliderCenterZ),
                    CollisionMask = fish.CollisionMask,
                    Exclude = new Godot.Collections.Array<Rid> { fish.GetRid() },
                };
                if (scene.GetWorld3D().DirectSpaceState.IntersectShape(query, 1).Count != 0)
                    continue;
                fish.GlobalPosition = at;
                camera.SnapToFish();
                passed = CameraClear(scene, fish, camera);
            }
            Need(passed, "adult camera/body has no clear pose near " + id);
            GD.Print($"GROWTH_SHELTER_CAMERA_OK id={id} distance={fish.GlobalPosition.DistanceTo(placement.Position):F2}");
        }
    }

    private static void ClearanceSurvey(PikeEncounter scene, FishController fish)
    {
        var catalog = PikeCoverFixture.LoadCatalog();
        var juvenileRadius = scene.Growth.BodyFor("juvenile").Radius;
        var adultRadius = scene.Growth.BodyFor("adult").Radius;
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
                        if (at.Y > PikeCoverFixture.WaterY - .65f * adultRadius) continue;
                        var juvenile = Clear(juvenileRadius, at);
                        var adult = Clear(adultRadius, at);
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
                    new SightTarget(center, adultRadius, 1f),
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
            using var shape = new SphereShape3D { Radius = radius };
            using var query = new PhysicsShapeQueryParameters3D
            {
                Shape = shape,
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
