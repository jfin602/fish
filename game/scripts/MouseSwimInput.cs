using Godot;

namespace Fish;

/// <summary>Bounded captured motion, consumed once by the profile resolver per physics tick.</summary>
public sealed class MouseSwimInput
{
    private const float MaximumEventPixels = 300f;
    private const float MaximumQueuedPixels = 80f;
    private Vector2 _pendingPixels;
    private Vector2 _adaptiveNoisePixels;
    private Vector2 _adaptiveRemainingPixels;
    private bool _ignoreNextMotion = true;

    public void Clear()
    {
        _pendingPixels = Vector2.Zero;
        _adaptiveNoisePixels = Vector2.Zero;
        _adaptiveRemainingPixels = Vector2.Zero;
        _ignoreNextMotion = true;
    }

    public void DiscardAdaptiveSteering()
    {
        _pendingPixels = Vector2.Zero;
        _adaptiveNoisePixels = Vector2.Zero;
        _adaptiveRemainingPixels = Vector2.Zero;
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
        var pixels = ConsumePixels();
        if (profile.CameraControl == CameraControlPolicy.AdaptiveFollow)
            pixels = FilterAdaptivePixels(pixels, delta);
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

    private Vector2 FilterAdaptivePixels(Vector2 pixels, float delta)
    {
        // Accumulate sub-threshold movement so slow deliberate steering is retained.
        _adaptiveNoisePixels += pixels;
        var admitted = new Vector2(Admit(ref _adaptiveNoisePixels.X), Admit(ref _adaptiveNoisePixels.Y));
        _adaptiveRemainingPixels += admitted;
        var alpha = 1f - Mathf.Exp(-Mathf.Max(0f, delta) / 0.12f);
        if (admitted != Vector2.Zero) alpha = Mathf.Max(alpha, 0.55f);
        if (admitted.Length() >= 12f) alpha = Mathf.Max(alpha, 0.85f);
        // Spend queued displacement once; the short tail cannot amplify a turn.
        var output = _adaptiveRemainingPixels * alpha;
        _adaptiveRemainingPixels -= output;
        if (_adaptiveRemainingPixels.LengthSquared() < 0.01f) _adaptiveRemainingPixels = Vector2.Zero;
        return output;

        static float Admit(ref float accumulated)
        {
            if (Mathf.Abs(accumulated) < 2f) return 0f;
            var result = accumulated;
            accumulated = 0f;
            return result;
        }
    }

    public Vector2 ConsumePixels()
    {
        var pixels = _pendingPixels;
        _pendingPixels = Vector2.Zero;
        return pixels;
    }
}
