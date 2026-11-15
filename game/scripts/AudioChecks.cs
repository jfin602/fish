#nullable enable
using Godot;
using System;
using System.Linq;

namespace Fish;

public static class AudioChecks
{
    private static int _scenePass;
    private static void Require(bool condition, string detail)
    {
        if (!condition) throw new InvalidOperationException("AUDIO_CHECK_FAILED " + detail);
    }

    public static void Run(PikeEncounter encounter)
    {
        try
        {
            _scenePass++;
            Require(_scenePass <= 2, "bounded scene retry");
            var audio = encounter.GetNode<WorldAudio>("WorldAudio");
            var fish = encounter.GetNode<FishController>("Fish");
            Require(encounter.GetChildren().Count(node => node is WorldAudio) == 1,
                "one world audio node");
            Require(audio.AmbiencePlayer.GetParent() == audio, "bed is non-positional");
            Require(audio.AmbiencePlayer.Playing && audio.AmbienceStarts == 1,
                "one continuous bed");
            Require(audio.AmbiencePlayer.Bus == "Ambience" &&
                audio.SwimEmitter.Bus == "SFX" && audio.BoostEmitter.Bus == "SFX",
                "bus routing");
            Require(AudioServer.GetBusIndex("Ambience") >= 0 &&
                AudioServer.GetBusIndex("SFX") >= 0, "installed bus layout");
            Require(audio.SwimEmitter.GetParent() == fish &&
                audio.BoostEmitter.GetParent() == fish, "fish body emitters");
            Require(encounter.GetViewport().GetCamera3D() is { Current: true },
                "active camera listener");
            Require(audio.SwimEmitter.MaxDistance == 14f &&
                audio.BoostEmitter.MaxDistance == 14f, "positional range");
            var surface = fish.PondWaterlineHeight;
            var previous = float.NegativeInfinity;
            for (var i = 0; i <= 1000; i++)
            {
                var depth = i * .015f;
                var volume = WorldAudio.TargetVolume(surface, surface - depth,
                    audio.SurfaceVolumeDb, audio.DeepVolumeDb, audio.RiseDepth,
                    audio.FullDepth, audio.ResponsePower);
                Require(volume >= previous - .0001f && float.IsFinite(volume),
                    "monotone finite depth curve");
                if (i > 0) Require(volume - previous < .15f, "continuous depth curve");
                previous = volume;
            }
            Require(WorldAudio.TargetVolume(surface, surface, -39, -14, .5f, 9f, 1.25f) <
                WorldAudio.TargetVolume(surface, surface - 12f, -39, -14, .5f, 9f, 1.25f),
                "surface quieter than deep");
            Require(WorldAudio.TargetVolume(surface, surface + 100f, -39, -14, .5f, 9f, 1.25f)
                == -39f, "breach clamped");
            Require(float.IsFinite(WorldAudio.TargetVolume(float.NaN, float.PositiveInfinity,
                float.NaN, 200f, float.NaN, float.NaN, float.NaN)) &&
                float.IsFinite(WorldAudio.SmoothVolume(float.NaN, float.PositiveInfinity,
                    float.NaN, float.NaN)), "invalid samples clamped");
            var current = -39f;
            for (var i = 0; i < 120; i++)
            {
                var next = WorldAudio.SmoothVolume(current, -14f, 1f / 60f, 1.5f);
                Require(next >= current && next <= -14f, "smooth descent");
                current = next;
            }
            for (var i = 0; i < 120; i++)
            {
                var next = WorldAudio.SmoothVolume(current, -39f, 1f / 60f, 1.5f);
                Require(next <= current && next >= -39f, "smooth ascent");
                current = next;
            }
            Require(audio.AmbienceStarts == 1, "gain does not restart loop");
            audio.SwimEmitter.VolumeDb = WorldAudio.SwimGainDb("juvenile");
            audio.BoostEmitter.VolumeDb = WorldAudio.BoostGainDb("juvenile");
            var swimDb = audio.SwimEmitter.VolumeDb;
            var boostDb = audio.BoostEmitter.VolumeDb;
            var initialDb = audio.CurrentVolumeDb;
            for (var i = 0; i < 120; i++) audio.UpdateDepth(surface - 12f, 1f / 60f);
            Require(audio.CurrentVolumeDb > initialDb &&
                Mathf.IsEqualApprox(audio.AmbiencePlayer.VolumeDb, audio.CurrentVolumeDb) &&
                audio.AmbiencePlayer.Playing && audio.AmbienceStarts == 1,
                "live depth gain without replay");
            var deeperDb = audio.CurrentVolumeDb;
            for (var i = 0; i < 120; i++) audio.UpdateDepth(surface, 1f / 60f);
            Require(audio.CurrentVolumeDb < deeperDb && audio.AmbienceStarts == 1,
                "live ascent without replay");
            Require(audio.SwimEmitter.VolumeDb == swimDb &&
                audio.BoostEmitter.VolumeDb == boostDb,
                "depth does not change swim or boost gain");
            CheckGate();
            CheckVariation();
            Require(!WorldAudio.UsesLargeSwim("larva") &&
                !WorldAudio.UsesLargeSwim("juvenile") &&
                WorldAudio.UsesLargeSwim("subadult") &&
                WorldAudio.UsesLargeSwim("adult") &&
                WorldAudio.SwimGainDb("larva") < WorldAudio.SwimGainDb("juvenile") &&
                WorldAudio.SwimGainDb("juvenile") < WorldAudio.SwimGainDb("adult") &&
                WorldAudio.BoostGainDb("larva") < WorldAudio.BoostGainDb("adult"),
                "registered stage controls sound and gain");
            var firstTurn = audio.PlaySwimCue(turning: true, turnStrength: 0f);
            var firstPitch = firstTurn.PitchScale;
            var firstGain = firstTurn.VolumeDb;
            var firstLengthFraction = audio.ScheduledLength(firstTurn) * firstPitch /
                (float)firstTurn.Stream.GetLength();
            var secondTurn = audio.PlaySwimCue(turning: true, turnStrength: 1f);
            var secondLengthFraction = audio.ScheduledLength(secondTurn) * secondTurn.PitchScale /
                (float)secondTurn.Stream.GetLength();
            Require(firstTurn != secondTurn && firstTurn.Playing && secondTurn.Playing &&
                firstTurn.PitchScale == firstPitch && firstTurn.VolumeDb == firstGain &&
                Mathf.Abs(secondTurn.PitchScale - firstPitch) >= .049f &&
                Mathf.Abs(secondTurn.VolumeDb - firstGain) >= 1.49f &&
                secondLengthFraction > firstLengthFraction + .09f,
                "turn cues overlap with independent pitch and gain; stronger turns last longer");
            var thirdTurn = audio.PlaySwimCue(turning: true, turnStrength: 1f);
            var thirdLengthFraction = audio.ScheduledLength(thirdTurn) * thirdTurn.PitchScale /
                (float)thirdTurn.Stream.GetLength();
            Require(thirdTurn != secondTurn && thirdTurn.Playing && secondTurn.Playing &&
                Mathf.Abs(thirdLengthFraction - secondLengthFraction) >= .079f,
                "turn duration varies between plays at the same strength");
            var firstBoost = audio.PlayBoostCue();
            var secondBoost = audio.PlayBoostCue();
            Require(firstBoost != secondBoost && firstBoost.Playing && secondBoost.Playing,
                "boost cues can overlap");
            audio.ResetEvents();
            var timedFirst = audio.PlaySwimCue();
            var originalGain = timedFirst.VolumeDb;
            var duration = audio.ScheduledLength(timedFirst);
            Require(duration > .1f, "swim cue has a scheduled length");
            audio.AdvanceCueLengths(duration - .06f);
            Require(timedFirst.Playing && timedFirst.VolumeDb < originalGain,
                "short cue fades before stopping");
            var timedSecond = audio.PlaySwimCue();
            audio.AdvanceCueLengths(.07f);
            Require(!timedFirst.Playing && timedSecond.Playing,
                "first cue ends without cutting off a later cue");
            audio.Shutdown();
            audio.Free();
            var tree = encounter.GetTree();
            if (_scenePass == 1)
            {
                tree.CreateTimer(.5).Timeout += () =>
                {
                    if (tree.ChangeSceneToFile(PikeEncounterAssets.ScenePath) != Error.Ok)
                    {
                        GD.PushError("AUDIO_CHECK_FAILED encounter retry scene load");
                        tree.Quit(1);
                    }
                };
            }
            else
            {
                tree.CreateTimer(.5).Timeout += () =>
                {
                    if (tree.ChangeSceneToFile("res://scenes/world_select.tscn") != Error.Ok)
                    {
                        GD.PushError("AUDIO_CHECK_FAILED world return scene load");
                        tree.Quit(1);
                        return;
                    }
                    tree.CreateTimer(.5).Timeout += () =>
                    {
                        var selected = tree.CurrentScene;
                        if (selected is not WorldSelect ||
                            selected.GetChildren().Any(node => node is WorldAudio))
                        {
                            GD.PushError("AUDIO_CHECK_FAILED world return retained audio");
                            tree.Quit(1);
                            return;
                        }
                        GD.Print("AUDIO_CHECK_OK bed=1 curve=monotone,smooth swim_depth=fixed buses=Ambience,SFX emitter=fish listener=active-camera events=motion,turn-strength,pitch,turn-volume,length,overlap,boost retry=one-bed return=no-bed");
                        tree.Quit();
                    };
                };
            }
        }
        catch (Exception error)
        {
            encounter.GetNodeOrNull<WorldAudio>("WorldAudio")?.Shutdown();
            GD.PushError(error.ToString());
            encounter.GetTree().CreateTimer(.1).Timeout += () => encounter.GetTree().Quit(1);
        }
    }

    private static void CheckGate()
    {
        var gate = new SwimAudioGate();
        const float tick = 1f / 60f;
        SwimAudioEvent Step(float effort = 0, float speed = 0, float turn = 0,
            bool boost = false, bool eligible = true, float delta = tick) =>
            gate.Step(effort, speed, 10, turn, boost, eligible, delta);
        Require(Step() == SwimAudioEvent.None, "idle motor is silent");
        Require(Step(effort: 1) == SwimAudioEvent.None,
            "control without physical movement stays silent");
        Require(Step(effort: 1, speed: 1.3f) == SwimAudioEvent.Start,
            "moving swim starts a cue");
        Require(Step(effort: 1, speed: 2) == SwimAudioEvent.None,
            "sustained thrust does not replay onset");
        for (var i = 0; i < 90; i++)
            Require(Step(speed: 3.99f, turn: 1f) == SwimAudioEvent.None,
                "strong turns below 40 percent full speed stay silent");
        Require(Step(speed: 4f, turn: 1f) == SwimAudioEvent.None,
            "reaching 40 percent speed still requires sustained rotation");
        Require(Step(speed: 4f, delta: .3f) == SwimAudioEvent.None,
            "turn activity resets after a quiet interval");
        for (var i = 0; i < 90; i++)
            Require(Step(speed: 4f, turn: .3f) == SwimAudioEvent.None,
                "light repeated mouse turning stays silent");
        Require(Step(speed: 4f, turn: 1f) == SwimAudioEvent.None,
            "one-frame mouse flick does not play a cue");
        Require(Step(speed: 4f, turn: 0, delta: .3f) == SwimAudioEvent.None,
            "quiet turn interval stays silent");
        var turnCues = 0;
        for (var i = 0; i < 12; i++)
            if (Step(speed: 4f, turn: .8f) == SwimAudioEvent.Turn) turnCues++;
        Require(turnCues == 1, "sustained measured turn starts once after smoothing");
        for (var i = 0; i < 30; i++)
            Require(Step(speed: 4f, turn: .8f) == SwimAudioEvent.None,
                "sustained turn observes the longer cue spacing");
        turnCues = 0;
        for (var i = 0; i < 60; i++)
            if (Step(speed: 4f, turn: .8f) == SwimAudioEvent.Turn) turnCues++;
        Require(turnCues is >= 1 and <= 2,
            "sustained turn renews cues at bounded cadence");
        Require(Step(speed: 4f, turn: 0, delta: .3f) == SwimAudioEvent.None,
            "turn quiet period resets cadence");
        turnCues = 0;
        for (var i = 0; i < 12; i++)
            if (Step(speed: 4f, turn: .8f) == SwimAudioEvent.Turn) turnCues++;
        Require(turnCues == 1, "new deliberate turn plays after quiet period");
        for (var i = 0; i < 90; i++)
            Require(Step(speed: 3.99f, turn: 1f) == SwimAudioEvent.None,
                "falling below turn speed stops new cues during sustained steering");
        Require(Step(boost: true) == SwimAudioEvent.Boost &&
            Step(boost: true) == SwimAudioEvent.None,
            "actual boost transition plays once");
        Require(Step(eligible: false) == SwimAudioEvent.None &&
            Step() == SwimAudioEvent.None, "pause resets event gate");
        for (var i = 0; i <= 20; i++)
        {
            var sample = i / 20f;
            var shortTurn = WorldAudio.VariedTurnLengthFraction(0, 1, sample);
            var longTurn = WorldAudio.VariedTurnLengthFraction(1, 1, sample);
            Require(shortTurn >= .439f && shortTurn <= .641f &&
                longTurn >= .739f && longTurn <= .941f &&
                longTurn > shortTurn + .089f,
                "turn duration follows measured turn strength despite variation");
        }
    }

    private static void CheckVariation()
    {
        var previous = 1f;
        for (var i = 0; i <= 100; i++)
        {
            var pitch = WorldAudio.VariedPitch(1f, previous, i / 100f);
            Require(pitch >= .84f && pitch <= 1.16f &&
                Mathf.Abs(pitch - previous) >= .0499f,
                "swim pitch changes clearly between cues");
            previous = pitch;
        }
        var boostPitch = WorldAudio.VariedPitch(.82f, .82f, .5f);
        Require(boostPitch >= .82f * .84f && boostPitch <= .82f * 1.16f &&
            !Mathf.IsEqualApprox(boostPitch, .82f), "boost pitch varies around stage base");
        previous = -17f;
        for (var i = 0; i <= 100; i++)
        {
            var gain = WorldAudio.VariedTurnGain(-17f, previous, i / 100f);
            Require(gain >= -20f && gain <= -14f &&
                Mathf.Abs(gain - previous) >= 1.49f,
                "turn volume varies within a controlled range");
            previous = gain;
        }
        for (var i = 0; i <= 20; i++)
            for (var j = 0; j <= 20; j++)
            {
                var prior = -20f + i * .3f;
                var gain = WorldAudio.VariedTurnGain(-17f, prior, j / 20f);
                Require(gain >= -20f && gain <= -14f &&
                    Mathf.Abs(gain - prior) >= 1.49f,
                    "turn gain remains distinct for varied prior levels");
            }
        previous = 1f;
        for (var i = 0; i <= 100; i++)
        {
            var length = WorldAudio.VariedLengthFraction(previous, i / 100f);
            Require(length >= .72f && length <= .98f &&
                Mathf.Abs(length - previous) >= .0799f,
                "cue length varies independently within the selected range");
            previous = length;
        }
    }
}
