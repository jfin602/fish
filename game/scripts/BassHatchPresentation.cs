#nullable enable
using Godot;
using System;

namespace Fish;

/// <summary>Registered Bass egg and measured shell opening shared by both Pond sessions.</summary>
public static class BassHatchPresentation
{
    public readonly record struct Egg(Node3D Visual, AnimationPlayer Animation, double HatchLength);

    public static Egg Create(Node3D owner, Vector3 nestPosition, Vector3 larvaSpawn,
        Func<float, float, float> bed)
    {
        var asset = PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "egg")
            ?? throw new InvalidOperationException("BASS_EGG_UNREGISTERED");
        if (!PondCreatureCatalog.ExportMatches(asset))
            throw new InvalidOperationException("BASS_EGG_EXPORT_MISMATCH");
        var scene = CharacterCatalog.ModelScene(asset.ModelPath)
            ?? throw new InvalidOperationException("BASS_EGG_IMPORT_UNAVAILABLE");
        var visual = scene.Instantiate<Node3D>();
        visual.Name = "BassEggVisual";
        owner.AddChild(visual);
        visual.GlobalPosition = nestPosition + Vector3.Up * .045f;
        // This is a nest prop scale. Authoritative larva dimensions stay registered.
        visual.Scale = Vector3.One * (12f /
            PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "adult")!.Dimensions[1]);
        GroundShell(visual, larvaSpawn, bed);
        var animation = visual.FindChild("AnimationPlayer", true, false) as AnimationPlayer;
        if (animation is null || !animation.HasAnimation(asset.Clip("Idle")) ||
            !animation.HasAnimation(asset.Clip("Hatch")))
            throw new InvalidOperationException("BASS_EGG_HATCH_ACTION_UNAVAILABLE");
        var hatch = animation.GetAnimation(asset.Clip("Hatch"));
        if (hatch is null || hatch.Length <= 0f || hatch.GetTrackCount() == 0)
            throw new InvalidOperationException("BASS_EGG_HATCH_ACTION_EMPTY");
        animation.Play(asset.Clip("Idle"));
        foreach (var node in visual.FindChildren("*", "MeshInstance3D", true, false))
            if (node is MeshInstance3D mesh &&
                mesh.Name.ToString().Contains("embryo", StringComparison.OrdinalIgnoreCase))
                mesh.Visible = false;
        return new Egg(visual, animation, hatch.Length);
    }

    private static void GroundShell(Node3D egg, Vector3 larvaSpawn,
        Func<float, float, float> bed)
    {
        for (var pass = 0; pass < 3; pass++)
        {
            var opening = (larvaSpawn - egg.GlobalPosition).Normalized();
            var yaw = Mathf.Atan2(opening.X, opening.Z);
            var pitch = Mathf.Atan2(opening.Y,
                new Vector2(opening.X, opening.Z).Length());
            egg.Rotation = new Vector3(-pitch, yaw, 0f);
            var (bottom, _) = ShellBounds(egg, opening);
            egg.GlobalPosition += Vector3.Up *
                (bed(egg.GlobalPosition.X, egg.GlobalPosition.Z) + .001f - bottom);
        }
    }

    public static (float Bottom, float Front) ShellBounds(Node3D egg, Vector3 opening)
    {
        var bottom = float.PositiveInfinity;
        var front = float.NegativeInfinity;
        foreach (var child in egg.FindChildren("*", "MeshInstance3D", true, false))
        {
            if (child is not MeshInstance3D mesh ||
                !mesh.Name.ToString().Contains("shell", StringComparison.OrdinalIgnoreCase)) continue;
            var box = mesh.GetAabb();
            for (var x = 0; x <= 1; x++)
            for (var y = 0; y <= 1; y++)
            for (var z = 0; z <= 1; z++)
            {
                var corner = mesh.GlobalTransform *
                    (box.Position + box.Size * new Vector3(x, y, z));
                bottom = Mathf.Min(bottom, corner.Y);
                front = Mathf.Max(front, (corner - egg.GlobalPosition).Dot(opening));
            }
        }
        if (!float.IsFinite(bottom) || !float.IsFinite(front))
            throw new InvalidOperationException("BASS_EGG_SHELL_MESH_MISSING");
        return (bottom, front);
    }
}
