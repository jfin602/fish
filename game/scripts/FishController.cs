using Godot;

namespace Fish;

/// <summary>Authoritative fish transform, velocity and collision body.</summary>
public partial class FishController : CharacterBody3D
{
    [Export] public SwimTuning Tuning { get; set; } = new();
    private const float HorizontalLimit = 8.3f;
    private const float VerticalLimit = 3.0f;
    private SwimState _state;
    private bool _inputArmed = true;

    public override void _Ready() => MotionMode = MotionModeEnum.Floating;

    public override void _PhysicsProcess(double delta)
    {
        if (!_inputArmed)
        {
            Velocity = Vector3.Zero;
            // The release tick itself cannot propel the fish.
            if (KeyboardSwimInput.AllReleased()) _inputArmed = true;
            return;
        }

        _state = SwimMotor.Step(_state, KeyboardSwimInput.Read(), (float)delta, Tuning);
        Basis = new Basis(Vector3.Up, -_state.Yaw) * new Basis(Vector3.Right, _state.Pitch);
        Velocity = _state.Velocity;
        MoveAndSlide();

        // Collision and the existing habitat bounds remain authoritative.
        var clamped = new Vector3(
            Mathf.Clamp(Position.X, -HorizontalLimit, HorizontalLimit),
            Mathf.Clamp(Position.Y, -VerticalLimit, VerticalLimit),
            Mathf.Clamp(Position.Z, -HorizontalLimit, HorizontalLimit));
        var effective = Velocity;
        if (!Mathf.IsEqualApprox(clamped.X, Position.X)) effective.X = 0;
        if (!Mathf.IsEqualApprox(clamped.Y, Position.Y)) effective.Y = 0;
        if (!Mathf.IsEqualApprox(clamped.Z, Position.Z)) effective.Z = 0;
        Position = clamped;
        Velocity = effective;
        _state.Velocity = effective;
        _state.Speed = effective.Length();
    }

    public void ClearAndDisarmInput()
    {
        _inputArmed = false;
        _state.Speed = 0;
        _state.Velocity = Vector3.Zero;
        Velocity = Vector3.Zero;
    }

    public override void _Notification(int what)
    {
        if (what != NotificationApplicationFocusOut) return;
        KeyboardSwimInput.ReleaseAll();
        ClearAndDisarmInput();
    }
}
