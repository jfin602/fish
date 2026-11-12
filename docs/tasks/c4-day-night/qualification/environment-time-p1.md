# C4 day/night P1 — environment time and solar state

**2026-11-12.** Direct implementation from clean `main` at `d3fe1f2757b0823bebaabe8d48f11d68cd57681f`; root version remains `0.4.11`. This is a working-tree candidate without a runner checkpoint commit. P2–P4 and visual/native day/night qualification remain open; P4 P12 remains NOT GREEN.

## Implemented

`EnvironmentClock` is one Main-owned gameplay-scene node. It advances from Godot's process delta, inherits scene-tree pause, and holds normalized day fraction and hour of day. Full-day seconds are configurable within 60–7200; the current 900-second default is a tunable starting value. No wall-clock or closed-game elapsed time is consulted. Time is not saved. `SolarState.At` continuously derives a normalized direction to the sun, elevation, daylight, twilight, night, direct-light and ambient-light scalars. It owns no renderer resources or fish/camera authority.

`UnderwaterAtmosphere` binds to that node and exposes its state for P2/P3 consumers. Existing sky, sun, water and underwater values are still rendered as before. The Pond→Ocean→Pond atmosphere fixture keeps the same clock when its presentation object is replaced. Debug set-time and 1–120× acceleration methods and launch arguments require a Debug build plus `--environment-time-debug`; no ordinary UI or persistence path was added.

## Executed T2 checks

On Godot 4.7.2 .NET / .NET SDK 10.0.403, `./tools/dev doctor`, `./tools/dev import`, and `./tools/dev build` passed. An early build had zero errors and two `NU1900` warnings because NuGet vulnerability metadata could not be fetched; the final Debug build had zero warnings and errors. `./tools/dev inspect-environment-time` passed both normal and explicitly enabled debug runs: 241 cycle samples, 06:00/12:00/18:00/00:00 anchors, smooth dawn/sunset/midnight boundaries, fraction wrap, duration bounds, debug bounds/isolation, paused process frames and resumed process frames. Both runs supplied the same time and speed launch arguments; the normal run kept the default state while the debug run applied them. `./tools/dev smoke-world` passed normal Ocean startup and asserted that the atmosphere reads Main's clock. `./tools/dev inspect-pond-camera-immersion` passed its pause/reentry and unchanged fish-state checks.

A separate `dotnet build Fish.csproj -c Release --nologo` passed with zero warnings and errors, checking the build with the debug-only hook methods excluded.

The first `./tools/dev inspect-pond-atmosphere` run failed because that special diagnostic initialized its own atmosphere before the normal startup path created the clock. The diagnostic now creates the same Main-owned clock before its Pond→Ocean→Pond sequence. After rebuilding, the rerun passed, including shared-clock binding through all three profile states, profile isolation, and unchanged fish state. The failed run is retained here as a repaired integration issue, not a passing observation.

Final `bash -n tools/dev` and `git diff --check` passed. Status contains only the C4 P1 source, check, command, and handoff/evidence files plus Godot UID sidecars; there is no package-lock or vendor payload. `package.json` still declares `0.4.11`. The new clock source contains no FishController, SwimMotor, camera, wall-clock, shader, or material access.

No sky, sun, water, turbidity or weather visuals were qualified by these headless checks. P2 owns dynamic sky and direct/ambient lighting; P3 owns water and underwater daylight response.
