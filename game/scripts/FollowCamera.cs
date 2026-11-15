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
    private const float QuietVerticalResponse = 8f;
    private const float ActiveVerticalResponse = 16f;
    private const float PitchPresentationResponse = 12f;

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
    private bool _larvaView;
    private float _followY;
    private float _viewForwardY;
    private bool _followTargetInitialized;
    private Vector3 _previousLookTarget;
    private Vector3 _currentLookTarget;
    private bool _collisionCompressed;

    public void ConfigureEncounterLarvaView(bool enabled)
    {
        _larvaView = enabled;
        if (_camera is not null)
        {
            _camera.Near = enabled ? .006f : .08f;
            // The Pond surface extends beyond 50 m from the nursery. Clipping it
            // produced a camera-following hard horizon in the fry view.
            _camera.Far = enabled ? 200f : 4000f;
        }
        ResetZoom();
        ResetFollow();
    }

    private float ViewScale => Mathf.Max(_larvaView ? .11f : .32f, _fish.BodyScale);

    public override void _Ready()
    {
        TopLevel = true; // Do not inherit fish pitch or later visual banking.
        PhysicsInterpolationMode = PhysicsInterpolationModeEnum.On;
        ProcessMode = ProcessModeEnum.Always;
        _fish = GetParent<FishController>();
        // The egg emergence moves in render time; enable body interpolation at
        // the handoff to physics-driven swimming instead of during the intro.
        if (_fish.GetParent() is not PikeEncounter)
            _fish.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.On;
        _zoomDistance = _fish.Tuning.CameraDistance;
        _camera = GetNode<Camera3D>("Camera3D");
        _camera.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;
        _camera.Position = Vector3.Zero;
        _camera.Rotation = Vector3.Zero;
        _camera.Fov = _fish.Tuning.CameraFieldOfView;
        _camera.Near = 0.08f;
        _camera.KeepAspect = Camera3D.KeepAspectEnum.Height;
        _probe = new SphereShape3D { Radius = ProbeRadius };
        _previousFishPosition = _fish.GlobalPosition;
        UpdateFollowTarget(0f, true);
        GlobalBasis = Basis.Identity;
        GlobalPosition = DesiredPosition(out var lookTarget, out _);
        _previousLookTarget = _currentLookTarget = lookTarget;
        FaceTarget(lookTarget, GlobalPosition);
        ResetPhysicsInterpolation();
        GetViewport().SizeChanged += SnapToFish;
    }

    public void ResetFollow()
    {
        _resetPending = true;
        _adaptiveInitialized = false;
    }

    public void SnapToFish()
    {
        UpdateFollowTarget(0f, true);
        var horizontalForward = -_fish.GlobalBasis.Z;
        horizontalForward.Y = 0;
        _adaptiveRear = horizontalForward.LengthSquared() > .0001f
            ? horizontalForward.Normalized() : _lastHorizontalForward;
        var distance = _zoomDistance * ViewScale;
        var eyeRise = (_fish.Tuning.CameraHeight - .35f) * ViewScale *
            _zoomDistance / Mathf.Max(.01f, _fish.Tuning.CameraDistance) -
            _viewForwardY * distance * PitchHeightFraction;
        _adaptiveElevation = Mathf.Atan2(eyeRise, distance);
        _adaptiveLookAhead = 1f;
        _adaptiveInitialized = true;
        _adaptiveFreeLookActive = false;
        _adaptiveReturning = false;
        _probe.Radius = _larvaView ? .012f : ProbeRadius * ViewScale;
        var desired = DesiredPosition(out var target, out var anchor);
        var safe = UnobstructedPosition(anchor, desired);
        GlobalPosition = ResolveEncounterOverlap(anchor, safe);
        _collisionCompressed = safe.DistanceTo(desired) > .02f * ViewScale;
        _previousLookTarget = _currentLookTarget = target;
        FaceTarget(target, GlobalPosition);
        _fish.ResetPhysicsInterpolation();
        ResetPhysicsInterpolation();
        _previousFishPosition = _fish.GlobalPosition;
        _resetPending = false;
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
        if (!_fish.CanZoomCamera || _fish.DebugWheelControlsTime ||
            @event is not InputEventMouseButton { Pressed: true } wheel)
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
        _probe.Radius = _larvaView ? .012f : ProbeRadius * ViewScale;

        var fishPosition = _fish.GlobalPosition;
        var teleported = fishPosition.DistanceTo(_previousFishPosition) > TeleportDistance;
        if (teleported) _resetPending = true;
        _previousFishPosition = fishPosition;
        UpdateFollowTarget((float)delta, _resetPending);

        var desired = DesiredPosition(out var lookTarget, out var anchor);
        _previousLookTarget = _currentLookTarget;
        _currentLookTarget = lookTarget;
        var safeDesired = UnobstructedPosition(anchor, desired);
        var obstructed = safeDesired.DistanceTo(desired) > .02f * ViewScale;
        if (_resetPending)
        {
            GlobalPosition = ResolveEncounterOverlap(anchor, safeDesired);
            _collisionCompressed = obstructed;
            if (teleported) _fish.ResetPhysicsInterpolation();
            _resetPending = false;
            _previousLookTarget = _currentLookTarget;
            ResetPhysicsInterpolation();
        }
        else
        {
            if (obstructed) _collisionCompressed = true;
            var compressed = obstructed &&
                safeDesired.DistanceTo(anchor) + .05f * ViewScale < GlobalPosition.DistanceTo(anchor);
            var recovering = !obstructed && _collisionCompressed;
            var response = compressed ? 30.0f : recovering
                ? _fish.Tuning.CameraRecoveryResponse : _fish.Tuning.CameraFollowResponse;
            var fraction = 1.0f - Mathf.Exp(-Mathf.Max(0.01f, response) * (float)delta);
            var candidate = GlobalPosition.Lerp(safeDesired, fraction);
            GlobalPosition = ResolveEncounterOverlap(anchor, UnobstructedPosition(anchor, candidate));
            if (recovering && GlobalPosition.DistanceTo(safeDesired) < .05f * ViewScale)
                _collisionCompressed = false;
        }
        FaceTarget(lookTarget, GlobalPosition);
    }

    public override void _Process(double delta)
    {
        if (GetTree().Paused || !IsPhysicsProcessing()) return;
        // Collision and the follow solve remain in physics. Only the horizon is
        // reconstructed per rendered frame from interpolated, already-safe poses.
        var fraction = Mathf.Clamp((float)Engine.GetPhysicsInterpolationFraction(), 0f, 1f);
        var eye = GetGlobalTransformInterpolated().Origin;
        var target = _previousLookTarget.Lerp(_currentLookTarget, fraction);
        FaceTarget(target, eye);
    }

    private void UpdateFollowTarget(float delta, bool snap)
    {
        var fishY = _fish.GlobalPosition.Y;
        var fishForwardY = (-_fish.GlobalBasis.Z).Y;
        if (snap || !_followTargetInitialized)
        {
            _followY = fishY;
            _viewForwardY = fishForwardY;
            _followTargetInitialized = true;
            return;
        }
        // Swimming upward or downward should be responsive. Near level travel,
        // filter tiny contact and animation-scale changes from the eye height.
        var verticalResponse = Mathf.Abs(_fish.Velocity.Y) > .35f
            ? ActiveVerticalResponse : QuietVerticalResponse;
        _followY = Mathf.Lerp(_followY, fishY,
            1f - Mathf.Exp(-verticalResponse * delta));
        _viewForwardY = Mathf.Lerp(_viewForwardY, fishForwardY,
            1f - Mathf.Exp(-PitchPresentationResponse * delta));
    }

    private Vector3 DesiredPosition(out Vector3 lookTarget, out Vector3 anchor)
    {
        var actualPosition = _fish.GlobalPosition;
        var fishPosition = new Vector3(actualPosition.X, _followY, actualPosition.Z);
        var forward = -_fish.GlobalBasis.Z;
        forward.Y = _viewForwardY;
        forward = forward.Normalized();
        var horizontal = new Vector3(forward.X, 0, forward.Z);
        if (horizontal.LengthSquared() > 0.0001f) _lastHorizontalForward = horizontal.Normalized();
        var tuning = _fish.Tuning;
        var bodyScale = ViewScale;
        anchor = fishPosition + Vector3.Up * (0.35f * bodyScale);
        if (_fish.CameraControl == CameraControlPolicy.MouseOrbit360)
        {
            var orbitDistance = _zoomDistance * bodyScale;
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
            var adaptiveDistance = _zoomDistance * bodyScale;
            var adaptiveEyeHeight = bodyScale * (0.35f + (tuning.CameraHeight - 0.35f) *
                _zoomDistance / Mathf.Max(0.01f, tuning.CameraDistance) - forward.Y * _zoomDistance * PitchHeightFraction);
            var radius = Mathf.Sqrt(adaptiveDistance * adaptiveDistance + Mathf.Pow(adaptiveEyeHeight - 0.35f * bodyScale, 2));
            _lastCameraRear = _adaptiveRear;
            var normalTarget = fishPosition + forward * (tuning.CameraLookAhead * bodyScale) + Vector3.Up * (0.15f * bodyScale);
            lookTarget = anchor.Lerp(normalTarget, _adaptiveLookAhead);
            return anchor - _adaptiveRear * (radius * Mathf.Cos(_adaptiveElevation)) +
                Vector3.Up * (radius * Mathf.Sin(_adaptiveElevation));
        }
        lookTarget = fishPosition + forward * (tuning.CameraLookAhead * bodyScale) + Vector3.Up * (0.15f * bodyScale);
        var distance = _zoomDistance * bodyScale;
        var eyeHeight = bodyScale * (0.35f + (tuning.CameraHeight - 0.35f) *
            _zoomDistance / Mathf.Max(0.01f, tuning.CameraDistance) - forward.Y * _zoomDistance * PitchHeightFraction);
        var elevation = Mathf.Atan2(eyeHeight - 0.35f * bodyScale, distance) + _panPitch;
        var orbitRadius = Mathf.Sqrt(distance * distance +
            Mathf.Pow(eyeHeight - 0.35f * bodyScale, 2));
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
        var anchor = new Vector3(_fish.GlobalPosition.X, _followY, _fish.GlobalPosition.Z) +
            Vector3.Up * (0.35f * ViewScale);
        var fishForward = new Vector3(-_fish.GlobalBasis.Z.X, 0, -_fish.GlobalBasis.Z.Z);
        if (fishForward.LengthSquared() > 0.0001f) fishForward = fishForward.Normalized();
        else fishForward = _lastHorizontalForward;
        var tuning = _fish.Tuning;
        var distance = _zoomDistance * ViewScale;
        var forwardY = _viewForwardY;
        var eyeHeight = ViewScale * 0.35f + (tuning.CameraHeight - 0.35f) * ViewScale *
            _zoomDistance / Mathf.Max(0.01f, tuning.CameraDistance) - forwardY * distance * PitchHeightFraction;
        var normalElevation = Mathf.Atan2(eyeHeight - 0.35f * ViewScale, distance);
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
                pixels = Vector2.Zero; // Discard motion queued before Left Shift was held.
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
        var minimumDistance = MinimumDistance * ViewScale;
        if (safe.DistanceTo(anchor) >= minimumDistance) return safe;

        // A post close behind the fish can leave no usable view on the centerline.
        // Try either shoulder before accepting a camera position inside the body.
        var right = _lastCameraRear.Cross(Vector3.Up).Normalized();
        var best = safe;
        var bestTravel = float.PositiveInfinity;
        foreach (var side in new[] { 1.0f, -1.0f })
        {
            var shoulder = anchor + (-_lastCameraRear * 1.15f +
                Vector3.Up * 0.8f + right * (side * 1.1f)) * ViewScale;
            var clear = CastSafe(anchor, shoulder);
            if (clear.DistanceTo(anchor) < minimumDistance) continue;
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
            Margin = _larvaView ? .001f : .02f,
            Exclude = new Godot.Collections.Array<Rid> { _fish.GetRid() },
        };
        var proportions = GetWorld3D().DirectSpaceState.CastMotion(query);
        if (proportions.Length == 0 || proportions[0] >= 0.999f) return target;
        // A nearby wall can force less than the preferred minimum distance.
        var allowed = Mathf.Max(_larvaView ? .008f : .05f,
            length * proportions[0] - (_larvaView ? .008f : ObstacleClearance));
        return anchor + motion.Normalized() * Mathf.Min(allowed, length);
    }

    private Vector3 ResolveEncounterOverlap(Vector3 anchor, Vector3 candidate)
    {
        // Near the Bass-sized hollow, the cast can start inside a wood shell.
        // An initial overlap gives CastMotion no safe travel fraction, so verify
        // the final eye location against the actual cover shape as well.
        if (_fish.GetNode<CollisionShape3D>("Collision").Shape is not CapsuleShape3D ||
            !Overlaps(candidate)) return candidate;
        for (var step = 1; step <= 16; step++)
        {
            var towardFish = candidate.Lerp(anchor, step / 16f);
            if (!Overlaps(towardFish)) return towardFish;
        }
        var fallbackDistances = _larvaView
            ? new[] { .02f, .04f, .06f, .08f, .10f, .12f }
            : new[] { .2f, .4f, .6f, .8f, 1f, 1.2f };
        foreach (var distance in fallbackDistances)
            foreach (var direction in new[] { Vector3.Up, Vector3.Down, Vector3.Right,
                Vector3.Left, Vector3.Forward, Vector3.Back })
            {
                var nearby = anchor + direction * distance;
                if (!Overlaps(nearby)) return nearby;
            }
        return candidate;
    }

    private bool Overlaps(Vector3 point)
    {
        using var query = new PhysicsShapeQueryParameters3D
        {
            Shape = _probe,
            Transform = new Transform3D(Basis.Identity, point),
            CollisionMask = 1,
            Exclude = new Godot.Collections.Array<Rid> { _fish.GetRid() },
        };
        return GetWorld3D().DirectSpaceState.IntersectShape(query, 1).Count != 0;
    }

    private void FaceTarget(Vector3 target, Vector3 eye)
    {
        var toward = target - eye;
        if (toward.LengthSquared() < 0.0001f) return;
        var back = -toward.Normalized();
        var horizontal = new Vector3(toward.X, 0, toward.Z);
        var azimuthRight = horizontal.LengthSquared() > 0.0001f
            ? horizontal.Normalized().Cross(Vector3.Up)
            : _lastCameraRear.Cross(Vector3.Up);
        if (_fish.CameraControl is CameraControlPolicy.MouseOrbit360 or CameraControlPolicy.AdaptiveFollow)
        {
            var right = azimuthRight - back * azimuthRight.Dot(back);
            if (right.LengthSquared() < 0.0001f)
                right = back.Cross(Vector3.Up);
            if (right.LengthSquared() < 0.0001f)
                right = back.Cross(Vector3.Forward);
            right = right.Normalized();
            _camera.GlobalBasis = new Basis(right, back.Cross(right).Normalized(), back);
            return;
        }
        var levelRight = Vector3.Up.Cross(back);
        if (levelRight.LengthSquared() < 0.0001f) levelRight = azimuthRight;
        levelRight = levelRight.Normalized();
        _camera.GlobalBasis = new Basis(levelRight, back.Cross(levelRight).Normalized(), back);
    }
}
