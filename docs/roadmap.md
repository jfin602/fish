# Fish roadmap

**Status: 2026-11-09. P0 foundation COMPLETE on Linux/X11; P1 IN PROGRESS through direct P4 implementation, with all G1 Gates still NOT OBSERVED. Later gameplay Phases NOT STARTED.** [P0 closeout](tasks/p0/closeout.md) is the source-bound foundation record. The earlier documentation/runner starter is recorded separately in [repository-bootstrap evidence](tasks/repository-bootstrap/qualification.md).

This is the single Phase/Gate progress authority. A Gate passes when all its criteria have source-bound evidence; a Phase passes when all its Gates pass. [Validation](validation.md) defines scenarios, while task qualification records hold results. Update status here with a link to the actual evidence.

## Delivery direction

Build one small, complete fish life before expanding content: a readable 3D habitat, feeding and predator escape, meaningful growth, save/resume, permanent death, a factual life recounting, and a persistent main-menu archive. The [decision register](decisions-and-questions.md) identifies open final-species, specific-biome, balance, detailed-presentation and larger-world choices within the selected ocean direction.

| Phase | Observable outcome | State |
| --- | --- | --- |
<<<<<<< Updated upstream
| P0 — Godot and MCP foundation | The real 3D project builds, opens, runs and can be inspected through the correct editor connection. | NOT STARTED; [stack authored](tasks/p0/README.md). |
| P1 — Swim through a habitat | Settle swimming dynamics in both desktop modes, using the four starter assets and fullscreen/windowed-fullscreen play. | IN PROGRESS through direct P4 work; Gates not passed. |
=======
| P0 — Godot and MCP foundation | The real 3D project builds, opens, runs and can be inspected through the correct editor connection. | COMPLETE on Linux/X11; [closeout](tasks/p0/closeout.md). |
| P1 — Swim through a habitat | Control a readable fish in a bounded 3D environment using the primary third-person camera. | IN PROGRESS through direct P4 work; Gates not passed. |
>>>>>>> Stashed changes
| P2 — Feed, escape and grow | Food, prey, predators and growth produce the first changing food-chain loop. | NOT STARTED. |
| P3 — A life across sessions | Continue one living fish; death permanently ends it with recoverable durable records. | NOT STARTED. |
| P4 — Remember every life | Show a brief true recounting and revisit all saved stories from Life History. | NOT STARTED. |
| P5 — Qualify the complete experience | Observe the integrated life loop, optional first person and chosen desktop presentation. | NOT STARTED. |

## P0 — Godot and MCP foundation

**Status: COMPLETE on the observed Linux/X11 desktop.** [Foundation design](planning/p0/foundation.md); [p0 closeout](tasks/p0/closeout.md). Scenarios B01–B04 passed as recorded in the [native playtest](tasks/p0/qualification/native-playtest.md).

### G0.1 — Reproducible 3D project and CLI

**Result: PASS.** Actual pins, C# scene execution, wrapper and scene round trip are in [implementation evidence](tasks/p0/qualification/implementation.md).

- Godot .NET/C# project and coherent engine/SDK pins exist under `game/`.
- A real main scene contains a small 3D test habitat, controllable placeholder fish and third-person camera; prototype controls are documented.
- The repository-relative developer CLI reports readiness, imports, compiles and performs a bounded real-scene smoke with meaningful failure behavior.
- Saved-scene inspection and disposable save/reload automation work without leaving source artifacts.
- Current capabilities and platform evidence are documented precisely.

### G0.2 — Correct, isolated Godot Editor MCP connection

**Result: PASS.** Matched components, Fish-local configuration, live peer identity and editor save/reload are in [implementation](tasks/p0/qualification/implementation.md) and [native playtest](tasks/p0/qualification/native-playtest.md).

- The selected stable server and matching addon are installed/configured with traceable versions and notices.
- Fish's local configuration preserves Space Factory and unrelated client settings, uses a distinct local bridge, and stays out of Git.
- Live server/peer inspection identifies this Fish checkout before mutation; real scene inspection and a bounded saved-scene operation work.
- C# compilation remains an actual .NET build; a configured server or TCP listener alone cannot pass this Gate.

### G0.3 — Native development loop

**Result: PASS.** Live keyboard/action state, prototype bounds, captured game frames, editor output and clean stop are in the [native playtest](tasks/p0/qualification/native-playtest.md).

- Open the real Fish editor and launch the actual main scene on the selected desktop.
- Observe the 3D fish/camera, demonstrate movement including vertical travel, release input, and confirm the foundation bounds.
- Use the live MCP/runtime path to inspect state, capture real game pixels and verify a bounded input change; stop cleanly and check output.
- Record source revision, platform/tool versions, actions/results and evidence. A reload or unavailable native tool remains an explicit gap until performed.

These Gates qualify the development foundation only. Final swimming feel, food-chain gameplay, persistence, history and other platforms remain later work.

## P1 — Swim through a habitat

**Status: IN PROGRESS through direct P8 follow-camera implementation; all G1 Gates remain NOT OBSERVED as complete.** [P1 design](planning/p1/core-swimming.md); [p1 execution stack](tasks/p1/README.md); [P1–P4/P6–P8 implementation evidence](tasks/p1/qualification/implementation.md) and [P5 native evidence](tasks/p1/qualification/native-display-and-assets.md). The provisional display and selected assets were observed on Linux/X11, and P6–P8 headless motor/input/camera checks passed; final V01–V02 and V16–V17 acceptance awaits body motion and owner tuning.

**Design accepted — 2026-11-09:** the [completed worksheet](planning/p1/swim-controls-worksheet.md) establishes the core controls and seven playtest criteria, owned by the [swimming contract](systems/swimming-and-camera.md) and [validation V01–V02](validation.md#core-movement-acceptance). Implementation and native acceptance remain to be observed. Lock the implemented core through actual playtesting and owner acceptance before beginning **[Movement abilities](planning/p1/movement-abilities.md)**, the next selected implementation focus.

**P1 delivery scope — 2026-11-09:** use the approved four-asset starter set in the real course, develop fullscreen from the first task, and provide windowed fullscreen for testing. P1 closes only when the dynamics are tuned and accepted, the final profile is recorded, and all four Gates below pass. P1–P8 were executed directly; the phase runner has not run this stack. P5's native Linux/X11 checkpoint and P6–P8 headless motor/input/camera checks are recorded; owner tuning and final review remain ahead.

### G1.1 — Swimming and physical space

**Result: NOT OBSERVED.**

- Implement the accepted mouse-only mode and keyboard fallback; each completes the same core movement course comfortably.
- Observe quick acceleration, gradual release slowdown, keyboard backward-input slowing, curved turns/turnarounds, gap navigation, and pitch-based travel to upper/lower landmarks and back to level travel.
- Observe clean input release and pause/resume with coherent collisions and boundaries. Intended coasting remains distinguishable from stuck input.
- Rendering/view changes do not alter equivalent movement/time outcomes. Qualify the core controls before the Movement abilities follow-up.

### G1.2 — Third-person readability

**Result: NOT OBSERVED.**

- Observe the accepted third-person framing: behind/slightly above, moderate distance, whole fish and route ahead visible, smooth direction/depth following, and adjustment near geometry.
- Observe visible swimming/gliding body animation and turning lean with a steady camera horizon.
- The test habitat uses the four selected starter assets, with readable cover, open travel, landmarks at different depths, and gaps; G1.3 owns their preparation and native integration evidence.
- Observe focus/release and supported-viewport behavior in both modes; camera/UI state does not become simulation authority.
- Record the owner's judgment that several minutes of ordinary swimming feel natural, nimble, comfortable and easy to control in both modes. This actual acceptance, together with G1.1, locks the core for the Movement abilities follow-up.

### G1.3 — Selected ocean assets in the running course

**Result: NOT OBSERVED.**

- The controlled visual fish is Quaternius `Fish3` / `ClownFish`; Rock Face 01, Coast Sand 01 on authored seabed geometry, and Smithsonian *Heliopora coerulea* are visibly present in the actual P1 course. Additional Quaternius fish may populate the scene without implying ecology mechanics.
- Actual package/source/license/credit identity, preparation, local installation and import status are recorded; restricted originals, conversions and embedded payloads remain outside public tracked content.
- Scale, materials, fin/coral transparency, fish deformation, collision clearance and representative scene performance are observed. The visible geometry agrees with the tested navigation and camera.
- Missing required resources produce an actionable diagnostic. A placeholder or unapproved substitute cannot qualify this Gate. The test fish does not settle the final species/growth-art decision.

### G1.4 — Fullscreen presentation and input lifecycle

**Result: NOT OBSERVED.**

- Fullscreen is the fresh-profile startup presentation from the first P1 implementation task; windowed fullscreen provides a full-display testing option. Record actual backend behavior when requested modes are equivalent.
- Local display choice and temporary developer overrides work as documented. Observe actual native windows, viewport dimensions and readable anchored UI/camera framing.
- Test both desktop input modes across both display options, including pause, mode changes, Alt-Tab/focus loss, explicit resume and fresh input after recapture. UI actions must not leak into propulsion.
- Mouse-only controls include pointer-accessible pause/settings; keyboard fallback includes complete keyboard navigation. Display transitions do not alter authoritative fish state or advance paused simulation.

P1 completion also requires a source/configuration/asset-bound accepted tuning record and the owner's explicit acceptance of the seven swimming criteria in both modes. Core feel cannot remain an unfinished later-polish item. The next selected movement scope is Movement abilities after this acceptance and P1 review.

## P2 — Feed, escape and grow

**Status: NOT STARTED.** Scenarios V03–V05. [Ecology contract](systems/ecology-and-growth.md).

### G2.1 — Feeding and visible growth

**Result: NOT OBSERVED.**

- Food and suitable smaller prey can be consumed through defined actions and eligibility rules.
- Each consumed entity grants its reward exactly once; unavailable/ineligible targets do not grant growth.
- Growth visibly updates the same fish and relevant body/collision/camera behavior without clipping, invalid spawning or new identity.
- Demonstrate a meaningful food-chain reversal using a documented species/size rule; growth changes habitat use and hunting opportunity.

### G2.2 — Readable predation and meaningful events

**Result: NOT OBSERVED.**

- A predator can detect, pursue and catch the player; an observable escape route/timing works in a separate scenario.
- Threat cues, attack commitment and failure cause are understandable in native play, including danger from outside the immediate forward view.
- Death and key growth/hunt/escape events have actual authoritative causes and stable identities for later persistence/story use.
- New test lives reset the intended prototype state; durable multi-session behavior is not claimed yet.

## P3 — A life across sessions

**Status: NOT STARTED.** Scenarios V06–V09. [Run/persistence contract](systems/survival-and-runs.md).

### G3.1 — Suspend and resume one living fish

**Result: NOT OBSERVED.**

- Save, close and resume the same life with coherent growth, health, position, simulation time, consumed entities and relevant encounter state.
- No offline hunger, predation, cooldown progress or growth is applied.
- Continue/New Life behavior follows an explicitly chosen active-slot/checkpoint policy; replacing a living save cannot silently erase it.
- Storage incompatibility/corruption is reported with recoverable data preserved, separately from in-world death.

### G3.2 — Durable death and independent history records

**Result: NOT OBSERVED.**

- An actual terminal outcome prevents normal and supported recovery paths from reviving that run.
- Death retains the event/final-state data needed for a recounting under one stable run ID.
- Interruption/retry at material death/archive boundaries converges to one terminal outcome and one completed-life record.
- A new hatchling receives a new run identity without clearing prior completed-life records.

## P4 — Remember every life

**Status: NOT STARTED.** Scenarios V10–V12. [Story/archive contract](systems/life-stories-and-history.md).

### G4.1 — Brief, factual recounting

**Result: NOT OBSERVED.**

- Death presents a brief account built from supported events and final state; each factual claim is traceable.
- Short and eventful lives produce appropriate accounts without fabricated achievements, intent or attacker identity.
- Skipping presentation preserves the final saved narrative; a failed presentation cannot reverse death.

### G4.2 — Main-menu Life History

**Result: NOT OBSERVED.**

- Main menu exposes Life History with an honest empty state and access to every saved completed story.
- Open, read, return, start another life, close and reopen: the prior entries/text remain intact and the dead fish remains unplayable.
- A representative larger archive preserves reachability and readable navigation, with no silent truncation or duplicate lives.

## P5 — Qualify the complete experience

**Status: NOT STARTED.** Scenarios V13–V15.

### G5.1 — Optional first person and presentation

**Result: NOT OBSERVED.**

- Toggle the optional first-person view on the same fish and return to third person without changing simulation state or invalidating a save.
- Observe camera clipping, input/focus, threat feedback, text readability and recap navigation with the implemented size range.
- Keep the primary third-person experience readable; document the selected visual/performance scope from measured play.

### G5.2 — Full life and platform qualification

**Result: NOT OBSERVED.**

- Complete a real feeding/growth/escape sequence, save and resume it across application sessions, then die and review its true saved story in the main menu.
- Start another hatchling and prove the prior history remains; repeat with a short life and verify distinct identities/accounts.
- Record the complete flow on an identified source/platform without debug rewards substituting for the claimed gameplay.
- Qualify any claimed export and target OS separately. Configured Windows/macOS presets or the reference game's results are insufficient.

## After this slice

Use observed play to choose more species/habitats, content generation, long-life pacing, art/audio depth, discovery/cosmetic carryover and performance budgets. Those choices stay open until they affect a selected task. Do not turn the initial doc set into an obligation to build every proposed mechanic at once.
