# Ecology, feeding, and growth

## Status and ownership

**P5 fixture feeding and stage growth are implemented; P5 is owner-closed for progression / NOT GREEN with a partial T3 matrix.** Naturalistic stealth survival is the owner's main gameplay direction. Eating food and smaller fish, hiding from hunters and growing from hatchling to large fish are confirmed. The [P9 source checks](../tasks/p5/qualification/p9-feeding.md) and [P10 source checks](../tasks/p5/qualification/p10-growth.md) preserve the earlier proxy-based candidate. The [P11 native record](../tasks/p5/qualification/native-feeding-growth.md) tracks the owner's request to replace the orange stand-ins with findable real Bass larvae and the later natural-movement fix; [P5 closeout](../tasks/p5/closeout.md) records the incomplete final native Gate review. This document owns food-chain and growth transactions; [stealth and detection](stealth-and-detection.md) owns sensing, physical cover, suspicion, concealment and pursuit. Species tuning, diets, thresholds, damage, hunger and numerical balance remain open.

## S25 — automatic mouth-contact feeding (approved, not implemented)

The player no longer requests a bite. Eligible live food (microfauna, invertebrates or appropriate Bluegill) is **automatically consumed only on real unobstructed contact with a small forward-facing mouth interaction zone** aligned to the fish's current anatomical stage. Honor species/diet, stage, size, distance, alive/unconsumed identity, solid obstruction and reach; side, tail, behind-target and remote collisions cannot feed. Never pull or teleport prey into range. Send one confirmed contact per stable target to the existing FeedingLedger for exactly-once nutrition and event accounting. The optional post-hatch egg shell is reachable by the same contact rules but yields zero nutrition; yolk is not active feeding.

Small food gets quiet collection feedback. Real fish prey automatically triggers bite/swallow animation and genuine event-driven sound/blood where valid; duplicate or invalid contact creates no reward or miss sound. Current `PikeEncounter.cs` still binds Space to `TryBite`, which is **source debt** for [M2A](../tasks/c5-polish/manual/M2A-auto-feeding-and-speed-boost.txt), not evidence of S25 implementation. Geometry and numeric timing are native-tunable.

## C5 — planned Bass first-life and Bluegill prey correction

**Owner-approved S24; design only, not yet applied to gameplay.** The previously implemented P5 fixture is **juvenile Bluegill eating Bass larvae/juveniles**; the P9–P12 source and native logs above are preserved as historical, not retroactively reinterpreted. The [C5 correction design](../planning/c5-post-p5-ecology-lifecycle.md) replaces the *future fixture's* fish roles: registered Largemouth Bass `FAF-FISH-007` is the sole player, registered Bluegill `FAF-FISH-006` of varied size/stage is the only **fish prey** in this encounter, and Northern Pike `FAF-FISH-010` hunts either actual individual fish when sensory evidence supports it.

Begin Bass as an **egg in a shallow nest**, with a short skippable hatching cutscene that hands off to a physically tiny, controllable larva/fry; a yolk-supported phase precedes first active feeding. Small zooplankton and appropriately sized aquatic invertebrates may support early nutrition, as **non-fish food**, before Bluegill fry/juveniles become physically eligible. Avoid non-shipping orange orbs, arbitrary large-fish bites and treating an egg as a motor-driven swimmer. Same Bass identity, true five-stage authored resources, real body/collider/camera size and nutrition ledger rules apply throughout. Numerical stage thresholds, feeding rates and juvenile fish eligibility remain tunable pending tests.

Bluegill schools contain separately alive/consumed actors with bounded schooling/scatter/regroup; younger Bluegill prefer shallow cover and larger ones can school farther/deeper. Avoid spawning prey inside solid logs or locking fish to invisible depth partitions. Real consumption remains exactly once by stable target ID, whether Bass eats Bluegill or Pike eats a school member. A predation event may trigger **presentation-only underwater blood**, not an extra feeding reward or chemical sensor. The old P5 Bluegill/Bass figures below are historical baseline details until C5 is actually implemented.

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
