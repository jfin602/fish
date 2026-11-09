# Fish Boot Guide

Read [AGENTS.md](AGENTS.md) first. This is a concise handoff; [the roadmap](docs/roadmap.md) owns Phase/Gate progress and [development setup](docs/development.md) owns command availability and tool pins.

## Current starting point — 2026-11-09

| Area | Actual state |
| --- | --- |
| Product | Owner-approved 3D fish survival, a saved living fish across sessions, permanent death, and a main-menu archive of brief life stories. |
| Documentation | Initial product/system contracts, decisions, roadmap, workflow, and setup instructions authored. See [starter evidence](docs/tasks/repository-bootstrap/qualification.md). |
| Phase runner | Space Factory's Node.js runner ported as `fish:phase` and `fish:phase:validate`; package version starts at `0.0.0`. This is development metadata. |
| Game and developer CLI | `game/` and `tools/dev` have not been created. No Fish game runtime exists yet. |
| Engine direction | Godot 4 .NET/C#; reference pins and 3D renderer decision are in [technical direction](docs/technical-direction.md). |
| Godot Editor MCP | Required development integration, researched and specified; not installed or observed for Fish by this starter. |
| Gameplay/platform evidence | None. Space Factory's Linux observations do not qualify Fish; no Fish platform has passed. |
| Next task | Execute the authored [p0 Godot/MCP foundation stack](docs/tasks/p0/README.md). All game Phases remain NOT STARTED. |

## Product contract to preserve

One fish's life is one run. The fish grows by feeding while avoiding predators, with third person as the primary perspective and first person as an optional view. Closing the game suspends the living run; death ends it permanently. Each dead life produces a short factual recounting retained under **Life History** in the main menu, independent of later runs.

The [decision register](docs/decisions-and-questions.md) separates confirmed behavior from unsettled species, habitat, art, balance, and progression choices. Do not invent those choices as settled requirements.

## Next bounded scope

P0 creates a visibly runnable 3D Godot .NET foundation, a thin developer CLI, and an isolated Godot Editor MCP connection. It ends with real editor/runtime observations on the owner's available desktop. It does not implement feeding, predators, growth, saves, death, stories, or the history menu.

1. Inspect the real checkout and local tools; preserve existing owner work and the Space Factory setup.
2. Read [p0's plan and prompts](docs/tasks/p0/README.md) and the routed setup docs.
3. Validate with `npm run fish:phase:validate -- p0`.
4. Execute only the authorized bootstrap scope, preserving manual checkpoints and truthful evidence.
5. Reconcile the roadmap, task records, module map, and setup docs after actual execution. The next gameplay phase is P1 swimming/habitat, not an automatic extension of p0.

Keep detailed results in task evidence. Update this guide after a real change in the next action or current implementation.
