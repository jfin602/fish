#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fish;

/// <summary>Fixed input replay through the live fish motor for W2 Forward+ A/B.</summary>
public static class PondWakeRenderNativeProbe
{
    public static async void Run(PikeEncounter scene)
    {
        var tree = scene.GetTree();
        try
        {
            if (RenderingServer.GetCurrentRenderingMethod() != "gl_compatibility" &&
                RenderingServer.GetCurrentRenderingMethod() != "forward_plus")
                throw new Exception("unexpected renderer");
            var args = OS.GetCmdlineUserArgs();
            var shoreScenario = args.Contains("--wake-scenario=shore");
            var mode = (args.Contains("--wake-comparison=new") ? "new" : "rings") +
                (shoreScenario ? "-shore" : "");
            var outputArg = args.FirstOrDefault(x => x.StartsWith("--wake-render-output=", StringComparison.Ordinal));
            if (outputArg is null) throw new Exception("missing output directory");
            var folder = Path.Combine(outputArg["--wake-render-output=".Length..], mode);
            Directory.CreateDirectory(folder);
            scene.SetPhysicsProcess(false);
            scene.GetNode<CanvasLayer>("FixtureHud").Visible = false;
            var fish = scene.GetNode<FishController>("Fish");
            foreach (var prey in scene.PreyActors) prey.SetPhysicsProcess(false);
            var water = fish.GetNode<WaterReaction>("WaterReaction");
            if (water.GetNodeOrNull<MeshInstance3D>("SurfaceRing0") is null)
                throw new Exception("debug A/B ring pool missing");
            water.SetWakeLegacyComparison(mode.StartsWith("rings", StringComparison.Ordinal));
            var rig = fish.GetNode<FollowCamera>("CameraRig");
            rig.SetProcess(false);
            rig.SetPhysicsProcess(false);
            scene.GetViewport().SizeChanged -= rig.SnapToFish;
            var camera = fish.GetNode<Camera3D>("CameraRig/Camera3D");
            camera.Current = true;
            camera.Fov = 50f;
            var surface = scene.GetNode<UnderwaterAtmosphere>("UnderwaterAtmosphere");
            // Freeze automatic ticks before the first await. Otherwise an
            // expensive scene load can start A/B at different wave phases.
            fish.SetPhysicsProcess(false);
            water.SetPhysicsProcess(false);
            surface.SetPhysicsProcess(false);
            scene.GetNode<EnvironmentClock>("EnvironmentClock").SetPhysicsProcess(false);
            fish.SetInputMode("keyboard");
            for (var i = 0; i < 3; i++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                surface._PhysicsProcess(1d / 60d);
                fish._PhysicsProcess(1d / 60d);
                water._PhysicsProcess(1d / 60d);
            }
            // The origin intersects central habitat props in the baked fixture.
            // Start in an open water lane so input exercises the live motor.
            const float routeZ = -30f;
            fish.GlobalPosition = new Vector3(0f,
                fish.PondSurfaceLimitAt(0f, routeZ) - .005f, routeZ);
            if (shoreScenario)
            {
                var basin = scene.GetNode<SeabedSurface>("Habitat/Seabed").Basin;
                var bestZ = -50f;
                var error = float.MaxValue;
                for (var z = -65f; z < -20f; z += .1f)
                {
                    var difference = Mathf.Abs(basin.DistanceToShore(0f, z) - .9f);
                    if (difference >= error) continue;
                    error = difference;
                    bestZ = z;
                }
                fish.GlobalPosition = new Vector3(0f,
                    fish.PondSurfaceLimitAt(0f, bestZ) - .005f, bestZ);
                GD.Print($"WAKE_RENDER_SHORE start_z={bestZ:F3} shore_distance={basin.DistanceToShore(0f, bestZ):F3}");
            }
            fish.GlobalBasis = Basis.Identity;
            water.Suspend();
            // Capture PNGs can skip arbitrary engine physics frames. Advance the
            // three dependent owners once per scripted tick so A/B input, motor,
            // historical path and wave phase remain identical.
            var frames = new List<double>();
            var draws = new List<double>();
            var primitives = new List<double>();
            var captures = shoreScenario
                ? new (int Tick, string Name, string View)[]
                {
                    (25, "shore-top", "top"),
                    (60, "shore-oblique", "oblique"),
                }
                : new (int Tick, string Name, string View)[]
                {
                    (25, "straight-top", "top"),
                    (64, "curve-oblique-a", "oblique"),
                    (65, "curve-oblique-b", "oblique"),
                    (75, "curve-waterline", "waterline"),
                    (95, "boost-waterline-a", "waterline"),
                    (96, "boost-waterline-b", "waterline"),
                    (125, "glide-top", "top"),
                    (145, "coast-oblique", "oblique"),
                    (165, "dive-under", "under"),
                    (195, "deep-under", "under"),
                    (360, "rest-top", "top"),
                };
            var captureIndex = 0;
            KeyInput(fish, Key.W, true);
            var lastFrame = Time.GetTicksUsec();
            for (var tick = 0; tick <= (shoreScenario ? 70 : 365); tick++)
            {
                if (!shoreScenario)
                {
                    if (tick == 40) KeyInput(fish, Key.D, true);
                    if (tick == 76) { KeyInput(fish, Key.D, false); KeyInput(fish, Key.Space, true); }
                    if (tick == 110) { KeyInput(fish, Key.Space, false); KeyInput(fish, Key.W, false); }
                    if (tick == 150) { KeyInput(fish, Key.W, true); KeyInput(fish, Key.Down, true); }
                    if (tick == 180) { KeyInput(fish, Key.W, false); KeyInput(fish, Key.Down, false); }
                }
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                surface._PhysicsProcess(1d / 60d);
                fish._PhysicsProcess(1d / 60d);
                water._PhysicsProcess(1d / 60d);
                await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
                var now = Time.GetTicksUsec();
                if (tick >= 15)
                {
                    frames.Add((now - lastFrame) / 1000.0);
                    draws.Add(Performance.GetMonitor(Performance.Monitor.RenderTotalDrawCallsInFrame));
                    primitives.Add(Performance.GetMonitor(Performance.Monitor.RenderTotalPrimitivesInFrame));
                }
                lastFrame = now;
                if (captureIndex >= captures.Length || captures[captureIndex].Tick != tick) continue;
                var shot = captures[captureIndex++];
                PlaceCamera(camera, fish, surface, shot.View);
                await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
                var file = Path.Combine(folder, shot.Name + ".png");
                if (scene.GetViewport().GetTexture().GetImage().SavePng(file) != Error.Ok)
                    throw new Exception("capture failed " + file);
                var position = fish.GlobalPosition;
                if (!shoreScenario && tick == 25 && position.Z > routeZ - .05f)
                    throw new Exception("central replay did not translate through water");
                if (!shoreScenario && tick == 95 && !fish.BoostActive)
                    throw new Exception("central replay did not engage boost");
                var body = PlayerBodyGeometry.Measure(fish);
                var contact = PondWakeContacts.At(body, fish.GlobalTransform,
                    surface.GetSurfaceHeight);
                GD.Print($"WAKE_RENDER_CAPTURE mode={mode} tick={tick} name={shot.Name} view={shot.View} " +
                    $"pos={position} speed={fish.Velocity.Length():F3} boost={fish.BoostActive} " +
                    $"samples={water.WakePath.Count} triangles={water.ActiveWakeTriangles} " +
                    $"rings={water.ActivePondRings} surface={surface.GetSurfaceHeight(position.X, position.Z):F4} " +
                    $"clock={surface.SurfaceTime:F4} contact={contact.Touching} " +
                    $"top_minus_surface={position.Y + body.TopReach - contact.SurfaceY:F4} file={file}");
                // PNG encoding and disk I/O are outside the frame-cost sample.
                lastFrame = Time.GetTicksUsec();
            }
            KeyInput(fish, Key.W, false);
            KeyInput(fish, Key.Space, false);
            KeyInput(fish, Key.D, false);
            KeyInput(fish, Key.Down, false);
            if (captureIndex != captures.Length) throw new Exception("missing capture");
            GD.Print($"WAKE_RENDER_COST mode={mode} frames={frames.Count} " +
                $"frame_median_ms={Percentile(frames, .5):F3} frame_p95_ms={Percentile(frames, .95):F3} " +
                $"draws_median={Percentile(draws, .5):F0} draws_p95={Percentile(draws, .95):F0} " +
                $"primitives_median={Percentile(primitives, .5):F0} primitives_p95={Percentile(primitives, .95):F0} " +
                $"adapter={RenderingServer.GetVideoAdapterName()} renderer={RenderingServer.GetCurrentRenderingMethod()}");
            GD.Print($"WAKE_RENDER_NATIVE_OK mode={mode} captures={captureIndex} input_replay=yes");
            tree.Quit();
        }
        catch (Exception error)
        {
            GD.PushError("WAKE_RENDER_NATIVE_FAILED " + error);
            tree.Quit(1);
        }
    }

    private static void KeyInput(FishController fish, Key key, bool pressed) =>
        fish._Input(new InputEventKey { Keycode = key, PhysicalKeycode = key, Pressed = pressed });

    private static void PlaceCamera(Camera3D camera, FishController fish,
        UnderwaterAtmosphere surface, string view)
    {
        var at = fish.GlobalPosition;
        var waterY = surface.GetSurfaceHeight(at.X, at.Z);
        var look = new Vector3(at.X, waterY, at.Z);
        switch (view)
        {
            case "top":
                camera.GlobalPosition = look + new Vector3(0f, 1.65f, .03f);
                camera.LookAt(look, Vector3.Forward);
                break;
            case "oblique":
                camera.GlobalPosition = look + new Vector3(1.05f, .75f, 1.40f);
                camera.LookAt(look, Vector3.Up);
                break;
            case "waterline":
                camera.GlobalPosition = look + new Vector3(1.35f, .055f, .18f);
                camera.LookAt(look + Vector3.Down * .02f, Vector3.Up);
                break;
            default:
                camera.GlobalPosition = look + new Vector3(1.25f, -.28f, .1f);
                camera.LookAt(look + Vector3.Up * .05f, Vector3.Up);
                break;
        }
    }

    private static double Percentile(List<double> values, double fraction)
    {
        values.Sort();
        return values[(int)Math.Clamp(Math.Round((values.Count - 1) * fraction), 0, values.Count - 1)];
    }
}
