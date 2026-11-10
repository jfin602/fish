# Task stacks and evidence

Task folders contain the finite material needed to execute a selected outcome. Follow [the workflow](../workflow.md) and [runner grammar](../phase-runner.md); start with the root agent/boot guides and retrieve relevant material only.

## Current folders

| Folder | Purpose and state |
| --- | --- |
| [p0](p0/README.md) | Godot/C#/MCP foundation: directly executed and qualified on the recorded Linux/X11 scope; see P0 closeout. |
| [p1](p1/README.md) | Core swimming/assets/display: direct implementation through P9 at the authoring snapshot; final native tuning/closeout remain open. |
| [c1-bindings](c1-bindings/README.md) | P1–P4 implemented; P5 native evidence is partial and its closeout is NOT GREEN. Finish the listed native/owner checkpoint at unchanged `0.1.9`. |
| [p2](p2/README.md) | **Authored, unexecuted** underwater-world Phase: renderer lock, natural reef habitat, underwater atmosphere and fish-reactive water; requires actual P1 close plus deliberate `0.2.0` entry. |
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
