#nullable enable
using Godot;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Fish;

/// <summary>P8 loaded-world, menu, stage registry and habitat boundary integration.</summary>
public static class PondCreatureChecks
{
    private static bool _started;
    public static bool Start(Main main)
    {
        if (_started) return false;
        _started = true;
        Run(main);
        return true;
    }

    private static async void Run(Main main)
    {
        var tree = main.GetTree();
        try
        {
            await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            var expected = Enumerable.Range(6, 5).Select(i => $"FAF-FISH-{i:000}").ToArray();
            var stages = new[] { "egg", "larva", "juvenile", "subadult", "adult" };
            Require(PondCreatureCatalog.FishStages.Count == 25 &&
                PondCreatureCatalog.Ambient.Count == 7, "registry count");
            foreach (var asset in PondCreatureCatalog.FishStages.Concat(PondCreatureCatalog.Ambient))
            {
                var path = ProjectSettings.GlobalizePath(asset.ModelPath);
                Require(File.Exists(path) &&
                    Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))
                        .Equals(asset.Sha256, StringComparison.OrdinalIgnoreCase),
                    $"GLB changed or missing: {asset.Id}/{asset.Stage}");
                Require(PondCreatureCatalog.ExportMatches(asset) &&
                    !PondCreatureCatalog.ExportMatches(asset with { Sha256 = new string('0', 64) }),
                    $"GLB hash gate: {asset.Id}/{asset.Stage}");
                Require(ResourceLoader.Load<PackedScene>(asset.ModelPath) is not null,
                    $"GLB import missing: {asset.Id}/{asset.Stage}");
            }
            foreach (var id in expected)
            foreach (var stage in stages)
            {
                var asset = PondCreatureCatalog.Stage(id, stage);
                Require(asset is not null &&
                    SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.PondId, id,
                        stage, SpeciesRole.Ecological, out _, out _) &&
                    !SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.OceanId, id,
                        stage, SpeciesRole.Ecological, out _, out _),
                    $"stage registry/habitat {id}/{stage}");
                Require(asset!.FacingDegrees == (stage == "egg" ? 0f : 180f),
                    $"stage facing metadata {id}/{stage}");
            }
            Require(!SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.PondId,
                "FAF-FISH-999", "egg", SpeciesRole.Ecological, out _, out var unknown) &&
                unknown.StartsWith("SPECIES_ID_UNKNOWN"), "missing stage fallback");
            WorldSession.Select(WorldCatalog.PondId);
            var select = GD.Load<PackedScene>("res://scenes/character_select.tscn")
                .Instantiate<CharacterSelect>();
            tree.Root.AddChild(select);
            await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            for (var index = 0; index < expected.Length; index++)
            {
                var entry = CharacterCatalog.Find(expected[index])!;
                var tile = select.FindChild($"Character_{index}", true, false) as Button;
                Require(tile is not null, $"Pond tile {entry.Id}");
                tile!.EmitSignal(Button.SignalName.Pressed);
                var preview = select.FindChild("PreviewModel", true, false) as Node3D;
                var player = preview?.FindChildren("*", "AnimationPlayer", true, false)
                    .OfType<AnimationPlayer>().FirstOrDefault();
                Require(player is not null && player.IsPlaying() &&
                    player.CurrentAnimation == entry.GlideClip, $"Pond preview {entry.Id}");
                Require(preview!.Transform.Basis.Z.Normalized().Dot(
                    (entry.PresentationTransform.Basis *
                     new Basis(Vector3.Up, Mathf.DegToRad(entry.PreviewYawDegrees))).Z.Normalized()) > .999f,
                    $"Pond preview yaw {entry.Id}");
                var turntable = preview.GetParent<Node3D>();
                Require(Mathf.Abs(turntable.Rotation.Y) < .1f,
                    $"Pond preview reset after species switch {entry.Id}");
                var previewPanel = select.FindChild("LivePreview", true, false)!
                    .GetParent<SubViewportContainer>();
                var beforeDrag = turntable.Rotation.Y;
                previewPanel.EmitSignal(Control.SignalName.GuiInput,
                    new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true });
                previewPanel.EmitSignal(Control.SignalName.GuiInput,
                    new InputEventMouseMotion { Relative = new Vector2(60, 0) });
                previewPanel.EmitSignal(Control.SignalName.GuiInput,
                    new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = false });
                Require(Mathf.Abs(Mathf.Wrap(turntable.Rotation.Y - beforeDrag, -Mathf.Pi, Mathf.Pi)) > .5f,
                    $"Pond preview drag {entry.Id}");
                DemoSelection.CharacterId = entry.Id;
                var world = GD.Load<PackedScene>("res://scenes/main.tscn").Instantiate<Main>();
                tree.Root.AddChild(world);
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                var fish = world.GetNode<FishController>("Fish");
                fish.SetActivePreset(ControlPreset.Builtins()[0]);
                var visual = fish.GetNodeOrNull<Node3D>("PlayerVisual");
                var ambient = world.GetNodeOrNull<FafAmbientPondLife>("Habitat/PondAmbientLife");
                Require(visual is not null && ambient is not null &&
                    ambient.Population.Count == 12 &&
                    fish.GetNode<CollisionShape3D>("Collision").Shape is SphereShape3D shape &&
                    Mathf.Abs(shape.Radius - .48f) < .001f &&
                    fish.GetNodeOrNull<WaterReaction>("WaterReaction") is not null,
                    $"Pond world/player/ambient {entry.Id}");
                var visualAnimation = visual!.FindChildren("*", "AnimationPlayer", true, false)
                    .OfType<AnimationPlayer>().FirstOrDefault();
                Require(visualAnimation is not null && visualAnimation.IsPlaying() &&
                    visualAnimation.HasAnimation(entry.SwimClip) &&
                    visual.GetNodeOrNull<Node>("SwimPresentation") is not null,
                    $"Pond player action {entry.Id}");
                Require(NoseAlignment(fish, visual) > .999f,
                    $"Pond player nose at rest {entry.Id}");
                for (var frame = 0; frame < 2; frame++)
                    await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                SendKey(fish, Key.W, true);
                for (var frame = 0; frame < 12; frame++)
                    await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                Require(fish.PresentationEffort > .8f && fish.PresentationSpeed > .05f,
                    $"Pond swim input {entry.Id} effort={fish.PresentationEffort} speed={fish.PresentationSpeed} armed={fish.GetInputArmed()} held={fish.GetHeldInputCount()} paused={tree.Paused}");
                Require(NoseAlignment(fish, visual) > .999f,
                    $"Pond player nose while swimming {entry.Id}");
                Require(visualAnimation!.CurrentAnimation is not null &&
                    visualAnimation.CurrentAnimation != entry.GlideClip,
                    $"Pond cruise action {entry.Id}");
                SendKey(fish, Key.A, true);
                SendKey(fish, Key.Up, true);
                for (var frame = 0; frame < 6; frame++)
                    await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                Require(fish.LastYaw < 0 && fish.LastPitch > 0,
                    $"Pond turn/climb input {entry.Id}");
                Require(NoseAlignment(fish, visual) > .999f,
                    $"Pond player nose after yaw/pitch {entry.Id}");
                SendKey(fish, Key.W, false);
                SendKey(fish, Key.A, false);
                SendKey(fish, Key.Up, false);
                var basin = world.GetNode<SeabedSurface>("Habitat/Seabed").Basin;
                for (var a = 0; a < ambient!.Population.Count; a++)
                foreach (var time in new[] { 0f, 5f, 20f })
                {
                    var pose = ambient.GetPose(a, time).Origin;
                    Require(basin.DistanceToShore(pose.X, pose.Z) > .3f &&
                        pose.Y <= GeneratedBasin.WaterSurfaceY + .1f &&
                        pose.Y > basin.HeightAt(pose.X, pose.Z) - .1f,
                        $"Pond ambient path escaped habitat: {a}/{time}");
                }
                Require(ambient.Population.Count(a => a.Id.StartsWith("FAF-CREATURE-")) == 3 &&
                    ambient.Population.Count(a => a.Id.StartsWith("FAF-INSECT-")) == 4 &&
                    ambient.Population.Count(a => a.Id.StartsWith("FAF-FISH-")) == 5,
                    "ambient family census");
                world.Free();
                WorldSession.Unload();
                GD.Print($"POND_CHARACTER_OK id={entry.Id} stage=adult ambient=12");
            }
            select.Free();
            DemoSelection.Clear();
            WorldSession.Select(WorldCatalog.OceanId);
            Require(SpeciesCompatibility.Available(WorldCatalog.OceanId, SpeciesRole.Selectable).Count == 5 &&
                SpeciesCompatibility.Available(WorldCatalog.PondId, SpeciesRole.Selectable).Count == 5,
                "cross-world selection census");
            GD.Print("POND_CREATURE_CHECK_OK fish=5 stages=25 creatures=3 insects=4 ambient=12");
            tree.Quit();
        }
        catch (Exception error)
        {
            GD.PushError($"POND_CREATURE_CHECK_FAILED {error}");
            tree.Quit(1);
        }
    }

    private static void Require(bool valid, string message)
    { if (!valid) throw new InvalidOperationException(message); }
    private static float NoseAlignment(FishController fish, Node3D visual)
    {
        var skeleton = visual.FindChildren("*", "Skeleton3D", true, false)
            .OfType<Skeleton3D>().Single();
        var head = skeleton.ToGlobal(skeleton.GetBoneGlobalRest(skeleton.FindBone("spine_01")).Origin);
        var tail = skeleton.ToGlobal(skeleton.GetBoneGlobalRest(skeleton.FindBone("tail_fin")).Origin);
        return (head - tail).Normalized().Dot((-fish.GlobalBasis.Z).Normalized());
    }
    private static void SendKey(FishController fish, Key key, bool pressed) =>
        fish._Input(new InputEventKey { Keycode = key, PhysicalKeycode = key, Pressed = pressed });
}
