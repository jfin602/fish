#nullable enable
using Godot;
using System;
using System.Linq;

namespace Fish;

/// <summary>Normal Pond frog route, registered fry sensing and contact, then fresh-life cleanup.</summary>
public static class P6FrogChecks
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
            var frog = session.Frog;
            var ambient = main.GetNode<FafAmbientPondLife>("Habitat/PondAmbientLife");
            var asset = PondCreatureCatalog.Ambient.Single(a => a.Id == PondFrogActor.SpeciesId);
            var animation = frog.GetNode<Node3D>("Rig")
                .FindChildren("*", "AnimationPlayer", true, false).OfType<AnimationPlayer>().Single();
            Need(WorldSession.ActiveManifest?.Seed == 71 &&
                frog.StableId == WorldSession.ActiveManifest.WorldId + ":frog:shore:0" &&
                ambient.Population.All(a => a.Id != PondFrogActor.SpeciesId) &&
                SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.PondId, PondFrogActor.SpeciesId,
                    null, SpeciesRole.Ecological, out _, out _) &&
                !SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.OceanId, PondFrogActor.SpeciesId,
                    null, SpeciesRole.Ecological, out _, out _) &&
                PondCreatureCatalog.ExportMatches(asset) &&
                new[] { "IdleShore", "SurfaceFloat", "Swim", "TurnLeft", "TurnRight", "Hop", "Dive" }
                    .All(role => animation.HasAnimation(asset.Clip(role))) &&
                frog.GetNode<CollisionShape3D>("Collision").Shape is CapsuleShape3D shape &&
                Mathf.Abs(shape.Radius - .07f) < .0001f && Mathf.Abs(shape.Height - .24f) < .0001f,
                "registered frog rig, scene slot or collider invalid");
            Need(frog.Route.Count == 3 &&
                bed.Basin.DistanceToShore(frog.Route[0].X, frog.Route[0].Z) < 0 &&
                bed.Basin.DistanceToShore(frog.Route[1].X, frog.Route[1].Z) > 0 &&
                bed.Basin.DistanceToShore(frog.Route[2].X, frog.Route[2].Z) > 0,
                "bank-to-water route invalid");
            if (_oldLife is not null)
            {
                Need(session.Growth.Identity != _oldLife && session.CatchEvent is null &&
                    session.NpcCatchEvents.Count == 0 && !session.Caught &&
                    frog.MotorSteps == 0 && frog.TargetId is null &&
                    session.PreyActors.All(p => p.IsAlive) &&
                    frog.ClipRole == "IdleShore", "frog retry retained old pursuit or consumption");
                GD.Print($"P6_FROG_CHECK_OK world={WorldSession.ActiveManifest!.WorldId} seed=71 " +
                    $"actor={frog.StableId} old_life={_oldLife} new_life={session.Growth.Identity} " +
                    $"event={_oldEvent} retry=clear");
                tree.Quit();
                return;
            }

            session.SkipIntro();
            for (var i = 0; i < 300 && session.IntroActive; i++)
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(!session.IntroActive && session.Growth.Stage == "larva", "frog test hatch failed");
            session.SetPhysicsProcess(false);
            fish.SetPhysicsProcess(false);
            fish.GlobalPosition = session.NestPosition + Vector3.Up * .2f;
            var start = frog.GlobalPosition;
            var visited = new System.Collections.Generic.HashSet<string>();
            var minShore = float.PositiveInfinity;
            var maxShore = float.NegativeInfinity;
            var minClearance = float.PositiveInfinity;
            double time = 0;
            for (var i = 0; i < 650; i++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                time += 1.0 / 60;
                session.TickFrogForCheck(time, 1f / 60);
                visited.Add(frog.ClipRole);
                var routeShore = bed.Basin.DistanceToShore(frog.GlobalPosition.X, frog.GlobalPosition.Z);
                minShore = Mathf.Min(minShore, routeShore);
                maxShore = Mathf.Max(maxShore, routeShore);
                minClearance = Mathf.Min(minClearance, frog.GlobalPosition.Y -
                    bed.GetHeightAt(frog.GlobalPosition.X, frog.GlobalPosition.Z));
            }
            var signed = bed.Basin.DistanceToShore(frog.GlobalPosition.X, frog.GlobalPosition.Z);
            GD.Print($"P6_FROG_ROUTE actor={frog.StableId} start={start} now={frog.GlobalPosition} " +
                $"shore={signed:F3} clips={string.Join(',', visited.Order())} steps={frog.MotorSteps}");
            Need(frog.MotorSteps > 600 && frog.GlobalPosition.DistanceTo(start) > .25f &&
                visited.Contains("Hop") && visited.Contains("Swim") &&
                visited.Contains("SurfaceFloat") &&
                minClearance >= .03f && minShore >= -.85f && maxShore <= 1.5f,
                "physical bank/surface route or clip switching failed");
            var beforePause = frog.GlobalTransform;
            tree.Paused = true;
            session.TickFrogForCheck(time + 5, 1f / 60);
            Need(frog.GlobalTransform == beforePause && session.GameSeconds == time,
                "paused frog or game clock advanced");
            tree.Paused = false;

            fish.GlobalPosition = frog.Route[1];
            fish.GlobalPosition = new Vector3(fish.GlobalPosition.X,
                fish.PondSurfaceHeightAt(fish.GlobalPosition.X, fish.GlobalPosition.Z) - .055f,
                fish.GlobalPosition.Z);
            var water = SightEnvironment.ForBakedWorld(bed.Basin,
                fish.PondSurfaceHeightAt(fish.GlobalPosition.X, fish.GlobalPosition.Z), SolarState.At(.5));
            var sight = new WorldSightSensor(main.GetWorld3D().DirectSpaceState,
                Array.Empty<SoftSightCover>(), frog.GetRid(), fish.GetRid(), 1.35f, 2.7f);
            var heading = (fish.GlobalPosition - frog.MouthPosition).Normalized();
            var open = sight.Observe(frog.MouthPosition, heading,
                new SightTarget(fish.GlobalPosition, session.Growth.ColliderRadius, 1f),
                water, time + 1, false);
            Need(open.Detected && open.ClearRays > 0, "frog nearby fry sight unavailable");
            var barrier = new StaticBody3D { Name = "FrogSightBarrier", CollisionLayer = 1 };
            barrier.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = new Vector3(.28f, .3f, .28f) } });
            session.AddChild(barrier);
            barrier.GlobalPosition = frog.MouthPosition.Lerp(fish.GlobalPosition, .5f);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            sight.Reset();
            var blocked = sight.Observe(frog.MouthPosition, heading,
                new SightTarget(fish.GlobalPosition, session.Growth.ColliderRadius, 1f),
                water, time + 1.4, false);
            Need(!blocked.Detected && blocked.ClearRays == 0,
                "frog sight crossed solid shore cover");
            barrier.QueueFree();
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            fish.GlobalPosition = session.NestPosition + Vector3.Up * .2f;

            var adult = session.PreyActors.First(p => p.Stage == "adult");
            var adultHome = adult.GlobalPosition;
            adult.GlobalPosition = frog.GlobalPosition + (frog.Route[0] - frog.GlobalPosition).Normalized() * .35f;
            adult.GlobalPosition = new Vector3(adult.GlobalPosition.X, frog.GlobalPosition.Y,
                adult.GlobalPosition.Z);
            frog.LookAt(adult.GlobalPosition, Vector3.Up);
            var evadeClips = new System.Collections.Generic.HashSet<string>();
            var evaded = false;
            for (var i = 0; i < 190; i++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                time += 1.0 / 60;
                session.TickFrogForCheck(time, 1f / 60);
                evadeClips.Add(frog.ClipRole);
                evaded |= frog.Mode == PondFrogMode.Evade;
                if (i == 18) adult.GlobalPosition = adultHome;
            }
            Need(evaded && evadeClips.Contains("Dive") && frog.TargetId is null &&
                bed.Basin.DistanceToShore(frog.GlobalPosition.X, frog.GlobalPosition.Z) < .3f,
                "frog did not evade nearby sensed large fish toward bank");
            GD.Print($"P6_FROG_EVADE actor={frog.StableId} clips={string.Join(',', evadeClips.Order())} " +
                $"shore={bed.Basin.DistanceToShore(frog.GlobalPosition.X, frog.GlobalPosition.Z):F3}");

            var juvenile = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "juvenile")!;
            var larva = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "larva")!;
            Need(PondFrogActor.Eligible(new PondFrogTarget("fry", larva.Id, "larva",
                    larva.Dimensions[1], frog.Route[2], .01f, fish.GetRid(), true, true)) &&
                !PondFrogActor.Eligible(new PondFrogTarget("large", juvenile.Id, "juvenile",
                    juvenile.Dimensions[1], frog.Route[2], .05f, fish.GetRid(), true, true)),
                "frog registered stage limit failed");
            // A local larval Bluegill stands in for a future autonomous NPC target.
            var npc = session.PreyActors.First(p => p.Stage == "larva");
            npc.GlobalPosition = frog.Route[2];
            npc.GlobalPosition = new Vector3(npc.GlobalPosition.X,
                fish.PondSurfaceHeightAt(npc.GlobalPosition.X, npc.GlobalPosition.Z) - .055f,
                npc.GlobalPosition.Z);
            Need(bed.Basin.DistanceToShore(npc.GlobalPosition.X, npc.GlobalPosition.Z) > 0,
                "NPC target not in shallow habitat");
            var beforeNpc = session.NpcCatchEvents.Count;
            var npcLastMode = frog.Mode;
            for (var i = 0; i < 1300 && session.NpcCatchEvents.Count == beforeNpc; i++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                time += 1.0 / 60;
                session.TickFrogForCheck(time, 1f / 60);
                if (frog.Mode != npcLastMode)
                {
                    GD.Print($"P6_FROG_NPC_TRACE time={time:F2} mode={frog.Mode} " +
                        $"frog={frog.GlobalPosition} target={npc.GlobalPosition} " +
                        $"mouth_distance={frog.MouthPosition.DistanceTo(npc.GlobalPosition):F3}");
                    npcLastMode = frog.Mode;
                }
            }
            Need(session.NpcCatchEvents.Count == beforeNpc + 1 && !npc.IsAlive &&
                session.NpcCatchEvents[0].PreyId == npc.StableId,
                $"frog never sensed and physically ate eligible NPC fry mode={frog.Mode} " +
                $"frog={frog.GlobalPosition} target={npc.GlobalPosition} " +
                $"mouth_distance={frog.MouthPosition.DistanceTo(npc.GlobalPosition):F3}");
            GD.Print($"P6_FROG_NPC event={session.NpcCatchEvents[0].Id} " +
                $"mouth={session.NpcCatchEvents[0].ContactPosition} time={time:F2}");
            for (var i = 0; i < 80; i++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                time += 1.0 / 60;
                session.TickFrogForCheck(time, 1f / 60);
            }
            Need(session.NpcCatchEvents.Count == 1, "frog double-consumed NPC");

            fish.GlobalPosition = frog.Route[1];
            fish.GlobalPosition = new Vector3(fish.GlobalPosition.X,
                fish.PondSurfaceHeightAt(fish.GlobalPosition.X, fish.GlobalPosition.Z) - .055f,
                fish.GlobalPosition.Z);
            time = Math.Max(time, 38);
            var disengagedBefore = frog.Disengagements;
            var sawFry = false;
            for (var i = 0; i < 700 && !sawFry; i++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                time += 1.0 / 60;
                session.TickFrogForCheck(time, 1f / 60);
                sawFry = frog.TargetId == session.Growth.Identity &&
                    frog.Mode is PondFrogMode.Notice or PondFrogMode.Strike;
            }
            Need(sawFry, "frog never acquired nearby player fry");
            fish.GlobalPosition = new Vector3(fish.GlobalPosition.X,
                fish.PondSurfaceHeightAt(fish.GlobalPosition.X, fish.GlobalPosition.Z) - .7f,
                fish.GlobalPosition.Z);
            for (var i = 0; i < 75; i++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                time += 1.0 / 60;
                session.TickFrogForCheck(time, 1f / 60);
            }
            Need(frog.Disengagements > disengagedBefore && frog.TargetId is null &&
                !session.Caught, "frog tracked inaccessible deep fry after sight loss");
            fish.GlobalPosition = frog.Route[1];
            fish.GlobalPosition = new Vector3(fish.GlobalPosition.X,
                fish.PondSurfaceHeightAt(fish.GlobalPosition.X, fish.GlobalPosition.Z) - .055f,
                fish.GlobalPosition.Z);
            time += 15;
            var lastMode = frog.Mode;
            var lastTarget = frog.TargetId;
            for (var i = 0; i < 700 && !session.Caught; i++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                time += 1.0 / 60;
                session.TickFrogForCheck(time, 1f / 60);
                if (frog.Mode != lastMode || frog.TargetId != lastTarget)
                {
                    GD.Print($"P6_FROG_TRACE time={time:F2} mode={frog.Mode} target={frog.TargetId} " +
                        $"frog={frog.GlobalPosition} fish={fish.GlobalPosition} " +
                        $"mouth_distance={frog.MouthPosition.DistanceTo(fish.GlobalPosition):F3}");
                    lastMode = frog.Mode;
                    lastTarget = frog.TargetId;
                }
            }
            Need(session.Caught && session.CatchEvent is { } outcome &&
                outcome.PredatorId == frog.StableId && outcome.BassId == session.Growth.Identity,
                $"frog never made a physical fry catch mode={frog.Mode} target={frog.TargetId} " +
                $"frog={frog.GlobalPosition} fish={fish.GlobalPosition} " +
                $"distance={frog.MouthPosition.DistanceTo(fish.GlobalPosition):F3} " +
                $"shore={bed.Basin.DistanceToShore(frog.GlobalPosition.X, frog.GlobalPosition.Z):F2} " +
                $"disengagements={frog.Disengagements} seconds={time:F2}");
            _oldLife = session.Growth.Identity;
            _oldEvent = session.CatchEvent!.Value.Id;
            session.Retry();
        }
        catch (Exception error) { Fail(tree, error); }
    }

    private static void Need(bool good, string message)
    { if (!good) throw new InvalidOperationException(message); }

    private static void Fail(SceneTree tree, Exception error)
    { GD.PushError("P6_FROG_CHECK_FAILED " + error); tree.Quit(1); }
}
