using Godot;

namespace Fish;

/// <summary>Normal-play P2 habitat readiness and presentation; no fish movement authority.</summary>
public static class P2HabitatWorld
{
    public static bool TryPopulate(Node3D main, FishController fish, out string diagnostic)
    {
        var habitat = main.GetNodeOrNull<Node3D>("Habitat");
        var seabed = habitat?.GetNodeOrNull<SeabedSurface>("Seabed");
        var spawn = habitat?.GetNodeOrNull<Marker3D>("Spawn");
        var composition = habitat?.GetNodeOrNull<ReefComposition>("Composition/ReefComposition");
        if (habitat is null || seabed is null || spawn is null || composition is null)
        {
            diagnostic = "FISH_HABITAT_INVALID action=check-p2-habitat-scene-structure";
            return false;
        }
        if (!HabitatPalette.TryLoad(out var palette, out diagnostic)) return false;
        if (!PlayerArt.TryLoadAndAttach(fish, out diagnostic)) return false;
        if (!seabed.Build(palette))
        {
            diagnostic = "FISH_HABITAT_INVALID action=build-continuous-seabed-and-collision";
            return false;
        }
        if (!composition.TryBuild(seabed, out diagnostic)) return false;

        fish.Position = spawn.Position;
        diagnostic = "";
        GD.Print("FISH_HABITAT_READY player=Fish3 seabed=continuous palette=CoastSand01,Ground060,Gravel041,Rock063 course=fixture-only");
        return true;
    }
}
