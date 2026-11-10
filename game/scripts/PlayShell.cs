#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

/// <summary>Small test-session shell with explicit desktop mode and display ownership.</summary>
public partial class PlayShell : Node
{
    private readonly FishController _fish;
    private readonly Label _help;
    private readonly DisplayPreferences _preferences;
    private Control _overlay = null!;
    private PanelContainer _panel = null!;
    private Button _resume = null!;
    private Button _options = null!;
    private Label _sensitivityLabel = null!;
    private HSlider _legacySensitivity = null!;
    private CheckButton _legacyInvert = null!;
    private OptionButton _mode = null!;
    private ControlPresetsPanel _presetEditor = null!;
    private GraphicsPanel _graphicsPanel = null!;
    private readonly LightingPresetStore _lightingPresets = new();
    private Label? _quickPresetLabel;
    private double _quickPresetSeconds;
    private readonly ControlPresetStore _presets = new();
    private readonly List<ControlPreset> _profileChoices = [];

    public PlayShell(FishController fish, Label help, DisplayPreferences preferences)
    {
        _fish = fish;
        _help = help;
        _preferences = preferences;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Ready()
    {
        _presets.Load();
        _lightingPresets.Load();
        _fish.ConfigureMouse(_preferences.MouseSensitivity, _preferences.InvertMouseY);
        _fish.SetActivePreset(_presets.Current);
        Input.MouseMode = _fish.UsesMouseInput
            ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
        if (_fish.UsesMouseInput) _fish.ResetMouseCapture();

        var canvas = new CanvasLayer { Layer = 10 };
        AddChild(canvas);
        _options = new Button
        {
            Name = "OptionsButton", Text = "Options (Middle)", FocusMode = Control.FocusModeEnum.All,
            Visible = true,
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        ApplyFocusStyle(_options);
        _options.Pressed += Pause;
        canvas.AddChild(_options);
        if (OS.IsDebugBuild())
        {
            _quickPresetLabel = new Label
            {
                Name = "QuickPresetLabel", Visible = false,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            _quickPresetLabel.AddThemeFontSizeOverride("font_size", 18);
            canvas.AddChild(_quickPresetLabel);
        }
        _overlay = new Control { Visible = false, MouseFilter = Control.MouseFilterEnum.Ignore };
        canvas.AddChild(_overlay);

        var shade = new ColorRect
        {
            Color = new Color(0.02f, 0.08f, 0.13f, 0.82f),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        _overlay.AddChild(shade);
        shade.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        var center = new CenterContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
        _overlay.AddChild(center);
        center.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _panel = new PanelContainer { MouseFilter = Control.MouseFilterEnum.Stop };
        center.AddChild(_panel);
        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 16);
        margin.AddThemeConstantOverride("margin_right", 16);
        margin.AddThemeConstantOverride("margin_top", 12);
        margin.AddThemeConstantOverride("margin_bottom", 12);
        _panel.AddChild(margin);
        var items = new VBoxContainer();
        items.AddThemeConstantOverride("separation", 6);
        margin.AddChild(items);

        var title = new Label { Text = "Paused", HorizontalAlignment = HorizontalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size", 23);
        items.AddChild(title);

        var displayLabel = new Label { Text = "Display" };
        displayLabel.AddThemeFontSizeOverride("font_size", 16);
        items.AddChild(displayLabel);
        var display = new OptionButton { Name = "DisplayModeOption", FocusMode = Control.FocusModeEnum.All };
        display.AddItem("Fullscreen", 0);
        display.AddItem("Windowed fullscreen", 1);
        display.Select(_preferences.ActiveDisplayMode == DisplayPreferences.Fullscreen ? 0 : 1);
        ApplyFocusStyle(display);
        display.ItemSelected += index =>
        {
            _fish.ClearAndDisarmInput();
            var mode = index == 0 ? DisplayPreferences.Fullscreen : DisplayPreferences.WindowedFullscreen;
            GetWindow().Mode = DisplayPreferences.ToWindowMode(mode);
            _preferences.ChooseDisplayMode(mode);
            UpdateLayout();
        };
        items.AddChild(display);

        var modeLabel = new Label { Text = "Swim controls" };
        modeLabel.AddThemeFontSizeOverride("font_size", 16);
        items.AddChild(modeLabel);
        _mode = new OptionButton { Name = "InputModeOption", FocusMode = Control.FocusModeEnum.All };
        RefreshModeChoices();
        ApplyFocusStyle(_mode);
        _mode.ItemSelected += index =>
        {
            if (index < 0 || index >= _profileChoices.Count) return;
            TryChooseProfile(_profileChoices[(int)index]);
        };
        items.AddChild(_mode);
        var presetsButton = new Button { Name = "ControlPresetsButton", Text = "Control Presets…", FocusMode = Control.FocusModeEnum.All };
        ApplyFocusStyle(presetsButton);
        presetsButton.Pressed += () =>
        {
            _panel.Visible = false;
            _presetEditor.Open();
            _fish.ClearAndDisarmInput();
        };
        items.AddChild(presetsButton);
        var graphicsButton = new Button { Name = "GfxButton", Text = "GFX…", FocusMode = Control.FocusModeEnum.All };
        ApplyFocusStyle(graphicsButton);
        graphicsButton.Pressed += () =>
        {
            _panel.Visible = false;
            _graphicsPanel.Open();
            _fish.ClearAndDisarmInput();
        };
        items.AddChild(graphicsButton);

        _sensitivityLabel = new Label();
        items.AddChild(_sensitivityLabel);
        _legacySensitivity = new HSlider
        {
            Name = "MouseSensitivitySlider", MinValue = 0.04, MaxValue = 0.4,
            Step = 0.01, Value = _preferences.MouseSensitivity,
            FocusMode = Control.FocusModeEnum.All
        };
        ApplyFocusStyle(_legacySensitivity);
        _legacySensitivity.ValueChanged += value =>
        {
            _preferences.ChooseMouseSensitivity((float)value);
            _fish.ConfigureMouse(_preferences.MouseSensitivity, _preferences.InvertMouseY);
            _fish.ClearAndDisarmInput();
            UpdateSensitivityLabel();
        };
        items.AddChild(_legacySensitivity);

        _legacyInvert = new CheckButton
        {
            Name = "InvertMouseYToggle", Text = "Invert mouse vertical",
            ButtonPressed = _preferences.InvertMouseY,
            FocusMode = Control.FocusModeEnum.All
        };
        ApplyFocusStyle(_legacyInvert);
        _legacyInvert.Toggled += value =>
        {
            _preferences.ChooseInvertMouseY(value);
            _fish.ConfigureMouse(_preferences.MouseSensitivity, value);
            _fish.ClearAndDisarmInput();
        };
        items.AddChild(_legacyInvert);

        var showHelp = new CheckButton
        {
            Name = "ShowHelpToggle",
            Text = "Show swimming controls",
            ButtonPressed = _preferences.ShowControlHelp,
            FocusMode = Control.FocusModeEnum.All
        };
        ApplyFocusStyle(showHelp);
        showHelp.Toggled += on =>
        {
            _preferences.ChooseControlHelp(on);
            UpdateHelp();
        };
        items.AddChild(showHelp);

        _resume = new Button { Name = "ResumeButton", Text = "Resume", FocusMode = Control.FocusModeEnum.All };
        ApplyFocusStyle(_resume);
        _resume.Pressed += Resume;
        items.AddChild(_resume);
        var quit = new Button { Name = "QuitButton", Text = "Quit test session", FocusMode = Control.FocusModeEnum.All };
        ApplyFocusStyle(quit);
        quit.Pressed += () => GetTree().Quit();
        items.AddChild(quit);

        _presetEditor = new ControlPresetsPanel(_presets, () => _fish.ActivePreset, profile =>
        {
            ActivateProfile(profile);
            RefreshModeChoices();
        }, () =>
        {
            _panel.Visible = true;
            RefreshModeChoices();
            _resume.GrabFocus();
        });
        center.AddChild(_presetEditor);
        _graphicsPanel = new GraphicsPanel(_lightingPresets,
            () => GetParent()?.GetNodeOrNull<UnderwaterAtmosphere>("UnderwaterAtmosphere"), () =>
            {
                _panel.Visible = true;
                _resume.GrabFocus();
            });
        center.AddChild(_graphicsPanel);

        GetViewport().SizeChanged += UpdateLayout;
        UpdateLegacyMouseControls();
        UpdateLayout();
    }

    public override void _Input(InputEvent @event)
    {
        if (_overlay.Visible && _graphicsPanel.Visible)
        {
            if (@event is InputEventKey { Pressed: true, Echo: false, PhysicalKeycode: Key.Escape })
            {
                _graphicsPanel.RequestClose();
                GetViewport().SetInputAsHandled();
            }
            return;
        }
        if (_overlay.Visible && _presetEditor.Visible)
        {
            if (_presetEditor.HandleModalInput(@event))
            {
                GetViewport().SetInputAsHandled();
                return;
            }
            if (@event is InputEventKey { Pressed: true, Echo: false, PhysicalKeycode: Key.Escape })
            {
                _presetEditor.RequestClose();
                GetViewport().SetInputAsHandled();
                return;
            }
            return;
        }
        if (CanQuickCycle() && !_overlay.Visible &&
            @event is InputEventKey { Pressed: true, Echo: false, PhysicalKeycode: Key.F6 })
        {
            RefreshModeChoices();
            var current = _profileChoices.FindIndex(profile => profile.Id == _presets.CurrentId);
            TryChooseProfile(_profileChoices[(current + 1) % _profileChoices.Count]);
            GetViewport().SetInputAsHandled();
            return;
        }
        if (_fish.IsPauseEvent(@event))
        {
            // Mouse clicks in an open menu belong to Controls, even if the active
            // profile binds that button to Pause/Options during gameplay.
            if (_overlay.Visible && @event is InputEventMouseButton) return;
            if (_overlay.Visible && @event is InputEventKey) Resume();
            else if (!_overlay.Visible) Pause();
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _Process(double delta)
    {
        if (_quickPresetLabel is null || !_quickPresetLabel.Visible) return;
        _quickPresetSeconds -= delta;
        if (_quickPresetSeconds <= 0) _quickPresetLabel.Visible = false;
    }

    public override void _Notification(int what)
    {
        if ((what == (int)NotificationApplicationFocusOut || what == (int)NotificationWMWindowFocusOut) && IsNodeReady())
        {
            _presetEditor.CancelModal();
            Pause();
        }
    }

    private void Pause()
    {
        _fish.ClearAndDisarmInput();
        _fish.GetNodeOrNull<WaterReaction>("WaterReaction")?.Suspend();
        if (_overlay.Visible) return;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GetTree().Paused = true;
        _overlay.Visible = true;
        _options.Visible = false;
        UpdateHelp();
        _resume.GrabFocus();
    }

    private void Resume()
    {
        if (_graphicsPanel.Visible)
        {
            _graphicsPanel.Hide();
            _panel.Visible = true;
        }
        if (_presetEditor.Visible)
        {
            _presetEditor.CancelModal();
            _presetEditor.Hide();
            _panel.Visible = true;
        }
        GetViewport().SetInputAsHandled();
        _fish.ClearAndDisarmInput();
        _overlay.Visible = false;
        _options.Visible = true;
        GetTree().Paused = false;
        Input.MouseMode = _fish.UsesMouseInput
            ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
        if (_fish.UsesMouseInput) _fish.ResetMouseCapture();
        UpdateHelp();
        _resume.ReleaseFocus();
    }

    private void UpdateLayout()
    {
        var viewport = GetViewport().GetVisibleRect().Size;
        // CanvasLayer is not a Control parent, so the overlay needs an explicit viewport size.
        _overlay.Position = Vector2.Zero;
        _overlay.Size = viewport;
        _options.Position = new Vector2(Mathf.Max(0, viewport.X - 164), 12);
        _options.Size = new Vector2(148, 34);
        _panel.CustomMinimumSize = new Vector2(Mathf.Min(480, Mathf.Max(240, viewport.X - 32)), 0);
        if (_quickPresetLabel is not null)
        {
            _quickPresetLabel.Position = new Vector2(20, Mathf.Max(12, viewport.Y - 48));
            _quickPresetLabel.Size = new Vector2(Mathf.Max(200, viewport.X - 40), 32);
        }
        _presetEditor.CustomMinimumSize = new Vector2(Mathf.Min(760, Mathf.Max(310, viewport.X - 24)),
            Mathf.Min(650, Mathf.Max(300, viewport.Y - 24)));
        _graphicsPanel.CustomMinimumSize = new Vector2(Mathf.Min(660, Mathf.Max(310, viewport.X - 24)),
            Mathf.Min(720, Mathf.Max(300, viewport.Y - 24)));
        UpdateHelp();
    }

    private void UpdateHelp()
    {
        var viewport = GetViewport().GetVisibleRect().Size;
        _help.Visible = _preferences.ShowControlHelp && !_overlay.Visible;
        var profile = _fish.ActivePreset;
        string Keys(ControlAction action) => string.Join("/", profile.Bindings(action).Select(ControlPresetsPanel.BindingName));
        string Axis(SteeringAxis axis) =>
            (profile.MouseX.Steering == axis ? "Mouse X" : "") +
            (profile.MouseY.Steering == axis ? (profile.MouseX.Steering == axis ? "/Mouse Y" : "Mouse Y") : "");
        string Steering(ControlAction left, ControlAction right, SteeringAxis axis)
        {
            var keys = Keys(left).Length == 0 && Keys(right).Length == 0
                ? "" : $"{Keys(left)}/{Keys(right)}";
            return string.Join(" + ", new[] { keys, Axis(axis) }.Where(part => part.Length > 0));
        }
        var otherPause = string.Join("/", profile.Bindings(ControlAction.PauseOptions)
            .Where(binding => binding != new ControlBinding(BindingDevice.PhysicalKey, ControlPreset.RecoveryKey))
            .Select(ControlPresetsPanel.BindingName));
        _help.Text = profile.CameraControl == CameraControlPolicy.AdaptiveFollow
            ? $"{profile.Name}  Hold Left Shift + mouse: 360 free-look  " +
              $"Turn: {Steering(ControlAction.YawLeft, ControlAction.YawRight, SteeringAxis.Yaw)}  " +
              $"Pitch: {Steering(ControlAction.PitchUp, ControlAction.PitchDown, SteeringAxis.Pitch)}  " +
              $"Thrust: {Keys(ControlAction.Thrust)}  Slow: {Keys(ControlAction.Slow)}  " +
              $"Wheel: zoom  Pause: Esc{(otherPause.Length == 0 ? "" : "/" + otherPause)}" +
              (CanQuickCycle() ? "  Debug: F6 cycles presets" : "")
            : profile.CameraControl == CameraControlPolicy.MouseOrbit360
            ? $"{profile.Name}  Mouse: 360 camera  Pitch up/down: {Keys(ControlAction.PitchUp)}/{Keys(ControlAction.PitchDown)}  " +
              $"Turn left/right: {Keys(ControlAction.YawLeft)}/{Keys(ControlAction.YawRight)}  " +
              $"Thrust: {Keys(ControlAction.Thrust)}  Slow: {Keys(ControlAction.Slow)}  " +
              $"Wheel: zoom  Options: Esc{(otherPause.Length == 0 ? "" : "/" + otherPause)}" +
              (CanQuickCycle() ? "  Debug: F6 cycles presets" : "")
            : $"{profile.Name}  Swim: {Keys(ControlAction.Thrust)}  Slow: {Keys(ControlAction.Slow)}  " +
              $"Turn: {Steering(ControlAction.YawLeft, ControlAction.YawRight, SteeringAxis.Yaw)}  " +
              $"Pitch: {Steering(ControlAction.PitchUp, ControlAction.PitchDown, SteeringAxis.Pitch)}  " +
              $"Wheel: zoom  Pause: Esc{(otherPause.Length == 0 ? "" : "/" + otherPause)}" +
              (CanQuickCycle() ? "  Debug: F6 cycles presets" : "");
        _help.Position = new Vector2(20, 60);
        _help.Size = new Vector2(Mathf.Max(200, viewport.X - 40), 120);
        _help.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _help.AddThemeFontSizeOverride("font_size", viewport.Y < 700 ? 16 : 19);
    }

    public void ApplySavedLighting() =>
        GetParent()?.GetNodeOrNull<UnderwaterAtmosphere>("UnderwaterAtmosphere")?.ApplyLighting(_lightingPresets.Current.Settings);

    private void UpdateSensitivityLabel() =>
        _sensitivityLabel.Text = _presets.Current.IsBuiltin
            ? $"Mouse sensitivity: {_preferences.MouseSensitivity:F2}° per pixel"
            : "Custom mouse settings: edit in Control Presets";

    private bool CanQuickCycle()
    {
        if (!OS.IsDebugBuild()) return false;
        // A user binding wins throughout the cycle, including in a profile not yet active.
        var f6 = new ControlBinding(BindingDevice.PhysicalKey, (int)Key.F6);
        return !_presets.List().Any(profile => Enum.GetValues<ControlAction>()
            .Any(action => profile.Bindings(action).Contains(f6)));
    }

    private void UpdateLegacyMouseControls()
    {
        _legacySensitivity.Editable = _presets.Current.IsBuiltin;
        _legacyInvert.Disabled = !_presets.Current.IsBuiltin;
        UpdateSensitivityLabel();
    }

    private void ActivateProfile(ControlPreset profile)
    {
        _fish.SetActivePreset(profile);
        _preferences.ChooseInputMode(_fish.ActiveInputMode);
        UpdateLegacyMouseControls();
        UpdateHelp();
    }

    private bool TryChooseProfile(ControlPreset profile)
    {
        var previous = _presets.CurrentId;
        try
        {
            _presets.Choose(profile.Id);
            _presets.Save();
        }
        catch (Exception error) when (error is ArgumentException or System.IO.IOException or UnauthorizedAccessException)
        {
            _presets.Choose(previous);
            RefreshModeChoices();
            _sensitivityLabel.Text = "Control selection could not be saved: " + error.Message;
            return false;
        }
        ActivateProfile(profile);
        RefreshModeChoices();
        if (_quickPresetLabel is not null && !_overlay.Visible)
        {
            _quickPresetLabel.Text = $"Active controls: {profile.Name}";
            _quickPresetLabel.Visible = true;
            _quickPresetSeconds = 2.5;
        }
        return true;
    }

    private void RefreshModeChoices()
    {
        _profileChoices.Clear();
        var all = _presets.List();
        foreach (var id in new[] { ControlPreset.KeyboardId, ControlPreset.MouseId,
                     ControlPreset.HybridId, ControlPreset.Hybrid2Id, ControlPreset.AdaptiveFollowId })
            _profileChoices.Add(all.First(profile => profile.Id == id));
        _profileChoices.AddRange(all.Where(profile => !profile.IsBuiltin));
        _mode.Clear();
        for (var i = 0; i < _profileChoices.Count; i++)
            _mode.AddItem(_profileChoices[i].Name, i);
        _mode.Select(_profileChoices.FindIndex(profile => profile.Id == _presets.CurrentId));
    }

    private static void ApplyFocusStyle(Control control)
    {
        var focus = new StyleBoxFlat { BgColor = Colors.Transparent, BorderColor = new Color(1.0f, 0.85f, 0.3f) };
        focus.SetBorderWidthAll(3);
        control.AddThemeStyleboxOverride("focus", focus);
    }
}
