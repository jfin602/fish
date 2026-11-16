#nullable enable
using Godot;
using System;
using System.Linq;

namespace Fish;

/// <summary>Normal Pond Bass route, hatch, stage, feeding and ephemeral retry integration.</summary>
public static class P6PondEntryChecks
{
    private static string? _firstIdentity;
    private static int _firstNutrition;
    private static bool _routePassed;

    public static async void RunRoute(WorldSelect worldSelect)
    {
        var tree = worldSelect.GetTree();
        try
        {
            (worldSelect.FindChild("World_pond", true, false) as Button)!
                .EmitSignal(BaseButton.SignalName.Pressed);
            (worldSelect.FindChild("ChooseWorldButton", true, false) as Button)!
                .EmitSignal(BaseButton.SignalName.Pressed);
            for (var i = 0; i < 3; i++)
                await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            var select = tree.CurrentScene as CharacterSelect;
            Need(select is not null && WorldSession.SelectedWorldId == WorldCatalog.PondId,
                "World Select did not enter freshwater Character Select");
            var bass = select!.FindChildren("Character_*", "Button", true, false)
                .OfType<Button>().Single(button => button.Text.Contains("Bass", StringComparison.Ordinal));
            Need(!bass.Disabled, "Bass choice unavailable");
            bass.EmitSignal(BaseButton.SignalName.Pressed);
            _routePassed = true;
            (select.FindChild("EnterWorldButton", true, false) as Button)!
                .EmitSignal(BaseButton.SignalName.Pressed);
            for (var i = 0; i < 3; i++)
                await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            Need(tree.CurrentScene is Main main &&
                DemoSelection.CharacterId == PikeEncounterAssets.BassId &&
                main.GetNodeOrNull<PondBassSession>("PondBassSession") is not null,
                "Bass menu choice did not enter the normal Pond session");
        }
        catch (Exception error)
        {
            GD.PushError("P6_POND_ROUTE_CHECK_FAILED " + error);
            tree.Quit(1);
        }
    }

    public static async void Run(PondBassSession session)
    {
        var tree = session.GetTree();
        try
        {
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            if (_firstIdentity is not null)
            {
                Need(session.Growth.Identity != _firstIdentity &&
                    session.Growth.Stage == "egg" && session.Feeding.Nutrition == 0 &&
                    session.Feeding.Events.Count == 0 && !session.Caught &&
                    session.FoodActors.All(food => food.Available) &&
                    session.PreyActors.All(prey => prey.IsAlive) &&
                    WorldSession.ActiveManifest?.WorldId == WorldCatalog.Resolve(WorldCatalog.PondId).Bake?.WorldId,
                    "new life retained the prior session or lost the pinned Pond");
                GD.Print($"P6_POND_ENTRY_OK world={WorldSession.ActiveManifest!.WorldId} " +
                    $"seed={WorldSession.ActiveManifest.Seed} former_nutrition={_firstNutrition} " +
                    $"stage={session.Growth.Stage} food={session.FoodActors.Count} " +
                    $"prey={session.PreyActors.Count} ambient=" +
                    session.GetParent<Main>().GetNode<FafAmbientPondLife>("Habitat/PondAmbientLife").Population.Count);
                tree.Quit();
                return;
            }
            var manifest = WorldSession.ActiveManifest;
            var pinned = WorldCatalog.Resolve(WorldCatalog.PondId).Bake;
            var main = session.GetParent<Main>();
            var fish = main.GetNode<FishController>("Fish");
            var ambient = main.GetNode<FafAmbientPondLife>("Habitat/PondAmbientLife");
            Need(_routePassed && WorldCatalog.All.Count == 2 && manifest is not null && pinned is not null &&
                manifest.WorldId == pinned.WorldId && manifest.Seed == pinned.Seed &&
                WorldSession.SelectedWorldId == WorldCatalog.PondId &&
                DemoSelection.CharacterId == PikeEncounterAssets.BassId &&
                PondBassSession.IsSelected(WorldCatalog.PondId, PikeEncounterAssets.BassId) &&
                !PondBassSession.IsSelected(WorldCatalog.OceanId, PikeEncounterAssets.BassId) &&
                !PondBassSession.IsSelected(WorldCatalog.PondId, PikeEncounterAssets.BluegillId) &&
                !PondBassSession.IsSelected(WorldCatalog.PondId, null) &&
                main.GetNodeOrNull<PikeEncounter>("PikeEncounter") is null,
                "Bass route or world isolation invalid");
            Need(ambient.Population.Count == 11 &&
                ambient.Population.All(info => info.Id != PikeEncounterAssets.BluegillId) &&
                ambient.Population.Any(info => info.Id == "FAF-CREATURE-001") &&
                ambient.Population.Any(info => info.Id == "FAF-CREATURE-002") &&
                session.PreyActors.Count == 6 && session.FoodActors.Count == 16 &&
                session.PreyActors.All(prey => prey.SpeciesId == PikeEncounterAssets.BluegillId) &&
                session.FoodActors.Select(food => food.StableId)
                    .Concat(session.PreyActors.Select(prey => prey.StableId)).Distinct().Count() == 22,
                "ambient/ecological roster or stable IDs duplicated");
            var identity = session.Growth.Identity;
            Need(session.IntroActive && session.Growth.Stage == "egg" &&
                !fish.Visible && fish.GetNode<CollisionShape3D>("Collision").Disabled &&
                session.Feeding.Events.Count == 0 &&
                session.NestPosition.Y > main.GetNode<SeabedSurface>("Habitat/Seabed")
                    .GetHeightAt(session.NestPosition.X, session.NestPosition.Z) &&
                fish.PondSurfaceHeightAt(session.NestPosition.X, session.NestPosition.Z) -
                    session.NestPosition.Y is > .7f and < 1.7f,
                "egg or shallow nest invalid");
            var egg = session.GetNode<Node3D>("BassEggVisual");
            Need(egg.GlobalBasis.Z.Normalized().Dot(
                (fish.GlobalPosition - egg.GlobalPosition).Normalized()) > .99f &&
                Mathf.Abs(BassHatchPresentation.ShellBounds(egg,
                    egg.GlobalBasis.Z.Normalized()).Bottom -
                    main.GetNode<SeabedSurface>("Habitat/Seabed")
                        .GetHeightAt(session.NestPosition.X, session.NestPosition.Z)) < .02f,
                "normal Pond shell opening or bed contact invalid");
            var shell = main.GetNode<PlayShell>("PlayShell");
            (shell.FindChild("OptionsButton", true, false) as Button)!
                .EmitSignal(BaseButton.SignalName.Pressed);
            Need(tree.Paused && session.IntroActive && session.GameSeconds == 0,
                "egg pause advanced life time");
            session.SkipIntro();
            Need(session.IntroActive && session.Growth.Stage == "egg",
                "paused skip changed stage");
            (shell.FindChild("ResumeButton", true, false) as Button)!
                .EmitSignal(BaseButton.SignalName.Pressed);
            Need(!tree.Paused && session.IntroActive, "intro did not resume");
            session.SkipIntro();
            session.SkipIntro();
            for (var i = 0; i < 300 && session.IntroActive; i++)
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Need(!session.IntroActive && session.Growth.Stage == "larva" &&
                session.Growth.Identity == identity && fish.Visible &&
                !fish.GetNode<CollisionShape3D>("Collision").Disabled &&
                Mathf.Abs(fish.BodyRadius - session.Growth.Body.Radius) < .0001f &&
                fish.EncounterBoostEnabled && session.GetNodeOrNull<Camera3D>("HatchCamera") is not null,
                "skip did not hand off one real larval Bass");
            session.SetPhysicsProcess(false);
            fish.SetPhysicsProcess(false);
            var first = session.FoodActors.First(food => food.Kind == FeedingLedger.MicrofaunaId);
            void Place(PondFoodActor food, float reach) =>
                food.Relocate(fish.MouthPosition + fish.MouthForward * reach, session.GameSeconds);
            Place(first, .012f);
            Need(!session.TryConsumeFood(first).Consumed && session.Feeding.Nutrition == 0,
                "yolk gate failed");
            session.SetHatchSecondsForCheck(12.1);
            Need(session.TryConsumeFood(first).Consumed &&
                !session.TryConsumeFood(first).Consumed && session.Feeding.Nutrition == 1 &&
                session.Reticle.ConfirmedFeeds == 1,
                "larval contact/once-only ledger/reticle failed");
            foreach (var food in session.FoodActors.Where(food =>
                         food.Kind == FeedingLedger.MicrofaunaId && food.Available).Take(5))
            { Place(food, .012f); Need(session.TryConsumeFood(food).Consumed, "larval meal missed"); }
            Need(session.Growth.Stage == "juvenile" && session.Growth.Identity == identity &&
                Mathf.Abs(fish.BodyRadius - session.Growth.Body.Radius) < .0001f,
                "juvenile growth lost identity or fitted body");
            var bed = main.GetNode<SeabedSurface>("Habitat/Seabed");
            var toward = (bed.Basin.DeepCore - new Vector2(session.NestPosition.X,
                session.NestPosition.Z)).Normalized();
            var open = session.NestPosition + new Vector3(toward.X, 0, toward.Y) * 5f;
            open.Y = Mathf.Min(fish.PondSurfaceHeightAt(open.X, open.Z) - .45f,
                bed.GetHeightAt(open.X, open.Z) + .8f);
            fish.GlobalPosition = open;
            fish.GetNode<FollowCamera>("CameraRig").SnapToFish();
            foreach (var food in session.FoodActors.Where(food =>
                         food.Kind == FeedingLedger.InsectId && food.Available).Take(4))
            { Place(food, .025f); Need(session.TryConsumeFood(food).Consumed, "juvenile nymph missed"); }
            Need(session.Growth.Stage == "subadult" && session.Growth.Identity == identity,
                "subadult growth failed");
            foreach (var prey in session.PreyActors.Where(prey => prey.Stage == "larva").Take(3))
            {
                var radius = ((SphereShape3D)prey.GetNode<CollisionShape3D>("Collision").Shape).Radius;
                prey.GlobalPosition = fish.MouthPosition + fish.MouthForward * radius;
                Need(session.TryConsumePrey(prey).Consumed &&
                    !session.TryConsumePrey(prey).Consumed, "real Bluegill contact duplicated or missed");
            }
            if (session.Growth.Stage != "adult")
            {
                var core = bed.Basin.DeepCore;
                for (var i = 0; i < 24 && session.Growth.Stage != "adult"; i++)
                {
                    var angle = i * 2.399f;
                    var x = core.X + Mathf.Cos(angle) * (2f + i * .5f);
                    var z = core.Y + Mathf.Sin(angle) * (2f + i * .5f);
                    fish.GlobalPosition = new Vector3(x,
                        Mathf.Min(fish.PondSurfaceHeightAt(x, z) - 1.3f,
                            bed.GetHeightAt(x, z) + 2f), z);
                    session.TryAdvanceGrowth();
                }
            }
            Need(session.Growth.Stage == "adult" && session.Growth.Identity == identity &&
                session.Feeding.Nutrition == 26 &&
                session.Feeding.Events.Count is >= 13 and <= 14 &&
                session.Reticle.ConfirmedFeeds == 13 &&
                Mathf.Abs(fish.BodyRadius - session.Growth.Body.Radius) < .0001f,
                "adult progression or feed count invalid");
            fish.SetPhysicsProcess(true);
            session.SetPhysicsProcess(true);
            _firstIdentity = identity;
            _firstNutrition = session.Feeding.Nutrition;
            session.Catch("p6-check-contact");
            Need(session.Caught && tree.Paused &&
                !fish.GetNode<Node3D>("PlayerVisual").Visible &&
                fish.CollisionLayer == 0 && !fish.IsPhysicsProcessing(),
                "session-local caught state failed");
            session.Retry();
        }
        catch (Exception error)
        {
            GD.PushError("P6_POND_ENTRY_CHECK_FAILED " + error);
            tree.Quit(1);
        }
    }

    private static void Need(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
