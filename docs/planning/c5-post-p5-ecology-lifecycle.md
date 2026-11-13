# C5 — Post-P5 ecology, Bass lifecycle and qualification polish

**Decision:** S24, owner-approved 2026-11-13. **Status:** approved working design; **NOT IMPLEMENTED or qualified as C5**. Historical P5 is **OWNER CLOSED FOR PROGRESSION / NOT GREEN** with G5.1–G5.3 open. Current `main` at review `23e08c93189d94acbe3c9032ce39bf871d9f301d` has subject `0.5.13`, while `package.json` remains **`0.5.12`**. Correction tasks must preserve that actual root version unless separately authorized. [P5 closeout](../tasks/p5/closeout.md) retains its earlier Bluegill-versus-Bass evidence.

[Manual execution instructions](../tasks/c5-polish/README.md) · [Ecology](../systems/ecology-and-growth.md) · [Sensing](../systems/stealth-and-detection.md) · [Validation](../validation.md) · [Roadmap](../roadmap.md)

## Intended player outcome

Keep the **third World Select destination** (the Pike Encounter / development test scene), still **without Character Select**, but switch its fixed player from Bluegill `FAF-FISH-006` to **Largemouth Bass `FAF-FISH-007`**. Bass starts life in a shallow nest, hatches, transitions to an actual tiny swimming larva/fry, grows by species-appropriate feeding, later hunts Bluegill from distinct age/size-appropriate schools, and survives a Northern Pike `FAF-FISH-010` that can independently pursue the player **or** a legitimate individual Bluegill. Fish bite/consumption causes a restrained event-driven blood cloud that disperses through the water. The existing normal **Ocean** and **Freshwater Pond** remain unchanged predator-free test worlds, retaining their character selection and baked identities. This is one standalone fixture, not a third habitat or a new persistent game run.

> Every movement is a decision about exposure. Every hiding place is an opportunity to survive. Every feeding opportunity is a reason to take a risk.

## Current source reality (do not rewrite history)

- `PikeEncounter.cs` currently initializes a registered **juvenile Bluegill** and four Bass juveniles plus six edible Bass larvae, with temporary caught/escaped fixture outcomes. `PondGrowthState.cs` is a Bluegill-only juvenile→subadult→adult controller, and `FeedingLedger.cs` permits registered Bass prey under fixture size rules. `PreyActor.cs` provides physical prey with movement and sensing; `PikeActor.cs` and `PikeAwareness` target the Bluegill/player path.
- `PondCreatureCatalog` already lists five authored Bass stages (egg, larva, juvenile, subadult, adult) and five Bluegill stages. No new fish modeling is assumed. However, authored egg/larva art does **not** prove a hatching movie, fry movement physics, suitable camera scale or an actual food chain.
- Existing P5 qualification covers source-level feeding, movement, sensing and partial native owner observations but **does not** prove the final three-zone route, complete stage/hunt transition, honest Pike contact, native two-world regression, or natural 5+5 cover appearance. [P5 closeout](../tasks/p5/closeout.md) is authoritative for what was and was not observed.
- The P5 fixture uses an independently baked Pond seed `5266`; original Ocean seed `42` and Pond seed `71` are independent. Preserve those existing manifests and all original P5 evidence. No blanket P5 Gate GREEN, `0.5.13` package version, or P6 persistence is implied by approval of this correction.

## Lifecycle design and first food

**Egg / cinematic:** open a new Bass encounter with a brief, restrained, skippable **shallow nest hatching scene** (prefer reuse of the existing Bass egg/larva asset and Pond shader/lighting/camera). Establish one new fixture fish identity before cinematic start. The egg itself does not swim, consume prey or use a fish motor. No magical morph from adult, instant unexplained stage jump or dummy orange nutrition spheres. Once hatched, the same Bass life becomes a **tiny playable larva/fry** at the actual registered scale, with camera/body/collision/clipping suitable for its size. Retain smooth transition into existing controls and no sudden loss of input/focus.

**Early feeding:** yolk reserves cover the nonfeeding interval; afterward, tiny **zooplankton/microfauna** provide early food. These are **not fish prey** and need visible, credible low-cost particles/simple appropriately sized actors, not the previously rejected orange-orb presentation. Then introduce existing freshwater aquatic insect-larva/other suitable invertebrates as the bass grows. Once jaw/body eligibility permits, it may consume **registered Bluegill fry/larvae or small juveniles**, advancing toward subadult/adult. Do not claim biology-derived numeric bite ratios or exact nutrition thresholds until tuned and measured. Keep food/contact idempotent and stage-specific, with one source of authoritative timing and event identity.

**Growth:** one `FAF-FISH-007` Bass identity throughout egg→larva/fry→juvenile→subadult→adult. Asset/animation state, body size, real collision, bite reach, prey eligibility, camera and shelter clearance must follow the registered stage; no simple uniform rescale masquerading as a life-stage asset. Stage transitions must respect physical space/waterline, avoid camera/collider snags in tight new wood/log/root cover, and not erase earned nutrition. Endgame size and long-life balancing remain future choices.

## Bluegill schools and habitat

**Only Bluegill** `FAF-FISH-006` appears as **fish prey in this fixture**; early zooplankton/insects are non-fish food. Fish from other habitats and Bass-as-prey are excluded from this encounter without deleting their global catalog entries. Form **several bounded schools**, with actual individual entity IDs, species, developmental stages and independent alive/consumed state.

- **Shallows:** small Bluegill larvae/fry and juveniles cluster around eelgrass/coontail and natural low cover, where young Bass starts.
- **Intermediate:** mixed juvenile/subadult schools move through foliage/root/log transitions and forage lanes.
- **Deeper:** more mature subadult/adult Bluegill may form looser groups with broader movement routes. These are preferred zones, not magic depth fences; allow natural overlap and excursions.

Use modest schooling rules (neighbor separation/cohesion/alignment, local route, predator disturbance/scatter and regroup) rather than all-to-all expensive AI per member; keep bounded populations, sensed target IDs, physical clearance, time/pause/reset semantics and stable deterministic placements. Actual size should govern edible eligibility, not school label. Avoid spawning schools inside colliders or food directly within unreachable dense vegetation. Normal Pond/Ocean passive ambient populations remain unchanged.

## Pike sensory targeting and emergent escape

Replace the Pike's player-only target assumption with a **shared prey-candidate selection** mechanism: Bass player and individual live Bluegill are each sensed using the current world-space sight, movement disturbance, lighting, murk and true solid/soft cover model. A school is **not** one magical aggregate target; proximity to a school supplies multiple legitimate candidates. Evaluate confidence/evidence, target reach, prey size, distance, relative movement and current pursuit commitment. Pike switches targets only with actually observed stronger opportunities, bounded hysteresis/commitment and plausible turn/acceleration; it does not instantly retarget hidden fish or know an unseen school exists.

If a Pike pursues a Bluegill member, that member must be an actual physical target with real damage/consumption/caught semantics and a once-only removal/event. Neighboring schools may scatter and regroup due to a sensed threat, creating an **unreliable player escape opportunity**. Approaching a school is **not** a guaranteed escape trick, a fixed AI reset trigger or stealth invulnerability. If the Pike has clear and strong evidence of the Bass, it can keep chasing Bass. Preserve finite last-known position per selected target and never use the player's hidden live transform for targeting.

## Event-driven dissipating blood

For a confirmed **fish** strike with injury or valid consumption, emit a bounded, tasteful, low-cost reddish-brown underwater cloud at actual contact position. Gradually expand, diffuse and fade with existing water lighting/turbidity, respecting real game time/pause/retry and unloading. Explicitly separate **authoritative strike/consume event ID** from VFX emission; no duplicate cloud from repeated physics/collision callbacks, UI input or animation loops. Blood is *presentation-only*: no scent/chemical AI knowledge, new fish tracking or permanent stain from this correction. Zooplankton feeding need not create blood. Provide low-graphics/reduced-effects behavior and measure native Forward+ cost.

## Manual work and qualification

Use [c5-polish manual prompts](../tasks/c5-polish/README.md) **M1–M8** in order. They are **manual Codex briefs, not phase-runner tasks**. Each prompt owns one coherent change, tests it, records evidence and **stops for owner review** before the next. M8 is the final native qualification/closeout brief and **must end by launching the actual Fish game** with `./tools/dev run --display-mode=windowed-fullscreen` after checks, then hand keyboard/mouse to the owner; keep the game running rather than executing a headless-only probe and calling it accepted. If no X11/Godot/GPU native session is possible, report concrete blocker and leave the T3 rows NOT OBSERVED without fake launch. Actual owner feedback determines next repair.

- **V37:** skippable egg-to-larva continuum, same Bass ID, first playable fry/camera/input/reset.
- **V38:** stage-appropriate invertebrate→Bluegill nutrition, exactly-once transactions, body/camera/clearance through growth.
- **V39:** habitat-appropriate age-structured Bluegill schools, real schooling/scatter/regroup, fixed population and no cross-habitat leakage.
- **V40:** Pike target switching among actual Bass/Bluegill with bounded evidence, realistic attack/feeding and credible school-assisted escape.
- **V41:** genuine bite/consumption blood VFX, pause/reset/render cost and once-only emission.
- **Historic P5 debt:** additionally capture **V03–V05/V33–V36** with native Bluegill-school/Bass updated identities and the full shallow→midwater→deep new 5+5 FAF cover route; preserve earlier fixture evidence and note supersession explicitly. V36's original Bluegill-specific entry is historic evidence; the new Bass routing expectation belongs to V37, not rewriting what the old test originally checked.

## Scope and open decisions

**Confirmed:** Bass player, starting egg hatch cinematic and tiny first playable fish, Bluegill-only *fish* prey with size/depth-dependent schools, Pike sensing other prey and possible school-assisted escape, dissipating blood effect, final owner native play, and the continued isolated World Select third option. **Working/tunable:** exact first food species and geometry, stage thresholds, school sizes/routes/ranges, target score/hysteresis, attack damage and blood duration/color/cost. Do not impose ungrounded numbers or invent new FAF fish models.

**Deferred:** normal Pond/Ocean production predator spawning, new marine habitat species, real persistent death/archive/save/resume (P6/P7), weather, chemical/blood scent, all-to-all multi-species ecosystem or automatic completed-P5 Gate claims. Root development version remains **`0.5.12`** in this manual correction unless the owner separately authorizes a new baseline. P5's historical `0.5.13` commit subject is not permission to change package.json.
