# C5 M4 — age-structured Bluegill schools (manual execution)

**2026-11-13; headless T2 candidate, owner-native V39 NOT OBSERVED.** Source is `jfin602/fish`, branch `main`, HEAD `d6e8d2d06946ce115e79e953c20fbc39311ba680`, root package `0.5.12`. The tree already contained uncommitted M2/M3, audio and documentation work before M4; M4 did not commit, reset, push or run the phase runner. Fish Godot Editor MCP `2026.11.04` reported `bridge.connected=false`, so there was no editor or owner-play observation. CLI used Godot **4.7.2.stable.mono** and the repository's `tools/dev` commands.

## Implemented encounter behavior

- Pike Encounter seed `5266` now creates **three fixed, small schools of 16 real `FAF-FISH-006` Bluegill**: nursery (4 larvae, 2 juveniles), root/log refuge (2 larvae, 2 juveniles, 2 subadults), and deeper route (2 subadults, 2 adults). Each has a stable stage-qualified ID, original stage GLB and swim clips, a stage-sized sphere collider, independent alive/consumed state, and the existing exactly-once feeding ledger. Model scale is 1:1; no new FAF fish export or ordinary-world species eligibility changed.
- Seeded placement tries bounded positions around the three authored cover routes and rejects bed, waterline, shoreline, player/Pike and physics-shape overlap. Nursery center is near actual ShoreGrowth ribbon grass and coontail; its nearest measured instances were each 4.3 m away. The nursery's local ShoreGrowth meshes join prey sensing within the 128-bound budget. School depth is a soft moving goal; only physical bed/surface/collision stop a fish.
- Groups use at most six live members. Each fish combines its own forage phase with local separation, cohesion, alignment and a slowly moving group goal. Player and Pike visual evidence use separate world-space sensors; a recent alarm can spread only to a neighbor within 2.1 m. Prey scatter, then settle and regroup as the existing awareness memory expires. Stage speed caps increase with size. An eaten actor leaves active steering, while survivors remain individuals.
- Model-scene wrappers are held in a bounded catalog cache across Retry to reduce Godot/.NET resource-finalizer churn. This did **not** eliminate the pre-existing intermittent Godot handle diagnostic in the longer isolation command.

## Checks actually run

| Check | Result and evidence |
| --- | --- |
| `./tools/dev build` | **PASS**, zero warnings/errors on the final source. |
| `./tools/dev inspect-bluegill-schools` | **PASS**: 3 schools, 16 unique fish, stage counts 6/4/4/2; mean depths 1.43/6.40/7.51 m; registered rigs, native scale, collider and spawn clearance; local alarm, pause, regroup and once-only ledger. Maximum 72 neighbor comparisons for one 16-fish update. The final 180-frame actor-update sample was 141.1 ms total on this headless run; it excludes rendering and full sensory/physics frame cost. |
| `./tools/dev inspect-prey` | **PASS**: forage travel, sight/cover attenuation, movement cues, physical root/fork/split-wood behavior and reset. The old fixed-spawn assertion was updated for the new nursery. |
| `./tools/dev inspect-pike-encounter` | **PASS**: three destinations/two habitats, three retries with identical school positions and stable IDs, clean old actor disposal, ordinary route. |
| `./tools/dev inspect-world-isolation` | **Intermittent FAIL**. A run after the M4 changes reached `WORLD_ISOLATION_CHECK_OK` and exited cleanly; a later run reached the same marker but the Godot CLI emitted `System.InvalidOperationException: Handle is not initialized` during repeated scene changes and `tools/dev` correctly returned failure. M1/M3 evidence already records this intermittent Godot .NET check error. Do not count the marker alone as a clean final pass. |
| `./tools/dev inspect-feeding`, `inspect-growth`, `inspect-hatch` | **PASS** in this M4 session for once-only food/fish feeding, Bass growth and hatch/retry. `inspect-hatch` was repeated after moving the nursery near the nest and passed with 16 prey. |
| `git diff --check` | **PASS** after M4 documentation reconciliation. |

The first school check failed because its 3-second regroup window ended before the existing 2.8-second prey memory plus settle time; the check now covers the full decay. The first nursery location missed shoreline ribbon grass by 6.6 m; moving the school to the real shoreline cover produced the measured 4.3 m proximity. These failures were corrected and are not native visual qualification.

## Remaining qualification

**NOT OBSERVED:** native school visibility in Pond murk/light, natural swimming appearance and animation, real player discovery of larvae, full Forward+ frame cost, and owner playthrough. The final long isolation command remains intermittently **FAIL** with the known Godot handle diagnostic. No P5 Gate changes. Stop here for owner review; M5 is the next *manual* brief only after this result is reviewed.
