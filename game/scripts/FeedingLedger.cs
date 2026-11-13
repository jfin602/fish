#nullable enable
using System;
using System.Collections.Generic;

namespace Fish;

/// <summary>Session-local authoritative feeding result. A successful ID commits once.</summary>
public sealed class FeedingLedger
{
    private readonly HashSet<string> _consumed = new(StringComparer.Ordinal);
    private readonly List<FeedEvent> _events = new();
    public int Nutrition { get; private set; }
    public IReadOnlyList<FeedEvent> Events => _events;
    public bool IsConsumed(string id) => _consumed.Contains(id);
    public static bool IsEligiblePrey(string speciesId, string stage, float length, float playerLength) =>
        speciesId == "FAF-FISH-007" && stage == "larva" &&
        float.IsFinite(length) && float.IsFinite(playerLength) &&
        length > 0f && playerLength > 0f && length <= playerLength * .45f;

    public FeedResult TryConsume(FeedCandidate target, bool accessible, double gameSeconds)
    {
        if (string.IsNullOrWhiteSpace(target.Id) || _consumed.Contains(target.Id))
            return new(false, "already consumed or invalid ID", 0);
        if (!target.Alive) return new(false, "target is dead", 0);
        if (target.Habitat != WorldCatalog.PondId || target.PlayerHabitat != target.Habitat)
            return new(false, "incompatible habitat", 0);
        if (target.Kind is not (FeedKind.Prey or FeedKind.TinyFood))
            return new(false, "unknown food kind", 0);
        if (target.Kind == FeedKind.Prey &&
            !IsEligiblePrey(target.SpeciesId, target.Stage, target.Length, target.PlayerLength))
            return new(false, "prey is not eligible", 0);
        if (target.Kind == FeedKind.TinyFood &&
            (target.SpeciesId != "fixture-invertebrate" || target.Stage != "proxy"))
            return new(false, "food is not eligible", 0);
        if (!accessible) return new(false, "out of reach or blocked", 0);
        if (!double.IsFinite(gameSeconds) || gameSeconds < 0)
            return new(false, "invalid game time", 0);
        var award = target.Kind == FeedKind.Prey ? 4 : 1;
        // All validation precedes the single commit; this is the only nutrition mutation.
        _consumed.Add(target.Id);
        Nutrition += award;
        _events.Add(new FeedEvent(target.Id, target.Kind, gameSeconds, award));
        return new(true, "consumed", award);
    }
}

public enum FeedKind { TinyFood, Prey }
public readonly record struct FeedCandidate(string Id, FeedKind Kind, string SpeciesId,
    string Stage, string Habitat, string PlayerHabitat, float Length, float PlayerLength, bool Alive);
public readonly record struct FeedResult(bool Consumed, string Reason, int Award);
public readonly record struct FeedEvent(string TargetId, FeedKind Kind, double GameSeconds, int Nutrition);
