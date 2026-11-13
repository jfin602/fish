# P5 P3 — World-space perception source check

**2026-11-12 result: PASS for focused source/headless checks; native gameplay: NOT OBSERVED.** Pre-task HEAD was `c9486e3c408ff02d7c81807ead15ae7003d553da` on `main`, clean, root `0.5.2`. That commit is the P2 runner checkpoint; [P2 evidence](p2-cover.md) records the actual FAF intake and seed-5202 fixture. [P4 closeout](../../p4/closeout.md) authorizes progression while retaining NOT GREEN Gates. P2 was a T2 task, not a manual stop. This P3 record describes uncommitted source for the runner checkpoint and claims no implementation commit.

## Implemented boundary

`WorldSightSensor` evaluates an observer and target in world space, with target radius/contrast, heading, 30 m range, three bounded physics rays, seven intervening water samples and independent solar/depth illumination. It checks real collision including the P2 wood wrappers and mud bed, excluding the two actor bodies. Visible mesh bounds from the 47-asset P5 cover fixture supply partial plant attenuation; they confer no hard occlusion or categorical hidden state. Sampling is capped at five updates per game second per sensor pair and 128 visible mesh bounds, with pause/invalid/habitat/reset clearing stale observations. Reasons and score components are diagnostic data for the focused check and later AI wiring. No camera, GFX preset, shader fog or rendered pixels are read.

The fixture uses its actual bed and waterline for a deterministic sediment approximation and a fixed noon solar state matching its fixed scene light. `SightEnvironment.ForBakedWorld` samples the existing seeded `PondTurbidityField` for baked Pond and uses a clear baseline for Ocean; neither ordinary world gained a sensing actor or predator. P5's Pike remains stationary. The plant proxy is a conservative visible-mesh bound, so fine leaf gaps and native readability remain for later review.

## Evidence

Source SHA-256: `game/scripts/WorldSightSensor.cs` `755c0abb15d3f2e2fcdd7f320833d014c92fdcf7eb38c8818cac92e3d9185a5e`; `game/dev/PikeSightChecks.cs` `b6d7de12862bab17a02cadfee5bbdd98e87a909006fe587c9d17ad394da839f3`. Fixture seed `5202`; Pond basin seed `71`; Ocean baseline seed `42`. Godot Editor MCP server `2026.11.04` reported bridge **disconnected**, with no connected project identity; no editor mutation or native play claim was made. CLI `tools/dev` points to this repository's `game/` project and loaded Fish scenes.

| Actual command | Result and limit |
| --- | --- |
| `./tools/dev build` | **PASS**, Godot .NET/C# zero warnings/errors after sensing changes. |
| `./tools/dev inspect-pike-sight` | **PASS**, seed 5202: near open `0.460`, 12 m clear path `0.131`, muddy intervening path `0.024`; 66 visible plant mesh bounds. Checked near/far/rear, three water depths and target sizes, noon/night, actual Pond field repeatability and Ocean baseline, frequency/pause/invalid/reset, camera/fog invariance, blocked/open rays for all five new wood families and partial attenuation for all five new plant families. Fixed-pose and scripted assertions only. |
| `./tools/dev inspect-pike-cover` | **PASS**, existing P2 fixture route/clearance regression: 47 instances, 193 collision shapes, fish/Pike hollow traversal and zero sampled camera overlaps. |
| `./tools/dev inspect-ocean-equal-input` | **PASS**, baked seed-42 Ocean equal-physics 30-step route. |
| `./tools/dev inspect-pond-atmosphere` | **PASS**, seed-71 Pond turbidity and Pond→Ocean→Pond profile isolation; unchanged fish state. |
| `git diff --check` | **PASS**, no whitespace errors in the uncommitted P3 diff. |

The first focused check exposed an unsuitable cross-wall line for the split hollow and a plant line obstructed by adjacent wood. The final check uses P2's known longitudinal hollow opening and scans actual visible plant bounds for an unobstructed foliage sightline. These were check-fixture corrections; the source implementation and historical P2–P4 failures remain separate. Native controlled hiding/readability, fine plant mesh gaps, all approach angles, and real predator behavior are **NOT OBSERVED**. V05/V33–V36 and all P5 Gates remain unqualified.
