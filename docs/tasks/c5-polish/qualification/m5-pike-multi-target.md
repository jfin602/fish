# M5 — Independent Pike hunting and school-assisted evasion

**2026-11-14, manual M5 only. T2 PASS; V40 native feel NOT OBSERVED.** Source: `jfin602/fish`, `main` at `d6e8d2d06946ce115e79e953c20fbc39311ba680`, root `package.json` **`0.5.12`**. The working tree already held M2–M4, Pike contact, and audio changes before M5. No commit, push, phase runner, or next manual brief ran. A fresh Godot Editor MCP `2026.11.04` bridge identified **Fish** at `/home/jfin/dev/fish/game/`, Godot **4.7.2 .NET**, active `movement_course.tscn`; no editor scene was edited or saved. The editor bridge connection is identity evidence, not an owner playthrough.

## Implemented behavior

- Each living Bass or Bluegill has a stable target ID and actual stage length. Pike receives separate world-space sight and finite movement cues for every individual Bluegill. The selection budget is 17 fish (Bass plus 16 Bluegill). Sight uses direction, three solid-occlusion rays, soft foliage, seven turbidity samples and light. Candidate positions enter Pike awareness only through sensed evidence; per-ID last-known positions expire after seven game seconds and dead fish are removed.
- Opportunity scores use fresh evidence and sensed distance. A switch needs 1.2 seconds since the last switch and a stronger opportunity over the current target. Physical Pike collider length sets edible size. Existing 80-degree-per-second turning, acceleration and collision-limited movement continue through a switch; committed movement now also takes the clear-direction route.
- Pike can commit to a single Bluegill, verify fresh sight and mouth contact, check a clear strike ray, consume that exact ID once, remove its collision/visual, and alarm nearby members of its school. Bass contact still ends only the fixture. Bass escape from diversion requires a verified Bass chase, a real switch away, and 6.5 game seconds without renewed Bass visibility or pursuit. Pause does not advance the clock; Retry resets sensors, cues and selection.

## Checks and limits

| Check | Result | Evidence |
| --- | --- | --- |
| `./tools/dev build` | **PASS** | Godot .NET/C# build, zero warnings and errors. |
| `./tools/dev inspect-pike-ai` | **PASS** | Real placed Bass/Bluegill sight produced both Bass retention and Bluegill switch; synthetic cases checked unseen/dead/oversize fish, hysteresis, pause, delayed diversion, renewed Bass pursuit, finite memory, physical Bluegill strike and once-only removal, school alarm, existing Bass catch/escape, wood avoidance and turn bounds. |
| `./tools/dev inspect-pike-sight` | **PASS** | Five wood and five plant sight cases, hard cover, soft attenuation, turbidity and camera stability. |
| `./tools/dev inspect-prey` | **PASS** | Forage/flee/cover and bounded prey sensing. |
| `./tools/dev inspect-pike-encounter` | **PASS** | Third destination, two habitats, retries and fixture isolation. |
| `./tools/dev inspect-world-isolation` | **PASS on this run** | Two Ocean/Pond/Pike cycles, three repeats, no predator/feeding/cover leakage. M4's earlier intermittent Godot .NET handle failure remains historical evidence. |
| `git diff --check` | **PASS** | No whitespace errors. |

The target-choice probe evaluated **17 candidates** per call; its 1,000-choice diagnostic took **3.84 ms** on this host. The exposed Bass fixture recorded **51 actual Pike-facing sight samples over 0.60 game seconds**, with upper bounds of **153 rays and 357 water samples**. These are headless diagnostic counts, not a sustained native frame-pacing measurement.

**NOT OBSERVED:** an owner-swum school diversion, continuous perceived turn/acceleration feel, readable pursuit and scatter in native rendering, and full V40/P5 Gate fairness. The deterministic target-switch and delayed-escape tests prove the decision and accounting rules; they do not replace an owner-controlled live route. Keep [P5 closeout](../../p5/closeout.md) historical and **NOT GREEN**. M6 is the next manual brief only after owner review; M8 owns the final native playthrough.

## Owner screenshot follow-up: solid prop size

On 2026-11-14 the owner supplied a paused subadult Pike Encounter screenshot with foreground wood obscuring much of the view and clarified the requirement: solid object collision must follow its displayed size without hand-sized boxes per model. The screenshot is owner-supplied visual evidence, not an independently reproduced editor play session or proof of the exact collider at that position.

`StaticVisualCollision.Install` now builds a static concave shape from each visible prop mesh and copies the mesh's local-to-body transform. One shape is cached per asset and mesh part, so instances scaled from small shore sticks to full logs share geometry while their body transform supplies the displayed size. The rule replaces gameplay collision proxies for Pike Encounter wood, its scaled shore wood, and ordinary Pond structural wood and bank accents. Baked terrain and Ocean static geometry already derive collision from generated/imported mesh; hidden boundary limits and moving fish retain separate gameplay shapes. Source FAF exports and standalone wrapper proofs remain untouched.

After this change, `./tools/dev import`, `./tools/dev build`, `./tools/dev inspect-pike-cover`, `./tools/dev inspect-pike-ai`, `./tools/dev inspect-pike-sight`, `./tools/dev inspect-prey`, `./tools/dev inspect-growth`, `./tools/dev inspect-pike-encounter`, `./tools/dev inspect-pond-scenery`, direct base-Pond `--pond-inspect=res://worlds/base/pond-p4-71`, `./tools/dev inspect-world-isolation`, and `git diff --check` passed. The cover check verified all ten full-size fixture logs and both small shore-wood scale bands have visual-matched collision; Bass and Pike crossed the open split hollow, and the follow camera had zero measured overlap samples. It reused **20** mesh shapes for repeated Pond/encounter props. The normal Pond hero log remained traversable and the world-isolation cycle passed on this run. These are headless physics and structural checks. The owner's specific obscured view and post-fix native feel remain **NOT OBSERVED**; stop for owner review before M6.
