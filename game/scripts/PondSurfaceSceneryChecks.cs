#nullable enable
using Godot;
using System;
using System.Linq;

namespace Fish;

/// <summary>Focused baked-world checks for P11 geometry, placement and visual state.</summary>
public static class PondSurfaceSceneryChecks
{
    public static void Run(Main main)
    {
        var habitat = main.GetNode<Node3D>("Habitat");
        var seabed = habitat.GetNode<SeabedSurface>("Seabed");
        var basin = seabed.Basin;
        var atmosphere = main.GetNode<UnderwaterAtmosphere>("UnderwaterAtmosphere");
        var surface = main.GetNode<MeshInstance3D>("UnderwaterAtmosphere/WaterEffects/OverheadSurface");
        var water = main.GetNode<WaterReaction>("Fish/WaterReaction");
        var fish = main.GetNode<FishController>("Fish");
        var camera = main.GetNode<Camera3D>("Fish/CameraRig/Camera3D");
        var env = atmosphere.WaterEnvironment ?? throw new Exception("Pond environment missing");
        Require(basin.IsPond && seabed.GeneratedCollision is not null &&
            surface.Mesh is ArrayMesh && surface.Mesh.GetSurfaceCount() == 1 &&
            Mathf.Abs(surface.GlobalPosition.Y - GeneratedBasin.WaterSurfaceY) < .001f,
            "Pond water mesh does not share the generated bank/collision datum");
        var material = (ShaderMaterial)surface.MaterialOverride;
        Require((float)material.GetShaderParameter("shore_mask_enabled") == 1f &&
            (float)material.GetShaderParameter("air_surface_enabled") == 1f &&
            material.Shader.Code.Contains("FRONT_FACING") && material.Shader.Code.Contains("shore_blend"),
            "two-sided shore-clipped material missing");
        var vertices = surface.Mesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Vertex].AsVector3Array();
        Require(vertices.Length is > 10000 and < 70000 && vertices.Length % 3 == 0,
            $"surface tessellation unbounded: {vertices.Length}");
        var outside = 0;
        foreach (var vertex in vertices)
        {
            if (basin.DistanceToShore(vertex.X, vertex.Z) < -.15f) outside++;
            Require(Mathf.Abs(vertex.Y) < .001f, "nonlevel shoreline mesh");
        }
        Require(outside == 0, $"water covers dry bank at {outside} vertices");
        for (var i = 0; i < vertices.Length; i += 3)
            Require((vertices[i + 1] - vertices[i]).Cross(vertices[i + 2] - vertices[i]).Y >= -.0001f,
                "air-facing water triangle has reversed winding");
        var covered = 0;
        for (var i = 0; i < basin.ShorePointCount; i += 16)
        {
            var shore = basin.ShorePoint(i);
            var inward = Inward(basin, shore);
            var wet = shore + inward * 3f;
            var dry = shore - inward * 3f;
            Require(basin.DistanceToShore(wet.X, wet.Y) > 1.8f &&
                basin.DistanceToShore(dry.X, dry.Y) < -1.8f &&
                basin.HeightAt(dry.X, dry.Y) > GeneratedBasin.WaterSurfaceY,
                "baked bank support missing at shore sample");
            if (HasTriangle(vertices, wet)) covered++;
            Require(!HasTriangle(vertices, dry), "water triangle reaches dry bank");
        }
        Require(covered == 16, $"near-bank water coverage {covered}/16");
        var catalog = PondFafEnvironment.LoadCatalog();
        var faf = habitat.GetNode<Node3D>("PondFafEnvironment");
        var shoreCount = (int)faf.GetMeta("surface_shore_count");
        var shorePlan = PondFafEnvironment.PlanSurfaceShore(basin, catalog);
        Require(shoreCount == shorePlan.Count && shoreCount >= 180 && shoreCount <= 190 &&
            faf.GetChildren().OfType<MultiMeshInstance3D>().Any(n =>
                (string)n.GetMeta("faf_id") == "FAF-PLANT-007B") &&
            catalog.Any(a => a.Id == "FAF-WOOD-003A"),
            "supported owner FAF shore dressing missing");
        Require(shorePlan.All(p => Mathf.Abs(p.Position.Y - basin.HeightAt(p.Position.X,
                    p.Position.Z)) <= .101f) &&
            shorePlan.Where(p => p.Id is "FAF-PLANT-006A" or "FAF-PLANT-007B")
                .All(p => basin.DistanceToShore(p.Position.X, p.Position.Z) > .5f &&
                    p.Position.Y < atmosphere.SurfaceHeight) &&
            shorePlan.Where(p => p.Id is "FAF-PLANT-005A" or "FAF-WOOD-003A" or
                "FAF-TERRAIN-002A" or "FAF-TERRAIN-002B")
                .All(p => basin.DistanceToShore(p.Position.X, p.Position.Z) < -.5f) &&
            shorePlan.Count(p => p.Id == "FAF-WOOD-003A") == 16,
            "shore plants, trunks or bank accents lack actual terrain support");
        Require(faf.GetChildren().OfType<MultiMeshInstance3D>().Where(n =>
                (string)n.GetMeta("faf_id") is "FAF-PLANT-004A" or "FAF-PLANT-004B" or
                    "FAF-PLANT-004C" or "FAF-PLANT-009A" or "FAF-PLANT-009B")
                .Select(n => (int)n.GetMeta("placement_count")).Sum() >= 300,
            "lily tops/stems or floating muck are absent from the Pond surface");
        var space = habitat.GetWorld3D().DirectSpaceState;
        var decorative = faf.GetChildren().OfType<Node3D>()
            .SelectMany(n => n.GetChildren().OfType<StaticBody3D>()).ToArray();
        var query = new PhysicsShapeQueryParameters3D
        {
            Shape = new SphereShape3D { Radius = GeneratedWorldRoutes.CameraRadius },
            CollisionMask = 1,
            Exclude = new Godot.Collections.Array<Rid> { fish.GetRid(), seabed.GetRid(),
                habitat.GetNode<StaticBody3D>("HiddenLimits").GetRid() },
        };
        var clearance = 0;
        var authoredObstructions = 0;
        for (var i = 0; i < basin.ShorePointCount; i += 16)
        {
            var shore = basin.ShorePoint(i);
            var wet = shore + Inward(basin, shore) * 3.5f;
            foreach (var y in new[] { GeneratedBasin.WaterSurfaceY - .25f,
                GeneratedBasin.WaterSurfaceY + .8f })
            {
                query.Transform = new Transform3D(Basis.Identity, new Vector3(wet.X, y, wet.Y));
                var hits = space.IntersectShape(query, 1);
                if (hits.Count == 0) clearance++;
                else
                {
                    Require(hits[0]["collider"].AsGodotObject() is StaticBody3D body &&
                        decorative.Contains(body), "unmatched decorative shore collision");
                    authoredObstructions++;
                }
            }
        }
        Require(clearance >= 24, $"too few unobstructed fish/camera bank approaches: {clearance}/32");
        var originalCamera = camera.GlobalTransform;
        var originalFish = fish.GlobalTransform;
        var originalVelocity = fish.Velocity;
        var originalSettings = atmosphere.CaptureLighting();
        try
        {
            var core = basin.DeepCore;
            camera.GlobalPosition = new Vector3(core.X, atmosphere.SurfaceHeight + 2f, core.Y);
            atmosphere.RefreshDepth(true, 2f);
            Require(atmosphere.CameraImmersion01 < .001f &&
                Mathf.Abs(env.FogDepthBegin - WaterVisualProfile.Pond.AirFogBegin) < .01f &&
                Mathf.Abs(env.FogDepthEnd - WaterVisualProfile.Pond.AirFogEnd) < .01f &&
                env.FogDensity > .9f &&
                (float)((ShaderMaterial)env.Sky.SkyMaterial).GetShaderParameter("overcast") == 1f,
                "overcast air/far-bound concealment profile missing");
            Require(WaterReaction.SurfaceContactLevel(1f, 1f, .3f) >
                WaterReaction.SurfaceContactLevel(1f, 0f, .3f) * 3f &&
                WaterReaction.SurfaceContactLevel(1f, 1f, 2f) == 0f &&
                WaterVisualProfile.Ocean.SurfaceWakeStrength == 0f,
                "surface contact, glide or Ocean isolation mapping failed");
            var fishBefore = fish.GlobalTransform;
            var velocityBefore = fish.Velocity;
            var start = Time.GetTicksUsec();
            for (var tick = 0; tick < 240; tick++)
            {
                water.SamplePondSurface(new Vector3(core.X, atmosphere.SurfaceHeight - .25f, core.Y),
                    new Vector3(0, 0, fish.Tuning.MaxSpeed), 1f, 1f / 60f);
                Require(water.ActivePondRings <= WaterReaction.PondRingCapacity,
                    "surface effect pool overran");
            }
            var cpuMs = (Time.GetTicksUsec() - start) / 1000.0;
            Require(water.ActivePondRings > 0 && water.GetNode<GpuParticles3D>("SurfaceSplash").Amount == 6 &&
                fish.GlobalTransform == fishBefore && fish.Velocity == velocityBefore,
                "surface sampling changed motor or failed to emit");
            water.Suspend();
            Require(water.ActivePondRings == 0 &&
                !water.GetNode<GpuParticles3D>("SurfaceSplash").Emitting,
                "surface ring or splash survived pause/focus reset");
            camera.GlobalPosition = new Vector3(core.X, atmosphere.SurfaceHeight - 2f, core.Y);
            atmosphere.RefreshDepth(true, 2f);
            Require(atmosphere.CameraImmersion01 > .999f && env.FogDepthBegin > 4.9f &&
                WaterVisualProfile.For(WorldEnvironmentProfile.Ocean) == WaterVisualProfile.Ocean,
                "water profile failed to restore or Ocean profile leaked");
            GD.Print($"POND_SCENERY_CHECK_OK vertices={vertices.Length} shore_samples={covered}/16 shore_faf={shoreCount} camera_fish_clearance={clearance}/32 authored_obstructions={authoredObstructions} structures={decorative.Length} rings={WaterReaction.PondRingCapacity} splash=6 sample_cpu_ms={cpuMs:F2} ocean_profile=isolated");
        }
        finally
        {
            water.Suspend();
            camera.GlobalTransform = originalCamera;
            fish.GlobalTransform = originalFish;
            fish.Velocity = originalVelocity;
            atmosphere.ApplyLighting(originalSettings);
            atmosphere.RefreshDepth(true, 2f);
        }
    }

    private static Vector2 Inward(GeneratedBasin basin, Vector2 shore) => new Vector2(
        basin.DistanceToShore(shore.X + 1f, shore.Y) - basin.DistanceToShore(shore.X - 1f, shore.Y),
        basin.DistanceToShore(shore.X, shore.Y + 1f) - basin.DistanceToShore(shore.X, shore.Y - 1f)).Normalized();

    private static bool HasTriangle(Vector3[] vertices, Vector2 point)
    {
        for (var i = 0; i < vertices.Length; i += 3)
        {
            var a = new Vector2(vertices[i].X, vertices[i].Z);
            var b = new Vector2(vertices[i + 1].X, vertices[i + 1].Z);
            var c = new Vector2(vertices[i + 2].X, vertices[i + 2].Z);
            var ab = (b - a).Cross(point - a);
            var bc = (c - b).Cross(point - b);
            var ca = (a - c).Cross(point - c);
            if ((ab >= -.001f && bc >= -.001f && ca >= -.001f) ||
                (ab <= .001f && bc <= .001f && ca <= .001f)) return true;
        }
        return false;
    }

    private static void Require(bool condition, string reason)
    {
        if (!condition) throw new Exception(reason);
    }
}
