# Task stacks and evidence

Task folders contain the finite material needed to execute a selected outcome. Follow [the workflow](../workflow.md) and [runner grammar](../phase-runner.md); start with the root agent/boot guides and retrieve relevant material only.

## Current folders

| Folder | Purpose and state |
| --- | --- |
| [p0](p0/README.md) | Godot/C#/MCP foundation: directly executed and qualified on the recorded Linux/X11 scope; see P0 closeout. |
| [p1](p1/README.md) | Core swimming/assets/display: **COMPLETE** under direct owner acceptance; [closeout](p1/closeout.md) records four Gate dispositions and limits. |
| [c1-bindings](c1-bindings/README.md) | Implemented and reconciled into P1; [closeout](c1-bindings/closeout.md) retains advanced native qualification gaps. |
| [c2-watet-light-fx](c2-watet-light-fx/README.md) | **OWNER GREEN / COMPLETE FOR CORRECTION SCOPE**: overhead water, organic moving caustics, caustic-aligned shafts, GFX presets and native owner acceptance; root remains `0.2.6`. |
| [c2-depth](c2-depth/README.md) | **AUTHORED / NOT EXECUTED** Correction 2 follow-up for a five-ledged default basin plus actual water-column depth/extinction; root remains `0.2.6`. |
| [p2](p2/README.md) | **OWNER CLOSED FOR PROGRESSION / NOT GREEN**: G2.1/G2.2 pass; G2.3/V20 remains an accepted gap under S16. P3 transition authorized. |
| [p3](p3/README.md) | **IN PROGRESS at `0.3.6` / NOT GREEN**: FAF Ocean world C3 corrections, native route/style acceptance pending; preserve historical failures. |
| [p4](p4/README.md) | **AUTHORED / NOT EXECUTED**: Ocean + Freshwater Pond World Select, habitat-restricted fish, low-poly Pond FAF and surface fish/camera/shore; `0.4.1–0.4.14` only after an explicit `0.4.0` entry from accepted/reconciled P3. |
| [c3-world-gen](c3-world-gen/README.md) | **P1–P6 IMPLEMENTED / P7 NATIVE FAILED** at `0.3.6`: seeded compiler/bake/load works partly, while visual naturalness and alternate-seed routes fail; four [manual M1–M4 repairs](c3-world-gen/manual/README.md) and owner review are pending before P4. |
| [c3-visibility](c3-visibility/README.md) | **P1–P3 IMPLEMENTED / P4 NATIVE NOT GREEN** at `0.3.6`: visibility source exists but clearance/native observation failed; revisit after accepted C3 world repair, without inventing V25 PASS. |
| [repository-bootstrap](repository-bootstrap/qualification.md) | Evidence for the original documentation/runner import; no gameplay qualification. |

## Stack layout

Use `README.md` for scope, prerequisites, sequence, baseline/version metadata and current execution disposition. Include `prompt-assessment.md`, `implementation-plan.md`, contiguous `P1-...txt` execution files, and one final closeout whose filename and TASK title contain `closeout`.

Execution evidence is created only after checks occur, under `qualification/`, with a single final `closeout.md` when reached. Do not seed passing reports or fictional logs into an unexecuted stack. Preserve executed prompts and original failures; append repairs and their new evidence.

Each ordinary prompt has one coherent outcome and lists its narrow reads, allowed scope, deferrals, implementation steps, validation tier, commands/scenarios, observable done condition and version rule. Use exact metadata understood by the runner. For example:

```text
TASK: Phase 0 / P1 — Godot 3D bootstrap

MODEL / REASONING / USAGE
- Recommended configuration: `GPT-6 Sol High`.
- Browser required: no.
- This prompt's assigned project version is `0.0.1`.
```

Native/editor observation uses the canonical `- Browser required: yes.` field. It pauses the runner before that prompt and requires an interactive native-capable session or actual owner observations. The final closeout remains reviewable even with the runner's `--closeout` option.

Validate before execution:

```bash
npm run fish:phase:validate -- <folder>
```

Writing prompts does not run them. Executing an owner-selected stack authorizes its bounded changes without another activation step. The runner owns its checkpoint commits; direct interactive execution must be recorded honestly and reconciled before later Git-based resume. [The runner guide](../phase-runner.md) is authoritative for parser/version/checkpoint details.

## Evidence fields

Record source revision plus relevant dirty-state identity, platform and tool versions, scenario, actual actions/commands, expected and observed results, PASS/FAIL/NOT OBSERVED or a specific BLOCKED reason, retained artifacts, and unresolved work. Link selected Gate results from the roadmap. Do not present compilation, source inspection, or imported test results as proof of a player workflow.
