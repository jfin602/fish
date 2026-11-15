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
            fish.ResetPhysicsInterpolation();
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
                    var sampleFood = mouth + fish.MouthForward * .3f + Vector3.Up * .03f;
                    Need(FeedingReticle.TryProjectFoodPath(camera, mouth, fish.MouthForward,
                            sampleFood, viewport, out _, out var pathScreen) &&
                        pathScreen.DistanceTo(camera.UnprojectPosition(mouth + fish.MouthForward * .3f)) < .01f,
                        "target-depth mouth path failed: " + stage + " zoom=" + zoom);
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
            fish.SetEncounterBassBody(scene.Growth.BodyFor("larva"));
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
            void RefreshGuide()
            {
                fish.SetPhysicsProcess(true);
                hud._Process(0);
                fish.SetPhysicsProcess(false);
            }
            Need(scene.Growth.Stage == "larva", "reticle check did not use larval diet");
            var tiny = scene.FoodActors.First(f => f.Kind == FeedingLedger.MicrofaunaId);
            var mouthAtMiss = fish.MouthPosition;
            var headingDistance = Mathf.Max(.8f, fish.BodyForwardReach * 4f);
            Need(FeedingReticle.TryProjectHeading(camera, mouthAtMiss, fish.MouthForward,
                headingDistance, viewport, out var headingAtMiss), "larval heading lost");
            var rayOrigin = camera.ProjectRayOrigin(headingAtMiss);
            var rayDirection = camera.ProjectRayNormal(headingAtMiss);
            var missDepth = mouthAtMiss.Z - .3f;
            var rayLength = (missDepth - rayOrigin.Z) / rayDirection.Z;
            tiny.Relocate(rayOrigin + rayDirection * rayLength);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            var missVector = tiny.GlobalPosition - mouthAtMiss;
            var yolkResult = scene.TryBiteFood(tiny);
            RefreshGuide();
            Need(hud.GuidedFoodId is null && yolkResult.Reason == "food is not available at this stage",
                "yolk-period food received an eligible guide");
            scene.MovementCues!.Step(fish.GlobalPosition, Vector3.Zero, 0f, 0f,
                FeedingLedger.YolkSeconds + .1, fish.MotorStepCount + 1, false);
            var missResult = scene.TryBiteFood(tiny);
            RefreshGuide();
            Need(FeedingReticle.TryProjectFoodPath(camera, mouthAtMiss, fish.MouthForward,
                    tiny.GlobalPosition, viewport, out var foodAtMiss, out var pathAtMiss) &&
                foodAtMiss.DistanceTo(headingAtMiss) < .1f &&
                hud.GuidedFoodId == tiny.StableId &&
                hud.GuidedFoodPosition.DistanceTo(foodAtMiss) < 2f &&
                pathAtMiss.DistanceTo(foodAtMiss) > 5f,
                "aligned off-path larval food did not show a depth-aware gap");
            var clearExclude = new Godot.Collections.Array<Rid> { fish.GetRid() };
            bool obstruction;
            using (var clearQuery = PhysicsRayQueryParameters3D.Create(mouthAtMiss,
                tiny.GlobalPosition, 1, clearExclude))
                obstruction = scene.GetWorld3D().DirectSpaceState.IntersectRay(clearQuery).Count != 0;
            GD.Print($"P6_LARVAL_AIM_DIAG seed={PikeCoverFixture.Seed} frame={Engine.GetPhysicsFrames()} actor={scene.Growth.Identity} target={tiny.StableId} kind={tiny.Kind} time={scene.MovementCues.SimulationSeconds:F3} stage={scene.Growth.Stage} diet={FeedingLedger.FoodAward(tiny.Kind, scene.Growth.Stage, scene.MovementCues.SimulationSeconds)} preset={fish.ActivePreset.Id} viewport={viewport} camera={camera.GlobalTransform} mouth={mouthAtMiss} forward={fish.MouthForward} target_pos={tiny.GlobalPosition} distance={missVector.Length():F4} dot={fish.MouthForward.Dot(missVector.Normalized()):F4} reach={scene.Growth.MouthRadius * 1.5f:F4} screen_gap={pathAtMiss.DistanceTo(foodAtMiss):F1} obstruction={obstruction} yolk_result={yolkResult.Reason} result={missResult.Reason}");
            Need(!missResult.Consumed, "aligned off-path larval target unexpectedly fed");
            Need(!obstruction && missResult.Reason == "out of reach or blocked",
                "off-path miss was not isolated from obstruction and diet");
            camera.GlobalPosition = open + Vector3.Right * .4f + Vector3.Back * 1.2f + Vector3.Up * .15f;
            camera.LookAt(open + Vector3.Right * .15f + Vector3.Forward * .5f, Vector3.Up);
            RefreshGuide();
            Need(hud.GuidedFoodId == tiny.StableId &&
                hud.GuidedFoodPosition.DistanceTo(camera.UnprojectPosition(tiny.GlobalPosition)) < 2f,
                "orbit or follow offset detached guide from visible food");
            camera.GlobalPosition = open + Vector3.Back * 2f + Vector3.Up * .2f;
            camera.LookAt(open + Vector3.Back * 4f, Vector3.Up);
            RefreshGuide();
            Need(hud.GuidedFoodId is null, "behind-camera food received a guide");
            camera.LookAt(open + Vector3.Right * 8f, Vector3.Up);
            RefreshGuide();
            Need(hud.GuidedFoodId is null, "offscreen food received a guide");
            camera.LookAt(open + Vector3.Forward * .5f, Vector3.Up);
            tiny.Relocate(mouthAtMiss + Vector3.Right * .2f);
            RefreshGuide();
            Need(hud.GuidedFoodId is null, "side food received a positive guide");
            tiny.Relocate(mouthAtMiss + Vector3.Back * .2f);
            RefreshGuide();
            Need(hud.GuidedFoodId is null, "behind food received a positive guide");
            tiny.Relocate(open + Vector3.Down * 1.6f + Vector3.Forward * .3f);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            using (var blockedQuery = PhysicsRayQueryParameters3D.Create(mouthAtMiss,
                tiny.GlobalPosition, 1, clearExclude))
                Need(scene.GetWorld3D().DirectSpaceState.IntersectRay(blockedQuery).Count != 0,
                    "fixture bed did not obstruct the blocked-food check");
            RefreshGuide();
            Need(hud.GuidedFoodId is null && !scene.TryBiteFood(tiny).Consumed,
                "blocked food received a guide or feed");
            tiny.Relocate(mouthAtMiss + Vector3.Forward * .3f);
            var unavailable = scene.FoodActors.First(f => f.Kind == FeedingLedger.MicrofaunaId && f != tiny);
            unavailable.Relocate(mouthAtMiss + Vector3.Forward * .3f);
            unavailable.Consume();
            RefreshGuide();
            Need(hud.GuidedFoodId == tiny.StableId, "unavailable food replaced the guide");
            tiny.Relocate(mouthAtMiss + Vector3.Forward * .012f,
                scene.MovementCues!.SimulationSeconds);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            var before = scene.Feeding.Events.Count;
            var beforeNutrition = scene.Feeding.Nutrition;
            // Headless audio playback can outlive SceneTree.Quit; this check owns the
            // feed ledger and reticle pulse, not the separate sound path.
            fish.GetNode<AudioStreamPlayer3D>("SmallFeedSound").Stream = null;
            scene.SetPhysicsProcess(true);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            scene.SetPhysicsProcess(false);
            Need(!tiny.Available && scene.Feeding.Events.Count == before + 1 &&
                scene.Feeding.Nutrition == beforeNutrition + 1 && hud.ConfirmedFeeds == 1,
                "valid automatic larval tiny-food feed did not reward and confirm once");
            RefreshGuide();
            Need(hud.GuidedFoodId is null, "consumed food retained a guide");
            Need(!scene.TryBiteFood(tiny).Consumed &&
                scene.Feeding.Events.Count == before + 1 &&
                scene.Feeding.Nutrition == beforeNutrition + 1 && hud.ConfirmedFeeds == 1,
                "duplicate feed retriggered reticle");
            fish.ClearAndDisarmInput();
            hud._Process(0);
            Need(!hud.Visible && !hud.HeadingVisible, "disarm retained reticle");
            scene._Input(new InputEventKey { Keycode = Key.Escape, Pressed = true });
            hud._Process(0);
            Need(tree.Paused && !hud.Visible, "pause retained reticle");
            GD.Print($"P6_RETICLE_CHECK_OK stages={stages.Length} zooms=2 viewport={viewport} larval_feed_events={scene.Feeding.Events.Count} feed_time={scene.Feeding.Events.Last().GameSeconds:F3} confirmations={hud.ConfirmedFeeds} seed={PikeCoverFixture.Seed}");
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
