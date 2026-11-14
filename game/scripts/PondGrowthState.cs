#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace Fish;

public readonly record struct BassBodyProfile(float Radius, float Height,
    float CenterZ, float VisualLength)
{
    public bool Contains(Vector3 point, float tolerance = 0f)
    {
        var halfAxis = Height * .5f - Radius;
        var relativeZ = point.Z - CenterZ;
        var nearestZ = Mathf.Clamp(relativeZ, -halfAxis, halfAxis);
        var offset = new Vector3(point.X, point.Y, relativeZ - nearestZ);
        return offset.LengthSquared() <= (Radius + tolerance) * (Radius + tolerance);
    }
}

/// <summary>One fixture Bass identity from egg through registered swimming stages.</summary>
public sealed class PondGrowthState
{
    public const string SpeciesId = PikeEncounterAssets.BassId;
    public static readonly IReadOnlyDictionary<string, int> Thresholds =
        new Dictionary<string, int> { ["larva"] = 0, ["juvenile"] = 6, ["subadult"] = 14, ["adult"] = 26 };
    private static readonly string[] Stages = { "egg", "larva", "juvenile", "subadult", "adult" };
    private readonly PondCreatureAsset[] _assets = new PondCreatureAsset[5];
    private readonly BassBodyProfile[] _bodies = new BassBodyProfile[5];
    private int _index;

    public string Stage => Stages[_index];
    public PondCreatureAsset Asset => _assets[_index];
    public float Length => Asset.Dimensions[1];
    public float ColliderRadius => _bodies[_index].Radius;
    public float ColliderLength => _bodies[_index].Height;
    public float ColliderCenterZ => _bodies[_index].CenterZ;
    public float VisualLength => _bodies[_index].VisualLength;
    public BassBodyProfile Body => _bodies[_index];
    public float BiteReach => Mathf.Clamp(VisualLength * 1.75f, .08f, 1.1f);
    public float MouthRadius => Mathf.Max(.012f, ColliderRadius * .55f);
    public string Identity { get; }
    public int NextThreshold => _index + 1 < Stages.Length ? Thresholds[Stages[_index + 1]] : int.MaxValue;

    public PondGrowthState(string identity, string startStage = "egg")
    {
        if (string.IsNullOrWhiteSpace(identity)) throw new ArgumentException("identity required", nameof(identity));
        Identity = identity;
        for (var i = 0; i < Stages.Length; i++)
        {
            if (!SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.PondId, SpeciesId,
                    Stages[i], SpeciesRole.Ecological, out var asset, out var diagnostic) || asset is null)
                throw new InvalidOperationException("BASS_GROWTH_STAGE_UNAVAILABLE " + diagnostic);
            _assets[i] = asset;
            if (i > 1 && _assets[i].Dimensions[1] <= _assets[i - 1].Dimensions[1])
                throw new InvalidOperationException("BASS_GROWTH_SIZE_ORDER_INVALID");
        }
        for (var i = 1; i < Stages.Length; i++)
            _bodies[i] = FitBody(_assets[i]);
        _index = Array.IndexOf(Stages, startStage);
        if (_index < 0) throw new ArgumentException("invalid Bass start stage", nameof(startStage));
    }

    public PondCreatureAsset? Next(int nutrition) => _index >= 1 && nutrition >= NextThreshold && _index + 1 < Stages.Length
        ? _assets[_index + 1] : null;

    public void Hatch()
    {
        if (_index != 0) throw new InvalidOperationException("BASS_HATCH_OUT_OF_ORDER");
        _index = 1;
    }

    public float RadiusForNext() => _bodies[_index + 1].Radius;
    public float LengthForNext() => _bodies[_index + 1].Height;
    public float CenterForNext() => _bodies[_index + 1].CenterZ;
    public float VisualLengthForNext() => _bodies[_index + 1].VisualLength;
    public BassBodyProfile BodyForNext() => _bodies[_index + 1];
    public BassBodyProfile BodyFor(string stage)
    {
        var index = Array.IndexOf(Stages, stage);
        if (index < 1) throw new ArgumentException("swimming Bass stage required", nameof(stage));
        return _bodies[index];
    }

    public SwimTuning StageTuning(SwimTuning baseline)
    {
        var tuning = (SwimTuning)baseline.Duplicate();
        var (speed, acceleration, coast, brake, reverse, reverseAcceleration,
            yaw, pitch, alignment) = Stage switch
        {
            "larva" => (.22f, .42f, .22f, .8f, .11f, .4f, 105f, 65f, 2.2f),
            "juvenile" => (.48f, .80f, .35f, 1.2f, .18f, .55f, 120f, 75f, 2.7f),
            "subadult" => (1.4f, 2.4f, .85f, 3.2f, .4f, 1.0f, 135f, 85f, 4.0f),
            "adult" => (1.55f, 2.6f, 1.0f, 4.0f, .5f, 1.2f, 150f, 95f, 4.5f),
            _ => throw new InvalidOperationException("BASS_EGG_CANNOT_SWIM"),
        };
        tuning.MaxSpeed = speed;
        tuning.Acceleration = acceleration;
        tuning.CoastDeceleration = coast;
        tuning.BrakeDeceleration = brake;
        tuning.ReverseMaxSpeed = reverse;
        tuning.ReverseAcceleration = reverseAcceleration;
        tuning.YawRateDegrees = yaw;
        tuning.PitchRateDegrees = pitch;
        tuning.VelocityAlignment = alignment;
        return tuning;
    }

    private BassBodyProfile FitBody(PondCreatureAsset asset)
    {
        var scene = ResourceLoader.Load<PackedScene>(asset.ModelPath)
            ?? throw new InvalidOperationException("BASS_BODY_MODEL_UNAVAILABLE");
        var visual = scene.Instantiate<Node3D>();
        try
        {
            visual.Transform = Entry(asset).PresentationTransform;
            var points = new List<(float z, float radial)>();
            var radialMaximum = 0f;
            foreach (var found in visual.FindChildren("*", "MeshInstance3D", true, false))
            {
                if (found is not MeshInstance3D { Mesh: not null } mesh) continue;
                var transform = mesh.Transform;
                var parent = mesh.GetParent();
                while (parent is Node3D node)
                {
                    transform = node.Transform * transform;
                    if (node == visual) break;
                    parent = node.GetParent();
                }
                for (var surface = 0; surface < mesh.Mesh.GetSurfaceCount(); surface++)
                {
                    var vertices = mesh.Mesh.SurfaceGetArrays(surface)
                        [(int)Mesh.ArrayType.Vertex].AsVector3Array();
                    foreach (var vertex in vertices)
                    {
                        var point = transform * vertex;
                        var radial = Mathf.Sqrt(point.X * point.X + point.Y * point.Y);
                        points.Add((point.Z, radial));
                        radialMaximum = Mathf.Max(radialMaximum, radial);
                    }
                }
            }
            if (points.Count == 0) throw new InvalidOperationException("BASS_BODY_VERTICES_UNAVAILABLE");
            // Rest-pose vertices set the envelope. A small skin-motion allowance
            // protects fin and tail tips during imported rotation-only clips.
            var radius = radialMaximum * 1.08f + .0005f;
            var left = float.PositiveInfinity;
            var right = float.NegativeInfinity;
            foreach (var (z, radial) in points)
            {
                var allowance = Mathf.Sqrt(Mathf.Max(0f, radius * radius - radial * radial));
                left = Mathf.Min(left, z + allowance);
                right = Mathf.Max(right, z - allowance);
            }
            if (right < left) right = left;
            var extra = Mathf.Max(.001f, asset.Dimensions[1] / _assets[^1].Dimensions[1] * .015f);
            return new BassBodyProfile(radius, right - left + 2f * radius + extra * 2f,
                (left + right) * .5f, asset.Dimensions[1] / _assets[^1].Dimensions[1]);
        }
        finally { visual.Free(); }
    }

    public void Commit(PondCreatureAsset next)
    {
        if (_index + 1 >= Stages.Length || !ReferenceEquals(_assets[_index + 1], next))
            throw new InvalidOperationException("BASS_GROWTH_OUT_OF_ORDER");
        _index++;
    }

    public CharacterEntry Entry(PondCreatureAsset asset) => new(SpeciesId, asset.Name,
        "Pond growth fixture", "Registered Bass stage", asset.ModelPath,
        1f / _assets[^1].Dimensions[1], asset.FacingDegrees, 30f,
        asset.Clip("Glide"), asset.Clip("Swim"), asset.Clip("SwimFast"),
        WorldHabitat.Freshwater, true, false, true, true);
}
