#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Fish;

public enum HabitatRole
{
    GrassMeadow, BranchingReef, CoralGarden, RockyShelf, SandyClearing,
    CaveEntrance, RockySlope, DropOffReef, DeepRubble, DeepRockField,
    DeepSparseFan, OpenWaterCorridor,
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
    int VariantCount, int SupportFailuresBefore, int SupportFailuresAfter,
    int RouteFailuresBefore, int RouteFailuresAfter, int IntersectionsBefore,
    int IntersectionsAfter, int RepeatedOrientations, float MinimumSpacing,
    int DensitySpikes, int Repaired, int Removed, int EstimatedInstances,
    int EstimatedTriangles, int EstimatedBatches)
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

/// <summary>Deterministic 12 m habitat patches filled from cached four metre terrain cells.</summary>
public sealed class GeneratedFafPlacement
{
    private readonly record struct Cell(int X, int Z, WorldEnvironmentSample Field, HabitatRole Role);
    private readonly WorldEnvironmentAnalysis _environment;
    private readonly GeneratedWorldRoutes _routes;
    private readonly WorldCompileContext _context;
    private readonly List<GeneratedFafInstance> _placed = new();
    private readonly List<Cell> _cells = new();
    private readonly Dictionary<HabitatRole, float> _habitatArea = new();
    private readonly Dictionary<HabitatRole, int> _habitatPatches = new();
    private int _supportRejected, _routeRejected;

    public IReadOnlyList<GeneratedFafInstance> Instances => _placed;
    public IReadOnlyDictionary<HabitatRole, float> HabitatArea => _habitatArea;
    public IReadOnlyDictionary<HabitatRole, int> HabitatPatches => _habitatPatches;
    public int AnchorCount => _habitatArea.Count;
    public long PlanningMilliseconds { get; private set; }
    public long PlacementMilliseconds { get; private set; }
    public WorldPlacementScore Score { get; private set; } = null!;

    public GeneratedFafPlacement(WorldEnvironmentAnalysis environment, GeneratedWorldRoutes routes, ulong seed)
    {
        _environment = environment;
        _routes = routes;
        _context = new WorldCompileContext(new WorldGenerationSettings(seed));
    }

    public bool Build(out string diagnostic)
    {
        WorldGenerationAudit.MarkStage();
        if (_cells.Count != 0) { diagnostic = "WORLD_PLACEMENT_INVALID action=single-build"; return false; }
        var timer = Stopwatch.StartNew();
        PlanCells();
        PlanningMilliseconds = timer.ElapsedMilliseconds;
        GD.Print($"WORLD_STAGE habitat-planning elapsed_ms={PlanningMilliseconds} cells={_cells.Count} patches={string.Join(',', _habitatPatches.OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value}"))} area_m2={string.Join(',', _habitatArea.OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value:F0}"))}");
        timer.Restart();
        foreach (var asset in FafRuntimeCatalog.Environment.Where(a => a.Family == FafEnvironmentFamily.Terrain))
            if (!PlaceStructural(asset, 1))
            { diagnostic = $"WORLD_PLACEMENT_INVALID id={asset.Id} action=no-supported-site"; return false; }
        foreach (var asset in FafRuntimeCatalog.Environment.Where(a => a.Family == FafEnvironmentFamily.Rock))
            if (!PlaceStructural(asset, 5))
            { diagnostic = $"WORLD_PLACEMENT_INVALID id={asset.Id} action=no-supported-site"; return false; }
        FillDecorations();
        FillEntranceBlend();
        PlacementMilliseconds = timer.ElapsedMilliseconds;
        Score = Measure();
        if (Score.VariantCount != FafRuntimeCatalog.Environment.Count ||
            Score.SupportFailuresAfter != 0 || Score.RouteFailuresAfter != 0 ||
            Score.IntersectionsAfter != 0 ||
            Score.EstimatedInstances < 2000 || Score.LivingByDepth["shallow"] + Score.LivingByDepth["upper"] <=
            3 * (Score.LivingByDepth["lower"] + Score.LivingByDepth["deep"]))
        { diagnostic = $"WORLD_PLACEMENT_INVALID {Score}"; return false; }
        GD.Print($"WORLD_STAGE dense-placement elapsed_ms={PlacementMilliseconds} {Score}");
        diagnostic = "";
        return true;
    }

    private void PlanCells()
    {
        var side = WorldEnvironmentAnalysis.CellsPerSide;
        var patches = new HashSet<(int X, int Z, HabitatRole Role)>();
        for (var z = 1; z < side - 1; z++)
        for (var x = 1; x < side - 1; x++)
        {
            var field = _environment.SampleGrid(x, z);
            if (field.Depth < .55f || field.Depth > 45f || field.PerimeterDistance < 1.2f ||
                field.Normal.Y < .68f) continue;
            // One stable choice spans a 3x3 block of cached cells. The final depth at
            // each cell overrides the block where a patch crosses a depth ledge.
            var region = _context.Stream("habitat-region", $"{x / 3}:{z / 3}");
            var roll = NextFloat(ref region);
            var role = SelectRole(field, roll);
            _cells.Add(new Cell(x, z, field, role));
            _habitatArea[role] = _habitatArea.GetValueOrDefault(role) + 16f;
            if (patches.Add((x / 3, z / 3, role)))
                _habitatPatches[role] = _habitatPatches.GetValueOrDefault(role) + 1;
        }
    }

    private static HabitatRole SelectRole(WorldEnvironmentSample f, float roll)
    {
        if (f.RouteDistance < 4.2f) return HabitatRole.OpenWaterCorridor;
        if (f.EntranceDistance < 9f && f.Depth < 25f)
            return f.EntranceDistance < 4.5f ? HabitatRole.RockyShelf : HabitatRole.CaveEntrance;
        if (f.Depth < 2f)
            return f.Slope > .35f ? HabitatRole.RockyShelf :
                roll < .62f ? HabitatRole.GrassMeadow :
                roll < .85f ? HabitatRole.SandyClearing : HabitatRole.CoralGarden;
        if (f.Depth < 6f)
            return f.Slope > .45f ? HabitatRole.RockyShelf :
                roll < .32f ? HabitatRole.GrassMeadow :
                roll < .62f ? HabitatRole.BranchingReef :
                roll < .9f ? HabitatRole.CoralGarden : HabitatRole.SandyClearing;
        if (f.Depth < 15f)
            return f.Slope > .48f ? HabitatRole.DropOffReef :
                roll < .25f ? HabitatRole.CoralGarden :
                roll < .43f ? HabitatRole.BranchingReef :
                roll < .7f ? HabitatRole.RockySlope :
                roll < .87f ? HabitatRole.RockyShelf : HabitatRole.SandyClearing;
        if (f.Depth < 30f)
            return f.Slope > .46f ? HabitatRole.DropOffReef :
                roll < .43f ? HabitatRole.RockySlope :
                roll < .72f ? HabitatRole.DeepRubble :
                roll < .88f ? HabitatRole.DeepSparseFan : HabitatRole.OpenWaterCorridor;
        return roll < .47f ? HabitatRole.DeepRockField :
            roll < .75f ? HabitatRole.DeepRubble :
            roll < .88f ? HabitatRole.DeepSparseFan : HabitatRole.OpenWaterCorridor;
    }

    private bool PlaceStructural(FafEnvironmentEntry asset, int wanted)
    {
        var random = _context.Stream("habitat-structural", asset.Id);
        var eligible = _cells.Where(c => Allows(asset, c.Role, c.Field.Depth) &&
            c.Field.RouteDistance > 5f && c.Field.EntranceDistance > 3.2f &&
            c.Field.Normal.Y > .72f).ToArray();
        if (eligible.Length == 0) return false;
        var placed = 0;
        for (var attempt = 0; attempt < eligible.Length * 2 && placed < wanted; attempt++)
        {
            var cell = eligible[(int)(random.Next() % (ulong)eligible.Length)];
            if (!TryPlace(asset, cell, ref random, true, out var piece)) continue;
            if (_placed.Any(p => p.Asset.Family is FafEnvironmentFamily.Terrain or FafEnvironmentFamily.Rock &&
                new Vector2(p.Position.X - piece.Position.X, p.Position.Z - piece.Position.Z).Length() <
                p.Radius + piece.Radius + .8f)) continue;
            _placed.Add(piece);
            placed++;
        }
        return placed == wanted;
    }

    private void FillDecorations()
    {
        var families = new[] { FafEnvironmentFamily.Plant, FafEnvironmentFamily.Coral,
            FafEnvironmentFamily.Detail };
        var choices = Enum.GetValues<HabitatRole>().ToDictionary(role => role,
            role => families.ToDictionary(family => family,
                family => FafRuntimeCatalog.Environment.Where(a => a.Family == family &&
                    Allows(a, role, RoleDepth(role))).ToArray()));
        foreach (var cell in _cells)
        {
            var random = _context.Stream("habitat-fill", $"{cell.X}:{cell.Z}");
            var gap = NextFloat(ref random);
            if (gap < (cell.Role == HabitatRole.SandyClearing ? .66f : .16f)) continue;
            foreach (var family in families)
            {
                var options = choices[cell.Role][family].Where(a => Allows(a, cell.Role, cell.Field.Depth)).ToArray();
                if (options.Length == 0) continue;
                var mean = Density(cell.Role, family, cell.Field.Depth);
                // Shared 12 m roll gives broad masses. Cell roll and gaps break their edges.
                var cluster = _context.Stream("habitat-cluster", $"{cell.X / 3}:{cell.Z / 3}:{family}");
                mean *= .65f + .7f * NextFloat(ref cluster);
                var count = (int)mean + (NextFloat(ref random) < mean % 1f ? 1 : 0);
                for (var i = 0; i < count; i++)
                {
                    var asset = options[(int)(random.Next() % (ulong)options.Length)];
                    if (TryPlace(asset, cell, ref random, false, out var piece)) _placed.Add(piece);
                }
            }
        }
    }

    private void FillEntranceBlend()
    {
        var assets = FafRuntimeCatalog.Environment.Where(a => a.Family is
            FafEnvironmentFamily.Plant or FafEnvironmentFamily.Coral or FafEnvironmentFamily.Detail).ToArray();
        for (var mouth = 0; mouth < _environment.Entrances.Count; mouth++)
        {
            var entrance = _environment.Entrances[mouth];
            var outward = new Vector2(entrance.Outward.X, entrance.Outward.Z).Normalized();
            var side = new Vector2(-outward.Y, outward.X);
            var random = _context.Stream("habitat-mouth-blend", mouth.ToString());
            for (var i = 0; i < 52; i++)
            {
                if (NextFloat(ref random) < .23f) continue;
                var sign = i % 2 == 0 ? 1f : -1f;
                var sideDistance = 2.8f + NextFloat(ref random) * (sign > 0 ? 5.4f : 3.8f);
                var outwardDistance = -1.5f + NextFloat(ref random) * 12f;
                var xz = new Vector2(entrance.Center.X, entrance.Center.Z) +
                    side * sideDistance * sign + outward * outwardDistance;
                if (_environment.DistanceToShore(xz.X, xz.Y) < 2f) continue;
                // Start below the roof so cave-floor contacts are reachable. The
                // downward ray still validates actual generated mesh support.
                var hit = _environment.RaycastSurface(
                    new Vector3(xz.X, entrance.Center.Y + 2f, xz.Y), Vector3.Down, 12f);
                if (!float.IsFinite(hit.Distance) || hit.Normal.Y < .58f) continue;
                var depth = Mathf.Max(0f, WorldEnvironmentAnalysis.WaterSurfaceY - hit.Position.Y);
                if (depth < 1f || depth > 30f) continue;
                var family = i % 4 == 0 ? FafEnvironmentFamily.Detail :
                    i % 3 == 0 ? FafEnvironmentFamily.Coral : FafEnvironmentFamily.Plant;
                var options = assets.Where(a => a.Family == family &&
                    Allows(a, HabitatRole.CaveEntrance, depth)).ToArray();
                if (options.Length == 0) continue;
                var asset = options[(int)(random.Next() % (ulong)options.Length)];
                var scale = .7f + .45f * NextFloat(ref random);
                var radius = .5f * Mathf.Max(asset.SizeMeters.X, asset.SizeMeters.Z) * scale;
                var yaw = NextFloat(ref random) * Mathf.Tau;
                var position = hit.Position - Vector3.Up * .02f;
                var top = position + Vector3.Up * asset.SizeMeters.Y * scale;
                if (_routes.IsProtected(top, .22f)) { _routeRejected++; continue; }
                _placed.Add(new GeneratedFafInstance(asset, HabitatRole.CaveEntrance,
                    DepthBand(depth), position, hit.Normal, yaw, scale, radius, 0f, 1f));
            }
        }
    }

    private static float Density(HabitatRole role, FafEnvironmentFamily family, float depth)
    {
        if (role == HabitatRole.OpenWaterCorridor) return family == FafEnvironmentFamily.Detail ? .15f : 0f;
        if (depth >= 30f) return family switch
        { FafEnvironmentFamily.Plant => 0f, FafEnvironmentFamily.Coral => .09f, _ => .35f };
        if (depth >= 15f) return family switch
        { FafEnvironmentFamily.Plant => .08f, FafEnvironmentFamily.Coral => .22f, _ => .6f };
        if (depth >= 6f) return family switch
        { FafEnvironmentFamily.Plant => role is HabitatRole.RockySlope or HabitatRole.DropOffReef ? .35f : 1.2f,
          FafEnvironmentFamily.Coral => role == HabitatRole.SandyClearing ? .12f : .85f, _ => 1.1f };
        return family switch
        { FafEnvironmentFamily.Plant => role == HabitatRole.GrassMeadow ? 3.7f : 2.2f,
          FafEnvironmentFamily.Coral => role == HabitatRole.SandyClearing ? .2f : 1.35f,
          _ => 1.55f };
    }

    private bool TryPlace(FafEnvironmentEntry asset, Cell cell, ref StableWorldRandom random,
        bool structural, out GeneratedFafInstance piece)
    {
        piece = null!;
        var x = cell.Field.Surface.X + (NextFloat(ref random) - .5f) * 3.6f;
        var z = cell.Field.Surface.Z + (NextFloat(ref random) - .5f) * 3.6f;
        if (_environment.DistanceToShore(x, z) < 1.5f) return false;
        var hit = _environment.ProjectDown(new Vector3(x, 60f, z));
        if (!float.IsFinite(hit.Distance) || hit.Normal.Y < .68f) { _supportRejected++; return false; }
        var depth = Mathf.Max(0f, WorldEnvironmentAnalysis.WaterSurfaceY - hit.Position.Y);
        if (depth < .5f || depth > 45f || !Allows(asset, cell.Role, depth)) return false;
        var scale = asset.Family switch
        { FafEnvironmentFamily.Terrain => .34f + .09f * NextFloat(ref random),
          FafEnvironmentFamily.Rock => .8f + .25f * NextFloat(ref random),
          FafEnvironmentFamily.Detail => .8f + .4f * NextFloat(ref random),
          _ => .76f + .38f * NextFloat(ref random) };
        var radius = .5f * Mathf.Max(asset.SizeMeters.X, asset.SizeMeters.Z) * scale;
        var yaw = NextFloat(ref random) * Mathf.Tau;
        var footprint = 0f;
        var lowest = hit.Position.Y;
        if (structural)
        {
            for (var i = 0; i < 4; i++)
            {
                var angle = yaw + i * Mathf.Pi * .5f;
                var support = _environment.ProjectDown(new Vector3(x + Mathf.Cos(angle) * radius * .75f,
                    60f, z + Mathf.Sin(angle) * radius * .75f));
                if (!float.IsFinite(support.Distance) || support.Normal.Y < .65f)
                { _supportRejected++; return false; }
                footprint = Mathf.Max(footprint, Mathf.Abs(support.Position.Y - hit.Position.Y));
                lowest = Mathf.Min(lowest, support.Position.Y);
            }
            if (footprint > (asset.Family == FafEnvironmentFamily.Terrain ? 1.2f : .42f))
            { _supportRejected++; return false; }
        }
        var embed = asset.Family == FafEnvironmentFamily.Terrain ? .16f :
            asset.Family == FafEnvironmentFamily.Rock ? .12f : .015f;
        var position = new Vector3(x,
            (asset.Family == FafEnvironmentFamily.Terrain ? hit.Position.Y : lowest) - embed, z);
        var top = position + Vector3.Up * asset.SizeMeters.Y * scale;
        if (cell.Field.RouteDistance < 8f && _routes.IsProtected(top, structural ? .3f : .22f))
        { _routeRejected++; return false; }
        if (cell.Field.EntranceDistance < (structural ? radius + 3f : radius + 2.4f))
        { _routeRejected++; return false; }
        piece = new GeneratedFafInstance(asset, cell.Role, DepthBand(depth), position,
            hit.Normal, yaw, scale, radius, footprint, 1f - depth / 50f);
        return true;
    }

    private WorldPlacementScore Measure()
    {
        var roles = Enum.GetValues<HabitatRole>().ToDictionary(x => x.ToString(),
            x => _placed.Count(p => p.Role == x));
        var bands = new[] { "shallow", "upper", "middle", "lower", "deep" };
        var depths = bands.ToDictionary(x => x, x => _placed.Count(p => p.DepthBand == x));
        var families = Enum.GetValues<FafEnvironmentFamily>().ToDictionary(x => x.ToString(),
            x => _placed.Count(p => p.Asset.Family == x));
        var living = bands.ToDictionary(x => x, x => _placed.Count(p => p.DepthBand == x &&
            p.Asset.Family is FafEnvironmentFamily.Coral or FafEnvironmentFamily.Plant));
        var triangles = _placed.Sum(p => p.Asset.Family switch
        { FafEnvironmentFamily.Terrain => 1200, FafEnvironmentFamily.Rock => 250,
          FafEnvironmentFamily.Coral => 700, FafEnvironmentFamily.Plant => 260, _ => 100 });
        var batches = _placed.Count(p => p.Asset.Family is FafEnvironmentFamily.Terrain or FafEnvironmentFamily.Rock) +
            _placed.Where(p => p.Asset.Family is not (FafEnvironmentFamily.Terrain or FafEnvironmentFamily.Rock))
                .Select(p => p.Asset.Id).Distinct(StringComparer.Ordinal).Count();
        var structural = _placed.Where(p => p.Asset.Family is FafEnvironmentFamily.Terrain or
            FafEnvironmentFamily.Rock).ToArray();
        var supportAfter = structural.Count(p => p.FootprintDelta >
            (p.Asset.Family == FafEnvironmentFamily.Terrain ? 1.2f : .42f));
        var routeAfter = _placed.Count(p => _routes.IsProtected(
            p.Position + Vector3.Up * p.Asset.SizeMeters.Y * p.Scale,
            p.Asset.Family is FafEnvironmentFamily.Terrain or FafEnvironmentFamily.Rock ? .3f : .22f));
        var intersectionsAfter = 0;
        var minimumSpacing = float.PositiveInfinity;
        for (var i = 0; i < structural.Length; i++)
        for (var j = i + 1; j < structural.Length; j++)
        {
            var a = structural[i];
            var b = structural[j];
            var spacing = new Vector2(a.Position.X - b.Position.X,
                a.Position.Z - b.Position.Z).Length() - a.Radius - b.Radius;
            minimumSpacing = Mathf.Min(minimumSpacing, spacing);
            if (spacing < .8f) intersectionsAfter++;
        }
        return new WorldPlacementScore(roles, depths, families, living,
            _placed.Select(p => p.Asset.Id).Distinct(StringComparer.Ordinal).Count(),
            _supportRejected, supportAfter, _routeRejected, routeAfter, 0,
            intersectionsAfter, 0, minimumSpacing, 0, 0, 0,
            _placed.Count, triangles, batches);
    }

    private static bool Allows(FafEnvironmentEntry asset, HabitatRole role, float depth)
    {
        if (role == HabitatRole.OpenWaterCorridor) return asset.Family == FafEnvironmentFamily.Detail;
        if (depth >= 30f) return asset.Family switch
        { FafEnvironmentFamily.Terrain => asset.Variant is 'A' or 'D' or 'F',
          FafEnvironmentFamily.Rock => true,
          FafEnvironmentFamily.Coral => asset.Type is "fan" or "tube",
          FafEnvironmentFamily.Plant => false, _ => true };
        if (depth >= 15f) return asset.Family switch
        { FafEnvironmentFamily.Terrain => asset.Variant is 'D' or 'E' or 'F',
          FafEnvironmentFamily.Rock => true,
          FafEnvironmentFamily.Coral => asset.Type is "fan" or "tube",
          FafEnvironmentFamily.Plant => asset.Type == "reef_algae" && depth < 19f,
          _ => true };
        if (asset.Family == FafEnvironmentFamily.Terrain) return asset.Variant switch
        { 'A' => role == HabitatRole.SandyClearing,
          'B' => role == HabitatRole.GrassMeadow,
          'C' => role is HabitatRole.CoralGarden or HabitatRole.BranchingReef,
          'D' => role is HabitatRole.RockySlope or HabitatRole.RockyShelf,
          'E' => role == HabitatRole.DropOffReef,
          _ => role is HabitatRole.RockySlope or HabitatRole.RockyShelf };
        if (asset.Family == FafEnvironmentFamily.Rock)
            return role is HabitatRole.RockyShelf or HabitatRole.RockySlope or HabitatRole.CaveEntrance or
                HabitatRole.DropOffReef or HabitatRole.DeepRubble;
        if (asset.Family == FafEnvironmentFamily.Coral)
            return role switch
            { HabitatRole.GrassMeadow => asset.Type == "rounded",
              HabitatRole.BranchingReef => asset.Type is "branching" or "rounded",
              HabitatRole.CaveEntrance => asset.Type is "fan" or "rounded" or "tube",
              HabitatRole.SandyClearing => asset.Type == "rounded",
              _ => true };
        if (asset.Family == FafEnvironmentFamily.Plant)
            return role switch
            { HabitatRole.GrassMeadow => asset.Type == "sea_grass",
              HabitatRole.BranchingReef => asset.Type is "sea_grass" or "reef_algae",
              HabitatRole.RockySlope or HabitatRole.DropOffReef => asset.Type == "reef_algae",
              HabitatRole.SandyClearing => asset.Type == "sea_grass",
              _ => true };
        return true;
    }

    private static float RoleDepth(HabitatRole role) => role switch
    { HabitatRole.DeepRockField or HabitatRole.DeepSparseFan or HabitatRole.DeepRubble => 32f,
      HabitatRole.RockySlope or HabitatRole.DropOffReef => 11f, _ => 4f };
    private static string DepthBand(float depth) => depth < 2f ? "shallow" : depth < 6f ? "upper" :
        depth < 15f ? "middle" : depth < 30f ? "lower" : "deep";
    private static float NextFloat(ref StableWorldRandom random) =>
        (float)((random.Next() >> 11) * (1.0 / 9007199254740992.0));
}
