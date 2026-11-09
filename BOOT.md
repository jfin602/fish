# Fish Boot Guide

Read [AGENTS.md](AGENTS.md) first. This is a concise handoff; [the roadmap](docs/roadmap.md) owns Phase/Gate progress and [development setup](docs/development.md) owns command availability and tool pins.

## Current handoff — 2026-11-09

| Area | Actual state |
| --- | --- |
| Product | Owner-approved 3D fish survival, a saved living fish across sessions, permanent death, and a main-menu archive of brief life stories. |
<<<<<<< Updated upstream
| Documentation | Core-controls design and playtest criteria approved; [worksheet](docs/planning/p1/swim-controls-worksheet.md) complete at 10/10. [Swimming](docs/systems/swimming-and-camera.md) owns the accepted contract. See [starter evidence](docs/tasks/repository-bootstrap/qualification.md) for the original import. |
| Phase runner | Space Factory's Node.js runner ported as `fish:phase` and `fish:phase:validate`; package version starts at `0.0.0`. This is development metadata. |
| Game and developer CLI | The reviewed GitHub snapshot has no `game/` or `tools/dev`. The owner reports playing a local tech demo; its checkout and runtime have not been inspected by this documentation review. |
| Engine direction | Godot 4 .NET/C#; reference pins and 3D renderer decision are in [technical direction](docs/technical-direction.md). |
| Art and habitat | Realistic ocean creatures and environments selected; [art and assets](docs/art-and-assets.md) owns the nine-asset shortlist and four-asset starter set. Downloads, imports, and runtime suitability remain unverified. |
| Godot Editor MCP | Required development integration, researched and specified; not installed or observed for Fish by this starter. |
| Gameplay/platform evidence | None. Space Factory's Linux observations do not qualify Fish; no Fish platform has passed. |
| Next task | Inspect the real checkout and reconcile P0 evidence, then prepare the deliberate `0.1.0` entry baseline and execute the authored [p1 stack](docs/tasks/p1/README.md). Use [p0](docs/tasks/p0/README.md) only for missing foundation requirements. |
| P1 plan | Assessed and authored, not executed: four selected starter assets, both desktop controls, camera/body motion, native tuning and accepted dynamics by phase end. Root version remains `0.0.0` in this published starter. |
| Display | Develop fullscreen from the first P1 task, with windowed fullscreen for testing; actual display/input behavior remains unobserved. |
| Controls sequence | Design approved; actual core playtest acceptance and P1 review still required. [Movement abilities](docs/planning/p1/movement-abilities.md) follows. Free look is deferred; controller support comes later. |
=======
| Documentation | Product/system contracts and workflow remain; [P0 closeout](docs/tasks/p0/closeout.md) records implemented foundation evidence. |
| Phase runner | Node.js `fish:phase` and `fish:phase:validate` retained. Direct P0 execution reached package checkpoint `0.0.3`; no runner checkpoint commits were made. |
| Game and developer CLI | `game/` contains a runnable 3D C# foundation; `tools/dev` imports, builds, checks, opens editor and runs Fish. |
| Engine | Godot 4.7.2 .NET, Godot.NET.Sdk 4.7.2, SDK 10.0.403, `net10.0`, Compatibility renderer on this Linux desktop. |
| Godot Editor MCP | Matched 2026.11.04 server/addon, isolated Fish-local 19080 bridge; live Fish peer and scene operations passed. Reload Codex in Fish to expose the local tools in the app. |
| Gameplay/platform evidence | P0 development foundation G0.1–G0.3 passed on Linux/X11 only. Survival gameplay, exports and other platforms remain unqualified. |
| Next task | Plan the separate [P1 swimming/habitat Phase](docs/roadmap.md#p1--swim-through-a-habitat); do not infer final swimming quality from foundation controls. |
>>>>>>> Stashed changes

## Product contract to preserve

One fish's life is one run. The fish grows by feeding while avoiding predators, with third person as the primary perspective and first person as an optional view. Closing the game suspends the living run; death ends it permanently. Each dead life produces a short factual recounting retained under **Life History** in the main menu, independent of later runs.

The [decision register](docs/decisions-and-questions.md) separates confirmed behavior and the realistic ocean direction from unsettled final player species, specific ocean biome, detailed presentation, balance, and progression choices. [Art and assets](docs/art-and-assets.md) records the approved sources and starter order. Do not treat selection as an asset import or a passed gameplay Gate.

## Completed foundation boundary

<<<<<<< Updated upstream
The design discussion is complete. Inspect the real checkout first: the published snapshot is the starter, while the owner reports a local demo. Preserve existing work and reconcile what p0 has actually completed before rerunning foundation tasks.

1. Verify the current checkout, local tools, and Fish/Space Factory MCP isolation using the [p0 task](docs/tasks/p0/README.md) and [development setup](docs/development.md). P0 establishes the 3D foundation and tools; it does not qualify final swimming feel.
2. Follow [P1 entry and resume](docs/tasks/p1/manual/entry-and-resume.md) after actual P0 readiness; deliberately prepare the `0.1.0` baseline before starting the runner. Do not rerun P0 or manufacture history for already verified work.
3. Execute the [P1 stack](docs/tasks/p1/README.md): fullscreen shell, acquisition/preparation of the four starter assets, course, early native inspection, shared motor and both input modes, camera and body motion.
4. Complete the dedicated owner tuning task and [V01–V02/V16–V17](docs/validation.md). Settle and record the dynamics; all G1.1–G1.4 require actual evidence before P1 passes. Respect the native/manual boundaries at task P5, P10 and P11.
5. After core acceptance and P1 review, route the selected Movement abilities follow-up. Authored prompts and grammar validation do not advance a gameplay Gate.
=======
P0 created a visibly runnable 3D Godot .NET foundation, a thin developer CLI, and an isolated Godot Editor MCP connection. It ended with real editor/runtime observations on the owner's Linux desktop. It did not implement feeding, predators, growth, saves, death, stories, or the history menu.

Review [P0's implementation](docs/tasks/p0/qualification/implementation.md), [native playtest](docs/tasks/p0/qualification/native-playtest.md), and [closeout](docs/tasks/p0/closeout.md) for exact checks and limitations. Open the editor with `./tools/dev editor` or run Fish with `./tools/dev run` from this checkout. The next gameplay phase is P1 swimming/habitat, not an automatic extension of p0.
>>>>>>> Stashed changes

Keep detailed results in task evidence. Update this guide after a real change in the next action or current implementation.
