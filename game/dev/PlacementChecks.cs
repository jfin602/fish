#nullable enable
using Fish;
using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public static class PlacementChecks
{
    public static void Run(Node3D main)
    {
        var reef = main.GetNode<ReefComposition>("Habitat/Composition/ReefComposition");
        var dressing = reef.GetNode<FafReefDressing>("Dressing");
        var field = reef.EnvironmentAnalysis ?? throw new Exception("Missing environment field");
        var routes = reef.GeneratedRoutes ?? throw new Exception("Missing protected routes");
        var actual = dressing.Placements;
        Require(actual.Count > 2000 && dressing.WorldScore is { } score, "Normal-play plan missing");
        var first = new GeneratedFafPlacement(field, routes, 42);
        Require(first.Build(out var diagnostic), diagnostic);
        var baseline = Fingerprint(actual);
        Require(baseline == Fingerprint(first.Instances), "Same-seed placement differs from normal play");
        Require(dressing.HabitatArea.OrderBy(x => x.Key).SequenceEqual(
                first.HabitatArea.OrderBy(x => x.Key)) &&
            dressing.HabitatPatches.OrderBy(x => x.Key).SequenceEqual(
                first.HabitatPatches.OrderBy(x => x.Key)),
            "Same-seed habitat area or patch counts differ from normal play");
        Validate(first, routes, "42");
        ValidateVisuals(dressing);
        GD.Print($"WORLD_PLACEMENT_SEED seed=42 hash={baseline[..16]} habitats={first.AnchorCount} {first.Score}");
        foreach (var seed in new ulong[] { 43, 44 })
        {
            var basin = new GeneratedBasin(seed);
            var regions = GeologicalRegionGenerator.Generate(basin);
            var alternateRoutes = new GeneratedWorldRoutes(basin, regions);
            var alternateField = new WorldEnvironmentAnalysis(basin, regions, alternateRoutes);
            var plan = new GeneratedFafPlacement(alternateField, alternateRoutes, seed);
            Require(plan.Build(out diagnostic), $"Seed {seed}: {diagnostic}");
            Validate(plan, alternateRoutes, seed.ToString(CultureInfo.InvariantCulture));
            var hash = Fingerprint(plan.Instances);
            Require(hash != baseline, $"Seed {seed} repeats seed 42");
            GD.Print($"WORLD_PLACEMENT_SEED seed={seed} hash={hash[..16]} habitats={plan.AnchorCount} {plan.Score}");
        }
        GD.Print("WORLD_PLACEMENT_CHECK_OK seeds=42,43,44 same_seed=true different_seed=true variants=38 support=0 route=0 grid=false");
    }

    private static void Validate(GeneratedFafPlacement plan, GeneratedWorldRoutes routes, string seed)
    {
        var score = plan.Score;
        Require(score.VariantCount == 38 && score.ByFamily.Values.Sum() == plan.Instances.Count,
            $"Seed {seed}: catalog coverage or family accounting");
        Require(score.SupportFailuresAfter == 0 && score.RouteFailuresAfter == 0 &&
            score.EstimatedInstances > 2000 && score.EstimatedBatches < 100,
            $"Seed {seed}: unresolved critical placement failure");
        Require(score.ByFamily["Terrain"] == 6 && score.ByFamily["Rock"] == 20 &&
            score.ByFamily["Coral"] > 100 && score.ByFamily["Plant"] > 100 &&
            score.ByFamily["Detail"] > 100,
            $"Seed {seed}: habitat fill incomplete");
        Require(score.LivingByDepth["shallow"] + score.LivingByDepth["upper"] >
                3 * (score.LivingByDepth["lower"] + score.LivingByDepth["deep"]),
            $"Seed {seed}: shallow-to-deep living density lost");
        var fractional = plan.Instances.Count(p =>
            Mathf.Abs(p.Position.X - Mathf.Round(p.Position.X)) > .04f &&
            Mathf.Abs(p.Position.Z - Mathf.Round(p.Position.Z)) > .04f);
        Require(fractional > plan.Instances.Count * .7f,
            $"Seed {seed}: regular grid failure");
        foreach (var piece in plan.Instances)
        {
            Require(piece.Position.IsFinite() && piece.Normal.IsFinite() &&
                piece.FootprintDelta <= (piece.Asset.Family == FafEnvironmentFamily.Terrain ? 1.2f :
                    piece.Asset.Family == FafEnvironmentFamily.Rock ? .42f : .33f),
                $"Seed {seed}: unsupported {piece.Asset.Id}");
            var top = piece.Position + Vector3.Up * piece.Asset.SizeMeters.Y * piece.Scale;
            Require(!routes.IsProtected(top, piece.Asset.Family is FafEnvironmentFamily.Terrain or
                FafEnvironmentFamily.Rock ? .3f : .22f),
                $"Seed {seed}: protected route contains {piece.Asset.Id}");
        }
    }

    private static void ValidateVisuals(FafReefDressing dressing)
    {
        var visuals = dressing.GetNode<Node3D>("Visuals");
        var solids = 0;
        foreach (var child in visuals.GetChildren())
        {
            if (child is not Node3D root || !root.HasMeta("faf_id")) continue;
            var id = root.GetMeta("faf_id").AsString();
            if (!id.StartsWith("FAF-TERRAIN", StringComparison.Ordinal) &&
                !id.StartsWith("FAF-ROCK", StringComparison.Ordinal)) continue;
            var mesh = FindMesh(root) ?? throw new Exception($"Missing major mesh {id}");
            var body = root.FindChild(id.StartsWith("FAF-TERRAIN", StringComparison.Ordinal) ?
                "DrapedSolid" : "Solid", true, false) as StaticBody3D;
            Require(body?.GetNodeOrNull<CollisionShape3D>("Shape")?.Shape is not null,
                $"Missing major collision {id}");
            Require(mesh.Mesh is not null && mesh.Mesh.GetFaces().Length > 0,
                $"Missing major visual triangles {id}");
            solids++;
        }
        Require(solids == 26, $"Expected 26 supported terrain/rock solids, got {solids}");
    }

    private static MeshInstance3D? FindMesh(Node node)
    {
        if (node is MeshInstance3D mesh) return mesh;
        foreach (var child in node.GetChildren())
            if (FindMesh(child) is { } found) return found;
        return null;
    }

    private static string Fingerprint(IEnumerable<GeneratedFafInstance> pieces) =>
        BakedWorldContract.Hash(string.Join("\n", pieces.Select(p => FormattableString.Invariant(
            $"{p.Asset.Id}|{p.Role}|{p.Position.X:R}|{p.Position.Y:R}|{p.Position.Z:R}|{p.Yaw:R}|{p.Scale:R}"))));
    private static void Require(bool condition, string message)
    { if (!condition) throw new Exception(message); }
}
