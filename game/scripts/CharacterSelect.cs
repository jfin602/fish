#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace Fish;

/// <summary>Lightweight, isolated art preview and tech-demo launch screen.</summary>
public partial class CharacterSelect : Control
{
    private static bool _checkStarted;
    private readonly DisplayPreferences _preferences = new();
    private IReadOnlyList<CharacterEntry> _entries = Array.Empty<CharacterEntry>();
    private string[] _diagnostics = Array.Empty<string>();
    private Button[] _tiles = Array.Empty<Button>();
    private Label _name = null!;
    private Label _category = null!;
    private Label _description = null!;
    private Label _status = null!;
    private Button _enter = null!;
    private ScrollContainer _gallery = null!;
    private SubViewport _preview = null!;
    private Node3D _turntable = null!;
    private Node3D? _model;
    private AnimationPlayer? _animation;
    private string _clip = "";
    private int _selected;
    private bool _dragging;
    private bool _transitioning;
    private bool _invalidDisplay;
    private string _shownWorldId = "";

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
        GD.Print($"FISH_DISPLAY_MODE requested={mode} source=character-select effective={GetWindow().Mode}");
    }

    public override void _Ready()
    {
        if (_invalidDisplay) return;
        Name = "CharacterSelect";
        SetAnchorsPreset(LayoutPreset.TopLeft);
        MouseFilter = MouseFilterEnum.Pass;
        UpdateScreenSize();
        GetViewport().SizeChanged += UpdateScreenSize;
        RebuildForWorld();
        if (!_checkStarted && Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--character-check"))
        {
            _checkStarted = true;
            CharacterChecks.Run(this);
        }
    }

    private void RebuildForWorld()
    {
        _transitioning = false;
        _shownWorldId = WorldSession.SelectedWorldId;
        _dragging = false;
        _model = null;
        _animation = null;
        _clip = "";
        foreach (var child in GetChildren()) child.Free();
        _entries = SpeciesCompatibility.Available(_shownWorldId, SpeciesRole.Selectable);
        _diagnostics = new string[_entries.Count];
        _tiles = new Button[_entries.Count];
        _selected = 0;
        BuildInterface();
        for (var i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            _diagnostics[i] = "";
            if (CharacterCatalog.TryInstantiate(entry, out var model, out _, out var diagnostic))
                model!.Free();
            else _diagnostics[i] = diagnostic;
            _tiles[i].Disabled = _diagnostics[i].Length > 0;
        }
        if (_entries.Count > 0) Select(0);
        else
        {
            _name.Text = WorldSession.SelectedWorldId == WorldCatalog.PondId
                ? "Freshwater fish unavailable" : "No native fish installed";
            _category.Text = "NO SELECTABLE SPECIES";
            _description.Text = "This habitat has no installed native fish yet.";
            _status.Text = WorldSession.SelectedWorldId == WorldCatalog.PondId &&
                CharacterCatalog.PondIntakeDiagnostic.Length > 0
                ? CharacterCatalog.PondIntakeDiagnostic
                : "No compatible fish are available. Go back to World Select.";
            _enter.Disabled = true;
        }
        (_entries.Count > 0 ? _enter : FindChild("BackToWorldButton", true, false) as Button)?.GrabFocus();
        GD.Print($"FISH_CHARACTER_SELECT_READY world={_shownWorldId} entries={_entries.Count}");
    }

    public override void _ExitTree()
    {
        GetViewport().SizeChanged -= UpdateScreenSize;
    }

    public override void _Input(InputEvent @event)
    {
        if (_transitioning) return;
        if (@event is not InputEventKey { Pressed: true, Echo: false } key) return;
        if (_entries.Count > 0 && (key.Keycode is Key.Left or Key.A))
        {
            Select((_selected - 1 + _entries.Count) % _entries.Count);
            GetViewport().SetInputAsHandled();
        }
        else if (_entries.Count > 0 && (key.Keycode is Key.Right or Key.D))
        {
            Select((_selected + 1) % _entries.Count);
            GetViewport().SetInputAsHandled();
        }
        else if (key.Keycode == Key.Enter && GetViewport().GuiGetFocusOwner() is Button focus &&
            Array.IndexOf(_tiles, focus) is var tileIndex && tileIndex >= 0)
        {
            Select(tileIndex);
            GetViewport().SetInputAsHandled();
        }
        else if (key.Keycode == Key.Enter && !_enter.Disabled &&
            (GetViewport().GuiGetFocusOwner() is null || GetViewport().GuiGetFocusOwner() == _enter))
        {
            GetViewport().SetInputAsHandled();
            EnterWorld();
        }
        else if (key.Keycode == Key.Escape)
        {
            GetViewport().SetInputAsHandled();
            BackToWorldSelect();
        }
    }

    public override void _Process(double delta)
    {
        if (_shownWorldId != WorldSession.SelectedWorldId) RebuildForWorld();
        if (_model is null) return;
        _turntable.RotateY((float)delta * .14f);
        if (_animation is null || _clip.Length == 0) return;
        var animation = _animation.GetAnimation(_clip);
        if (animation is not null && animation.LoopMode == Animation.LoopModeEnum.None &&
            _animation.CurrentAnimationPosition >= animation.Length - delta * _animation.SpeedScale)
            _animation.Seek(0, true);
    }

    private void UpdateScreenSize()
    {
        // Changing the window mode in _EnterTree can leave the root Control at
        // its content minimum until the next viewport resize.
        Size = GetViewport().GetVisibleRect().Size;
    }

    private void BuildInterface()
    {
        var background = new ColorRect { Color = new Color(.015f, .105f, .145f), MouseFilter = MouseFilterEnum.Ignore };
        AddChild(background);
        Fill(background);
        var margin = new MarginContainer();
        AddChild(margin);
        Fill(margin);
        foreach (var side in new[] { "left", "right" }) margin.AddThemeConstantOverride("margin_" + side, 24);
        margin.AddThemeConstantOverride("margin_top", 18);
        margin.AddThemeConstantOverride("margin_bottom", 18);
        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", 12);
        margin.AddChild(column);

        var header = new HBoxContainer();
        column.AddChild(header);
        var brand = Label("FISH  /  CHARACTER SELECT", 31, new Color(.91f, .98f, .94f));
        brand.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        header.AddChild(brand);
        header.AddChild(Label("EXPLORE THE REEF", 15, new Color(.48f, .83f, .83f)));

        var hero = new HBoxContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        hero.AddThemeConstantOverride("separation", 16);
        column.AddChild(hero);
        var info = Panel(new Color(.035f, .19f, .23f, .96f));
        info.CustomMinimumSize = new Vector2(285, 0);
        info.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        hero.AddChild(info);
        var infoMargin = new MarginContainer();
        foreach (var side in new[] { "left", "right", "top", "bottom" })
            infoMargin.AddThemeConstantOverride("margin_" + side, 22);
        info.AddChild(infoMargin);
        var details = new VBoxContainer();
        details.AddThemeConstantOverride("separation", 12);
        infoMargin.AddChild(details);
        details.AddChild(Label("01  /  CHOOSE YOUR SWIMMER", 14, new Color(.48f, .84f, .82f)));
        _name = Label("", 34, new Color(.98f, .91f, .67f));
        _name.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        details.AddChild(_name);
        _category = Label("", 17, new Color(.65f, .89f, .89f));
        details.AddChild(_category);
        _description = Label("", 19, Colors.White);
        _description.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        details.AddChild(_description);
        details.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore });
        _status = Label("", 15, new Color(.95f, .75f, .48f));
        _status.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        details.AddChild(_status);
        details.AddChild(Label("Drag the fish to rotate  •  ← → to browse", 14, new Color(.57f, .79f, .8f)));

        var previewPanel = Panel(new Color(.025f, .145f, .19f));
        previewPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        previewPanel.SizeFlagsStretchRatio = 1.65f;
        hero.AddChild(previewPanel);
        var previewContainer = new SubViewportContainer { Stretch = true, MouseFilter = MouseFilterEnum.Stop };
        Fill(previewContainer);
        previewPanel.AddChild(previewContainer);
        previewContainer.GuiInput += PreviewInput;
        _preview = new SubViewport { Name = "LivePreview", OwnWorld3D = true, TransparentBg = false,
            Size = new Vector2I(720, 420), RenderTargetUpdateMode = SubViewport.UpdateMode.Always };
        previewContainer.AddChild(_preview);
        var world = new Node3D();
        _preview.AddChild(world);
        var env = new WorldEnvironment();
        world.AddChild(env);
        env.Environment = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.Color,
            BackgroundColor = new Color(.025f, .18f, .23f),
            AmbientLightSource = Godot.Environment.AmbientSource.Color,
            AmbientLightColor = new Color(.48f, .78f, .82f),
            AmbientLightEnergy = .55f,
        };
        var sun = new DirectionalLight3D { LightEnergy = 1.5f, RotationDegrees = new Vector3(-35, -35, 0) };
        world.AddChild(sun);
        var fill = new OmniLight3D { LightEnergy = .65f, Position = new Vector3(-2, 1.5f, 2), OmniRange = 8 };
        world.AddChild(fill);
        _turntable = new Node3D { Name = "Turntable" };
        world.AddChild(_turntable);
        var camera = new Camera3D { Current = true, Position = new Vector3(0, .12f, 3.8f), Fov = 38 };
        world.AddChild(camera);
        camera.LookAt(new Vector3(0, 0, 0));

        column.AddChild(Label("02  /  THE COLLECTION", 14, new Color(.49f, .84f, .82f)));
        _gallery = new ScrollContainer { Name = "CharacterGallery", CustomMinimumSize = new Vector2(0, 82),
            HorizontalScrollMode = ScrollContainer.ScrollMode.Auto, VerticalScrollMode = ScrollContainer.ScrollMode.Disabled };
        column.AddChild(_gallery);
        var strip = new HBoxContainer();
        strip.AddThemeConstantOverride("separation", 8);
        _gallery.AddChild(strip);
        for (var i = 0; i < _entries.Count; i++)
        {
            var index = i;
            var button = new Button { Name = "Character_" + i, CustomMinimumSize = new Vector2(164, 68),
                FocusMode = FocusModeEnum.All };
            button.Pressed += () => Select(index);
            ApplyFocusStyle(button);
            strip.AddChild(button);
            _tiles[i] = button;
        }

        var actions = new HBoxContainer();
        actions.AddThemeConstantOverride("separation", 10);
        column.AddChild(actions);
        var previous = Action("← Previous", () => { if (_entries.Count > 0) Select((_selected - 1 + _entries.Count) % _entries.Count); });
        previous.Disabled = _entries.Count == 0;
        actions.AddChild(previous);
        var next = Action("Next →", () => { if (_entries.Count > 0) Select((_selected + 1) % _entries.Count); });
        next.Disabled = _entries.Count == 0;
        actions.AddChild(next);
        actions.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore });
        _enter = Action("Enter World", EnterWorld);
        _enter.Name = "EnterWorldButton";
        _enter.CustomMinimumSize = new Vector2(190, 48);
        actions.AddChild(_enter);
        var back = Action("Back to World Select", BackToWorldSelect);
        back.Name = "BackToWorldButton";
        actions.AddChild(back);
        actions.AddChild(Action("Quit", () => GetTree().Quit()));
    }

    private void Select(int index)
    {
        _selected = index;
        var entry = _entries[index];
        _name.Text = entry.Name;
        _category.Text = entry.Category.ToUpperInvariant() + "   /   " + entry.Id;
        _description.Text = entry.Description;
        _status.Text = _diagnostics[index].Length == 0 ? "READY TO SWIM" : _diagnostics[index];
        _enter.Disabled = _diagnostics[index].Length > 0;
        for (var i = 0; i < _tiles.Length; i++)
        {
            _tiles[i].Text = (i == index ? "✓ SELECTED\n" : $"{i + 1:00}  ") +
                _entries[i].Name + (_diagnostics[i].Length > 0 ? "  • UNAVAILABLE" : "");
            _tiles[i].Modulate = i == index ? new Color(1, .9f, .62f) : Colors.White;
        }
        _gallery.ScrollHorizontal = Math.Max(0, index * 172 - 344);
        if (_model is not null) { _turntable.RemoveChild(_model); _model.Free(); _model = null; }
        _animation = null;
        if (_diagnostics[index].Length > 0) return;
        if (!CharacterCatalog.TryInstantiate(entry, out _model, out _animation, out var diagnostic))
        {
            _diagnostics[index] = diagnostic;
            _status.Text = diagnostic;
            _enter.Disabled = true;
            return;
        }
        _turntable.AddChild(_model!);
        _model!.Name = "PreviewModel";
        _model.Transform = entry.PresentationTransform;
        _model.RotateY(Mathf.DegToRad(entry.PreviewYawDegrees));
        _turntable.Rotation = Vector3.Zero;
        _clip = entry.GlideClip.Length > 0 ? entry.GlideClip : entry.SwimClip;
        _animation!.Play(_clip);
        _animation.SpeedScale = entry.GlideClip.Length > 0 ? .75f : .4f;
    }

    private void PreviewInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left } button)
            _dragging = button.Pressed;
        else if (@event is InputEventMouseMotion motion && _dragging)
            _turntable.RotateY(-motion.Relative.X * .012f);
    }

    private void EnterWorld()
    {
        if (_transitioning || _enter.Disabled) return;
        if (_entries.Count == 0) return;
        if (!SpeciesCompatibility.TryResolve(WorldSession.SelectedWorldId,
            _entries[_selected].Id, SpeciesRole.Selectable, out _, out var diagnostic))
        { _status.Text = diagnostic; _enter.Disabled = true; return; }
        DemoSelection.CharacterId = _entries[_selected].Id;
        _transitioning = true;
        _enter.Disabled = true;
        _dragging = false;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GetTree().Paused = false;
        var result = GetTree().ChangeSceneToFile("res://scenes/main.tscn");
        if (result == Error.Ok) return;
        _transitioning = false;
        _enter.Disabled = false;
        _status.Text = $"Could not enter world ({result}).";
    }

    private void BackToWorldSelect()
    {
        if (_transitioning) return;
        _transitioning = true;
        _dragging = false;
        DemoSelection.Clear();
        WorldSession.Unload();
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GetTree().Paused = false;
        var result = GetTree().ChangeSceneToFile("res://scenes/world_select.tscn");
        if (result == Error.Ok) return;
        _transitioning = false;
        _status.Text = $"Could not return to World Select ({result}).";
    }

    private static Label Label(string text, int size, Color color)
    {
        var label = new Label { Text = text, Modulate = color, MouseFilter = MouseFilterEnum.Ignore };
        label.AddThemeFontSizeOverride("font_size", size);
        return label;
    }

    private static PanelContainer Panel(Color color)
    {
        var panel = new PanelContainer();
        var style = new StyleBoxFlat { BgColor = color, BorderColor = new Color(.22f, .56f, .57f) };
        style.SetBorderWidthAll(1);
        style.SetCornerRadiusAll(10);
        panel.AddThemeStyleboxOverride("panel", style);
        return panel;
    }

    private static Button Action(string title, Action action)
    {
        var button = new Button { Text = title, CustomMinimumSize = new Vector2(108, 48) };
        ApplyFocusStyle(button);
        button.Pressed += action;
        return button;
    }

    private static void ApplyFocusStyle(Button button)
    {
        var focus = new StyleBoxFlat { BgColor = Colors.Transparent,
            BorderColor = new Color(.99f, .83f, .46f) };
        focus.SetBorderWidthAll(3);
        focus.SetCornerRadiusAll(5);
        button.AddThemeStyleboxOverride("focus", focus);
    }

    private static void Fill(Control control)
    {
        control.AnchorLeft = 0;
        control.AnchorTop = 0;
        control.AnchorRight = 1;
        control.AnchorBottom = 1;
        control.OffsetLeft = 0;
        control.OffsetTop = 0;
        control.OffsetRight = 0;
        control.OffsetBottom = 0;
    }
}
