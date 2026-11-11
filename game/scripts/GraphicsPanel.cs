#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace Fish;

/// <summary>Paused, live graphics tuning with local lighting snapshots.</summary>
public partial class GraphicsPanel : PanelContainer
{
    private LightingPresetStore _store;
    private readonly Func<UnderwaterAtmosphere?> _atmosphere;
    private readonly Action _closed;
    private readonly List<(HSlider Slider, Label Value, Func<LightingSettings, float> Read)> _sliders = [];
    private OptionButton _choices = null!;
    private LineEdit _name = null!;
    private Label _status = null!;
    private LightingSettings _draft = new();
    private bool _refreshing;

    public GraphicsPanel(LightingPresetStore store, Func<UnderwaterAtmosphere?> atmosphere, Action closed)
    {
        _store = store;
        _atmosphere = atmosphere;
        _closed = closed;
        Name = "GraphicsPanel";
        Visible = false;
        ProcessMode = ProcessModeEnum.Always;
        MouseFilter = MouseFilterEnum.Stop;
    }

    public void SetStore(LightingPresetStore store)
    {
        _store = store;
        _draft = store.Current.Settings.Copy();
        RefreshChoices();
    }

    public override void _Ready()
    {
        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 18);
        margin.AddThemeConstantOverride("margin_right", 18);
        margin.AddThemeConstantOverride("margin_top", 14);
        margin.AddThemeConstantOverride("margin_bottom", 14);
        AddChild(margin);
        var outer = new VBoxContainer();
        outer.AddThemeConstantOverride("separation", 8);
        margin.AddChild(outer);
        var title = new Label { Text = "GFX · Water & Lighting", HorizontalAlignment = HorizontalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size", 22);
        outer.AddChild(title);
        var help = new Label { Text = "Changes preview live. Lower visibility is murkier; higher is clearer. Save to keep a custom preset.",
            AutowrapMode = TextServer.AutowrapMode.WordSmart };
        outer.AddChild(help);
        _choices = new OptionButton { Name = "LightingPresetChoice" };
        _choices.ItemSelected += index =>
        {
            if (_refreshing || index < 0 || index >= _store.List().Count) return;
            Try(() =>
            {
                _store.Choose(_store.List()[(int)index].Id);
                LoadCurrent();
            });
        };
        outer.AddChild(_choices);
        _name = new LineEdit { Name = "LightingPresetName", PlaceholderText = "New preset name (up to 32 characters)" };
        outer.AddChild(_name);
        var actions = new HFlowContainer();
        outer.AddChild(actions);
        AddButton(actions, "Save As", () => Try(() => { _store.SaveAs(_name.Text, _draft); LoadCurrent(); }));
        AddButton(actions, "Save", () => Try(() => { _store.UpdateCurrent(_draft); _status.Text = "Lighting saved."; }));
        AddButton(actions, "Rename", () => Try(() => { _store.RenameCurrent(_name.Text); LoadCurrent(); }));
        AddButton(actions, "Delete", () => Try(() => { _store.DeleteCurrent(); LoadCurrent(); }));
        AddButton(actions, "Reset", LoadCurrent);

        var scroll = new ScrollContainer { Name = "GraphicsSliders", SizeFlagsVertical = SizeFlags.ExpandFill };
        outer.AddChild(scroll);
        var rows = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        rows.AddThemeConstantOverride("separation", 5);
        scroll.AddChild(rows);
        AddSection(rows, "Surface");
        AddSlider(rows, "Wave height", 0.25, 1.2, 0.01, s => s.SurfaceWaveHeight, (s, v) => s.SurfaceWaveHeight = v);
        AddSlider(rows, "Wave speed", 0.25, 2, 0.01, s => s.SurfaceWaveSpeed, (s, v) => s.SurfaceWaveSpeed = v);
        AddSlider(rows, "Wave detail", 0, 1, 0.01, s => s.SurfaceWaveDetail, (s, v) => s.SurfaceWaveDetail = v);
        AddSection(rows, "Caustics & shafts");
        AddSlider(rows, "Caustic brightness", 0, 0.8, 0.01, s => s.CausticEnergy, (s, v) => s.CausticEnergy = v);
        AddSlider(rows, "Caustic speed", 0, 0.5, 0.01, s => s.CausticDrift, (s, v) => s.CausticDrift = v);
        AddSlider(rows, "Caustic travel", 0, 1.5, 0.01, s => s.CausticTravel, (s, v) => s.CausticTravel = v);
        AddSlider(rows, "Visible shafts", 0, 8, 1, s => s.ShaftCount, (s, v) => s.ShaftCount = Mathf.RoundToInt(v));
        AddSlider(rows, "Shaft brightness", 0, 2, 0.01, s => s.ShaftBrightness, (s, v) => s.ShaftBrightness = v);
        AddSection(rows, "Water light & depth");
        AddSlider(rows, "Sun brightness", 0, 3, 0.01, s => s.SunEnergy, (s, v) => s.SunEnergy = v);
        AddSlider(rows, "Ambient brightness", 0, 1, 0.01, s => s.AmbientEnergy, (s, v) => s.AmbientEnergy = v);
        AddSlider(rows, "Volumetric haze", 0.001, 0.06, 0.001, s => s.VolumetricDensity, (s, v) => s.VolumetricDensity = v);
        AddSlider(rows, "Visibility Distance (m)", 18, 60, 1, s => s.VisibilityDistanceMeters, (s, v) => s.VisibilityDistanceMeters = v);
        AddSlider(rows, "Motes", 0, 160, 1, s => s.MoteCount, (s, v) => s.MoteCount = Mathf.RoundToInt(v));
        AddSlider(rows, "Screen shimmer (px)", 0, 0.2, 0.01, s => s.ScreenWarpPixels, (s, v) => s.ScreenWarpPixels = v);
        _status = new Label { Name = "GraphicsStatus", Text = "" };
        outer.AddChild(_status);
        var back = new Button { Name = "GraphicsBackButton", Text = "Back to settings" };
        back.Pressed += RequestClose;
        outer.AddChild(back);
        RefreshChoices();
    }

    private static void AddSection(VBoxContainer rows, string text)
    {
        var label = new Label { Text = text };
        label.AddThemeFontSizeOverride("font_size", 17);
        rows.AddChild(label);
    }

    private static void AddButton(HFlowContainer row, string text, Action action)
    {
        var button = new Button { Text = text, FocusMode = FocusModeEnum.All };
        button.Pressed += action;
        row.AddChild(button);
    }

    private void AddSlider(VBoxContainer rows, string name, double min, double max, double step,
        Func<LightingSettings, float> read, Action<LightingSettings, float> write)
    {
        var row = new HBoxContainer();
        rows.AddChild(row);
        var label = new Label { Text = name, CustomMinimumSize = new Vector2(178, 0) };
        row.AddChild(label);
        var slider = new HSlider { Name = name == "Visibility Distance (m)" ? "VisibilityDistanceSlider" : name.Replace(" ", ""),
            MinValue = min, MaxValue = max, Step = step,
            SizeFlagsHorizontal = SizeFlags.ExpandFill, CustomMinimumSize = new Vector2(200, 0),
            FocusMode = FocusModeEnum.All };
        row.AddChild(slider);
        var value = new Label { CustomMinimumSize = new Vector2(56, 0), HorizontalAlignment = HorizontalAlignment.Right };
        row.AddChild(value);
        slider.ValueChanged += amount =>
        {
            value.Text = step < 0.001 ? amount.ToString("F4") : step < 0.01 ? amount.ToString("F3") :
                step < 1 ? amount.ToString("F2") : amount.ToString("F0");
            if (_refreshing) return;
            var changed = _draft.Copy();
            write(changed, (float)amount);
            if (!changed.IsValid()) return;
            _draft = changed;
            _atmosphere()?.ApplyLighting(_draft);
            _status.Text = "Previewing changes. Save to keep them.";
        };
        _sliders.Add((slider, value, read));
    }

    public void Open()
    {
        LoadCurrent();
        Visible = true;
        _choices.GrabFocus();
    }

    private void LoadCurrent()
    {
        _draft = _store.Current.Settings.Copy();
        _atmosphere()?.ApplyLighting(_draft);
        _name.Text = _store.Current.IsBuiltin ? "" : _store.Current.Name;
        RefreshChoices();
        _status.Text = $"Active lighting: {_store.Current.Name}";
    }

    private void RefreshChoices()
    {
        _refreshing = true;
        _choices.Clear();
        var choices = _store.List();
        var selected = 0;
        for (var i = 0; i < choices.Count; i++)
        {
            _choices.AddItem(choices[i].Name, i);
            if (choices[i].Id == _store.CurrentId) selected = i;
        }
        _choices.Select(selected);
        foreach (var (slider, value, read) in _sliders)
        {
            slider.Value = read(_draft);
            var step = slider.Step;
            value.Text = step < 0.001 ? slider.Value.ToString("F4") : step < 0.01 ?
                slider.Value.ToString("F3") : step < 1 ? slider.Value.ToString("F2") : slider.Value.ToString("F0");
        }
        _refreshing = false;
    }

    private void Try(Action action)
    {
        try { action(); }
        catch (Exception error) when (error is ArgumentException or IOException or UnauthorizedAccessException)
        { _status.Text = error.Message; }
    }

    public void RequestClose()
    {
        Visible = false;
        _closed();
    }
}
