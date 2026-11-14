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
    public static bool IsEligiblePrey(string speciesId, string stage, float length,
        string playerStage, float playerLength) =>
        speciesId == PikeEncounterAssets.BluegillId &&
        float.IsFinite(length) && float.IsFinite(playerLength) &&
        length > 0f && playerLength > 0f &&
        (stage == "larva" && length <= playerLength * .65f ||
         stage == "juvenile" && playerStage == "adult" && length <= playerLength * .8f);

    public FeedResult TryConsume(FeedCandidate target, bool accessible, double gameSeconds)
    {
        if (string.IsNullOrWhiteSpace(target.Id) || _consumed.Contains(target.Id))
            return new(false, "already consumed or invalid ID", 0);
        if (!target.Alive) return new(false, "target is dead", 0);
        if (target.Habitat != WorldCatalog.PondId || target.PlayerHabitat != target.Habitat)
            return new(false, "incompatible habitat", 0);
        if (!IsEligiblePrey(target.SpeciesId, target.Stage, target.Length,
                target.PlayerStage, target.PlayerLength))
            return new(false, "prey is not eligible", 0);
        if (!accessible) return new(false, "out of reach or blocked", 0);
        if (!double.IsFinite(gameSeconds) || gameSeconds < 0)
            return new(false, "invalid game time", 0);
        const int award = 4;
        // All validation precedes the single commit; this is the only nutrition mutation.
        _consumed.Add(target.Id);
        Nutrition += award;
        _events.Add(new FeedEvent(target.Id, target.Stage, gameSeconds, award));
        return new(true, "consumed", award);
    }
}

public readonly record struct FeedCandidate(string Id, string SpeciesId,
    string Stage, string Habitat, string PlayerHabitat, float Length,
    string PlayerStage, float PlayerLength, bool Alive);
public readonly record struct FeedResult(bool Consumed, string Reason, int Award);
public readonly record struct FeedEvent(string TargetId, string Stage, double GameSeconds, int Nutrition);
