#nullable enable
using System;
using System.Diagnostics;
using Fish;
using Godot;

public static class BasinChecks
{
    public static void RunStructural()
    {
        var previous = new GeneratedBasin(42);
        foreach (var seed in new ulong[] { 42, 43, 44 })
        {
            var timer = Stopwatch.StartNew();
            var basin = seed == 42 ? previous : new GeneratedBasin(seed);
            var copy = new GeneratedBasin(seed);
            Require(basin.ShoreMin == copy.ShoreMin && basin.ShoreMax == copy.ShoreMax &&
                basin.DeepCore == copy.DeepCore && basin.DeepFloorY == copy.DeepFloorY,
                "same seed metadata changed");
            var width = basin.ShoreMax.X - basin.ShoreMin.X;
            var height = basin.ShoreMax.Y - basin.ShoreMin.Y;
            Require(width >= 245f && width <= 355f && height >= 245f && height <= 355f,
                $"footprint outside target: {width:F1}x{height:F1}");
            Require(basin.MaxPerimeterRadius - basin.MinPerimeterRadius > 25f,
                "shoreline lacks broad radius variation");
            var radialDifference = 0f;
            for (var i = 0; i < basin.ShorePointCount; i++)
            {
                var point = basin.ShorePoint(i);
                Require(point == copy.ShorePoint(i), "shoreline changed for same seed");
                if (seed != 42) radialDifference += point.DistanceTo(previous.ShorePoint(i));
            }
            if (seed != 42) Require(radialDifference / basin.ShorePointCount > 10f,
                "seed produces too similar an outline");
            var leastPeriodicDifference = float.PositiveInfinity;
            for (var symmetry = 2; symmetry <= 6; symmetry++)
            {
                var difference = 0f;
                var offset = basin.ShorePointCount / symmetry;
                for (var i = 0; i < basin.ShorePointCount; i++)
                    difference += Mathf.Abs(basin.ShorePoint(i).Length() -
                        basin.ShorePoint(i + offset).Length());
                leastPeriodicDifference = Mathf.Min(leastPeriodicDifference,
                    difference / basin.ShorePointCount);
            }
            Require(leastPeriodicDifference > 9f,
                $"outline has a periodic angular read: {leastPeriodicDifference:F1}");

            var approaches = 0;
            for (var i = 0; i < basin.ShorePointCount; i += 32)
            {
                var point = basin.ShorePoint(i);
                var tangent = (basin.ShorePoint(i + 2) - basin.ShorePoint(i - 2)).Normalized();
                var inward = new Vector2(-tangent.Y, tangent.X);
                if (basin.DistanceToShore(point.X + inward.X * 4f, point.Y + inward.Y * 4f) < 0f)
                    inward = -inward;
                var d4 = basin.DistanceToShore(point.X + inward.X * 4f, point.Y + inward.Y * 4f);
                var d20 = basin.DistanceToShore(point.X + inward.X * 20f, point.Y + inward.Y * 20f);
                var h4 = basin.HeightAt(point.X + inward.X * 4f, point.Y + inward.Y * 4f);
                var h20 = basin.HeightAt(point.X + inward.X * 20f, point.Y + inward.Y * 20f);
                Require(d4 > 1f && d20 > d4 + 6f && h4 > h20 + 1f,
                    $"shore approach did not deepen: bearing={i} distances={d4:F1}/{d20:F1} heights={h4:F1}/{h20:F1}");
                approaches++;
            }
            var counts = new int[5];
            var wet = 0; var maxStep = 0f; var maxDepth = 0f;
            for (var z = -176; z <= 176; z += 2)
            for (var x = -176; x <= 176; x += 2)
            {
                var shore = basin.DistanceToShore(x, z);
                if (shore <= 0f) continue;
                wet++;
                var y = basin.HeightAt(x, z);
                var depth = GeneratedBasin.WaterSurfaceY - y;
                Require(float.IsFinite(y) && y == copy.HeightAt(x, z) &&
                    shore == copy.DistanceToShore(x, z), "field changed for same seed");
                var band = depth < 2f ? 0 : depth < 6f ? 1 : depth < 15f ? 2 : depth < 30f ? 3 : 4;
                counts[band]++;
                maxDepth = Mathf.Max(maxDepth, depth);
                if (basin.DistanceToShore(x + 2f, z) > 0f)
                    maxStep = Mathf.Max(maxStep, Mathf.Abs(y - basin.HeightAt(x + 2f, z)));
            }
            Require(counts[0] > wet / 100 && counts[1] > wet / 50 &&
                counts[2] > wet / 10 && counts[3] > wet / 10 && counts[4] > wet / 200,
                $"insufficient depth area: {string.Join(',', counts)} of {wet}");
            Require(basin.DistanceToShore(basin.DeepCore.X, basin.DeepCore.Y) > 35f &&
                maxDepth >= 30f && maxDepth <= 45f && maxStep < 3.5f,
                $"core/continuity failed: core={basin.DeepCore} depth={maxDepth:F1} step={maxStep:F1}");
            GD.Print($"SHORE_FIELD_SEED seed={seed} size={width:F1}x{height:F1} " +
                $"shore_radius={basin.MinPerimeterRadius:F1}..{basin.MaxPerimeterRadius:F1} " +
                $"inside_max={basin.MaximumInsideDistance:F1} core={basin.DeepCore} " +
                $"core_shore={basin.DistanceToShore(basin.DeepCore.X, basin.DeepCore.Y):F1} " +
                $"max_depth={maxDepth:F1} area_m2={string.Join(',', Array.ConvertAll(counts, c => c * 4))} " +
                $"max_2m_step={maxStep:F2} symmetry_difference={leastPeriodicDifference:F1} " +
                $"approaches={approaches} elapsed_ms={timer.ElapsedMilliseconds}");
        }
        GD.Print($"SHORE_FIELD_CHECK_OK version={GeneratedBasin.AlgorithmVersion} seeds=42,43,44");
    }

    public static void Run(Main main)
    {
        RunStructural();
        var seabed = main.GetNode<SeabedSurface>("Habitat/Seabed");
        var mesh = seabed.GeneratedMesh ?? throw new Exception("generated mesh missing");
        var collision = seabed.GeneratedCollision ?? throw new Exception("generated collision missing");
        Require(mesh.GetSurfaceCount() == 1 && collision.BackfaceCollision,
            "seabed must have one two-sided mesh/collision pair");
        var vertices = mesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Vertex].AsVector3Array();
        var triangles = collision.Data;
        Require(vertices.Length == GeneratedBasin.CellsPerSide * GeneratedBasin.CellsPerSide * 6 &&
            triangles.Length == vertices.Length, "mesh/collision inventory differs");
        for (var i = 0; i < vertices.Length; i++)
            Require(vertices[i].DistanceTo(triangles[i]) < .0001f,
                "collision triangle differs from visual triangle");
        var limits = main.GetNode<StaticBody3D>("Habitat/HiddenLimits");
        foreach (var name in new[] { "West", "East", "North", "South" })
            Require(Mathf.Abs(limits.GetNode<CollisionShape3D>(name).Position.X) >
                seabed.Basin.ShoreMax.X + 10f ||
                Mathf.Abs(limits.GetNode<CollisionShape3D>(name).Position.Z) >
                seabed.Basin.ShoreMax.Y + 10f,
                "fail-safe wall inside natural shoreline");
        GD.Print($"BASIN_CHECK_OK seed={seabed.Basin.Seed} triangles={vertices.Length / 3} collision=identical");
    }

    private static void Require(bool condition, string reason)
    {
        if (!condition) throw new Exception(reason);
    }
}
