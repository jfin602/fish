using Godot;

namespace Fish;

/// <summary>Initial movement and camera defaults in metres and seconds; P10 owns feel tuning.</summary>
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
    [Export] public float CameraDistance { get; set; } = 4.2f;
    [Export] public float CameraHeight { get; set; } = 1.1f;
    [Export] public float CameraLookAhead { get; set; } = 1.5f;
    [Export] public float CameraFieldOfView { get; set; } = 68.0f;
    [Export] public float CameraFollowResponse { get; set; } = 7.0f;
    [Export] public float CameraRecoveryResponse { get; set; } = 3.0f;
}
