# Fish

A planned 3D survival game about one fish's life, from vulnerable hatchling to large predator. Eat food and smaller fish, find shelter, and survive long enough to change your place in the food chain.

Play primarily in third person, with an optional first-person view. Continue the same living fish across multiple sessions. Death ends that life permanently and the next begins as a hatchling. Each dead fish leaves a brief, event-backed life story that can be revisited from **Life History** in the main menu.

## Current state

This is the documentation and development-workflow starter. It includes the ported Space Factory phase runner and an authored Godot/MCP bootstrap stack. No Godot project or gameplay has been implemented or qualified yet.

The chosen technical direction is Godot 4 .NET with C# and Godot Editor MCP, following Space Factory's development approach and adapting the game to 3D. Exact reference versions and the limits of their verification are in [development setup](docs/development.md).

## Start here

- [Current state and next task](BOOT.md)
- [Agent and contributor guide](AGENTS.md)
- [Documentation index](docs/README.md)
- [Game design](docs/game-design.md)
- [Roadmap and acceptance Gates](docs/roadmap.md)
- [Development workflow](docs/workflow.md)
- [Godot and MCP setup contract](docs/development.md)
- [P0 bootstrap stack](docs/tasks/p0/README.md)
- [Decisions and open questions](docs/decisions-and-questions.md)

## Development runner

From the repository root, use Node.js 24. The runner has no npm dependencies, so no install step is required.

```bash
npm run fish:phase:validate -- p0
npm run test:runner
```

When ready to execute the authored stack with a locally installed and authenticated Codex CLI:

```bash
npm run fish:phase -- p0
```

P0 prepares the Godot 3D project, integrates the editor MCP, and ends with a required native closeout. The runner pauses for prompts marked as requiring visible/manual observation. See [the runner guide](docs/phase-runner.md) for exact behavior, versions, checkpoints, and resume rules. `tools/dev` is created by p0; it is not present in this starter.

## Repository layout

| Location | Responsibility |
| --- | --- |
| `AGENTS.md`, `BOOT.md`, `MODULES.md` | Task routing, current handoff, and current/planned module boundaries. |
| `docs/` | Product contracts, workflow, roadmap, setup, validation, and decisions. |
| `docs/planning/`, `docs/tasks/` | Scoped designs, executable prompts, and source-bound evidence. |
| `scripts/`, `test/unit/`, `package.json` | Development phase runner and its tests/checkpoint metadata. |
| `game/`, `tools/` | Planned Godot project and developer CLI, created during p0. |
| `known-issues.md`, `feature-ideas.md` | Observed defects and proposed additions. |
| `artifacts/`, `.codex-runs/` | Ignored generated outputs when tools run. |

Workflow and runner adapted from [jfin602/space-factory at ce163753](https://github.com/jfin602/space-factory/tree/ce163753d8973ccd018f2344c6175c52fe4b113d). Fish has its own design, state, roadmap, and evidence; the reference game's implementation and prior results are not Fish results.
