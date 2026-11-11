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
