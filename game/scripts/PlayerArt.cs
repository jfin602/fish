using Godot;

namespace Fish;

/// <summary>Checked Fish3 import and visual attachment shared by normal play and the P1 fixture.</summary>
public static class PlayerArt
{
    public const string Source = "res://assets/vendor-local/quaternius/Fish3.fbx";

    public static bool TryLoadAndAttach(FishController fish, out string diagnostic)
    {
        if (DemoSelection.CharacterId is null)
        {
            if (WorldSession.SelectedWorldId == WorldCatalog.OceanId)
                return TryLoadLegacy(fish, out diagnostic);
            if (WorldSession.SelectedWorldId == WorldCatalog.PondId &&
                OS.GetEnvironment("FISH_POND_VISUAL_FIXTURE") == "1")
            {
                // The primitive nodes already in main.tscn are a P7 camera fixture.
                // This never registers a selectable or ecological freshwater species.
                diagnostic = "";
                GD.Print("FISH_POND_VISUAL_FIXTURE_ONLY primitive-body no-species-id");
                return true;
            }
            diagnostic = "SPECIES_SELECTION_REQUIRED action=choose-native-species";
            return false;
        }
        if (!SpeciesCompatibility.TryResolve(WorldSession.SelectedWorldId, DemoSelection.CharacterId,
            SpeciesRole.Selectable, out var entry, out diagnostic,
            WorldSession.ActiveManifest?.WorldId)) return false;
        return TryLoadAndAttach(fish, entry!, out diagnostic);
    }

    public static bool TryLoadAndAttach(FishController fish, CharacterEntry entry, out string diagnostic)
    {
        if (!SpeciesCompatibility.TryResolve(WorldSession.SelectedWorldId, entry.Id,
            SpeciesRole.Selectable, out var resolved, out diagnostic) || resolved != entry) return false;
        if (!CharacterCatalog.TryInstantiate(entry, out var visual, out var animation, out diagnostic))
            return false;
        fish.AddChild(visual!);
        visual!.Name = "PlayerVisual";
        visual.Transform = entry.PresentationTransform;
        visual.AddChild(new CatalogFishVisualMotion(fish, animation!, entry));
        ApplyPresentation(fish, entry);
        diagnostic = "";
        return true;
    }

    private static bool TryLoadLegacy(FishController fish, out string diagnostic)
    {
        if (!FileAccess.FileExists(Source))
        {
            diagnostic = $"FISH_ASSET_MISSING item=Quaternius Fish3 path={Source} action=prepare-source-under-game/assets/vendor-local";
            return false;
        }
        var scene = ResourceLoader.Load<PackedScene>(Source);
        if (scene is null)
        {
            diagnostic = $"FISH_ASSET_INVALID item=Quaternius Fish3 path={Source} action=run-tools-dev-import-and-check-dependencies";
            return false;
        }
        return TryAttach(fish, scene, out diagnostic);
    }

    private static void ApplyPresentation(FishController fish, CharacterEntry entry)
    {
        foreach (var part in new[] { "Body", "Tail", "TopFin", "LeftEye", "RightEye" })
            if (fish.GetNodeOrNull<MeshInstance3D>(part) is { } placeholder) placeholder.Visible = false;
    }

    public static bool TryAttach(FishController fish, PackedScene scene, out string diagnostic)
    {
        var visual = scene.Instantiate<Node3D>();
        var skeleton = visual.GetNodeOrNull<Skeleton3D>("Armature/Skeleton3D");
        var mesh = visual.GetNodeOrNull<MeshInstance3D>("Armature/Skeleton3D/ClownFish");
        var animation = visual.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        if (skeleton is null || skeleton.GetBoneCount() < 8 || mesh?.Mesh is null ||
            mesh.Skin is null || mesh.Skin.GetBindCount() < 6 ||
            mesh.Mesh.GetSurfaceCount() < 3 || animation is null ||
            !animation.HasAnimation("Armature|Swim"))
        {
            visual.Free();
            diagnostic = "FISH_ASSET_INVALID item=Quaternius-Fish3 action=check-FBX-mesh-skin-and-swim-import";
            return false;
        }
        var clip = animation.GetAnimation("Armature|Swim");
        if (clip is null || clip.GetTrackCount() == 0 || clip.Length <= 0)
        {
            visual.Free();
            diagnostic = "FISH_ASSET_INVALID item=Quaternius-Fish3 action=check-swim-clip";
            return false;
        }
        for (var track = 0; track < clip.GetTrackCount(); track++)
        {
            if (clip.TrackGetType(track) == Animation.TrackType.Rotation3D) continue;
            visual.Free();
            diagnostic = "FISH_ASSET_INVALID item=Quaternius-Fish3 action=remove-imported-translation-or-scale-track";
            return false;
        }
        for (var surface = 0; surface < mesh.Mesh.GetSurfaceCount(); surface++)
        {
            if (mesh.Mesh.SurfaceGetMaterial(surface) is BaseMaterial3D) continue;
            visual.Free();
            diagnostic = "FISH_ASSET_INVALID item=Quaternius-Fish3 action=check-imported-body-stripes-outline-materials";
            return false;
        }
        var motion = FishVisualMotion.CreateVerified(fish, visual, skeleton, animation, clip);
        if (motion is null)
        {
            visual.Free();
            diagnostic = "FISH_ASSET_INVALID item=Quaternius-Fish3 action=check-Root-Spine3-Tail-swim-tracks";
            return false;
        }

        fish.AddChild(visual);
        visual.Name = "PlayerVisual";
        visual.Scale = Vector3.One * 0.12f;
        // Fish3 imports nose-first toward +Z; the authoritative motor faces -Z.
        visual.RotationDegrees = new Vector3(0, 180, 0);
        visual.AddChild(motion);
        foreach (var part in new[] { "Body", "Tail", "TopFin", "LeftEye", "RightEye" })
            if (fish.GetNodeOrNull<MeshInstance3D>(part) is { } placeholder) placeholder.Visible = false;
        diagnostic = "";
        return true;
    }
}
