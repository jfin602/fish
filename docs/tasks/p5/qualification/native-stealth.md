# P5 P8 — Native stealth and prey awareness review

**2026-11-13 disposition: P8 OWNER GREEN FOR PROGRESSION at `0.5.8`; detailed T3 matrix remains partial.** The owner first reported that the larva could not be found, selected the verified Bass juvenile, requested several juveniles, then judged the four-juvenile candidate “looks good” and explicitly directed “mark it green, change version to 0.5.8 and ill start the runner.” This approves the P8 manual checkpoint and next-task progression. It does not establish a complete PASS for V05, V33, V34, V35 or V36, or change any P5 Gate to GREEN.

## Entry and source identity

- `main` HEAD `099a00ef0e80b7b3d8a1842238dff5d0ea97d1b6`, exact subject `0.5.7`, clean at entry. The P7 evidence is [p7-prey.md](p7-prey.md); the runner's `2026-11-13T13-50-11-929Z` run records P7 passed and `browser_execution_required` at P8 before launch. The root package version is `0.5.7`.
- [P4 closeout](../../p4/closeout.md) explicitly authorizes P5 progression while retaining NOT GREEN. [P6 review](native-m1.md) records the owner's limited progression authorization and unresolved M1 fairness/cover gaps. Neither is P8 owner approval.
- The Fish command selected `/home/jfin/dev/fish/game`, Godot `4.7.2.stable.mono.official`, .NET SDK `10.0.403`, display `:0`, Forward+ on AMD Radeon 880M, and Fish addon `2026.11.04`. The P5 fixture is separately baked Pond seed `5266`, world `world-6c98f9e946b4ede8`, content `f056c3216989617bb02565ee98a2bbf41d2a72373d11dc0c6a09703e417cd072`; its manifest SHA-256 is `c042b0a7cc847c553f88fbdf75f1a6f19ad6293aadd7b497167567f143814a9b`.
- Godot MCP server `2026.11.04` reported `bridge.connected=false` at `ws://127.0.0.1:19080`. Another `godot-editor-mcp` process held that port. Therefore the live editor project identity could not be verified and no editor mutation, MCP input, game screenshot or runtime probe claim is made.

## Commands and observations from this review

| Command or action | Result | Evidence limit |
| --- | --- | --- |
| `git status --short --branch`; `git rev-parse HEAD`; `git show -s --format='%s' HEAD`; `node -p "require('./package.json').version"` | **PASS** — clean `main`, actual P7 checkpoint and root `0.5.7`. | Source/version entry only. |
| `./tools/dev doctor` | **PASS** — Fish path, native toolchain, display and addon identified. | Does not establish a connected editor. |
| `godot_get_server_info`; `godot_list_toolsets` | **BLOCKED** for live editor use — Fish MCP bridge offline; enabled toolsets were inventoried. | No MCP native input or capture. |
| `./tools/dev run --display-mode=windowed-fullscreen` | **PASS** for launch and build, zero warnings/errors. Native Vulkan Forward+ reported AMD Radeon 880M. World Select reported three entries and two habitats; the separate Pike scene reported Bluegill, Pike, Bass larva, 47 cover placements and seed `5266`. The game logged escapes at `90.22 s` and `108.55 s`, a fixture reload between them, then a return to World Select and clean process exit. | The log confirms two native escapes, a retry and return, but does not identify input, route, sight conditions, fairness, naturalness, performance or normal-world isolation. No screenshot was captured. |
| `git diff --check`; local Markdown link scan; `npm run fish:phase:validate -- p5` | **PASS** after the `0.5.8` documentation update. The stack still identifies P8 as Browser-required and P9 as the next implementation prompt. | Grammar, links and whitespace only; they do not qualify gameplay. |

## Manual T3 matrix

| Scope | P8 result and remaining observation |
| --- | --- |
| V33/V34: still, glide and burst in open water, reeds and behind wood; above/below sensing; investigation, chase, last-known search and disengagement | **NOT OBSERVED.** P7/P6 scripted checks and prior native outcome logs remain prior evidence only. |
| V05: fair hidden escape and exposed catch with readable strike/contact | **NOT OBSERVED** for fairness and route. The first candidate logged escapes at `90.22 s` and `108.55 s`; the four-juvenile candidate logged a catch at `86.12 s`. Their sensory and player context are unknown. P6's earlier native failures remain open. |
| V35: slow covered versus fast exposed approach and genuine prey flight | **FAIL** for initial larva discoverability: the owner's direct report was “i cant find the larva”. The revised juvenile candidate is **NOT OBSERVED** by the owner yet. Bite, nutrition, eligibility and once-only reward are outside P8 and remain for P9/P11. |
| V36: repeated retry, return, focus, pause and ordinary Ocean/Pond entry without predators or actor leaks | **NOT OBSERVED** as a full native sequence. Runtime logs show three World Select entries and two habitats, fixture reloads and a return after the later catch. Focus/pause and ordinary Ocean/Pond hand-play remain open. |
| Five new plant families and five new wood families, shallow→intermediate→deep route, concealment/exposure, grounding, side entries, fish/camera/Pike clearance, naturalness and dense-cover frame pacing | **NOT OBSERVED** in owner-controlled movement. The launch reported 47 placements but did not identify or qualify any family's visual or stealth role. |
| Owner readability, fun and progression approval | Initial prey discoverability **FAIL** was repaired. The owner said the four-juvenile candidate “looks good” and explicitly approved `0.5.8` and runner progression. This is **P8 GREEN FOR PROGRESSION**; a separate detailed fun/fairness judgment was not recorded. |

## Bounded prey-discovery repair awaiting owner review

The owner selected the existing Bass juvenile stage (`FAF-FISH-007`, 0.235 m long) for P8 awareness review because the registered larva is only 0.058 m long and was not findable. The first repair placed one juvenile about 4.7 m from the Bluegill. After that native run, the owner requested several Bass juveniles. The current isolated encounter spawns four verified juvenile assets at distinct safe shallow positions, each with its own physical body, swim clips, sight sensor, disturbance field and bounded awareness. The nearest starts at `(-15, 7.349, 20)` about 4.7 m from the Bluegill; the encounter status gives a short natural location hint. It adds no bite, nutrition, growth, HUD target marker or ordinary-world actor. The earlier larva source evidence remains historical P7 evidence; it is not overwritten as a juvenile result.

Current source SHA-256: `PikeEncounter.cs` `1559a56b053613b7ae7365852604f35eb8cd56fb2759137233b80538dd78fd4c`, `PreyChecks.cs` `79065be0158ffcec2d2e3e7d1d3fbaeb35a9183d90f141748d2119a774b49b4c`, `PikeEncounterChecks.cs` `8a5508344ab86c0db787f27162c5f874d3d2e610523dbb8ac19c11f1e862c886`.

| Actual focused check after change | Result and limit |
| --- | --- |
| `./tools/dev build` | **PASS**, zero warnings/errors. |
| `./tools/dev inspect-prey` | **PASS** on the four-juvenile revision for registered dimensions/rig, four distinct near-start spawns outside solid cover, unchanged fixed-pose sensory comparisons, physical flight, pause/reset and sampled wood traversal. An intermediate single-prey candidate at `(-14, 18)` **FAILED** because it overlapped solid cover; the later positions passed. This remains headless, not a visibility judgment. |
| `./tools/dev inspect-pike-encounter` | **PASS** on the four-juvenile revision for three routes/two habitats, three retries, all old prey actors freed, normal Pond/Ocean paths and fixture cleanup. Scripted, not owner navigation. |
| `./tools/dev inspect-pike-ai` | **PASS** on the four-juvenile revision for scripted Pike catch/search/escape and pause/restart. |

The single-juvenile candidate was reopened with `./tools/dev run --display-mode=windowed-fullscreen`. The native Vulkan Forward+ process logged escapes at `126.75 s` and `127.95 s`, a retry after each, then returned to World Select and exited cleanly. The four-juvenile revision then loaded in the native game, logged a caught outcome at `86.12 s`, returned to World Select and exited cleanly. The owner's “looks good” approval is the native judgment on that candidate; no source screenshot, frame pacing measure, completed cover-route report, slow/fast prey comparison, catch-contact/fairness account, or normal-world traversal was provided. The logs do not establish those results. Root version `0.5.8` identifies the approved P8 candidate; the exact-subject `0.5.8` Git checkpoint is still pending, and no commit/push was made here. The original failure and unobserved qualification cells remain in this record. Feeding and growth remain P9–P11 work.
