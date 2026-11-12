# Fish roadmap

**Status: 2026-11-11. P0 and P1 are COMPLETE. P2 and P3 are OWNER CLOSED FOR PROGRESSION / NOT GREEN.** P2 G2.3/V20 remains an accepted gap. P3/P1–P6 and C3 M1–M4 are implemented; the corrected Ocean `ocean-m4-42` is the P4 foundation, but G3.1–G3.3 remain FAIL / NOT GREEN and G3.4 remains NOT GREEN. M4 repaired the earlier alternate-seed bake failure; its shallow geological face and cave transitions still fail visual criteria, full native traversal is not observed, and V25 visibility remains unqualified. The owner explicitly approved P4 progression with these gaps retained; [P3 direct closeout](tasks/p3/closeout.md) owns the decision. **P4 P12 native V32 review remains FAIL / NOT GREEN at root `0.4.11`; no P4 Gate is qualified.** P12-R1 repairs air-side fish visibility and Pond near-field lighting; R2 darkens the Pond with depth-based muddy-green transmission after owner rejection of pale trials. Fixed native views pass focused checks, but owner judgment of the final R2 image, continuous deep/return traversal and controlled camera review remain open. [P12 native evidence and R1/R2 addenda](tasks/p4/qualification/native-surface.md) own the result and limits. This is the single Phase/Gate status authority.

**2026-11-12 surface-candidate update:** [P12-R3](tasks/p4/qualification/r3-surface-reflections.md) adds a reversible Pond reflection/sky experiment from saved checkpoint `cc8cbb9`. The pinned .NET build and four affected state checks pass after restoring their approved local asset inputs. Fifteen actual baked-Pond fixed-pose captures on software Forward+ supplement the isolated shader diagnostic; owner-hardware cost, motion stability and controlled play remain open. P12/V32 are still **FAIL / NOT GREEN** at `0.4.11`; no Gate or runner advancement follows. The [R3 native handoff](tasks/p4/manual/R3-surface-reflections-native.txt) is the next surface review route.


A Gate passes only with source-bound evidence for all of its criteria. A Phase passes only when all required Gates pass. [Validation](validation.md) defines scenarios; task qualification records hold actual results.

## Delivery direction

Build one small, complete fish life in deliberate layers: first make swimming enjoyable, then make the place worth swimming through, then add the food chain, persistence/death, remembered lives, and full integrated qualification. The current P2 decision intentionally puts the underwater world before ecology. The owner then selected P4 to add World Select and a freshwater Pond alongside the preserved Ocean; feeding/predators/growth move to P5 after both habitats are established.

| Phase | Observable outcome | State |
| --- | --- | --- |
| P0 — Godot and MCP foundation | The real 3D project builds, opens, runs and can be inspected through the correct editor connection. | COMPLETE on Linux/X11; [closeout](tasks/p0/closeout.md). |
| P1 — Swim through a habitat | Settle core swimming, third-person camera/body motion, selected starter assets and fullscreen/input lifecycle. | **COMPLETE; [closeout](tasks/p1/closeout.md).** |
| **P2 — Build the underwater world** | Replace the prototype-course look with a natural reef space, convincing underwater atmosphere, and movement-reactive water presentation. | **OWNER CLOSED FOR PROGRESSION / NOT GREEN**; G2.1/G2.2 pass, G2.3/V20 is an accepted open gap under S16. |
| **P3 — Rebuild and dress the reef** | Rebuild the accepted varying-depth world with the complete original FAF environment library, caves/tunnels and depth-based living density, plus a five-fish Character Select and clean return-to-select flow. | **OWNER CLOSED FOR PROGRESSION / NOT GREEN** at implemented `0.3.6`; [direct closeout](tasks/p3/closeout.md) preserves failed and unobserved criteria, with no P7/P8 runner checkpoints. |
| **P4 — Freshwater Pond and World Select** | Add Ocean and Pond as distinct selectable worlds, compatible fish/ambient populations, freshwater assets, bank/sky and surface-swimming/camera immersion. | **IN PROGRESS / P12 V32 FAIL, NOT GREEN:** root `0.4.11`; R2 darkens the Pond with depth-based transmission; owner judgment of the final candidate, camera comfort and continuous swim routes remain open. No P4 Gate qualified. [P12 evidence](tasks/p4/qualification/native-surface.md), [P11 implementation](tasks/p4/qualification/implementation.md), [P8 intake](tasks/p4/qualification/integration.md), [P7 visual review](tasks/p4/qualification/native-pond.md). |
| P5 — Stealth, feed, escape and grow | Naturalistic detection, concealment, investigation, pursuit/escape and stealth hunting anchor the feeding/growth loop in compatible habitats. | NOT STARTED; formerly P4. |
| P6 — A life across sessions | Continue one living fish in its original world; death permanently ends it with recoverable durable records. | NOT STARTED; formerly P5. |
| P7 — Remember every life | Show a brief true recounting and revisit all saved stories from Life History. | NOT STARTED; formerly P6. |
| P8 — Qualify the complete experience | Observe the integrated life loop, optional first person and chosen desktop presentation across the selected habitat scope. | NOT STARTED; formerly P7. |

## P0 — Godot and MCP foundation

**Status: COMPLETE on the observed Linux/X11 desktop.** [Foundation design](planning/p0/foundation.md); [p0 closeout](tasks/p0/closeout.md). Scenarios B01–B04 passed as recorded in the P0 evidence.

### G0.1 — Reproducible 3D project and CLI

**Result: PASS.**

- Godot .NET/C# project and coherent engine/SDK pins exist under `game/`.
- The repository-relative developer CLI imports, builds, checks, opens the editor/runs the game, and performs bounded scene automation with meaningful failures.
- The foundation scene, C# execution and saved-scene round trips were observed at the documented source/platform boundary.

### G0.2 — Correct, isolated Godot Editor MCP connection

**Result: PASS.**

- Selected server/addon components and Fish-local routing were configured without replacing Space Factory settings.
- A live peer identified this Fish checkout before editor mutation; scene inspection/save/reload succeeded.
- C# compilation remains an actual .NET build rather than an MCP-connectivity inference.

### G0.3 — Native development loop

**Result: PASS.**

- The actual Fish editor/main scene ran on Linux/X11.
- Prototype horizontal/vertical movement, release, runtime state, running-game pixels and clean stop were observed.
- This qualifies the development loop, not final swimming/world quality.

## P1 — Swim through a habitat

**Status: COMPLETE; G1.1–G1.4 PASS for the owner-accepted progression baseline.** [P1 closeout](tasks/p1/closeout.md) owns the disposition, [accepted tuning](tasks/p1/qualification/accepted-tuning.md) owns the actual inherited values, and the [P1 stack](tasks/p1/README.md) preserves authored history. P1–P9 were directly implemented; c1 controls were reconciled; the owner directly accepted the current candidate on 2026-11-09. The originally authored P10 iterative tuning session was superseded, and P11 evidence review was performed directly. No `0.1.10`/`0.1.11` runner checkpoints are claimed.

The completed controls worksheet established the qualitative core. Existing source-bound checks, bounded Linux/X11 native observations and the owner's explicit acceptance establish the baseline for progression. The authored full per-mode route/duration and display/input matrix was not completed; its gaps remain explicit in the closeout and later refinement is deliberately deferred. P2 preserves this baseline while testing the fish in a natural world.

### G1.1 — Swimming and physical space

**Result: PASS for the accepted P1 baseline.** [Gate evidence and limits](tasks/p1/closeout.md#candidate-and-review-boundary).

- Source-bound motor/scene checks establish acceleration, coast, active brake, curved yaw, pitched travel, rock collision, gap clearance, bounds and pause behavior.
- Bounded native c1 observations show keyboard, mouse and Hybrid input responses; one Hybrid route passed the gap.
- Owner acceptance closes the progression decision. A completed same-course native route in every profile was not recorded; later refinement remains open.
- Camera and rendering remain read-only with respect to the authoritative fish; source-bound checks cover that isolation.

### G1.2 — Third-person readability

**Result: PASS for the accepted P1 baseline.** [Gate evidence and limits](tasks/p1/closeout.md#candidate-and-review-boundary).

- Native P10 captures and owner feedback rejected the sideways and shoulder candidates and favored the centered elevated view.
- Saved-scene camera and animation checks establish follow/obstruction, horizon and Fish3 swim/glide/turn motion without root movement.
- The owner directly accepted the current camera/body candidate for progression; per-mode several-minute durations were not recorded.

### G1.3 — Selected ocean assets in the running course

**Result: PASS for the accepted P1 baseline.** [Gate evidence and limits](tasks/p1/closeout.md#candidate-and-review-boundary).

- Quaternius `Fish3` / `ClownFish`, Rock Face 01, Coast Sand 01 and Smithsonian *Heliopora coerulea* are the required P1 set.
- Provenance/preparation/local installation/public-source boundaries remain recorded.
- P5 native observation established visual integration, missing-resource diagnostics, collision paths and a representative local performance sample; later source checks retained readiness. The sample predates the final c1 controls and expanded box.
- These starter assets are inputs to P2 composition; they do not by themselves make a finished world.

### G1.4 — Fullscreen presentation and input lifecycle

**Result: PASS for the accepted P1 baseline.** [Gate evidence and limits](tasks/p1/closeout.md#candidate-and-review-boundary).

- Fresh-profile fullscreen and windowed-fullscreen requests were observed as borderless full-display windows on the tested X11 backend; exclusive monitor ownership was not proven.
- Native P5/c1 sessions observed menu, focus, display and basic profile input paths; saved-scene checks cover disarm/rearm, first-frame neutral input and switching.
- Advanced c1 native switching/editor and exhaustive four-cell comfort checks remain unobserved and intentionally deferred.

P1 completion records the final accepted tuning/profile. [Movement abilities](planning/p1/movement-abilities.md) remain approved future movement work, but the owner has superseded their immediate scheduling: **P2 worldbuilding is the next roadmap Phase after P1**.

## P2 — Build the underwater world

**Status: OWNER CLOSED FOR PROGRESSION / NOT GREEN.** The owner explicitly authorized moving to P3 at root `0.3.0` while preserving G2.3/V20 as an accepted evidence gap. No missing V20 observation is converted to PASS. The original P2 implementation and owner-green [water/light correction](tasks/c2-watet-light-fx/closeout.md) are retained. [c2-depth](tasks/c2-depth/README.md) P1–P3 implemented the five-band candidate and depth lighting; [P4 native review](tasks/c2-depth/qualification/native-depth.md) historically failed unreachable endpoints and visual criteria. [R1](tasks/c2-depth/qualification/r1-repair.md) repaired the candidate; its [native closeout](tasks/c2-depth/qualification/native-depth.md#r1-addendum--repaired-candidate-native-observation) passed actual authored routes and received the owner's direct acceptance of terrain, depth presentation, effects, controls/UI and smoothness. G2.1 and G2.2 pass for this source; G2.3 remains open. [P2 design](planning/p2/underwater-world.md); [execution stack](tasks/p2/README.md).

P2 turns the movement laboratory into a place that already feels like a game when the player simply swims around. The target is a compact authored reef-like habitat; it does not settle the final production biome or larger procedural-world strategy.

### G2.1 — Natural reef habitat and traversal

**Result: PASS for the R1 observed candidate.** P3/P4 established the continuous collidable seabed, separate P1 fixture and complete prepared model/material placement. The [c2-depth P4 native review](tasks/c2-depth/qualification/native-depth.md) historically failed because both lanes' shallow/deep markers were beyond the ±24.9 m fish bound and the visible terrain read as terraces/rim. The later [R1 native run](tasks/c2-depth/qualification/native-depth.md#r1-addendum--repaired-candidate-native-observation) reached every repaired authored marker, returned on both lanes, and crossed tunnel/shelter using normal key events. The owner confirmed direct hand-play of those routes and accepted naturalness, perimeter, collision and camera comfort. Focused containment/route checks passed. V18 passes; the [P8 criterion review](tasks/p2/closeout.md#g21--natural-habitat-and-traversal--v18) remains historical baseline evidence.

- Normal play no longer exposes the P1 flat-floor/ceiling/test-course presentation, colored target spheres, camera-check post or other obvious developer geometry.
- Build a continuous natural seabed with varied elevation, coral/reef areas, rock walls/formations, arches or tunnels/caves, overhangs, open swimming pockets and recognizable landmarks at multiple depths.
- The default basin contains **at least five natural traversable reef ledges/shelves at distinct elevations**, with irregular slopes, cliffs, channels/drop-offs and alternate transitions rather than a visible staircase. Useful viewpoints can read several depth bands together.
- The player cannot escape beneath the seabed or through intended world limits. Visual geometry and simplified collision agree well enough that tunnels, cover, shelf transitions and camera clearance are understandable.
- At least one broad route, one tighter rock passage/tunnel and a continuous shallow-to-deep-to-shallow vertical route can be traversed comfortably with the accepted P1 fish/camera.
- Preserve the P1 movement course as a dedicated qualification/development fixture or equivalent; removing it from normal presentation must not erase historical evidence.

### G2.2 — Convincing underwater atmosphere and renderer

**Result: PASS for the R1 observed candidate.**

P5 established the Forward+ atmosphere foundation, and the owner-green [c2 water/light correction](tasks/c2-watet-light-fx/closeout.md) added the animated overhead surface, organic moving caustics, caustic-aligned shafts and GFX presets. The [P4 record](tasks/c2-depth/qualification/native-depth.md) historically found no distinct bright deep overhead surface. The [R1 native addendum](tasks/c2-depth/qualification/native-depth.md#r1-addendum--repaired-candidate-native-observation) captures cooler/dimmer shallow-to-deep views, a bright region in two fixed deep upward angles and warmed same-source performance counters. The owner directly accepted horizontal range progression, several-band world-depth read, deep upward orientation in motion, caustic/shaft attenuation, GFX/preset comfort and smoothness. Focused water-column and unchanged-movement checks passed. V19 passes for this candidate; the 60 Hz cap and prior varied samples remain measurement limits.

- P2/P1 provides the Compatibility/Forward+ comparison. The owner ran Forward+ on the real desktop, observed smooth performance and selected it for P2. P2/P2 locked the tracked project to Forward+ and recorded that evidence; another native Compatibility run is not required unless Forward+ later develops a concrete blocker.
- With **Forward+**, combine distance/depth haze or fog, underwater color/contrast treatment, directional light from above, moving caustic-style illumination, suspended water matter, and restrained full-view water treatment so near/far/deep views read as submerged rather than merely blue-tinted.
- Actual world depth below the real water surface changes the presentation: shallows stay brighter/warmer with strong caustic/shaft cues; lower ledges become progressively cooler/dimmer and those surface-light effects weaken.
- View-distance extinction is separately depth-sensitive: comparable horizontal views lose contrast/range progressively as the camera descends, with far reef masses transitioning detail -> silhouette -> water instead of meeting a visible fog wall.
- Several ledges visible in the same frame can read as different depth bands; camera-depth changes alone must not make every world elevation share identical lighting/color.
- The effect stack remains comfortable in motion and preserves readable fish/world silhouettes. HUD/pause/settings remain crisp and are not accidentally treated as geometry inside the water.
- Renderer/presentation changes do not change authoritative swimming, time, input semantics or fish identity. Representative native performance is recorded on the depth-structured P2 habitat.

### G2.3 — Movement-reactive water presentation

**Result: ACCEPTED GAP / NOT GREEN.** [P6 implementation and T2 evidence](tasks/p2/qualification/implementation.md#p2-p6--fish-reactive-water) covers bounded GPU wake/bubbles, pause/focus cleanup and equal-intent movement traces. P7 recorded native response ratios and round-bubble captures. [P8 review](tasks/p2/closeout.md#g23--fish-reactive-water--v20) retains missing owner effect judgment, combined pitch/turn geometry view and actual desktop focus/refocus. On 2026-11-10 the owner accepted this gap for progression and authorized the `0.3.0` P3 entry.

- Water visibly reacts to the fish using bounded presentation effects: ordinary swimming has a readable but restrained disturbance, acceleration increases it, gliding reduces it, and hard turning produces a directional change/turbulence cue.
- Effects originate from believable body/tail space, remain coherent through pitch/yaw, do not obscure the camera, and clear/freeze appropriately across pause/reset/focus lifecycle.
- The effect reads movement state without applying forces, modifying collision or becoming the source of speed/turn state.
- Particle/trail counts and materials are bounded and representative native performance remains acceptable for the chosen P2 scene.

## P3 — Rebuild and dress the reef

**Status: OWNER CLOSED FOR PROGRESSION / NOT GREEN at implemented `0.3.6`.** P1–P6 and C3 M1–M4 are implemented. The owner explicitly approved moving to P4 with M4 visual/native gaps and V25 preserved. The [direct closeout](tasks/p3/closeout.md) maps V21–V27 and records that P7/P8 were not completed as runner checkpoints. Earlier [P3 native art](tasks/p3/qualification/native-world-art.md), [C3 P7](tasks/c3-world-gen/closeout.md) and [visibility](tasks/c3-visibility/closeout.md) results remain historical evidence, not silently converted to PASS. The P2 G2.3/V20 gap is unchanged.

P3 is the first style-proving world-art phase. **S18/c3-world-gen supersedes the original authored/modular terrain assumption:** P1–P6 and M1–M4 implement the shore-first compiler, generated geology/routes, terrain-aware dense habitats and full bake/load. The accepted progression Base World is `ocean-m4-42`, world `world-6cba9a67e6275297`, generator `c3-full-12`; the older `qualification-42` remains intact as provisional history. M4's [record](tasks/c3-world-gen/qualification/m4-habitats.md) shows clean multi-seed bakes but still fails visual criteria and leaves complete native traversal unobserved.

### G3.1 — Procedural basin, geology and baked world

**Result: FAIL / NOT GREEN at progression.** M4 verifies seed-42/43/44 full bakes, identity, connected routes and generator-free load, resolving the earlier alternate-seed compile failure. The exposed cave shell and incomplete native perimeter/passage traversal still prevent a PASS. See [P3 closeout](tasks/p3/closeout.md).

- Compile a deterministic irregular basin whose useful deepest region is near the center and which rises toward a visibly non-circular natural perimeter.
- Normal play does not perceive square/four-wall containment or a perfect circular rim; fail-safe collision remains outside/behind natural terrain.
- Generate true 3D geological cave/tunnel/arch/overhang regions with actual floor/walls/ceilings and usable collision/camera clearance.
- Generate/protect connected shallow/middle/deep/return/alternate/cave routes before decoration.
- Same seed/settings/generator version reproduces the same world; the accepted candidate can be baked and loaded without rerunning generation.
- Preserve Forward+ water/depth presentation, movement/camera authority and the P1 fixture.

### G3.2 — Natural terrain-aware reef composition

**Result: FAIL / NOT GREEN at progression.** M4 verifies 7,960 seed-42 placements, all 38 variants, supported solids and a strong shallow-to-deep density gradient. An ordinary shallow view still shows dark exposed geology, and cave transitions remain visibly angular. Full player-view contact review is unobserved.

- All required FAF environment variants participate meaningfully through habitat/cluster rules rather than checklist/random XYZ placement.
- Placement is derived from generated terrain/geology fields, projected to real surfaces and spaced non-uniformly; route/cave clearances are protected before dressing.
- Large rocks and FAF terrain accents are supported/embedded by footprint tests; no unresolved major floating ground/rock pieces remain.
- Shallow/high areas are visibly the lushest and most colorful; upper/middle remain rich but readable; lower/deep become progressively sparser and more geological.
- Relaxation/repair removes major intersections, blocked routes, obvious repeated orientation/spacing and density spikes.
- Optional plant sway remains presentation-only with fixed roots and no movement/collision authority.

### G3.3 — Target style, ambient life and performance

**Result: FAIL / NOT GREEN at progression.** M4 records fast generation/baked load and short Forward+ point samples, while full native performance/traversal and final style acceptance remain unobserved. C3's [V25 native matrix](tasks/c3-visibility/qualification/native-visibility.md#v25-and-visibility-contract-matrix) remains unqualified. The owner's progression approval does not qualify these observations.

- Near, mid and distant gameplay-camera views belong beside the owner-approved FAF reef target and materially improve on style-proof 001: intentional faceting, warm sand/cool stone base, vivid controlled coral colors, green vegetation, layered blue-water composition and readable negative swim space.
- Where used, FAF-FISH-001–004 provide bounded non-gameplay ambient reef life; FAF-FISH-005 shark remains deferred to ecology by default.
- The rebuilt world remains readable and comfortable through full depth traversal, caves and tunnels under the accepted Forward+ atmosphere.
- Representative dense-shallow, cave/middle and deep-sparse views have recorded native performance on the target desktop, with owner acceptance of the final visual direction.
- Full candidate generation and baked load times are recorded; the owner target is ≤120 s generation and baked load materially faster without generator execution.
- Explicit candidate-to-Base-World promotion is reviewable and preserves seed/generator/settings identity.

### G3.4 — Character select and session routing

**Result: NOT GREEN.** Five fish and selection/return pass focused checks and earlier short native cycles, but complete owner readability/feel and native duration remain **NOT OBSERVED** at closeout.

- Fresh session entry shows a usable Character Select page before normal play.
- Animated 3D previews and readable names expose all five FAF fish: Yellow Tang, Ocellaris Clownfish, Blue-Green Chromis, Reef Butterflyfish and Blacktip Reef Shark.
- Any of the five can start normal play as the player visual while the authoritative FishController, collider, movement tuning, controls, camera and water-reaction behavior remain identical across choices.
- Per-character presentation normalization keeps source scale/orientation usable without turning the shark or other fish into different gameplay sizes/stats.
- Pause/Game Menu → Return to Character Select clears held/queued input and pointer capture, unloads/resets the current ephemeral session and returns to selection without death, save/resume, abandonment or Life History side effects.
- After returning, selecting a different fish starts a clean new P3 session with the newly selected visual and no stale player/input state.

## P4 — Freshwater Pond and World Select

**Status: IN PROGRESS / P12 V32 FAIL, NOT GREEN at `0.4.11`; no P4 Gate qualified.** The owner selected this direction on 2026-11-11 and accepted the corrected Ocean as a progression foundation while retaining the P3 gaps. [P11 evidence](tasks/p4/qualification/implementation.md) records catalog/loading, independent Pond bake, atmosphere, FAF placement, generated-waterline constraint, camera immersion and shoreline/reaction source checks. [P12 native evidence](tasks/p4/qualification/native-surface.md) retains the original obscured Bluegill and near-field darkness, then records the R1 repair and R2 dark muddy-green transmission candidate, scripted native retests, fixed-view performance and unsampled owner comfort/control routes. The owner has not approved P12 or version `0.4.12`. [P7 native evidence](tasks/p4/qualification/native-pond.md) retains its incomplete deep/return route; [P8 intake evidence](tasks/p4/qualification/integration.md) retains five Pond choices, 25 stage imports and partial native creature review. [P4 design](planning/p4/freshwater-world.md) is the scope authority; planned scenarios V28–V32. Do not reinterpret the failed P3 G3.1–G3.3/V25–V27 as passed or erase past ocean investments.

P4 adds a World Select step that offers **Ocean** (the existing reef project) and **Freshwater Pond** (a new dark-green, banked environment). New Life routes through World Select and then a habitat-filtered Character Select. Both worlds retain their own baked data, art, atmosphere and permissible fish while sharing the compiler, loading framework, swimming/camera/controls, Forward+, and FAF tooling. Marine species are never eligible or spawned in the Pond; freshwater species never in the Ocean.

### G4.1 — World Select and independent loading

**Result: NOT GREEN / PARTIALLY OBSERVED.** P7 native World Select and Ocean return passed. P8 headless checks load each of five real Pond fish. P12 natively loaded Bluegill Pond→Yellow Tang Ocean→Bluegill Pond and repaired a rapid Character Select launch error; full input-focus, leak and species matrix review remains open. [P7](tasks/p4/qualification/native-pond.md), [P8](tasks/p4/qualification/integration.md) and [P12](tasks/p4/qualification/native-surface.md) evidence own the distinct results.

- Both worlds are selectable and load matching baked resources without runtime generation; the Ocean remains playable and visually intact.
- Navigation and back/return behavior maintain correct input focus, capture, selected world, and character scope.
- Missing resources fail visibly rather than silently loading the other habitat; repeated switches do not leak state.

### G4.2 — Natural freshwater world and atmosphere

**Result: FAIL / NOT GREEN; P7 was owner-cleared for P8 progression only.** P7 native seed-71 and seed-72 views show near-field darkness and insufficient cover/terrain readability; initial P12 confirms the darkness and obscured Bluegill. P12-R1 repaired the surface face selection and fixed native near-field views. The owner rejected pale R2 trials; the final darker R2 candidate has fixed-view evidence but awaits owner judgment of lighting, crossing band and camera comfort. One P7 hero-trunk passage passed; complete middle/deep/return traversal remains unobserved. [P7](tasks/p4/qualification/native-pond.md) and [P12/R1/R2](tasks/p4/qualification/native-surface.md) evidence own the observations and limits.

- The Pond has irregular shallow banks, muddy sediment, submerged logs/roots, vegetation zones, deeper areas and valid routes/collision/camera clearance. Real hard shelter and soft concealment form traversable cover networks with exposed lanes; P4 does not simulate predator detection.
- Per-habitat Forward+ green-water lighting/fog creates limited distant visibility while retaining near-field readability; ocean appearance remains unchanged.
- Deterministic generate/bake/load, stable world identity, performance and native owner visual review are evidenced.
- A fish can swim just under the Pond surface with limited partial breach, without hitting an invisible ceiling, escaping the water or destabilizing the existing motor.
- Its third-person camera smoothly crosses above/below the waterline across the accepted presets, with correct air–water atmosphere restoration, obstacle clearance and readable fish/UI.
- Actual generated banks, nearby cattails/reeds, floating plants and sky appear above water with hidden far bounds; modest fish-reactive ripples/wakes remain visual-only and performant.
- V32 native repeated-crossing evidence demonstrates no clipping, snapping, flicker or ceiling glitch. Preserve Ocean underwater behavior; bird hunting and airborne fish gameplay remain later scope.

### G4.3 — Freshwater FAF assets

**Result: PARTIALLY VERIFIED / NOT GREEN.** All required owner FAF source/GLB and headless Godot intake checks pass; bounded native fish views were observed, while full integrated appearance/performance and owner style approval remain open. [P8 evidence](tasks/p4/qualification/integration.md).

- Original freshwater fish plus a representative pond terrain/wood/plant/ambient-life library are made, registered and integrated without overwriting marine assets.
- Blender/GLB, rigging, material, manifest/provenance and Godot native review follow FAF contracts.
- Wildlife may be ambient-only; predator AI, feeding and growth are later scope.

### G4.4 — Habitat-restricted species and native regression

**Result: HEADLESS POSITIVE/NEGATIVE CHECKS PASS / NOT GREEN.** Native Pond entry with the new roster was observed; full Pond/Ocean return and repeated-switch review remain open. [P8 evidence](tasks/p4/qualification/integration.md).

- Character Select filters ocean-only and freshwater-only species correctly; ambient and later ecological spawns use the same fail-closed habitat compatibility policy.
- Cross-habitat positive/negative tests and native entry/return/re-entry trials prove species never cross ecosystems in normal play.
- Both environments are traversable with shared controls/camera; prior P1–P3 assets, ocean rendering, world generation and controls remain usable.

## P5 — Stealth, feed, escape and grow

**Status: NOT STARTED.** Scenarios V03–V05 and proposed V33–V35. [Ecology contract](systems/ecology-and-growth.md) and [stealth contract](systems/stealth-and-detection.md). Owner-approved identity: naturalistic stealth is the primary gameplay mechanic, not a secondary ability.

### G5.1 — Feeding and visible growth

**Result: NOT OBSERVED.**

- Food and suitable smaller prey can be consumed through defined actions and eligibility rules.
- Each consumed entity grants its reward exactly once; unavailable/ineligible targets do not grant growth.
- Growth visibly updates the same fish and relevant body/collision/camera behavior without clipping, invalid spawning or new identity.
- Demonstrate a meaningful food-chain reversal using a documented species/size rule; growth changes habitat use and hunting opportunity.

### G5.2 — Detection, concealment and escape

**Result: NOT OBSERVED.**

- A predator uses sensed visual/movement evidence, cover occlusion and bounded last-known-target memory; it cannot track a concealed fish with perfect knowledge.
- Native play demonstrates suspicion, investigation, a credible concealment/escape, loss of contact and searching/disengagement, alongside a readable successful predator strike or lethal encounter.
- Physical shelter and soft cover have different effects; moving rapidly or leaving cover creates fair exposure risk. Threat cues, commitment and failure cause remain understandable even outside the immediate forward view.
- Death, genuine pursuit, escape and predation events have authoritative causes/stable identities for later persistence/story use; don't fabricate an escape from every chase.
- New test lives reset intended prototype state; durable multi-session behavior is not claimed yet.

### G5.3 — Stealth hunting and ecosystem consistency

**Result: NOT OBSERVED.**

- The player can approach prey through usable cover/slow movement, and careless exposure can alert prey to flee toward reachable shelter.
- Player and NPC outcomes share coherent movement, sight, obstruction, distance, body-size/cover and prey-eligibility rules; gameplay cannot derive knowledge from a hidden UI or camera-only state.
- At least one native stealth-hunt encounter demonstrates a fair approach and prey response. More sophisticated species sensors and full ecosystem simulation are later scope.

## P6 — A life across sessions

**Status: NOT STARTED.** Scenarios V06–V09. [Run/persistence contract](systems/survival-and-runs.md).

### G6.1 — Suspend and resume one living fish

**Result: NOT OBSERVED.**

- Save, close and resume the same life with coherent growth, health, position, simulation time, consumed entities and relevant encounter state.
- No offline hunger, predation, cooldown progress or growth is applied.
- Continue/New Life follows an explicitly chosen active-slot/checkpoint policy; replacing a living save cannot silently erase it.
- Storage incompatibility/corruption is reported with recoverable data preserved, separately from in-world death.

### G6.2 — Durable death and independent history records

**Result: NOT OBSERVED.**

- An actual terminal outcome prevents normal/supported recovery paths from reviving that run.
- Death retains event/final-state data needed for a recounting under one stable run ID.
- Interruption/retry at material death/archive boundaries converges to one terminal outcome and one completed-life record.
- A new hatchling receives a new run identity without clearing prior completed-life records.

## P7 — Remember every life

**Status: NOT STARTED.** Scenarios V10–V12. [Story/archive contract](systems/life-stories-and-history.md).

### G7.1 — Brief, factual recounting

**Result: NOT OBSERVED.**

- Death presents a brief account built from supported events/final state; every factual claim is traceable.
- Short and eventful lives produce appropriate accounts without fabricated achievements, intent or attacker identity.
- Skipping presentation preserves the final saved narrative; presentation failure cannot reverse death.

### G7.2 — Main-menu Life History

**Result: NOT OBSERVED.**

- Main menu exposes Life History with an honest empty state and access to every saved completed story.
- Open/read/return/start another life/restart: prior entries and text remain intact and dead fish remain unplayable.
- A representative larger archive preserves reachability/readability with no silent truncation or duplicate lives.

## P8 — Qualify the complete experience

**Status: NOT STARTED.** Scenarios V13–V15.

### G8.1 — Optional first person and presentation

**Result: NOT OBSERVED.**

- Toggle optional first person on the same fish and return to third person without changing simulation state or invalidating a save.
- Observe camera clipping, input/focus, threat feedback, text readability and recap navigation across the implemented size range.
- Keep the primary third-person experience readable and record the selected visual/performance scope from measured play.

### G8.2 — Full life and platform qualification

**Result: NOT OBSERVED.**

- Complete feeding/growth/escape, suspend/resume across sessions, die, and read the true saved story.
- Start another hatchling and prove prior history remains; repeat with a short life and distinct identity/account.
- Record the complete flow on an identified source/platform without debug rewards substituting for claimed gameplay.
- Qualify each claimed export/target OS separately.

## After this slice

Use observed play to refine Ocean and Pond species rosters, future habitats (rivers, marshes, additional marine regions), long-life pacing, art/audio depth, discovery/cosmetic carryover and production performance budgets. Ocean and Pond are two distinct supported habitats, not a claim that every future ecosystem is built.
