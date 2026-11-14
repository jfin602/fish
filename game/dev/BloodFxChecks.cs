#nullable enable
using Godot;
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace Fish;

/// <summary>Headless M6 event, render-state, lifetime and effect-budget probe.</summary>
public static class BloodFxChecks
{
    public static async void RunNative(PikeEncounter scene)
    {
        var tree = scene.GetTree();
        try
        {
            scene.SetPhysicsProcess(false);
            var fish = scene.GetNode<FishController>("Fish");
            fish.SetPhysicsProcess(false);
            foreach (var prey in scene.PreyActors) prey.SetPhysicsProcess(false);
            var contact = new Vector3(0f, Mathf.Min(PikeCoverFixture.WaterY - 1.5f,
                PikeCoverFixture.Bed(0, 20) + 2f), 20f);
            var rig = fish.GetNode<FollowCamera>("CameraRig");
            rig.SetProcess(false);
            rig.SetPhysicsProcess(false);
            var camera = fish.GetNode<Camera3D>("CameraRig/Camera3D");
            camera.GlobalPosition = contact + new Vector3(0f, .1f, 1.25f);
            camera.LookAt(contact, Vector3.Up);
            camera.Current = true;
            var baseline = 0d;
            for (var frame = 0; frame < 20; frame++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
                if (frame >= 10) baseline += Performance.GetMonitor(Performance.Monitor.RenderTotalDrawCallsInFrame);
            }
            baseline /= 10;
            var fx = scene.Blood;
            for (var i = 0; i < FishBloodFx.Capacity; i++)
                Need(fx.TryEmit("native:probe:" + i,
                    contact + new Vector3((i - 1.5f) * .16f, 0f, 0f)), "native emitter refused probe");
            var active = 0d;
            for (var frame = 0; frame < 20; frame++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
                if (frame >= 10) active += Performance.GetMonitor(Performance.Monitor.RenderTotalDrawCallsInFrame);
            }
            active /= 10;
            var output = OS.GetCmdlineUserArgs().FirstOrDefault(arg => arg.StartsWith("--blood-output=", StringComparison.Ordinal));
            if (output is not null)
            {
                var path = output["--blood-output=".Length..];
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                Need(scene.GetViewport().GetTexture().GetImage().SavePng(path) == Error.Ok,
                    "native screenshot save failed");
            }
            GD.Print($"BLOOD_FX_NATIVE_OK renderer={RenderingServer.GetCurrentRenderingMethod()} pool={fx.ActiveCount} particles={fx.ParticleBudget} draw_baseline={baseline:F1} draw_active={active:F1} draw_delta={active - baseline:F1} screenshot={(output is null ? "none" : "saved")}");
            tree.Quit();
        }
        catch (Exception error)
        {
            GD.PushError("BLOOD_FX_NATIVE_FAILED " + error);
            tree.Quit(1);
        }
    }

    public static async void Run(PikeEncounter scene)
    {
        var tree = scene.GetTree();
        try
        {
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            scene.SetPhysicsProcess(false);
            var fish = scene.GetNode<FishController>("Fish");
            fish.SetPhysicsProcess(false);
            foreach (var prey in scene.PreyActors) prey.SetPhysicsProcess(false);
            var fx = scene.Blood;
            fx.SetProcess(false);
            var off = OS.GetCmdlineUserArgs().Contains("--effects=off");
            var low = OS.GetCmdlineUserArgs().Contains("--graphics=low");
            Need(fx.Enabled == !off && fx.LowGraphics == low &&
                fx.ParticleBudget == (low ? 24 : 48) && fx.GetChildCount() == FishBloodFx.Capacity &&
                fx.ActiveCount == 0 && fx.EmissionCount == 0 &&
                scene.PredationEvents.Count == 0, "initial blood budget/state invalid");
            Need(!scene.TryBite(scene.PreyActors[0]).Consumed &&
                scene.PredationEvents.Count == 0 && fx.EmissionCount == 0,
                "invalid bite emitted blood");

            var isolated = new FishBloodFx { Name = "BloodFxProbe" };
            scene.AddChild(isolated);
            isolated.SetProcess(false);
            var contact = new Vector3(0f, 3f, 0f);
            isolated.Configure(!off, low);
            Need(isolated.TryEmit("test:one", contact) == !off &&
                !isolated.TryEmit("test:one", contact) &&
                isolated.SeenEventIds.Count == 1 &&
                isolated.EmissionCount == (off ? 0 : 1), "duplicate/off event state invalid");
            if (!off)
            {
                var cloud = isolated.GetNode<GpuParticles3D>("BloodCloud0");
                Need(cloud.Visible && cloud.Emitting && cloud.GlobalPosition == contact &&
                    cloud.Amount == (low ? 6 : FishBloodFx.ParticlesPerCloud) &&
                    cloud.OneShot && !cloud.LocalCoords &&
                    cloud.CastShadow == GeometryInstance3D.ShadowCastingSetting.Off,
                    "cloud position or GPU render settings invalid");
                var material = (StandardMaterial3D)((QuadMesh)cloud.DrawPass1).Material;
                Need(material.AlbedoTexture is not null, "soft cloud texture missing");
                var colorAtBite = material.AlbedoColor;
                isolated._Process(isolated.DurationSeconds * .5f);
                var colorAtHalf = material.AlbedoColor;
                Need(colorAtHalf.A < colorAtBite.A && colorAtHalf.G > colorAtBite.G &&
                    isolated.ActiveCount == 1, "blood did not soften/dilute");
                isolated.SetPaused(true);
                isolated._Process(isolated.DurationSeconds);
                Need(isolated.ActiveCount == 1 && cloud.SpeedScale == 0f,
                    "pause aged blood or left GPU running");
                isolated.SetPaused(false);
                var priorTimeScale = Engine.TimeScale;
                try
                {
                    Engine.TimeScale = .5f;
                    isolated._Process(.01);
                    Need(Mathf.IsEqualApprox(cloud.SpeedScale, .5f),
                        "GPU particles ignored game time scale");
                }
                finally { Engine.TimeScale = priorTimeScale; }
                isolated._Process(isolated.DurationSeconds * .5f);
                Need(isolated.ActiveCount == 0 && !cloud.Visible && !cloud.Emitting,
                    "blood did not expire cleanly");
                for (var i = 0; i < FishBloodFx.Capacity + 2; i++)
                    isolated.TryEmit("test:pool:" + i, contact);
                Need(isolated.ActiveCount == FishBloodFx.Capacity &&
                    isolated.EmissionCount == FishBloodFx.Capacity + 1 &&
                    isolated.SeenEventIds.Count == FishBloodFx.Capacity + 3,
                    "particle pool exceeded bounded capacity or replayed overflow");
                isolated.Clear();
                Need(isolated.ActiveCount == 0 &&
                    isolated.GetChildren().OfType<GpuParticles3D>().All(p => !p.Visible && !p.Emitting),
                    "clear left GPU emitters active");
            }
            isolated.QueueFree();

            // Pike and Bass paths are exercised by PikeAiChecks and GrowthChecks;
            // this probe also verifies that settings cannot alter feeding state.
            Need(scene.Feeding.Nutrition == 0 && scene.Feeding.Events.Count == 0 &&
                scene.PredationEvents.Count == 0, "visual probe mutated feeding");
            GD.Print($"BLOOD_FX_CHECK_OK enabled={!off} low={low} pool={FishBloodFx.Capacity} particles={fx.ParticleBudget} duration={fx.DurationSeconds:F1} active={fx.ActiveCount}");
            tree.Quit();
        }
        catch (Exception error)
        {
            GD.PushError("BLOOD_FX_CHECK_FAILED " + error);
            tree.Quit(1);
        }
    }

    private static void Need(bool condition, string reason)
    {
        if (!condition) throw new Exception(reason);
    }
}
