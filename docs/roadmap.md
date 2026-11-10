# Fish roadmap

**Status: 2026-11-09. P0 foundation COMPLETE on Linux/X11; P1 COMPLETE under the owner's accepted progression baseline; P2 NOT GREEN after P8 evidence review because the P7 native candidate awaits direct owner judgment and missing observations.** This is the single Phase/Gate progress authority. [P1 closeout](tasks/p1/closeout.md) distinguishes source checks, native observations, owner acceptance and deferred refinement.

A Gate passes only with source-bound evidence for all of its criteria. A Phase passes only when all required Gates pass. [Validation](validation.md) defines scenarios; task qualification records hold actual results.

## Delivery direction

Build one small, complete fish life in deliberate layers: first make swimming enjoyable, then make the place worth swimming through, then add the food chain, persistence/death, remembered lives, and full integrated qualification. The current P2 decision intentionally puts the underwater world before ecology so feeding and predator behavior are designed inside useful terrain rather than around the flat P1 movement laboratory.

| Phase | Observable outcome | State |
| --- | --- | --- |
| P0 — Godot and MCP foundation | The real 3D project builds, opens, runs and can be inspected through the correct editor connection. | COMPLETE on Linux/X11; [closeout](tasks/p0/closeout.md). |
| P1 — Swim through a habitat | Settle core swimming, third-person camera/body motion, selected starter assets and fullscreen/input lifecycle. | **COMPLETE; [closeout](tasks/p1/closeout.md).** |
| **P2 — Build the underwater world** | Replace the prototype-course look with a natural reef space, convincing underwater atmosphere, and movement-reactive water presentation. | **IN PROGRESS / NOT GREEN**; P1–P6 implemented, [P7 native candidate](tasks/p2/qualification/native-world.md) and [P8 review](tasks/p2/closeout.md) BLOCKED; G2.1–G2.3 open. |
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

**Status: COMPLETE; G1.1–G1.4 PASS for the owner-accepted progression baseline.** [P1 closeout](tasks/p1/closeout.md) owns the disposition, [accepted tuning](tasks/p1/qualification/accepted-tuning.md) owns the actual inherited values, and the [P1 stack](tasks/p1/README.md) preserves authored history. P1–P9 were directly implemented; c1 controls were reconciled; the owner directly accepted the current candidate on 2026-11-09. The originally authored P10 iterative tuning session was superseded, and P11 evidence review was performed directly. No `0.1.10`/`0.1.11` runner checkpoints are claimed.

The completed controls worksheet established the qualitative core. Existing source-bound checks, bounded Linux/X11 native observations and the owner's explicit acceptance establish the baseline for progression. The authored full per-mode route/duration and display/input matrix was not completed; its gaps remain explicit in the closeout and later refinement is deliberately deferred. P2 preserves this baseline while testing the fish in a natural world.

### G1.1 — Swimming and physical space

**Result: PASS for the accepted P1 baseline.** [Gate evidence and limits](tasks/p1/closeout.md#candidate-and-review-boundary).

- Source-bound motor/scene checks establish acceleration, coast, active brake, curved yaw, pitched travel, rock collision, gap clearance, bounds and pause behavior.
- Bounded native c1 observations show keyboard, mouse and Hybrid input responses; one Hybrid route passed the gap.
- Owner acceptance closes the progression decision. A completed same-course native route in every profile was not recorded; later refinement remains open.
- Camera and rendering remain read-only with respect to the authoritative fish; source-bound checks cover that isolation.

### G1.2 — Third-person readability

**Result: PASS for the accepted P1 baseline.** [Gate evidence and limits](tasks/p1/closeout.md#candidate-and-review-boundary).

- Native P10 captures and owner feedback rejected the sideways and shoulder candidates and favored the centered elevated view.
- Saved-scene camera and animation checks establish follow/obstruction, horizon and Fish3 swim/glide/turn motion without root movement.
- The owner directly accepted the current camera/body candidate for progression; per-mode several-minute durations were not recorded.

### G1.3 — Selected ocean assets in the running course

**Result: PASS for the accepted P1 baseline.** [Gate evidence and limits](tasks/p1/closeout.md#candidate-and-review-boundary).

- Quaternius `Fish3` / `ClownFish`, Rock Face 01, Coast Sand 01 and Smithsonian *Heliopora coerulea* are the required P1 set.
- Provenance/preparation/local installation/public-source boundaries remain recorded.
- P5 native observation established visual integration, missing-resource diagnostics, collision paths and a representative local performance sample; later source checks retained readiness. The sample predates the final c1 controls and expanded box.
- These starter assets are inputs to P2 composition; they do not by themselves make a finished world.

### G1.4 — Fullscreen presentation and input lifecycle

**Result: PASS for the accepted P1 baseline.** [Gate evidence and limits](tasks/p1/closeout.md#candidate-and-review-boundary).

- Fresh-profile fullscreen and windowed-fullscreen requests were observed as borderless full-display windows on the tested X11 backend; exclusive monitor ownership was not proven.
- Native P5/c1 sessions observed menu, focus, display and basic profile input paths; saved-scene checks cover disarm/rearm, first-frame neutral input and switching.
- Advanced c1 native switching/editor and exhaustive four-cell comfort checks remain unobserved and intentionally deferred.

P1 completion records the final accepted tuning/profile. [Movement abilities](planning/p1/movement-abilities.md) remain approved future movement work, but the owner has superseded their immediate scheduling: **P2 worldbuilding is the next roadmap Phase after P1**.

## P2 — Build the underwater world

**Status: IN PROGRESS / NOT GREEN.** The original P2 implementation and owner-green [water/light correction](tasks/c2-watet-light-fx/closeout.md) are retained, but the owner identified missing water-column depth as a required follow-up. [c2-depth](tasks/c2-depth/README.md) is AUTHORED / NOT EXECUTED; G2 Gates remain open until the new basin/depth candidate and remaining native criteria are observed. [P2 design](planning/p2/underwater-world.md); [execution stack](tasks/p2/README.md).

P2 turns the movement laboratory into a place that already feels like a game when the player simply swims around. The target is a compact authored reef-like habitat; it does not settle the final production biome or larger procedural-world strategy.

### G2.1 — Natural reef habitat and traversal

**Result: BLOCKED.** P3/P4 established the continuous collidable seabed, separate P1 fixture and complete prepared model/material placement. P7 reached open, tunnel, shelter and vertical markers in a real window and repaired square seabed transitions. [P8 criterion review](tasks/p2/closeout.md#g21--natural-habitat-and-traversal--v18) retains missing owner visibility/comfort judgment and native perimeter use.

- Normal play no longer exposes the P1 flat-floor/ceiling/test-course presentation, colored target spheres, camera-check post or other obvious developer geometry.
- Build a continuous natural seabed with varied elevation, coral/reef areas, rock walls/formations, arches or tunnels/caves, overhangs, open swimming pockets and recognizable landmarks at multiple depths.
- The default basin contains **at least five natural traversable reef ledges/shelves at distinct elevations**, with irregular slopes, cliffs, channels/drop-offs and alternate transitions rather than a visible staircase. Useful viewpoints can read several depth bands together.
- The player cannot escape beneath the seabed or through intended world limits. Visual geometry and simplified collision agree well enough that tunnels, cover, shelf transitions and camera clearance are understandable.
- At least one broad route, one tighter rock passage/tunnel and a continuous shallow-to-deep-to-shallow vertical route can be traversed comfortably with the accepted P1 fish/camera.
- Preserve the P1 movement course as a dedicated qualification/development fixture or equivalent; removing it from normal presentation must not erase historical evidence.

### G2.2 — Convincing underwater atmosphere and renderer

**Result: BLOCKED.**

P5 established the Forward+ atmosphere foundation, and the owner-green [c2 water/light correction](tasks/c2-watet-light-fx/closeout.md) added the animated overhead surface, organic moving caustics, caustic-aligned shafts and GFX presets. The remaining G2.2 gap is now explicit: the world still needs convincing **actual water-column depth**, with light/color and useful visibility changing as the player descends. [c2-depth](tasks/c2-depth/README.md) owns that follow-up; prior P7/P8 observations remain historical evidence, not proof of the new depth contract.

- P2/P1 provides the Compatibility/Forward+ comparison. The owner ran Forward+ on the real desktop, observed smooth performance and selected it for P2. P2/P2 locked the tracked project to Forward+ and recorded that evidence; another native Compatibility run is not required unless Forward+ later develops a concrete blocker.
- With **Forward+**, combine distance/depth haze or fog, underwater color/contrast treatment, directional light from above, moving caustic-style illumination, suspended water matter, and restrained full-view water treatment so near/far/deep views read as submerged rather than merely blue-tinted.
- Actual world depth below the real water surface changes the presentation: shallows stay brighter/warmer with strong caustic/shaft cues; lower ledges become progressively cooler/dimmer and those surface-light effects weaken.
- View-distance extinction is separately depth-sensitive: comparable horizontal views lose contrast/range progressively as the camera descends, with far reef masses transitioning detail -> silhouette -> water instead of meeting a visible fog wall.
- Several ledges visible in the same frame can read as different depth bands; camera-depth changes alone must not make every world elevation share identical lighting/color.
- The effect stack remains comfortable in motion and preserves readable fish/world silhouettes. HUD/pause/settings remain crisp and are not accidentally treated as geometry inside the water.
- Renderer/presentation changes do not change authoritative swimming, time, input semantics or fish identity. Representative native performance is recorded on the depth-structured P2 habitat.

### G2.3 — Movement-reactive water presentation

**Result: BLOCKED.** [P6 implementation and T2 evidence](tasks/p2/qualification/implementation.md#p2-p6--fish-reactive-water) covers bounded GPU wake/bubbles, pause/focus cleanup and equal-intent movement traces. P7 recorded native response ratios and round-bubble captures. [P8 review](tasks/p2/closeout.md#g23--fish-reactive-water--v20) retains missing owner effect judgment, combined pitch/turn geometry view and actual desktop focus/refocus.

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
