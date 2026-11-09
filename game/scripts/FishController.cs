using Godot;

namespace Fish;

/// <summary>Foundation prototype controls: WASD/arrows horizontal, E/Space up, Q/PageDown down.</summary>
public partial class FishController : CharacterBody3D
{
    [Export] public float SwimSpeed { get; set; } = 4.0f;
    private const float HorizontalLimit = 8.3f;
    private const float VerticalLimit = 3.0f;
    private static readonly string[] SwimActions =
        ["swim_left", "swim_right", "swim_forward", "swim_back", "swim_up", "swim_down"];
    private bool _inputArmed = true;

    public override void _Ready() => MotionMode = MotionModeEnum.Floating;

    public override void _PhysicsProcess(double delta)
    {
        if (!_inputArmed)
        {
            Velocity = Vector3.Zero;
            // Wait for every old key to be released, then skip one more physics tick.
            if (AllSwimActionsReleased()) _inputArmed = true;
            return;
        }
        var horizontal = Input.GetVector("swim_left", "swim_right", "swim_forward", "swim_back");
        var vertical = Input.GetActionStrength("swim_up") - Input.GetActionStrength("swim_down");
        var direction = new Vector3(horizontal.X, vertical, horizontal.Y).Normalized();
        Velocity = direction * SwimSpeed;
        MoveAndSlide();

        // A second, explicit limit is useful even when a collision is missed by a future scene edit.
        Position = new Vector3(
            Mathf.Clamp(Position.X, -HorizontalLimit, HorizontalLimit),
            Mathf.Clamp(Position.Y, -VerticalLimit, VerticalLimit),
            Mathf.Clamp(Position.Z, -HorizontalLimit, HorizontalLimit));
    }

    public void ClearAndDisarmInput()
    {
        _inputArmed = false;
        Velocity = Vector3.Zero;
    }

    private static bool AllSwimActionsReleased()
    {
        foreach (var action in SwimActions)
            if (Input.IsActionPressed(action)) return false;
        return true;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusOut)
        {
            foreach (var action in SwimActions)
                Input.ActionRelease(action);
            ClearAndDisarmInput();
        }
    }
}
