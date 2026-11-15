#nullable enable
using Godot;
using System;
using System.IO;
using System.Linq;

namespace Fish;

/// <summary>Forward+ W1 contact-sheet poses; paths are staged diagnostics, not gameplay footage.</summary>
public static class PondWakeNativeProbe
{
    public static async void Run(PikeEncounter scene)
    {
        var tree = scene.GetTree();
        try
        {
            var outputArg = OS.GetCmdlineUserArgs().FirstOrDefault(x =>
                x.StartsWith("--wake-output=", StringComparison.Ordinal));
            var folder = outputArg is null ? "artifacts/wake-path/native" :
                outputArg["--wake-output=".Length..];
            Directory.CreateDirectory(folder);
            scene.SetPhysicsProcess(false);
            scene.GetNode<CanvasLayer>("FixtureHud").Visible = false;
            var fish = scene.GetNode<FishController>("Fish");
            fish.SetPhysicsProcess(false);
            foreach (var prey in scene.PreyActors) prey.SetPhysicsProcess(false);
            var water = fish.GetNode<WaterReaction>("WaterReaction");
            if (water.GetNodeOrNull<MeshInstance3D>("WakePathDebug") is null)
                throw new Exception("Debug view was not enabled for native probe");
            water.SetPhysicsProcess(false);
            var rig = fish.GetNode<FollowCamera>("CameraRig");
            rig.SetProcess(false);
            rig.SetPhysicsProcess(false);
            scene.GetViewport().SizeChanged -= rig.SnapToFish;
            var camera = fish.GetNode<Camera3D>("CameraRig/Camera3D");
            camera.Current = true;
            var surface = scene.GetNode<UnderwaterAtmosphere>("UnderwaterAtmosphere");
            var count = 0;
            foreach (var stage in new[] { "larva", "adult" })
            {
                var growth = new PondGrowthState("wake-native", stage);
                if (!PlayerArt.TryReplacePondStage(fish, growth.Entry(growth.Asset), out var why))
                    throw new Exception("stage visual " + why);
                fish.SetEncounterBassBody(growth.Body);
                water.ConfigureFromPlayerVisual();
                var body = PlayerBodyGeometry.Measure(fish);
                foreach (var route in new[] { "straight", "curve", "deep" })
                {
                    water.WakePath.Clear();
                    var deep = route == "deep";
                    var routeLength = stage == "larva" ? .32f : 1.25f;
                    var finalX = 0f;
                    var finalZ = 0f;
                    for (var i = 0; i <= 72 && !deep; i++)
                    {
                        var t = i / 72f;
                        var x = -routeLength + routeLength * t;
                        var z = route == "curve" ? routeLength * .45f * t * t : 0f;
                        var yaw = route == "curve" ? -.65f * t : 0f;
                        var y = surface.GetSurfaceHeight(x, z) - body.TopReach * .35f;
                        var pose = new Transform3D(new Basis(Vector3.Up, yaw), new Vector3(x, y, z));
                        var contact = PondWakeContacts.At(body, pose, surface.GetSurfaceHeight);
                        water.WakePath.Step(contact, body.Length, .7f, stage == "larva" ? 1 : 2,
                            1f / 60f);
                        finalX = x;
                        finalZ = z;
                    }
                    var finalY = surface.GetSurfaceHeight(finalX, finalZ) -
                        (deep ? body.Radius * 5f : body.TopReach * .35f);
                    fish.GlobalTransform = new Transform3D(
                        new Basis(Vector3.Up, route == "curve" ? -.65f : 0f),
                        new Vector3(finalX, finalY, finalZ));
                    water.RefreshWakeDebug();
                    var distance = stage == "larva" ? .38f : 1.8f;
                    camera.GlobalPosition = fish.GlobalPosition +
                        (deep ? new Vector3(distance * .3f, distance * .25f, distance)
                            : new Vector3(0f, distance, distance * .35f));
                    camera.LookAt(fish.GlobalPosition, Vector3.Up);
                    for (var frame = 0; frame < (count == 0 ? 20 : 4); frame++)
                        await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
                    var path = Path.Combine(folder, stage + "-" + route + ".png");
                    if (scene.GetViewport().GetTexture().GetImage().SavePng(path) != Error.Ok)
                        throw new Exception("screenshot failed " + path);
                    GD.Print($"WAKE_NATIVE_CAPTURE stage={stage} route={route} samples={water.WakePath.Count} surface_y={surface.GetSurfaceHeight(finalX, finalZ):F4} body_y={finalY:F4} renderer={RenderingServer.GetCurrentRenderingMethod()} file={path}");
                    count++;
                }
            }
            if (count != 6) throw new Exception("capture count");
            GD.Print("WAKE_NATIVE_OK captures=6 diagnostic_staged_path=yes");
            tree.Quit();
        }
        catch (Exception error)
        {
            GD.PushError("WAKE_NATIVE_FAILED " + error);
            tree.Quit(1);
        }
    }
}
