# C5 M3 — early Bass food and all-stage growth

**Status, 2026-11-13: corrected headless T2 PASS; owner-native speed and collision recheck pending.** The owner reported that second-stage growth gave excessive speed and nearly stranded the Bass on shore. M3 is a manual brief. M4 has not been started, and no P5 Gate is promoted.

## Source identity and preserved work

- Repository: `jfin602/fish`, branch `main`, HEAD `d6e8d2d06946ce115e79e953c20fbc39311ba680`; root `package.json` is **`0.5.12`**. The historical `0.5.13` commit subject is not a package version change.
- At entry, the worktree already held M2 corrections in `BOOT.md`, `docs/planning/c5-post-p5-ecology-lifecycle.md`, `docs/tasks/c5-polish/qualification/m2-hatching-and-tiny-bass.md`, `game/dev/HatchChecks.cs`, `game/scripts/FishController.cs`, `game/scripts/PikeCoverFixture.cs` and `game/scripts/PikeEncounter.cs`, plus separate audio work. All were preserved. No reset, rebase, commit or push was made.
- Godot Editor MCP server `2026.11.04` reported `bridge.connected=false` at `ws://127.0.0.1:19080`, with no active scene or selected editor project. The CLI ran Fish Godot **4.7.2 .NET** from this checkout. No editor mutation or native play was claimed.

## Implemented behavior and tuning

The fixture now starts an egg/larva on yolk. After 12 simulation seconds, the larva can eat non-fish zooplankton clouds for 1 nutrition each; six distinct clouds reach juvenile at 6. Juvenile Bass can eat aquatic green darner nymphs for 2 each and reaches subadult at 14. Subadult Bass can eat registered Bluegill larvae worth 4 each and reaches adult at 26. Adult Bass can eat Bluegill juveniles worth 6 if they are at most 45% of Bass length; larvae remain eligible at a 60% cap. Adult Bluegill, Bass, Pike and fish from other habitats remain ineligible. These thresholds/ratios are proposed encounter tuning, not biology-derived values.

Eight muted, copepod-like clouds and eight real `FAF-INSECT-001A` nymph exports appear only near the encounter nest. The nymph export is named in `assets/faf/pond/insects/pond_insect_001_manifest.json`; its asset remains unchanged. The clouds use small non-emissive bodies and antennae, with bounded game-clock drift and stable IDs. The nearest currently edible type, direction and distance appear in the encounter status; the yolk interval has a countdown. The shell remains a one-time zero-nutrition bite. There are no orange proxy orbs or new fish species.

`PondGrowthState` retains one `FAF-FISH-007` Bass ID and chooses the real registered egg, larva, juvenile, subadult and adult exports. It derives the capsule/body length, width-based radius, mouth radius and bite reach from the current export. `FishController` and `FollowCamera` already use body scale for collider, camera and animation replacement; M3 switches the larval camera profile at juvenile growth. Pike and prey LOS use the current fish body radius. Stage expansion uses a full capsule collision query and surface limit; blocked growth sets an open-water hint and retains earned nutrition for retry. Feeding commits a stable target ID once after stage, habitat, life, reach, facing, clear-ray and time checks.

### Owner-reported M3 speed and body correction

The first native M3 build failed the owner's speed/shore expectation: growth into juvenile restored the shared `swim_tuning.tres` 4.0 m/s cap in one step from the 0.22 m/s larva. The follow-up now assigns fixture-only per-stage maximum speeds of **0.22, 0.48, 0.95, 1.55 m/s** for larva, juvenile, subadult and adult, with increasing acceleration of **0.42, 0.80, 1.50, 2.60 m/s²**. These are proposed play tuning, not measured biology. The shared Ocean/Pond tuning resource is unchanged.

Each swimming-stage capsule is now fitted to vertices of its registered imported Bass mesh in fish-local coordinates. Stage-specific radial and longitudinal allowances cover rest-pose tips and leave a small allowance for skin motion; each stage has its own pivot offset. The previous juvenile/subadult/adult tails extended beyond their fitted capsule by about 0.006/0.020/0.032 m. The headless fit checks now contain every sampled rest-pose vertex. Fitted `(radius, height)` in meters are larva `(0.0110, 0.0819)`, juvenile `(0.0594, 0.3341)`, subadult `(0.1436, 0.7082)`, adult `(0.2232, 1.0521)`. The egg is non-swimming and has no live body collider. Growth clearance uses the next fitted capsule; visual length separately drives camera and bite reach. Animated mesh deformation and native visual feel still need owner review.

After the correction, the Fish Godot 4.7.2 .NET CLI returned **PASS** for `./tools/dev build` (zero warnings/errors), `inspect-growth`, `inspect-microfauna`, `inspect-hatch`, `inspect-feeding`, `inspect-prey`, `inspect-pike-encounter`, `inspect-species`, `inspect-world-isolation`, and `git diff --check`. `inspect-growth` covered per-stage vertex containment, 120-frame thin-wall contact at juvenile/subadult/adult speeds, a 240-frame juvenile ascent at the shallow nest, wall/surface growth deferral, root-ball/hollow-log clearances and camera poses. `inspect-hatch` covered the larval wall and surface. A new test initially marked the subadult wall crossing as **FAIL** because it assumed the full unpitched forward reach after the ascent test; the fish was pitched upward and remained on the near side. The probe now checks wall crossing against the minimum capsule radius and passes. This test correction is retained as a diagnostic, not evidence of a gameplay collision failure.

## Checks and outcomes

| Command | Result | Observed evidence |
| --- | --- | --- |
| `./tools/dev build` | **PASS** | Fish .NET compiled with zero warnings/errors after final code edits. |
| `./tools/dev inspect-feeding` | **PASS** | Egg/yolk and stage policy, invalid habitat/dead/out-of-reach/repeated attempts, physical wall/rear/distant nymph refusal and exactly-once ledger; `FEEDING_CHECK_OK food=16 juvenile_nutrition=8`. |
| `./tools/dev inspect-microfauna` | **PASS** | Actual egg hatch, 12-second yolk gate, six unique clouds, four nymphs, three Bluegill larvae and one stable Bass ID through adult at 26 nutrition/13 events; `MICROFAUNA_CHECK_OK ... chain=zooplankton,nymph,bluegill`. |
| `./tools/dev inspect-growth` | **PASS** | All five registered exports, threshold reentry, stage-specific rig/capsule/camera, deferred wall/surface growth with nutrition retained, pause, duplicate refusal, adult Bluegill size check, real root-ball/hollow-log clearances and camera poses; `GROWTH_CHECK_OK ... stage=adult nutrition=32`, plus two `GROWTH_SHELTER_CAMERA_OK` markers. |
| `./tools/dev inspect-prey` | **PASS** | Existing physical Bluegill prey/cover behavior retained. |
| `./tools/dev inspect-hatch` | **PASS** | Existing M2 watch/skip, shell, fry camera, pause/retry and shore checks retained. |
| `./tools/dev inspect-species` | **PASS** | Ocean/Pond selection and species populations unchanged; fixture isolated. |
| `./tools/dev inspect-world-isolation` | **PASS** | World Select still has three destinations but two habitats, and returns to ordinary worlds without fixture leakage. |
| `git diff --check` | **PASS** | No whitespace errors. |
| Owner-native speed/shore observation | **FAIL in first M3 build; corrected build NOT OBSERVED** | Owner reported a large second-stage speed jump and near-stranding on shore. No owner replay after this correction has been reported. |
| Godot Editor MCP | **NOT OBSERVED** | Bridge disconnected; no Fish editor session. Headless assertions do not establish animated silhouette, camera comfort, or native rendering cost. |

The first `inspect-growth` invocation reached its success marker but **failed at Godot shutdown** with leaked temporary `SphereShape3D` references from the cover survey. The survey now disposes each query shape. After rebuilding, a fresh `inspect-growth` run exited 0 with its marker and no engine errors. That failed attempt is retained here rather than counted as a clean pass.

## Changed files for M3

- Gameplay: `game/scripts/FeedingLedger.cs`, `game/scripts/PondGrowthState.cs`, `game/scripts/FishController.cs`, `game/scripts/PikeEncounter.cs`, new `game/scripts/PondFoodActor.cs` and its Godot UID sidecar.
- Focused checks: `game/dev/FeedingChecks.cs`, `game/dev/GrowthChecks.cs`, `game/dev/HatchChecks.cs`, new `game/dev/MicrofaunaChecks.cs` with its Godot UID sidecar, and `tools/dev` for `inspect-microfauna`.
- Documentation: `docs/systems/ecology-and-growth.md`, `docs/tasks/c5-polish/README.md`, `docs/tasks/c5-polish/implementation-plan.md`, `docs/planning/c5-post-p5-ecology-lifecycle.md`, `BOOT.md`, and this evidence file. Edits in shared M2 files were additive to existing local M2 work.

## Owner review and next manual step

Owner-native review should replay larva→juvenile growth to confirm the speed ramp feels gradual, confirm no body or animated-fin clipping through solid cover at every stage, and retry the shallow shore ascent where the original defect occurred. The nursery food readability, first-target cue, bite/growth fairness and Forward+ cost also remain open. Corrected M2 native hatch/feel and M1 contact recheck remain open. **Stop here.** M4 Bluegill schools is the next *separate* manual brief after owner review; it has not been invoked.

## 2026-11-13 subadult speed follow-up after M4

The owner reported that subadult Bass speed was **too slow** in native play of the M4-era build (**FAIL** for that feel criterion). This later observation does not change the earlier M3 result above. The encounter-only subadult `StageTuning` now has a **1.40 m/s** forward cap and **2.40 m/s²** acceleration, up from 0.95 and 1.50. Its coast/brake/reverse and velocity alignment were adjusted with the faster forward swim. Juvenile remains at 0.48 m/s and adult at 1.55 m/s; ordinary Pond/Ocean tuning remains unchanged.

After this correction, `./tools/dev build` **PASS** (zero warnings/errors), `./tools/dev inspect-growth` **PASS** and `git diff --check` **PASS**. The growth probe measured **1.40 m/s** and **1.00 m** travel after one second of sustained forward motor input, exercised the actual growth transition and its 120-frame subadult wall contact at the new cap, and retained the juvenile 240-frame shore check. Godot Editor MCP reported `bridge.connected=false`, so the corrected subadult speed, animation and camera feel are **NOT OBSERVED** in native play. The Fish headless CLI was Godot **4.7.2 .NET** against this checkout; root package remained `0.5.12` on `main` at `d6e8d2d06946ce115e79e953c20fbc39311ba680`. No commit or push was made. **Stop for owner review; M5 remains the next separate manual brief.**
