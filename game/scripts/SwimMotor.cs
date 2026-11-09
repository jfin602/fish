using Godot;

namespace Fish;

public readonly record struct SwimIntent(float Thrust, float Slow, float Yaw, float Pitch);

public struct SwimState
{
    public float Speed;
    public float Yaw;
    public float Pitch;
    public Vector3 Velocity;
}

/// <summary>Physics-time movement math shared by keyboard and the future mouse adapter.</summary>
public static class SwimMotor
{
    public static SwimState Step(SwimState state, SwimIntent intent, float delta, SwimTuning tuning)
    {
        if (delta <= 0) return state;
        var yaw = Mathf.Clamp(intent.Yaw, -1, 1);
        var pitch = Mathf.Clamp(intent.Pitch, -1, 1);
        state.Yaw = Mathf.Wrap(state.Yaw + yaw * Mathf.DegToRad(tuning.YawRateDegrees) * delta,
            -Mathf.Pi, Mathf.Pi);
        state.Pitch = Mathf.Clamp(state.Pitch + pitch * Mathf.DegToRad(tuning.PitchRateDegrees) * delta,
            -Mathf.DegToRad(tuning.PitchLimitDegrees), Mathf.DegToRad(tuning.PitchLimitDegrees));

        // Slowing wins over thrust. Neither command can request reverse speed.
        var rate = intent.Slow > 0 ? tuning.BrakeDeceleration * Mathf.Clamp(intent.Slow, 0, 1)
            : intent.Thrust > 0 ? tuning.Acceleration * Mathf.Clamp(intent.Thrust, 0, 1)
            : tuning.CoastDeceleration;
        var targetSpeed = intent.Slow > 0 || intent.Thrust <= 0 ? 0 : tuning.MaxSpeed;
        state.Speed = Mathf.Clamp(Mathf.MoveToward(state.Speed, targetSpeed, rate * delta), 0, tuning.MaxSpeed);

        var forward = Forward(state.Yaw, state.Pitch);
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
