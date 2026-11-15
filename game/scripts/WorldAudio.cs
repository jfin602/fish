#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace Fish;

public enum SwimAudioEvent { None, Start, Turn, Boost }

/// <summary>Presentation-only transition detector; it never writes motor state.</summary>
public sealed class SwimAudioGate
{
    private bool _boostWasActive;
    private bool _startArmed = true;
    private float _idleSeconds;
    private float _turnCooldown;
    private float _turnQuietSeconds;
    private float _turnActivity;
    public float CurrentTurnStrength => TurnStrength(_turnActivity);

    public void Reset()
    {
        _boostWasActive = false;
        _startArmed = true;
        _idleSeconds = 0;
        _turnCooldown = 0;
        _turnQuietSeconds = 0;
        _turnActivity = 0;
    }

    public SwimAudioEvent Step(float effort, float speed, float maximumSpeed,
        float angularTurn, bool boostActive, bool eligible, float delta)
    {
        if (!eligible)
        {
            Reset();
            return SwimAudioEvent.None;
        }
        delta = float.IsFinite(delta) ? Mathf.Max(delta, 0) : 0;
        var fullSpeed = Mathf.Max(.01f, maximumSpeed);
        var moving = effort >= .2f && speed >= fullSpeed * .12f;
        var fastEnoughToTurn = speed >= fullSpeed * .4f;
        if (effort < .1f && speed <= fullSpeed * .04f)
        {
            _idleSeconds += delta;
            if (_idleSeconds >= .2f) _startArmed = true;
        }
        else _idleSeconds = 0;
        _turnCooldown = Mathf.Max(0, _turnCooldown - delta);
        angularTurn = float.IsFinite(angularTurn) ? Mathf.Clamp(angularTurn, 0f, 1f) : 0f;
        _turnActivity = fastEnoughToTurn
            ? Mathf.Lerp(_turnActivity, angularTurn, 1f - Mathf.Exp(-delta / .12f))
            : 0f;
        if (!fastEnoughToTurn || angularTurn < .4f)
        {
            _turnQuietSeconds += delta;
            if (_turnQuietSeconds >= .18f) _turnCooldown = 0;
        }
        else _turnQuietSeconds = 0;
        var boostStarted = boostActive && !_boostWasActive;
        _boostWasActive = boostActive;
        if (boostStarted)
        {
            _startArmed = false;
            return SwimAudioEvent.Boost;
        }
        if (fastEnoughToTurn && angularTurn >= .4f && _turnActivity >= .4f &&
            _turnCooldown <= 0)
        {
            _turnCooldown = Mathf.Lerp(.65f, .9f, CurrentTurnStrength);
            if (moving) _startArmed = false;
            return SwimAudioEvent.Turn;
        }
        if (moving && _startArmed)
        {
            _startArmed = false;
            return SwimAudioEvent.Start;
        }
        return SwimAudioEvent.None;
    }

    public static float TurnStrength(float angularTurn) =>
        Mathf.Clamp((angularTurn - .4f) / .6f, 0f, 1f);
}

/// <summary>One habitat ambience bed and reusable positional player event emitters.</summary>
public partial class WorldAudio : Node3D
{
    private sealed class TimedCue(AudioStreamPlayer3D voice, float duration, float gainDb)
    {
        public AudioStreamPlayer3D Voice { get; } = voice;
        public float Duration { get; } = duration;
        public float GainDb { get; } = gainDb;
        public float Elapsed { get; set; }
    }

    [Export] public float SurfaceVolumeDb { get; set; } = -39f;
    [Export] public float DeepVolumeDb { get; set; } = -14f;
    [Export] public float RiseDepth { get; set; } = .5f;
    [Export] public float FullDepth { get; set; } = 9f;
    [Export] public float ResponsePower { get; set; } = 1.25f;
    [Export] public float FadeSpeed { get; set; } = 1.5f;

    public const string PondBed = "res://assets/sfx/pond_underwater_deep.ogg";
    private FishController _fish = null!;
    private Func<string> _stage = null!;
    private string _bedPath = "";
    private AudioStreamPlayer _ambience = null!;
    private AudioStreamPlayer3D _swim = null!;
    private AudioStreamPlayer3D _boost = null!;
    private readonly List<AudioStreamPlayer3D> _swimVoices = [];
    private readonly List<AudioStreamPlayer3D> _boostVoices = [];
    private readonly List<TimedCue> _timedCues = [];
    private readonly SwimAudioGate _gate = new();
    private float _waterline;
    private float _currentDb;
    private bool _focusPaused;
    private AudioStream _gentle = null!;
    private AudioStream? _bed;
    private AudioStream _large = null!;
    private AudioStream _largeBoost = null!;
    private readonly RandomNumberGenerator _pitchRandom = new();
    private float _lastSwimPitch = 1f;
    private float _lastBoostPitch = 1f;
    private float _lastTurnVolumeDb;
    private float _lastLengthFraction = 1f;
    private float _lastTurnLengthFraction = 1f;
    public int AmbienceStarts { get; private set; }
    public int SwimStarts { get; private set; }
    public int BoostStarts { get; private set; }
    public float CurrentVolumeDb => _currentDb;
    public AudioStreamPlayer AmbiencePlayer => _ambience;
    public AudioStreamPlayer3D SwimEmitter => _swim;
    public AudioStreamPlayer3D BoostEmitter => _boost;

    public void ConfigurePond(FishController fish, PondGrowthState growth)
    {
        if (!float.IsFinite(fish.PondWaterlineHeight))
            throw new InvalidOperationException("A loaded Pond water surface is required for audio");
        Configure(fish, fish.PondWaterlineHeight, () => growth.Stage, PondBed);
    }

    public void Configure(FishController fish, float waterline,
        Func<string> stage, string bedPath)
    {
        if (!float.IsFinite(waterline) || stage is null || string.IsNullOrWhiteSpace(bedPath))
            throw new ArgumentException("A valid habitat waterline, growth stage and bed are required");
        if (!bedPath.StartsWith("res://assets/sfx/", StringComparison.Ordinal))
            throw new ArgumentException("Runtime audio must use an approved assets/sfx path", nameof(bedPath));
        _fish = fish;
        _stage = stage;
        _bedPath = bedPath;
        _waterline = waterline;
    }

    public static float TargetVolume(float surfaceY, float fishY, float surfaceDb,
        float deepDb, float riseDepth, float fullDepth, float power)
    {
        var shallow = Mathf.Clamp(float.IsFinite(surfaceDb) ? surfaceDb : -40f, -80f, 0f);
        if (!float.IsFinite(surfaceY) || !float.IsFinite(fishY)) return shallow;
        var deep = Mathf.Clamp(float.IsFinite(deepDb) ? deepDb : -14f, shallow, 0f);
        var start = Mathf.Max(0f, float.IsFinite(riseDepth) ? riseDepth : 0f);
        var end = Mathf.Max(start + .01f, float.IsFinite(fullDepth) ? fullDepth : start + 1f);
        var depth = Mathf.Max(0f, surfaceY - fishY);
        var x = Mathf.Clamp((depth - start) / (end - start), 0f, 1f);
        var smooth = x * x * (3f - 2f * x);
        return Mathf.Lerp(shallow, deep, Mathf.Pow(smooth,
            Mathf.Clamp(float.IsFinite(power) ? power : 1f, .2f, 4f)));
    }

    public static float SmoothVolume(float current, float target, float delta, float fadeSpeed)
    {
        current = Mathf.Clamp(float.IsFinite(current) ? current : -40f, -80f, 0f);
        target = Mathf.Clamp(float.IsFinite(target) ? target : current, -80f, 0f);
        var seconds = float.IsFinite(delta) ? Mathf.Max(0f, delta) : 0f;
        var rate = Mathf.Clamp(float.IsFinite(fadeSpeed) ? fadeSpeed : 1f, .05f, 20f);
        return Mathf.Clamp(Mathf.Lerp(current, target, 1f - Mathf.Exp(-rate * seconds)), -80f, 0f);
    }

    public static bool UsesLargeSwim(string stage) => stage is "subadult" or "adult";
    public static float SwimGainDb(string stage) => stage switch
    {
        "larva" => -32f, "juvenile" => -17f, "subadult" => -16f, _ => -13f,
    };
    public static float BoostGainDb(string stage) => stage switch
    {
        "larva" => -26f, "juvenile" => -15f, "subadult" => -13f, _ => -11f,
    };

    public static float VariedPitch(float basePitch, float previousPitch, float sample)
    {
        var offset = Mathf.Lerp(-.16f, .16f, Mathf.Clamp(sample, 0f, 1f));
        var pitch = basePitch * (1f + offset);
        if (Mathf.Abs(pitch - previousPitch) < .05f * basePitch)
            pitch = basePitch * (1f + (offset >= 0f ? -.12f : .12f));
        return pitch;
    }

    public static float VariedTurnGain(float baseDb, float previousDb, float sample)
    {
        var offset = Mathf.Lerp(-3f, 3f, Mathf.Clamp(sample, 0f, 1f));
        var gain = baseDb + offset;
        if (Mathf.Abs(gain - previousDb) < 1.5f)
            gain = baseDb + (offset >= 0f ? -3f : 3f);
        return gain;
    }

    public static float VariedLengthFraction(float previous, float sample)
    {
        var fraction = Mathf.Lerp(.72f, .98f, Mathf.Clamp(sample, 0f, 1f));
        if (Mathf.Abs(fraction - previous) < .08f)
            fraction = previous > .85f ? .72f : .98f;
        return fraction;
    }

    public static float VariedTurnLengthFraction(float turnStrength, float previous, float sample)
    {
        var center = Mathf.Lerp(.54f, .84f, Mathf.Clamp(turnStrength, 0f, 1f));
        var low = center - .1f;
        var high = center + .1f;
        var fraction = Mathf.Lerp(low, high, Mathf.Clamp(sample, 0f, 1f));
        if (Mathf.Abs(fraction - previous) < .08f)
            fraction = Mathf.Abs(low - previous) > Mathf.Abs(high - previous) ? low : high;
        return fraction;
    }

    public override void _Ready()
    {
        if (_fish is null || _stage is null || !IsInstanceValid(_fish))
            throw new InvalidOperationException("World audio must be configured before entering the tree");
        ProcessPriority = 20; // Observe the completed fish motor tick.
        _pitchRandom.Randomize();
        _gentle = Load("res://assets/sfx/swim_gentle_mono.wav");
        _large = Load("res://assets/sfx/swim_large_mono.wav");
        _largeBoost = Load("res://assets/sfx/escape_burst_mono.wav");
        _currentDb = TargetVolume(_waterline, _fish.GlobalPosition.Y, SurfaceVolumeDb,
            DeepVolumeDb, RiseDepth, FullDepth, ResponsePower);
        _bed = Load(_bedPath);
        _ambience = new AudioStreamPlayer { Name = "UnderwaterBed", Bus = "Ambience",
            Stream = _bed, VolumeDb = _currentDb };
        AddChild(_ambience);
        _ambience.Play();
        AmbienceStarts++;
        _swim = CreateEmitter(_fish, "SwimStart");
        _boost = CreateEmitter(_fish, "BoostStart");
        _swimVoices.Add(_swim);
        _boostVoices.Add(_boost);
    }

    private static AudioStream Load(string path) => ResourceLoader.Load<AudioStream>(path)
        ?? throw new InvalidOperationException("AUDIO_ASSET_UNAVAILABLE " + path);

    public static AudioStreamPlayer3D CreateEmitter(Node3D source, string name,
        float maxDistance = 14f)
    {
        var player = new AudioStreamPlayer3D { Name = name, Bus = "SFX",
            MaxDistance = maxDistance, UnitSize = 2f, MaxDb = -8f };
        source.AddChild(player);
        return player;
    }

    public override void _PhysicsProcess(double delta)
    {
        var seconds = (float)delta;
        UpdateDepth(_fish.GlobalPosition.Y, seconds);
        AdvanceCueLengths(seconds);
        switch (_gate.Step(_fish.PresentationEffort, _fish.PresentationSpeed,
            _fish.Tuning.MaxSpeed, _fish.PresentationAngularTurn,
            _fish.BoostActive, CanPlayCue(), seconds))
        {
            case SwimAudioEvent.Start:
                PlaySwimCue();
                break;
            case SwimAudioEvent.Turn:
                PlaySwimCue(turning: true, turnStrength: _gate.CurrentTurnStrength);
                break;
            case SwimAudioEvent.Boost:
                PlayBoostCue();
                break;
        }
    }

    private AudioStreamPlayer3D AvailableVoice(List<AudioStreamPlayer3D> voices, string name)
    {
        foreach (var voice in voices)
            if (!voice.Playing)
            {
                _timedCues.RemoveAll(cue => cue.Voice == voice);
                return voice;
            }
        var added = CreateEmitter(_fish, name + (voices.Count + 1));
        voices.Add(added);
        return added;
    }

    private void ScheduleLength(AudioStreamPlayer3D voice, float? fraction = null)
    {
        if (fraction is null)
            _lastLengthFraction = VariedLengthFraction(_lastLengthFraction, _pitchRandom.Randf());
        var sourceSeconds = (float)(voice.Stream?.GetLength() ?? 0d);
        var duration = Mathf.Max(.1f, sourceSeconds / voice.PitchScale *
            (fraction ?? _lastLengthFraction));
        _timedCues.Add(new TimedCue(voice, duration, voice.VolumeDb));
    }

    internal void AdvanceCueLengths(float seconds)
    {
        if (!float.IsFinite(seconds) || seconds <= 0f) return;
        for (var i = _timedCues.Count - 1; i >= 0; i--)
        {
            var cue = _timedCues[i];
            if (!cue.Voice.Playing)
            {
                _timedCues.RemoveAt(i);
                continue;
            }
            cue.Elapsed += seconds;
            if (cue.Elapsed >= cue.Duration)
            {
                cue.Voice.Stop();
                _timedCues.RemoveAt(i);
                continue;
            }
            var fade = Mathf.Min(.12f, cue.Duration * .25f);
            var fadeProgress = Mathf.Clamp((cue.Elapsed - (cue.Duration - fade)) / fade, 0f, 1f);
            cue.Voice.VolumeDb = cue.GainDb - 35f * fadeProgress;
        }
    }

    internal float ScheduledLength(AudioStreamPlayer3D voice)
    {
        foreach (var cue in _timedCues)
            if (cue.Voice == voice) return cue.Duration;
        return 0f;
    }

    internal AudioStreamPlayer3D PlaySwimCue(bool turning = false, float turnStrength = 0f)
    {
        var voice = AvailableVoice(_swimVoices, "SwimStart");
        voice.Stream = UsesLargeSwim(_stage()) ? _large : _gentle;
        var baseDb = SwimGainDb(_stage());
        if (turning)
        {
            _lastTurnVolumeDb = VariedTurnGain(baseDb, _lastTurnVolumeDb,
                _pitchRandom.Randf());
            voice.VolumeDb = _lastTurnVolumeDb;
        }
        else voice.VolumeDb = baseDb;
        _lastSwimPitch = VariedPitch(1f, _lastSwimPitch, _pitchRandom.Randf());
        voice.PitchScale = _lastSwimPitch;
        voice.Play();
        if (turning)
            _lastTurnLengthFraction = VariedTurnLengthFraction(turnStrength,
                _lastTurnLengthFraction, _pitchRandom.Randf());
        ScheduleLength(voice, turning ? _lastTurnLengthFraction : null);
        SwimStarts++;
        return voice;
    }

    internal AudioStreamPlayer3D PlayBoostCue()
    {
        var voice = AvailableVoice(_boostVoices, "BoostStart");
        voice.Stream = UsesLargeSwim(_stage()) ? _large : _largeBoost;
        _lastBoostPitch = VariedPitch(UsesLargeSwim(_stage()) ? .82f : 1f,
            _lastBoostPitch, _pitchRandom.Randf());
        voice.PitchScale = _lastBoostPitch;
        voice.VolumeDb = BoostGainDb(_stage());
        voice.Play();
        ScheduleLength(voice);
        BoostStarts++;
        return voice;
    }

    private bool CanPlayCue() => !_focusPaused && !GetTree().Paused &&
        _fish.GetInputArmed() && GetViewport().GetCamera3D() is not null &&
        !_stage().Equals("egg", StringComparison.Ordinal) && _fish.IsPhysicsProcessing();

    public void UpdateDepth(float fishY, float seconds)
    {
        var target = TargetVolume(_waterline, fishY, SurfaceVolumeDb,
            DeepVolumeDb, RiseDepth, FullDepth, ResponsePower);
        _currentDb = SmoothVolume(_currentDb, target, seconds, FadeSpeed);
        _ambience.VolumeDb = _currentDb;
    }

    public void ResetEvents()
    {
        _gate.Reset();
        foreach (var voice in _swimVoices) voice.Stop();
        foreach (var voice in _boostVoices) voice.Stop();
        _timedCues.Clear();
    }

    public void Shutdown()
    {
        _timedCues.Clear();
        if (_ambience is not null)
        {
            _ambience.Stop();
            _ambience.Stream = null;
        }
        _bed?.Dispose();
        _bed = null;
        foreach (var voice in _swimVoices)
        {
            if (!IsInstanceValid(voice)) continue;
            voice.Stop();
            voice.Stream = null;
        }
        foreach (var voice in _boostVoices)
        {
            if (!IsInstanceValid(voice)) continue;
            voice.Stop();
            voice.Stream = null;
        }
    }

    public override void _Notification(int what)
    {
        if (_ambience is null) return;
        if (what == NotificationApplicationFocusOut)
        {
            _focusPaused = true;
            ResetEvents();
            _ambience.StreamPaused = true;
        }
        else if (what == NotificationApplicationFocusIn)
        {
            _focusPaused = false;
            _ambience.StreamPaused = false;
        }
    }

    public override void _ExitTree() => Shutdown();
}
