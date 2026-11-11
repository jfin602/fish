#nullable enable
using Godot;
using System;
using System.Linq;

namespace Fish;

/// <summary>Focused policy, baked population and world-switch regression.</summary>
public static class SpeciesCompatibilityChecks
{
    public static void Run(Main main)
    {
        try
        {
            WorldSession.Select(WorldCatalog.OceanId);
            var ocean = WorldCatalog.Resolve(WorldCatalog.OceanId);
            Require(CharacterCatalog.All.Count == 10, "fish catalog count");
            foreach (var candidate in CharacterCatalog.All.Where(e => e.NativeHabitat == WorldHabitat.Marine))
            {
                Require(candidate.NativeHabitat == WorldHabitat.Marine && candidate.Selectable &&
                    candidate.Ecological && !candidate.FixtureOnly, "marine metadata " + candidate.Id);
                Accept(WorldCatalog.OceanId, candidate.Id, SpeciesRole.Selectable);
                Accept(WorldCatalog.OceanId, candidate.Id, SpeciesRole.Ecological);
                Reject(WorldCatalog.PondId, candidate.Id, SpeciesRole.Selectable,
                    "SPECIES_HABITAT_INCOMPATIBLE");
                Reject(WorldCatalog.PondId, candidate.Id, SpeciesRole.Ambient,
                    "SPECIES_HABITAT_INCOMPATIBLE");
                if (candidate.Ambient) Accept(WorldCatalog.OceanId, candidate.Id, SpeciesRole.Ambient);
                else Reject(WorldCatalog.OceanId, candidate.Id, SpeciesRole.Ambient,
                    "SPECIES_ROLE_INELIGIBLE");
            }
            Require(SpeciesCompatibility.Available(WorldCatalog.OceanId, SpeciesRole.Selectable).Count == 5 &&
                SpeciesCompatibility.Available(WorldCatalog.PondId, SpeciesRole.Selectable).Count == 5 &&
                SpeciesCompatibility.Available(WorldCatalog.OceanId, SpeciesRole.Ambient).Count == 4,
                "filtered role catalogs");
            foreach (var candidate in CharacterCatalog.All.Where(e => e.NativeHabitat == WorldHabitat.Freshwater))
            {
                Accept(WorldCatalog.PondId, candidate.Id, SpeciesRole.Selectable);
                Accept(WorldCatalog.PondId, candidate.Id, SpeciesRole.Ambient);
                Reject(WorldCatalog.OceanId, candidate.Id, SpeciesRole.Selectable,
                    "SPECIES_HABITAT_INCOMPATIBLE");
                Reject(WorldCatalog.OceanId, candidate.Id, SpeciesRole.Ambient,
                    "SPECIES_HABITAT_INCOMPATIBLE");
            }
            foreach (var candidate in PondCreatureCatalog.Ambient)
            {
                Require(SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.PondId,
                    candidate.Id, null, SpeciesRole.Ambient, out _, out _), "Pond ambient rejected");
                Require(!SpeciesCompatibility.TryResolvePondAsset(WorldCatalog.OceanId,
                    candidate.Id, null, SpeciesRole.Ambient, out _, out _), "Pond ambient leaked to Ocean");
            }
            Reject("lake", "FAF-FISH-001", SpeciesRole.Selectable, "SPECIES_WORLD_UNKNOWN");
            Reject(null, "FAF-FISH-001", SpeciesRole.Selectable, "SPECIES_WORLD_UNKNOWN");
            Reject(WorldCatalog.OceanId, null, SpeciesRole.Selectable, "SPECIES_ID_UNKNOWN");
            Reject(WorldCatalog.OceanId, "FAF-FISH-999", SpeciesRole.Selectable, "SPECIES_ID_UNKNOWN");
            Require(CharacterCatalog.Find("FAF-FISH-999") is null, "unknown ID fell back");
            Reject(WorldCatalog.OceanId, "FAF-FISH-001", SpeciesRole.Ambient,
                "SPECIES_BAKE_INCOMPATIBLE", "world-wrong");
            Require(!FafAmbientReefLife.TrySpawnEntry(WorldCatalog.PondId, "FAF-FISH-001",
                out _, out _, out var spawnDiagnostic) &&
                spawnDiagnostic.StartsWith("SPECIES_HABITAT_INCOMPATIBLE"), "ambient entry bypassed policy");

            var fixture = CharacterCatalog.Default with { Id = "FIXTURE-POND-001",
                Name = "Primitive Pond fixture only", ModelPath = "res://dev/pond_species_fixture.tscn",
                NativeHabitat = WorldHabitat.Freshwater, FixtureOnly = true, Ambient = true };
            var fixtureCatalog = new[] { fixture };
            Require(SpeciesCompatibility.TryResolveFrom(fixtureCatalog, WorldCatalog.PondId,
                fixture.Id, SpeciesRole.Selectable, out _, out _, allowFixture: true) &&
                SpeciesCompatibility.TryResolveFrom(fixtureCatalog, WorldCatalog.PondId,
                fixture.Id, SpeciesRole.Ambient, out _, out _, allowFixture: true),
                "fixture Pond eligibility");
            Require(!SpeciesCompatibility.TryResolveFrom(fixtureCatalog, WorldCatalog.PondId,
                fixture.Id, SpeciesRole.Selectable, out _, out var fixtureDiagnostic) &&
                fixtureDiagnostic.StartsWith("SPECIES_FIXTURE_ONLY"), "fixture entered production policy");
            Require(!SpeciesCompatibility.TryResolveFrom(fixtureCatalog, WorldCatalog.OceanId,
                fixture.Id, SpeciesRole.Selectable, out _, out _, allowFixture: true),
                "fixture leaked into Ocean");
            var missing = fixture with { Id = "FIXTURE-MISSING", ModelPath = "res://dev/no-such-model.tscn" };
            Require(!SpeciesCompatibility.TryResolveFrom(new[] { missing }, WorldCatalog.PondId,
                missing.Id, SpeciesRole.Selectable, out _, out var missingDiagnostic, allowFixture: true) &&
                missingDiagnostic.StartsWith("SPECIES_MODEL_MISSING"), "missing model accepted");
            var nonNative = fixture with { Id = "FIXTURE-BAD-HABITAT", NativeHabitat = (WorldHabitat)99 };
            Require(!SpeciesCompatibility.TryResolveFrom(new[] { nonNative }, WorldCatalog.PondId,
                nonNative.Id, SpeciesRole.Selectable, out _, out _, allowFixture: true),
                "unknown habitat tag accepted");

            WorldSession.Select(WorldCatalog.PondId);
            DemoSelection.Clear();
            Require(!PlayerArt.TryLoadAndAttach(main.GetNode<FishController>("Fish"),
                out var noSelectionDiagnostic) &&
                noSelectionDiagnostic.StartsWith("SPECIES_SELECTION_REQUIRED"),
                "Pond loaded legacy marine fish without selection");
            var selectionScene = GD.Load<PackedScene>("res://scenes/character_select.tscn");
            var pondSelect = selectionScene.Instantiate<CharacterSelect>();
            main.AddChild(pondSelect);
            Require(pondSelect.FindChild("Character_0", true, false) is Button &&
                pondSelect.FindChild("Character_4", true, false) is Button &&
                pondSelect.FindChild("PreviewModel", true, false) is Node3D &&
                pondSelect.FindChild("EnterWorldButton", true, false) is Button { Disabled: false },
                "Pond populated selection state");
            pondSelect._Input(new InputEventKey { Keycode = Key.Right, Pressed = true });
            Require(pondSelect.FindChild("Character_1", true, false) is Button next &&
                next.Text.Contains("SELECTED"), "Pond focus did not advance");
            pondSelect.Free();
            WorldSession.Select(WorldCatalog.OceanId);
            var oceanSelect = selectionScene.Instantiate<CharacterSelect>();
            main.AddChild(oceanSelect);
            var first = oceanSelect.FindChild("Character_0", true, false) as Button;
            Require(first is not null && first.Text.Contains("SELECTED") &&
                oceanSelect.FindChild("Character_4", true, false) is Button &&
                oceanSelect.FindChild("PreviewModel", true, false) is Node3D,
                "Ocean selection did not reset after Pond scene reload");
            oceanSelect._Input(new InputEventKey { Keycode = Key.Left, Pressed = true });
            Require(oceanSelect.FindChild("Character_4", true, false) is Button last &&
                last.Text.Contains("SELECTED"), "Ocean focus wrap lost last fish");
            oceanSelect._Input(new InputEventKey { Keycode = Key.Right, Pressed = true });
            Require(first!.Text.Contains("SELECTED"), "Ocean focus wrap lost first fish");
            WorldSession.Select(WorldCatalog.PondId);
            oceanSelect._Process(0);
            Require(oceanSelect.FindChild("Character_0", true, false) is Button &&
                oceanSelect.FindChild("Character_4", true, false) is Button &&
                oceanSelect.FindChild("PreviewModel", true, false) is Node3D &&
                oceanSelect.FindChild("EnterWorldButton", true, false) is Button { Disabled: false },
                "live habitat change did not rebuild Pond choices or preview");
            WorldSession.Select(WorldCatalog.OceanId);
            oceanSelect._Process(0);
            Require(oceanSelect.FindChild("Character_0", true, false) is Button reset &&
                reset.Text.Contains("SELECTED") &&
                oceanSelect.FindChild("PreviewModel", true, false) is Node3D,
                "live habitat return did not restore first Ocean choice");
            oceanSelect.Free();

            var habitatScene = ocean.LoadHabitatScene();
            var wrongHabitat = habitatScene.Instantiate<Node3D>();
            try
            {
                try
                {
                    FullWorldPackage.Load(ocean.Bake!.Directory, wrongHabitat, WorldCatalog.PondId);
                    throw new Exception("Ocean bake loaded as Pond");
                }
                catch (System.IO.InvalidDataException error)
                {
                    Require(error.Message.Contains("species incompatible"),
                        "wrong-world bake lacked species diagnostic");
                }
            }
            finally { wrongHabitat.Free(); }
            string? firstPopulation = null;
            for (var pass = 0; pass < 2; pass++)
            {
                WorldSession.Select(WorldCatalog.OceanId);
                var habitat = habitatScene.Instantiate<Node3D>();
                try
                {
                    var loaded = WorldCatalog.Load(ocean.Id, habitat);
                    var ambient = habitat.GetNode<FafAmbientReefLife>(
                        "Composition/ReefComposition/AmbientLife");
                    Require(loaded.Seed == 42 && loaded.WorldId == ocean.Bake!.WorldId &&
                        ambient.FishCount == 31 && ambient.SchoolCount == 9 &&
                        ambient.Population.All(f => f.Id is not "FAF-FISH-005"),
                        "accepted ambient population");
                    var signature = string.Join("|", ambient.Population.Select(f =>
                        $"{f.Id}:{f.SchoolName}:{f.MemberIndex}:{f.Clip}"));
                    Require(firstPopulation is null || signature == firstPopulation,
                        "repeat load changed deterministic population");
                    firstPopulation = signature;
                    var baked = ambient.ExportBaked();
                    Require(FafAmbientReefLife.ValidateBakedSpecies(baked, ocean.Id,
                        loaded.WorldId, out _), "valid baked species rejected");
                    Require(!FafAmbientReefLife.ValidateBakedSpecies(baked, WorldCatalog.PondId,
                        loaded.WorldId, out _), "Ocean ambient allowed in Pond bake");
                    Require(!FafAmbientReefLife.ValidateBakedSpecies(baked, ocean.Id,
                        "wrong-world", out _), "wrong bake accepted");
                    var badSchools = baked.Schools.ToArray();
                    badSchools[0] = badSchools[0] with { FishId = "FAF-FISH-999" };
                    Require(!FafAmbientReefLife.ValidateBakedSpecies(
                        baked with { Schools = badSchools }, ocean.Id, loaded.WorldId, out _),
                        "invalid ambient school accepted");
                    WorldSession.Bind(habitat, loaded);
                    WorldSession.Select(WorldCatalog.PondId);
                    Require(ambient.FishCount == 0 && ambient.SchoolCount == 0 &&
                        ambient.GetNode<Node3D>("Visuals").GetChildCount() == 0 &&
                        WorldSession.ActiveHabitat is null, "world switch retained ambient population");
                }
                finally { WorldSession.Unload(); habitat.Free(); }
            }
            WorldSession.Select(WorldCatalog.OceanId);
            GD.Print("SPECIES_CHECK_OK marine=5 ocean-selectable=5 pond-selectable=5 ocean-ambient=31 pond-wildlife=7 fixture=isolated switch=cleared seed=42");
            main.GetTree().Quit();
        }
        catch (Exception error)
        {
            GD.PushError($"SPECIES_CHECK_FAILED {error}");
            main.GetTree().Quit(1);
        }
    }

    private static void Accept(string world, string id, SpeciesRole role) =>
        Require(SpeciesCompatibility.TryResolve(world, id, role, out _, out var diagnostic),
            $"{world}/{id}/{role} rejected {diagnostic}");
    private static void Reject(string? world, string? id, SpeciesRole role,
        string expected, string? bakedWorldId = null) =>
        Require(!SpeciesCompatibility.TryResolve(world, id, role, out _, out var diagnostic,
            bakedWorldId) && diagnostic.StartsWith(expected),
            $"{world}/{id}/{role} unexpected {diagnostic}");
    private static void Require(bool valid, string message)
    { if (!valid) throw new Exception(message); }
}
