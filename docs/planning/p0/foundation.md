# P0 — Godot and MCP foundation

**State:** authored design, no engine implementation or native evidence. [P0 Gates](../../roadmap.md#p0--godot-and-mcp-foundation) own acceptance; [task stack](../../tasks/p0/README.md) owns execution.

## Outcome

Open Fish in the Godot .NET editor, play a small visible 3D scene with a controllable fish placeholder and third-person camera, and inspect/control the actual Fish editor/runtime through an isolated Godot Editor MCP connection. Provide the repeatable CLI and truthful setup documentation needed for later gameplay.

## Starting point

The repository contains design/workflow docs, a Node.js 24 phase runner and its tests. `game/`, `tools/dev`, native Godot evidence and the Fish MCP connection are absent. Source conventions come from Space Factory commit `ce163753d8973ccd018f2344c6175c52fe4b113d`; its runtime and 2D game content are not the starting Fish implementation.

## Scoped choices

- Follow the reference Godot 4.7.2 .NET/C# and .NET 10.0.403 pins if locally usable; verify actual compatibility and document any necessary deliberate adjustment.
- Use Compatibility for the simple 3D foundation. Final water/lighting/renderer choice remains open.
- Use primitive Godot geometry for a small test habitat, simple fish-shaped body, static collision bounds, light and stationary visual landmarks.
- Provide simple documented horizontal/vertical movement and a camera behind/above the fish. These are bootstrap defaults, not final swimming design or a chosen real species.
- Keep the MVP main scene focused on development. No mocked life saves, stories, menu history or fictitious completed fish.
- Configure MCP in ignored Fish-local client/environment files, with a matching stable addon and a distinct verified local bridge.

## Order and acceptance

1. **P1:** create the Godot/C# project and CLI, then obtain bounded actual-scene and saved-scene checks. G0.1 may gain evidence; native results await P3.
2. **P2:** install/enable/configure the matching MCP server/addon and prepare the correct isolated Fish environment. Record configuration checks and any client restart needed. G0.2 is not passed solely from configuration.
3. **P3:** deliberately run the native closeout in a session with the real editor/MCP tools; verify the peer path, scene edit round trip, rendered game, actual input and logs; reconcile all P0 Gate evidence.

The runner can execute P1/P2 and pauses at P3's inherited `Browser required: yes` marker. P3 requires native Godot evidence, not a web application. A newly configured MCP server may need a Codex session reload; preserve the handoff and do not silently downgrade the requirement.

## Deferrals

Feeding, prey/predator AI, growth/stamina balance, final movement feel, first-person polish, save/resume, permanent death, event journaling, recap/history menus, production assets, advanced water, procedural environments and release exports belong to later phases. Future modules may be documented but should not be created as empty frameworks.

## Verification limits

P0 establishes a development loop. It does not qualify the eventual survival game or another platform. Evidence must name the source, real commands, platform, actual scene/input observations, and exact gaps. [Validation](../../validation.md) defines B01–B04.
