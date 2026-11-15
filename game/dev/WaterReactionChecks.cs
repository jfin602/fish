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
            water.ActivePondRings <= WaterReaction.PondRingCapacity,
            "stage scale or particle budget invalid");
        return water;
    }

    public static void CheckEgg(PikeEncounter scene)
    {
        var fish = scene.GetNode<FishController>("Fish");
        Need(fish.GetChildren().OfType<WaterReaction>().Count() == 1 &&
            fish.GetNode<GpuParticles3D>("WaterReaction/LeftWake").AmountRatio == 0f &&
            fish.GetNode<GpuParticles3D>("WaterReaction/RightWake").AmountRatio == 0f &&
            fish.GetNode<WaterReaction>("WaterReaction").ActivePondRings == 0,
            "egg emitted swimming effects");
    }

    public static void CheckSurface(PikeEncounter scene)
    {
        var water = CheckStage(scene);
        var fish = scene.GetNode<FishController>("Fish");
        var radius = scene.Growth.Body.Radius;
        var depth = radius * .7f;
        Need(WaterReaction.SurfaceContactLevel(1f, 1f, depth, radius) > .025f &&
            WaterReaction.SurfaceContactLevel(1f, 1f, radius * 2f, radius) == 0f,
            "body-dependent surface proximity invalid");
        var position = new Vector3(0f, fish.PondWaterlineHeight - depth, 0f);
        for (var i = 0; i < 35; i++)
            water.SamplePondSurface(position, Vector3.Forward * fish.Tuning.MaxSpeed, 1f, 1f / 60f);
        Need(water.ActivePondRings is > 0 and <= WaterReaction.PondRingCapacity,
            "near-surface movement did not emit bounded rings");
        water.Suspend();
        for (var i = 0; i < 35; i++)
            water.SamplePondSurface(position - Vector3.Up * 3f,
                Vector3.Forward * fish.Tuning.MaxSpeed, 1f, 1f / 60f);
        Need(water.ActivePondRings == 0, "deep movement emitted rings");
    }

    private static void Need(bool valid, string message)
    {
        if (!valid) throw new Exception("WATER_REACTION_CHECK_FAILED " + message);
    }
}
