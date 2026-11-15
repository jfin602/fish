# Shared player water reactions — local correction evidence

**Status:** Implemented in the dirty local tree on 2026-11-15. Focused headless checks pass; owner native visual qualification across all Bass stages and both ordinary worlds remains open. Package version and roadmap Gates are unchanged.

## Cause and correction

Pike Encounter initialized its player separately from `Main` and omitted `WaterReaction`. The old shared wake also used fixed Fish3 tail offsets and a GPU ribbon that produced a long cyan line near the tiny Bass fry in a live Forward+ capture. The encounter now attaches exactly one shared component after the Pond waterline is known and updates it when the Bass hatches or grows. Pause, caught state and scene exit suspend it.

The same `WaterReaction` now measures the attached player mesh in Ocean, Pond and Pike Encounter. Its wake emitters span the rear body; short independent particles replace the stretched ribbon. Particle lifetimes vary, with a longer maximum and bounded emission. Sizes follow model length, surface contact follows body radius, and boost enhancement follows actual velocity, effort and active energy. Pond particles use a dark green blue tint drawn from the Pond water profile; Ocean particles use a lighter tint drawn from the Ocean profile. Normal playable species use a capsule fitted to their model, while Bass stages keep their registered mesh-derived capsules. Visual effects remain presentation-only and do not feed predator sensing.

## Checks and limits

- `./tools/dev import` and `./tools/dev build`: pass, zero build warnings.
- `./tools/dev inspect-hatch`, `inspect-growth`, `inspect-auto-feed-boost`: pass. These cover egg suppression, one effect owner, stage geometry, bounded surface rings, growth cleanup and boost movement.
- `./tools/dev inspect-water`, `inspect-pond-surface`, `inspect-character-select`, `inspect-pond-creatures`: pass. These cover shared Ocean/Pond routes and measured player colliders.
- Native Godot 4.7.2 Forward+ captures: the original line is visible in `artifacts/pike-water-reactions/larva-idle-later.png`; the later shared-particle wake has no ribbon in `artifacts/pike-water-reactions/final-native-fry.png`. These images are ignored local artifacts, not owner approval.
- The live Pike fry scene was launched through Godot Editor MCP. A single capture does not establish the desired appearance during juvenile, subadult and adult boost or near-surface swimming. No frame-time comparison was recorded. The owner still needs to judge those native views before this correction is qualified.

Unrelated in-progress Pond lighting files and diagnostics were preserved. The roadmap remains the sole Phase/Gate authority.
