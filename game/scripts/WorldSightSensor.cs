#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace Fish;

/// <summary>World inputs only. The fixture has its own water column; baked Pond uses its seeded field.</summary>
public sealed class SightEnvironment
{
    public WorldEnvironmentProfile Habitat { get; }
    public float SurfaceY { get; }
    public Func<Vector3, float> Sediment { get; }
    public SolarState Solar { get; }

    public SightEnvironment(WorldEnvironmentProfile habitat, float surfaceY,
        Func<Vector3, float> sediment, SolarState solar)
    {
        Habitat = habitat;
        SurfaceY = surfaceY;
        Sediment = sediment;
        Solar = solar;
    }

    public static SightEnvironment ForBakedWorld(GeneratedBasin basin, float surfaceY, SolarState solar)
    {
        if (basin.IsPond)
        {
            var field = new PondTurbidityField(basin, surfaceY);
            return new SightEnvironment(WorldEnvironmentProfile.Pond, surfaceY, field.Sample, solar);
        }
        return new SightEnvironment(WorldEnvironmentProfile.Ocean, surfaceY, _ => 0f, solar);
    }
}

/// <summary>Bounds computed from visible plant meshes, never an authored safe-zone volume.</summary>
public readonly record struct SoftSightCover(string FafId, Aabb VisibleBounds);
public readonly record struct SightTarget(Vector3 Position, float Radius, float Contrast);
public readonly record struct SightEvidence(bool Detected, float Strength, string Reason,
    float Water, float Light, float SoftCover, int ClearRays, int Rays, int WaterSamples,
    Vector3? SensedPosition = null, double SensedAt = 0);

/// <summary>One observer/target pair. At most three physics rays, seven water samples and 128 mesh bounds per update.</summary>
public sealed class WorldSightSensor
{
    public const float Range = 30f;
    public const double SampleIntervalSeconds = .2;
    private const float Threshold = .065f;
    private readonly PhysicsDirectSpaceState3D _space;
    private readonly IReadOnlyList<SoftSightCover> _plants;
    private readonly Godot.Collections.Array<Rid> _exclude;
    private readonly float _range;
    private readonly float _acuity;
    private double _lastTime = double.NegativeInfinity;
    private SightEvidence _last;

    public WorldSightSensor(PhysicsDirectSpaceState3D space, IReadOnlyList<SoftSightCover> plants,
        Rid observer, Rid target, float range = Range, float acuity = 1f)
    {
        if (plants.Count > 128) throw new ArgumentOutOfRangeException(nameof(plants));
        if (!float.IsFinite(range) || range <= 0 || !float.IsFinite(acuity) || acuity <= 0)
            throw new ArgumentOutOfRangeException(nameof(range));
        _space = space;
        _plants = plants;
        _exclude = new Godot.Collections.Array<Rid> { observer, target };
        _range = range;
        _acuity = acuity;
    }

    public void Reset()
    {
        _lastTime = double.NegativeInfinity;
        _last = default;
    }

    public SightEvidence Observe(Vector3 observer, Vector3 heading, SightTarget? target,
        SightEnvironment environment, double gameSeconds, bool paused)
    {
        if (paused) { Reset(); return Empty("paused"); }
        if (target is not { } fish || !Finite(observer) || !Finite(heading) ||
            !Finite(fish.Position) || !float.IsFinite(fish.Radius) || fish.Radius <= 0 ||
            !float.IsFinite(fish.Contrast) || fish.Contrast <= 0 || heading.LengthSquared() < .01f ||
            !double.IsFinite(gameSeconds)) { Reset(); return Empty("invalid-target"); }
        if (environment.Habitat is not (WorldEnvironmentProfile.Ocean or WorldEnvironmentProfile.Pond) ||
            !float.IsFinite(environment.SurfaceY)) { Reset(); return Empty("unknown-habitat"); }
        if (gameSeconds >= _lastTime && gameSeconds - _lastTime < SampleIntervalSeconds)
            return _last;
        if (gameSeconds < _lastTime) Reset();
        _lastTime = gameSeconds;
        var to = fish.Position - observer;
        var distance = to.Length();
        if (distance < .01f || distance > _range) return _last = Empty("range");
        var direction = to / distance;
        var facing = heading.Normalized().Dot(direction);
        if (facing < -.15f) return _last = Empty("orientation");
        var angleFactor = Mathf.Clamp((facing + .15f) / .7f, 0f, 1f);
        var side = direction.Cross(Vector3.Up).Normalized();
        if (side.LengthSquared() < .01f) side = Vector3.Right;
        var offsets = new[] { Vector3.Zero, side * fish.Radius * .65f, Vector3.Up * fish.Radius * .65f };
        var clear = 0;
        var cover = 0f;
        foreach (var offset in offsets)
        {
            var end = fish.Position + offset;
            using var ray = PhysicsRayQueryParameters3D.Create(observer, end, 1, _exclude);
            if (_space.IntersectRay(ray).Count > 0) continue;
            clear++;
            cover += SoftCoverTransmission.Along(observer, end, _plants, .2f);
        }
        if (clear == 0) return _last = new SightEvidence(false, 0f, "hard-cover", 0f, 0f, 0f, 0, 3, 0);
        var sediment = 0f;
        const int waterSamples = 7;
        for (var i = 0; i < waterSamples; i++)
        {
            var sample = environment.Sediment(observer.Lerp(fish.Position, (i + .5f) / waterSamples));
            if (!float.IsFinite(sample)) return _last = Empty("invalid-water");
            sediment += Mathf.Clamp(sample, 0f, 1f) / waterSamples;
        }
        // Sediment affects transmission only. Solar illumination is independent of shadow/fog settings.
        var water = Mathf.Exp(-distance * (.025f + .22f * sediment));
        var depth = Mathf.Max(0f, environment.SurfaceY - fish.Position.Y);
        var light = (.24f + .76f * environment.Solar.AmbientLight) *
            Mathf.Exp(-depth * (environment.Habitat == WorldEnvironmentProfile.Pond ? .045f : .025f));
        var silhouette = Mathf.Clamp(fish.Radius * 5f / distance, .06f, 1f);
        var soft = cover / 3f;
        var score = _acuity * angleFactor * water * light * silhouette *
            Mathf.Clamp(fish.Contrast, 0f, 1f) * soft;
        return _last = new SightEvidence(score >= Threshold, score,
            score >= Threshold ? "visible" : "weak-evidence", water, light, soft,
            clear, 3, waterSamples, score >= Threshold ? fish.Position : null, gameSeconds);
    }

    private static SightEvidence Empty(string reason) => new(false, 0f, reason, 0f, 0f, 0f, 0, 0, 0);
    private static bool Finite(Vector3 v) => float.IsFinite(v.X) && float.IsFinite(v.Y) && float.IsFinite(v.Z);

}

/// <summary>Visible foliage reduces evidence along a line without declaring a safe zone.</summary>
public static class SoftCoverTransmission
{
    public static float Along(Vector3 from, Vector3 to, IReadOnlyList<SoftSightCover> plants,
        float minimum)
    {
        var length = 0f;
        foreach (var plant in plants) length += SegmentBoxLength(from, to, plant.VisibleBounds);
        return Mathf.Max(Mathf.Clamp(minimum, 0f, 1f), Mathf.Exp(-.8f * Mathf.Min(length, 12f)));
    }

    private static float SegmentBoxLength(Vector3 from, Vector3 to, Aabb box)
    {
        var direction = to - from;
        var start = 0f;
        var end = 1f;
        for (var axis = 0; axis < 3; axis++)
        {
            var origin = from[axis];
            var delta = direction[axis];
            var low = box.Position[axis];
            var high = box.End[axis];
            if (Mathf.Abs(delta) < 1e-6f)
            {
                if (origin < low || origin > high) return 0f;
                continue;
            }
            var a = (low - origin) / delta;
            var b = (high - origin) / delta;
            start = Mathf.Max(start, Mathf.Min(a, b));
            end = Mathf.Min(end, Mathf.Max(a, b));
            if (end <= start) return 0f;
        }
        return (end - start) * direction.Length();
    }
}
