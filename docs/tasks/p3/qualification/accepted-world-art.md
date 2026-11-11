# P3 world-art acceptance record

**State, 2026-11-10: no accepted P3 world-art candidate.** The P7 native [review](native-world-art.md) found visible patchwork floor joins, slab-like geology and insufficient shallow/middle layering against the [owner-supplied reef target](../../../fish-asset-factory/references/reef-style-target.png). The owner has not made the required explicit P7 judgment on either the world-art direction or the repaired Character Select. Root version remains `0.3.6`; `0.3.7` and P8 are not qualified.

## Implemented candidate, not visual acceptance

| Area | Current authored candidate | P7 acceptance |
| --- | --- | --- |
| Normal scene and authority | `character_select.tscn` starts normal play in `main.tscn`, which instances `p2_habitat.tscn` and its reef composition. P2 seabed, collision, five depth bands, controls, follow camera and Forward+ water/light remain authoritative. | **PASS for preserved implementation; NOT OBSERVED for complete P7 traversal** |
| FAF terrain and geology | All six terrain variants contribute to 51 draped visual chunks. Four rock variants contribute to 38 geology placements around the west tunnel, east cave, deep corridor, shelves and landmarks. P2 route collision remains primary; three simplified geology boxes cover canyon walls and cave lip. | **FAIL for live visual integration** — flat shelves, exposed joins and unnatural hero masses. |
| Reef population | All 12 coral, 8 plant and 8 detail variants appear in 28 named MultiMesh batches: 295 placements, with authored shallow/upper/middle/lower/deep counts 128/84/54/20/9. Decorative dressing adds no collision. | **PASS for inventory and numerical gradient; FAIL for visible lushness/coherence** from the normal camera. |
| Ambient life | Nine presentation-only schools hold 31 fish, distributed 13/8/6/2/2 by band. They do not implement ecology or predator AI. Plants currently have no sway. | **PASS for load/count; NOT OBSERVED for sustained native loop quality**. |
| Character Select | Five selectable FAF fish use animated previews and replace only the player visual. P7 repaired fullscreen layout, focus and Enter handling. Pause → Return to Character Select passes the focused route check and five short native cycles. | **PASS for observed route; BLOCKED on owner readability, scale and feel judgment**. See [Character Select evidence](character-select.md). |
| Performance | P7 native 1920 × 1200 Forward+ marker samples measured 201/116/43 median draw calls and 60/49/53 median FPS for shallow/middle/deep. No density, grouping, ambient or decorative-collision change was made in P7. | **PASS for measurement; BLOCKED for an accepted performance budget and sustained hand-play**. |

The historical P1–P6 [implementation evidence](implementation.md) qualifies asset loading, authored positions, route margins and character/session mechanics at its stated scope. The P2 [closeout](../../p2/closeout.md) remains the movement/water/depth baseline, including its open G2.3/V20 gap. Neither document is a P7 owner style decision.

## Repair boundary before acceptance

Keep the full 38-variant environment inventory and five character choices. Rework the terrain/underlay transition and cave/arch masses first, then revisit shallow/middle dressing and depth color from the follow camera. Verify continuous native route/camera behavior and representative performance after those changes. Finally ask the owner to judge the revised Character Select and world-art captures during normal play. Do not assign `0.3.7` until the candidate is reproducible and both judgments are positive; P8 remains a later closeout.

## 2026-11-11 progression note

The earlier state above remains the P7 observation at its source. Subsequent C3 M1–M4 produced the corrected `c3-full-12` Ocean candidate. The owner approved progression to P4 with its exposed shallow geological face, angular cave transitions, incomplete full native traversal and V25 visibility qualification kept open. That decision accepts the Ocean as a P4 foundation; it does not mark this P7 art review PASS or create `0.3.7`/`0.3.8` checkpoints. See the [P3 direct closeout](../closeout.md) and [M4 evidence](../../c3-world-gen/qualification/m4-habitats.md).
