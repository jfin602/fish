# C5 — Manual Bass lifecycle, Bluegill schools and Pike ecology polish

**Status: M1 CONTACT AND ESCAPE-CONTINUATION CORRECTIONS CHECKED HEADLESS / OWNER NATIVE RECHECK PENDING; M2–M8 NOT EXECUTED.** Owner approved [S24](../../decisions-and-questions.md) on 2026-11-13. Root Fish package baseline remains **`0.5.12`**; a historical commit subject `0.5.13` does not change that. Historical [P5 closeout](../p5/closeout.md) is **OWNER CLOSED FOR PROGRESSION / NOT GREEN**, all G5.1–G5.3 native gaps preserved. [M1 evidence](qualification/m1-bass-player-and-bluegill-prey.md) records implementation checks, intermittent world-isolation .NET check errors, and native limits.

[Assessment](prompt-assessment.md) · [Implementation plan](implementation-plan.md) · [C5 approved design](../../planning/c5-post-p5-ecology-lifecycle.md) · [manual prompt list](manual/README.md) · [P5 closeout](../p5/closeout.md) · [validation](../../validation.md) · [roadmap](../../roadmap.md)

## Goal and context

Improve **only the existing separate Pike Encounter**, the third World Select destination; keep Ocean/Pond ordinary non-enemy worlds intact. Bass becomes the one fixed player `FAF-FISH-007`, starting in a shallow nest as egg then hatchable tiny fry. Tiny Bass initially relies on yolk and non-fish microfauna, then aquatic invertebrates and **Bluegill `FAF-FISH-006` as its sole fish prey** when actual size permits. Bluegill of varied stages school in believable shallow/medium/deeper areas; Pike can hunt Bass or actual Bluegill, allowing situational school-assisted escape. Confirmed fish injury/feeding creates modest dissipating blood.

## Manual execution, not runner automation

**M1–M8 are independent manually copied Codex briefs under `docs/tasks/c5-polish/manual/`.** They are **not** runner-compatible `P<number>-` prompts and there are deliberately no direct `.txt` entries in the correction folder. **Do NOT run `npm run fish:phase -- c5-polish` or `npm run fish:phase:validate -- c5-polish`; the runner grammar is intentionally not applicable.** Review the M1 result; only after owner approval proceed to M2, and likewise for each later brief. Manual prompts do not self-commit, self-push or advance a phase version. Preserve and reconcile in-progress work at every step.

### Order

| Step | Manual brief | Scope / evidence |
| --- | --- | --- |
| M1 | [Bass player and Bluegill-only fish prey](manual/M1-bass-player-and-bluegill-prey.txt) | T2 implementation and targeted tests |
| M2 | [Shallow egg hatch and controllable Bass fry](manual/M2-hatching-cutscene-and-tiny-bass.txt) | T2 implementation and targeted tests |
| M3 | [Early Bass food and all-stage growth](manual/M3-early-diet-and-growth.txt) | T2 implementation and targeted tests |
| M4 | [Age-structured Bluegill schools in real Pond cover](manual/M4-bluegill-schools-by-age-and-depth.txt) | T2 implementation and targeted tests |
| M5 | [Independent Pike hunting and school-assisted evasion](manual/M5-pike-multi-target-and-school-escape.txt) | T2 implementation and targeted tests |
| M6 | [Dissipating underwater blood on genuine fish predation](manual/M6-blood-cloud-on-real-fish-events.txt) | T2 implementation and targeted tests |
| M7 | [C5 integration checks and owner-native launch readiness](manual/M7-regression-and-native-playtest-preflight.txt) | T2 regression + native preflight |
| M8 | [Launch Fish for the owner's full visual/playthrough qualification](manual/M8-owner-native-launch-and-qualification.txt) | **T3 final native game launch and owner qualification** |

**The final M8 brief explicitly ends with `./tools/dev run --display-mode=windowed-fullscreen`, leaving the native game open for the owner's actual visual/playthrough review.** M8 has not been executed.

## Safe source/version/evidence boundaries

Start from actual source, not stale descriptions. The historical P5 fixture played Bluegill against Bass larvae/juveniles. M1 switches the fixture to juvenile Bass against Bluegill larvae/juveniles, with Bass juvenile→subadult→adult growth and Bluegill-only fish feeding. `PikeActor` still receives player-centered awareness; M2–M8 own the remaining lifecycle, ecology and native work. Five Bass and Bluegill stages already have genuine FAF exports. Historical P5 `qualification/` and closeout stay unchanged.

Keep actual package version **`0.5.12`**, unless a separate owner version decision is recorded. P6 durable save/death and P7 Life History are deferred. Do not modify normal Ocean/Pond bake/world identities or propagate encounter predators into them.

Manual evidence belongs under `docs/tasks/c5-polish/qualification/` only after a task actually runs. **C5 approval is not source implementation or native qualification**. Roadmap remains the sole Gate-status authority.
