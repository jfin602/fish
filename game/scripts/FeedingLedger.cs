#nullable enable
using System;
using System.Collections.Generic;

namespace Fish;

/// <summary>Session-local authoritative feeding result. A successful ID commits once.</summary>
public sealed class FeedingLedger
{
    public const string MicrofaunaId = "pond:zooplankton";
    public const string InsectId = "FAF-INSECT-001A";
    public const double YolkSeconds = 12;
    public const float BluegillLarvaRatio = .60f;
    public const float BluegillJuvenileRatio = .45f;
    private readonly HashSet<string> _consumed = new(StringComparer.Ordinal);
    private readonly List<FeedEvent> _events = new();
    public int Nutrition { get; private set; }
    public IReadOnlyList<FeedEvent> Events => _events;
    public bool IsConsumed(string id) => _consumed.Contains(id);
    public static bool IsEligiblePrey(string speciesId, string stage, float length,
        string playerStage, float playerLength) =>
        speciesId == PikeEncounterAssets.BluegillId && playerStage is "subadult" or "adult" &&
        float.IsFinite(length) && float.IsFinite(playerLength) &&
        length > 0f && playerLength > 0f &&
        (stage == "larva" && length <= playerLength * BluegillLarvaRatio ||
         stage == "juvenile" && playerStage == "adult" &&
         length <= playerLength * BluegillJuvenileRatio);

    public static int FoodAward(string kind, string playerStage, double secondsSinceHatch) =>
        kind switch
        {
            MicrofaunaId when playerStage == "larva" && double.IsFinite(secondsSinceHatch) &&
                secondsSinceHatch >= YolkSeconds => 1,
            InsectId when playerStage is "juvenile" or "subadult" => 2,
            _ => 0,
        };

    public FeedResult TryConsumeFood(string id, string kind, string playerStage,
        bool accessible, double gameSeconds, double secondsSinceHatch)
    {
        if (string.IsNullOrWhiteSpace(id) || _consumed.Contains(id))
            return new(false, "already consumed or invalid ID", 0);
        var award = FoodAward(kind, playerStage, secondsSinceHatch);
        if (award == 0) return new(false, "food is not available at this stage", 0);
        if (!accessible) return new(false, "out of reach or blocked", 0);
        if (!double.IsFinite(gameSeconds) || gameSeconds < 0)
            return new(false, "invalid game time", 0);
        _consumed.Add(id);
        Nutrition += award;
        _events.Add(new FeedEvent(id, kind, gameSeconds, award));
        return new(true, "consumed", award);
    }

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
        var award = target.Stage == "juvenile" ? 6 : 4;
        // All validation precedes the single commit; this is the only nutrition mutation.
        _consumed.Add(target.Id);
        Nutrition += award;
        _events.Add(new FeedEvent(target.Id, target.Stage, gameSeconds, award));
        return new(true, "consumed", award);
    }

    public FeedResult TryConsumeEggShell(string id, bool accessible, double gameSeconds)
    {
        if (string.IsNullOrWhiteSpace(id) || _consumed.Contains(id))
            return new(false, "already consumed or invalid ID", 0);
        if (!accessible) return new(false, "out of reach or blocked", 0);
        if (!double.IsFinite(gameSeconds) || gameSeconds < 0)
            return new(false, "invalid game time", 0);
        _consumed.Add(id);
        // A shed shell is a first bite, not a growth meal. M3 owns larval diet.
        _events.Add(new FeedEvent(id, "egg-shell", gameSeconds, 0));
        return new(true, "consumed", 0);
    }
}

public readonly record struct FeedCandidate(string Id, string SpeciesId,
    string Stage, string Habitat, string PlayerHabitat, float Length,
    string PlayerStage, float PlayerLength, bool Alive);
public readonly record struct FeedResult(bool Consumed, string Reason, int Award);
public readonly record struct FeedEvent(string TargetId, string Stage, double GameSeconds, int Nutrition);
