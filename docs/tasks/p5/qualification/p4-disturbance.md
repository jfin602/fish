# P5 P4 — Movement disturbance and cover source check

**2026-11-13 result: PASS for focused source/headless checks; native gameplay NOT OBSERVED.** Pre-task HEAD `e527efad1096677dd83f65d6cc52c241d0ad2116` on `main`, root `0.5.3`; this is the P3 checkpoint. P4's owner [progression closeout](../../p4/closeout.md) and [P3 evidence](p3-perception.md) satisfy entry. P3 was T2, with no intervening manual stop. This runner turn resumed an existing dirty P4 working tree and preserved its partial implementation. This record covers uncommitted P4 source for the runner checkpoint and claims no implementation commit.

## Source behavior

The isolated seed-5202 Pike Encounter samples FishController's post-collision velocity, motor tick count, effort and turn after each physics step. A fixture-local clock advances once per completed motor tick. Cues have a 0.25 s minimum spacing, 1.5 s lifetime, at most eight records, and radii clamped to 0.65–8 m. Speed, positive acceleration, effort and turn increase cue energy; still fish produce none and gliding produces less. Pausing freezes the cue clock and events. Focus-style input clearing stops new powered cues; retry/exit clear the field. Sensing reads cues and real physics rays without writing fish/motor state. Wood blocks sight via the existing P3 sensor and damps a movement cue without erasing it; visible plant mesh bounds reduce both sight and motion evidence with nonzero floors. There is no authored safe-zone state or predator suspicion yet. WaterReaction, camera, fog and GFX settings do not feed the sensor.

Source SHA-256: `MovementDisturbanceField.cs` `36717d2a8b434a29270841ea6477460de2ae9e8585db4e535d9b7453e22e270e`; `WorldSightSensor.cs` `512caf64216616046e12258f39c7a8df2457fa18519c87170c58b5e35b09a3c7`; `PikeEncounter.cs` `53ea7086743bde42e72e04bb4f05041655b87b0d7997c0290f5404d5d30e08d9`; `PikeDisturbanceChecks.cs` `72b9b7b37cffb431dd88241457e52e3f36f616cd6014e74d7e352cbc6129cede`. Fixture seed 5202; Ocean regression seed 42. Godot Editor MCP server `2026.11.04` reported bridge disconnected at Fish's configured port, so connected editor identity, native gameplay and owner response are **NOT OBSERVED**. No editor mutation was attempted.

| Actual command | Result and limit |
| --- | --- |
| `./tools/dev build` | **PASS**, Godot .NET/C# zero warnings/errors. |
| `./tools/dev inspect-pike-disturbance` | **PASS**, seed 5202: open cue `0.133`, wood damped cue `0.057`, five plant families reduce but do not zero cues, real fish input produced three bounded events. Checks still/glide/burst/turn order, actual motor coast, 1,000-step budget/TTL, stop/decay, duplicate ticks, pause, camera/fog/preset independence, motor read-only behavior, focus-style input clearing and reset. Fixed/scripted observations only. |
| `./tools/dev inspect-pike-sight` | **PASS**, real wood rays and five plant families still satisfy P3 sight behavior; 66 visible plant mesh bounds. |
| `./tools/dev inspect-pike-cover` | **PASS**, 47 instances, 193 wood collision shapes, fish/Pike hollow traversal and no sampled camera overlaps. |
| `./tools/dev smoke-world` | **PASS**, existing swim motor checks including coast and step-schedule stability. |
| `./tools/dev inspect-ocean-equal-input` | **PASS**, baked Ocean seed-42 equal-input 30-step trace with effects on/off. |
| `git diff --check` | **PASS** after the final uncommitted diff. |

The first cue check failed because its nominal open observer was outside the intended cue radius. The next run found the hand-picked wood points unsuitable for a nearby blocked line. The final check uses an in-radius open observer and searches real split-log wrapper rays. These were check-fixture corrections; neither is a native play result. Native player-controlled hiding, fine leaf gaps, all cover approach angles, sustained performance, awareness/pursuit and owner judgment remain **NOT OBSERVED**. V05/V33–V36 and all P5 Gates remain unqualified.

The dirty-tree continuation reran every command in the table against the source hashes above. `./tools/dev build` again reported zero warnings/errors; the cue, sight, cover, swim and Ocean markers all passed. `bash -n tools/dev` and `git diff --check` passed. The MCP server again reported its Fish bridge disconnected; no editor mutation or native owner playtest was performed.
