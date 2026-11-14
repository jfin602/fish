using Godot;
using System;

namespace Fish;

/// <summary>Authoritative fish transform, velocity and collision body.</summary>
public partial class FishController : CharacterBody3D
{
    [Export] public SwimTuning Tuning { get; set; } = new();
    // Safety clamp sits just inside the physical wall (inner face 37.2 m, fish radius .48 m).
    private float _horizontalLimit = 36.7f;
    private float _minimumY = -9f;
    private float _maximumY = 9f;
    private float? _pondSurfaceCenterLimit;
    public float PondWaterlineHeight { get; private set; } = float.NaN;
    public float PondColliderRadius { get; private set; }
    public float PondSurfaceCenterLimit => _pondSurfaceCenterLimit ?? float.NaN;
    public float BodyScale { get; private set; } = 1f;
    public float BodyRadius => GetNode<CollisionShape3D>("Collision").Shape is SphereShape3D sphere
        ? sphere.Radius : GetNode<CollisionShape3D>("Collision").Shape is CapsuleShape3D capsule
            ? capsule.Radius : 0f;
    public float BodyForwardReach => GetNode<CollisionShape3D>("Collision") is { Shape: CapsuleShape3D capsule } collision
        ? capsule.Height * .5f - collision.Position.Z : BodyRadius;
    public void SetEncounterBassBody(float radius, float length)
    {
        if (!float.IsFinite(radius) || !float.IsFinite(length) || radius <= 0f ||
            length < radius * 2f ||
            GetNodeOrNull<CollisionShape3D>("Collision") is not { } collision)
            throw new InvalidOperationException("BASS_BODY_DIMENSIONS_INVALID");
        collision.Shape = new CapsuleShape3D { Radius = radius, Height = length };
        collision.Rotation = new Vector3(Mathf.Pi * .5f, 0f, 0f);
        // Mesh bounds put the larva nose/tail midpoint at +4.5% of its length.
        // The former +8% offset left the visible mouth beyond the collider.
        collision.Position = new Vector3(0f, 0f, length * .045f);
        // Godot's default 1 mm recovery margin is a large fraction of a fry.
        // Keep its contact response proportional to the registered body.
        SafeMargin = Mathf.Clamp(radius * .08f, .00025f, .001f);
        MaxSlides = 8;
        PondColliderRadius = radius;
        BodyScale = length;
        if (float.IsFinite(PondWaterlineHeight))
            _pondSurfaceCenterLimit = PondWaterlineHeight - .65f * radius;
        GetNodeOrNull<FollowCamera>("CameraRig")?.ResetFollow();
    }
    public void SetPondBodyRadius(float radius)
    {
        if (!float.IsFinite(radius) || radius <= 0f ||
            GetNodeOrNull<CollisionShape3D>("Collision") is not { Shape: SphereShape3D } collision)
            throw new InvalidOperationException("POND_BODY_RADIUS_INVALID");
        collision.Shape = new SphereShape3D { Radius = radius };
        PondColliderRadius = radius;
        BodyScale = radius / .48f;
        if (float.IsFinite(PondWaterlineHeight))
            _pondSurfaceCenterLimit = PondWaterlineHeight - .65f * radius;
        GetNodeOrNull<FollowCamera>("CameraRig")?.ResetFollow();
    }
    public void ConfigureWorldBounds(float horizontalLimit, float minimumY, float maximumY)
    {
        if (horizontalLimit < 36.7f || minimumY >= maximumY)
            throw new System.ArgumentOutOfRangeException(nameof(horizontalLimit));
        _horizontalLimit = horizontalLimit;
        _minimumY = minimumY;
        _maximumY = maximumY;
    }

    public void ConfigurePondWaterline(SeabedSurface seabed)
    {
        if (!seabed.TryGetPondWaterSurfaceHeight(out var waterline) ||
            GetNodeOrNull<CollisionShape3D>("Collision") is not { Shape: SphereShape3D sphere } collision ||
            !collision.Position.IsZeroApprox() || !collision.Scale.IsEqualApprox(Vector3.One) ||
            !Scale.IsEqualApprox(Vector3.One) || !float.IsFinite(sphere.Radius) || sphere.Radius <= 0f)
            throw new InvalidOperationException(
                "POND_WATERLINE_INVALID action=check-generated-surface-and-fish-collider");
        PondWaterlineHeight = waterline;
        PondColliderRadius = sphere.Radius;
        // The collider remains mostly submerged. At the level plane its top
        // can emerge by 35% of its radius; dorsal anatomy can emerge farther.
        _pondSurfaceCenterLimit = waterline - 0.65f * sphere.Radius;
        GD.Print($"POND_WATERLINE_READY water_y={waterline:F3} radius={sphere.Radius:F3} center_max={_pondSurfaceCenterLimit:F3} upper_collision=disabled");
    }

    public static Vector3 ConstrainPondAscent(Vector3 velocity, float centerY,
        float centerLimitY, float delta)
    {
        if (velocity.Y <= 0f || delta <= 0f) return velocity;
        const float approachBand = 0.35f;
        var remaining = Mathf.Max(0f, centerLimitY - centerY);
        if (remaining >= approachBand) return velocity;
        var vertical = Mathf.Min(velocity.Y * Mathf.SmoothStep(0f, 1f,
            remaining / approachBand), remaining / delta);
        var horizontal = new Vector2(velocity.X, velocity.Z);
        if (horizontal.IsZeroApprox()) return new Vector3(0f, vertical, 0f);
        var horizontalSpeed = Mathf.Sqrt(Mathf.Max(0f,
            velocity.LengthSquared() - vertical * vertical));
        var tangent = horizontal.Normalized() * horizontalSpeed;
        return new Vector3(tangent.X, vertical, tangent.Y);
    }
    private readonly ProfileSwimInput _input = new();
    private SwimState _state;
    private bool _inputArmed = true;
    private float _mouseSensitivity = 0.12f;
    private bool _invertMouseY;
    private bool _legacyMouseSettings;
    private bool _mouseCaptureActive;
    private bool _hatchLocked;
    private float _fryYaw;
    private float _fryPitch;

    public void SetEncounterHatchLocked(bool locked)
    {
        _hatchLocked = locked;
        ClearAndDisarmInput();
        GetNode<CollisionShape3D>("Collision").Disabled = locked;
    }

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
    public bool DebugWheelControlsTime { get; set; }
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
        if (_hatchLocked) return;
        _input.Accept(@event, _mouseCaptureActive && _inputArmed && !GetTree().Paused);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_hatchLocked) return;
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
        if (GetNode<CollisionShape3D>("Collision").Shape is CapsuleShape3D { Height: < .12f })
        {
            var turnFraction = 1f - Mathf.Exp(-6f * (float)delta);
            _fryYaw = Mathf.Lerp(_fryYaw, intent.Yaw, turnFraction);
            _fryPitch = Mathf.Lerp(_fryPitch, intent.Pitch, turnFraction);
            intent = intent with { Yaw = _fryYaw, Pitch = _fryPitch };
        }
        LastThrust = intent.Thrust;
        LastSlow = intent.Slow;
        LastYaw = intent.Yaw;
        LastPitch = intent.Pitch;
        var previousYaw = _state.Yaw;
        var next = SwimMotor.Step(_state, intent, (float)delta, Tuning);
        if (GetNode<CollisionShape3D>("Collision").Shape is CapsuleShape3D &&
            (intent.Yaw != 0f || intent.Pitch != 0f) &&
            EncounterTurnOverlaps(next))
        {
            // A long body can rotate into a branch without translating. Keep
            // motor state and physical heading together when a turn is blocked.
            var yawOnly = SwimMotor.Step(_state, intent with { Pitch = 0f }, (float)delta, Tuning);
            var pitchOnly = SwimMotor.Step(_state, intent with { Yaw = 0f }, (float)delta, Tuning);
            next = !EncounterTurnOverlaps(yawOnly) ? yawOnly
                : !EncounterTurnOverlaps(pitchOnly) ? pitchOnly
                : SwimMotor.Step(_state, intent with { Yaw = 0f, Pitch = 0f }, (float)delta, Tuning);
        }
        _state = next;
        MotorStepCount++;
        PresentationEffort = _state.Reversing
            ? Mathf.Clamp(intent.Slow, 0, 1)
            : intent.Slow > 0 ? 0 : Mathf.Clamp(intent.Thrust, 0, 1);
        PresentationTurn = delta > 0 && Tuning.YawRateDegrees > 0
            ? Mathf.Clamp(Mathf.Wrap(_state.Yaw - previousYaw, -Mathf.Pi, Mathf.Pi) /
                (Mathf.DegToRad(Tuning.YawRateDegrees) * (float)delta), -1, 1)
            : 0;
        Basis = new Basis(Vector3.Up, -_state.Yaw) * new Basis(Vector3.Right, _state.Pitch);
        Velocity = _pondSurfaceCenterLimit is { } limit
            ? ConstrainPondAscent(_state.Velocity, Position.Y, limit, (float)delta)
            : _state.Velocity;
        MoveAndSlide();

        // Collision and the existing habitat bounds remain authoritative.
        var clamped = new Vector3(
            Mathf.Clamp(Position.X, -_horizontalLimit, _horizontalLimit),
            _pondSurfaceCenterLimit is { } surfaceLimit
                ? Mathf.Clamp(Position.Y, _minimumY, surfaceLimit)
                : Mathf.Clamp(Position.Y, _minimumY, _maximumY),
            Mathf.Clamp(Position.Z, -_horizontalLimit, _horizontalLimit));
        var effective = Velocity;
        if (!Mathf.IsEqualApprox(clamped.X, Position.X)) effective.X = 0;
        if (!Mathf.IsEqualApprox(clamped.Y, Position.Y)) effective.Y = 0;
        if (!Mathf.IsEqualApprox(clamped.Z, Position.Z)) effective.Z = 0;
        Position = clamped;
        Velocity = effective;
        _state.Velocity = effective;
        _state.Speed = effective.Length();
    }

    private bool EncounterTurnOverlaps(SwimState state)
    {
        var collider = GetNode<CollisionShape3D>("Collision");
        if (collider.Shape is not CapsuleShape3D) return false;
        var heading = new Basis(Vector3.Up, -state.Yaw) *
            new Basis(Vector3.Right, state.Pitch);
        using var query = new PhysicsShapeQueryParameters3D
        {
            Shape = collider.Shape,
            Transform = new Transform3D(heading, GlobalPosition) * collider.Transform,
            CollisionMask = CollisionMask,
            Margin = 0f,
            Exclude = new Godot.Collections.Array<Rid> { GetRid() },
        };
        return GetWorld3D().DirectSpaceState.IntersectShape(query, 1).Count != 0;
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
        _fryYaw = _fryPitch = 0;
        if (stopMotion)
        {
            _state.Speed = 0;
            _state.Velocity = Vector3.Zero;
            _state.Reversing = false;
            _state.WaitingToReverse = false;
            _state.ReverseTransitionWaitSeconds = 0;
            Velocity = Vector3.Zero;
        }
        else
        {
            _state.WaitingToReverse = false;
            _state.ReverseTransitionWaitSeconds = 0;
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
