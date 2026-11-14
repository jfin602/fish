# C5 M6 — Fish predation blood FX evidence

**2026-11-14 source:** `jfin602/fish`, `main` at `d6e8d2d06946ce115e79e953c20fbc39311ba680`; root `package.json` remains `0.5.12`. The working tree already contained uncommitted M2–M5, fixture, audio and documentation work. M6 did not commit, push, reset or execute the phase runner. A Fish 4.7.2 .NET editor process was open, but the Godot Editor MCP bridge reported disconnected, so no editor play observation is claimed.

## Implemented

- The successful Bass→Bluegill ledger commit, physical Pike→Bluegill strike, and verified Pike→Bass catch each record one session-local `FishPredationEvent` with stable predator/target IDs, a unique event ID, game time and a world-space mouth/prey contact. Failed bites, food, shell, overlap and animation callbacks do not register fish-predation events. The event set prevents duplicate visual dispatch; feeding and Pike sensing do not read the FX.
- `FishBloodFx` owns four preallocated, world-space, one-shot GPU particle emitters. Each has at most 12 soft alpha quads (six with `--graphics=low`), no shadows, a 1.5 second tunable life, outward drift, rust-to-Pond-brown/green tint and declining opacity. Full-pool events are remembered but do not allocate more GPU emitters. `--effects=off` or `--blood-fx=off` suppresses emission without altering predation events; the test uses `--effects=off`. Pause freezes GPU speed and lifespan; resume restores engine time scale; retry/return clears emitters before scene unload.

## Checks and limits

| Result | Command / evidence |
| --- | --- |
| **PASS** | `./tools/dev import` generated both new `.cs.uid` sidecars. `./tools/dev build` completed with 0 warnings and 0 errors. |
| **PASS** | `./tools/dev inspect-blood-fx`: normal, `--effects=off`, and `--graphics=low`. It checked duplicate IDs, exact emitter position, one-shot/world-space/no-shadow state, soft texture and fade, pause, engine time scale, expiry, full-pool cap, clear and feeding invariance. An initial run exposed an uninitialized pool ID array; fixed before the passing run. |
| **PASS** | `./tools/dev inspect-feeding`, `./tools/dev inspect-growth`, `./tools/dev inspect-pike-ai`: existing food/growth/Pike checks passed with added one-event-per-real-fish-catch assertions. Growth consumed three larvae and a juvenile; Pike consumed a school Bluegill and caught Bass. Invalid/repeated bites did not add events. |
| **PASS** | Direct headless `--growth-check --effects=off` and `--pike-ai-check --effects=off`: real Bass and Pike predation/nutrition/catch outcomes still passed with zero blood emissions. |
| **PASS** | `./tools/dev inspect-pike-encounter`: three retries and return to World Select passed with a test-only active cloud, pause speed zero, old emitter disposal and a fresh empty pool. `./tools/dev inspect-world-isolation`: two Ocean/Pond/Pike cycles passed this run. The historically intermittent Godot .NET handle failure remains a known risk, not a new Gate pass. |
| **PASS, bounded native smoke** | A separate Godot 4.7.2 Vulkan Forward+ run on AMD Radeon 880M executed `--blood-fx-native-probe` with a fixed underwater camera and four test-only clouds. Ten-frame average draw calls were **417.0 baseline / 421.0 active**, a **+4.0** draw-call delta. Full pool bound: 4 emitters and 48 particles (24 in low mode), 1.5 seconds. [Captured fixed-view frame](../../../../artifacts/blood-native/blood-fixed-view.png) is a scripted visual probe, not a screenshot of an actual fish bite. The first probe was invalid as a fixed view because the follow camera moved; the corrected probe disabled that camera update and produced this measurement. |
| **PASS** | `git diff --check`; root package version unchanged. |
| **NOT OBSERVED** | Owner-controlled bite visibility, palette, timing and feel; actual native gameplay catches; long-session frame-time distribution; M8 underwater aesthetic approval. No owner play claim or P5 Gate upgrade. |

M6 is ready for owner diff review. The next brief is **M7**, manually selected only after review. M8 remains the owner-native playthrough and visual approval step.

## 2026-11-14 owner screenshot follow-up — caught aftermath

The owner supplied `paused.png`, showing a frozen catch, visible Bass model and centered menu, then reported that an initial unpaused build showed no blood. This is owner feedback on a local build, not an owner approval of the correction below. The blood emitter is a separate world node: the verified catch recorded one active emission even after `PlayerVisual` was hidden. The first native catch frame showed the cloud was too faint and mostly hidden by the Pike.

The verified Pike→Bass catch now leaves the scene running. It hides and disables the Bass body, holds a side view of the contact, keeps the small schools moving, and physically steers the caught Pike away without further sensing or bites. The blood fades over its existing 1.5 second life. Retry/Return appears after a 2 second clear view; Escape can reveal it sooner. The Pike continues to swim with the menu open. Ordinary voluntary pause/resume remains separate. The blood quads have larger soft coverage and a stronger muted rust tint, still bounded by four pooled emitters and 48 particles (24 in low graphics).

| Result | Follow-up evidence |
| --- | --- |
| **PASS** | `./tools/dev build`: zero warnings/errors. `./tools/dev inspect-pike-ai`: real physical catch, hidden Bass, unpaused tree, held camera, Pike moved more than 1 m, blood expired, no extra predation event or perception sample, delayed menu. The first revision of this check used frame count to measure FX age and failed; the corrected check waits 2.5 game seconds. |
| **PASS** | Direct `--pike-ai-check --effects=off`: the same catch/aftermath succeeds with no blood emitter. `./tools/dev inspect-blood-fx`: normal, effects off and low graphics. `./tools/dev inspect-feeding`, `./tools/dev inspect-growth`, `./tools/dev inspect-pike-encounter` and `./tools/dev inspect-world-isolation`: fish feeding/growth, retry/return cleanup and Ocean/Pond/Pike isolation. `git diff --check`. |
| **PASS, bounded native smoke** | Godot 4.7.2 .NET Vulkan Forward+ on AMD Radeon 880M ran the scripted physical catch. [Catch cloud](../../../../artifacts/aftermath-native/catch-cloud.png) shows the Bass gone and the contact cloud visible; [diluting cloud](../../../../artifacts/aftermath-native/diluting-cloud.png) shows the Pike farther away while the cloud softens; [after fade](../../../../artifacts/aftermath-native/after-fade.png) shows the cloud gone and the retry menu available. This is a scripted probe, not an owner playthrough. A repeated four-emitter fixed-view probe measured 417.0 baseline / 421.0 active draw calls, **+4.0**, with 48 maximum particles. |
| **NOT OBSERVED** | Final owner-controlled catch timing, cloud feel across Pond locations, full playthrough and M8 aesthetic approval. No P5 Gate change. |
