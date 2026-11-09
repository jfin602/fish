# Decisions and open questions

**Baseline date:** 2026-11-09. This register records the owner's Fish discussion, repository initialization request, and subsequent accepted design decisions. It is a design record, not implementation evidence.

## Provenance

- **S1 — Initial concept:** the owner described a 3D game in which the player is a fish, begins as a hatchling, grows by eating food and smaller fish, and tries to avoid being eaten. Third person is primary; first person is optional and mainly for fun.
- **S2 — Life continuity:** the owner chose the same fish across multiple sessions while retaining roguelike permanent death: dying means restarting as a hatchling.
- **S3 — Accepted continuity and records:** the owner affirmed the described save/resume and death behavior, including a paused world while closed, and liked keeping a history of fish lives. The owner then requested a brief recounting of a fish's life after it dies.
- **S4 — History and repository:** the owner affirmed the factual-life-story direction, required a main-menu section for looking back on all stories, and requested a starter in `jfin602/fish` using the workflow and documentation style of `jfin602/space-factory`.
- **S5 — Development stack:** in the same request, the owner chose Godot and Godot MCP in the same manner as Space Factory and asked for a Codex prompt to create the Godot project after the documentation was set up. The port adopts the reference's Godot 4 .NET/C# direction; exact engine, SDK, and MCP installation details are owned by [development setup](development.md).

- **S6 — Core controls and Movement abilities (2026-11-09):** in [worksheet Q08](planning/p1/swim-controls-worksheet.md#q08--extra-movement-actions), the owner chose to keep the current controls and first make normal swimming, turning, and slowing feel right. The owner selected the short speed burst, precision mode, and quick evasive turn as the next implementation focus once that core is locked down, named **Movement abilities**.

- **S7 — Ocean art and asset selection (2026-11-09):** the owner requested free realistic fish models and surrounding scenery, chose "ocean creatures and environment" from the researched options, and then directed the assistant to lock the resulting shortlist into the repository docs. [Art and assets](art-and-assets.md) owns the nine approved sources, the four-asset first import set, and the preparation and licensing boundaries. This selects the art/sourcing direction; it does not claim acquisition, Godot import, or gameplay qualification.

- **S8 — Final core-controls review (2026-11-09):** the owner completed the [ten-question worksheet](planning/p1/swim-controls-worksheet.md), accepted all seven proposed core playtest criteria in Q10, and requested: “Yes, lock all this down in the docs with a final /docs-review.” This approves the documented design and criteria; actual implementation and owner play acceptance remain separate.

- **S9 — P1 delivery and fullscreen scope (2026-11-09):** the owner requested `/docs-apply + /prompt-ass + /prompt-plan + /prompt-write p1`, then directed: “Use the assets chosen in the docs as part of the p1 plan. By the end of p1 the swimming dynamics should be set. Also develop for full screen from the start. With a windowed full screen mode for easier testing”. The [P1 design](planning/p1/core-swimming.md) and [execution stack](tasks/p1/README.md) apply this scope without claiming execution.

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
| C13 | The repository bootstrap prepares documentation and a Codex Godot setup prompt; gameplay completion is not claimed by the starter. | S5 |
| C14 | Lock down core swimming, turning, and slowing before adding movement abilities. | S6 |
| C15 | The next implementation focus after core lock is [Movement abilities](planning/p1/movement-abilities.md): short speed burst, precision mode, and quick evasive turn. | S6 |
| C16 | Use realistic ocean creatures and environments as the initial habitat and visual direction, starting from free assets suitable for eventual commercial game use. | S7 |
| C17 | Use the approved [ocean asset shortlist and starter order](art-and-assets.md): Emperor Angelfish, Rock Face 01, Coast Sand 01, and Soft Coral Set form the first art/import set; inspect Sea life next, with the other entries reserved for later expansion. | S7 |
| C18 | Qualify mouse-only controls and a keyboard fallback first; controller support follows later, with eventual support for both desktop and controller. | S8; worksheet Q02 |
| C19 | In mouse-only mode, mouse movement steers, vertical movement pitches, and a held mouse button supplies speed/thrust input. | S8; Q02, Q06 |
| C20 | Keyboard left/right inputs turn through curved paths; backward input slows the fish. Exact bindings remain open. | S8; Q05 |
| C21 | Both desktop modes change depth by pitching the fish up/down and swimming in that direction. | S8; Q06 |
| C22 | Swimming feels natural, nimble, agile and easy, with slight turning anticipation, quick acceleration and more gradual slowdown. | S8; Q01, Q04 |
| C23 | The camera follows behind/slightly above at moderate distance, shows the whole fish and route ahead, follows smoothly, and adjusts near obstacles for visibility. | S8; Q07 |
| C24 | Visible tail/body motion eases during gliding; the fish bends/leans into turns while the camera follows with a steady horizon. | S8; Q09 |
| C25 | Free look is deferred for the initial core-controls scope. | S8; Q03 |
| C26 | The seven accepted playtest criteria must be observed in both desktop modes, including the owner's judgment of enjoyable, natural and easy swimming, before the implemented core is locked for progression. | S8; Q10 |
| C27 | P1 integrates and visibly uses the four selected starter assets: Emperor Angelfish, Rock Face 01, Coast Sand 01 and Soft Coral Set. | S9; S7 starter order |
| C28 | P1 finishes with swimming dynamics tuned, documented and accepted through actual owner play in both desktop modes; unfinished core feel is not deferred beyond the phase. | S9; S8 criteria |
| C29 | Develop the game for fullscreen from the start, with windowed fullscreen available for easier testing. | S9 |

## Working designs

| ID | Proposed approach | Owner |
| --- | --- | --- |
| W01 | One compact habitat with food, cover, one prey type, and one predator type proves the initial loop. | [Game design](game-design.md) |
| W02 | Growth changes usable cover, edible prey, and hunting capability within the same fish identity. | [Ecology and growth](systems/ecology-and-growth.md) |
| W03 | Superseded by S6 and S8: the original swimming/bite/burst bundle is replaced by the confirmed core-controls contract, followed by Movement abilities after actual core acceptance; bite belongs to later feeding work. | [Swimming and camera](systems/swimming-and-camera.md) |
| W04 | Predators approach and commit to readable attacks; the player can escape through movement and shelter. | [Ecology and growth](systems/ecology-and-growth.md) |
| W05 | Small food uses contact collection while live prey requires an aimed feeding action. | [Ecology and growth](systems/ecology-and-growth.md) |
| W06 | A local authored-template system selects a few actual milestones for the short recounting. | [Life stories and history](systems/life-stories-and-history.md) |
| W07 | A final-size fish image, optional name, short story, and supporting statistics form the death/history entry. | [UX](ux-and-presentation.md) |
| W08 | Early growth milestones arrive quickly; larger stages can last longer across sessions. | [Opening experience](tutorial.md) |
| W09 | Discovery records and cosmetic rewards could provide carryover without altering starting survival stats. | [Feature ideas](../feature-ideas.md) |
| W10 | The [P1 plan](planning/p1/core-swimming.md) provides reversible initial bindings, a shared speed-demand motor, display mappings and focus policy; final core settings are settled through P1 playtesting. | [Core swimming plan](planning/p1/core-swimming.md) |

Engineering invariants in the system documents preserve confirmed behavior: stable run identity, idempotent feeding/death/archive operations, no offline simulation, factual story provenance, and separation of active saves from history. Their exact code and storage format remain unimplemented.

## Open decisions

Resolve choices when they affect the next concrete task. A reversible prototype may adopt a documented scoped assumption without settling the entire game.

| ID | Decision | When it matters |
| --- | --- | --- |
| Q01 | Final player species and specific ocean biome; the marine setting and realistic asset direction are confirmed by S7. | Before committing to production content and ecological composition. |
| Q02 | Only the [remaining tuning and bindings](systems/swimming-and-camera.md#remaining-tuning-and-bindings), later controller details, and Movement abilities specifics remain open. Qualitative core controls and playtest criteria are confirmed in C18–C26. | Resolve and record core bindings/tuning before P1 closes; controller and ability details follow their later scope. |
| Q03 | Growth model, edible-size rule, body/collision scaling, maximum size, and balancing rates. | Before qualifying the first feeding-to-growth loop. |
| Q04 | Hunger, diet restrictions, injury recovery, attack damage, and additional death causes. | Before broad survival mechanics are implemented. |
| Q05 | Intended life/session lengths, full-size payoff, aging, and any victory or ending. | Before long-term pacing and content expansion. |
| Q06 | Authored versus procedural habitats, seeds, starting variation, and population replenishment. | Before expanding the first small habitat. |
| Q07 | One or multiple living save slots, manual/automatic checkpoint policy, abandonment records, and supported recovery guarantees. | Before save/menu qualification. |
| Q08 | Narrative length, fish naming, portraits, narration, filtering, sorting, and optional archive deletion. | Before the first polished history flow. |
| Q09 | Cross-run discovery persistence, cosmetics, unlocks, or permanent stat progression. | Before implementing any carryover beyond saved stories. |
| Q10 | Detailed visual treatment, water effects, audio direction, performance budget, renderer choice, and platform qualification within the confirmed realistic ocean direction. | Bootstrap records its scoped choice; production decisions follow measured needs. |

## Design approval and actual core acceptance

S8 closes the design discussion. It does not claim that a local demo has passed: [validation V01–V02](validation.md#core-movement-acceptance) requires native observations and owner acceptance of the implemented feel. The roadmap's implementation and Gate results remain the evidence authority.

## Keeping the register current

Record consequential decisions with date, source or approving task, scope, rationale, and links to affected owners. Move proposals into accepted status only when a real decision occurs. Record implementation and observed results separately in [the roadmap](roadmap.md) and task evidence.

Use [decision records](decisions/README.md) when a durable engineering choice needs more explanation. Preserve prior reasoning when a decision is superseded.
