using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Godot;

namespace Fish;

// These versions identify the format and algorithm, not the Fish application version.
public sealed record WorldGenerationSettings(ulong Seed, int SettingsVersion = 1,
    string GeneratorVersion = "c3-fixture-1")
{
    public void Validate()
    {
        if (SettingsVersion != 1 || GeneratorVersion != "c3-fixture-1")
            throw new InvalidDataException("Unsupported world generation settings or generator version");
    }
}

public sealed record WorldCompileProgress(string Stage, int Completed, int Total,
    long ElapsedMilliseconds);

/// <summary>Independent named streams; calling order and frame state cannot perturb another zone.</summary>
public sealed class WorldCompileContext
{
    public WorldGenerationSettings Settings { get; }
    public WorldCompileContext(WorldGenerationSettings settings)
    {
        settings.Validate();
        Settings = settings;
    }

    public StableWorldRandom Stream(string subsystem, string zone)
    {
        if (string.IsNullOrWhiteSpace(subsystem) || string.IsNullOrWhiteSpace(zone))
            throw new ArgumentException("A world stream requires subsystem and zone names");
        var key = FormattableString.Invariant($"{Settings.GeneratorVersion}|{Settings.SettingsVersion}|{Settings.Seed}|{subsystem.Length}:{subsystem}|{zone.Length}:{zone}");
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return new StableWorldRandom(BinaryPrimitives.ReadUInt64LittleEndian(bytes));
    }
}

// Explicit SplitMix64 algorithm: no Godot/global PRNG or platform-dependent Random implementation.
public struct StableWorldRandom
{
    private ulong _state;
    public StableWorldRandom(ulong seed) => _state = seed;
    public ulong Next()
    {
        unchecked
        {
            _state += 0x9e3779b97f4a7c15UL;
            var value = _state;
            value = (value ^ (value >> 30)) * 0xbf58476d1ce4e5b9UL;
            value = (value ^ (value >> 27)) * 0x94d049bb133111ebUL;
            return value ^ (value >> 31);
        }
    }
}

public static class WorldCompiler
{
    // Candidates live under the Godot user-data directory. A caller supplies a child directory,
    // allowing repeated compiles to be compared without overwriting either candidate.
    public static string CandidateRoot => ProjectSettings.GlobalizePath("user://world-candidates");

    public static string CatalogIdentity(IEnumerable<FafEnvironmentEntry> entries)
    {
        var rows = entries.Select(entry => $"{entry.Id}|{entry.ScenePath}|{entry.Family}|{entry.Variant}")
            .OrderBy(row => row, StringComparer.Ordinal);
        return BakedWorldContract.Hash(string.Join("\n", rows));
    }

    public static GeneratedWorldManifest CompileFixture(WorldGenerationSettings settings,
        string outputDirectory, IEnumerable<FafEnvironmentEntry> sourceCatalog,
        Action<WorldCompileProgress> progress = null)
    {
        var context = new WorldCompileContext(settings);
        if (Directory.Exists(outputDirectory) && Directory.EnumerateFileSystemEntries(outputDirectory).Any())
            throw new IOException($"Candidate directory is not empty: {outputDirectory}");
        Directory.CreateDirectory(outputDirectory);
        var timings = new List<WorldStageTiming>();
        var total = Stopwatch.StartNew();
        var stage = Stopwatch.StartNew();
        var catalog = sourceCatalog.OrderBy(entry => entry.Id, StringComparer.Ordinal).ToArray();
        if (catalog.Length == 0 || catalog.Select(entry => entry.Id).Distinct(StringComparer.Ordinal).Count() != catalog.Length)
            throw new InvalidDataException("Catalog is empty or contains duplicate IDs");
        var catalogHash = CatalogIdentity(catalog);
        Finish("catalog", 1);

        stage.Restart();
        // Tiny schema fixture only. P2+ replace this payload with basin, geology, routes and placement.
        // Each zone has its own stream, so traversal order cannot change the sample values.
        var zones = new[] { "deep", "shallow", "middle" }.OrderBy(x => x, StringComparer.Ordinal);
        var samples = zones.Select(zone =>
        {
            var random = context.Stream("fixture-basin", zone);
            return new FixtureSample(zone, random.Next(), random.Next());
        }).ToArray();
        var fixture = new FixtureWorld(1, samples);
        var fixtureBytes = JsonSerializer.SerializeToUtf8Bytes(fixture);
        var contentHash = BakedWorldContract.Hash(fixtureBytes);
        Finish("fixture", 2);

        stage.Restart();
        var worldId = BakedWorldContract.WorldId(settings.GeneratorVersion, settings.SettingsVersion, settings.Seed, catalogHash);
        File.WriteAllBytes(Path.Combine(outputDirectory, BakedWorldContract.FixtureFileName), fixtureBytes);
        Finish("bake", 3);

        stage.Restart();
        // Timings are diagnostic and deliberately excluded from the identity/content hashes.
        var manifest = new GeneratedWorldManifest(BakedWorldContract.FormatVersion, worldId, settings.Seed,
            settings.SettingsVersion, settings.GeneratorVersion, catalogHash,
            new[] { new BakedResource(BakedWorldContract.FixtureFileName, contentHash) },
            new WorldSummary(samples.Length, contentHash), timings);
        // Measure serialization/write, then persist the final diagnostic timing in the manifest.
        var manifestPath = Path.Combine(outputDirectory, BakedWorldContract.ManifestFileName);
        File.WriteAllBytes(manifestPath, JsonSerializer.SerializeToUtf8Bytes(manifest,
            new JsonSerializerOptions { WriteIndented = true }));
        Finish("manifest", 4);
        File.WriteAllBytes(manifestPath, JsonSerializer.SerializeToUtf8Bytes(manifest,
            new JsonSerializerOptions { WriteIndented = true }));
        return manifest;

        void Finish(string name, int completed)
        {
            stage.Stop();
            timings.Add(new WorldStageTiming(name, stage.ElapsedMilliseconds));
            progress?.Invoke(new WorldCompileProgress(name, completed, 4, total.ElapsedMilliseconds));
        }
    }

}
