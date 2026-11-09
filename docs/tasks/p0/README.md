# P0 — Godot and MCP foundation

**State: directly executed and closed out on Linux/X11.** [Implementation evidence](qualification/implementation.md), [native playtest](qualification/native-playtest.md), and [closeout](closeout.md) record what passed. This was not a phase-runner execution.

## Scope

Create Fish's 3D Godot .NET/C# foundation and development CLI, configure the selected Godot Editor MCP without disturbing Space Factory, and deliberately qualify the native editor/play loop. [Foundation design](../../planning/p0/foundation.md), [technical direction](../../technical-direction.md), [development setup](../../development.md), and [P0 Gates](../../roadmap.md#p0--godot-and-mcp-foundation) own the contracts.

No feeding, ecosystem AI, growth, saves, death, recounting, history menu, advanced water, or production assets are implemented in this bootstrap. The placeholder movement exists to prove the development loop; P1 later qualifies swimming as gameplay.

## Sequence and version metadata

Prepared root `package.json` baseline: **`0.0.0`**. Inspect the actual checkout and preserve any compatible local continuation. Phase/checkpoint versions are development metadata, independent of engine/SDK pins or game release status.

| Order | Prompt | Assigned version | Execution state |
| --- | --- | --- | --- |
| P1 | [Godot 3D bootstrap](P1-godot-3d-bootstrap.txt) | `0.0.1` | PASS; T1 project/CLI checks. |
| P2 | [Godot MCP integration](P2-godot-mcp-integration.txt) | `0.0.2` | PASS; T1 configuration/installation checks. |
| P3 | [Native foundation closeout](P3-native-foundation-closeout.txt) | `0.0.3` | PASS; T3 native observation and closeout. |

P3 is the sole final closeout. Its exact `- Browser required: yes.` metadata denotes a manual/native evidence boundary in runner mode. Direct execution fulfilled that boundary with the real editor, MCP and rendered game.

## Execution

Read [assessment](prompt-assessment.md) and [implementation plan](implementation-plan.md) when needed for a task boundary. Validate first:

```bash
npm run fish:phase:validate -- p0
```

The authored runner invocation for P0 was deliberately not used; P0 was executed directly in this interactive session:

```bash
npm run fish:phase -- p0
```

No recursive runner was launched and no runner checkpoint history was fabricated. The resulting changes remain reviewable in the working tree.

The current app session's loaded MCP tools still point to the existing global connection. Reload/open Codex from this trusted Fish project to expose the local 19080 configuration in the app. P3's live Gate evidence was obtained through the same installed MCP server's direct client, connected to the Fish editor and verified before mutation.

## Execution evidence

The reports are [implementation](qualification/implementation.md), [native playtest](qualification/native-playtest.md), and [closeout](closeout.md).

The assigned checkpoint sequence reached root package version `0.0.3` after the passing closeout. This is development metadata, not a game release.

Later gameplay Phases stay NOT STARTED. The direct live evidence supports G0.1–G0.3; the MCP output ring's `output_pending` behavior remains a documented limitation, with editor process output checked separately.
