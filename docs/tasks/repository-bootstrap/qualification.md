# Repository bootstrap record

Date: 2026-11-09. Scope: documentation, workflow tooling and authored setup prompts for `jfin602/fish`.

## Source and target

- Inspected reference: [jfin602/space-factory at ce163753d8973ccd018f2344c6175c52fe4b113d](https://github.com/jfin602/space-factory/tree/ce163753d8973ccd018f2344c6175c52fe4b113d).
- Initial Fish main: [2d99450b90999893a94c9dd1e8f89420ddea9e15](https://github.com/jfin602/fish/commit/2d99450b90999893a94c9dd1e8f89420ddea9e15), containing only the repository title README.
- Product source: the owner's project conversation, summarized with requirement provenance in [the decision register](../../decisions-and-questions.md).
- The source snapshot did not contain the later Godot Editor MCP integration. Its separate selected setup was verified against the pinned upstream release and official client documentation linked in [development setup](../../development.md); no installation or live Fish result is inferred.

## What was added

1. Task-first AGENTS routing, concise BOOT handoff, current/planned module map, documentation index, workflow, planning/task/evidence conventions and issue/feature registries.
2. Fish-specific vision, game/system contracts, UX, decisions and roadmap, including saved living runs, permanent death and all completed factual stories in main-menu Life History.
3. The source Node phase runner, parser/validator and tests adapted to Fish command/file names. Runtime behavior was preserved rather than redesigned.
4. An assessed and planned p0 stack: Godot/C# foundation, MCP configuration, then native closeout after any needed client reload.

## Checks

| Check | Actual result |
| --- | --- |
| Source inspection | PASS — reference router/workflow/development/architecture, prompt conventions, runner/runtime modules and tests inspected at the pinned revision. |
| Runner tests | PASS — `npm run test:runner`, 106 tests passed, zero failed/skipped/cancelled, Node.js 24.22.1. |
| Runner syntax | PASS — all three Node runtime/validator modules passed syntax checks. |
| Port fidelity | PASS — runtime modules and inherited tests match the source after project-name and attribution normalization. |
| P0 prompt parser | PASS — `npm run fish:phase:validate -- p0`; contiguous P1/P2 implementations at `0.0.1`/`0.0.2`, sole P3 native closeout at `0.0.3`, canonical manual metadata. |
| Documentation links/content/diff | PASS — 197 relative file/heading links resolve across 41 starter files; no trailing whitespace or missing final newlines; `git diff --check` passed. Independent product, MCP setup and runner-contract reviews found no blocking contradictions. |
| Local-data exclusion | PASS — Git ignores Fish's local Codex configuration, local executable settings, generated artifacts, runner output and Godot cache; the upstream addon/license paths remain eligible for tracking. |

The runner tests exercise development orchestration; they do not launch or qualify Fish gameplay. No real Codex implementation child, Godot editor, MCP bridge, game runtime or export was executed for this repository starter.

## Disposition and next action

Documentation/tooling starter checks passed. Game Phases remain NOT STARTED and gameplay Gates NOT OBSERVED. Execute [p0](../p0/README.md) on the intended development machine, preserve its native handoff, and collect actual setup evidence before updating the roadmap.
