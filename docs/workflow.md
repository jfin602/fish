# Fish Development Workflow

Adapted from Space Factory's task-first documentation and execution workflow. Start with [AGENTS.md](../AGENTS.md) and [BOOT.md](../BOOT.md), then use the narrow task routes. The slash commands here are assistant authoring conventions, not installed shell commands or game features.

## 1. Establish a bounded task

Identify the requested player/developer outcome, applicable Phase/Gate, acceptance criteria, exclusions, source revision, local changes, dependencies, affected state owners, and smallest meaningful checks. Keep those findings through interruptions. Use [MODULES](../MODULES.md) and [the documentation index](README.md) as routers, not a request to read everything.

A direct request to write, import, fix, or implement authorizes that bounded work. A small fix needs a proportionate plan. Substantial work benefits from assessment, plan, and executable prompts; a combined request can complete those stages together.

## 2. Documentation and decisions

`/docs-review` investigates current source and relevant decisions, then proposes concrete edits with affected files, reasons, and consequential open choices. Apply when the owner requests application; a direct documentation-write request already authorizes the scoped edits. `/docs-apply` can be conversational shorthand for applying a reviewed proposal, not a native Fish command.

Update each owning document when behavior or a decision changes. Preserve confirmed requirements and label working designs, examples, implemented state, and observed results distinctly. Record material decisions in [the register](decisions-and-questions.md); use [a detailed decision record](decisions/README.md) when warranted. Document review does not implement gameplay or pass Gates.

## 3. Defects and feature ideas

| Request | Action |
| --- | --- |
| `/issue <report>` | Add an observed defect to [known issues](../known-issues.md), with expected behavior, context, impact, evidence, and clearly labeled hypotheses. |
| `/feature <idea>` | Add a proposed enhancement to [feature ideas](../feature-ideas.md), with value, constraints, and real open questions. |
| `/resolve <ID>` | Record a dated, evidenced issue resolution while preserving its original report. |
| `/resolve +<ID>` | Mark a feature shipped only when implementation and evidence support it. |

Use a random unused four-character ID from `ABCDEFGHJKLMNPQRSTUVWXYZ23456789`; prepend `+` for ideas. Use `### ID — YYYY-MM-DD — Title`, never reuse IDs, and report the actual changed entry. A missing ID is a not-found result, not a new entry. Deferred/rejected/superseded work retains that disposition. Registries do not silently alter the roadmap or authorize implementation.

## 4. Assess, plan, and write

The sequence is `/prompt-ass` → `/prompt-plan` → `/prompt-write <folder>`.

### /prompt-ass

Define the outcome, preserved behavior, dependencies, risks, work order, deferrals, acceptance evidence, and each prompt's validation tier. Keep ordinary prompts focused on one coherent responsibility.

### /prompt-plan

Inspect actual source and commands. Trace only the relevant simulation, scene, input, persistence, or tooling boundaries. Name likely files, failure cases, checks, and any required native observation. Phase design belongs in [planning](planning/README.md); task-specific assessment and implementation plans live with [the task stack](tasks/README.md).

### /prompt-write <folder>

Create contiguous P1-based `.txt` execution briefs under `docs/tasks/<folder>/`. Include a goal, prerequisites, narrow readings, allowed scope, deferrals, implementation steps, validation tier, actual commands/scenarios, observable done condition, version rule, and final report. Follow [the runner's grammar](phase-runner.md) exactly.

Use the canonical `- Browser required: yes.` or `- Browser required: no.` metadata. The historical field also means native Godot/MCP/manual observation. `yes` pauses the runner before execution. It does not imply that Fish is a browser game or that ordinary browser tools can control Godot. One final closeout must be identified in both its filename and TASK title.

Use the supported `GPT-6 Sol Medium`, `GPT-6 Sol High`, or `GPT-6 Sol XHigh` recommendations according to the task and owner configuration. These select a spawned Codex process; they do not change an already active session. Preserve the runner's legacy mappings without silently switching models.

Before finishing, validate the stack with `npm run fish:phase:validate -- <folder>`. Keep ordinary prompts near the inherited 8-minute planning target, 10-minute soft ceiling, and 15-minute ordinary budget. Dedicated installation/integration/native qualification can need longer; record the reason. Time estimates never permit skipped checks or false completion. Split oversized independent work and preserve partial progress when a task overruns.

## 5. Execute and recover

A request to execute a selected stack authorizes its implementation without another activation step. Writing a stack does not execute it. Inspect current source and existing work before following an older plan. Continue compatible partial work and revise a materially invalid plan; do not discard local changes or create fictional checkpoints.

From the root, run `npm run fish:phase -- <folder>` for runner orchestration. [The runner guide](phase-runner.md) owns model selection, retries, metadata, Git-based resume, dirty-tree decisions, and manual boundaries. P0 starts at package `0.0.0`, with P1/P2/P3 assigned `0.0.1`/`0.0.2`/`0.0.3`. These versions describe development checkpoints, not gameplay release quality. Corrections keep the existing version.

During runner execution, the runner owns implementation checkpoint commits. A continuation accepting existing dirty work can use `git add -A`; inspect scope first. Do not create duplicate task commits from inside the spawned prompt. For direct interactive execution, follow each prompt's validation/version requirements, record direct execution, and leave final closeout changes reviewable; do not fabricate runner history. A later resume must reconcile that history before automatically rerunning tasks.

Use the Godot developer CLI after p0 creates it. Use MCP for the connected editor and real runtime observations, with a verified project identity. The Node runner coordinates work; it is not a game test harness. A client restart needed to load a new MCP configuration is a concrete handoff: save current progress and exact next action, and keep the missing live result unobserved.

Follow current publication authorization, scope commits, recheck remote HEAD before publication, and preserve concurrent work. Do not force a history rewrite to finish an import.

## 6. Validation tiers

| Tier | Purpose | Fish examples |
| --- | --- | --- |
| T1 — Focused | Prove the changed behavior and immediate invariants. | Documentation links/diffs, runner grammar, C# compilation, one real scene smoke, consuming food once. |
| T2 — Integration | Prove affected systems work together. | Growth updates body/camera, save resumes an encounter, death finalizes a journal/archive entry. |
| T3 — Qualification | Prove the claimed player/platform result. | Native swimming and threat readability, multi-session life followed by death/recap/history, platform-specific play/export. |

Select the lowest tier supporting the claim. Inspect command composition: a future `tools/dev check` must document its import/build/smoke work so callers avoid redundant component runs. Add meaningful coverage for risks such as double feeding, camera-driven simulation, resurrection after terminal death, duplicate/missing archive entries, and unsupported narrative claims when those behaviors exist.

Use native play for movement feel, vertical navigation, predator readability, menu focus, and recap/history usability. Escalate only for a changed shared contract, a concrete broader risk, or a required Gate. Once sufficiently verified, stop optional retesting.

## 7. Evidence, Gates, and closeout

Record source revision and relevant dirty-source identity, platform and tool versions, scenario/seed, actions, expected/observed outcomes, artifacts, and limitations. Distinguish prior evidence from checks actually rerun. Use **PASS**, **FAIL**, and **NOT OBSERVED**; use **BLOCKED** with its specific missing dependency.

Separate implementation, verification, and permission to progress. A compiled scene, screenshot, authored prompt, or accepted gap does not prove an unobserved player workflow. [The roadmap](roadmap.md) is the single Phase/Gate status authority. Pass a Gate only with its required evidence; pass a Phase only when all its Gates pass.

Keep detailed results under task `qualification/` records and a single `closeout.md`. Preserve failures and append repairs with their new evidence. Logs/exports live in ignored `artifacts/`, runner output in `.codex-runs/`; retain essential evidence with the task or at a stable referenced location before cleanup. Do not reference temporary screenshots as if they were durably available.

At closeout, reconcile affected roadmap results, task records, BOOT, MODULES, and changed owning docs once. Route the next bounded scope, with unresolved gaps explicit. Documentation reconciliation needs content/link/diff checks, not a new gameplay run on an unchanged candidate.

## Provenance

Ported on 2026-11-09 from [jfin602/space-factory at ce163753](https://github.com/jfin602/space-factory/tree/ce163753d8973ccd018f2344c6175c52fe4b113d), specifically its `AGENTS.md`, `BOOT.md`, `MODULES.md`, `docs/workflow.md`, planning/task/evidence conventions, and Node phase runner. Space Factory had adapted the workflow from Dope; [the runner guide](phase-runner.md) preserves code provenance. Space Factory gameplay, assets, historical Gates, and local machine results were not imported as Fish implementation or evidence.
