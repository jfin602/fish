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
                "deep boost incorrectly eligible for surface rings");
            water.SamplePondSurface(new Vector3(x, atmosphere.GetSurfaceHeight(x, z) -
                    FishController.PondCenterClearanceFraction * radius, z),
                new Vector3(0f, 0f, fish.Tuning.MaxSpeed), 1f, 1f);
            Require(water.ActivePondRings > 0, "contact ring missing");
            var ring = water.GetNode<MeshInstance3D>("SurfaceRing0");
            var ringPosition = ring.GlobalPosition;
            Require(Mathf.Abs(ringPosition.Y - atmosphere.GetSurfaceHeight(
                ringPosition.X, ringPosition.Z) - .045f) < .001f,
                "ring not placed on its own local wave");
            await Frames(tree, 30);
            Require(Mathf.Abs(ring.GlobalPosition.X - ringPosition.X) < .001f &&
                Mathf.Abs(ring.GlobalPosition.Z - ringPosition.Z) < .001f &&
                Mathf.Abs(ring.GlobalPosition.Y - atmosphere.GetSurfaceHeight(
                    ringPosition.X, ringPosition.Z) - .045f) < .002f,
                "ring did not remain world anchored on moving water");
            var contactRings = water.ActivePondRings;
            water.Suspend();
            water.SamplePondSurface(new Vector3(x,
                    atmosphere.GetSurfaceHeight(x, z) - radius * 3f, z),
                new Vector3(0f, 0f, fish.Tuning.MaxSpeed * 1.8f), 1f, 1f);
            Require(water.ActivePondRings == 0, "deep boosted fish emitted a surface ring");
            water.SamplePondSurface(new Vector3(x,
                    atmosphere.GetSurfaceHeight(x, z) + radius * .4f, z),
                new Vector3(0f, 0f, fish.Tuning.MaxSpeed), 1f, 1f);
            Require(water.ActivePondRings == 0, "airborne fish emitted a surface ring");
            var start = Time.GetTicksUsec();
            var sum = 0f;
            for (var i = 0; i < 100000; i++)
                sum += atmosphere.GetSurfaceHeight(x + i % 29, z + i % 31);
            var sampleMs = (Time.GetTicksUsec() - start) / 1000.0;
            Require(float.IsFinite(sum), "surface sampler returned nonfinite height");
            GD.Print($"DYNAMIC_WATERLINE_CHECK_OK vertex_error={maxError:F7} " +
                $"shore={shoreSamples} open={openSamples} follow_gap={maxFollowGap:F4} " +
                $"step={maxStep:F4} deep_drift={Mathf.Abs(fish.GlobalPosition.Y - deepY):F4} " +
                $"sample_100k_ms={sampleMs:F2} contact_rings={contactRings} deep_boost_rings=0");
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
