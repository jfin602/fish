#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

/// <summary>Sparse Pond-only visual life. The baked world, fish motor and ecology own no part of these paths.</summary>
public partial class FafAmbientPondLife : Node3D
{
    public sealed record ActorInfo(string Id, string? Stage, string Habitat, string Clip,
        Vector3 Center, float Radius, float Speed);
    private readonly List<ActorInfo> _population = new();
    private readonly List<Node3D> _actors = new();
    private readonly List<AnimationPlayer> _animations = new();
    private SeabedSurface _seabed = null!;
    private float _time;
    public IReadOnlyList<ActorInfo> Population => _population;

    public static FafAmbientPondLife Install(Node3D habitat)
    {
        if (habitat.GetNodeOrNull<FafAmbientPondLife>("PondAmbientLife") is not null)
            throw new InvalidOperationException("POND_AMBIENT_ALREADY_INSTALLED");
        var seabed = habitat.GetNode<SeabedSurface>("Seabed");
        if (!seabed.Basin.IsPond) throw new InvalidOperationException("POND_AMBIENT_WRONG_HABITAT");
        var life = new FafAmbientPondLife { Name = "PondAmbientLife", _seabed = seabed };
        habitat.AddChild(life);
        try { life.Populate(); }
        catch { habitat.RemoveChild(life); life.Free(); throw; }
        return life;
    }

    private void Populate()
    {
        var entries = PondCreatureCatalog.Ambient
            .Select(asset => (Asset: asset, Fish: (CharacterEntry?)null))
            .Concat(PondCreatureCatalog.AdultChoices().Select(fish =>
                (Asset: PondCreatureCatalog.Stage(fish.Id, "adult")!, Fish: (CharacterEntry?)fish)))
            .ToArray();
        if (entries.Length != 12) throw new InvalidOperationException("POND_AMBIENT_ROSTER_INCOMPLETE");
        for (var i = 0; i < entries.Length; i++)
        {
            var (asset, fish) = entries[i];
            const SpeciesRole role = SpeciesRole.Ambient;
            if (fish is null ?
                !SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.PondId, asset.Id, null,
                    role, out _, out var diagnostic) :
                !SpeciesCompatibility.TryResolve(WorldCatalog.PondId, asset.Id,
                    role, out _, out diagnostic))
                throw new InvalidOperationException($"POND_AMBIENT_REJECTED id={asset.Id} {diagnostic}");
            var scene = ResourceLoader.Load<PackedScene>(asset.ModelPath)
                ?? throw new InvalidOperationException($"POND_AMBIENT_MODEL_MISSING id={asset.Id}");
            var model = scene.Instantiate<Node3D>();
            var animation = model.FindChildren("*", "AnimationPlayer", true, false)
                .OfType<AnimationPlayer>().FirstOrDefault();
            var clipRole = fish is not null ? "Glide" : asset.Id switch
            {
                "FAF-CREATURE-001" => "IdleShore",
                "FAF-CREATURE-002" => "SurfaceSwim",
                "FAF-CREATURE-003" => "BottomWalk",
                _ when asset.Id.StartsWith("FAF-INSECT-001", StringComparison.Ordinal) => "Crawl",
                _ => "Row"
            };
            var clip = asset.Clip(clipRole);
            if (animation is null || !animation.HasAnimation(clip) || HasPhysics(model))
            { model.Free(); throw new InvalidOperationException($"POND_AMBIENT_RIG_INVALID id={asset.Id}"); }
            var site = Site(i, fish is not null);
            var floor = _seabed.GetHeightAt(site.X, site.Y);
            var water = GeneratedBasin.WaterSurfaceY;
            var y = fish is not null ? Mathf.Min(water - .5f, floor + 1.2f) : asset.Id switch
            {
                "FAF-CREATURE-001" => floor + .08f,
                "FAF-CREATURE-002" => water - .04f,
                "FAF-CREATURE-003" => floor + .12f,
                _ when asset.Id.StartsWith("FAF-INSECT-001", StringComparison.Ordinal) => floor + .035f,
                _ => water - .02f
            };
            var radius = fish is not null ? .55f : asset.Id switch
            {
                "FAF-CREATURE-001" => 0f,
                "FAF-CREATURE-002" => .30f,
                "FAF-CREATURE-003" => .12f,
                _ when asset.Id.StartsWith("FAF-INSECT-001", StringComparison.Ordinal) => .055f,
                _ => .16f
            };
            var speed = fish is not null ? .28f : asset.Id.StartsWith("FAF-INSECT-", StringComparison.Ordinal) ? .32f : .18f;
            var actor = new Node3D { Name = asset.Id.Replace('-', '_') };
            actor.SetMeta("faf_id", asset.Id);
            actor.SetMeta("faf_habitat", WorldCatalog.PondId);
            actor.SetMeta("faf_stage", asset.Stage ?? "ambient");
            AddChild(actor);
            actor.AddChild(model);
            model.Name = "Rig";
            if (fish is not null) model.Scale = Vector3.One * (fish.WorldScale * .60f);
            if (asset.Id.StartsWith("FAF-INSECT-002", StringComparison.Ordinal))
                model.Rotation = new Vector3(-.25f, 0, Mathf.Pi);
            animation.Play(clip);
            _population.Add(new ActorInfo(asset.Id, asset.Stage, WorldCatalog.PondId,
                clip, new Vector3(site.X, y, site.Y), radius, speed));
            _actors.Add(actor);
            _animations.Add(animation);
            actor.Transform = GetPose(i, 0);
        }
        GD.Print($"POND_AMBIENT_READY actors={_population.Count} creatures=3 insects=4 fish=5");
    }

    private Vector2 Site(int index, bool fish)
    {
        var basin = _seabed.Basin;
        var shore = basin.ShorePoint((index * 19 + 11) % basin.ShorePointCount);
        var toward = (basin.DeepCore - shore).Normalized();
        foreach (var offset in fish ? new[] { 11f, 14f, 17f, 20f } : new[] { 3f, 5f, 7f, 9f })
        {
            var point = shore + toward * offset;
            var depth = GeneratedBasin.WaterSurfaceY - basin.HeightAt(point.X, point.Y);
            if (basin.DistanceToShore(point.X, point.Y) > (fish ? 3f : 1f) &&
                depth > (fish ? 1.8f : .25f)) return point;
        }
        throw new InvalidOperationException($"POND_AMBIENT_SITE_MISSING id={index}");
    }

    public Transform3D GetPose(int index, float time)
    {
        var info = _population[index];
        var phase = time * info.Speed + index * .43f;
        var offset = new Vector2(Mathf.Cos(phase), Mathf.Sin(phase)) * info.Radius;
        var position = info.Center + new Vector3(offset.X, 0, offset.Y);
        if (_seabed.Basin.DistanceToShore(position.X, position.Z) < .4f)
            position = info.Center;
        var floor = _seabed.GetHeightAt(position.X, position.Z);
        if (info.Id.StartsWith("FAF-FISH-", StringComparison.Ordinal))
            position.Y = Mathf.Max(floor + .35f, Mathf.Min(position.Y, GeneratedBasin.WaterSurfaceY - .45f));
        else if (info.Id is "FAF-CREATURE-003" || info.Id.StartsWith("FAF-INSECT-001", StringComparison.Ordinal))
            position.Y = floor + (info.Id is "FAF-CREATURE-003" ? .12f : .035f);
        var yaw = info.Radius > 0 ? -phase + Mathf.Pi * .5f : 0f;
        return new Transform3D(new Basis(Vector3.Up, yaw), position);
    }

    public override void _Process(double delta)
    {
        if (WorldSession.SelectedWorldId != WorldCatalog.PondId)
        { ClearPopulation(); SetProcess(false); return; }
        if (delta <= 0 || GetTree().Paused) return;
        _time += (float)delta;
        for (var i = 0; i < _actors.Count; i++)
        {
            _actors[i].Transform = GetPose(i, _time);
            var animation = _animations[i];
            var clip = animation.GetAnimation(_population[i].Clip);
            if (clip is not null && clip.LoopMode == Animation.LoopModeEnum.None &&
                animation.CurrentAnimationPosition >= clip.Length - (float)delta)
                animation.Seek(0, true);
        }
    }

    public void ClearPopulation()
    {
        foreach (var actor in _actors)
            if (GodotObject.IsInstanceValid(actor))
            { RemoveChild(actor); actor.QueueFree(); }
        _population.Clear();
        _actors.Clear();
        _animations.Clear();
        _time = 0;
    }

    private static bool HasPhysics(Node node) => node is CollisionObject3D or CollisionShape3D ||
        node.GetChildren().Any(HasPhysics);
}
