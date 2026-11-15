#nullable enable
using Godot;
using System;
using System.Linq;

namespace Fish;

/// <summary>Deterministic P6 projection, lifecycle and ledger-driven feedback checks.</summary>
public static class P6ReticleChecks
{
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
            var rig = fish.GetNode<FollowCamera>("CameraRig");
            rig.SetPhysicsProcess(false);
            rig.SetProcess(false);
            var camera = rig.GetNode<Camera3D>("Camera3D");
            var hud = scene.GetNode<CanvasLayer>("FeedingReticleLayer")
                .GetNode<FeedingReticle>("FeedingReticle");
            Need(hud.MouseFilter == Control.MouseFilterEnum.Ignore &&
                hud.GetParent<CanvasLayer>().Layer <
                scene.GetNode<CanvasLayer>("FixtureHud").Layer,
                "reticle layer or input transparency incorrect");
            var viewport = scene.GetViewport().GetVisibleRect().Size;
            Need(viewport.X > 0 && viewport.Y > 0, "invalid viewport");
            var open = new Vector3(0, PikeCoverFixture.Bed(0, 20) + 1.2f, 20);
            fish.GlobalPosition = open;
            fish.GlobalBasis = Basis.Identity;
            var stages = new[] { "larva", "juvenile", "subadult", "adult" };
            foreach (var stage in stages)
            {
                fish.SetEncounterBassBody(scene.Growth.BodyFor(stage));
                var mouth = fish.MouthPosition;
                Need(mouth.DistanceTo(open + Vector3.Forward * fish.BodyForwardReach) < .0001f,
                    "mouth no longer uses the fitted body: " + stage);
                foreach (var zoom in new[] { .6f, 2.5f })
                {
                    camera.GlobalPosition = open + Vector3.Back * zoom + Vector3.Up * .2f;
                    camera.LookAt(open + Vector3.Forward * .5f, Vector3.Up);
                    var distance = Mathf.Max(.8f, fish.BodyForwardReach * 4f);
                    Need(FeedingReticle.TryProjectHeading(camera, mouth, fish.MouthForward,
                            distance, viewport, out var projected) &&
                        projected.DistanceTo(camera.UnprojectPosition(mouth + fish.MouthForward * distance)) < .01f,
                        "mouth projection failed: " + stage + " zoom=" + zoom);
                    var farDistance = Mathf.Max(5f, fish.BodyForwardReach * 15f);
                    Need(FeedingReticle.TryProjectHeading(camera, mouth, fish.MouthForward,
                            farDistance, viewport, out _),
                        "far heading projection failed: " + stage);
                    camera.LookAt(open + Vector3.Right * .25f, Vector3.Up);
                    Need(FeedingReticle.TryProjectHeading(camera, mouth, fish.MouthForward,
                            distance, viewport, out var divergent) &&
                        divergent.DistanceTo(viewport * .5f) > 12f,
                        "view and mouth divergence hidden: " + stage);
                }
            }
            fish.SetEncounterBassBody(scene.Growth.BodyFor("juvenile"));
            camera.GlobalPosition = open + Vector3.Back * 2f;
            camera.LookAt(open + Vector3.Back * 4f, Vector3.Up);
            Need(!FeedingReticle.TryProjectHeading(camera, fish.MouthPosition,
                    fish.MouthForward, 1f, viewport, out _),
                "behind-camera heading displayed");
            camera.GlobalPosition = open + Vector3.Back * 2f;
            camera.LookAt(open + Vector3.Right * 8f, Vector3.Up);
            Need(!FeedingReticle.TryProjectHeading(camera, fish.MouthPosition,
                    fish.MouthForward, 1f, viewport, out _),
                "offscreen heading displayed");
            Need(!FeedingReticle.ShouldShow(false, true, true, true, false, true) &&
                !FeedingReticle.ShouldShow(true, false, true, true, false, true) &&
                !FeedingReticle.ShouldShow(true, true, true, false, false, true) &&
                !FeedingReticle.ShouldShow(true, true, true, true, true, true) &&
                !FeedingReticle.ShouldShow(true, true, true, true, false, false) &&
                FeedingReticle.ShouldShow(true, true, true, true, false, true),
                "reticle lifecycle policy failed");
            camera.GlobalPosition = open + Vector3.Back * 2f + Vector3.Up * .2f;
            camera.LookAt(open + Vector3.Forward * .5f, Vector3.Up);
            foreach (var preset in ControlPreset.Builtins())
            {
                fish.SetActivePreset(preset);
                Need(FeedingReticle.TryProjectHeading(camera, fish.MouthPosition,
                        fish.MouthForward, 1f, viewport, out _),
                    "projection depends on control preset " + preset.Id);
            }
            fish.SetPhysicsProcess(true);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            hud._Process(0);
            Need(fish.GetInputArmed() && hud.Visible && hud.Size == viewport,
                $"reticle did not fill the viewport when armed armed={fish.GetInputArmed()} visible={hud.Visible} size={hud.Size} viewport={viewport}");
            fish.SetPhysicsProcess(false);
            var nymph = scene.FoodActors.First(f => f.Kind == FeedingLedger.InsectId);
            nymph.Relocate(open + Vector3.Forward * (fish.BodyForwardReach + .04f));
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            var before = scene.Feeding.Events.Count;
            Need(scene.TryBiteFood(nymph).Consumed &&
                scene.Feeding.Events.Count == before + 1 && hud.ConfirmedFeeds == 1,
                "valid physical feed did not confirm once");
            Need(!scene.TryBiteFood(nymph).Consumed &&
                scene.Feeding.Events.Count == before + 1 && hud.ConfirmedFeeds == 1,
                "duplicate feed retriggered reticle");
            fish.ClearAndDisarmInput();
            hud._Process(0);
            Need(!hud.Visible && !hud.HeadingVisible, "disarm retained reticle");
            scene._Input(new InputEventKey { Keycode = Key.Escape, Pressed = true });
            hud._Process(0);
            Need(tree.Paused && !hud.Visible, "pause retained reticle");
            GD.Print($"P6_RETICLE_CHECK_OK stages={stages.Length} zooms=2 viewport={viewport} feed_events={scene.Feeding.Events.Count} confirmations={hud.ConfirmedFeeds} seed={PikeCoverFixture.Seed}");
            tree.Quit();
        }
        catch (Exception error)
        {
            GD.PushError("P6_RETICLE_CHECK_FAILED " + error);
            tree.Quit(1);
        }
    }

    private static void Need(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
