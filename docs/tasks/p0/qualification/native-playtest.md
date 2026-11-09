# P0 native playtest — Linux desktop

Candidate: local `main` based on `13da1a2d187d75c0e505a66f632fd45c46136098`, with the uncommitted P0 working tree described in [implementation evidence](implementation.md). Engine: Godot 4.7.2 .NET; SDK: 10.0.403; MCP server/addon: 2026.11.04; display: X11 `:0`, AMD Radeon 880M, Mesa 25.2.8 Compatibility renderer. This was direct interactive work, not a runner checkpoint. The Fish editor was launched with `./tools/dev editor` and its task-owned play sessions were stopped cleanly.

| Scenario | Expected | Observed | Result |
| --- | --- | --- | --- |
| B01 | Import/build and C# assertions on actual main scene | Final `./tools/dev check` passed: 0 build warnings/errors, `FISH_FOUNDATION_READY`, `FISH_SMOKE_OK`; missing marker/error/timeout are wrapper failures. | PASS |
| B02 | CWD-safe CLI, saved-scene round trip and missing-scene failure | `inspect-scene` from `/tmp` returned `Main` with 5 children. GDScript helper saved/reloaded marker `(1.25, 2.5, -3.75)` then removed fixture. Missing path failed nonzero. | PASS |
| B03 | Correct live Fish peer, main scene and disposable editor save/reload | `godot_get_server_info` identified `/home/jfin/dev/fish/game/`, server/addon 2026.11.04, Godot 4.7.2, C# project present. MCP main-tree inspection found `Fish`, `Camera3D`, `Sun`, `Habitat` floor/walls/landmarks. MCP created `res://.automation-fixtures/mcp_round_trip.tscn`, set Marker position `(1.25, 2.5, -3.75)`, saved, reloaded, read back the same value, then closed and removed the file. Its serialized `transform` contained those values. | PASS |
| B04 | Real game, horizontal/vertical/release/bounds, state and pixels | MCP played `res://scenes/main.tscn`; live tree included `/root/MCPRuntimeProbe` and `/root/Main/Fish`. See measured states and captures below. Runtime stopped with `playing=false`; editor stayed usable. | PASS |

## State and rendered observations

The controlled action run began at fish position `(0, 0, 0)` with velocity `(0, 0, 0)`. Holding `swim_right` moved x to `2.20`; after release x settled at `2.67` and velocity was zero. Holding `swim_up` moved y to `2.07`; after release y settled at `2.53` and velocity was zero. Continuing right reached the explicit x bound `8.3`. The camera's global position there was `(8.3, 4.63, 6.0)`, behind and above the fish. The short post-release displacement is one or more physics frames of input-delivery latency; later samples stayed at the released axis value.

A separate key-bound run used the actual mapped `D` and `E` key events. `D` moved x from 0 to `2.07`; release settled at `2.53` with zero velocity. `E` reached y=`3.0`, the vertical cap; after release position remained at y=`3.0` with zero velocity. This confirms the documented keyboard mapping as well as direct action injection. Static floor/ceiling/walls were present in the live scene; these readings qualify the explicit prototype limits, not final swimming or collision feel.

MCP running-game screenshots were inspected: [initial view](native-game-start.png) and [after travel to the x bound](native-game.png). They show the orange fish-shaped primitive from the third-person camera, green landmarks, blue floor, surrounding walls and prototype control text. These are actual game frames, not editor viewport captures. The left wall renders very dark and the scene is deliberately plain; production art and water treatment remain later work.

The x-bound capture preceded the final C# change that added only the `FISH_FOUNDATION_READY` diagnostic print. The saved scene, input map, movement code and rendering were unchanged; the final composed check and initial-view capture followed that print addition.

The editor process printed `FISH_FOUNDATION_READY` and no error lines during the final live run. `godot_runtime_get_game_output` was also queried while playing and reported `connected=true`, `entries=[]`, `ready=false`, `reason=output_pending` even after the C# print; the editor process output is the observed runtime-output source. This MCP output-stream limitation is recorded rather than interpreted as proof that the ring captured all messages. The first immediate probe read after play returned `PRECONDITION_FAILED`; it became ready on a bounded retry. Earlier exploratory input observations drifted before controlled measurements; the input-map device/PageDown defect was repaired and the final no-input and key/action runs above were repeated. Original failures are retained in [implementation evidence](implementation.md).

Only this Linux desktop and the foundation behavior above are qualified. No final swimming quality, advanced water, first-person play, survival loop, export or other platform was tested.
