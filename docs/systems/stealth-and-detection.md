# Stealth and detection

## Status, owner decision, and boundaries

**Owner-approved direction (S20, 2026-11-11):** naturalistic ecosystem stealth is Fish's core gameplay identity across Ocean and Freshwater Pond. **Mechanics in this document are working designs, not implemented or qualified P5 behavior.** P4 supplies physically usable cover networks, not predator sensing or gameplay stealth state.

> Every movement is a decision about exposure. Every hiding place is an opportunity to survive. Every feeding opportunity is a reason to take a risk.

Every movement should have a meaningful exposure tradeoff. Shelter can preserve a life, but feeding requires leaving safety. The player should learn how the ecosystem behaves, not memorize arbitrary invisible safe zones. Stealth also applies when the player stalks prey; growth changes who hides from whom. The [vision](../vision.md) owns the core identity, [ecology and growth](ecology-and-growth.md) owns feeding/growth, and [the roadmap](../roadmap.md) owns Gate status.

## Exposure and the underwater environment

- **Hard shelter:** real rocks, hollow trunks, enclosed routes and other solid geometry can obstruct sight and/or prevent a larger body entering. Visual and collision bounds must match closely enough for player and predator movement. Shelter that blocks the hunter does not erase knowledge that the player entered it.
- **Soft cover:** weeds, reeds, roots, suspended sediment and partially obscuring structures may weaken visual evidence, but aren't automatically safe. A predator may still see motion, approach, or investigate.
- **Open water:** increases exposure when visual clarity and lighting permit, especially when travelling between cover areas to feed.
- **Water clarity and lighting:** distance extinction, depth, contrast and silhouette change visual information for both hunter and prey. Darkness and murk should not grant supernatural certainty or turn every nearby obstacle into an unreadable collision hazard. Pond's S21 spatial clarity design treats sediment/turbidity and illumination as distinct environmental properties; a shaded region is not automatically muddy, and murk does not replace solid occlusion.
- **Cover networks:** the world should offer linked resting/hiding locations and exposed crossings with multiple credible escape routes where geometry allows. This is a world/placement affordance; P4 does not need stealth-state flags.

Physical size and life stage change which gaps and shelters remain accessible. No purely decorative foliage volume may guarantee invulnerability.

## Working sensory model

1. **Vision:** target distance, orientation, visible projected silhouette, light/contrast, water clarity and actual obstruction determine whether a target can be observed. Use efficient bounded checks in 3D space, not a camera-based player-detection hack.
2. **Local water movement:** actual fish speed, acceleration, tail movement or abrupt maneuvers may create limited near-field disturbance evidence. A simple timed spatial event/field is sufficient initially; do not attempt full fluid simulation or treat the existing visual wake effect as simulation authority. Lateral-line-inspired detection is local and tunable, not long-range magic.
3. **Other senses:** strong splash/sound cues and species-specific chemical sensing are possible future systems. They must not be silently simulated with omniscience before rules and feedback are defined.
4. **Habitat conditions:** current, background motion, time/depth and species abilities can eventually modulate perception, but first prove a small deterministic, testable subset. Ocean and Pond share behavioral principles while allowing separate values.

When P5 implements sensing, start with camera-independent, reproducible queries of environmental clarity across hunter–target space (including meaningful intervening water, not just the predator's location), then combine them with occlusion, illumination/contrast, orientation and movement evidence. The Pond's P4 presentation may prototype its own visual approximation, but do **not** infer predator perception from camera fog, shader samples, graphics preset sliders or the player's viewpoint. A common baked world-space field is a preferred later integration if it proves inexpensive; no sensor values, occlusion claims or gameplay safety bonuses are implemented by S21.

All AI perception and authoritative outcomes derive from simulation state. Camera position, graphics presets, UI indicators, shader fog and decorative effects must not directly reveal targets to AI or alter fish movement. Presentation should represent the same environment properties where practical, without coupling render state to authoritative detection.

## Predator awareness and search (working design)

Predators have ordinary non-player activity even when the player is absent: foraging, resting or swimming their habitat routes. A plausible awareness model supports **unaware/ordinary activity, suspicion, investigation, confident pursuit or attack commitment, last-known-position search, and disengagement**. This is not a mandatory fixed linear sequence: strong evidence may trigger immediate evasive or predatory behavior; lost contact can move an actor back toward ordinary activity.

- Suspicion can arise from an incomplete silhouette or nearby disturbance. The predator checks the source, rather than knowing which fish caused it.
- Visual confirmation can trigger pursuit and a readable attack commitment, bounded by turn rate, body clearance and normal movement.
- Breaking sight updates the predator's finite last-known information. It may search the last seen area or nearby accessible cover, not follow an invisible player through a wall.
- Investigation must eventually resolve or change based on new evidence; the fish is not pinned forever by a permanently telepathic enemy.
- Successful escape must be evidenced by actual pursuit/disengagement state. Avoid calling every momentary turn, lost line-of-sight frame, or routine departure a documented escape.

Prey use the same physical world and perception principles but can react by fleeing, taking cover or changing route. Species differ in plausible temperament, preferred habitat and sensory strengths only after distinct behaviors have playtest value; this is not yet a numeric balance contract.

## Hiding, pursuit and stealth hunting

- The player can choose to remain still, glide or move carefully around cover to limit new exposure; rapid movement may evade a committed strike but may create new sensory evidence.
- When discovered, escape remains playable through turning, speed, occluding obstacles, body-size-limited passages and route choice, with understandable attack commitment and no unavoidable instant teleport catches.
- When hunting, the player faces the same risk of exposing an approach. Prey should have a real chance to notice an open, fast approach and flee toward reachable shelter; a careful approach can succeed.
- Feeding succeeds only through [ecology's](ecology-and-growth.md) actual eligibility and one-time consumption rules; stealth is never a bypass of prey identity or food accounting.
- Death, pursuit, valid escapes, strikes and successful hunts must be grounded in real events for [Life History](life-stories-and-history.md), not generated from cinematic impressions.

## Readability, fairness and accessibility

The default presentation should be immersive: changed predator facing/behavior, searching motion, moving plants, prey flight, ripples and restrained audio/directional cues. Avoid a permanent arbitrary 'hidden percentage' display or omnipresent enemy cones. Optional accessibility cues and developer sensing visualizations may be stronger but must not change sensor authority.

A player should generally understand why a predator saw them, why a hiding place worked or failed, and when a committed attack can be evaded. Account for predators outside the view, above and below, using clear non-color-only feedback. Uncertainty creates tension; inexplicable knowledge and invisible lethal attacks create unfairness.

## Implementation and scope sequence

- **P4 — Environment only:** keep Pond and Ocean independent; integrate real hollow-log passages, wood/rock occlusion, plant cover, exposed lanes, size-appropriate gaps and camera/collider clearance. No predator AI, sensor scoring, attacks or stealth HUD.
- **P5 — Small playable proof:** one compatible player, food, prey, predator and cover network. Prove sensing, visual obstruction, local movement evidence, suspicion/investigation, genuine hiding/escape, readable pursuit/strike, stealth hunting, and feeding/growth changes. Start with reversible parameters and record actual native observations.
- **P6 — Continuity:** persist relevant encounter, suspicion and last-known-location state with the living fish. Closing and reopening cannot be used to reset a predator or convert a pending attack into safety. No offline simulation.
- **Later:** optional sophisticated species senses, schooling, bird hunting at the surface, **dynamic** fish- or predator-disturbed sediment clouds and their sensing consequences, richer ecosystem interactions and detailed sound/chemical behavior. Static Pond clarity variation is instead a P4 visual requirement.

## Acceptance and evidence

P5 [G5.2/G5.3](../roadmap.md) and [V05/V33–V35](../validation.md) require native comparisons of stationary/slow/fast movement, solid occlusion versus plant cover, predator investigation and loss of contact, an exposed caught player, and a cautious versus careless prey approach. Log source revision, fixture/seed, relevant sensor/AI states, observations, fairness issues, performance and owner judgement. Headless tests for occlusion, line-of-sight, stale memory, species eligibility, time/pause and reward invariants complement but do not replace native threat-readability qualification.

## Open implementation decisions

Sensor units and ranges; authoritative clarity/light-to-visibility mapping and line-sampling cost; how local disturbance travels or decays; alert timers and memory; individual/species differences; UI/auditory accessibility; how gameplay fog shares physical parameters with the graphics preset; and exact death/damage rules are **not decided by S20**. Scope and tune them at P5. Do not infer hidden P4 or P5 implementation from this approved design document.
