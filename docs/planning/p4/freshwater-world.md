# P4 — Freshwater Pond and World Selection

**Decision date:** 2026-11-11. **Status:** owner-approved future design; NOT IMPLEMENTED / NOT QUALIFIED. **Dependency:** finish the P3 `c3-world-gen` manual M1–M4 repair and re-qualify the Ocean candidate before beginning P4. **Version:** P3 remains `0.3.6` during corrections; P4 uses the `0.4.x` development line only when deliberately entered.

## Outcome and preservation contract

Expand Fish from the current ocean/reef prototype into a two-habitat aquatic survival game. A new **World Select** step offers **Ocean** and **Freshwater Pond**; each choice leads to a habitat-filtered **Character Select**, then loads the matching environment. Preserve the entire ocean investment: five marine FAF fish, reef art, Forward+ depth/light/water, movement/camera/controls, procedural world compiler, and baked Base World workflow. Do not overwrite, delete, or retheme the Ocean world in order to make the Pond.

Ocean creatures cannot be player-selectable, ambient-spawned or ecological-spawned in the Pond, and freshwater creatures cannot appear in the Ocean. A documented development-only fixture may deliberately bypass those rules for tests, but never as the normal gameplay fallback.

P4 creates a usable new habitat and distinct species/content catalog. Feeding, predator AI, growth, per-species mechanics, and persistence remain subsequent phases.

## Entry and sequencing

1. Finish the existing `docs/tasks/c3-world-gen/manual/` prompts **M1 → M2 → M3 → M4**, one at a time with owner visual review. Do not automatically chain them.
2. Re-run relevant C3 generation/route/visual native qualification; do not fabricate P3 GREEN while acceptance, alternate seeds, visual composition, or visibility remain unqualified.
3. Retain the accepted Ocean world as a baked/loadable selectable option. Preserve generated candidate and Base World provenance, including seed, generator/settings version and resource hashes.
4. Plan and implement P4 against actual P3 source and the accepted compiler. P4's new features must not become a pretext to bypass C3 route/geology defects.

## World entry and navigation

The proposed entry for **New Life** is:

`Main Menu → World Select (Ocean | Freshwater Pond) → Character Select (compatible fish only) → Play`.

For the current tech-demo front door, World Select may be the first interactive selection screen; the future Main Menu still provides Continue, New Life, Life History, Settings and Quit. `Continue` bypasses world/species choice and resumes the saved fish in its original world. World Select is not a world-transfer ability.

Keep explicit Back actions: Character Select → World Select, World Select → Main Menu (when Main Menu is present), and Pause/Game Menu → a safe selection flow for ephemeral P4 demo sessions. Before later save/persistence implementation these are session-routing actions, not death, life abandonment, story creation or recorded migration.

Each world card needs a readable name, representative visual/preview, explicit selection/focus and loading/error handling. Pointer and keyboard must work. No held swimming input or leaked mouse capture crosses menu/world transitions. A failed world load does not quietly drop the player into the other world.

## World and species contract

Keep a **shared finite deterministic compiler and baked loader**. Supply habitat-specific settings and content rather than cloning a second incompatible engine:

- Stable world/habitat identifier (e.g. `ocean` or `pond`), environment art/lighting/fog profile, generator settings/seed/version, baked-resource identity, safe spawn configuration and asset catalog.
- Stable species identifier, native habitat eligibility, FAF asset/animation references and selectable/ambient eligibility. Species/biome matching must be validated by one authoritative compatibility policy used by Character Select, ambient spawning and later ecology.
- Missing/unknown habitat or species tags fail closed, with a visible authoring/load diagnostic; they never mean "allowed everywhere".
- Asset and species identity must not be inferred from a display name or file-path substring. Preserve the current five marine FAF fish and their common P3 visual-only movement/collider envelope for this phase.
- Ocean selection always loads ocean-appropriate resources, atmosphere and life; pond selection always loads freshwater-appropriate resources and life. Shared controls and camera retain authority. Both must survive repeated switching and clean reloads.

## Freshwater pond world

Create an irregular **banked freshwater pond/small lake** that uses the corrected shoreline-first water footprint, distance-to-shore/depth field, real terrain/geology, protected routes, deterministic placement and bake/load workflow.

First visual target: a temperate freshwater habitat with muddy olive-green water, shallows, sand/silt/mud, algae-coated stones, submerged wood/roots, pondweed, reeds/cattails, lily pads/duckweed and darker deep areas. Broad shallow banks and a relatively dark central basin should be distinct; not every point must be equally muddy, deep or dark.

- **Shallow bank:** light-filtered green/golden water, denser plants/roots, navigable hatchling-sized hiding places.
- **Intermediate habitat:** submerged trees/logs, pockets, open swim lanes and cover; structure belongs in the terrain rather than floating as a decorative bridge.
- **Deep pond:** lower light and shorter visibility, sparse plants, subdued geological/wood silhouettes and comprehensible exits.
- Visibility should reveal areas progressively rather than exposing the far bank immediately. Make nearby obstacles, the fish, routes, menu and key silhouettes readable in motion. Water color alone is insufficient; preserve overhead cues and depth/distance extinction.
- Reuse performance-conscious Forward+ rendering and shared water/light systems through independently tunable per-habitat profiles; the pond must not permanently mutate the ocean's saved appearance/settings.
- Every solid object's visible bounds, simplified collision and camera clearance need usable agreement. Do not recreate P3's floating collision/terrain issues.

## Surface swimming and air–water camera transition (P4 requirement)

The pond waterline must be an explorable visual boundary, **not an invisible ceiling**. A swimming fish stays waterborne, but its existing third-person camera can rise into the air while following it. This is a required part of Pond presentation and native qualification, not a separate terrestrial biome.

- **Swimming and contact:** the fish can swim immediately beneath the surface without an abrupt vertical stop, ceiling penetration, jitter or camera glitch. Dorsal fin, back or mouth may partially breach where anatomy allows; ordinary controls cannot launch the whole fish into air. Keep one authoritative underwater motor and physical collision policy. Jumping, full breaches and land travel are later features.
- **Camera:** the follow/orbit/zoom camera crosses the waterline smoothly without snapping, teleporting, jerky rotations, forced reframing or lost fish visibility. Preserve the existing control presets (including Adaptive Follow), obstacle avoidance and steady horizon. Camera movement cannot displace the fish or bypass geometry.
- **Air–water transition:** smoothly change underwater green fog, distance absorption, screen distortion and lighting to above-water air/sky rendering using a stable transition band. Repeated skimming/crossing and pausing must not cause flicker, popping, clipping or harsh exposure shifts. On submersion, restore the existing caustics, shafts, underwater depth effects and legible UI. Immersion state is presentation-only.
- **Fish-reactive surface:** reuse water-reaction effects where practical for small position/velocity-driven ripples, a bounded trailing wake and optional light splashes during partial breaches. Do not require expensive fluid simulation or feed visual impulses into authoritative motion. Effects must behave correctly across pause, focus and session reset.
- **Visible shoreline:** above the water, render a believable near section of the **actual generated shoreline**, including mud/grassy banks, cattails, reeds, floating leaves/lily pads, atmospheric sky and daylight/cloud cover. Use distance atmosphere to hide far/unfinished bounds; no second artificial perimeter or fully explorable land world. Bank/plant visual bounds and physical clearance should agree.
- **Future bird encounters:** allow later bird silhouettes, shadows, ambient calls and diving/hunting encounters to attach to the surface scene, without implementing bird detection, attacks, damage, pursuit or full predator AI in P4.
- **Habitat reuse:** Pond is the first required surface qualification. Camera crossing, immersion profiles and surface reactions should be shared/configurable rather than duplicated. Keep Ocean's underwater controls, lighting and water intact; optional Ocean above-surface scenery is a separate future qualification, not automatically complete.
- **Performance:** favor existing Forward+ and inexpensive surface effects; avoid gratuitous secondary scene rendering. Native qualification must include frame pacing and errors during waterline crossing.

## Freshwater Fish Asset Factory

Preserve the marine reef style contract as Ocean's style. Extend the **shared stylized low-poly aquatic naturalism** geometry/rigging/export rules with a freshwater palette and species-specific anatomy. Build original Blender masters/GLBs and provenance records under new, unambiguous FAF identities rather than overwriting existing ocean assets.

Initial pack families:

1. **Terrain/details:** mud, sediment, pebbles, algae-coated rock, eroded banks, leaf litter.
2. **Wood and cover:** fallen trees, hollow logs, stumps, exposed roots, branching cover.
3. **Plants:** pondweed, reeds, cattails, lily pads, duckweed and freshwater algae.
4. **Fish:** a small, deliberately bounded native roster such as minnow, bluegill/sunfish, largemouth bass, perch and catfish. These are candidate species, not all mandatory before an initial playable slice.
5. **Non-fish life:** representative frog/tadpole, turtle, water snake, crayfish and aquatic insects, prioritized by visual/gameplay value and cost. Their P4 role may be ambient/presentation only.

Animal movement should be plausible for each body type; don't turn every turtle/snake/frog into a fish or silently give it full predator gameplay. Future fish assets must follow existing gill/breathing rigging standards where anatomically appropriate. Asset count, performance, collision and review evidence belong to the eventual P4 implementation stack.

## Proposed acceptance Gates

### G4.1 — World selection and independent loading

- World Select offers both Ocean and Pond and precedes Character Select for a new demo session.
- Each world loads its distinct baked resources and atmosphere without regenerating on ordinary entry.
- Switching back and forth does not leak old scene objects, fish choice, input, fog presets or spawn state.
- Existing Ocean plays as before; no deletion/regression of the five marine FAF character previews.
- Failed/missing world resources report an error rather than fallback to the wrong environment.

### G4.2 — Freshwater terrain and presentation

- Pond has recognizable irregular banks, coherent underwater depth, supported natural geometry, roots/logs/vegetation clusters, open routes and believable hiding spaces.
- Shallow/middle/deep views are distinct; murky visibility hides distant bounds without obscuring close obstacles or player.
- Route/collision/camera validation and bounded performance pass on real native Forward+ play.
- The generator and baked load remain deterministic and repeatable for an explicit seed/settings/version.
- Fish can cruise just below the surface, with limited fin/back/mouth breach but no ordinary airborne swimming; no invisible ceiling or camera snap occurs.
- The third-person camera crosses above and below the surface smoothly, preserving fish visibility, camera collision, control presets and underwater restoration.
- Fish-reactive ripples/wakes remain bounded and believable; nearby generated banks, cattails/reeds, floating plants and sky are visible above water while distant bounds remain concealed.
- Repeated waterline crossings preserve readable UI, stable native Forward+ performance and ocean rendering without regression. Qualify these with V32.

### G4.3 — Original freshwater asset pack

- A representative native freshwater player fish, coherent vegetation/terrain/wood kit and ambient life are present with source/rigging/GLB/manifest checks.
- Reusable FAF construction rules are applied; assets remain identifiable, style-consistent and correctly placed.
- Ocean assets remain intact and available only in their compatible habitat (shared generic geometry may be explicitly marked habitat-neutral).

### G4.4 — Habitat-restricted characters and regression

- Character Select filters to native species of the chosen world, including after navigating Back, switching worlds and restarting the app.
- Ambient and future spawn-entry policies use the same fail-closed compatibility check.
- Negative tests prove no marine species in Pond and no freshwater species in Ocean during ordinary play.
- Play both worlds with real controls, return to selection and review visuals/clearances in native desktop qualification.
- Preserve P1 movement/camera, P2 ocean/Forward+ effects, P3 procedural/baked asset work and truthful Phase/Gate evidence.

## Validation outline

Add five P4 scenarios (V28–V32) in `docs/validation.md`: world routing/loading/reloading, pond world traversal/atmosphere, FAF asset+performance proof, cross-habitat species checks, and a dedicated native surface-swimming/air–water camera transition trial. Use focused manifest/registry tests followed by native gameplay and owner visual judgment. HEAD/source revision, test seeds, viewport/settings, performance/error information and observed gaps must be recorded. No authored plan alone qualifies G4.x.

## Subsequent phases and save boundary

Existing feed/escape/grow work becomes **P5**; persistence/death becomes **P6**; factual Life History becomes **P7**; integrated experience qualification becomes **P8**. Their mechanic and acceptance intent is preserved, with Gate references and future validation mappings shifted accordingly.

At P6, a living run stores both stable species identity and **selected habitat plus generated/baked-world identity**. Continue restores that same world and fish directly, without World Select as a migration prompt. World changes cannot reset pursuit, consumed targets, growth or death state; invalid/incompatible content is an error/recovery condition, not an automatic species conversion.

## Explicit deferrals and open implementation choices

No P4 feeding economy, predator combat (including bird attacks/hunting), aerial fish movement/jumping, terrestrial exploration, advanced fluid simulation, species stats, growth balance, breeding, full wildlife behavior, living-save implementation or history system. Those remain later scoped work. Implementation may begin with a small freshwater species subset and expand after native visual review. Exact fish roster, biome size/visibility numerical presets and creature density remain tuning/asset-planning choices, not unreviewed production commitments.
