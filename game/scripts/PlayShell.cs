using Godot;

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

    public PlayShell(FishController fish, Label help, DisplayPreferences preferences)
    {
        _fish = fish;
        _help = help;
        _preferences = preferences;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Ready()
    {
        _fish.ConfigureMouse(_preferences.MouseSensitivity, _preferences.InvertMouseY);
        _fish.SetInputMode(_preferences.InputMode);
        Input.MouseMode = _preferences.InputMode == DisplayPreferences.MouseMode
            ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
        if (_preferences.InputMode == DisplayPreferences.MouseMode) _fish.ResetMouseCapture();

        var canvas = new CanvasLayer();
        AddChild(canvas);
        _options = new Button
        {
            Name = "OptionsButton", Text = "Options", FocusMode = Control.FocusModeEnum.All,
            Visible = _preferences.InputMode == DisplayPreferences.KeyboardMode,
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        ApplyFocusStyle(_options);
        _options.Pressed += Pause;
        canvas.AddChild(_options);
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
        var mode = new OptionButton { Name = "InputModeOption", FocusMode = Control.FocusModeEnum.All };
        mode.AddItem("Keyboard", 0);
        mode.AddItem("Mouse only", 1);
        mode.Select(_preferences.InputMode == DisplayPreferences.MouseMode ? 1 : 0);
        ApplyFocusStyle(mode);
        mode.ItemSelected += index =>
        {
            var choice = index == 1 ? DisplayPreferences.MouseMode : DisplayPreferences.KeyboardMode;
            _fish.SetInputMode(choice);
            _preferences.ChooseInputMode(choice);
            UpdateHelp();
        };
        items.AddChild(mode);

        _sensitivityLabel = new Label();
        items.AddChild(_sensitivityLabel);
        var sensitivity = new HSlider
        {
            Name = "MouseSensitivitySlider", MinValue = 0.04, MaxValue = 0.4,
            Step = 0.01, Value = _preferences.MouseSensitivity,
            FocusMode = Control.FocusModeEnum.All
        };
        ApplyFocusStyle(sensitivity);
        sensitivity.ValueChanged += value =>
        {
            _preferences.ChooseMouseSensitivity((float)value);
            _fish.ConfigureMouse(_preferences.MouseSensitivity, _preferences.InvertMouseY);
            _fish.ClearAndDisarmInput();
            UpdateSensitivityLabel();
        };
        items.AddChild(sensitivity);

        var invert = new CheckButton
        {
            Name = "InvertMouseYToggle", Text = "Invert mouse vertical",
            ButtonPressed = _preferences.InvertMouseY,
            FocusMode = Control.FocusModeEnum.All
        };
        ApplyFocusStyle(invert);
        invert.Toggled += value =>
        {
            _preferences.ChooseInvertMouseY(value);
            _fish.ConfigureMouse(_preferences.MouseSensitivity, value);
            _fish.ClearAndDisarmInput();
        };
        items.AddChild(invert);

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

        GetViewport().SizeChanged += UpdateLayout;
        UpdateSensitivityLabel();
        UpdateLayout();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.Escape)
        {
            if (_overlay.Visible) Resume();
            else Pause();
            GetViewport().SetInputAsHandled();
        }
        else if (@event is InputEventMouseButton button && button.Pressed &&
                 button.ButtonIndex == MouseButton.Right &&
                 _preferences.InputMode == DisplayPreferences.MouseMode)
        {
            if (!_overlay.Visible) Pause();
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _Notification(int what)
    {
        if ((what == (int)NotificationApplicationFocusOut || what == (int)NotificationWMWindowFocusOut) && IsNodeReady())
            Pause();
    }

    private void Pause()
    {
        _fish.ClearAndDisarmInput();
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
        GetViewport().SetInputAsHandled();
        _fish.ClearAndDisarmInput();
        _overlay.Visible = false;
        _options.Visible = _preferences.InputMode == DisplayPreferences.KeyboardMode;
        GetTree().Paused = false;
        Input.MouseMode = _preferences.InputMode == DisplayPreferences.MouseMode
            ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
        if (_preferences.InputMode == DisplayPreferences.MouseMode) _fish.ResetMouseCapture();
        UpdateHelp();
        _resume.ReleaseFocus();
    }

    private void UpdateLayout()
    {
        var viewport = GetViewport().GetVisibleRect().Size;
        // CanvasLayer is not a Control parent, so the overlay needs an explicit viewport size.
        _overlay.Position = Vector2.Zero;
        _overlay.Size = viewport;
        _options.Position = new Vector2(Mathf.Max(0, viewport.X - 112), 12);
        _options.Size = new Vector2(96, 34);
        _panel.CustomMinimumSize = new Vector2(Mathf.Min(480, Mathf.Max(240, viewport.X - 32)), 0);
        UpdateHelp();
    }

    private void UpdateHelp()
    {
        var viewport = GetViewport().GetVisibleRect().Size;
        _help.Visible = _preferences.ShowControlHelp && !_overlay.Visible;
        _help.Text = _preferences.InputMode == DisplayPreferences.MouseMode
            ? "Mouse: move to turn/pitch   Hold left: swim   Right: pause/options"
            : "W: thrust   S: slow   A/D: turn   Up/Down: pitch   Esc: pause/options";
        _help.Position = new Vector2(20, 16);
        _help.Size = new Vector2(Mathf.Max(200, viewport.X - 40), 80);
        _help.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _help.AddThemeFontSizeOverride("font_size", viewport.Y < 700 ? 16 : 19);
    }

    private void UpdateSensitivityLabel() =>
        _sensitivityLabel.Text = $"Mouse sensitivity: {_preferences.MouseSensitivity:F2}° per pixel";

    private static void ApplyFocusStyle(Control control)
    {
        var focus = new StyleBoxFlat { BgColor = Colors.Transparent, BorderColor = new Color(1.0f, 0.85f, 0.3f) };
        focus.SetBorderWidthAll(3);
        control.AddThemeStyleboxOverride("focus", focus);
    }
}
