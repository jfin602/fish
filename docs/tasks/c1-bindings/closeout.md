# C1 bindings — P5 closeout

**Final disposition, 2026-11-09: implementation complete and reconciled into the owner-accepted P1 baseline, with advanced native qualification limits retained.** The owner accepted the current post-c1 controls as good enough to move on and deferred further refinement. [Native evidence](qualification/native-controls.md) records a Linux/X11 Fish-window session and one repaired help-layout defect; [implementation evidence](qualification/implementation.md) covers the model, editor, switching and first-frame paths headlessly. No unresolved functional failure of ordinary swimming, pause/focus recovery or selected controls is recorded. The prior **NOT GREEN** disposition below accurately described the narrower P5 matrix before this owner decision. Its unobserved advanced actions remain unobserved; no new native session or comprehensive P5 PASS is claimed. [P1 closeout](../p1/closeout.md) owns the Phase 1 Gate decision.

## Historical P5 matrix before owner acceptance

| Scoped criterion | P5 result | Evidence / remaining work |
| --- | --- | --- |
| Keyboard Only, Mouse Only, Hybrid Swim respond in fullscreen and windowed fullscreen | PASS for basic visible input | Native session exercised each basic path; exact speed, turn and depth dynamics still belong to P1 P10. Current Mouse Only uses right-button slow and middle-button menu. |
| Create, duplicate, rename, save/apply, key/button capture, sensitivity/inversion, conflict warning and recovery from unbound control | PASS for listed paths | Native editor actions and isolated saved JSON in [native evidence](qualification/native-controls.md). |
| Revert and restart | PASS | Native session confirmed both with saved custom profiles. |
| Delete and complete keyboard-focus editor workflow | NOT OBSERVED | Native session used pointer editing and pause keyboard focus; P3 headless coverage does not replace native evidence. |
| Same obstacle route with two distinct hybrids; safe held-input live switching, momentum and simulation continuity | NOT OBSERVED | Two fresh route attempts occurred, but only the built-in reached the gap. No native state samples proved continuity. P4 headless [implementation evidence](qualification/implementation.md) remains separate. |
| Debug F6 cycle | PASS | F6 visibly changed built-ins. |
| F6 custom-binding priority | NOT OBSERVED | No native F6-bound custom trial. |
| Focus loss/refocus, pointer capture, display change, pause/resume and live help | PASS | Native refocus opened Pause. The overlapping-help failure was fixed and re-observed after a targeted build. |
| First-frame leak after resume | NOT OBSERVED | Native state samples were not captured; P4 saved-scene check remains separate. |
| Final owner approval / P1 dynamics | NOT OBSERVED | Requires P1 P10; no owner comfort judgment was supplied. |

The earlier P1–P4 failed import attempts and later focused checks remain in [implementation evidence](qualification/implementation.md). After the P5 help repair, `./tools/dev build` passed with zero warnings/errors and the saved-scene `preset_editor_check.gd` printed `PRESET_EDITOR_OK`; these checks do not qualify the missing native actions. `package.json` remains `0.1.9`; no `package-lock.json` or new third-party asset was introduced.

**Historical follow-up, now deferred:** the [P5 follow-up qualification](qualification/native-controls.md#follow-up-qualification-task) listed advanced native comparison/editor actions. The owner closed the P1 progression decision with the current candidate; those unobserved actions and future alternate-binding refinements can be revisited when gameplay makes them useful. The historical partial matrix is preserved rather than relabeled as an observed PASS.
