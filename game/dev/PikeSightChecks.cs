using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fish;

/// <summary>Fixed seed 5202 physics and world-input sensing checks; no native gameplay claim.</summary>
public static class PikeSightChecks
{
    public static async void Run(PikeEncounter scene)
    {
        var tree = scene.GetTree();
        try
        {
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            var fish = scene.GetNode<FishController>("Fish");
            var pike = scene.GetNode<CharacterBody3D>("NorthernPike");
            var visual = PikeCoverFixture.VisibleSoftCover(scene.GetNode<Node3D>("Cover"));
            Need(visual.Count is > 0 and <= 128, "visible foliage budget");
            Need(visual.Select(p => p.FafId[..13]).Distinct().Count() >= 5,
                "soft cover families missing");
            var water = scene.SightWater!;
            SightEvidence Query(Vector3 from, Vector3 to, SightEnvironment env,
                bool usePlants = true, float radius = .48f) =>
                new WorldSightSensor(scene.GetWorld3D().DirectSpaceState,
                    usePlants ? visual : Array.Empty<SoftSightCover>(), pike.GetRid(), fish.GetRid())
                    .Observe(from, to - from, new SightTarget(to, radius, 1f), env, 0, false);

            var near = Query(new Vector3(0, 1, 9), new Vector3(0, 1, 5), water);
            Need(near.Detected && near.ClearRays == 3, "near open water invisible " + near);
            Need(!Query(new Vector3(0, 1, 31), new Vector3(0, 1, -1), water).Detected,
                "far target visible");
            var back = new WorldSightSensor(scene.GetWorld3D().DirectSpaceState, visual,
                pike.GetRid(), fish.GetRid()).Observe(new Vector3(0, 1, 9), Vector3.Forward,
                new SightTarget(new Vector3(0, 1, 12), .48f, 1), water, 0, false);
            Need(back.Reason == "orientation", "rear target observed");

            foreach (var (from, to) in new[]
            {
                (new Vector3(0, 2, 18), new Vector3(0, 2, 12)),
                (new Vector3(0, .4f, 5), new Vector3(0, .4f, -1)),
                (new Vector3(0, -2.1f, -14), new Vector3(0, -2.1f, -20)),
            })
            {
                var open = Query(from, to, water);
                Need(open.ClearRays > 0 && open.WaterSamples == 7, "depth line blocked " + open);
                var small = Query(from, to, water, radius: .25f);
                Need(small.ClearRays > 0 && small.Strength < open.Strength,
                    "target silhouette not effective");
            }
            var clear = new SightEnvironment(WorldEnvironmentProfile.Pond, 3.5f,
                _ => 0f, SolarState.At(.5));
            var mud = new SightEnvironment(WorldEnvironmentProfile.Pond, 3.5f,
                p => p.Z is < 7 and > -3 ? .9f : 0f, SolarState.At(.5));
            var start = new Vector3(0, .7f, 10);
            var end = new Vector3(0, .7f, -2);
            var clearPath = Query(start, end, clear);
            var mudPath = Query(start, end, mud);
            Need(clearPath.Strength > mudPath.Strength && clearPath.Water > mudPath.Water,
                "intervening sediment ignored");
            var night = new SightEnvironment(WorldEnvironmentProfile.Pond, 3.5f,
                _ => 0f, SolarState.At(0));
            Need(Query(start, end, clear).Strength > Query(start, end, night).Strength,
                "solar illumination ignored");
            var pond = new GeneratedBasin(71, pond: true);
            var pondField = SightEnvironment.ForBakedWorld(pond,
                GeneratedBasin.WaterSurfaceY, SolarState.At(.5));
            var point = new Vector3(pond.DeepCore.X, pond.DeepFloorY + 2f, pond.DeepCore.Y);
            Need(pondField.Sediment(point) > 0f &&
                Mathf.IsEqualApprox(pondField.Sediment(point),
                    new PondTurbidityField(new GeneratedBasin(71, pond: true),
                        GeneratedBasin.WaterSurfaceY).Sample(point)), "Pond field drift");
            Need(SightEnvironment.ForBakedWorld(new GeneratedBasin(42), 10.39f,
                SolarState.At(.5)).Sediment(Vector3.Zero) == 0f, "Ocean baseline sediment");

            var sample = new WorldSightSensor(scene.GetWorld3D().DirectSpaceState, visual,
                pike.GetRid(), fish.GetRid());
            var one = sample.Observe(start, end - start, new SightTarget(end, .48f, 1), clear, 1, false);
            var held = sample.Observe(start, end - start, new SightTarget(end + Vector3.Right, .48f, 1),
                mud, 1.05, false);
            Need(one.Equals(held), "query frequency unbounded");
            Need(sample.Observe(start, end - start, new SightTarget(end, .48f, 1), clear,
                1.06, true).Reason == "paused", "pause query advanced");
            Need(sample.Observe(start, end - start, new SightTarget(end, .48f, 1), mud,
                1.06, false).Strength < one.Strength, "pause kept stale sample");
            Need(sample.Observe(start, end - start, null, clear, 1.06, false).Reason ==
                "invalid-target", "invalid target accepted");
            Need(sample.Observe(start, end - start, new SightTarget(end, .48f, 1),
                new SightEnvironment((WorldEnvironmentProfile)99, 3.5f, _ => 0f,
                    SolarState.At(.5)), 1.06, false).Reason == "unknown-habitat",
                "unknown habitat accepted");
            sample.Reset();
            Need(sample.Observe(start, end - start, new SightTarget(end, .48f, 1), mud,
                1.06, false).Strength < one.Strength, "reset kept stale sample");

            var camera = fish.GetNode<Camera3D>("CameraRig/Camera3D");
            var saved = camera.Transform;
            var envNode = scene.GetNode<WorldEnvironment>("PondColumnLight").Environment;
            var fog = envNode.FogEnabled;
            var before = Query(start, end, clear);
            camera.Position += new Vector3(8, 4, -9);
            envNode.FogEnabled = !fog;
            var after = Query(start, end, clear);
            camera.Transform = saved;
            envNode.FogEnabled = fog;
            Need(before.Equals(after), "camera or fog changed world sight");

            var catalog = PikeCoverFixture.LoadCatalog();
            foreach (var id in new[] { "FAF-WOOD-005A", "FAF-WOOD-006A", "FAF-WOOD-007A",
                "FAF-WOOD-008A", "FAF-WOOD-009A" })
            {
                var placement = scene.CoverPlan.Single(p => p.Id == id);
                var basis = new Basis(Vector3.Up, placement.Yaw);
                var asset = catalog.Assets.Single(a => a.Id == id);
                var half = asset.Size[0] * .7f + 1f;
                var blocked = false;
                var opening = false;
                var blockedCount = 0;
                var openCount = 0;
                foreach (var y in new[] { .8f, 1.5f, 2.3f, 3.2f })
                foreach (var z in new[] { -2f, 0f, 2f })
                {
                    var from = placement.Position + basis * new Vector3(-half, y, z);
                    var to = placement.Position + basis * new Vector3(half, y, z);
                    var result = Query(from, to, clear, usePlants: false);
                    blocked |= result.Reason == "hard-cover";
                    opening |= result.ClearRays > 0;
                    if (result.Reason == "hard-cover") blockedCount++;
                    if (result.ClearRays > 0) openCount++;
                }
                if (id == "FAF-WOOD-008A")
                {
                    var from = placement.Position + basis * new Vector3(0, 1.95f, 7.7f * .55f);
                    var to = placement.Position + basis * new Vector3(0, 1.95f, -7.7f * .55f);
                    var result = Query(from, to, clear, usePlants: false);
                    opening |= result.ClearRays > 0;
                    if (result.ClearRays > 0) openCount++;
                }
                if (id == "FAF-WOOD-007A")
                {
                    var from = placement.Position + basis * new Vector3(0, 1.4f, 6.4f);
                    var to = placement.Position + basis * new Vector3(0, 1.4f, 3.6f);
                    Need(Query(from, to, clear, usePlants: false).ClearRays > 0,
                        "root-ball undercut visual refuge closed");
                }
                if (id == "FAF-WOOD-009A")
                {
                    var from = placement.Position + basis * new Vector3(-1.8f, 1.4f, 2);
                    var to = placement.Position + basis * new Vector3(-1.8f, 4.2f, .5f);
                    var result = Query(from, to, clear, usePlants: false);
                    opening |= result.ClearRays > 0;
                    if (result.ClearRays > 0) openCount++;
                }
                Need(blocked && opening, id + $" solid/open sightlines absent blocked={blockedCount} open={openCount}");
            }
            foreach (var family in new[] { "010", "011", "012", "013", "014" })
            {
                var softened = false;
                foreach (var proxy in visual.Where(p => p.FafId.Contains("PLANT-" + family)))
                foreach (var height in new[] { .35f, .6f, .82f })
                {
                    var center = proxy.VisibleBounds.GetCenter();
                    var y = Mathf.Max(proxy.VisibleBounds.Position.Y + height * proxy.VisibleBounds.Size.Y,
                        PikeCoverFixture.Bed(center.X, center.Z) + .6f);
                    var from = new Vector3(proxy.VisibleBounds.Position.X - 1f, y, center.Z);
                    var to = new Vector3(proxy.VisibleBounds.End.X + 1f, y, center.Z);
                    var with = Query(from, to, clear);
                    var without = Query(from, to, clear, usePlants: false);
                    softened |= with.ClearRays > 0 && with.SoftCover < without.SoftCover;
                }
                Need(softened, family + " visible foliage did not soften an open line");
            }
            GD.Print($"PIKE_SIGHT_CHECK_OK seed={PikeCoverFixture.Seed} plants={visual.Count} near={near.Strength:F3} clear={clearPath.Strength:F3} mud={mudPath.Strength:F3} camera=stable wood=5 plants=5");
            tree.Quit();
        }
        catch (Exception error)
        {
            GD.PushError("PIKE_SIGHT_CHECK_FAILED " + error);
            tree.Quit(1);
        }
    }

    private static void Need(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
