# Decisions and open questions

**Baseline date:** 2026-11-09. This register records the owner's Fish discussion and the repository initialization request. It is a design record, not implementation evidence.

## Provenance

- **S1 — Initial concept:** the owner described a 3D game in which the player is a fish, begins as a hatchling, grows by eating food and smaller fish, and tries to avoid being eaten. Third person is primary; first person is optional and mainly for fun.
- **S2 — Life continuity:** the owner chose the same fish across multiple sessions while retaining roguelike permanent death: dying means restarting as a hatchling.
- **S3 — Accepted continuity and records:** the owner affirmed the described save/resume and death behavior, including a paused world while closed, and liked keeping a history of fish lives. The owner then requested a brief recounting of a fish's life after it dies.
- **S4 — History and repository:** the owner affirmed the factual-life-story direction, required a main-menu section for looking back on all stories, and requested a starter in `jfin602/fish` using the workflow and documentation style of `jfin602/space-factory`.
- **S5 — Development stack:** in the same request, the owner chose Godot and Godot MCP in the same manner as Space Factory and asked for a Codex prompt to create the Godot project after the documentation was set up. The port adopts the reference's Godot 4 .NET/C# direction; exact engine, SDK, and MCP installation details are owned by [development setup](development.md).

Assistant examples, names, locations, quantities, and proposed features remain working designs unless specifically adopted. Porting the reference workflow does not import its 2D game mechanics, existing qualification results, or completed Phases.

## Confirmed requirements

| ID | Requirement | Source |
| --- | --- | --- |
| C01 | The game is 3D and the player directly controls a fish. | S1 |
| C02 | The fish begins as a hatchling and can grow into a large fish if it survives. | S1 |
| C03 | Feeding on food and suitable smaller fish, while avoiding being eaten, is the core loop. | S1 |
| C04 | Third person is the primary view; optional first person observes the same game. | S1 |
| C05 | One fish's life can continue across multiple play sessions. | S2 |
| C06 | Death permanently ends that life; the next run starts as a hatchling. | S2 |
| C07 | Leaving preserves the living fish and world; the closed game does not advance survival simulation. | S3 |
| C08 | Death is followed by a brief recounting of that fish's life. | S3 |
| C09 | Life stories use events that actually happened in the run. | S4 |
| C10 | A main-menu history section retains and exposes all completed-life stories. | S4 |
| C11 | Project documentation and the starter workflow belong in `jfin602/fish`, following Space Factory's conventions. | S4 |
| C12 | Use Godot and Godot Editor MCP, adopting the reference's Godot .NET/C# development direction. | S5 |
| C13 | The current work prepares the repository/docs and a Codex Godot setup prompt; gameplay completion is not claimed by the starter. | S5 |

## Working designs

| ID | Proposed approach | Owner |
| --- | --- | --- |
| W01 | One compact habitat with food, cover, one prey type, and one predator type proves the initial loop. | [Game design](game-design.md) |
| W02 | Growth changes usable cover, edible prey, and hunting capability within the same fish identity. | [Ecology and growth](systems/ecology-and-growth.md) |
| W03 | A camera behind and above the fish, direct swimming, a bite, and a short speed burst form the first control prototype. | [Swimming and camera](systems/swimming-and-camera.md) |
| W04 | Predators approach and commit to readable attacks; the player can escape through movement and shelter. | [Ecology and growth](systems/ecology-and-growth.md) |
| W05 | Small food uses contact collection while live prey requires an aimed feeding action. | [Ecology and growth](systems/ecology-and-growth.md) |
| W06 | A local authored-template system selects a few actual milestones for the short recounting. | [Life stories and history](systems/life-stories-and-history.md) |
| W07 | A final-size fish image, optional name, short story, and supporting statistics form the death/history entry. | [UX](ux-and-presentation.md) |
| W08 | Early growth milestones arrive quickly; larger stages can last longer across sessions. | [Opening experience](tutorial.md) |
| W09 | Discovery records and cosmetic rewards could provide carryover without altering starting survival stats. | [Feature ideas](../feature-ideas.md) |

Engineering invariants in the system documents preserve confirmed behavior: stable run identity, idempotent feeding/death/archive operations, no offline simulation, factual story provenance, and separation of active saves from history. Their exact code and storage format remain unimplemented.

## Open decisions

Resolve choices when they affect the next concrete task. A reversible prototype may adopt a documented scoped assumption without settling the entire game.

| ID | Decision | When it matters |
| --- | --- | --- |
| Q01 | Player species, freshwater or marine habitat, and visual identity. | Before committing to production content and art. |
| Q02 | Exact controls, steering, speed burst, input devices, and camera tuning. | During the first playable 3D movement work. |
| Q03 | Growth model, edible-size rule, body/collision scaling, maximum size, and balancing rates. | Before qualifying the first feeding-to-growth loop. |
| Q04 | Hunger, diet restrictions, injury recovery, attack damage, and additional death causes. | Before broad survival mechanics are implemented. |
| Q05 | Intended life/session lengths, full-size payoff, aging, and any victory or ending. | Before long-term pacing and content expansion. |
| Q06 | Authored versus procedural habitats, seeds, starting variation, and population replenishment. | Before expanding the first small habitat. |
| Q07 | One or multiple living save slots, manual/automatic checkpoint policy, abandonment records, and supported recovery guarantees. | Before save/menu qualification. |
| Q08 | Narrative length, fish naming, portraits, narration, filtering, sorting, and optional archive deletion. | Before the first polished history flow. |
| Q09 | Cross-run discovery persistence, cosmetics, unlocks, or permanent stat progression. | Before implementing any carryover beyond saved stories. |
| Q10 | Final art/audio style, performance budget, renderer choice, and platform qualification. | Bootstrap records its scoped choice; production decisions follow measured needs. |

## Keeping the register current

Record consequential decisions with date, source or approving task, scope, rationale, and links to affected owners. Move proposals into accepted status only when a real decision occurs. Record implementation and observed results separately in [the roadmap](roadmap.md) and task evidence.

Use [decision records](decisions/README.md) when a durable engineering choice needs more explanation. Preserve prior reasoning when a decision is superseded.
