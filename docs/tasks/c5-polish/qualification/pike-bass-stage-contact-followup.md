# Pike contact across Bass stages — owner follow-up, 2026-11-13

## Report and cause

**FAIL in owner native play:** the subadult Bass collision body appeared to push the Pike away, preventing a kill. The owner asked for the same glitch to be checked at every size. Before this correction, encounter Bass, Pike and live Bluegill used collision layer/mask `1/1`, the same layer as solid Pond cover. The Pike's `MoveAndSlide` could therefore stop on a Bass collider before its mouth reached the fitted Bass capsule. The previous M1 contact probe teleported the bodies together, so it did not expose the blocked approach.

## Correction and focused evidence

Encounter swimmers now occupy collision layer **2** and use collision mask **1**. They still collide with solid Pond bed/wood/roots, while physical fish bodies no longer block Pike movement. Pike sensing, clear-cover rays and catch contact still use the real Bass capsule and the current stage radius. The egg collider remains disabled during the intro. Ordinary Ocean and Pond player/ambient collision settings were not changed.

`PikeAiChecks` now approaches a stationary Bass with actual Pike body movement, then verifies clear sight and mouth-to-capsule contact for each swimming stage. The Fish Godot **4.7.2 .NET** headless run reported:

| Bass stage | Radius (m) | Pike approach | Clear sight and mouth contact |
| --- | ---: | ---: | --- |
| Larva | 0.0110 | 0.680 m | PASS |
| Juvenile | 0.0594 | 0.680 m | PASS |
| Subadult | 0.1436 | 0.680 m | PASS |
| Adult | 0.2232 | 0.680 m | PASS |

The full fixture AI also produced `PIKE_CATCH_CONTACT` and `outcome=caught` against the juvenile after windup. `./tools/dev build`, `inspect-pike-ai`, `inspect-growth`, `inspect-feeding`, `inspect-prey`, `inspect-bluegill-schools`, `inspect-pike-encounter` and `inspect-hatch` **PASS**. Growth retains wall and shore clearance at juvenile, subadult and adult; the hatch check retains larval wall clearance. Encounter route/retry checks still report three destinations and two habitats. `git diff --check` **PASS**.

The first build of the new probe **FAIL** on a C# local-name conflict and was fixed. The first Pike AI run **FAIL** because the Bass still loaded at layer `1` despite the scene value; runtime initialization now explicitly sets layer `2`. A later Pike AI run reached all four contact markers but **FAIL** on an old escaped-juvenile travel assertion expecting over 0.15 m in 20 frames. With the current 0.48 m/s juvenile cap, that threshold was stale; the probe now checks nonzero thrust and over 0.03 m of travel and **PASS**. These failed attempts are retained as diagnostic history, not final passes.

Godot Editor MCP reported `bridge.connected=false`, so the corrected contact and kill feel are **NOT OBSERVED** in native play. The probe verifies stage contact geometry and sight; a full AI catch was exercised at juvenile only. Owner should replay an exposed Pike strike against subadult Bass and watch for a kill without a push-away, then check larva/juvenile/adult if available. No P5/C5 Gate status changed. Source: `jfin602/fish`, `main`, HEAD `d6e8d2d06946ce115e79e953c20fbc39311ba680`, root package `0.5.12`. No commit, push or M5 execution. **Stop for owner review.**
