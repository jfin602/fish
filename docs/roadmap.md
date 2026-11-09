# Fish roadmap

**Status: 2026-11-09. P0 foundation COMPLETE on Linux/X11; P1 IN PROGRESS through direct P9 implementation with final native tuning/closeout still open; P2 underwater-world design selected and planned but NOT STARTED.** This is the single Phase/Gate progress authority.

A Gate passes only with source-bound evidence for all of its criteria. A Phase passes only when all required Gates pass. [Validation](validation.md) defines scenarios; task qualification records hold actual results.

## Delivery direction

Build one small, complete fish life in deliberate layers: first make swimming enjoyable, then make the place worth swimming through, then add the food chain, persistence/death, remembered lives, and full integrated qualification. The current P2 decision intentionally puts the underwater world before ecology so feeding and predator behavior are designed inside useful terrain rather than around the flat P1 movement laboratory.

| Phase | Observable outcome | State |
| --- | --- | --- |
| P0 — Godot and MCP foundation | The real 3D project builds, opens, runs and can be inspected through the correct editor connection. | COMPLETE on Linux/X11; [closeout](tasks/p0/closeout.md). |
| P1 — Swim through a habitat | Settle core swimming, third-person camera/body motion, selected starter assets and fullscreen/input lifecycle. | IN PROGRESS through direct P9; final native tuning and Gate review open. |
| **P2 — Build the underwater world** | Replace the prototype-course look with a natural reef space, convincing underwater atmosphere, and movement-reactive water presentation. | **NOT STARTED; [design](planning/p2/underwater-world.md) and [stack](tasks/p2/README.md) authored.** |
| P3 — Feed, escape and grow | Food, prey, predators and growth produce the first changing food-chain loop inside the usable habitat. | NOT STARTED. |
| P4 — A life across sessions | Continue one living fish; death permanently ends it with recoverable durable records. | NOT STARTED. |
| P5 — Remember every life | Show a brief true recounting and revisit all saved stories from Life History. | NOT STARTED. |
| P6 — Qualify the complete experience | Observe the integrated life loop, optional first person and chosen desktop presentation. | NOT STARTED. |

## P0 — Godot and MCP foundation

**Status: COMPLETE on the observed Linux/X11 desktop.** [Foundation design](planning/p0/foundation.md); [p0 closeout](tasks/p0/closeout.md). Scenarios B01–B04 passed as recorded in the P0 evidence.

### G0.1 — Reproducible 3D project and CLI

**Result: PASS.**

- Godot .NET/C# project and coherent engine/SDK pins exist under `game/`.
- The repository-relative developer CLI imports, builds, checks, opens the editor/runs the game, and performs bounded scene automation with meaningful failures.
- The foundation scene, C# execution and saved-scene round trips were observed at the documented source/platform boundary.

### G0.2 — Correct, isolated Godot Editor MCP connection

**Result: PASS.**

- Selected server/addon components and Fish-local routing were configured without replacing Space Factory settings.
- A live peer identified this Fish checkout before editor mutation; scene inspection/save/reload succeeded.
- C# compilation remains an actual .NET build rather than an MCP-connectivity inference.

### G0.3 — Native development loop

**Result: PASS.**

- The actual Fish editor/main scene ran on Linux/X11.
- Prototype horizontal/vertical movement, release, runtime state, running-game pixels and clean stop were observed.
- This qualifies the development loop, not final swimming/world quality.

## P1 — Swim through a habitat

**Status: IN PROGRESS through direct P9 fish-motion implementation; all G1 Gates remain open pending the final native candidate and evidence review.** [P1 design](planning/p1/core-swimming.md); [p1 stack](tasks/p1/README.md); [implementation evidence](tasks/p1/qualification/implementation.md); [P5 native evidence](tasks/p1/qualification/native-display-and-assets.md).

The completed controls worksheet establishes the qualitative core and seven owner-play criteria. P1 closes only when the actual bindings/tuning, camera/body motion, selected assets and display/input lifecycle are validated together and the owner accepts the swimming feel. P2 planning does not waive that boundary.

### G1.1 — Swimming and physical space

**Result: NOT OBSERVED as complete.**

- The accepted desktop control paths complete the same core movement course comfortably.
- Observe quick acceleration, gradual release slowdown, explicit slowing behavior, curved turns/turnarounds, gap navigation and pitch-based upper/lower travel.
- Observe clean release and pause/resume with coherent collision/bounds; intended coasting is distinguishable from stuck input.
- Rendering/view changes do not change equivalent authoritative movement/time outcomes.

### G1.2 — Third-person readability

**Result: NOT OBSERVED as complete.**

- Observe behind/slightly-above framing at moderate distance with the whole fish and useful route ahead visible.
- Observe smooth direction/depth follow, obstacle adjustment, visible swimming/glide motion and turning bend/lean with a steady horizon.
- Record the owner's judgment after several minutes of ordinary swimming that the implemented core is natural, nimble, comfortable and easy to control.

### G1.3 — Selected ocean assets in the running course

**Result: NOT OBSERVED as complete.**

- Quaternius `Fish3` / `ClownFish`, Rock Face 01, Coast Sand 01 and Smithsonian *Heliopora coerulea* are the required P1 set.
- Provenance/preparation/local installation/public-source boundaries remain recorded.
- Scale, materials/deformation, collision agreement, representative scene performance and missing-resource diagnostics must apply to the final P1 candidate.
- These starter assets are inputs to P2 composition; they do not by themselves make a finished world.

### G1.4 — Fullscreen presentation and input lifecycle

**Result: NOT OBSERVED as complete.**

- Fresh-profile fullscreen and windowed-fullscreen testing behavior work as documented for the observed backend.
- Both accepted desktop input paths survive pause, mode/profile changes, focus loss/refocus and display changes without leaked propulsion or trapped UI focus.
- Camera/UI framing remains readable at tested viewport dimensions.

P1 completion records the final accepted tuning/profile. [Movement abilities](planning/p1/movement-abilities.md) remain approved future movement work, but the owner has superseded their immediate scheduling: **P2 worldbuilding is the next roadmap Phase after P1**.

## P2 — Build the underwater world

**Status: NOT STARTED.** [P2 design](planning/p2/underwater-world.md); [execution stack](tasks/p2/README.md). Scenarios V18–V20.

P2 turns the movement laboratory into a place that already feels like a game when the player simply swims around. The target is a compact authored reef-like habitat; it does not settle the final production biome or larger procedural-world strategy.

### G2.1 — Natural reef habitat and traversal

**Result: NOT OBSERVED.**

- Normal play no longer exposes the P1 flat-floor/ceiling/test-course presentation, colored target spheres, camera-check post or other obvious developer geometry.
- Build a continuous natural seabed with varied elevation, coral/reef areas, rock walls/formations, arches or tunnels/caves, overhangs, open swimming pockets and recognizable landmarks at multiple depths.
- The player cannot escape beneath the seabed or through intended world limits. Visual geometry and simplified collision agree well enough that tunnels, cover and camera clearance are understandable.
- At least one broad route, one tighter rock passage/tunnel and one meaningful vertical route can be traversed comfortably with the accepted P1 fish/camera.
- Preserve the P1 movement course as a dedicated qualification/development fixture or equivalent; removing it from normal presentation must not erase historical evidence.

### G2.2 — Convincing underwater atmosphere and renderer

**Result: NOT OBSERVED.**

- Compare the actual post-P1 candidate under the current Compatibility renderer and Forward+ on the target desktop before locking the P2 renderer. Record startup/material correctness, available effects, native visual result, errors and representative performance; do not select Forward+ by assumption.
- With the selected renderer, combine distance/depth haze or fog, underwater color/contrast treatment, directional light from above, moving caustic-style illumination, suspended water matter, and restrained full-view water treatment so near/far/deep views read as submerged rather than merely blue-tinted.
- The effect stack remains comfortable in motion and preserves readable fish/world silhouettes. HUD/pause/settings remain crisp and are not accidentally treated as geometry inside the water.
- Renderer/presentation changes do not change authoritative swimming, time, input semantics or fish identity. Representative native performance is recorded on the P2 habitat.

### G2.3 — Movement-reactive water presentation

**Result: NOT OBSERVED.**

- Water visibly reacts to the fish using bounded presentation effects: ordinary swimming has a readable but restrained disturbance, acceleration increases it, gliding reduces it, and hard turning produces a directional change/turbulence cue.
- Effects originate from believable body/tail space, remain coherent through pitch/yaw, do not obscure the camera, and clear/freeze appropriately across pause/reset/focus lifecycle.
- The effect reads movement state without applying forces, modifying collision or becoming the source of speed/turn state.
- Particle/trail counts and materials are bounded and representative native performance remains acceptable for the chosen P2 scene.

## P3 — Feed, escape and grow

**Status: NOT STARTED.** Scenarios V03–V05. [Ecology contract](systems/ecology-and-growth.md).

### G3.1 — Feeding and visible growth

**Result: NOT OBSERVED.**

- Food and suitable smaller prey can be consumed through defined actions and eligibility rules.
- Each consumed entity grants its reward exactly once; unavailable/ineligible targets do not grant growth.
- Growth visibly updates the same fish and relevant body/collision/camera behavior without clipping, invalid spawning or new identity.
- Demonstrate a meaningful food-chain reversal using a documented species/size rule; growth changes habitat use and hunting opportunity.

### G3.2 — Readable predation and meaningful events

**Result: NOT OBSERVED.**

- A predator can detect, pursue and catch the player; an observable escape route/timing works in a separate scenario.
- Threat cues, attack commitment and failure cause are understandable in native play, including danger outside the immediate forward view.
- Death and key growth/hunt/escape events have authoritative causes/stable identities for later persistence/story use.
- New test lives reset intended prototype state; durable multi-session behavior is not claimed yet.

## P4 — A life across sessions

**Status: NOT STARTED.** Scenarios V06–V09. [Run/persistence contract](systems/survival-and-runs.md).

### G4.1 — Suspend and resume one living fish

**Result: NOT OBSERVED.**

- Save, close and resume the same life with coherent growth, health, position, simulation time, consumed entities and relevant encounter state.
- No offline hunger, predation, cooldown progress or growth is applied.
- Continue/New Life follows an explicitly chosen active-slot/checkpoint policy; replacing a living save cannot silently erase it.
- Storage incompatibility/corruption is reported with recoverable data preserved, separately from in-world death.

### G4.2 — Durable death and independent history records

**Result: NOT OBSERVED.**

- An actual terminal outcome prevents normal/supported recovery paths from reviving that run.
- Death retains event/final-state data needed for a recounting under one stable run ID.
- Interruption/retry at material death/archive boundaries converges to one terminal outcome and one completed-life record.
- A new hatchling receives a new run identity without clearing prior completed-life records.

## P5 — Remember every life

**Status: NOT STARTED.** Scenarios V10–V12. [Story/archive contract](systems/life-stories-and-history.md).

### G5.1 — Brief, factual recounting

**Result: NOT OBSERVED.**

- Death presents a brief account built from supported events/final state; every factual claim is traceable.
- Short and eventful lives produce appropriate accounts without fabricated achievements, intent or attacker identity.
- Skipping presentation preserves the final saved narrative; presentation failure cannot reverse death.

### G5.2 — Main-menu Life History

**Result: NOT OBSERVED.**

- Main menu exposes Life History with an honest empty state and access to every saved completed story.
- Open/read/return/start another life/restart: prior entries and text remain intact and dead fish remain unplayable.
- A representative larger archive preserves reachability/readability with no silent truncation or duplicate lives.

## P6 — Qualify the complete experience

**Status: NOT STARTED.** Scenarios V13–V15.

### G6.1 — Optional first person and presentation

**Result: NOT OBSERVED.**

- Toggle optional first person on the same fish and return to third person without changing simulation state or invalidating a save.
- Observe camera clipping, input/focus, threat feedback, text readability and recap navigation across the implemented size range.
- Keep the primary third-person experience readable and record the selected visual/performance scope from measured play.

### G6.2 — Full life and platform qualification

**Result: NOT OBSERVED.**

- Complete feeding/growth/escape, suspend/resume across sessions, die, and read the true saved story.
- Start another hatchling and prove prior history remains; repeat with a short life and distinct identity/account.
- Record the complete flow on an identified source/platform without debug rewards substituting for claimed gameplay.
- Qualify each claimed export/target OS separately.

## After this slice

Use observed play to choose the final species/biome, larger habitat strategy, additional species/vegetation, long-life pacing, art/audio depth, discovery/cosmetic carryover and production performance budgets. Do not turn the first reef slice into an obligation to solve world generation or every future visual effect.
