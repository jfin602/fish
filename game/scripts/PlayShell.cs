using Godot;

namespace Fish;

/// <summary>Small test-session shell around the provisional P0 controls.</summary>
public partial class PlayShell : Node
{
    private readonly FishController _fish;
    private readonly Label _help;
    private readonly DisplayPreferences _preferences;
    private Control _overlay = null!;
    private PanelContainer _panel = null!;
    private Button _resume = null!;

    public PlayShell(FishController fish, Label help, DisplayPreferences preferences)
    {
        _fish = fish;
        _help = help;
        _preferences = preferences;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Ready()
    {
        var canvas = new CanvasLayer();
        AddChild(canvas);
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
        margin.AddThemeConstantOverride("margin_left", 24);
        margin.AddThemeConstantOverride("margin_right", 24);
        margin.AddThemeConstantOverride("margin_top", 20);
        margin.AddThemeConstantOverride("margin_bottom", 20);
        _panel.AddChild(margin);
        var items = new VBoxContainer();
        items.AddThemeConstantOverride("separation", 14);
        margin.AddChild(items);

        var title = new Label { Text = "Paused", HorizontalAlignment = HorizontalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size", 28);
        items.AddChild(title);

        var displayLabel = new Label { Text = "Display" };
        displayLabel.AddThemeFontSizeOverride("font_size", 19);
        items.AddChild(displayLabel);
        var display = new OptionButton { FocusMode = Control.FocusModeEnum.All };
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

        var showHelp = new CheckButton
        {
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

        _resume = new Button { Text = "Resume", FocusMode = Control.FocusModeEnum.All };
        ApplyFocusStyle(_resume);
        _resume.Pressed += Resume;
        items.AddChild(_resume);
        var quit = new Button { Text = "Quit test session", FocusMode = Control.FocusModeEnum.All };
        ApplyFocusStyle(quit);
        quit.Pressed += () => GetTree().Quit();
        items.AddChild(quit);

        GetViewport().SizeChanged += UpdateLayout;
        UpdateLayout();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey key || !key.Pressed || key.Echo || key.Keycode != Key.Escape) return;
        if (_overlay.Visible) Resume();
        else Pause();
        GetViewport().SetInputAsHandled();
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
        UpdateHelp();
        _resume.GrabFocus();
    }

    private void Resume()
    {
        GetViewport().SetInputAsHandled();
        _fish.ClearAndDisarmInput();
        _overlay.Visible = false;
        GetTree().Paused = false;
        UpdateHelp();
        _resume.ReleaseFocus();
    }

    private void UpdateLayout()
    {
        var viewport = GetViewport().GetVisibleRect().Size;
        // CanvasLayer is not a Control parent, so the overlay needs an explicit viewport size.
        _overlay.Position = Vector2.Zero;
        _overlay.Size = viewport;
        _panel.CustomMinimumSize = new Vector2(Mathf.Min(480, Mathf.Max(240, viewport.X - 32)), 0);
        UpdateHelp();
    }

    private void UpdateHelp()
    {
        var viewport = GetViewport().GetVisibleRect().Size;
        _help.Visible = _preferences.ShowControlHelp && !_overlay.Visible;
        _help.Text = "W: thrust   S: slow   A/D: turn   Up/Down: pitch   Esc: pause/options";
        _help.Position = new Vector2(20, 16);
        _help.Size = new Vector2(Mathf.Max(200, viewport.X - 40), 80);
        _help.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _help.AddThemeFontSizeOverride("font_size", viewport.Y < 700 ? 16 : 19);
    }

    private static void ApplyFocusStyle(Control control)
    {
        var focus = new StyleBoxFlat { BgColor = Colors.Transparent, BorderColor = new Color(1.0f, 0.85f, 0.3f) };
        focus.SetBorderWidthAll(3);
        control.AddThemeStyleboxOverride("focus", focus);
    }
}
