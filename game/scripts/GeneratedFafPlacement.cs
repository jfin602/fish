#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

public enum HabitatRole
{
    CoralGarden, GrassMeadow, SandyClearing, RockySlope,
    CaveEntrance, ShelteredPocket, DropOffReef, DeepRubble,
}

public sealed record GeneratedFafInstance(FafEnvironmentEntry Asset, HabitatRole Role,
    string DepthBand, Vector3 Position, Vector3 Normal, float Yaw, float Scale,
    float Radius, float FootprintDelta, float Suitability)
{
    public Transform3D Transform
    {
        get
        {
            var up = Normal.Slerp(Vector3.Up, Asset.Family == FafEnvironmentFamily.Terrain ? .75f : .35f).Normalized();
            var forward = new Vector3(Mathf.Sin(Yaw), 0f, Mathf.Cos(Yaw));
            var right = up.Cross(forward).Normalized();
            forward = right.Cross(up).Normalized();
            return new Transform3D(new Basis(right, up, forward).Scaled(Vector3.One * Scale), Position);
        }
    }
}

public sealed record WorldPlacementScore(
    IReadOnlyDictionary<string, int> ByRole, IReadOnlyDictionary<string, int> ByDepth,
    IReadOnlyDictionary<string, int> ByFamily, IReadOnlyDictionary<string, int> LivingByDepth,
    int VariantCount, int SupportFailuresBefore,
    int SupportFailuresAfter, int RouteFailuresBefore, int RouteFailuresAfter,
    int IntersectionsBefore, int IntersectionsAfter, int RepeatedOrientations,
    float MinimumSpacing, int DensitySpikes, int Repaired, int Removed,
    int EstimatedInstances, int EstimatedTriangles, int EstimatedBatches)
{
    public override string ToString() =>
        $"variants={VariantCount} roles={string.Join(',', ByRole.Select(x => x.Key + ':' + x.Value))} " +
        $"depth={string.Join(',', ByDepth.Select(x => x.Key + ':' + x.Value))} " +
        $"family={string.Join(',', ByFamily.Select(x => x.Key + ':' + x.Value))} " +
        $"living_depth={string.Join(',', LivingByDepth.Select(x => x.Key + ':' + x.Value))} " +
        $"support={SupportFailuresBefore}/{SupportFailuresAfter} route={RouteFailuresBefore}/{RouteFailuresAfter} " +
        $"intersections={IntersectionsBefore}/{IntersectionsAfter} repeat_yaw={RepeatedOrientations} " +
        $"min_spacing={MinimumSpacing:F2} density_spikes={DensitySpikes} repaired={Repaired} removed={Removed} " +
        $"instances={EstimatedInstances} triangles_est={EstimatedTriangles} batches_est={EstimatedBatches}";
}

/// <summary>Seeded habitat anchors, dart throwing and bounded local repair over actual mesh projections.</summary>
public sealed class GeneratedFafPlacement
{
    private readonly record struct Anchor(HabitatRole Role, Vector3 Position, float Score);
    private readonly WorldEnvironmentAnalysis _environment;
    private readonly GeneratedWorldRoutes _routes;
    private readonly WorldCompileContext _context;
    private readonly List<GeneratedFafInstance> _placed = new();
    private readonly List<Anchor> _anchors = new();
    private int _supportRejected, _routeRejected, _spacingRejected, _repaired, _removed;

    public IReadOnlyList<GeneratedFafInstance> Instances => _placed;
    public int AnchorCount => _anchors.Count;
    public WorldPlacementScore Score { get; private set; } = null!;

    public GeneratedFafPlacement(WorldEnvironmentAnalysis environment, GeneratedWorldRoutes routes, ulong seed)
    {
        _environment = environment;
        _routes = routes;
        _context = new WorldCompileContext(new WorldGenerationSettings(seed));
    }

    public bool Build(out string diagnostic)
    {
        if (_placed.Count != 0) { diagnostic = "WORLD_PLACEMENT_INVALID action=single-build"; return false; }
        SelectAnchors();
        if (_anchors.Count < 12) { diagnostic = $"WORLD_PLACEMENT_INVALID anchors={_anchors.Count}"; return false; }
        foreach (var asset in FafRuntimeCatalog.Environment)
        {
            var quota = asset.Family switch
            {
                FafEnvironmentFamily.Terrain => 1,
                FafEnvironmentFamily.Rock => 3,
                FafEnvironmentFamily.Coral => 4,
                FafEnvironmentFamily.Plant => 6,
                _ => 3,
            };
            var random = _context.Stream("faf-placement", asset.Id);
            for (var i = 0; i < quota; i++)
            {
                if (FindCandidate(asset, ref random, -1, out var instance)) _placed.Add(instance);
                else if (i == 0)
                {
                    diagnostic = $"WORLD_PLACEMENT_INVALID id={asset.Id} action=no-supported-site anchors={string.Join(',', _anchors.Where(a => a.Role == HabitatRole.SandyClearing).Select(a => a.Position))} support={_supportRejected} route={_routeRejected} spacing={_spacingRejected}";
                    return false;
                }
            }
        }
        Relax();
        Score = Measure();
        if (Score.VariantCount != FafRuntimeCatalog.Environment.Count ||
            Score.SupportFailuresAfter != 0 || Score.RouteFailuresAfter != 0 ||
            Score.IntersectionsAfter != 0 || Score.DensitySpikes != 0)
        {
            diagnostic = $"WORLD_PLACEMENT_INVALID {Score}";
            return false;
        }
        diagnostic = "";
        return true;
    }

    private void SelectAnchors()
    {
        foreach (var role in Enum.GetValues<HabitatRole>())
        {
            var random = _context.Stream("habitat-anchors", role.ToString());
            var candidates = new List<Anchor>();
            for (var z = 3; z < WorldEnvironmentAnalysis.CellsPerSide - 3; z++)
            for (var x = 3; x < WorldEnvironmentAnalysis.CellsPerSide - 3; x++)
            {
                var field = _environment.SampleGrid(x, z);
                if (field.Depth < 2f || field.Depth > 24f || field.PerimeterDistance < 4f ||
                    field.Normal.Y < .75f ||
                    field.RouteDistance < 3.5f || field.EntranceDistance < 3.2f) continue;
                var suitability = RoleScore(role, field);
                if (suitability < .25f) continue;
                candidates.Add(new Anchor(role, field.Surface, suitability + NextFloat(ref random) * .16f));
            }
            foreach (var candidate in candidates.OrderByDescending(x => x.Score))
            {
                if (_anchors.Any(a => new Vector2(a.Position.X, a.Position.Z).DistanceTo(
                        new Vector2(candidate.Position.X, candidate.Position.Z)) <
                        (a.Role == role ? 6f : 3.5f))) continue;
                _anchors.Add(candidate);
                if (_anchors.Count(a => a.Role == role) == 6) break;
            }
        }
    }

    private bool FindCandidate(FafEnvironmentEntry asset, ref StableWorldRandom random,
        int replacing, out GeneratedFafInstance result)
    {
        result = null!;
        var best = float.NegativeInfinity;
        var usable = _anchors.Where(a => RoleAllows(asset, a.Role)).ToArray();
        if (usable.Length == 0) return false;
        for (var attempt = 0; attempt < 96; attempt++)
        {
            var anchor = usable[(int)(random.Next() % (ulong)usable.Length)];
            var theta = NextFloat(ref random) * Mathf.Tau;
            var distance = Mathf.Sqrt(NextFloat(ref random)) *
                (asset.Family == FafEnvironmentFamily.Terrain ? 2f : 5.5f);
            var x = anchor.Position.X + Mathf.Cos(theta) * distance;
            var z = anchor.Position.Z + Mathf.Sin(theta) * distance;
            if (Mathf.Abs(x) > 36f || Mathf.Abs(z) > 36f) continue;
            var projected = _environment.SampleProjected(new Vector3(x, 45f, z), Vector3.Down, 70f);
            var field = projected.Fields;
            if (field.Depth < 1.8f || field.Depth > 24f || field.PerimeterDistance < 2.5f ||
                field.Normal.Y < .72f) continue;
            var suitability = RoleScore(anchor.Role, field) * .5f + FamilyScore(asset, field) * .5f;
            if (suitability < .25f) continue;
            var scale = asset.Family switch
            {
                FafEnvironmentFamily.Terrain => .34f + .09f * NextFloat(ref random),
                FafEnvironmentFamily.Rock => .88f + .22f * NextFloat(ref random),
                FafEnvironmentFamily.Detail => 1.05f + .25f * NextFloat(ref random),
                _ => .84f + .3f * NextFloat(ref random),
            };
            var radius = .5f * Mathf.Max(asset.SizeMeters.X, asset.SizeMeters.Z) * scale;
            var yaw = NextFloat(ref random) * Mathf.Tau;
            var height = asset.SizeMeters.Y * scale;
            var embed = asset.Family switch
            {
                FafEnvironmentFamily.Terrain => .16f,
                FafEnvironmentFamily.Rock => Mathf.Min(.19f, height * .18f),
                _ => .015f,
            };
            var footprint = FootprintDelta(field.Surface, radius * .75f, yaw,
                out var supported, out var lowest);
            var tolerance = asset.Family switch
            {
                FafEnvironmentFamily.Terrain => 1.2f,
                FafEnvironmentFamily.Rock => .42f,
                _ => .33f,
            };
            if (!supported || footprint > tolerance)
            { _supportRejected++; continue; }
            var position = new Vector3(x,
                (asset.Family == FafEnvironmentFamily.Terrain ? field.Surface.Y : lowest) - embed, z);
            if (RouteViolation(asset, position, radius, scale, yaw) ||
                field.EntranceDistance < radius + 2.4f)
            { _routeRejected++; continue; }
            var tooClose = false;
            var nearCount = 0;
            foreach (var (other, index) in _placed.Select((p, i) => (p, i)))
            {
                if (index == replacing) continue;
                var gap = new Vector2(position.X - other.Position.X, position.Z - other.Position.Z).Length();
                var minimum = Mathf.Max(.32f, (radius + other.Radius) * .78f);
                if (gap < minimum) { tooClose = true; break; }
                if (gap < 3f) nearCount++;
            }
            if (tooClose || nearCount > 7) { _spacingRejected++; continue; }
            suitability -= nearCount * .028f;
            suitability += NextFloat(ref random) * .08f;
            if (suitability <= best) continue;
            best = suitability;
            result = new GeneratedFafInstance(asset, anchor.Role, DepthBand(field.Depth),
                position, field.Normal, yaw, scale, radius, footprint, suitability);
        }
        return best > float.NegativeInfinity;
    }

    private float FootprintDelta(Vector3 center, float radius, float yaw,
        out bool supported, out float lowest)
    {
        var max = 0f;
        supported = true;
        lowest = center.Y;
        for (var i = 0; i < 4; i++)
        {
            var angle = yaw + i * Mathf.Pi * .5f;
            var x = center.X + Mathf.Cos(angle) * radius;
            var z = center.Z + Mathf.Sin(angle) * radius;
            var hit = _environment.ProjectDown(new Vector3(x, 45f, z));
            if (!float.IsFinite(hit.Distance) || hit.Normal.Y < .65f)
            { supported = false; break; }
            max = Mathf.Max(max, Mathf.Abs(hit.Position.Y - center.Y));
            lowest = Mathf.Min(lowest, hit.Position.Y);
        }
        return max;
    }

    private bool RouteViolation(FafEnvironmentEntry asset, Vector3 position,
        float radius, float scale, float yaw)
    {
        var top = position + Vector3.Up * asset.SizeMeters.Y * scale;
        if (asset.Family == FafEnvironmentFamily.Terrain)
            top.Y = _environment.ProjectDown(new Vector3(position.X, 45f, position.Z)).Position.Y +
                asset.SizeMeters.Y * scale - .08f;
        var clearance = asset.Family == FafEnvironmentFamily.Terrain ? .12f : .22f;
        if (_routes.IsProtected(top, clearance)) return true;
        if (asset.Family is not (FafEnvironmentFamily.Terrain or FafEnvironmentFamily.Rock)) return false;
        for (var i = 0; i < 4; i++)
        {
            var angle = yaw + i * Mathf.Pi * .5f;
            var at = top + new Vector3(Mathf.Cos(angle) * radius * .75f, 0f,
                Mathf.Sin(angle) * radius * .75f);
            if (asset.Family == FafEnvironmentFamily.Terrain)
                at.Y = _environment.ProjectDown(new Vector3(at.X, 45f, at.Z)).Position.Y +
                    asset.SizeMeters.Y * scale - .08f;
            if (_routes.IsProtected(at, clearance)) return true;
        }
        return false;
    }

    private void Relax()
    {
        // Two bounded local repair sweeps. Major overlaps, density spikes and route
        // violations move to another suitable dart; a failed move removes the piece.
        for (var pass = 0; pass < 2; pass++)
        for (var i = _placed.Count - 1; i >= 0; i--)
        {
            var piece = _placed[i];
            var invalid = RouteViolation(piece.Asset, piece.Position, piece.Radius, piece.Scale, piece.Yaw) ||
                _placed.Where((_, index) => index != i).Any(other =>
                    new Vector2(piece.Position.X - other.Position.X, piece.Position.Z - other.Position.Z).Length() <
                    Mathf.Max(.32f, (piece.Radius + other.Radius) * .78f));
            var local = _placed.Count(other =>
                other != piece && new Vector2(piece.Position.X - other.Position.X,
                    piece.Position.Z - other.Position.Z).Length() < 3f);
            if (!invalid && local <= 7) continue;
            var random = _context.Stream("faf-repair", piece.Asset.Id + ":" + i + ":" + pass);
            if (FindCandidate(piece.Asset, ref random, i, out var replacement))
            { _placed[i] = replacement; _repaired++; }
            else { _placed.RemoveAt(i); _removed++; }
        }
        // A repeated nearby azimuth is noticeable even when the footprints clear.
        for (var i = 0; i < _placed.Count; i++)
        for (var j = 0; j < i; j++)
        {
            var a = _placed[i]; var b = _placed[j];
            if (a.Asset.Id != b.Asset.Id || a.Position.DistanceTo(b.Position) > 5f ||
                Mathf.Abs(Mathf.Wrap(a.Yaw - b.Yaw, -Mathf.Pi, Mathf.Pi)) > .2f) continue;
            _placed[i] = a with { Yaw = Mathf.PosMod(a.Yaw + .71f, Mathf.Tau) };
            _repaired++;
        }
    }

    private WorldPlacementScore Measure()
    {
        var roles = Enum.GetValues<HabitatRole>().ToDictionary(x => x.ToString(), x => _placed.Count(p => p.Role == x));
        var depths = new[] { "shallow", "upper", "middle", "lower", "deep" }
            .ToDictionary(x => x, x => _placed.Count(p => p.DepthBand == x));
        var families = Enum.GetValues<FafEnvironmentFamily>()
            .ToDictionary(x => x.ToString(), x => _placed.Count(p => p.Asset.Family == x));
        var living = depths.Keys.ToDictionary(x => x, x => _placed.Count(p => p.DepthBand == x &&
            (p.Asset.Family is FafEnvironmentFamily.Coral or FafEnvironmentFamily.Plant)));
        var support = 0; var routes = 0; var overlaps = 0; var repeat = 0; var spikes = 0;
        var minimum = float.PositiveInfinity;
        for (var i = 0; i < _placed.Count; i++)
        {
            var a = _placed[i];
            if (a.FootprintDelta > (a.Asset.Family == FafEnvironmentFamily.Terrain ? 1.2f :
                a.Asset.Family == FafEnvironmentFamily.Rock ? .42f : .33f)) support++;
            if (RouteViolation(a.Asset, a.Position, a.Radius, a.Scale, a.Yaw)) routes++;
            var near = 0;
            for (var j = 0; j < _placed.Count; j++)
            {
                if (i == j) continue;
                var b = _placed[j];
                var distance = new Vector2(a.Position.X - b.Position.X, a.Position.Z - b.Position.Z).Length();
                if (distance < 3f) near++;
                if (j >= i) continue;
                minimum = Mathf.Min(minimum, distance);
                if (distance < Mathf.Max(.32f, (a.Radius + b.Radius) * .78f)) overlaps++;
                if (a.Asset.Id == b.Asset.Id && distance < 5f &&
                    Mathf.Abs(Mathf.Wrap(a.Yaw - b.Yaw, -Mathf.Pi, Mathf.Pi)) < .2f) repeat++;
            }
            if (near > 7) spikes++;
        }
        // Catalog triangle counts are bounded here as a conservative planning estimate;
        // P6 will record actual imported mesh surfaces and frame costs.
        var triangleEstimate = _placed.Sum(p => p.Asset.Family switch
        { FafEnvironmentFamily.Terrain => 1200, FafEnvironmentFamily.Rock => 250,
            FafEnvironmentFamily.Coral => 700, FafEnvironmentFamily.Plant => 260, _ => 100 });
        return new WorldPlacementScore(roles, depths, families, living,
            _placed.Select(p => p.Asset.Id).Distinct(StringComparer.Ordinal).Count(),
            _supportRejected, support, _routeRejected, routes, _spacingRejected,
            overlaps, repeat, minimum, spikes, _repaired, _removed, _placed.Count,
            triangleEstimate, _placed.Count(p => p.Asset.Family is FafEnvironmentFamily.Terrain or FafEnvironmentFamily.Rock) +
                _placed.Where(p => p.Asset.Family is not (FafEnvironmentFamily.Terrain or FafEnvironmentFamily.Rock))
                    .Select(p => p.Asset.Id).Distinct(StringComparer.Ordinal).Count());
    }

    private static bool RoleAllows(FafEnvironmentEntry asset, HabitatRole role) => asset.Family switch
    {
        FafEnvironmentFamily.Terrain => asset.Variant switch
        {
            'A' => role == HabitatRole.SandyClearing,
            'B' => role is HabitatRole.GrassMeadow or HabitatRole.ShelteredPocket,
            'C' => role is HabitatRole.CoralGarden or HabitatRole.DropOffReef,
            'D' => role is HabitatRole.RockySlope or HabitatRole.DropOffReef,
            'E' => role == HabitatRole.DropOffReef,
            _ => role is HabitatRole.RockySlope or HabitatRole.DeepRubble,
        },
        FafEnvironmentFamily.Rock => role is HabitatRole.RockySlope or HabitatRole.CaveEntrance
            or HabitatRole.ShelteredPocket or HabitatRole.DropOffReef or HabitatRole.DeepRubble,
        FafEnvironmentFamily.Coral => role is HabitatRole.CoralGarden or HabitatRole.ShelteredPocket
            or HabitatRole.DropOffReef or HabitatRole.RockySlope,
        FafEnvironmentFamily.Plant => asset.Type switch
        {
            "sea_grass" => role is HabitatRole.GrassMeadow or HabitatRole.SandyClearing,
            "broad_seaweed" => role is HabitatRole.CoralGarden or HabitatRole.ShelteredPocket,
            _ => role is HabitatRole.RockySlope or HabitatRole.CoralGarden,
        },
        _ => role != HabitatRole.GrassMeadow || asset.Type is "shell" or "pebble",
    };

    private static float RoleScore(HabitatRole role, WorldEnvironmentSample f)
    {
        static float Bell(float value, float center, float spread) =>
            Mathf.Clamp(1f - Mathf.Abs(value - center) / spread, 0f, 1f);
        var depth = f.Depth;
        var slope = f.Slope;
        return role switch
        {
            HabitatRole.CoralGarden => .55f * Bell(depth, 10f, 7f) + .25f * Bell(slope, .17f, .3f) + .2f * f.Exposure,
            HabitatRole.GrassMeadow => .65f * Bell(depth, 9f, 6f) + .35f * Bell(slope, .05f, .2f),
            HabitatRole.SandyClearing => .55f * Bell(depth, 12f, 10f) + .45f * Bell(slope, .03f, .15f),
            HabitatRole.RockySlope => .4f * Bell(depth, 15f, 10f) + .35f * Bell(slope, .32f, .4f) + .25f * (1f - Mathf.Min(f.GeologyDistance / 12f, 1f)),
            HabitatRole.CaveEntrance => .45f * Bell(f.CaveDistance, 6f, 7f) + .35f * Bell(f.EntranceDistance, 7f, 6f) + .2f * f.Shelter,
            HabitatRole.ShelteredPocket => .5f * Bell(depth, 11f, 8f) + .5f * f.Shelter,
            HabitatRole.DropOffReef => .45f * Bell(depth, 17f, 9f) + .35f * Bell(slope, .4f, .45f) + .2f * f.Exposure,
            _ => .7f * Bell(depth, 21f, 7f) + .3f * (1f - Mathf.Min(f.GeologyDistance / 12f, 1f)),
        };
    }

    private static float FamilyScore(FafEnvironmentEntry asset, WorldEnvironmentSample f)
    {
        var score = Mathf.Clamp(f.RouteDistance / 7f, 0f, 1f) * .12f +
            Mathf.Clamp(f.EntranceDistance / 8f, 0f, 1f) * .1f;
        score += asset.Family switch
        {
            FafEnvironmentFamily.Terrain => .35f * (1f - Mathf.Clamp(f.Slope / .35f, 0f, 1f)) + .25f * f.Exposure,
            FafEnvironmentFamily.Rock => .3f * Mathf.Clamp(f.Slope / .45f, 0f, 1f) + .23f * (1f - Mathf.Clamp(f.GeologyDistance / 12f, 0f, 1f)),
            FafEnvironmentFamily.Coral => .3f * Mathf.Clamp((20f - f.Depth) / 13f, 0f, 1f) +
                (asset.Type == "fan" ? .25f * f.Exposure : .25f * (1f - Mathf.Clamp(f.Slope / .55f, 0f, 1f))),
            FafEnvironmentFamily.Plant => .4f * Mathf.Clamp((18f - f.Depth) / 11f, 0f, 1f) +
                .2f * (asset.Type == "sea_grass" ? 1f - Mathf.Clamp(f.Slope / .3f, 0f, 1f) : f.Shelter),
            _ => .25f * (1f - Mathf.Clamp(f.Slope / .5f, 0f, 1f)) +
                .2f * (asset.Type == "rubble" ? 1f - Mathf.Clamp(f.GeologyDistance / 12f, 0f, 1f) : f.Exposure),
        };
        if (asset.Family == FafEnvironmentFamily.Plant && f.Depth > 17f) score -= 1f;
        if (asset.Family == FafEnvironmentFamily.Coral && f.Depth > 20f) score -= .7f;
        return score;
    }

    private static string DepthBand(float depth) => depth < 10f ? "shallow" : depth < 13f ? "upper" :
        depth < 16f ? "middle" : depth < 19f ? "lower" : "deep";
    private static float NextFloat(ref StableWorldRandom random) =>
        (float)((random.Next() >> 11) * (1.0 / 9007199254740992.0));
}
