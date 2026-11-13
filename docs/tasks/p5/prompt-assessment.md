# P5 prompt assessment — scoped stealth and food-chain qualification

**Assessment date:** 2026-11-12. **Planning only.** GitHub main at 535116c5b6907681786b233f34af15bc2a87d4b5, root 0.4.11. P4 P12/V32 FAIL / NOT GREEN and still undergoing visual/camera work. P5 is not started and must NOT be executed until owner approves explicit P4 closeout/progression and deliberate root 0.5.0 baseline.

## User goal

Make naturalistic stealth central to Fish. First establish a real Bluegill versus Pike encounter reachable directly from the third World Select card in an isolated scene, leaving normal Ocean and Pond test worlds free from the new predator system. Build perception, hard/soft cover, movement disturbance, investigation, bounded memory and fair escape. Then use the same mechanics to implement prey response, actual feeding and growth before P5 closeout.

## Source boundaries to preserve

- `WorldSelect.cs` currently builds two cards from `WorldCatalog.All`; `WorldCatalog.cs` defines the two authoritative habitat IDs `ocean`/`pond`. Never add the Pike Encounter as a third habitat; introduce menu destinations separately.
- `PondCreatureCatalog` contains authentic Pond adult Bluegill `FAF-FISH-006` and Pike `FAF-FISH-010`, with asset/rig validation and habitat eligibility via `CharacterCatalog/SpeciesCompatibility`. `DemoSelection` is session only.
- `FishController`, `SwimMotor`, `FollowCamera`, control presets, `PlayShell` and water/camera presentation are shared. No stealth override may alter authoritative swim or old P1/P4 input feel.
- `PondFafEnvironment` and FAF hollow trunks/reeds give starting cover, but geometry and camera clearance must be measured. The owner supplied **two new FAF production briefs** (five underwater vegetation + five large wood families); finished new exports/manifests were absent from GitHub at review, so [P5 asset intake](stealth-cover-asset-intake.md) makes P2 block on missing verified inputs. `PondTurbidityField.Sample` can inform world-space sight; renderer fog/GFX sliders are NOT simulation authority.
- `FafAmbientPondLife` is presentation-only, not predator AI. Environmental day/night and P4 water clarity remain owner-sensitive and currently unqualified.
- Normal play baked Ocean and Pond identities must not change; no new original art or duplicate GLB export is authorized.

## Work decomposition

P1 creates the direct scene. **P2 first verifies/imports all ten new FAF families, then composes naturally connected shallow grass, intermediate root/weed and deeper hollow/branch-crown encounter regions** with exposed lanes; missing packs block P2. P3–P5 then implement world-space vision, movement/cover signals and Pike hunting. P6 is first native **M1** owner test. P7 builds shared prey noticing/fleeing, P8 is second native stealth checkpoint. P9 feeding makes every eligible consume a once-only transaction. P10 growth uses genuine life-stage assets and changes size/collider/camera/edible prey; P11 is native integrated feeding/growth review. P12 performs cross-habitat isolation checks. P13 performs manual final G5.1–G5.3 closeout.

## Risks and handling

| Risk | Mitigation |
| --- | --- |
| P4 not ready for progression | Require explicit owner progression/closeout and deliberate `0.5.0` entry; do not run P5 from `0.4.11`. |
| Third menu card breaks species/world integrity | Menu destination separate from WorldCatalog; fail closed on wrong/missing models; test three routes and repeated return. |
| Cheating or unreadable Pike | Last **sensed** location and finite memory, real occlusion and movement evidence; native fair-catch and hidden-escape comparison. |
| Missing/unverified FAF expansion or concurrent registry conflict | P2 checks actual source/manifest/unique IDs/GLB hashes/structural variants/wrappers and stops without art generation or pack-001 substitution. |
| Unnatural set dressing | Terrain/depth-aware rooted and varied three-zone composition, no repeated rows or opaque hedge; native visual review. |
| Foliage or log cheats | Real wood LoS/physics vs visible soft plant density, no invisible safe zones; fish/Pike/camera clearance. |
| GPU fog controlling sensors | Perception in world space, independent of camera and graphics preset; bounded sampling on actual turbidity field. |
| Rendering or movement slowdowns | Bounded sensory checks, frame pacing during active encounter, read-only motion inputs. |
| Biology/growth unrealistic | Verify eligible size/stage/prey; no Bluegill-eats-Pike shortcut, no fake egg swimming, same-fish identity. |
| Encounter reset leaks | Clear timers, AI memories, consumed IDs, input/pause/capture and fixture actors; return normal worlds intact. |
| Tests mistaken for native play | P6/P8/P11/P13 manual owner gates; record FAIL/NOT OBSERVED/BLOCKED and do not advance version if failed. |

## Explicit deferrals and evidence tiers

The fixture has temporary caught/escaped outcomes, **not** permanent death, living-save state or Life History. Those belong to P6/P7. First-person qualification, bird/snake attacks, whole-world ecosystem and rich audio/chemical senses are later choices. P1–P5, P7, P9, P10 and P12 are focused T2 changes; P6/P8/P11/P13 demand native owner T3. No Phase/Gate changes come from writing prompts.
