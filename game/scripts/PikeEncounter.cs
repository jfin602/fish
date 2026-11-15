#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

public readonly record struct PikeSensoryEvidence(SightEvidence Sight, DisturbanceEvidence Disturbance);
public readonly record struct FishPredationEvent(string Id, string PredatorId,
    string TargetId, Vector3 Contact, double GameSeconds);

/// <summary>Session-local Pike encounter. Outcomes never alter a persistent life.</summary>
public partial class PikeEncounter : Node3D
{
    private const double AftermathMenuDelaySeconds = 2.0;
    private FishController _fish = null!;
    private PikeActor _pike = null!;
    private readonly List<PreyActor> _prey = new();
    private readonly List<BluegillSchool> _schools = new();
    private readonly List<BluegillSchool> _preySchool = new();
    private readonly List<PondFoodActor> _food = new();
    private readonly FeedingLedger _feeding = new();
    private readonly List<FishPredationEvent> _predationEvents = new();
    private readonly HashSet<string> _predationIds = new(StringComparer.Ordinal);
    private FishBloodFx _blood = null!;
    private PondGrowthState _growth = null!;
    private string _biteFeedback = "";
    private double _biteFeedbackUntil;
    private bool _growthClearanceBlocked;
    private readonly List<WorldSightSensor> _preySight = new();
    private readonly List<WorldSightSensor> _preyPikeSight = new();
    private readonly List<WorldSightSensor> _pikePreySight = new();
    private readonly List<MovementDisturbanceField> _preyCues = new();
    private readonly List<MovementDisturbanceField> _pikePreyCues = new();
    private readonly PikeTargetSelector _pikeTargets = new();
    private readonly PikeBassDiversion _bassDiversion = new();
    private readonly List<PikeCandidate> _pikeCandidates = new(PikeTargetSelector.MaxCandidates);
    private AnimationPlayer? _pikeAnimation;
    private CharacterEntry? _pikeEntry;
    private Label _status = null!;
    private ProgressBar _boostMeter = null!;
    private AudioStreamPlayer3D _smallFeedSound = null!;
    private AudioStreamPlayer3D _fishFeedSound = null!;
    private WorldAudio _audio = null!;
    private Control _menu = null!;
    private Button _retry = null!;
    private Button _resume = null!;
    private Button _skip = null!;
    private Node3D? _eggVisual;
    private AnimationPlayer? _eggAnimation;
    private Camera3D? _introCamera;
    private SwimTuning? _standardTuning;
    private double _introSeconds;
    private double _hatchEndSeconds;
    private double _emergenceStartSeconds;
    private double _cameraPanStartSeconds;
    private double _introEndSeconds;
    private Vector3 _larvaSpawn;
    private Vector3 _emergenceVisualStart;
    private Transform3D _introCameraStart;
    private bool _emergenceStarted;
    private bool _skipRequested;
    private bool _shellEaten;
    private bool _hatchClipStarted;
    private bool _introActive;
    private bool _transitioning;
    private bool _failed;
    private bool _ended;
    private double _aftermathSeconds;
    private string _outcome = "active";
    private int _escapeCount;
    private double _escapeFeedbackUntil;
    public string Outcome => _outcome;
    public int EscapeCount => _escapeCount;
    public string PikeTargetId => _pikeTargets.TargetId;
    public int PikeTargetEvaluations => _pikeTargets.Evaluated;
    public int PikePerceptionSamples => (PikeSight?.SampleCount ?? 0) +
        _pikePreySight.Sum(sensor => sensor.SampleCount);
    public IReadOnlyList<PikeCoverPlacement> CoverPlan { get; private set; } = Array.Empty<PikeCoverPlacement>();
    public WorldSightSensor? PikeSight { get; private set; }
    public SightEnvironment? SightWater { get; private set; }
    public MovementDisturbanceField? MovementCues { get; private set; }
    public WorldSightSensor? PreySight { get; private set; }
    public MovementDisturbanceField? PreyCues { get; private set; }
    public IReadOnlyList<PreyActor> PreyActors => _prey;
    public IReadOnlyList<BluegillSchool> BluegillSchools => _schools;
    public IReadOnlyList<PondFoodActor> FoodActors => _food;
    public FeedingLedger Feeding => _feeding;
    public IReadOnlyList<FishPredationEvent> PredationEvents => _predationEvents;
    public FishBloodFx Blood => _blood;
    public PondGrowthState Growth => _growth;
    public bool IntroActive => _introActive;
    public bool EmergenceStarted => _emergenceStarted;
    public bool ShellAvailable => !_shellEaten && _eggVisual is not null && !_introActive;
    public Vector3 NestPosition { get; private set; }

    public PikeEncounter() => ProcessPriority = 10; // Observe FishController after its motor tick.

    public override void _Process(double delta)
    {
        if (_ended && !_failed && !_transitioning)
        {
            _aftermathSeconds += delta;
            if (_aftermathSeconds >= AftermathMenuDelaySeconds && !_menu.Visible)
            {
                _menu.Visible = true;
                _retry.GrabFocus();
            }
            return;
        }
        if (!_introActive || _failed || _transitioning) return;
        _introSeconds += delta * (_skipRequested ? 4.0 : 1.0);
        if (!_hatchClipStarted && _introSeconds >= .85)
        {
            _eggAnimation!.Play(PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "egg")!.Clip("Hatch"));
            _eggAnimation.SpeedScale = _skipRequested ? 4f : 1f;
            _hatchClipStarted = true;
        }
        if (_hatchClipStarted && !_emergenceStarted && _introSeconds >= _emergenceStartSeconds)
            BeginEmergence();
        if (_emergenceStarted)
        {
            var fraction = Mathf.SmoothStep(0f, 1f, (float)Mathf.Clamp(
                (_introSeconds - _emergenceStartSeconds) / 1.15, 0, 1));
            _fish.GlobalPosition = _emergenceVisualStart.Lerp(_larvaSpawn, fraction);
            var pan = Mathf.SmoothStep(0f, 1f, (float)Mathf.Clamp(
                (_introSeconds - _cameraPanStartSeconds) /
                (_introEndSeconds - _cameraPanStartSeconds), 0, 1));
            var fryCamera = _fish.GetNode<Camera3D>("CameraRig/Camera3D");
            _introCamera!.GlobalTransform = _introCameraStart.InterpolateWith(fryCamera.GlobalTransform, pan);
            _introCamera.Fov = Mathf.Lerp(42f, fryCamera.Fov, pan);
        }
        if (_emergenceStarted && _introSeconds >= _introEndSeconds)
            FinishIntro();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_failed || _transitioning || _introActive || MovementCues is null || _pike is null) return;
        if (_ended)
        {
            _pike.TickAfterCatch((float)delta);
            UpdatePikeAnimation(delta);
            var aftermathTime = MovementCues.SimulationSeconds + _aftermathSeconds;
            foreach (var school in _schools) school.ResetBudget();
            for (var i = 0; i < _prey.Count; i++)
            {
                var actor = _prey[i];
                if (actor.IsAlive)
                    actor.Tick(default, default, aftermathTime, (float)delta, false,
                        _preySchool[i].For(actor, aftermathTime));
            }
            return;
        }
        MovementCues.Step(_fish.GlobalPosition, _fish.Velocity, _fish.PresentationEffort,
            _fish.PresentationTurn, delta, _fish.MotorStepCount, GetTree().Paused);
        foreach (var cues in _preyCues)
            cues.Step(_fish.GlobalPosition, _fish.Velocity, _fish.PresentationEffort,
                _fish.PresentationTurn, delta, _fish.MotorStepCount, GetTree().Paused);
        var observed = ObserveFish();
        var now = MovementCues.SimulationSeconds;
        foreach (var food in _food) food.Tick(now);
        _pikeCandidates.Clear();
        _pikeCandidates.Add(new PikeCandidate(_growth.Identity, true, true, _growth.Length,
            observed.Sight, observed.Disturbance));
        for (var i = 0; i < _prey.Count; i++)
        {
            var prey = _prey[i];
            if (!prey.IsAlive) continue;
            var radius = ((SphereShape3D)prey.GetNode<CollisionShape3D>("Collision").Shape).Radius;
            var sight = _pikePreySight[i].Observe(_pike.GlobalPosition, -_pike.GlobalBasis.Z,
                new SightTarget(prey.GlobalPosition, radius, 1f), SightWater!, now, false);
            _pikeCandidates.Add(new PikeCandidate(prey.StableId, false, true, prey.Length,
                sight, _pikePreyCues[i].Sense(_pike.GlobalPosition, false)));
        }
        var selected = _pikeTargets.Select(_pikeCandidates, _pike.GlobalPosition, now, false);
        if (selected is { } target && _pike.Mind.TargetId != target.Id)
        {
            if (_pike.Mind.TargetIsBass && _pike.Mind.HadVerifiedChase &&
                _pike.Mind.Mode is (PikeMode.Chase or PikeMode.Commit or PikeMode.Search) &&
                !target.IsBass)
                _bassDiversion.SwitchedFromBass(now, true);
            _pike.Mind.Retarget(target.Id, target.IsBass);
            GD.Print($"PIKE_TARGET_SWITCH id={target.Id} bass={target.IsBass} seconds={now:F2}");
        }
        _pike.Tick(selected?.Sight ?? default, selected?.Disturbance ?? default,
            now, (float)delta, GetTree().Paused);
        foreach (var school in _schools) school.ResetBudget();
        for (var i = 0; i < _prey.Count; i++)
        {
            var actor = _prey[i];
            if (!actor.IsAlive) continue;
            var sight = _preySight[i];
            var cues = _preyCues[i];
            var preySight = sight.Observe(actor.GlobalPosition, -actor.GlobalBasis.Z,
                new SightTarget(_fish.GlobalPosition, _fish.BodyRadius, 1f), SightWater!,
                cues.SimulationSeconds, GetTree().Paused);
            var pikeSight = _preyPikeSight[i].Observe(actor.GlobalPosition, -actor.GlobalBasis.Z,
                new SightTarget(_pike.GlobalPosition, .55f, 1f), SightWater!,
                cues.SimulationSeconds, GetTree().Paused);
            if (pikeSight.Detected && (!preySight.Detected || pikeSight.Strength > preySight.Strength))
                preySight = pikeSight;
            actor.Tick(preySight, cues.Sense(actor.GlobalPosition, GetTree().Paused),
                cues.SimulationSeconds, (float)delta, GetTree().Paused,
                _preySchool[i].For(actor, now));
            _pikePreyCues[i].Step(actor.GlobalPosition, actor.Velocity,
                Mathf.Clamp(actor.ActualSpeed / 2f, 0f, 1f), 0f,
                delta, actor.MotorStepCount, GetTree().Paused);
        }
        TryBite(); // Validated forward mouth contact; no player bite input.
        TryAdvanceGrowth();
        UpdatePikeAnimation(delta);
        if (_pike.Mind.StrikeReady)
        {
            // A committed strike needs fresh, clear contact at physical bite range.
            if (selected is { IsBass: false } preyTarget)
            {
                var index = _prey.FindIndex(p => p.StableId == preyTarget.Id && p.IsAlive);
                if (index >= 0) TryPikeConsumePrey(_prey[index], now);
            }
            else if (selected is { IsBass: true })
            {
                PikeSight!.Reset();
                var close = ObserveFish().Sight;
                if (close.Detected && close.ClearRays > 0 && _pike.MouthCanReach(_fish))
                {
                    GD.Print($"PIKE_CATCH_CONTACT pike={_pike.GlobalPosition} bass={_fish.GlobalPosition} distance={_pike.GlobalPosition.DistanceTo(_fish.GlobalPosition):F3} mouth={_pike.MouthPosition} bass_radius={_fish.BodyRadius:F3}");
                    _pike.Mind.Catch(now);
                    CommitPredation("pike:bass:" + _growth.Identity,
                        PikeEncounterAssets.PikeId, _growth.Identity, _pike.MouthPosition, now);
                    Finish("caught", "Caught by the Pike after a committed strike.");
                    return;
                }
            }
        }
        if (_bassDiversion.Step(now, observed.Sight.Detected,
            _pike.Mind.TargetId == _growth.Identity, GetTree().Paused))
        {
            _escapeCount++;
            _escapeFeedbackUntil = now + 6.0;
            GD.Print($"PIKE_ENCOUNTER_ESCAPE count={_escapeCount} seconds={now:F2} reason=target-switch");
        }
        if (_pike.Mind.ConsumeEscape())
        {
            _escapeCount++;
            _escapeFeedbackUntil = now + 6.0;
            GD.Print($"PIKE_ENCOUNTER_ESCAPE count={_escapeCount} seconds={now:F2} bass={_fish.GlobalPosition} pike={_pike.GlobalPosition}");
        }
        _boostMeter.Value = _fish.BoostEnergy * 100f;
        _status.Text = $"PIKE ENCOUNTER · Bass {_growth.Stage} · nutrition {_feeding.Nutrition}/" +
            (_growth.NextThreshold == int.MaxValue ? "mature" : _growth.NextThreshold.ToString()) +
            FoodCue(now) + " · Hold Space: boost · Esc: pause" +
            (_growthClearanceBlocked ? " · growth ready: swim into open water" : "") +
            (now < _escapeFeedbackUntil ? $" · Escaped Pike {_escapeCount} — keep swimming" : "") +
            (now < _biteFeedbackUntil ? " · " + _biteFeedback : "");
    }

    public PikeSensoryEvidence ObserveFish()
    {
        if (_failed || _transitioning || PikeSight is null || SightWater is null || MovementCues is null)
            return default;
        var pike = GetNode<CharacterBody3D>("NorthernPike");
        var paused = GetTree().Paused;
        var sight = PikeSight.Observe(pike.GlobalPosition, -pike.GlobalBasis.Z,
            new SightTarget(_fish.GlobalPosition, _fish.BodyRadius, 1f), SightWater,
            MovementCues.SimulationSeconds, paused);
        return new PikeSensoryEvidence(sight, MovementCues.Sense(pike.GlobalPosition, paused));
    }

    private bool PikeStrikeClear(PreyActor prey)
    {
        var exclude = new Godot.Collections.Array<Rid> { _pike.GetRid(), prey.GetRid() };
        using var ray = PhysicsRayQueryParameters3D.Create(_pike.MouthPosition,
            prey.GlobalPosition, 1, exclude);
        return GetWorld3D().DirectSpaceState.IntersectRay(ray).Count == 0;
    }

    internal bool TryPikeConsumePrey(PreyActor prey, double seconds)
    {
        var index = _prey.IndexOf(prey);
        if (index < 0 || !prey.IsAlive || !_pike.Mind.StrikeReady ||
            _pike.Mind.TargetId != prey.StableId || _failed || _ended || _transitioning ||
            GetTree().Paused) return false;
        _pikePreySight[index].Reset();
        var radius = ((SphereShape3D)prey.GetNode<CollisionShape3D>("Collision").Shape).Radius;
        var close = _pikePreySight[index].Observe(_pike.GlobalPosition,
            -_pike.GlobalBasis.Z, new SightTarget(prey.GlobalPosition, radius, 1f),
            SightWater!, seconds, false);
        if (!close.Detected || close.ClearRays == 0 || !_pike.MouthCanReach(prey) ||
            !PikeStrikeClear(prey)) return false;
        var contact = PreyContact(_pike.MouthPosition, prey);
        prey.Consume();
        CommitPredation("pike:bluegill:" + prey.StableId,
            PikeEncounterAssets.PikeId, prey.StableId, contact, seconds);
        _preySchool[index].RaiseAlarm(_pike.GlobalPosition, seconds);
        GD.Print($"PIKE_BLUEGILL_CONSUMED id={prey.StableId} stage={prey.Stage} seconds={seconds:F2}");
        _pikeTargets.Remove(prey.StableId);
        _pike.Mind.Retarget("", false);
        return true;
    }

    public override void _Ready()
    {
        if (Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--pike-seed-survey"))
        { PikeSeedSurvey.Run(GetTree()); return; }
        Name = "PikeEncounter";
        _fish = GetNode<FishController>("Fish");
        GetTree().Paused = false;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        DemoSelection.Clear();
        WorldSession.Select(WorldCatalog.PondId);
        BuildInterface();
        _smallFeedSound = FeedSound("SmallFeedSound", "res://assets/sfx/feeding_small.wav", -24f);
        _fishFeedSound = FeedSound("FishFeedSound", "res://assets/sfx/feeding_small.wav", -18f);
        _blood = new FishBloodFx { Name = "FishBloodFx" };
        AddChild(_blood);
        var args = OS.GetCmdlineUserArgs();
        _blood.Configure(!Array.Exists(args, arg => arg is "--effects=off" or "--blood-fx=off"),
            Array.Exists(args, arg => arg == "--graphics=low"));
        try
        {
            var cover = PikeCoverFixture.LoadCatalog();
            CoverPlan = PikeCoverFixture.Plan(cover);
            PikeCoverFixture.Build(this, cover, CoverPlan);
            BuildNest(cover);
        }
        catch (Exception error)
        {
            Fail("PIKE_COVER_UNAVAILABLE " + error.Message);
            return;
        }
        try { InitializeActors(); }
        catch (Exception error) { Fail("PIKE_ACTOR_UNAVAILABLE " + error.Message); return; }
        var otherHeadlessCheck = DisplayServer.GetName() == "headless" &&
            Array.Exists(OS.GetCmdlineUserArgs(), arg => arg != "--audio-check" &&
                (arg.EndsWith("-check", StringComparison.Ordinal) ||
                 arg == "--blood-fx-native-probe"));
        if (!_failed && !otherHeadlessCheck)
        {
            _audio = new WorldAudio { Name = "WorldAudio" };
            _audio.ConfigurePond(_fish, _growth);
            AddChild(_audio);
        }
        if (!_failed && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--pike-cover-check"))
            PikeCoverChecks.Run(this);
        if (!_failed && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--pike-sight-check"))
            PikeSightChecks.Run(this);
        if (!_failed && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--pike-disturbance-check"))
            PikeDisturbanceChecks.Run(this);
        if (!_failed && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--pike-ai-check"))
            PikeAiChecks.Run(this);
        if (!_failed && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--prey-check"))
            PreyChecks.Run(this);
        if (!_failed && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--bluegill-school-check"))
            BluegillSchoolChecks.Run(this);
        if (!_failed && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--feeding-check"))
            FeedingChecks.Run(this);
        if (!_failed && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--auto-feed-boost-check"))
            AutoFeedBoostChecks.Run(this);
        if (!_failed && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--growth-check"))
            GrowthChecks.Run(this);
        if (!_failed && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--hatch-check"))
            HatchChecks.Run(this);
        if (!_failed && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--audio-check"))
            AudioChecks.Run(this);
        if (!_failed && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--microfauna-check"))
            MicrofaunaChecks.Run(this);
        if (!_failed && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--blood-fx-check"))
            BloodFxChecks.Run(this);
        if (!_failed && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--blood-fx-native-probe"))
            BloodFxChecks.RunNative(this);
    }

    private void InitializeActors()
    {
        if (!PikeEncounterAssets.TryValidate(out var bass, out var pike, out var diagnostic))
        { Fail(diagnostic); return; }
        DemoSelection.CharacterId = bass!.Id;
        var legacyCheck = Array.Exists(OS.GetCmdlineUserArgs(), arg => arg is
            "--pike-encounter-check" or "--world-isolation-check" or "--pike-cover-check" or
            "--pike-sight-check" or "--pike-disturbance-check" or "--pike-ai-check" or
            "--prey-check" or "--bluegill-school-check" or "--feeding-check" or "--auto-feed-boost-check" or "--growth-check" or
            "--blood-fx-check" or "--blood-fx-native-probe");
        _growth = new PondGrowthState($"pike-{PikeCoverFixture.Seed}:player-bass",
            legacyCheck ? "juvenile" : "egg");
        // Swim bodies must pass through one another so a Pike mouth can reach a
        // Bass at every stage. Their masks still collide with solid Pond cover.
        _fish.CollisionLayer = 2;
        _fish.CollisionMask = 1;
        _fish.ConfigureWorldBounds(GeneratedBasin.HalfExtent - 1f,
            GetNode<SeabedSurface>("Habitat/Seabed").DeepFloorY - 2f,
            PikeCoverFixture.WaterY - .31f);
        _fish.ConfigurePondWaterline(GetNode<SeabedSurface>("Habitat/Seabed"));
        _standardTuning = _fish.Tuning;
        if (legacyCheck)
        {
            if (!PlayerArt.TryReplacePondStage(_fish, _growth.Entry(_growth.Asset), out diagnostic))
            { Fail(diagnostic); return; }
            _fish.SetEncounterBassBody(_growth.Body);
            _fish.Tuning = _growth.StageTuning(_standardTuning);
        }
        else _fish.SetEncounterHatchLocked(true);
        EnsureWaterReaction();
        if (!CharacterCatalog.TryInstantiate(pike!, out var pikeModel, out var animation, out diagnostic))
        { Fail(diagnostic); return; }
        var actor = GetNode<PikeActor>("NorthernPike");
        _pike = actor;
        _pikeTargets.SetPhysicalEdibility(
            ((CapsuleShape3D)actor.GetNode<CollisionShape3D>("Collision").Shape).Height);
        actor.Position = new Vector3(0, PikeCoverFixture.Bed(0, 5) + 1.7f, 5);
        _larvaSpawn = NestPosition + Vector3.Up * .12f + Vector3.Forward * .05f;
        _fish.Position = legacyCheck ? PikeCoverFixture.ConcealedSpawn(CoverPlan, actor.Position)
            : _larvaSpawn;
        pikeModel!.Name = "PikeVisual";
        actor.AddChild(pikeModel);
        pikeModel.Transform = pike!.PresentationTransform;
        pikeModel.Scale *= 3f;
        animation!.Play(pike.SwimClip);
        _pikeAnimation = animation;
        _pikeEntry = pike;
        var plants = PikeCoverFixture.VisibleSoftCover(GetNode<Node3D>("Cover"));
        PikeSight = new WorldSightSensor(GetWorld3D().DirectSpaceState, plants,
            actor.GetRid(), _fish.GetRid(), range: 42f, acuity: 2.5f);
        MovementCues = new MovementDisturbanceField(GetWorld3D().DirectSpaceState, plants,
            actor.GetRid(), _fish.GetRid());
        var basin = GetNode<SeabedSurface>("Habitat/Seabed").Basin;
        var turbidity = new PondTurbidityField(basin, PikeCoverFixture.WaterY);
        SightWater = new SightEnvironment(WorldEnvironmentProfile.Pond, PikeCoverFixture.WaterY,
            turbidity.Sample, SolarState.At(.5));
        InitializePrey(plants);
        InitializeFood();
        actor.Configure();
        var preferences = new DisplayPreferences();
        preferences.Load();
        _fish.ConfigureMouse(preferences.MouseSensitivity, preferences.InvertMouseY);
        var presets = new ControlPresetStore();
        presets.Load();
        _fish.SetActivePreset(presets.Current);
        _fish.EncounterBoostEnabled = true;
        _fish.ClearAndDisarmInput();
        if (legacyCheck)
        {
            Input.MouseMode = _fish.UsesMouseInput ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
            if (_fish.UsesMouseInput) _fish.ResetMouseCapture();
            _status.Text = "PIKE ENCOUNTER · juvenile Bass: aquatic nymphs near nest · Hold Space: boost · Esc: pause";
        }
        else StartIntro();
        GD.Print($"PIKE_ENCOUNTER_READY player={PondGrowthState.SpeciesId} identity={_growth.Identity} stage={_growth.Stage} pike={PikeEncounterAssets.PikeId} prey={PikeEncounterAssets.BluegillId} schools={_schools.Count} prey_count={_prey.Count} world=fixture cover={CoverPlan.Count} seed={PikeCoverFixture.Seed} player_pos={_fish.GlobalPosition} prey_pos={_prey[0].GlobalPosition} larva_pos={_prey[4].GlobalPosition}");
    }

    private WaterReaction EnsureWaterReaction()
    {
        var water = _fish.GetNodeOrNull<WaterReaction>("WaterReaction");
        if (water is null)
        {
            water = new WaterReaction(_fish, WaterVisualProfile.Pond, _fish.PondWaterlineHeight);
            _fish.AddChild(water);
        }
        if (_growth.Stage != "egg") water.ConfigureFromPlayerVisual();
        return water;
    }

    private void BuildNest(PikeCoverCatalog catalog)
    {
        var (x, z) = (PikeCoverFixture.BassNestShore.X, PikeCoverFixture.BassNestShore.Y);
        var bed = PikeCoverFixture.Bed(x, z);
        if (PikeCoverFixture.WaterY - bed is < .8f or > 1.5f ||
            PikeCoverFixture.ShoreDistance(x, z) is < 1.5f or > 3.5f)
            throw new InvalidOperationException("BASS_NEST_DEPTH_INVALID");
        NestPosition = new Vector3(x, bed + .025f, z);
        var nest = new Node3D { Name = "BassNest", Position = NestPosition };
        AddChild(nest);
        var surface = new SurfaceTool();
        surface.Begin(Mesh.PrimitiveType.Triangles);
        const int slices = 24;
        for (var i = 0; i < slices; i++)
        {
            var a = Mathf.Tau * i / slices;
            var b = Mathf.Tau * (i + 1) / slices;
            surface.AddVertex(new Vector3(0f, -.007f, 0f));
            foreach (var angle in new[] { b, a })
            {
                var dx = Mathf.Cos(angle) * .58f;
                var dz = Mathf.Sin(angle) * .58f;
                surface.AddVertex(new Vector3(dx,
                    PikeCoverFixture.Bed(x + dx, z + dz) - bed - .007f, dz));
            }
        }
        surface.GenerateNormals();
        var substrate = new MeshInstance3D { Name = "NestSubstrate",
            Mesh = surface.Commit(),
            MaterialOverride = new StandardMaterial3D
            { AlbedoColor = new Color(.22f, .19f, .12f), Roughness = 1f } };
        nest.AddChild(substrate);
        // Reuse registered nursery grass around the depression; the imported
        // plants have no collision, leaving a clear route for the tiny fry.
        var grass = catalog.Assets.Single(a => a.Id == "FAF-PLANT-010A");
        var scene = ResourceLoader.Load<PackedScene>(grass.Glb)
            ?? throw new InvalidOperationException("BASS_NEST_PLANT_UNAVAILABLE");
        var offsets = new[] { new Vector2(-1.2f, -.7f), new Vector2(1.15f, -.65f),
            new Vector2(-.9f, .8f), new Vector2(1f, .9f),
            new Vector2(-1.7f, -.2f), new Vector2(1.7f, .1f),
            new Vector2(-.4f, -1.45f), new Vector2(.55f, -1.4f),
            new Vector2(-1.25f, 1.5f), new Vector2(1.35f, 1.4f) };
        for (var i = 0; i < offsets.Length; i++)
        {
            var point = offsets[i];
            var px = x + point.X; var pz = z + point.Y;
            var plantBed = PikeCoverFixture.Bed(px, pz);
            if (PikeCoverFixture.WaterY - plantBed < .65f) continue;
            var plant = scene.Instantiate<Node3D>();
            plant.Name = "NestGrass" + i;
            nest.AddChild(plant);
            plant.GlobalPosition = new Vector3(px, plantBed - .035f, pz);
            plant.Rotation = new Vector3(0f, i * 1.13f, 0f);
            plant.Scale = Vector3.One * (.78f + (i % 3) * .08f);
        }
    }

    private void StartIntro()
    {
        var egg = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "egg")
            ?? throw new InvalidOperationException("BASS_EGG_UNREGISTERED");
        if (!PondCreatureCatalog.ExportMatches(egg))
            throw new InvalidOperationException("BASS_EGG_EXPORT_MISMATCH");
        var scene = CharacterCatalog.ModelScene(egg.ModelPath)
            ?? throw new InvalidOperationException("BASS_EGG_IMPORT_UNAVAILABLE");
        _eggVisual = scene.Instantiate<Node3D>();
        _eggVisual.Name = "BassEggVisual";
        AddChild(_eggVisual);
        _eggVisual.GlobalPosition = NestPosition + Vector3.Up * .045f;
        // The registered egg export is 3.4 mm long while its larva is 58 mm.
        // Enlarge only the nest prop so the actual larva can emerge through it.
        _eggVisual.Scale = Vector3.One * (12f /
            PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "adult")!.Dimensions[1]);
        GroundEggShell();
        _eggAnimation = _eggVisual.FindChild("AnimationPlayer", true, false) as AnimationPlayer;
        if (_eggAnimation is null || !_eggAnimation.HasAnimation(egg.Clip("Idle")) ||
            !_eggAnimation.HasAnimation(egg.Clip("Hatch")))
            throw new InvalidOperationException("BASS_EGG_HATCH_ACTION_UNAVAILABLE");
        var hatch = _eggAnimation.GetAnimation(egg.Clip("Hatch"));
        if (hatch is null || hatch.Length <= 0f || hatch.GetTrackCount() == 0)
            throw new InvalidOperationException("BASS_EGG_HATCH_ACTION_EMPTY");
        _eggAnimation.Play(egg.Clip("Idle"));
        // The export includes a separate embryo mesh offset to the right. The
        // registered shell halves and their Hatch Action remain untouched.
        foreach (var node in _eggVisual.FindChildren("*", "MeshInstance3D", true, false))
            if (node is MeshInstance3D mesh && mesh.Name.ToString().Contains("embryo", StringComparison.OrdinalIgnoreCase))
                mesh.Visible = false;
        _hatchEndSeconds = .85 + hatch.Length;
        _emergenceStartSeconds = .85 + hatch.Length * .55;
        _cameraPanStartSeconds = _emergenceStartSeconds + .25;
        _introEndSeconds = _hatchEndSeconds + 1.7;
        _introCamera = new Camera3D { Name = "HatchCamera", Near = .003f, Far = 200f,
            Fov = 42f, Current = true };
        AddChild(_introCamera);
        _introCamera.GlobalPosition = _eggVisual.GlobalPosition + new Vector3(.10f, .08f, .15f);
        _introCamera.LookAt(_eggVisual.GlobalPosition, Vector3.Up);
        _introCameraStart = _introCamera.GlobalTransform;
        _introActive = true;
        _skip.Visible = true;
        _fish.Visible = false;
        _fish.SetEncounterHatchLocked(true);
        _fish.GetNode<Camera3D>("CameraRig/Camera3D").Current = false;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        _status.Text = "BASS NEST · yolk-fed egg · Enter or Skip Intro to hatch · Esc: pause";
        GD.Print($"BASS_HATCH_BEGIN identity={_growth.Identity} stage={_growth.Stage} nest={NestPosition}");
    }

    private void GroundEggShell()
    {
        // The imported shell's pivot is above its actual lowest mesh point.
        // Aim its visible +Z opening at the fry, then settle that lowest point
        // onto the local bed. Repeating accounts for the changed exit pitch.
        var bed = PikeCoverFixture.Bed(NestPosition.X, NestPosition.Z);
        for (var pass = 0; pass < 3; pass++)
        {
            var emergence = (_larvaSpawn - _eggVisual!.GlobalPosition).Normalized();
            var pitch = Mathf.Atan2(emergence.Y, -emergence.Z);
            _eggVisual.Rotation = new Vector3(-pitch, Mathf.Pi, 0f);
            var (bottom, _) = ShellBounds(_eggVisual, emergence);
            _eggVisual.GlobalPosition += Vector3.Up * (bed + .001f - bottom);
        }
    }

    private static (float Bottom, float Front) ShellBounds(Node3D egg, Vector3 opening)
    {
        var bottom = float.PositiveInfinity;
        var front = float.NegativeInfinity;
        foreach (var child in egg.FindChildren("*", "MeshInstance3D", true, false))
        {
            if (child is not MeshInstance3D mesh ||
                !mesh.Name.ToString().Contains("shell", StringComparison.OrdinalIgnoreCase)) continue;
            var box = mesh.GetAabb();
            for (var x = 0; x <= 1; x++)
            for (var y = 0; y <= 1; y++)
            for (var z = 0; z <= 1; z++)
            {
                var corner = mesh.GlobalTransform *
                    (box.Position + box.Size * new Vector3(x, y, z));
                bottom = Mathf.Min(bottom, corner.Y);
                front = Mathf.Max(front, (corner - egg.GlobalPosition).Dot(opening));
            }
        }
        if (!float.IsFinite(bottom) || !float.IsFinite(front))
            throw new InvalidOperationException("BASS_EGG_SHELL_MESH_MISSING");
        return (bottom, front);
    }

    public void CompleteHatch()
    {
        if (!_introActive || _failed || _transitioning || _ended || GetTree().Paused) return;
        _skipRequested = true;
        if (_hatchClipStarted) _eggAnimation!.SpeedScale = 4f;
    }

    private void BeginEmergence()
    {
        if (_emergenceStarted) return;
        _emergenceStarted = true;
        _growth.Hatch();
        if (!PlayerArt.TryReplacePondStage(_fish, _growth.Entry(_growth.Asset), out var diagnostic))
        { Fail("BASS_LARVA_VISUAL_UNAVAILABLE " + diagnostic); return; }
        _fish.SetEncounterBassBody(_growth.Body);
        _fish.Tuning = _growth.StageTuning(_standardTuning!);
        EnsureWaterReaction();
        var fryCamera = _fish.GetNode<FollowCamera>("CameraRig");
        fryCamera.ConfigureEncounterLarvaView(true);
        var opening = _eggVisual!.GlobalBasis.Z.Normalized();
        _fish.GlobalPosition = _eggVisual.GlobalPosition;
        _fish.LookAt(_fish.GlobalPosition + opening, Vector3.Up);
        var (_, lip) = ShellBounds(_eggVisual, opening);
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
        _emergenceVisualStart = _eggVisual.GlobalPosition + opening * (lip + rear + .003f);
        if ((_larvaSpawn - _emergenceVisualStart).Dot(opening) <= .005f)
            throw new InvalidOperationException("BASS_LARVA_EXIT_PATH_BLOCKED");
        _fish.GlobalPosition = _emergenceVisualStart;
        fryCamera.SnapToFish();
        _fish.Visible = true;
        _status.Text = "BASS FRY · emerging from the egg · Esc: pause";
    }

    private void FinishIntro()
    {
        if (!_introActive || !_emergenceStarted) return;
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
        _audio?.ResetEvents();
        _introActive = false;
        _skip.Visible = false;
        Input.MouseMode = _fish.UsesMouseInput ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
        if (_fish.UsesMouseInput) _fish.ResetMouseCapture();
        _status.Text = "BASS FRY · yolk reserves · mouth contact: feed · Hold Space: boost · Esc: pause";
        GD.Print($"BASS_HATCH_COMPLETE identity={_growth.Identity} stage={_growth.Stage} nutrition={_feeding.Nutrition} position={_fish.GlobalPosition}");
    }

    private void InitializePrey(IReadOnlyList<SoftSightCover> plants)
    {
        // The nursery uses the real M2 shoreline growth. Limit the added mesh
        // bounds to this local patch so per-fish sensing stays below its cap.
        var nurseryCenter = new Vector3(-18.5f,
            PikeCoverFixture.Bed(-18.5f, 25f) + .65f, 25f);
        var preyPlants = plants.Concat(PikeCoverFixture.VisibleSoftCover(
                GetNode<Node3D>("ShoreGrowth"))
            .Where(plant => plant.VisibleBounds.GetCenter().DistanceTo(nurseryCenter) < 9f))
            .ToArray();
        if (preyPlants.Length > 128)
            throw new InvalidOperationException("BLUEGILL_SOFT_COVER_BUDGET");
        // Three bounded groups at real fixture cover. The first ten entries
        // keep the historical juvenile/larva test identity and feeding order.
        _schools.Add(new BluegillSchool("nursery", nurseryCenter, .8f));
        _schools.Add(new BluegillSchool("refuge", new Vector3(-11.5f,
            PikeCoverFixture.Bed(-11.5f, 4.2f) + 1.35f, 4.2f), 2.7f));
        _schools.Add(new BluegillSchool("deep", new Vector3(-2.5f,
            PikeCoverFixture.Bed(-2.5f, -18.5f) + 2.5f, -18.5f), 4.5f));
        var plan = new (string Stage, int School)[]
        {
            ("juvenile", 0), ("juvenile", 0), ("juvenile", 1), ("juvenile", 1),
            ("larva", 0), ("larva", 0), ("larva", 0), ("larva", 0),
            ("larva", 1), ("larva", 1),
            ("subadult", 1), ("subadult", 1), ("subadult", 2), ("subadult", 2),
            ("adult", 2), ("adult", 2),
        };
        var random = new Random(PikeCoverFixture.Seed ^ 0xB106);
        var assets = new Dictionary<string, (PondCreatureAsset Asset, PackedScene Scene)>();
        var indices = new Dictionary<string, int>();
        foreach (var (stage, schoolIndex) in plan)
        {
            if (!assets.TryGetValue(stage, out var source))
            {
                if (!SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.PondId,
                    PikeEncounterAssets.BluegillId, stage, SpeciesRole.Ecological,
                    out var asset, out var diagnostic) || asset is null ||
                    !PondCreatureCatalog.ExportMatches(asset))
                    throw new InvalidOperationException("PREY_STAGE_UNAVAILABLE " + stage + " " + diagnostic);
                var packed = CharacterCatalog.ModelScene(asset.ModelPath)
                    ?? throw new InvalidOperationException("PREY_MODEL_UNAVAILABLE " + stage);
                source = (asset, packed);
                assets.Add(stage, source);
            }
            var index = indices.GetValueOrDefault(stage);
            indices[stage] = index + 1;
            var radius = stage switch
            {
                "larva" => .006f, "juvenile" => .03f,
                "subadult" => .075f, _ => .12f,
            };
            var school = _schools[schoolIndex];
            var spawn = ClearSchoolSpawn(school, radius, random);
            var model = source.Scene.Instantiate<Node3D>();
            var animation = model.FindChild("AnimationPlayer", true, false) as AnimationPlayer
                ?? throw new InvalidOperationException("PREY_ANIMATION_UNAVAILABLE " + stage);
            var swim = source.Asset.Clip("Swim");
            var fast = source.Asset.Clip("SwimFast");
            if (!animation.HasAnimation(swim) || !animation.HasAnimation(fast))
                throw new InvalidOperationException("PREY_SWIM_CLIPS_UNAVAILABLE " + stage);
            var body = new PreyActor { Name = stage == "juvenile" ? "BluegillPrey" + index :
                "Bluegill" + char.ToUpperInvariant(stage[0]) + stage[1..] + index,
                CollisionLayer = 2, CollisionMask = 1,
                MotionMode = CharacterBody3D.MotionModeEnum.Floating };
            body.SetIdentity($"pike-{PikeCoverFixture.Seed}:bluegill-{stage}:{index}",
                source.Asset.Id, stage, source.Asset.Dimensions[1]);
            AddChild(body);
            body.GlobalPosition = spawn;
            body.AddChild(new CollisionShape3D { Name = "Collision",
                Shape = new SphereShape3D { Radius = radius } });
            model.Name = "PreyVisual";
            body.AddChild(model);
            model.Rotation = new Vector3(0, Mathf.Pi, 0);
            body.Configure(preyPlants, animation, swim, fast);
            school.Add(body);
            _prey.Add(body);
            _preySchool.Add(school);
            _preySight.Add(new WorldSightSensor(GetWorld3D().DirectSpaceState, preyPlants,
                body.GetRid(), _fish.GetRid(), range: 24f));
            _preyPikeSight.Add(new WorldSightSensor(GetWorld3D().DirectSpaceState, preyPlants,
                body.GetRid(), _pike.GetRid(), range: 24f));
            _pikePreySight.Add(new WorldSightSensor(GetWorld3D().DirectSpaceState, preyPlants,
                _pike.GetRid(), body.GetRid()));
            _preyCues.Add(new MovementDisturbanceField(GetWorld3D().DirectSpaceState, preyPlants,
                body.GetRid(), _fish.GetRid()));
            _pikePreyCues.Add(new MovementDisturbanceField(GetWorld3D().DirectSpaceState, preyPlants,
                _pike.GetRid(), body.GetRid()));
        }
        PreySight = _preySight[0];
        PreyCues = _preyCues[0];
    }

    private Vector3 ClearSchoolSpawn(BluegillSchool school, float radius, Random random)
    {
        var center = school.Center;
        for (var attempt = 0; attempt < 48; attempt++)
        {
            var angle = (float)random.NextDouble() * Mathf.Tau;
            var distance = .45f + (float)random.NextDouble() * 1.55f;
            var x = center.X + Mathf.Cos(angle) * distance;
            var z = center.Z + Mathf.Sin(angle) * distance;
            var bed = PikeCoverFixture.Bed(x, z);
            var y = bed + (center.Y - PikeCoverFixture.Bed(center.X, center.Z)) +
                (float)(random.NextDouble() - .5) * .25f;
            var point = new Vector3(x, y, z);
            if (PikeCoverFixture.ShoreDistance(x, z) < 2f ||
                y < bed + radius + .2f || y > PikeCoverFixture.WaterY - radius - .3f ||
                point.DistanceTo(_fish.GlobalPosition) < .7f ||
                point.DistanceTo(_pike.GlobalPosition) < 1.5f) continue;
            using var shape = new SphereShape3D { Radius = radius + .08f };
            using var query = new PhysicsShapeQueryParameters3D
            {
                Shape = shape,
                Transform = new Transform3D(Basis.Identity, point), CollisionMask = 1,
            };
            if (GetWorld3D().DirectSpaceState.IntersectShape(query, 1).Count == 0)
                return point;
        }
        throw new InvalidOperationException("BLUEGILL_SPAWN_CLEARANCE " + school.Id);
    }

    private void InitializeFood()
    {
        // Fixed, bounded nursery targets: a findable first cloud and a short trail.
        for (var i = 0; i < 8; i++)
        {
            var angle = i * Mathf.Tau / 8f;
            var distance = .30f + (i % 3) * .13f;
            var x = NestPosition.X + Mathf.Sin(angle) * distance;
            var z = NestPosition.Z + Mathf.Cos(angle) * distance;
            AddFood(i, FeedingLedger.MicrofaunaId, new Vector3(x,
                PikeCoverFixture.Bed(x, z) + .16f, z));
        }
        for (var i = 0; i < 8; i++)
        {
            var angle = i * Mathf.Tau / 8f;
            var x = NestPosition.X + Mathf.Sin(angle) * (1.05f + .18f * (i % 2));
            var z = NestPosition.Z + Mathf.Cos(angle) * (1.05f + .18f * (i % 2));
            AddFood(i, FeedingLedger.InsectId, new Vector3(x,
                PikeCoverFixture.Bed(x, z) + .22f, z));
        }

        void AddFood(int index, string kind, Vector3 at)
        {
            var actor = new PondFoodActor { Name = kind == FeedingLedger.MicrofaunaId
                ? "Zooplankton" + index : "DarnerNymph" + index };
            AddChild(actor);
            actor.Configure($"pike-{PikeCoverFixture.Seed}:{kind}:{index}", kind, at, index);
            _food.Add(actor);
        }
    }

    private void Fail(string diagnostic)
    {
        PikeSight?.Reset();
        MovementCues?.Reset();
        ResetPrey();
        _pike?.ResetActor();
        _pikeTargets.Reset();
        _bassDiversion.Reset();
        _failed = true;
        _resume.Disabled = true;
        DemoSelection.Clear();
        _fish.ClearAndDisarmInput();
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GetTree().Paused = true;
        _status.Text = "Pike Encounter unavailable: " + diagnostic;
        _menu.Visible = true;
        _retry.GrabFocus();
        GD.Print("PIKE_ENCOUNTER_UNAVAILABLE " + diagnostic);
        if (Array.Exists(OS.GetCmdlineUserArgs(), arg => (arg.StartsWith("--pike-", StringComparison.Ordinal) ||
            arg is "--prey-check" or "--bluegill-school-check" or "--feeding-check" or "--growth-check")
            && arg.EndsWith("-check", StringComparison.Ordinal)))
            GetTree().CallDeferred(SceneTree.MethodName.Quit, 1);
    }

    public FeedResult TryBite(PreyActor? target = null)
    {
        if (_failed || _transitioning || _ended || _introActive || GetTree().Paused)
            return new(false, "encounter inactive", 0);
        var mouth = MouthPosition();
        if (target is null && ShellAvailable && _growth.Stage == "larva" &&
            ShellContact(mouth))
        {
            var shellVisual = _eggVisual!;
            var shellResult = _feeding.TryConsumeEggShell(_growth.Identity + ":shell", true,
                MovementCues?.SimulationSeconds ?? 0);
            if (shellResult.Consumed)
            {
                _shellEaten = true;
                shellVisual.QueueFree();
                _eggVisual = null;
                _smallFeedSound.Play();
                GD.Print($"FEED_EVENT id={_growth.Identity}:shell kind=egg-shell nutrition={_feeding.Nutrition}");
            }
            if (shellResult.Consumed) ShowFeedFeedback("Egg shell eaten");
            return shellResult;
        }
        if (target is null)
        {
            var food = _food.Where(f => f.Available &&
                    FeedingLedger.FoodAward(f.Kind, _growth.Stage, MovementCues?.SimulationSeconds ?? 0) > 0 &&
                    FoodReachable(f))
                .OrderBy(f => f.GlobalPosition.DistanceTo(mouth)).FirstOrDefault();
            if (food is not null) return TryBiteFood(food);
        }
        target ??= _prey.Where(p => p.IsAlive &&
                FeedingLedger.IsEligiblePrey(p.SpeciesId, p.Stage, p.Length,
                    _growth.Stage, _growth.Length) &&
                PreyReachable(p))
            .OrderBy(p => p.GlobalPosition.DistanceTo(mouth)).FirstOrDefault();
        if (target is null || !_prey.Contains(target))
            return new(false, "no eligible prey", 0);
        var reachable = PreyReachable(target);
        var result = _feeding.TryConsume(new FeedCandidate(target.StableId,
            target.SpeciesId, target.Stage, WorldCatalog.PondId, WorldSession.SelectedWorldId,
            target.Length, _growth.Stage, _growth.Length, target.IsAlive), reachable,
            MovementCues?.SimulationSeconds ?? 0);
        if (result.Consumed)
        {
            var contact = PreyContact(mouth, target);
            target.Consume();
            CommitPredation("bass:bluegill:" + target.StableId,
                _growth.Identity, target.StableId, contact,
                MovementCues?.SimulationSeconds ?? 0);
            _fish.GetNode<Node3D>("PlayerVisual").GetNode<CatalogFishVisualMotion>("SwimPresentation")
                .TriggerFeedingCue();
            _fishFeedSound.Play();
            TryAdvanceGrowth();
            GD.Print($"FEED_EVENT id={target.StableId} kind=prey stage={target.Stage} nutrition={_feeding.Nutrition}");
        }
        if (result.Consumed) ShowFeedFeedback("Bluegill " + target.Stage + " eaten");
        return result;
    }

    public FeedResult TryBiteFood(PondFoodActor? target)
    {
        if (_failed || _transitioning || _ended || _introActive || GetTree().Paused ||
            target is null || !_food.Contains(target))
            return new(false, "encounter inactive or invalid food", 0);
        var result = _feeding.TryConsumeFood(target.StableId, target.Kind, _growth.Stage,
            target.Available && FoodReachable(target), MovementCues?.SimulationSeconds ?? 0,
            MovementCues?.SimulationSeconds ?? 0);
        if (result.Consumed)
        {
            target.Consume();
            _smallFeedSound.Play();
            TryAdvanceGrowth();
            GD.Print($"FEED_EVENT id={target.StableId} kind={target.Kind} nutrition={_feeding.Nutrition}");
        }
        if (result.Consumed) ShowFeedFeedback(target.Kind == FeedingLedger.MicrofaunaId
            ? "Zooplankton eaten" : "Aquatic nymph eaten");
        return result;
    }

    private Vector3 MouthPosition() => _fish.GlobalPosition - _fish.GlobalBasis.Z *
        _fish.BodyForwardReach;

    private bool ShellContact(Vector3 mouth)
    {
        if (_eggVisual is null) return false;
        var forward = -_fish.GlobalBasis.Z;
        foreach (var found in _eggVisual.FindChildren("*", "MeshInstance3D", true, false))
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

    private bool PreyReachable(PreyActor target)
    {
        if (!target.IsAlive) return false;
        var mouth = MouthPosition();
        var to = target.GlobalPosition - mouth;
        var radius = ((SphereShape3D)target.GetNode<CollisionShape3D>("Collision").Shape).Radius;
        var forward = -_fish.GlobalBasis.Z;
        // Surface contact with a small jaw volume; the prey centre may be
        // farther away by its own physical radius, but cannot be behind or to the side.
        return to.LengthSquared() > .000001f &&
            to.Length() <= radius + _growth.MouthRadius * 1.5f &&
            forward.Dot((target.GlobalPosition - _fish.GlobalPosition).Normalized()) >= .8f &&
            forward.Dot(to.Normalized()) >= .35f &&
            ClearPath(mouth, target.GlobalPosition, target.GetRid());
    }

    private bool FoodReachable(PondFoodActor target)
    {
        var mouth = MouthPosition();
        var to = target.GlobalPosition - mouth;
        return to.Length() > .005f && to.Length() <= _growth.MouthRadius * 1.5f &&
            (-_fish.GlobalBasis.Z).Dot(to.Normalized()) >= .65f &&
            ClearPath(mouth, target.GlobalPosition);
    }

    private string FoodCue(double now)
    {
        if (_growth.Stage == "egg") return " · yolk · hatching";
        if (_growth.Stage == "larva" && now < FeedingLedger.YolkSeconds)
            return $" · yolk reserves · zooplankton in {Mathf.CeilToInt((float)(FeedingLedger.YolkSeconds - now))}s" +
                (ShellAvailable ? " · shell: touch with mouth" : "");
        var kind = _growth.Stage switch
        {
            "larva" => FeedingLedger.MicrofaunaId,
            "juvenile" => FeedingLedger.InsectId,
            _ => PikeEncounterAssets.BluegillId,
        };
        var positions = kind == PikeEncounterAssets.BluegillId
            ? _prey.Where(p => p.IsAlive && FeedingLedger.IsEligiblePrey(p.SpeciesId,
                p.Stage, p.Length, _growth.Stage, _growth.Length)).Select(p => p.GlobalPosition)
            : _food.Where(f => f.Available && f.Kind == kind).Select(f => f.GlobalPosition);
        var nearest = positions.OrderBy(p => p.DistanceTo(_fish.GlobalPosition)).FirstOrDefault();
        if (nearest == default) return " · no eligible food nearby";
        var delta = nearest - _fish.GlobalPosition;
        var ahead = (-_fish.GlobalBasis.Z).Dot(delta.Normalized());
        var right = _fish.GlobalBasis.X.Dot(delta.Normalized());
        var direction = ahead > .7f ? "ahead" : ahead < -.7f ? "behind" :
            right >= 0 ? "right" : "left";
        var label = kind == FeedingLedger.MicrofaunaId ? "zooplankton cloud" :
            kind == FeedingLedger.InsectId ? "aquatic nymph" : "size-eligible Bluegill";
        return $" · {label} {direction} {delta.Length():F1}m · touch with mouth to feed";
    }

    private bool ClearPath(Vector3 from, Vector3 to, Rid target = default)
    {
        var exclude = new Godot.Collections.Array<Rid> { _fish.GetRid() };
        if (target.IsValid) exclude.Add(target);
        using var query = PhysicsRayQueryParameters3D.Create(from, to, 1, exclude);
        return GetWorld3D().DirectSpaceState.IntersectRay(query).Count == 0;
    }

    public bool TryAdvanceGrowth()
    {
        if (_growth is null || _failed || _transitioning || _ended || GetTree().Paused ||
            _growth.Next(_feeding.Nutrition) is not { } next) return false;
        var nextBody = _growth.BodyForNext();
        var radius = nextBody.Radius;
        var length = nextBody.Height;
        if (_fish.GlobalPosition.Y > _fish.PondWaterlineHeight - .65f * radius)
        { _growthClearanceBlocked = true; return false; }
        using var grownShape = new CapsuleShape3D { Radius = radius, Height = length };
        using var query = new PhysicsShapeQueryParameters3D
        {
            Shape = grownShape,
            Transform = new Transform3D(_fish.GlobalBasis * new Basis(Vector3.Right, Mathf.Pi * .5f),
                _fish.GlobalPosition + _fish.GlobalBasis.Z * nextBody.CenterZ),
            CollisionMask = _fish.CollisionMask,
            Exclude = new Godot.Collections.Array<Rid> { _fish.GetRid() },
        };
        if (GetWorld3D().DirectSpaceState.IntersectShape(query, 1).Count != 0)
        { _growthClearanceBlocked = true; return false; }
        if (!PlayerArt.TryReplacePondStage(_fish, _growth.Entry(next), out var diagnostic))
        { Fail("BASS_GROWTH_VISUAL_UNAVAILABLE " + diagnostic); return false; }
        _fish.SetEncounterBassBody(nextBody);
        _growth.Commit(next);
        _fish.Tuning = _growth.StageTuning(_standardTuning!);
        EnsureWaterReaction();
        _growthClearanceBlocked = false;
        if (_growth.Stage == "juvenile")
        {
            _fish.GetNode<FollowCamera>("CameraRig").ConfigureEncounterLarvaView(false);
        }
        PikeSight?.Reset();
        foreach (var sight in _preySight) sight.Reset();
        GD.Print($"GROWTH_EVENT identity={_growth.Identity} stage={_growth.Stage} nutrition={_feeding.Nutrition} radius={radius:F3} speed_cap={_fish.Tuning.MaxSpeed:F2}");
        return true;
    }

    private void ShowFeedFeedback(string message)
    {
        _biteFeedback = message;
        _biteFeedbackUntil = (MovementCues?.SimulationSeconds ?? 0) + 1.6;
    }

    private AudioStreamPlayer3D FeedSound(string name, string path, float volumeDb)
    {
        var stream = ResourceLoader.Load<AudioStream>(path)
            ?? throw new InvalidOperationException("FEED_SOUND_UNAVAILABLE " + path);
        var player = new AudioStreamPlayer3D { Name = name, Stream = stream, Bus = "SFX",
            VolumeDb = volumeDb, MaxDistance = 12f };
        _fish.AddChild(player);
        return player;
    }

    private static Vector3 PreyContact(Vector3 mouth, PreyActor prey)
    {
        var center = prey.GlobalPosition;
        var radius = ((SphereShape3D)prey.GetNode<CollisionShape3D>("Collision").Shape).Radius;
        var towardMouth = mouth - center;
        return center + (towardMouth.LengthSquared() > .000001f
            ? towardMouth.Normalized() * radius : Vector3.Zero);
    }

    private void CommitPredation(string id, string predatorId, string targetId,
        Vector3 contact, double seconds)
    {
        if (!_predationIds.Add(id)) return;
        _predationEvents.Add(new FishPredationEvent(id, predatorId, targetId, contact, seconds));
        _blood.TryEmit(id, contact);
    }

    public override void _Input(InputEvent @event)
    {
        if (_transitioning || _failed || @event is not InputEventKey { Pressed: true, Echo: false } key) return;
        if (_ended)
        {
            if (key.Keycode == Key.Escape && !_menu.Visible)
            {
                _menu.Visible = true;
                _retry.GrabFocus();
                GetViewport().SetInputAsHandled();
            }
            return;
        }
        if (_introActive && !_menu.Visible &&
            (key.Keycode is Key.Enter or Key.KpEnter or Key.Space))
        { CompleteHatch(); GetViewport().SetInputAsHandled(); return; }
        if (key.Keycode != Key.Escape) return;
        if (_menu.Visible) Resume(); else Pause();
        GetViewport().SetInputAsHandled();
    }

    public override void _Notification(int what)
    {
        if (what is (int)NotificationApplicationFocusOut or (int)NotificationWMWindowFocusOut && IsNodeReady() && !_failed && !_ended)
            Pause();
    }

    private void BuildInterface()
    {
        var canvas = new CanvasLayer { Name = "FixtureHud", Layer = 10 };
        AddChild(canvas);
        _status = new Label { Name = "EncounterStatus", Position = new Vector2(18, 16),
            CustomMinimumSize = new Vector2(420, 60), AutowrapMode = TextServer.AutowrapMode.WordSmart,
            MouseFilter = Control.MouseFilterEnum.Ignore };
        canvas.AddChild(_status);
        _boostMeter = new ProgressBar { Name = "BoostEnergy", Position = new Vector2(18, 68),
            CustomMinimumSize = new Vector2(180, 12), MaxValue = 100, Value = 100,
            ShowPercentage = false, MouseFilter = Control.MouseFilterEnum.Ignore };
        canvas.AddChild(_boostMeter);
        _skip = new Button { Name = "SkipIntro", Text = "Skip Intro",
            Position = new Vector2(18, 82), Visible = false,
            FocusMode = Control.FocusModeEnum.All };
        _skip.Pressed += CompleteHatch;
        canvas.AddChild(_skip);
        _menu = new CenterContainer { Name = "EncounterMenu", Visible = false,
            ProcessMode = ProcessModeEnum.Always, MouseFilter = Control.MouseFilterEnum.Pass };
        _menu.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        canvas.AddChild(_menu);
        var panel = new PanelContainer { CustomMinimumSize = new Vector2(300, 0) };
        _menu.AddChild(panel);
        var actions = new VBoxContainer();
        panel.AddChild(actions);
        _resume = new Button { Name = "ResumeEncounter", Text = "Resume Encounter", FocusMode = Control.FocusModeEnum.All };
        _resume.Pressed += Resume;
        actions.AddChild(_resume);
        _retry = new Button { Name = "RetryEncounter", Text = "Retry Encounter", FocusMode = Control.FocusModeEnum.All };
        _retry.Pressed += Retry;
        actions.AddChild(_retry);
        var back = new Button { Name = "ReturnToWorldSelect", Text = "Return to World Select", FocusMode = Control.FocusModeEnum.All };
        back.Pressed += ReturnToWorldSelect;
        actions.AddChild(back);
    }

    private void Pause()
    {
        _fish.ClearAndDisarmInput();
        _fish.GetNodeOrNull<WaterReaction>("WaterReaction")?.Suspend();
        _audio?.ResetEvents();
        _blood.SetPaused(true);
        GetTree().Paused = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        _menu.Visible = true;
        _retry.GrabFocus();
    }

    private void Resume()
    {
        if (_failed || _transitioning || _ended) return;
        _fish.ClearAndDisarmInput();
        _audio?.ResetEvents();
        _menu.Visible = false;
        GetTree().Paused = false;
        _blood.SetPaused(false);
        Input.MouseMode = !_introActive && _fish.UsesMouseInput ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
        if (!_introActive && _fish.UsesMouseInput) _fish.ResetMouseCapture();
    }

    private void Retry() => Leave(PikeEncounterAssets.ScenePath);
    private void ReturnToWorldSelect() => Leave("res://scenes/world_select.tscn");

    private void Finish(string outcome, string message)
    {
        _outcome = outcome;
        _ended = true;
        _aftermathSeconds = 0;
        _pike.BeginAfterCatch();
        _fish.ClearAndDisarmInput();
        _fish.GetNodeOrNull<WaterReaction>("WaterReaction")?.Suspend();
        _audio?.ResetEvents();
        _fish.SetPhysicsProcess(false);
        _fish.CollisionLayer = 0;
        _fish.CollisionMask = 0;
        _fish.GetNode<Node3D>("PlayerVisual").Hide();
        var rig = _fish.GetNode<FollowCamera>("CameraRig");
        rig.SetPhysicsProcess(false);
        rig.SetProcess(false);
        var contact = _pike.MouthPosition;
        var forward = -_pike.GlobalBasis.Z;
        var side = forward.Cross(Vector3.Up).Normalized();
        var view = contact - side * 2.5f + forward * .8f + Vector3.Up * .65f;
        view.Y = Mathf.Clamp(view.Y, PikeCoverFixture.Bed(view.X, view.Z) + .5f,
            PikeCoverFixture.WaterY - .35f);
        var camera = _fish.GetNode<Camera3D>("CameraRig/Camera3D");
        camera.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;
        camera.GlobalPosition = view;
        camera.LookAt(contact, Vector3.Up);
        Input.MouseMode = Input.MouseModeEnum.Visible;
        _status.Text = message + " Retry or return to World Select.";
        _resume.Disabled = true;
        _menu.Visible = false;
        GD.Print($"PIKE_ENCOUNTER_OUTCOME outcome={outcome} seconds={MovementCues?.SimulationSeconds:F2}");
    }

    private void UpdatePikeAnimation(double delta)
    {
        if (_pikeAnimation is null || _pikeEntry is null) return;
        var clip = _pike.Mind.Mode switch
        {
            PikeMode.Chase => _pikeEntry.FastClip,
            PikeMode.Commit => _pikeEntry.FastClip,
            _ => _pikeEntry.SwimClip,
        };
        if (_pikeAnimation.CurrentAnimation != clip) _pikeAnimation.Play(clip, .22);
        _pikeAnimation.SpeedScale = Mathf.Clamp(.72f + _pike.ActualSpeed * .13f, .72f, 1.55f);
        var source = _pikeAnimation.GetAnimation(clip);
        if (source is not null && source.LoopMode == Animation.LoopModeEnum.None &&
            _pikeAnimation.CurrentAnimationPosition >= source.Length - (float)delta * _pikeAnimation.SpeedScale)
            _pikeAnimation.Seek(0, true);
    }

    private void Leave(string scene)
    {
        if (_transitioning) return;
        _blood.Clear();
        PikeSight?.Reset();
        MovementCues?.Reset();
        ResetPrey();
        _pike?.ResetActor();
        _pikeTargets.Reset();
        _bassDiversion.Reset();
        _transitioning = true;
        _fish.ClearAndDisarmInput();
        _fish.GetNodeOrNull<WaterReaction>("WaterReaction")?.Suspend();
        _audio?.ResetEvents();
        _audio?.Shutdown();
        GetTree().Paused = false;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        DemoSelection.Clear();
        WorldSession.Select(WorldCatalog.OceanId);
        if (GetTree().ChangeSceneToFile(scene) != Error.Ok)
        {
            _transitioning = false;
            Fail("PIKE_SCENE_LOAD_FAILED action=restore-scene");
        }
    }

    private void ResetPrey()
    {
        foreach (var sight in _preySight) sight.Reset();
        foreach (var sight in _preyPikeSight) sight.Reset();
        foreach (var sight in _pikePreySight) sight.Reset();
        foreach (var cues in _preyCues) cues.Reset();
        foreach (var cues in _pikePreyCues) cues.Reset();
        foreach (var actor in _prey) actor.ResetActor();
        foreach (var school in _schools) school.ResetBudget();
    }
}
