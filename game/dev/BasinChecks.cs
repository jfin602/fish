#nullable enable
using System;
using Fish;
using Godot;

public static class BasinChecks
{
    public static void Run(Main main)
    {
        var seabed = main.GetNode<SeabedSurface>("Habitat/Seabed");
        var field = seabed.Basin;
        var same = new GeneratedBasin(field.Seed);
        var other = new GeneratedBasin(field.Seed + 1);
        Require(field.DeepCore == same.DeepCore && field.DeepFloorY == same.DeepFloorY &&
            field.MinPerimeterRadius == same.MinPerimeterRadius &&
            field.MaxPerimeterRadius == same.MaxPerimeterRadius, "same seed metadata changed");
        Require(field.DeepCore.Length() < 9f && field.DeepFloorY < -8f,
            "useful minimum escaped central deep core");
        Require(field.MaxPerimeterRadius - field.MinPerimeterRadius > 5f,
            "angular domain is too circular");

        var min = float.PositiveInfinity;
        var maxStep = 0f;
        var deepSum = 0f;
        var edgeSum = 0f;
        var deepCount = 0;
        var edgeCount = 0;
        var changed = false;
        for (var z = -80; z <= 80; z++)
        for (var x = -80; x <= 80; x++)
        {
            var px = x * 0.5f;
            var pz = z * 0.5f;
            var y = field.HeightAt(px, pz);
            Require(float.IsFinite(y), "nonfinite basin sample");
            min = Mathf.Min(min, y);
            if (x < 80) maxStep = Mathf.Max(maxStep, Mathf.Abs(y - field.HeightAt(px + 0.5f, pz)));
            if (z < 80) maxStep = Mathf.Max(maxStep, Mathf.Abs(y - field.HeightAt(px, pz + 0.5f)));
            var distance = Mathf.Sqrt(px * px + pz * pz);
            if (distance <= 8f) { deepSum += y; deepCount++; }
            if (distance is >= 34f and <= 36f) { edgeSum += y; edgeCount++; }
            if (x % 7 == 0 && z % 9 == 0 && field.HeightAt(px, pz) != other.HeightAt(px, pz)) changed = true;
            Require(y == same.HeightAt(px, pz), "same seed dense sample changed");
        }
        Require(Mathf.Abs(min - field.DeepFloorY) < 0.02f && maxStep < 2.5f &&
            edgeSum / edgeCount > deepSum / deepCount + 14f && changed,
            $"deep ordering, continuity, outward rise or seed variation failed min={min:F2} core={field.DeepFloorY:F2} step={maxStep:F2} deep={deepSum / deepCount:F2} edge={edgeSum / edgeCount:F2} changed={changed}");

        var playableLow = float.PositiveInfinity;
        var playableHigh = 0f;
        for (var i = 0; i < 144; i++)
        {
            var angle = i * Mathf.Tau / 144f;
            var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            var exit = 0f;
            for (var radius = 18f; radius <= 37f; radius += 0.25f)
                if (field.HeightAt(direction.X * radius, direction.Y * radius) >= 9f)
                { exit = radius; break; }
            Require(exit > 23f && exit < 36.7f &&
                field.HeightAt(direction.X * 36.7f, direction.Y * 36.7f) > 9f,
                $"ordinary route could reach a square clamp before terrain bank angle={angle:F2} exit={exit:F2} y={field.HeightAt(direction.X * 36.7f, direction.Y * 36.7f):F2}");
            playableLow = Mathf.Min(playableLow, exit);
            playableHigh = Mathf.Max(playableHigh, exit);
        }
        Require(playableHigh - playableLow > 3f, "usable perimeter is too regular");

        var mesh = seabed.GeneratedMesh ?? throw new Exception("generated mesh missing");
        var collision = seabed.GeneratedCollision ?? throw new Exception("generated collision missing");
        Require(mesh.GetSurfaceCount() == 1 && collision.BackfaceCollision,
            "open seabed must have one two-sided mesh/collision pair");
        var vertices = mesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Vertex].AsVector3Array();
        var triangles = collision.Data;
        Require(vertices.Length == GeneratedBasin.CellsPerSide * GeneratedBasin.CellsPerSide * 6 &&
            triangles.Length == vertices.Length, "mesh and collision triangle inventory differs");
        for (var i = 0; i < vertices.Length; i++)
            Require(vertices[i].DistanceTo(triangles[i]) < 0.0001f,
                "collision triangle differs from visual triangle");

        var limits = main.GetNode<StaticBody3D>("Habitat/HiddenLimits");
        foreach (var name in new[] { "West", "East", "North", "South" })
            Require(Mathf.Abs(limits.GetNode<CollisionShape3D>(name).Position.X) >= 46f ||
                Mathf.Abs(limits.GetNode<CollisionShape3D>(name).Position.Z) >= 46f,
                "fail-safe wall is inside natural bank");

        GD.Print($"BASIN_CHECK_OK seed={field.Seed} version={GeneratedBasin.AlgorithmVersion} " +
            $"core=({field.DeepCore.X:F1},{field.DeepCore.Y:F1}) min_y={min:F2} " +
            $"domain_radius={field.MinPerimeterRadius:F2}..{field.MaxPerimeterRadius:F2} " +
            $"usable_radius={playableLow:F2}..{playableHigh:F2} max_half_step={maxStep:F2} " +
            $"triangles={vertices.Length / 3} same_seed=true collision=identical walls=safety-only");
    }

    private static void Require(bool condition, string reason)
    {
        if (!condition) throw new Exception(reason);
    }
}
