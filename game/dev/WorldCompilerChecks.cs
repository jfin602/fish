using System;
using System.IO;
using System.Linq;
using System.Globalization;
using Fish;
using Godot;

public static class WorldCompilerChecks
{
    public static void Run()
    {
        var root = Path.Combine(WorldCompiler.CandidateRoot, "p1-check-" + Guid.NewGuid().ToString("N"));
        try
        {
            var settings = new WorldGenerationSettings(42);
            var stages = 0;
            var first = WorldCompiler.CompileFixture(settings, Path.Combine(root, "same-a"),
                FafRuntimeCatalog.Environment, p => { stages++; GD.Print($"WORLD_COMPILE_STAGE {p.Stage} {p.Completed}/{p.Total} elapsed_ms={p.ElapsedMilliseconds}"); });
            var originalCulture = CultureInfo.CurrentCulture;
            GeneratedWorldManifest second;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
                second = WorldCompiler.CompileFixture(settings, Path.Combine(root, "same-b"),
                    FafRuntimeCatalog.Environment.Reverse());
            }
            finally { CultureInfo.CurrentCulture = originalCulture; }
            var third = WorldCompiler.CompileFixture(new WorldGenerationSettings(43),
                Path.Combine(root, "other-seed"), FafRuntimeCatalog.Environment);
            var loaded = BakedWorldLoader.Load(Path.Combine(root, "same-a"), first.CatalogSha256);
            if (stages != 4 || first.WorldId != second.WorldId ||
                first.CatalogSha256 != second.CatalogSha256 ||
                first.Summary != second.Summary ||
                first.Resources.Single() != second.Resources.Single() ||
                first.WorldId == third.WorldId || first.Summary.ContentSha256 == third.Summary.ContentSha256 ||
                loaded.Manifest.WorldId != first.WorldId || loaded.Manifest.Timings.Count != 4 ||
                loaded.Resources.Count != 1)
                throw new InvalidDataException("Repeatability, catalog order, seed identity or baked load failed");

            // Corruption is a hard failure, never a reason to regenerate at load time.
            var fixturePath = Path.Combine(root, "same-a", BakedWorldContract.FixtureFileName);
            File.AppendAllText(fixturePath, "corrupt");
            var corruptionRejected = false;
            try { BakedWorldLoader.Load(Path.Combine(root, "same-a"), first.CatalogSha256); }
            catch (InvalidDataException) { corruptionRejected = true; }
            if (!corruptionRejected) throw new InvalidDataException("Corrupt baked resource was accepted");
            GD.Print($"WORLD_COMPILER_CHECK_OK world={first.WorldId} catalog={first.CatalogSha256} content={first.Summary.ContentSha256} stages={stages} repeat=true reorder=true culture=true different_seed=true baked_load=true corruption_rejected=true");
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
