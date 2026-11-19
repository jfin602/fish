#nullable enable
using Godot;
using System;

namespace Fish;

/// <summary>Physical fixture Pike. Steering sees only the awareness destination and solid geometry.</summary>
public partial class PikeActor : CharacterBody3D
{
    private readonly PikeAwareness _mind = new();
    private Vector3 _patrolOrigin;
    private float _speed;
    private Vector3 _steeringDirection = Vector3.Forward;
    private Vector3 _afterCatchGoal;
    public PikeAwareness Mind => _mind;
    public string StableId { get; private set; } = PikeEncounterAssets.PikeId;
    public Vector3 LastSteeringTarget { get; private set; }
    public float ActualSpeed => Velocity.Length();

    public Vector3 MouthPosition => GlobalPosition - GlobalBasis.Z * 1.32f;

    public bool MouthCanReach(FishController fish)
    {
        var collision = fish.GetNode<CollisionShape3D>("Collision");
        if (collision.Shape is not CapsuleShape3D capsule) return false;
        var forward = -GlobalBasis.Z;
        var mouth = MouthPosition;
        var local = collision.ToLocal(mouth);
        var spine = Mathf.Clamp(local.Y, -capsule.Height * .5f + capsule.Radius,
            capsule.Height * .5f - capsule.Radius);
        return local.DistanceTo(new Vector3(0f, spine, 0f)) <= capsule.Radius + .06f &&
            forward.Dot((fish.GlobalPosition - GlobalPosition).Normalized()) > .65f;
    }

    public bool MouthCanReach(PreyActor prey)
    {
        if (!prey.IsAlive) return false;
        var collision = prey.GetNode<CollisionShape3D>("Collision");
        var local = collision.ToLocal(MouthPosition);
        var reaches = collision.Shape switch
        {
            SphereShape3D sphere => local.Length() <= sphere.Radius + .13f,
            CapsuleShape3D capsule => local.DistanceTo(new Vector3(0,
                Mathf.Clamp(local.Y, -capsule.Height * .5f + capsule.Radius,
                    capsule.Height * .5f - capsule.Radius), 0)) <= capsule.Radius + .13f,
            _ => false,
        };
        var toward = prey.GlobalPosition - GlobalPosition;
        return reaches && toward.LengthSquared() > .000001f &&
            (-GlobalBasis.Z).Dot(toward.Normalized()) > .65f;
    }

    public void Configure(string? stableId = null)
    {
        StableId = stableId ?? PikeEncounterAssets.PikeId;
        _patrolOrigin = GlobalPosition;
        ResetActor();
    }

    public void ResetActor()
    {
        _mind.Reset();
        _speed = 0;
        _steeringDirection = -GlobalBasis.Z;
        Velocity = Vector3.Zero;
        LastSteeringTarget = _patrolOrigin;
        _afterCatchGoal = Vector3.Zero;
    }

    /// <summary>Continue the committed strike into open water after the Bass is caught.</summary>
    public void BeginAfterCatch()
    {
        var forward = -GlobalBasis.Z;
        var side = forward.Cross(Vector3.Up).Normalized();
        // Veer away from the held catch view, leaving the contact cloud in frame.
        _afterCatchGoal = GlobalPosition + side * 10f + forward * 3f;
        _afterCatchGoal = SubmergedGoal(_afterCatchGoal, 1.7f);
        LastSteeringTarget = _afterCatchGoal;
    }

    public void TickAfterCatch(float delta)
    {
        if (_mind.Mode != PikeMode.Caught || delta <= 0f) return;
        var to = _afterCatchGoal - GlobalPosition;
        if (to.LengthSquared() < .7f * .7f)
        {
            _speed = Mathf.MoveToward(_speed, 0f, 3f * delta);
            Velocity = -GlobalBasis.Z * _speed;
            MoveAndSlide();
            return;
        }
        var direction = ClearDirection(to.Normalized());
        _steeringDirection = _steeringDirection.Slerp(direction,
            1f - Mathf.Exp(-4f * delta)).Normalized();
        var targetBasis = Basis.LookingAt(_steeringDirection,
            Mathf.Abs(_steeringDirection.Dot(Vector3.Up)) > .95f ? Vector3.Forward : Vector3.Up);
        var current = GlobalBasis.GetRotationQuaternion();
        var desired = targetBasis.GetRotationQuaternion();
        var angle = current.AngleTo(desired);
        GlobalBasis = new Basis(current.Slerp(desired,
            angle < .0001f ? 1f : Mathf.Min(1f, Mathf.DegToRad(80f) * delta / angle)));
        _speed = Mathf.MoveToward(_speed, 2.3f, 3f * delta);
        Velocity = -GlobalBasis.Z * _speed;
        MoveAndSlide();
        _speed = Mathf.Min(_speed, Velocity.Length() + .5f);
    }

    public void Tick(SightEvidence sight, DisturbanceEvidence disturbance,
        double seconds, float delta, bool paused)
    {
        if (paused || delta <= 0) return;
        _mind.Step(GlobalPosition, sight, disturbance, seconds, false);
        if (_mind.Mode == PikeMode.Caught) { Velocity = Vector3.Zero; return; }
        var goal = _mind.Destination(seconds, _patrolOrigin);
        // Keep the body submerged and above the continuous mud bed. This is a
        // movement constraint, never a perception shortcut.
        goal = SubmergedGoal(goal, 1.5f);
        // Follow the rising bed before descending toward a deeper search point.
        goal.Y = Mathf.Min(PikeCoverFixture.WaterY - 1f,
            Mathf.Max(goal.Y,
                PikeCoverFixture.Bed(GlobalPosition.X, GlobalPosition.Z) + 1.7f));
        LastSteeringTarget = goal;
        var to = goal - GlobalPosition;
        if (to.Length() < .7f)
        {
            _speed = Mathf.MoveToward(_speed, 0f, 5f * delta);
            Velocity = -GlobalBasis.Z * _speed;
            MoveAndSlide();
            return;
        }
        var chosen = ClearDirection(to.Normalized());
        _steeringDirection = _steeringDirection.Slerp(chosen,
            1f - Mathf.Exp(-4f * delta)).Normalized();
        var targetBasis = Basis.LookingAt(_steeringDirection,
            Mathf.Abs(_steeringDirection.Dot(Vector3.Up)) > .95f ? Vector3.Forward : Vector3.Up);
        var current = GlobalBasis.GetRotationQuaternion();
        var desired = targetBasis.GetRotationQuaternion();
        var angle = current.AngleTo(desired);
        var fraction = angle < .0001f ? 1f : Mathf.Min(1f, Mathf.DegToRad(80f) * delta / angle);
        GlobalBasis = new Basis(current.Slerp(desired, fraction));
        _speed = Mathf.MoveToward(_speed, _mind.DesiredSpeed, 5.5f * delta);
        Velocity = -GlobalBasis.Z * _speed;
        MoveAndSlide();
        // Physics contact, including wood and bed, is authoritative. No direct position writes.
        _speed = Mathf.Min(_speed, Velocity.Length() + .5f);
    }

    private Vector3 SubmergedGoal(Vector3 requested, float bedClearance)
    {
        var upper = PikeCoverFixture.WaterY - 1f;
        // A large Pike cannot enter the shore's shallow water column. Keep its
        // pursuit physical by steering back toward deep patrol water.
        var goal = requested;
        if (PikeCoverFixture.Bed(goal.X, goal.Z) + bedClearance > upper)
            goal = _patrolOrigin;
        var lower = PikeCoverFixture.Bed(goal.X, goal.Z) + bedClearance;
        if (lower > upper)
            throw new InvalidOperationException("PIKE_PATROL_DEPTH_INVALID");
        goal.Y = Mathf.Clamp(goal.Y, lower, upper);
        return goal;
    }

    private Vector3 ClearDirection(Vector3 direct)
    {
        var ahead = GlobalPosition + direct * 1.2f;
        if (ahead.Y >= PikeCoverFixture.Bed(ahead.X, ahead.Z) + 1.5f &&
            ahead.Y <= PikeCoverFixture.WaterY - 1.0f &&
            !TestMove(GlobalTransform, direct * 1.2f)) return direct;
        var side = direct.Cross(Vector3.Up).Normalized();
        if (side.LengthSquared() < .01f) side = Vector3.Right;
        var candidates = new[]
        {
            (direct + side * .7f).Normalized(), (direct - side * .7f).Normalized(),
            (direct + Vector3.Up * .8f).Normalized(), (direct - Vector3.Up * .8f).Normalized(),
            (direct + side * 1.6f).Normalized(), (direct - side * 1.6f).Normalized(),
            (direct + side + Vector3.Up * .65f).Normalized(),
            (direct - side + Vector3.Up * .65f).Normalized(),
            (direct + side - Vector3.Up * .65f).Normalized(),
            (direct - side - Vector3.Up * .65f).Normalized(),
        };
        var best = -999f;
        var selected = direct;
        foreach (var direction in candidates)
        {
            var next = GlobalPosition + direction * 1.2f;
            if (next.Y < PikeCoverFixture.Bed(next.X, next.Z) + 1.5f ||
                next.Y > PikeCoverFixture.WaterY - 1.0f) continue;
            if (TestMove(GlobalTransform, direction * 1.2f)) continue;
            var score = .7f * direction.Dot(direct) + .3f * direction.Dot(_steeringDirection);
            if (score <= best) continue;
            best = score;
            selected = direction;
        }
        // If boxed in, turn toward a side. MoveAndSlide still prevents penetration.
        return best < -900f
            ? (GlobalPosition.Y < PikeCoverFixture.Bed(GlobalPosition.X, GlobalPosition.Z) + 1.7f
                ? Vector3.Up : side)
            : selected;
    }
}
