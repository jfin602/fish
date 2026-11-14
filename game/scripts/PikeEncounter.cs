#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

public readonly record struct PikeSensoryEvidence(SightEvidence Sight, DisturbanceEvidence Disturbance);

/// <summary>Session-local Pike encounter. Outcomes never alter a persistent life.</summary>
public partial class PikeEncounter : Node3D
{
    private FishController _fish = null!;
    private PikeActor _pike = null!;
    private readonly List<PreyActor> _prey = new();
    private readonly FeedingLedger _feeding = new();
    private PondGrowthState _growth = null!;
    private string _biteFeedback = "";
    private double _biteFeedbackUntil;
    private readonly List<WorldSightSensor> _preySight = new();
    private readonly List<MovementDisturbanceField> _preyCues = new();
    private AnimationPlayer? _pikeAnimation;
    private CharacterEntry? _pikeEntry;
    private Label _status = null!;
    private Control _menu = null!;
    private Button _retry = null!;
    private Button _resume = null!;
    private bool _transitioning;
    private bool _failed;
    private bool _ended;
    private string _outcome = "active";
    private int _escapeCount;
    private double _escapeFeedbackUntil;
    public string Outcome => _outcome;
    public int EscapeCount => _escapeCount;
    public IReadOnlyList<PikeCoverPlacement> CoverPlan { get; private set; } = Array.Empty<PikeCoverPlacement>();
    public WorldSightSensor? PikeSight { get; private set; }
    public SightEnvironment? SightWater { get; private set; }
    public MovementDisturbanceField? MovementCues { get; private set; }
    public WorldSightSensor? PreySight { get; private set; }
    public MovementDisturbanceField? PreyCues { get; private set; }
    public IReadOnlyList<PreyActor> PreyActors => _prey;
    public FeedingLedger Feeding => _feeding;
    public PondGrowthState Growth => _growth;

    public PikeEncounter() => ProcessPriority = 10; // Observe FishController after its motor tick.

    public override void _PhysicsProcess(double delta)
    {
        if (_failed || _transitioning || _ended || MovementCues is null || _pike is null) return;
        MovementCues.Step(_fish.GlobalPosition, _fish.Velocity, _fish.PresentationEffort,
            _fish.PresentationTurn, delta, _fish.MotorStepCount, GetTree().Paused);
        foreach (var cues in _preyCues)
            cues.Step(_fish.GlobalPosition, _fish.Velocity, _fish.PresentationEffort,
                _fish.PresentationTurn, delta, _fish.MotorStepCount, GetTree().Paused);
        var observed = ObserveFish();
        var now = MovementCues.SimulationSeconds;
        _pike.Tick(observed.Sight, observed.Disturbance, now, (float)delta, GetTree().Paused);
        for (var i = 0; i < _prey.Count; i++)
        {
            var actor = _prey[i];
            if (!actor.IsAlive) continue;
            var sight = _preySight[i];
            var cues = _preyCues[i];
            var preySight = sight.Observe(actor.GlobalPosition, -actor.GlobalBasis.Z,
                new SightTarget(_fish.GlobalPosition, _fish.BodyRadius, 1f), SightWater!,
                cues.SimulationSeconds, GetTree().Paused);
            actor.Tick(preySight, cues.Sense(actor.GlobalPosition, GetTree().Paused),
                cues.SimulationSeconds, (float)delta, GetTree().Paused);
        }
        TryAdvanceGrowth();
        UpdatePikeAnimation(delta);
        if (_pike.Mind.StrikeReady)
        {
            // A committed strike needs fresh, clear contact at physical bite range.
            PikeSight!.Reset();
            var close = ObserveFish().Sight;
            if (close.Detected && close.ClearRays > 0 &&
                _pike.MouthCanReach(_fish))
            {
                GD.Print($"PIKE_CATCH_CONTACT pike={_pike.GlobalPosition} bass={_fish.GlobalPosition} distance={_pike.GlobalPosition.DistanceTo(_fish.GlobalPosition):F3} mouth={_pike.MouthPosition} bass_radius={_fish.BodyRadius:F3}");
                _pike.Mind.Catch(now);
                Finish("caught", "Caught by the Pike after a committed strike.");
                return;
            }
        }
        if (_pike.Mind.ConsumeEscape())
        {
            _escapeCount++;
            _escapeFeedbackUntil = now + 6.0;
            GD.Print($"PIKE_ENCOUNTER_ESCAPE count={_escapeCount} seconds={now:F2} bass={_fish.GlobalPosition} pike={_pike.GlobalPosition}");
        }
        _status.Text = $"PIKE ENCOUNTER · {_pike.Mind.Mode} · Bass {_growth.Stage} · nutrition {_feeding.Nutrition} · Space: bite · Esc: pause" +
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
        try
        {
            var cover = PikeCoverFixture.LoadCatalog();
            CoverPlan = PikeCoverFixture.Plan(cover);
            PikeCoverFixture.Build(this, cover, CoverPlan);
        }
        catch (Exception error)
        {
            Fail("PIKE_COVER_UNAVAILABLE " + error.Message);
            return;
        }
        try { InitializeActors(); }
        catch (Exception error) { Fail("PIKE_ACTOR_UNAVAILABLE " + error.Message); return; }
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
        if (!_failed && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--feeding-check"))
            FeedingChecks.Run(this);
        if (!_failed && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--growth-check"))
            GrowthChecks.Run(this);
    }

    private void InitializeActors()
    {
        if (!PikeEncounterAssets.TryValidate(out var bass, out var pike, out var diagnostic))
        { Fail(diagnostic); return; }
        DemoSelection.CharacterId = bass!.Id;
        _growth = new PondGrowthState($"pike-{PikeCoverFixture.Seed}:player-bass");
        _fish.ConfigureWorldBounds(GeneratedBasin.HalfExtent - 1f,
            GetNode<SeabedSurface>("Habitat/Seabed").DeepFloorY - 2f,
            PikeCoverFixture.WaterY - .31f);
        _fish.ConfigurePondWaterline(GetNode<SeabedSurface>("Habitat/Seabed"));
        if (!PlayerArt.TryReplacePondStage(_fish, _growth.Entry(_growth.Asset), out diagnostic))
        { Fail(diagnostic); return; }
        _fish.SetEncounterBassBody(_growth.ColliderRadius, _growth.ColliderLength);
        if (!CharacterCatalog.TryInstantiate(pike!, out var pikeModel, out var animation, out diagnostic))
        { Fail(diagnostic); return; }
        var actor = GetNode<PikeActor>("NorthernPike");
        _pike = actor;
        actor.Position = new Vector3(0, PikeCoverFixture.Bed(0, 5) + 1.7f, 5);
        _fish.Position = PikeCoverFixture.ConcealedSpawn(CoverPlan, actor.Position);
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
        actor.Configure();
        var preferences = new DisplayPreferences();
        preferences.Load();
        _fish.ConfigureMouse(preferences.MouseSensitivity, preferences.InvertMouseY);
        var presets = new ControlPresetStore();
        presets.Load();
        _fish.SetActivePreset(presets.Current);
        _fish.ClearAndDisarmInput();
        Input.MouseMode = _fish.UsesMouseInput ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
        if (_fish.UsesMouseInput) _fish.ResetMouseCapture();
        _status.Text = "PIKE ENCOUNTER · juvenile Bass: Bluegill larvae near shallow grass · Space: bite · Esc: pause";
        GD.Print($"PIKE_ENCOUNTER_READY player={PondGrowthState.SpeciesId} identity={_growth.Identity} stage={_growth.Stage} pike={PikeEncounterAssets.PikeId} prey={PikeEncounterAssets.BluegillId} juvenile_count=4 larva_count=6 world=fixture cover={CoverPlan.Count} seed={PikeCoverFixture.Seed} player_pos={_fish.GlobalPosition} prey_pos={_prey[0].GlobalPosition} larva_pos={_prey[4].GlobalPosition}");
    }

    private void InitializePrey(IReadOnlyList<SoftSightCover> plants)
    {
        if (!SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.PondId, PikeEncounterAssets.BluegillId,
                "juvenile", SpeciesRole.Ecological, out var asset, out var diagnostic) || asset is null)
            throw new InvalidOperationException("PREY_ASSET_UNAVAILABLE " + diagnostic);
        var scene = ResourceLoader.Load<PackedScene>(asset.ModelPath, "", ResourceLoader.CacheMode.Ignore)
            ?? throw new InvalidOperationException("PREY_MODEL_UNAVAILABLE");
        var swim = asset.Clip("Swim");
        var fast = asset.Clip("SwimFast");
        var spawns = new[]
        {
            new Vector2(-15f, 20f), new Vector2(-17f, 18f),
            new Vector2(-14f, 21f), new Vector2(-16f, 19f),
        };
        for (var i = 0; i < spawns.Length; i++)
        {
            var model = scene.Instantiate<Node3D>();
            var animation = model.FindChild("AnimationPlayer", true, false) as AnimationPlayer
                ?? throw new InvalidOperationException("PREY_ANIMATION_UNAVAILABLE");
            if (!animation.HasAnimation(swim) || !animation.HasAnimation(fast))
                throw new InvalidOperationException("PREY_SWIM_CLIPS_UNAVAILABLE");
            var body = new PreyActor { Name = "BluegillPrey" + i, CollisionLayer = 1, CollisionMask = 1,
                MotionMode = CharacterBody3D.MotionModeEnum.Floating };
            body.SetIdentity($"pike-{PikeCoverFixture.Seed}:bluegill-juvenile:{i}",
                asset.Id, "juvenile", asset.Dimensions[1]);
            AddChild(body);
            var point = spawns[i];
            body.GlobalPosition = new Vector3(point.X,
                PikeCoverFixture.Bed(point.X, point.Y) + .75f, point.Y);
            body.AddChild(new CollisionShape3D { Name = "Collision",
                Shape = new SphereShape3D { Radius = .075f } });
            model.Name = "PreyVisual";
            body.AddChild(model);
            model.Rotation = new Vector3(0, Mathf.Pi, 0);
            body.Configure(plants, animation, swim, fast);
            _prey.Add(body);
            _preySight.Add(new WorldSightSensor(GetWorld3D().DirectSpaceState, plants,
                body.GetRid(), _fish.GetRid(), range: 24f));
            _preyCues.Add(new MovementDisturbanceField(GetWorld3D().DirectSpaceState, plants,
                body.GetRid(), _fish.GetRid()));
        }
        PreySight = _preySight[0];
        PreyCues = _preyCues[0];
        AddBluegillLarvae(plants);
    }

    private void AddBluegillLarvae(IReadOnlyList<SoftSightCover> plants)
    {
        if (!SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.PondId, PikeEncounterAssets.BluegillId,
                "larva", SpeciesRole.Ecological, out var asset, out var diagnostic) || asset is null)
            throw new InvalidOperationException("LARVA_ASSET_UNAVAILABLE " + diagnostic);
        var scene = ResourceLoader.Load<PackedScene>(asset.ModelPath, "", ResourceLoader.CacheMode.Ignore)
            ?? throw new InvalidOperationException("LARVA_MODEL_UNAVAILABLE");
        var swim = asset.Clip("Swim");
        var fast = asset.Clip("SwimFast");
        var points = new[]
        {
            (Position: new Vector2(-17f, 21.4f), AboveBed: 1.35f),
            (Position: new Vector2(-15.5f, 20.6f), AboveBed: 1.2f),
            (Position: new Vector2(-9.9f, 11.2f), AboveBed: .75f),
            (Position: new Vector2(-14.2f, -3.0f), AboveBed: .75f),
            (Position: new Vector2(-10.5f, -11.8f), AboveBed: .75f),
            (Position: new Vector2(0f, -19.5f), AboveBed: .75f),
        };
        for (var i = 0; i < points.Length; i++)
        {
            var model = scene.Instantiate<Node3D>();
            var animation = model.FindChild("AnimationPlayer", true, false) as AnimationPlayer
                ?? throw new InvalidOperationException("LARVA_ANIMATION_UNAVAILABLE");
            if (!animation.HasAnimation(swim) || !animation.HasAnimation(fast))
                throw new InvalidOperationException("LARVA_SWIM_CLIPS_UNAVAILABLE");
            var body = new PreyActor { Name = "BluegillLarva" + i, CollisionLayer = 1, CollisionMask = 1,
                MotionMode = CharacterBody3D.MotionModeEnum.Floating };
            body.SetIdentity($"pike-{PikeCoverFixture.Seed}:bluegill-larva:{i}",
                asset.Id, "larva", asset.Dimensions[1]);
            AddChild(body);
            var point = points[i].Position;
            body.GlobalPosition = new Vector3(point.X,
                PikeCoverFixture.Bed(point.X, point.Y) + points[i].AboveBed, point.Y);
            body.AddChild(new CollisionShape3D { Name = "Collision",
                Shape = new SphereShape3D { Radius = .07f } });
            model.Name = "PreyVisual";
            body.AddChild(model);
            model.Rotation = new Vector3(0, Mathf.Pi, 0);
            var adultBluegill = PondCreatureCatalog.Stage(PikeEncounterAssets.BluegillId, "adult")
                ?? throw new InvalidOperationException("BLUEGILL_ADULT_SCALE_UNAVAILABLE");
            model.Scale = Vector3.One / adultBluegill.Dimensions[1];
            body.Configure(plants, animation, swim, fast);
            _prey.Add(body);
            _preySight.Add(new WorldSightSensor(GetWorld3D().DirectSpaceState, plants,
                body.GetRid(), _fish.GetRid(), range: 24f));
            _preyCues.Add(new MovementDisturbanceField(GetWorld3D().DirectSpaceState, plants,
                body.GetRid(), _fish.GetRid()));
        }
    }

    private void Fail(string diagnostic)
    {
        PikeSight?.Reset();
        MovementCues?.Reset();
        ResetPrey();
        _pike?.ResetActor();
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
            arg is "--prey-check" or "--feeding-check" or "--growth-check")
            && arg.EndsWith("-check", StringComparison.Ordinal)))
            GetTree().CallDeferred(SceneTree.MethodName.Quit, 1);
    }

    public FeedResult TryBite(PreyActor? target = null)
    {
        if (_failed || _transitioning || _ended || GetTree().Paused)
            return new(false, "encounter inactive", 0);
        var mouth = _fish.GlobalPosition - _fish.GlobalBasis.Z * _fish.BodyForwardReach;
        target ??= _prey.Where(p => p.IsAlive &&
                FeedingLedger.IsEligiblePrey(p.SpeciesId, p.Stage, p.Length,
                    _growth.Stage, _growth.Length) &&
                p.GlobalPosition.DistanceTo(mouth) <= 1.1f &&
                (-_fish.GlobalBasis.Z).Dot((p.GlobalPosition - mouth).Normalized()) >= .35f &&
                ClearPath(mouth, p.GlobalPosition, p.GetRid()))
            .OrderBy(p => p.GlobalPosition.DistanceTo(mouth)).FirstOrDefault();
        if (target is null || !_prey.Contains(target))
            return ReportBite(new(false, "no eligible prey", 0));
        var to = target.GlobalPosition - mouth;
        var reachable = target.IsAlive && to.Length() <= 1.1f && to.Length() > .01f &&
            (-_fish.GlobalBasis.Z).Dot(to.Normalized()) >= .35f &&
            ClearPath(mouth, target.GlobalPosition, target.GetRid());
        var result = _feeding.TryConsume(new FeedCandidate(target.StableId,
            target.SpeciesId, target.Stage, WorldCatalog.PondId, WorldSession.SelectedWorldId,
            target.Length, _growth.Stage, _growth.Length, target.IsAlive), reachable,
            MovementCues?.SimulationSeconds ?? 0);
        if (result.Consumed)
        {
            target.Consume();
            TryAdvanceGrowth();
            GD.Print($"FEED_EVENT id={target.StableId} kind=prey stage={target.Stage} nutrition={_feeding.Nutrition}");
        }
        return ReportBite(result, target.Stage);
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
        var radius = _growth.RadiusForNext();
        var length = _growth.LengthForNext();
        if (_fish.GlobalPosition.Y > _fish.PondWaterlineHeight - .65f * radius) return false;
        using var grownShape = new CapsuleShape3D { Radius = radius, Height = length };
        using var query = new PhysicsShapeQueryParameters3D
        {
            Shape = grownShape,
            Transform = new Transform3D(_fish.GlobalBasis * new Basis(Vector3.Right, Mathf.Pi * .5f),
                _fish.GlobalPosition + _fish.GlobalBasis.Z * length * .08f),
            CollisionMask = _fish.CollisionMask,
            Exclude = new Godot.Collections.Array<Rid> { _fish.GetRid() },
        };
        if (GetWorld3D().DirectSpaceState.IntersectShape(query, 1).Count != 0) return false;
        if (!PlayerArt.TryReplacePondStage(_fish, _growth.Entry(next), out var diagnostic))
        { Fail("BASS_GROWTH_VISUAL_UNAVAILABLE " + diagnostic); return false; }
        _fish.SetEncounterBassBody(radius, length);
        _growth.Commit(next);
        PikeSight?.Reset();
        foreach (var sight in _preySight) sight.Reset();
        GD.Print($"GROWTH_EVENT identity={_growth.Identity} stage={_growth.Stage} nutrition={_feeding.Nutrition} radius={radius:F3}");
        return true;
    }

    private FeedResult ReportBite(FeedResult result, string? stage = null)
    {
        _biteFeedback = result.Consumed ? "Bluegill " + stage + " eaten" : "Bite missed: " + result.Reason;
        _biteFeedbackUntil = (MovementCues?.SimulationSeconds ?? 0) + 1.6;
        return result;
    }

    public override void _Input(InputEvent @event)
    {
        if (_transitioning || _failed || _ended || @event is not InputEventKey { Pressed: true, Echo: false } key) return;
        if ((key.PhysicalKeycode == Key.Space || key.Keycode == Key.Space) && !_menu.Visible)
        { TryBite(); GetViewport().SetInputAsHandled(); return; }
        if (key.Keycode != Key.Escape) return;
        if (_menu.Visible) Resume(); else Pause();
        GetViewport().SetInputAsHandled();
    }

    public override void _Notification(int what)
    {
        if (what is (int)NotificationApplicationFocusOut or (int)NotificationWMWindowFocusOut && IsNodeReady() && !_failed)
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
        GetTree().Paused = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        _menu.Visible = true;
        _retry.GrabFocus();
    }

    private void Resume()
    {
        if (_failed || _transitioning || _ended) return;
        _fish.ClearAndDisarmInput();
        _menu.Visible = false;
        GetTree().Paused = false;
        Input.MouseMode = _fish.UsesMouseInput ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
        if (_fish.UsesMouseInput) _fish.ResetMouseCapture();
    }

    private void Retry() => Leave(PikeEncounterAssets.ScenePath);
    private void ReturnToWorldSelect() => Leave("res://scenes/world_select.tscn");

    private void Finish(string outcome, string message)
    {
        _outcome = outcome;
        _ended = true;
        _fish.ClearAndDisarmInput();
        GetTree().Paused = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        _status.Text = message + " Retry or return to World Select.";
        _resume.Disabled = true;
        _menu.Visible = true;
        _retry.GrabFocus();
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
        PikeSight?.Reset();
        MovementCues?.Reset();
        ResetPrey();
        _pike?.ResetActor();
        _transitioning = true;
        _fish.ClearAndDisarmInput();
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
        foreach (var cues in _preyCues) cues.Reset();
        foreach (var actor in _prey) actor.ResetActor();
    }
}
