# P4 prompt assessment — two habitats and surface water

**Assessment 2026-11-11. Status: authored, not executed.** The owner requested `/prompt-ass + /prompt-plan + /prompt-write p4` after selecting a dark, ominous-but-beautiful freshwater Pond alongside the preserved Ocean. The new surface-camera/waterline requirement is in the approved [P4 design](../../planning/p4/freshwater-world.md) and [V32](../../validation.md).

## Player outcome

New Life enters World Select, chooses Ocean or Pond, chooses a native fish, then swims in the selected world. The Ocean remains unchanged as an available world. Pond has an irregular shoreline, algae/mud/logs/roots/lilies/reeds, dark olive-green water with limited visibility and a visible above-water shoreline. The fish swims at the surface with limited partial breach; the camera can rise above water smoothly, see sky and near banks, then return underwater without clipping or harsh fog/light jumps.

## Current reality and prerequisites

The documented root is still `0.3.6`; P3 C3 seed-42 candidate was not accepted; alternate seeds/route reliability, generated geology and world aesthetics failed; V25 visibility remains unqualified. Four manual repairs M1–M4 and native re-review precede P4. P2 G2.3/V20 remains a historically accepted open gap. **No new P4 phase baseline may be inferred from these documents.** At execution inspect local HEAD/dirty tree and verify a reviewed P3→P4 handoff, retained Ocean bake and deliberate `0.4.0` root baseline.

Existing reusable source (recheck at execution):
- `WorldGeneration`, `BakedWorldContract`, `BakedWorldLoader`, `FullWorldPackage`, `GeneratedBasin`, terrain/geology/route/dressing compiler.
- `CharacterCatalog`, `CharacterSelect`, `DemoSelection`, `PlayerArt`, `PlayShell`.
- `FishController`, `SwimMotor`, `FollowCamera`, control presets including Adaptive Follow, `WaterReaction`.
- `UnderwaterAtmosphere`, `LightingSettings`, existing Forward+ water, shafts, caustics, fog and screen pass; FAF Blender/GLB tooling.
Exact current paths and method ownership are to be traced, not assumed unchanged.

## Boundaries to protect

1. **Ocean never replaced.** Separate world resources and visual profiles; preserve all five marine FAF characters and ocean art, previously accepted controls/renderer and P1 course fixture.
2. **No species leakage.** One explicit eligibility policy for Character Select, ambient spawn and future ecology; unknown tags fail closed. Pond player art must be a freshwater species, not a renamed Ocean tropical fish.
3. **One authoritative motor.** Surface contact constrains the real fish body; camera/weather/post passes cannot add motion or turn pause into swimming. No aerial fish flight or land locomotion.
4. **Shared compiler/load contracts.** Pond customizes shoreline, depth, vegetation and atmosphere without a copy-pasted second generator. Bake/load identity persists and normal entry never compiles live.
5. **Performance/readability.** Murky water hides far boundaries but preserves immediate fish and hazards. Use low-poly assets, cheap effects, coherent collision and native measurements.
6. **Life semantics.** P4 session navigation is ephemeral; future P6 Continue restores original world/species/baked identity. No save/death/story or predator/feeding system in P4.
7. **Truthful qualification.** Native waterline/camera stability, owner look/feel, two-world re-entry and rendering need real observed evidence. Compiled fixtures cannot certify them.

## Decomposition and validation

| Task | Critical result | Tier |
| --- | --- | --- |
| P1 | World manifest, baked resource and failure contract | T2 |
| P2 | New World Select screen/return routing | T2 |
| P3 | One fail-closed compatibility policy | T2 |
| P4 | Seeded Pond formation, shoreline and baked resources | T2 |
| P5 | Habitat-specific water/light/fog profiles | T2 |
| P6 | Lean freshwater environment FAF/placement | T2 |
| P7 | Native world-art and traversal review | T3 / manual |
| P8 | Rigged freshwater player fish and bounded ambient life | T2 |
| P9 | Authoritative near-surface swim/partial breach | T2 |
| P10 | Follow camera air–water transition and screen state | T2 |
| P11 | Generated shoreline/sky plus cheap surface ripples | T2 |
| P12 | Native surface/camera/preset/performance review | T3 / manual |
| P13 | Multiworld regression and resource-performance integration | T2 |
| P14 | Gate evidence closeout | T3 review / manual |

## Material risks and tests

- **Start on broken P3 Ocean:** hard verified dependency/owner decision; never fabricate completed status.
- **False baked world identity/stale cache:** manifest/version/resource hash test, negative missing/incompatible and no generator call at load.
- **Select wrong habitat:** explicit species/habitat tags and cross-world negative selection/ambient tests, including Back/reload/error paths.
- **Partial world load leaves mixed scenes/effects:** unload/reset assertions and visual reentry to Ocean.
- **Pond becomes a recolored Ocean reef:** own biome settings, structural wood/mud/lily assets and distinct native review.
- **Pond is too dark:** three-depth near/mid/far readability native owner checkpoint and adjustable independent profile.
- **Terrain and log collision lie:** real surface projection/support and fish-camera sweep/route-clearance tests.
- **Assets block execution:** first cheap primitive/placeholder in P4/P5, then original tracked FAF minimal pack in P6/P8; never claim placeholders as accepted final models.
- **Surface invisible ceiling or fish launch:** explicit body shape clearance and physically enforced surface cap; repeated upward glide, turn and pause tests.
- **Camera oscillates between air/water:** hysteresis/interpolation/continuous sampled water height with no root movement; repeated crossing and frame pacing test.
- **UI/sky/underwater wrong blend:** camera-only immersion state, independent per-world presets, restoration after reentry, no full screen flicker.
- **Ocean surface accidentally claimed:** Pond V32 is required; Ocean above-water remains separately unqualified.
- **Fourteen prompts become unbounded:** narrow changed owners, focused T1/T2 probes, manual checkpoints at P7/P12; stop and record blockers rather than smuggling scope or inventing progress.
- **Runner history corrupted:** exact `0.4.0` entry, sequential `0.4.1–0.4.14`, no self-commits/pushes inside prompts; human review at final closeout.

## Acceptance evidence

Map V28–V32 and G4.1–G4.4 in the roadmap **only after** source-bound checks and owner/native observations. A visually convincing isolated Blender image does not prove a real traversable Godot habitat. No P4 completion is implied by writing this assessment.
