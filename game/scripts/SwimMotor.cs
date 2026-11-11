using Godot;

namespace Fish;

public readonly record struct SwimIntent(float Thrust, float Slow, float Yaw, float Pitch);

public struct SwimState
{
    public float Speed;
    public float Yaw;
    public float Pitch;
    public Vector3 Velocity;
    public bool Reversing;
    public bool WaitingToReverse;
    public float ReverseTransitionWaitSeconds;
}

/// <summary>Physics-time movement math shared by keyboard and the future mouse adapter.</summary>
public static class SwimMotor
{
    private const float ReverseTransitionPauseSeconds = 0.25f;

    public static SwimState Step(SwimState state, SwimIntent intent, float delta, SwimTuning tuning)
    {
        if (delta <= 0) return state;
        var yaw = Mathf.Clamp(intent.Yaw, -1, 1);
        var pitch = Mathf.Clamp(intent.Pitch, -1, 1);
        state.Yaw = Mathf.Wrap(state.Yaw + yaw * Mathf.DegToRad(tuning.YawRateDegrees) * delta,
            -Mathf.Pi, Mathf.Pi);
        state.Pitch = Mathf.Clamp(state.Pitch + pitch * Mathf.DegToRad(tuning.PitchRateDegrees) * delta,
            -Mathf.DegToRad(tuning.PitchLimitDegrees), Mathf.DegToRad(tuning.PitchLimitDegrees));

        // Only a forward-to-reverse change pauses at rest. Slow from a stationary
        // start reverses immediately; forward thrust brakes reverse motion first.
        if (intent.Slow > 0)
        {
            var slow = Mathf.Clamp(intent.Slow, 0, 1);
            if (state.Reversing)
            {
                state.Speed = Mathf.MoveToward(state.Speed, tuning.ReverseMaxSpeed,
                    tuning.ReverseAcceleration * slow * delta);
            }
            else if (state.Speed > 0)
            {
                state.Speed = Mathf.MoveToward(state.Speed, 0,
                    tuning.BrakeDeceleration * slow * delta);
                state.WaitingToReverse = true;
                state.ReverseTransitionWaitSeconds = 0;
            }
            else if (state.WaitingToReverse)
            {
                state.ReverseTransitionWaitSeconds += delta;
                if (state.ReverseTransitionWaitSeconds > ReverseTransitionPauseSeconds)
                {
                    state.WaitingToReverse = false;
                    state.Reversing = true;
                    state.Speed = Mathf.Min(tuning.ReverseMaxSpeed,
                        tuning.ReverseAcceleration * slow *
                        (state.ReverseTransitionWaitSeconds - ReverseTransitionPauseSeconds));
                    state.ReverseTransitionWaitSeconds = 0;
                }
            }
            else
            {
                state.Reversing = true;
                state.Speed = Mathf.MoveToward(0, tuning.ReverseMaxSpeed,
                    tuning.ReverseAcceleration * slow * delta);
            }
        }
        else
        {
            state.WaitingToReverse = false;
            state.ReverseTransitionWaitSeconds = 0;
            if (state.Reversing)
            {
                var deceleration = intent.Thrust > 0 ? tuning.BrakeDeceleration
                    : tuning.CoastDeceleration;
                state.Speed = Mathf.MoveToward(state.Speed, 0, deceleration * delta);
                if (state.Speed <= 0) state.Reversing = false;
            }
            else
            {
                var rate = intent.Thrust > 0 ? tuning.Acceleration * Mathf.Clamp(intent.Thrust, 0, 1)
                    : tuning.CoastDeceleration;
                var targetSpeed = intent.Thrust > 0 ? tuning.MaxSpeed : 0;
                state.Speed = Mathf.MoveToward(state.Speed, targetSpeed, rate * delta);
            }
        }
        state.Speed = Mathf.Clamp(state.Speed, 0, state.Reversing ? tuning.ReverseMaxSpeed : tuning.MaxSpeed);

        var forward = Forward(state.Yaw, state.Pitch) * (state.Reversing ? -1 : 1);
        var direction = state.Velocity.LengthSquared() > 0.000001f
            ? state.Velocity.Normalized().Slerp(forward,
                1 - Mathf.Exp(-tuning.VelocityAlignment * delta)).Normalized()
            : forward;
        state.Velocity = direction * state.Speed;
        return state;
    }

    public static Vector3 Forward(float yaw, float pitch) => new(
        Mathf.Sin(yaw) * Mathf.Cos(pitch), Mathf.Sin(pitch), -Mathf.Cos(yaw) * Mathf.Cos(pitch));
}
