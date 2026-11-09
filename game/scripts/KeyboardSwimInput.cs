using Godot;

namespace Fish;

/// <summary>Keyboard-only intent; it never changes fish state directly.</summary>
public static class KeyboardSwimInput
{
    private static readonly string[] Actions =
        ["swim_thrust", "swim_slow", "swim_yaw_left", "swim_yaw_right", "swim_pitch_up", "swim_pitch_down"];

    public static SwimIntent Read() => new(
        Input.GetActionStrength("swim_thrust"),
        Input.GetActionStrength("swim_slow"),
        Input.GetActionStrength("swim_yaw_right") - Input.GetActionStrength("swim_yaw_left"),
        Input.IsKeyPressed(Key.Shift) ? 0 :
            Input.GetActionStrength("swim_pitch_up") - Input.GetActionStrength("swim_pitch_down"));

    public static bool AllReleased()
    {
        foreach (var action in Actions)
            if (Input.IsActionPressed(action)) return false;
        return true;
    }

    public static void ReleaseAll()
    {
        foreach (var action in Actions) Input.ActionRelease(action);
    }
}
