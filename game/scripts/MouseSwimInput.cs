using Godot;

namespace Fish;

/// <summary>Bounded captured motion, consumed once by the profile resolver per physics tick.</summary>
public sealed class MouseSwimInput
{
    private const float MaximumEventPixels = 300f;
    private const float MaximumQueuedPixels = 80f;
    private Vector2 _pendingPixels;
    private bool _ignoreNextMotion = true;

    public void Clear()
    {
        _pendingPixels = Vector2.Zero;
        _ignoreNextMotion = true;
    }

    public void OnCaptureStarted() => Clear();

    public void Accept(InputEvent @event)
    {
        if (@event is not InputEventMouseMotion motion) return;
        if (_ignoreNextMotion)
        {
            _ignoreNextMotion = false;
            return;
        }
        if (motion.Relative.Length() > MaximumEventPixels) return;
        _pendingPixels = new Vector2(
            Mathf.Clamp(_pendingPixels.X + motion.Relative.X, -MaximumQueuedPixels, MaximumQueuedPixels),
            Mathf.Clamp(_pendingPixels.Y + motion.Relative.Y, -MaximumQueuedPixels, MaximumQueuedPixels));
    }

    public Vector2 ConsumeSteering(float delta, SwimTuning tuning, ControlPreset profile,
        float sensitivity, bool invertY)
    {
        var pixels = _pendingPixels;
        _pendingPixels = Vector2.Zero;
        if (delta <= 0) return Vector2.Zero;
        var yaw = 0f;
        var pitch = 0f;
        AddAxis(profile.MouseX, pixels.X, profile.MouseX.Inverted, false);
        AddAxis(profile.MouseY, pixels.Y, invertY, true);
        return new Vector2(yaw, pitch);

        void AddAxis(MouseAxisAssignment assignment, float pixelsOnAxis, bool inverted, bool vertical)
        {
            var degrees = pixelsOnAxis * sensitivity * ((vertical ? -1 : 1) * (inverted ? -1 : 1));
            if (assignment.Steering == SteeringAxis.Yaw)
                yaw += degrees / (Mathf.Max(0.001f, tuning.YawRateDegrees) * delta);
            else if (assignment.Steering == SteeringAxis.Pitch)
                pitch += degrees / (Mathf.Max(0.001f, tuning.PitchRateDegrees) * delta);
        }
    }
}
