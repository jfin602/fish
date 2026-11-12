# c4-day-night implementation plan

## P1 — Environment clock and solar state

Inspect actual pause ownership in PlayShell/scene tree and existing atmosphere initialization. Add the smallest shared environment-time object/service suitable for current architecture.

Likely responsibilities:

- normalized day fraction / hours;
- configurable full-day duration;
- pause-aware advancement using game process state rather than wall-clock elapsed time;
- deterministic `SetTime` and bounded acceleration/debug hooks;
- derived solar elevation/direction plus day/twilight/night scalar outputs;
- no renderer/resource mutation inside the clock itself.

Add focused checks for wraparound, finite values, continuous mapping, pause freeze and debug isolation.

## P2 — Procedural sky and lighting

Refactor atmosphere initialization so the Forward+ Environment can use a dynamic/procedural sky instead of the current static color-only background.

Drive the existing DirectionalLight3D from solar state:

- rotation/direction;
- color;
- energy;
- volumetric contribution.

Drive environment/sky parameters continuously for dawn/day/sunset/twilight/night. Prefer bounded resource parameter updates and reuse; avoid creating resources every frame.

Night should be dark and readable. A cheap moon/star/night contribution may be used, but do not add a second expensive shadow-casting light unless evidence proves it necessary.

Add focused checks that sampled times produce distinct finite states and no abrupt invalid values.

## P3 — Water and underwater daylight

Make the current water/underwater presentation consume the shared solar state.

Likely integration points include:

- water surface sun direction/highlight/daylight parameters;
- sky/reflection contribution;
- `surface_daylight`;
- shaft direction/brightness;
- caustic energy/color;
- underwater ambient/direct energy.

Keep habitat-specific response curves. Pond stays muddy/green and Ocean stays blue/turquoise. Do not alter extinction/turbidity to simulate night.

Add time-sweep diagnostics/probes that can set fixed time deterministically and report relevant environment/shader values. Keep ordinary gameplay free of debug UI unless a temporary tuning control is clearly useful and removed before closeout.

Measure same-view Forward+ frame timing/draw/primitives at representative day and night states if the sky technique changes render cost materially.

## P4 — Native closeout

Use native Godot 4.7.2 .NET Forward+ and a controlled Pond/Ocean route.

Observe representative fixed times approximately:

- 06:00 dawn
- 09:00 morning
- 12:00 midday
- 17:00 afternoon
- sunset
- twilight
- 00:00 night

At key points capture above-water and underwater states.

Check:

- smooth accelerated transition through a complete cycle;
- sky/horizon quality;
- sun direction and environmental shadows/highlights;
- Pond water reflection/highlight behavior;
- near-surface fish readability retained;
- underwater daylight/shaft/caustic response;
- pause freeze and resume;
- Pond→Ocean→Pond profile isolation;
- errors and performance.

Owner visual judgment is required before calling the correction green.

## Likely source

Inspect rather than assume, but likely affected source includes:

- `game/scripts/UnderwaterAtmosphere.cs`;
- one new environment clock/solar-state C# source;
- `game/shaders/water_surface.gdshader`;
- sky shader/material/resource source if used;
- focused atmosphere/day-night checks and `tools/dev` inspection wiring where appropriate.

Do not change authoritative fish movement/camera/collision/world-generation code unless a concrete integration blocker is discovered and explicitly reported.
