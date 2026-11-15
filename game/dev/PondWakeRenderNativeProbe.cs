#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;

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
            var lightScenario = args.Contains("--wake-scenario=light");
            var breachScenario = args.Contains("--wake-scenario=breach");
            var followScenario = args.Contains("--wake-scenario=follow");
            var stageArg = args.FirstOrDefault(x => x.StartsWith("--wake-stage=", StringComparison.Ordinal));
            var stage = stageArg?["--wake-stage=".Length..];
            var depthArg = args.FirstOrDefault(x => x.StartsWith("--wake-depth=", StringComparison.Ordinal));
            var startDepth = depthArg is null ? (float?)null :
                float.Parse(depthArg["--wake-depth=".Length..], CultureInfo.InvariantCulture);
            var repair = args.FirstOrDefault(x => x.StartsWith("--wake-repair=", StringComparison.Ordinal));
            var mode = repair is null ? (args.Contains("--wake-comparison=new") ? "new" : "rings") :
                repair["--wake-repair=".Length..];
            if (shoreScenario) mode += "-shore";
            var label = args.FirstOrDefault(x => x.StartsWith("--wake-capture-label=", StringComparison.Ordinal));
            if (label is not null) mode = label["--wake-capture-label=".Length..];
            var outputArg = args.FirstOrDefault(x => x.StartsWith("--wake-render-output=", StringComparison.Ordinal));
            if (outputArg is null) throw new Exception("missing output directory");
            var folder = Path.Combine(outputArg["--wake-render-output=".Length..], mode);
            Directory.CreateDirectory(folder);
            scene.SetPhysicsProcess(false);
            scene.GetNode<CanvasLayer>("FixtureHud").Visible = false;
            var fish = scene.GetNode<FishController>("Fish");
            foreach (var prey in scene.PreyActors) prey.SetPhysicsProcess(false);
            var water = fish.GetNode<WaterReaction>("WaterReaction");
            if (stage is not null)
            {
                if (!new[] { "larva", "juvenile", "subadult", "adult" }.Contains(stage))
                    throw new Exception("invalid wake stage");
                var growth = new PondGrowthState("wake-render", stage);
                if (!PlayerArt.TryReplacePondStage(fish, growth.Entry(growth.Asset), out var reason))
                    throw new Exception("stage visual " + stage + " " + reason);
                fish.SetEncounterBassBody(growth.Body);
                water.ConfigureFromPlayerVisual();
            }
            if (water.GetNodeOrNull<MeshInstance3D>("SurfaceRing0") is null)
                throw new Exception("debug A/B ring pool missing");
            water.SetWakeLegacyComparison(mode.StartsWith("rings", StringComparison.Ordinal));
            var rig = fish.GetNode<FollowCamera>("CameraRig");
            if (followScenario && stage is not null)
                rig.ConfigureEncounterLarvaView(stage == "larva");
            if (!followScenario)
            {
                rig.SetProcess(false);
                rig.SetPhysicsProcess(false);
                scene.GetViewport().SizeChanged -= rig.SnapToFish;
            }
            var camera = fish.GetNode<Camera3D>("CameraRig/Camera3D");
            camera.Current = true;
            camera.Fov = 50f;
            var surface = scene.GetNode<UnderwaterAtmosphere>("UnderwaterAtmosphere");
            // Freeze automatic ticks before the first await. Otherwise an
            // expensive scene load can start A/B at different wave phases.
            fish.SetPhysicsProcess(false);
            water.SetPhysicsProcess(false);
            surface.SetPhysicsProcess(false);
            scene.GetNode<EnvironmentClock>("EnvironmentClock").SetProcess(false);
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
                startDepth is { } requestedDepth
                    ? surface.GetSurfaceHeight(0f, routeZ) - requestedDepth
                    : fish.PondSurfaceLimitAt(0f, routeZ) - (breachScenario ? .075f : .005f), routeZ);
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
            if (followScenario) rig.SnapToFish();
            water.Suspend();
            // Capture PNGs can skip arbitrary engine physics frames. Advance the
            // three dependent owners once per scripted tick so A/B input, motor,
            // historical path and wave phase remain identical.
            var frames = new List<double>();
            var draws = new List<double>();
            var primitives = new List<double>();
            var captures = breachScenario
                ? new (int Tick, string Name, string View)[]
                {
                    (10, "breach-start", "oblique"), (25, "breach-rise", "waterline"),
                    (40, "breach-contact", "oblique"), (55, "breach-later", "under-diagonal"),
                }
                : lightScenario
                ? new (int Tick, string Name, string View)[]
                {
                    (95, "boost-top", "top"), (95, "boost-waterline", "waterline"),
                    (95, "boost-under", "under-up"),
                }
                : shoreScenario
                ? new (int Tick, string Name, string View)[]
                {
                    (25, "shore-top", "top"),
                    (60, "shore-oblique", "oblique"),
                }
                : followScenario
                ? new (int Tick, string Name, string View)[]
                {
                    (45, "follow-cruise", "follow"),
                    (95, "follow-boost", "follow"),
                }
                : stage is not null
                ? new (int Tick, string Name, string View)[]
                {
                    (95, "stage-top", "top"),
                    (95, "stage-oblique", "oblique"),
                    (95, "stage-under", "under-diagonal"),
                }
                : new (int Tick, string Name, string View)[]
                {
                    (25, "straight-top", "top"),
                    (64, "curve-oblique-a", "oblique"),
                    (65, "curve-oblique-b", "oblique"),
                    (75, "curve-waterline", "waterline"),
                    (95, "boost-waterline-a", "waterline"),
                    (95, "boost-top", "top"),
                    (95, "boost-oblique", "oblique"),
                    (95, "boost-waterline-intersect", "intersect"),
                    (95, "boost-under-up", "under-up"),
                    (95, "boost-under-diagonal", "under-diagonal"),
                    (96, "boost-waterline-b", "waterline"),
                    (125, "glide-top", "top"),
                    (145, "coast-oblique", "oblique"),
                    (165, "dive-under", "under"),
                    (195, "deep-under", "under"),
                    (360, "rest-top", "top"),
                };
            var captureIndex = 0;
            KeyInput(fish, Key.W, true);
            if (breachScenario) KeyInput(fish, Key.Up, true);
            var lastFrame = Time.GetTicksUsec();
            for (var tick = 0; tick <= (breachScenario ? 65 : lightScenario ? 100 : shoreScenario ? 70 : stage is not null ? 100 : 365); tick++)
            {
                if (!shoreScenario && !breachScenario)
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
                while (captureIndex < captures.Length && captures[captureIndex].Tick == tick)
                {
                var shot = captures[captureIndex++];
                if (!followScenario) PlaceCamera(camera, fish, surface, shot.View);
                await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
                var file = Path.Combine(folder, shot.Name + ".png");
                if (scene.GetViewport().GetTexture().GetImage().SavePng(file) != Error.Ok)
                    throw new Exception("capture failed " + file);
                var position = fish.GlobalPosition;
                if (!shoreScenario && tick == 25 && position.Z > routeZ - .05f)
                    throw new Exception("central replay did not translate through water");
                if (!shoreScenario && !breachScenario && tick == 95 && !fish.BoostActive)
                    throw new Exception("central replay did not engage boost");
                var body = PlayerBodyGeometry.Measure(fish);
                var contact = PondWakeContacts.At(body, fish.GlobalTransform,
                    surface.GetSurfaceHeight);
                if (followScenario && water.WakePath.Count > 1)
                {
                    var first = water.WakePath[0];
                    var last = water.WakePath[water.WakePath.Count - 1];
                    GD.Print($"WAKE_FOLLOW_GEOMETRY fin_left={contact.FinLeft} fin_screen={camera.UnprojectPosition(contact.FinLeft)} " +
                        $"contact_screen={camera.UnprojectPosition(contact.Left)} " +
                        $"first_center={first.Center} first_depth={first.LeftY:F4} " +
                        $"last_center={last.Center} last_depth={last.LeftY:F4} " +
                        $"fish_screen={camera.UnprojectPosition(position)}");
                }
                GD.Print($"WAKE_RENDER_CAPTURE mode={mode} stage={stage ?? "fixture"} tick={tick} name={shot.Name} view={shot.View} " +
                    $"pos={position} speed={fish.Velocity.Length():F3} boost={fish.BoostActive} " +
                    $"samples={water.WakePath.Count} triangles={water.ActiveWakeTriangles} " +
                    $"rings={water.ActivePondRings} surface={surface.GetSurfaceHeight(position.X, position.Z):F4} " +
                    $"clock={surface.SurfaceTime:F4} solar_hour={surface.Solar.HourOfDay:F2} " +
                    $"camera={camera.GlobalPosition} camera_basis={camera.GlobalBasis} contact={contact.Touching} " +
                    $"depth={contact.SurfaceY - position.Y:F4} top_minus_surface={position.Y + body.TopReach - contact.SurfaceY:F4} file={file}");
                if (breachScenario)
                    GD.Print($"WAKE_BREACH_OBSERVATION tick={tick} vertical_speed={fish.Velocity.Y:F4} " +
                        $"splash_emitting={water.GetNode<GpuParticles3D>("SurfaceSplash").Emitting} " +
                        $"splash_y={water.GetNode<GpuParticles3D>("SurfaceSplash").GlobalPosition.Y:F4}");
                // PNG encoding and disk I/O are outside the frame-cost sample.
                lastFrame = Time.GetTicksUsec();
                }
            }
            KeyInput(fish, Key.W, false);
            KeyInput(fish, Key.Space, false);
            KeyInput(fish, Key.D, false);
            KeyInput(fish, Key.Down, false);
            KeyInput(fish, Key.Up, false);
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
            case "intersect":
                camera.GlobalPosition = look + new Vector3(1.35f, 0f, .18f);
                camera.LookAt(look + Vector3.Down * .02f, Vector3.Up);
                break;
            case "under-up":
                camera.GlobalPosition = look + new Vector3(.35f, -.35f, .04f);
                camera.LookAt(look + Vector3.Up * .06f, Vector3.Up);
                break;
            case "under-diagonal":
                camera.GlobalPosition = look + new Vector3(1.25f, -.24f, .25f);
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
