# Fish

A planned 3D survival game about one fish's life, from vulnerable hatchling to large predator. Eat food and smaller fish, find shelter, and survive long enough to change your place in the food chain.

Play primarily in third person, with an optional first-person view. Continue the same living fish across multiple sessions. Death ends that life permanently and the next begins as a hatchling. Each dead fish leaves a brief, event-backed life story that can be revisited from **Life History** in the main menu.

## Current state

<<<<<<< Updated upstream
This published starter includes documentation, the ported Space Factory phase runner, and authored Godot/MCP foundation and core-swimming stacks. It has no Godot project or developer CLI yet. The owner's reported local tech demo has not been inspected by this documentation work.
=======
P0's development foundation is implemented: a small runnable 3D Godot .NET scene, a C# placeholder fish with prototype movement, a developer CLI, and an isolated Godot Editor MCP setup. The [P0 closeout](docs/tasks/p0/closeout.md) qualifies this foundation on Linux/X11. Survival gameplay, saves, death and Life History remain planned.
>>>>>>> Stashed changes

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
- [P1 core swimming, selected assets and fullscreen stack](docs/tasks/p1/README.md)
- [Decisions and open questions](docs/decisions-and-questions.md)

## Development runner

From the repository root, use Node.js 24. The runner has no npm dependencies, so no install step is required.

```bash
npm run fish:phase:validate -- p0
npm run fish:phase:validate -- p1
npm run test:runner
```

The P0 stack has already been executed directly. The phase runner remains available for future task stacks with a locally installed and authenticated Codex CLI:

```bash
npm run fish:phase -- <future-stack>
```

P0 was executed directly in an interactive session; no recursive runner or runner checkpoint commits were made. The runner remains available for later stacks. See [the runner guide](docs/phase-runner.md) for exact behavior, versions, checkpoints and resume rules.

Open the Fish editor with `./tools/dev editor` or build and run the prototype with `./tools/dev run`. Run `./tools/dev doctor` to inspect local tool readiness and `./tools/dev check` for the bounded import/build/C# scene and save/reload checks. The CLI works from any working directory.

P1 is assessed and authored to settle both desktop swimming modes using the four starter assets, fullscreen and windowed fullscreen. Its [entry procedure](docs/tasks/p1/manual/entry-and-resume.md) requires actual P0 completion and a deliberate `0.1.0` baseline before `npm run fish:phase -- p1`. Authoring leaves the current root version unchanged. P1 includes native/manual checks and actual owner acceptance of the dynamics.

## Repository layout

| Location | Responsibility |
| --- | --- |
| `AGENTS.md`, `BOOT.md`, `MODULES.md` | Task routing, current handoff, and current/planned module boundaries. |
| `docs/` | Product contracts, workflow, roadmap, setup, validation, and decisions. |
| `docs/planning/`, `docs/tasks/` | Scoped designs, executable prompts, and source-bound evidence. |
| `scripts/`, `test/unit/`, `package.json` | Development phase runner and its tests/checkpoint metadata. |
| `game/`, `tools/` | Implemented Godot .NET foundation and developer CLI. |
| `known-issues.md`, `feature-ideas.md` | Observed defects and proposed additions. |
| `artifacts/`, `.codex-runs/` | Ignored generated outputs when tools run. |

Workflow and runner adapted from [jfin602/space-factory at ce163753](https://github.com/jfin602/space-factory/tree/ce163753d8973ccd018f2344c6175c52fe4b113d). Fish has its own design, state, roadmap, and evidence; the reference game's implementation and prior results are not Fish results.
