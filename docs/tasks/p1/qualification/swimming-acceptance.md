# P10 native swimming qualification — in progress

**Result: NOT ACCEPTED. Root version remains `0.1.9`.** The owner rejected the first sideways-facing fish and then the left-shoulder view. The current candidate centers the camera above the fish, lags slightly through turns, uses right mouse hold to slow, and adds camera keys and wheel zoom. The owner said “perfect this is looking good” before requesting wheel zoom; that is positive interim feedback, not explicit acceptance of the final candidate in both modes. Seven criteria in both modes, the four display/input cells and final acceptance remain incomplete. No G1 Gate is marked passed by this record.

## Candidate identity and platform

- Started from clean `main` revision `86b5e5ab5f9c5ac3ba5ca876c9d6331fccef894e`, package `0.1.9`, on 2026-11-09. Current P10 changes are uncommitted: Fish3 facing correction, centered elevated follow view with turn lag, right-button braking, mouse-mode camera keys with Shift hold, wheel zoom in either mode, matching scene probes and documentation. The final dirty-source fingerprint must be recorded after tuning stops.
- Actual selected art is Quaternius Animated Fish Pack `Fish3` / `ClownFish` (owner ZIP SHA-256 `ac4df41e480b2054f784c06384b71079d8f31298dea50346c262216bf78a4ad2`, working FBX `142bb6559b928c61ff839eecbba4ee0cfafa964df37720db6a6ba3a90b4ed639`), Rock Face 01, Coast Sand 01 and Smithsonian *Heliopora coerulea*. Owner decisions C30–C31 supersede the earlier Emperor Angelfish and Soft Coral Set prompt names. Original and working art remains in ignored local storage; [the starter manifest](../../../assets/p1-starter-manifest.md) owns terms and all source hashes.
- Ubuntu Linux/Cinnamon/X11 on one 1920 × 1200, 60 Hz eDP display; AMD Ryzen AI 9 465, Radeon 880M, Mesa 25.2.8 OpenGL 4.6 Compatibility. `./tools/dev doctor` reported Godot 4.7.2 .NET and SDK 10.0.403. Godot MCP server/addon 2026.11.04 connected to the exact Fish peer `/home/jfin/dev/fish/game/`, editor scene `res://scenes/main.tscn`; editor play is not being used as native display proof.
- `tools/dev run` was inspected: it builds before independent native launch and forwards only validated `--display-mode=...` user arguments. The first candidate used `XDG_DATA_HOME="$PWD/artifacts/p10/profile-fresh" ./tools/dev run --display-mode=fullscreen` (one successful build, zero warnings/errors). Repaired launches use the pinned `GODOT_CMD` from ignored `tools/local.env` with an isolated `artifacts/p10/profile-repaired` profile to avoid unchanged repeat builds. Native output reports `FISH_STARTER_WORLD_READY player=Fish3 ambient=7 rocks=2 coral=2 sand=1 course=movement_course`.

## Rejected attempt and repair

| Attempt | Native observation and owner feedback | Action/result |
| --- | --- | --- |
| P9 configuration, Fish3 visual Y 90°, centered camera | [Native rejected capture](captures/p10-rejected-sideways.png), SHA-256 `cb679bc58e8c74d59cd7c6f81a62e7cf472ced655764d9e11de6fca941b78fdc`: fish side profile points across the screen while motor travel and course are forward. The owner played and reported: “I played, its kin of unplayable. the fish is the wrong orientation. you have it sideways to the camera”. Play duration and other criteria were not supplied. | Imported Face-to-Tail rest axis was inspected in the real FBX; the fish nose lies along imported +Z after the armature correction. Set the visual's Y rotation to 180° so nose agrees with motor -Z. Added a real-actor facing assertion to `animation_scene_check.gd`. |
| Facing correction with centered camera | An intermediate local capture under ignored `artifacts/p10/` showed the correctly aligned fish edge-on from directly behind; still too narrow to read. | Added `CameraShoulderOffset` to the shared tuning resource. A +2.8 m offset put the camera near the course post and the fish's dark side; moved it to -2.8 m. This remains provisional pending owner play. |
| Facing correction with left-shoulder camera | [Native shoulder capture](captures/p10-repaired-left-shoulder.png), SHA-256 `ee8f956b18d5f54b96f879692278ab0b6c1f8e12eebc5df6a9deb0d1efc2acf7`: fish and route visible. The owner said the side view was distracting and requested a centered view slightly above and looking down. | Rejected the -2.8 m shoulder offset. Set it to 0 m, raised the camera to 2.0 m, shortened distance to 3.8 m and look-ahead to 0.8 m, and set follow response to 4.5 s⁻¹ for slight turn lag. |
| Centered camera and new input behavior | [Centered native capture](captures/p10-centered-above.png), SHA-256 `83be5e9255098dbfff0134f3b2adf8ca4a6966493caad841909ef353e761f083`. The owner asked for right-click braking, mouse-mode WASD camera pan with Shift hold, then said “perfect this is looking good” and requested scroll-wheel zoom. | Right hold now requests the common motor's S brake; middle click or Esc pauses. Mouse-mode W/S move camera elevation, A/D orbit; axes return when released unless Shift is held. Wheel zoom works in either input mode, 2.2–6.5 m at 0.4 m per notch. The camera offset clears on pause/mode change; zoom survives pause and resets on mode change. |

The native fullscreen window was `(0,0)` 1920 × 1200, borderless, `_NET_WM_STATE_FULLSCREEN`, with zero frame extents, and reported `requested=fullscreen source=launch-override effective=ExclusiveFullscreen`. That establishes this X11 full-display presentation, not exclusive monitor ownership. In a later native X11 event probe, XTest right button 3 left gameplay visible and middle button 2 opened the pause panel. A preceding `gnome-screenshot` had caused an apparent pause through focus loss, so the event probe used `ffmpeg` X11 capture without stealing focus. This proves the button routing in that native session, not full owner comfort. No completed four-cell input/display matrix, final frame timing sample, current-candidate missing-resource recheck or saved-choice/override recheck is claimed yet.

## Current focused checks

`./tools/dev check` on the centered camera and right-brake/Shift-pan candidate passed: import, .NET build with zero warnings/errors, motor/main-scene smoke, input lifecycle (`INPUT_LIFECYCLE_OK keyboard mouse right-slow middle-pause camera-pan shift-hold focus capture rearm preferences`), camera probe (`CAMERA_SCENE_OK turn_lag=0.253 blocked_z=1.820 recovery_z=3.786 viewport=(1280.0, 720.0)`) and animation probe. The first expanded input probe failed because it sampled the camera before the follow filter settled; the fixture was corrected to establish the baseline after settling. After adding wheel zoom, `./tools/dev build` passed with zero warnings/errors and the focused saved-scene input probe passed `INPUT_LIFECYCLE_OK keyboard mouse right-slow middle-pause camera-pan shift-hold wheel-zoom focus capture rearm preferences`. A wheel recovery assertion initially waited too briefly for the documented 3 s⁻¹ camera recovery response; it now waits 75 physics ticks and sends each synthetic wheel event once. The final composed check is still required after wheel work. Headless UI and input checks do not establish native feel or final owner acceptance.

## Seven owner-play criteria

| Accepted criterion | Keyboard fallback | Mouse-only |
| --- | --- | --- |
| 1. Quick start and predictable slowing | PENDING repaired candidate; earlier candidate rejected before criterion assessment. | PENDING. |
| 2. Circle rocks, turn around and pass gap | PENDING. | PENDING. |
| 3. Pitch to upper/lower landmarks and return level | PENDING. | PENDING. |
| 4. Readable fish, route, camera and body motion | Initial candidate REJECTED for sideways fish; left-shoulder camera also REJECTED. Centered/elevated candidate received positive interim feedback, but full criterion PENDING. | PENDING final candidate assessment. |
| 5. Pause/resume without stale input | PENDING. | PENDING. |
| 6. Complete same course comfortably | PENDING. | PENDING. |
| 7. Several minutes of natural, nimble, easy swimming | Initial owner play REJECTED; duration not recorded. Repaired candidate PENDING. | PENDING. |

## Display and input matrix

| Mode | Fullscreen | Windowed fullscreen |
| --- | --- | --- |
| Keyboard fallback | Native launch and captures on interim candidates observed; final wheel candidate, full course, pause, Alt-Tab, menu and owner judgment PENDING. | PENDING. |
| Mouse-only | PENDING. | PENDING. |

**Next action:** complete the final composed check and native wheel check, then record each criterion in both modes, the four native display/input cells, performance and V16/V17 rechecks. Record actual durations, final source/configuration fingerprints and explicit acceptance. Create `accepted-tuning.md` only for accepted final values; retain `0.1.9` until all P10 evidence passes. The owner-directed camera-key orbit is an explicit change to the earlier P10 prohibition on free look; only the mouse-mode, self-returning/Shift-held behavior described above is implemented.
