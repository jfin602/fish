#nullable enable
using Godot;
using System;
using System.Linq;

namespace Fish;

/// <summary>Contact without input and real motor boost in the C5 fixture.</summary>
public static class AutoFeedBoostChecks
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
            foreach (var actor in scene.PreyActors) actor.SetPhysicsProcess(false);
            var open = new Vector3(0, PikeCoverFixture.Bed(0, 20) + 1.2f, 20);
            fish.GlobalPosition = open;
            fish.GlobalBasis = Basis.Identity;
            var nymph = scene.FoodActors.First(f => f.Kind == FeedingLedger.InsectId);
            nymph.Relocate(open + Vector3.Forward * (fish.BodyForwardReach + .04f));
            Need(scene.Feeding.Events.Count == 0, "food committed before contact tick");
            scene.SetPhysicsProcess(true);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(scene.Feeding.Events.Count == 1 && !nymph.Available &&
                scene.Feeding.Nutrition == 2 && scene.PredationEvents.Count == 0,
                "no-button nymph contact failed");
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(scene.Feeding.Events.Count == 1, "repeated contact duplicated nutrition");
            scene.SetPhysicsProcess(false);
            var other = scene.FoodActors.First(f => f.Kind == FeedingLedger.InsectId && f.Available);
            other.Relocate(open + Vector3.Back * .4f);
            Need(!scene.TryBiteFood(other).Consumed, "rear food consumed");
            other.Relocate(open + Vector3.Right * (fish.BodyForwardReach + .02f));
            Need(!scene.TryBiteFood(other).Consumed, "side food consumed");
            other.Relocate(open + Vector3.Forward * 2f);
            Need(!scene.TryBiteFood(other).Consumed, "remote food consumed");
            foreach (var meal in scene.FoodActors.Where(f => f.Kind == FeedingLedger.InsectId &&
                         f.Available).Take(6))
            {
                meal.Relocate(open + Vector3.Forward * (fish.BodyForwardReach + .02f));
                Need(scene.TryBiteFood(meal).Consumed, "growth setup meal failed");
            }
            Need(scene.Growth.Stage == "subadult", "subadult growth setup failed");
            var prey = scene.PreyActors.First(p => p.Stage == "larva" && p.IsAlive);
            var preyRadius = ((SphereShape3D)prey.GetNode<CollisionShape3D>("Collision").Shape).Radius;
            prey.GlobalPosition = open + Vector3.Forward * (fish.BodyForwardReach + preyRadius * .5f);
            var beforeEvents = scene.Feeding.Events.Count;
            scene.SetPhysicsProcess(true);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            scene.SetPhysicsProcess(false);
            Need(!prey.IsAlive && scene.Feeding.Events.Count == beforeEvents + 1 &&
                scene.PredationEvents.Count == 1 && scene.Blood.SeenEventIds.Count == 1 &&
                fish.GetNode<Node3D>("PlayerVisual").GetNode<CatalogFishVisualMotion>("SwimPresentation").FeedingCueActive &&
                fish.GetNode<AudioStreamPlayer3D>("FishFeedSound").Playing,
                "no-button Bluegill contact did not commit one meal, injury and sound");

            fish.SetActivePreset(ControlPreset.Builtins().Single(p => p.Id == ControlPreset.KeyboardId));
            fish.SetPhysicsProcess(true);
            await Frames(2);
            fish._Input(new InputEventKey { PhysicalKeycode = Key.W, Pressed = true });
            await Frames(45);
            var ordinary = fish.Velocity.Length();
            Need(ordinary > .25f && ordinary <= fish.Tuning.MaxSpeed + .01f,
                "ordinary propulsion lost");
            fish._Input(new InputEventKey { PhysicalKeycode = Key.Space, Pressed = true });
            await Frames(50);
            var boosted = fish.Velocity.Length();
            Need(boosted > ordinary * 1.25f && fish.BoostEnergy < 1f && fish.BoostActive &&
                MovementDisturbanceField.Energy(boosted, 0f, 1f, 0f) >
                MovementDisturbanceField.Energy(ordinary, 0f, 1f, 0f),
                "Space did not produce real faster, louder movement");
            await Frames(160);
            Need(fish.BoostEnergy < 1f && !fish.BoostActive &&
                fish.Velocity.Length() <= fish.Tuning.MaxSpeed + .02f,
                "boost did not exhaust and return to normal speed");
            fish._Input(new InputEventKey { PhysicalKeycode = Key.Space, Pressed = false });
            fish._Input(new InputEventKey { PhysicalKeycode = Key.W, Pressed = false });
            await Frames(170);
            Need(fish.BoostEnergy > .4f && !fish.BoostActive,
                "boost failed to recover after release");
            fish._Input(new InputEventKey { PhysicalKeycode = Key.W, Pressed = true });
            fish._Input(new InputEventKey { PhysicalKeycode = Key.Space, Pressed = true });
            await Frames(3);
            Need(fish.BoostActive, "fresh press did not re-arm boost");
            scene._Input(new InputEventKey { Keycode = Key.Escape, Pressed = true });
            Need(tree.Paused && !fish.BoostActive, "pause retained active boost");
            var pausedEnergy = fish.BoostEnergy;
            for (var i = 0; i < 3; i++) await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            Need(Mathf.IsEqualApprox(fish.BoostEnergy, pausedEnergy),
                "paused game drained boost energy");
            (scene.FindChild("ResumeEncounter", true, false) as Button)!
                .EmitSignal(BaseButton.SignalName.Pressed);
            fish._Input(new InputEventKey { PhysicalKeycode = Key.W, Pressed = false });
            fish._Input(new InputEventKey { PhysicalKeycode = Key.Space, Pressed = false });
            await Frames(3);
            Need(!tree.Paused && !fish.BoostActive, "resume restarted stale boost");
            fish.ClearAndDisarmInput();
            fish._Input(new InputEventKey { PhysicalKeycode = Key.Space, Pressed = true });
            await Frames(2);
            Need(!fish.BoostActive, "disarmed Space restarted boost");
            fish._Input(new InputEventKey { PhysicalKeycode = Key.Space, Pressed = false });
            fish._Notification((int)Node.NotificationApplicationFocusOut);
            Need(!fish.BoostActive && fish.GetHeldInputCount() == 0,
                "focus loss retained boost input");
            foreach (var profile in ControlPreset.Builtins())
            {
                fish.SetActivePreset(profile);
                fish.GlobalPosition = open;
                await Frames(3);
                var keyboardThrust = profile.Bindings(ControlAction.Thrust).Any(binding =>
                    binding.Device == BindingDevice.PhysicalKey && binding.Code == (int)Key.W);
                if (keyboardThrust)
                    fish._Input(new InputEventKey { PhysicalKeycode = Key.W, Pressed = true });
                else
                    fish._Input(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true });
                if (profile.CameraControl == CameraControlPolicy.AdaptiveFollow)
                    fish._Input(new InputEventKey { PhysicalKeycode = Key.Shift,
                        Location = KeyLocation.Left, Pressed = true });
                fish._Input(new InputEventKey { PhysicalKeycode = Key.Space, Pressed = true });
                await Frames(3);
                Need(fish.BoostActive &&
                    (profile.CameraControl != CameraControlPolicy.AdaptiveFollow || fish.AdaptiveFreeLookHeld),
                    "preset lost boost or Adaptive Follow free look: " + profile.Id);
                fish._Input(new InputEventKey { PhysicalKeycode = Key.Space, Pressed = false });
                if (keyboardThrust)
                    fish._Input(new InputEventKey { PhysicalKeycode = Key.W, Pressed = false });
                else
                    fish._Input(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = false });
                if (profile.CameraControl == CameraControlPolicy.AdaptiveFollow)
                    fish._Input(new InputEventKey { PhysicalKeycode = Key.Shift,
                        Location = KeyLocation.Left, Pressed = false });
            }
            GD.Print($"AUTO_FEED_BOOST_CHECK_OK food=auto-once ordinary={ordinary:F3} boosted={boosted:F3} energy={fish.BoostEnergy:F2}");
            tree.Quit();

            async System.Threading.Tasks.Task Frames(int count)
            {
                for (var i = 0; i < count; i++)
                    await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            }
        }
        catch (Exception error)
        {
            GD.PushError("AUTO_FEED_BOOST_CHECK_FAILED " + error);
            tree.Quit(1);
        }
    }

    private static void Need(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
