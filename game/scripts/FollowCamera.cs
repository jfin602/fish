using Godot;

namespace Fish;

/// <summary>World-space third-person view; reads the fish but never moves it.</summary>
public partial class FollowCamera : Node3D
{
    private const float ProbeRadius = 0.22f;
    private const float ObstacleClearance = 0.12f;
    private const float MinimumDistance = 0.75f;
    private const float TeleportDistance = 2.5f;
    private const float PitchHeightFraction = 0.35f;
    private const float AdaptiveFollowResponse = 4.0f; // About 0.25 seconds.
    private const float AdaptiveHeadingLimit = 35.0f;

    private FishController _fish = null!;
    private Camera3D _camera = null!;
    private SphereShape3D _probe = null!;
    private Vector3 _previousFishPosition;
    private Vector3 _lastHorizontalForward = Vector3.Forward;
    private Vector3 _lastCameraRear = Vector3.Forward;
    private readonly MouseSwimInput _orbitMouse = new();
    private float _orbitYaw;
    private float _orbitPitch;
    private Vector3 _adaptiveRear = Vector3.Forward;
    private float _adaptiveElevation;
    private float _adaptiveLookAhead = 1f;
    private bool _adaptiveInitialized;
    private bool _adaptiveFreeLookActive;
    private bool _adaptiveReturning;
    private float _panYaw;
    private float _panPitch;
    private float _zoomDistance;
    private bool _resetPending = true;

    public override void _Ready()
    {
        TopLevel = true; // Do not inherit fish pitch or later visual banking.
        ProcessMode = ProcessModeEnum.Always;
        _fish = GetParent<FishController>();
        _zoomDistance = _fish.Tuning.CameraDistance;
        _camera = GetNode<Camera3D>("Camera3D");
        _camera.Position = Vector3.Zero;
        _camera.Rotation = Vector3.Zero;
        _camera.Fov = _fish.Tuning.CameraFieldOfView;
        _camera.Near = 0.08f;
        _camera.KeepAspect = Camera3D.KeepAspectEnum.Height;
        _probe = new SphereShape3D { Radius = ProbeRadius };
        _previousFishPosition = _fish.GlobalPosition;
        GlobalPosition = DesiredPosition(out var lookTarget, out _);
        FaceTarget(lookTarget);
        GetViewport().SizeChanged += ResetFollow;
    }

    public void ResetFollow()
    {
        _resetPending = true;
        _adaptiveInitialized = false;
    }

    public void ResetZoom() => _zoomDistance = _fish.Tuning.CameraDistance;

    public void ClearLookOffset()
    {
        _panYaw = 0;
        _panPitch = 0;
        _orbitMouse.Clear();
        _adaptiveFreeLookActive = false;
        if (_fish.CameraControl == CameraControlPolicy.LegacyPan) _resetPending = true;
    }

    public void ClearQueuedMouseMotion() => _orbitMouse.Clear();
    public void OnCaptureStarted() => _orbitMouse.OnCaptureStarted();
    public void OnPresetChanged()
    {
        _orbitMouse.Clear();
        _orbitYaw = _orbitPitch = _panYaw = _panPitch = 0;
        _adaptiveInitialized = false;
        _adaptiveFreeLookActive = false;
        _adaptiveReturning = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (_fish.CanOrbitCamera ||
            (_fish.CameraControl == CameraControlPolicy.AdaptiveFollow && _fish.CanZoomCamera))
            _orbitMouse.Accept(@event);
        if (!_fish.CanZoomCamera || @event is not InputEventMouseButton { Pressed: true } wheel)
            return;
        var direction = wheel.ButtonIndex switch
        {
            MouseButton.WheelUp => -1,
            MouseButton.WheelDown => 1,
            _ => 0,
        };
        if (direction == 0) return;
        var tuning = _fish.Tuning;
        _zoomDistance = Mathf.Clamp(_zoomDistance + direction * tuning.CameraZoomStep,
            tuning.CameraZoomMinimumDistance, tuning.CameraZoomMaximumDistance);
        GetViewport().SetInputAsHandled();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GetTree().Paused)
        {
            _orbitMouse.Clear();
            _panYaw = 0;
            _panPitch = 0;
            _adaptiveFreeLookActive = false;
            if (_fish.CameraControl == CameraControlPolicy.LegacyPan) _resetPending = true;
            return;
        }

        UpdateCameraControls((float)delta);

        var fishPosition = _fish.GlobalPosition;
        if (fishPosition.DistanceTo(_previousFishPosition) > TeleportDistance) _resetPending = true;
        _previousFishPosition = fishPosition;

        var desired = DesiredPosition(out var lookTarget, out var anchor);
        var safeDesired = UnobstructedPosition(anchor, desired);
        if (_resetPending)
        {
            GlobalPosition = safeDesired;
            _resetPending = false;
        }
        else
        {
            var compressed = safeDesired.DistanceTo(anchor) + 0.05f < GlobalPosition.DistanceTo(anchor);
            var recovering = safeDesired.DistanceTo(anchor) > GlobalPosition.DistanceTo(anchor) + 0.05f;
            var response = compressed ? 30.0f : recovering
                ? _fish.Tuning.CameraRecoveryResponse : _fish.Tuning.CameraFollowResponse;
            var fraction = 1.0f - Mathf.Exp(-Mathf.Max(0.01f, response) * (float)delta);
            var candidate = GlobalPosition.Lerp(safeDesired, fraction);
            GlobalPosition = UnobstructedPosition(anchor, candidate);
        }
        FaceTarget(lookTarget);
    }

    private Vector3 DesiredPosition(out Vector3 lookTarget, out Vector3 anchor)
    {
        var fishPosition = _fish.GlobalPosition;
        var forward = (-_fish.GlobalBasis.Z).Normalized();
        var horizontal = new Vector3(forward.X, 0, forward.Z);
        if (horizontal.LengthSquared() > 0.0001f) _lastHorizontalForward = horizontal.Normalized();
        var tuning = _fish.Tuning;
        anchor = fishPosition + Vector3.Up * 0.35f;
        if (_fish.CameraControl == CameraControlPolicy.MouseOrbit360)
        {
            var orbitDistance = _zoomDistance;
            var baseElevation = Mathf.Atan2(tuning.CameraHeight - 0.35f, tuning.CameraDistance);
            var orbitElevation = baseElevation + _orbitPitch;
            var rear = new Basis(Vector3.Up, _orbitYaw) * _lastHorizontalForward;
            _lastCameraRear = rear.Normalized();
            lookTarget = anchor;
            return anchor - _lastCameraRear * (orbitDistance * Mathf.Cos(orbitElevation)) +
                Vector3.Up * (orbitDistance * Mathf.Sin(orbitElevation));
        }
        if (_fish.CameraControl == CameraControlPolicy.AdaptiveFollow)
        {
            var adaptiveDistance = _zoomDistance;
            var adaptiveEyeHeight = 0.35f + (tuning.CameraHeight - 0.35f) *
                adaptiveDistance / Mathf.Max(0.01f, tuning.CameraDistance) - forward.Y * adaptiveDistance * PitchHeightFraction;
            var radius = Mathf.Sqrt(adaptiveDistance * adaptiveDistance + Mathf.Pow(adaptiveEyeHeight - 0.35f, 2));
            _lastCameraRear = _adaptiveRear;
            var normalTarget = fishPosition + forward * tuning.CameraLookAhead + Vector3.Up * 0.15f;
            lookTarget = anchor.Lerp(normalTarget, _adaptiveLookAhead);
            return anchor - _adaptiveRear * (radius * Mathf.Cos(_adaptiveElevation)) +
                Vector3.Up * (radius * Mathf.Sin(_adaptiveElevation));
        }
        lookTarget = fishPosition + forward * tuning.CameraLookAhead + Vector3.Up * 0.15f;
        var distance = _zoomDistance;
        var eyeHeight = 0.35f + (tuning.CameraHeight - 0.35f) *
            distance / Mathf.Max(0.01f, tuning.CameraDistance) - forward.Y * distance * PitchHeightFraction;
        var elevation = Mathf.Atan2(eyeHeight - 0.35f, distance) + _panPitch;
        var orbitRadius = Mathf.Sqrt(distance * distance +
            Mathf.Pow(eyeHeight - 0.35f, 2));
        _lastCameraRear = (new Basis(Vector3.Up, _panYaw) * _lastHorizontalForward).Normalized();
        var right = _lastCameraRear.Cross(Vector3.Up).Normalized();
        return anchor - _lastCameraRear * (orbitRadius * Mathf.Cos(elevation)) +
            right * tuning.CameraShoulderOffset + Vector3.Up * (orbitRadius * Mathf.Sin(elevation));
    }

    private void UpdateCameraControls(float delta)
    {
        if (_fish.CameraControl == CameraControlPolicy.AdaptiveFollow)
        {
            UpdateAdaptiveFollow(delta);
            return;
        }
        if (_fish.CameraControl == CameraControlPolicy.MouseOrbit360)
        {
            var pixels = _orbitMouse.ConsumePixels();
            if (_fish.CanOrbitCamera)
            {
                var sensitivity = Mathf.DegToRad(_fish.ActiveMouseSensitivity);
                _orbitYaw = Mathf.Wrap(_orbitYaw + pixels.X * sensitivity *
                    (_fish.ActivePreset.MouseX.Inverted ? -1 : 1), -Mathf.Pi, Mathf.Pi);
                _orbitPitch = Mathf.Wrap(_orbitPitch + pixels.Y * sensitivity *
                    (_fish.ActiveMouseInvertY ? -1 : 1), -Mathf.Pi, Mathf.Pi);
            }
            return;
        }
        var mouseMode = _fish.ActiveInputMode == DisplayPreferences.MouseMode;
        if (mouseMode ? !_fish.CanUseMouseCamera : !_fish.CanZoomCamera)
        {
            _panYaw = 0;
            _panPitch = 0;
            return;
        }

        var tuning = _fish.Tuning;
        var hold = Input.IsKeyPressed(Key.Shift);
        var yaw = mouseMode
            ? (Input.IsKeyPressed(Key.D) ? 1 : 0) - (Input.IsKeyPressed(Key.A) ? 1 : 0)
            : (Input.IsKeyPressed(Key.Right) ? 1 : 0) - (Input.IsKeyPressed(Key.Left) ? 1 : 0);
        var pitch = mouseMode
            ? (Input.IsKeyPressed(Key.W) ? 1 : 0) - (Input.IsKeyPressed(Key.S) ? 1 : 0)
            : hold ? (Input.IsKeyPressed(Key.Up) ? 1 : 0) - (Input.IsKeyPressed(Key.Down) ? 1 : 0) : 0;
        var decay = 1.0f - Mathf.Exp(-Mathf.Max(0.01f, tuning.CameraPanReturnResponse) * delta);
        _panYaw = yaw != 0
            ? Mathf.Clamp(_panYaw + Mathf.DegToRad(tuning.CameraPanYawRateDegrees) * yaw * delta,
                -Mathf.DegToRad(tuning.CameraPanYawLimitDegrees), Mathf.DegToRad(tuning.CameraPanYawLimitDegrees))
            : hold ? _panYaw : Mathf.Lerp(_panYaw, 0, decay);
        _panPitch = pitch != 0
            ? Mathf.Clamp(_panPitch + Mathf.DegToRad(tuning.CameraPanPitchRateDegrees) * pitch * delta,
                -Mathf.DegToRad(tuning.CameraPanPitchLimitDegrees), Mathf.DegToRad(tuning.CameraPanPitchLimitDegrees))
            : hold ? _panPitch : Mathf.Lerp(_panPitch, 0, decay);
    }

    private void UpdateAdaptiveFollow(float delta)
    {
        var anchor = _fish.GlobalPosition + Vector3.Up * 0.35f;
        var fishForward = new Vector3(-_fish.GlobalBasis.Z.X, 0, -_fish.GlobalBasis.Z.Z);
        if (fishForward.LengthSquared() > 0.0001f) fishForward = fishForward.Normalized();
        else fishForward = _lastHorizontalForward;
        var tuning = _fish.Tuning;
        var distance = _zoomDistance;
        var forwardY = (-_fish.GlobalBasis.Z).Y;
        var eyeHeight = 0.35f + (tuning.CameraHeight - 0.35f) *
            distance / Mathf.Max(0.01f, tuning.CameraDistance) - forwardY * distance * PitchHeightFraction;
        var normalElevation = Mathf.Atan2(eyeHeight - 0.35f, distance);
        if (!_adaptiveInitialized)
        {
            var fromEye = anchor - GlobalPosition;
            var horizontal = new Vector3(fromEye.X, 0, fromEye.Z);
            _adaptiveRear = horizontal.LengthSquared() > 0.0001f ? horizontal.Normalized() : fishForward;
            _adaptiveElevation = Mathf.Atan2(GlobalPosition.Y - anchor.Y, horizontal.Length());
            _adaptiveInitialized = true;
        }

        var freeLook = _fish.AdaptiveFreeLookHeld;
        var pixels = _orbitMouse.ConsumePixels();
        if (freeLook)
        {
            if (!_adaptiveFreeLookActive)
            {
                // Begin at the current eye, including any collision compression.
                var fromEye = anchor - GlobalPosition;
                var horizontal = new Vector3(fromEye.X, 0, fromEye.Z);
                if (horizontal.LengthSquared() > 0.0001f) _adaptiveRear = horizontal.Normalized();
                _adaptiveElevation = Mathf.Atan2(GlobalPosition.Y - anchor.Y, horizontal.Length());
                pixels = Vector2.Zero; // Discard motion queued before C was held.
            }
            else
            {
                var sensitivity = Mathf.DegToRad(_fish.ActiveMouseSensitivity);
                _adaptiveRear = (new Basis(Vector3.Up, pixels.X * sensitivity *
                    (_fish.ActivePreset.MouseX.Inverted ? -1 : 1)) * _adaptiveRear).Normalized();
                _adaptiveElevation = Mathf.Wrap(_adaptiveElevation + pixels.Y * sensitivity *
                    (_fish.ActiveMouseInvertY ? -1 : 1), -Mathf.Pi, Mathf.Pi);
            }
            _adaptiveReturning = true;
        }
        else
        {
            var fraction = 1f - Mathf.Exp(-AdaptiveFollowResponse * delta);
            var difference = Mathf.Wrap(Mathf.Atan2(fishForward.X, fishForward.Z) -
                Mathf.Atan2(_adaptiveRear.X, _adaptiveRear.Z), -Mathf.Pi, Mathf.Pi);
            _adaptiveRear = (new Basis(Vector3.Up, difference * fraction) * _adaptiveRear).Normalized();
            if (_adaptiveReturning && Mathf.Abs(difference) <= Mathf.DegToRad(AdaptiveHeadingLimit))
                _adaptiveReturning = false;
            if (!_adaptiveReturning)
            {
                difference = Mathf.Wrap(Mathf.Atan2(fishForward.X, fishForward.Z) -
                    Mathf.Atan2(_adaptiveRear.X, _adaptiveRear.Z), -Mathf.Pi, Mathf.Pi);
                var limit = Mathf.DegToRad(AdaptiveHeadingLimit);
                if (Mathf.Abs(difference) > limit)
                    _adaptiveRear = (new Basis(Vector3.Up, difference - Mathf.Sign(difference) * limit) *
                        _adaptiveRear).Normalized();
            }
            _adaptiveElevation = Mathf.LerpAngle(_adaptiveElevation, normalElevation, fraction);
        }
        _adaptiveLookAhead = Mathf.Lerp(_adaptiveLookAhead, freeLook ? 0f : 1f,
            1f - Mathf.Exp(-8f * delta));
        _adaptiveFreeLookActive = freeLook;
    }

    private Vector3 UnobstructedPosition(Vector3 anchor, Vector3 target)
    {
        var safe = CastSafe(anchor, target);
        if (safe.DistanceTo(anchor) >= MinimumDistance) return safe;

        // A post close behind the fish can leave no usable view on the centerline.
        // Try either shoulder before accepting a camera position inside the body.
        var right = _lastCameraRear.Cross(Vector3.Up).Normalized();
        var best = safe;
        var bestTravel = float.PositiveInfinity;
        foreach (var side in new[] { 1.0f, -1.0f })
        {
            var shoulder = anchor - _lastCameraRear * 1.15f +
                Vector3.Up * 0.8f + right * (side * 1.1f);
            var clear = CastSafe(anchor, shoulder);
            if (clear.DistanceTo(anchor) < MinimumDistance) continue;
            var travel = clear.DistanceTo(GlobalPosition);
            if (travel >= bestTravel) continue;
            best = clear;
            bestTravel = travel;
        }
        return best;
    }

    private Vector3 CastSafe(Vector3 anchor, Vector3 target)
    {
        var motion = target - anchor;
        var length = motion.Length();
        if (length < 0.001f) return anchor - _lastHorizontalForward * MinimumDistance;
        var query = new PhysicsShapeQueryParameters3D
        {
            Shape = _probe,
            Transform = new Transform3D(Basis.Identity, anchor),
            Motion = motion,
            CollisionMask = 1,
            Margin = 0.02f,
            Exclude = new Godot.Collections.Array<Rid> { _fish.GetRid() },
        };
        var proportions = GetWorld3D().DirectSpaceState.CastMotion(query);
        if (proportions.Length == 0 || proportions[0] >= 0.999f) return target;
        // A nearby wall can force less than the preferred minimum distance.
        var allowed = Mathf.Max(0.05f, length * proportions[0] - ObstacleClearance);
        return anchor + motion.Normalized() * Mathf.Min(allowed, length);
    }

    private void FaceTarget(Vector3 target)
    {
        var toward = target - GlobalPosition;
        if (toward.LengthSquared() < 0.0001f) return;
        if (_fish.CameraControl is CameraControlPolicy.MouseOrbit360 or CameraControlPolicy.AdaptiveFollow)
        {
            var back = -toward.Normalized();
            var azimuthRight = _lastCameraRear.Cross(Vector3.Up).Normalized();
            var right = azimuthRight - back * azimuthRight.Dot(back);
            if (right.LengthSquared() < 0.0001f)
                right = back.Cross(Vector3.Up);
            if (right.LengthSquared() < 0.0001f)
                right = back.Cross(Vector3.Forward);
            right = right.Normalized();
            GlobalBasis = new Basis(right, back.Cross(right).Normalized(), back);
            return;
        }
        LookAt(target, Vector3.Up);
    }
}
