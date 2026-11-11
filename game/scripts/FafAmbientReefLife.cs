#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

/// <summary>Visual-only fish loops rooted at generated habitat sites.</summary>
public partial class FafAmbientReefLife : Node3D
{
    public sealed record BakedSchool(string Name, string Zone, string FishId, int Count,
        float X, float Z, float Lift, float RadiusX, float RadiusZ, float Speed, float Yaw,
        float Floor);
    public sealed record BakedAmbient(IReadOnlyList<BakedSchool> Schools,
        IReadOnlyList<AmbientFish> Fish);
    private readonly record struct School(string Name, string Zone, string FishId, int Count,
        float X, float Z, float Lift, float RadiusX, float RadiusZ, float Speed, float Yaw);
    public readonly record struct AmbientFish(string Id, string Zone, string SchoolName,
        int SchoolIndex, int MemberIndex, string Clip);

    // The templates own population and motion only. X/Z/yaw come from the world plan.
    private static readonly School[] Templates =
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
    private readonly List<School> _schools = new();
    private readonly List<float> _schoolFloors = new();
    private readonly List<Node3D> _actors = new();
    private readonly List<AnimationPlayer> _animations = new();
    private SeabedSurface? _seabed;
    private float _elapsed;

    public IReadOnlyList<AmbientFish> Population => _population;
    public int SchoolCount => _schools.Count;
    public int FishCount => _population.Count;
    public float Elapsed => _elapsed;

    public BakedAmbient ExportBaked() => new(
        _schools.Select((s, i) => new BakedSchool(s.Name, s.Zone, s.FishId, s.Count,
            s.X, s.Z, s.Lift, s.RadiusX, s.RadiusZ, s.Speed, s.Yaw, _schoolFloors[i])).ToArray(),
        _population.ToArray());

    public void RestoreBaked(BakedAmbient state, SeabedSurface seabed)
    {
        if (_population.Count != 0 || _schools.Count != 0 || state.Schools.Count != Templates.Length)
            throw new InvalidOperationException("Invalid baked ambient state");
        foreach (var s in state.Schools)
        {
            _schools.Add(new School(s.Name, s.Zone, s.FishId, s.Count, s.X, s.Z,
                s.Lift, s.RadiusX, s.RadiusZ, s.Speed, s.Yaw));
            _schoolFloors.Add(s.Floor);
        }
        var visuals = GetNode<Node3D>("Visuals");
        foreach (var fish in state.Fish)
        {
            if (fish.SchoolIndex < 0 || fish.SchoolIndex >= _schools.Count ||
                fish.SchoolName != _schools[fish.SchoolIndex].Name)
                throw new InvalidOperationException("Invalid baked ambient fish index");
            var actor = visuals.GetNode<Node3D>($"{fish.SchoolName}_{fish.MemberIndex + 1}");
            var player = actor.GetNode<AnimationPlayer>("Rig/AnimationPlayer");
            if (!player.HasAnimation(fish.Clip)) throw new InvalidOperationException("Missing baked fish animation");
            _population.Add(fish);
            _actors.Add(actor);
            _animations.Add(player);
            player.Play(fish.Clip);
        }
        _seabed = seabed;
    }

    public bool TryBuild(SeabedSurface seabed, IReadOnlyList<GeneratedFafInstance> placements,
        GeneratedWorldRoutes routes, out string diagnostic)
    {
        var visuals = GetNodeOrNull<Node3D>("Visuals");
        if (visuals is null || visuals.GetChildCount() != 0 || _population.Count != 0)
        {
            diagnostic = "FAF_AMBIENT_INVALID action=check-empty-visual-root";
            return false;
        }
        var packedFish = new Dictionary<string, PackedScene>();
        for (var schoolIndex = 0; schoolIndex < Templates.Length; schoolIndex++)
        {
            var template = Templates[schoolIndex];
            var candidates = placements.Where(p =>
                    p.Asset.Family is FafEnvironmentFamily.Coral or FafEnvironmentFamily.Plant or FafEnvironmentFamily.Rock)
                .OrderBy(p => Mathf.Abs(ZoneRank(p.DepthBand) - ZoneRank(template.Zone)) * 3f -
                    p.Suitability - .04f * Mathf.Min(
                        routes.DistanceToRoute(p.Position + Vector3.Up * template.Lift), 6f))
                .ToArray();
            var selected = candidates.FirstOrDefault(p =>
                !_schools.Any(s => new Vector2(s.X, s.Z).DistanceTo(new Vector2(p.Position.X, p.Position.Z)) < 2.5f));
            if (selected is null)
            {
                diagnostic = $"FAF_AMBIENT_INVALID zone={template.Zone} action=no-safe-habitat-site";
                return false;
            }
            var school = template with { Zone = selected.DepthBand,
                X = selected.Position.X, Z = selected.Position.Z,
                Yaw = selected.Yaw };
            _schools.Add(school);
            _schoolFloors.Add(selected.Position.Y);
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
        GD.Print($"FAF_AMBIENT_READY schools={SchoolCount} fish={FishCount} " +
            string.Join(' ', new[] { "shallow", "upper", "middle", "lower", "deep" }
                .Select(zone => $"{zone}={_population.Count(f => f.Zone == zone)}")));
        return true;
    }

    public Transform3D GetPose(int index, float time, SeabedSurface seabed)
    {
        var fish = _population[index];
        var school = _schools[fish.SchoolIndex];
        var phase = time * school.Speed + 2f * Mathf.Pi * fish.MemberIndex / school.Count
            + SchoolIndexPhase(fish.SchoolIndex);
        var c = Mathf.Cos(phase);
        var s = Mathf.Sin(phase);
        var local = new Vector2(school.RadiusX * c, school.RadiusZ * s).Rotated(school.Yaw);
        var tangent = new Vector2(-school.RadiusX * s, school.RadiusZ * c).Rotated(school.Yaw);
        var floor = _schoolFloors[fish.SchoolIndex];
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
    private static int ZoneRank(string zone) => zone switch
    { "shallow" => 0, "upper" => 1, "middle" => 2, "lower" => 3, _ => 4 };

    private static bool HasPhysics(Node node)
    {
        if (node is CollisionObject3D or CollisionShape3D) return true;
        foreach (var child in node.GetChildren())
            if (HasPhysics(child)) return true;
        return false;
    }
}
