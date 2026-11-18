# P6 P8 — Native early Bass survival review

**Status: STOP / runtime repair required before owner qualification.** The first owner play entered the separate Pike Encounter fixture. The ordinary Pond retry loaded, but its live output then repeatedly threw a Bluegill cover-selection exception. V44–V47 are not qualified. This sheet does not record a Gate PASS, a manual checkpoint, or permission to continue to P9.

## Session identity and preflight

| Field | Value at preparation |
| --- | --- |
| Branch / source revision | `main` / `4037f765fd412bf8c29c414257830ddfc989aa08` |
| Working tree | Dirty at entry: P6 P7 frog/snake source, checks, evidence and `0.6.7` metadata; existing Godot MCP addon, camera, water, sound, lighting and art work preserved. See `git status --short --branch` at review time for the exact path list. |
| Root version / proposed manual version | `0.6.7` / `0.6.8` only after genuine owner completion and authorization |
| World Select route | **Freshwater Pond** → **Bass** → hatch; not the third Pike Encounter fixture |
| Pinned Base World | `res://worlds/base/pond-p4-71`, manifest `world-5e3b4fee759acbe3`, seed `71`, content SHA-256 `d76eabbb5b6fff2f91418b49b2de92dd090541d609b46d0725ad9fc85cbda347`; generator `pond-full-1` |
| Player and other species | Bass `FAF-FISH-007`; Bluegill `FAF-FISH-006`; Northern leopard frog `FAF-CREATURE-001`; Northern water snake `FAF-CREATURE-002` |
| Expected stable actor IDs | Bass life `pond-world-5e3b4fee759acbe3:bass-life:<new GUID>`; adult Bluegill includes `world-5e3b4fee759acbe3:bluegill:adult:6`; frog `world-5e3b4fee759acbe3:frog:shore:0`; snake `world-5e3b4fee759acbe3:snake:shore:0`. Record actual IDs from the native session when available. |
| Fish editor bridge | Fresh task-owned Fish MCP server on `ws://127.0.0.1:19080`, live editor `/home/jfin/dev/fish/game/`, Godot `4.7.2` .NET, matching server/addon `2026.11.04b2`, C# project supported; verified before this sheet was written. |
| Desktop / renderer / lighting | Linux/X11, AMD Radeon 880M, Vulkan Forward+; Pond's current solar lighting and moving waterline. Record the actual in-game hour/lighting and any changes during play. |
| Native launch | `./tools/dev run --display-mode=windowed-fullscreen`; record exact build/launch result below and leave the game window open for owner control. |
| Control setup | Record the active preset. Keyboard Only: W thrust, S slow, A/D yaw, Up/Down pitch; arrows and Shift camera look. Mouse and Hybrid presets use left mouse thrust, right mouse slow, wheel zoom, middle mouse or Esc pause. Hybrid 2 uses mouse orbit with W/S pitch and A/D yaw. Adaptive Follow uses mouse steering and temporary independent look while holding Left Shift. Space boosts. The pause menu can change presets. |

## Preflight and launch record

| Action | Exact observed result |
| --- | --- |
| Smallest relevant current preflight | `./tools/dev doctor` exited 0: Godot `4.7.2.stable.mono`, .NET SDK `10.0.403`, Fish project files ready, X11 display `:0`, addon `2026.11.04b2`. `./tools/dev build` exited 0 with 0 warnings and 0 errors. `./tools/dev inspect-p6-pond-entry` exited 0 with `P6_POND_ENTRY_OK`, world `world-5e3b4fee759acbe3`, seed `71`, 23 food, 8 prey and 9 remaining ambient actors. Its scripted feeding/growth/retry trace is not owner play. |
| Fish .NET / Forward+ desktop launch | `./tools/dev run --display-mode=windowed-fullscreen` built with 0 warnings and 0 errors and remains running. Godot `4.7.2.stable.mono` reported Vulkan Forward+ on AMD Radeon 880M and `FISH_WORLD_SELECT_READY entries=3 habitats=2`; display mode reported `effective=Fullscreen`. This proves a native window opened, not P8 play quality. |
| Live bridge recheck after launch | `godot_health_check` on the fresh Fish MCP session returned `bridge_connected=true`, `editor_status=ready`, no bind/version error, matching server/addon `2026.11.04b2`, Godot `4.7.2-stable`. `godot_get_server_info` before the sheet identified the live editor peer as `/home/jfin/dev/fish/game/` with C# project support. |
| Owner retry launch | The first native game exited after the Pike Encounter session. The Fish editor bridge remained connected; `./tools/dev run --display-mode=windowed-fullscreen` reopened World Select with a 0-warning, 0-error build and Vulkan Forward+ on AMD Radeon 880M. |
| Actual native session life ID / initial game hour | The retry log confirms ordinary Pond `P6_POND_BASS_READY` and hatch of life `pond-world-5e3b4fee759acbe3:bass-life:b8b29762a5fa4d459577c9c06ea04b01`, seed `71`, 23 food and 8 prey. Initial game hour was not captured. |
| Runtime blocker | Repeated native `System.ArgumentException` in `PreyActor.ChooseCover` at `game/scripts/PreyActor.cs:183`, called by `PondBassSession._PhysicsProcess` at line 550 after hatch. One observed invalid height interval was lower `10.301243` > upper `10.010851`; another upper was `10.0895195`. The `Mathf.Clamp` bounds are `bed + 0.32 m` and `water - 0.35 m`, which invert in water shallower than `0.67 m`. The headless entry check did not expose this active flee/cover route. The repeated exception prevents a reliable early-survival review. |

## Owner play matrix — V44–V47

Write the action and what appeared or happened in **Observed**. Enter one of `PASS`, `FAIL`, or `NOT OBSERVED` in **Disposition** only after owner review. Leave both cells blank until attempted. The prior scripted T2 checks and isolated rig captures are supplementary evidence only.

| Scenario and expected action | Observed owner action and result | Disposition |
| --- | --- | --- |
| **V44 hatch and aim:** Select ordinary Pond Bass, watch or skip hatch, acquire controllable larva, aim the reticle at visible eligible tiny food, feed through real forward mouth contact and see one restrained confirmation. Record aligned misses and any false guidance. |  |  |
| **V44 view controls:** Compare centered follow with camera lag, zoom, Hybrid 2 independent orbit and Adaptive Follow Shift look; cross the moving surface and, if practical, compare fry/juvenile body framing and night or low-light visibility. Feeding eligibility must follow the fish mouth, not camera center. |  |  |
| **V45 food route:** Consume nearby nursery food, then travel to multiple reed, shore/root/wood and vegetation patches. Note whether targets are discoverable and reachable, whether exhausted sites stay empty, and whether a new route sustains growth without circling the nest. |  |  |
| **V46 protected route:** Feed or traverse near real foliage/solid shelter, with an adult Bluegill in the area. Record whether cover and lower movement reduce detection and whether a visible escape route exists. |  |  |
| **V46 exposed route:** Swim or boost across an exposed shallow approach; record adult Bluegill notice, pursuit, strike, escape or catch, the fish stage, predator ID if available, and visible cause. Avoid claiming a cause from a caught screen alone. |  |  |
| **V47 frog:** Observe the registered frog at bank/surface/game distance, its locomotion, notice and finite attack or retreat. A tiny fry should be at risk only within its shore and surface envelope; record collision, cover and visual/audio readability. | Owner: “i didnt see any frogs or snakes”. The inspected native log places this play in Pike Encounter, which does not contain the normal-Pond frog actor; ordinary Pond frog visibility is not observed. | NOT OBSERVED |
| **V47 snake:** Observe the registered snake crawl, surface swim or dive near reeds and shallow water; test a sensed approach and actual cover/escape or catch. Record line of sight, collision, fair warning and any near-camera obstruction. | Same owner report and fixture route; ordinary Pond snake visibility is not observed. | NOT OBSERVED |
| **Return/retry:** On catch or voluntary return, use the actual menu and start a new Bass life. Record whether the next identity and stock reset and whether any pursuit or caught state leaks. |  |  |
| **Performance and presentation:** During sustained free swim and threat moments, record actual frame times or FPS source, plus audio, water/lighting, body-scale, camera and collider defects. Attach screenshots only if actually captured. |  |  |

## Owner feedback and decision

- Exact owner feedback: “i didnt see any frogs or snakes” (2026-11-17). Native output from the still-running game shows `PIKE_ENCOUNTER_READY ... world=fixture ... seed=5266`, Bass hatch/feed/growth, `PIKE_ENCOUNTER_OUTCOME outcome=caught seconds=333.08`, then return to World Select. It does not show the normal Pond `P6_POND_BASS_READY` marker. The fixture's lack of P6 frog/snake actors explains this session; it is not evidence that the normal Pond actors failed to spawn or render.
- Predator cause(s) observed by the owner, with stage and IDs if known: 
- Screenshots / frame-time captures (path, timestamp and method): 
- Essential-loop blocker and targeted repair plan, if any: Repair `PreyActor` water-column goal selection so shallow plant centers and fallback flee points cannot call `Mathf.Clamp` with reversed height bounds. Skip invalid cover sites or choose a physically valid nearby water-column point, while retaining solid collision and flee behavior; check the same bounds in its normal forage path. Add a seed-71 active Pond check that exercises Bluegill flee/cover near shallow shore, then rebuild and repeat native Forward+ play with no exception before asking the owner to judge frog/snake discovery, warnings and escape. Do not infer that this exception caused the owner's earlier no-sighting report, which came from the Pike fixture.
- Owner decision (`proceed`, `repair`, or `not yet reviewed`): STOP for the observed runtime repair. Owner visual verdict in the ordinary Pond is still not recorded; P9 progression is not authorized.
- Manual `0.6.8` checkpoint and runner resume authorization: 

Full G6.8/V51 whole-life qualification remains unobserved here. P2's earlier larval reticle failure and deferred native retry remain distinct until an actual owner result addresses them.
