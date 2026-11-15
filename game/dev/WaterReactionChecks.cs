#nullable enable
using Godot;
using System;
using System.Linq;

namespace Fish;

/// <summary>Focused stage, surface and budget assertions for the shared presentation component.</summary>
public static class WaterReactionChecks
{
    public static WaterReaction CheckStage(PikeEncounter scene)
    {
        var fish = scene.GetNode<FishController>("Fish");
        var effects = fish.GetChildren().OfType<WaterReaction>().ToArray();
        Need(effects.Length == 1, "Pike must own exactly one WaterReaction");
        var water = effects[0];
        var body = PlayerBodyGeometry.Measure(fish);
        var tail = body.TailZ + body.Radius * .08f;
        var wakeCenter = tail - body.Length * .22f;
        var left = water.GetNode<GpuParticles3D>("LeftWake");
        var right = water.GetNode<GpuParticles3D>("RightWake");
        var bubbles = water.GetNode<GpuParticles3D>("TailBubbles");
        Need(Mathf.Abs(left.Position.Z - wakeCenter) < .0001f &&
            Mathf.Abs(right.Position.Z - wakeCenter) < .0001f &&
            Mathf.Abs(bubbles.Position.Z - (tail - body.Length * .10f)) < .0001f &&
            Mathf.Abs(left.Position.X + body.Radius * .42f) < .0001f &&
            Mathf.Abs(right.Position.X - body.Radius * .42f) < .0001f,
            "emitters drifted from measured body");
        var wakeProcess = (ParticleProcessMaterial)left.ProcessMaterial;
        Need(wakeProcess.EmissionShape == ParticleProcessMaterial.EmissionShapeEnum.Box &&
            Mathf.Abs(wakeProcess.EmissionBoxExtents.Z - body.Length * .25f) < .0001f &&
            wakeProcess.LifetimeRandomness >= .5f,
            "wake does not vary its body origin and lifespan");
        var scale = Mathf.Clamp(body.Length / .95f, .025f, 4f);
        var pondTint = ((StandardMaterial3D)((SphereMesh)left.DrawPass1).Material).AlbedoColor;
        Need(Mathf.Abs(((SphereMesh)left.DrawPass1).Radius - .012f * scale) < .001f &&
            Mathf.Abs(((SphereMesh)bubbles.DrawPass1).Radius - .018f * scale) < .0001f &&
            pondTint.G < .4f && pondTint.B < .4f &&
            left.Amount == 16 && right.Amount == 16 && bubbles.Amount == 12 &&
            !left.TrailEnabled && !right.TrailEnabled &&
            !left.LocalCoords && !right.LocalCoords && !bubbles.LocalCoords &&
            water.GetNode<GpuParticles3D>("SurfaceSplash").Amount == 6 &&
            water.GetNode<MeshInstance3D>("PondWakeFronts").Mesh is ImmediateMesh &&
            water.GetNodeOrNull<MeshInstance3D>("SurfaceRing0") is null,
            "stage scale or particle budget invalid");
        return water;
    }

    public static void CheckEgg(PikeEncounter scene)
    {
        var fish = scene.GetNode<FishController>("Fish");
        Need(fish.GetChildren().OfType<WaterReaction>().Count() == 1 &&
            fish.GetNode<GpuParticles3D>("WaterReaction/LeftWake").AmountRatio == 0f &&
            fish.GetNode<GpuParticles3D>("WaterReaction/RightWake").AmountRatio == 0f &&
            fish.GetNode<WaterReaction>("WaterReaction").ActiveWakeTriangles == 0,
            "egg emitted swimming effects");
    }

    public static void CheckSurface(PikeEncounter scene)
    {
        var water = CheckStage(scene);
        var fish = scene.GetNode<FishController>("Fish");
        var radius = scene.Growth.Body.Radius;
        var depth = PlayerBodyGeometry.Measure(fish).TopReach * .8f;
        Need(WaterReaction.SurfaceContactLevel(1f, 1f, depth, radius) > .025f &&
            WaterReaction.SurfaceContactLevel(1f, 1f, radius * 2f, radius) == 0f,
            "body-dependent surface proximity invalid");
        var body = PlayerBodyGeometry.Measure(fish);
        for (var i = 0; i < 35; i++)
        {
            var z = -body.Length + i * body.Length * .06f;
            var pose = new Transform3D(Basis.Identity, new Vector3(0f,
                fish.PondSurfaceHeightAt(0f, z) - body.TopReach * .35f, z));
            var contact = PondWakeContacts.At(body, pose, fish.PondSurfaceHeightAt);
            water.WakePath.Step(contact, body.Length, .7f, 1, 1f / 60f);
        }
        water.RefreshWakeRenderForProbe();
        Need(water.ActiveWakeTriangles is > 0 and <= PondWakeRibbon.MaxTriangles,
            "near-surface movement did not emit bounded fronts");
        var mesh = (ImmediateMesh)water.GetNode<MeshInstance3D>("PondWakeFronts").Mesh;
        var vertex = mesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Vertex].AsVector3Array()[0];
        Need(vertex.Y < fish.PondSurfaceHeightAt(vertex.X, vertex.Z) + .033f &&
            vertex.Y > fish.PondSurfaceHeightAt(vertex.X, vertex.Z) - .30f,
            "wake vertex left its fin-to-surface depth range");
        water.Suspend();
        Need(water.ActiveWakeTriangles == 0 && mesh.GetSurfaceCount() == 0,
            "pause retained surface fronts");
    }

    private static void Need(bool valid, string message)
    {
        if (!valid) throw new Exception("WATER_REACTION_CHECK_FAILED " + message);
    }
}
