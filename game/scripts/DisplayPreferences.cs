using Godot;

namespace Fish;

/// <summary>Local presentation preferences; never part of a living fish save.</summary>
public sealed class DisplayPreferences
{
    public const string Fullscreen = "fullscreen";
    public const string WindowedFullscreen = "windowed-fullscreen";
    public const string KeyboardMode = "keyboard";
    public const string MouseMode = "mouse";
    public const string Path = "user://preferences.cfg";

    public string SavedDisplayMode { get; private set; } = Fullscreen;
    public string ActiveDisplayMode { get; set; } = Fullscreen;
    public bool ShowControlHelp { get; private set; } = true;
    public string InputMode { get; private set; } = KeyboardMode;
    public float MouseSensitivity { get; private set; } = 0.12f; // degrees per captured pixel
    public bool InvertMouseY { get; private set; }
    private readonly ConfigFile _config = new();

    public static bool IsSupportedDisplayMode(string mode) => mode is Fullscreen or WindowedFullscreen;
    public static bool IsSupportedInputMode(string mode) => mode is KeyboardMode or MouseMode;

    public static Window.ModeEnum ToWindowMode(string mode) =>
        mode == WindowedFullscreen ? Window.ModeEnum.Fullscreen : Window.ModeEnum.ExclusiveFullscreen;

    public void Load()
    {
        if (!FileAccess.FileExists(Path)) return;
        var error = _config.Load(Path);
        if (error != Error.Ok)
        {
            GD.PushWarning($"Could not load local preferences ({error}); using defaults");
            return;
        }
        var mode = _config.GetValue("display", "mode", Fullscreen).AsString();
        if (IsSupportedDisplayMode(mode)) SavedDisplayMode = mode;
        else GD.PushWarning($"Unsupported saved display mode '{mode}'; using fullscreen");
        ShowControlHelp = _config.GetValue("input", "show_control_help", true).AsBool();
        var inputMode = _config.GetValue("input", "mode", KeyboardMode).AsString();
        if (IsSupportedInputMode(inputMode)) InputMode = inputMode;
        else GD.PushWarning($"Unsupported saved input mode '{inputMode}'; using keyboard");
        var sensitivity = _config.GetValue("input", "mouse_sensitivity", 0.12f).AsSingle();
        if (float.IsFinite(sensitivity) && sensitivity is >= 0.04f and <= 0.4f)
            MouseSensitivity = sensitivity;
        else GD.PushWarning("Invalid saved mouse sensitivity; using 0.12 degrees/pixel");
        InvertMouseY = _config.GetValue("input", "invert_mouse_y", false).AsBool();
    }

    public void ChooseDisplayMode(string mode)
    {
        if (!IsSupportedDisplayMode(mode)) return;
        SavedDisplayMode = mode;
        ActiveDisplayMode = mode;
        Save();
    }

    public void ChooseControlHelp(bool show)
    {
        ShowControlHelp = show;
        Save();
    }

    public void ChooseInputMode(string mode)
    {
        if (!IsSupportedInputMode(mode)) return;
        InputMode = mode;
        Save();
    }

    public void ChooseMouseSensitivity(float value)
    {
        if (!float.IsFinite(value)) return;
        MouseSensitivity = Mathf.Clamp(value, 0.04f, 0.4f);
        Save();
    }

    public void ChooseInvertMouseY(bool invert)
    {
        InvertMouseY = invert;
        Save();
    }

    private void Save()
    {
        _config.SetValue("display", "mode", SavedDisplayMode);
        _config.SetValue("input", "show_control_help", ShowControlHelp);
        _config.SetValue("input", "mode", InputMode);
        _config.SetValue("input", "mouse_sensitivity", MouseSensitivity);
        _config.SetValue("input", "invert_mouse_y", InvertMouseY);
        var error = _config.Save(Path);
        if (error != Error.Ok) GD.PushWarning($"Could not save local preferences ({error})");
    }
}
