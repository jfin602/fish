#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace Fish;

public readonly record struct PikeSensoryEvidence(SightEvidence Sight, DisturbanceEvidence Disturbance);

/// <summary>Session-local Pike encounter. Outcomes never alter a persistent life.</summary>
public partial class PikeEncounter : Node3D
{
    private FishController _fish = null!;
    private PikeActor _pike = null!;
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
    public string Outcome => _outcome;
    public IReadOnlyList<PikeCoverPlacement> CoverPlan { get; private set; } = Array.Empty<PikeCoverPlacement>();
    public WorldSightSensor? PikeSight { get; private set; }
    public SightEnvironment? SightWater { get; private set; }
    public MovementDisturbanceField? MovementCues { get; private set; }

    public PikeEncounter() => ProcessPriority = 10; // Observe FishController after its motor tick.

    public override void _PhysicsProcess(double delta)
    {
        if (_failed || _transitioning || _ended || MovementCues is null || _pike is null) return;
        MovementCues.Step(_fish.GlobalPosition, _fish.Velocity, _fish.PresentationEffort,
            _fish.PresentationTurn, delta, _fish.MotorStepCount, GetTree().Paused);
        var observed = ObserveFish();
        var now = MovementCues.SimulationSeconds;
        _pike.Tick(observed.Sight, observed.Disturbance, now, (float)delta, GetTree().Paused);
        UpdatePikeAnimation();
        if (_pike.Mind.StrikeReady)
        {
            // A committed strike needs fresh, clear contact at physical bite range.
            PikeSight!.Reset();
            var close = ObserveFish().Sight;
            if (close.Detected && close.ClearRays > 0 &&
                _pike.GlobalPosition.DistanceTo(_fish.GlobalPosition) <= 1.15f &&
                (-_pike.GlobalBasis.Z).Dot((_fish.GlobalPosition - _pike.GlobalPosition).Normalized()) > .35f)
            {
                _pike.Mind.Catch(now);
                Finish("caught", "Caught by the Pike after a committed strike.");
                return;
            }
        }
        if (_pike.Mind.Escaped)
        {
            Finish("escaped", "Escaped: the Pike searched its last sensed area and disengaged.");
            return;
        }
        _status.Text = $"PIKE ENCOUNTER · {_pike.Mind.Mode} · Shallow grass → roots → deep wood · Esc: pause";
    }

    public PikeSensoryEvidence ObserveFish()
    {
        if (_failed || _transitioning || PikeSight is null || SightWater is null || MovementCues is null)
            return default;
        var pike = GetNode<CharacterBody3D>("NorthernPike");
        var paused = GetTree().Paused;
        var sight = PikeSight.Observe(pike.GlobalPosition, -pike.GlobalBasis.Z,
            new SightTarget(_fish.GlobalPosition, .48f, 1f), SightWater,
            MovementCues.SimulationSeconds, paused);
        return new PikeSensoryEvidence(sight, MovementCues.Sense(pike.GlobalPosition, paused));
    }

    public override void _Ready()
    {
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
        InitializeActors();
        if (!_failed && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--pike-cover-check"))
            PikeCoverChecks.Run(this);
        if (!_failed && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--pike-sight-check"))
            PikeSightChecks.Run(this);
        if (!_failed && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--pike-disturbance-check"))
            PikeDisturbanceChecks.Run(this);
        if (!_failed && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--pike-ai-check"))
            PikeAiChecks.Run(this);
    }

    private void InitializeActors()
    {
        if (!PikeEncounterAssets.TryValidate(out var bluegill, out var pike, out var diagnostic))
        { Fail(diagnostic); return; }
        DemoSelection.CharacterId = PikeEncounterAssets.BluegillId;
        _fish.ConfigureWorldBounds(36.7f, -7f, PikeCoverFixture.WaterY - .31f);
        if (!PlayerArt.TryLoadAndAttach(_fish, bluegill!, out diagnostic))
        { Fail(diagnostic); return; }
        if (!CharacterCatalog.TryInstantiate(pike!, out var pikeModel, out var animation, out diagnostic))
        { Fail(diagnostic); return; }
        var actor = GetNode<PikeActor>("NorthernPike");
        _pike = actor;
        pikeModel!.Name = "PikeVisual";
        actor.AddChild(pikeModel);
        pikeModel.Transform = pike!.PresentationTransform;
        animation!.Play(pike.GlideClip);
        _pikeAnimation = animation;
        _pikeEntry = pike;
        var plants = PikeCoverFixture.VisibleSoftCover(GetNode<Node3D>("Cover"));
        PikeSight = new WorldSightSensor(GetWorld3D().DirectSpaceState, plants,
            actor.GetRid(), _fish.GetRid());
        MovementCues = new MovementDisturbanceField(GetWorld3D().DirectSpaceState, plants,
            actor.GetRid(), _fish.GetRid());
        SightWater = new SightEnvironment(WorldEnvironmentProfile.Pond, PikeCoverFixture.WaterY,
            PikeCoverFixture.FixtureSediment, SolarState.At(.5));
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
        _status.Text = "PIKE ENCOUNTER · Shallow grass → root refuge → deep wood maze · Esc: pause";
        GD.Print($"PIKE_ENCOUNTER_READY player=FAF-FISH-006 pike=FAF-FISH-010 world=fixture cover={CoverPlan.Count} seed={PikeCoverFixture.Seed}");
    }

    private void Fail(string diagnostic)
    {
        PikeSight?.Reset();
        MovementCues?.Reset();
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
    }

    public override void _Input(InputEvent @event)
    {
        if (_transitioning || _failed || _ended || @event is not InputEventKey { Pressed: true, Echo: false } key ||
            key.Keycode != Key.Escape) return;
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

    private void UpdatePikeAnimation()
    {
        if (_pikeAnimation is null || _pikeEntry is null) return;
        var clip = _pike.Mind.Mode switch
        {
            PikeMode.Chase => _pikeEntry.FastClip,
            PikeMode.Commit when _pike.Mind.StrikeReady => _pikeEntry.FastClip,
            PikeMode.Investigate or PikeMode.Search => _pikeEntry.SwimClip,
            _ => _pikeEntry.GlideClip,
        };
        if (_pikeAnimation.CurrentAnimation != clip) _pikeAnimation.Play(clip);
    }

    private void Leave(string scene)
    {
        if (_transitioning) return;
        PikeSight?.Reset();
        MovementCues?.Reset();
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
}
