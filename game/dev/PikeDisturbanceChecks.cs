#nullable enable
using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fish;

/// <summary>Generated Pond motor, cue clock and real-cover assertions; no native player judgment.</summary>
public static class PikeDisturbanceChecks
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
            var plants = PikeCoverFixture.VisibleSoftCover(scene.GetNode<Node3D>("Cover"));
            var space = scene.GetWorld3D().DirectSpaceState;
            var field = new MovementDisturbanceField(space, plants, pike.GetRid(), fish.GetRid());
            var still = MovementDisturbanceField.Energy(0, 0, 0, 0);
            var glide = MovementDisturbanceField.Energy(3, 0, 0, 0);
            var burst = MovementDisturbanceField.Energy(3, 5, 1, 0);
            var turn = MovementDisturbanceField.Energy(3, 0, 1, 1);
            Need(still == 0 && glide > 0 && burst > glide && turn > glide,
                "still/glide/burst/turn ordering");
            var tuning = fish.Tuning;
            var state = new SwimState();
            var coast = new SwimState();
            for (var i = 0; i < 30; i++) state = SwimMotor.Step(state,
                new SwimIntent(1, 0, 0, 0), 1f / 60f, tuning);
            coast = state;
            for (var i = 0; i < 12; i++) coast = SwimMotor.Step(coast, default, 1f / 60f, tuning);
            Need(MovementDisturbanceField.Energy(state.Speed, tuning.Acceleration, 1, 0) >
                MovementDisturbanceField.Energy(coast.Speed, 0, 0, 0),
                "actual motor burst did not exceed coast");

            var source = new Vector3(0, PikeCoverFixture.Bed(0, 5) + 2f, 5);
            field.Step(source, Vector3.Zero, 0, 0, 1d / 60, 1, false);
            Need(field.Events.Count == 0, "still generated cue");
            for (var i = 2; i <= 1000; i++)
            {
                field.Step(source, new Vector3(0, 0, -3), 1, 0, 1d / 60, i, false);
                Need(field.Events.Count <= MovementDisturbanceField.Capacity,
                    "event budget exceeded");
            }
            Need(field.Events.Count is > 0 and <= 7 && field.Events.All(c =>
                c.Radius <= 8 && c.Radius >= .65f && c.Strength is > 0 and <= 1),
                "event radius, strength or TTL unbounded");
            var before = field.Sense(source + Vector3.Right, false);
            Need(before.Strength > 0 && before.Events == field.Events.Count,
                "open movement not sensed");
            var clock = field.SimulationSeconds;
            var count = field.Events.Count;
            field.Step(source, new Vector3(0, 0, -3), 1, 0, 1d / 60, 1000, false);
            field.Step(source, new Vector3(0, 0, -3), 1, 0, 1d / 60, 1001, true);
            Need(field.SimulationSeconds == clock && field.Events.Count == count &&
                field.Sense(source + Vector3.Right, true).Strength == 0,
                "duplicate or paused clock advance");
            for (var i = 1001; i <= 1100; i++)
                field.Step(source, Vector3.Zero, 0, 0, 1d / 60, i, false);
            Need(field.Events.Count == 0 && field.Sense(source, false).Strength == 0,
                "stopped cue did not decay");

            var openField = new MovementDisturbanceField(space,
                Array.Empty<SoftSightCover>(), pike.GetRid(), fish.GetRid());
            var observer = new Vector3(0, PikeCoverFixture.Bed(0, 7) + 2f, 7);
            openField.Step(source, new Vector3(0, 0, -4), 1, 0, 1d / 60, 1, false);
            var open = openField.Sense(observer, false);
            Need(open.Strength > 0 && !open.HardCover, "open lane cue absent");
            var camera = fish.GetNode<Camera3D>("CameraRig/Camera3D");
            var original = camera.Transform;
            var environment = scene.GetNode<WorldEnvironment>("WorldEnvironment").Environment;
            var fog = environment.FogEnabled;
            camera.Position += new Vector3(4, 3, 2);
            environment.FogEnabled = !fog;
            Need(openField.Sense(observer, false).Equals(open), "camera/fog changed cue");
            fish.SetInputMode("mouse");
            Need(openField.Sense(observer, false).Equals(open), "preset changed cue");
            camera.Transform = original;
            environment.FogEnabled = fog;

            var softFamilies = 0;
            foreach (var family in new[] { "010", "011", "012", "013", "014" })
            {
                var softened = false;
                foreach (var plant in plants.Where(p => p.FafId.Contains("PLANT-" + family)))
                {
                    var box = plant.VisibleBounds;
                    var center = box.GetCenter();
                    var y = Mathf.Max(box.Position.Y + box.Size.Y * .6f,
                        PikeCoverFixture.Bed(center.X, center.Z) + .6f);
                    var from = new Vector3(box.Position.X - .4f, y, center.Z);
                    var to = new Vector3(box.End.X + .4f, y, center.Z);
                    var withPlants = new MovementDisturbanceField(space, plants, pike.GetRid(), fish.GetRid());
                    var withoutPlants = new MovementDisturbanceField(space,
                        Array.Empty<SoftSightCover>(), pike.GetRid(), fish.GetRid());
                    foreach (var candidate in new[] { withPlants, withoutPlants })
                        candidate.Step(to, new Vector3(0, 0, -5), 1, 1, 1d / 60, 1, false);
                    var covered = withPlants.Sense(from, false);
                    var exposed = withoutPlants.Sense(from, false);
                    softened |= exposed.Strength > 0 && !exposed.HardCover &&
                        covered.Strength > 0 && covered.Strength < exposed.Strength &&
                        covered.SoftCover >= .35f;
                }
                Need(softened, "plant family lacks passable partial cue " + family);
                softFamilies++;
            }
            var wood = scene.CoverPlan.Single(p => p.Id == "FAF-WOOD-008A");
            var woodBasis = new Basis(Vector3.Up, wood.Yaw);
            var woodCue = default(DisturbanceEvidence);
            foreach (var y in new[] { 1.2f, 1.7f, 2.2f, 2.8f })
            foreach (var z in new[] { -2f, 0f, 2f })
            foreach (var span in new[] { 1.5f, 2.5f, 3.5f })
            {
                var left = wood.Position + woodBasis * new Vector3(-span, y, z);
                var right = wood.Position + woodBasis * new Vector3(span, y, z);
                var woodField = new MovementDisturbanceField(space,
                    Array.Empty<SoftSightCover>(), pike.GetRid(), fish.GetRid());
                woodField.Step(right, new Vector3(0, 0, -6), 1, 1, 1d / 60, 1, false);
                var candidate = woodField.Sense(left, false);
                if (candidate.HardCover && candidate.Strength > 0) woodCue = candidate;
            }
            Need(woodCue.HardCover && woodCue.Strength > 0,
                "wood did not damp a persistent movement cue");

            field.Reset();
            Need(field.Events.Count == 0 && field.SimulationSeconds == 0,
                "retry/reset retained cues");
            var live = scene.MovementCues!;
            fish.SetInputMode("keyboard");
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = Key.W, Pressed = true });
            for (var i = 0; i < 45; i++) await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = Key.W, Pressed = false });
            Need(fish.MotorStepCount > 40 && live.Events.Count > 0,
                "real fish motor generated no movement cues");
            var velocityBeforeSense = fish.Velocity;
            _ = scene.ObserveFish();
            Need(fish.Velocity.IsEqualApprox(velocityBeforeSense),
                "sensing changed the fish motor");
            var liveTime = live.SimulationSeconds;
            var liveCount = live.Events.Count;
            tree.Paused = true;
            Need(scene.ObserveFish().Disturbance.Strength == 0 &&
                live.SimulationSeconds == liveTime && live.Events.Count == liveCount,
                "pause changed or exposed stored cues");
            tree.Paused = false;
            fish.ClearAndDisarmInput();
            for (var i = 0; i < 3; i++) await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(live.Events.Count == liveCount, "focus-style input clearing duplicated cues");
            live.Reset();
            Need(live.Events.Count == 0 && live.SimulationSeconds == 0,
                "fixture retry reset left cue state");
            GD.Print($"PIKE_DISTURBANCE_CHECK_OK seed={PikeCoverFixture.Seed} plants={softFamilies} " +
                $"open={open.Strength:F3} wood={woodCue.Strength:F3} live_events={liveCount}");
            tree.Quit();
        }
        catch (Exception error)
        {
            GD.PushError("PIKE_DISTURBANCE_CHECK_FAILED " + error);
            tree.Quit(1);
        }
    }

    private static void Need(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
