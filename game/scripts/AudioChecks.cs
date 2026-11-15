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
            Require(audio.SurfaceActorCount == encounter.PreyActors.Count,
                "all moving Bluegill actors share surface observation");
            Require(audio.AmbiencePlayer.GetParent() == audio, "bed is non-positional");
            Require(audio.ShallowAmbiencePlayer.GetParent() == audio,
                "shallow layer is non-positional");
            Require(audio.AmbiencePlayer.Playing && audio.AmbienceStarts == 1,
                "one continuous bed");
            Require(audio.ShallowAmbiencePlayer.Playing && audio.ShallowAmbienceStarts == 1,
                "one continuous shallow layer");
            Require(Mathf.IsEqualApprox(audio.ShallowSurfaceVolumeDb, -20f),
                "shallow surface level is doubled");
            Require(audio.AmbiencePlayer.Stream is AudioStreamOggVorbis { Loop: true },
                "approved bed loops");
            Require(audio.ShallowAmbiencePlayer.Stream is AudioStreamOggVorbis { Loop: true } &&
                audio.ShallowAmbiencePlayer.Stream.ResourcePath == WorldAudio.PondShallows,
                "approved shallow import loops");
            Require(audio.AmbiencePlayer.Bus == "Ambience" &&
                audio.ShallowAmbiencePlayer.Bus == "Ambience" &&
                audio.SwimEmitter.Bus == "SFX" && audio.BoostEmitter.Bus == "SFX" &&
                audio.SplashEmitter.Bus == "SFX",
                "bus routing");
            Require(AudioServer.GetBusIndex("Ambience") >= 0 &&
                AudioServer.GetBusIndex("SFX") >= 0, "installed bus layout");
            CheckBubbleSchedule();
            CheckBubbleRuntime(encounter, audio, fish);
            Require(audio.SwimEmitter.GetParent() == fish &&
                audio.BoostEmitter.GetParent() == fish &&
                audio.SplashEmitter.GetParent() == fish, "fish body emitters");
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
            var movingSurface = fish.PondSurfaceHeightAt(fish.GlobalPosition.X,
                fish.GlobalPosition.Z);
            var shallowPrevious = float.PositiveInfinity;
            for (var i = 0; i <= 500; i++)
            {
                var depth = i * .01f;
                var gain = WorldAudio.ShallowTargetVolume(movingSurface,
                    movingSurface - depth, audio.ShallowSurfaceVolumeDb,
                    audio.ShallowFadeDepth);
                Require(float.IsFinite(gain) && gain <= shallowPrevious + .0001f &&
                    gain >= -80f, "shallow depth curve is finite and monotone");
                if (i > 0) Require(shallowPrevious - gain < .2f,
                    "shallow fade is continuous");
                shallowPrevious = gain;
            }
            Require(Mathf.IsEqualApprox(WorldAudio.ShallowTargetVolume(movingSurface,
                movingSurface, audio.ShallowSurfaceVolumeDb, audio.ShallowFadeDepth),
                audio.ShallowSurfaceVolumeDb) &&
                WorldAudio.ShallowTargetVolume(movingSurface, movingSurface - 5f,
                    audio.ShallowSurfaceVolumeDb, audio.ShallowFadeDepth) == -80f &&
                WorldAudio.ShallowTargetVolume(movingSurface, movingSurface - 12f,
                    audio.ShallowSurfaceVolumeDb, audio.ShallowFadeDepth) == -80f,
                "shallow layer is quiet at surface and silent by five metres");
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
            Require(audio.CurrentShallowVolumeDb < -78f &&
                Mathf.IsEqualApprox(audio.ShallowAmbiencePlayer.VolumeDb,
                    audio.CurrentShallowVolumeDb) &&
                audio.ShallowAmbiencePlayer.Playing && audio.ShallowAmbienceStarts == 1,
                "shallow layer fades without replay at deep fish position");
            var deeperDb = audio.CurrentVolumeDb;
            for (var i = 0; i < 120; i++) audio.UpdateDepth(surface, 1f / 60f);
            Require(audio.CurrentVolumeDb < deeperDb && audio.AmbienceStarts == 1,
                "live ascent without replay");
            Require(audio.CurrentShallowVolumeDb > -40f &&
                audio.ShallowAmbienceStarts == 1 && audio.ShallowAmbiencePlayer.Playing,
                "shallow layer rises smoothly on fish ascent");
            var deepBeforeShallowTuning = audio.CurrentVolumeDb;
            audio.ShallowSurfaceVolumeDb = -40f;
            audio.UpdateDepth(surface, 1f / 60f);
            var expectedDeep = WorldAudio.SmoothVolume(deepBeforeShallowTuning,
                WorldAudio.TargetVolume(surface, surface, audio.SurfaceVolumeDb,
                    audio.DeepVolumeDb, audio.RiseDepth, audio.FullDepth,
                    audio.ResponsePower), 1f / 60f, audio.FadeSpeed);
            Require(Mathf.IsEqualApprox(audio.CurrentVolumeDb, expectedDeep) &&
                Mathf.IsEqualApprox(audio.AmbiencePlayer.VolumeDb, expectedDeep) &&
                audio.AmbienceStarts == 1, "shallow tuning cannot alter deep response");
            audio.ShallowSurfaceVolumeDb = -20f;
            Require(audio.SwimEmitter.VolumeDb == swimDb &&
                audio.BoostEmitter.VolumeDb == boostDb,
                "depth does not change swim or boost gain");
            CheckGate();
            CheckVariation();
            CheckSurface(encounter.Growth);
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
            var splashPoint = fish.GlobalPosition + new Vector3(0f, .25f, 0f);
            var firstSplash = audio.PlaySurfaceCue(SurfaceSplashEvent.Arrival, splashPoint, .4f)!;
            var firstSplashPitch = firstSplash.PitchScale;
            var firstSplashGain = firstSplash.VolumeDb;
            var firstSplashLength = audio.ScheduledLength(firstSplash);
            var secondSplash = audio.PlaySurfaceCue(SurfaceSplashEvent.Arrival, splashPoint, .8f)!;
            Require(firstSplash != secondSplash && firstSplash.Playing && secondSplash.Playing &&
                firstBoost.Playing && firstSplash.Stream == secondSplash.Stream &&
                firstSplash.Bus == "SFX" && firstSplash.GlobalPosition.DistanceTo(splashPoint) < .001f &&
                firstSplash.PitchScale == firstSplashPitch && firstSplash.VolumeDb == firstSplashGain &&
                audio.ScheduledLength(firstSplash) == firstSplashLength &&
                Mathf.Abs(firstSplash.PitchScale - secondSplash.PitchScale) >= .044f &&
                Mathf.Abs(firstSplash.VolumeDb - secondSplash.VolumeDb) >= .999f,
                "positional splash overlaps boost with independent variation");
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
            var bubbleBeforePause = audio.BubbleStarts;
            for (var i = 0; i < 240 && audio.BubbleStarts == bubbleBeforePause; i++)
                audio.AdvanceBubbleAccents(.25f, true);
            Require(audio.BubbleStarts == bubbleBeforePause + 1,
                "underwater accent scheduled before pause");
            var sceneTree = encounter.GetTree();
            sceneTree.Paused = true;
            try
            {
                Require(!audio.CanProcess() && !audio.ShallowAmbiencePlayer.CanProcess() &&
                    !audio.BubbleAccentPlayer.CanProcess() && !audio.BubbleAccentPlayer.Playing &&
                    audio.AmbienceStarts == 1 && audio.ShallowAmbienceStarts == 1,
                    "scene pause holds beds and clears bubble accent");
            }
            finally { sceneTree.Paused = false; }
            audio.AdvanceBubbleAccents(.25f, true);
            Require(audio.BubbleStarts == bubbleBeforePause + 1,
                "scene resume has no queued bubble burst");
            Require(audio.AmbiencePlayer.Playing && audio.ShallowAmbiencePlayer.Playing &&
                audio.AmbienceStarts == 1 && audio.ShallowAmbienceStarts == 1,
                "ambience resumes without replay");
            audio.Shutdown();
            Require(!audio.AmbiencePlayer.Playing && !audio.ShallowAmbiencePlayer.Playing &&
                !audio.BubbleAccentPlayer.Playing && audio.BubbleAccentPlayer.Stream is null &&
                audio.AmbiencePlayer.Stream is null &&
                audio.ShallowAmbiencePlayer.Stream is null,
                "ambience layers and bubble accent release on scene shutdown");
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
                        GD.Print("AUDIO_CHECK_OK deep=1 shallow=1 shallow_fade=surface-to-5m shallow_loop=on deep_independent=ok bubbles=mono,sparse,underwater,cleanup pause=held retry=one-pair return=no-bed events=motion,turn,boost,splash");
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

    private static void CheckBubbleSchedule()
    {
        var schedule = new BubbleAccentSchedule(7931);
        var previousTime = float.NegativeInfinity;
        var previousTake = -1;
        var seen = new bool[3];
        for (var minute = 0; minute < 24; minute++)
        {
            var count = 0;
            for (var tick = 0; tick < 240; tick++)
            {
                var take = schedule.Step(.25f, true);
                if (take is not { } index) continue;
                var time = minute * 60f + (tick + 1) * .25f;
                Require(index >= 0 && index < 3 && index != previousTake,
                    "bubble take changes on each event");
                Require(time - previousTime >= BubbleAccentSchedule.MinimumSpacingSeconds - .001f,
                    "bubble schedule avoids clusters");
                seen[index] = true;
                previousTake = index;
                previousTime = time;
                count++;
            }
            Require(count is >= 1 and <= 4, "one to four irregular bubble accents per active minute");
        }
        Require(seen.All(value => value), "all three bubble takes selected");
        schedule.Step(60f, false);
        Require(schedule.Step(.25f, true) is null,
            "inactive time cannot queue bubble accents");
    }

    private static void CheckBubbleRuntime(PikeEncounter encounter, WorldAudio audio,
        FishController fish)
    {
        foreach (var path in WorldAudio.PondBubblePaths)
            Require(path.StartsWith("res://assets/sfx/", StringComparison.Ordinal) &&
                ResourceLoader.Load<AudioStreamWav>(path) is { Stereo: false },
                "approved mono bubble derivatives import");
        Require(audio.BubbleAccentPlayer.GetParent() == audio &&
            audio.BubbleAccentPlayer.Bus == "SFX" &&
            audio.BubbleAccentPlayer.MaxDistance == 12f &&
            audio.BubbleAccentPlayer.UnitSize == 2f &&
            audio.BubbleAccentPlayer.MaxDb == -8f &&
            audio.BubbleAccentPlayer.VolumeDb == -8f,
            "bubble accent is a positional environment voice");
        var originalPosition = fish.GlobalPosition;
        var surface = fish.PondSurfaceHeightAt(originalPosition.X, originalPosition.Z);
        Require(WorldAudio.IsFullyUnderwater(originalPosition.Y, surface, fish.BodyRadius) &&
            !WorldAudio.IsFullyUnderwater(surface, surface, fish.BodyRadius) &&
            !WorldAudio.IsFullyUnderwater(surface + 1f, surface, fish.BodyRadius),
            "moving surface and fish body gate bubble playback");
        var seabed = encounter.GetNode<SeabedSurface>("Habitat/Seabed");
        for (var i = 0; i < 24; i++)
        {
            Require(audio.TryChooseBubblePosition(originalPosition, out var point),
                "underwater bubble position available");
            var horizontal = new Vector2(point.X - originalPosition.X,
                point.Z - originalPosition.Z).Length();
            var local = seabed.ToLocal(new Vector3(point.X, 0f, point.Z));
            var floorY = seabed.ToGlobal(new Vector3(local.X,
                seabed.GetHeightAt(local.X, local.Z), local.Z)).Y;
            Require(horizontal >= 2.999f && horizontal <= 9.001f &&
                point.Y >= floorY + .349f &&
                point.Y <= fish.PondSurfaceHeightAt(point.X, point.Z) - .349f,
                "bubble positions stay in the water column at varied distances");
        }
        for (var i = 0; i < 240 && audio.BubbleStarts == 0; i++)
            audio.AdvanceBubbleAccents(.25f, true);
        Require(audio.BubbleStarts == 1 && audio.BubbleAccentPlayer.Playing &&
            audio.BubbleAccentPlayer.Stream is AudioStreamWav { Stereo: false } &&
            WorldAudio.PondBubblePaths.Contains(audio.BubbleAccentPlayer.Stream.ResourcePath),
            "scheduled mono bubble accent plays once on SFX");
        fish.GlobalPosition = new Vector3(originalPosition.X, surface + 1f,
            originalPosition.Z);
        try
        {
            audio.AdvanceBubbleAccents(.25f, true);
            Require(!audio.BubbleAccentPlayer.Playing, "surface gate stops bubble playback");
        }
        finally { fish.GlobalPosition = originalPosition; }
        audio.AdvanceBubbleAccents(.25f, true);
        Require(audio.BubbleStarts == 1, "underwater return cannot release queued accents");
        for (var i = 0; i < 240 && audio.BubbleStarts == 1; i++)
            audio.AdvanceBubbleAccents(.25f, true);
        Require(audio.BubbleStarts == 2, "underwater accent restarts after fresh spacing");
        audio._Notification((int)Node.NotificationApplicationFocusOut);
        Require(!audio.BubbleAccentPlayer.Playing &&
            audio.AmbiencePlayer.StreamPaused && audio.ShallowAmbiencePlayer.StreamPaused,
            "focus loss stops accent and pauses beds");
        audio._Notification((int)Node.NotificationApplicationFocusIn);
        audio.AdvanceBubbleAccents(.25f, true);
        Require(audio.BubbleStarts == 2 && !audio.BubbleAccentPlayer.Playing,
            "focus return cannot release queued accents");
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
        var profile = new SfxVariationProfile(1f, .86f, 1.14f, .045f,
            -20f, -2.5f, 2.5f, 1f, .68f, .98f, .07f,
            .74f, .98f, 3f);
        var splash = new SfxVariationState(17);
        var independent = new SfxVariationState(17);
        var swim = new SfxVariationState(99);
        SfxVariation? priorSplash = null;
        for (var i = 0; i < 100; i++)
        {
            var cue = splash.Next(profile, .5f);
            swim.Next(profile);
            var untouched = independent.Next(profile, .5f);
            Require(cue == untouched && cue.Pitch is >= .86f and <= 1.14f &&
                cue.VolumeDb is >= -21f and <= -16f &&
                cue.DurationFraction is >= .71f and <= .98f &&
                Mathf.Abs(cue.PlayedSeconds(1.335f) -
                    1.335f * cue.DurationFraction / cue.Pitch) < .00001f,
                "seeded variation is bounded, independent and pitch-scaled");
            if (priorSplash is { } last)
                Require(Mathf.Abs(cue.Pitch - last.Pitch) >= .0449f &&
                    Mathf.Abs(cue.DurationFraction - last.DurationFraction) >= .0699f &&
                    Mathf.Abs(cue.VolumeDb - last.VolumeDb) >= .999f,
                    "adjacent splash choices differ");
            priorSplash = cue;
        }
    }

    private static void CheckSurface(PondGrowthState growth)
    {
        const float tick = .05f;
        var gate = new SurfaceSplashDetector();
        SurfaceSplashEvent Step(float bodyY, float surfaceY = 0f, bool eligible = true,
            float speed = 1f) => gate.Step(bodyY, surfaceY, .1f, speed,
                tick, eligible, out _);
        Require(Step(-.30f) == SurfaceSplashEvent.None, "initial submerged sample silent");
        for (var i = 0; i < 3; i++) Step(-.30f);
        Require(Step(-.06f) == SurfaceSplashEvent.Arrival,
            "first upward body contact splashes once");
        for (var i = 0; i < 8; i++)
            Require(Step(-.06f) == SurfaceSplashEvent.None,
                "sustained contact stays quiet");
        Require(Step(-.11f) == SurfaceSplashEvent.RapidDive,
            "measured fast descent after contact splashes once");
        for (var i = 0; i < 8; i++)
            Require(Step(-.11f - i * .06f) == SurfaceSplashEvent.None,
                "one dive has one event");
        Require(Step(-.06f) == SurfaceSplashEvent.Arrival,
            "meaningful submersion rearms arrival");
        Require(Step(-.06f, eligible: false) == SurfaceSplashEvent.None &&
            Step(-.06f) == SurfaceSplashEvent.None,
            "pause or hatch reset has no ghost event");

        gate.Reset();
        Step(-.30f);
        for (var i = 0; i < 3; i++) Step(-.30f);
        for (var i = 0; i < 8; i++)
            Require(Step(-.06f, surfaceY: i % 2 == 0 ? .025f : -.025f) ==
                SurfaceSplashEvent.None, "waves alone do not trigger arrival");
        gate.Reset();
        Step(-.30f);
        for (var i = 0; i < 3; i++) Step(-.30f);
        Step(-.06f);
        for (var i = 0; i < 8; i++) Step(-.06f);
        for (var i = 1; i <= 8; i++)
            Require(Step(-.06f - i * .01f) == SurfaceSplashEvent.None,
                "gentle descent remains quiet");
        for (var i = 0; i < 4; i++)
            Require(Step(-.14f) == SurfaceSplashEvent.None,
                "boost without vertical movement is silent");
        foreach (var stage in new[] { "larva", "juvenile", "subadult", "adult" })
        {
            var radius = growth.BodyFor(stage).Radius;
            Require(float.IsFinite(radius) && radius > 0f &&
                gate.ContactRadiusFraction * radius <
                    Mathf.Max(gate.MinimumRearmDepth, gate.RearmRadiusFraction * radius),
                "all Bass stages have usable contact and rearm distances");
            GD.Print($"SURFACE_SPLASH_STAGE stage={stage} radius={radius:F4} contact={gate.ContactRadiusFraction * radius:F4} rearm={Mathf.Max(gate.MinimumRearmDepth, gate.RearmRadiusFraction * radius):F4}");
        }
    }
}
