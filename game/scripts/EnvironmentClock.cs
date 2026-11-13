#nullable enable
using Godot;
using System;
using System.Globalization;
using System.Linq;

namespace Fish;

/// <summary>Renderer-independent time of day and solar values for the active game scene.</summary>
public partial class EnvironmentClock : Node
{
    public const double MinimumDaySeconds = 60;
    public const double MaximumDaySeconds = 7200;
    public const double MaximumDebugAcceleration = 120;

    private double _daySeconds = 900;
    private double _dayFraction = 0.5;
    private double _acceleration = 1;

    public EnvironmentClock()
    {
        // Consumers observe the clock's current-frame value.
        ProcessPriority = -1;
    }

    [Export(PropertyHint.Range, "60,7200,1")]
    public double FullDayDurationSeconds
    {
        get => _daySeconds;
        set
        {
            if (!double.IsFinite(value) || value < MinimumDaySeconds || value > MaximumDaySeconds)
                throw new ArgumentOutOfRangeException(nameof(value));
            _daySeconds = value;
        }
    }

    public double DayFraction => _dayFraction;
    public double HourOfDay => _dayFraction * 24;
    public double GetHourOfDay() => HourOfDay;
    public SolarState Solar => SolarState.At(_dayFraction);

    /// <summary>Adjust presentation time without advancing the game simulation.</summary>
    public void ScrubMinutes(double minutes)
    {
        if (!double.IsFinite(minutes)) throw new ArgumentOutOfRangeException(nameof(minutes));
        _dayFraction = WrapFraction(_dayFraction + minutes / (24 * 60));
    }

    public override void _Ready()
    {
#if DEBUG
        if (!DebugEnabled) return;
        foreach (var argument in OS.GetCmdlineUserArgs())
        {
            if (argument.StartsWith("--environment-hour=", StringComparison.Ordinal))
                SetTimeForDebug(double.Parse(argument["--environment-hour=".Length..], CultureInfo.InvariantCulture));
            if (argument.StartsWith("--environment-speed=", StringComparison.Ordinal))
                SetAccelerationForDebug(double.Parse(argument["--environment-speed=".Length..], CultureInfo.InvariantCulture));
        }
#endif
    }

    public override void _Process(double delta)
    {
        if (GetTree().Paused || !double.IsFinite(delta) || delta <= 0) return;
        _dayFraction = WrapFraction(_dayFraction + delta * _acceleration / _daySeconds);
    }

    public static double WrapFraction(double fraction)
    {
        if (!double.IsFinite(fraction)) throw new ArgumentOutOfRangeException(nameof(fraction));
        var wrapped = fraction % 1;
        return wrapped < 0 ? wrapped + 1 : wrapped;
    }

#if DEBUG
    private static bool DebugEnabled => OS.IsDebugBuild() &&
        OS.GetCmdlineUserArgs().Contains("--environment-time-debug");

    public void SetTimeForDebug(double hour)
    {
        if (!DebugEnabled) throw new InvalidOperationException("Environment time debugging is disabled");
        if (!double.IsFinite(hour)) throw new ArgumentOutOfRangeException(nameof(hour));
        _dayFraction = WrapFraction(hour / 24);
    }

    public void SetAccelerationForDebug(double acceleration)
    {
        if (!DebugEnabled) throw new InvalidOperationException("Environment time debugging is disabled");
        if (!double.IsFinite(acceleration) || acceleration < 1 || acceleration > MaximumDebugAcceleration)
            throw new ArgumentOutOfRangeException(nameof(acceleration));
        _acceleration = acceleration;
    }
#endif
}

/// <summary>Continuous, unit-bounded solar values; direction points from the world toward the sun.</summary>
public readonly record struct SolarState(double DayFraction, double HourOfDay,
    Vector3 DirectionToSun, float ElevationRadians, float Daylight, float Twilight, float Night,
    float DirectLight, float AmbientLight)
{
    public static SolarState At(double fraction)
    {
        var time = EnvironmentClock.WrapFraction(fraction);
        var phase = (time - 0.25) * Math.Tau;
        var elevation = (float)Math.Sin(phase);
        var horizontal = (float)Math.Cos(phase);
        // The west horizon follows the Pond bank approach, a fixed world direction.
        // The opposite night direction lets the moon set along that same shore view at dawn.
        var direction = new Vector3(horizontal * 0.887818f, elevation,
            horizontal * -0.460194f).Normalized();
        var daylight = Smooth(-0.10f, 0.20f, elevation);
        var twilight = Smooth(-0.25f, -0.02f, elevation) * (1f - Smooth(0.02f, 0.30f, elevation));
        var night = 1f - Smooth(-0.20f, -0.04f, elevation);
        return new SolarState(time, time * 24, direction, (float)Math.Asin(elevation),
            daylight, twilight, night, daylight * Mathf.Max(0f, elevation),
            0.08f + 0.92f * daylight);
    }

    private static float Smooth(float edge0, float edge1, float value)
    {
        var x = Mathf.Clamp((value - edge0) / (edge1 - edge0), 0f, 1f);
        return x * x * (3f - 2f * x);
    }
}
