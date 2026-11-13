# P5 P5 — Pike awareness, pursuit and fixture outcomes

**2026-11-13: PASS for focused source/headless behavior; native fairness NOT OBSERVED.** Pre-task `main` HEAD was `df0c593abcfd0678558d5bc16e9b68de38f3d05c` (`0.5.4`), with a clean working tree. That commit is the actual P4 runner checkpoint; [P4 source evidence](p4-disturbance.md) and the [P4 owner progression disposition](../../p4/closeout.md) satisfy entry. There was no intervening P5 manual checkpoint. This P5 implementation is left uncommitted for the phase runner at root `0.5.5`.

## Checked behavior

The fixture Pike is a `CharacterBody3D` with its original horizontal capsule, now driven by `PikeActor` and a separate `PikeAwareness` decision model. The model receives only sampled `SightEvidence.SensedPosition/SensedAt` or the strongest bounded `DisturbanceEvidence.Position/SensedAt`. Cached sight keeps its sampled coordinates and timestamp; an unlocated, stale or weak sight sample cannot update target memory. The actor patrols without a target, investigates a local movement cue, chases confirmed sight, slows during a 0.55 s attack windup, and has a short committed strike window. A catch needs a fresh clear sight sample, facing and physical bite range. After lost contact it searches the last sensed location and finite nearby offsets, then disengages after 6.5 s; the fixture reports an ephemeral escape only after a verified chase. Retry/exit reset the actor, sensors, cue clock, memory, input and outcome. No normal Ocean/Pond predator, save, death or Life History state is written.

Steering is speed and turn bounded, uses real body `MoveAndSlide` collision, probes full-body `TestMove` clearance, and follows the rising mud bed before descending. The fixed-seed probe crosses open water and tests blocked lines and physical pursuit against long-tree ends, forks, root balls, split hollows and sunken crowns. The prior P2 cover traversal check holds the new ecology tick while it moves actors to measure clearance. Imported fixture scenes stay alive across rapid retry to avoid a Godot/.NET wrapper-finalizer error during scene churn.

Source SHA-256 at this check:

| Source | SHA-256 |
| --- | --- |
| `PikeAwareness.cs` | `7bc00d2b14ed3bc1b71c14534321df2a93bafd70b239d305ca639faa0eaf78fb` |
| `PikeActor.cs` | `558e743f4b7650bc94bb01d3653d89439ea54e2a3b8076f8dd90ffe6f7faa758` |
| `PikeEncounter.cs` | `9117df5d80298e0ed271d989bde2e387005337fb0d9ca9afb1ec50fcde051cab` |
| `WorldSightSensor.cs` | `0699a3651a4ed575b28edefce6c3e283f8a60ee87bf4853fa682c60f105397b3` |
| `MovementDisturbanceField.cs` | `95eb0df32ee91d1563f6ebec7f76dbafcd83f3c44d3e45d2ad1d1ad61ada110c` |
| `PikeCoverFixture.cs` | `2460ac5d1edf0f34585a73475f0f7d5a2c345cf108105860545e956edd1b2c3b` |
| `PikeAiChecks.cs` | `2fa8e0e7454f8261c4eedc166d7a29f3a2906aad93bc8260a05e48643db41f78` |
| `PikeCoverChecks.cs` | `673f7e4c5790b6af8672b753e291ac837d2579c890263f7ab3aeda3b57f88c8f` |
| `p5_pike_encounter.tscn` | `ffb29609fcce84901ea317627b98072b66fa99eeb3f4b614922ddfd1777a98fc` |
| `tools/dev` | `d50d8b3f9b5cebf9b6335dd60ca6e38f5799f1ea40159e4d343674cadde14fc8` |

The fixture seed is **5202**. The Ocean and Pond route checks use their accepted baked worlds, including Ocean seed 42 and Pond seed 71. Godot 4.7.2 .NET was launched through the Fish-only `tools/dev` project path. Godot Editor MCP server `2026.11.04` reported `bridge_connected=false` at `ws://127.0.0.1:19080`, so connected editor identity and native play were **NOT OBSERVED**; no editor mutation was attempted.

| Actual command | Result |
| --- | --- |
| `./tools/dev build` | **PASS**, zero warnings/errors after final source change. |
| `./tools/dev inspect-pike-ai` | **PASS**, `PIKE_AI_CHECK_OK`: insufficient/stale/unlocated sight, absent-target patrol, suspicious slow cue, investigation, headless open-water fixture catch at 1.27 s, five wood blocked-sight/search/disengage cases, body penetration and turn/speed checks on all five, pause and repeated reset. Escaped fixture conclusion is decision/occlusion checked, not a full player-controlled escape run. |
| `./tools/dev inspect-pike-sight` | **PASS**, `PIKE_SIGHT_CHECK_OK`, five wood and five plant families. |
| `./tools/dev inspect-pike-disturbance` | **PASS**, `PIKE_DISTURBANCE_CHECK_OK`, finite movement evidence. |
| `./tools/dev inspect-pike-cover` | **PASS**, `PIKE_COVER_CHECK_OK`, 47 placements, 193 wood shapes, fish/Pike hollow passage and camera clearance. |
| `./tools/dev inspect-pike-encounter` | **PASS**, `PIKE_ENCOUNTER_CHECK_OK`, three destinations, two habitats, three retries and Pond-to-fixture isolation. |
| `./tools/dev inspect-world-select` | **PASS**, `WORLD_SELECT_CHECK_OK`, both normal Character Select routes. |
| `./tools/dev inspect-scene res://scenes/p5_pike_encounter.tscn` | **PASS**, `SCENE_OK` after giving the scene tool an ignored local Godot data profile. |
| `./tools/dev smoke-world` | **PASS**, `FISH_MOTOR_CHECK_OK` and `FISH_SMOKE_OK` on normal Ocean. |
| `bash -n tools/dev`; `git diff --check` | **PASS**. |

During development the new physical probe first found mud-bed overlap beside the split hollow; local floor following and candidate clearance fixed it. The earlier P2 cover probe failed when the active Pike interrupted its scripted traversal; suspending ecology for that probe restored its original measurement. Rapid encounter retries initially emitted Godot/.NET `Handle is not initialized` errors when imported scene wrappers were finalized; retaining those imported scenes fixed the repeated route check. `inspect-scene` initially failed to write Godot's default user log in this sandbox; an ignored `XDG_DATA_HOME` profile fixed the check. The first telegraph tuning did not catch within the probe's short ProcessFrame budget; the check now waits for the actual 1.27 s physics outcome. These failures are preserved here, not reported as passes.

**NOT OBSERVED:** visible native chase/attack readability, owner-controlled hidden escape and caught comparison, performance/frame pacing, and all approach angles through the new FAF structures. P6 is the manual M1 review. V05/V33–V36 and every P5 Gate remain unqualified.
