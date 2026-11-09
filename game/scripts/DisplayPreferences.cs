using Godot;

namespace Fish;

/// <summary>Local presentation preferences; never part of a living fish save.</summary>
public sealed class DisplayPreferences
{
    public const string Fullscreen = "fullscreen";
    public const string WindowedFullscreen = "windowed-fullscreen";
    public const string Path = "user://preferences.cfg";

    public string SavedDisplayMode { get; private set; } = Fullscreen;
    public string ActiveDisplayMode { get; set; } = Fullscreen;
    public bool ShowControlHelp { get; private set; } = true;
    private readonly ConfigFile _config = new();

    public static bool IsSupportedDisplayMode(string mode) => mode is Fullscreen or WindowedFullscreen;

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

    private void Save()
    {
        _config.SetValue("display", "mode", SavedDisplayMode);
        _config.SetValue("input", "show_control_help", ShowControlHelp);
        var error = _config.Save(Path);
        if (error != Error.Ok) GD.PushWarning($"Could not save local preferences ({error})");
    }
}
