#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace Fish;

/// <summary>Fixed visual-only fish loops. Nothing here owns simulation or run state.</summary>
public partial class FafAmbientReefLife : Node3D
{
    private readonly record struct School(string Name, string Zone, string FishId, int Count,
        float X, float Z, float Lift, float RadiusX, float RadiusZ, float Speed, float Yaw);
    public readonly record struct AmbientFish(string Id, string Zone, string SchoolName,
        int SchoolIndex, int MemberIndex, string Clip);

    // These shoulder pockets leave the open lanes, cave mouths and canyon quiet.
    // Upper water carries the animation; the lower/deep reef has only passing accents.
    private static readonly School[] Schools =
    {
        new("WestSun", "shallow", "FAF-FISH-001", 5, -24, 23, 2.7f, 1.35f, .72f, .35f, .3f),
        new("NorthChromis", "shallow", "FAF-FISH-003", 4, -14, 27, 2.9f, 1.1f, .65f, .4f, -.6f),
        new("EastButterflies", "shallow", "FAF-FISH-004", 4, 25, 24, 2.8f, 1.25f, .7f, .32f, .8f),
        new("WestClowns", "upper", "FAF-FISH-002", 4, -23, 13, 2.5f, .95f, .7f, .38f, -.35f),
        new("EastChromis", "upper", "FAF-FISH-003", 4, 25, 13, 3.0f, 1.3f, .8f, .42f, .55f),
        new("WestMiddle", "middle", "FAF-FISH-001", 3, -25, 3, 2.7f, 1.2f, .6f, .34f, .1f),
        new("EastMiddle", "middle", "FAF-FISH-004", 3, 27, -2, 2.8f, 1.1f, .65f, .31f, -.7f),
        new("LowerPass", "lower", "FAF-FISH-002", 2, 26, -12, 2.6f, 1.0f, .6f, .29f, .4f),
        new("DeepPass", "deep", "FAF-FISH-003", 2, 25, -24, 2.5f, 1.15f, .55f, .27f, -.3f),
    };

    private readonly List<AmbientFish> _population = new();
    private readonly List<Node3D> _actors = new();
    private readonly List<AnimationPlayer> _animations = new();
    private SeabedSurface? _seabed;
    private float _elapsed;

    public IReadOnlyList<AmbientFish> Population => _population;
    public int SchoolCount => Schools.Length;
    public int FishCount => _population.Count;
    public float Elapsed => _elapsed;

    public bool TryBuild(SeabedSurface seabed, out string diagnostic)
    {
        var visuals = GetNodeOrNull<Node3D>("Visuals");
        if (visuals is null || visuals.GetChildCount() != 0 || _population.Count != 0)
        {
            diagnostic = "FAF_AMBIENT_INVALID action=check-empty-visual-root";
            return false;
        }
        var packedFish = new Dictionary<string, PackedScene>();
        for (var schoolIndex = 0; schoolIndex < Schools.Length; schoolIndex++)
        {
            var school = Schools[schoolIndex];
            var entry = CharacterCatalog.Find(school.FishId);
            if (entry.Id != school.FishId || entry.Id == "FAF-FISH-005")
            {
                diagnostic = $"FAF_AMBIENT_INVALID school={school.Name} action=check-nonpredator-catalog-id";
                return false;
            }
            if (!packedFish.TryGetValue(entry.Id, out var packed))
            {
                packed = ResourceLoader.Load<PackedScene>(entry.ModelPath);
                if (packed is null)
                {
                    diagnostic = $"FAF_AMBIENT_INVALID id={entry.Id} action=load-catalog-visual";
                    return false;
                }
                packedFish.Add(entry.Id, packed);
            }
            for (var member = 0; member < school.Count; member++)
            {
                var model = packed.Instantiate<Node3D>();
                var animation = model.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
                if (animation is null || !animation.HasAnimation(entry.GlideClip) ||
                    !animation.HasAnimation(entry.SwimClip) || !animation.HasAnimation(entry.FastClip) ||
                    HasPhysics(model))
                {
                    model.Free();
                    diagnostic = $"FAF_AMBIENT_INVALID id={entry.Id} action=check-animation-and-visual-only-import";
                    return false;
                }
                var actor = new Node3D { Name = $"{school.Name}_{member + 1}" };
                actor.SetMeta("faf_id", entry.Id);
                actor.SetMeta("zone", school.Zone);
                actor.SetMeta("school", school.Name);
                visuals.AddChild(actor);
                model.Name = "Rig";
                model.Transform = new Transform3D(entry.PresentationTransform.Basis.Scaled(Vector3.One * .56f), Vector3.Zero);
                actor.AddChild(model);
                var clip = member % 5 == 0 ? entry.GlideClip : member % 4 == 0 ? entry.FastClip : entry.SwimClip;
                animation.Play(clip);
                animation.SpeedScale = .72f + .07f * ((member + schoolIndex) % 4);
                _population.Add(new AmbientFish(entry.Id, school.Zone, school.Name, schoolIndex, member, clip));
                _actors.Add(actor);
                _animations.Add(animation);
                actor.Transform = GetPose(_population.Count - 1, 0, seabed);
            }
        }
        if (_population.Count > 32)
        {
            diagnostic = "FAF_AMBIENT_INVALID action=population-cap";
            return false;
        }
        diagnostic = "";
        _seabed = seabed;
        GD.Print($"FAF_AMBIENT_READY schools={SchoolCount} fish={FishCount} shallow=13 upper=8 middle=6 lower=2 deep=2");
        return true;
    }

    public Transform3D GetPose(int index, float time, SeabedSurface seabed)
    {
        var fish = _population[index];
        var school = Schools[fish.SchoolIndex];
        var phase = time * school.Speed + 2f * Mathf.Pi * fish.MemberIndex / school.Count
            + SchoolIndexPhase(fish.SchoolIndex);
        var c = Mathf.Cos(phase);
        var s = Mathf.Sin(phase);
        var local = new Vector2(school.RadiusX * c, school.RadiusZ * s).Rotated(school.Yaw);
        var tangent = new Vector2(-school.RadiusX * s, school.RadiusZ * c).Rotated(school.Yaw);
        var floor = seabed.GetHeightAt(school.X, school.Z);
        var y = Mathf.Min(8.25f, floor + school.Lift) + .13f * Mathf.Sin(phase * 2f + fish.MemberIndex);
        var yaw = Mathf.Atan2(-tangent.X, -tangent.Y);
        return new Transform3D(new Basis(Vector3.Up, yaw),
            new Vector3(school.X + local.X, y, school.Z + local.Y));
    }

    public override void _Process(double delta)
    {
        if (delta <= 0 || GetTree().Paused || _population.Count == 0) return;
        if (_seabed is not { } seabed) return;
        _elapsed += (float)delta;
        for (var i = 0; i < _actors.Count; i++)
        {
            _actors[i].Transform = GetPose(i, _elapsed, seabed);
            var player = _animations[i];
            var clip = player.GetAnimation(_population[i].Clip);
            if (clip is not null && clip.LoopMode == Animation.LoopModeEnum.None &&
                player.CurrentAnimationPosition >= clip.Length - (float)delta * player.SpeedScale)
                player.Seek(0, true);
        }
    }

    private static float SchoolIndexPhase(int index) => .37f * index;

    private static bool HasPhysics(Node node)
    {
        if (node is CollisionObject3D or CollisionShape3D) return true;
        foreach (var child in node.GetChildren())
            if (HasPhysics(child)) return true;
        return false;
    }
}
