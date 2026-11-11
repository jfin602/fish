# User experience and presentation

## Status

This document separates confirmed interface requirements from working presentation ideas. P1's small pause/options and display/input interface is implemented under its accepted baseline; the main menu, survival UI and archive are later work. P2's world presentation is implemented but still awaits owner qualification in [P8 review](tasks/p2/closeout.md). [The decision register](decisions-and-questions.md) owns requirement status; [the roadmap](roadmap.md) owns progress.

## Confirmed perspective and continuity

Third person is the main experience in a stylized 3D aquatic world: the existing Ocean and a planned freshwater Pond are distinct habitats. The player sees the same fish grow from hatchling to large fish. Optional first person offers another view of that life. Camera changes must not reset survival state or encounters.

A living fish can be continued across sessions. Death ends that life permanently. The main menu contains a history section for all completed fish stories, and death presents a brief recounting of the life that just ended.

## P3 Character Select

**Owner-selected for P3 — 2026-11-10:** before normal P3 play, show a dedicated Character Select page using the five existing FAF fish candidates: Yellow Tang, Ocellaris Clownfish, Blue-Green Chromis, Reef Butterflyfish and Blacktip Reef Shark.

The page should show a real animated 3D preview of the highlighted fish, its readable name, clear selection/focus state, and a Start/Play action. Mouse and keyboard users must be able to browse all five choices. The preview is presentation only and must not mutate gameplay state.

For P3, fish choice changes **visual body/animation only**. Every choice uses the same movement motor, collision footprint, camera, controls, tuning and water-reaction behavior. Per-character presentation scaling/orientation may normalize the models into one shared player envelope; selecting the Blacktip Reef Shark does not grant larger collision, speed, predator behavior or species stats.

Character choice is session-local. It is not yet a saved fish identity, New Life policy, growth/species system or persistence feature. Fresh P3 entry goes to Character Select; Start enters the reef with the selected visual.

The Pause/Game Menu includes **Return to Character Select**. That action clears gameplay input and mouse capture, exits the current ephemeral P3 play scene and returns to the selection page. It must not be represented as death, archive a story, save/abandon a living run or invoke later persistence rules. After return, the last selected fish may remain highlighted in memory, and the player can choose another fish and start a clean session.

## P4 World Select and habitat-filtered characters

**Owner-selected for P4 — 2026-11-11; P2 menu routing implemented.** Preserve P3's five-fish marine Character Select for Ocean; World Select now opens before Character Select for a fresh demo session. The screen offers **Ocean** and **Freshwater Pond** with readable previews, selection/focus, Back and Start/Choose behavior; each world loads its own environment and compatible character list. Marine fish cannot be selected in Pond and freshwater fish cannot be selected in Ocean. The same restriction also governs ambient/spawned populations.

At P2, Ocean is the only playable route. Pond remains selected when its absent bake produces a visible unavailable error; no character or play scene opens. Habitat-filtered species and ambient eligibility are P3 work, and the Pond bake is P4 work. The demo's World Select Back action quits the demo; there is no implemented Main Menu yet.

Proposed New Life path: **Main Menu → World Select → Character Select → Play**. In the current tech demo World Select may be the first gameplay selection screen. Character Select → Back returns to World Select; session-only Pause → Return to selection must clear held input and mouse capture. Neither navigation nor switching demo worlds is a death, save, archived life or gameplay world migration. A failed load stays visible as an error, not an implicit fallback to the other world.

When saved lives are implemented in P6, **Continue** goes directly to the fish's stored world and species; World Select is only for starting a new life. Main-menu Life History remains global across habitats. [P4 plan](planning/p4/freshwater-world.md) owns the full proposed UI and native acceptance.

## Main menu

| Action | Intended behavior |
| --- | --- |
| Continue | Available for a valid living save; resumes that fish and its current situation. |
| New life | Opens World Select, then compatible Character Select, to start a new hatchling under the explicit active-save replacement policy. |
| Life History | Opens the saved collection of completed-life stories, including a truthful empty state. |
| Settings | Provides the controls, display, audio, and accessibility options implemented for the current scope. |
| Quit | Leaves without advancing the world while closed. |

These labels are working UI wording. Continue, New life, and Life History must represent actual state. A technical save error should be explained and preserve data; it must not be presented as a dead fish or an empty history.

## During play

Prioritize the fish, nearby food, useful shelter, and threats. Size differences should be visually legible. If health, stamina, hunger, or growth meters are implemented, derive them from authoritative state and show information that helps the next decision.

The approved [swimming presentation](systems/swimming-and-camera.md) places the camera behind and slightly above the fish at a moderate distance, showing its whole body and useful space ahead. It follows direction and depth changes smoothly, adjusts near obstacles to keep the fish visible, and keeps a steady horizon while the body bends and leans into turns. Visible tail and body motion eases during a glide.

Plants, rocks, depth changes, and lighting provide spatial cues. Ocean retains its vibrant low-poly reef contract; Pond adds murky-green freshwater lighting, mud, logs and vegetation. [FAF style](fish-asset-factory/art-style.md) owns shared/habitat look, and [art and assets](art-and-assets.md) owns sources and preparation checks. P2 remains the historical first underwater-world treatment.

Predator approach and attack feedback should offer an understandable chance to react. Avoid relying only on color to distinguish edible targets, danger, or an unavailable action. An implemented burst, bite, or escape should have feedback that matches its actual outcome.

## Underwater world presentation

**Owner-selected for P2 — 2026-11-09:** normal play should stop looking like a flat test course and instead read immediately as a natural underwater environment. Use a layered presentation: varied seabed/reef geometry, depth/distance haze, underwater color/contrast, light from above, moving caustic-style illumination, sparse suspended matter and a restrained whole-view water treatment. A uniform blue overlay by itself is insufficient.

The player should be able to read open routes, rock tunnels/arches, overhangs, coral floor and upper/lower landmarks while the camera remains comfortable. Avoid distortion/noise strong enough to cause motion discomfort or hide collision boundaries. World limits should be visually natural rather than exposed box walls; the fish must not be able to swim below the seabed.

**P3 visibility/discovery requirement (owner-selected 2026-11-10; implementation pending):** the player must not see recognizable geometry at the opposite playable map boundary from an ordinary underwater viewpoint. As the fish swims, nearby terrain and colorful reef detail remain clear, intermediate features soften, and distant landmarks appear first as silhouettes before gradually becoming recognizable. This is a sense-of-place and exploration requirement, not permission to hide collision hazards, obscure the player fish or replace approved sunlight/water effects with a dark overlay. Shallow/middle/deep visibility targets and GFX tuning/migration are defined in the [underwater visibility contract](systems/underwater-visibility.md).

Water should visibly react to movement. Ordinary swimming has a subtle wake/disturbance; stronger acceleration produces more activity; gliding reduces it; hard turns create a directional change. These are presentation effects driven read-only from actual movement state. They do not add force, change speed/collision or become simulation authority.

Pause/settings/HUD are interface elements, not submerged geometry. Keep their text and focus state crisp and usable even if the 3D view receives screen-space distortion or color treatment. [P2 validation V18–V20](validation.md) owns native acceptance.

The current Forward+ candidate renders distance/height/volumetric fog, a moving overhead surface, top light, drifting broken caustics, caustic-aligned ribbon shafts, suspended motes and a restrained screen pass below the pause UI. It adds a tail-space ribbon/bubble response for movement. Pause settings include a GFX panel for live water/light sliders and locally saved lighting presets. [Correction 2 evidence](tasks/c2-watet-light-fx/qualification/native-water-light.md) records the revised native candidate and remaining owner/performance judgment; [P8 review](tasks/p2/closeout.md) retains the broader world acceptance boundary.

## Fullscreen presentation

**Owner-selected for P1 — 2026-11-09:** develop for fullscreen from the start, with windowed fullscreen available for easier testing. A fresh settings profile starts fullscreen; windowed fullscreen fills the desktop display without window decorations and supports the testing workflow. The [P1 plan](planning/p1/core-swimming.md#fullscreen-from-the-first-task) owns provisional engine mappings and launch behavior; actual platform support must be observed.

Design camera framing, readable control help and the small pause/options interface for actual display dimensions. Use anchored/scaled layout, pointer operation and complete keyboard focus/navigation. Retain the local display preference and keep a temporary development override separate from saved choice. These preferences do not implement living-save persistence.

Pause and clear input on focus loss; resume deliberately with fresh input after recapture. Display/mode changes must not leak menu clicks into propulsion or change authoritative fish state. Native fullscreen and windowed-fullscreen observation is required in both desktop input modes under [V17](validation.md); an embedded editor preview does not establish that result. The final accepted swimming profile includes the display/camera behavior tested in P1.

## Pause, saving, and recovery

Provide a clear way to pause and leave a session. During P3, **Return to Character Select** is an explicit ephemeral-session routing action and is separate from future save/death/abandonment behavior. Saving should communicate whether the operation succeeded. If it fails, retain the current life and explain the problem; do not falsely claim the player can safely resume a state that was not saved.

The game is paused while closed. Resume should restore the relevant danger state without surprise input carried from the menu. See [survival and runs](systems/survival-and-runs.md) for persistence rules.

## Death and recounting

The proposed sequence is a brief death transition, the fish's final identity/appearance, a short life account, and supporting statistics. Keep the cause factual and understandable. A skip action advances the presentation while retaining the same archived entry.

The archive should remain reachable whether the player starts another life immediately or returns later. Viewing a dead fish's story is a read-only reflection on that life, not a resume action.

[Life stories and history](systems/life-stories-and-history.md) owns event selection, factuality, retention, and idempotent archive behavior. A portrait, player-given name, narration, or animated timeline is optional until adopted for a task.

## Acceptance targets

- A first-time player can identify food, shelter, and an approaching threat in the native 3D view.
- Growth is visible and the camera remains usable at the tested sizes.
- Menu actions accurately reflect living, dead, missing, and unreadable-save states.
- The recounting can be skipped and later read in full from **Life History**.
- A player can browse all retained stories and return to the menu without changing the active run.
- Text, input focus, and any animations remain readable in the tested display conditions.
- In the P2 habitat, normal swimming clearly reads as underwater at near/far/deep viewpoints without relying only on a flat tint.
- The P2 environment provides natural seabed/reef routes, rock passage/cover and vertical navigation without visible developer-course presentation.
- Movement-reactive water is visible but restrained, tracks the fish coherently, and does not change authoritative movement.
- P3 Character Select exposes all five animated FAF fish, starts play with the chosen visual under the shared gameplay contract, and Return to Character Select works without death/save/history side effects.

These targets require native observation where presentation or input is claimed. Headless checks and MCP connectivity alone are not proof of a usable interface.
