# c4-day-night — shared day/night environment correction

**State:** **AUTHORED / NOT EXECUTED.** **Type:** Correction 4. **Required root version:** `0.4.11` unchanged.

## Purpose

Replace Fish's effectively static sky/sun lighting with a shared continuous in-game day/night cycle before any weather system is added.

The correction should establish one environmental time source and one solar state consumed by:

- the procedural/dynamic sky;
- the real DirectionalLight3D;
- ambient/environment lighting;
- Pond/Ocean water reflections and surface daylight;
- underwater shafts, caustics and readable daylight response.

Pond is the first visual target because its improved reflective water currently exposes the flat/static sky as a bottleneck. The architecture must remain shared/configurable so Ocean consumes the same solar state without losing its accepted blue/turquoise presentation.

**Weather is explicitly deferred.** Existing overcast/static tuning may remain as compatibility behavior, but cloud systems, rain/storms, weather fog, wind and weather-driven water changes are not part of this stack.

[Day/night contract](../../systems/day-night-and-weather.md) · [P4 design](../../planning/p4/freshwater-world.md) · [Underwater visibility](../../systems/underwater-visibility.md) · [P12 evidence](../p4/qualification/native-surface.md)

## Source boundary at authoring

At authoring, `UnderwaterAtmosphere` creates the Forward+ Environment, sets a color background, configures a fixed DirectionalLight3D rotation/color/energy, and owns surface shader inputs, shafts, caustics and water profiles. The current water shader already consumes sun direction/daylight-style parameters, so day/night should centralize those inputs rather than layering another unrelated lighting system on top.

Normal gameplay pause already exists through the PlayShell path. The new environment clock must follow the actual gameplay pause contract rather than advancing from wall-clock time.

## Prompt order

| Step | Prompt | Outcome | Tier / handoff |
| --- | --- | --- | --- |
| P1 | [Environment clock and solar state](P1-environment-clock-and-solar-state.txt) | One bounded pause-aware environment-time source and continuous solar-state mapping with deterministic debug set/accelerate hooks | T2 |
| P2 | [Procedural sky and direct lighting](P2-procedural-sky-and-lighting.txt) | Dynamic sky/horizon plus real sun/ambient lighting driven from the shared solar state | T2 |
| P3 | [Water and underwater daylight integration](P3-water-and-underwater-daylight.txt) | Reflection/daylight/shafts/caustics consume the same solar state with Pond/Ocean profile isolation and bounded cost | T2 |
| P4 | [Native day/night closeout](P4-native-day-night-closeout.txt) | Real Forward+ sweep through representative times, pause/profile/performance checks and owner visual decision | T3 manual closeout |

P4 is the sole final closeout. Its canonical `Browser required: yes.` metadata means the runner stops for native/manual observation.

## Acceptance boundary

The correction is successful only when the native scene reads as one continuous environment through the day:

- moving sun direction and shadow/highlight direction;
- non-flat sky from dawn through night;
- smooth color/energy transitions without visible preset jumps;
- Pond water reflecting the actual changing sky/light rather than a fixed gray source;
- underwater daylight, shafts and caustics coherently responding to solar state;
- night genuinely darker without faking darkness by increasing turbidity;
- pause freezing environment time;
- Pond/Ocean preserving separate optical profiles;
- weather still absent from the implementation.

Exact full-day duration remains configurable and is not permanently locked by authoring. Debug time acceleration/set-time exists only to make qualification practical.

## Execute

From a reviewed checkout at root `0.4.11`:

```bash
npm run fish:phase:validate -- c4-day-night
npm run fish:phase -- c4-day-night
```

Writing this stack does not execute it. Correction prompts keep `0.4.11` unchanged, do not self-commit or push, and must preserve P12's truthful NOT GREEN evidence until later owner qualification.
