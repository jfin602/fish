#nullable enable
using Godot;
using System;

namespace Fish;

/// <summary>Bounded, world-space surface fronts built from the historical body stations.</summary>
public static class PondWakeRibbon
{
    public const int CrossVertices = 5;
    public const int Substeps = 2;
    public const int MaxTriangles = 2 * (PondWakePath.Capacity - 1) * Substeps *
        (CrossVertices - 1) * 2 + 16 + 8 * (PondWakePath.Capacity / 4 + 1);
    private static readonly float[] Cross = [-1f, -.48f, 0f, .48f, 1f];
    private static readonly float[] Feather = [0f, .38f, 1f, .38f, 0f];

    public readonly record struct Station(Vector2 Position, Vector2 Outward,
        float HalfWidth, float Strength, int Segment);

    public static Station Project(PondWakeSample sample, float seconds, int side)
    {
        var age = Mathf.Clamp((seconds - sample.BirthSeconds) / PondWakePath.Lifetime, 0f, 1f);
        var left = new Vector2(-sample.Heading.Y, sample.Heading.X);
        var outward = left * (side < 0 ? -1f : 1f);
        var basePoint = side < 0 ? sample.Left : sample.Right;
        var spread = sample.Width * (.18f + .76f * sample.Energy) * Mathf.Pow(age, .82f);
        var noise = Mathf.Sin(sample.Center.X * 17.2f + sample.Center.Y * 12.7f +
            sample.BirthSeconds * 4.1f) * sample.Width * .025f * age;
        var width = sample.Width * (.028f + .13f * age) * (1f + .2f * sample.Energy);
        var fade = Mathf.Pow(1f - age, 1.8f) * Mathf.Clamp(age / .055f, 0f, 1f);
        var breakup = .82f + .18f * Mathf.Sin(sample.Center.X * 21.3f +
            sample.Center.Y * 9.8f + sample.BirthSeconds * 7.3f);
        return new Station(basePoint + outward * (spread + noise), outward,
            Mathf.Max(.001f, width), sample.Energy * fade * breakup, sample.Segment);
    }

    public static int Draw(ImmediateMesh mesh, PondWakePath path, float seconds,
        Func<float, float, float> height, Func<float, float, float>? shore,
        Material material, float strength = 1f,
        PondWakeContacts? currentContact = null, float currentEnergy = 0f)
    {
        mesh.ClearSurfaces();
        if (path.Count < 2 || strength <= 0f) return 0;
        var begun = false;
        var triangles = 0;
        for (var side = -1; side <= 1; side += 2)
        for (var i = 1; i < path.Count; i++)
        {
            var aSample = path[i - 1];
            var bSample = path[i];
            if (aSample.Segment != bSample.Segment) continue;
            var p0 = Project(path[Mathf.Max(0, i - 2)], seconds, side);
            var p1 = Project(aSample, seconds, side);
            var p2 = Project(bSample, seconds, side);
            var p3 = Project(path[Mathf.Min(path.Count - 1, i + 1)], seconds, side);
            if (path[Mathf.Max(0, i - 2)].Segment != aSample.Segment) p0 = p1;
            if (path[Mathf.Min(path.Count - 1, i + 1)].Segment != bSample.Segment) p3 = p2;
            for (var step = 0; step < Substeps; step++)
            {
                var t0 = (float)step / Substeps;
                var t1 = (float)(step + 1) / Substeps;
                var first = Smooth(p0, p1, p2, p3, t0);
                var second = Smooth(p0, p1, p2, p3, t1);
                if (first.Strength < .002f && second.Strength < .002f) continue;
                if (shore is not null && (shore(first.Position.X, first.Position.Y) < .16f ||
                    shore(second.Position.X, second.Position.Y) < .16f)) continue;
                if (!begun)
                {
                    mesh.SurfaceBegin(Mesh.PrimitiveType.Triangles, material);
                    begun = true;
                }
                for (var c = 0; c < CrossVertices - 1; c++)
                {
                    Emit(first, c, strength); Emit(second, c, strength); Emit(first, c + 1, strength);
                    Emit(first, c + 1, strength); Emit(second, c, strength); Emit(second, c + 1, strength);
                    triangles += 2;
                }
            }
        }
        // A short shoulder pressure line leads the historical fronts. It is
        // absent at rest or below the interface and uses the same vertex Y.
        if (currentContact is { Touching: true } contact && currentEnergy > .03f)
        {
            var bow = new Vector2(contact.Bow.X, contact.Bow.Z);
            var heading = new Vector2(contact.Heading.X, contact.Heading.Z).Normalized();
            var left = new Vector2(-heading.Y, heading.X);
            for (var side = -1; side <= 1; side += 2)
            {
                var outward = left * (side < 0 ? -1f : 1f);
                var shoulder = side < 0 ? contact.Left : contact.Right;
                var end = new Vector2(shoulder.X, shoulder.Z);
                var front = new Station(bow + outward * contact.Width * .10f,
                    outward, contact.Width * .025f, currentEnergy * .16f, 0);
                var back = new Station(bow.Lerp(end, .63f), outward,
                    contact.Width * .055f, currentEnergy * .10f, 0);
                if (shore is not null && shore(back.Position.X, back.Position.Y) < .16f) continue;
                BeginIfNeeded();
                for (var c = 0; c < CrossVertices - 1; c++)
                {
                    Emit(front, c, strength); Emit(back, c, strength); Emit(front, c + 1, strength);
                    Emit(front, c + 1, strength); Emit(back, c, strength); Emit(back, c + 1, strength);
                    triangles += 2;
                }
            }
        }
        // Sparse centerline eddies follow old tail stations. The deterministic
        // phase breaks them up without emitting new nodes or a foam texture.
        for (var i = 0; i < path.Count; i += 4)
        {
            var sample = path[i];
            var age = Mathf.Clamp((seconds - sample.BirthSeconds) / PondWakePath.Lifetime, 0f, 1f);
            var fade = sample.Energy * .13f * Mathf.Sin(Mathf.Pi * age) * strength;
            if (fade < .008f) continue;
            var tail = sample.Center - sample.Heading * sample.Length * .42f;
            if (shore is not null && shore(tail.X, tail.Y) < .16f) continue;
            var sideways = new Vector2(-sample.Heading.Y, sample.Heading.X);
            var wobble = Mathf.Sin(sample.BirthSeconds * 13.1f + sample.Center.X * 8f) *
                sample.Width * .09f;
            var first = new Station(tail + sideways * wobble, sideways,
                sample.Width * .10f, fade, sample.Segment);
            var second = first with { Position = first.Position - sample.Heading * sample.Length * .12f,
                Strength = fade * .3f };
            BeginIfNeeded();
            for (var c = 0; c < CrossVertices - 1; c++)
            {
                Emit(first, c, 1f); Emit(second, c, 1f); Emit(first, c + 1, 1f);
                Emit(first, c + 1, 1f); Emit(second, c, 1f); Emit(second, c + 1, 1f);
                triangles += 2;
            }
        }
        if (begun) mesh.SurfaceEnd();
        return triangles;

        void Emit(Station station, int cross, float gain)
        {
            var point = station.Position + station.Outward * Cross[cross] * station.HalfWidth;
            var y = height(point.X, point.Y);
            mesh.SurfaceSetNormal(Vector3.Up);
            mesh.SurfaceSetColor(new Color(1f, 1f, 1f,
                Mathf.Clamp(station.Strength * Feather[cross] * gain, 0f, 1f)));
            mesh.SurfaceSetUV(new Vector2(Cross[cross], station.Position.X * 3.7f + station.Position.Y * 2.1f));
            mesh.SurfaceAddVertex(new Vector3(point.X, y + .032f, point.Y));
        }
        void BeginIfNeeded()
        {
            if (begun) return;
            mesh.SurfaceBegin(Mesh.PrimitiveType.Triangles, material);
            begun = true;
        }
    }

    private static Station Smooth(Station p0, Station p1, Station p2, Station p3, float t)
    {
        var d = p2.Position - p1.Position;
        var tangentA = .5f * (p2.Position - p0.Position);
        var tangentB = .5f * (p3.Position - p1.Position);
        var limit = Mathf.Max(.001f, d.Length() * .8f);
        tangentA = tangentA.LimitLength(limit);
        tangentB = tangentB.LimitLength(limit);
        var t2 = t * t;
        var t3 = t2 * t;
        var point = (2f * t3 - 3f * t2 + 1f) * p1.Position +
            (t3 - 2f * t2 + t) * tangentA +
            (-2f * t3 + 3f * t2) * p2.Position +
            (t3 - t2) * tangentB;
        return new Station(point, p1.Outward.Lerp(p2.Outward, t).Normalized(),
            Mathf.Lerp(p1.HalfWidth, p2.HalfWidth, t),
            Mathf.Lerp(p1.Strength, p2.Strength, t), p1.Segment);
    }
}
