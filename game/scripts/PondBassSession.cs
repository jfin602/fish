#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

/// <summary>One ephemeral Bass life in the independently baked normal Pond.</summary>
public partial class PondBassSession : Node3D
{
    private readonly FishController _fish;
    private readonly PlayShell _shell;
    private readonly SeabedSurface _bed;
    private readonly FeedingLedger _feeding = new();
    private readonly List<PondFoodActor> _food = new();
    private PondFoodPatches _patches = null!;
    private readonly List<PreyActor> _prey = new();
    private readonly List<BluegillSchool> _preySchools = new();
    private readonly List<WorldSightSensor> _preySight = new();
    private readonly List<MovementDisturbanceField> _preyCues = new();
    private readonly List<BluegillThreatMind> _threats = new();
    private PondTurbidityField _turbidity = null!;
    private PondCatchEvent? _catchEvent;
    private PondGrowthState _growth = null!;
    private SwimTuning _baseline = null!;
    private FeedingReticle _reticle = null!;
    private Node3D? _eggVisual;
    private AnimationPlayer? _eggAnimation;
    private Camera3D? _introCamera;
    private Label _status = null!;
    private ProgressBar _boost = null!;
    private Button _skip = null!;
    private Control _caughtMenu = null!;
    private AudioStreamPlayer3D _smallFeedSound = null!;
    private AudioStreamPlayer3D _fishFeedSound = null!;
    private Vector3 _nest;
    private Vector3 _larvaSpawn;
    private Vector3 _emergenceStart;
    private Transform3D _introCameraStart;
    private double _introSeconds;
    private double _emergenceAt;
    private double _cameraPanAt;
    private double _introEndAt;
    private double _gameSeconds;
    private double _hatchSeconds;
    private double _feedbackUntil;
    private string _feedback = "";
    private bool _introActive;
    private bool _emerged;
    private bool _skipRequested;
    private bool _hatchClipStarted;
    private bool _shellEaten;
    private bool _caught;
    private bool _failed;
    private bool _growthBlocked;
    private bool _transitioning;

    public static bool IsSelected(string worldId, string? speciesId) =>
        worldId == WorldCatalog.PondId && speciesId == PikeEncounterAssets.BassId;

    public PondBassSession(FishController fish, PlayShell shell, SeabedSurface bed)
    {
        _fish = fish;
        _shell = shell;
        _bed = bed;
        Name = "PondBassSession";
        ProcessPriority = 10;
    }

    public PondGrowthState Growth => _growth;
    public FeedingLedger Feeding => _feeding;
    public IReadOnlyList<PondFoodActor> FoodActors => _food;
    public IReadOnlyList<PondFoodPatchSite> FoodPatchSites => _patches.Sites;
    public int AvailableFoodCount => _patches.AvailableCount;
    public IReadOnlyList<PreyActor> PreyActors => _prey;
    public IReadOnlyList<BluegillSchool> BluegillSchools => _preySchools.Distinct().ToArray();
    internal IReadOnlyList<BluegillThreatMind> BluegillThreats => _threats;
    public PondCatchEvent? CatchEvent => _catchEvent;
    public bool IntroActive => _introActive;
    public bool Caught => _caught;
    public Vector3 NestPosition => _nest;
    public double GameSeconds => _gameSeconds;
    public double HatchSeconds => _hatchSeconds;
    public FeedingReticle Reticle => _reticle;
    public bool ReadyForChecks => !_failed && _growth is not null && _reticle is not null;

    internal void SetHatchSecondsForCheck(double seconds)
    {
        if (!OS.GetCmdlineUserArgs().Any(arg => arg is "--p6-pond-entry-check" or
            "--p6-food-check" or "--p6-bluegill-threat-check"))
            throw new InvalidOperationException("P6_POND_CHECK_CLOCK_FORBIDDEN");
        _hatchSeconds = seconds;
        _gameSeconds = seconds;
    }

    internal void AdvanceFoodForCheck(double seconds)
    {
        if (!OS.GetCmdlineUserArgs().Contains("--p6-food-check"))
            throw new InvalidOperationException("P6_FOOD_CHECK_CLOCK_FORBIDDEN");
        if (GetTree().Paused) return;
        _gameSeconds += seconds;
        _hatchSeconds += seconds;
        _patches.Tick(_gameSeconds);
    }

    internal bool CanReachFoodForCheck(PondFoodActor food)
    {
        if (!OS.GetCmdlineUserArgs().Contains("--p6-food-check"))
            throw new InvalidOperationException("P6_FOOD_CHECK_REACH_FORBIDDEN");
        return _food.Contains(food) && food.Available && FoodReachable(food);
    }

    public override void _Ready()
    {
        try
        {
            var manifest = WorldSession.ActiveManifest;
            var pinned = WorldCatalog.Resolve(WorldCatalog.PondId).Bake;
            if (!IsSelected(WorldSession.SelectedWorldId, DemoSelection.CharacterId) ||
                WorldSession.ActiveHabitat != GetParent<Node3D>().GetNode<Node3D>("Habitat") ||
                manifest is null || pinned is null || manifest.WorldId != pinned.WorldId ||
                manifest.Seed != pinned.Seed || !_bed.Basin.IsPond)
                throw new InvalidOperationException("POND_BASS_ROUTE_OR_BAKE_INVALID");
            _growth = new PondGrowthState($"pond-{manifest.WorldId}:bass-life:{Guid.NewGuid():N}");
            _baseline = _fish.Tuning;
            _fish.CollisionLayer = 2;
            _fish.CollisionMask = 1;
            _fish.EncounterBoostEnabled = true;
            _fish.SetEncounterHatchLocked(true);
            _fish.ClearAndDisarmInput();
            FindNest();
            _fish.GlobalPosition = _larvaSpawn;
            _fish.GetNode<FollowCamera>("CameraRig").SnapToFish();
            BuildInterface();
            _shell.ConfigureSessionRetry(Retry);
            _smallFeedSound = FeedSound("SmallFeedSound", -24f);
            _fishFeedSound = FeedSound("FishFeedSound", -18f);
            _reticle = FeedingReticle.Attach(this, _fish,
                _fish.GetNode<Camera3D>("CameraRig/Camera3D"),
                () => !_introActive && !_caught && !_failed && !_transitioning &&
                    !_shell.PauseMenuVisible, GuideFoodCandidates);
            AddFood();
            _turbidity = new PondTurbidityField(_bed.Basin, _fish.PondWaterlineHeight);
            AddPrey();
            StartIntro();
            GD.Print($"P6_POND_BASS_READY world={manifest.WorldId} seed={manifest.Seed} " +
                $"identity={_growth.Identity} stage={_growth.Stage} nest={_nest} " +
                $"food={_food.Count} prey={_prey.Count}");
        }
        catch (Exception error)
        {
            _failed = true;
            _fish.ClearAndDisarmInput();
            GD.PushError("P6_POND_ENTRY_FAILED " + error);
            GetTree().CallDeferred(SceneTree.MethodName.Quit, 1);
        }
    }

    private void FindNest()
    {
        var basin = _bed.Basin;
        var spawn = WorldSession.ActiveHabitat!.GetNode<Marker3D>("Spawn").GlobalPosition;
        var best = float.PositiveInfinity;
        for (var i = 0; i < basin.ShorePointCount; i++)
        {
            var shore = basin.ShorePoint(i);
            var toward = (basin.DeepCore - shore).Normalized();
            foreach (var offset in new[] { 1.8f, 2.3f, 2.8f, 3.3f })
            {
                var point = shore + toward * offset;
                var floor = _bed.GetHeightAt(point.X, point.Y);
                var surface = _fish.PondSurfaceHeightAt(point.X, point.Y);
                var depth = surface - floor;
                if (depth < .75f || depth > 1.6f ||
                    basin.DistanceToShore(point.X, point.Y) < 1.4f) continue;
                var site = new Vector3(point.X, floor + .025f, point.Y);
                var exit = site + Vector3.Up * .12f +
                    new Vector3(toward.X, 0f, toward.Y) * .16f;
                if (exit.Y > _fish.PondSurfaceLimitAt(exit.X, exit.Z) - .08f ||
                    !ClearSphere(exit, .09f) || !ClearSphere(site + Vector3.Up * .12f, .12f)) continue;
                var score = site.DistanceTo(spawn) + i * .0001f;
                if (score >= best) continue;
                best = score;
                _nest = site;
                _larvaSpawn = exit;
            }
        }
        if (!float.IsFinite(best)) throw new InvalidOperationException("POND_BASS_NEST_UNAVAILABLE");
        var nest = new Node3D { Name = "BassNest" };
        AddChild(nest);
        nest.GlobalPosition = _nest;
        var substrate = new MeshInstance3D { Name = "NestSubstrate",
            Mesh = new CylinderMesh { TopRadius = .4f, BottomRadius = .5f, Height = .012f },
            MaterialOverride = new StandardMaterial3D {
                AlbedoColor = new Color(.22f, .19f, .12f), Roughness = 1f } };
        nest.AddChild(substrate);
    }

    private bool ClearSphere(Vector3 at, float radius)
    {
        using var shape = new SphereShape3D { Radius = radius };
        using var query = new PhysicsShapeQueryParameters3D {
            Shape = shape, Transform = new Transform3D(Basis.Identity, at), CollisionMask = 1,
            Exclude = new Godot.Collections.Array<Rid> { _fish.GetRid() } };
        return GetWorld3D().DirectSpaceState.IntersectShape(query, 1).Count == 0;
    }

    private void AddFood()
    {
        var manifest = WorldSession.ActiveManifest!;
        var habitat = WorldSession.ActiveHabitat!;
        var placements = habitat.GetNode<PondFafHabitat>("PondFafEnvironment").Placements;
        _patches = new PondFoodPatches(this, _fish, _bed, _food,
            manifest.WorldId, manifest.Seed, _nest, placements);
    }

    private void AddPrey()
    {
        var worldId = WorldSession.ActiveManifest!.WorldId;
        var habitat = WorldSession.ActiveHabitat!;
        var cover = PondSoftCover.Near(habitat.GetNode<PondFafHabitat>("PondFafEnvironment").Placements,
            _nest, 18f);
        var basinDirection = (_bed.Basin.DeepCore - new Vector2(_nest.X, _nest.Z)).Normalized();
        var toward = new Vector3(basinDirection.X, 0f, basinDirection.Y);
        var side = new Vector3(-toward.Z, 0f, toward.X);
        var adultCenter = _nest + toward * 4.7f + side * 1.5f + Vector3.Up * .72f;
        var youngSchool = new BluegillSchool("pond-nursery", _nest + toward * 2.6f + Vector3.Up * .5f, .8f);
        var adultSchool = new BluegillSchool("pond-shallow-adults", adultCenter, 2.4f);
        var bassAdultLength = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "adult")!.Dimensions[1];
        for (var i = 0; i < 8; i++)
        {
            var stage = i < 4 ? "larva" : i < 6 ? "juvenile" : "adult";
            var school = i < 6 ? youngSchool : adultSchool;
            if (!SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.PondId,
                    PikeEncounterAssets.BluegillId, stage, SpeciesRole.Ecological,
                    out var asset, out var diagnostic) || asset is null)
                throw new InvalidOperationException("POND_BASS_PREY_STAGE_UNAVAILABLE " + diagnostic);
            var entry = new CharacterEntry(asset.Id, asset.Name, "Pond prey", "Registered Bluegill",
                asset.ModelPath, 1f / bassAdultLength, asset.FacingDegrees, 30f,
                asset.Clip("Glide"), asset.Clip("Swim"), asset.Clip("SwimFast"),
                WorldHabitat.Freshwater, false, false, true, true);
            if (!CharacterCatalog.TryInstantiate(entry, out var model, out var animation, out diagnostic))
                throw new InvalidOperationException("POND_BASS_PREY_MODEL_UNAVAILABLE " + diagnostic);
            var at = Vector3.Zero;
            var found = false;
            for (var attempt = 0; attempt < 40; attempt++)
            {
                var angle = i * Mathf.Tau / 8f + attempt * 2.399f;
                var candidate = stage == "adult"
                    ? adultCenter + toward * (attempt * .13f) +
                        side * Mathf.Sin(angle) * (1.2f + attempt * .05f)
                    : _nest + toward * (2.2f + i % 3 * .7f + attempt * .12f) +
                        side * Mathf.Sin(angle) * (1.1f + attempt * .045f);
                candidate.Y = _bed.GetHeightAt(candidate.X, candidate.Z) +
                    (stage == "larva" ? .38f : .65f);
                if (candidate.Y > _fish.PondSurfaceHeightAt(candidate.X, candidate.Z) - .3f ||
                    !ClearSphere(candidate, stage == "larva" ? .04f : stage == "adult" ? .13f : .08f) ||
                    _prey.Any(existing => existing.GlobalPosition.DistanceTo(candidate) < .18f))
                    continue;
                at = candidate;
                found = true;
                break;
            }
            if (!found)
                throw new InvalidOperationException($"POND_BASS_PREY_SITE_BLOCKED stage={stage} index={i}");
            var body = new PreyActor { Name = "PondBluegill" + i,
                CollisionLayer = 2, CollisionMask = 1,
                MotionMode = CharacterBody3D.MotionModeEnum.Floating };
            body.SetIdentity($"{worldId}:bluegill:{stage}:{i}", asset.Id, stage,
                asset.Dimensions[1]);
            AddChild(body);
            body.GlobalPosition = at;
            body.AddChild(new CollisionShape3D { Name = "Collision",
                Shape = new SphereShape3D { Radius = stage == "larva" ? .006f : stage == "adult" ? .12f : .03f } });
            body.AddChild(model!);
            model!.Name = "PreyVisual";
            model.Transform = entry.PresentationTransform;
            body.ConfigureHabitat(_bed.GetHeightAt, _fish.PondSurfaceHeightAt);
            var localCover = cover.OrderBy(plant => plant.VisibleBounds.GetCenter().DistanceSquaredTo(at))
                .Take(128).ToArray();
            body.Configure(localCover, animation!, entry.SwimClip, entry.FastClip);
            school.Add(body);
            _prey.Add(body);
            _preySchools.Add(school);
            _preySight.Add(new WorldSightSensor(GetWorld3D().DirectSpaceState, localCover,
                body.GetRid(), _fish.GetRid(), range: stage == "adult" ? 3.2f : 7f,
                acuity: stage == "adult" ? 3.2f : 1.7f));
            _preyCues.Add(new MovementDisturbanceField(GetWorld3D().DirectSpaceState,
                localCover, body.GetRid(), _fish.GetRid(), speedReference: .6f));
            _threats.Add(new BluegillThreatMind());
        }
    }

    private void BuildInterface()
    {
        var canvas = new CanvasLayer { Name = "PondBassHud", Layer = 10 };
        AddChild(canvas);
        _status = new Label { Name = "BassStatus", Position = new Vector2(18, 82),
            CustomMinimumSize = new Vector2(650, 42),
            MouseFilter = Control.MouseFilterEnum.Ignore };
        canvas.AddChild(_status);
        _boost = new ProgressBar { Name = "BassBoost", Position = new Vector2(18, 126),
            CustomMinimumSize = new Vector2(180, 12), MaxValue = 100, Value = 100,
            ShowPercentage = false, MouseFilter = Control.MouseFilterEnum.Ignore };
        canvas.AddChild(_boost);
        _skip = new Button { Name = "SkipBassHatch", Text = "Skip Intro",
            Position = new Vector2(18, 148), Visible = false };
        _skip.Pressed += SkipIntro;
        canvas.AddChild(_skip);
        var caughtLayer = new CanvasLayer { Name = "CaughtLayer", Layer = 12,
            ProcessMode = ProcessModeEnum.Always };
        AddChild(caughtLayer);
        _caughtMenu = new CenterContainer { Name = "CaughtMenu", Visible = false,
            ProcessMode = ProcessModeEnum.Always, MouseFilter = Control.MouseFilterEnum.Pass };
        _caughtMenu.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        caughtLayer.AddChild(_caughtMenu);
        var actions = new VBoxContainer { CustomMinimumSize = new Vector2(320, 0) };
        _caughtMenu.AddChild(actions);
        actions.AddChild(new Label { Text = "Caught — this Bass life ended", HorizontalAlignment = HorizontalAlignment.Center });
        var retry = new Button { Text = "Start a new Bass life" };
        retry.Pressed += Retry;
        actions.AddChild(retry);
        var back = new Button { Text = "Return to World Select" };
        back.Pressed += ReturnToWorldSelect;
        actions.AddChild(back);
    }

    private AudioStreamPlayer3D FeedSound(string name, float volumeDb)
    {
        var stream = ResourceLoader.Load<AudioStream>("res://assets/sfx/feeding_small.wav")
            ?? throw new InvalidOperationException("POND_BASS_FEED_SOUND_UNAVAILABLE");
        var player = new AudioStreamPlayer3D { Name = name, Stream = stream, Bus = "SFX",
            VolumeDb = volumeDb, MaxDistance = 12f };
        _fish.AddChild(player);
        return player;
    }

    private void StartIntro()
    {
        var egg = BassHatchPresentation.Create(this, _nest, _larvaSpawn, _bed.GetHeightAt);
        _eggVisual = egg.Visual;
        _eggAnimation = egg.Animation;
        _emergenceAt = .85 + egg.HatchLength * .55;
        _cameraPanAt = _emergenceAt + .25;
        _introEndAt = .85 + egg.HatchLength + 1.7;
        _introCamera = new Camera3D { Name = "HatchCamera", Near = .003f, Far = 200f,
            Fov = 42f, Current = true };
        AddChild(_introCamera);
        _introCamera.GlobalPosition = _eggVisual.GlobalPosition + new Vector3(.10f, .08f, .15f);
        _introCamera.LookAt(_eggVisual.GlobalPosition, Vector3.Up);
        _introCameraStart = _introCamera.GlobalTransform;
        _introActive = true;
        _skip.Visible = true;
        _fish.Visible = false;
        _fish.GetNode<Camera3D>("CameraRig/Camera3D").Current = false;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        _status.Text = "BASS NEST · yolk-fed egg · Enter or Skip Intro to hatch · Esc: pause";
        GD.Print($"BASS_HATCH_BEGIN identity={_growth.Identity} stage={_growth.Stage} nest={_nest}");
    }

    public override void _Process(double delta)
    {
        if (!_introActive || _failed || _caught || _transitioning || GetTree().Paused) return;
        _introSeconds += delta * (_skipRequested ? 4.0 : 1.0);
        if (!_hatchClipStarted && _introSeconds >= .85)
        {
            _eggAnimation!.Play(PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "egg")!.Clip("Hatch"));
            _eggAnimation.SpeedScale = _skipRequested ? 4f : 1f;
            _hatchClipStarted = true;
        }
        if (_hatchClipStarted && !_emerged && _introSeconds >= _emergenceAt)
            BeginEmergence();
        if (_emerged)
        {
            var fraction = Mathf.SmoothStep(0f, 1f, (float)Mathf.Clamp(
                (_introSeconds - _emergenceAt) / 1.15, 0, 1));
            _fish.GlobalPosition = _emergenceStart.Lerp(_larvaSpawn, fraction);
            var pan = Mathf.SmoothStep(0f, 1f, (float)Mathf.Clamp(
                (_introSeconds - _cameraPanAt) / (_introEndAt - _cameraPanAt), 0, 1));
            var fryCamera = _fish.GetNode<Camera3D>("CameraRig/Camera3D");
            _introCamera!.GlobalTransform = _introCameraStart.InterpolateWith(fryCamera.GlobalTransform, pan);
            _introCamera.Fov = Mathf.Lerp(42f, fryCamera.Fov, pan);
        }
        if (_emerged && _introSeconds >= _introEndAt) FinishIntro();
    }

    private void BeginEmergence()
    {
        _emerged = true;
        _growth.Hatch();
        if (!PlayerArt.TryReplacePondStage(_fish, _growth.Entry(_growth.Asset), out var diagnostic))
            throw new InvalidOperationException("POND_BASS_LARVA_VISUAL_UNAVAILABLE " + diagnostic);
        _fish.SetEncounterBassBody(_growth.Body);
        _fish.Tuning = _growth.StageTuning(_baseline);
        _fish.GetNode<WaterReaction>("WaterReaction").ConfigureFromPlayerVisual();
        var fryCamera = _fish.GetNode<FollowCamera>("CameraRig");
        fryCamera.ConfigureEncounterLarvaView(true);
        var opening = _eggVisual!.GlobalBasis.Z.Normalized();
        _fish.GlobalPosition = _eggVisual.GlobalPosition;
        _fish.LookAt(_fish.GlobalPosition + opening, Vector3.Up);
        var (_, lip) = BassHatchPresentation.ShellBounds(_eggVisual, opening);
        var rear = 0f;
        var visual = _fish.GetNode<Node3D>("PlayerVisual");
        foreach (var child in visual.FindChildren("*", "MeshInstance3D", true, false))
        {
            if (child is not MeshInstance3D { Mesh: not null } mesh) continue;
            var box = mesh.GetAabb();
            for (var x = 0; x <= 1; x++)
            for (var y = 0; y <= 1; y++)
            for (var z = 0; z <= 1; z++)
            {
                var corner = mesh.GlobalTransform *
                    (box.Position + box.Size * new Vector3(x, y, z));
                rear = Mathf.Max(rear, -((corner - _fish.GlobalPosition).Dot(opening)));
            }
        }
        _emergenceStart = _eggVisual.GlobalPosition + opening * (lip + rear + .003f);
        if ((_larvaSpawn - _emergenceStart).Dot(opening) <= .005f)
            throw new InvalidOperationException("POND_BASS_LARVA_EXIT_BLOCKED");
        _fish.GlobalPosition = _emergenceStart;
        fryCamera.SnapToFish();
        _fish.Visible = true;
        _status.Text = "BASS FRY · emerging from the egg · Esc: pause";
    }

    private void FinishIntro()
    {
        _fish.GlobalPosition = _larvaSpawn;
        _fish.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.On;
        _fish.ResetPhysicsInterpolation();
        _fish.GetNode<FollowCamera>("CameraRig").SnapToFish();
        var fryCamera = _fish.GetNode<Camera3D>("CameraRig/Camera3D");
        _introCamera!.GlobalTransform = fryCamera.GlobalTransform;
        _introCamera.Fov = fryCamera.Fov;
        _introCamera.Current = false;
        fryCamera.Current = true;
        _fish.SetEncounterHatchLocked(false);
        _fish.ClearAndDisarmInput();
        _introActive = false;
        _skip.Visible = false;
        Input.MouseMode = _fish.UsesMouseInput ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
        if (_fish.UsesMouseInput) _fish.ResetMouseCapture();
        GD.Print($"BASS_HATCH_COMPLETE identity={_growth.Identity} stage={_growth.Stage} " +
            $"nutrition={_feeding.Nutrition} position={_fish.GlobalPosition}");
    }

    public void SkipIntro()
    {
        if (_introActive && !_caught && !_failed && !GetTree().Paused)
        { _skipRequested = true; if (_hatchClipStarted) _eggAnimation!.SpeedScale = 4f; }
    }

    public override void _Input(InputEvent @event)
    {
        if (_introActive && !_caught && !_failed &&
            @event is InputEventKey { Pressed: true, Echo: false } key &&
            key.Keycode is Key.Enter or Key.KpEnter or Key.Space)
        { SkipIntro(); GetViewport().SetInputAsHandled(); }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_introActive || _caught || _failed || _transitioning || GetTree().Paused) return;
        _gameSeconds += delta;
        _hatchSeconds += delta;
        _patches.Tick(_gameSeconds);
        var sightWater = new SightEnvironment(WorldEnvironmentProfile.Pond,
            _fish.PondSurfaceHeightAt(_fish.GlobalPosition.X, _fish.GlobalPosition.Z),
            _turbidity.Sample, GetParent<Node3D>().GetNode<UnderwaterAtmosphere>("UnderwaterAtmosphere").Solar);
        for (var i = 0; i < _prey.Count; i++)
        {
            var prey = _prey[i];
            if (!prey.IsAlive) continue;
            _preyCues[i].Step(_fish.GlobalPosition, _fish.Velocity,
                _fish.PresentationEffort, _fish.PresentationAngularTurn,
                delta, _fish.MotorStepCount, false);
            var eligible = BluegillPredation.Eligible(prey.SpeciesId, prey.Stage, prey.Length,
                PondGrowthState.SpeciesId, _growth.Stage, _growth.Length);
            var sight = _preySight[i].Observe(prey.GlobalPosition - prey.GlobalBasis.Z * .08f,
                -prey.GlobalBasis.Z,
                new SightTarget(_fish.GlobalPosition, _growth.ColliderRadius, 1f),
                sightWater, _gameSeconds, false);
            var cue = _preyCues[i].Sense(prey.GlobalPosition, false);
            var school = _preySchools[i].For(prey, _gameSeconds);
            if (eligible)
            {
                _threats[i].Step(sight, cue, _gameSeconds, false);
                prey.Tick(default, default, _gameSeconds, (float)delta, false,
                    school, _threats[i].Destination);
                if (_threats[i].StrikeReady && TryBluegillCatch(prey, _growth.Identity)) return;
            }
            else
            {
                _threats[i].Reset();
                var flee = _growth.Stage != "egg" && _growth.Length > prey.Length * 1.2f;
                prey.Tick(flee ? sight : default, flee ? cue : default,
                    _gameSeconds, (float)delta, false, school);
            }
        }
        TryAutoFeed();
        TryAdvanceGrowth();
        _boost.Value = _fish.BoostEnergy * 100f;
        _status.Text = $"FRESHWATER POND · Bass {_growth.Stage} · nutrition {_feeding.Nutrition}/" +
            (_growth.NextThreshold == int.MaxValue ? "mature" : _growth.NextThreshold.ToString()) +
            (_growthBlocked ? " · grow in open water" : "") +
            (_gameSeconds < _feedbackUntil ? " · " + _feedback : "") +
            " · mouth contact: feed · Hold Space: boost · Esc: options";
    }

    public FeedResult TryAutoFeed()
    {
        if (_introActive || _caught || _failed || _transitioning || GetTree().Paused)
            return new(false, "session inactive", 0);
        var mouth = _fish.MouthPosition;
        if (!_shellEaten && _growth.Stage == "larva" && _eggVisual is not null && ShellContact(mouth))
        {
            var result = _feeding.TryConsumeEggShell(_growth.Identity + ":shell", true, _gameSeconds);
            if (result.Consumed)
            {
                _shellEaten = true;
                _eggVisual.QueueFree();
                _eggVisual = null;
                _smallFeedSound.Play();
                Feedback("Egg shell eaten");
            }
            return result;
        }
        var food = _food.Where(f => f.Available &&
                FeedingLedger.FoodAward(f.Kind, _growth.Stage, _hatchSeconds,
                    normalPondSurvival: true) > 0 &&
                FoodReachable(f)).OrderBy(f => f.GlobalPosition.DistanceTo(mouth)).FirstOrDefault();
        if (food is not null) return TryConsumeFood(food);
        var prey = _prey.Where(p => p.IsAlive && FeedingLedger.IsEligiblePrey(
                p.SpeciesId, p.Stage, p.Length, _growth.Stage, _growth.Length) &&
                PreyReachable(p)).OrderBy(p => p.GlobalPosition.DistanceTo(mouth)).FirstOrDefault();
        return prey is null ? new(false, "no eligible prey", 0) : TryConsumePrey(prey);
    }

    public FeedResult TryConsumeFood(PondFoodActor food)
    {
        if (_introActive || _caught || _failed || GetTree().Paused || !_food.Contains(food))
            return new(false, "session inactive or foreign food", 0);
        var result = _feeding.TryConsumeFood(food.StableId, food.Kind, _growth.Stage,
            food.Available && FoodReachable(food), _gameSeconds, _hatchSeconds,
            normalPondSurvival: true);
        if (result.Consumed)
        {
            food.Consume();
            _patches.Consumed(food, _gameSeconds);
            _smallFeedSound.Play();
            _reticle.ConfirmCommittedFeed();
            Feedback(food.Kind == FeedingLedger.MicrofaunaId ? "Zooplankton eaten" : "Aquatic nymph eaten");
            GD.Print($"P6_POND_FEED id={food.StableId} kind={food.Kind} nutrition={_feeding.Nutrition}");
            TryAdvanceGrowth();
        }
        return result;
    }

    public FeedResult TryConsumePrey(PreyActor prey)
    {
        if (_introActive || _caught || _failed || GetTree().Paused || !_prey.Contains(prey))
            return new(false, "session inactive or foreign prey", 0);
        var result = _feeding.TryConsume(new FeedCandidate(prey.StableId, prey.SpeciesId,
            prey.Stage, WorldCatalog.PondId, WorldSession.SelectedWorldId, prey.Length,
            _growth.Stage, _growth.Length, prey.IsAlive), PreyReachable(prey), _gameSeconds);
        if (result.Consumed)
        {
            prey.Consume();
            _fish.GetNode<Node3D>("PlayerVisual")
                .GetNode<CatalogFishVisualMotion>("SwimPresentation").TriggerFeedingCue();
            _fishFeedSound.Play();
            _reticle.ConfirmCommittedFeed();
            Feedback("Bluegill " + prey.Stage + " eaten");
            GD.Print($"P6_POND_FEED id={prey.StableId} kind=prey stage={prey.Stage} nutrition={_feeding.Nutrition}");
            TryAdvanceGrowth();
        }
        return result;
    }

    private void Feedback(string message) { _feedback = message; _feedbackUntil = _gameSeconds + 1.6; }

    private bool FoodReachable(PondFoodActor food)
    {
        var mouth = _fish.MouthPosition;
        var to = food.GlobalPosition - mouth;
        return to.Length() > .005f && to.Length() <= _growth.MouthRadius * 1.5f &&
            _fish.MouthForward.Dot(to.Normalized()) >= .65f && ClearPath(mouth, food.GlobalPosition);
    }

    private bool PreyReachable(PreyActor prey)
    {
        if (!prey.IsAlive) return false;
        var mouth = _fish.MouthPosition;
        var to = prey.GlobalPosition - mouth;
        var radius = ((SphereShape3D)prey.GetNode<CollisionShape3D>("Collision").Shape).Radius;
        return to.LengthSquared() > .000001f &&
            to.Length() <= radius + _growth.MouthRadius * 1.5f &&
            _fish.MouthForward.Dot((prey.GlobalPosition - _fish.GlobalPosition).Normalized()) >= .8f &&
            _fish.MouthForward.Dot(to.Normalized()) >= .35f &&
            ClearPath(mouth, prey.GlobalPosition, prey.GetRid());
    }

    private bool ShellContact(Vector3 mouth)
    {
        var forward = _fish.MouthForward;
        foreach (var found in _eggVisual!.FindChildren("*", "MeshInstance3D", true, false))
        {
            if (found is not MeshInstance3D { Mesh: not null } mesh || !mesh.Visible ||
                !mesh.Name.ToString().Contains("shell", StringComparison.OrdinalIgnoreCase)) continue;
            var bounds = mesh.GetAabb();
            var local = mesh.ToLocal(mouth);
            var corner = bounds.Position + bounds.Size;
            var nearest = mesh.ToGlobal(new Vector3(
                Mathf.Clamp(local.X, bounds.Position.X, corner.X),
                Mathf.Clamp(local.Y, bounds.Position.Y, corner.Y),
                Mathf.Clamp(local.Z, bounds.Position.Z, corner.Z)));
            var to = nearest - mouth;
            var fromBody = nearest - _fish.GlobalPosition;
            if (to.Length() <= _growth.MouthRadius * 1.5f &&
                fromBody.LengthSquared() > .000001f &&
                forward.Dot(fromBody.Normalized()) >= .65f &&
                (to.LengthSquared() < .000001f || forward.Dot(to.Normalized()) >= .35f) &&
                ClearPath(mouth, nearest)) return true;
        }
        return false;
    }

    private bool ClearPath(Vector3 from, Vector3 to, Rid target = default)
    {
        var exclude = new Godot.Collections.Array<Rid> { _fish.GetRid() };
        if (target.IsValid) exclude.Add(target);
        using var query = PhysicsRayQueryParameters3D.Create(from, to, 1, exclude);
        return GetWorld3D().DirectSpaceState.IntersectRay(query).Count == 0;
    }

    private IEnumerable<FoodAimCandidate> GuideFoodCandidates()
    {
        var camera = _fish.GetNode<Camera3D>("CameraRig/Camera3D");
        var mouth = _fish.MouthPosition;
        foreach (var food in _food)
        {
            var to = food.GlobalPosition - mouth;
            if (!food.Available || !food.IsVisibleInTree() ||
                _feeding.IsConsumed(food.StableId) ||
                FeedingLedger.FoodAward(food.Kind, _growth.Stage, _hatchSeconds,
                    normalPondSurvival: true) <= 0 ||
                to.LengthSquared() < .000025f || to.LengthSquared() > 1.44f ||
                _fish.MouthForward.Dot(to.Normalized()) < .35f ||
                !ClearPath(mouth, food.GlobalPosition) ||
                !ClearPath(camera.GlobalPosition, food.GlobalPosition)) continue;
            yield return new FoodAimCandidate(food.StableId, food.GlobalPosition);
        }
    }

    public bool TryAdvanceGrowth()
    {
        if (_introActive || _caught || _failed || GetTree().Paused ||
            _growth.Next(_feeding.Nutrition) is not { } next) return false;
        var nextBody = _growth.BodyForNext();
        if (_fish.GlobalPosition.Y > _fish.PondSurfaceHeightAt(
                _fish.GlobalPosition.X, _fish.GlobalPosition.Z) -
                FishController.PondCenterClearanceFraction * nextBody.Radius)
        { _growthBlocked = true; return false; }
        using var shape = new CapsuleShape3D { Radius = nextBody.Radius, Height = nextBody.Height };
        using var query = new PhysicsShapeQueryParameters3D {
            Shape = shape,
            Transform = new Transform3D(_fish.GlobalBasis * new Basis(Vector3.Right, Mathf.Pi * .5f),
                _fish.GlobalPosition + _fish.GlobalBasis.Z * nextBody.CenterZ),
            CollisionMask = _fish.CollisionMask,
            Exclude = new Godot.Collections.Array<Rid> { _fish.GetRid() },
        };
        if (GetWorld3D().DirectSpaceState.IntersectShape(query, 1).Count != 0)
        { _growthBlocked = true; return false; }
        if (!PlayerArt.TryReplacePondStage(_fish, _growth.Entry(next), out var diagnostic))
            throw new InvalidOperationException("POND_BASS_GROWTH_VISUAL_UNAVAILABLE " + diagnostic);
        _fish.SetEncounterBassBody(nextBody);
        _growth.Commit(next);
        _fish.Tuning = _growth.StageTuning(_baseline);
        _fish.GetNode<WaterReaction>("WaterReaction").ConfigureFromPlayerVisual();
        _growthBlocked = false;
        if (_growth.Stage == "juvenile")
            _fish.GetNode<FollowCamera>("CameraRig").ConfigureEncounterLarvaView(false);
        GD.Print($"P6_POND_GROWTH identity={_growth.Identity} stage={_growth.Stage} " +
            $"nutrition={_feeding.Nutrition} radius={nextBody.Radius:F4}");
        return true;
    }

    public void Catch(string sourceId)
    {
        if (_introActive || _caught || _failed || _transitioning || GetTree().Paused ||
            string.IsNullOrWhiteSpace(sourceId)) return;
        _caught = true;
        _fish.ClearAndDisarmInput();
        _fish.SetPhysicsProcess(false);
        _fish.CollisionLayer = 0;
        _fish.CollisionMask = 0;
        _fish.GetNode<Node3D>("PlayerVisual").Hide();
        _shell.SetSessionTerminal(true);
        GetTree().Paused = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        _caughtMenu.Visible = true;
        GD.Print($"P6_POND_CAUGHT identity={_growth.Identity} source={sourceId} seconds={_gameSeconds:F2}");
    }

    internal bool TryBluegillCatch(PreyActor predator, string bassIdentity)
    {
        var index = _prey.IndexOf(predator);
        if (index < 0 || _caught || _introActive || _failed || _transitioning || GetTree().Paused ||
            bassIdentity != _growth.Identity || !predator.IsAlive ||
            !_threats[index].StrikeReady ||
            !BluegillPredation.Eligible(predator.SpeciesId, predator.Stage, predator.Length,
                PondGrowthState.SpeciesId, _growth.Stage, _growth.Length) ||
            predator.GetNode<CollisionShape3D>("Collision").Shape is not SphereShape3D) return false;
        var mouth = predator.MouthPosition;
        var collision = _fish.GetNode<CollisionShape3D>("Collision");
        if (collision.Shape is not CapsuleShape3D capsule) return false;
        var local = collision.ToLocal(mouth);
        var spine = Mathf.Clamp(local.Y, -capsule.Height * .5f + capsule.Radius,
            capsule.Height * .5f - capsule.Radius);
        if (local.DistanceTo(new Vector3(0f, spine, 0f)) > capsule.Radius + .025f ||
            (-predator.GlobalBasis.Z).Dot((_fish.GlobalPosition - predator.GlobalPosition).Normalized()) < .72f ||
            !ClearPath(predator.GlobalPosition, mouth, predator.GetRid()) ||
            !ClearPath(mouth, _fish.GlobalPosition, predator.GetRid())) return false;
        _catchEvent = new PondCatchEvent(_growth.Identity + ":caught:" + predator.StableId,
            _growth.Identity, predator.StableId, _gameSeconds, mouth);
        GD.Print($"P6_BLUEGILL_CONTACT event={_catchEvent.Value.Id} predator={predator.StableId} " +
            $"prey={_growth.Identity} mouth={mouth} bass={_fish.GlobalPosition}");
        Catch(predator.StableId);
        return true;
    }

    public void Retry()
    {
        if (_transitioning) return;
        _transitioning = true;
        _fish.ClearAndDisarmInput();
        GetTree().Paused = false;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        var error = GetTree().ReloadCurrentScene();
        if (error != Error.Ok) GD.PushError($"P6_POND_RETRY_FAILED code={error}");
    }

    private void ReturnToWorldSelect()
    {
        if (_transitioning) return;
        _transitioning = true;
        _fish.ClearAndDisarmInput();
        GetTree().Paused = false;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        DemoSelection.Clear();
        WorldSession.Unload();
        var error = GetTree().ChangeSceneToFile("res://scenes/world_select.tscn");
        if (error != Error.Ok) GD.PushError($"P6_POND_RETURN_FAILED code={error}");
    }
}

public readonly record struct PondCatchEvent(string Id, string BassId, string PredatorId,
    double GameSeconds, Vector3 ContactPosition);
