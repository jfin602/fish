using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Fish;

// Disk contract shared by the offline compiler and the loader. Timings are diagnostics;
// they are excluded from world identity and baked content hashes.
public sealed record WorldStageTiming(string Stage, long ElapsedMilliseconds);
public sealed record BakedResource(string Path, string Sha256);
public sealed record WorldSummary(int FixtureSampleCount, string ContentSha256);
public sealed record GeneratedWorldManifest(
    int FormatVersion, string WorldId, ulong Seed, int SettingsVersion,
    string GeneratorVersion, string CatalogSha256, IReadOnlyList<BakedResource> Resources,
    WorldSummary Summary, IReadOnlyList<WorldStageTiming> Timings);

public static class BakedWorldContract
{
    public const int FormatVersion = 1;
    public const string ManifestFileName = "manifest.json";
    public const string FixtureFileName = "fixture-world.json";

    public static string Hash(string text) => Hash(Encoding.UTF8.GetBytes(text));
    public static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
    public static string WorldId(string generatorVersion, int settingsVersion, ulong seed, string catalogSha256) =>
        "world-" + Hash(FormattableString.Invariant($"{generatorVersion}|{settingsVersion}|{seed}|{catalogSha256}"))[..16];
}

public sealed record FixtureSample(string Zone, ulong A, ulong B);
public sealed record FixtureWorld(int FormatVersion, IReadOnlyList<FixtureSample> Samples);
