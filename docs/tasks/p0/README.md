# P0 — Godot and MCP foundation

**State: assessed, planned and authored; NOT EXECUTED.** No Fish Godot project, editor connection or native gameplay is qualified by this stack's existence.

## Scope

Create Fish's 3D Godot .NET/C# foundation and development CLI, configure the selected Godot Editor MCP without disturbing Space Factory, and deliberately qualify the native editor/play loop. [Foundation design](../../planning/p0/foundation.md), [technical direction](../../technical-direction.md), [development setup](../../development.md), and [P0 Gates](../../roadmap.md#p0--godot-and-mcp-foundation) own the contracts.

No feeding, ecosystem AI, growth, saves, death, recounting, history menu, advanced water, or production assets are implemented in this bootstrap. The placeholder movement exists to prove the development loop; P1 later qualifies swimming as gameplay.

## Sequence and version metadata

Prepared root `package.json` baseline: **`0.0.0`**. Inspect the actual checkout and preserve any compatible local continuation. Phase/checkpoint versions are development metadata, independent of engine/SDK pins or game release status.

| Order | Prompt | Assigned version | Execution state |
| --- | --- | --- | --- |
| P1 | [Godot 3D bootstrap](P1-godot-3d-bootstrap.txt) | `0.0.1` | NOT EXECUTED; T1 project/CLI checks. |
| P2 | [Godot MCP integration](P2-godot-mcp-integration.txt) | `0.0.2` | NOT EXECUTED; T1 configuration/installation checks. |
| P3 | [Native foundation closeout](P3-native-foundation-closeout.txt) | `0.0.3` | NOT EXECUTED; T3 native/manual handoff. |

P3 is the sole final closeout. Its exact `- Browser required: yes.` metadata causes a manual handoff even with `--closeout`. This means native Godot/MCP evidence, not browser gameplay.

## Execution

Read [assessment](prompt-assessment.md) and [implementation plan](implementation-plan.md) when needed for a task boundary. Validate first:

```bash
npm run fish:phase:validate -- p0
```

The requested runner workflow is:

```bash
npm run fish:phase -- p0
```

It executes pending P1/P2 work with its normal configuration, retry and checkpoint rules, then pauses for the native P3 closeout. A direct interactive Codex session can instead execute the same briefs in order under explicit owner instruction. Do not launch a recursive runner from an already running task or fabricate runner checkpoint commits for direct execution.

P2 may require a Codex client reload to expose the new local MCP tools. Record completed configuration, actual checks, and the exact P3 continuation action. P2 completion is configuration success; live G0.2/G0.3 results remain unobserved until P3.

## Evidence to create during execution

Use `qualification/implementation.md` for P1/P2 commands, source identity, environment/configuration observations and limitations. Use `qualification/native-playtest.md` for P3's actual editor/MCP/game input and capture evidence. Create `closeout.md` for the final criterion mapping and disposition. These reports are not prefilled in the starter.

After each task's required checks pass, set only its assigned root package version. A failed fresh task retains the previous checkpoint. Continue partial work without discarding it or falsely advancing metadata. The runner owns implementation checkpoint commits; direct execution and native closeout leave changes reviewable under the workflow.

At P3, reconcile only evidence-supported P0 Gates, setup docs, module map and BOOT. Later Phases stay NOT STARTED. If a native tool, editor, client reload or dependency prevents a required observation, record that exact gap and preserve the work; do not promote a headless/configuration result to native qualification.
