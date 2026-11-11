#nullable enable
using System;
using System.Collections.Generic;
using Godot;

namespace Fish;

/// <summary>A finite, bakeable closed mesh and its identical static triangle collision.</summary>
public sealed record GeologicalRegion(string Id, string Kind, ArrayMesh Mesh,
    ConcavePolygonShape3D Collision, Vector3[] Centerline, float NominalFishSideRoom,
    float? NominalCameraHeadroom);

/// <summary>Terrain-selected passages lofted through continuous seabed shoulders.</summary>
public static class GeologicalRegionGenerator
{
    public const string AlgorithmVersion = "c3-geology-terrain-4";
    private const int Sides = 20;
    private const int Stations = 16;
    private const float RouteLift = 1.8f;

    public static IReadOnlyList<GeologicalRegion> Generate(GeneratedBasin basin)
    {
        WorldGenerationAudit.MarkStage();
        var result = new List<GeologicalRegion>(5);
        foreach (var site in basin.GeologySites)
            result.Add(site.Kind == "canyon" ? BuildCanyon(basin, site) : BuildShell(basin, site));
        return result;
    }

    private static GeologicalRegion BuildShell(GeneratedBasin basin, GeologySite site)
    {
        var random = new WorldCompileContext(new WorldGenerationSettings(basin.Seed))
            .Stream("geology", site.Id + "|" + AlgorithmVersion);
        var phase = (float)(random.Next() % 6283) / 1000f;
        var sideways = new Vector2(-site.Axis.Y, site.Axis.X);
        var start = site.Center - site.Axis * site.Length * .5f;
        var end = site.Center + site.Axis * site.Length * .5f;
        var bend = site.Kind == "arch" ? .7f : site.Id == "east-swim-through" ? 2.4f : 1.3f;
        var control = site.Center + sideways * bend * Mathf.Sin(phase);
        var inner = new Vector3[Stations, Sides];
        var outer = new Vector3[Stations, Sides];
        var route = new Vector3[Stations];
        var minimumWidth = float.PositiveInfinity;
        var minimumHeight = float.PositiveInfinity;
        for (var station = 0; station < Stations; station++)
        {
            var t = station / (float)(Stations - 1);
            var p = Bezier(start, control, end, t);
            var tangent = (BezierDerivative(start, control, end, t)).Normalized();
            var side = new Vector2(-tangent.Y, tangent.X);
            var swell = Mathf.Pow(Mathf.Sin(Mathf.Pi * t), 2f);
            var floorLift = .95f + .12f * Mathf.Sin(phase + t * 5f) * swell;
            var bulge = site.Kind == "cave" ? 1.8f : .3f;
            var width = site.HalfWidth + bulge * swell + .24f * Mathf.Sin(phase + t * 7f);
            var height = (site.Kind == "arch" ? 6.8f : 6.6f) +
                (site.Kind == "cave" ? .8f : .3f) * swell + .25f * Mathf.Sin(phase * .7f + t * 9f);
            var floor = basin.HeightAt(p.X, p.Y) + floorLift;
            route[station] = new Vector3(p.X, floor + RouteLift, p.Y);
            minimumWidth = Mathf.Min(minimumWidth, width);
            minimumHeight = Mathf.Min(minimumHeight, height);
            for (var ring = 0; ring < Sides; ring++)
            {
                var angle = ring * Mathf.Tau / Sides;
                var cosine = Mathf.Cos(angle);
                var sine = Mathf.Sin(angle);
                var sideWarp = 1f + .08f * Mathf.Sin(angle * 3f + phase + t * 3f) * swell;
                var innerU = width * cosine * sideWarp + .14f * Mathf.Sin(phase + t * 4f) * swell;
                var innerXZ = p + side * innerU;
                var innerFloor = basin.HeightAt(innerXZ.X, innerXZ.Y) + floorLift;
                inner[station, ring] = new Vector3(innerXZ.X,
                    innerFloor + height * (sine + 1f) * .5f, innerXZ.Y);
                var outerU = (width + 2.5f + .2f * Mathf.Sin(phase + t * 4f)) * cosine;
                var outerXZ = p + side * outerU;
                var outerFloor = basin.HeightAt(outerXZ.X, outerXZ.Y) - 1.7f;
                outer[station, ring] = new Vector3(outerXZ.X,
                    outerFloor + (height + 2.7f) * (sine + 1f) * .5f, outerXZ.Y);
            }
        }
        var mesh = NewMesh(site.Id);
        using var tool = Begin(mesh);
        for (var station = 0; station < Stations - 1; station++)
        for (var ring = 0; ring < Sides; ring++)
        {
            var next = (ring + 1) % Sides;
            var center = route[station].Lerp(route[station + 1], .5f);
            var inward = center - (inner[station, ring] + inner[station, next] +
                inner[station + 1, ring] + inner[station + 1, next]) * .25f;
            Quad(tool, inner[station, ring], inner[station + 1, ring],
                inner[station + 1, next], inner[station, next], inward);
            var outward = (outer[station, ring] + outer[station, next] +
                outer[station + 1, ring] + outer[station + 1, next]) * .25f - center;
            Quad(tool, outer[station, ring], outer[station, next],
                outer[station + 1, next], outer[station + 1, ring], outward);
        }
        var startDirection = route[0] - route[1];
        var endDirection = route[^1] - route[^2];
        for (var ring = 0; ring < Sides; ring++)
        {
            var next = (ring + 1) % Sides;
            Quad(tool, inner[0, ring], inner[0, next], outer[0, next], outer[0, ring], startDirection);
            Quad(tool, inner[Stations - 1, ring], outer[Stations - 1, ring],
                outer[Stations - 1, next], inner[Stations - 1, next], endDirection);
        }
        tool.Commit(mesh);
        var shape = MakeCollision(mesh);
        return new GeologicalRegion(site.Id, site.Kind, mesh, shape, route,
            minimumWidth - .48f, minimumHeight - RouteLift - 2f - .22f);
    }

    private static GeologicalRegion BuildCanyon(GeneratedBasin basin, GeologySite site)
    {
        const int columns = 13;
        var offsets = new[] { -9f, -7f, -5f, -3.8f, -3.1f, -2.6f, 0f,
            2.6f, 3.1f, 3.8f, 5f, 7f, 9f };
        var heights = new[] { -1.3f, -.2f, 2.7f, 5.2f, 4.5f, .75f, .7f,
            .75f, 4.4f, 5.4f, 3.1f, -.2f, -1.3f };
        var top = new Vector3[Stations, columns];
        var bottom = new Vector3[Stations, columns];
        var route = new Vector3[Stations];
        for (var station = 0; station < Stations; station++)
        {
            var t = station / (float)(Stations - 1);
            var p = site.Center + site.Axis * ((t - .5f) * site.Length) +
                new Vector2(-site.Axis.Y, site.Axis.X) * (.8f * Mathf.Sin(t * Mathf.Pi));
            var tangent = site.Axis;
            var side = new Vector2(tangent.Y, -tangent.X);
            route[station] = new Vector3(p.X,
                basin.HeightAt(p.X, p.Y) + heights[6] * (.55f + .45f * Mathf.Sin(Mathf.Pi * t)) + RouteLift, p.Y);
            for (var column = 0; column < columns; column++)
            {
                var q = p + side * offsets[column];
                var fade = Mathf.Sin(Mathf.Pi * t);
                var rise = heights[column] * (.55f + .45f * fade);
                top[station, column] = new Vector3(q.X, basin.HeightAt(q.X, q.Y) + rise, q.Y);
                bottom[station, column] = new Vector3(q.X, basin.HeightAt(q.X, q.Y) - 2.5f, q.Y);
            }
        }
        var mesh = NewMesh("canyon-ridge");
        using var tool = Begin(mesh);
        var outward = new Vector3(site.Axis.Y, 0f, -site.Axis.X);
        var forward = new Vector3(site.Axis.X, 0f, site.Axis.Y);
        for (var station = 0; station < Stations - 1; station++)
        for (var column = 0; column < columns - 1; column++)
        {
            Quad(tool, top[station, column], top[station + 1, column],
                top[station + 1, column + 1], top[station, column + 1], Vector3.Up);
            Quad(tool, bottom[station, column], bottom[station, column + 1],
                bottom[station + 1, column + 1], bottom[station + 1, column], Vector3.Down);
        }
        for (var station = 0; station < Stations - 1; station++)
        {
            Quad(tool, top[station, 0], bottom[station, 0], bottom[station + 1, 0],
                top[station + 1, 0], -outward);
            Quad(tool, top[station, columns - 1], top[station + 1, columns - 1],
                bottom[station + 1, columns - 1], bottom[station, columns - 1], outward);
        }
        for (var column = 0; column < columns - 1; column++)
        {
            Quad(tool, top[0, column], top[0, column + 1], bottom[0, column + 1],
                bottom[0, column], -forward);
            Quad(tool, top[Stations - 1, column], bottom[Stations - 1, column],
                bottom[Stations - 1, column + 1], top[Stations - 1, column + 1], forward);
        }
        tool.Commit(mesh);
        return new GeologicalRegion("canyon-ridge", "canyon", mesh, MakeCollision(mesh), route, 2.6f, null);
    }

    private static ArrayMesh NewMesh(string name) => new() { ResourceName = name };

    private static SurfaceTool Begin(ArrayMesh mesh)
    {
        var tool = new SurfaceTool();
        tool.Begin(Mesh.PrimitiveType.Triangles);
        tool.SetMaterial(new StandardMaterial3D
        {
            ResourceName = "Generated cool reef geology",
            AlbedoColor = new Color(.56f, .65f, .62f),
            Roughness = .95f,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
        });
        return tool;
    }

    private static ConcavePolygonShape3D MakeCollision(ArrayMesh mesh)
    {
        var shape = mesh.CreateTrimeshShape() as ConcavePolygonShape3D
            ?? throw new InvalidOperationException("Generated geological mesh has no triangle collision");
        shape.BackfaceCollision = true;
        return shape;
    }

    private static void Quad(SurfaceTool tool, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 desired)
    {
        if ((b - a).Cross(c - a).Dot(desired) < 0f) (b, d) = (d, b);
        Triangle(tool, a, b, c);
        Triangle(tool, a, c, d);
    }

    private static void Triangle(SurfaceTool tool, Vector3 a, Vector3 b, Vector3 c)
    {
        var normal = (b - a).Cross(c - a).Normalized();
        foreach (var vertex in new[] { a, b, c })
        {
            tool.SetNormal(normal);
            tool.SetUV(new Vector2(vertex.X, vertex.Z) * .12f);
            tool.AddVertex(vertex);
        }
    }

    private static Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, float t) =>
        a * (1f - t) * (1f - t) + 2f * b * t * (1f - t) + c * t * t;

    private static Vector2 BezierDerivative(Vector2 a, Vector2 b, Vector2 c, float t) =>
        2f * (b - a) * (1f - t) + 2f * (c - b) * t;
}
