# Fish Boot Guide

Read [AGENTS.md](AGENTS.md) first. This is a concise handoff; [the roadmap](docs/roadmap.md) owns Phase/Gate progress and [development setup](docs/development.md) owns command availability and tool pins.

## Current handoff — 2026-11-09

| Area | Actual state |
| --- | --- |
| Product | Owner-approved 3D fish survival: one living fish across sessions, permanent death, a brief factual life recounting, and a main-menu Life History archive. |
| Foundation | P0 is complete on the observed Linux/X11 desktop; the Godot .NET project, developer CLI, and isolated Godot Editor MCP path are implemented and qualified for the foundation scope. |
| Current gameplay | P1 is in progress through direct P9 implementation. The shared motor, desktop inputs, follow camera, Fish3 animation, selected starter assets, fullscreen shell, and movement course exist. Final native two-mode tuning/owner acceptance is still required. |
| Controls correction | [c1-bindings](docs/tasks/c1-bindings/README.md) is authored at root version `0.1.9`; execute/reconcile it only against the actual current P1 tree. It does not close P1 by itself. |
| Next Phase | **P2 — Build the underwater world** is the selected next roadmap Phase after P1 closes. It replaces the prototype-course presentation with a natural reef habitat, locks the renderer from measured testing, adds underwater atmosphere, and adds movement-reactive water presentation. |
| Renderer | The current implementation uses Godot Compatibility. P2 does **not** preselect Forward+; its first tasks compare the actual P1 candidate under Compatibility and Forward+ on the target desktop, then record one renderer for the phase. |
| Assets | Quaternius Fish3/ClownFish, Rock Face 01, Coast Sand 01, and Smithsonian *Heliopora coerulea* are the P1 starter set. P2 may prepare efficient derivatives/instances and add only provenance-safe free environment assets when the approved habitat cannot be reached with the existing set. |
| Root version | Published root `package.json` is `0.1.9` at this handoff. P2 authoring does not change it. After P1 is actually closed, deliberately prepare the `0.2.0` phase-entry baseline before running [p2](docs/tasks/p2/README.md). |
| Later gameplay | Feeding/predators/growth move to P3. Persistence/death becomes P4, life recounting/history P5, and integrated qualification P6. [Movement abilities](docs/planning/p1/movement-abilities.md) remain accepted future work but are no longer the immediate post-P1 roadmap step. |

## Product contract to preserve

One fish's life is one run. The fish grows by feeding while avoiding predators, with third person as the primary perspective and first person as an optional view. Closing the game suspends the living run; death ends it permanently. Each dead life produces a short factual recounting retained under **Life History** in the main menu, independent of later runs.

The [decision register](docs/decisions-and-questions.md) separates confirmed behavior from unsettled final player species, production biome, balance, long-life pacing and later presentation choices. P2 establishes a convincing reef-like underwater environment for the current slice without claiming the final production biome or world-generation strategy.

## P1 completion boundary

P1 remains the movement-quality gate. Finish the active control correction when applicable, then complete [P10 owner tuning](docs/tasks/p1/P10-owner-tuning-and-dynamics-lock.txt) and [P11 closeout](docs/tasks/p1/P11-swimming-closeout.txt). P1 is complete only when G1.1–G1.4 have source-bound evidence and the owner accepts the actual implemented swimming feel. Do not use P2 authoring to waive an unfinished P1 requirement.

The P1 movement course remains useful as a development/qualification fixture. P2 should stop presenting that course as the normal game world rather than deleting evidence needed to understand P1.

## P2 entry and boundary

After P1 closes:

1. Inspect branch/HEAD, local changes, package version, final P1 tuning, current renderer and actual native evidence. Preserve compatible work.
2. Deliberately prepare root version **`0.2.0`** before invoking the runner; P2 prompts then target `0.2.1` onward.
3. Execute [the P2 stack](docs/tasks/p2/README.md): renderer trial/lock, natural habitat, reef/tunnels, underwater atmosphere, fish-reactive water, native tuning, and closeout.
4. Keep rendering/world presentation read-only with respect to authoritative fish movement. A particle wake, post effect, fog change or camera effect must not change the motor, fish identity or simulation time.
5. Do not implement feeding, predators, growth, saves, death/history, controller, first person, or Movement abilities as hidden P2 scope.

Keep detailed execution results in task evidence. Update this guide only when actual implementation/evidence or the selected next action changes.
