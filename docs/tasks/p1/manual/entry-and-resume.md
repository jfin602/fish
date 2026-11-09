# Phase 1 entry and manual resume

This is execution guidance, not evidence that the stack has run. Authoring leaves the root package version at `0.0.0`. Follow [the runner contract](../../../phase-runner.md) and [workflow](../../../workflow.md); preserve existing work and record direct execution separately from runner history.

## Enter on an actual foundation

1. Inspect the real checkout, branch/revision, local changes, `game/`, `tools/dev`, current package version and existing qualification. The owner's local tech demo may contain useful work absent from the published starter. Preserve and reconcile it.
2. Require evidence-supported P0 G0.1–G0.3 PASS, including the native Fish/MCP observations and reviewed P0 closeout. Run only missing or invalidated foundation work through the existing p0 briefs. A completed document or version label alone does not establish a foundation. Do not invent p0 checkpoint commits to make history look compatible.
3. Reconcile actual P0 completion and package metadata first. Then deliberately establish a reviewed Phase 1 baseline of `0.1.0` in root `package.json`, with a normal scoped coordinator commit recording the transition and its evidence. Do not fabricate a version-only history for unexecuted work. This transition occurs before P1: the runner's entry checks cannot be satisfied by asking P1 to fix its own baseline.
4. Use Node.js 24 and run `npm run fish:phase:validate -- p1`. No npm install is required. Stack validation checks grammar/version planning, not native readiness, Gate results, baseline/history compatibility or owner acceptance. Inspect actual command/tool availability before execution.

For an existing Phase 1 continuation, inspect its real checkpoints instead of resetting to `0.1.0`. Preserve compatible partial work and reconcile inconsistent metadata/history explicitly.

## Run and pause deliberately

From the repository root, start `npm run fish:phase -- p1` only when stack execution is authorized. Do not start a nested runner from a running prompt. The runner owns automated implementation checkpoint commits and requires a contiguous, Git-proven completed prefix matching the package version.

The exact metadata `- Browser required: yes.` causes a pause before that prompt executes. This stack's mandatory manual boundaries are P5, P10 and P11. The inherited field represents native Godot/MCP or human observation; it neither implies browser gameplay nor supplies unavailable native tools. P2 is not an unconditional manual checkpoint, although a concrete account or asset-package dependency may block acquisition. Execute a pending manual brief in an interactive session with the necessary capability. A client reload is a concrete continuation step: save progress, reload, verify the actual Fish peer and resume the pending observations. Configuration success is not live qualification.

## Complete an intermediate manual checkpoint

Before accepting completion, the coordinator reviews the actual diff, evidence, required observations and assigned package version. The executing prompt does not self-commit or push. Missing required evidence, owner rejection or an unfinished repair leaves the previous version and records the exact next action.

| Boundary | Required result | Assigned version and exact checkpoint subject |
| --- | --- | --- |
| P5 | Native display/asset observations complete; final controls still require later qualification. | `0.1.5` |
| P10 | All seven criteria in both modes, display smoke matrix, saved tuning and explicit acceptance of final feel. | `0.1.10` |

After a genuinely completed manual implementation prompt, the coordinator may make its exact-subject version checkpoint, with root `package.json` matching, before runner resume. This records work actually executed; it must not synthesize missing earlier checkpoints. Review staged paths deliberately: the runner's dirty-tree continuation can stage all existing changes. Preserve unrelated work and do not accept that continuation blindly.

Resume with `npm run fish:phase -- p1`. The runner determines the next task from Git/version evidence, not a prose PASS, an empty commit, or a handwritten task list. If directly executed work has no compatible checkpoint chain, reconcile actual history first or continue directly under the workflow; do not trick the runner into skipping work.

## Final manual boundary

P11 remains interactive because its Browser-required metadata is `yes`, including when the runner is launched with `--closeout`. Review applicable T3 evidence and perform T1 reconciliation. Reuse owner acceptance when relevant gameplay source, configuration and asset identities are unchanged; documentation-only edits do not require another swim session.

All G1.1–G1.4 must pass. P11 sets `0.1.11` only on success and otherwise retains `0.1.10`. Leave its final changes uncommitted and reviewable. No manual handoff, accepted limitation or design approval converts a missing required native observation into PASS.
