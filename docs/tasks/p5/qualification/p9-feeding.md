# P5 P9 — Bite, prey eligibility and nutrition

**2026-11-13 source result: PASS at T2; native feeding and hunt feel: NOT OBSERVED.** This is uncommitted `0.5.9` work for the phase runner. Pre-task `main` HEAD was `d63c19fd552c1cb6ef1cac6328206cfec594554f`, exact subject `0.5.8`, with a clean tree. [P8 manual evidence](native-stealth.md) records the owner's explicit “mark it green, change version to 0.5.8 and ill start the runner” authorization, and the actual P8 checkpoint is HEAD. [P4 closeout](../../p4/closeout.md) permits P5 progression while remaining NOT GREEN.

## Implemented fixture contract

- The adult Bluegill's registered length is 0.309825 m. The P8 Bass juvenile is 0.235077 m (76% of Bluegill length), so it remains a non-edible awareness actor. Adult Northern Pike is 1.0752 m and is ineligible. Five registered Bass larvae (0.058056 m) are the edible prey at open ribbon grass, pondweed, root ball, split log and branch crown approaches. The four P8 juveniles remain present.
- Five small glowing invertebrate **non-shipping fixture proxies** occupy the open sides of those cover transitions. They are not a new FAF asset or an ordinary-world species. One initial ribbon position overlapped authored wood in the focused check; it was moved to the open central crossing before the final PASS.
- Space triggers a directed bite. The live larva must be in front of the fish, within 1.1 m of its mouth, in the same Pond habitat, under the current 45% body-length rule, and across a clear physics ray. Tiny food uses 0.56 m contact plus a clear physics ray. Both leave movement, sight, cues and Pike processing active. Bite misses give a short on-screen reason.
- Session-local stable IDs and `FeedingLedger` commit consumption once. A larva awards 4 nutrition; a proxy awards 1. Only successful consumption emits a measured target/time event. Invalid, dead, already-consumed, incompatible, rear, distant or blocked attempts award zero. No durable save, hunger drain, growth, death or story was added.

## Actual checks

| Command / observation | Result and limit |
| --- | --- |
| `./tools/dev build` | **PASS** final source, 0 warnings and 0 errors. Earlier intermediate build caught a test array `Count` mistake; corrected before final check. |
| `./tools/dev inspect-feeding` | **PASS** seed 5266, baked fixture world `world-6c98f9e946b4ede8`. Registered size/habitat/dead/duplicate/blocked policy checks; a simulated Space key dispatched one Bass larva bite, with duplicate and rear/distant attempts, five food collections, distinct IDs, spawn/food wood-overlap checks, and a physical wall whose collider was confirmed on the bite ray. Final ledger: 9 nutrition, 6 events, no duplicate reward. Fixed poses and synthetic input do not prove native player hunt feel. |
| `./tools/dev inspect-prey` | **PASS** four P8 juveniles retain shared sensory/flight and tested root/fork/split wood traversal. Five new larvae use the same physical movement code; a full native larva movement route was not observed. |
| `./tools/dev inspect-pike-ai` | **PASS** scripted catch/search/escape, pause and restart with feeding actors present. |
| `./tools/dev inspect-pike-encounter` | **PASS** on a sequential rerun: three destinations, two habitats, three fixture retries, normal-world entry and cleanup. A prior invocation run concurrently with other Godot checks printed two `Handle is not initialized` engine errors during repeated scene load and returned FAIL despite its completion marker. The sequential rerun printed no such errors. This transient failure remains recorded. |
| `git diff --check` | **PASS** after implementation and documentation edits. |
| Godot MCP `godot_get_server_info`, `godot_list_toolsets` | **BLOCKED** for live editor use: server 2026.11.04 reported `bridge.connected=false` at `ws://127.0.0.1:19080`. No editor mutation or selected live project identity claim. Source and developer CLI target `/home/jfin/dev/fish/game`. |
| Native owner-controlled feeding, exposure, prey visibility, performance | **NOT OBSERVED.** P11 must review these, including tiny larva discoverability after P8's earlier failure. |

The selected world still loads the seed-5266 baked Pond fixture with no generation invocation, 47 cover placements, and unchanged Ocean/Pond catalog routes. The food placement check proves the five proxy centers are outside solid wood and reachable at a fixed nearby pose; it does not prove a player can comfortably find or pursue every larva from each approach. The wall test uses a temporary real physics body; existing prey cover checks test authored wood collision. No P5 Gate is GREEN.

Source SHA-256 at this result: `FeedingLedger.cs` `92b77f5257ad5a741083c59791ba162a19d7733bbc5a395fb04a989ceab0c481`; `FixtureFood.cs` `bfc3aba6df8294a63b469d4399bd577c3f2afa7ab07b9fd90f59006b8ba84814`; `PikeEncounter.cs` `5c08a028de1dbc18430bdec1f1d358491bfc4fc368a0ea703166e1ba110af7a6`; `PreyActor.cs` `ceae33fa8fd3cbefd413a0759b6c460be356c0a00b2885e389c4ebb73903186e`; `FeedingChecks.cs` `eb7297a88c9e84138fcc7841e35851787c2c5b35fab6160e2621272bc5751746`. Root `package.json` is `0.5.9`; no implementation commit or push was made inside this task.
