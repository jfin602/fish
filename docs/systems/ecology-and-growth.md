# Ecology, feeding, and growth

## Status and ownership

**Planned; no implementation or gameplay qualification.** Eating food and suitable smaller fish, avoiding predators, and growing from hatchling to large fish are confirmed. Species, diets, size thresholds, damage, hunger, and numerical balance remain open. This document owns their working gameplay rules.

## A bounded first food chain

Start with one compact habitat, one player species, small food, one prey type, and one predator type. Use clear visual size differences and a documented rule for what can be eaten. The player should learn the rule through understandable encounters.

The prototype should allow at least one meaningful role reversal: something initially dangerous becomes suitable prey after the player has grown enough. Whether that means the same individual or another member of its species depends on actual entity tracking. A later story must use the identity the simulation really recorded.

## Feeding transaction

Successful feeding consumes a real target or food item and awards nutrition once. A target already consumed, dead, invalid, or outside the allowed interaction cannot produce a second reward. Visual effects, repeated input, overlapping collision callbacks, and saving during an encounter must not duplicate nutrition.

The working control proposal is contact collection for tiny food and a directed bite for live prey. Adopt the simplest rule that gives the first hunt and escape a readable outcome. Bite timing, reach, damage, and instant swallowing require a scoped decision before qualification.

## Growth

Food is the proposed driver of growth. Growth changes visible body size and relevant capability within the same fish identity. Exact levels, continuous scaling, thresholds, maximum size, and rates remain tuning decisions.

Size should change the use of space: a small fish can fit into cover unavailable to a large fish, while a larger fish can pursue bigger meals. Growth must feel rewarding. Handling tradeoffs should preserve useful advantages rather than erase progress.

Keep the authoritative size/growth state separate from its presentation. Movement, collision, edible-target checks, and camera offsets must consistently derive from the selected growth model. Reopening the game or changing the camera must not award growth or change edible-target eligibility.

## Predator and prey behavior

Proposed prey behavior is simple local fleeing with use of available cover. Proposed predator behavior includes detection, approach, a readable attack commitment, pursuit, and disengagement. AI should act on the same physical and feeding rules used to explain player outcomes.

The initial environment need not simulate a complete self-balancing ecosystem. Add schooling, breeding, competing predators, environmental cycles, and population recovery when a concrete gameplay need justifies them.

Food replenishment and enemy respawning need explicit rules. Save/load must not regenerate a cleared feeding spot or reset a pursuit by accident. Later region streaming needs the same treatment.

## Hunger and injury

A draining hunger meter is a proposed option, not a confirmed requirement. Feeding already has a growth purpose. If hunger is adopted, define its pressure, warning, and terminal consequence; do not add offline hunger. Injury, healing, and species-specific defenses likewise require explicit behavior and visible feedback.

## Acceptance targets

- Award a consumed item or prey reward exactly once.
- Show growth that corresponds to actual feeding and persists across sessions.
- Apply the same documented size rules before and after resume.
- Make at least one hunt and one escape understandable in native play.
- Prove the role reversal without silently replacing the player fish.
- Preserve consumed targets, food state, and encounter state through saving.
- Record real milestones for [life stories](life-stories-and-history.md) without promoting a mere pursuit into a kill or an ordinary turn into an escape.

The [roadmap](../roadmap.md) determines when these targets enter a Phase; [validation](../validation.md) determines the evidence required.
