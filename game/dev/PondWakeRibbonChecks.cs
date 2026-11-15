#nullable enable
using Godot;
using System;

namespace Fish;

/// <summary>W2 bounded mesh, historical heading, local Y and lifecycle checks.</summary>
public static class PondWakeRibbonChecks
{
    public static void Run(PikeEncounter scene)
    {
        try
        {
            var fish = scene.GetNode<FishController>("Fish");
            var water = fish.GetNode<WaterReaction>("WaterReaction");
            var atmosphere = scene.GetNode<UnderwaterAtmosphere>("UnderwaterAtmosphere");
            Need(water.GetNodeOrNull<MeshInstance3D>("SurfaceRing0") is null,
                "production ring pool still present");
            var mesh = new ImmediateMesh();
            var material = new StandardMaterial3D();
            var path = new PondWakePath();
            for (var i = 0; i < 120; i++)
            {
                var t = i / 60f;
                var x = t < .8f ? t * .8f : .64f + (t - .8f) * .3f;
                var z = t < .8f ? 0f : (t - .8f) * (t - .8f) * .5f;
                var heading = t < .8f ? new Vector2(1f, 0f) :
                    new Vector2(.3f, (t - .8f)).Normalized();
                var leftDir = new Vector2(-heading.Y, heading.X);
                var left = new Vector2(x, z) - leftDir * .12f;
                var right = new Vector2(x, z) + leftDir * .12f;
                var contact = new PondWakeContacts(Vector3.Zero,
                    new Vector3(left.X, 0f, left.Y), new Vector3(right.X, 0f, right.Y),
                    Vector3.Zero, new Vector3(heading.X, 0f, heading.Y), 0f, .24f, true);
                path.Step(contact, .5f, .72f, 1, 1f / 60f);
            }
            Need(path.Count > 10 && path.Count <= PondWakePath.Capacity,
                "test route did not fill bounded history");
            var first = path[0];
            var projected = PondWakeRibbon.Project(first, path.Seconds, -1);
            var moved = new PondWakeSample(first.Center + new Vector2(8f, 0f),
                new Vector2(0f, -1f), first.Left + new Vector2(8f, 0f),
                first.Right + new Vector2(8f, 0f), first.BirthSeconds, first.Length,
                first.Width, first.Energy, first.StageRevision, first.Segment);
            Need(PondWakeRibbon.Project(first, path.Seconds, -1) == projected &&
                PondWakeRibbon.Project(moved, path.Seconds, -1).Position.X > projected.Position.X + 7f,
                "old route transformed with current heading");
            var triangles = PondWakeRibbon.Draw(mesh, path, path.Seconds,
                atmosphere.GetRenderedSurfaceHeight, null, material);
            Need(triangles is > 0 and <= PondWakeRibbon.MaxTriangles && mesh.GetSurfaceCount() == 1,
                "mesh triangle budget or assembly");
            var vertices = mesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Vertex].AsVector3Array();
            Need(vertices.Length == triangles * 3, "mesh triangle count");
            var spreadY = false;
            var firstY = vertices[0].Y;
            foreach (var vertex in vertices)
            {
                Need(vertex.IsFinite() && Mathf.Abs(vertex.Y -
                    atmosphere.GetRenderedSurfaceHeight(vertex.X, vertex.Z) - .032f) < .002f,
                    "wake vertex not on its local moving surface");
                if (Mathf.Abs(vertex.Y - firstY) > .0001f) spreadY = true;
            }
            Need(spreadY, "wake was flattened to one Y");
            Need(PondWakeRibbon.Draw(mesh, path, path.Seconds,
                atmosphere.GetRenderedSurfaceHeight, (_, _) => 0f, material) == 0 &&
                mesh.GetSurfaceCount() == 0, "dry shoreline rendered fronts");
            path.Clear();
            Need(PondWakeRibbon.Draw(mesh, path, path.Seconds,
                atmosphere.GetRenderedSurfaceHeight, null, material) == 0,
                "clear retained fronts");
            water.Suspend();
            Need(water.ActiveWakeTriangles == 0, "suspend retained live front");
            GD.Print($"WAKE_RIBBON_CHECK_OK triangles={triangles} cap={PondWakeRibbon.MaxTriangles} " +
                $"vertices={vertices.Length} historic_heading=ok curved=ok local_wave_y=ok dry_shore=ok clear=ok");
            scene.GetTree().Quit();
        }
        catch (Exception error)
        {
            GD.PushError("WAKE_RIBBON_CHECK_FAILED " + error);
            scene.GetTree().Quit(1);
        }
    }

    private static void Need(bool value, string reason)
    {
        if (!value) throw new InvalidOperationException(reason);
    }
}
