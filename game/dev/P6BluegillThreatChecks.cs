#nullable enable
using Godot;
using System;
using System.Linq;

namespace Fish;

/// <summary>Seeded normal-Pond Bluegill sight, movement, obstruction and catch checks.</summary>
public static class P6BluegillThreatChecks
{
    private static string? _caughtLife;
    private static string? _caughtEvent;

    public static async void RunRoute(WorldSelect worldSelect)
    {
        var tree = worldSelect.GetTree();
        try
        {
            (worldSelect.FindChild("World_pond", true, false) as Button)!
                .EmitSignal(BaseButton.SignalName.Pressed);
            (worldSelect.FindChild("ChooseWorldButton", true, false) as Button)!
                .EmitSignal(BaseButton.SignalName.Pressed);
            for (var i = 0; i < 3; i++) await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            var select = tree.CurrentScene as CharacterSelect;
            Need(select is not null, "Pond selection missing");
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
            var adults = session.PreyActors.Where(p => p.Stage == "adult").ToArray();
            Need(WorldSession.ActiveManifest?.Seed == 71 && adults.Length == 2 &&
                session.PreyActors.Count == 8 && session.BluegillSchools.Count == 2 &&
                adults.All(p => p.SchoolId == "pond-shallow-adults") &&
                adults.All(p => p.GlobalPosition.DistanceTo(session.NestPosition) > 3.2f) &&
                session.PreyActors.Select(p => p.StableId).Distinct().Count() == 8,
                "seeded adult school or stable identities missing");
            var adult = adults[0];
            var juvenile = session.PreyActors.First(p => p.Stage == "juvenile");
            var bluegillSubadult = PondCreatureCatalog.Stage(PikeEncounterAssets.BluegillId, "subadult")!;
            var bassLarva = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "larva")!;
            var bassJuvenile = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "juvenile")!;
            var bassSubadult = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "subadult")!;
            var bassAdult = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "adult")!;
            Need(BluegillPredation.Eligible(adult.SpeciesId, adult.Stage, adult.Length,
                    PikeEncounterAssets.BassId, "larva", bassLarva.Dimensions[1]) &&
                !BluegillPredation.Eligible(juvenile.SpeciesId, juvenile.Stage, juvenile.Length,
                    PikeEncounterAssets.BassId, "larva", bassLarva.Dimensions[1]) &&
                !BluegillPredation.Eligible(PikeEncounterAssets.BluegillId, "subadult",
                    bluegillSubadult.Dimensions[1], PikeEncounterAssets.BassId,
                    "larva", bassLarva.Dimensions[1]) &&
                !BluegillPredation.Eligible(adult.SpeciesId, adult.Stage, adult.Length,
                    PikeEncounterAssets.BassId, "juvenile", bassJuvenile.Dimensions[1]) &&
                !BluegillPredation.Eligible(adult.SpeciesId, adult.Stage, adult.Length,
                    PikeEncounterAssets.BassId, "subadult", bassSubadult.Dimensions[1]) &&
                !BluegillPredation.Eligible(adult.SpeciesId, adult.Stage, adult.Length,
                    PikeEncounterAssets.BassId, "adult", bassAdult.Dimensions[1]) &&
                FeedingLedger.IsEligiblePrey(session.PreyActors.First(p => p.Stage == "larva").SpeciesId,
                    "larva", session.PreyActors.First(p => p.Stage == "larva").Length,
                    "subadult", PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "subadult")!.Dimensions[1]),
                "registered diet or size gate invalid");
            if (_caughtLife is not null)
            {
                Need(session.Growth.Identity != _caughtLife && session.CatchEvent is null &&
                    !session.Caught && session.GameSeconds == 0 &&
                    !session.TryBluegillCatch(adult, _caughtLife),
                    "retry retained catch or accepted old Bass identity");
                GD.Print($"P6_BLUEGILL_THREAT_CHECK_OK world={WorldSession.ActiveManifest!.WorldId} " +
                    $"seed=71 adult={adult.StableId} old_life={_caughtLife} " +
                    $"new_life={session.Growth.Identity} event={_caughtEvent} retry=clear");
                tree.Quit();
                return;
            }

            session.SkipIntro();
            for (var i = 0; i < 300 && session.IntroActive; i++)
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(!session.IntroActive && session.Growth.Stage == "larva", "Bass hatch failed");
            session.SetPhysicsProcess(false);
            fish.SetPhysicsProcess(false);
            session.SetHatchSecondsForCheck(5);
            var habitat = main.GetNode<PondFafHabitat>("Habitat/PondFafEnvironment");
            var plants = PondSoftCover.Near(habitat.Placements, adult.GlobalPosition, 8f)
                .OrderBy(p => p.VisibleBounds.GetCenter().DistanceSquaredTo(adult.GlobalPosition))
                .Take(128).ToArray();
            var water = SightEnvironment.ForBakedWorld(
                main.GetNode<SeabedSurface>("Habitat/Seabed").Basin,
                fish.PondSurfaceHeightAt(adult.GlobalPosition.X, adult.GlobalPosition.Z),
                SolarState.At(.5));
            var sensor = new WorldSightSensor(main.GetWorld3D().DirectSpaceState, plants,
                adult.GetRid(), fish.GetRid(), range: 3.2f, acuity: 3.2f);
            var originalFish = fish.GlobalTransform;
            SightEvidence open = default;
            var found = false;
            for (var step = 0; step < 24 && !found; step++)
            {
                var angle = step * Mathf.Tau / 24f;
                var direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                var candidate = adult.GlobalPosition + direction * .8f;
                candidate.Y = adult.GlobalPosition.Y;
                fish.GlobalPosition = candidate;
                adult.LookAt(candidate, Vector3.Up);
                sensor.Reset();
                open = sensor.Observe(adult.GlobalPosition - adult.GlobalBasis.Z * .08f,
                    -adult.GlobalBasis.Z, new SightTarget(fish.GlobalPosition,
                        session.Growth.ColliderRadius, 1f), water, 5, false);
                found = open.Detected && open.ClearRays > 0 && open.SoftCover > .55f;
            }
            Need(found, "no seeded exposed sight route near adult");
            GD.Print($"P6_BLUEGILL_SIGHT adult={adult.StableId} bass={session.Growth.Identity} " +
                $"score={open.Strength:F3} rays={open.ClearRays} soft={open.SoftCover:F3} " +
                $"at={fish.GlobalPosition}");
            var camera = fish.GetNode<Camera3D>("CameraRig/Camera3D");
            var cameraBefore = camera.GlobalTransform;
            camera.GlobalPosition += new Vector3(2, 1, -3);
            sensor.Reset();
            var afterCamera = sensor.Observe(adult.GlobalPosition - adult.GlobalBasis.Z * .08f,
                -adult.GlobalBasis.Z, new SightTarget(fish.GlobalPosition,
                    session.Growth.ColliderRadius, 1f), water, 5, false);
            camera.GlobalTransform = cameraBefore;
            Need(afterCamera.Detected == open.Detected &&
                Mathf.Abs(afterCamera.Strength - open.Strength) < .00001f,
                "camera or reticle changed world sight");
            Need(TryActualPlantConcealment(main, session, adult, fish, plants, sensor, water,
                    out var concealed),
                "no actual planted concealment line reduced adult sight");
            sensor.Reset();
            open = sensor.Observe(adult.GlobalPosition - adult.GlobalBasis.Z * .08f,
                -adult.GlobalBasis.Z, new SightTarget(fish.GlobalPosition,
                    session.Growth.ColliderRadius, 1f), water, 5, false);
            var cue = new MovementDisturbanceField(main.GetWorld3D().DirectSpaceState, plants,
                adult.GetRid(), fish.GetRid(), speedReference: .6f);
            cue.Step(fish.GlobalPosition, Vector3.Zero, 0, 0, .3, 1, false);
            Need(cue.Events.Count == 0, "stationary fry emitted movement cue");
            cue.Step(fish.GlobalPosition, new Vector3(.6f, 0, 0), 1, .7f, .3, 2, false);
            Need(cue.Events.Count == 1 && cue.Sense(adult.GlobalPosition, false).Strength > 0,
                "fast fry did not emit a sensed local cue");

            var mind = session.BluegillThreats[session.PreyActors.ToList().IndexOf(adult)];
            mind.Reset();
            mind.Step(open, default, 5, false);
            Need(mind.Mode == BluegillThreatMode.Notice && !mind.StrikeReady,
                "adult skipped warning window");
            sensor.Reset();
            var sight = sensor.Observe(adult.GlobalPosition - adult.GlobalBasis.Z * .08f,
                -adult.GlobalBasis.Z, new SightTarget(fish.GlobalPosition,
                    session.Growth.ColliderRadius, 1f), water, 5.5, false);
            mind.Step(sight, default, 5.5, false);
            Need(mind.Mode == BluegillThreatMode.Chase && !mind.StrikeReady,
                "adult did not pursue after repeated real sight");
            var beforePause = session.GameSeconds;
            var eventsBeforePause = cue.Events.Count;
            var shell = main.GetNode<PlayShell>("PlayShell");
            (shell.FindChild("OptionsButton", true, false) as Button)!
                .EmitSignal(BaseButton.SignalName.Pressed);
            mind.Step(concealed, default, 8, true);
            cue.Step(fish.GlobalPosition, new Vector3(.6f, 0, 0), 1, 1, 1, 3, true);
            Need(tree.Paused && session.GameSeconds == beforePause &&
                mind.Mode == BluegillThreatMode.Chase && cue.Events.Count == eventsBeforePause,
                "pause advanced Bluegill threat or game time");
            (shell.FindChild("ResumeButton", true, false) as Button)!
                .EmitSignal(BaseButton.SignalName.Pressed);
            var motorBefore = adult.MotorStepCount;
            adult.Tick(default, default, 5.6, .05f, false,
                session.BluegillSchools.Single(s => s.Id == adult.SchoolId).For(adult, 5.6),
                mind.Destination);
            Need(adult.MotorStepCount > motorBefore && adult.ActualSpeed > 0,
                "adult did not turn and move through physical motor");
            var search = new BluegillThreatMind();
            search.Step(open, default, 5, false);
            search.Step(sight, default, 5.5, false);
            search.Step(concealed, default, 6.1, false);
            Need(search.Mode == BluegillThreatMode.Search && search.Destination is not null,
                "concealment failed to start last-seen search");
            search.Step(concealed, default, 8.4, false);
            Need(search.Mode == BluegillThreatMode.Patrol && search.Destination is null &&
                search.Disengagements == 1, "adult kept omniscient target memory");
            var realPlant = plants.FirstOrDefault(p => p.VisibleBounds.Size.Length() > .1f);
            Need(realPlant.VisibleBounds.Size.Length() > .1f &&
                SoftCoverTransmission.Along(realPlant.VisibleBounds.Position - Vector3.One,
                    realPlant.VisibleBounds.End + Vector3.One, new[] { realPlant }, .2f) < 1f,
                "installed Pond foliage does not attenuate sight");

            // The diagnostic places a thin solid between the body and mouth, like a log lip.
            fish.GlobalPosition = adult.MouthPosition + -adult.GlobalBasis.Z * .005f;
            session.SetHatchSecondsForCheck(5.9);
            sensor.Reset();
            sight = sensor.Observe(adult.GlobalPosition - adult.GlobalBasis.Z * .08f,
                -adult.GlobalBasis.Z, new SightTarget(fish.GlobalPosition,
                    session.Growth.ColliderRadius, 1f), water, 5.9, false);
            mind.Step(sight, default, 5.9, false);
            Need(mind.StrikeReady, "adult was not ready after escape window");
            var barrier = new StaticBody3D { Name = "BluegillLogBarrierCheck", CollisionLayer = 1 };
            main.AddChild(barrier);
            barrier.GlobalPosition = adult.GlobalPosition.Lerp(adult.MouthPosition, .55f);
            barrier.LookAt(adult.MouthPosition, Vector3.Up);
            barrier.AddChild(new CollisionShape3D { Shape = new BoxShape3D {
                Size = new Vector3(.35f, .35f, .025f) } });
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            using (var ray = PhysicsRayQueryParameters3D.Create(adult.GlobalPosition,
                adult.MouthPosition, 1, new Godot.Collections.Array<Rid> { adult.GetRid(), fish.GetRid() }))
                Need(main.GetWorld3D().DirectSpaceState.IntersectRay(ray).Count > 0,
                    "solid barrier did not intersect strike line");
            Need(!session.TryBluegillCatch(adult, session.Growth.Identity) &&
                session.CatchEvent is null, "adult struck through solid log barrier");
            barrier.Free();
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(!session.TryBluegillCatch(adult, "old-bass-life"),
                "old Bass identity accepted");
            Need(session.TryBluegillCatch(adult, session.Growth.Identity),
                "clear facing mouth contact did not catch tiny Bass");
            var caught = session.CatchEvent ?? throw new InvalidOperationException("catch event missing");
            Need(caught.BassId == session.Growth.Identity &&
                caught.PredatorId == adult.StableId && tree.Paused &&
                !session.TryBluegillCatch(adult, session.Growth.Identity) &&
                session.CatchEvent == caught,
                "catch event duplicated or failed terminal pause");
            GD.Print($"P6_BLUEGILL_CATCH_TRACE event={caught.Id} " +
                $"predator={adult.StableId} prey={caught.BassId} " +
                $"seconds={caught.GameSeconds:F2} duplicate=blocked");
            _caughtLife = session.Growth.Identity;
            _caughtEvent = caught.Id;
            fish.GlobalTransform = originalFish;
            session.Retry();
        }
        catch (Exception error) { Fail(tree, error); }
    }

    private static void Need(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }

    private static bool TryActualPlantConcealment(Main main, PondBassSession session,
        PreyActor adult, FishController fish, SoftSightCover[] plants,
        WorldSightSensor sensor, SightEnvironment water, out SightEvidence concealed)
    {
        concealed = default;
        var adultBefore = adult.GlobalTransform;
        var fishBefore = fish.GlobalTransform;
        var bed = main.GetNode<SeabedSurface>("Habitat/Seabed");
        try
        {
            foreach (var plant in plants.OrderByDescending(p => p.VisibleBounds.Size.Length()))
            {
                var center = plant.VisibleBounds.GetCenter();
                foreach (var axis in new[] { Vector3.Right, Vector3.Forward })
                {
                    var half = axis == Vector3.Right ? plant.VisibleBounds.Size.X * .5f :
                        plant.VisibleBounds.Size.Z * .5f;
                    if (half < .12f || half > 1.1f) continue;
                    var from = center - axis * (half + .30f);
                    var to = center + axis * (half + .30f);
                    var y = center.Y;
                    if (y < bed.GetHeightAt(from.X, from.Z) + .2f ||
                        y < bed.GetHeightAt(to.X, to.Z) + .2f ||
                        y > fish.PondSurfaceHeightAt(from.X, from.Z) - .2f ||
                        y > fish.PondSurfaceHeightAt(to.X, to.Z) - .2f) continue;
                    from.Y = to.Y = y;
                    adult.GlobalPosition = from;
                    fish.GlobalPosition = to;
                    adult.LookAt(to, Vector3.Up);
                    sensor.Reset();
                    var covered = sensor.Observe(from - adult.GlobalBasis.Z * .08f,
                        -adult.GlobalBasis.Z, new SightTarget(to, session.Growth.ColliderRadius, 1f),
                        water, 5, false);
                    if (covered.Detected || covered.ClearRays == 0 || covered.SoftCover >= .35f)
                        continue;
                    GD.Print($"P6_BLUEGILL_COVER plant={plant.FafId} soft={covered.SoftCover:F3} " +
                        $"rays={covered.ClearRays} adult={from} bass={to}");
                    concealed = covered;
                    return true;
                }
            }
            return false;
        }
        finally
        {
            adult.GlobalTransform = adultBefore;
            fish.GlobalTransform = fishBefore;
        }
    }
    private static void Fail(SceneTree tree, Exception error)
    { GD.PushError("P6_BLUEGILL_THREAT_CHECK_FAILED " + error); tree.Paused = false; tree.Quit(1); }
}
