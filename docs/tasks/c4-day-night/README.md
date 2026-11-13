# c4-day-night — shared day/night environment correction

**State:** **P1–P3 directly implemented and T2 checked; P4 native candidate NOT GREEN.** Native captures and performance checks are recorded. The owner accepted the solar-fog repair for the pale sunset shore. A [night readability and Pond shore alignment candidate](qualification/night-readability-candidate.md) is implemented and captured, but the whole cycle has no owner acceptance. **Type:** Correction 4. Its authored prompts required `0.4.11` unchanged; the [P4 direct closeout](../p4/closeout.md) deliberately establishes `0.5.0` without passing c4. [P1 evidence](qualification/environment-time-p1.md) · [P2 evidence](qualification/solar-lighting-p2.md) · [P3 evidence](qualification/solar-water-p3.md) · [P4 native evidence](qualification/native-day-night-p4.md).

The [focused night optics repair candidate](qualification/night-optics-repair/README.md) records the later moonlit sky, dark water, underside stars/moon, matched native captures and remaining review limit. It does not alter the P4 NOT GREEN status.

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
| P1 | [Environment clock and solar state](P1-environment-clock-and-solar-state.txt) | Directly implemented: one bounded pause-aware environment-time source and continuous solar-state mapping with deterministic debug set/accelerate hooks | T2 passed; [evidence](qualification/environment-time-p1.md) |
| P2 | [Procedural sky and direct lighting](P2-procedural-sky-and-lighting.txt) | Directly implemented: dynamic sky/horizon plus real sun/ambient lighting driven from the shared solar state | T2 passed; [evidence](qualification/solar-lighting-p2.md) |
| P3 | [Water and underwater daylight integration](P3-water-and-underwater-daylight.txt) | Directly implemented: water reflection/daylight and underwater light/shafts/caustics consume the shared solar state with Pond/Ocean profile isolation | T2 passed; [evidence](qualification/solar-water-p3.md) |
| P4 | [Native day/night closeout](P4-native-day-night-closeout.txt) | Native Forward+ fixed-time captures, accelerated sweep, pause/profile/performance checks recorded; night readability and owner decision remain open | T3 [evidence](qualification/native-day-night-p4.md), NOT GREEN |

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
