# Fish Agent Guide

## Start with the task

Read this guide, then [BOOT.md](BOOT.md). Inspect repository identity, branch, revision, and local changes before editing. Preserve existing work. Keep the requested outcome, constraints, acceptance criteria, completed steps, current step, and verification evidence explicit through interruptions.

Use the routing table below to read only the relevant documents and source. Documentation reading is preparation; report implementation progress through changed behavior and evidence. Current user instructions authorize their bounded requested work. Do not add a second approval or activation step to an already authorized task.

## Project and current boundary

Fish is a planned 3D fish survival game: hatchling to large fish, primarily third person, with an optional first-person view. A living fish persists across play sessions; death permanently ends that life. A brief account of actual life events is retained in the main-menu Life History archive.

The repository contains documentation, a Node.js development phase runner, and the completed P0 Godot .NET/C# foundation. Follow the Godot Editor MCP direction in [technical direction](docs/technical-direction.md). [Development setup](docs/development.md) owns exact pins, local configuration, command availability, and platform evidence; [P0 closeout](docs/tasks/p0/closeout.md) owns the foundation qualification.

| Need | Existing command from the repository root |
| --- | --- |
| Validate a task stack without executing it | `npm run fish:phase:validate -- <folder>` |
| Execute an explicitly selected task stack | `npm run fish:phase -- <folder>` |
| Check runner behavior and prompt grammar | `npm run test:runner` |

<<<<<<< Updated upstream
The authored stacks are [p0 foundation](docs/tasks/p0/README.md) and [p1 core swimming](docs/tasks/p1/README.md). P1 execution requires actual P0 readiness and its documented Phase-entry baseline; authoring is not execution. Node.js 24 is required; no npm dependencies or install step are needed. `tools/dev` commands are planned in p0 and become available only after that implementation. Check the actual files before invoking them.
=======
The first stack `p0` was directly executed. Node.js 24 is required for runner commands; no npm dependencies or install step are needed. `tools/dev` now provides `doctor`, `import`, `build`, `check`, `editor`, `run`, `inspect-scene` and `automation-smoke`.
>>>>>>> Stashed changes

## Documentation routing

| Task | Read the relevant parts of |
| --- | --- |
| Current scope and next action | [BOOT](BOOT.md), [roadmap](docs/roadmap.md) |
| Process, planning, or executable prompts | [Workflow](docs/workflow.md), [runner](docs/phase-runner.md), [planning](docs/planning/README.md), [task stacks](docs/tasks/README.md) |
| Godot/C#/MCP setup and commands | [Development](docs/development.md), [technical direction](docs/technical-direction.md), [p0 task](docs/tasks/p0/README.md) |
| Architecture and source ownership | [MODULES](MODULES.md), [technical direction](docs/technical-direction.md), affected source |
| Product scope and accepted choices | [Vision](docs/vision.md), [game design](docs/game-design.md), [decisions](docs/decisions-and-questions.md) |
| Swimming, input, cameras | [Swimming and camera](docs/systems/swimming-and-camera.md) |
| Food, prey, predators, growth | [Ecology and growth](docs/systems/ecology-and-growth.md) |
| Save/resume, pause, death, new lives | [Survival and runs](docs/systems/survival-and-runs.md) |
| Life events, recounting, persistent archive | [Life stories and history](docs/systems/life-stories-and-history.md) |
| Menus, HUD, onboarding, presentation | [UX](docs/ux-and-presentation.md), [tutorial](docs/tutorial.md) |
| Ocean art, model sources, asset licenses and imports | [Art and assets](docs/art-and-assets.md) |
| Acceptance and regressions | [Validation](docs/validation.md), relevant Gate and task evidence |
| Defects and proposed enhancements | [Known issues](known-issues.md), [feature ideas](feature-ideas.md) |

[The documentation index](docs/README.md) identifies the owner of each fact. Follow a specific reference when needed; avoid reloading the whole corpus after recovery.

## Implementation practices

- Work in small, observable increments. Distinguish accepted requirements, working designs, implemented behavior, and qualified results.
- Keep authoritative fish/run/ecology state separate from input, cameras, rendering, and UI. Camera changes and archive browsing must not advance or rewrite the simulation.
- Use one explicit game clock. Closed-game time does not simulate hunger, predators, or growth. Persist enough encounter state to avoid turning normal resume into an escape exploit.
- Give lives and important events stable identities. Death is terminal; history storage survives new-life creation. Treat interrupted or corrupt persistence as an application/storage problem, not an invented in-world death.
- Build stories from recorded events and measured outcomes. Never invent escapes, kills, locations, motives, or milestones to fill a narrative. Archive one final account per dead life, with idempotent recovery.
- Use C# for first-party gameplay. Keep any small GDScript automation and the upstream MCP addon separate from gameplay; retain necessary Godot UID sidecars and upstream notices. Introduce abstractions when actual work needs them.
- Verify the selected MCP project's identity before editor mutations. Keep Fish and Space Factory connections isolated; coordinate open-editor saves with external scene edits.
- Keep machine paths, MCP local configuration, generated caches, logs, exports, and runner output in ignored locations. Preserve user configuration and unrelated changes.

## Workflow and evidence

For substantial planned work, use `/prompt-ass` → `/prompt-plan` → `/prompt-write <folder>` as defined in [the workflow](docs/workflow.md). These are assistant authoring conventions. Writing a prompt does not execute it. An explicit request to execute a selected stack authorizes that scoped implementation.

The runner owns checkpoint commits during runner execution. Its accepted dirty-tree continuation can stage all existing changes; review scope before accepting it. Use the exact `- Browser required: yes.` metadata for native Godot/MCP/manual observation. This inherited parser field means the runner pauses; it does not require web gameplay or prove that desktop tools are present. Closeout requires human review even with `--closeout`.

Use T1 focused checks by default, T2 for affected-system integration, and T3 for explicit player/platform qualification. Inspect command composition and avoid repeated builds or unrelated suites. Documentation changes need content/link/diff checks. Test material risks such as duplicate growth, terminal resurrection, lost history, and fabricated recap events when those systems exist.

Report actual changes, checks, outcomes, and gaps. A headless build does not prove camera feel, visible editor control, or live input. [The roadmap](docs/roadmap.md) is the single Phase/Gate status authority; a Phase passes only when all required Gates have evidence. Update owning docs and concise BOOT routing, while preserving executed prompts and historical failures in task evidence.
