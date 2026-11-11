# Survival, saves, and runs

## Status and confirmed rules

**Planned; no save, death, or recovery behavior is implemented or qualified.** One fish's life is one run and can span multiple sessions. Save-and-quit preserves that living fish. Death permanently ends the life, and the next run begins as a hatchling. The closed game does not advance the world.

[Life stories and history](life-stories-and-history.md) owns the completed-life archive. [Technical direction](../technical-direction.md) owns implementation boundaries; exact storage format belongs to the future persistence task.

## Life and session are different

A **run** has a stable identity from hatching until death. A **session** is one period of play within that run. Quitting ends a session, not a life. Returning must restore the same run identity rather than create a replacement fish with similar numbers.

The intended initial interface offers **Continue** for a living saved fish and **New life** when no living fish exists. Multiple simultaneous living save slots are an open design choice. Starting another life while one exists must have an explicit policy; it must not silently erase or narrate the existing fish as dead.

## Habitat and world identity (P4/P6 boundary)

**P4 planned; persistence implementation deferred until P6.** A new life starts with World Select (Ocean or Freshwater Pond), then a compatible species. World Select is not available as a way to move a living fish across habitats. On persistent runs store an authoritative stable **habitat/world ID**, species ID, and the exact generated/baked Base World identity (including any seed/settings/version data needed to reload it), alongside existing fish, encounter and resource state. These values must survive save/close/resume unchanged.

**Continue** must reopen the stored world directly. Do not let menu navigation or failed loading switch a fish from Pond to Ocean or the reverse; an incompatible species/world or missing resource is an explicit save/load/recovery error, never automatic migration, death, or a reset of danger/food. Historical Life History entries retain enough habitat identity to label a completed life truthfully without claiming cross-habitat travel. Exact save format remains a P6 choice.

## Suspend and resume

A supported save captures a coherent game state. Relevant fields include the run identity, native species ID, habitat/world ID, baked-world identity, fish state, location, elapsed simulation time, consumed food/prey, and the world and encounter state needed to continue fairly. The exact representation is a technical choice.

No offline simulation is applied. Hunger, predators, growth, cooldowns, and event timestamps must follow the selected game-time rules rather than elapsed wall-clock time while closed. The history may record real-world creation and completion dates separately from time survived.

Save-and-resume preserves danger. It must not refill health or stamina, respawn food, clear a predator's pursuit, teleport the fish to shelter, or advance growth simply because a scene was rebuilt. Save boundaries should occur at coherent simulation points so a consumed target cannot reappear with its reward already granted.

An application interruption can require recovery from the last durable save according to a documented checkpoint policy. It is not a fictional death. Do not promise zero lost progress before the persistence policy has been implemented and tested.

## Terminal death

Only an authoritative gameplay outcome ends a life. Examples may include being eaten or other explicitly adopted damage rules. A low resource reading alone must not imply death before its selected terminal condition occurs.

The terminal transition must be durable and idempotent:

1. Record the actual terminal event, run identity, final state, and cause.
2. Make that run ineligible for ordinary continuation.
3. Finalize its completed-life record and event-backed story exactly once.
4. Present the recounting and allow another life to begin.

These describe required results, not a mandated sequence of separate file writes. Use a transactional snapshot, recoverable journal, or another bounded approach that proves the results under interruption. A single process crash between terminal recording and archive completion must not resurrect the fish or create duplicate stories.

If story finalization is interrupted, recovery finishes or exposes the pending record using the same run identity. It does not rerun simulation or invent missing events. Presentation failure must not make a dead fish playable.

## Persistence boundaries

| Concern | Required behavior |
| --- | --- |
| Living save | Restores the same active run and relevant world state. |
| Terminal marker or equivalent | Prevents supported resume/recovery paths from loading a recorded dead run as alive. |
| Completed-life archive | Retains stories after the active run ends or a new one starts. |
| Duplicate death callback | Does not create a second death, nutrition award, or archive entry. |
| Repeated archive recovery | Converges on one completed-life entry for the run ID. |
| Unreadable or unsupported save | Reports a technical problem and preserves recoverable data; does not invent a death. |
| Missing active save | Allows the defined new-life flow without clearing history. |

Local save editing and adversarial anti-cheat are outside the starter's scope. The game must still uphold permanent death throughout its own normal and supported recovery flows.

## History and deletion

Ending, replacing, or deleting an active save is separate from deleting completed-life history. Each destructive action must name its scope. Starting a new life must preserve archived stories. A future reset-all option must clearly identify that it removes both current progress and history.

Discarding a living save is not a predation event. Whether abandoned lives receive a separate non-death record remains an open design question; do not silently classify them as deaths.

## Acceptance targets

- Save a living fish, close the game, resume, and compare identity, habitat/world and baked-world identity, growth, location, and relevant threat/resource state.
- Advance real-world time while closed and demonstrate no offline survival simulation.
- Die, reopen the game, and verify that **Continue** cannot revive that run.
- Interrupt each material death/archive persistence boundary and recover one terminal run with one history record.
- Start a new hatchling and retain the previous fish's story.
- Surface a corrupted or unsupported save without clearing the archive or fabricating a death.
- Distinguish observed survival behavior, recovery tests, and unresolved edge cases in the [roadmap](../roadmap.md).
