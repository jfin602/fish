using Godot;

namespace Fish;

/// <summary>Authoritative fish transform, velocity and collision body.</summary>
public partial class FishController : CharacterBody3D
{
    [Export] public SwimTuning Tuning { get; set; } = new();
    private const float HorizontalLimit = 24.9f;
    private const float VerticalLimit = 9.0f;
    private readonly MouseSwimInput _mouse = new();
    private SwimState _state;
    private bool _inputArmed = true;
    private string _inputMode = DisplayPreferences.KeyboardMode;
    private float _mouseSensitivity = 0.12f;
    private bool _invertMouseY;
    private bool _mouseCaptureActive;

    public string ActiveInputMode => _inputMode;
    public bool CanUseMouseCamera => _inputMode == DisplayPreferences.MouseMode &&
        _mouseCaptureActive && _inputArmed && !GetTree().Paused;
    public bool CanZoomCamera => _inputArmed && !GetTree().Paused &&
        (_inputMode == DisplayPreferences.KeyboardMode || _mouseCaptureActive);
    public float PresentationSpeed => _state.Speed;
    public float PresentationEffort { get; private set; }
    public float PresentationTurn { get; private set; }

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
            PresentationEffort = 0;
            PresentationTurn = 0;
            // The release tick itself cannot propel the fish.
            if (KeyboardSwimInput.AllReleased() &&
                !Input.IsMouseButtonPressed(MouseButton.Left) &&
                !Input.IsMouseButtonPressed(MouseButton.Right) &&
                !Input.IsMouseButtonPressed(MouseButton.Middle)) _inputArmed = true;
            return;
        }

        var intent = _inputMode == DisplayPreferences.MouseMode
            ? _mouse.Consume((float)delta, Tuning, _mouseSensitivity, _invertMouseY)
            : KeyboardSwimInput.Read();
        var previousYaw = _state.Yaw;
        _state = SwimMotor.Step(_state, intent, (float)delta, Tuning);
        PresentationEffort = intent.Slow > 0 ? 0 : Mathf.Clamp(intent.Thrust, 0, 1);
        PresentationTurn = delta > 0 && Tuning.YawRateDegrees > 0
            ? Mathf.Clamp(Mathf.Wrap(_state.Yaw - previousYaw, -Mathf.Pi, Mathf.Pi) /
                (Mathf.DegToRad(Tuning.YawRateDegrees) * (float)delta), -1, 1)
            : 0;
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
        PresentationEffort = 0;
        PresentationTurn = 0;
        _state.Speed = 0;
        _state.Velocity = Vector3.Zero;
        Velocity = Vector3.Zero;
        GetNodeOrNull<FollowCamera>("CameraRig")?.ClearLookOffset();
    }

    public void SetInputMode(string mode)
    {
        if (!DisplayPreferences.IsSupportedInputMode(mode)) return;
        if (_inputMode != mode) GetNodeOrNull<FollowCamera>("CameraRig")?.ResetZoom();
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
