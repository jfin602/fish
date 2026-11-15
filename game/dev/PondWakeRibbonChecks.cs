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
            var last = path[path.Count - 1];
            var live = new PondWakeContacts(
                new Vector3(last.Center.X, 0f, last.Center.Y - last.Length * .2f),
                new Vector3(last.Left.X, 0f, last.Left.Y),
                new Vector3(last.Right.X, 0f, last.Right.Y), Vector3.Zero,
                new Vector3(last.Heading.X, 0f, last.Heading.Y), 0f, last.Width, true)
            {
                FinLeft = new Vector3(last.Left.X, -.12f, last.Left.Y - .04f),
                FinRight = new Vector3(last.Right.X, -.12f, last.Right.Y - .04f),
                HasFinOrigins = true,
            };
            var liveTriangles = PondWakeRibbon.Draw(mesh, path, path.Seconds,
                atmosphere.GetRenderedSurfaceHeight, null, material, 1f, live, .72f, 1.85f);
            Need(liveTriangles == triangles + 32 && liveTriangles <= PondWakeRibbon.MaxTriangles,
                "current fins not bridged to historical fronts");
            triangles = PondWakeRibbon.Draw(mesh, path, path.Seconds,
                atmosphere.GetRenderedSurfaceHeight, null, material);
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
            var finDepthPath = new PondWakePath();
            var finDepthY = atmosphere.GetRenderedSurfaceHeight(0f, 0f) - .12f;
            Need(!PondUnderwaterTurnWake.Update(false, 41f, .6f, .7f, false, false) &&
                PondUnderwaterTurnWake.Update(false, 42f, .6f, .7f, false, false) &&
                PondUnderwaterTurnWake.Update(true, 22f, .6f, .7f, false, false) &&
                !PondUnderwaterTurnWake.Update(true, 21f, .6f, .7f, false, false) &&
                !PondUnderwaterTurnWake.Update(false, 90f, .6f, .2f, true, false) &&
                !PondUnderwaterTurnWake.Update(false, 90f, .1f, .7f, false, false) &&
                PondUnderwaterTurnWake.Update(false, 0f, .01f, .7f, false, true) &&
                !PondUnderwaterTurnWake.Update(true, 0f, .6f, .7f, false, false) &&
                !PondUnderwaterTurnWake.Update(false, 0f, .6f, .1f, true, true),
                "underwater turn/boost entry, release, surface or motion gate");
            for (var i = 0; i < 4; i++)
            {
                var offset = i * .08f;
                var deepContact = new PondWakeContacts(Vector3.Zero,
                    new Vector3(offset, 0f, -.12f), new Vector3(offset, 0f, .12f),
                    Vector3.Zero, Vector3.Right, 0f, .24f, true)
                {
                    FinLeft = new Vector3(offset, finDepthY, -.12f),
                    FinRight = new Vector3(offset, finDepthY, .12f),
                    HasFinOrigins = true,
                    UnderwaterTurn = true,
                };
                finDepthPath.Step(deepContact, .5f, .72f, 1, .06f);
            }
            Need(PondWakeRibbon.Draw(mesh, finDepthPath, finDepthPath.Seconds,
                atmosphere.GetRenderedSurfaceHeight, null, material) > 0,
                "fin-depth trail missing");
            var depthVertices = mesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Vertex]
                .AsVector3Array();
            foreach (var vertex in depthVertices)
                Need(Mathf.Abs(vertex.Y - finDepthY) < .001f,
                    "underwater turn ribbon rose from the fins to the surface");
            var turnColors = mesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Color].AsColorArray();
            foreach (var color in turnColors)
                Need(color.R < .5f, "underwater turn used air-facing surface tint");
            var turnSegment = finDepthPath[finDepthPath.Count - 1].Segment;
            for (var i = 0; i < 3; i++)
            {
                var offset = .32f + i * .08f;
                finDepthPath.Step(new PondWakeContacts(Vector3.Zero,
                    new Vector3(offset, finDepthY, -.12f),
                    new Vector3(offset, finDepthY, .12f), Vector3.Zero,
                    Vector3.Right, 0f, .24f, true)
                {
                    FinLeft = new Vector3(offset, finDepthY, -.12f),
                    FinRight = new Vector3(offset, finDepthY, .12f),
                    HasFinOrigins = true,
                }, .5f, .72f, 1, .06f);
            }
            Need(finDepthPath[finDepthPath.Count - 1].Segment != turnSegment,
                "surface/underwater mode switch bridged two water levels");
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
