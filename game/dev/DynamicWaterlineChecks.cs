#nullable enable
using Godot;
using System;

namespace Fish;

/// <summary>Deterministic wave agreement and moving-boundary integration on the baked Pond.</summary>
public static class DynamicWaterlineChecks
{
    public static async void Start(Main main)
    {
        var tree = main.GetTree();
        try
        {
            var fish = main.GetNode<FishController>("Fish");
            var atmosphere = main.GetNode<UnderwaterAtmosphere>("UnderwaterAtmosphere");
            var basin = main.GetNode<SeabedSurface>("Habitat/Seabed").Basin;
            var surface = main.GetNode<MeshInstance3D>(
                "UnderwaterAtmosphere/WaterEffects/OverheadSurface");
            var shader = ((ShaderMaterial)surface.MaterialOverride).Shader.Code;
            Require(shader.Contains("VERTEX.y += shore_blend * wave_height *") &&
                shader.Contains("0.10 * sin(a) + 0.065 * sin(b) + wave_detail * 0.045 * sin(c)"),
                "shader geometry contract changed");
            var arrays = surface.Mesh.SurfaceGetArrays(0);
            var vertices = arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
            var colors = arrays[(int)Mesh.ArrayType.Color].AsColorArray();
            var settings = atmosphere.CaptureLighting();
            var maxError = 0f;
            var shoreSamples = 0;
            var openSamples = 0;
            // At mesh vertices, the shader reads exactly this COLOR.r. The
            // analytic shoreline approximation is checked separately below.
            for (var i = 0; i < vertices.Length; i += 67)
            {
                var vertex = vertices[i];
                var color = colors[i].R;
                var analytic = PondWaterline.ShoreBlend(basin, vertex.X, vertex.Z);
                Require(Mathf.Abs(color - analytic) < .0041f,
                    $"shore color disagrees with CPU mask i={i} vertex={vertex} color={color:F4} analytic={analytic:F4}");
                if (color is > .05f and < .95f) shoreSamples++;
                if (color > .99f) openSamples++;
                foreach (var time in new[] { 0f, 2.3f, 7.8f, 14.2f })
                {
                    var phase = time * settings.SurfaceWaveSpeed;
                    var expected = color * settings.SurfaceWaveHeight * (
                        .10f * MathF.Sin(vertex.X * .19f + vertex.Z * .07f + phase * .34f) +
                        .065f * MathF.Sin(vertex.X * -.11f + vertex.Z * .17f - phase * .27f) +
                        settings.SurfaceWaveDetail * .045f *
                        MathF.Sin(vertex.X * .31f - vertex.Z * .24f + phase * .47f));
                    maxError = Mathf.Max(maxError, Mathf.Abs(expected - PondWaterline.Displacement(
                        vertex.X, vertex.Z, time, settings.SurfaceWaveHeight,
                        settings.SurfaceWaveSpeed, settings.SurfaceWaveDetail, color)));
                }
            }
            Require(maxError < .00001f && shoreSamples > 0 && openSamples > 0,
                "wave peaks/troughs or shoreline failed to match shader math");
            await Frames(tree, 3);
            var changedSettings = settings.Copy();
            changedSettings.SurfaceWaveHeight = .35f;
            changedSettings.SurfaceWaveSpeed = 1.65f;
            atmosphere.ApplyLighting(changedSettings);
            var material = (ShaderMaterial)surface.MaterialOverride;
            Require(Mathf.Abs((float)material.GetShaderParameter("wave_height") - .35f) < .0001f &&
                Mathf.Abs((float)material.GetShaderParameter("wave_speed") - 1.65f) < .0001f,
                "graphics wave uniforms did not update");
            var settingsPoint = basin.DeepCore;
            var expectedAdjusted = atmosphere.SurfaceHeight + PondWaterline.Displacement(
                settingsPoint.X, settingsPoint.Y, atmosphere.SurfaceTime,
                changedSettings.SurfaceWaveHeight, changedSettings.SurfaceWaveSpeed,
                changedSettings.SurfaceWaveDetail, 1f);
            Require(Mathf.Abs(atmosphere.GetSurfaceHeight(settingsPoint.X, settingsPoint.Y) -
                expectedAdjusted) < .001f, "graphics wave settings did not reach physical sampler");
            atmosphere.ApplyLighting(settings);

            var core = basin.DeepCore;
            var x = core.X; var z = core.Y;
            var pauseTime = atmosphere.SurfaceTime;
            tree.Paused = true;
            await tree.ToSignal(tree.CreateTimer(.12, true), SceneTreeTimer.SignalName.Timeout);
            Require(Mathf.Abs(atmosphere.SurfaceTime - pauseTime) < .000001f,
                "paused presentation clock moved");
            tree.Paused = false;
            await Frames(tree, 2);
            Require(atmosphere.SurfaceTime - pauseTime is > 0f and < .07f,
                "resume jumped wave time");
            fish.ClearAndDisarmInput();
            fish.GlobalPosition = new Vector3(x, fish.PondSurfaceLimitAt(x, z) - .005f, z);
            await Frames(tree, 3);
            var maxFollowGap = 0f;
            var maxStep = 0f;
            var changed = false;
            var firstLimit = fish.PondSurfaceCenterLimit;
            for (var frame = 0; frame < 180; frame++)
            {
                var before = fish.GlobalPosition;
                await Frames(tree, 1);
                var limit = fish.PondSurfaceCenterLimit;
                changed |= Mathf.Abs(limit - firstLimit) > .01f;
                maxFollowGap = Mathf.Max(maxFollowGap, Mathf.Abs(limit - fish.GlobalPosition.Y));
                maxStep = Mathf.Max(maxStep, fish.GlobalPosition.DistanceTo(before));
                Require(fish.GlobalPosition.Y <= limit + .002f, "fish above falling wave");
            }
            Require(changed && maxFollowGap < .04f && maxStep < .04f,
                "surface contact failed to track moving limit smoothly");

            fish.ClearAndDisarmInput();
            fish.GlobalPosition = new Vector3(x, atmosphere.GetSurfaceHeight(x, z) - 4f, z);
            await Frames(tree, 3);
            var deepY = fish.GlobalPosition.Y;
            await Frames(tree, 120);
            Require(Mathf.Abs(fish.GlobalPosition.Y - deepY) < .001f,
                "deep fish bobbed with wave");

            var water = main.GetNode<WaterReaction>("Fish/WaterReaction");
            var radius = fish.BodyRadius;
            Require(WaterReaction.SurfaceContactLevel(1f, 1f, radius * .65f, radius) > 0f &&
                WaterReaction.SurfaceContactLevel(1f, 1f, radius * 3f, radius) == 0f,
                "deep boost incorrectly eligible for surface fronts");
            water.SetPhysicsProcess(false);
            var body = PlayerBodyGeometry.Measure(fish);
            water.WakePath.Clear();
            for (var i = 0; i < 35; i++)
            {
                var travel = z + i * body.Length * .065f;
                var pose = new Transform3D(Basis.Identity, new Vector3(x,
                    atmosphere.GetSurfaceHeight(x, travel) - body.TopReach * .35f, travel));
                water.WakePath.Step(PondWakeContacts.At(body, pose, atmosphere.GetSurfaceHeight),
                    body.Length, .7f, 1, 1f / 60f);
            }
            water.RefreshWakeRenderForProbe();
            var contactTriangles = water.ActiveWakeTriangles;
            Require(contactTriangles is > 0 and <= PondWakeRibbon.MaxTriangles,
                "contact fronts missing or unbounded");
            var wakeMesh = (ImmediateMesh)water.GetNode<MeshInstance3D>("PondWakeFronts").Mesh;
            var wakeVertex = wakeMesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Vertex].AsVector3Array()[0];
            Require(wakeVertex.IsFinite() &&
                wakeVertex.Y <= atmosphere.GetRenderedSurfaceHeight(
                    wakeVertex.X, wakeVertex.Z) + .033f &&
                wakeVertex.Y >= water.WakePath[0].LeftY - .02f,
                "front vertex left its fin-to-surface depth range");
            await Frames(tree, 30);
            water.RefreshWakeRenderForProbe();
            wakeVertex = wakeMesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Vertex].AsVector3Array()[0];
            Require(wakeVertex.IsFinite() &&
                wakeVertex.Y <= atmosphere.GetRenderedSurfaceHeight(
                    wakeVertex.X, wakeVertex.Z) + .033f &&
                wakeVertex.Y >= water.WakePath[0].LeftY - .02f,
                "aged front left its fin-to-surface depth range");
            water.Suspend();
            var deepPose = new Transform3D(Basis.Identity, new Vector3(x,
                atmosphere.GetSurfaceHeight(x, z) - radius * 4f, z));
            for (var i = 0; i < 35; i++)
            {
                deepPose.Origin.Z += body.Length * .065f;
                water.WakePath.Step(PondWakeContacts.At(body, deepPose, atmosphere.GetSurfaceHeight),
                    body.Length, 1f, 1, 1f / 60f);
            }
            water.RefreshWakeRenderForProbe();
            Require(water.ActiveWakeTriangles == 0, "deep boosted fish emitted surface fronts");
            water.SetPhysicsProcess(true);
            var start = Time.GetTicksUsec();
            var sum = 0f;
            for (var i = 0; i < 100000; i++)
                sum += atmosphere.GetSurfaceHeight(x + i % 29, z + i % 31);
            var sampleMs = (Time.GetTicksUsec() - start) / 1000.0;
            Require(float.IsFinite(sum), "surface sampler returned nonfinite height");
            GD.Print($"DYNAMIC_WATERLINE_CHECK_OK vertex_error={maxError:F7} " +
                $"shore={shoreSamples} open={openSamples} follow_gap={maxFollowGap:F4} " +
                $"step={maxStep:F4} deep_drift={Mathf.Abs(fish.GlobalPosition.Y - deepY):F4} " +
                $"sample_100k_ms={sampleMs:F2} contact_triangles={contactTriangles} deep_boost_triangles=0");
            tree.Quit();
        }
        catch (Exception error)
        {
            tree.Paused = false;
            GD.PushError($"DYNAMIC_WATERLINE_CHECK_FAILED {error}");
            tree.Quit(1);
        }
    }

    private static async System.Threading.Tasks.Task Frames(SceneTree tree, int count)
    {
        for (var i = 0; i < count; i++)
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
    }

    private static void Require(bool condition, string reason)
    {
        if (!condition) throw new InvalidOperationException(reason);
    }
}
