# c3-visibility — underwater discovery fog correction

**State:** P1–P3 IMPLEMENTED; P4 NATIVE REVIEW PARTIAL / V25 NOT QUALIFIED. See [closeout](closeout.md) and [native matrix](qualification/native-visibility.md).
**Type:** Correction 3; initial authoring baseline: main at 9e858110a6ec571e964d72e26ef1de57bfdad36b (before this folder) and root 0.3.0.  
**Version:** Every prompt requires **unchanged 0.3.6**, as explicitly selected by the owner. This is a strict runner preflight, not an instruction to change or reset a local checkout.

## Purpose

Implement the [owner-selected underwater visibility and discovery contract](../../systems/underwater-visibility.md): nearby colorful reef remains clear, progressively distant geometry softens into blue-green water, and opposite playable boundaries are not recognizable from ordinary underwater views. Deeper camera positions shorten useful visibility, while water surface/caustics/shafts and near-field navigation remain comfortable.

The existing Forward+ UnderwaterAtmosphere, LightingSettings, LightingPresetStore, GraphicsPanel, WaterColumnChecks and game/dev probes are the implementation owners. Do not replace the renderer or redesign the accepted fish motor/camera, P3 world art or Character Select.

Read [assessment](prompt-assessment.md), [implementation plan](implementation-plan.md), [P3 reef plan](../../planning/p3/reef-world-rebuild.md), [V25](../../validation.md), and [runner grammar](../../phase-runner.md).

## Conflict-safe entry with P3

**P3 owns versions 0.3.1–0.3.8.** The owner changed this correction's execution version to **0.3.6** (P3's P6 checkpoint). The original GitHub authoring baseline was 0.3.0, and the owner's local P3 implementation/checkpoint state is not verifiable through GitHub alone. Run c3 only after confirming P3 P1–P6 are actually completed, the local root package version is 0.3.6, and c3-world-gen has been completed/reconciled first; do not treat the version number alone as evidence that P6 is complete. Do not execute this correction concurrently with P3 or edit prompts beneath a running runner. If package.json differs from 0.3.6, **stop** and reconcile the actual completed P3 prefix before execution. Never downgrade package.json, fake P3 checkpoints, reset local work or force a dirty continuation.

P1–P3 were implemented at the verified `0.3.6` checkpoint. Existing P2 owner-green Forward+ water/light/depth results are preserved. P2 V18/V19 PASS stays historical; P2 V20/G2.3 remains OPEN. The P4 native record leaves V25 not qualified.

## Prompt order

| Step | Prompt | Outcome | Tier / manual |
| --- | --- | --- | --- |
| P1 | [Depth-aware visibility extinction](P1-distance-extinction.txt) | Implemented; T2 checks in [evidence](qualification/implementation.md) | T2 |
| P2 | [Visibility GFX and preset compatibility](P2-visibility-gfx-presets.txt) | Implemented; isolated migration fixture passed | T2 |
| P3 | [Visibility integration and diagnostics](P3-integration-and-probes.txt) | Implemented; native A/B probe subsequently measured | T2 |
| P4 | [Native visibility closeout](P4-native-visibility-closeout.txt) | Partial playable review; collision clearance failed and several V25 criteria remain unobserved | T3, [closeout](closeout.md) |

P4 is the sole final closeout and uses the runner's canonical Browser required: yes. field for a *native Godot/manual* handoff; this does not mean a web browser can test gameplay. Runner-owned implementation checkpoints must not self-commit. Closeout remains reviewable.

## Acceptance and evidence

Native V25 must show: opposing boundaries concealed in at least two unobstructed directions, gradually emerging/receding reef landmarks, shallower longer and deeper shorter useful visibility, near obstacle/cave/player-fish readability, accepted overhead surface/caustics/shafts/UI, safe saved GFX presets and representative same-scene performance. The numeric targets in the contract are **starting art ranges**, not artificial clipping planes or achieved facts.

Evidence is in [P1–P3 implementation](qualification/implementation.md), [P4 native review](qualification/native-visibility.md) and [closeout](closeout.md). Do not mark an original P3 Gate or overall Phase green solely because c3's source checks or cost probe pass.

## Commands

From the Fish repository root after reviewing local P3 version/checkpoints:

```bash
npm run fish:phase:validate -- c3-visibility
npm run fish:phase -- c3-visibility
```

The first command validates prompt metadata, numbering and version rules; it does not run Godot or Codex. The second executes pending implementation prompts and stops at the native P4 handoff. **Writing this stack does not execute it.**
