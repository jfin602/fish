# C5 — Manual Bass lifecycle, Bluegill schools and Pike ecology polish

**Owner screenshot follow-up:** [M5 collision evidence](qualification/m5-pike-multi-target.md#owner-screenshot-follow-up-solid-prop-size) records mesh-derived solid prop collision for Pond and Pike Encounter, with headless checks passing; the owner's obscured view after the change remains unobserved.

**Status: M1–M6 implemented with T2 checks; owner-native review remains open, including later subadult Bass speed, [Pike contact](qualification/pike-bass-stage-contact-followup.md), and caught-aftermath corrections. The M4/M5 long isolation check has an intermittent Godot .NET handle failure; M7–M8 NOT EXECUTED.** Owner approved [S24](../../decisions-and-questions.md) on 2026-11-13 and **S25 automatic forward-mouth feeding and limited Spacebar boost** on 2026-11-14; S25 is design only and belongs to revised M7. Root Fish package baseline remains **`0.5.12`**. Historical [P5 closeout](../p5/closeout.md) is **OWNER CLOSED FOR PROGRESSION / NOT GREEN**, all G5.1–G5.3 native gaps preserved. [M1 evidence](qualification/m1-bass-player-and-bluegill-prey.md), [M2 evidence](qualification/m2-hatching-and-tiny-bass.md), [M3 evidence](qualification/m3-early-diet-and-growth.md), [M4 evidence](qualification/m4-bluegill-schools.md), [M5 evidence](qualification/m5-pike-multi-target.md), and [M6 evidence](qualification/m6-blood-fx.md) keep checks and owner observations separate.

[Assessment](prompt-assessment.md) · [Implementation plan](implementation-plan.md) · [M3 evidence](qualification/m3-early-diet-and-growth.md) · [M4 evidence](qualification/m4-bluegill-schools.md) · [M5 evidence](qualification/m5-pike-multi-target.md) · [M6 evidence](qualification/m6-blood-fx.md) · [C5 approved design](../../planning/c5-post-p5-ecology-lifecycle.md) · [manual prompt list](manual/README.md) · [P5 closeout](../p5/closeout.md) · [validation](../../validation.md) · [roadmap](../../roadmap.md)

## Goal and context

Improve **only the existing separate Pike Encounter**, the third World Select destination; keep Ocean/Pond ordinary non-enemy worlds intact. Bass becomes the one fixed player `FAF-FISH-007`, starting in a shallow nest as egg then hatchable tiny fry. Tiny Bass initially relies on yolk and non-fish microfauna, then aquatic invertebrates and **Bluegill `FAF-FISH-006` as its sole fish prey** when actual size permits. Bluegill of varied stages school in believable shallow/medium/deeper areas; Pike can hunt Bass or actual Bluegill, allowing situational school-assisted escape. Confirmed fish injury/feeding creates modest dissipating blood.

## Manual execution, not runner automation

**2026-11-14 C5 sequencing update:** M1–M6 are present in local source and evidence. Do not rerun them or execute M2A retroactively. **Revised M7** implements S25 automatic mouth feeding and limited Spacebar boost plus T2 integration preflight against the completed C5 food/schools/Pike/blood work; **M8** provides owner-native T3 qualification. S25 is NOT yet implemented.

**M1–M8 are manually copied Codex briefs (M2A remains only as superseded planning) under `docs/tasks/c5-polish/manual/`.** They are **not** runner-compatible `P<number>-` prompts and there are deliberately no direct `.txt` entries in the correction folder. **Do NOT run `npm run fish:phase -- c5-polish` or `npm run fish:phase:validate -- c5-polish`; the runner grammar is intentionally not applicable.** Begin with revised **M7** only after owner review and inspection of actual local source/evidence; stop for review before M8. Manual prompts do not self-commit, self-push or advance a phase version. Preserve in-progress work at every step.

### Order

| Step | Manual brief | Scope / evidence |
| --- | --- | --- |
| M1 | [Bass player and Bluegill-only fish prey](manual/M1-bass-player-and-bluegill-prey.txt) | T2 implementation and targeted tests |
| M2 | [Shallow egg hatch and controllable Bass fry](manual/M2-hatching-cutscene-and-tiny-bass.txt) | T2 implementation and targeted tests |
| **M2A (superseded)** | [Earlier S25 planning brief](manual/M2A-auto-feeding-and-speed-boost.txt) | Not a separate execution step; scope moved to M7 |
| M3 | [Early Bass food and all-stage growth](manual/M3-early-diet-and-growth.txt) | T2 implementation and targeted tests |
| M4 | [Age-structured Bluegill schools in real Pond cover](manual/M4-bluegill-schools-by-age-and-depth.txt) | T2 implementation and targeted tests |
| M5 | [Independent Pike hunting and school-assisted evasion](manual/M5-pike-multi-target-and-school-escape.txt) | T2 implementation and targeted tests |
| M6 | [Dissipating underwater blood on genuine fish predation](manual/M6-blood-cloud-on-real-fish-events.txt) | T2 implementation and targeted tests |
| M7 | [Implement S25 feeding/boost and C5 integration preflight](manual/M7-regression-and-native-playtest-preflight.txt) | **S25 implementation + T2 source and integration checks** |
| M8 | [Owner-native launch including S25](manual/M8-owner-native-launch-and-qualification.txt) | **T3 real native owner playthrough and qualification** |

**The final M8 brief explicitly ends with `./tools/dev run --display-mode=windowed-fullscreen`, leaving the native game open for the owner's actual visual/playthrough review.** M8 has not been executed.

## Safe source/version/evidence boundaries

Start from actual source, not stale descriptions. The historical P5 fixture played Bluegill against Bass larvae/juveniles. M1 switches those roles; M2 begins the normal fixture in a Bass egg nest and hatches into the registered playable Bass larva. Earlier M1 checks explicitly start at juvenile for their original growth/contact scope. M3 implements first food and growth; M4 adds staged Bluegill schools; M5 adds individual Bluegill Pike sensing, bounded choice and once-only physical consumption. M6 adds blood at committed fish predation contacts with a bounded particle pool and a live caught aftermath. Current source still binds Space to manual bite; M7 must implement S25 automatic mouth feeding and finite boost on top of this M3–M6 work. M8 performs native qualification. Five Bass and Bluegill stages have genuine FAF exports. Historical P5 `qualification/` and closeout stay unchanged.

Keep actual package version **`0.5.12`**, unless a separate owner version decision is recorded. P6 durable save/death and P7 Life History are deferred. Do not modify normal Ocean/Pond bake/world identities or propagate encounter predators into them.

Manual evidence belongs under `docs/tasks/c5-polish/qualification/` only after a task actually runs. **C5 approval is not source implementation or native qualification**. Roadmap remains the sole Gate-status authority.
