#nullable enable
using Godot;
using System;
using System.Linq;

namespace Fish;

/// <summary>W1 body/water/path checks, run inside the real Pike Pond scene.</summary>
public static class PondWakePathChecks
{
    public static async void Run(PikeEncounter scene)
    {
        var tree = scene.GetTree();
        try
        {
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            scene.SetPhysicsProcess(false);
            var fish = scene.GetNode<FishController>("Fish");
            var water = fish.GetNode<WaterReaction>("WaterReaction");
            Need(water.GetNodeOrNull<MeshInstance3D>("WakePathDebug") is null &&
                water.ActivePondRings <= WaterReaction.PondRingCapacity,
                "production path debug leaked or old ring budget changed");
            fish.SetInputMode("keyboard");
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            fish.GlobalPosition = new Vector3(0f,
                fish.PondSurfaceLimitAt(0f, 0f) - .005f, 0f);
            fish._Input(new InputEventKey { Keycode = Key.W,
                PhysicalKeycode = Key.W, Pressed = true });
            for (var i = 0; i < 80; i++)
            {
                if (i == 45)
                    fish._Input(new InputEventKey { Keycode = Key.D,
                        PhysicalKeycode = Key.D, Pressed = true });
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            }
            fish._Input(new InputEventKey { Keycode = Key.D,
                PhysicalKeycode = Key.D, Pressed = false });
            fish._Input(new InputEventKey { Keycode = Key.W,
                PhysicalKeycode = Key.W, Pressed = false });
            Need(water.WakePath.Count > 3, "actual motor route made no surface path");
            var liveCount = water.WakePath.Count;
            var firstLive = water.WakePath[0];
            Need(firstLive.Center.DistanceTo(new Vector2(fish.GlobalPosition.X,
                fish.GlobalPosition.Z)) > .05f, "live history followed current fish");
            fish.ClearAndDisarmInput();
            fish.SetPhysicsProcess(false);
            water.SetPhysicsProcess(false);
            water.Suspend();
            var atmosphere = scene.GetNode<UnderwaterAtmosphere>("UnderwaterAtmosphere");
            var originalPose = fish.GlobalTransform;
            var originalVelocity = fish.Velocity;
            var camera = fish.GetNode<Camera3D>("CameraRig/Camera3D");
            var stageLengths = new float[4];
            var index = 0;
            foreach (var stage in new[] { "larva", "juvenile", "subadult", "adult" })
            {
                var growth = new PondGrowthState("wake-check", stage);
                Need(PlayerArt.TryReplacePondStage(fish, growth.Entry(growth.Asset),
                    out var reason), "stage visual " + stage + " " + reason);
                fish.SetEncounterBassBody(growth.Body);
                water.ConfigureFromPlayerVisual();
                var body = PlayerBodyGeometry.Measure(fish);
                stageLengths[index++] = body.Length;
                var nearY = fish.PondSurfaceHeightAt(fish.GlobalPosition.X,
                    fish.GlobalPosition.Z) - body.TopReach * .35f;
                var pose = new Transform3D(fish.GlobalBasis,
                    new Vector3(fish.GlobalPosition.X, nearY, fish.GlobalPosition.Z));
                var contact = PondWakeContacts.At(body, pose, fish.PondSurfaceHeightAt);
                var cameraPose = camera.GlobalTransform;
                camera.GlobalPosition += new Vector3(3f, 2f, -1f);
                var cameraIndependent = PondWakeContacts.At(body, pose, fish.PondSurfaceHeightAt);
                camera.GlobalTransform = cameraPose;
                Need(contact == cameraIndependent, "camera altered body contacts " + stage);
                Need(contact.Touching && contact.Width > 0f &&
                    contact.Bow.DistanceTo(contact.Tail) > body.Length * .5f &&
                    contact.Left.DistanceTo(contact.Right) > body.Radius &&
                    contact.Left.DistanceTo(contact.Right) < body.Radius * 1.4f,
                    "stage rest-pose anchor geometry " + stage);
                foreach (var point in new[] { contact.Bow, contact.Left, contact.Right, contact.Tail })
                    Need(Mathf.Abs(point.Y - atmosphere.GetSurfaceHeight(point.X, point.Z)) < .0001f,
                        "anchor used a different wave provider " + stage);
                var tilted = PondWakeContacts.At(body,
                    new Transform3D(new Basis(Vector3.Up, .8f) *
                        new Basis(Vector3.Right, .25f), pose.Origin), fish.PondSurfaceHeightAt);
                Need(tilted.Left.DistanceTo(contact.Left) > body.Radius * .1f &&
                    tilted.Heading.DistanceTo(contact.Heading) > .1f,
                    "yaw/pitch ignored " + stage);
                var deep = PondWakeContacts.At(body,
                    new Transform3D(pose.Basis, pose.Origin - Vector3.Up * body.Radius * 5f),
                    fish.PondSurfaceHeightAt);
                Need(!deep.Touching, "deep contact " + stage);
                Need(water.WakePath.Count == 0, "growth did not reset history " + stage);
                GD.Print($"WAKE_STAGE stage={stage} length={body.Length:F4} radius={body.Radius:F4} top={body.TopReach:F4} width={contact.Width:F4} surface={contact.SurfaceY:F4}");
            }
            Need(stageLengths.Zip(stageLengths.Skip(1), (a, b) => b > a).All(x => x),
                "stage dimensions did not grow");
            Need(fish.GlobalTransform == originalPose && fish.Velocity == originalVelocity,
                "presentation checks changed authoritative pose/velocity");
            var crest = float.NegativeInfinity;
            var trough = float.PositiveInfinity;
            for (var i = 0; i < 800; i++)
            {
                var displacement = PondWaterline.Displacement(i * .13f - 50f,
                    i * .071f - 24f, i * .019f, atmosphere.SurfaceWaveHeight,
                    atmosphere.SurfaceWaveSpeed, atmosphere.SurfaceWaveDetail, 1f);
                crest = Mathf.Max(crest, displacement);
                trough = Mathf.Min(trough, displacement);
                Need(Mathf.Abs(displacement) <= atmosphere.SurfaceWaveHeight * .202f &&
                    PondWaterline.Displacement(i * .13f, i * .071f, i * .019f,
                        atmosphere.SurfaceWaveHeight, atmosphere.SurfaceWaveSpeed,
                        atmosphere.SurfaceWaveDetail, 0f) == 0f,
                    "wave extrema or dry-shore damping");
            }
            Need(crest > .08f && trough < -.08f, "wave extrema were not exercised");
            CheckPaths();
            GD.Print($"WAKE_PATH_CHECK_OK live_samples={liveCount} stages=4 frame_rates=30,60,120 turns=ok reverse=ok deep_blocked=ok pause_growth_retry=ok capacity=48 wave=shared crest={crest:F4} trough={trough:F4}");
            tree.Quit();
        }
        catch (Exception error)
        {
            GD.PushError("WAKE_PATH_CHECK_FAILED " + error);
            tree.Quit(1);
        }
    }

    private static void CheckPaths()
    {
        PondWakeContacts At(float x, float z, Vector2 heading, bool touching = true)
        {
            var center = new Vector3(x, 0f, z);
            var side = new Vector3(-heading.Y, 0f, heading.X) * .12f;
            return new PondWakeContacts(center + new Vector3(0, 0, -.2f), center - side,
                center + side, center + new Vector3(0, 0, .3f),
                new Vector3(heading.X, 0f, heading.Y), 0f, .24f, touching);
        }
        PondWakePath Route(int fps)
        {
            var path = new PondWakePath();
            for (var frame = 0; frame <= fps * 2; frame++)
            {
                var t = frame / (float)fps;
                var x = t <= 1f ? t * t * .6f : .6f + (t - 1f) * .8f;
                var z = t <= 1f ? 0f : (t - 1f) * (t - 1f) * .8f;
                var heading = t <= 1f ? new Vector2(1, 0) :
                    new Vector2(.8f, 1.6f * (t - 1f)).Normalized();
                path.Step(At(x, z, heading), .8f, .7f, 1, 1f / fps);
            }
            return path;
        }
        var slow = Route(30);
        var medium = Route(60);
        var fast = Route(120);
        Need(slow.Count > 8 && medium.Count > 8 && fast.Count > 8 &&
            Math.Abs(slow.Count - medium.Count) <= 3 && Math.Abs(fast.Count - medium.Count) <= 3,
            "frame-rate dependent sampling density");
        foreach (var path in new[] { slow, medium, fast })
        {
            Need(path.Count <= PondWakePath.Capacity &&
                path[path.Count - 1].Center.DistanceTo(new Vector2(1.4f, .8f)) < .1f,
                "route endpoint/capacity");
            var old = path[0];
            Need(old.Center.Y < .1f && old.Heading.Y < .1f &&
                path[path.Count - 1].Heading.Y > .7f,
                "old turn history rotated with new heading");
            Need(old.Left != old.Right && path[path.Count - 1].Left != path[path.Count - 1].Right,
                "side source positions missing");
            for (var i = 1; i < path.Count; i++)
                Need(path[i].BirthSeconds > path[i - 1].BirthSeconds &&
                    path[i].Center.DistanceTo(path[i - 1].Center) < .25f &&
                    path[i].BirthSeconds - path[i - 1].BirthSeconds < .25f,
                    "time/space sampling bounds");
        }
        var reverse = new PondWakePath();
        for (var i = 0; i < 40; i++)
            reverse.Step(At(-i * .02f, 0f, Vector2.Right), .5f, .5f, 1, 1f / 60f);
        Need(reverse.Count > 2 && reverse[reverse.Count - 1].Center.X < reverse[0].Center.X,
            "reverse path missing");
        var before = reverse.Count;
        for (var i = 0; i < 30; i++)
            reverse.Step(At(-.78f, 0f, Vector2.Right), .5f, 1f, 1, 1f / 60f);
        Need(reverse.Count <= before && reverse.Count > 0, "blocked boost emitted samples");
        for (var i = 0; i < 20; i++)
            reverse.Step(At(-.78f - i * .03f, 0f, Vector2.Right, false),
                .5f, 1f, 1, 1f / 60f);
        Need(reverse.Count <= before, "deep movement emitted samples");
        var oldSegment = reverse[reverse.Count - 1].Segment;
        for (var i = 0; i < 10; i++)
            reverse.Step(At(-1.38f - i * .02f, 0f, Vector2.Right),
                .5f, 1f, 1, 1f / 60f);
        Need(reverse[reverse.Count - 1].Segment != oldSegment,
            "resurfacing joined old wake path");
        reverse.Suspend();
        Need(reverse.Count == 0, "pause/focus did not clear");
        reverse.Step(At(20f, 20f, Vector2.Right), .5f, 1f, 1, 1f / 60f);
        Need(reverse.Count == 0, "resume linked distant position");
        for (var i = 1; i <= 20; i++)
            reverse.Step(At(20f + i * .03f, 20f, Vector2.Right), .5f, 1f, 1, 1f / 60f);
        Need(reverse.Count > 0, "resume did not restart locally");
        reverse.Step(At(40f, 40f, Vector2.Right), .5f, 1f, 1, 1f / 60f);
        Need(reverse.Count == 0, "teleport linked remote locations");
        for (var i = 0; i < 600; i++)
            reverse.Step(At(40f + i * .03f, 40f, Vector2.Right), .5f, 1f, 2, 1f / 120f);
        Need(reverse.Count <= PondWakePath.Capacity && reverse.Count > 0 &&
            reverse.Seconds - reverse[0].BirthSeconds <= PondWakePath.Lifetime,
            "fixed capacity/lifetime budget");
        Need(reverse.Count == PondWakePath.Capacity, "fixed storage did not wrap");
        reverse.Clear();
        Need(reverse.Count == 0, "growth/retry reset");
        var noBacklog = new PondWakePath();
        noBacklog.Step(At(0, 0, Vector2.Right), .5f, 1f, 1, 1f / 60f);
        noBacklog.Step(At(.1f, 0, Vector2.Right), .5f, 1f, 1, 2f);
        Need(noBacklog.Count == 0, "long pause delta made backlog");
    }

    private static void Need(bool ok, string message)
    { if (!ok) throw new Exception(message); }
}
