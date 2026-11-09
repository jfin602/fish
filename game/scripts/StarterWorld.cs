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

    public static bool TryPopulate(Node3D main, FishController fish, out string diagnostic)
    {
        foreach (var (item, path) in Required)
        {
            if (FileAccess.FileExists(path) && ResourceLoader.Exists(path)) continue;
            diagnostic = $"FISH_ASSET_MISSING item={item} path={path} action=prepare-source-under-game/assets/vendor-local";
            return false;
        }

        var playerScene = ResourceLoader.Load<PackedScene>(Required[0].Path);
        var rockScene = ResourceLoader.Load<PackedScene>(Required[1].Path);
        var coralScene = ResourceLoader.Load<PackedScene>(Required[5].Path);
        var diffuse = ResourceLoader.Load<Texture2D>(Required[2].Path);
        var normal = ResourceLoader.Load<Texture2D>(Required[3].Path);
        var roughness = ResourceLoader.Load<Texture2D>(Required[4].Path);
        if (playerScene is null || rockScene is null || coralScene is null ||
            diffuse is null || normal is null || roughness is null)
        {
            diagnostic = "FISH_ASSET_INVALID action=reimport-starter-files-and-check-dependencies";
            return false;
        }

        var playerVisual = playerScene.Instantiate<Node3D>();
        var playerSkeleton = playerVisual.GetNodeOrNull<Skeleton3D>("Armature/Skeleton3D");
        var playerAnimation = playerVisual.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        if (playerSkeleton is null || playerSkeleton.GetBoneCount() == 0 ||
            playerAnimation is null || !playerAnimation.HasAnimation("Armature|Swim"))
        {
            playerVisual.Free();
            diagnostic = "FISH_ASSET_INVALID item=Quaternius-Fish3 action=check-FBX-skeleton-and-swim-import";
            return false;
        }

        fish.AddChild(playerVisual);
        playerVisual.Name = "PlayerVisual";
        playerVisual.Scale = Vector3.One * 0.12f;
        playerVisual.RotationDegrees = new Vector3(0, 90, 0);
        PlayLoop(playerAnimation, "Armature|Swim");
        foreach (var oldPart in new[] { "Body", "Tail", "TopFin", "LeftEye", "RightEye" })
            if (fish.GetNodeOrNull<MeshInstance3D>(oldPart) is { } oldMesh) oldMesh.Visible = false;

        var floor = main.GetNode<MeshInstance3D>("Habitat/Floor/Mesh");
        floor.MaterialOverride = new StandardMaterial3D
        {
            AlbedoTexture = diffuse,
            NormalEnabled = true,
            NormalTexture = normal,
            RoughnessTexture = roughness,
            Uv1Scale = new Vector3(3, 3, 1),
        };
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
        AddScene(decor, rockScene, "RockWest", new Vector3(-5.3f, -4.1f, -3.6f), 0.55f);
        AddScene(decor, rockScene, "RockEast", new Vector3(5.2f, -4.1f, -5.6f), 0.42f);
        AddScene(decor, coralScene, "CoralWest", new Vector3(-4.6f, -3.9f, -2.4f), 3.5f);
        AddScene(decor, coralScene, "CoralEast", new Vector3(4.8f, -3.9f, -4.8f), 3.0f);
        foreach (var marker in new[] { "CoralNear", "CoralFar" })
            if (main.GetNodeOrNull<MeshInstance3D>($"Habitat/Landmarks/{marker}") is { } oldMarker)
                oldMarker.Visible = false;

        var ambient = new Node3D { Name = "AmbientFish" };
        main.GetNode<Node3D>("Habitat").AddChild(ambient);
        AddFish(ambient, "Fish1", "SchoolFishA", new Vector3(-3.5f, -0.4f, -4.5f), 0.10f);
        AddFish(ambient, "Fish2", "SchoolFishB", new Vector3(3.0f, 0.6f, -6.1f), 0.10f);
        AddFish(ambient, "Fish3", "SchoolFishC", new Vector3(1.3f, -1.7f, -3.2f), 0.09f);
        AddFish(ambient, "Manta ray", "Manta", new Vector3(-5.7f, 1.7f, -7.2f), 0.16f);
        AddFish(ambient, "Dolphin", "Dolphin", new Vector3(5.9f, 1.6f, -7.5f), 0.12f);
        AddFish(ambient, "Shark", "Shark", new Vector3(-6.6f, -0.3f, 3.8f), 0.14f);
        AddFish(ambient, "Whale", "Whale", new Vector3(5.8f, 0.2f, 4.7f), 0.12f);

        diagnostic = "";
        GD.Print("FISH_STARTER_WORLD_READY player=Fish3 ambient=7 rocks=2 coral=2 sand=1");
        return true;
    }

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
