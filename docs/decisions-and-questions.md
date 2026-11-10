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

- **S10 — P2 underwater world (2026-11-09):** with core controls nearing completion, the owner selected worldbuilding as Phase 2. The current flat/two-ground/test-shape presentation should be replaced by a natural coral/reef floor, rock tunnels and a generally pleasant environment. The whole screen/world must read as underwater, and the fish should create fluid-looking water disturbance that highlights its movement. The accepted documentation review places a measured Compatibility-versus-Forward+ renderer comparison at P2 entry, keeps the P1 movement course as a development fixture rather than normal presentation, and moves feeding/predators/growth to P3.
- **S11 — Forward+ selected for P2 (2026-11-09):** after P2/P1 created the reversible renderer comparison, the owner ran the Forward+ path on the real desktop. It ran smoothly and looked perhaps slightly better; the owner chose Forward+ because its additional rendering capabilities are worthwhile for the underwater-world work. This resolves the P2 renderer choice. Compatibility remains a diagnostic comparison path unless a concrete Forward+ blocker requires a new owner decision.
- **S12 — Production visual style and FAF (2026-11-10):** the owner selected bright stylized low-poly reef naturalism and requested a repeatable original-art factory. This supersedes S7's realistic rendering target while retaining its historical asset sourcing/provenance record and recognizable marine anatomy. [FAF art style](fish-asset-factory/art-style.md) owns the visual contract; [FAF](fish-asset-factory/README.md) owns future original-asset standards and review.
- **S13 — Depth-structured world and water column (2026-11-10):** after completing the water/sunlight correction, the owner identified missing depth-of-vision as the next P2 presentation gap. The normal basin should expose at least five natural traversable ledges at different elevations so the player can swim between clearly different water depths. Actual depth below the real water surface must progressively reduce available light, warm color, caustics/shafts and useful visual range while distance extinction makes far geometry trail into the water. [P2 underwater-world plan](planning/p2/underwater-world.md) owns the detailed contract; [c2-depth](tasks/c2-depth/README.md) is the bounded implementation stack.
- **S14 — P3 FAF reef rebuild (2026-11-10):** after completing the core FAF terrain, rock, coral, plant, detail and reef-fish asset families, the owner selected Phase 3 as a rebuild of the current varying-depth world using those original assets. The world should be filled out with natural caves, tunnels, arches and reef detail; higher/shallower areas should carry more vegetation and living density, while lower/deeper areas become progressively sparser and more geological. [P3 reef-world plan](planning/p3/reef-world-rebuild.md) owns the phase design; feeding/predators/growth shift to P4.
- **S15 — P3 Character Select (2026-11-10):** the owner added a Character Select page to P3 using **all five FAF fish models**, explicitly including the Blacktip Reef Shark. Fresh session entry should select a fish before play, and the in-game Pause/Game Menu must provide a way to return to Character Select. For P3 this is a visual/player-body choice only: every fish shares the current movement, collision, camera and gameplay tuning; returning to selection is session routing, not death, save/resume, abandonment or Life History.

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
| C15 | [Movement abilities](planning/p1/movement-abilities.md)—short speed burst, precision mode, and quick evasive turn—remain an approved future feature group. S10 supersedes only their immediate scheduling: P2 worldbuilding is next after P1. | S6; S10 |
| C16 | Historical initial direction: use realistic ocean creatures and environments, starting from free assets suitable for eventual commercial game use. The rendering target is superseded by C37; source/provenance requirements remain. | S7; S12 |
| C17 | Original [ocean starter order](art-and-assets.md) used Emperor Angelfish, Rock Face 01, Coast Sand 01 and Soft Coral Set. This named set is superseded by C30 after actual acquisition checks. | S7 |
| C18 | Qualify mouse-only controls and a keyboard fallback first; controller support follows later, with eventual support for both desktop and controller. | S8; worksheet Q02 |
| C19 | In mouse-only mode, mouse movement steers, vertical movement pitches, and a held mouse button supplies speed/thrust input. | S8; Q02, Q06 |
| C20 | Keyboard left/right inputs turn through curved paths; backward input slows the fish. Exact bindings remain open. | S8; Q05 |
| C21 | Both desktop modes change depth by pitching the fish up/down and swimming in that direction. | S8; Q06 |
| C22 | Swimming feels natural, nimble, agile and easy, with slight turning anticipation, quick acceleration and more gradual slowdown. | S8; Q01, Q04 |
| C23 | The camera follows behind/slightly above at moderate distance, shows the whole fish and route ahead, follows smoothly, and adjusts near obstacles for visibility. | S8; Q07 |
| C24 | Visible tail/body motion eases during gliding; the fish bends/leans into turns while the camera follows with a steady horizon. | S8; Q09 |
| C25 | Free look is deferred for the initial core-controls scope. | S8; Q03 |
| C26 | The seven accepted playtest criteria must be observed in both desktop modes, including the owner's judgment of enjoyable, natural and easy swimming, before the implemented core is locked for progression. | S8; Q10 |
| C27 | P1 integrates and visibly uses four verified starter assets. The original named set is superseded by C30; G1.3 still requires four actual assets in native play. | S9; C30 |
| C28 | P1 finishes with swimming dynamics tuned, documented and accepted through actual owner play in both desktop modes; unfinished core feel is not deferred beyond the phase. | S9; S8 criteria |
| C29 | Develop the game for fullscreen from the start, with windowed fullscreen available for easier testing. | S9 |
| C30 | The first P1 set is a **still unselected** realistic rigged ocean fish with a verified swim cycle, Rock Face 01, Coast Sand 01, and Smithsonian *Heliopora coerulea*. Emperor Angelfish is removed because official acquisition needs Unity/account-backed tooling; Soft Coral Set is replaced because its official anonymous download is unavailable. Sea life and the downloaded Great White Shark are evaluation/later content, and Rigged Archer Fish is excluded from the ocean starter. Final player species remains open. | Owner's resumed P2 direction, 2026-11-09; [P2 manifest](assets/p1-starter-manifest.md) |
| C31 | For the P1 test scene, use the owner-provided [Quaternius Animated Fish Pack](assets/p1-starter-manifest.md#quaternius-pack-inspection). Fish3 (`ClownFish`) is the provisional small player fish; other pack fish may populate the world without implementing ecology or predator behavior. At the time this was a low-poly prototype exception; C37 later made intentional low-poly the production style. C31 fills C30's unselected fish slot; it does not decide the final player species. | Owner request and pack inspection, 2026-11-09; S12 |
| C32 | After P1 core swimming closes, the next roadmap Phase is **P2 — Build the underwater world**. S14 later inserts the FAF reef-art rebuild as P3, moving feeding/predators/growth to P4 and shifting later phases again. | S10; S14 |
| C33 | P2 normal play replaces the flat/test-course look with a natural reef-like seabed, coral areas, rock formations with a tunnel/tight passage, open pockets, cover/overhang and meaningful vertical routes. Obvious developer shapes and exposed box-room boundaries do not remain the finished presentation. | S10 |
| C34 | P2 must make the scene read unmistakably as underwater using layered atmosphere rather than only a flat tint: depth/distance treatment, underwater lighting/color, moving caustic-style light, suspended matter and restrained whole-view water treatment are in scope. | S10 |
| C35 | P2 adds movement-reactive water presentation around the fish. Ordinary swimming, acceleration/high effort, gliding and hard turns must produce visibly different disturbance without changing authoritative movement physics. | S10 |
| C36 | P2/P1 provides a reversible Compatibility-versus-Forward+ comparison. After a real desktop Forward+ trial, the owner selected **Forward+** for P2; P2/P2 locks that choice in the tracked project. Compatibility is diagnostic only unless a concrete Forward+ blocker causes an explicit owner reversal. | S10; S11 |
| C37 | Original Fish production art follows **bright stylized low-poly reef naturalism**: identifiable marine life and natural proportions, intentional facets, colorful layered reef composition, simple materials and underwater light/fog finish. Low-poly is the chosen style, not merely a temporary prototype exception. The [FAF art-style contract](fish-asset-factory/art-style.md) owns detailed acceptance. | S12 |
| C38 | P2 underwater presentation uses actual depth below the real water surface plus separate view-distance extinction. Increasing depth progressively reduces illumination, warm color, caustic/shaft presence and useful visual range; far geometry transitions from detail to silhouette to water rather than hitting an obvious fog wall. | S13 |
| C39 | The default P2 normal-play basin exposes **at least five natural traversable reef ledges/shelves at distinct elevations**, connected by slopes, drop-offs, channels and alternate ascent/descent routes rather than an artificial staircase. Multiple depth bands should be visible together where composition allows. | S13 |
| C40 | The accepted P1 follow view rejects the side-offset camera and stays centered slightly above with modest turn lag and zoom. Mouse Only and keyboard include the limited camera-offset/pan behavior documented in [swimming and camera](systems/swimming-and-camera.md); later Hybrid 2 and Adaptive Follow remain separately implemented presets pending native feel judgment rather than silently redefining the accepted P1 baseline. | Direct owner P10 feedback and later control corrections, 2026-11-09 to 2026-11-10 |
| C41 | **P3 rebuilds the accepted varying-depth P2 world with the completed FAF asset library** rather than introducing ecology. Feeding/predators/growth move to P4; persistence to P5; life-history presentation to P6; integrated qualification to P7. | S14 |
| C42 | P3 preserves the accepted five-band P2 navigation/water-column structure but replaces the visible test/terrace presentation with FAF terrain, rock geology, caves/tunnels, coral, vegetation and reef-floor detail while keeping collision/camera routes honest. | S14 |
| C43 | P3 uses an intentional depth-density gradient: highest/shallow areas are the lushest and most colorful; middle depths balance living reef and geology; lower/deep areas become progressively sparser, darker and more rock-dominant while retaining readable routes. | S14 |
| C44 | P3 normal play must coherently integrate the completed core FAF environment library: six terrain variants, four rock variants, twelve coral variants, eight plant variants and eight detail variants. Asset coverage must participate in real composition rather than hidden checklist placement. | S14 |
| C45 | P3 Character Select exposes **FAF-FISH-001 Yellow Tang, FAF-FISH-002 Ocellaris Clownfish, FAF-FISH-003 Blue-Green Chromis, FAF-FISH-004 Reef Butterflyfish, and FAF-FISH-005 Blacktip Reef Shark** as selectable player visuals with animated 3D previews. | S15 |
| C46 | P3 character selection is presentation-only: every choice uses the same authoritative FishController, collision footprint, SwimMotor, controls, camera, tuning and water-reaction contract. Per-character visual scale/orientation may be normalized, including the shark; P3 does not add species stats or predator behavior. | S15 |
| C47 | Fresh P3 session entry opens Character Select before normal play. Selecting a fish starts the reef session with that visual; selection is session-local and does not establish saved-life/species persistence. | S15 |
| C48 | Pause/Game Menu includes **Return to Character Select**, which clears gameplay input/capture and returns to selection without recording death, saving/abandoning a life, creating history, or otherwise invoking later persistence semantics. | S15 |

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
| W11 | P2 uses one compact authored/modular reef habitat before committing to production terrain/procedural generation. The P1 course remains a separate qualification fixture. | [P2 underwater-world plan](planning/p2/underwater-world.md) |
| W12 | Fish-reactive water is a rendering/particle/shader presentation effect driven read-only from movement state, not a fluid simulation or alternate movement authority. | [P2 underwater-world plan](planning/p2/underwater-world.md) |
| W13 | P3 uses FAF-FISH-001–004 as bounded presentation-only ambient reef life when practical. All five FAF fish, including FAF-FISH-005 Blacktip Reef Shark, are selectable player visuals; shark selection remains visual-only and does not imply predator behavior. | [P3 reef-world plan](planning/p3/reef-world-rebuild.md) |

Engineering invariants in the system documents preserve confirmed behavior: stable run identity, idempotent feeding/death/archive operations, no offline simulation, factual story provenance, and separation of active saves from history. Their exact code and storage format remain unimplemented.

## Open decisions

Resolve choices when they affect the next concrete task. A reversible prototype may adopt a documented scoped assumption without settling the entire game.

| ID | Decision | When it matters |
| --- | --- | --- |
| Q01 | Final player species and specific ocean biome remain open. The marine setting and C37 visual language are confirmed. | Before committing to production content and ecological composition. |
| Q02 | Only the [remaining tuning and bindings](systems/swimming-and-camera.md#remaining-tuning-and-bindings), later controller details, and Movement abilities specifics remain open. Qualitative core controls and playtest criteria are confirmed in C18–C26. | Resolve and record core bindings/tuning before P1 closes; controller and ability details follow their later scope. |
| Q03 | Growth model, edible-size rule, body/collision scaling, maximum size, and balancing rates. | Before qualifying the first feeding-to-growth loop. |
| Q04 | Hunger, diet restrictions, injury recovery, attack damage, and additional death causes. | Before broad survival mechanics are implemented. |
| Q05 | Intended life/session lengths, full-size payoff, aging, and any victory or ending. | Before long-term pacing and content expansion. |
| Q06 | P2 uses a compact authored/modular habitat for the current slice. Procedural generation, seeds, larger-world variation and population replenishment remain open for later expansion. | Before expanding beyond the first qualified habitat. |
| Q07 | One or multiple living save slots, manual/automatic checkpoint policy, abandonment records, and supported recovery guarantees. | Before save/menu qualification. |
| Q08 | Narrative length, fish naming, portraits, narration, filtering, sorting, and optional archive deletion. | Before the first polished history flow. |
| Q09 | Cross-run discovery persistence, cosmetics, unlocks, or permanent stat progression. | Before implementing any carryover beyond saved stories. |
| Q10 | **Resolved for P2:** use Forward+ for the underwater-world phase, based on the owner's real desktop trial after P2/P1. Audio direction, production performance budgets, final biome, exact asset treatment and wider platform qualification remain open; C37 settles the broad visual style. | Revisit the renderer only if a concrete Forward+ blocker emerges or later platform work requires another decision. |

## Design approval and actual core acceptance

S8 closes the P1 core-control design discussion. It does not claim that a local demo has passed: [validation V01–V02](validation.md#core-movement-acceptance) requires native observations and owner acceptance of the implemented feel. S10 subsequently changes the roadmap sequence after that core lock: P2 worldbuilding comes next, while Movement abilities remain accepted future work. The roadmap's implementation and Gate results remain the evidence authority.

## Keeping the register current

Record consequential decisions with date, source or approving task, scope, rationale, and links to affected owners. Move proposals into accepted status only when a real decision occurs. Record implementation and observed results separately in [the roadmap](roadmap.md) and task evidence.

Use [decision records](decisions/README.md) when a durable engineering choice needs more explanation. Preserve prior reasoning when a decision is superseded.
