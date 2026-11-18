# P6 P8 — Native early Bass survival review

**Status: P8 OWNER GREEN FOR PROGRESSION at `0.6.8`; V44–V47 retain the observation limits below.** Earlier native runs exposed Bluegill and Pike shallow-water exceptions; repairs are recorded below. The owner then found a rock that admitted Bass and contained edible plankton. The current candidate adds physical stone collision and a focused food-clearance check. The owner explicitly approved the P8 checkpoint after reviewing this candidate. This approval does not supply missing per-scenario play observations or qualify the later whole-life G6.8/V51 gate.

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

## 2026-11-18 owner correction and Pond replacement candidate

The owner rejected the split between the old ordinary Pond and Pike Encounter: “Theres alot of issues. The water effects from the pike encounter didnt carry over. The initial cut scene is skewed and not inframe. the starter world isnt the same. the frogs are swimming backwards. the school of bluegill just sit there frozen in time until you get near them. We should take the exact pike encounter world and include the new creatures there. then just completely remove the pond world. this would become the pond world. and we'd have only 2 selection on world select. no need for the character select screen either”. For Ocean, the owner chose “Keep current Ocean default”. This supersedes the earlier third-encounter menu and old seed-71 Pond production route. It does not qualify V44–V47 or authorize P9.

| Field | Current candidate / observation |
| --- | --- |
| Source at entry | `main`, HEAD `a6f4d7d2a3967a4f18229720910fb2e225895ec5`, clean tree, root `0.6.7` |
| Editing state | Dirty with targeted source, route-check and evidence changes; no commit, push or `0.6.8` version bump |
| Live editor bridge | Fish Godot .NET editor project `/home/jfin/dev/fish/game/`, bridge `ws://127.0.0.1:19080`, server/addon `2026.11.04b2`, `bridge_connected=true`, `editor_status=ready` at preflight and before native launch |
| Playable world identity | Pond card launches the former Pike scene and its exact seed-5266 package `res://worlds/encounters/pike-m1-5266`: `world-6c98f9e946b4ede8`, content `f056c3216989617bb02565ee98a2bbf41d2a72373d11dc0c6a09703e417cd072`, manifest `c042b0a7cc847c553f88fbdf75f1a6f19ad6293aadd7b497167567f143814a9b`. The old seed-71 package was removed from the repository working tree and active catalog/menu path. Historical seed-71 probes remain source records but require their retired package to reproduce. |
| Species and actor identity | Bass `FAF-FISH-007`, Bluegill `FAF-FISH-006`, Pike `FAF-FISH-010`, frog `FAF-CREATURE-001`, snake `FAF-CREATURE-002`. Frog `world-6c98f9e946b4ede8:frog:shore:0`; snake `world-6c98f9e946b4ede8:snake:shore:0`. New Bass retry generates a distinct `pond-world-6c98f9e946b4ede8:bass-life:<GUID>` identity. |
| Renderer / controls | Native Godot `4.7.2.stable.mono`, Vulkan Forward+, AMD Radeon 880M. World Select offers Ocean and Pond. Pond starts Bass directly; Ocean starts the former default Yellow Tang `FAF-FISH-001` directly. The owner's actual control preset in the game was not captured. |
| Focused build and route check | `./tools/dev build`: 0 warnings, 0 errors. `./tools/dev inspect-world-select`: exit 0, `WORLD_SELECT_CHECK_OK entries=2 pond=world-6c98f9e946b4ede8 seed=5266 frog=...:frog:shore:0 snake=...:snake:shore:0 ocean_default=FAF-FISH-001`. This is a scripted T2 route check, not owner play. |
| Live editor scene | Godot MCP editor play of `res://scenes/p5_pike_encounter.tscn` showed `PondLeopardFrog`, `PondNorthernWaterSnake`, 16 Bluegill and 23 distributed food actors in the live tree. One captured viewport showed the in-game frog model near the bank and water effects present; this was an editor probe, not a saved screenshot or owner acceptance. A sampled adult Bluegill had nonzero velocity `(−0.155, 0.307, −1.306)` m/s, and a sampled juvenile had nonzero velocity `(0.557, 0.091, −0.379)` m/s. These samples do not establish sustained schooling quality. |
| Native launch | `./tools/dev run --display-mode=windowed-fullscreen` built with 0 warnings/errors and opened a Forward+ fullscreen window. Output reported `FISH_WORLD_SELECT_READY entries=2 habitats=2`, Pond seed 5266, hatch life `pond-world-6c98f9e946b4ede8:bass-life:b96ab3528f7c42cebc865eeee34f4334`, and one committed nursery zooplankton feed event. The game window remains open. The log does not identify who controlled those actions, so they are not counted as owner review. |

### New owner review cells

| Expected action in the replacement Pond | Owner observed | PASS / FAIL / NOT OBSERVED |
| --- | --- | --- |
| Choose Pond, watch hatch framing, and compare water and starter world with the former Pike scene |  |  |
| Aim and eat near the nest, then find and deplete food at reeds, roots, vegetation and shore patches |  |  |
| Observe moving Bluegill schools while far away, then test adult notice, pursuit and cover/escape |  |  |
| Watch frog swim with its head leading; observe real surface/shore movement and sensed behavior |  |  |
| Watch snake shore crawl/swim/dive and a fair warning, escape or contact |  |  |
| Retry or return; compare stage framing, zoom, independent look, night and surface visibility |  |  |
| Record actual frame times, audio/visual/collider issues and saved screenshots if captured |  |  |

**Disposition:** replacement candidate built and launched; owner judgment of the replacement Pond remains pending. The prior seed-71 native failure remains historical. `0.6.8`, G6.1–G6.8 and V44–V51 remain unqualified; the runner must stay stopped before P9.

### Native shallow-water repair during replacement review

The first replacement native run progressed through hatch, three nursery feeds and one reeds-west feed, then repeatedly threw `System.ArgumentException` from `PikeActor.Tick` line 108 at game time about 114 s. One invalid height interval had lower `9.414572` > upper `9.39`: the Pike's 1.5 m bed clearance cannot fit under its 1.0 m surface clearance at a shallow pursuit goal. That runtime failure made the first candidate unplayable. The game window was stopped to prevent continued error output.

`PikeActor` now steers toward its deep patrol origin when a requested chase or aftermath goal lacks room for its body; the movement remains physical, with no direct teleport. A new focused shallow-goal assertion in `PikeAiChecks` passes, and `./tools/dev inspect-pike-ai` exits 0 with `PIKE_AI_CHECK_OK`. `./tools/dev inspect-bluegill-schools` also exits 0 with `BLUEGILL_SCHOOL_CHECK_OK`. `./tools/dev inspect-world-catalog` exits 0 for the seed-5266 Pond and unchanged seed-42 Ocean. These checks do not establish sustained native fairness.

After a zero-warning, zero-error rebuild, `./tools/dev run --display-mode=windowed-fullscreen` relaunched the Forward+ window; output again reported two world choices, seed 5266 and hatch identity `pond-world-6c98f9e946b4ede8:bass-life:31346a00b2a5492383608c77b90391a1`. The window remains open. A full repeated owner route and frame-time sample are still unobserved.

### Owner rock and food collision report, 2026-11-18

Exact owner feedback: “Its looking good. check collisions for rocks and other solid objects. I found some plankton inside a rock and was able to swim inside it and eat it”. This is an owner-observed **FAIL** for the solid-rock boundary and food accessibility in the replacement Pond. The exact rock instance, food actor ID and coordinates were not captured; no claim is made about a particular instance.

The source audit found that ordinary Pond wood and eroded banks use displayed-mesh static collision, as do the Pike scene's authored wood and shore sticks. The 100 batched algae-stone and pebble placements had visuals but no physics bodies. `PondFoodPatches.FindPosition` rejects occupied points by querying physics layer 1, so it could not detect those visible rocks. The replacement candidate now installs a static collision body for each of those 100 displayed placements on that same layer before food is populated. The existing mouth-to-food line-of-sight check remains authoritative for consumption.

`./tools/dev build` exited 0 with 0 warnings and 0 errors. `./tools/dev inspect-world-select` exited 0 with `POND_SOLID_FOOD_CHECK_OK solids=100 food=23` and `WORLD_SELECT_CHECK_OK entries=2 pond=world-6c98f9e946b4ede8 seed=5266`. The focused check confirms every stone/pebble placement has a physical shape, ray hits on representative stone variants resolve to those bodies, and all 23 initial food actors are clear of solid geometry. It is a scripted physics regression, not an owner PASS. `./tools/dev run --display-mode=windowed-fullscreen` rebuilt with 0 warnings/errors and opened the native Vulkan Forward+ Pond window on AMD Radeon 880M; it reported hatch life `pond-world-6c98f9e946b4ede8:bass-life:975f257454ae4076a74da81301c4bd19` and `BASS_HATCH_COMPLETE`. The window was running at the latest check. Actual owner collision retry, exact frame times and visual confirmation remain unobserved.

| Repair review action | Owner observed | PASS / FAIL / NOT OBSERVED |
| --- | --- | --- |
| Revisit the rock or other algae-covered stone, press Bass against several sides, and confirm the body cannot enter it |  |  |
| Inspect nearby plankton and verify each target is visible, outside the stone and reachable by a clear mouth path |  |  |
| Check authored logs, sticks and eroded bank solids for unexpected gaps while feeding nearby |  |  |

**Decision before the owner's checkpoint reply:** repair candidate ready for owner native retry; the reported defect stays FAIL until that retry. P8 and the `0.6.8` manual checkpoint remained on hold, and the runner stayed stopped before P9.

### Owner P8 checkpoint decision — 2026-11-18

The owner responded to the repair candidate: **“mark p8 green. commit like the runner would”**. This explicitly approves the P8 manual checkpoint and progression to the next implementation task. It supersedes the preceding hold decision **for P8 progression**. The owner did not provide a new rock retest result, a detailed V44–V47 route matrix, predator-cause account, frame-time sample, or screenshot with this decision. The earlier observed rock penetration remains a recorded failure on the prior candidate; the physical-collision repair has focused T2 evidence, and its owner-native result is **NOT OBSERVED**. Frog/snake discovery, cautious and exposed Bluegill routes, and detailed reticle/camera judgments are also not promoted to scenario PASS by the checkpoint approval.

The approved candidate uses the exact seed-5266 former Pike environment as ordinary Pond, with two World Select entries and direct Bass/Tang launch. The live Fish Godot .NET editor bridge was connected and ready to `/home/jfin/dev/fish/game/` before checkpoint work. The current source boundary is `main` at parent `a6f4d7d2a3967a4f18229720910fb2e225895ec5`, with the P8 replacement and repair staged for the direct `0.6.8` checkpoint. The runner stays stopped during this manual checkpoint; P9 work begins only through a later explicit execution or runner resume. Full G6.8/V51 remains unobserved.

Checkpoint preflight: `./tools/dev build` exited 0 with 0 warnings and 0 errors. `./tools/dev inspect-world-select` exited 0 with `POND_SOLID_FOOD_CHECK_OK solids=100 food=23` and `WORLD_SELECT_CHECK_OK entries=2 pond=world-6c98f9e946b4ede8 seed=5266 frog=world-6c98f9e946b4ede8:frog:shore:0 snake=world-6c98f9e946b4ede8:snake:shore:0 ocean_default=FAF-FISH-001`. The first `npm run fish:phase:validate -- p6` exited 1 because the historical P5 prompt had no parser-recognized assigned-version line; its two informal version lines were replaced with the equivalent assigned `0.6.5` metadata. The second validation exited 0 with all 13 prompts and P8 mapped to `0.6.8`. `git diff --check` and `git diff --cached --check` exited 0. The last native game process was no longer running at checkpoint preflight; the Fish editor and MCP bridge remained connected. No new native play or frame-time result is claimed from these commands.

Checkpoint history limit: P6 frog and P7 snake implementation/evidence are present in parent `a6f4d7d` (`clean`), but reachable Git history has no exact `0.6.6` or `0.6.7` commit subjects. The runner's contiguous-prefix rule will reject the later `0.6.8` marker while those earlier subjects are absent. This manual P8 commit is owner-authorized and uses the assigned version subject; it does not fabricate P6/P7 runner checkpoints or authorize an automatic P9 run. Reconcile that history deliberately before attempting runner resume.
