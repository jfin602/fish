#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fish;

/// <summary>Controls-only local snapshots. Call Save after a confirmed edit or selection.</summary>
public sealed class ControlPresetStore
{
    public const string DefaultPath = "user://control-presets.json";
    public const int MaxCustomProfiles = 16;
    private const int MaxFileBytes = 65536;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = true
    };
    private readonly string _path;
    private readonly string _legacyPath;
    private readonly Dictionary<string, ControlPreset> _profiles = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _origins = new(StringComparer.Ordinal);
    public string CurrentId { get; private set; } = ControlPreset.KeyboardId;
    public ControlPreset Current => _profiles[CurrentId];
    public IReadOnlyList<ControlPreset> List() => _profiles.Values
        .OrderBy(profile => profile.IsBuiltin ? 0 : 1)
        .ThenBy(profile => profile.Name, StringComparer.OrdinalIgnoreCase).ToArray();

    public ControlPresetStore(string path = DefaultPath, string legacyPath = DisplayPreferences.Path)
    {
        _path = Resolve(path);
        _legacyPath = Resolve(legacyPath);
        ResetToDefaults();
    }

    private static string Resolve(string path) => path.StartsWith("user://", StringComparison.Ordinal)
        ? ProjectSettings.GlobalizePath(path) : path;

    private void ResetToDefaults()
    {
        _profiles.Clear();
        _origins.Clear();
        foreach (var template in ControlPreset.Builtins()) _profiles.Add(template.Id, template);
        CurrentId = ControlPreset.KeyboardId;
    }

    public void Load()
    {
        ResetToDefaults();
        if (!File.Exists(_path))
        {
            MigrateLegacy();
            return;
        }
        try
        {
            if (new FileInfo(_path).Length > MaxFileBytes) throw new InvalidDataException("Controls file exceeds size limit");
            var data = JsonSerializer.Deserialize<StoreData>(File.ReadAllText(_path), JsonOptions)
                ?? throw new InvalidDataException("Empty controls file");
            if (data.SchemaVersion != ControlPreset.SchemaVersion || data.Profiles is null ||
                data.Profiles.Count > MaxCustomProfiles || data.CurrentId is null)
                throw new InvalidDataException("Unsupported or incomplete controls schema");
            foreach (var entry in data.Profiles)
            {
                if (entry is null || entry.Id is null || entry.Name is null || entry.OriginId is null ||
                    entry.Bindings is null || !_profiles.ContainsKey(entry.OriginId) ||
                    entry.Id.StartsWith("builtin:", StringComparison.Ordinal))
                    throw new InvalidDataException("Invalid custom profile entry");
                var preset = new ControlPreset(entry.Id, entry.Name, entry.Bindings,
                    entry.MouseX, entry.MouseY, entry.MouseSensitivity);
                if (preset.IsBuiltin || !_profiles.TryAdd(preset.Id, preset) ||
                    _profiles.Values.Count(p => p.Name.Equals(preset.Name, StringComparison.OrdinalIgnoreCase)) != 1)
                    throw new InvalidDataException("Duplicate profile identity or name");
                _origins.Add(preset.Id, entry.OriginId);
            }
            if (!_profiles.ContainsKey(data.CurrentId)) throw new InvalidDataException("Selected profile is missing");
            CurrentId = data.CurrentId;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or
            ArgumentException or InvalidDataException or NotSupportedException)
        {
            PreserveCorruptFile();
            GD.PushWarning($"Control presets could not be loaded ({error.Message}); using safe legacy/default controls");
            ResetToDefaults();
            MigrateLegacy();
        }
    }

    private void PreserveCorruptFile()
    {
        try
        {
            var backup = _path + ".corrupt-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + ".json";
            File.Copy(_path, backup, overwrite: false);
        }
        catch (IOException) { GD.PushWarning("Could not preserve corrupt controls file"); }
        catch (UnauthorizedAccessException) { GD.PushWarning("Could not preserve corrupt controls file"); }
    }

    private void MigrateLegacy()
    {
        if (!File.Exists(_legacyPath)) return;
        var config = new ConfigFile();
        if (config.Load(_legacyPath) != Error.Ok) return;
        try
        {
            var mode = config.GetValue("input", "mode", DisplayPreferences.KeyboardMode).AsString();
            CurrentId = mode == DisplayPreferences.MouseMode ? ControlPreset.MouseId : ControlPreset.KeyboardId;
            var sensitivity = config.GetValue("input", "mouse_sensitivity", ControlPreset.DefaultSensitivity).AsSingle();
            var invertY = config.GetValue("input", "invert_mouse_y", false).AsBool();
            if (!float.IsFinite(sensitivity) || sensitivity < 0.04f || sensitivity > 0.4f)
                sensitivity = ControlPreset.DefaultSensitivity;
            if (Math.Abs(sensitivity - ControlPreset.DefaultSensitivity) < 0.00001f && !invertY) return;
            var migrated = Create("Legacy Mouse Settings", ControlPreset.MouseId).WithMouse(
                new(SteeringAxis.Yaw), new(SteeringAxis.Pitch, invertY), sensitivity);
            Update(migrated);
            if (mode == DisplayPreferences.MouseMode) CurrentId = migrated.Id;
        }
        catch (Exception error) when (error is InvalidCastException or ArgumentException)
        {
            GD.PushWarning($"Invalid legacy control preference ({error.Message}); using keyboard defaults");
            ResetToDefaults();
        }
    }

    public ControlPreset Create(string name, string templateId = ControlPreset.KeyboardId) => Duplicate(templateId, name);

    public ControlPreset Duplicate(string sourceId, string name)
    {
        if (_origins.Count >= MaxCustomProfiles) throw new InvalidOperationException("Too many custom profiles");
        if (!_profiles.TryGetValue(sourceId, out var source)) throw new ArgumentException("Unknown source profile");
        if (_profiles.Values.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("Profile name already exists");
        var id = "custom:" + Guid.NewGuid().ToString("N");
        var profile = source.Copy(id, name);
        _profiles.Add(id, profile);
        _origins.Add(id, source.IsBuiltin ? source.Id : _origins[source.Id]);
        return profile;
    }

    public void Update(ControlPreset preset)
    {
        if (!_origins.ContainsKey(preset.Id)) throw new InvalidOperationException("Built-in or unknown profile is immutable");
        if (_profiles.Values.Any(p => p.Id != preset.Id && p.Name.Equals(preset.Name, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("Profile name already exists");
        _profiles[preset.Id] = preset;
    }

    public void Rename(string id, string name)
    {
        if (!_profiles.TryGetValue(id, out var preset)) throw new ArgumentException("Unknown profile");
        Update(preset.WithName(name));
    }

    public void Reset(string id)
    {
        if (!_origins.TryGetValue(id, out var origin))
            throw new InvalidOperationException("Built-in or unknown profile cannot be reset");
        _profiles[id] = _profiles[origin].Copy(id, _profiles[id].Name);
    }

    public void Delete(string id)
    {
        if (!_origins.Remove(id)) throw new InvalidOperationException("Built-in or unknown profile cannot be deleted");
        _profiles.Remove(id);
        if (CurrentId == id) CurrentId = ControlPreset.KeyboardId;
    }

    public void Choose(string id)
    {
        if (!_profiles.ContainsKey(id)) throw new ArgumentException("Unknown profile");
        CurrentId = id;
    }

    public void Save()
    {
        var data = new StoreData
        {
            SchemaVersion = ControlPreset.SchemaVersion,
            CurrentId = CurrentId,
            Profiles = _origins.Keys.OrderBy(id => id, StringComparer.Ordinal).Select(id =>
            {
                var profile = _profiles[id];
                return (ProfileData?)new ProfileData
                {
                    Id = id, Name = profile.Name, OriginId = _origins[id],
                    Bindings = Enum.GetValues<ControlAction>().ToDictionary(action => action,
                        action => profile.Bindings(action).ToArray()),
                    MouseX = profile.MouseX, MouseY = profile.MouseY,
                    MouseSensitivity = profile.MouseSensitivity
                };
            }).ToList()
        };
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var temporary = _path + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(data, JsonOptions));
            File.Move(temporary, _path, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private sealed class StoreData
    {
        public int SchemaVersion { get; set; }
        public string? CurrentId { get; set; }
        public List<ProfileData?>? Profiles { get; set; }
    }

    private sealed class ProfileData
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? OriginId { get; set; }
        public Dictionary<ControlAction, ControlBinding[]>? Bindings { get; set; }
        public MouseAxisAssignment MouseX { get; set; }
        public MouseAxisAssignment MouseY { get; set; }
        public float MouseSensitivity { get; set; }
    }
}
