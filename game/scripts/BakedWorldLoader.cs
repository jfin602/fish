using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Fish;

/// <summary>Reads and verifies a saved package. No generator or compile context is invoked here.</summary>
public static class BakedWorldLoader
{
    public static (GeneratedWorldManifest Manifest, IReadOnlyDictionary<string, byte[]> Resources)
        Load(string candidateDirectory, string expectedCatalogSha256)
    {
        var manifestPath = Path.Combine(candidateDirectory, BakedWorldContract.ManifestFileName);
        var manifest = JsonSerializer.Deserialize<GeneratedWorldManifest>(File.ReadAllBytes(manifestPath))
            ?? throw new InvalidDataException("Empty world manifest");
        if (manifest.FormatVersion != BakedWorldContract.FormatVersion || manifest.SettingsVersion != 1 ||
            manifest.GeneratorVersion != "c3-fixture-1" ||
            !string.Equals(manifest.CatalogSha256, expectedCatalogSha256, StringComparison.Ordinal) ||
            manifest.WorldId != BakedWorldContract.WorldId(manifest.GeneratorVersion,
                manifest.SettingsVersion, manifest.Seed, manifest.CatalogSha256) || manifest.Resources is null ||
            manifest.Resources.Count == 0 || manifest.Summary is null)
            throw new InvalidDataException("Unsupported or incompatible baked world manifest");

        var resources = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        foreach (var resource in manifest.Resources)
        {
            if (string.IsNullOrWhiteSpace(resource.Path) || Path.IsPathRooted(resource.Path) ||
                resource.Path.Contains('/') || resource.Path.Contains('\\') || resource.Path is "." or ".." ||
                !resources.TryAdd(resource.Path, Array.Empty<byte>()))
                throw new InvalidDataException("Invalid or duplicate baked resource path");
            var bytes = File.ReadAllBytes(Path.Combine(candidateDirectory, resource.Path));
            if (!string.Equals(BakedWorldContract.Hash(bytes), resource.Sha256, StringComparison.Ordinal))
                throw new InvalidDataException($"Baked resource hash mismatch: {resource.Path}");
            resources[resource.Path] = bytes;
        }
        if (resources.Count != 1 || !resources.TryGetValue(BakedWorldContract.FixtureFileName, out var fixtureBytes) ||
            !string.Equals(BakedWorldContract.Hash(fixtureBytes), manifest.Summary.ContentSha256, StringComparison.Ordinal))
            throw new InvalidDataException("Baked world summary mismatch");
        var fixture = JsonSerializer.Deserialize<FixtureWorld>(fixtureBytes)
            ?? throw new InvalidDataException("Empty baked fixture");
        if (fixture.FormatVersion != 1 || fixture.Samples is null ||
            fixture.Samples.Count != manifest.Summary.FixtureSampleCount ||
            fixture.Samples.Select(x => x.Zone).Distinct(StringComparer.Ordinal).Count() != fixture.Samples.Count)
            throw new InvalidDataException("Invalid baked fixture schema or summary");
        return (manifest, resources);
    }
}
