#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace Fish;

public enum FafEnvironmentFamily { Terrain, Rock, Coral, Plant, Detail }

/// <summary>Original FAF visual, with dimensions in imported Godot X/Y/Z metres.</summary>
public sealed record FafEnvironmentEntry(
    string Id, FafEnvironmentFamily Family, char Variant, string Type,
    string ScenePath, Vector3 SizeMeters, float ContactY, string Palette,
    bool HasPlantHeightMask)
{
    // Plant UV.y is 0 at its rooted base and 1 at the tips. No sway or physics is installed.
    public string? HeightMaskCoordinate => HasPlantHeightMask ? "UV.y" : null;

    public PackedScene LoadScene() => ResourceLoader.Load<PackedScene>(ScenePath)
        ?? throw new InvalidOperationException($"FAF scene unavailable: {Id} {ScenePath}");

    /// <summary>Visual-only placement root. Terrain collision is authored by the world separately.</summary>
    public Node3D CreateVisual()
    {
        var imported = LoadScene().Instantiate<Node3D>();
        if (HasPhysics(imported))
        {
            imported.Free();
            throw new InvalidOperationException($"FAF visual unexpectedly contains collision: {Id}");
        }
        var root = new Node3D { Name = Id.Replace('-', '_') };
        root.SetMeta("faf_id", Id);
        root.SetMeta("faf_scene_path", ScenePath);
        root.AddChild(imported);
        return root;
    }

    private static bool HasPhysics(Node node)
    {
        if (node is CollisionObject3D or CollisionShape3D or CollisionPolygon3D) return true;
        foreach (var child in node.GetChildren())
            if (HasPhysics(child)) return true;
        return false;
    }
}

/// <summary>Small fixed P3 placement boundary backed by the tracked FAF GLBs.</summary>
public static class FafRuntimeCatalog
{
    // Environment metadata is transcribed from assets/faf manifests and the FAF-ROCK-001 candidate record.
    public static IReadOnlyList<FafEnvironmentEntry> Environment { get; } = new FafEnvironmentEntry[]
    {
        new("FAF-TERRAIN-001A", FafEnvironmentFamily.Terrain, 'A', "open_sand", "res://assets/faf/terrain/terrain_001a_open_sand.glb", new Vector3(11.30207f, 0.115f, 9.66536f), -0.09f, "", false),
        new("FAF-TERRAIN-001B", FafEnvironmentFamily.Terrain, 'B', "rolling", "res://assets/faf/terrain/terrain_001b_rolling.glb", new Vector3(5.58059f, 0.28586f, 4.8535f), -0.09f, "", false),
        new("FAF-TERRAIN-001C", FafEnvironmentFamily.Terrain, 'C', "reef_shelf", "res://assets/faf/terrain/terrain_001c_reef_shelf.glb", new Vector3(5.43793f, 1.01357f, 4.8413f), -0.09f, "", false),
        new("FAF-TERRAIN-001D", FafEnvironmentFamily.Terrain, 'D', "slope", "res://assets/faf/terrain/terrain_001d_slope.glb", new Vector3(5.80388f, 0.59733f, 4.44369f), -0.09f, "", false),
        new("FAF-TERRAIN-001E", FafEnvironmentFamily.Terrain, 'E', "drop_off", "res://assets/faf/terrain/terrain_001e_drop_off.glb", new Vector3(4.8058f, 1.10088f, 5.14451f), -0.09f, "", false),
        new("FAF-TERRAIN-001F", FafEnvironmentFamily.Terrain, 'F', "rocky_floor", "res://assets/faf/terrain/terrain_001f_rocky_floor.glb", new Vector3(5.09786f, 0.47823f, 5.316f), -0.09f, "", false),
        new("FAF-ROCK-001A", FafEnvironmentFamily.Rock, 'A', "reef_boulder", "res://assets/faf/rocks/rock_001a_reef_boulder.glb", new Vector3(1.56131f, 0.88574f, 1.29143f), 0f, "stone.cool_slate", false),
        new("FAF-ROCK-001B", FafEnvironmentFamily.Rock, 'B', "reef_boulder", "res://assets/faf/rocks/rock_001b_reef_boulder.glb", new Vector3(1.85f, 0.57224f, 2.035f), 0f, "stone.cool_slate", false),
        new("FAF-ROCK-001C", FafEnvironmentFamily.Rock, 'C', "reef_boulder", "res://assets/faf/rocks/rock_001c_reef_boulder.glb", new Vector3(1.22297f, 1.39174f, 0.975f), 0f, "stone.cool_slate", false),
        new("FAF-ROCK-001D", FafEnvironmentFamily.Rock, 'D', "reef_boulder", "res://assets/faf/rocks/rock_001d_reef_boulder.glb", new Vector3(1.46411f, 1.03483f, 1.04133f), 0f, "stone.cool_slate", false),
        new("FAF-CORAL-001A", FafEnvironmentFamily.Coral, 'A', "branching", "res://assets/faf/coral/coral_001a_branching.glb", new Vector3(0.90817f, 0.83144f, 0.94779f), 0f, "reef_warm.red", false),
        new("FAF-CORAL-001B", FafEnvironmentFamily.Coral, 'B', "branching", "res://assets/faf/coral/coral_001b_branching.glb", new Vector3(1.37414f, 1.03839f, 1.57959f), 0f, "reef_warm.pink", false),
        new("FAF-CORAL-001C", FafEnvironmentFamily.Coral, 'C', "branching", "res://assets/faf/coral/coral_001c_branching.glb", new Vector3(0.75898f, 1.07156f, 0.70483f), 0f, "reef_warm.orange", false),
        new("FAF-CORAL-002A", FafEnvironmentFamily.Coral, 'A', "rounded", "res://assets/faf/coral/coral_002a_rounded.glb", new Vector3(0.8486f, 0.47825f, 0.82633f), 0f, "reef_warm.orange", false),
        new("FAF-CORAL-002B", FafEnvironmentFamily.Coral, 'B', "rounded", "res://assets/faf/coral/coral_002b_rounded.glb", new Vector3(1.1775f, 0.44964f, 1.24173f), 0f, "reef_warm.yellow", false),
        new("FAF-CORAL-002C", FafEnvironmentFamily.Coral, 'C', "rounded", "res://assets/faf/coral/coral_002c_rounded.glb", new Vector3(0.68018f, 0.47799f, 0.6354f), 0f, "reef_cool.muted_purple", false),
        new("FAF-CORAL-003A", FafEnvironmentFamily.Coral, 'A', "tube", "res://assets/faf/coral/coral_003a_tube.glb", new Vector3(0.6501f, 0.65452f, 0.61906f), 0f, "reef_warm.pink", false),
        new("FAF-CORAL-003B", FafEnvironmentFamily.Coral, 'B', "tube", "res://assets/faf/coral/coral_003b_tube.glb", new Vector3(0.91061f, 0.77229f, 0.88923f), 0f, "reef_cool.cyan", false),
        new("FAF-CORAL-003C", FafEnvironmentFamily.Coral, 'C', "tube", "res://assets/faf/coral/coral_003c_tube.glb", new Vector3(0.60234f, 0.79087f, 0.51155f), 0f, "reef_warm.yellow", false),
        new("FAF-CORAL-004A", FafEnvironmentFamily.Coral, 'A', "fan", "res://assets/faf/coral/coral_004a_fan.glb", new Vector3(1.04707f, 0.76215f, 0.29322f), 0f, "reef_cool.muted_purple", false),
        new("FAF-CORAL-004B", FafEnvironmentFamily.Coral, 'B', "fan", "res://assets/faf/coral/coral_004b_fan.glb", new Vector3(1.43247f, 0.80608f, 0.31981f), 0f, "reef_warm.red", false),
        new("FAF-CORAL-004C", FafEnvironmentFamily.Coral, 'C', "fan", "res://assets/faf/coral/coral_004c_fan.glb", new Vector3(0.76271f, 0.90268f, 0.23798f), 0f, "reef_cool.blue", false),
        new("FAF-PLANT-001A", FafEnvironmentFamily.Plant, 'A', "sea_grass", "res://assets/faf/plants/plant_001a_sea_grass.glb", new Vector3(0.62945f, 0.57901f, 0.60833f), 0f, "vegetation.green", true),
        new("FAF-PLANT-001B", FafEnvironmentFamily.Plant, 'B', "sea_grass", "res://assets/faf/plants/plant_001b_sea_grass.glb", new Vector3(0.77544f, 0.71399f, 0.9348f), 0f, "vegetation.deep_green", true),
        new("FAF-PLANT-001C", FafEnvironmentFamily.Plant, 'C', "sea_grass", "res://assets/faf/plants/plant_001c_sea_grass.glb", new Vector3(0.43314f, 0.4449f, 0.47468f), 0f, "vegetation.yellow_green", true),
        new("FAF-PLANT-002A", FafEnvironmentFamily.Plant, 'A', "broad_seaweed", "res://assets/faf/plants/plant_002a_broad_seaweed.glb", new Vector3(0.96142f, 1.11925f, 0.83592f), 0f, "vegetation.deep_green", true),
        new("FAF-PLANT-002B", FafEnvironmentFamily.Plant, 'B', "broad_seaweed", "res://assets/faf/plants/plant_002b_broad_seaweed.glb", new Vector3(1.11889f, 1.29736f, 1.15617f), 0f, "vegetation.green", true),
        new("FAF-PLANT-002C", FafEnvironmentFamily.Plant, 'C', "broad_seaweed", "res://assets/faf/plants/plant_002c_broad_seaweed.glb", new Vector3(0.571f, 0.92364f, 0.5836f), 0f, "vegetation.muted_teal", true),
        new("FAF-PLANT-003A", FafEnvironmentFamily.Plant, 'A', "reef_algae", "res://assets/faf/plants/plant_003a_reef_algae.glb", new Vector3(0.7458f, 0.30321f, 0.73936f), 0f, "vegetation.yellow_green", true),
        new("FAF-PLANT-003B", FafEnvironmentFamily.Plant, 'B', "reef_algae", "res://assets/faf/plants/plant_003b_reef_algae.glb", new Vector3(0.77608f, 0.41691f, 0.69537f), 0f, "vegetation.deep_green", true),
        new("FAF-DETAIL-001A", FafEnvironmentFamily.Detail, 'A', "shell", "res://assets/faf/details/detail_001a_fan_shell.glb", new Vector3(0.1593f, 0.05327f, 0.11272f), 0f, "detail.shell_cream", false),
        new("FAF-DETAIL-001B", FafEnvironmentFamily.Detail, 'B', "shell", "res://assets/faf/details/detail_001b_ridge_shell.glb", new Vector3(0.11535f, 0.06461f, 0.14196f), 0f, "detail.shell_sand", false),
        new("FAF-DETAIL-002A", FafEnvironmentFamily.Detail, 'A', "rubble", "res://assets/faf/details/detail_002a_coral_rubble.glb", new Vector3(0.17042f, 0.0445f, 0.11637f), 0f, "detail.rubble_coral", false),
        new("FAF-DETAIL-002B", FafEnvironmentFamily.Detail, 'B', "rubble", "res://assets/faf/details/detail_002b_reef_rubble.glb", new Vector3(0.17332f, 0.04932f, 0.05472f), 0f, "detail.rubble_stone", false),
        new("FAF-DETAIL-003A", FafEnvironmentFamily.Detail, 'A', "pebble", "res://assets/faf/details/detail_003a_slate_pebbles.glb", new Vector3(0.12795f, 0.01245f, 0.11195f), 0f, "detail.pebble_slate", false),
        new("FAF-DETAIL-003B", FafEnvironmentFamily.Detail, 'B', "pebble", "res://assets/faf/details/detail_003b_warm_pebbles.glb", new Vector3(0.16286f, 0.01068f, 0.18853f), 0f, "detail.pebble_warm", false),
        new("FAF-DETAIL-004A", FafEnvironmentFamily.Detail, 'A', "sponge", "res://assets/faf/details/detail_004a_encrusting_sponge.glb", new Vector3(0.14392f, 0.03158f, 0.14109f), 0f, "detail.sponge_ochre", false),
        new("FAF-DETAIL-004B", FafEnvironmentFamily.Detail, 'B', "sponge", "res://assets/faf/details/detail_004b_cup_sponge.glb", new Vector3(0.16297f, 0.10678f, 0.08657f), 0f, "detail.sponge_rose", false),
    };

    public static IReadOnlyList<CharacterEntry> Fish => CharacterCatalog.All;

    public static FafEnvironmentEntry GetEnvironment(string id)
    {
        foreach (var entry in Environment)
            if (entry.Id == id) return entry;
        throw new KeyNotFoundException($"Unknown FAF environment ID: {id}");
    }
}
