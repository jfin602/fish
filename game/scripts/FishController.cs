using Godot;
using System;

namespace Fish;

/// <summary>Authoritative fish transform, velocity and collision body.</summary>
public partial class FishController : CharacterBody3D
{
    public const float PondCenterClearanceFraction = .35f;
    [Export] public SwimTuning Tuning { get; set; } = new();
    // Safety clamp sits just inside the physical wall (inner face 37.2 m, fish radius .48 m).
    private float _horizontalLimit = 36.7f;
    private float _minimumY = -9f;
    private float _maximumY = 9f;
    private float? _pondSurfaceCenterLimit;
    private UnderwaterAtmosphere _pondAtmosphere;
    private float? _previousSurfaceLimit;
    private bool _surfaceFollowing;
    public float PondWaterlineHeight { get; private set; } = float.NaN;
    public float PondColliderRadius { get; private set; }
    public float PondSurfaceCenterLimit => float.IsFinite(PondWaterlineHeight)
        ? PondSurfaceLimitAt(GlobalPosition.X, GlobalPosition.Z) : float.NaN;
    public float PondSurfaceHeightAt(float worldX, float worldZ) =>
        _pondAtmosphere?.GetSurfaceHeight(worldX, worldZ) ?? PondWaterlineHeight;
    public float PondSurfaceLimitAt(float worldX, float worldZ) =>
        PondSurfaceHeightAt(worldX, worldZ) - PondCenterClearanceFraction * BodyRadius;
    public void BindPondSurface(UnderwaterAtmosphere atmosphere)
    {
        if (atmosphere.ProfileId != WorldEnvironmentProfile.Pond ||
            !float.IsFinite(PondWaterlineHeight))
            throw new InvalidOperationException("POND_SURFACE_BIND_INVALID");
        _pondAtmosphere = atmosphere;
        _previousSurfaceLimit = null;
        _surfaceFollowing = false;
    }
    public float BodyScale { get; private set; } = 1f;
    public float BodyRadius => GetNode<CollisionShape3D>("Collision").Shape is SphereShape3D sphere
        ? sphere.Radius : GetNode<CollisionShape3D>("Collision").Shape is CapsuleShape3D capsule
            ? capsule.Radius : 0f;
    public float BodyForwardReach => GetNode<CollisionShape3D>("Collision") is { Shape: CapsuleShape3D capsule } collision
        ? capsule.Height * .5f - collision.Position.Z : BodyRadius;
    public Vector3 MouthPosition => GlobalPosition - GlobalBasis.Z * BodyForwardReach;
    public Vector3 MouthForward => -GlobalBasis.Z;
    public void SetEncounterBassBody(BassBodyProfile body)
    {
        var (radius, length, centerZ, visualLength) = body;
        if (!float.IsFinite(radius) || !float.IsFinite(length) || radius <= 0f ||
            length < radius * 2f || !float.IsFinite(centerZ) ||
            !float.IsFinite(visualLength) || visualLength <= 0f ||
            GetNodeOrNull<CollisionShape3D>("Collision") is not { } collision)
            throw new InvalidOperationException("BASS_BODY_DIMENSIONS_INVALID");
        collision.Shape = new CapsuleShape3D { Radius = radius, Height = length };
        collision.Rotation = new Vector3(Mathf.Pi * .5f, 0f, 0f);
        // Each registered Bass rig has a different pivot-to-nose/tail offset.
        collision.Position = new Vector3(0f, 0f, centerZ);
        // Godot's default 1 mm recovery margin is a large fraction of a fry.
        // Keep its contact response proportional to the registered body.
        SafeMargin = Mathf.Clamp(radius * .08f, .00025f, .001f);
        MaxSlides = 8;
        PondColliderRadius = radius;
        BodyScale = visualLength;
        if (float.IsFinite(PondWaterlineHeight))
            _pondSurfaceCenterLimit = PondWaterlineHeight - PondCenterClearanceFraction * radius;
        _previousSurfaceLimit = null;
        _surfaceFollowing = false;
        GetNodeOrNull<FollowCamera>("CameraRig")?.ResetFollow();
    }
    public void FitCollisionToPlayerVisual()
    {
        var body = PlayerBodyGeometry.Measure(this);
        var collision = GetNode<CollisionShape3D>("Collision");
        collision.Shape = new CapsuleShape3D { Radius = body.Radius, Height = body.Height };
        collision.Rotation = new Vector3(Mathf.Pi * .5f, 0f, 0f);
        collision.Position = new Vector3(0f, 0f, body.CenterZ);
        SafeMargin = Mathf.Clamp(body.Radius * .08f, .00025f, .001f);
        PondColliderRadius = body.Radius;
        BodyScale = Mathf.Clamp(body.Length / .95f, .025f, 4f);
        if (float.IsFinite(PondWaterlineHeight))
            _pondSurfaceCenterLimit = PondWaterlineHeight - PondCenterClearanceFraction * body.Radius;
        _previousSurfaceLimit = null;
        _surfaceFollowing = false;
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
            _pondSurfaceCenterLimit = PondWaterlineHeight - PondCenterClearanceFraction * radius;
        _previousSurfaceLimit = null;
        _surfaceFollowing = false;
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
            GetNodeOrNull<CollisionShape3D>("Collision") is not { } collision ||
            !collision.Scale.IsEqualApprox(Vector3.One) || !Scale.IsEqualApprox(Vector3.One) ||
            !float.IsFinite(BodyRadius) || BodyRadius <= 0f)
            throw new InvalidOperationException(
                "POND_WATERLINE_INVALID action=check-generated-surface-and-fish-collider");
        PondWaterlineHeight = waterline;
        PondColliderRadius = BodyRadius;
        // The collider remains partly submerged. The registered body radius
        // allows the visible dorsal surface to reach the moving interface.
        _pondSurfaceCenterLimit = waterline - PondCenterClearanceFraction * BodyRadius;
        _previousSurfaceLimit = null;
        _surfaceFollowing = false;
        GD.Print($"POND_WATERLINE_READY water_y={waterline:F3} radius={BodyRadius:F3} center_max={_pondSurfaceCenterLimit:F3} upper_collision=disabled");
    }

    public static Vector3 ConstrainPondAscent(Vector3 velocity, float centerY,
        float centerLimitY, float delta, float approachBand = 0.35f)
    {
        if (velocity.Y <= 0f || delta <= 0f) return velocity;
        approachBand = Mathf.Max(.01f, approachBand);
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
    private bool _boostHeld;
    private bool _boostBlockedUntilRelease;
    private bool _boostExhausted;
    private float _boostEnergy = 1f;
    public bool EncounterBoostEnabled { get; set; }
    [Export] public float BoostSpeedMultiplier { get; set; } = 1.8f;
    [Export] public float BoostDrainSeconds { get; set; } = 2.5f;
    [Export] public float BoostRechargeSeconds { get; set; } = 5f;
    [Export] public float BoostRearmFraction { get; set; } = .25f;
    public float BoostEnergy => _boostEnergy;
    public bool BoostActive { get; private set; }

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
    public float PresentationPitchTurn { get; private set; }
    public float PresentationAngularTurn { get; private set; }
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
    public int GetHeldInputCount() => _input.HeldCount + (_boostHeld ? 1 : 0);
    public int GetMotorStepCount() => MotorStepCount;

    public override void _Ready() => MotionMode = MotionModeEnum.Floating;

    public override void _Input(InputEvent @event)
    {
        if (EncounterBoostEnabled && @event is InputEventKey key && !key.Echo &&
            (key.PhysicalKeycode == Key.Space || key.Keycode == Key.Space))
        {
            if (!key.Pressed)
            {
                _boostHeld = false;
                _boostBlockedUntilRelease = false;
            }
            else if (!_boostBlockedUntilRelease && _inputArmed && !_hatchLocked && !GetTree().Paused &&
                (!_boostExhausted || _boostEnergy >= BoostRearmFraction))
            {
                _boostHeld = true;
                _boostExhausted = false;
            }
            return; // Space is reserved for boost in this encounter, even in old custom presets.
        }
        if (_hatchLocked) return;
        _input.Accept(@event, _mouseCaptureActive && _inputArmed && !GetTree().Paused);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_hatchLocked) return;
        if (_boostBlockedUntilRelease && !Input.IsKeyPressed(Key.Space))
            _boostBlockedUntilRelease = false;
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
        BoostActive = EncounterBoostEnabled && _boostHeld && !_boostExhausted &&
            intent.Thrust > 0 && intent.Slow <= 0 && _boostEnergy > 0;
        if (BoostActive)
        {
            _boostEnergy = Mathf.Max(0f, _boostEnergy - (float)delta / Mathf.Max(.1f, BoostDrainSeconds));
            if (_boostEnergy <= 0f)
            {
                _boostExhausted = true;
                _boostHeld = false;
                _boostBlockedUntilRelease = true;
            }
        }
        else if (EncounterBoostEnabled)
            _boostEnergy = Mathf.Min(1f, _boostEnergy + (float)delta / Mathf.Max(.1f, BoostRechargeSeconds));
        var boostMultiplier = BoostActive ? Mathf.Max(1f, BoostSpeedMultiplier) : 1f;
        var previousYaw = _state.Yaw;
        var previousPitch = _state.Pitch;
        var next = SwimMotor.Step(_state, intent, (float)delta, Tuning, boostMultiplier);
        if (GetNode<CollisionShape3D>("Collision").Shape is CapsuleShape3D &&
            (intent.Yaw != 0f || intent.Pitch != 0f) &&
            EncounterTurnOverlaps(next))
        {
            // A long body can rotate into a branch without translating. Keep
            // motor state and physical heading together when a turn is blocked.
            var yawOnly = SwimMotor.Step(_state, intent with { Pitch = 0f }, (float)delta, Tuning, boostMultiplier);
            var pitchOnly = SwimMotor.Step(_state, intent with { Yaw = 0f }, (float)delta, Tuning, boostMultiplier);
            next = !EncounterTurnOverlaps(yawOnly) ? yawOnly
                : !EncounterTurnOverlaps(pitchOnly) ? pitchOnly
                : SwimMotor.Step(_state, intent with { Yaw = 0f, Pitch = 0f }, (float)delta, Tuning, boostMultiplier);
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
        PresentationPitchTurn = delta > 0 && Tuning.PitchRateDegrees > 0
            ? Mathf.Clamp((_state.Pitch - previousPitch) /
                (Mathf.DegToRad(Tuning.PitchRateDegrees) * (float)delta), -1, 1)
            : 0;
        PresentationAngularTurn = Mathf.Clamp(new Vector2(PresentationTurn, PresentationPitchTurn).Length(), 0, 1);
        Basis = new Basis(Vector3.Up, -_state.Yaw) * new Basis(Vector3.Right, _state.Pitch);
        var surfaceLimit = _pondSurfaceCenterLimit is not null
            ? PondSurfaceLimitAt(GlobalPosition.X, GlobalPosition.Z) : float.NaN;
        var followVelocity = 0f;
        if (float.IsFinite(surfaceLimit) && _previousSurfaceLimit is { } previousLimit &&
            _surfaceFollowing && Mathf.Abs(_state.Velocity.Y) < .15f)
            followVelocity = SurfaceFollowVelocity(previousLimit, surfaceLimit, (float)delta);
        Velocity = _pondSurfaceCenterLimit is not null
            ? ConstrainPondAscent(_state.Velocity, GlobalPosition.Y, surfaceLimit, (float)delta,
                Mathf.Min(.35f, Mathf.Max(.03f, BodyScale * .35f)))
            : _state.Velocity;
        Velocity += Vector3.Up * followVelocity;
        var requestedY = Velocity.Y;
        MoveAndSlide();

        // Collision and the existing habitat bounds remain authoritative.
        var clamped = new Vector3(
            Mathf.Clamp(Position.X, -_horizontalLimit, _horizontalLimit),
            _pondSurfaceCenterLimit is not null
                ? Mathf.Clamp(Position.Y, _minimumY,
                    PondSurfaceLimitAt(GlobalPosition.X, GlobalPosition.Z))
                : Mathf.Clamp(Position.Y, _minimumY, _maximumY),
            Mathf.Clamp(Position.Z, -_horizontalLimit, _horizontalLimit));
        var effective = Velocity;
        var clampedY = !Mathf.IsEqualApprox(clamped.Y, Position.Y);
        if (!Mathf.IsEqualApprox(clamped.X, Position.X)) effective.X = 0;
        if (!Mathf.IsEqualApprox(clamped.Y, Position.Y)) effective.Y = 0;
        if (!Mathf.IsEqualApprox(clamped.Z, Position.Z)) effective.Z = 0;
        Position = clamped;
        Velocity = effective;
        _state.Velocity = effective;
        if (!clampedY && Mathf.Abs(effective.Y - requestedY) < .001f)
            _state.Velocity -= Vector3.Up * followVelocity;
        _state.Speed = _state.Velocity.Length();
        if (_pondSurfaceCenterLimit is not null)
        {
            var currentLimit = PondSurfaceLimitAt(GlobalPosition.X, GlobalPosition.Z);
            _surfaceFollowing = currentLimit - GlobalPosition.Y <=
                Mathf.Max(.012f, BodyRadius * .15f) && _state.Velocity.Y >= -.15f;
            _previousSurfaceLimit = currentLimit;
        }
    }

    public static float SurfaceFollowVelocity(float previousLimit, float currentLimit, float delta) =>
        delta > 0f ? Mathf.Clamp((currentLimit - previousLimit) / delta, -1f, 1f) : 0f;

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
        _previousSurfaceLimit = null;
        _surfaceFollowing = false;
        _boostBlockedUntilRelease |= _boostHeld;
        _boostHeld = false;
        BoostActive = false;
        _inputArmed = false;
        _mouseCaptureActive = false;
        _input.ClearQueued();
        PresentationEffort = 0;
        PresentationTurn = 0;
        PresentationPitchTurn = 0;
        PresentationAngularTurn = 0;
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
