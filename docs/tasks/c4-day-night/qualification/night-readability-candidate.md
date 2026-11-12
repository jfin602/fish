# c4 night readability and Pond shore alignment — 2026-11-12

**Disposition: native candidate, NOT GREEN.** The requested night correction and the owner's Pond shore direction adjustment are implemented at root `0.4.11`. These fixed diagnostic views establish a reviewable rendering candidate; they do not constitute player-view or owner acceptance. P4/P12 and c4 remain NOT GREEN.

## Behavior

- Pond and Ocean now use separate low night ambient floors. Pond stays olive/blue-green and retains nearby fish, log and floor shapes; Ocean stays blue/cyan-black. Direct light, shafts and caustics diminish with the shared cycle. The directional light supplies only weak moonlight at night; no new light, shadow map, render pass or camera-relative helper was added.
- Existing fog/extinction density and the seeded Pond turbidity volume do not increase at night. Ocean fog *color* darkens with illumination, while its density stays fixed. A bounded saturation reduction in the existing underwater screen pass and a faint underside surface glow support distance loss and upward orientation.
- The fixed world sunset axis now matches the Pond far-shore view. In the native scene the camera forward is `(-0.887818, 0, 0.460194)` and the 18:00 sun direction is the same. The opposite night direction places the moon on that axis as it descends toward 06:00. A small sky moon disc/halo follows the moon direction; warm twilight remains tied to the actual sun direction. This affects both habitats through the shared solar state and does not pin either body to a moving camera.

## Native views

Captured with Godot `4.7.2.stable.mono`, Forward+, AMD Radeon 880M, the baked seed-71 Pond and seed-42 Ocean, and an isolated keyboard-control data profile. The [native probe](../../../../game/dev/c4_day_night_native_probe.gd) holds the shared clock at each labeled hour and moves a diagnostic camera. Its setup and completion markers passed with no `ERROR`, `WARNING`, shader failure or script failure in the successful final runs. The views remain diagnostic and do not prove actual player camera feel.

| View | Evidence | Observation |
| --- | --- | --- |
| Pond near fish, 12:00 / 00:00 | [day](night-readability-evidence/verified-pond_pond_night_near_1200.jpg) · [night](night-readability-evidence/verified-pond_pond_night_near_0000.jpg) | Night fish and local log retain an outline; background water loses detail. |
| Pond mid, cover, open, deep, up at 00:00 | [mid](night-readability-evidence/verified-pond_pond_night_mid_0000.jpg) · [cover](night-readability-evidence/verified-pond_pond_night_cover_0000.jpg) · [open](night-readability-evidence/verified-pond_pond_night_open_0000.jpg) · [deep](night-readability-evidence/verified-pond_pond_night_deep_0000.jpg) · [up](night-readability-evidence/verified-pond_pond_night_up_0000.jpg) | Cover and immediate floor remain faintly legible; open/deep distance is much less certain; the surface is a dim upward cue. |
| Ocean near, 12:00 / 00:00; deep 00:00 | [day](night-readability-evidence/verified-ocean_ocean_night_near_1200.jpg) · [night](night-readability-evidence/verified-ocean_ocean_night_near_0000.jpg) · [deep](night-readability-evidence/verified-ocean_ocean_night_deep_0000.jpg) | Blue/cyan profile is distinct from Pond and darkens toward depth. |
| Pond far shore, moon setting 05:00 / 05:30 | [05:00](night-readability-evidence/alignment-final2_pond_far_shore_0500.jpg) · [05:30](night-readability-evidence/alignment-final2_pond_far_shore_0550.jpg) | Moon descends on the centerline above the bank. |
| Pond far shore, sun setting 17:30 / 18:00 | [17:30](night-readability-evidence/alignment-final2_pond_far_shore_1750.jpg) · [18:00](night-readability-evidence/alignment-final2_pond_far_shore_1800.jpg) | Sun descends on the same centerline. |

## Checks and limits

- `./tools/dev build`, `inspect-environment-time`, `inspect-solar-lighting` and `inspect-pond-atmosphere` passed. The solar checks covered 1,441 samples per habitat and resource reuse. The native fixed shore pass showed `direction_error=0` and `shaft_error=0` at all sampled hours.
- Native PlayShell pause/resume passed three cycles with clock hour and surface time frozen during pause. The accelerated Pond cycle passed 440 frames and one wrap with no observed preset jump or new sky/surface resources.
- Same-view 90-sample capped Forward+ Pond noon/night waterline medians were `16.65/16.67 ms` at 74 draws; underwater `16.66/16.62 ms` at 74 draws. Ocean waterline medians were `18.80/16.79 ms` at 153 draws; underwater `16.69/16.67 ms` at 126/125 draws. Ocean waterline p95 varied across both states, so these measurements only show no clear night-specific cost at the cap. They preceded the small final sky-axis/moon-disc edit; no final uncapped GPU timing was taken.
- A hollow-trunk/shelter entrance and moving player-camera route were not separately captured. Real OS focus-out/refocus remains unverified as recorded in [P4 native evidence](native-day-night-p4.md). Full-cycle visual acceptance remains the owner's decision.

No version advance, Gate pass, weather system, commit or push was made.
