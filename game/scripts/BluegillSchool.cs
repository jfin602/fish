#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace Fish;

/// <summary>Small encounter-local group. Only members of this school are examined.</summary>
public sealed class BluegillSchool
{
    private readonly List<PreyActor> _members = new();
    private readonly Vector3 _center;
    private readonly float _phase;
    private Vector3? _attackPosition;
    private double _attackAt = double.NegativeInfinity;
    public string Id { get; }
    public Vector3 Center => _center;
    public IReadOnlyList<PreyActor> Members => _members;
    public int NeighborChecks { get; private set; }

    public BluegillSchool(string id, Vector3 center, float phase)
    { Id = id; _center = center; _phase = phase; }

    public void Add(PreyActor actor)
    {
        if (_members.Count >= 6) throw new InvalidOperationException("BLUEGILL_SCHOOL_CAP");
        _members.Add(actor);
        actor.SchoolId = Id;
    }

    public void ResetBudget() => NeighborChecks = 0;

    public void RaiseAlarm(Vector3 pikePosition, double seconds)
    {
        _attackPosition = pikePosition;
        _attackAt = seconds;
    }

    public SchoolSteering For(PreyActor actor, double seconds)
    {
        var position = actor.GlobalPosition;
        var separation = Vector3.Zero;
        var center = Vector3.Zero;
        var alignment = Vector3.Zero;
        Vector3? alarm = null;
        if (_attackPosition is { } attack && seconds - _attackAt is >= 0 and < 1.2 &&
            position.DistanceTo(attack) < 3.2f)
            alarm = attack;
        var count = 0;
        foreach (var member in _members)
        {
            if (member == actor || !member.IsAlive) continue;
            NeighborChecks++;
            var offset = position - member.GlobalPosition;
            var distance = offset.Length();
            if (distance > 3.2f) continue;
            count++;
            center += member.GlobalPosition;
            alignment += -member.GlobalBasis.Z;
            var clearance = .28f + (actor.Length + member.Length) * .65f;
            if (distance < clearance && distance > .001f)
                separation += offset / distance * (clearance - distance) / clearance;
            // An alarm spreads only to nearby members, and only while the
            // observing fish still remembers a recent, real sensory event.
            if (distance < 2.1f && member.Mind.LastSensedPosition is { } threat &&
                seconds - member.Mind.LastSensedAt is >= 0 and < .45)
                alarm = threat;
        }
        var route = _center + new Vector3(
            Mathf.Cos((float)seconds * .17f + _phase) * 1.1f,
            Mathf.Sin((float)seconds * .11f + _phase) * .25f,
            Mathf.Sin((float)seconds * .17f + _phase) * 1.1f);
        var routeDirection = (route - position).Normalized();
        var neighborDirection = count > 0
            ? (center / count - position).Normalized() * .65f +
              alignment.Normalized() * .25f + separation * 1.35f
            : Vector3.Zero;
        return new SchoolSteering(routeDirection * .55f + neighborDirection, alarm, count);
    }
}

public readonly record struct SchoolSteering(Vector3 Direction, Vector3? Alarm, int Neighbors);
