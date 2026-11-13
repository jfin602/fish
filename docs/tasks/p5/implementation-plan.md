# P5 implementation plan — Pike stealth foundation and feed/grow

**State:** execution design only; not source implementation. Start from the [assessment](prompt-assessment.md), [new FAF intake contract](stealth-cover-asset-intake.md), [approved P5 plan](../../planning/p5/stealth-foundation.md), [stealth rules](../../systems/stealth-and-detection.md) and [ecology](../../systems/ecology-and-growth.md). GitHub main at 535116c5b6907681786b233f34af15bc2a87d4b5, root 0.4.11. P4 P12/V32 FAIL / NOT GREEN and still undergoing visual/camera work. P5 is not started and must NOT be executed until owner approves explicit P4 closeout/progression and deliberate root 0.5.0 baseline.

## Preflight and architecture

At actual implementation inspect Git source/working tree, P4 owner progression/closeout evidence, version, registered Pond fish stages/asset hashes, Fish-only editor/MCP connection and `tools/dev` command composition. The `p5` phase requires clean/reconciled root `0.5.0` after explicit P4 handoff, not a guessed update during prompt writing.

Use a distinct `p5_pike_encounter.tscn`, with session-local fixture ownership and reusable ecological C# logic. **P2 is a strict producer-asset handoff and 3-zone composition task** for five new underwater vegetation plus five new large wood families; no missing-pack fallback, new Blender modeling or normal Pond/Ocean changes. The scene must be ecologically clustered, rooted on actual terrain and traversable under real Bluegill/Pike/FollowCamera collision. Keep the two authoritative `WorldCatalog` habitats untouched; create a separate three-destination menu model. Resolve Bluegill/Pike from exact registered Pond species IDs; use existing third-person controls and camera. Give Pike its own physical actor and AI, not an ambient presentation model with magical chase behavior. World-space sensor queries own sight/motion evidence and last-known positions, **not** shader fog or camera transforms. Fixture retry/exit owns cleanup of all AI, inputs, pause and temporary outcomes.

## Ordered implementation

| Stage | Source responsibilities | Failure modes and focused checks |
| --- | --- | --- |
| P1 — Direct menu/fixture | `WorldSelect.cs`, scene/controller, routing checks | Three destinations, two worlds, no Character Select in fixture, missing assets refuse, return/retry clean |
| P2 — New FAF intake and encounter zones | Verify exact **5+5 new expansion** sources/manifests/variants/GLBs, then stage all ten across shallow ribbon-grass bank, intermediate weed/coontail/root refuge and deeper split-log/branch-crown maze | **BLOCK** on missing pack; real rooted ground contact, valid openings, soft/hard cover, no invisible obstacles or prop grids |
| P3 — World-space sight | new bounded perception component, clarity/illumination data, physics occlusion | World position/angle, path turbidity and obstacle effect; stable across camera/presets |
| P4 — Local disturbances | read-only motion sampling and finite TTL events, soft-cover response | Still/glide/burst changes, cover not invulnerable, pause and reset stable |
| P5 — Pike hunting | Pike actor/steering/memory, ephemeral caught/escaped transitions | Pursuit on real evidence, last-seen searching, accessible routes and fair strike |
| P6 — Native M1 | native game and owner-observed diagnostics/evidence | Repeatable real hidden escape and exposed catch; STOP if failed |
| P7 — Prey sensing | one plausible prey/test proxy using same sensory rules | Careful approach differs from reckless; flight toward real cover |
| P8 — Native shared stealth | real play plus menu/perf/cover review | Native owner judgement and no incomplete V35 bite claim |
| P9 — Feeding | food/bite state, consumption IDs, prey eligibility | Exactly once, no invalid/repeated rewards, prey and Pike still respond |
| P10 — Growth | nutrition thresholds, authentic stage model, size/collider/camera logic | No clipping or fake species replacement, plausible role/cover change |
| P11 — Native feed/grow | native multi-stage pursuit/hunt/food/growth review | Real owner observation of V03/V04/V35; STOP if failed |
| P12 — Regression | WorldSelect, both worlds, ecology cleanup/metrics | No normal-world predators, preserved hashes, no leaking sensors/rewards |
| P13 — Closeout | evidence mapping and roadmap/BOOT/MODULES reconciliation | Honest G5.1–G5.3 status; no P6 life/death claims |

## Manual stop and version handling

Use exact runner metadata `- Browser required: yes.` for P6, P8, P11 and P13, which pauses before implementation. Every numbered prompt has the next `0.5.x` target; this is *not* permission to skip a failed native checkpoint. The runner owns actual version checkpoint commits. The final P13 closeout remains owner reviewable and may retain the previous version if not accepted. Do not self-commit/push during spawned prompts.

## Validation

Use `npm run fish:phase:validate -- p5` only to prove grammar/ordering. Focused checks of affected systems + build/scene smoke demonstrate code behavior; actual native fish input and owner judgement are required for stealth readability, chase, eating/growth, screen clarity and comfortable camera. Record source/seed/preset/OS/native steps, performance, gaps and files in `qualification/`. Never infer GREEN merely from authored plans, an AI transcript, a headless Godot compile or an unobserved screenshot. The roadmap remains Gate authority.
