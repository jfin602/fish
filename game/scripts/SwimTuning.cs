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
    [Export] public float ReverseMaxSpeed { get; set; } = 1.5f;
    [Export] public float ReverseAcceleration { get; set; } = 5.0f;
    [Export] public float YawRateDegrees { get; set; } = 150.0f;
    [Export] public float PitchRateDegrees { get; set; } = 95.0f;
    [Export] public float PitchLimitDegrees { get; set; } = 55.0f;
    [Export] public float VelocityAlignment { get; set; } = 4.5f;
    [Export] public float CameraDistance { get; set; } = 3.8f;
    [Export] public float CameraShoulderOffset { get; set; } = 0.0f;
    [Export] public float CameraHeight { get; set; } = 2.0f;
    [Export] public float CameraLookAhead { get; set; } = 0.8f;
    [Export] public float CameraFieldOfView { get; set; } = 68.0f;
    [Export] public float CameraFollowResponse { get; set; } = 4.5f;
    [Export] public float CameraRecoveryResponse { get; set; } = 3.0f;
    [Export] public float CameraPanYawRateDegrees { get; set; } = 90.0f;
    [Export] public float CameraPanPitchRateDegrees { get; set; } = 65.0f;
    [Export] public float CameraPanYawLimitDegrees { get; set; } = 85.0f;
    [Export] public float CameraPanPitchLimitDegrees { get; set; } = 35.0f;
    [Export] public float CameraPanReturnResponse { get; set; } = 5.0f;
    [Export] public float CameraZoomMinimumDistance { get; set; } = 2.2f;
    [Export] public float CameraZoomMaximumDistance { get; set; } = 6.5f;
    [Export] public float CameraZoomStep { get; set; } = 0.4f;
    [Export] public float SwimCadenceHz { get; set; } = 1.15f;
    [Export] public float GlideCadenceHz { get; set; } = 0.35f;
    [Export] public float SwimIntensity { get; set; } = 1.0f;
    [Export] public float GlideIntensity { get; set; } = 0.12f;
    [Export] public float SwimActivityResponse { get; set; } = 7.0f;
    [Export] public float GlideEasingResponse { get; set; } = 3.0f;
    [Export] public float TurnBendDegrees { get; set; } = 8.0f;
    [Export] public float TurnLeanDegrees { get; set; } = 6.0f;
    [Export] public float TurnResponse { get; set; } = 8.0f;
}
