using Godot;
using System;

namespace Fish;

/// <summary>Local P1 art boundary. Source files are installed under ignored vendor-local storage.</summary>
public static class StarterWorld
{
    private const string Base = "res://assets/vendor-local/";
    private static readonly (string Item, string Path)[] Required =
    [
        ("Quaternius Fish3", Base + "quaternius/Fish3.fbx"),
        ("Rock Face 01", Base + "rock_face_01/rock_face_01_2k.gltf"),
        ("Coast Sand 01 diffuse", Base + "coast_sand_01/coast_sand_01_Diffuse_2k.jpg"),
        ("Coast Sand 01 normal", Base + "coast_sand_01/coast_sand_01_nor_gl_2k.jpg"),
        ("Coast Sand 01 roughness", Base + "coast_sand_01/coast_sand_01_Rough_2k.jpg"),
        ("Heliopora coral", Base + "heliopora-coerulea.glb"),
    ];
    private static readonly string[] RockDependencies =
    [
        Base + "rock_face_01/rock_face_01.bin",
        Base + "rock_face_01/textures/rock_face_01_arm_2k.jpg",
        Base + "rock_face_01/textures/rock_face_01_diff_2k.jpg",
        Base + "rock_face_01/textures/rock_face_01_nor_gl_2k.jpg",
    ];

    public static bool TryPopulate(Node3D main, FishController fish, out string diagnostic)
    {
        foreach (var path in RockDependencies)
        {
            if (!FileAccess.FileExists(path))
            {
                diagnostic = $"FISH_ASSET_MISSING item=Rock-Face-01-dependency path={path} action=copy-complete-gltf-working-set";
                return false;
            }
            if (!path.EndsWith(".jpg", StringComparison.Ordinal)) continue;
            if (ResourceLoader.Exists(path) && ResourceLoader.Load<Texture2D>(path) is not null) continue;
            diagnostic = $"FISH_ASSET_INVALID item=Rock-Face-01-dependency path={path} action=run-tools-dev-import";
            return false;
        }
        if (!TryLoad(0, out PackedScene playerScene, out diagnostic)) return false;
        if (!TryLoad(1, out PackedScene rockScene, out diagnostic)) return false;
        if (!TryLoad(2, out Texture2D diffuse, out diagnostic)) return false;
        if (!TryLoad(3, out Texture2D normal, out diagnostic)) return false;
        if (!TryLoad(4, out Texture2D roughness, out diagnostic)) return false;
        if (!TryLoad(5, out PackedScene coralScene, out diagnostic)) return false;

        var playerVisual = playerScene.Instantiate<Node3D>();
        var playerSkeleton = playerVisual.GetNodeOrNull<Skeleton3D>("Armature/Skeleton3D");
        var playerMesh = playerVisual.GetNodeOrNull<MeshInstance3D>("Armature/Skeleton3D/ClownFish");
        var playerAnimation = playerVisual.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        if (playerSkeleton is null || playerSkeleton.GetBoneCount() < 8 ||
            playerMesh?.Mesh is null || playerMesh.Skin is null || playerMesh.Skin.GetBindCount() < 6 ||
            playerMesh.Mesh.GetSurfaceCount() < 3 ||
            playerAnimation is null || !playerAnimation.HasAnimation("Armature|Swim"))
        {
            playerVisual.Free();
            diagnostic = "FISH_ASSET_INVALID item=Quaternius-Fish3 action=check-FBX-mesh-skin-and-swim-import";
            return false;
        }
        var swimClip = playerAnimation.GetAnimation("Armature|Swim");
        if (swimClip is null || swimClip.GetTrackCount() == 0 || swimClip.Length <= 0)
        {
            playerVisual.Free();
            diagnostic = "FISH_ASSET_INVALID item=Quaternius-Fish3 action=check-swim-clip";
            return false;
        }
        for (var track = 0; track < swimClip.GetTrackCount(); track++)
        {
            if (swimClip.TrackGetType(track) == Animation.TrackType.Rotation3D) continue;
            playerVisual.Free();
            diagnostic = "FISH_ASSET_INVALID item=Quaternius-Fish3 action=remove-imported-translation-or-scale-track";
            return false;
        }
        if (!HasMaterialSurfaces(playerMesh.Mesh, 3))
        {
            playerVisual.Free();
            diagnostic = "FISH_ASSET_INVALID item=Quaternius-Fish3 action=check-imported-body-stripes-outline-materials";
            return false;
        }
        var motion = FishVisualMotion.CreateVerified(fish, playerVisual, playerSkeleton,
            playerAnimation, swimClip);
        if (motion is null)
        {
            playerVisual.Free();
            diagnostic = "FISH_ASSET_INVALID item=Quaternius-Fish3 action=check-Root-Spine3-Tail-swim-tracks";
            return false;
        }
        if (!HasPreparedStaticMaterial(rockScene, "rock_face_01", true, true, true))
        {
            playerVisual.Free();
            diagnostic = "FISH_ASSET_INVALID item=Rock-Face-01 action=check-gltf-binary-and-texture-dependencies";
            return false;
        }
        if (!HasPreparedStaticMaterial(coralScene, "Mesh", true, true, false))
        {
            playerVisual.Free();
            diagnostic = "FISH_ASSET_INVALID item=Heliopora-coral action=check-GLB-mesh-and-material";
            return false;
        }
        if (!IsTwoKilopixelTexture(diffuse) || !IsTwoKilopixelTexture(normal) ||
            !IsTwoKilopixelTexture(roughness))
        {
            playerVisual.Free();
            diagnostic = "FISH_ASSET_INVALID item=Coast-Sand-01 action=check-2K-diffuse-normal-roughness-maps";
            return false;
        }

        fish.AddChild(playerVisual);
        playerVisual.Name = "PlayerVisual";
        playerVisual.Scale = Vector3.One * 0.12f;
        // Fish3's Face-to-Tail axis imports toward +Z; align its nose with motor forward (-Z).
        playerVisual.RotationDegrees = new Vector3(0, 180, 0);
        playerVisual.AddChild(motion);
        foreach (var oldPart in new[] { "Body", "Tail", "TopFin", "LeftEye", "RightEye" })
            if (fish.GetNodeOrNull<MeshInstance3D>(oldPart) is { } oldMesh) oldMesh.Visible = false;

        var floor = main.GetNode<MeshInstance3D>("Habitat/Floor/Mesh");
        var sandMaterial = new StandardMaterial3D
        {
            AlbedoTexture = diffuse,
            NormalEnabled = true,
            NormalTexture = normal,
            RoughnessTexture = roughness,
            Uv1Scale = new Vector3(9, 9, 1),
        };
        floor.MaterialOverride = sandMaterial;
        var shelfSandMaterial = (StandardMaterial3D)sandMaterial.Duplicate();
        shelfSandMaterial.Uv1Scale = new Vector3(3, 3, 1);
        main.GetNode<MeshInstance3D>("Habitat/CourseShelf/Mesh").MaterialOverride = shelfSandMaterial;
        main.GetNode<WorldEnvironment>("WorldEnvironment").Environment = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.Color,
            BackgroundColor = new Color(0.025f, 0.19f, 0.24f),
        };
        foreach (var wall in new[] { "WestWall", "EastWall" })
            if (main.GetNodeOrNull<MeshInstance3D>($"Habitat/{wall}/Mesh") is { } wallMesh)
                wallMesh.Visible = false;

        var decor = new Node3D { Name = "StarterDecor" };
        main.GetNode<Node3D>("Habitat").AddChild(decor);
        AddScene(decor, rockScene, "RockWest", CoursePosition(main, "RockWest"), 0.55f);
        AddScene(decor, rockScene, "RockEast", CoursePosition(main, "RockEast"), 0.55f);
        AddScene(decor, coralScene, "CoralWest", CoursePosition(main, "CoralWest"), 3.5f);
        AddScene(decor, coralScene, "CoralEast", CoursePosition(main, "CoralEast"), 3.0f);
        foreach (var marker in new[] { "CoralNear", "CoralFar" })
            if (main.GetNodeOrNull<MeshInstance3D>($"Habitat/Landmarks/{marker}") is { } oldMarker)
                oldMarker.Visible = false;

        var ambient = new Node3D { Name = "AmbientFish" };
        main.GetNode<Node3D>("Habitat").AddChild(ambient);
        AddFish(ambient, "Fish1", "SchoolFishA", new Vector3(-3.5f, -0.4f, -4.5f), 0.10f);
        AddFish(ambient, "Fish2", "SchoolFishB", new Vector3(3.0f, 0.6f, -6.1f), 0.10f);
        AddFish(ambient, "Fish3", "SchoolFishC", new Vector3(0, 1.4f, -7.2f), 0.09f);
        AddFish(ambient, "Manta ray", "Manta", new Vector3(-5.7f, 1.7f, -7.2f), 0.16f);
        AddFish(ambient, "Dolphin", "Dolphin", new Vector3(5.9f, 1.6f, -7.5f), 0.12f);
        AddFish(ambient, "Shark", "Shark", new Vector3(-6.6f, -0.3f, 3.8f), 0.14f);
        AddFish(ambient, "Whale", "Whale", new Vector3(5.8f, 0.2f, 4.7f), 0.12f);

        diagnostic = "";
        GD.Print("FISH_STARTER_WORLD_READY player=Fish3 ambient=7 rocks=2 coral=2 sand=1 course=movement_course");
        return true;
    }

    private static bool TryLoad<T>(int index, out T resource, out string diagnostic) where T : Resource
    {
        resource = null!;
        var (item, path) = Required[index];
        if (!FileAccess.FileExists(path))
        {
            diagnostic = $"FISH_ASSET_MISSING item={item} path={path} action=prepare-source-under-game/assets/vendor-local";
            return false;
        }
        if (!ResourceLoader.Exists(path) || (resource = ResourceLoader.Load<T>(path)) is null)
        {
            diagnostic = $"FISH_ASSET_INVALID item={item} path={path} action=run-tools-dev-import-and-check-dependencies";
            return false;
        }
        diagnostic = "";
        return true;
    }

    private static bool HasMaterialSurfaces(Mesh mesh, int minimumSurfaces)
    {
        if (mesh.GetSurfaceCount() < minimumSurfaces) return false;
        for (var i = 0; i < mesh.GetSurfaceCount(); i++)
            if (mesh.SurfaceGetMaterial(i) is not BaseMaterial3D) return false;
        return true;
    }

    private static bool HasPreparedStaticMaterial(PackedScene scene, string meshPath,
        bool albedo, bool normal, bool roughness)
    {
        var root = scene.Instantiate<Node3D>();
        var mesh = root.GetNodeOrNull<MeshInstance3D>(meshPath)?.Mesh;
        var material = mesh?.GetSurfaceCount() > 0 ? mesh.SurfaceGetMaterial(0) as BaseMaterial3D : null;
        var valid = material is not null && (!albedo || material.AlbedoTexture is not null) &&
            (!normal || material.NormalTexture is not null) &&
            (!roughness || material.RoughnessTexture is not null);
        root.Free();
        return valid;
    }

    private static bool IsTwoKilopixelTexture(Texture2D texture) =>
        texture.GetWidth() == 2048 && texture.GetHeight() == 2048;

    private static Vector3 CoursePosition(Node3D main, string marker) =>
        main.GetNode<Node3D>($"Habitat/MovementCourse/{marker}").Position;

    private static void AddScene(Node3D parent, PackedScene scene, string name, Vector3 position, float scale)
    {
        var node = scene.Instantiate<Node3D>();
        parent.AddChild(node);
        node.Name = name;
        node.Position = position;
        node.Scale = Vector3.One * scale;
    }

    private static void AddFish(Node3D parent, string source, string name, Vector3 position, float scale)
    {
        var path = Base + $"quaternius/{source}.fbx";
        if (!ResourceLoader.Exists(path)) return;
        var scene = ResourceLoader.Load<PackedScene>(path);
        if (scene is null) return;
        var node = scene.Instantiate<Node3D>();
        parent.AddChild(node);
        node.Name = name;
        node.Position = position;
        node.Scale = Vector3.One * scale;
        node.RotationDegrees = new Vector3(0, 90, 0);
        if (node.GetNodeOrNull<AnimationPlayer>("AnimationPlayer") is { } player)
        {
            var clip = source == "Fish2" ? "Armature|Swim_001" : "Armature|Swim";
            if (player.HasAnimation(clip)) PlayLoop(player, clip);
        }
    }

    private static void PlayLoop(AnimationPlayer player, string clip)
    {
        // Imported clips are nonlooping. Replay the same pose clip without touching body translation.
        player.AnimationFinished += finished =>
        {
            if (finished == clip && player.IsInsideTree()) player.Play(clip);
        };
        player.Play(clip);
    }
}
