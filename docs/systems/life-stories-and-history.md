# Life stories and history

## Status and confirmed experience

**Planned; no recounting or history interface is implemented or qualified.** After a fish dies, the game briefly recounts its life. Every completed life remains available in a dedicated **Life History** section of the main menu, where the player can look back on all their stories.

The narrative is based on actual events from that run. The accepted presentation direction is a gentle nature-documentary tone, a short account of meaningful moments, and a persistent record. Fish naming, portraits, exact prose length, and supporting statistic layout remain proposed interface details.

## Record the event before telling the story

Use authoritative simulation events to identify what occurred. Candidate milestones include hatching, a first successful hunt, a verified close escape, entering a newly discovered area, meaningful growth, and the terminal encounter.

Each stored event needs enough context to support its eventual claim: run identity, stable event identity or deduplication key, simulation time, event type, and relevant fish, species, area, or cause identifiers. An event such as an escape needs a defined gameplay condition; merely losing sight of a predator does not establish a near-death escape.

Keep the event set bounded and useful. Semantic milestones and selected highlights are sufficient; a complete recording of every frame or input is not required. Any pruning policy must retain the facts used by an already finalized story.

## Selecting the recounting

The working approach is to select a few turning points and render them with local authored templates. This makes a small, inspectable first implementation possible. Online generation, external model services, and runtime LLM dependencies are not required by the concept.

Candidate ordering is the place of birth, an early survival or feeding milestone, one or two later highlights, and the actual death. Selection should reflect that fish's life rather than force every record into the same heroic arc.

| Recorded evidence | Permitted account |
| --- | --- |
| First successful prey consumption | The fish caught its first prey. |
| Defined pursuit ended through escape | The fish escaped that encounter. |
| First entry into a tracked area | The fish reached that area. |
| Consumption of a previously threatening species | The fish eventually fed on that species. |
| Stable identity proves the exact earlier predator was consumed | The story may refer to that individual. |
| Authoritative terminal event and known cause | The story identifies the supported cause of death. |

Do not invent emotion, intent, unseen locations, kill counts, escapes, or exact attacker identity. A phrase such as “its final hunt” requires a real pursuit or hunting event; an ordinary movement into an area does not support it. Use a general cause when the recorded evidence cannot justify a more specific one.

A short life can produce a short account of hatching and death. Missing optional milestones should never block finalization or cause filler achievements.

## Stable completed-life entry

A completed-life record should contain the run ID, final fish identity and size information, supported life statistics, terminal cause, selected event references, and the finalized narrative with its format/template version. Exact schema fields are a future implementation decision.

Use the run ID as the archive identity. Retrying a write or reopening after a crash must update or recover the same entry, not append a duplicate. Archive persistence is independent from the current living save. Starting another run must not overwrite the previous fish's story.

Save the finalized account so reopening it presents the same life. A later template change should not silently rewrite all older stories. Any future migration or explicit regeneration must preserve provenance and the underlying facts.

[Survival and runs](survival-and-runs.md) owns the transaction that makes death terminal and archive completion recoverable. A failed narrative render must not undo death or erase its underlying record.

## Main-menu history

The **Life History** entry is present even when there are no completed lives. Its empty state explains that stories will appear after a fish's life ends; it must not seed fictional example lives into the player's archive.

The proposed list shows a fish label, completion date, final size, and a short identifying detail. Selecting a row opens the complete stored recounting and supporting statistics. Return navigation should preserve the player's position in the list.

All stored stories remain reachable as the collection grows. Pagination, lazy loading, filtering, and sorting are implementation options; they must not silently discard older lives. Viewing, skipping, or revisiting a recounting has no effect on run status or archive count.

Deleting an individual story, clearing history, or resetting all data are distinct possible actions. None is required for the first archive slice. If introduced, label and confirm the exact deletion scope; an ordinary **New life** action never clears history.

## Presentation

After death, allow a brief readable pause, show the fish's identity or image where available, and present the short account. A skip or continue action advances to the next menu without changing what was saved. Narration, animated timelines, and recorded gameplay replays are optional future features.

Support ordinary text readability and reduced motion. The player must be able to revisit the full text without waiting through the original reveal animation. Exact pacing and length should be qualified with both short and eventful lives.

## Acceptance targets

- Generate distinct, truthful accounts from a short life, an eventful life, and a life with incomplete optional metadata.
- Trace each factual sentence to recorded events or final-state data.
- Persist one history entry per dead run through duplicate callbacks and interruption recovery.
- Skip the death presentation, start a new run, reopen the game, and find the same story in the main menu.
- Display an empty archive and a large archive without fictional entries or lost records.
- Keep a dead fish unavailable for continuation when viewing its story.
- Preserve older finalized accounts through an ordinary application update.

These are future qualification targets. [Validation](../validation.md) and [the roadmap](../roadmap.md) own evidence and completion status.
