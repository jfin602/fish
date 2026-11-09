using Godot;
using System;

namespace Fish;

/// <summary>Bounded, deterministic P6 assertions run by the existing C# scene smoke.</summary>
public static class MotorChecks
{
    public static void Run(SwimTuning tuning)
    {
        const float dt = 1f / 60f;
        var state = new SwimState();
        for (var i = 0; i < 15; i++) state = SwimMotor.Step(state, new SwimIntent(1, 0, 0, 0), dt, tuning);
        Require(state.Speed > 2.9f && state.Speed < tuning.MaxSpeed, "acceleration response");
        for (var i = 0; i < 15; i++) state = SwimMotor.Step(state, new SwimIntent(1, 0, 0, 0), dt, tuning);
        Require(Mathf.IsEqualApprox(state.Speed, tuning.MaxSpeed), "speed cap");

        var atRelease = state.Speed;
        for (var i = 0; i < 30; i++)
        {
            var previous = state.Speed;
            state = SwimMotor.Step(state, default, dt, tuning);
            Require(state.Speed <= previous && state.Speed >= 0, "monotonic coast");
        }
        Require(state.Speed > 2.9f && state.Speed < atRelease, "visible coast interval");
        var coastSpeed = state.Speed;
        for (var i = 0; i < 30; i++) state = SwimMotor.Step(state, new SwimIntent(0, 1, 0, 0), dt, tuning);
        Require(state.Speed == 0 && state.Velocity.Length() == 0 && coastSpeed > state.Speed,
            "keyboard braking without reversal");
        for (var i = 0; i < 30; i++) state = SwimMotor.Step(state, new SwimIntent(1, 1, 0, 0), dt, tuning);
        Require(state.Speed == 0, "braking priority over simultaneous thrust");

        state = new SwimState();
        for (var i = 0; i < 30; i++) state = SwimMotor.Step(state, new SwimIntent(1, 0, 0, 0), dt, tuning);
        for (var i = 0; i < 30; i++) state = SwimMotor.Step(state, new SwimIntent(1, 0, 1, 0), dt, tuning);
        Require(state.Yaw > 1.2f && state.Velocity.X > 0.3f && state.Velocity.Z < 0,
            "curved right turn");
        Require(state.Velocity.Normalized().Dot(SwimMotor.Forward(state.Yaw, state.Pitch)) < 0.999f,
            "velocity lags heading through a turn");

        state = new SwimState();
        for (var i = 0; i < 25; i++) state = SwimMotor.Step(state, new SwimIntent(1, 0, 0, 1), dt, tuning);
        Require(state.Pitch > 0.5f && state.Velocity.Y > 0.3f,
            "nose-up travel follows heading");
        for (var i = 0; i < 180; i++) state = SwimMotor.Step(state, new SwimIntent(1, 0, 0, 1), dt, tuning);
        Require(Mathf.IsEqualApprox(state.Pitch, Mathf.DegToRad(tuning.PitchLimitDegrees)),
            "pitch upper limit");
        var heldPitch = state.Pitch;
        for (var i = 0; i < 30; i++) state = SwimMotor.Step(state, default, dt, tuning);
        Require(Mathf.IsEqualApprox(state.Pitch, heldPitch), "no automatic leveling");
        for (var i = 0; i < 180; i++) state = SwimMotor.Step(state, new SwimIntent(0, 0, 0, -1), dt, tuning);
        Require(Mathf.IsEqualApprox(state.Pitch, -heldPitch), "pitch lower limit and near-rest steering");
        state = SwimMotor.Step(new SwimState(), new SwimIntent(0, 0, 1, 0), dt, tuning);
        Require(state.Yaw > 0 && state.Speed == 0, "near-rest yaw");

        var shortSteps = Sequence(tuning, 1f / 120f, 120);
        var longSteps = Sequence(tuning, 1f / 30f, 30);
        Require(shortSteps.Position.DistanceTo(longSteps.Position) < 0.12f &&
                Mathf.Abs(shortSteps.State.Speed - longSteps.State.Speed) < 0.02f &&
                Mathf.Abs(shortSteps.State.Yaw - longSteps.State.Yaw) < 0.001f,
            "one-second intent sequence is stable across physics step schedules");
        GD.Print($"FISH_MOTOR_CHECK_OK coast_half_second={atRelease - coastSpeed:F2} " +
            $"step_position_difference={shortSteps.Position.DistanceTo(longSteps.Position):F3}");
    }

    private static (SwimState State, Vector3 Position) Sequence(SwimTuning tuning, float dt, int count)
    {
        var state = new SwimState();
        var position = Vector3.Zero;
        for (var i = 0; i < count; i++)
        {
            var time = i * dt;
            var intent = time < 0.5f ? new SwimIntent(1, 0, 0, 0)
                : new SwimIntent(1, 0, 1, 0);
            state = SwimMotor.Step(state, intent, dt, tuning);
            position += state.Velocity * dt;
            Require(float.IsFinite(state.Speed) && float.IsFinite(state.Velocity.X) &&
                    float.IsFinite(state.Velocity.Y) && float.IsFinite(state.Velocity.Z),
                "finite movement state");
        }
        return (state, position);
    }

    private static void Require(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException(label);
    }
}
