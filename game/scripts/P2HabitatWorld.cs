using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Fish;

/// <summary>Normal-play P2 habitat readiness and presentation; no fish movement authority.</summary>
public static class P2HabitatWorld
{
    public static bool TryPopulate(Node3D main, FishController fish, out string diagnostic)
    {
        var args = OS.GetCmdlineUserArgs();
        var generation = args.FirstOrDefault(a => a.StartsWith("--world-generate=", StringComparison.Ordinal));
        var pondGeneration = args.FirstOrDefault(a => a.StartsWith("--pond-generate=", StringComparison.Ordinal));
        var baked = args.FirstOrDefault(a => a.StartsWith("--world-package=", StringComparison.Ordinal));
        var selected = args.FirstOrDefault(a => a.StartsWith("--world-id=", StringComparison.Ordinal));
        var legacyCheck = args.Any(a => a is "--basin-check" or "--geology-mesh-check" or
            "--route-field-check" or "--placement-check" or "--depth-check");
        if (generation is null && pondGeneration is null && !legacyCheck)
        {
            WorldSession.Unload();
            try
            {
                if (selected is not null) WorldSession.Select(selected["--world-id=".Length..]);
                if (baked is not null && WorldSession.SelectedWorldId != WorldCatalog.OceanId)
                    throw new System.IO.InvalidDataException("WORLD_DIAGNOSTIC_CONFLICT action=remove-world-package-override");
                var bakedHabitat = main.GetNode<Node3D>("Habitat");
                if (WorldSession.SelectedWorldId == WorldCatalog.PondId)
                {
                    main.RemoveChild(bakedHabitat);
                    bakedHabitat.Free();
                    bakedHabitat = WorldCatalog.Resolve(WorldCatalog.PondId)
                        .LoadHabitatScene().Instantiate<Node3D>();
                    bakedHabitat.Name = "Habitat";
                    main.AddChild(bakedHabitat);
                }
                var manifest = baked is null
                    ? WorldCatalog.Load(WorldSession.SelectedWorldId, bakedHabitat)
                    : FullWorldPackage.Load(baked["--world-package=".Length..], bakedHabitat,
                        WorldSession.SelectedWorldId).Manifest;
                if (!PlayerArt.TryLoadAndAttach(fish, out diagnostic)) return false;
                if (manifest.GeneratorVersion != "c3-full-1")
                    fish.ConfigureWorldBounds(GeneratedBasin.HalfExtent - 1f, -40f, 9f);
                fish.Position = bakedHabitat.GetNode<Marker3D>("Spawn").Position;
                WorldSession.Bind(bakedHabitat, manifest);
                diagnostic = "";
                GD.Print($"FISH_HABITAT_READY player={DemoSelection.CharacterId ?? "legacy-Fish3"} world={manifest.WorldId} mode=baked");
                return true;
            }
            catch (Exception error)
            {
                WorldSession.Unload();
                diagnostic = $"WORLD_BAKED_LOAD_FAILED {error.Message}";
                return false;
            }
        }
        var seed = GeneratedBasin.DefaultSeed;
        if (pondGeneration is not null)
        {
            if (!ulong.TryParse(pondGeneration["--pond-generate=".Length..], out seed))
            {
                diagnostic = "POND_COMPILE_FAILED invalid seed";
                return false;
            }
            return TryCompilePond(main, fish, seed, args, out diagnostic);
        }
        if (generation is not null && !ulong.TryParse(generation["--world-generate=".Length..], out seed))
        {
            diagnostic = "WORLD_COMPILE_FAILED invalid seed";
            return false;
        }
        var total = Stopwatch.StartNew();
        var stage = Stopwatch.StartNew();
        var timings = new List<WorldStageTiming>();
        void Stage(string name, long milliseconds)
        {
            timings.Add(new WorldStageTiming(name, milliseconds));
            GD.Print($"WORLD_STAGE {name} elapsed_ms={milliseconds} total_ms={total.ElapsedMilliseconds}");
        }
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
        Stage("palette-player", stage.ElapsedMilliseconds);
        stage.Restart();
        seabed.SetBasinSeed(seed);
        fish.ConfigureWorldBounds(GeneratedBasin.HalfExtent - 1f, -40f, 9f);
        if (!seabed.Build(palette))
        {
            diagnostic = "FISH_HABITAT_INVALID action=build-continuous-seabed-and-collision";
            return false;
        }
        Stage("basin-mesh-collision", stage.ElapsedMilliseconds);
        if (!composition.TryBuild(seabed, WorldSession.SelectedWorldId, out diagnostic, Stage)) return false;
        GD.Print($"WORLD_GENERATION_READY seed={seed} total_ms={total.ElapsedMilliseconds}");
        if (generation is not null)
        {
            var output = args.FirstOrDefault(a => a.StartsWith("--world-output=", StringComparison.Ordinal));
            if (output is null)
            {
                diagnostic = "WORLD_COMPILE_FAILED missing output package";
                return false;
            }
            try
            {
                var package = FullWorldPackage.Bake(habitat, seed,
                    output["--world-output=".Length..], timings);
                GD.Print($"WORLD_FULL_COMPILE_OK id={package.WorldId} seed={seed} content={package.ContentSha256} total_ms={total.ElapsedMilliseconds}");
            }
            catch (Exception error)
            {
                diagnostic = $"WORLD_COMPILE_FAILED {error}";
                return false;
            }
        }

        fish.Position = spawn.Position;
        diagnostic = "";
        GD.Print($"FISH_HABITAT_READY player={DemoSelection.CharacterId ?? "legacy-Fish3"} seabed=continuous palette=CoastSand01,Ground060,Gravel041,Rock063 course=fixture-only");
        return true;
    }

    private static bool TryCompilePond(Node3D main, FishController fish, ulong seed,
        string[] args, out string diagnostic)
    {
        var output = args.FirstOrDefault(a => a.StartsWith("--world-output=", StringComparison.Ordinal));
        if (output is null)
        {
            diagnostic = "POND_COMPILE_FAILED missing output package";
            return false;
        }
        var total = Stopwatch.StartNew();
        var stage = Stopwatch.StartNew();
        var timings = new List<WorldStageTiming>();
        void Stage(string name)
        {
            timings.Add(new WorldStageTiming(name, stage.ElapsedMilliseconds));
            GD.Print($"POND_STAGE {name} elapsed_ms={stage.ElapsedMilliseconds} total_ms={total.ElapsedMilliseconds}");
            stage.Restart();
        }
        try
        {
            var habitat = main.GetNode<Node3D>("Habitat");
            var seabed = habitat.GetNode<SeabedSurface>("Seabed");
            var reef = habitat.GetNode<ReefComposition>("Composition/ReefComposition");
            if (!HabitatPalette.TryLoad(out var palette, out diagnostic)) return false;
            Stage("palette");
            seabed.SetBasinSeed(seed, pond: true);
            if (!seabed.Build(palette)) throw new InvalidOperationException("Pond terrain mesh failed");
            Stage("basin-mesh-collision");
            using var planner = new WorldRoutePlanner(habitat.GetWorld3D().DirectSpaceState,
                fish.GetRid(), seabed.Basin, Array.Empty<GeologicalRegion>());
            var routes = new GeneratedWorldRoutes(seabed.Basin, Array.Empty<GeologicalRegion>(),
                planner.PointClear);
            routes.RepairOpenConnectors(habitat.GetWorld3D().DirectSpaceState,
                fish.GetRid(), seabed.Basin, Array.Empty<GeologicalRegion>());
            habitat.GetNode<Marker3D>("Spawn").Position = routes.Nodes["spawn"].Position;
            Stage("route-repair");
            var analysis = new WorldEnvironmentAnalysis(seabed.Basin,
                Array.Empty<GeologicalRegion>(), routes);
            reef.InstallPondAnalysis(routes, analysis);
            Stage("environment-analysis");
            var package = FullWorldPackage.BakePond(habitat, seed,
                output["--world-output=".Length..], timings);
            GD.Print($"WORLD_FULL_COMPILE_OK habitat=pond id={package.WorldId} seed={seed} content={package.ContentSha256} total_ms={total.ElapsedMilliseconds}");
            diagnostic = "";
            return true;
        }
        catch (Exception error)
        {
            diagnostic = $"POND_COMPILE_FAILED {error}";
            return false;
        }
    }
}
