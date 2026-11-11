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
