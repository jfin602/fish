# P3/P7 native world-art and performance review

**Disposition, 2026-11-10: FAIL for target style; owner acceptance BLOCKED.** This is an observed `0.3.6` candidate, not a qualified `0.3.7` release. Branch `main`, pre-task HEAD `2261d4facc577fa85f3ca15e21cb34c46c7eec2c`, with only the P7 Character Select repair and evidence tooling uncommitted. P2 movement, collision, camera, seabed/depth query, water and light authority were not edited. P2 G2.3/V20 remains its accepted open gap.

## Native setup and method

Godot `4.7.2.stable.mono` / .NET SDK `10.0.403`, Vulkan Forward+, AMD Radeon 880M (RADV GFX1150), Linux/X11 `:0`, 1920 × 1200 windowed fullscreen. Normal play used the saved Character Select → `main.tscn` route, keyboard movement, Escape Pause menu and the actual third-person follow camera. The P2 continuous collidable seabed, five named depth bands, cyan surface, depth fog/lighting, shafts and caustics remain the runtime base. The Godot Editor MCP bridge was disconnected; no editor mutation was performed.

The repeatable [native probe](../../../../game/dev/p3_native_probe.gd) instantiates the normal main scene with Forward+, samples 90 rendered frames at each authored marker and captures the follow camera at 1920 × 1200. It teleports between markers; it does **not** prove swimming reachability or collision comfort. Its player is the legacy fallback fish because no Character Select identity is supplied. Separate [five-fish native captures](character-select.md#p7-native-addendum--2026-11-10) show the selected bodies. The probe also toggled the P2 underlay and FAF terrain visuals in memory for diagnosis; no saved scene was changed.

For a repeat run, invoke the pinned Godot .NET executable documented in [development setup](../../../development.md) from the repository root with `--path game --rendering-method forward_plus --script res://dev/p3_native_probe.gd -- --p3-capture-dir="$PWD/artifacts/p3-p7-native/probe"` in a visible 1920 × 1200 X11 session. The emitted `P3_NATIVE_SAMPLE` lines are the source of the table below.

| P7 area | Status | Observation |
| --- | --- | --- |
| Character Select/session routing | **PASS for observed native route; BLOCKED for owner acceptance** | Five selected fish loaded and returned to Character Select; see [character selection](character-select.md). |
| Shallow → upper → middle → lower → deep → return and alternate route | **NOT OBSERVED** | Native probe visited all five markers, but no continuous hand-controlled traversal of both lanes was completed. P2 accepted route evidence and P3 headless clearance checks remain structural evidence, not this P7 native traversal. |
| Both tunnel/arch routes, cave chamber, deep corridor and overhang clearance | **NOT OBSERVED** | [Tunnel entrance](evidence/p7/TunnelEntry.png) and [shelter](evidence/p7/ShelterCenter.png) were viewed from markers. A live passage and camera compression/recovery test was not completed. P3 structural clearance evidence reports 0.62 m minimum visual-envelope margin and 1.38→4.25 m camera recovery; those checks do not qualify native feel. |
| Depth density gradient | **FAIL visually; PASS structurally** | The authored 28 pockets / 295 placements are weighted shallow to deep (128/84/54/20/9); 31 ambient fish are distributed 13/8/6/2/2. The [shallow capture](evidence/p7/DepthShallow.png) has more color than [middle](evidence/p7/DepthMiddle.png) and [deep](evidence/p7/DepthDeep.png), but broad central views do not read as a lush high reef or a rich middle reef. Lower/deep are sparse, yet patchwork dominates rather than intentional geology. All 38 variants load and are placed by P1–P6 checks, but coherence of each variant in normal play is **NOT OBSERVED**. |
| Target style and natural geology | **FAIL** | Against [reef style target](../../../fish-asset-factory/references/reef-style-target.png) and [style-proof 001](../../../fish-asset-factory/style-proof-001/README.md), the live scene has large hard-edged light/dark floor patches, visible overlaps, isolated flat ledges and floating-looking slabs. The caves/arches do not read more naturally than proof 001's weak three-boulder lintel. Warm sand, cool faceted stone and controlled coral colors exist locally, but broad views lack the target's layered lushness, arch mass and clear integrated swim-through. [Combined spawn](evidence/p7/spawn-combined.png) versus [P2 underlay alone](evidence/p7/spawn-underlay-only.png) isolates the strongest patchwork to the added visual composition; a temporary no-shadow probe did not materially improve it. This comparison does not establish a single root cause. |
| Ambient motion | **NOT OBSERVED visually** | Nine schools / 31 fish loaded in native logs. Their loops and route interactions were not judged during sustained swim. Plant sway is not installed; current plants are static, so fixed-root/phase behavior is not applicable. |
| Performance | **PASS for representative measurement; BLOCKED for acceptance** | The sample table below is reproducible, with no engine errors in the probe. No production frame-time target or owner smoothness decision is set. Density/cost tuning was not applied because the dominant P7 failure is world composition, and a shadow toggle did not address it. |

### Captured views

The [shallow](evidence/p7/DepthShallow.png), [middle](evidence/p7/DepthMiddle.png), [tunnel](evidence/p7/TunnelEntry.png), [shelter](evidence/p7/ShelterCenter.png) and [deep](evidence/p7/DepthDeep.png) images retain the actual P2 atmosphere and follow camera. The shallow close vegetation and coral pockets show that the assets can be colored and faceted coherently at short range. Wide framing exposes the terrain joins and thin geological layering. The deep image has negative space but reads as a mostly empty, patchy basin. No fully hand-controlled foreground/midground/background swim-through was captured, so that requested style view remains **NOT OBSERVED**. No visible modular perimeter was established in these views; absence of a perimeter during full traversal is also **NOT OBSERVED**.

### Performance sample

The probe recorded Godot `TIME_FPS` and median per-frame draw-call/primitive monitors over 90 frames after 0.6 seconds settling at each marker. These are local, short, marker-teleport observations with the whole normal scene (including 31 ambient rigs) loaded. They are relative diagnostic data, not a production benchmark. The shallow minimum includes startup/settling behavior.

| View | FPS min / median | Draw calls median | Rendered primitives median |
| --- | ---: | ---: | ---: |
| Dense shallow | 20 / 60 | 201 | 100,948 |
| Upper | 49 / 52 | 151 | 86,362 |
| Mixed middle | 49 / 49 | 116 | 76,762 |
| Lower | 53 / 53 | 77 | 58,724 |
| Deep sparse | 53 / 53 | 43 | 34,648 |
| Tunnel entrance | 42 / 60 | 72 | 56,114 |
| Cave/shelter | 50 / 50 | 77 | 49,516 |
| Deep corridor | 53 / 53 | 51 | 36,456 |

The scene retains 51 terrain chunks, 38 geology pieces, 28 MultiMesh dressing batches with 295 placements, and 9 ambient schools with 31 fish. No variant, placement, fish or decorative collision was removed during P7. The no-shadow visual comparison did not justify a saved shadow change; no density or population tuning is claimed.

## Repair and handoff

1. Repair the FAF terrain / P2 underlay composition so overlapping chunks and height transitions stop forming visible plate edges and high-contrast floor patches from the follow camera. Diagnose geometry/material overlap before adding more dressing.
2. Rebuild the cave and arch hero forms into continuous rock masses with a legible entrance, roof and exit; review alongside proof 001 and the reef target.
3. Place a visible shallow/high reef foreground and richer middle layering around the actual swim lanes while keeping the route open; preserve all 38 variants and the shallow-to-deep count gradient.
4. Re-run continuous native traversal in both directions and through both tunnels/cave/deep corridor, recording camera and collision honesty, then remeasure representative frame behavior after changes.
5. Obtain explicit owner judgment on the repaired world-art direction and Character Select readability/scale/feel. Only then consider P7 `0.3.7` and P8 closeout.

The present candidate is **rejected for P7 progression on the observed style criteria**. This is a reviewer observation from the native captures, not a claim that the owner has accepted or rejected the candidate. Root version remains `0.3.6`.
