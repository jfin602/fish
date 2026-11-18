#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

/// <summary>Focused spatial, collision, sensing and lifecycle evidence for the Pond snake.</summary>
public static class P6SnakeChecks
{
    private static string? _oldLife;
    private static string? _oldEvent;

    public static async void RunRoute(WorldSelect selectWorld)
    {
        var tree = selectWorld.GetTree();
        try
        {
            (selectWorld.FindChild("World_pond", true, false) as Button)!
                .EmitSignal(BaseButton.SignalName.Pressed);
            (selectWorld.FindChild("ChooseWorldButton", true, false) as Button)!
                .EmitSignal(BaseButton.SignalName.Pressed);
            for (var i = 0; i < 3; i++) await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            var select = tree.CurrentScene as CharacterSelect;
            Need(select is not null, "Pond character selection missing");
            select!.FindChildren("Character_*", "Button", true, false).OfType<Button>()
                .Single(button => button.Text.Contains("Bass", StringComparison.Ordinal))
                .EmitSignal(BaseButton.SignalName.Pressed);
            (select.FindChild("EnterWorldButton", true, false) as Button)!
                .EmitSignal(BaseButton.SignalName.Pressed);
        }
        catch (Exception error) { Fail(tree, error); }
    }

    public static async void Run(PondBassSession session)
    {
        var tree = session.GetTree();
        try
        {
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            var main = session.GetParent<Main>();
            var fish = main.GetNode<FishController>("Fish");
            var bed = main.GetNode<SeabedSurface>("Habitat/Seabed");
            var snake = session.Snake;
            var ambient = main.GetNode<FafAmbientPondLife>("Habitat/PondAmbientLife");
            var asset = PondCreatureCatalog.Ambient.Single(a => a.Id == PondSnakeActor.SpeciesId);
            var animation = snake.GetNode<Node3D>("Rig")
                .FindChildren("*", "AnimationPlayer", true, false).OfType<AnimationPlayer>().Single();
            Need(WorldSession.ActiveManifest?.Seed == 71 &&
                snake.StableId == WorldSession.ActiveManifest.WorldId + ":snake:shore:0" &&
                snake.StableId != session.Frog.StableId &&
                ambient.Population.All(a => a.Id != PondSnakeActor.SpeciesId) &&
                SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.PondId, PondSnakeActor.SpeciesId,
                    null, SpeciesRole.Ecological, out _, out _) &&
                !SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.OceanId, PondSnakeActor.SpeciesId,
                    null, SpeciesRole.Ecological, out _, out _) &&
                PondCreatureCatalog.ExportMatches(asset) &&
                new[] { "IdleFloat", "SurfaceSwim", "UnderwaterSwim", "ShoreCrawl",
                    "TurnLeft", "TurnRight", "Dive" }
                    .All(role => animation.HasAnimation(asset.Clip(role))) &&
                snake.GetNode<CollisionShape3D>("Collision").Shape is CapsuleShape3D shape &&
                Mathf.Abs(shape.Radius - .055f) < .0001f && Mathf.Abs(shape.Height - .90f) < .0001f,
                "registered snake rig, slot or collider invalid");
            Need(snake.Route.Count == 4 &&
                bed.Basin.DistanceToShore(snake.Route[0].X, snake.Route[0].Z) < 0 &&
                bed.Basin.DistanceToShore(snake.Route[1].X, snake.Route[1].Z) > 0 &&
                snake.Route[3].Y < snake.Route[2].Y,
                "snake bank/surface/dive route invalid");
            GD.Print("P6_SNAKE_ROUTE_POINTS " + string.Join(";", snake.Route.Select(p =>
                $"{p} shore={bed.Basin.DistanceToShore(p.X, p.Z):F3} depth={fish.PondSurfaceHeightAt(p.X, p.Z) - bed.GetHeightAt(p.X, p.Z):F3}")));
            if (_oldLife is not null)
            {
                Need(session.Growth.Identity != _oldLife && session.CatchEvent is null &&
                    session.NpcCatchEvents.Count == 0 && !session.Caught &&
                    snake.MotorSteps == 0 && snake.TargetId is null &&
                    session.PreyActors.All(p => p.IsAlive), "snake retry retained old pursuit or consumption");
                GD.Print($"P6_SNAKE_CHECK_OK world={WorldSession.ActiveManifest!.WorldId} seed=71 " +
                    $"actor={snake.StableId} old_life={_oldLife} new_life={session.Growth.Identity} " +
                    $"event={_oldEvent} retry=clear");
                tree.Quit();
                return;
            }
            session.SkipIntro();
            for (var i = 0; i < 300 && session.IntroActive; i++)
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(!session.IntroActive, "snake test hatch failed");
            session.SetPhysicsProcess(false);
            fish.SetPhysicsProcess(false);
            fish.GlobalPosition = session.NestPosition + Vector3.Up * .2f;
            var start = snake.GlobalPosition;
            var clips = new HashSet<string>();
            var minShore = float.PositiveInfinity;
            var maxShore = float.NegativeInfinity;
            var minFloor = float.PositiveInfinity;
            var maxDepth = float.NegativeInfinity;
            double time = 0;
            for (var i = 0; i < 900; i++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                time += 1.0 / 60;
                session.TickSnakeForCheck(time, 1f / 60);
                clips.Add(snake.ClipRole);
                var shore = bed.Basin.DistanceToShore(snake.GlobalPosition.X, snake.GlobalPosition.Z);
                minShore = Mathf.Min(minShore, shore);
                maxShore = Mathf.Max(maxShore, shore);
                minFloor = Mathf.Min(minFloor, snake.GlobalPosition.Y -
                    bed.GetHeightAt(snake.GlobalPosition.X, snake.GlobalPosition.Z));
                maxDepth = Mathf.Max(maxDepth, fish.PondSurfaceHeightAt(snake.GlobalPosition.X, snake.GlobalPosition.Z) - snake.GlobalPosition.Y);
                if (i % 150 == 0) GD.Print($"P6_SNAKE_ROUTE_TRACE i={i} route={snake.RouteIndex} " +
                    $"pos={snake.GlobalPosition} speed={snake.ActualSpeed:F3} stuck={snake.StuckSteps} " +
                    $"depth={fish.PondSurfaceHeightAt(snake.GlobalPosition.X, snake.GlobalPosition.Z) - snake.GlobalPosition.Y:F3}");
            }
            GD.Print($"P6_SNAKE_ROUTE actor={snake.StableId} start={start} now={snake.GlobalPosition} " +
                $"shore=[{minShore:F3},{maxShore:F3}] floor={minFloor:F3} depth={maxDepth:F3} " +
                $"clips={string.Join(',', clips.Order())} steps={snake.MotorSteps} samples={snake.SightSamples}");
            Need(snake.MotorSteps >= 900 && snake.GlobalPosition.DistanceTo(start) > .3f &&
                clips.Contains("ShoreCrawl") && clips.Contains("SurfaceSwim") &&
                clips.Contains("IdleFloat") && clips.Contains("Dive") &&
                clips.Contains("UnderwaterSwim") && minFloor >= -.05f &&
                minShore >= -.75f && maxShore <= 2.3f && snake.SightSamples <= 45,
                "physical snake route, clip switching or perception budget failed");
            var beforePause = snake.GlobalTransform;
            tree.Paused = true;
            session.TickSnakeForCheck(time + 4, 1f / 60);
            Need(snake.GlobalTransform == beforePause && session.GameSeconds == time,
                "paused snake or game clock advanced");
            tree.Paused = false;

            var juvenile = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "juvenile")!;
            var adult = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "adult")!;
            Need(PondSnakeActor.Eligible(new PondSnakeTarget("young", juvenile.Id, "juvenile",
                    juvenile.Dimensions[1], snake.Route[1], .03f, fish.GetRid(), true, true)) &&
                !PondSnakeActor.Eligible(new PondSnakeTarget("grown", adult.Id, "adult",
                    adult.Dimensions[1], snake.Route[1], .12f, fish.GetRid(), true, true)),
                "snake age/size eligibility invalid");

            // A long collider-driven advance must stop at solid wood-sized cover.
            snake.GlobalPosition = snake.Route[1];
            snake.LookAt(snake.Route[2], Vector3.Up);
            var solid = new StaticBody3D { Name = "SnakeWoodClearance", CollisionLayer = 1 };
            solid.AddChild(new CollisionShape3D { Shape = new BoxShape3D {
                Size = new Vector3(1.4f, .45f, .18f) } });
            session.AddChild(solid);
            solid.GlobalBasis = snake.GlobalBasis;
            solid.GlobalPosition = snake.GlobalPosition - snake.GlobalBasis.Z * .67f;
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(snake.TestMove(snake.GlobalTransform, -snake.GlobalBasis.Z * .55f),
                "snake capsule did not see solid cover");
            var forward = -snake.GlobalBasis.Z;
            var from = snake.GlobalPosition;
            for (var i = 0; i < 45; i++)
            {
                snake.Velocity = forward * .7f;
                snake.MoveAndSlide();
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            }
            var advance = (snake.GlobalPosition - from).Dot(forward);
            Need(advance < .22f, $"snake passed solid cover advance={advance:F3}");
            GD.Print($"P6_SNAKE_COLLISION advance={advance:F3} blocked=true");
            snake.Velocity = Vector3.Zero;
            solid.QueueFree();
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);

            // A larval Bass can use a real gap narrower than the snake's body. The snake
            // first acquires it in open water, then must lose contact at the blocked lane.
            snake.GlobalPosition = snake.Route[1];
            snake.LookAt(snake.Route[2], Vector3.Up);
            forward = -snake.GlobalBasis.Z;
            fish.GlobalPosition = snake.GlobalPosition + forward * 1.2f;
            fish.GlobalPosition = new Vector3(fish.GlobalPosition.X,
                fish.PondSurfaceHeightAt(fish.GlobalPosition.X, fish.GlobalPosition.Z) - .055f,
                fish.GlobalPosition.Z);
            for (var i = 0; i < 180 && snake.TargetId != session.Growth.Identity; i++)
            { await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame); time += 1.0 / 60;
                session.TickSnakeForCheck(time, 1f / 60); }
            Need(snake.TargetId == session.Growth.Identity && !session.Caught,
                "snake did not acquire open Bass before cover escape");
            var gap = .09f;
            var width = 1.2f;
            var coverParts = new List<StaticBody3D>();
            foreach (var sign in new[] { -1f, 1f })
            {
                var part = new StaticBody3D { Name = "SnakeGapWood", CollisionLayer = 1 };
                part.AddChild(new CollisionShape3D { Shape = new BoxShape3D {
                    Size = new Vector3(width, .45f, .20f) } });
                session.AddChild(part);
                part.GlobalBasis = snake.GlobalBasis;
                part.GlobalPosition = snake.Route[1] + forward * .67f +
                    snake.GlobalBasis.X * sign * (gap + width) * .5f;
                coverParts.Add(part);
            }
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(fish.BodyRadius * 2 < gap && gap < .11f,
                "Bass-sized cover gap is not physically selective");
            var gapLoss = snake.Disengagements;
            for (var i = 0; i < 420; i++)
            { await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame); time += 1.0 / 60;
                session.TickSnakeForCheck(time, 1f / 60); }
            Need(!session.Caught && snake.Disengagements > gapLoss,
                "snake crossed narrow wood cover or pursued forever");
            fish.GlobalPosition = snake.Route[1] + forward * .30f;
            fish.GlobalPosition = new Vector3(fish.GlobalPosition.X,
                fish.PondSurfaceHeightAt(fish.GlobalPosition.X, fish.GlobalPosition.Z) - .055f,
                fish.GlobalPosition.Z);
            fish.GlobalBasis = snake.GlobalBasis;
            var fishStart = fish.GlobalPosition;
            for (var i = 0; i < 120; i++)
            {
                fish.Velocity = forward * .32f;
                fish.MoveAndSlide();
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            }
            var fishAdvance = (fish.GlobalPosition - fishStart).Dot(forward);
            Need(fishAdvance > .50f,
                $"Bass body failed physical cover escape advance={fishAdvance:F3}");
            GD.Print($"P6_SNAKE_COVER gap={gap:F3} bass_diameter={fish.BodyRadius * 2:F3} " +
                $"fish_advance={fishAdvance:F3} disengagements={snake.Disengagements - gapLoss} caught={session.Caught}");
            fish.Velocity = Vector3.Zero;
            foreach (var part in coverParts) part.QueueFree();
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            fish.GlobalPosition = session.NestPosition + Vector3.Up * .2f;
            time += 5;

            // Solid cover blocks a previously visible, nearby fish. Its stable ID stays the same.
            fish.GlobalPosition = snake.MouthPosition - snake.GlobalBasis.Z * .65f;
            fish.GlobalPosition = new Vector3(fish.GlobalPosition.X,
                fish.PondSurfaceHeightAt(fish.GlobalPosition.X, fish.GlobalPosition.Z) - .055f,
                fish.GlobalPosition.Z);
            var water = SightEnvironment.ForBakedWorld(bed.Basin,
                fish.PondSurfaceHeightAt(fish.GlobalPosition.X, fish.GlobalPosition.Z), SolarState.At(.5));
            var sensor = new WorldSightSensor(main.GetWorld3D().DirectSpaceState,
                Array.Empty<SoftSightCover>(), snake.GetRid(), fish.GetRid(), 2.4f, 2.2f);
            var open = sensor.Observe(snake.MouthPosition, -snake.GlobalBasis.Z,
                new SightTarget(fish.GlobalPosition, session.Growth.ColliderRadius, 1f), water, time + 1, false);
            Need(open.Detected && open.ClearRays > 0, "snake open-water sight unavailable");
            var segment = fish.GlobalPosition - snake.MouthPosition;
            var reedBox = new Aabb(new Vector3(
                Mathf.Min(snake.MouthPosition.X, fish.GlobalPosition.X),
                Mathf.Min(snake.MouthPosition.Y, fish.GlobalPosition.Y),
                Mathf.Min(snake.MouthPosition.Z, fish.GlobalPosition.Z)) - Vector3.One * .18f,
                new Vector3(Mathf.Abs(segment.X), Mathf.Abs(segment.Y), Mathf.Abs(segment.Z)) +
                Vector3.One * .36f);
            var reedSight = new WorldSightSensor(main.GetWorld3D().DirectSpaceState,
                new[] { new SoftSightCover("test-reeds", reedBox) },
                snake.GetRid(), fish.GetRid(), 2.4f, 2.2f);
            var screened = reedSight.Observe(snake.MouthPosition, -snake.GlobalBasis.Z,
                new SightTarget(fish.GlobalPosition, session.Growth.ColliderRadius, 1f), water, time + 1, false);
            Need(screened.Strength < open.Strength, "soft reeds did not reduce snake sight");
            var barrier = new StaticBody3D { Name = "SnakeSightBarrier", CollisionLayer = 1 };
            barrier.AddChild(new CollisionShape3D { Shape = new BoxShape3D {
                Size = new Vector3(.4f, .4f, .4f) } });
            session.AddChild(barrier);
            barrier.GlobalPosition = snake.MouthPosition.Lerp(fish.GlobalPosition, .5f);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            sensor.Reset();
            var blocked = sensor.Observe(snake.MouthPosition, -snake.GlobalBasis.Z,
                new SightTarget(fish.GlobalPosition, session.Growth.ColliderRadius, 1f), water, time + 1.4, false);
            Need(!blocked.Detected && blocked.ClearRays == 0, "snake sight crossed solid wood");
            barrier.QueueFree();
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            fish.GlobalPosition = session.NestPosition + Vector3.Up * .2f;

            var npc = session.PreyActors.First(p => p.Stage == "larva");
            snake.GlobalPosition = snake.Route[1];
            snake.LookAt(snake.Route[2], Vector3.Up);
            npc.GlobalPosition = snake.MouthPosition - snake.GlobalBasis.Z * .34f;
            npc.GlobalPosition = new Vector3(npc.GlobalPosition.X,
                fish.PondSurfaceHeightAt(npc.GlobalPosition.X, npc.GlobalPosition.Z) - .055f,
                npc.GlobalPosition.Z);
            snake.LookAt(npc.GlobalPosition, Vector3.Up);
            var beforeNpc = session.NpcCatchEvents.Count;
            for (var i = 0; i < 900 && session.NpcCatchEvents.Count == beforeNpc; i++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                time += 1.0 / 60;
                session.TickSnakeForCheck(time, 1f / 60);
            }
            Need(session.NpcCatchEvents.Count == beforeNpc + 1 && !npc.IsAlive &&
                session.NpcCatchEvents[^1].PreyId == npc.StableId,
                $"snake missed NPC fry mode={snake.Mode} snake={snake.GlobalPosition} " +
                $"prey={npc.GlobalPosition} mouth={snake.MouthPosition}");
            GD.Print($"P6_SNAKE_NPC event={session.NpcCatchEvents[^1].Id} " +
                $"mouth={session.NpcCatchEvents[^1].ContactPosition} time={time:F2}");
            for (var i = 0; i < 50; i++)
            { await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame); time += 1.0 / 60;
                session.TickSnakeForCheck(time, 1f / 60); }
            Need(session.NpcCatchEvents.Count == beforeNpc + 1, "snake double-consumed NPC");

            fish.GlobalPosition = snake.Route[1];
            fish.GlobalPosition = new Vector3(fish.GlobalPosition.X,
                fish.PondSurfaceHeightAt(fish.GlobalPosition.X, fish.GlobalPosition.Z) - .055f,
                fish.GlobalPosition.Z);
            time += 10;
            var beforeLoss = snake.Disengagements;
            for (var i = 0; i < 700 && snake.TargetId != session.Growth.Identity; i++)
            { await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame); time += 1.0 / 60;
                session.TickSnakeForCheck(time, 1f / 60); }
            Need(snake.TargetId == session.Growth.Identity, "snake never acquired small Bass");
            fish.GlobalPosition = new Vector3(fish.GlobalPosition.X,
                fish.PondSurfaceHeightAt(fish.GlobalPosition.X, fish.GlobalPosition.Z) - .8f,
                fish.GlobalPosition.Z);
            for (var i = 0; i < 180; i++)
            { await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame); time += 1.0 / 60;
                session.TickSnakeForCheck(time, 1f / 60); }
            Need(snake.Disengagements > beforeLoss && snake.TargetId is null && !session.Caught,
                "snake tracked hidden/deep fish indefinitely");

            // Reset only the diagnostic setup after the independent loss test; the ensuing catch
            // must still be earned by turning, motor travel and a live mouth overlap.
            snake.GlobalPosition = snake.Route[1];
            fish.GlobalPosition = snake.Route[1] +
                (snake.Route[2] - snake.Route[1]).Normalized() * .85f;
            fish.GlobalPosition = new Vector3(fish.GlobalPosition.X,
                fish.PondSurfaceHeightAt(fish.GlobalPosition.X, fish.GlobalPosition.Z) - .055f,
                fish.GlobalPosition.Z);
            snake.LookAt(fish.GlobalPosition, Vector3.Up);
            time += 7;
            for (var i = 0; i < 850 && !session.Caught; i++)
            { await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame); time += 1.0 / 60;
                session.TickSnakeForCheck(time, 1f / 60); }
            Need(session.Caught && session.CatchEvent is { } caught &&
                caught.PredatorId == snake.StableId && caught.BassId == session.Growth.Identity,
                $"snake never physically caught Bass mode={snake.Mode} snake={snake.GlobalPosition} " +
                $"fish={fish.GlobalPosition} mouth={snake.MouthPosition}");
            Need(snake.SightSamples is > 0 and < 250,
                $"snake perception cost unbounded samples={snake.SightSamples}");
            _oldLife = session.Growth.Identity;
            _oldEvent = session.CatchEvent!.Value.Id;
            session.Retry();
        }
        catch (Exception error) { Fail(tree, error); }
    }

    private static void Need(bool good, string message)
    { if (!good) throw new InvalidOperationException(message); }

    private static void Fail(SceneTree tree, Exception error)
    { GD.PushError("P6_SNAKE_CHECK_FAILED " + error); tree.Quit(1); }
}
