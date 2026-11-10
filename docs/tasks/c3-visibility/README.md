# c3-visibility — underwater discovery fog correction

**State:** AUTHORED / NOT EXECUTED / NOT QUALIFIED.  
**Type:** Correction 3; initial authoring baseline: main at 9e858110a6ec571e964d72e26ef1de57bfdad36b (before this folder) and root 0.3.0.  
**Version:** Every prompt requires **unchanged 0.3.0**. This is a strict runner preflight, not an instruction to reset a newer local checkout.

## Purpose

Implement the [owner-selected underwater visibility and discovery contract](../../systems/underwater-visibility.md): nearby colorful reef remains clear, progressively distant geometry softens into blue-green water, and opposite playable boundaries are not recognizable from ordinary underwater views. Deeper camera positions shorten useful visibility, while water surface/caustics/shafts and near-field navigation remain comfortable.

The existing Forward+ UnderwaterAtmosphere, LightingSettings, LightingPresetStore, GraphicsPanel, WaterColumnChecks and game/dev probes are the implementation owners. Do not replace the renderer or redesign the accepted fish motor/camera, P3 world art or Character Select.

Read [assessment](prompt-assessment.md), [implementation plan](implementation-plan.md), [P3 reef plan](../../planning/p3/reef-world-rebuild.md), [V25](../../validation.md), and [runner grammar](../../phase-runner.md).

## Conflict-safe entry with P3

**P3 owns versions 0.3.1–0.3.8.** GitHub currently advertises 0.3.0, but the owner's local worktree and any in-progress runner are not observable from GitHub. Do not execute this correction concurrently with P3 or alter prompt files while the P3 runner is active. If actual package.json differs from 0.3.0, the correction **must stop** at runner preflight. Reconcile the verified P3 checkpoint/version and then revise c3's unchanged-version metadata across the stack under an explicit reviewed workflow. Never downgrade package.json, fake P3 checkpoints, reset local work or force a dirty continuation.

Use a separate clean checkpoint/worktree or an owner-accepted safe pause. Existing P2 owner-green Forward+ water/light/depth results are preserved. P2 V18/V19 PASS stays historical; P2 V20/G2.3 remains OPEN. Correction V25 is not yet implemented/qualified.

## Prompt order

| Step | Prompt | Outcome | Tier / manual |
| --- | --- | --- | --- |
| P1 | [Depth-aware visibility extinction](P1-distance-extinction.txt) | Stronger distance extinction, depth-tuned useful range, readable near field | T2, no pause |
| P2 | [Visibility GFX and preset compatibility](P2-visibility-gfx-presets.txt) | Visibility Distance (m), old custom preset compatibility and isolation | T2, no pause |
| P3 | [Visibility integration and diagnostics](P3-integration-and-probes.txt) | Actual world/UI integration, focused checks, matched A/B probe | T2, no pause |
| P4 | [Native visibility closeout](P4-native-visibility-closeout.txt) | Owner playable exploration test, visual acceptance and performance evidence | T3, **mandatory manual pause and final closeout** |

P4 is the sole final closeout and uses the runner's canonical Browser required: yes. field for a *native Godot/manual* handoff; this does not mean a web browser can test gameplay. Runner-owned implementation checkpoints must not self-commit. Closeout remains reviewable.

## Acceptance and evidence

Native V25 must show: opposing boundaries concealed in at least two unobstructed directions, gradually emerging/receding reef landmarks, shallower longer and deeper shorter useful visibility, near obstacle/cave/player-fish readability, accepted overhead surface/caustics/shafts/UI, safe saved GFX presets and representative same-scene performance. The numeric targets in the contract are **starting art ranges**, not artificial clipping planes or achieved facts.

Write evidence only when the task actually executes:
- qualification/implementation.md — P1–P3 source and focused check outcomes;
- qualification/native-visibility.md — P4 native screenshots, route, current settings and measured cost;
- closeout.md — passed/failed/unobserved items, actual owner decision and follow-up.

No files listed above exist as proof of completion at authoring time. Do not mark an original P3 Gate or overall Phase green solely because c3's tests compile.

## Commands

From the Fish repository root after reviewing local P3 version/checkpoints:

```bash
npm run fish:phase:validate -- c3-visibility
npm run fish:phase -- c3-visibility
```

The first command validates prompt metadata, numbering and version rules; it does not run Godot or Codex. The second executes pending implementation prompts and stops at the native P4 handoff. **Writing this stack does not execute it.**
