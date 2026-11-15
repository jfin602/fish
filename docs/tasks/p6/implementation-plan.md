# P6 implementation plan — The Living Pond

**Status: authored, NOT EXECUTED.** P6 has thirteen runner-compatible prompts including four manual/native pauses. Read the [assessment](prompt-assessment.md) and [approved S27 design](../../planning/p6/living-pond.md). [Roadmap](../../roadmap.md) owns G6.1–G6.8; [validation](../../validation.md) owns V44–V51. A task report and prompt file are **not** implementation.

## Entry and source/phase contract

From the actual local Fish checkout, read AGENTS.md then BOOT.md, docs/workflow.md, docs/phase-runner.md, and this task's README. Inspect `git status --short --branch`, `git log -n 5 --oneline`, `git rev-parse HEAD`, `package.json`, `tools/dev` commands and active Godot .NET project/editor. Preserve all compatible in-flight changes (especially C5 S25, camera smoothing, moving waterline, C5 wake and audio). Remote reviewed main at authoring: `22b4de15a7316971995cd921c3791d7f25354b27`, package `0.5.12`; local owner checkout may differ. Phase baseline `0.6.0` must be deliberate and clean/reviewed before runner entry. Never force a main branch push, auto-include unreviewed dirty changes or fabricate `0.5.13`.

For each prompt, preserve prior checkpoint evidence; check source and exact commands before altering them. Use C# for first-party gameplay and small Godot scene/UI changes. The runner owns implementation checkpoint commits at assigned versions; native manual checkpoints stop and require owner-controlled review. Run targeted `tools/dev` checks and `import`/`build` only when needed; avoid the broad `check` aggregate for every step. All new P6 diagnostics must be accessible via named `tools/dev inspect-...` subcommands with stable pass/fail markers, where practical; update check wrappers only with tests actually implemented.

## Source boundary map to verify (not a fixed patch list)

| Concern | Likely starting source | Proposed authority or responsibility |
| --- | --- | --- |
| Normal world entry | `WorldSelect.cs`, `CharacterSelect.cs`, `Main.cs`, `P2HabitatWorld.cs`, `WorldSession`, `PondCreatureCatalog` | Branch only **Pond Bass choice** into survival scene/session; preserve other Pond character art demos, Ocean, third Pike Encounter entry, original bakes and input focus |
| P6 session orchestrator | Existing `PikeEncounter.cs`, `PondGrowthState.cs`, `FeedingLedger.cs`, `PondFoodActor` and scene/entourage helpers | Reuse/extract small simulation services for a **normal Pond** Bass lifecycle; no direct fixture instance copy, no duplicate feeding ledger or enemy registry |
| Reticle | `FishController.cs`, `FollowCamera.cs`, shared HUD/shell in `PikeEncounter.cs`, `PlayShell.cs` | Centered visual control HUD; separate view projection of real fish mouth forward based on fitted body and current camera; actual feed event owns success pulse |
| Food habitat | `SeabedSurface.cs`, `GeneratedBasin`, Pond baked manifest/route fields, `PondFafEnvironment`, `FafAmbientPondLife`, `PondFoodActor` | Spawn accessible deterministic patches by shore/depth/cover; stable IDs, session-local depletion and bounded replenishment using game time |
| Predation and prey | `WorldSightSensor.cs`, `MovementDisturbanceField.cs`, `PikeAwareness.cs`, `PikeActor.cs`, `PreyAwareness.cs`, `PreyActor.cs`, `BluegillSchool.cs` | Reuse actual sensing/cover, pursuit and one-time contacts; generalized species-size-diet contract and bounded target selection for NPC vs NPC |
| Frog and snake | `PondCreatureCatalog.cs`, `FafAmbientPondLife.cs`, creature pack 001 original GLBs/manifest | Existing Northern leopard frog/swim-hop and Northern water snake/shore-surface-underwater clips; new *gameplay actors* without duplication by ambient presentation |
| Other NPC Bass | Registered five `FAF-FISH-007` stage GLBs, `PreyActor`/movement and shared feeding eligibility | Real independent Bass IDs and stage/size, solitary behaviors, true cannibalism only when eligible |
| Depth exploration/Pike | `PikeCoverFixture.cs`, Pond baked route/depth/collision fields, ambient placement and perception services | Populate meaningful ordinary Pond regions non-destructively, place reachable Pike in appropriate cover; no script-only stage lock |

Do not assume exact class names or paths when local source differs; discover ownership first, then keep implementation modular rather than forcing the listed guesses.

## Ordered integration milestones and focused tests

### P1 — Reticle (G6.1/V44, T2)
Create a small canvas HUD overlay and a separate projected forward-mouth cue (especially on orbit/free-look). Use exact current camera and stage-relative mouth state; handle camera behind fish, offscreen headings, viewport, bright surface, low visibility, pause/focus/hatch and every control preset. Real legal feed event yields one pulse. **Test** deterministic projection divergence/recenter, body-scale changes, camera snapshot alignment, no altered physics/feeding/cues. Add `inspect-p6-reticle` only after implementing a matching check. Preserve world controls and UI focus.

### P2 — Native reticle (T3 manual STOP)
Run targeted integration preflight, launch native Forward+ Fish window and leave it open. Owner swims/feeds in the existing **Pike Encounter** for direct small-food aiming review, comparing normal follow and two independent-look presets, sizes, view zoom and surface crossing. Record actual HUD comfort and false positives. No G6.1 PASS claimed for *normal Pond* before P3 integration.

### P3 — Normal Pond Bass entry (T2)
Preserve World Select, habitat filtering and normal Pond characters; route only selected Bass to actual hatch/nest and stage-accurate session. Share C5 growth, feeding/boost, camera, cutscene and collision services without instantiating the entire `PikeEncounter`. Distinguish ephemeral reset from durable save. **Test** valid/invalid selection, hatch/skip/retry, unchanged Ocean/Pond other species/fixture, stable baked manifest and no duplicated ambient wild animals. Add `inspect-p6-pond-entry` after check creation.

### P4 — Habitat food (G6.2/V45, T2)
Introduce stable patches with per-region suitability using real world terrain and cover data; bounded initial food, consumed flags, controlled replenishment/migration game-time and alternate safe routes. Grow through all stages with legit reward. **Test** deterministic seed/candidate placement, accessible fish/camera path, repeat contact, depletion/recovery, paused time, no soft lock and not endless nursery progression. `inspect-p6-food` only when implemented.

### P5 — Adult Bluegill first threat (G6.3/V46, T2)
Extend individual Bluegill to detect/pursue **tiny size-eligible Bass**, not arbitrary larger juveniles. Compare solid vs soft cover and open-water speed; finite last-known search and genuine physical attack/catch. Preserve genuine Bluegill prey when Bass becomes large enough. **Test** legitimate small-fry catch, protected escape, oversize Bass rejection, target switch when Bass not observable, no accidental all-pond hostility. `inspect-p6-bluegill-threat` if implemented.

### P6 — Frog (part G6.4/V47, T2)
Use registered leopard frog in bank/surface placements, correct `IdleShore`, `SurfaceFloat`, `Swim`, `Hop` and `Dive` as real gameplay movement presentation, with actual physical root/collision. Make only rare feasible very-small-fish attacks. **Test** shore/surface transitions, occlusion, contact, inaccessible fish, sensory timeout, no infinite underwater hunting, pause and scene cleanup. `inspect-p6-frog` if implemented.

### P7 — Snake (rest G6.4/V47, T2)
Reuse water snake model with `ShoreCrawl`, `SurfaceSwim`, `UnderwaterSwim`, `Turn`, `Dive`. Route near reeds, shoreline and shallow water; physically committed strikes, sensible size limits and finite disengagement. **Test** snake root follows actual positions, true mouth contact, moving surface changes, obstacle avoidance, no through-log pursuit, proper frog/Bluegill coexistence and bounded cost. `inspect-p6-snake` if implemented.

### P8 — Native shallow gameplay (T3 manual STOP)
After fresh build/preflight, open the game for owner and record controlled early Bass experiences: visible/hidden adult Bluegill, frog and snake shoreline roles, food distribution, reticle comfort, hiding/retry and stable performance. This is a **real stop** even if headless passes. If fairness/visibility fails, keep G6.2–G6.4 NOT GREEN and repair before continuing.

### P9 — Solitary Bass competitors (G6.5/V48, T2)
Spawn bounded staged individual Bass actors, not school instances. Use real cover, forage, sight/search and size/diet eligibility for independent cannibalistic attacks. **Test** younger Bass hiding, larger Bass legitimate catch, immunity of oversized fish, no scene/identity/food double removal, individual navigation/paused state.

### P10 — Multi-species NPC ecosystem (G6.6/V49, T2)
Generalize prey interfaces, target selection and one-time actual predator-vs-NPC feeding across Pike, Bluegill, Bass, frog and snake. Keep perception O(bounded nearby candidates), not all-to-all per frame, with deterministic selection/locks and no owner-camera dependence. **Test** player-offscreen predation, simultaneous claimant collision, deleted prey cleanup, pause/retry, native-capable cost instrumentation. No performance PASS from headless alone.

### P11 — Natural exploration/Pike (G6.7/V50, T2)
Place/patrol predators and food by connected nursery/reeds/midwater/deep vegetation and actual world routes with realistic Pike risk; prey scarcity should motivate movement without absolute age gates or infinite famine. **Test** an early deep excursion is physically possible/risky, older Bass can return shallow, cover/camera clearance changes, route can recover from depleted patches, seed-reproducible habitat populations, Ocean and fixture still independent.

### P12 — Full native Bass life (G6.8/V51, T3 manual STOP)
Before launch author a numbered owner walkthrough under `docs/tasks/p6/qualification/` (do not prefill PASS). Include all G6.1–G6.8/V44–V51 and historic C5/P5 debt distinction. Run needed focused tests, then **`./tools/dev run --display-mode=windowed-fullscreen`** in the real desktop and **leave the window open**. Owner plays an ordinary Pond Bass continuously hatch→adult, with genuine prey, threats, exploration and deeper Pike; record observation and frame pacing. Headless passes or game launch alone cannot pass T3.

### P13 — Closeout (manual STOP)
Review all P1–P12 source, tests, owner T3 reports, pending visual/fairness problems, baked world and two-world isolation. Update `docs/tasks/p6/closeout.md`, roadmap, BOOT, MODULES and impacted system contracts strictly according to observed evidence; label PASS/FAIL/NOT OBSERVED/BLOCKED and permit owner-closed progression if requested without fabricating GREEN. This closeout leaves changes **uncommitted for owner review**. The assigned `0.6.13` is only applied when actual native completion/owner approval supports it; otherwise retain the last proven checkpoint.

## Verification and history rules

T1 covers changed code/build/direct contract tests; T2 covers actor/feeding/perception/scene isolation; T3 demands genuine native owner testing. Each prompt reports relevant source SHAs, world seed, actual tests, pass markers/failed conditions, performance observations (not made up) and follow-up. A failed/manual step cannot be silently skipped; prompt stack execution resumes only with a legitimate checkpoint. Never rewrite old P5/C5 evidence, create duplicate world identities, enable normal Ocean predators or implement P7/P8/P9 functionality as hidden P6 scope.
