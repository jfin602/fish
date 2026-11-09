# Fish roadmap

**Baseline: 2026-11-09. All game Phases are NOT STARTED; all game Gates are NOT OBSERVED.** The documentation/runner starter is recorded separately in [repository-bootstrap evidence](tasks/repository-bootstrap/qualification.md). Authored prompts, passing runner tests, and Space Factory's results do not qualify a Fish gameplay Gate.

This is the single Phase/Gate progress authority. A Gate passes when all its criteria have source-bound evidence; a Phase passes when all its Gates pass. [Validation](validation.md) defines scenarios, while task qualification records hold results. Update status here with a link to the actual evidence.

## Delivery direction

Build one small, complete fish life before expanding content: a readable 3D habitat, feeding and predator escape, meaningful growth, save/resume, permanent death, a factual life recounting, and a persistent main-menu archive. The [decision register](decisions-and-questions.md) identifies unselected species, habitat, balance, art and larger-world choices.

| Phase | Observable outcome | State |
| --- | --- | --- |
| P0 — Godot and MCP foundation | The real 3D project builds, opens, runs and can be inspected through the correct editor connection. | NOT STARTED; [stack authored](tasks/p0/README.md). |
| P1 — Swim through a habitat | Control a readable fish in a bounded 3D environment using the primary third-person camera. | NOT STARTED. |
| P2 — Feed, escape and grow | Food, prey, predators and growth produce the first changing food-chain loop. | NOT STARTED. |
| P3 — A life across sessions | Continue one living fish; death permanently ends it with recoverable durable records. | NOT STARTED. |
| P4 — Remember every life | Show a brief true recounting and revisit all saved stories from Life History. | NOT STARTED. |
| P5 — Qualify the complete experience | Observe the integrated life loop, optional first person and chosen desktop presentation. | NOT STARTED. |

## P0 — Godot and MCP foundation

**Status: NOT STARTED.** [Foundation design](planning/p0/foundation.md); [p0 stack](tasks/p0/README.md). Scenarios B01–B04.

### G0.1 — Reproducible 3D project and CLI

**Result: NOT OBSERVED.**

- Godot .NET/C# project and coherent engine/SDK pins exist under `game/`.
- A real main scene contains a small 3D test habitat, controllable placeholder fish and third-person camera; prototype controls are documented.
- The repository-relative developer CLI reports readiness, imports, compiles and performs a bounded real-scene smoke with meaningful failure behavior.
- Saved-scene inspection and disposable save/reload automation work without leaving source artifacts.
- Current capabilities and platform evidence are documented precisely.

### G0.2 — Correct, isolated Godot Editor MCP connection

**Result: NOT OBSERVED.**

- The selected stable server and matching addon are installed/configured with traceable versions and notices.
- Fish's local configuration preserves Space Factory and unrelated client settings, uses a distinct local bridge, and stays out of Git.
- Live server/peer inspection identifies this Fish checkout before mutation; real scene inspection and a bounded saved-scene operation work.
- C# compilation remains an actual .NET build; a configured server or TCP listener alone cannot pass this Gate.

### G0.3 — Native development loop

**Result: NOT OBSERVED.**

- Open the real Fish editor and launch the actual main scene on the selected desktop.
- Observe the 3D fish/camera, demonstrate movement including vertical travel, release input, and confirm the foundation bounds.
- Use the live MCP/runtime path to inspect state, capture real game pixels and verify a bounded input change; stop cleanly and check output.
- Record source revision, platform/tool versions, actions/results and evidence. A reload or unavailable native tool remains an explicit gap until performed.

These Gates qualify the development foundation only. Final swimming feel, food-chain gameplay, persistence, history and other platforms remain later work.

## P1 — Swim through a habitat

**Status: NOT STARTED.** Scenarios V01–V02.

**Owner-selected sequence — 2026-11-09:** first lock down the core controls using the [swim worksheet](planning/p1/swim-controls-worksheet.md) and actual playtesting. **[Movement abilities](planning/p1/movement-abilities.md)** is the next implementation focus after that core is accepted. The linked plan owns the three selected follow-ups and their remaining design choices.

### G1.1 — Swimming and physical space

**Result: NOT OBSERVED.**

- Adopt and document a bounded 3D control scheme; movement/turning feel responsive in actual play.
- Horizontal/vertical travel, collision and shelter geometry behave coherently, including repeated input and boundary cases.
- Rendering/view changes do not alter equivalent movement/time outcomes. Qualify the core controls before the Movement abilities follow-up.

### G1.2 — Third-person readability

**Result: NOT OBSERVED.**

- The camera follows the same fish, supports understandable heading/depth and handles nearby geometry without hiding necessary action.
- The initial habitat has readable shelter and exposed travel/feeding areas using provisional assets.
- Observe mouse/focus/release behavior and controls at the chosen supported viewport; camera/UI state does not become simulation authority.

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
