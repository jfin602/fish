#nullable enable
using Godot;
using System.Collections.Generic;

namespace Fish;

/// <summary>Menu routes are distinct from the two persistent habitat identities.</summary>
public sealed record WorldDestination(string Id, string DisplayName, string Description,
    Color Accent, string? WorldId = null)
{
    public bool IsEncounter => WorldId is null;
}

public static class WorldDestinations
{
    public const string PikeId = "pike-encounter";
    public static IReadOnlyList<WorldDestination> All { get; } = new[]
    {
        FromWorld(WorldCatalog.Resolve(WorldCatalog.OceanId)),
        FromWorld(WorldCatalog.Resolve(WorldCatalog.PondId)),
        new WorldDestination(PikeId, "Pike Encounter", "Bluegill vs Northern Pike · isolated development encounter",
            new Color(.38f, .48f, .22f)),
    };

    private static WorldDestination FromWorld(WorldDefinition world) =>
        new(world.Id, world.DisplayName, world.Preview.Description, world.Preview.Accent, world.Id);
}

public static class PikeEncounterAssets
{
    public const string ScenePath = "res://scenes/p5_pike_encounter.tscn";
    public const string BluegillId = "FAF-FISH-006";
    public const string PikeId = "FAF-FISH-010";

    public static bool TryValidate(out CharacterEntry? bluegill, out CharacterEntry? pike,
        out string diagnostic, string scenePath = ScenePath, string bluegillId = BluegillId,
        string pikeId = PikeId, IReadOnlyList<CharacterEntry>? catalog = null)
    {
        bluegill = null;
        pike = null;
        if (scenePath != ScenePath || !Godot.FileAccess.FileExists(scenePath) ||
            ResourceLoader.Load<PackedScene>(scenePath) is null)
        { diagnostic = "PIKE_SCENE_MISSING action=restore-and-import-encounter-scene"; return false; }
        if (bluegillId != BluegillId || pikeId != PikeId)
        { diagnostic = "PIKE_SPECIES_INELIGIBLE action=use-bluegill-and-northern-pike"; return false; }
        var entries = catalog ?? CharacterCatalog.All;
        if (!SpeciesCompatibility.TryResolveFrom(entries, WorldCatalog.PondId, bluegillId,
            SpeciesRole.Selectable, out bluegill, out diagnostic) || bluegill is null)
        { diagnostic = "PIKE_BLUEGILL_UNAVAILABLE " + diagnostic; return false; }
        if (!SpeciesCompatibility.TryResolveFrom(entries, WorldCatalog.PondId, pikeId,
            SpeciesRole.Ecological, out pike, out diagnostic) || pike is null)
        { diagnostic = "PIKE_MODEL_UNAVAILABLE " + diagnostic; return false; }
        foreach (var actor in new[] { bluegill, pike })
            if (!CharacterCatalog.TryInstantiate(actor, out var model, out _, out diagnostic))
            { diagnostic = $"PIKE_MODEL_INVALID id={actor.Id} {diagnostic}"; return false; }
            else model!.Free();
        diagnostic = "";
        return true;
    }
}
