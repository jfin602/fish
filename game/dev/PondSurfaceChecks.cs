#nullable enable
using Godot;
using System;
using System.Linq;

namespace Fish;

/// <summary>Actual baked Pond and CharacterBody3D surface/contact regression.</summary>
public static class PondSurfaceChecks
{
    public static async void Start(Main main)
    {
        var tree = main.GetTree();
        try
        {
            var fish = main.GetNode<FishController>("Fish");
            var habitat = main.GetNode<Node3D>("Habitat");
            var seabed = habitat.GetNode<SeabedSurface>("Seabed");
            var upper = habitat.GetNode<CollisionShape3D>("HiddenLimits/Upper");
            fish.SetActivePreset(ControlPreset.Builtins()[0]);
            await Frames(tree, 3);
            var hasWater = seabed.TryGetPondWaterSurfaceHeight(out var water);
            Require(WorldSession.ActiveManifest?.WorldId ==
                WorldCatalog.Resolve(WorldCatalog.PondId).Bake?.WorldId &&
                WorldSession.SelectedWorldId == WorldCatalog.PondId &&
                hasWater &&
                Mathf.Abs(water - fish.PondWaterlineHeight) < .001f && upper.Disabled,
                "selected baked Pond surface/upper collision");
            var radius = fish.PondColliderRadius;
            var limit = fish.PondSurfaceCenterLimit;
            Require(Mathf.Abs(radius - .48f) < .001f &&
                Mathf.Abs(limit - (water - .65f * radius)) < .001f &&
                limit + radius > water && limit - radius < water,
                "collider-derived partial breach");
            var ascentAtLimit = FishController.ConstrainPondAscent(new Vector3(2f, 3f, -2f),
                limit, limit, 1f / 60f);
            Require(Mathf.Abs(ascentAtLimit.Y) < .00001f &&
                Mathf.Abs(ascentAtLimit.Length() - Mathf.Sqrt(17f)) < .0001f &&
                FishController.ConstrainPondAscent(new Vector3(1f, -2f, 0f),
                    limit, limit, 1f / 60f).Y == -2f,
                "water tangent projection and downward escape");
            var core = seabed.Basin.DeepCore;
            var deep = await Ascend(tree, fish, new Vector3(core.X, water - 5f, core.Y),
                limit, "deep", false);
            var shallow = await Ascend(tree, fish, new Vector3(core.X, water - 1.5f, core.Y),
                limit, "shallow", true);
            GD.Print($"POND_SURFACE_ASCENTS deep={deep.MaxY:F4}/{deep.MaxStepY:F4} shallow={shallow.MaxY:F4}/{shallow.MaxStepY:F4} limit={limit:F4}");
            Require(deep.MaxY > limit - .06f && shallow.MaxY > limit - .06f &&
                deep.MaxStepY < .07f && shallow.MaxStepY < .07f,
                "two-depth ascent reaches surface smoothly");
            var visualTop = VisualTop(fish);
            Require(visualTop > water && fish.GlobalPosition.Y - radius < water,
                "adult Bluegill anatomy should partly breach while collider remains immersed");

            var startTurn = fish.GlobalPosition;
            Key(fish, Godot.Key.W, true);
            Key(fish, Godot.Key.D, true);
            await Frames(tree, 28);
            Key(fish, Godot.Key.D, false);
            Require(fish.GlobalPosition.DistanceTo(startTurn) > .6f &&
                Mathf.Abs(fish.GlobalPosition.X - startTurn.X) > .12f &&
                fish.GlobalPosition.Y <= limit + .002f,
                "surface turn retains lateral travel");
            var cruiseSpeed = fish.Velocity.Length();
            Key(fish, Godot.Key.W, false);
            await Frames(tree, 25);
            var coastSpeed = fish.Velocity.Length();
            Key(fish, Godot.Key.S, true);
            await Frames(tree, 20);
            Key(fish, Godot.Key.S, false);
            Require(coastSpeed < cruiseSpeed && fish.Velocity.Length() < coastSpeed &&
                fish.GlobalPosition.Y <= limit + .002f,
                "coast and brake retain surface constraint");

            var beforeDescent = fish.GlobalPosition.Y;
            Key(fish, Godot.Key.Down, true);
            Key(fish, Godot.Key.W, true);
            await Frames(tree, 85);
            Key(fish, Godot.Key.Down, false);
            Key(fish, Godot.Key.W, false);
            Require(fish.GlobalPosition.Y < beforeDescent - .35f,
                "downward return leaves surface");

            fish.ClearAndDisarmInput();
            await Frames(tree, 3);
            fish.GlobalPosition = new Vector3(core.X, water - 1.6f, core.Y);
            var maxStep = 0f;
            var maxSpeed = 0f;
            for (var cycle = 0; cycle < 4; cycle++)
            {
                Key(fish, Godot.Key.Up, true);
                Key(fish, Godot.Key.W, true);
                for (var tick = 0; tick < 75; tick++)
                {
                    var previous = fish.GlobalPosition;
                    await Frames(tree, 1);
                    maxStep = Mathf.Max(maxStep, fish.GlobalPosition.DistanceTo(previous));
                    maxSpeed = Mathf.Max(maxSpeed, fish.Velocity.Length());
                    Require(fish.GlobalPosition.Y <= limit + .002f,
                        "repeated surface occupancy");
                }
                Key(fish, Godot.Key.Up, false);
                Key(fish, Godot.Key.Down, true);
                await Frames(tree, 55);
                Key(fish, Godot.Key.Down, false);
            }
            Key(fish, Godot.Key.W, false);
            Require(maxStep < .09f && maxSpeed <= fish.Tuning.MaxSpeed + .02f,
                "oscillation position/velocity spikes");

            fish.ClearAndDisarmInput();
            await Frames(tree, 2);
            fish.GlobalPosition = new Vector3(core.X, water - 1f, core.Y);
            var parked = fish.GlobalPosition;
            tree.Paused = true;
            await tree.ToSignal(tree.CreateTimer(.12, true), SceneTreeTimer.SignalName.Timeout);
            Require(fish.GlobalPosition.DistanceTo(parked) < .0001f,
                "pause moved fish at surface");
            tree.Paused = false;
            fish._Notification((int)FishController.NotificationApplicationFocusOut);
            await Frames(tree, 3);
            Require(fish.GetInputArmed() && fish.GetHeldInputCount() == 0 &&
                fish.Velocity.Length() < .001f &&
                fish.GlobalPosition.DistanceTo(parked) < .001f,
                "focus/reset released movement");

            fish.SetActivePreset(ControlPreset.Builtins().Single(p =>
                p.Id == ControlPreset.AdaptiveFollowId));
            await Frames(tree, 3);
            fish.GlobalPosition = new Vector3(core.X, water - 1f, core.Y);
            fish._Input(new InputEventMouseButton
            {
                ButtonIndex = MouseButton.Left, Pressed = true,
            });
            Key(fish, Godot.Key.W, true);
            var adaptiveStart = fish.GlobalPosition;
            for (var tick = 0; tick < 110; tick++)
            {
                await Frames(tree, 1);
                Require(fish.GlobalPosition.Y <= limit + .002f,
                    "Adaptive Follow ascent escaped Pond waterline");
            }
            fish._Input(new InputEventMouseButton
            {
                ButtonIndex = MouseButton.Left, Pressed = false,
            });
            Key(fish, Godot.Key.W, false);
            var adaptiveEnd = fish.GlobalPosition;
            Require(fish.CameraControl == CameraControlPolicy.AdaptiveFollow &&
                adaptiveEnd.DistanceTo(adaptiveStart) > 2f &&
                adaptiveEnd.Y > adaptiveStart.Y + .4f,
                "Adaptive Follow thrust/depth control at waterline");

            var bank = BankContact(fish, seabed, water);
            GD.Print($"POND_SURFACE_CHECK_OK world={WorldSession.ActiveManifest?.WorldId} " +
                $"water={water:F3} radius={radius:F3} center_max={limit:F3} " +
                $"visual_top={visualTop:F3} " +
                $"deep_max={deep.MaxY:F3} shallow_max={shallow.MaxY:F3} " +
                $"deep_step={deep.MaxStepY:F4} shallow_step={shallow.MaxStepY:F4} " +
                $"cruise={cruiseSpeed:F3} coast={coastSpeed:F3} " +
                $"adaptive_start={adaptiveStart} adaptive_end={adaptiveEnd} " +
                $"oscillation_step={maxStep:F4} oscillation_speed={maxSpeed:F3} bank={bank}");
            tree.Quit();
        }
        catch (Exception error)
        {
            tree.Paused = false;
            GD.PushError($"POND_SURFACE_CHECK_FAILED {error}");
            tree.Quit(1);
        }
    }

    private static async System.Threading.Tasks.Task<(float MaxY, float MaxStepY)> Ascend(
        SceneTree tree, FishController fish, Vector3 start, float limit, string label,
        bool turnWhileRising)
    {
        fish.ClearAndDisarmInput();
        await Frames(tree, 3);
        fish.GlobalPosition = start;
        await Frames(tree, 2);
        Key(fish, Godot.Key.Up, true);
        Key(fish, Godot.Key.W, true);
        var maxY = fish.GlobalPosition.Y;
        var maxStepY = 0f;
        for (var tick = 0; tick < 240; tick++)
        {
            if (turnWhileRising && tick == 25) Key(fish, Godot.Key.D, true);
            if (turnWhileRising && tick == 50) Key(fish, Godot.Key.D, false);
            var before = fish.GlobalPosition;
            await Frames(tree, 1);
            maxY = Mathf.Max(maxY, fish.GlobalPosition.Y);
            maxStepY = Mathf.Max(maxStepY, Mathf.Abs(fish.GlobalPosition.Y - before.Y));
            Require(fish.GlobalPosition.Y <= limit + .002f &&
                fish.Velocity.Length() <= fish.Tuning.MaxSpeed + .02f &&
                fish.GlobalPosition.Y >= before.Y - .002f,
                $"{label} sustained upward input escaped waterline");
        }
        Key(fish, Godot.Key.Up, false);
        Key(fish, Godot.Key.W, false);
        Require(!turnWhileRising || Mathf.Abs(fish.GlobalPosition.X - start.X) > .4f,
            "turning while rising must curve the swim path");
        GD.Print($"POND_SURFACE_ASCENT_TRACE label={label} start={start} end={fish.GlobalPosition} " +
            $"speed={fish.Velocity.Length():F3} armed={fish.GetInputArmed()} held={fish.GetHeldInputCount()} " +
            $"collisions={fish.GetSlideCollisionCount()} collider={(fish.GetSlideCollisionCount() > 0 ? fish.GetSlideCollision(0).GetCollider() : "none")}");
        return (maxY, maxStepY);
    }

    private static string BankContact(FishController fish, SeabedSurface seabed, float water)
    {
        // A direct shape sweep exercises the real generated shore collision,
        // independent of the now-disabled water ceiling and distant fail-safe wall.
        var basin = seabed.Basin;
        var x = basin.DeepCore.X;
        var z = basin.DeepCore.Y;
        while (basin.DistanceToShore(x, z) > 5f) z -= 1f;
        var start = new Vector3(x, basin.HeightAt(x, z) + fish.PondColliderRadius + .6f, z);
        fish.GlobalPosition = start;
        var hit = fish.MoveAndCollide(Vector3.Forward * 12f, testOnly: true);
        Require(hit is not null && hit.GetCollider() == seabed &&
            start.DistanceTo(hit.GetPosition()) < 12f &&
            fish.GlobalPosition.DistanceTo(start) < .001f,
            "generated bank must stop fish before hidden perimeter");
        return $"seabed travel={hit!.GetTravel().Length():F3} start_y={start.Y:F3} water_y={water:F3}";
    }

    private static float VisualTop(FishController fish)
    {
        var top = float.NegativeInfinity;
        foreach (var child in fish.GetNode<Node3D>("PlayerVisual")
            .FindChildren("*", "MeshInstance3D", true, false))
        {
            if (child is not MeshInstance3D mesh) continue;
            var bounds = mesh.GetAabb();
            for (var x = 0; x <= 1; x++)
            for (var y = 0; y <= 1; y++)
            for (var z = 0; z <= 1; z++)
            {
                var corner = bounds.Position + bounds.Size * new Vector3(x, y, z);
                top = Mathf.Max(top, mesh.ToGlobal(corner).Y);
            }
        }
        return top;
    }

    private static async System.Threading.Tasks.Task Frames(SceneTree tree, int count)
    {
        for (var i = 0; i < count; i++)
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
    }

    private static void Key(FishController fish, Godot.Key key, bool pressed) =>
        fish._Input(new InputEventKey { Keycode = key, PhysicalKeycode = key, Pressed = pressed });

    private static void Require(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException(label);
    }
}
