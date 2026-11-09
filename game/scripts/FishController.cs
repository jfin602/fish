using Godot;

namespace Fish;

/// <summary>Authoritative fish transform, velocity and collision body.</summary>
public partial class FishController : CharacterBody3D
{
    [Export] public SwimTuning Tuning { get; set; } = new();
    private const float HorizontalLimit = 8.3f;
    private const float VerticalLimit = 3.0f;
    private readonly MouseSwimInput _mouse = new();
    private SwimState _state;
    private bool _inputArmed = true;
    private string _inputMode = DisplayPreferences.KeyboardMode;
    private float _mouseSensitivity = 0.12f;
    private bool _invertMouseY;
    private bool _mouseCaptureActive;

    public string ActiveInputMode => _inputMode;

    public override void _Ready() => MotionMode = MotionModeEnum.Floating;

    public override void _Input(InputEvent @event)
    {
        if (_inputMode != DisplayPreferences.MouseMode || !_mouseCaptureActive || !_inputArmed ||
            GetTree().Paused) return;
        _mouse.Accept(@event);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_inputArmed)
        {
            Velocity = Vector3.Zero;
            // The release tick itself cannot propel the fish.
            if (KeyboardSwimInput.AllReleased() &&
                !Input.IsMouseButtonPressed(MouseButton.Left) &&
                !Input.IsMouseButtonPressed(MouseButton.Right)) _inputArmed = true;
            return;
        }

        var intent = _inputMode == DisplayPreferences.MouseMode
            ? _mouse.Consume((float)delta, Tuning, _mouseSensitivity, _invertMouseY)
            : KeyboardSwimInput.Read();
        _state = SwimMotor.Step(_state, intent, (float)delta, Tuning);
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
        _mouseCaptureActive = false;
        _mouse.Clear();
        _state.Speed = 0;
        _state.Velocity = Vector3.Zero;
        Velocity = Vector3.Zero;
    }

    public void SetInputMode(string mode)
    {
        if (!DisplayPreferences.IsSupportedInputMode(mode)) return;
        ClearAndDisarmInput();
        _inputMode = mode;
    }

    public void ConfigureMouse(float sensitivity, bool invertY)
    {
        _mouseSensitivity = Mathf.Clamp(sensitivity, 0.04f, 0.4f);
        _invertMouseY = invertY;
        _mouse.Clear();
    }

    public void ResetMouseCapture()
    {
        _mouse.OnCaptureStarted();
        _mouseCaptureActive = true;
    }

    public override void _Notification(int what)
    {
        if (what != NotificationApplicationFocusOut) return;
        KeyboardSwimInput.ReleaseAll();
        ClearAndDisarmInput();
    }
}
