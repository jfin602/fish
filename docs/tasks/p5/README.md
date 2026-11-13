# P5 — Stealth, feed, escape and grow

**Status: P1–P11 CHECKPOINTED; P12 T2 SOURCE CHECKED at uncommitted `0.5.12`; P6 OWNER-CLOSED FOR PROGRESSION / NOT GREEN; P8 OWNER GREEN FOR PROGRESSION.** The exact-subject P11 checkpoint is `b20a62b`; [P11 evidence](qualification/native-feeding-growth.md) retains the owner's limited approval and open T3 cells. [P12 evidence](qualification/p12-isolation.md) records repeated fixture/normal-world switching and its headless limits. P4 is [owner-closed for progression / NOT GREEN](../p4/closeout.md). [P9 source evidence](qualification/p9-feeding.md) and [P10 source evidence](qualification/p10-growth.md) remain historical; no P5 Gate is GREEN.

[Assessment](prompt-assessment.md) · [Implementation plan](implementation-plan.md) · [New FAF cover intake](stealth-cover-asset-intake.md) · [P5 approved design](../../planning/p5/stealth-foundation.md) · [stealth system](../../systems/stealth-and-detection.md) · [ecology/growth](../../systems/ecology-and-growth.md) · [roadmap](../../roadmap.md) · [validation](../../validation.md) · [runner](../../phase-runner.md)

## Goal and boundaries

**M1 — Survive the Pike:** a third World Select **destination**, not third habitat, directly starts a separate Bluegill (`FAF-FISH-006`) versus Northern Pike (`FAF-FISH-010`) encounter with no Character Select. The ten new Pond FAF foliage/large-wood families blend naturally into shallow, midwater and deeper encounter cover, with real hard/soft shelter and exposed crossings, bounded sensory evidence, investigation, pursuit, last-known search, retreat and a fair caught/escaped outcome prove the basic stealth loop. Add one prey sensing/evasion actor, then feeding and growth later within P5. Normal Ocean and Freshwater Pond remain predator-free development/test worlds and preserve their current world/character selection paths.

> Every movement is a decision about exposure. Every hiding place is an opportunity to survive. Every feeding opportunity is a reason to take a risk.

## Entry and version guard

1. The [P4 owner disposition](../p4/closeout.md) and deliberate `0.5.0` entry are recorded; P4 remains NOT GREEN. Preserve its evidence and do not invent `0.4.12`–`0.4.14` checkpoints.
2. Verify live branch/HEAD, dirty work and compatibility with current P4/c4 corrections, Fish project and model/SDK availability. Never modify the P4 runner snapshot or overwrite unmerged art/world work.
3. **P2 strict asset input:** Both new 5+5 FAF producer packs were verified from actual sources/GLBs/manifests at P2. The [intake record](qualification/p2-cover.md) preserves hashes and limits. No original pack 001 substitution or normal Pond/Ocean asset install occurred.
4. Each prompt has one assigned `0.5.x` development checkpoint. The runner, not Codex inside the task, owns implementation commits. The **manual P6, P8 and P11 checkpoints** must be completed by a native-capable session with owner judgment, and P13 is a **manual final closeout**. Do not auto-resume a failed/blocked review.
5. Only P5 fixture outcomes are ephemeral. P6 saves/death, P7 archive, whole-world advanced AI and visual correction tasks remain out of scope. P5 does not make the existing Pond/Ocean ordinary routes into active predator ecosystems.

## Ordered prompts

| Task | Responsibility | Version | Check |
| --- | --- | --- | --- |
| [P1](P1-world-select-encounter-routing.txt) | Pike Encounter entry and session routing | `0.5.1` | T2 |
| [P2](P2-pike-fixture-cover-and-clearance.txt) | **Verify new 5+5 FAF pack intake; naturally blended three-zone cover fixture and Pike placement** | `0.5.2` | T2 |
| [P3](P3-world-space-perception.txt) | Camera-independent world-space perception | `0.5.3` | T2 |
| [P4](P4-disturbance-and-cover-signals.txt) | Movement disturbances and concealment signals | `0.5.4` | T2 |
| [P5](P5-pike-awareness-and-pursuit.txt) | Pike investigation, pursuit and escape | `0.5.5` | T2 |
| [P6](P6-native-pike-encounter-review.txt) | Native M1 Survive the Pike review | `0.5.6` | T3 manual stop |
| [P7](P7-prey-awareness-and-evasion.txt) | Shared prey sensing and evasive behavior | `0.5.7` | T2 |
| [P8](P8-native-stealth-integration-review.txt) | Native stealth and prey awareness review | `0.5.8` | T3 manual stop |
| [P9](P9-feeding-and-prey-transactions.txt) | Bite, prey eligibility and nutrition | `0.5.9` | T2 |
| [P10](P10-growth-stages-and-food-chain.txt) | Growth stages, body and food-chain reversal | `0.5.10` | T2 |
| [P11](P11-native-feeding-growth-review.txt) | Native feeding and growth review | `0.5.11` | T3 manual stop |
| [P12](P12-two-world-isolation-regression.txt) | Normal-world isolation and lifecycle regression | `0.5.12` | T2 |
| [P13](P13-phase-5-closeout.txt) | Phase 5 closeout | `0.5.13` | T3 manual stop |

**P1–P8** implement the approved F1–F8 stealth foundation; **P2 is an asset-readiness gate and encounter-composition task**, including the first Bluegill/Pike hand-play checkpoint. **P9–P13** finish feeding, growth, two-world regression and the full P5 closeout. A qualified **M1** scene does not alone make G5.1–G5.3 GREEN.

## Validation and actual evidence

After checking out these docs, validate the grammar without executing gameplay:

```bash
npm run fish:phase:validate -- p5
```

Only once P4's owner handoff and `0.5.0` phase baseline exist, execute intentionally:

```bash
npm run fish:phase -- p5
```

The existing `tools/dev` supports `build`, `import`, `smoke-world`, `inspect-world-select`, `inspect-pike-encounter`, `inspect-pike-cover`, `inspect-pike-sight`, `inspect-pike-disturbance` and `inspect-pike-ai`. Avoid redundant broad builds. Record actual evidence under `docs/tasks/p5/qualification/`; only an executed final closeout writes `docs/tasks/p5/closeout.md`. [Roadmap](../../roadmap.md) alone owns Gate status.
