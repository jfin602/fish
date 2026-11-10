#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace Fish;

/// <summary>Tech-demo art choices. Entries are data; swimming authority stays in FishController.</summary>
public sealed record CharacterEntry(
    string Id, string Name, string Category, string Description, string ModelPath,
    float WorldScale, float FacingDegrees, float PreviewYawDegrees,
    string GlideClip, string SwimClip, string FastClip)
{
    public string PlayerScenePath => ModelPath;
    public string PreviewScenePath => ModelPath;
    public Transform3D PresentationTransform => new(
        Basis.FromEuler(new Vector3(0, Mathf.DegToRad(FacingDegrees), 0)).Scaled(Vector3.One * WorldScale),
        Vector3.Zero);
}

public static class CharacterCatalog
{
    private const string Faf = "res://assets/faf/fish/";
    public static IReadOnlyList<CharacterEntry> All { get; } = new CharacterEntry[]
    {
        new("FAF-FISH-001", "Yellow Tang", "Reef fish", "A bright, agile reef swimmer.", Faf + "fish_001_yellow_tang.glb", 4.3f, 180, 30, "fish_001_yellow_tang__Glide", "fish_001_yellow_tang__Swim", "fish_001_yellow_tang__SwimFast"),
        new("FAF-FISH-002", "Ocellaris Clownfish", "Reef fish", "A striped fish at home among the reef.", Faf + "fish_002_ocellaris_clownfish.glb", 6.2f, 180, 30, "fish_002_ocellaris_clownfish__Glide", "fish_002_ocellaris_clownfish__Swim", "fish_002_ocellaris_clownfish__SwimFast"),
        new("FAF-FISH-003", "Blue-Green Chromis", "Reef fish", "A small blue-green swimmer.", Faf + "fish_003_blue_green_chromis.glb", 6.4f, 180, 30, "fish_003_blue_green_chromis__Glide", "fish_003_blue_green_chromis__Swim", "fish_003_blue_green_chromis__SwimFast"),
        new("FAF-FISH-004", "Reef Butterflyfish", "Reef fish", "A tall-finned reef explorer.", Faf + "fish_004_reef_butterflyfish.glb", 5.0f, 180, 30, "fish_004_reef_butterflyfish__Glide", "fish_004_reef_butterflyfish__Swim", "fish_004_reef_butterflyfish__SwimFast"),
        new("FAF-FISH-005", "Blacktip Reef Shark", "Shark", "A sleek shark with striking dark fin tips.", Faf + "fish_005_blacktip_reef_shark.glb", .64f, 180, 30, "fish_005_blacktip_reef_shark__Glide", "fish_005_blacktip_reef_shark__Swim", "fish_005_blacktip_reef_shark__SwimFast"),
    };

    public static CharacterEntry Default => All[0];
    public static CharacterEntry Find(string? id)
    {
        foreach (var entry in All)
            if (entry.Id == id) return entry;
        return Default;
    }

    public static bool TryInstantiate(CharacterEntry entry, out Node3D? model,
        out AnimationPlayer? player, out string diagnostic)
    {
        model = null;
        player = null;
        if (!FileAccess.FileExists(entry.ModelPath))
        {
            diagnostic = $"FISH_ASSET_MISSING item={entry.Id} path={entry.ModelPath} action=restore-source-asset";
            return false;
        }
        var scene = ResourceLoader.Load<PackedScene>(entry.ModelPath);
        if (scene is null)
        {
            diagnostic = $"FISH_ASSET_INVALID item={entry.Id} path={entry.ModelPath} action=run-tools-dev-import";
            return false;
        }
        model = scene.Instantiate<Node3D>();
        foreach (var node in model.FindChildren("*", "MeshInstance3D", true, false))
            if (node is MeshInstance3D { Mesh: not null }) { player = FindPlayer(model); break; }
        if (player is null || !player.HasAnimation(entry.SwimClip) ||
            (entry.GlideClip.Length > 0 && !player.HasAnimation(entry.GlideClip)) ||
            (entry.FastClip.Length > 0 && !player.HasAnimation(entry.FastClip)) ||
            !AnimationStaysOnRig(player, entry))
        {
            model.Free();
            model = null;
            diagnostic = $"FISH_ASSET_INVALID item={entry.Id} path={entry.ModelPath} action=check-mesh-and-catalog-animation-mapping";
            return false;
        }
        diagnostic = "";
        return true;
    }

    private static AnimationPlayer? FindPlayer(Node3D model)
    {
        foreach (var node in model.FindChildren("*", "AnimationPlayer", true, false))
            if (node is AnimationPlayer player) return player;
        return null;
    }

    private static bool AnimationStaysOnRig(AnimationPlayer player, CharacterEntry entry)
    {
        foreach (var name in new[] { entry.GlideClip, entry.SwimClip, entry.FastClip })
        {
            if (name.Length == 0) continue;
            var clip = player.GetAnimation(name);
            if (clip is null || clip.Length <= 0 || clip.GetTrackCount() == 0) return false;
            for (var track = 0; track < clip.GetTrackCount(); track++)
                if (clip.TrackGetType(track) != Animation.TrackType.Rotation3D) return false;
        }
        return true;
    }
}

/// <summary>Selection lives for one process and never enters a survival save.</summary>
public static class DemoSelection
{
    public static string? CharacterId { get; set; }
    public static void Clear() => CharacterId = null;
}
