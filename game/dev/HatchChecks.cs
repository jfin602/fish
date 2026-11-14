#nullable enable
using Godot;
using System;
using System.Linq;

namespace Fish;

/// <summary>Headless egg, skip, pause, camera and retry state checks.</summary>
public static class HatchChecks
{
    private static bool _started;

    public static async void Run(PikeEncounter first)
    {
        if (_started) return;
        _started = true;
        var tree = first.GetTree();
        try
        {
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            var fish = first.GetNode<FishController>("Fish");
            var identity = first.Growth.Identity;
            var start = fish.GlobalPosition;
            Need(first.IntroActive && first.Growth.Stage == "egg" &&
                first.GetNodeOrNull<Node3D>("BassEggVisual") is not null &&
                first.GetNodeOrNull<Node3D>("BassNest") is not null &&
                fish.GetNode<CollisionShape3D>("Collision").Disabled &&
                !fish.Visible && fish.MotorStepCount == 0 &&
                first.MovementCues!.SimulationSeconds == 0 &&
                first.Feeding.Nutrition == 0 && first.Feeding.Events.Count == 0 &&
                first.PreyActors.Count == 10 &&
                first.Growth.Next(100) is null &&
                !first.TryBite().Consumed && !first.TryAdvanceGrowth(),
                "egg acquired motor, prey, nutrition or Pike clock");
            var egg = first.GetNode<Node3D>("BassEggVisual");
            var animation = egg.FindChild("AnimationPlayer", true, false) as AnimationPlayer;
            Need(animation is not null && animation.HasAnimation(
                PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "egg")!.Clip("Hatch")),
                "registered hatch Action missing");
            Need(PikeCoverFixture.WaterY - first.NestPosition.Y is > 1.5f and < 3f &&
                first.NestPosition.Y > PikeCoverFixture.Bed(first.NestPosition.X, first.NestPosition.Z),
                "nest not on submerged bed");
            first._Input(new InputEventKey { Keycode = Key.Escape, Pressed = true });
            Need(tree.Paused && first.IntroActive, "intro pause did not hold egg");
            Button(first, "SkipIntro").EmitSignal(BaseButton.SignalName.Pressed);
            Need(first.IntroActive && first.Growth.Stage == "egg", "paused skip advanced hatch");
            Button(first, "ResumeEncounter").EmitSignal(BaseButton.SignalName.Pressed);
            Need(!tree.Paused && first.IntroActive && Input.MouseMode == Input.MouseModeEnum.Visible,
                "intro resume altered hatch or captured the pointer");
            first._Notification((int)Node.NotificationApplicationFocusOut);
            Need(tree.Paused && first.IntroActive, "focus loss did not hold egg");
            Button(first, "ResumeEncounter").EmitSignal(BaseButton.SignalName.Pressed);
            var sawHatchAction = false;
            var sawEmergence = false;
            var sawCameraPan = false;
            var priorEye = first.GetNode<Camera3D>("HatchCamera").GlobalPosition;
            for (var i = 0; i < 600 && first.IntroActive; i++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                if (GodotObject.IsInstanceValid(animation) && animation!.CurrentAnimation ==
                    PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "egg")!.Clip("Hatch"))
                    sawHatchAction = true;
                if (first.EmergenceStarted && fish.GetNodeOrNull<Node3D>("PlayerVisual") is null)
                    throw new Exception("larva did not emerge as a real player visual");
                sawEmergence |= first.EmergenceStarted && fish.Visible && first.Growth.Stage == "larva" &&
                    first.GetNodeOrNull<Node3D>("BassEggVisual") is not null;
                var eye = first.GetNode<Camera3D>("HatchCamera").GlobalPosition;
                sawCameraPan |= eye.DistanceTo(priorEye) > .001f;
                Need(eye.DistanceTo(priorEye) < .06f, "intro camera jumped");
                priorEye = eye;
            }
            Need(sawHatchAction && sawEmergence && sawCameraPan,
                "watched path lacked hatch, physical larva emergence or camera pan");
            CheckLarva(first, identity, start);
            var watchedPosition = fish.GlobalPosition;
            first._Input(new InputEventKey { Keycode = Key.Escape, Pressed = true });
            Button(first, "RetryEncounter").EmitSignal(BaseButton.SignalName.Pressed);
            await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            var second = tree.CurrentScene as PikeEncounter;
            Need(second is not null && !GodotObject.IsInstanceValid(first) && second.IntroActive &&
                second.Growth.Stage == "egg" && second.Growth.Identity == identity &&
                second.PreyActors.Count == 10 && second.Feeding.Events.Count == 0,
                "retry failed to restore one egg and fixed prey population");
            var skippedStart = second!.GetNode<FishController>("Fish").GlobalPosition;
            Button(second, "SkipIntro").EmitSignal(BaseButton.SignalName.Pressed);
            second._Input(new InputEventKey { Keycode = Key.Enter, Pressed = true });
            second.CompleteHatch(); // Repeated Skip/key cannot hatch twice.
            for (var i = 0; i < 240 && second.IntroActive; i++)
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            CheckLarva(second, identity, skippedStart);
            Need(second.GetNode<FishController>("Fish").GlobalPosition.DistanceTo(watchedPosition) < .001f,
                "watch and skip produced different larval positions");
            var fry = second.GetNode<FishController>("Fish");
            CheckVisualBodyBounds(fry);
            var shell = second.GetNode<Node3D>("BassEggVisual");
            fry.GlobalPosition = shell.GlobalPosition + Vector3.Back * .10f;
            fry.GlobalBasis = Basis.Identity;
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(second.TryBite().Consumed && !second.ShellAvailable &&
                second.Feeding.Events.Count == 1 && second.Feeding.Nutrition == 0 &&
                second.Growth.Stage == "larva" && !second.TryBite().Consumed,
                "shell was not a one-time first bite without premature growth");
            fry.SetActivePreset(ControlPreset.Builtins().Single(p => p.Id == ControlPreset.KeyboardId));
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            var before = fry.GlobalPosition;
            fry._Input(new InputEventKey { PhysicalKeycode = Key.W, Pressed = true });
            for (var i = 0; i < 35; i++) await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            fry._Input(new InputEventKey { PhysicalKeycode = Key.W, Pressed = false });
            Need(fry.GlobalPosition.DistanceTo(before) > .005f &&
                fry.GlobalPosition.Y <= fry.PondSurfaceCenterLimit &&
                second.Growth.Stage == "larva" && second.Feeding.Events.Count == 1,
                "fry did not swim slowly within the waterline");
            await CheckThinWall(second, fry);
            GD.Print("HATCH_CHECK_OK paths=watch,skip identity=stable stage=larva nutrition=0 prey=10 pause=held retry=clean camera=continuous shell=one-bite body=aligned wall=blocked fry=mobile");
            tree.Quit();
        }
        catch (Exception error)
        {
            GD.PushError("HATCH_CHECK_FAILED " + error);
            tree.Quit(1);
        }
    }

    private static void CheckLarva(PikeEncounter scene, string identity, Vector3 start)
    {
        var fish = scene.GetNode<FishController>("Fish");
        var camera = fish.GetNode<Camera3D>("CameraRig/Camera3D");
        Need(!scene.IntroActive && scene.Growth.Identity == identity &&
            scene.Growth.Stage == "larva" && scene.Feeding.Nutrition == 0 &&
            scene.Feeding.Events.Count == 0 && scene.PreyActors.Count == 10 &&
            scene.GetNodeOrNull<Node3D>("BassEggVisual") is not null && scene.ShellAvailable &&
            fish.Visible && !fish.GetNode<CollisionShape3D>("Collision").Disabled &&
            fish.GetNodeOrNull<Node3D>("PlayerVisual") is not null &&
            fish.GetNode<CollisionShape3D>("Collision").Shape is CapsuleShape3D capsule &&
            Math.Abs(capsule.Radius - scene.Growth.ColliderRadius) < .0001f &&
            Math.Abs(capsule.Height - scene.Growth.ColliderLength) < .0001f &&
            camera.Current && camera.Near < .01f && camera.Far >= 150f &&
            camera.GlobalPosition.DistanceTo(fish.GlobalPosition) is > .05f and < 1.1f &&
            CameraClear(scene, fish, camera) &&
            FryBodyClear(scene, fish) &&
            fish.GlobalPosition.DistanceTo(start) < .001f &&
            fish.GlobalPosition.Y <= fish.PondSurfaceCenterLimit &&
            scene.MovementCues!.SimulationSeconds < .1,
            "hatch did not yield one tiny, submerged Bass without advancing Pike time");
    }

    private static bool CameraClear(PikeEncounter scene, FishController fish, Camera3D camera)
    {
        using var shape = new SphereShape3D { Radius = .006f };
        using var query = new PhysicsShapeQueryParameters3D
        {
            Shape = shape,
            Transform = new Transform3D(Basis.Identity, camera.GlobalPosition),
            CollisionMask = 1,
            Exclude = new Godot.Collections.Array<Rid> { fish.GetRid() },
        };
        return scene.GetWorld3D().DirectSpaceState.IntersectShape(query, 1).Count == 0;
    }

    private static bool FryBodyClear(PikeEncounter scene, FishController fish)
    {
        var collision = fish.GetNode<CollisionShape3D>("Collision");
        using var query = new PhysicsShapeQueryParameters3D
        {
            Shape = collision.Shape,
            Transform = collision.GlobalTransform,
            CollisionMask = fish.CollisionMask,
            Exclude = new Godot.Collections.Array<Rid> { fish.GetRid() },
        };
        return scene.GetWorld3D().DirectSpaceState.IntersectShape(query, 1).Count == 0;
    }

    private static void CheckVisualBodyBounds(FishController fish)
    {
        var collider = fish.GetNode<CollisionShape3D>("Collision");
        var capsule = collider.Shape as CapsuleShape3D ?? throw new Exception("fry capsule missing");
        var minZ = float.PositiveInfinity;
        var maxZ = float.NegativeInfinity;
        var maxX = 0f;
        var visual = fish.GetNode<Node3D>("PlayerVisual");
        foreach (var found in visual.FindChildren("*", "MeshInstance3D", true, false))
        {
            if (found is not MeshInstance3D { Mesh: not null } mesh) continue;
            var box = mesh.GetAabb();
            for (var mask = 0; mask < 8; mask++)
            {
                var local = box.Position + new Vector3(
                    (mask & 1) == 0 ? 0 : box.Size.X,
                    (mask & 2) == 0 ? 0 : box.Size.Y,
                    (mask & 4) == 0 ? 0 : box.Size.Z);
                var point = fish.ToLocal(mesh.GlobalTransform * local);
                minZ = Mathf.Min(minZ, point.Z);
                maxZ = Mathf.Max(maxZ, point.Z);
                maxX = Mathf.Max(maxX, Mathf.Abs(point.X));
            }
        }
        Need(float.IsFinite(minZ) &&
            minZ >= collider.Position.Z - capsule.Height * .5f - .001f &&
            maxZ <= collider.Position.Z + capsule.Height * .5f + .001f &&
            maxX <= capsule.Radius + .001f,
            $"fry mesh escapes body nose={minZ:F4} tail={maxZ:F4} width={maxX:F4}");
    }

    private static async System.Threading.Tasks.Task CheckThinWall(PikeEncounter scene, FishController fish)
    {
        var tree = scene.GetTree();
        fish.ClearAndDisarmInput();
        var open = new Vector3(0, PikeCoverFixture.Bed(0, 20) + 1.2f, 20);
        fish.GlobalPosition = open;
        fish.GlobalBasis = Basis.Identity;
        var wall = new StaticBody3D { Name = "FryThinWallProbe", CollisionLayer = 1 };
        scene.AddChild(wall);
        wall.GlobalPosition = open + Vector3.Forward * .12f;
        wall.AddChild(new CollisionShape3D { Shape = new BoxShape3D
            { Size = new Vector3(1f, 1f, .02f) } });
        var camera = fish.GetNode<Camera3D>("CameraRig/Camera3D");
        fish.GetNode<FollowCamera>("CameraRig").SnapToFish();
        await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
        await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
        fish._Input(new InputEventKey { PhysicalKeycode = Key.W, Pressed = true });
        var priorFish = fish.GlobalPosition;
        var priorEye = camera.GlobalPosition;
        for (var i = 0; i < 120; i++)
        {
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(fish.GlobalPosition.DistanceTo(priorFish) < .02f, "fry movement jumped");
            Need(camera.GlobalPosition.DistanceTo(priorEye) < .08f, "fry camera jumped near wall");
            priorFish = fish.GlobalPosition;
            priorEye = camera.GlobalPosition;
        }
        fish._Input(new InputEventKey { PhysicalKeycode = Key.W, Pressed = false });
        Need(fish.GlobalPosition.Z >= wall.GlobalPosition.Z + .01f + fish.BodyForwardReach - .003f,
            "fry passed through a thin physical wall");
        wall.QueueFree();
    }

    private static Button Button(Node root, string name) =>
        root.FindChild(name, true, false) as Button ?? throw new Exception("Missing button: " + name);

    private static void Need(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
