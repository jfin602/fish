#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

/// <summary>A live physical actor, never a school, HUD marker, or presentation effect.</summary>
public readonly record struct PondEcologyCandidate(string Id, string Species, string Stage,
    float Length, float Radius, Rid BodyRid, Node3D Body, bool Player, bool Alive)
{
    public Vector3 Position => Body.GlobalPosition;
}

public readonly record struct PondEcologyDiagnostics(int ActiveActors, int Queries,
    int CandidateChecks, int ClaimedPrey, int SightSamples, int SearchStates,
    int Interactions);

/// <summary>Scene-local spatial roster and one-claim-per-prey arbitration.</summary>
public sealed class PondEcology
{
    public const int MaxNearby = 8;
    public const int MaxActors = 64;
    public const float CellSize = 4f;
    public const double RefreshSeconds = .2;
    private readonly Dictionary<(int X, int Y, int Z), List<PondEcologyCandidate>> _cells = new();
    private readonly Dictionary<string, PondEcologyCandidate> _actors = new(StringComparer.Ordinal);
    private readonly HashSet<string> _claimed = new(StringComparer.Ordinal);
    private double _lastRefresh = double.NegativeInfinity;
    public int ActiveActors => _actors.Count;
    public int CandidateChecks { get; private set; }
    public int Queries { get; private set; }
    public int Claims => _claimed.Count;

    public static bool BluegillCanEatNpc(PreyActor hunter, PondEcologyCandidate prey) =>
        hunter.SpeciesId == PikeEncounterAssets.BluegillId && hunter.Stage == "adult" &&
        !prey.Player && prey.Body is PreyActor && prey.Alive &&
        (prey.Species == PikeEncounterAssets.BassId
            ? BluegillPredation.Eligible(hunter.SpeciesId, hunter.Stage, hunter.Length,
                prey.Species, prey.Stage, prey.Length)
            : prey.Species == PikeEncounterAssets.BluegillId && prey.Stage == "larva" &&
              prey.Length > 0 && prey.Length <= hunter.Length * BluegillPredation.MaximumPreyLengthRatio);

    public void Reset()
    {
        _cells.Clear(); _actors.Clear(); _claimed.Clear();
        _lastRefresh = double.NegativeInfinity;
        CandidateChecks = Queries = 0;
    }

    public void Refresh(IEnumerable<PondEcologyCandidate> actors, double seconds)
    {
        if (seconds >= _lastRefresh && seconds - _lastRefresh < RefreshSeconds) return;
        _lastRefresh = seconds;
        _cells.Clear(); _actors.Clear();
        foreach (var actor in actors)
        {
            if (!actor.Alive || _claimed.Contains(actor.Id) ||
                !GodotObject.IsInstanceValid(actor.Body)) continue;
            if (!_actors.TryAdd(actor.Id, actor))
                throw new InvalidOperationException("POND_ECOLOGY_DUPLICATE_ID " + actor.Id);
            if (_actors.Count > MaxActors)
                throw new InvalidOperationException("POND_ECOLOGY_ACTOR_BUDGET");
            var key = Cell(actor.Position);
            if (!_cells.TryGetValue(key, out var bucket))
                _cells[key] = bucket = new List<PondEcologyCandidate>();
            bucket.Add(actor);
        }
    }

    public IReadOnlyList<PondEcologyCandidate> Nearby(Vector3 origin, float range,
        string observerId, Func<PondEcologyCandidate, bool> eligible,
        string? pinnedId = null)
    {
        Queries++;
        var results = new List<PondEcologyCandidate>(MaxNearby);
        var center = Cell(origin);
        var radius = Mathf.CeilToInt(range / CellSize);
        var rangeSquared = range * range;
        IEnumerable<List<PondEcologyCandidate>> buckets;
        if (radius > 4)
            buckets = _cells.Values; // At most MaxActors occupied cells, even for a wide Pike scan.
        else
        {
            var nearby = new List<List<PondEcologyCandidate>>();
            for (var x = center.X - radius; x <= center.X + radius; x++)
            for (var y = center.Y - radius; y <= center.Y + radius; y++)
            for (var z = center.Z - radius; z <= center.Z + radius; z++)
                if (_cells.TryGetValue((x, y, z), out var bucket)) nearby.Add(bucket);
            buckets = nearby;
        }
        foreach (var bucket in buckets)
        {
            foreach (var actor in bucket)
            {
                CandidateChecks++;
                if (actor.Id == observerId || !actor.Alive || _claimed.Contains(actor.Id) ||
                    !eligible(actor) || actor.Position.DistanceSquaredTo(origin) > rangeSquared)
                    continue;
                results.Add(actor);
            }
        }
        return results.OrderBy(a => a.Id == pinnedId ? 0 : 1)
            .ThenBy(a => a.Position.DistanceSquaredTo(origin))
            .ThenBy(a => a.Id, StringComparer.Ordinal).Take(MaxNearby).ToArray();
    }

    public bool IsAvailable(string id, Rid body) =>
        !_claimed.Contains(id) && _actors.TryGetValue(id, out var actor) &&
        actor.BodyRid == body && actor.Alive && GodotObject.IsInstanceValid(actor.Body) &&
        (actor.Player || actor.Body is PreyActor { IsAlive: true });

    public bool TryClaimNpc(string id, Rid body, Func<bool> commit)
    {
        if (!IsAvailable(id, body) || _actors[id].Player || !_claimed.Add(id)) return false;
        if (commit()) { _actors.Remove(id); return true; }
        _claimed.Remove(id);
        return false;
    }

    private static (int X, int Y, int Z) Cell(Vector3 p) =>
        (Mathf.FloorToInt(p.X / CellSize), Mathf.FloorToInt(p.Y / CellSize),
            Mathf.FloorToInt(p.Z / CellSize));
}
