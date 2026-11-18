# P6 — The Living Pond

**Status: P1, P3, P4, P5, P6 and P7 IMPLEMENTED / T2 CANDIDATES; P2 owner-closed for progression / NOT GREEN; P8 owner-native review next.** The owner found that larval Bass can keep the reticle lined up with tiny food and still miss; [P2 evidence and R1 addendum](qualification/p2-reticle-native.md) records the observation, measured fixture reproduction, repair, focused checks and explicit decision to defer further repair and native retry until the end of the runner phases. The current R1 source candidate is retained; G6.1/V44 has no native acceptance. S27 owner-approved a complete Bass egg-to-adult **normal Freshwater Pond** survival progression before persistence. The thirteen-brief P6 stack validates; P1, P2 R1, P3, P4, P5, P6 and P7 have focused implementation and evidence, not a Gate PASS. [P3 normal Pond evidence](qualification/p3-pond-entry.md) records the entry route; [P4 food evidence](qualification/p4-food.md) records patches and renewal with native limits. [P5 Bluegill threat evidence](qualification/p5-bluegill-threat.md) records adult pursuit, cover and contact checks with native V46 still open. [P6 frog evidence](qualification/p6-frog.md) and [P7 snake evidence](qualification/p7-snake.md) record physical shoreline actors, rig captures and focused checks with native V47 still open. The separate `0.6.0` [development baseline](baseline.md) remains historical; root `0.6.7` is the uncommitted P7 implementation checkpoint; `0.6.2` is the owner-authorized P2 manual checkpoint. P5 remains owner-closed for progression / NOT GREEN with historic G5.1–G5.3 debt. C5 M1–M7 source/evidence and manual M8 remain distinct. [Approved design](../../planning/p6/living-pond.md) · [assessment](prompt-assessment.md) · [implementation plan](implementation-plan.md) · [roadmap](../../roadmap.md) · [validation V44–V51](../../validation.md) · [runner](../../phase-runner.md).

## Intended outcome

A playable Bass hatches in an ordinary, independent Pond and learns to aim/auto-feed using a truthful reticle, must forage beyond the nest under threat from adult Bluegill, a Northern leopard frog and Northern water snakes, gradually hunts smaller Bluegill and competes with larger solitary Bass, then enters deeper Pike cover. Food patches deplete/recover; predators can genuinely catch NPCs without requiring player proximity. All animals obey world-space sensing, real stage/size/diet and once-only feeding/catch. Existing **Pike Encounter** remains a third World Select regression destination, **not** a habitat; normal **Ocean** and other Pond species demos remain intact. P6 catch/retry is session-local. P7 save/durable death, P8 Life History and P9 integration remain deferred.

## Entry and version discipline

1. Start with AGENTS.md, BOOT.md and docs/workflow.md; inspect branch/HEAD, remote revision, local dirty tree, installed project identity, actual C5/S25 work and runner state. **Do not use GitHub's authored status as a substitute for checking the local working tree.** Preserve the owner's camera, wake, waterline, audio, SFX and any in-flight code. Do not reset/rebase/force-push or silently include unrelated dirty work.
2. Root `package.json` was **`0.5.12` at authoring** (GitHub main reviewed at `22b4de15a7316971995cd921c3791d7f25354b27`). Although earlier P5 commits have a `0.5.13` subject, no package `0.5.13` is claimed. The separate reviewed **`0.6.0` Phase 6 baseline** is now recorded in [baseline.md](baseline.md). The runner did not create that baseline; authoring these plans did not change the version.
3. P1–P13 each target the exact assigned `0.6.N` root version. Runner owns automatic implementation checkpoints; the owner directly authorized the exceptional P2 manual `0.6.2` checkpoint with its native aim gap recorded for later review. No spawned prompt may self-commit or self-push. Every manual prompt uses `- Browser required: yes.` and will stop the runner before native execution. Only a human-reviewed manual disposition with actual evidence and exact checkpoint/version may permit resume. The final closeout remains uncommitted for owner review.
4. Verify actual `tools/dev` command composition before running targeted checks. The P1–P13 execution prompts are authored and grammar-validated; an explicit implementation request is still needed to run the phase stack. Native Forward+ owner play, rather than headless state checks or still images, is required for the T3 Gates. A red native checkpoint requires an explicit owner disposition before progression; P2's deferred gap is recorded above and must return at P12/P13.

## Required Fish editor bridge for every P6 brief

The Godot Editor MCP bridge is an execution prerequisite, including for C#-only tasks. Check it **before source edits, validation, native review or version/checkpoint work**. A running MCP server, an open Godot process, `tools/dev doctor`, an import or a headless build alone does not prove that the server is connected to the correct live editor.

1. Inspect active Godot MCP server processes, bridge listeners and editor sessions, including their PID, port, project path and configuration. Close stale or conflicting **Fish-owned** MCP bridges and editors so this task has one Fish bridge. If a port belongs to another project, leave that active session alone and choose a verified unused Fish port. Preserve the Space Factory/global configuration; do not kill an unidentified process or overwrite another project's connection.
2. Start this task's own Fish MCP server from the trusted Fish project-local `.codex/config.toml` with `GODOT_MCP_PROJECT_DIR` set to the absolute path of this checkout's `game/` and a matching `GODOT_MCP_BRIDGE_URL`. Start the Godot .NET editor for that same project with `./tools/dev editor` and the same bridge URL from ignored `tools/local.env`. If the client needs a reload to pick up the local MCP override, reload it and resume only after the live checks pass. See [Godot Editor MCP setup](../../development.md#godot-editor-mcp) for the pinned command and isolation details.
3. Call `godot_health_check` and `godot_get_server_info` through the task's MCP connection. Require a connected editor bridge and verify the returned live editor project path resolves to this checkout's `game/`; check server/addon versions and .NET project identity. Record the observed result in task evidence. Recheck after a restart or connection drop before continuing.
4. If the Fish editor bridge remains disconnected, points to a different project, or drops during the task, **STOP and report BLOCKED** with the observed process/listener/health details and recovery attempted. Do not proceed with disk-only C# changes, `tools/dev` checks, a version bump, or a runner checkpoint. Resume only after the live Fish bridge is verified.

## Ordered tasks and checkpoints

| Prompt | Responsibility | Assigned version | Tier / handoff |
| --- | --- | --- | --- |
| [P1](P1-feeding-reticle-and-mouth-heading.txt) | Centered view reticle, honest projected mouth heading, genuine feeding pulse | `0.6.1` | T2 |
| [P2](P2-native-reticle-aim-review.txt) | Owner-native aim and camera-preset review | `0.6.2` | **T3 manual STOP** |
| [P3](P3-normal-pond-bass-survival-entry.txt) | Bass survival route in normal Pond; fixture isolation and hatch continuity | `0.6.3` | T2 |
| [P4](P4-distributed-food-and-patch-recovery.txt) | Habitat-based food, depletion and bounded renewal | `0.6.4` | T2 |
| [P5](P5-early-bluegill-predation.txt) | Adult Bluegill as size-eligible early predator | `0.6.5` | T2 |
| [P6](P6-northern-leopard-frog-ecology.txt) | Frog bank/surface behavior and plausibly limited tiny-fry hunting | `0.6.6` | T2 |
| [P7](P7-northern-water-snake-ecology.txt) | Snake shoreline/water locomotion and sensed predation | `0.6.7` | T2 |
| [P8](P8-native-early-survival-review.txt) | Owner-native fry/juvenile danger, food routes, cover and fairness | `0.6.8` | **T3 manual STOP** |
| [P9](P9-solitary-bass-competitors.txt) | Age-diverse independent Bass and credible cannibalism | `0.6.9` | T2 |
| [P10](P10-independent-multispecies-ecology.txt) | Shared bounded NPC-vs-NPC detection, choices, consumption | `0.6.10` | T2 |
| [P11](P11-habitat-exploration-and-deep-pike.txt) | Natural spatial progression and deeper Pike risk | `0.6.11` | T2 |
| [P12](P12-native-complete-bass-life-review.txt) | Owner-native whole-life and Ocean/fixture regression V44–V51 | `0.6.12` | **T3 manual STOP** |
| [P13](P13-phase-6-closeout.txt) | Evidence-backed Phase 6 closeout | `0.6.13` | **T3 manual STOP** |

## Scope, evidence and stop conditions

The existing **Northern leopard frog** `FAF-CREATURE-001` and **Northern water snake** `FAF-CREATURE-002` already have Blender sources, verified GLBs and animated roles. The frog now has scene-scoped normal Pond Bass AI and limited fry contact in [P6 evidence](qualification/p6-frog.md); the snake now has separate physical shallow hunter behavior in [P7 evidence](qualification/p7-snake.md). P6 uses the existing FAF sources without inventing a new bullfrog. The `FAF-CREATURE-003` snapping turtle is optional and **not** required for P6. Fish stage assets already exist; do not rebuild their Blender masters without owner direction.

Inspect current code rather than assuming old `MODULES.md` descriptions of Pike Encounter, C5, normal Pond, `P2HabitatWorld`, `WorldSelect`, `CharacterSelect`, `FeedingLedger`, `PondGrowthState`, `FollowCamera`, `FafAmbientPondLife` or `tools/dev` are current. Share authoritative prey/feeding/sensing rules; never make the reticle, sound, blood or fog presentation authoritative. Do not regenerate or overwrite pinned Ocean/Pond Base Worlds as a shortcut. New assets, any base-world promotion, new production species and any change to saved-life architecture are out of scope.

Task evidence belongs in `docs/tasks/p6/qualification/` **only after the task is run**. Record source/dirty tree, engine/renderer/GPU, seed/manifest/actor identity, real commands, actual PASS/FAIL/NOT OBSERVED/BLOCKED, owner observation, screenshots/traces if captured, performance and unobserved limitations. Do not create prefilled “PASS” reports. The roadmap owns all Gate statuses; compiling a fixture does not make G6.1–G6.8 GREEN.

## Validate and execute when P6 implementation is selected

```bash
npm run fish:phase:validate -- p6
# Only for an explicit P6 implementation request:
npm run fish:phase -- p6
```

The final native P12 prompt must leave a real game window open for the owner after writing its playthrough checklist. P13 reconciles evidence and Gate dispositions, without inventing a full PASS or choosing a version ahead of owner approval.
