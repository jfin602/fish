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
    private sealed class SurfaceActorCue(PreyActor actor, float radius,
        SfxOneShotPlayer arrival, SfxOneShotPlayer dive)
    {
        public PreyActor Actor = actor;
        public float Radius = radius;
        public SurfaceSplashDetector Detector = new();
        public SfxOneShotPlayer Arrival = arrival;
        public SfxOneShotPlayer Dive = dive;
    }
    [Export] public float SurfaceVolumeDb { get; set; } = -39f;
    [Export] public float DeepVolumeDb { get; set; } = -14f;
    [Export] public float RiseDepth { get; set; } = .5f;
    [Export] public float FullDepth { get; set; } = 9f;
    [Export] public float ResponsePower { get; set; } = 1.25f;
    [Export] public float FadeSpeed { get; set; } = 1.5f;
    [Export] public float ShallowSurfaceVolumeDb { get; set; } = -26f;
    [Export] public float ShallowFadeDepth { get; set; } = 5f;
    [Export] public float ShallowFadeSpeed { get; set; } = 2f;
    [Export] public float SplashArrivalVolumeDb { get; set; } = -20f;
    [Export] public float SplashDiveVolumeDb { get; set; } = -18f;
    [Export] public float SplashSizeGainDb { get; set; } = 9f;
    [Export] public float SplashForceGainDb { get; set; } = 3f;
    [Export] public float SplashPitchSpread { get; set; } = .14f;
    [Export] public float SplashDurationMin { get; set; } = .68f;
    [Export] public float SplashDurationMax { get; set; } = .98f;

    public const string PondBed = "res://assets/sfx/pond_underwater_deep.ogg";
    public const string PondShallows = "res://assets/sfx/pond_underwater_shallows.ogg";
    private FishController _fish = null!;
    private Func<string> _stage = null!;
    private string _bedPath = "";
    private AudioStreamPlayer _ambience = null!;
    private AudioStreamPlayer _shallowAmbience = null!;
    private SfxOneShotPlayer _swim = null!;
    private SfxOneShotPlayer _turn = null!;
    private SfxOneShotPlayer _boost = null!;
    private SfxOneShotPlayer _arrival = null!;
    private SfxOneShotPlayer _dive = null!;
    private readonly SwimAudioGate _gate = new();
    private readonly SurfaceSplashDetector _surface = new();
    private readonly List<SurfaceActorCue> _surfaceActors = [];
    private float _waterline;
    private float _currentDb;
    private float _currentShallowDb;
    private bool _focusPaused;
    private bool _wasEligible;
    private AudioStream _gentle = null!;
    private AudioStream? _bed;
    private AudioStream? _shallowBed;
    private AudioStream _large = null!;
    private AudioStream _largeBoost = null!;
    private AudioStream _splash = null!;
    public int AmbienceStarts { get; private set; }
    public int ShallowAmbienceStarts { get; private set; }
    public int SwimStarts { get; private set; }
    public int BoostStarts { get; private set; }
    public float CurrentVolumeDb => _currentDb;
    public float CurrentShallowVolumeDb => _currentShallowDb;
    public AudioStreamPlayer AmbiencePlayer => _ambience;
    public AudioStreamPlayer ShallowAmbiencePlayer => _shallowAmbience;
    public AudioStreamPlayer3D SwimEmitter => _swim.Primary;
    public AudioStreamPlayer3D BoostEmitter => _boost.Primary;
    public AudioStreamPlayer3D SplashEmitter => _arrival.Primary;
    internal int SurfaceActorCount => _surfaceActors.Count;

    public void RegisterSurfaceActor(PreyActor actor)
    {
        if (_surfaceActors.Exists(entry => entry.Actor == actor)) return;
        var radius = ((SphereShape3D)actor.GetNode<CollisionShape3D>("Collision").Shape).Radius;
        _surfaceActors.Add(new SurfaceActorCue(actor, radius,
            new SfxOneShotPlayer(actor, "SurfaceArrival", 3),
            new SfxOneShotPlayer(actor, "SurfaceDive", 3)));
    }

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

    public static float ShallowTargetVolume(float surfaceY, float fishY,
        float surfaceDb, float fadeDepth)
    {
        const float silentDb = -80f;
        if (!float.IsFinite(surfaceY) || !float.IsFinite(fishY)) return silentDb;
        var startDb = Mathf.Clamp(float.IsFinite(surfaceDb) ? surfaceDb : -26f,
            silentDb, 0f);
        var depthLimit = Mathf.Max(.01f,
            float.IsFinite(fadeDepth) ? fadeDepth : 5f);
        var depth = Mathf.Max(0f, surfaceY - fishY);
        var x = Mathf.Clamp(depth / depthLimit, 0f, 1f);
        var smooth = x * x * (3f - 2f * x);
        return Mathf.Lerp(startDb, silentDb, smooth);
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
        => SfxVariationState.Sample(PitchProfile(basePitch), 0f,
            previousPitch, null, null, sample, 0f, 0f).Pitch;

    public static float VariedTurnGain(float baseDb, float previousDb, float sample)
        => SfxVariationState.Sample(TurnProfile(baseDb, 0f), 0f,
            null, previousDb, null, 0f, sample, 0f).VolumeDb;

    public static float VariedLengthFraction(float previous, float sample)
        => SfxVariationState.Sample(PitchProfile(1f), 0f,
            null, null, previous, 0f, 0f, sample).DurationFraction;

    public static float VariedTurnLengthFraction(float turnStrength, float previous, float sample)
        => SfxVariationState.Sample(TurnProfile(0f, turnStrength), 0f,
            null, null, previous, 0f, 0f, sample).DurationFraction;

    private static SfxVariationProfile PitchProfile(float pitch, float gain = 0f) =>
        new(pitch, .84f, 1.16f, .05f, gain, 0f, 0f, 0f, .72f, .98f, .08f);

    private static SfxVariationProfile TurnProfile(float gain, float strength) =>
        new(1f, .84f, 1.16f, .05f, gain, -3f, 3f, 1.5f,
            .44f + .3f * Mathf.Clamp(strength, 0f, 1f),
            .64f + .3f * Mathf.Clamp(strength, 0f, 1f), .08f);

    public override void _Ready()
    {
        if (_fish is null || _stage is null || !IsInstanceValid(_fish))
            throw new InvalidOperationException("World audio must be configured before entering the tree");
        ProcessPriority = 20; // Observe the completed fish motor tick.
        _gentle = Load("res://assets/sfx/swim_gentle_mono.wav");
        _large = Load("res://assets/sfx/swim_large_mono.wav");
        _largeBoost = Load("res://assets/sfx/escape_burst_mono.wav");
        _splash = Load("res://assets/sfx/surface_breach_mono.wav");
        _currentDb = TargetVolume(_waterline, _fish.GlobalPosition.Y, SurfaceVolumeDb,
            DeepVolumeDb, RiseDepth, FullDepth, ResponsePower);
        _bed = Load(_bedPath);
        _ambience = new AudioStreamPlayer { Name = "UnderwaterBed", Bus = "Ambience",
            Stream = _bed, VolumeDb = _currentDb };
        AddChild(_ambience);
        _ambience.Play();
        AmbienceStarts++;
        if (_bedPath == PondBed)
        {
            _shallowBed = Load(PondShallows);
            var fishPosition = _fish.GlobalPosition;
            var shallowSurface = _fish.PondSurfaceHeightAt(fishPosition.X, fishPosition.Z);
            _currentShallowDb = ShallowTargetVolume(shallowSurface, fishPosition.Y,
                ShallowSurfaceVolumeDb, ShallowFadeDepth);
            _shallowAmbience = new AudioStreamPlayer { Name = "UnderwaterShallows",
                Bus = "Ambience", Stream = _shallowBed, VolumeDb = _currentShallowDb };
            AddChild(_shallowAmbience);
            _shallowAmbience.Play();
            ShallowAmbienceStarts++;
        }
        _swim = new SfxOneShotPlayer(_fish, "SwimStart");
        _turn = new SfxOneShotPlayer(_fish, "SwimTurn");
        _boost = new SfxOneShotPlayer(_fish, "BoostStart");
        _arrival = new SfxOneShotPlayer(_fish, "SurfaceArrival", 4);
        _dive = new SfxOneShotPlayer(_fish, "SurfaceDive", 4);
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
        var eligible = CanPlayCue();
        switch (_gate.Step(_fish.PresentationEffort, _fish.PresentationSpeed,
            _fish.Tuning.MaxSpeed, _fish.PresentationAngularTurn,
            _fish.BoostActive, eligible, seconds))
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
        if (!eligible && _wasEligible) ResetEvents();
        _wasEligible = eligible;
        var point = _fish.GlobalPosition;
        var surfaceY = _fish.PondSurfaceHeightAt(point.X, point.Z);
        var surfaceEvent = _surface.Step(point.Y, surfaceY, _fish.BodyRadius,
            _fish.Tuning.MaxSpeed, seconds, eligible, out var verticalSpeed);
        if (surfaceEvent != SurfaceSplashEvent.None)
            PlaySurfaceCue(surfaceEvent, new Vector3(point.X, surfaceY, point.Z), verticalSpeed);
        foreach (var entry in _surfaceActors)
        {
            entry.Arrival.Advance(seconds);
            entry.Dive.Advance(seconds);
            var actor = entry.Actor;
            if (!IsInstanceValid(actor)) continue;
            if (!actor.IsAlive)
            {
                entry.Arrival.Cancel();
                entry.Dive.Cancel();
            }
            var actorPoint = actor.GlobalPosition;
            var actorSurface = _fish.PondSurfaceHeightAt(actorPoint.X, actorPoint.Z);
            var speed = actor.Stage switch
            {
                "larva" => .34f, "juvenile" => .68f,
                "subadult" => 1.05f, "adult" => 1.35f, _ => .48f,
            };
            var actorEvent = entry.Detector.Step(actorPoint.Y, actorSurface,
                entry.Radius, speed, seconds, eligible && actor.IsAlive, out var actorVertical);
            if (actorEvent != SurfaceSplashEvent.None)
                PlaySurfaceCue(actorEvent,
                    new Vector3(actorPoint.X, actorSurface, actorPoint.Z), actorVertical,
                    entry.Radius, speed, entry.Arrival, entry.Dive);
        }
    }

    internal void AdvanceCueLengths(float seconds)
    {
        _swim.Advance(seconds);
        _turn.Advance(seconds);
        _boost.Advance(seconds);
        _arrival.Advance(seconds);
        _dive.Advance(seconds);
    }

    internal float ScheduledLength(AudioStreamPlayer3D voice) =>
        Mathf.Max(_swim.ScheduledLength(voice),
            Mathf.Max(_turn.ScheduledLength(voice),
            Mathf.Max(_boost.ScheduledLength(voice),
            Mathf.Max(_arrival.ScheduledLength(voice), _dive.ScheduledLength(voice)))));

    internal AudioStreamPlayer3D PlaySwimCue(bool turning = false, float turnStrength = 0f)
    {
        var large = UsesLargeSwim(_stage());
        var stream = large ? _large : _gentle;
        var gain = SwimGainDb(_stage());
        var voice = turning
            ? _turn.Play(stream, TurnProfile(gain, turnStrength))
            : _swim.Play(stream, PitchProfile(1f, gain));
        if (voice is null) return turning ? _turn.Primary : _swim.Primary;
        SwimStarts++;
        return voice;
    }

    internal AudioStreamPlayer3D PlayBoostCue()
    {
        var large = UsesLargeSwim(_stage());
        var voice = _boost.Play(large ? _large : _largeBoost,
            PitchProfile(large ? .82f : 1f, BoostGainDb(_stage())));
        if (voice is null) return _boost.Primary;
        BoostStarts++;
        return voice;
    }

    internal AudioStreamPlayer3D? PlaySurfaceCue(SurfaceSplashEvent kind,
        Vector3 contact, float verticalSpeed)
        => PlaySurfaceCue(kind, contact, verticalSpeed, _fish.BodyRadius,
            _fish.Tuning.MaxSpeed, _arrival, _dive);

    private AudioStreamPlayer3D? PlaySurfaceCue(SurfaceSplashEvent kind,
        Vector3 contact, float verticalSpeed, float radius, float maximumSpeed,
        SfxOneShotPlayer arrival, SfxOneShotPlayer dive)
    {
        // The size term is deliberately restrained for fry, yet reaches its full
        // value only on a large Bass. Speed is measured after collision resolution.
        var size = Mathf.Clamp((Mathf.Log(Mathf.Max(radius, .001f)) - Mathf.Log(.005f)) /
            (Mathf.Log(.5f) - Mathf.Log(.005f)), 0f, 1f);
        var force = Mathf.Clamp(Mathf.Abs(verticalSpeed) /
            Mathf.Max(.05f, maximumSpeed), 0f, 1f);
        var gain = (kind == SurfaceSplashEvent.Arrival
            ? SplashArrivalVolumeDb : SplashDiveVolumeDb) - SplashSizeGainDb * (1f - size);
        var profile = new SfxVariationProfile(1f, 1f - SplashPitchSpread,
            1f + SplashPitchSpread, .045f, gain, -2.5f, 2.5f, 1f,
            SplashDurationMin, SplashDurationMax, .07f,
            SplashDurationMin + .06f, SplashDurationMax,
            SplashForceGainDb, .10f);
        var player = kind == SurfaceSplashEvent.Arrival ? arrival : dive;
        return player.Play(_splash, profile, force, contact);
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
        if (_shallowAmbience is not null)
        {
            var fishPosition = _fish.GlobalPosition;
            var shallowSurface = _fish.PondSurfaceHeightAt(fishPosition.X, fishPosition.Z);
            var shallowTarget = ShallowTargetVolume(shallowSurface, fishY,
                ShallowSurfaceVolumeDb, ShallowFadeDepth);
            _currentShallowDb = SmoothVolume(_currentShallowDb, shallowTarget,
                seconds, ShallowFadeSpeed);
            _shallowAmbience.VolumeDb = _currentShallowDb;
        }
    }

    public void ResetEvents()
    {
        _wasEligible = false;
        _gate.Reset();
        _surface.Reset();
        _swim?.Cancel();
        _turn?.Cancel();
        _boost?.Cancel();
        _arrival?.Cancel();
        _dive?.Cancel();
        foreach (var entry in _surfaceActors)
        {
            entry.Detector.Reset();
            entry.Arrival.Cancel();
            entry.Dive.Cancel();
        }
    }

    public void Shutdown()
    {
        _surface.Reset();
        if (_ambience is not null)
        {
            _ambience.Stop();
            _ambience.Stream = null;
        }
        if (_shallowAmbience is not null)
        {
            _shallowAmbience.Stop();
            _shallowAmbience.Stream = null;
        }
        _bed?.Dispose();
        _bed = null;
        _shallowBed?.Dispose();
        _shallowBed = null;
        _swim?.Shutdown();
        _turn?.Shutdown();
        _boost?.Shutdown();
        _arrival?.Shutdown();
        _dive?.Shutdown();
        foreach (var entry in _surfaceActors)
        {
            entry.Detector.Reset();
            entry.Arrival.Shutdown();
            entry.Dive.Shutdown();
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
            if (_shallowAmbience is not null) _shallowAmbience.StreamPaused = true;
        }
        else if (what == NotificationApplicationFocusIn)
        {
            _focusPaused = false;
            _ambience.StreamPaused = false;
            if (_shallowAmbience is not null) _shallowAmbience.StreamPaused = false;
        }
    }

    public override void _ExitTree() => Shutdown();
}
