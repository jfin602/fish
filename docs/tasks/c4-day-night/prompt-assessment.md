# c4-day-night prompt assessment

## Requested outcome

Create the environmental foundation needed for realistic Fish lighting: a continuous day/night cycle first, with weather deliberately deferred to a later correction.

The immediate player-visible reason is the Pond surface. The new reflection work is now held back by the literal static/flat sky it reflects. Fixing the sky, sun and environmental daylight should improve the water naturally instead of adding more fake color inside the water shader.

## Preserve

- Root version `0.4.11`; P12 remains NOT GREEN.
- Current Pond dark muddy-green surface/transmission direction and near-surface fish readability.
- Existing Ocean Forward+ blue/turquoise water/light identity.
- FishController, SwimMotor, camera/control authority, collision and world generation.
- Existing pause semantics: environmental time must stop with gameplay.
- Spatial turbidity remains distinct from illumination.
- Current water/shaft/caustic performance-conscious presentation.
- Existing static overcast behavior only as compatibility where needed; do not turn it into a weather implementation.

## Architecture assessment

Current `UnderwaterAtmosphere` owns the Environment, fixed sun rotation/color/energy, surface material inputs, shafts and caustics. This is the correct consumer/integration layer but should not become the sole authoritative clock.

Introduce a small explicit environment-time/solar-state owner that can be queried by presentation and later gameplay systems. It should expose deterministic normalized time/day fraction plus derived solar direction/elevation/daylight values. Rendering then consumes the state.

Use continuous curves/interpolation across tuning anchors rather than hard state changes.

## Work order

1. Environment clock + solar-state mapping and debug set/accelerate hooks.
2. Dynamic/procedural sky plus direct/ambient light.
3. Water reflection/daylight and underwater shafts/caustics/ambient integration.
4. Native time sweep, pause, habitat switching and performance/visual review.

This order isolates state correctness before visual tuning.

## Risks

- Fighting ownership between fixed `UnderwaterAtmosphere` values and new solar values.
- Sky resources being rebuilt too often instead of updating bounded parameters.
- Reflection improving above water while underwater becomes too bright/dark.
- Night implemented as extra fog, destroying clarity semantics.
- Debug acceleration leaking into normal play.
- Pause mismatch causing sun movement while the game is stopped.
- Pond tuning leaking into Ocean after world switches.
- Introducing weather/cloud scope prematurely.
- Expensive dynamic sky/reflection techniques causing Forward+ regressions.

## Deferrals

Do not implement:

- weather/cloud simulation;
- rain/storm/fog weather events;
- wind-driven gameplay;
- ecology schedules;
- save/persistence of time-of-day;
- seasons, latitude, calendars or realistic astronomy;
- planar reflection cameras;
- multiple real shadow-casting celestial lights.

## Acceptance evidence

P1–P3 use T2 focused/integration checks for deterministic time mapping, pause behavior, sky/light resource integrity, habitat isolation and shader inputs.

P4 is T3 native/manual. Sweep representative times near 06:00, 09:00, 12:00, 17:00, sunset, twilight and 00:00, with above/below-water comparisons where practical. Record source revision, settings, actual observed colors/light/reflections, pause behavior, performance/errors and owner judgment.

A shader compile or screenshot at one time is not sufficient qualification.
