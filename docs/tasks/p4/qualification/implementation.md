# P4 P1 — World definitions and baked loader implementation

**Source before task:** `main` at `853de9c86b9798a0ce0c0ff36b43420f0d225e19`, root `0.4.0`, 2026-11-11. **Result:** implementation in the uncommitted working tree, root `0.4.1`; runner owns the checkpoint. P3 direct closeout explicitly authorized progression with G3/V25–V27 gaps retained. No P4 Gate is qualified by this source check.

## Contract and changed files

- `game/scripts/WorldCatalog.cs` defines stable `ocean` and `pond` IDs, typed marine/freshwater habitat, environment profile and FAF catalog routes, preview description/accent, spawn route metadata, actual Ocean habitat scene resource, and selected baked identity. The selected Pond manifest identity is `pond:not-installed`; its scene and bake are absent until P4. No Pond bake or generated geometry is claimed.
- Ocean selects `res://worlds/base/ocean-m4-42`, manifest ID `world-6cba9a67e6275297`, seed 42, settings 1, generator `c3-full-12`, catalog SHA-256 `d7f9e29007443eb53b09a780028f2176f23fcc01774fbec178cce80d1581cf9a`, content SHA-256 `ba397da2b4ff2d2982898a3c3519b0e5cd0443222e9d31650197d8bcb10e3879`, and manifest-file SHA-256 `b21b4dd4517b4f66a732bf8834218223912bca5886ffc5c6a511c7187f107865`. The package files were not edited.
- `game/scripts/FullWorldPackage.cs` exposes manifest reading for the catalog preflight. `game/scripts/P2HabitatWorld.cs` routes normal entry through the selected world and retains the explicit `--world-package=` diagnostic candidate path. `game/scripts/Main.cs` aborts a failed world entry with an actionable error and clears active references on unload. `WorldSession` holds selected world separately from `DemoSelection.CharacterId` and clears loaded habitat/manifest references on unload. No persistence or gameplay simulation state was added.
- `game/dev/WorldCatalogChecks.cs` and its Godot UID sidecar provide focused checks; `tools/dev inspect-world-catalog` invokes them. `package.json`, `BOOT.md`, `MODULES.md`, `docs/roadmap.md`, and the P4 README reflect the implemented P1 boundary. No source scene, Ocean Base World resource, lockfile, or renderer setting changed.

## Checks and observations

On the selected Fish project (`/home/jfin/dev/fish/game`), `tools/dev doctor` reported Godot 4.7.2 .NET, SDK 10.0.403, and Fish's local bridge setting. The Godot Editor MCP server reported `2026.11.04` but no connected editor; no editor mutation or native visual observation was made.

| Check | Result |
| --- | --- |
| `./tools/dev build` | PASS, zero warnings/errors. |
| `./tools/dev inspect-world-catalog` | PASS. Accepted Ocean loaded twice into fresh habitat instances with identical world/content identity, 875/803 ms then 874/796 ms on rerun, zero generator invocations; session unload cleared cached references. Rejected unknown ID, Pond without bake, incomplete package, changed/corrupt manifest, and version mismatch. |
| `./tools/dev smoke-world` | PASS. Normal Ocean `WORLD_BAKED_LOAD_OK` for the exact accepted ID/content, `generator_invocations=0`; Forward+ atmosphere and C# smoke markers present. |
| `./tools/dev inspect-character-select` | PASS. Five existing marine FAF choices loaded the accepted Ocean; select/play/return/reselect route passed. |
| Headless `--world-id=pond --smoke` and `--world-id=bogus --smoke` | Expected exit 1. Diagnostics were `WORLD_BAKE_NOT_INSTALLED world=pond action=complete-p4-pond-bake` and `WORLD_ID_UNKNOWN id=bogus action=choose-ocean-or-pond`; neither loaded an Ocean fallback. |
| `git diff --check` | PASS. |

The pre-existing dirty tree included Pond creature source/assets/checks and a Python bytecode change. Those files were preserved and were outside this P1 implementation; their integration requirements remain for later P4 work. The editor bridge was offline, and headless checks do not establish native image fidelity, fish feel, or Pond quality. P2 adds World Select navigation; P4 creates and qualifies the Pond package before it can load.

## P2 — World Select and navigation (2026-11-11)

**Pre-task source:** `main` at `7942c431ae5fa9c61d24871638154795ec34e780`, clean tree, root `0.4.1`. **Result:** uncommitted P2 changes at root `0.4.2`; the phase runner owns staging and the checkpoint commit. During execution, separate Pond creature assets, documentation and generator checks appeared in the shared working tree. They were not edited for P2 and must be reviewed separately before runner staging.

`project.godot` now starts `world_select.tscn`. `WorldSelect.cs` renders distinct Ocean and Freshwater Pond cards with names, catalog descriptions, honest pending-preview text, a written selected marker, focus borders, keyboard/pointer browsing, Choose, Back/Quit demo, loading text and visible load errors. It sizes to the viewport and switches to one column below 900 px. The existing `WorldSession` holds the requested world even when Pond preflight rejects its absent bake; no Ocean fallback or world scene is loaded. A successful Ocean preflight enters the five-fish Character Select, whose Back/Escape returns to World Select. Pause → Return to World Select clears/disarms fish input, suspends water reaction, restores the pointer, releases pause, unloads the previous play scene and clears session references. No persistence, death, archive or Main Menu action was added. The P3 Ocean motor, collider and baked package were not changed.

| Check | Result |
| --- | --- |
| `tools/dev doctor` | **PASS:** Fish project at `/home/jfin/dev/fish/game`, Godot 4.7.2 .NET, SDK 10.0.403, MCP addon 2026.11.04. MCP bridge reported disconnected. |
| `tools/dev import` | **BLOCKED** by sandbox socket permission when Godot's editor IDE server and MCP addon attempted listeners; it also could not write user editor settings. This is an environment failure, not an import PASS. |
| `XDG_DATA_HOME=/tmp/fish-p4-p2-inspect tools/dev inspect-scene res://scenes/world_select.tscn` | **PASS:** `SCENE_OK` for the actual World Select packed scene. The ordinary inspect invocation first failed because the default user log path was unwritable; the isolated writable data directory resolved it. |
| `tools/dev build` | **PASS:** zero warnings and errors after the final source edits. |
| `tools/dev inspect-world-select` | **PASS:** `WORLD_SELECT_CHECK_OK`; verified written selection and focus via keyboard/pointer, Pond's `WORLD_BAKE_NOT_INSTALLED` error and retained `pond` session ID, invalid ID rejection, Ocean → Character Select → Back → Ocean, Ocean → Play → focus loss/held-input clear → Return, scene/habitat cleanup, repeat Ocean with a different fish, and World Select Back/Quit action. Both Ocean loads reported `world-6cba9a67e6275297` and `generator_invocations=0`. |
| `tools/dev inspect-character-select` and `tools/dev smoke-world` | **PASS:** all five existing FAF fish loaded the accepted Ocean route; return/reselect and normal main scene smoke passed. |
| Existing `input_lifecycle_check.gd` | Initial **FAIL** was an origin-relative position assertion incompatible with the baked spawn; changed the assertion to compare against the observed starting position. Rerun **PASS**: `INPUT_LIFECYCLE_OK`, including focus loss, capture and rearm. |
| Headless windowed-fullscreen World Select route | **PASS** for startup/routing under `--display-mode=windowed-fullscreen`; logs show the requested mode through World Select, Character Select and Play. Headless reported the effective window as minimized, so visual sizing in either native display mode is **NOT OBSERVED**. |
| `git diff --check` | **PASS** after source and documentation changes. |

The editor bridge was offline, the editor import was blocked by sandbox sockets, and no native pointer/keyboard feel or visible fullscreen/windowed-fullscreen layout was observed. P3 habitat species enforcement is next. Pond generation/bake remains P4; no Pond resources or marine-fish fallback were invented here.

## P3 — Habitat species enforcement (2026-11-11)

**Pre-task source:** `main` at `bbabcb245021865cf06fc08ceb08c1362a73274d`, clean tree, root `0.4.2`. **Result:** uncommitted P3 source and evidence at root `0.4.3`; the phase runner owns staging and the implementation commit. During the task, unrelated Pond wood source/assets and `tools/asset-factory/blender/common/naming.py` changes appeared in the shared working tree. P3 did not edit them; review staging scope before the runner accepts dirty-tree continuation.

`CharacterEntry` now records an explicit native `WorldHabitat` and separate selectable, ambient and reserved ecological role flags. FAF-FISH-001–005 are Marine and selectable; 001–004 are ambient, while 005 is not. `SpeciesCompatibility` resolves only exact registered IDs and known worlds, verifies role, model and optional baked-world identity, and returns bounded diagnostics for unknown, incompatible, missing and fixture-only requests. Neither display names nor file paths determine habitat. `CharacterCatalog.Find` no longer maps unknown IDs to Yellow Tang. Player art, Character Select, reef ambient creation, baked ambient restoration and the explicit future spawn-entry API use this policy. An unset character in Pond cannot load the legacy Ocean Fish3. The Ocean diagnostic smoke path retains that existing legacy visual.

Character Select builds tiles and animated previews only for compatible installed species. It shows an empty, disabled freshwater state before P8, with Back focus; returning to Ocean creates five choices with first-selection reset and keyboard focus wrap. A selected-habitat change while the screen remains open rebuilds the tiles and preview. The primitive `game/dev/pond_species_fixture.tscn` is test-only, has no FAF identity, and is accepted only by an internal test override. Baked ambient IDs are checked before scene resources are installed. On a selected-world change, a bound habitat's prior ambient population is cleared. The accepted Ocean bake and all marine assets remain intact; no Pond production fish or bake was added.

| Check | Result |
| --- | --- |
| `./tools/dev build` | **PASS:** Godot 4.7.2 .NET / SDK 10, zero warnings or errors after final source edits. |
| `./tools/dev inspect-species` | **PASS:** all five IDs accepted as Ocean selectable and ecological, all five rejected for Pond, four accepted as Ocean ambient and shark rejected for ambient; unknown world/ID/tag, missing model, wrong bake and fixture-only production requests rejected. Pond fixture accepted only via explicit test override. Pond empty screen, disabled Enter, Back focus, Ocean reload/reset/wrap, live habitat-change rebuild and no legacy Pond player verified. Accepted bake loaded twice with seed 42, 31 fish / nine schools and identical population signature; wrong-world/ID baked population rejected; world switch cleared population. Both loads reported `generator_invocations=0`. |
| `./tools/dev inspect-world-select` | **PASS:** existing Ocean/Pond menu, Ocean Back/reentry and two play sessions passed; Pond remained unavailable with `WORLD_BAKE_NOT_INSTALLED` and did not fall back. |
| `./tools/dev smoke-world` | **PASS:** accepted Ocean world ID/content, Forward+ atmosphere and C# smoke marker; no runtime generator invocation. |
| `XDG_DATA_HOME=/tmp/... ./tools/dev inspect-scene res://scenes/character_select.tscn` and fixture scene | **PASS:** both packed scenes loaded; the fixture reported `root=PondSpeciesFixtureOnly children=1`. |
| `./tools/dev inspect-character-select` | **FAIL as a strict log-clean check:** all five `CHARACTER_OK` markers and `CHARACTER_ROUTE_OK`/`CHARACTER_CHECK_OK` were emitted, but repeated world instantiation logged `System.InvalidOperationException: Handle is not initialized` from Godot .NET `ScriptManagerBridge.SwapGCHandleForType` while instantiating a baked subscene. The same intermittent handle failure was recorded in the earlier P3 character qualification. No clean result is claimed for this run. |
| `./tools/dev inspect-ambient` | **FAIL:** the existing GDScript check reported `loop leaves the bounded water pocket` on the accepted baked world. The new focused P3 test verifies population identity, count and deterministic replay, not that older position-bound assertion. Ambient movement placement remains unqualified by this run. |
| `git diff --check` | **PASS** after P3 source/evidence changes. |

The Fish Godot Editor MCP bridge was disconnected (`godot_get_server_info`, server/addon 2026.11.04), so no editor mutation or native visual observation was claimed. This is focused headless policy and scene evidence, not a P4 Gate pass. Pond bake, real freshwater FAF fish and wildlife, and native cross-world review remain later P4 tasks.
