# C5 manual implementation plan

**Status:** M1 contact/escape corrections and revised M2 Bass hatch/fry passed focused headless checks. The owner reported defects in the first M2 native build; M1 owner native contact recheck and corrected M2 native review remain pending; Owner reports execution through M6; published GitHub may be behind local source/evidence. M2A was not separately executed and is superseded. M7/M8 and S25 implementation/qualification remain. Read [M1 evidence](qualification/m1-bass-player-and-bluegill-prey.md), [M2 evidence](qualification/m2-hatching-and-tiny-bass.md), [owner design](../../planning/c5-post-p5-ecology-lifecycle.md), and [original P5 closeout](../p5/closeout.md). Root `0.5.12` remains unchanged; do not invoke the phase runner or assign arbitrary correction versions.

## Source owners / implementation order

**2026-11-14 C5 sequencing update:** The owner reports completion through P6/M6. Current GitHub may not include unpushed local work; inspect local source and evidence before starting. Do not rerun M1–M6 or execute M2A retroactively. **Revised M7** implements S25 automatic mouth feeding and limited Spacebar boost, plus T2 integration preflight, directly against existing C5 food/schools/Pike/blood; **M8** provides owner-native T3 qualification. Prior M2A is superseded planning only; S25 is NOT yet claimed implemented.


| Step | Source and behavioral responsibility | Verification |
| --- | --- | --- |
| **M1** | `PikeEncounter`, `PondGrowthState`, `FeedingLedger`, actor/asset eligibility: fixed **Bass player**, **Bluegill-only fish prey** | Actual species IDs, juvenile proof baseline, normal route regression |
| **M2** | New egg/hatching presentation and tiny larval Bass movement using `PondCreatureCatalog` 5 real stages, `PlayerArt`, `FollowCamera`, `FishController` | Hatch/skip same authoritative state and safe camera/input |
| **M2A (superseded)** | Former S25 brief, retained for planning provenance | Do not execute separately; included in M7 |
| **M3** | Early invertebrate nutrition → size-eligible Bluegill consumption; 5 stages, real collider/camera/cover implications | Exactly-once rewards, no too-large prey, stage/progression |
| **M4** | `PreyActor`/new bounded school behavior for staged Bluegill; shallow nursery/intermediate/deeper routes | Cohesion, separation, scatter/regroup, size/age and performance |
| **M5** | `PikeActor/PikeAwareness` multi-target evidence and hysteresis; Pike catches Bluegill or Bass | Real individual target IDs, commitment, both possible player escape outcomes |
| **M6** | Pure presentation blood event from genuine fish bite/damage/consumption; lifespan/FX budget | Once-only, fade/diffuse, pause/retry and native performance |
| **M7** | Implement S25 automatic mouth feeding and limited Spacebar boost against actual local M1–M6 source; T2 integrated preflight | Physical eligible mouth contact, exactly-once ledger/FX, finite boost, pause/focus/retry, genuine disturbance, world isolation |
| **M8** | Owner-native playthrough of S25 and existing Bass/hatch/food/schools/Pike/blood and P5 open matrix | **T3: real running Fish window with owner judgment** |

## Execution safety

Manual prompts are under `manual/M1-...` through `manual/M8-...`, with exact names in [manual README](manual/README.md). The M2A file is superseded and retained for planning provenance only. These are not runner parser tasks. Owner reports completion through M6. Start revised M7 after inspecting the true local source/evidence; M8 follows owner review. Each brief ends with STOP. The final M8 brief is explicit: **the last action, after all preflight, is to launch `./tools/dev run --display-mode=windowed-fullscreen`** using real display, and leave Fish open for the owner.

Avoid rebuilding the Pond generator, swapping every habitat's fish or adding save/death. Use actual checks defined in `tools/dev`, and implement new focused checks before calling them. If Codex cannot access a graphical Godot desktop, report that as BLOCKED rather than pretending it launched Fish.

### P5 qualification debt and C5 additions

Retest **G5.1/V03–V04** food/growth and cover clearance, **G5.2/V05/V33–V34/V36** Pike contact, concealment, escape and third-route isolation, **G5.3/V35** slow-cover approach vs exposed Bluegill approach. Extend with **V37** hatching, **V38** diet/growth, **V39** schools, **V40** multi-target Pike, **V41** event-driven blood and **V42** automatic mouth feeding/boost. Record original P5 historical results separately; version/commit name does not certify success. Only the owner's real play/observations can convert relevant NOT OBSERVED T3 cells to PASS.
