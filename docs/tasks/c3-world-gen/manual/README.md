# c3-world-gen manual R1 repair prompts

These manual prompts repair the failed P7 generated-world candidate at root `0.3.6`.

They are **not** phase-runner prompts. Run them manually and in order, reviewing each result before starting the next. Do not automatically chain all four.

The existing deterministic compiler, manifest, bake/load boundary, Base World workflow, Character Select and Forward+ presentation are preserved. The failed seed-42 provisional Base World is evidence, not a visual target.

## Order

1. [M1 — Body shape, scale and depth field](M1-body-shape-scale-and-depth.txt)
2. [M2 — Terrain-conditioned geology and tunnel blending](M2-geology-and-tunnel-blending.txt)
3. [M3 — Generated route anchors and multi-seed reliability](M3-generated-routes-and-multiseed.txt)
4. [M4 — Dense habitats and depth-dependent reef composition](M4-dense-habitats-and-depth-composition.txt)

## Shared repair target

The next candidate should no longer resemble a compact bowl filled with a few dark slabs.

The world should be roughly **250–350 m across** for the first serious generated-world target, with the exact footprint varying by seed. Generate the irregular body-of-water footprint first, derive depth inward from its shoreline, add macro terrain, then geology, then routes, then dense habitat composition.

The shallow perimeter and central/deep interior should be visibly different environments. Functional tunnel/cave mesh should be buried into surrounding generated terrain and FAF surface geology so the structural shell is rarely obvious as one isolated object.

Keep c3-visibility on hold until the new generated-world visual structure is accepted.
