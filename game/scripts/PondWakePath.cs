#nullable enable
using Godot;
using System;

namespace Fish;

/// <summary>Fin origins and their trailing contacts on the local Pond interface.</summary>
public readonly record struct PondWakeContacts(Vector3 Bow, Vector3 Left, Vector3 Right,
    Vector3 Tail, Vector3 Heading, float SurfaceY, float Width, bool Touching)
{
    public Vector3 FinLeft { get; init; }
    public Vector3 FinRight { get; init; }
    public bool HasFinOrigins { get; init; }
    public bool UnderwaterTurn { get; init; }
    public const float NearSurfaceDepth = .2f;
    public const float NearSurfaceReleaseDepth = .25f;

    public static PondWakeContacts At(PlayerBodyGeometry body, Transform3D pose,
        Func<float, float, float> surfaceHeight, float depthLimit = NearSurfaceDepth,
        Vector3? leftFinLocal = null, Vector3? rightFinLocal = null)
    {
        // FishController faces local -Z. The measured fin rim is already in
        // fish coordinates and follows the current fish pose and growth stage.
        var noseZ = body.TailZ - body.Length;
        var bow = pose * new Vector3(0f, body.TopReach * .35f, noseZ + body.Length * .48f);
        var finLeft = pose * (leftFinLocal ?? body.LeftFin);
        var finRight = pose * (rightFinLocal ?? body.RightFin);
        var tail = pose * new Vector3(0f, body.TopReach * .18f,
            body.TailZ - body.Length * .08f);
        var heading = pose.Basis * Vector3.Forward;
        heading.Y = 0f;
        heading = heading.LengthSquared() > .000001f ? heading.Normalized() : Vector3.Forward;
        var rear = -heading * body.Length * .12f;
        var left = finLeft + rear;
        var right = finRight + rear;
        var halfWidth = Mathf.Max(new Vector2(left.X - right.X, left.Z - right.Z).Length() * .5f,
            .002f);
        var y = surfaceHeight((left.X + right.X) * .5f, (left.Z + right.Z) * .5f);
        var centerY = surfaceHeight(pose.Origin.X, pose.Origin.Z);
        // The moving surface still carries a pressure wake during shallow dips.
        // Measure from the fish origin so the 0.2 m band has the same meaning
        // for every growth stage, while retaining the exposed-body cutoff.
        var depth = centerY - pose.Origin.Y;
        var touching = float.IsFinite(y) && float.IsFinite(centerY) &&
            depth <= depthLimit + .0001f && depth >= -body.Radius * .20f;
        return new PondWakeContacts(
            new Vector3(bow.X, surfaceHeight(bow.X, bow.Z), bow.Z),
            left,
            right,
            new Vector3(tail.X, surfaceHeight(tail.X, tail.Z), tail.Z),
            heading, y, halfWidth * 2f, touching)
        { FinLeft = finLeft, FinRight = finRight, HasFinOrigins = true };
    }
}

public readonly record struct PondWakeSample(Vector2 Center, Vector2 Heading,
    Vector2 Left, Vector2 Right, float BirthSeconds, float Length, float Width,
    float Energy, int StageRevision, int Segment)
{
    public float LeftY { get; init; }
    public float RightY { get; init; }
    public bool HasFinDepth { get; init; }
    public bool UnderwaterTurn { get; init; }
}

/// <summary>Fixed storage for historical world paths. Time advances only with active physics.</summary>
public sealed class PondWakePath
{
    public const int Capacity = 48;
    public const float Lifetime = 2.2f;
    private readonly PondWakeSample[] _samples = new PondWakeSample[Capacity];
    private int _start;
    private int _count;
    private Vector2? _lastObserved;
    private float _lastBirth;
    private float _travelSinceBirth;
    private int _segment;
    private bool? _lastUnderwaterTurn;
    public float Seconds { get; private set; }
    public int Count => _count;
    public int Segment => _segment;
    public PondWakeSample this[int index] => index >= 0 && index < _count
        ? _samples[(_start + index) % Capacity] : throw new ArgumentOutOfRangeException(nameof(index));

    public void Clear()
    {
        _start = _count = 0;
        _lastObserved = null;
        _lastBirth = Seconds;
        _travelSinceBirth = 0f;
        _lastUnderwaterTurn = null;
        _segment++;
    }

    public void Suspend() => Clear();

    public bool Step(PondWakeContacts contact, float length, float energy,
        int stageRevision, float delta)
    {
        if (!float.IsFinite(delta) || delta <= 0f) return false;
        if (delta > .25f) { Clear(); return false; } // no pause/focus backlog
        Seconds += delta;
        while (_count > 0 && Seconds - this[0].BirthSeconds > Lifetime)
        { _start = (_start + 1) % Capacity; _count--; }
        var center = new Vector2((contact.Left.X + contact.Right.X) * .5f,
            (contact.Left.Z + contact.Right.Z) * .5f);
        if (!float.IsFinite(center.X) || !float.IsFinite(center.Y) ||
            !float.IsFinite(length) || length <= 0f || !float.IsFinite(energy) ||
            !contact.Touching || energy < .015f)
        {
            if (_lastObserved is not null) _segment++;
            _lastObserved = null;
            _lastUnderwaterTurn = null;
            _travelSinceBirth = 0f;
            _lastBirth = Seconds;
            return false;
        }
        if (_count > 0 && this[_count - 1].StageRevision != stageRevision) Clear();
        if (_lastUnderwaterTurn is { } priorMode && priorMode != contact.UnderwaterTurn)
        {
            _lastObserved = null;
            _travelSinceBirth = 0f;
            _segment++;
        }
        _lastUnderwaterTurn = contact.UnderwaterTurn;
        if (_lastObserved is not { } previous)
        { _lastObserved = center; _lastBirth = Seconds; return false; }
        var displacement = center.DistanceTo(previous);
        if (displacement > Mathf.Max(length * 2.5f, .45f))
        { Clear(); _lastObserved = center; return false; }
        _lastObserved = center;
        _travelSinceBirth += displacement;
        var minStep = Mathf.Max(length * .10f, .004f);
        if (_travelSinceBirth < minStep &&
            (Seconds - _lastBirth < .10f || _travelSinceBirth < minStep * .25f)) return false;
        if (Seconds - _lastBirth < .025f) return false;
        var direction = new Vector2(contact.Heading.X, contact.Heading.Z).Normalized();
        if (direction.LengthSquared() < .9f) return false;
        var sample = new PondWakeSample(center, direction,
            new Vector2(contact.Left.X, contact.Left.Z),
            new Vector2(contact.Right.X, contact.Right.Z), Seconds, length,
            contact.Width, Mathf.Clamp(energy, 0f, 1f), stageRevision, _segment)
        { LeftY = contact.FinLeft.Y, RightY = contact.FinRight.Y,
            HasFinDepth = contact.HasFinOrigins,
            UnderwaterTurn = contact.UnderwaterTurn };
        if (_count == Capacity) _start = (_start + 1) % Capacity;
        else _count++;
        _samples[(_start + _count - 1) % Capacity] = sample;
        _lastBirth = Seconds;
        _travelSinceBirth = 0f;
        return true;
    }
}
