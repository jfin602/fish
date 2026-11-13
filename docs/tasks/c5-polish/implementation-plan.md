# C5 manual implementation plan

**Status:** authored plan only; separate M1–M8 manual prompts. Read [assessment](prompt-assessment.md), [owner design](../../planning/c5-post-p5-ecology-lifecycle.md), and [original P5 closeout](../p5/closeout.md). Root `0.5.12` remains unchanged; do not invoke the phase runner or assign arbitrary correction versions.

## Source owners / implementation order

| Step | Source and behavioral responsibility | Verification |
| --- | --- | --- |
| **M1** | `PikeEncounter`, `PondGrowthState`, `FeedingLedger`, actor/asset eligibility: fixed **Bass player**, **Bluegill-only fish prey** | Actual species IDs, juvenile proof baseline, normal route regression |
| **M2** | New egg/hatching presentation and tiny larval Bass movement using `PondCreatureCatalog` 5 real stages, `PlayerArt`, `FollowCamera`, `FishController` | Hatch/skip same authoritative state and safe camera/input |
| **M3** | Early invertebrate nutrition → size-eligible Bluegill consumption; 5 stages, real collider/camera/cover implications | Exactly-once rewards, no too-large prey, stage/progression |
| **M4** | `PreyActor`/new bounded school behavior for staged Bluegill; shallow nursery/intermediate/deeper routes | Cohesion, separation, scatter/regroup, size/age and performance |
| **M5** | `PikeActor/PikeAwareness` multi-target evidence and hysteresis; Pike catches Bluegill or Bass | Real individual target IDs, commitment, both possible player escape outcomes |
| **M6** | Pure presentation blood event from genuine fish bite/damage/consumption; lifespan/FX budget | Once-only, fade/diffuse, pause/retry and native performance |
| **M7** | Cross-system regression, clean normal Ocean/Pond routing/scene cleanup and owner test checklist | T2 source results, no new native claims |
| **M8** | Owner-native scene launch, test hatching/food/schools/Pike/hiding/blood and original P5 uncovered matrix | **T3: launch actual Fish window and let owner play**, record feedback later |

## Execution safety

Manual prompts are located at `manual/M1-...` through `manual/M8-...`, with their exact file names in [manual README](manual/README.md). These are not runner parser tasks. Execute **one per interactive Codex session**; each ends with a STOP, review the diff and evidence before the next. The final M8 brief is explicit: **the last action, after all preflight, is to launch `./tools/dev run --display-mode=windowed-fullscreen`** using real display, and leave Fish open for the owner.

Avoid rebuilding the Pond generator, swapping every habitat's fish or adding save/death. Use actual checks defined in `tools/dev`, and implement new focused checks before calling them. If Codex cannot access a graphical Godot desktop, report that as BLOCKED rather than pretending it launched Fish.

### P5 qualification debt and C5 additions

Retest **G5.1/V03–V04** food/growth and cover clearance, **G5.2/V05/V33–V34/V36** Pike contact, concealment, escape and third-route isolation, **G5.3/V35** slow-cover approach vs exposed Bluegill approach. Extend with **V37** hatching, **V38** diet/growth, **V39** schools, **V40** multi-target Pike, **V41** event-driven blood. Record original P5 historical results separately; version/commit name does not certify success. Only the owner's real play/observations can convert relevant NOT OBSERVED T3 cells to PASS.
