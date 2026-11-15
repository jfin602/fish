# P6 — The Living Pond

**Status: ASSESSMENT AND PLAN AUTHORED / EXECUTION PROMPTS NOT YET WRITTEN.** S27 owner-approved a complete Bass egg-to-adult **normal Freshwater Pond** survival progression before persistence. The task table below is the planned stack, **not** implementation, qualification or a Gate PASS. The separate `0.6.0` [development baseline](baseline.md) is established. P5 remains owner-closed for progression / NOT GREEN with historic G5.1–G5.3 debt. C5 M1–M7 source/evidence and manual M8 remain distinct. [Approved design](../../planning/p6/living-pond.md) · [assessment](prompt-assessment.md) · [implementation plan](implementation-plan.md) · [roadmap](../../roadmap.md) · [validation V44–V51](../../validation.md) · [runner](../../phase-runner.md).

## Intended outcome

A playable Bass hatches in an ordinary, independent Pond and learns to aim/auto-feed using a truthful reticle, must forage beyond the nest under threat from adult Bluegill, a Northern leopard frog and Northern water snakes, gradually hunts smaller Bluegill and competes with larger solitary Bass, then enters deeper Pike cover. Food patches deplete/recover; predators can genuinely catch NPCs without requiring player proximity. All animals obey world-space sensing, real stage/size/diet and once-only feeding/catch. Existing **Pike Encounter** remains a third World Select regression destination, **not** a habitat; normal **Ocean** and other Pond species demos remain intact. P6 catch/retry is session-local. P7 save/durable death, P8 Life History and P9 integration remain deferred.

## Entry and version discipline

1. Start with AGENTS.md, BOOT.md and docs/workflow.md; inspect branch/HEAD, remote revision, local dirty tree, installed project identity, actual C5/S25 work and runner state. **Do not use GitHub's authored status as a substitute for checking the local working tree.** Preserve the owner's camera, wake, waterline, audio, SFX and any in-flight code. Do not reset/rebase/force-push or silently include unrelated dirty work.
2. Root `package.json` was **`0.5.12` at authoring** (GitHub main reviewed at `22b4de15a7316971995cd921c3791d7f25354b27`). Although earlier P5 commits have a `0.5.13` subject, no package `0.5.13` is claimed. The separate reviewed **`0.6.0` Phase 6 baseline** is now recorded in [baseline.md](baseline.md). The runner did not create that baseline; authoring these plans did not change the version.
3. P1–P13 each target the exact assigned `0.6.N` root version. Runner owns the checkpoint commit of successful implementation tasks; no spawned prompt may self-commit or self-push. Every manual prompt uses `- Browser required: yes.` and will stop the runner before native execution. Only a human-reviewed, completed manual step with actual evidence and exact checkpoint/version may permit resume. The final closeout remains uncommitted for owner review.
4. Verify actual `tools/dev` command composition before running targeted checks. Write and validate the P1–P13 execution prompts before using the phase runner; the current plan alone is not a valid prompt stack. Native Forward+ owner play, rather than headless state checks or still images, is required for the T3 Gates. Any red native checkpoint is a **STOP** requiring bounded repair/owner review, not an invitation to continue silently.

## Planned ordered tasks — prompt files pending

| Prompt | Responsibility | Assigned version | Tier / handoff |
| --- | --- | --- | --- |
| P1 | Centered view reticle, honest projected mouth heading, genuine feeding pulse | `0.6.1` | T2 |
| P2 | Owner-native aim and camera-preset review | `0.6.2` | **T3 manual STOP** |
| P3 | Bass survival route in normal Pond; fixture isolation and hatch continuity | `0.6.3` | T2 |
| P4 | Habitat-based food, depletion and bounded renewal | `0.6.4` | T2 |
| P5 | Adult Bluegill as size-eligible early predator | `0.6.5` | T2 |
| P6 | Frog bank/surface behavior and plausibly limited tiny-fry hunting | `0.6.6` | T2 |
| P7 | Snake shoreline/water locomotion and sensed predation | `0.6.7` | T2 |
| P8 | Owner-native fry/juvenile danger, food routes, cover and fairness | `0.6.8` | **T3 manual STOP** |
| P9 | Age-diverse independent Bass and credible cannibalism | `0.6.9` | T2 |
| P10 | Shared bounded NPC-vs-NPC detection, choices, consumption | `0.6.10` | T2 |
| P11 | Natural spatial progression and deeper Pike risk | `0.6.11` | T2 |
| P12 | Owner-native whole-life and Ocean/fixture regression V44–V51 | `0.6.12` | **T3 manual STOP** |
| P13 | Evidence-backed Phase 6 closeout | `0.6.13` | **T3 manual STOP** |

## Scope, evidence and stop conditions

The existing **Northern leopard frog** `FAF-CREATURE-001` and **Northern water snake** `FAF-CREATURE-002` already have Blender sources, verified GLBs and animated roles. They do **not** yet have AI/feeding/damage integration; P6 uses their existing FAF sources without inventing a new bullfrog. The `FAF-CREATURE-003` snapping turtle is optional and **not** required for P6. Fish stage assets already exist; do not rebuild their Blender masters without owner direction.

Inspect current code rather than assuming old `MODULES.md` descriptions of Pike Encounter, C5, normal Pond, `P2HabitatWorld`, `WorldSelect`, `CharacterSelect`, `FeedingLedger`, `PondGrowthState`, `FollowCamera`, `FafAmbientPondLife` or `tools/dev` are current. Share authoritative prey/feeding/sensing rules; never make the reticle, sound, blood or fog presentation authoritative. Do not regenerate or overwrite pinned Ocean/Pond Base Worlds as a shortcut. New assets, any base-world promotion, new production species and any change to saved-life architecture are out of scope.

Task evidence belongs in `docs/tasks/p6/qualification/` **only after the task is run**. Record source/dirty tree, engine/renderer/GPU, seed/manifest/actor identity, real commands, actual PASS/FAIL/NOT OBSERVED/BLOCKED, owner observation, screenshots/traces if captured, performance and unobserved limitations. Do not create prefilled “PASS” reports. The roadmap owns all Gate statuses; compiling a fixture does not make G6.1–G6.8 GREEN.

## Validate and execute after prompt authoring

```bash
npm run fish:phase:validate -- p6
# Only after the P1–P13 prompts are written, validated and reviewed:
npm run fish:phase -- p6
```

The final native P12 prompt must leave a real game window open for the owner after writing its playthrough checklist. P13 reconciles evidence and Gate dispositions, without inventing a full PASS or choosing a version ahead of owner approval.
