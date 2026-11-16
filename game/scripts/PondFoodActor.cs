#nullable enable
using Godot;
using System;

namespace Fish;

/// <summary>Pond non-fish food. The stable ID, not a visual particle, is consumed.</summary>
public partial class PondFoodActor : Node3D
{
    private Vector3 _home;
    private float _phase;
    public string StableId { get; private set; } = "";
    public string Kind { get; private set; } = "";
    public bool Available { get; private set; } = true;

    public void Configure(string id, string kind, Vector3 at, int index)
    {
        StableId = id;
        Kind = kind;
        _home = at;
        _phase = index * 1.37f;
        GlobalPosition = at;
        if (kind == FeedingLedger.MicrofaunaId) BuildZooplankton();
        else if (kind == FeedingLedger.InsectId) BuildInsect();
        else throw new ArgumentException("Unknown pond food", nameof(kind));
    }

    public void Tick(double seconds)
    {
        if (!Available) return;
        // A tiny bounded water-column drift, frozen with the session clock.
        GlobalPosition = _home + Drift(seconds);
    }

    private Vector3 Drift(double seconds)
    {
        var t = (float)seconds;
        return new Vector3(Mathf.Sin(t * .63f + _phase) * .025f,
            Mathf.Sin(t * .81f + _phase) * .013f,
            Mathf.Cos(t * .59f + _phase) * .025f);
    }

    // Keeps the food's drift origin aligned when a check relocates it.
    public void Relocate(Vector3 at, double seconds = 0)
    {
        _home = at - Drift(seconds);
        GlobalPosition = at;
    }

    public void Consume()
    {
        Available = false;
        Visible = false;
    }

    private void BuildZooplankton()
    {
        // A small copepod-like cloud with tapered bodies and antennae, not an orb pickup.
        var shell = new StandardMaterial3D { AlbedoColor = new Color(.29f, .38f, .35f),
            Roughness = .86f, Metallic = 0f };
        var feeler = new StandardMaterial3D { AlbedoColor = new Color(.34f, .43f, .39f),
            Roughness = 1f };
        for (var i = 0; i < 7; i++)
        {
            var angle = i * 2.399f;
            var spread = i == 0 ? 0f : .012f + (i % 3) * .006f;
            var center = new Vector3(Mathf.Cos(angle) * spread,
                (i % 3 - 1) * .007f, Mathf.Sin(angle) * spread);
            var body = new MeshInstance3D { Name = "CopepodBody" + i,
                Mesh = new SphereMesh { Radius = .0034f, Height = .008f },
                MaterialOverride = shell, Position = center,
                Scale = new Vector3(.7f, .75f, 1.4f) };
            AddChild(body);
            for (var side = -1; side <= 1; side += 2)
            {
                var antenna = new MeshInstance3D { Name = "Antenna" + i + "_" + side,
                    Mesh = new CylinderMesh { TopRadius = .00035f, BottomRadius = .00035f,
                        Height = .008f }, MaterialOverride = feeler,
                    Position = center + new Vector3(side * .003f, .003f, -.004f),
                    Rotation = new Vector3(0, 0, side * .55f) };
                AddChild(antenna);
            }
        }
    }

    private void BuildInsect()
    {
        const string path = "res://assets/faf/pond/insects/insect_001a_green_darner_nymph.glb";
        var scene = ResourceLoader.Load<PackedScene>(path)
            ?? throw new InvalidOperationException("POND_INSECT_EXPORT_UNAVAILABLE");
        var model = scene.Instantiate<Node3D>();
        model.Name = "DarnerNymphVisual";
        AddChild(model);
        // Bass is normalized by its adult export; the nymph shares that meter scale.
        model.Scale = Vector3.One / PondCreatureCatalog.Stage(PikeEncounterAssets.BassId, "adult")!.Dimensions[1];
        var animation = model.FindChild("AnimationPlayer", true, false) as AnimationPlayer;
        var clip = "insect_001a_green_darner_nymph__Crawl";
        if (animation is null || !animation.HasAnimation(clip))
            throw new InvalidOperationException("POND_INSECT_ANIMATION_UNAVAILABLE");
        animation.Play(clip);
    }
}
