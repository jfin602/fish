using Godot;

namespace Fish;

/// <summary>Authoritative fish transform, velocity and collision body.</summary>
public partial class FishController : CharacterBody3D
{
    [Export] public SwimTuning Tuning { get; set; } = new();
    private const float HorizontalLimit = 24.9f;
    private const float VerticalLimit = 9.0f;
    private readonly ProfileSwimInput _input = new();
    private SwimState _state;
    private bool _inputArmed = true;
    private float _mouseSensitivity = 0.12f;
    private bool _invertMouseY;
    private bool _legacyMouseSettings;
    private bool _mouseCaptureActive;

    public string ActiveInputMode => _input.UsesKeyboardSteering
        ? DisplayPreferences.KeyboardMode : DisplayPreferences.MouseMode;
    public string ActivePresetId => _input.Profile.Id;
    public ControlPreset ActivePreset => _input.Profile;
    public bool UsesMouseInput => _input.UsesMouse;
    public CameraControlPolicy CameraControl => _input.Profile.CameraControl;
    public float ActiveMouseSensitivity => _legacyMouseSettings ? _mouseSensitivity : _input.Profile.MouseSensitivity;
    public bool ActiveMouseInvertY => _legacyMouseSettings ? _invertMouseY : _input.Profile.MouseY.Inverted;
    public bool IsPauseEvent(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Echo: false } key)
        {
            var code = (int)key.PhysicalKeycode;
            if (code == 0) code = (int)key.Keycode;
            if (code == ControlPreset.RecoveryKey) return true;
            foreach (var binding in _input.Profile.Bindings(ControlAction.PauseOptions))
                if (binding.Device == BindingDevice.PhysicalKey && binding.Code == code) return true;
        }
        if (@event is InputEventMouseButton { Pressed: true } button)
        {
            if (_input.UsesMouse && (int)button.ButtonIndex == ControlPreset.RecoveryMouseButton)
                return true;
            foreach (var binding in _input.Profile.Bindings(ControlAction.PauseOptions))
                if (binding.Device == BindingDevice.MouseButton && binding.Code == (int)button.ButtonIndex)
                    return true;
        }
        return false;
    }
    public bool CanUseMouseCamera => !_input.UsesKeyboardSteering && _input.UsesMouse &&
        _mouseCaptureActive && _inputArmed && !GetTree().Paused;
    public bool CanOrbitCamera => CameraControl == CameraControlPolicy.MouseOrbit360 &&
        _mouseCaptureActive && _inputArmed && !GetTree().Paused;
    public bool AdaptiveFreeLookHeld => _input.AdaptiveFreeLookHeld &&
        _mouseCaptureActive && _inputArmed && !GetTree().Paused;
    public bool CanZoomCamera => _inputArmed && !GetTree().Paused &&
        (!_input.UsesMouse || _mouseCaptureActive);
    public float PresentationSpeed => _state.Speed;
    public float PresentationEffort { get; private set; }
    public float PresentationTurn { get; private set; }
    public float LastThrust { get; private set; }
    public float LastSlow { get; private set; }
    public float LastYaw { get; private set; }
    public float LastPitch { get; private set; }
    public int MotorStepCount { get; private set; }
    public string GetActivePresetId() => ActivePresetId;
    public float GetLastThrust() => LastThrust;
    public float GetLastSlow() => LastSlow;
    public float GetLastYaw() => LastYaw;
    public float GetLastPitch() => LastPitch;
    public bool GetInputArmed() => _inputArmed;
    public int GetHeldInputCount() => _input.HeldCount;
    public int GetMotorStepCount() => MotorStepCount;

    public override void _Ready() => MotionMode = MotionModeEnum.Floating;

    public override void _Input(InputEvent @event)
    {
        _input.Accept(@event, _mouseCaptureActive && _inputArmed && !GetTree().Paused);
    }

    public override void _PhysicsProcess(double delta)
    {
        var intent = new SwimIntent(0, 0, 0, 0);
        if (!_inputArmed)
        {
            // The release tick itself cannot propel the fish.
            if (_input.AllReleased) _inputArmed = true;
        }
        else
        {
            intent = _input.Consume((float)delta, Tuning, _mouseCaptureActive,
                ActiveMouseSensitivity, ActiveMouseInvertY);
        }
        LastThrust = intent.Thrust;
        LastSlow = intent.Slow;
        LastYaw = intent.Yaw;
        LastPitch = intent.Pitch;
        var previousYaw = _state.Yaw;
        _state = SwimMotor.Step(_state, intent, (float)delta, Tuning);
        MotorStepCount++;
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

    public void ClearAndDisarmInput() => ClearAndDisarmInput(true);

    public void ClearAndDisarmInput(bool stopMotion)
    {
        _inputArmed = false;
        _mouseCaptureActive = false;
        _input.ClearQueued();
        PresentationEffort = 0;
        PresentationTurn = 0;
        LastThrust = LastSlow = LastYaw = LastPitch = 0;
        if (stopMotion)
        {
            _state.Speed = 0;
            _state.Velocity = Vector3.Zero;
            Velocity = Vector3.Zero;
        }
        var camera = GetNodeOrNull<FollowCamera>("CameraRig");
        camera?.ClearQueuedMouseMotion();
        if (stopMotion) camera?.ClearLookOffset();
    }

    public void SetActivePreset(ControlPreset profile)
    {
        ClearAndDisarmInput(stopMotion: false);
        _input.Select(profile);
        _legacyMouseSettings = profile.IsBuiltin;
        GetNodeOrNull<FollowCamera>("CameraRig")?.OnPresetChanged();
        // Keep live switches usable too; a paused menu retains its visible pointer.
        if (!GetTree().Paused)
        {
            Input.MouseMode = _input.UsesMouse ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
            if (_input.UsesMouse) ResetMouseCapture();
        }
    }

    // Compatibility for the old two-choice shell and saved preferences.
    public void SetInputMode(string mode)
    {
        if (!DisplayPreferences.IsSupportedInputMode(mode)) return;
        var id = mode == DisplayPreferences.MouseMode ? ControlPreset.MouseId : ControlPreset.KeyboardId;
        foreach (var profile in ControlPreset.Builtins())
            if (profile.Id == id) { SetActivePreset(profile); return; }
    }

    public void ConfigureMouse(float sensitivity, bool invertY)
    {
        _mouseSensitivity = float.IsFinite(sensitivity)
            ? Mathf.Clamp(sensitivity, 0.04f, 0.4f) : ControlPreset.DefaultSensitivity;
        _invertMouseY = invertY;
        _input.ClearQueued();
    }

    public void ResetMouseCapture()
    {
        _input.CaptureStarted();
        _mouseCaptureActive = true;
        GetNodeOrNull<FollowCamera>("CameraRig")?.OnCaptureStarted();
    }

    public override void _Notification(int what)
    {
        if (what != NotificationApplicationFocusOut) return;
        ClearAndDisarmInput();
    }
}
