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

    private FishController _fish = null!;
    private Camera3D _camera = null!;
    private SphereShape3D _probe = null!;
    private Vector3 _previousFishPosition;
    private Vector3 _lastHorizontalForward = Vector3.Forward;
    private Vector3 _lastCameraRear = Vector3.Forward;
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

    public void ResetFollow() => _resetPending = true;

    public void ResetZoom() => _zoomDistance = _fish.Tuning.CameraDistance;

    public void ClearLookOffset()
    {
        _panYaw = 0;
        _panPitch = 0;
        _resetPending = true;
    }

    public override void _Input(InputEvent @event)
    {
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
            _panYaw = 0;
            _panPitch = 0;
            _resetPending = true;
            return;
        }

        UpdateMouseCamera((float)delta);

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

    private void UpdateMouseCamera(float delta)
    {
        if (!_fish.CanUseMouseCamera)
        {
            _panYaw = 0;
            _panPitch = 0;
            return;
        }

        var tuning = _fish.Tuning;
        var yaw = (Input.IsKeyPressed(Key.D) ? 1 : 0) - (Input.IsKeyPressed(Key.A) ? 1 : 0);
        var pitch = (Input.IsKeyPressed(Key.W) ? 1 : 0) - (Input.IsKeyPressed(Key.S) ? 1 : 0);
        var hold = Input.IsKeyPressed(Key.Shift);
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
        LookAt(target, Vector3.Up);
    }
}
