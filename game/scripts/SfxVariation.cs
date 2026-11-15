#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace Fish;

/// <summary>Per-category one-shot tuning. Fractions are measured in source time.</summary>
public readonly record struct SfxVariationProfile(
    float BasePitch, float PitchMin, float PitchMax, float PitchSeparation,
    float BaseVolumeDb, float VolumeMinDb, float VolumeMaxDb, float VolumeSeparationDb,
    float DurationMin, float DurationMax, float DurationSeparation,
    float IntenseDurationMin = float.NaN, float IntenseDurationMax = float.NaN,
    float IntensityGainDb = 0f, float FadeSeconds = .12f);

public readonly record struct SfxVariation(float Pitch, float VolumeDb, float DurationFraction)
{
    public float PlayedSeconds(float sourceSeconds) =>
        sourceSeconds * DurationFraction / Pitch;
}

/// <summary>Random history belongs to one sound category, never to an emitter.</summary>
public sealed class SfxVariationState
{
    private readonly RandomNumberGenerator _random = new();
    private float? _pitch, _volume, _duration;

    public SfxVariationState(ulong? seed = null)
    {
        if (seed is { } value) _random.Seed = value;
        else _random.Randomize();
    }

    public void Reset() => (_pitch, _volume, _duration) = (null, null, null);

    public SfxVariation Next(SfxVariationProfile profile, float intensity = 0f)
    {
        var result = Sample(profile, intensity, _pitch, _volume, _duration,
            _random.Randf(), _random.Randf(), _random.Randf());
        (_pitch, _volume, _duration) = (result.Pitch, result.VolumeDb, result.DurationFraction);
        return result;
    }

    // Explicit samples make all variation bounds and separation rules testable.
    public static SfxVariation Sample(SfxVariationProfile p, float intensity,
        float? previousPitch, float? previousVolume, float? previousDuration,
        float pitchSample, float volumeSample, float durationSample)
    {
        intensity = Mathf.Clamp(intensity, 0f, 1f);
        var pitch = Distinct(p.BasePitch * p.PitchMin, p.BasePitch * p.PitchMax,
            previousPitch, p.PitchSeparation * p.BasePitch, pitchSample);
        var intensityGain = p.IntensityGainDb * intensity;
        var gain = Distinct(p.BaseVolumeDb + p.VolumeMinDb + intensityGain,
            p.BaseVolumeDb + p.VolumeMaxDb + intensityGain, previousVolume,
            p.VolumeSeparationDb, volumeSample);
        var durationLow = float.IsFinite(p.IntenseDurationMin)
            ? Mathf.Lerp(p.DurationMin, p.IntenseDurationMin, intensity) : p.DurationMin;
        var durationHigh = float.IsFinite(p.IntenseDurationMax)
            ? Mathf.Lerp(p.DurationMax, p.IntenseDurationMax, intensity) : p.DurationMax;
        var duration = Distinct(durationLow, durationHigh, previousDuration,
            p.DurationSeparation, durationSample);
        return new SfxVariation(pitch, gain, duration);
    }

    private static float Distinct(float low, float high, float? previous,
        float separation, float sample)
    {
        var value = Mathf.Lerp(low, high, Mathf.Clamp(sample, 0f, 1f));
        if (previous is { } prior && Mathf.Abs(value - prior) < separation)
            value = Mathf.Abs(low - prior) > Mathf.Abs(high - prior) ? low : high;
        return value;
    }
}

/// <summary>Bounded, reusable 3D voices with independent gain, pitch and timed fades.</summary>
public sealed class SfxOneShotPlayer
{
    private sealed class Cue(AudioStreamPlayer3D voice, float duration, float gain, float fade)
    {
        public AudioStreamPlayer3D Voice = voice;
        public float Duration = duration;
        public float Gain = gain;
        public float Fade = fade;
        public float Elapsed;
    }

    private readonly Node3D _source;
    private readonly string _name;
    private readonly int _maxVoices;
    private readonly List<AudioStreamPlayer3D> _voices = [];
    private readonly List<Cue> _cues = [];
    private readonly SfxVariationState _state;
    public AudioStreamPlayer3D Primary => _voices[0];

    public SfxOneShotPlayer(Node3D source, string name, int maxVoices = 8,
        ulong? seed = null)
    {
        _source = source;
        _name = name;
        _maxVoices = maxVoices;
        _state = new SfxVariationState(seed);
        _voices.Add(WorldAudio.CreateEmitter(source, name));
    }

    public AudioStreamPlayer3D? Play(AudioStream stream, SfxVariationProfile profile,
        float intensity = 0f, Vector3? worldPosition = null)
    {
        AudioStreamPlayer3D? voice = null;
        foreach (var candidate in _voices)
            if (!candidate.Playing) { voice = candidate; break; }
        if (voice is null)
        {
            if (_voices.Count >= _maxVoices) return null;
            voice = WorldAudio.CreateEmitter(_source, _name + (_voices.Count + 1));
            _voices.Add(voice);
        }
        _cues.RemoveAll(cue => cue.Voice == voice);
        var selection = _state.Next(profile, intensity);
        voice.Position = worldPosition is { } point ? _source.ToLocal(point) : Vector3.Zero;
        voice.Stream = stream;
        voice.PitchScale = selection.Pitch;
        voice.VolumeDb = selection.VolumeDb;
        voice.Play();
        var length = (float)stream.GetLength();
        _cues.Add(new Cue(voice, Mathf.Max(.1f, selection.PlayedSeconds(length)),
            selection.VolumeDb, profile.FadeSeconds));
        return voice;
    }

    public void Advance(float seconds)
    {
        if (!float.IsFinite(seconds) || seconds <= 0f) return;
        for (var i = _cues.Count - 1; i >= 0; i--)
        {
            var cue = _cues[i];
            if (!cue.Voice.Playing) { _cues.RemoveAt(i); continue; }
            cue.Elapsed += seconds;
            if (cue.Elapsed >= cue.Duration)
            { cue.Voice.Stop(); _cues.RemoveAt(i); continue; }
            var fade = Mathf.Min(cue.Fade, cue.Duration * .25f);
            if (fade > 0f)
                cue.Voice.VolumeDb = cue.Gain - 35f *
                    Mathf.Clamp((cue.Elapsed - (cue.Duration - fade)) / fade, 0f, 1f);
        }
    }

    public float ScheduledLength(AudioStreamPlayer3D voice)
    {
        foreach (var cue in _cues)
            if (cue.Voice == voice) return cue.Duration;
        return 0f;
    }

    public void Cancel(bool resetVariation = false)
    {
        foreach (var voice in _voices) if (GodotObject.IsInstanceValid(voice)) voice.Stop();
        _cues.Clear();
        if (resetVariation) _state.Reset();
    }

    public void Shutdown()
    {
        Cancel(true);
        foreach (var voice in _voices)
            if (GodotObject.IsInstanceValid(voice))
            {
                voice.Stream = null;
                if (!voice.IsQueuedForDeletion()) voice.QueueFree();
            }
    }
}
