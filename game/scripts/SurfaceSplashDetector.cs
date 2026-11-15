#nullable enable
using Godot;
using System;

namespace Fish;

public enum SurfaceSplashEvent { None, Arrival, RapidDive }

/// <summary>Reads completed body motion against the moving Pond surface.</summary>
public sealed class SurfaceSplashDetector
{
    public float ContactRadiusFraction { get; set; } = .65f;
    public float RearmRadiusFraction { get; set; } = 1.5f;
    public float MinimumRearmDepth { get; set; } = .012f;
    public float ArrivalSpeedFraction { get; set; } = .12f;
    public float DiveSpeedFraction { get; set; } = .32f;
    public float MinimumArrivalSpeed { get; set; } = .035f;
    public float MinimumDiveSpeed { get; set; } = .07f;
    public float SubmergedRearmSeconds { get; set; } = .12f;
    public float ContactGraceSeconds { get; set; } = .22f;
    public float CooldownSeconds { get; set; } = .24f;

    private float? _previousDepth, _previousY;
    private float _submergedSeconds, _contactAge, _cooldown;
    private bool _arrivalArmed, _contacted, _divePlayed;

    public void Reset()
    {
        _previousDepth = _previousY = null;
        _submergedSeconds = _contactAge = _cooldown = 0f;
        _arrivalArmed = _contacted = _divePlayed = false;
    }

    public SurfaceSplashEvent Step(float bodyY, float surfaceY, float radius,
        float maximumSpeed, float delta, bool eligible, out float measuredSpeed)
    {
        measuredSpeed = 0f;
        if (!eligible || !float.IsFinite(bodyY) || !float.IsFinite(surfaceY) ||
            !float.IsFinite(radius) || radius <= 0f || !float.IsFinite(delta) || delta <= 0f)
        { Reset(); return SurfaceSplashEvent.None; }

        var depth = surfaceY - bodyY;
        var contact = ContactRadiusFraction * radius;
        var rearm = Mathf.Max(MinimumRearmDepth, RearmRadiusFraction * radius);
        _cooldown = Mathf.Max(0f, _cooldown - delta);
        if (_previousY is not { } previousY || _previousDepth is not { } previousDepth)
        {
            _previousY = bodyY;
            _previousDepth = depth;
            _contacted = depth <= contact;
            _contactAge = 0f;
            return SurfaceSplashEvent.None;
        }
        measuredSpeed = (bodyY - previousY) / delta;
        _previousY = bodyY;
        _previousDepth = depth;

        if (depth >= rearm)
        {
            _submergedSeconds += delta;
            if (_submergedSeconds >= SubmergedRearmSeconds)
            {
                _arrivalArmed = true;
                _contacted = false;
                _divePlayed = false;
            }
        }
        else _submergedSeconds = 0f;

        if (depth <= contact)
        {
            _contacted = true;
            _contactAge = 0f;
        }
        else _contactAge += delta;

        if (_arrivalArmed && previousDepth > contact && depth <= contact &&
            measuredSpeed >= Mathf.Max(MinimumArrivalSpeed,
                maximumSpeed * ArrivalSpeedFraction) && _cooldown <= 0f)
        {
            _arrivalArmed = false;
            _divePlayed = false;
            _cooldown = CooldownSeconds;
            return SurfaceSplashEvent.Arrival;
        }
        if (_contacted && !_divePlayed && _contactAge <= ContactGraceSeconds &&
            depth > previousDepth && measuredSpeed <= -Mathf.Max(MinimumDiveSpeed,
                maximumSpeed * DiveSpeedFraction) && _cooldown <= 0f)
        {
            _divePlayed = true;
            _cooldown = CooldownSeconds;
            return SurfaceSplashEvent.RapidDive;
        }
        return SurfaceSplashEvent.None;
    }
}
