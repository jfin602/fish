# P6 — The Living Pond: Bass survival from hatchling to adult

**Decision:** S27, owner-approved 2026-11-15. **State: DESIGN APPROVED / NOT STARTED.** This is the next phase after P5 owner-closed for progression / NOT GREEN. Approval does not implement or qualify any P6 system. [Roadmap](../../roadmap.md) owns Gates; [validation](../../validation.md) owns V44–V51; [ecology](../../systems/ecology-and-growth.md), [stealth](../../systems/stealth-and-detection.md), [swimming and camera](../../systems/swimming-and-camera.md) and [UX](../../ux-and-presentation.md) own their respective contracts.

## Player outcome and why this precedes persistence

The original P6 persistence plan is deferred unchanged to P7. Current P5/C5 encounter feeding can be completed by collecting nearby fixed food until a later Pike becomes relevant; the early game lacks meaningful hunting pressure, varied threats and reasons to explore. P6 must make one **continuous, interesting, dangerous Bass life** in the ordinary **Freshwater Pond** before saving that life becomes a worthwhile milestone.

The player hatches near safe nursery cover, learns to forage under threat, experiences adult Bluegill and shallow-water creature predation, gradually begins hunting smaller Bluegill, competes with larger solitary Bass and eventually explores deeper feeding grounds frequented by Pike. Danger exists from early play onward, not only at adulthood. Progress is **ecological and spatial**, not scripted depth gates or mandatory quests. The same species can change from danger to food as the Bass grows.

## Entry and existing-state boundary

- **P6 player path:** World Select → Freshwater Pond → Bass selection → genuine Bass egg/nest introduction → same Bass identity through larva/fry, juvenile, subadult and adult. Reuse the P5/C5 staged Bass exports and hatching, growth, feeding, schooling, predator, and cover systems after verifying their actual current local implementation. Other ordinary Pond character choices remain non-survival demos until separately supported; do not pretend they have full lifecycles.
- **Keep Pike Encounter:** the third World Select destination remains an isolated, repeatable gameplay/AI regression fixture with ephemeral outcomes, not a third habitat. Preserve its original seed 5266, source/evidence and C5 manual correction history. **Normal Pond** uses its own independently baked world and is where P6 introduces ecological threats. Ocean remains untouched, including species eligibility and world/bake identity.
- **Existing FAF input:** Bluegill `FAF-FISH-006` and Bass `FAF-FISH-007` have five registered stages; Pike `FAF-FISH-010` is already present. Original Pond creature pack 001 includes **Northern leopard frog** `FAF-CREATURE-001`, **Northern water snake** `FAF-CREATURE-002` and **common snapping turtle** `FAF-CREATURE-003`, with reviewed editable Blender sources, GLBs and movement animation clips. These are **technically verified candidate assets only**: no gameplay AI, attack, predator, damage or integrated natural Pond review is claimed. Reuse frog and snake assets before requesting additional species modeling; snapping turtle is optional later scope.
- Respect the camera, wake, audio, S25 input, and C5 work included in the `0.6.0` development baseline. Inspect the actual branch/HEAD and dirty tree before coding; preserve any later local changes.

## Full-life food-chain progression

| Player stage | Primary food and hunting | Natural threats | Space and lesson |
| --- | --- | --- | --- |
| Egg / newly hatched fry | Yolk first, then nearby zooplankton and tiny aquatic invertebrates | Adult Bluegill that can opportunistically consume tiny fry; occasional suitable amphibian encounters near surface | Nest, reeds, fine plants and tiny shelter gaps teach concealment and orientation |
| Juvenile Bass | Aquatic insect larvae, larger invertebrates, appropriate tadpoles where modeled | Adult Bluegill, water snakes and larger Bass | Shoreline/reed/root corridors teach deliberate crossings and response to pursuit |
| Subadult Bass | First size-eligible small Bluegill and supplemental invertebrates | Larger solitary Bass, snakes in appropriate habitat, occasionally Pike | Wood/vegetation transition lanes teach covered stalking and role reversal |
| Adult / mature Bass | Larger size-eligible Bluegill and other explicitly adopted, compatible food | Pike and larger competitors; no absolute invulnerability | Patch depletion pushes wider exploration into deeper vegetation edges and less-sheltered feeding areas |

These are **relative gameplay roles**, not fixed numerical diet, height or growth thresholds. Predation uses real body, species, stage and mouth eligibility. Adult Bluegill are a legitimate risk mainly to **very small** Bass, not an arbitrary always-lethal hunter of any juvenile. The existing **northern leopard frog** can occasionally take sufficiently tiny fish if playtests and naturalistic behavior support it; do not silently substitute an American bullfrog model or make it a persistent deep-water fish chaser. Water snakes are stronger shallow/surface fish predators. A Pike may sometimes approach shallow vegetation; its density/opportunity rises toward deeper cover, not through an invisible depth fence.

## P6 work order and Gates

### G6.1 — Feeding reticle and aim readability (V44)

A small, restrained center-screen viewing/feeding reticle appears during active swimming. Because the camera follows with lag and can independently orbit in Hybrid 2 or Adaptive Follow free-look, the center is **not always the fish's actual mouth direction**. Provide a subtle projected fish-forward mouth heading indicator when they diverge, with readable behavior near the edge of the view; do not pretend the center reticle alone guarantees a hit. Make the result legible across Bass sizes, camera presets/zoom, near cover and waterline crossing. During cinematic, pause/menu/focus loss or other disarmed state, hide or appropriately suspend gameplay cues.

On a **confirmed** eligible unobstructed forward mouth contact, use a brief, restrained reticle response; no false “edible/ready” status merely because camera center overlays a fish. Preserve automatic feeding with exactly-once nutrition and no player bite, camera-driven consumption, auto-aim, magnetic prey movement, enlarged bite zones or predator-sensor leak. Reticle is presentation only. **Fix this first** to remove the owner's immediate aiming frustration.

### G6.2 — Distributed living food and local depletion (V45)

Replace fixed near-nest infinite/easily repeated feeding routes in the P6 normal Pond survival path with deterministic, accessible food patches keyed to actual depth, light, vegetation, log and shoreline habitats. Occupied spots deplete when consumed; use bounded replenishment, drifting or moving food and school migration with stable entity and consumed state. No rapid identical-position respawn; no entire-life soft lock from exhausted food. Later Bass stages get diminishing reward from tiny food relative to their nutritional needs and benefit from wider hunting. Food remains discoverable without a permanent omniscient nearest-food waypoint. Tune hunger/starvation only after actual play; do not infer a required draining hunger meter.

### G6.3 — Early adult-Bluegill predation (V46)

Use appropriate adult Bluegill as bounded opportunistic predators of tiny Bass in nursery and reed areas, including real sensing, obstruction, approach, attack commitment, contact, escape and disengagement. No magical knowledge through plants or rocks. The player can feed safely with stealth; exposed careless approaches can result in a genuine catch. Larger Bass become size-ineligible for Bluegill attacks as appropriate. Preserve Bluegill age-structured schools and allow fish to react to threats without requiring the player nearby.

### G6.4 — Frog and water snake integration (V47)

Integrate existing Northern leopard frog and Northern water snake FAF models in spatially appropriate bank, surface, shallow and reed routes, with convincing movement, physical bounds, stage-appropriate diets, perception and attacks. Let the frog be mostly shoreline/near-surface and prey upon only genuinely plausible very small fish; snake patrols shore/water and can hunt juvenile Bass or fish NPCs when reachable. No long-distance tracking, teleporting or never-ending chase. Snapping turtles remain optional after the initial two animals are qualified. Separate actual frog and snake actions from animated ambient proof.

### G6.5 — Solitary Bass competitors and cannibalism (V48)

Introduce independent juvenile/subadult/adult Bass NPCs drawn from existing registered stage exports, not another school. They forage, rest, occupy sensible cover and sometimes hunt smaller Bass—including the player—when genuinely sensed, eligible and reachable. Smaller Bass use their own cover and avoidance. Individual identity, size and finite target memory matter; avoid universal aggressive swarms, friendly-fire exceptions without ecological rules, duplicate rewards or population explosion.

### G6.6 — Independent multi-species ecosystem interaction (V49)

Generalize existing Pike/school, prey and feeding foundations into a **bounded** ecosystem where Bass, Bluegill, snake, frog and Pike have truthful predator/prey eligibility and sensing, and can react to or consume NPCs with the player elsewhere. Remove dead prey once; handle competing contacts deterministically. A predator distracted by another target provides an opportunity, never guaranteed safety or an AI reset exploit. Each species retains believable movement envelope and habitat use. Background simulation budgets, pooled perception and lifecycle/reset checks are necessary; no full scientific ecosystem, breeding simulator or chemical/blood-scent detection.

### G6.7 — Natural exploration, depth and scarcity (V50)

Make nursery, reeds, roots/logs, transition vegetation, deeper edges and open water part of one continuous navigable Pond. Vary food density, species distribution, cover, travel risk and sight range by **physical habitat**, not age-lock zones. Local depletion and increased benefit from fish prey encourage movement toward deeper feeding grounds and eventually Pike. Do not spawn Pike exclusively at player adulthood or forbid an early risky deep swim. Ensure discoverability, paths, size-dependent cover clearances, shelter affordances and retreat alternatives in native play.

### G6.8 — Owner-qualified full Bass life (V51)

Play from egg hatch through larva, juvenile, subadult and adult/mature food-chain role reversal using ordinary native controls, legitimate food and danger, and the new reticle. Observe multiple genuine hunts and evades, at least one early shallow-water threat, Bass competition, independent NPC predation, food depletion/recovery, and a naturally motivated deeper Pike encounter. Check performance, camera/input/boost and two-world isolation in real Forward+ native play; retain all missing or failed cells as NOT GREEN. Owner approval for progression is distinct from fully passed Gates.

## Architecture and invariants

- **One authoritative clock** for predators, food, growth, cooldowns, nutrition, attack resolution and simulation; camera, reticle, wake, audio, blood and HUD only observe.
- **Physical feeding:** forward mouth contact, stage/diet/size, obstruction, one stable food/prey identity and one reward/event. Rendered reticle overlap is never an eligibility predicate.
- **Fair sensing:** scene-world sight, occlusion, vegetation and bounded local movement cues; finite last-known position. Camera, HUD, water fog sliders and the reticle cannot inform AI.
- **Deterministic reproducibility and bounded runtime:** stable placement identity, finite actor counts and updates, pause/focus/retry cleanup, no invisible colliders, sustained native cost samples.
- **Provenance:** reuse existing FAF creature exports and native scene checks, verify visuals and gameplay separately; no Ocean leakage or silent world/manifest re-bake.
- **Death in P6:** a real session-local terminal catch with restart/new hatchling, not durable permanent death, saved active runs, Life History or a stored archive. P7 adds persistence and true permanent-death storage; P8 adds factual Life History; P9 integrates and qualifies the entire experience.

## Implementation and review sequence

Assess current main/local source against the approved P5/C5 baseline, then prepare runner-compatible P6 prompts with `/prompt-ass` → `/prompt-plan` → `/prompt-write p6`. Use one bounded responsibility per prompt and include focused T1/T2 checks and owner-native T3 pauses. Start with reticle/feeding usability, then patch ecology, shallow threats, competitors, interspecies interactions, spatial progression and the final owner full-life playthrough. Do not launch P7 save/death or change the package version merely because this plan was approved. Preserve P5 closeout evidence and separately owned C5 repair tasks.
