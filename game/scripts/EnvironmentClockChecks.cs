#nullable enable
using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fish;

public static class EnvironmentClockChecks
{
    public static async Task Run(Main main)
    {
        try
        {
            var clock = main.GetNode<EnvironmentClock>("EnvironmentClock");
            var debug = OS.IsDebugBuild() && OS.GetCmdlineUserArgs().Contains("--environment-time-debug");
            Require(Math.Abs(clock.HourOfDay - (debug ? 6 : 12)) < 1e-9,
                "startup time hook was ignored or leaked into normal play");
            Require(clock.ProcessMode == Node.ProcessModeEnum.Inherit && clock.CanProcess(),
                "clock must inherit gameplay processing");
            Require(Math.Abs(EnvironmentClock.WrapFraction(-0.25) - 0.75) < 1e-12 &&
                Math.Abs(EnvironmentClock.WrapFraction(1.25) - 0.25) < 1e-12,
                "fraction wraparound");
            for (var tick = 0; tick <= 240; tick++)
            {
                var state = SolarState.At(tick / 240.0);
                Require(state.DayFraction >= 0 && state.DayFraction < 1 &&
                    state.HourOfDay >= 0 && state.HourOfDay < 24 &&
                    state.DirectionToSun.IsFinite() &&
                    Math.Abs(state.DirectionToSun.Length() - 1) < .0001f &&
                    float.IsFinite(state.ElevationRadians) &&
                    InUnit(state.Daylight) && InUnit(state.Twilight) && InUnit(state.Night) &&
                    InUnit(state.DirectLight) && InUnit(state.AmbientLight),
                    "solar state must remain finite and normalized");
            }
            var dawn = SolarState.At(6.0 / 24);
            var noon = SolarState.At(12.0 / 24);
            var sunset = SolarState.At(18.0 / 24);
            var midnight = SolarState.At(0);
            Require(Math.Abs(dawn.ElevationRadians) < .001f &&
                noon.ElevationRadians > 1.56f &&
                Math.Abs(sunset.ElevationRadians) < .001f &&
                midnight.ElevationRadians < -1.56f &&
                noon.Daylight > .99f && midnight.Daylight < .001f &&
                midnight.Night > .99f && dawn.Twilight > .1f && sunset.Twilight > .1f &&
                dawn.DirectionToSun.X > 0 && sunset.DirectionToSun.X < 0,
                "representative solar samples");
            var pondShoreView = new Vector3(-0.887818f, 0f, 0.460194f);
            Require(sunset.DirectionToSun.Dot(pondShoreView) > .9999f &&
                (-dawn.DirectionToSun).Dot(pondShoreView) > .9999f,
                "sunset and moonset must line up with the Pond shore view");
            var beforeBoundary = SolarState.At(1 - 1e-7);
            var afterBoundary = SolarState.At(1e-7);
            Require(beforeBoundary.DirectionToSun.DistanceTo(afterBoundary.DirectionToSun) < .00001f &&
                Math.Abs(beforeBoundary.Daylight - afterBoundary.Daylight) < .00001f &&
                Math.Abs(beforeBoundary.Night - afterBoundary.Night) < .00001f,
                "solar mapping discontinuity at midnight");
            foreach (var anchor in new[] { 6.0 / 24, 18.0 / 24 })
            {
                var left = SolarState.At(anchor - 1e-7);
                var right = SolarState.At(anchor + 1e-7);
                Require(left.DirectionToSun.DistanceTo(right.DirectionToSun) < .00001f &&
                    Math.Abs(left.Daylight - right.Daylight) < .00001f &&
                    Math.Abs(left.Twilight - right.Twilight) < .00001f,
                    "solar mapping discontinuity at dawn or sunset");
            }
            RequireThrows<ArgumentOutOfRangeException>(() => clock.FullDayDurationSeconds = 0);
            RequireThrows<ArgumentOutOfRangeException>(() => clock.FullDayDurationSeconds = double.NaN);
            clock.FullDayDurationSeconds = 120;
            var start = clock.DayFraction;
            clock._Process(120);
            Require(CircularDistance(clock.DayFraction, start) < 1e-9,
                "full-day advance must wrap");

#if DEBUG
            if (debug)
            {
                clock._Process(6);
                Require(Math.Abs(clock.HourOfDay - 18) < 1e-8,
                    "startup acceleration hook");
                clock.SetTimeForDebug(23.5);
                Require(Math.Abs(clock.HourOfDay - 23.5) < 1e-9, "debug set-time");
                clock.SetAccelerationForDebug(10);
                RequireThrows<ArgumentOutOfRangeException>(() => clock.SetAccelerationForDebug(121));
                RequireThrows<ArgumentOutOfRangeException>(() => clock.SetTimeForDebug(double.NaN));
                clock._Process(6);
                Require(Math.Abs(clock.HourOfDay - 11.5) < 1e-8, "bounded debug acceleration");
            }
            else
            {
                RequireThrows<InvalidOperationException>(() => clock.SetTimeForDebug(6));
                RequireThrows<InvalidOperationException>(() => clock.SetAccelerationForDebug(10));
            }
#endif

            main.GetTree().Paused = true;
            var pausedAt = clock.DayFraction;
            clock._Process(30);
            for (var frame = 0; frame < 3; frame++)
                await main.ToSignal(main.GetTree(), SceneTree.SignalName.ProcessFrame);
            Require(clock.DayFraction == pausedAt && !clock.CanProcess(),
                "scene-tree pause did not freeze time");
            main.GetTree().Paused = false;
            Require(clock.CanProcess(), "clock did not resume with gameplay");
            var resumeAt = clock.DayFraction;
            for (var frame = 0; frame < 3; frame++)
                await main.ToSignal(main.GetTree(), SceneTree.SignalName.ProcessFrame);
            Require(CircularDistance(clock.DayFraction, resumeAt) > 0,
                "resumed scene-tree processing did not advance time");
            resumeAt = clock.DayFraction;
            clock._Process(1);
            Require(CircularDistance(clock.DayFraction, resumeAt) > .0001,
                "resumed gameplay did not advance time");
            GD.Print($"ENVIRONMENT_TIME_CHECK_OK debug={OS.GetCmdlineUserArgs().Contains("--environment-time-debug")} samples=241 pause=verified");
            main.GetTree().Quit();
        }
        catch (Exception error)
        {
            GD.PushError($"ENVIRONMENT_TIME_CHECK_FAILED {error}");
            main.GetTree().Quit(1);
        }
        finally { main.GetTree().Paused = false; }
    }

    private static bool InUnit(float value) => float.IsFinite(value) && value >= 0 && value <= 1;
    private static double CircularDistance(double a, double b) =>
        Math.Min(Math.Abs(a - b), 1 - Math.Abs(a - b));
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
    private static void RequireThrows<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new Exception($"Expected {typeof(T).Name}");
    }
}
