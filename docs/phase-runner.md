# Fish phase runner

The Node.js runner executes ordered Codex implementation prompts, records progress and usage, creates Git checkpoints, and resumes from those checkpoints. Fish ports the workflow from [Space Factory revision `ce163753`](https://github.com/jfin602/space-factory/commit/ce163753d8973ccd018f2344c6175c52fe4b113d), which itself adapted [Dope revision `02bb1377`](https://github.com/jfin602/dope/commit/02bb13779acb7e2409f8c6ee42aeffccc0180c37). Gameplay is planned in Godot .NET/C#; this runner is development tooling.

The port preserves Phase and correction version rules, model selection, capacity retries, dirty-tree handling, manual GUI handoffs, and closeout review. Adding the tooling and authoring the [p0 stack](tasks/p0/README.md) do not execute its prompts or qualify a gameplay Gate.

**Starter state:** root version is `0.0.0`. The Godot project under `game/` and the developer wrapper at `tools/dev` have not been created. The p0 tasks establish them and the Godot Editor MCP connection; exact setup belongs to [development setup](development.md).

## Setup and commands

Run from the repository root with Node.js 24 (`>=24 <25`), npm, Git, and a locally installed, authenticated Codex CLI with access to the prompt's selected model. The inherited compatibility check requires Codex CLI 0.155.0 or newer for GPT-6 Sol prompts, or 0.144.0 or newer for the preserved GPT-5.6 model configurations. These are the runner's minimum checks, not a guarantee of account/model availability.

No npm dependencies or install step are needed. The inherited runner rejects a root `package-lock.json` before execution and if one appears during a prompt. Godot/.NET setup is separate; see [development setup](development.md).

| Command | Purpose |
| --- | --- |
| `npm run fish:phase:validate -- p0` | Validate the prompt grammar and version plan in `docs/tasks/p0/`; does not launch Codex or test gameplay. |
| `npm run fish:phase -- p0` | Execute pending implementation prompts and leave the final closeout manual. |
| `npm run fish:phase -- p0 --verbose` | Execute with verbose Codex output instead of the ordinary compact display. |
| `npm run fish:phase -- p0 --closeout` | Also execute an eligible closeout, leaving its changes for human review. |
| `npm run test:runner` | Run the runner behavior and prompt-grammar tests. |

Replace `p0` with the selected folder name, not a path. Equivalent direct entry points are `node scripts/fish-phase.mjs <folder> [--verbose] [--closeout]` and `node scripts/validate-fish-phase.mjs <folder>`. The authored [p0 stack](tasks/p0/README.md) is ready for its separately requested bootstrap execution; no prompt has been executed by this repository setup.

The runner starts a separate `codex exec --json` process for each prompt using its model and reasoning effort. The implementation prompt must name the actual Godot/build/check commands and required evidence; the runner does not automatically add `./tools/dev check` or a playtest.

## Task folders and version metadata

The private root [package.json](../package.json) starts at `0.0.0`. Its version is development roadmap/checkpoint metadata, independent of Godot and .NET pins and any future public game-release policy. Preserve the root manifest and its scripts when changing this version. The runner does not inspect or update Godot project version fields automatically.

| Folder | Prompt type and version rule |
| --- | --- |
| `pN`, including `p0` | `Phase N`; P1 targets `0.N.1`, P2 targets `0.N.2`, and so on. The initial baseline is `0.N.0`. |
| `pNa`, `pNb`, etc. | A pre-1.0 continuation of `Phase N`, restarting filenames at P1. P1's assigned `0.N.patch` sets the offset; later prompts increment the patch. Slice `a` requires a starting patch of at least 1; later letter slices require at least 2. The baseline is one patch below P1. |
| `p1-N` / `p2-N` | `Phase N` in the inherited major-1/major-2 family; P1 targets `1.N.1` / `2.N.1`, with the corresponding `1.N.0` / `2.N.0` baseline. |
| `cN-slug` | `Correction N`; every prompt declares the same existing root version and must leave it unchanged. |
| Descriptive correction name | A lowercase kebab name of 3–32 characters can identify a correction; its first prompt supplies the Phase number. Reserved names are rejected. Prefer `cN-slug` for new stacks. |

For the initial `0.0.0` baseline, an ordinary Phase stack starts at `p0` with P1 assigned `0.0.1`. Entering a later Phase requires its deliberate root baseline update, such as `0.2.0` before `p2`. The runner does not perform this new-Phase baseline transition for you. Choose continuation versions deliberately so they do not reuse prior checkpoint subjects. Version metadata alone does not advance the [roadmap](roadmap.md).

The authored p0 stack has three bounded prompts:

| Prompt | Responsibility | Assigned version | Manual handoff |
| --- | --- | --- | --- |
| P1 | Godot 3D project and local developer commands | `0.0.1` | No |
| P2 | Godot Editor MCP integration and diagnostics | `0.0.2` | No |
| P3 | Native foundation closeout and evidence review | `0.0.3` | Yes |

P1 and P2 are implementation work; P3 is the final native closeout. The actual prompt files own prerequisites, checks, and limitations. A failed local tool connection must be reported truthfully rather than counted as observed. The mandatory P3 handoff is reached even with `--closeout`.

The authored [p1 stack](tasks/p1/README.md) contains eleven tasks targeting `0.1.1` through `0.1.11`. After actual P0 evidence is reconciled, prepare its deliberate `0.1.0` baseline before runner invocation; authoring does not change the starter version. P5 and P10 are intermediate native/manual checkpoints, and P11 is the final Browser-required closeout. Follow [P1 entry and resume](tasks/p1/manual/entry-and-resume.md) for reviewed exact-subject checkpoints and the final human-review boundary. `npm run fish:phase:validate -- p1` validates grammar/version assignments only, not runtime readiness.

## Prompt grammar and templates

Place prompt files directly in `docs/tasks/<folder>/`. The runner reads all direct `.txt` entries and does not recurse into `manual/` or other subfolders. Use `P1-<lower-kebab-slug>.txt`, `P2-<lower-kebab-slug>.txt`, and so on, contiguous from P1 without leading zeros. Keep support notes in Markdown.

Each prompt has exactly one canonical `TASK` line, one recommended configuration, and the version metadata for its stack type. The task's Phase and prompt numbers must match the folder/filename. Keep the em dash, capitalization, backticks, and punctuation shown below. Include an explicit `Browser required` field even though the parser defaults an omitted field to `no`.

Example **`docs/tasks/p0/P1-foundation-check.txt`**:

```text
TASK: Phase 0 / P1 — Foundation check

- Recommended configuration: `GPT-6 Sol Medium`.
- Browser required: no.
- This prompt's assigned project version is `0.0.1`.

Goal: <one concrete outcome>
Read: <the narrow source and document references>
Scope: <allowed changes and preserved behavior>
Implement: <bounded steps, including the assigned root package.json version>
Validate: <tier, actual commands, expected results, and evidence to retain>
Report: <changed behavior, actual checks, gaps, and next routing>
Do not commit; the runner owns the implementation checkpoint.
```

Example final **`docs/tasks/p0/P3-closeout.txt`** (after P1 and P2):

```text
TASK: Phase 0 / P3 — Foundation closeout

- Recommended configuration: `GPT-6 Sol High`.
- Browser required: yes.
- This prompt's assigned project version is `0.0.3`.

Review the implementation and recorded evidence against the scoped criteria.
Reconcile the task record and affected owning documents once.
Report passed, failed, and unobserved checks separately.
Leave closeout changes uncommitted for human review.
```

There must be exactly one final closeout. Both its filename slug and TASK title must contain the word `closeout`; using it in only one is rejected. The final prompt still has the next assigned Phase version even though closeout execution can leave the previous version unchanged pending review.

A correction replaces the task and version metadata, for example in **`docs/tasks/c0-runner-fix/P1-repair.txt`**:

```text
TASK: Correction 0 / P1 — Repair the scoped defect

- Recommended configuration: `GPT-6 Sol Medium`.
- Browser required: no.
- Required unchanged project version: `0.0.0`.
```

Add the concrete implementation/validation body and a final `P2-closeout.txt` with `TASK: Correction 0 / P2 — Repair closeout` and the same unchanged version. Use the actual existing version throughout the correction. A correction must not include assigned-Phase-version metadata; a Phase prompt must not include correction unchanged-version metadata.

### Supported model labels

New prompts should normally select `GPT-6 Sol Medium`, `GPT-6 Sol High`, or `GPT-6 Sol XHigh` according to the work and the owner's available configuration. The parser's full inherited mapping remains unchanged:

| Exact recommended configuration | Codex model | Reasoning effort |
| --- | --- | --- |
| `GPT-6 Sol Medium` | `gpt-6-sol` | `medium` |
| `GPT-6 Sol High` | `gpt-6-sol` | `high` |
| `GPT-6 Sol XHigh` | `gpt-6-sol` | `xhigh` |
| `Luna Medium` / `Luna High` | `gpt-5.6-luna` | `medium` / `high` |
| `Terra Medium` / `Terra High` / `Terra Ultra` | `gpt-5.6-terra` | `medium` / `high` / `ultra` |
| `Sol Light` / `Sol Medium` / `Sol High` / `Sol Ultra` | `gpt-5.6-sol` | `low` / `medium` / `high` / `ultra` |

Unknown labels fail validation. An old `Sol Medium` prompt still selects its original model; it is not silently upgraded to GPT-6.

## Checkpoints, dirty work, and resume

Start with a reviewed clean working tree when possible. An interactive run with uncommitted changes asks `Continue with this dirty working tree? [y/N]`; the default is No. Choosing Yes treats that work as intentional partial implementation. The next successful checkpoint uses **`git add -A`**, so existing tracked changes and unignored new files can be included. Review that scope before accepting. An ordinary non-interactive CLI run rejects a dirty tree; there is no force/yes CLI flag.

For each implementation prompt, the runner verifies the expected package version, Codex success, lockfile absence, and Git state before making its checkpoint. It rejects unexpected self-commits and requires actual implementation changes. The checkpoint body contains Codex's final response. Keep unrelated changes out of an accepted dirty continuation.

Resume is based on reachable Git commit subjects and the matching root version, not a saved conversation or merely the latest `run.json`:

| Stack | Exact checkpoint subject |
| --- | --- |
| Phase | The assigned version alone, for example `0.0.1`. |
| Correction | `<folder>/P<number>: <TASK title>`, for example `c0-runner-fix/P1: Repair the scoped defect`. |

Rerun the same command to skip the contiguous completed implementation prefix. A later checkpoint with an earlier prompt missing is rejected. Duplicate correction subjects are ambiguous and rejected; for duplicate Phase version subjects, the newest reachable match is used. The package version must agree with that prefix; accepted partial dirty work may already carry the next pending Phase version. Prompt files are snapshotted on startup and checked before execution, so do not edit an active prompt stack underneath the runner.

An interrupted/failed attempt preserves partial changes for inspection. Explicit model-capacity errors receive up to three retries after 20-second delays, for four attempts total, with partial work retained. Other failures are not automatically retried. Inspect the failure and worktree before resuming; do not reset away recoverable work.

## Godot GUI handoff and closeout

Use the historical, parser-recognized field **`- Browser required: yes.`** for a prompt that needs a deliberate visible/manual handoff, including native Godot playtesting. Do not substitute `GUI required`; the runner does not recognize that metadata. The runner stops before launching such a prompt and records `browser_execution_required` with the prompt path.

Complete the handed-off task in an interactive environment with the necessary native desktop/screenshot/input tools, or have the owner perform the named observations. Browser access alone does not provide control of the Godot game window. This port adds no screenshot harness, game-input driver, or automatic gameplay acceptance. Record the observed source, actions, results, and limitations under the task's evidence. To resume past a manual implementation prompt, its authorized completion needs the correct version and exact Git checkpoint subject above; the handoff itself is not completion.

The final closeout is manual by default. `--closeout` allows the runner to execute a closeout whose Browser field is `no`, after a clean implementation boundary. A dirty initial tree cannot be carried directly into an automatic closeout when no implementation prompt remains. A Browser-required closeout still stops even with `--closeout`.

An executed closeout finishes as `closeout_executed_review_required`. It does not commit itself. A Phase closeout may retain the previous implementation version or set its assigned version; a correction must retain the unchanged version. Review the diff, evidence, version choice, and task disposition before committing the closeout through the authorized human/assistant workflow.

## Artifacts and limits

After preflight, a runner invocation writes ignored `.codex-runs/<folder>/<timestamp>/` artifacts:

- `run.json`: stack metadata, prompt states, timing, usage when available, failures, and checkpoint references.
- `P<number>.events.jsonl` and `P<number>.final.txt`: Codex event stream and final response.
- Retry-specific `.retry-<number>` event/final files and `P<number>.commit-message.txt` when a checkpoint is prepared.

Keep durable findings and required evidence linked from the task record before cleaning generated output. Ordinary Godot logs/exports remain under `artifacts/`.

The runner's success conditions concern process completion, version policy, and checkpoint integrity. It does not independently validate each claimed gameplay result, infer a failed Gate from prose such as `NOT GREEN`, or certify a playtest from a successful Codex exit. Prompts must request the actual checks; [validation](validation.md), task evidence, human review, and the [roadmap](roadmap.md) retain their existing authority.

Implementation lives in [the CLI](../scripts/fish-phase.mjs), [the core](../scripts/fish-phase-core.mjs), and [the validator](../scripts/validate-fish-phase.mjs). The port retains the source behavior rather than adding a Godot-specific runner engine.

## Port verification

The three runtime modules and two test suites were ported from the pinned Space Factory revision with project/CLI name changes and source-attribution comments. The runtime port does not add Godot-specific execution behavior. On 2026-11-09, `npm run test:runner` passed **106 tests with zero failures, skipped tests, or cancellations**, using Node.js `24.22.1`. The suites use mocked process calls and disposable Git repositories. They did not launch a live Codex implementation stack, Godot, or an MCP connection. Syntax checks passed for all three runtime modules, and a normalized comparison confirmed source parity after names and attribution changes. The authored p0 stack is validated separately by the repository setup; it remains unexecuted.
