#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace Fish;

/// <summary>Session-local P5 entry fixture. The Pike is a stationary actor until later tasks add AI.</summary>
public partial class PikeEncounter : Node3D
{
    private FishController _fish = null!;
    private Label _status = null!;
    private Control _menu = null!;
    private Button _retry = null!;
    private Button _resume = null!;
    private bool _transitioning;
    private bool _failed;
    public IReadOnlyList<PikeCoverPlacement> CoverPlan { get; private set; } = Array.Empty<PikeCoverPlacement>();
    public WorldSightSensor? PikeSight { get; private set; }
    public SightEnvironment? SightWater { get; private set; }

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
        var actor = GetNode<CharacterBody3D>("NorthernPike");
        pikeModel!.Name = "PikeVisual";
        actor.AddChild(pikeModel);
        pikeModel.Transform = pike!.PresentationTransform;
        animation!.Play(pike.GlideClip);
        var plants = PikeCoverFixture.VisibleSoftCover(GetNode<Node3D>("Cover"));
        PikeSight = new WorldSightSensor(GetWorld3D().DirectSpaceState, plants,
            actor.GetRid(), _fish.GetRid());
        SightWater = new SightEnvironment(WorldEnvironmentProfile.Pond, PikeCoverFixture.WaterY,
            PikeCoverFixture.FixtureSediment, SolarState.At(.5));
        var preferences = new DisplayPreferences();
        preferences.Load();
        _fish.ConfigureMouse(preferences.MouseSensitivity, preferences.InvertMouseY);
        var presets = new ControlPresetStore();
        presets.Load();
        _fish.SetActivePreset(presets.Current);
        _fish.ClearAndDisarmInput();
        Input.MouseMode = _fish.UsesMouseInput ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
        if (_fish.UsesMouseInput) _fish.ResetMouseCapture();
        _status.Text = "PIKE ENCOUNTER · Shallow grass → root refuge → deep wood maze · Pike stationary · Esc: pause";
        GD.Print($"PIKE_ENCOUNTER_READY player=FAF-FISH-006 pike=FAF-FISH-010 world=fixture cover={CoverPlan.Count} seed={PikeCoverFixture.Seed}");
    }

    private void Fail(string diagnostic)
    {
        PikeSight?.Reset();
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
        if (_transitioning || _failed || @event is not InputEventKey { Pressed: true, Echo: false } key ||
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
        if (_failed || _transitioning) return;
        _fish.ClearAndDisarmInput();
        _menu.Visible = false;
        GetTree().Paused = false;
        Input.MouseMode = _fish.UsesMouseInput ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
        if (_fish.UsesMouseInput) _fish.ResetMouseCapture();
    }

    private void Retry() => Leave(PikeEncounterAssets.ScenePath);
    private void ReturnToWorldSelect() => Leave("res://scenes/world_select.tscn");

    private void Leave(string scene)
    {
        if (_transitioning) return;
        PikeSight?.Reset();
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
