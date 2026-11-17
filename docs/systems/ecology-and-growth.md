# Ecology, feeding, and growth

## Status and ownership

**P5 fixture feeding and stage growth are implemented; P5 is owner-closed for progression / NOT GREEN with a partial T3 matrix.** Naturalistic stealth survival is the owner's main gameplay direction. Eating food and smaller fish, hiding from hunters and growing from hatchling to large fish are confirmed. The [P9 source checks](../tasks/p5/qualification/p9-feeding.md) and [P10 source checks](../tasks/p5/qualification/p10-growth.md) preserve the earlier proxy-based candidate. The [P11 native record](../tasks/p5/qualification/native-feeding-growth.md) tracks the owner's request to replace the orange stand-ins with findable real Bass larvae and the later natural-movement fix; [P5 closeout](../tasks/p5/closeout.md) records the incomplete final native Gate review. This document owns food-chain and growth transactions; [stealth and detection](stealth-and-detection.md) owns sensing, physical cover, suspicion, concealment and pursuit. Species tuning, diets, thresholds, damage, hunger and numerical balance remain open.

## S27 — P6 normal-Pond living food chain (P1/P3/P4/P5 T2 candidates; later ecology open)

The approved [P6 Living Pond](../planning/p6/living-pond.md) extends the C5 **isolated** Bass encounter into a complete Bass-only survival path in **ordinary Freshwater Pond**. The prior P5/C5 NPC, food and growth evidence is a foundation, not proof of a complete normal-Pond predator ecosystem, balanced scarcity or full-life play. Ocean and the independent Pike Encounter remain separate; other Pond selectable fish do not automatically gain the Bass survival mode.

- **Progressive risk:** tiny hatchling/fry must actually hide from opportunistic adult Bluegill; appropriately sized Northern leopard frogs may occasionally take very small fry, Northern water snakes patrol the shallow/surface habitat, and solitary larger Bass can prey on smaller Bass. Later Bass can hunt eligible Bluegill and venture toward Pike-rich deeper cover. No hard stage/depth gates or magically safe nursery.
- **Food patches:** deterministic habitat-based food locations and bounded movements, consumption IDs, depletion, recovery/migration and stage-scaled nutrition prevent repeated circles near the nest from being sufficient. Food returns in believable patches over game time, not instantly at the same point or via full reset; do not impose an unapproved hunger meter or permanent no-food soft lock.
- **Independent interactions:** real NPC vs NPC hunting/prey reactions, size/diet eligibility, fair sensing, physical mouth reach and one-time event/nutrition removal. Shared bounded selection cannot grant predators omniscient school awareness. Track performance and preserve pause/reset behavior.
- **Aim and feedback:** a center-screen reticle is a navigation aid, with separate projected actual-mouth heading when camera orbit/free-look differs. It does **not** cause feed success, magnetic prey motion, enlarged contact or extra nutrition; confirmed legal consumption may show a short pulse.
- **Assets:** existing `FAF-CREATURE-001` Northern leopard frog and `FAF-CREATURE-002` Northern water snake already have reviewed rigged GLBs and movement clips but **no gameplay predator AI is claimed**. Snapping turtle is an optional later candidate.

P4 implements normal-Pond food as nine seed-derived habitat patches, initially 15 zooplankton clouds and eight registered aquatic nymphs. Three nursery clouds never renew; other exhausted patches recover one moved actor after 150–240 game seconds with a new stable ID. Real terrain, shore, waterline and collision probes constrain placement, while FeedingLedger remains the one-time nutrition authority. Juveniles receive two nutrition per nymph until subadult at 14; normal-Pond subadults receive none from nymphs, including renewed actors, and need genuine eligible Bluegill to reach adult at 26. The fixture retains its existing nymph policy. [P4 evidence](../tasks/p6/qualification/p4-food.md) records focused checks and native limits. The Pike Encounter retains its separate 16-food fixture.

P5 adds two stage-identified adult Bluegill to the normal shallow Pond alongside four larval and two juvenile Bluegill. Registered adult length `0.309825 m` permits hunting the registered Bass larva `0.058056 m` under a 25% prey-length limit; registered juvenile and larger Bass exceed that limit, and non-adult Bluegill cannot attack through this path. The adult uses sensed world-space pursuit and physical mouth/capsule contact to create one session-local catch event; the Bass can still eat eligible smaller Bluegill after growth. [P5 evidence](../tasks/p6/qualification/p5-bluegill-threat.md) records the focused check and native V46 fairness gap. The full multi-species ecology remains later P6 scope.

P6 terminal catches and new hatchlings are **ephemeral**. P7 will persist full food, depletion, actor, threat and run state, record terminal death durably and prevent save/reload exploits. Stage thresholds, food density/recovery, exact diets and aggression are native-tunable, not approved numerical constants.

## S25 — automatic mouth-contact feeding (M7 headless implementation; native review open)

In the M7 encounter source, the player no longer requests a bite. Eligible live food (microfauna, invertebrates or appropriate Bluegill) is **automatically consumed only on unobstructed contact with a small forward-facing mouth interaction zone** aligned to the current fitted Bass body. Species/diet, stage, size, distance, alive/unconsumed identity, solid obstruction and reach are checked; side, tail, behind-target and remote collisions cannot feed. No prey is pulled or teleported. One confirmed contact per stable target enters the FeedingLedger for exactly-once nutrition and event accounting. The optional post-hatch egg shell yields zero nutrition; yolk is not active feeding. [M7 preflight](../tasks/c5-polish/qualification/preflight.md) records the headless evidence and native gap.

Small food gets quiet collection feedback. Confirmed fish prey triggers a short fast-swim feeding cue, the existing feeding-strike sound and event-driven blood; duplicate or invalid contact creates no reward or miss sound. The imported Bass stages have no dedicated jaw action, so native review must judge whether the cue reads as a swallow. Geometry and timing remain tunable.

## C5 — Bass first-life and Bluegill prey correction

**Owner-approved S24; M1–M6 have local implementation checks, while native C5 review remains open.** The previously implemented P5 fixture was **juvenile Bluegill eating Bass larvae/juveniles**; the P9–P12 source and native logs above are preserved as historical, not retroactively reinterpreted. The [C5 correction design](../planning/c5-post-p5-ecology-lifecycle.md) changes the fixture's fish roles: registered Largemouth Bass `FAF-FISH-007` is the sole player and registered Bluegill `FAF-FISH-006` is the only **fish prey**. M5 implements Pike hunting of actual Bluegill individuals.

The Bass begins as an **egg in a shallow nest** and hatches into a tiny controllable larva/fry. The yolk-supported interval precedes zooplankton, aquatic invertebrates and then size-eligible Bluegill. The same Bass ID and registered five-stage exports drive its body, camera, animation and feeding policy. The numerical values below are fixture tuning, not measured fish physiology.

| Bass stage / food | Availability and award | Next stage |
| --- | --- | --- |
| Egg; early larva on yolk | Egg cannot feed; larva uses yolk for 12 game seconds. Optional shell bite awards 0. | Larva after hatch |
| Larva; non-fish zooplankton | 1 per distinct cloud after yolk interval. | Juvenile at 6 total |
| Juvenile; aquatic green darner nymph | 2 per distinct nymph. | Subadult at 14 total |
| Subadult; Bluegill larvae | 4 per distinct larva, at most 60% of Bass length. | Adult at 26 total |
| Adult; Bluegill larvae or juveniles | 4 per larva; 6 per juvenile at most 45% of Bass length. Adult Bluegill excluded. | Mature cap |

Eight small, muted copepod-like zooplankton clouds and eight existing `FAF-INSECT-001A` green darner nymph exports are fixed nursery food in the encounter only. Food IDs and awards commit once; cloud drift uses encounter time and stops on pause. The first cloud is near the nest. The HUD points to the nearest eligible food and reports its distance, the yolk countdown, and when blocked growth needs open water. Green darner provenance and dimensions are in `assets/faf/pond/insects/pond_insect_001_manifest.json`; Bass and Bluegill rigs/dimensions are in `game/assets/faf/pond/pond_creature_catalog.json`. Bass export lengths from egg through adult are 0.003362, 0.058056, 0.235077, 0.495288 and 0.7337 m. [M3 evidence](../tasks/c5-polish/qualification/m3-early-diet-and-growth.md) records actual checks and the native review gap.

The Bass swimming stages use mesh-fitted capsules with stage-specific pivot offsets and fixture-only swim caps of 0.22, 0.48, 1.40 and 1.55 m/s from larva through adult. The owner reported that the first M3 juvenile transition accelerated too sharply and nearly stranded the fish, then reported the M4-era subadult was too slow. The subadult cap and acceleration have been raised; the corrected build passes headless wall, shore and stage checks but awaits owner-native replay. The egg is non-swimming. The [M3 evidence](../tasks/c5-polish/qualification/m3-early-diet-and-growth.md) records numerical fit and test limits.

M4 implements three encounter-only schools of 16 separate Bluegill (six larvae, four juveniles, four subadults, two adults) with registered 1:1 stage art, bounded local steering and individual alive/consumed state. Nursery, root/log refuge and deeper routes use seeded clear spawns and soft depth preferences; local sensed alarms can scatter nearby members before they regroup. [M4 evidence](../tasks/c5-polish/qualification/m4-bluegill-schools.md) records the headless results and native gap. Avoid spawning prey inside solid logs or locking fish to invisible depth partitions. Bass and Pike consumption remain exactly once by stable target ID. M6's predation event triggers **presentation-only underwater blood**, not an extra feeding reward or chemical sensor. The old P5 Bluegill/Bass figures below remain historical baseline details.

## A bounded first food chain

Start with one compact playable habitat, one player species, small food, one prey type, one predator type and a network of usable shelters separated by exposed feeding lanes. Use clear visual size differences and a documented rule for what can be eaten. The player learns when to hide, risk an exposed crossing, escape or stalk through repeatable, explainable encounters.

The prototype should allow at least one meaningful role reversal: something initially dangerous becomes suitable prey after the player has grown enough. Whether that means the same individual or another member of its species depends on actual entity tracking. A later story must use the identity the simulation really recorded.

## Feeding transaction

Successful feeding consumes a real target or food item and awards nutrition once. A target already consumed, dead, invalid, or outside the allowed interaction cannot produce a second reward. Visual effects, repeated input, overlapping collision callbacks, and saving during an encounter must not duplicate nutrition.

**Historical P5 fixture (superseded by S24/S25):** In the earlier isolated Pike fixture, Space makes a directed bite at a live registered Bass larva within 1.1 m of the mouth, in front of the Bluegill and across a clear physics ray. The 0.058 m larva is eligible for the 0.100 m juvenile Bluegill under a 65% length limit. The 0.235 m Bass juvenile becomes eligible only for the 0.310 m adult Bluegill under an 80% length limit; the adult Pike remains ineligible. The orange invertebrate proxies are removed from the encounter. Session-local stable target IDs, consumed flags and one nutrition ledger award 4 units per Bass. Successful consumption records the actual target, stage and game time; failed attempts record no hunt event. These are fixture working rules, not final diet, balance or P11 native qualification.

The historical fixture uses a directed bite for live prey. S25 supersedes manual input: automatic contact reach and swallowing presentation remain tuning subjects.

## Growth

Food drives the isolated fixture growth. Growth changes visible body size and relevant capability within the same fish identity. Long-life pacing, continuous scaling, maximum size, and final balance remain tuning decisions.

In the isolated fixture, one Bluegill identity starts at its verified 0.099684 m juvenile export, advances at 3 nutrition to the 0.209363 m subadult export, and at 11 nutrition to the 0.309825 m adult export. Under the P11 candidate, one Bass-larva bite reaches subadult and three distinct larva bites reach adult; adult Bluegill can then hunt the larger Bass juveniles. The egg is never treated as a swimmer. The same registered stage drives the model/clip, normalized body scale, collider radius, camera framing, Pike/prey sight size and prey eligibility. The existing adult collider radius is 0.48 m; juvenile and subadult radii are proportional to registered length. A stage waits if the larger physical sphere would overlap solid geometry or breach the waterline; already-awarded nutrition is retained. This is a real Bass food-chain change, not a Bluegill-versus-Pike predation reversal. Native feeding and growth judgment belongs to P11.

Size should change the use of space and exposure: a small fish can fit into hard shelter unavailable to a large fish, while a larger fish can pursue bigger meals but loses some concealment routes. Growth must feel rewarding. Handling tradeoffs should preserve useful advantages rather than erase progress.

Keep the authoritative size/growth state separate from its presentation. Movement, collision, edible-target checks, and camera offsets must consistently derive from the selected growth model. Reopening the game or changing the camera must not award growth or change edible-target eligibility.

## Predator and prey behavior

Proposed prey behavior includes sensing danger, breaking visual contact and fleeing to reachable shelter. Predator behavior includes ordinary activity, graded suspicion, investigation of a perceived disturbance, visual confirmation, a readable attack commitment, pursuit, last-known-location search and disengagement. These are behavior options, not an obligatory linear sequence. Sight obstruction, 3D distance, light/clarity, local movement disturbance and body/shelter clearance must have observable consequences. Neither prey nor predator should automatically know a hidden target's current position. The same explainable sensing and concealment principles apply when the player hunts prey; do not grant AI-only omniscience.

The initial environment need not simulate a complete self-balancing ecosystem. Add schooling, breeding, competing predators, environmental cycles, and population recovery when a concrete gameplay need justifies them.

Food replenishment and enemy respawning need explicit rules. Save/load must not regenerate a cleared feeding spot or reset a pursuit by accident. Later region streaming needs the same treatment.

## Hunger and injury

A draining hunger meter is a proposed option, not a confirmed requirement. Feeding already has a growth purpose. If hunger is adopted, define its pressure, warning, and terminal consequence; do not add offline hunger. Injury, healing, and species-specific defenses likewise require explicit behavior and visible feedback.

## Acceptance targets

- Award a consumed item or prey reward exactly once.
- Show growth that corresponds to actual feeding and persists across sessions.
- Apply the same documented size rules before and after resume.
- Demonstrate an understandable real hiding encounter, suspicion/investigation, loss of contact and escape in native play; a decorative plant overlap alone is not invulnerability.
- Demonstrate that the player can use cover and careful approach to stalk prey, and that prey can sense/reject careless approaches.
- Prove the role reversal without silently replacing the player fish.
- Preserve consumed targets, food state, and encounter state through saving.
- Record real milestones for [life stories](life-stories-and-history.md) without promoting a mere pursuit into a kill or an ordinary turn into an escape.

The [roadmap](../roadmap.md) determines when these targets enter a Phase; [validation](../validation.md) determines the evidence required.
