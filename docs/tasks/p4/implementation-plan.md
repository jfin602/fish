# P4 implementation plan — Freshwater Pond and World Select

**Scope:** execution plan for [P4](README.md), following the approved [design](../../planning/p4/freshwater-world.md), [FAF style](../../fish-asset-factory/art-style.md) and [V28–V32](../../validation.md). **Status: authored, unexecuted.** Root remains `0.3.6` at authoring; do not perform phase-entry `0.4.0` until corrected Ocean P3 qualification/owner authorization is established. The runner's manual stops are real gates, not auto-completion.

## Entry, source and test preflight

Read AGENTS, BOOT, workflow, runner, task README/assessment, P4 design, current P3/C3 closeout and manual repair records, relevant FAF records and only affected code. Inspect local branch HEAD, dirty tree, root version and all prior P3/c3 changes before editing. Check actual `tools/dev` commands, Godot 4.7.2 .NET/SDK availability, and Fish-only MCP. Never use an editor connection to Space Factory.

P4 begins from an accepted/reconciled P3 Ocean baked world; exact world/loader/render scene paths may have changed since BOOT. Make no assumption that the provisional `qualification-42` base world or seed 42 is approved. Default to T1/T2 focused checks; only claim native visual PASS from actual running game, input and owner judgment.

## Implementation order and source boundaries

### P1 — World definitions and baked loader (`0.4.1`)

Introduce the smallest world identity/catalog with stable `ocean` and `pond` identifiers, environment profile, seed/generator/settings/baked identity, selection preview, spawn, relevant FAF catalog and allowed world roles. Adapt existing loader to select a **previously baked** resource by world identity without hidden generation. Preserve existing ocean resource hashes and fail closed for missing/incompatible manifests. Add world selection session state independent of fish/play scene. Focused test: both manifests, invalid world, absent bake and no generator invocations.

### P2 — World Select and navigation (`0.4.2`)

Create World Select before Character Select; maintain in-progress P4 tech-demo routing distinct from future Main Menu/New Life/Continue behavior. Ocean and Pond cards with readable name/thumbnail/focus, keyboard + pointer, explicit Back, loading/errors, correct pointer capture/held input release. Ocean works with existing five FAF fish. Pond may temporarily show an explicit no-compatible-fish state before P8; never borrow Ocean fish as fallback.

### P3 — Habitat/species enforcement (`0.4.3`)

Extend the actual CharacterCatalog and ambient spawn source with stable species ID, native world(s), selectable/ambient flags and optional display/visual metadata. Centralize check in one fail-closed resolver; no path-string eligibility. Existing FAF-FISH-001–005 are Ocean-only. Add test-only temporary freshwater fixture if necessary, explicitly excluded from shipping FAF claims; any Pond player species subsequently needs an authentic freshwater GLB. Test filtered catalog, invalid tags, repeated navigation and ambient negative paths.

### P4 — Pond world generation and baked identity (`0.4.4`)

Use the **accepted corrected** shoreline-first compiler. Pond-specific irregular outline/depth-inward settings, meaningful shallow banks, deeper central water, route and clearance anchors, wood/plant suitability, terrain/collision and deterministic seed variation. Save as a Pond-specific compiled/baked package; do not bake into/overwrite Ocean's Base World path. Measure compiler stage times and native loader time, hash/identity, multiple successful seeds and no ordinary runtime generation. Do not resurrect prior fixed-radius bowl or stale C3 route hacks.

### P5 — Pond Forward+ atmosphere (`0.4.5`)

Build independent dark olive-green/brown, cloudy-day settings with shallow golden filtered light and deep low-visibility water. Reuse UnderwaterAtmosphere/LightingSettings/shaders through per-world profiles, not global palette rewriting. Motes, shafts and caustics bounded; UI crisp. Compare Ocean snapshot before/after Pond→Ocean return. Focused depth/visibility, GFX settings migration, preset leakage and renderer-independent motor tests.

### P6 — Lean freshwater FAF world kit (`0.4.6`)

Create a **small original first-pass** family of stylized low-poly mud/stone/detail, fallen logs/stumps/roots, lily pads, reeds and submerged pondweed/algae using tracked Blender source/generator/export. Integrate GLBs through terrain-aware deterministic clusters and support projection. Decorative plants cheap/no heavy collision; route-critical logs/roots get credible simplified colliders and camera clearance. Prefer representative variants over an exhaustive catalog. Record provenance and technical checks, no claim of final native style approval.

### P7 — Native Pond visual review (`0.4.7`, manual)

From real fish camera, inspect overcast lily canopy, sunny patches, tangled root corridor, open middle and deep basin, at least one shore approach, cave/log passage and generated perimeter. Compare against owner-provided reference pack if actually available. Verify real control/route/collision, fog reveal/readability and performance; return to Ocean and confirm no atmosphere contamination. Record native captures, seed/settings/owner judgment and defects; fix scoped issues before continuing.

### P8 — Freshwater fish and ambient wildlife (`0.4.8`)

Deliver at least **two genuinely freshwater** first-party selectable fish candidates (small minnow/sunfish-like is a sensible initial pair), with GLB, Blender source, fin/tail/body/gill rigging/animation as anatomically appropriate, normalized into P3's shared demo motor/collider envelope. Add at least **one** tiny passive ambient freshwater life form (e.g. tadpole or aquatic insect) plus optional species variety if time permits. No predator mechanics, health, combat or saves. Integrate as Pond-only species, retain all Ocean five. Source tests and isolated native view are prerequisites for claiming integration.

### P9 — Waterline-constrained swimming (`0.4.9`)

Replace the Pond's invisible ceiling notion with an authoritative fish waterline occupancy constraint at the actual water surface. The fish may skim just under and partially expose its fin/back/mouth where its shape permits, but normal input must not move the entire body into air. Constraint derives from body shape/position/depth, not the camera; preserve Swimmotor behavior at other depths, collision at banks, pause/input and Ocean behavior. Add deterministic high-frequency repeated surface-crossing tests.

### P10 — Camera air/water immersion (`0.4.10`)

Add camera-state immersion sampling using actual camera world position and water-surface height, with a stable interpolation band/hysteresis to avoid rapid flicker. Camera follows/orbits/zooms above and below without fish root motion, snapping, bad horizon or clipping. Blend underwater fog/color/caustics/shafts/screen treatment to air without distorting UI, including when camera is above and fish remains under water. Existing Mouse Only, Keyboard, Hybrid, Hybrid 2 and Adaptive Follow presets keep authority. Test repeated crossing/oscillation and reset/return.

### P11 — Above-water shore, sky and fish-reaction (`0.4.11`)

Expose nearby actual generated Pond bank when viewing above-water; add low-poly reeds/cattails, overhanging roots and sky/overcast daylight. Prefer one water surface with correct below/above normals, subtle reflections, shoreline blending and near-surface plant placement. Add capped location/velocity-driven wake, ripple rings and occasional tiny splash for partial fin breach without physics forces. Distant unfinished limits must remain concealed. No full terrestrial world, bird combat, expensive fluid simulation or required Ocean above-water expansion.

### P12 — Native waterline/camera qualification (`0.4.12`, manual)

Full V32 native controls: swim to surface, skim, turn, follow/orbit/zoom camera above and back below repeatedly; exercise fin/mouth partial breach but not flight, banks/cattails clearance, refocus/pause, multi-preset controls, graphics toggles and ocean regression. Look for flicker, pop, exposure/fog discontinuity, invisible ceiling, camera snap/jitter, input leak, wake resets, screen UI distortion and frame pacing. Capture actual source/settings/images and owner acceptance; do not claim GREEN on a headless check.

### P13 — Two-world integration/performance (`0.4.13`)

Run focused V28–V31 matrix: both manifests/loader, repeated world switches, all Ocean fish positive/Pond negative, all freshwater fish positive/Ocean negative, spawn/ambient registry, unsupported data, asset/bake integrity, route probes, per-zone perf, memory/resource cleanup and compile/bake/load timing. Compare one representative Ocean native view or prior same-source evidence and Pond source. Fix bounded regressions; significant visual changes require another native owner checkpoint.

### P14 — Phase 4 closeout (`0.4.14`, manual)

Map evidence to G4.1–G4.4/V28–V32, separate PASS/FAIL/NOT OBSERVED/BLOCKED and owner decisions, retain previous C3 and P2 evidence including G2.3/V20 gap. Reconcile roadmap, BOOT, MODULES, art/reference records, UX and task evidence only for actual implemented facts. Leave diff reviewable and closeout uncommitted. P5 feeding/ecology is next only if owner authorizes progress on the truthful result.

## Shared scope exclusions

No fish combat/predator AI (including birds), feeding/growth or species stats; no save/death/history; no new Ocean art replacement; no controller or movement-ability rework; no fish jumping, fully airborne locomotion, land traversal or physical fluid simulation; no assumption that a render image proves engine implementation.

## Performance and review rules

Measure native Forward+ on identified machine in overcast shallow plant density, log/root passage, deep murk, above-water shoreline and Ocean return. Prefer shared resources/MultiMesh, low-overdraw foliage, cheap shader fog/screen transition and bounded effects. Maintain gameplay readability. Record actual performance rather than inventing FPS budgets. Implement consistent visual style with current owner FAF reference pack; if reference images are not in repo yet, report it and use approved written style, not fabricated file paths.
