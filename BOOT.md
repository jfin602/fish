# Fish Boot Guide

Read [AGENTS.md](AGENTS.md) first. This is a concise handoff; [the roadmap](docs/roadmap.md) owns Phase/Gate progress and [development setup](docs/development.md) owns command availability and tool pins.

## Current starting point — 2026-11-09

| Area | Actual state |
| --- | --- |
| Product | Owner-approved 3D fish survival, a saved living fish across sessions, permanent death, and a main-menu archive of brief life stories. |
| Documentation | Core-controls design and playtest criteria approved; [worksheet](docs/planning/p1/swim-controls-worksheet.md) complete at 10/10. [Swimming](docs/systems/swimming-and-camera.md) owns the accepted contract. See [starter evidence](docs/tasks/repository-bootstrap/qualification.md) for the original import. |
| Phase runner | Space Factory's Node.js runner ported as `fish:phase` and `fish:phase:validate`; package version starts at `0.0.0`. This is development metadata. |
| Game and developer CLI | The reviewed GitHub snapshot has no `game/` or `tools/dev`. The owner reports playing a local tech demo; its checkout and runtime have not been inspected by this documentation review. |
| Engine direction | Godot 4 .NET/C#; reference pins and 3D renderer decision are in [technical direction](docs/technical-direction.md). |
| Art and habitat | Realistic ocean creatures and environments selected; [art and assets](docs/art-and-assets.md) owns the nine-asset shortlist and four-asset starter set. Downloads, imports, and runtime suitability remain unverified. |
| Godot Editor MCP | Required development integration, researched and specified; not installed or observed for Fish by this starter. |
| Gameplay/platform evidence | None. Space Factory's Linux observations do not qualify Fish; no Fish platform has passed. |
| Next task | Inspect the actual checkout and reconcile foundation evidence; use the authored [p0 stack](docs/tasks/p0/README.md) for missing foundation work. Then implement/tune the accepted core controls and perform [V01–V02](docs/validation.md#core-movement-acceptance). |
| Controls sequence | Design approved; actual core playtest acceptance still required. [Movement abilities](docs/planning/p1/movement-abilities.md) follows that acceptance. Free look is deferred; controller support comes later. |

## Product contract to preserve

One fish's life is one run. The fish grows by feeding while avoiding predators, with third person as the primary perspective and first person as an optional view. Closing the game suspends the living run; death ends it permanently. Each dead life produces a short factual recounting retained under **Life History** in the main menu, independent of later runs.

The [decision register](docs/decisions-and-questions.md) separates confirmed behavior and the realistic ocean direction from unsettled final player species, specific ocean biome, detailed presentation, balance, and progression choices. [Art and assets](docs/art-and-assets.md) records the approved sources and starter order. Do not treat selection as an asset import or a passed gameplay Gate.

## Next bounded scope

The design discussion is complete. Inspect the real checkout first: the published snapshot is the starter, while the owner reports a local demo. Preserve existing work and reconcile what p0 has actually completed before rerunning foundation tasks.

1. Verify the current checkout, local tools, and Fish/Space Factory MCP isolation using the [p0 task](docs/tasks/p0/README.md) and [development setup](docs/development.md). P0 establishes the 3D foundation and tools; it does not qualify final swimming feel.
2. Use the [approved swimming contract](docs/systems/swimming-and-camera.md), [remaining tuning/bindings](docs/systems/swimming-and-camera.md#remaining-tuning-and-bindings), and completed worksheet to scope the core implementation.
3. Implement and tune normal swimming, turning, depth travel, slowing, camera following, and body motion in the mouse-only mode and keyboard fallback.
4. Observe the same course and free swimming in both modes against [V01–V02](docs/validation.md#core-movement-acceptance), then record the owner's acceptance of the implemented feel in task evidence and the roadmap.
5. Once the actual core is accepted, move to the selected Movement abilities follow-up. The current design approval does not advance any gameplay Gate.

Keep detailed results in task evidence. Update this guide after a real change in the next action or current implementation.
