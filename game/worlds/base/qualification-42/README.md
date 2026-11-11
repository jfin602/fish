# Qualification Base World: seed 42

This is a reviewable **P7 qualification candidate**, not a shipping-world decision. It was explicitly promoted from the ignored `qualification-42-final` candidate with `./tools/dev promote-world qualification-42-final qualification-42` on 2026-11-10. Normal P3 play loads this baked package while native review is pending.

- Seed: `42`; settings version: `1`; generator version: `c3-full-1`.
- World ID: `world-76d44405d319640d`.
- Baked content SHA-256: `5b62157809953d93eeb0dfb3888540e3070b12512561ab6e3cdd1042750f4b9f`.
- `manifest.json` records resource hashes, stage timings and counts. `world-data.json` records routes, the 1,681 environment samples, placements and ambient life. The `.res`/`.scn` resources hold basin mesh/collision, generated geology, dressing and ambient visuals.
- FAF source models are referenced through the existing `res://assets/faf/` package; this directory does not include copies of the original FAF source payload. The local vendor working set remains governed by the existing ignored-asset setup.

P7 must inspect this world through native play and decide whether to retain, revise or replace it. The headless bake/load result is recorded in [the implementation evidence](../../../../docs/tasks/c3-world-gen/qualification/implementation.md).
