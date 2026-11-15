#nullable enable
using System;

namespace Fish;

/// <summary>Presentation-only gate for underwater turns and active boosts.</summary>
public static class PondUnderwaterTurnWake
{
    public const float StartDegreesPerSecond = 42f;
    public const float ContinueDegreesPerSecond = 22f;
    public const float MinimumSpeedRatio = .18f;

    public static bool Update(bool active, float degreesPerSecond, float speedRatio,
        float depth, bool surfaceWakeActive, bool boostActive)
    {
        if (!float.IsFinite(degreesPerSecond) || !float.IsFinite(speedRatio) ||
            !float.IsFinite(depth) || surfaceWakeActive ||
            depth <= PondWakeContacts.NearSurfaceDepth ||
            (speedRatio < MinimumSpeedRatio && !boostActive))
            return false;
        return boostActive ||
            degreesPerSecond >= (active ? ContinueDegreesPerSecond : StartDegreesPerSecond);
    }
}
