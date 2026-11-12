# Day/Night and Weather

**Decision date:** 2026-11-12  
**Status:** Owner-approved staged environment contract. Day/night is first; weather is a later modifier.  
**Applies to:** Shared Fish environmental presentation across Pond and Ocean, with habitat-specific optical response.

## Core decision

Fish uses one continuous in-game environmental time source. It is independent of real-world wall-clock time and exists so sky, sun, ambient light, water reflections, underwater illumination and later ecology can agree on the same world state.

Implement this in stages:

1. **Day/night first:** continuous solar time, sky, direct/ambient light, reflection response and underwater daylight.
2. **Weather second:** cloud cover, overcast, rain/storm, fog, wind and weather-driven surface changes modify the established solar baseline.

Do not build both systems at once. Existing static overcast tuning may remain as compatibility behavior while day/night is introduced, but it is not the final weather system.

## Time and ownership

The authoritative environment-time state exposes a normalized day fraction and a human-readable time-of-day value. Exact full-day duration remains configurable and must be tuned through native play rather than hard-coded as a permanent design decision.

Requirements:

- Game/environment time advances only while gameplay simulation is active.
- Pause freezes environmental time and all time-driven presentation.
- Closing the game does not imply offline environmental simulation.
- Development-only controls may set or accelerate time for qualification.
- Rendering consumes time; rendering does not become the authority for future ecology or saves.
- Pond and Ocean share the clock/solar state but may map it to different colors, attenuation, ambient floors and effect strengths.

## Continuous solar cycle

Use continuous interpolation, not a sequence of abrupt lighting presets. Dawn, morning, noon, afternoon, sunset, twilight and night are tuning anchors only.

Solar state should provide at least:

- sun direction/elevation;
- direct-light color and energy;
- ambient/environment energy and color;
- a daylight scalar usable by water transmission, shafts and caustics;
- sky/horizon parameters;
- optional moon/night-light state where needed for readability.

The existing directional sun should become a consumer of this state rather than retaining a fixed rotation/color/energy.

## Sky contract

The sky must no longer read as a flat uniform background. Prefer a bounded procedural/dynamic sky suitable for Godot Forward+ and continuous time changes.

Visual intent:

- **Dawn:** cool-to-warm horizon transition, low directional light.
- **Morning:** strengthening blue/cyan sky and warmer low-angle sun.
- **Midday:** brightest sky/daylight, compact solar highlight, short shadows.
- **Afternoon:** warmer directional light and longer shadows.
- **Sunset:** localized warm horizon and bright low solar region without tinting the entire world orange.
- **Twilight:** rapidly diminishing direct light with cool blue ambient.
- **Night:** dark blue-black atmosphere with enough moon/starlight or ambient floor for playable silhouettes; not full daylight and not total black.

The horizon must blend naturally into atmosphere and water reflections without a hard flat band.

## Water and reflection coupling

Water must consume the actual environmental state rather than inventing an unrelated sky color.

At minimum:

- the water reflection/highlight direction follows the real solar direction;
- reflected sky color/brightness changes with time;
- low sun produces elongated/broken warm reflection streaks where the surface supports them;
- midday reflection becomes brighter/cooler and generally more compact;
- night surface becomes substantially darker with only plausible night-sky/moon contribution;
- shallow transmission remains distinct from reflection and still obeys Pond/Ocean optical profiles.

Day/night must not undo the P12 surface-transmission work that keeps a near-surface fish readable from above.

## Underwater daylight

Underwater illumination derives from the same solar state through habitat-specific response curves.

Solar time may influence:

- surface daylight transmission;
- directional sun energy/color;
- caustic intensity and color;
- shaft direction/brightness;
- ambient water light;
- readable useful contrast.

It must **not** silently change turbidity/extinction just to make night darker. Darkness is an illumination change; suspended sediment and optical extinction remain separate properties.

Initial qualitative anchors:

| Time | Above water | Underwater |
| --- | --- | --- |
| Midday | strongest direct/ambient daylight | strongest shafts/caustics and useful contrast |
| Late afternoon | warmer, lower-angle sun | reduced brightness with warmer surface influence |
| Sunset | pronounced warm horizon/reflection | rapidly diminishing direct contribution |
| Twilight | cool ambient, weak direct light | low contrast and reduced natural visibility |
| Night | dark sky, moon/stars or low ambient floor | genuinely dark, local silhouettes/detail only |
| Dawn | cool-to-warm transition | daylight and contrast gradually return |

Exact values are tuning targets, not locked physical measurements.

## Habitat profiles

The solar state is shared, but visual response remains habitat-specific.

**Pond:** muddy olive-green transmission, stronger local canopy/bank contrast, limited shallow readability and darker deep response.

**Ocean:** preserve the accepted blue/turquoise Forward+ identity and its existing depth structure.

Switching habitats must not leak previous profile values, time controls or transient shader state.

## Weather — deferred Stage 2

Weather is intentionally excluded from the first day/night correction.

Later weather may add:

- clear / partly cloudy / overcast states;
- dynamic cloud coverage;
- rain and storm presentation;
- fog/mist;
- wind;
- weather-driven wave/ripple changes;
- weather-specific daylight attenuation and reflection breakup.

Weather modifies the solar baseline. It must not create a second independent sun/clock or permanently overwrite habitat profiles.

## Performance boundaries

Prefer bounded scalar/environment/shader updates and Godot Forward+ sky/environment features over expensive extra scene renders.

Avoid making these baseline requirements:

- planar-reflection cameras;
- multiple shadow-casting suns/moons;
- dense cloud raymarching;
- large additional full-resolution render targets;
- per-frame world regeneration.

Measure actual native Forward+ cost before accepting a more expensive technique.

## Native acceptance

Qualification must include fixed-state observations at representative times, approximately:

- 06:00
- 09:00
- 12:00
- 17:00
- sunset
- twilight
- 00:00

At each important state, inspect both above-water and underwater views where practical.

Verify:

- smooth continuous transitions without exposure/color pops;
- correct solar direction and moving shadows/highlights;
- water reflection responds to sky/time;
- underwater shafts/caustics/daylight agree with the sun;
- night is dark but playable rather than fogged or fully black;
- pause freezes the cycle;
- debug acceleration/set-time controls cannot leak into normal gameplay;
- Pond and Ocean retain independent visual profiles;
- no shader/runtime errors or material performance regression.

Native visual judgment is required. Headless/source checks can verify bounds, isolation and state mapping but cannot prove the day/night appearance.

## Future gameplay use

Later ecology may consume the same authoritative environmental time for activity windows, feeding risk, predator behavior or other natural rhythms. This document does not define those mechanics and the day/night rendering correction must not pre-implement them.
