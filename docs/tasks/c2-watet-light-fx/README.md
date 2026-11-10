# c2-watet-light-fx — water surface and sunlight presentation correction

**State:** AUTHORED / NOT EXECUTED. **Type:** Correction 2. **Root version:** `0.2.6` unchanged for every prompt.

The folder name intentionally preserves the owner-requested `c2-watet-light-fx` spelling so the runner command matches the authored stack.

## Purpose

Bring the existing P2 Forward+ underwater presentation toward the owner-selected visual target without changing fish movement, camera authority, habitat collision, or the low-poly art contract.

The target is a bright stylized reef seen through convincing water rather than a blue-filtered scene:

- a visible animated water surface overhead with bright sky/sun transmission when looking up;
- a small number of soft, broad sunlight shafts descending from the surface;
- organic, broken moving caustics on sand/rock/coral with no obvious square/grid/lattice pattern;
- stable near-field geometry and camera image, with little or no whole-screen wobble;
- existing water-blue depth falloff, motes and low-poly world readability retained;
- bounded GPU/draw-call cost suitable for the current Radeon 880M-class native baseline.

This correction is visual presentation only. It does not implement fluid physics, buoyancy, waves that affect the fish, new world geometry, P3 ecology, camera/control changes, SDFGI, extra shadow-casting lights, or a renderer switch.

## Current source boundary at authoring

Normal play uses Forward+ and `main.tscn -> p2_habitat.tscn`. `UnderwaterAtmosphere.cs` currently installs distance/height/volumetric fog, one directional sun, 128 GPU motes, one 84 × 84 m caustic decal generated from periodic sine functions, and `underwater_screen.gdshader` with a 0.55 px whole-view warp. There is no actual overhead water-surface mesh and no localized light-shaft geometry.

P7's recorded native samples were approximately 54 FPS open, 53 tunnel, 49 shelter and 41 upper on the Radeon 880M at 1920 × 1200. Those historical numbers are context, not a production target or an automatic baseline for this correction. Native closeout must compare the same revision/routes with effects enabled/disabled on the same machine and report the actual delta.

The upper habitat collision currently sits at about Y 9.75. A visual surface must derive placement from the actual habitat/limit rather than inventing a second physics ceiling.

## Prompt order

| Step | Prompt | Outcome | Tier / handoff |
| --- | --- | --- | --- |
| P1 | [Overhead water surface](P1-overhead-water-surface.txt) | Lightweight animated surface plus sky/sun-above read, integrated with existing atmosphere | T2, no native pause |
| P2 | [Organic caustics](P2-organic-caustics.txt) | Replace periodic grid-like projection with bounded irregular moving caustics | T2, no native pause |
| P3 | [Light shafts and performance integration](P3-light-shafts-and-performance.txt) | Cheap art-directed rays, reduced screen wobble, effect toggles/counters and integrated tuning | T2, no native pause |
| P4 | [Native water/light closeout](P4-native-water-light-closeout.txt) | Real-window visual/performance A/B and owner judgment against the target | T3 manual closeout |

Only P4 is this correction's final closeout. Its `Browser required: yes.` metadata means the runner pauses for an actual native Godot/owner checkpoint; it does not mean a web browser can qualify the effect.

## Acceptance boundary

Execution is successful only when the actual game view reads as a stylized sunlit underwater world: looking upward reveals a convincing moving surface and bright source above it, shafts feel spatial rather than like a flat filter, ground caustics are irregular and non-grid-like, and the camera/world remain comfortable and readable while swimming.

Performance acceptance is comparative. P3/P4 must use the same native route/view and source revision with effects on/off, record FPS/frame-time and relevant draw/primitives counters, and tune obvious regressions. A useful guardrail is to investigate and reduce a repeatable median regression around 10% or greater before asking for owner acceptance, rather than hiding cost behind the historical P7 numbers.

Do not mark P2, G2.1–G2.3, V18–V20 or the art-style world-finish checklist passed merely because the shaders compile. This correction supplies targeted implementation/evidence; roadmap status still requires the appropriate owner/native evidence.

## Execute

From a reviewed checkout:

```bash
npm run fish:phase:validate -- c2-watet-light-fx
npm run fish:phase -- c2-watet-light-fx
```

Writing this stack does not execute it. Runner-owned implementation prompts must not self-commit or push. P4 leaves its final closeout reviewable for human approval.
