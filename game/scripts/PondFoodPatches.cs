#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

public sealed record PondFoodPatchSite(string Id, string Region, string Kind,
    Vector3 Anchor, int InitialCount, double RecoverySeconds);

/// <summary>Session-local food population on the installed Pond's seed-derived habitat anchors.</summary>
public sealed class PondFoodPatches
{
    public const int MaximumActors = 23;
    private sealed class Patch
    {
        public readonly PondFoodPatchSite Site;
        public readonly List<PondFoodActor> Actors = new();
        public int Generation;
        public double NextRecovery = double.PositiveInfinity;
        public Vector3 LastConsumedPosition;
        public Patch(PondFoodPatchSite site) => Site = site;
    }

    private readonly Node3D _owner;
    private readonly FishController _fish;
    private readonly SeabedSurface _bed;
    private readonly List<PondFoodActor> _actors;
    private readonly List<Patch> _patches = new();
    private readonly Dictionary<string, Patch> _byId = new(StringComparer.Ordinal);
    private readonly ulong _seed;

    public IReadOnlyList<PondFoodPatchSite> Sites => _patches.Select(p => p.Site).ToArray();
    public int AvailableCount => _actors.Count(a => a.Available);

    public PondFoodPatches(Node3D owner, FishController fish, SeabedSurface bed,
        List<PondFoodActor> actors, string worldId, ulong seed, Vector3 nest,
        IReadOnlyList<PondFafPlacement> placements)
    {
        _owner = owner;
        _fish = fish;
        _bed = bed;
        _actors = actors;
        _seed = seed;
        foreach (var site in PlanSites(bed.Basin, nest, placements, worldId))
        {
            var patch = new Patch(site);
            _patches.Add(patch);
            for (var slot = 0; slot < site.InitialCount; slot++) Spawn(patch, slot, 0);
        }
        if (_actors.Count != MaximumActors || _actors.Select(a => a.StableId).Distinct().Count() != MaximumActors)
            throw new InvalidOperationException("POND_FOOD_POPULATION_INVALID");
    }

    public static IReadOnlyList<PondFoodPatchSite> PlanSites(GeneratedBasin basin, Vector3 nest,
        IReadOnlyList<PondFafPlacement> placements, string worldId)
    {
        if (!basin.IsPond || string.IsNullOrWhiteSpace(worldId))
            throw new ArgumentException("Pond seed and world identity required");
        var nest2 = new Vector2(nest.X, nest.Z);
        var inward = (basin.DeepCore - nest2).Normalized();
        var side = new Vector2(-inward.Y, inward.X);
        var chosen = new List<Vector2>();
        var sites = new List<PondFoodPatchSite>();
        void Add(string region, string kind, int count, double recovery,
            string? family, float targetDistance, int sideSign)
        {
            Vector3 anchor;
            if (family is null) anchor = nest;
            else
            {
                var candidates = placements.Where(p => family switch
                {
                    "reeds" => p.Zone is "reed" or "cattail" ||
                        (p.Zone == "surface_shore" && p.Id is "FAF-PLANT-006A" or "FAF-PLANT-007B"),
                    "wood" => p.Zone is "fallen_log" or "stump" or "hollow_trunk",
                    "vegetation" => p.Zone == "submerged_algae",
                    _ => false,
                }).Select(p => new { p.Position, Horizontal = new Vector2(p.Position.X, p.Position.Z) })
                    .Where(p => basin.DistanceToShore(p.Position.X, p.Position.Z) > -1.5f &&
                        p.Horizontal.DistanceTo(nest2) is > 1.5f and < 65f)
                    .OrderBy(p => Mathf.Abs(p.Horizontal.DistanceTo(nest2) - targetDistance) +
                        (sideSign == 0 ? 0f : (p.Horizontal - nest2).Dot(side) * sideSign < 0 ? 9f : 0f) +
                        (chosen.Any(c => c.DistanceTo(p.Horizontal) < 2.5f) ? 20f : 0f) +
                        Hash(basin.Seed, region, p.Position.X.GetHashCode()) % 1000 / 10000f)
                    .ToArray();
                if (candidates.Length == 0)
                    throw new InvalidOperationException($"POND_FOOD_HABITAT_MISSING region={region}");
                anchor = candidates[0].Position;
                chosen.Add(candidates[0].Horizontal);
            }
            sites.Add(new PondFoodPatchSite($"{worldId}:patch:{basin.Seed}:{region}", region,
                kind, anchor, count, recovery));
        }
        Add("nursery", FeedingLedger.MicrofaunaId, 3, 0, null, 0, 0);
        Add("reeds-west", FeedingLedger.MicrofaunaId, 3, 150, "reeds", 4, 1);
        Add("reeds-east", FeedingLedger.MicrofaunaId, 3, 150, "reeds", 5, -1);
        Add("roots", FeedingLedger.MicrofaunaId, 3, 180, "wood", 11, 0);
        Add("vegetation", FeedingLedger.MicrofaunaId, 3, 180, "vegetation", 18, 0);
        Add("reeds-insect", FeedingLedger.InsectId, 2, 210, "reeds", 10, 0);
        Add("wood-insect", FeedingLedger.InsectId, 2, 210, "wood", 20, 0);
        Add("transition-insect", FeedingLedger.InsectId, 2, 240, "vegetation", 27, 0);
        Add("midwater-insect", FeedingLedger.InsectId, 2, 240, "vegetation", 38, 0);
        return sites;
    }

    private static uint Hash(ulong seed, string region, int value)
    {
        uint hash = 2166136261;
        foreach (var c in region) hash = (hash ^ c) * 16777619;
        hash = (hash ^ (uint)seed) * 16777619;
        hash = (hash ^ (uint)(seed >> 32)) * 16777619;
        return (hash ^ (uint)value) * 16777619;
    }

    private Vector3 FindPosition(Patch patch, int slot, int generation)
    {
        var anchor = patch.Site.Anchor;
        var inward = (_bed.Basin.DeepCore - new Vector2(anchor.X, anchor.Z)).Normalized();
        var phase = Hash(_seed, patch.Site.Region, slot + generation * 37) % 6283 / 1000f;
        for (var attempt = 0; attempt < 80; attempt++)
        {
            var angle = phase + attempt * 2.3999632f;
            var radius = patch.Site.Region == "nursery" ? .22f + attempt % 3 * .09f :
                .35f + attempt % 8 * .22f;
            var horizontal = new Vector2(anchor.X, anchor.Z) +
                new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius + inward *
                (patch.Site.Region == "nursery" ? .05f : .5f);
            var x = horizontal.X;
            var z = horizontal.Y;
            var floor = _bed.GetHeightAt(x, z);
            var surface = _fish.PondSurfaceLimitAt(x, z);
            var lift = patch.Site.Kind == FeedingLedger.MicrofaunaId ? .18f : .30f;
            var at = new Vector3(x, floor + lift, z);
            if (_bed.Basin.DistanceToShore(x, z) < 1f || surface - floor < lift + .22f ||
                surface - at.Y < .15f ||
                (generation > 0 && at.DistanceTo(patch.LastConsumedPosition) < .20f) ||
                !Clear(at, .065f) || !Clear(at + Vector3.Up * .12f, .08f)) continue;
            return at;
        }
        throw new InvalidOperationException($"POND_FOOD_SITE_BLOCKED region={patch.Site.Region} slot={slot}");
    }

    private bool Clear(Vector3 position, float radius)
    {
        using var shape = new SphereShape3D { Radius = radius };
        using var query = new PhysicsShapeQueryParameters3D {
            Shape = shape, Transform = new Transform3D(Basis.Identity, position),
            CollisionMask = 1, Exclude = new Godot.Collections.Array<Rid> { _fish.GetRid() } };
        return _owner.GetWorld3D().DirectSpaceState.IntersectShape(query, 1).Count == 0;
    }

    private void Spawn(Patch patch, int slot, int generation)
    {
        var at = FindPosition(patch, slot, generation);
        var kind = patch.Site.Kind;
        var actor = new PondFoodActor { Name = (kind == FeedingLedger.MicrofaunaId ?
            "Zooplankton_" : "DarnerNymph_") + patch.Site.Region + "_" + generation + "_" + slot };
        _owner.AddChild(actor);
        actor.Configure($"{patch.Site.Id}:{kind}:{slot}:g{generation}", kind, at,
            (int)(Hash(_seed, patch.Site.Region, slot + generation * 37) % 10000));
        patch.Actors.Add(actor);
        _actors.Add(actor);
        _byId.Add(actor.StableId, patch);
    }

    public void Consumed(PondFoodActor actor, double gameSeconds)
    {
        if (!_byId.TryGetValue(actor.StableId, out var patch) || actor.Available)
            throw new InvalidOperationException("POND_FOOD_CONSUMPTION_ORDER_INVALID");
        patch.LastConsumedPosition = actor.GlobalPosition;
        if (patch.Site.RecoverySeconds > 0 && patch.Actors.All(a => !a.Available))
            patch.NextRecovery = gameSeconds + patch.Site.RecoverySeconds;
    }

    public void Tick(double gameSeconds)
    {
        foreach (var actor in _actors) actor.Tick(gameSeconds);
        foreach (var patch in _patches)
        {
            if (gameSeconds < patch.NextRecovery) continue;
            foreach (var actor in patch.Actors)
            {
                _byId.Remove(actor.StableId);
                _actors.Remove(actor);
                actor.QueueFree();
            }
            patch.Actors.Clear();
            patch.Generation++;
            Spawn(patch, patch.Generation % patch.Site.InitialCount, patch.Generation);
            patch.NextRecovery = double.PositiveInfinity;
        }
        if (_actors.Count > MaximumActors)
            throw new InvalidOperationException("POND_FOOD_ACTOR_LIMIT_EXCEEDED");
    }
}
