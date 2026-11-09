using Godot;

namespace Fish;

/// <summary>Captured mouse events become one bounded intent per physics tick.</summary>
public sealed class MouseSwimInput
{
    private const float MaximumEventPixels = 300f;
    private const float MaximumQueuedPixels = 80f;
    private Vector2 _pendingPixels;
    private bool _thrustHeld;
    private bool _slowHeld;
    private bool _ignoreNextMotion = true;

    public void Clear()
    {
        _pendingPixels = Vector2.Zero;
        _thrustHeld = false;
        _slowHeld = false;
        _ignoreNextMotion = true;
    }

    public void OnCaptureStarted()
    {
        Clear();
    }

    public void Accept(InputEvent @event)
    {
        if (@event is InputEventMouseButton button && button.ButtonIndex == MouseButton.Left)
        {
            _thrustHeld = button.Pressed;
            return;
        }
        if (@event is InputEventMouseButton brake && brake.ButtonIndex == MouseButton.Right)
        {
            _slowHeld = brake.Pressed;
            return;
        }
        if (@event is not InputEventMouseMotion motion) return;
        if (_ignoreNextMotion)
        {
            _ignoreNextMotion = false;
            return;
        }
        // Capture can generate a synthetic warp event. Never queue it for later ticks.
        if (motion.Relative.Length() > MaximumEventPixels) return;
        _pendingPixels = new Vector2(
            Mathf.Clamp(_pendingPixels.X + motion.Relative.X, -MaximumQueuedPixels, MaximumQueuedPixels),
            Mathf.Clamp(_pendingPixels.Y + motion.Relative.Y, -MaximumQueuedPixels, MaximumQueuedPixels));
    }

    public SwimIntent Consume(float delta, SwimTuning tuning, float sensitivity, bool invertY)
    {
        var pixels = _pendingPixels;
        _pendingPixels = Vector2.Zero;
        if (delta <= 0) return new SwimIntent(_thrustHeld ? 1 : 0, _slowHeld ? 1 : 0, 0, 0);
        var yaw = pixels.X * sensitivity / (Mathf.Max(0.001f, tuning.YawRateDegrees) * delta);
        var pitchPixels = invertY ? pixels.Y : -pixels.Y;
        var pitch = pitchPixels * sensitivity / (Mathf.Max(0.001f, tuning.PitchRateDegrees) * delta);
        return new SwimIntent(_thrustHeld ? 1 : 0, _slowHeld ? 1 : 0,
            Mathf.Clamp(yaw, -1, 1), Mathf.Clamp(pitch, -1, 1));
    }
}
