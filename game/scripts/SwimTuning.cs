using Godot;

namespace Fish;

/// <summary>Initial P6 movement defaults in metres, seconds and radians; P10 owns feel tuning.</summary>
[GlobalClass]
public partial class SwimTuning : Resource
{
    [Export] public float MaxSpeed { get; set; } = 4.0f;
    [Export] public float Acceleration { get; set; } = 12.0f;
    [Export] public float CoastDeceleration { get; set; } = 1.6f;
    [Export] public float BrakeDeceleration { get; set; } = 9.0f;
    [Export] public float YawRateDegrees { get; set; } = 150.0f;
    [Export] public float PitchRateDegrees { get; set; } = 95.0f;
    [Export] public float PitchLimitDegrees { get; set; } = 55.0f;
    [Export] public float VelocityAlignment { get; set; } = 4.5f;
}
