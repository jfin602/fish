#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Fish;

/// <summary>Graphics snapshots use the same bounded, atomic user-file pattern as control presets.</summary>
public sealed class LightingPresetStore
{
    public const string DefaultPath = "user://lighting-presets.json";
    public const string PondPath = "user://lighting-presets-pond.json";
    public const int MaxCustomPresets = 16;
    public const string DefaultId = "builtin:default";
    public const string SoftId = "builtin:soft";
    public const string PondCloudyId = "builtin:pond-cloudy";
    public const string PondShallowsId = "builtin:pond-shallows";
    private const int MaxFileBytes = 32768;
    private readonly string _path;
    private readonly WorldEnvironmentProfile _profile;
    private readonly Dictionary<string, LightingPreset> _presets = new(StringComparer.Ordinal);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    public string CurrentId { get; private set; } = DefaultId;
    public LightingPreset Current => _presets[CurrentId];
    public IReadOnlyList<LightingPreset> List() => _presets.Values
        .OrderBy(p => p.IsBuiltin ? 0 : 1).ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToArray();

    public LightingPresetStore(string path = DefaultPath, WorldEnvironmentProfile profile = WorldEnvironmentProfile.Ocean)
    {
        _profile = profile;
        _path = path.StartsWith("user://", StringComparison.Ordinal) ? ProjectSettings.GlobalizePath(path) : path;
        ResetDefaults();
    }

    public static LightingPresetStore ForProfile(WorldEnvironmentProfile profile) =>
        new(profile == WorldEnvironmentProfile.Pond ? PondPath : DefaultPath, profile);

    private void ResetDefaults()
    {
        _presets.Clear();
        if (_profile == WorldEnvironmentProfile.Pond)
        {
            _presets.Add(PondCloudyId, new LightingPreset(PondCloudyId, "Pond · Overcast", new LightingSettings
            {
                SurfaceWaveHeight = 0.45f, SurfaceWaveSpeed = 0.65f, SurfaceWaveDetail = 0.35f,
                CausticEnergy = 0.08f, CausticDrift = 0.08f, CausticTravel = 0.25f,
                ShaftCount = 2, ShaftBrightness = 0.32f, SunEnergy = 1.35f,
                AmbientEnergy = 0.78f, VolumetricDensity = 0.029f,
                VisibilityDistanceMeters = 18f, MoteCount = 72, ScreenWarpPixels = 0.04f,
            }));
            _presets.Add(PondShallowsId, new LightingPreset(PondShallowsId, "Pond · Brighter Shallows", new LightingSettings
            {
                SurfaceWaveHeight = 0.50f, SurfaceWaveSpeed = 0.7f, SurfaceWaveDetail = 0.4f,
                CausticEnergy = 0.16f, CausticDrift = 0.11f, CausticTravel = 0.35f,
                ShaftCount = 3, ShaftBrightness = 0.55f, SunEnergy = 1.65f,
                AmbientEnergy = 0.88f, VolumetricDensity = 0.024f,
                VisibilityDistanceMeters = 24f, MoteCount = 64, ScreenWarpPixels = 0.04f,
            }));
            CurrentId = PondCloudyId;
        }
        else
        {
            _presets.Add(DefaultId, new LightingPreset(DefaultId, "Natural", new LightingSettings()));
            _presets.Add(SoftId, new LightingPreset(SoftId, "Soft", new LightingSettings
            {
                SurfaceWaveDetail = 0.45f, CausticEnergy = 0.24f, ShaftCount = 4,
                ShaftBrightness = 0.65f, MoteCount = 80, ScreenWarpPixels = 0f,
            }));
            CurrentId = DefaultId;
        }
    }

    public void Load()
    {
        ResetDefaults();
        if (!File.Exists(_path)) return;
        try
        {
            if (new FileInfo(_path).Length > MaxFileBytes) throw new InvalidDataException("Lighting file exceeds size limit");
            var json = File.ReadAllText(_path);
            using var document = JsonDocument.Parse(json);
            var data = JsonSerializer.Deserialize<StoreData>(json, JsonOptions)
                ?? throw new InvalidDataException("Empty lighting file");
            if (data.SchemaVersion is not (1 or 2) || data.Presets is null || data.Presets.Count > MaxCustomPresets ||
                !TryGetProperty(document.RootElement, "presets", out var rawPresets) ||
                rawPresets.ValueKind != JsonValueKind.Array || rawPresets.GetArrayLength() != data.Presets.Count)
                throw new InvalidDataException("Unsupported lighting schema");
            for (var index = 0; index < data.Presets.Count; index++)
            {
                var preset = data.Presets[index];
                var raw = rawPresets[index];
                if (raw.ValueKind != JsonValueKind.Object ||
                    !TryGetProperty(raw, "settings", out var rawSettings) ||
                    rawSettings.ValueKind != JsonValueKind.Object ||
                    (data.SchemaVersion == 2 && !TryGetProperty(rawSettings, "visibilityDistanceMeters", out _)))
                    throw new InvalidDataException("Lighting visibility value is missing");
                if (data.SchemaVersion == 1 && preset?.Settings is not null)
                    preset.Settings.VisibilityDistanceMeters = LightingSettings.VisibilityFromLegacyDensity(preset.Settings.DistanceFogDensity);
                if (preset is null || preset.Id is null || preset.Name is null || preset.Settings is null ||
                    preset.Id.StartsWith("builtin:", StringComparison.Ordinal) ||
                    !Guid.TryParse(preset.Id, out _) || !ValidName(preset.Name) || !preset.Settings.IsValid() ||
                    !_presets.TryAdd(preset.Id, preset) || NameExists(preset.Name, preset.Id))
                    throw new InvalidDataException("Invalid lighting preset");
            }
            if (data.CurrentId is null || !_presets.ContainsKey(data.CurrentId))
                throw new InvalidDataException("Selected lighting preset is missing");
            CurrentId = data.CurrentId;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or
            ArgumentException or InvalidDataException or NotSupportedException)
        {
            try { File.Copy(_path, _path + ".corrupt-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + ".json"); }
            catch (IOException) { GD.PushWarning("Could not preserve corrupt lighting file"); }
            catch (UnauthorizedAccessException) { GD.PushWarning("Could not preserve corrupt lighting file"); }
            ResetDefaults();
            GD.PushWarning($"Lighting presets could not be loaded ({error.Message}); using {Current.Name}");
        }
    }

    public LightingPreset SaveAs(string name, LightingSettings settings)
    {
        if (_presets.Count - 2 >= MaxCustomPresets) throw new ArgumentException("Lighting preset limit reached");
        if (!ValidName(name) || NameExists(name, null) || !settings.IsValid())
            throw new ArgumentException("Enter a unique name and valid lighting values");
        var preset = new LightingPreset(Guid.NewGuid().ToString("D"), name.Trim(), settings.Copy());
        _presets.Add(preset.Id, preset);
        CurrentId = preset.Id;
        Save();
        return preset;
    }

    public void UpdateCurrent(LightingSettings settings)
    {
        if (Current.IsBuiltin || !settings.IsValid()) throw new ArgumentException("Select a custom preset to save changes");
        _presets[CurrentId] = Current with { Settings = settings.Copy() };
        Save();
    }

    public void RenameCurrent(string name)
    {
        if (Current.IsBuiltin || !ValidName(name) || NameExists(name, CurrentId))
            throw new ArgumentException("Enter a unique name for a custom preset");
        _presets[CurrentId] = Current with { Name = name.Trim() };
        Save();
    }

    public void DeleteCurrent()
    {
        if (Current.IsBuiltin) throw new ArgumentException("Built-in lighting presets cannot be deleted");
        _presets.Remove(CurrentId);
        CurrentId = _profile == WorldEnvironmentProfile.Pond ? PondCloudyId : DefaultId;
        Save();
    }

    public void Choose(string id)
    {
        if (!_presets.ContainsKey(id)) throw new ArgumentException("Unknown lighting preset");
        CurrentId = id;
        Save();
    }

    private bool NameExists(string name, string? exceptId) => _presets.Values.Any(p =>
        p.Id != exceptId && p.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
            foreach (var property in element.EnumerateObject())
                if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
        value = default;
        return false;
    }
    private static bool ValidName(string name) => !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= 32 &&
        !name.Any(char.IsControl);

    public void Save()
    {
        var data = new StoreData { SchemaVersion = 2, CurrentId = CurrentId,
            Presets = _presets.Values.Where(p => !p.IsBuiltin).OrderBy(p => p.Id, StringComparer.Ordinal)
                .Select(p => (LightingPreset?)p).ToList() };
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
        public List<LightingPreset?>? Presets { get; set; }
    }
}

public sealed record LightingPreset(string Id, string Name, LightingSettings Settings)
{
    public bool IsBuiltin => Id.StartsWith("builtin:", StringComparison.Ordinal);
}
