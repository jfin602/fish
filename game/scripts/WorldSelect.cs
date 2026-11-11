#nullable enable
using Godot;
using System;
using System.IO;

namespace Fish;

/// <summary>Session-only front door for the P4 tech demo.</summary>
public partial class WorldSelect : Control
{
    private readonly DisplayPreferences _preferences = new();
    private readonly Button[] _cards = new Button[WorldCatalog.All.Count];
    private GridContainer _grid = null!;
    private Button _choose = null!;
    private Button _back = null!;
    private Label _status = null!;
    private int _selected;
    private string _error = "";
    private bool _loading;
    private bool _invalidDisplay;
    private static bool _checkStarted;

    public override void _EnterTree()
    {
        _preferences.Load();
        var mode = _preferences.SavedDisplayMode;
        foreach (var arg in OS.GetCmdlineUserArgs())
        {
            if (!arg.StartsWith("--display-mode=", StringComparison.Ordinal)) continue;
            mode = arg["--display-mode=".Length..];
            if (DisplayPreferences.IsSupportedDisplayMode(mode)) continue;
            GD.PushError($"Unsupported display mode: {mode}");
            _invalidDisplay = true;
            GetTree().Quit(2);
            return;
        }
        _preferences.ActiveDisplayMode = mode;
        GetWindow().Mode = DisplayPreferences.ToWindowMode(mode);
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GetTree().Paused = false;
        WorldSession.Unload();
        DemoSelection.Clear();
        GD.Print($"FISH_DISPLAY_MODE requested={mode} source=world-select effective={GetWindow().Mode}");
    }

    public override void _Ready()
    {
        if (_invalidDisplay) return;
        Name = "WorldSelect";
        SetAnchorsPreset(LayoutPreset.TopLeft);
        MouseFilter = MouseFilterEnum.Pass;
        Resize();
        GetViewport().SizeChanged += Resize;
        BuildInterface();
        Select(0);
        _choose.GrabFocus();
        GD.Print("FISH_WORLD_SELECT_READY entries=2");
        if (!_checkStarted && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--world-select-check"))
        {
            _checkStarted = true;
            WorldSelectChecks.Run(this);
        }
    }

    public override void _ExitTree() => GetViewport().SizeChanged -= Resize;

    public override void _Input(InputEvent @event)
    {
        if (_loading || @event is not InputEventKey { Pressed: true, Echo: false } key) return;
        if (key.Keycode is Key.Left or Key.A)
        {
            Select((_selected + _cards.Length - 1) % _cards.Length);
            GetViewport().SetInputAsHandled();
        }
        else if (key.Keycode is Key.Right or Key.D)
        {
            Select((_selected + 1) % _cards.Length);
            GetViewport().SetInputAsHandled();
        }
        else if (key.Keycode == Key.Escape)
        {
            GetViewport().SetInputAsHandled();
            Back();
        }
        else if (key.Keycode == Key.Enter && GetViewport().GuiGetFocusOwner() is Button focus)
        {
            for (var i = 0; i < _cards.Length; i++)
                if (focus == _cards[i])
                {
                    Select(i);
                    GetViewport().SetInputAsHandled();
                    return;
                }
            if (focus == _choose)
            {
                GetViewport().SetInputAsHandled();
                Choose();
            }
        }
    }

    public override void _Notification(int what)
    {
        if (what is (int)NotificationApplicationFocusOut or (int)NotificationWMWindowFocusOut)
            Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void Resize()
    {
        Size = GetViewport().GetVisibleRect().Size;
        if (_grid is not null) _grid.Columns = Size.X < 900 ? 1 : 2;
    }

    private void BuildInterface()
    {
        var background = new ColorRect { Color = new Color(.018f, .085f, .11f), MouseFilter = MouseFilterEnum.Ignore };
        AddChild(background);
        Fill(background);
        var margin = new MarginContainer();
        AddChild(margin);
        Fill(margin);
        foreach (var side in new[] { "left", "right" }) margin.AddThemeConstantOverride("margin_" + side, 28);
        margin.AddThemeConstantOverride("margin_top", 24);
        margin.AddThemeConstantOverride("margin_bottom", 24);
        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", 18);
        margin.AddChild(column);
        column.AddChild(Text("FISH  /  WORLD SELECT", 32, Colors.White));
        column.AddChild(Text("Choose a habitat for this demo session", 19, new Color(.67f, .86f, .85f)));

        _grid = new GridContainer { Name = "WorldCards", Columns = Size.X < 900 ? 1 : 2,
            SizeFlagsVertical = SizeFlags.ExpandFill };
        _grid.AddThemeConstantOverride("h_separation", 18);
        _grid.AddThemeConstantOverride("v_separation", 18);
        column.AddChild(_grid);
        for (var i = 0; i < _cards.Length; i++)
        {
            var index = i;
            var world = WorldCatalog.All[i];
            var card = new Button { Name = "World_" + world.Id, FocusMode = FocusModeEnum.All,
                Alignment = HorizontalAlignment.Left, CustomMinimumSize = new Vector2(270, 240),
                SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
            var color = world.Preview.Accent;
            var normal = new StyleBoxFlat { BgColor = color.Darkened(.62f), BorderColor = color.Lightened(.24f) };
            normal.SetBorderWidthAll(2);
            normal.SetCornerRadiusAll(12);
            card.AddThemeStyleboxOverride("normal", normal);
            var focus = new StyleBoxFlat { BgColor = color.Darkened(.45f), BorderColor = new Color(1, .88f, .48f) };
            focus.SetBorderWidthAll(5);
            focus.SetCornerRadiusAll(12);
            card.AddThemeStyleboxOverride("focus", focus);
            card.AddThemeStyleboxOverride("hover", focus);
            card.Pressed += () => Select(index);
            card.FocusEntered += () => Select(index);
            card.MouseEntered += () => Select(index);
            _grid.AddChild(card);
            _cards[i] = card;
        }
        _status = Text("", 17, new Color(1, .85f, .6f));
        _status.Name = "WorldStatus";
        _status.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        column.AddChild(_status);
        var actions = new HBoxContainer();
        column.AddChild(actions);
        _back = new Button { Name = "WorldBackButton", Text = "Back / Quit demo", CustomMinimumSize = new Vector2(170, 48) };
        FocusStyle(_back);
        _back.Pressed += Back;
        actions.AddChild(_back);
        actions.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore });
        _choose = new Button { Name = "ChooseWorldButton", Text = "Choose world", CustomMinimumSize = new Vector2(190, 48) };
        FocusStyle(_choose);
        _choose.Pressed += Choose;
        actions.AddChild(_choose);
    }

    private void Select(int index)
    {
        if (_loading) return;
        if (index != _selected) _error = "";
        _selected = index;
        _status.Text = _error.Length > 0 ? _error :
            "← → browse  •  Tab changes focus  •  Enter chooses the focused action";
        for (var i = 0; i < _cards.Length; i++)
        {
            var world = WorldCatalog.All[i];
            _cards[i].Text = (i == index ? "✓ SELECTED\n\n" : "  SELECT\n\n") +
                world.DisplayName + "\n\n" + world.Preview.Description +
                "\n\nPreview art pending" + (world.Bake is null ? "\nWorld bake pending" : "");
        }
    }

    private void Choose()
    {
        if (_loading) return;
        _loading = true;
        _error = "";
        _choose.Disabled = true;
        _back.Disabled = true;
        _status.Text = $"Loading {WorldCatalog.All[_selected].DisplayName}…";
        CallDeferred(MethodName.CompleteChoice);
    }

    private void CompleteChoice()
    {
        var world = WorldCatalog.All[_selected];
        try
        {
            WorldSession.Select(world.Id);
            WorldCatalog.VerifyInstalledManifest(world);
            world.LoadHabitatScene();
            if (world.Id != WorldCatalog.OceanId)
                throw new InvalidDataException($"WORLD_CHARACTERS_UNAVAILABLE world={world.Id} action=complete-p4-freshwater-fish");
            DemoSelection.Clear();
            Input.MouseMode = Input.MouseModeEnum.Visible;
            GetTree().ChangeSceneToFile("res://scenes/character_select.tscn");
        }
        catch (Exception error) when (error is IOException or InvalidDataException)
        {
            _error = $"{world.DisplayName} is unavailable: {error.Message}";
            _status.Text = _error;
            _loading = false;
            _choose.Disabled = false;
            _back.Disabled = false;
            _choose.GrabFocus();
        }
    }

    private void Back()
    {
        if (_loading) return;
        DemoSelection.Clear();
        WorldSession.Unload();
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GetTree().Quit();
    }

    private static Label Text(string text, int size, Color color)
    {
        var label = new Label { Text = text, Modulate = color, MouseFilter = MouseFilterEnum.Ignore };
        label.AddThemeFontSizeOverride("font_size", size);
        return label;
    }

    private static void FocusStyle(Button button)
    {
        var focus = new StyleBoxFlat { BgColor = Colors.Transparent, BorderColor = new Color(1, .88f, .48f) };
        focus.SetBorderWidthAll(3);
        focus.SetCornerRadiusAll(5);
        button.AddThemeStyleboxOverride("focus", focus);
    }

    private static void Fill(Control control)
    {
        control.AnchorLeft = 0; control.AnchorTop = 0; control.AnchorRight = 1; control.AnchorBottom = 1;
        control.OffsetLeft = 0; control.OffsetTop = 0; control.OffsetRight = 0; control.OffsetBottom = 0;
    }
}
