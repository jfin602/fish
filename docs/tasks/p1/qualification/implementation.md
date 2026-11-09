# P1 implementation evidence

## P1 — Fullscreen play shell (direct execution, 2026-11-09)

Candidate: `main` at `2e3db09925e8fecf44e44514770b53bac34f88de`, with preexisting uncommitted P0 source/evidence and unresolved documentation merge entries, plus this uncommitted P1 shell. The root package was `0.0.3` on entry, rather than the planned `0.1.0` Phase-entry baseline. After focused checks passed, only root `package.json` advanced to this brief's assigned `0.1.1`; this does not imply a completed `0.1.0` baseline checkpoint. No runner was started, and no checkpoint commit or push was made. [P0 closeout](../../p0/closeout.md) records G0.1–G0.3 PASS and the native Linux foundation evidence. This P1 work used that existing C# scene and CLI; it did not rerun or recreate P0.

### Implemented behavior

- `game/project.godot` requests exclusive fullscreen on startup and scales canvas controls with the viewport. The existing Compatibility renderer, 4.7.2 .NET pin and provisional fish motor remain.
- `Main` loads local presentation preferences before its first frame, applies a validated game-owned `--display-mode` override for this session, and reports requested/source/effective mode. The pause shell offers Fullscreen (`Window.ModeEnum.ExclusiveFullscreen`) and Windowed fullscreen (`Window.ModeEnum.Fullscreen`). The installed Godot 4.7.2 API describes ordinary fullscreen as an undecorated, full-display window; on Wayland it treats exclusive fullscreen equivalently.
- `user://preferences.cfg` stores `display/mode` and `input/show_control_help`. Missing preferences default to fullscreen and visible help. Deliberate panel changes save locally; launch overrides do not. This is separate from future living-save data. Unknown existing preference sections are retained on save.
- The anchored pause panel supports pointer activation, Tab/arrows and visible keyboard focus on controls. It offers Resume, display choice, help toggle and Quit test session. The existing help text now wraps against the actual viewport and states only current P0 bindings.
- Escape pauses/resumes; application or window focus loss pauses. The tree stops physics, the provisional fish velocity is cleared, and swimming remains disarmed until old actions are released plus a skipped physics tick. Changing display clears input as well. The UI resume action does not feed the existing Space swim-up binding into movement.
- `tools/dev run` retains its build/launch composition and accepts only `--display-mode=fullscreen` or `--display-mode=windowed-fullscreen`; it forwards the selected value after Godot's `--` separator as one quoted argument. Invalid values fail before build.

### Focused checks and limits

| Check | Result |
| --- | --- |
| `./tools/dev doctor` | Godot 4.7.2 .NET, SDK 10.0.403, Fish project/addon 2026.11.04 and Linux X11 `:0` reported. MCP tool server at Fish bridge 19080 was disconnected, so no editor mutation or live MCP claim was made. |
| `./tools/dev build` | First attempt found two notification constant type errors; corrected. Subsequent builds: 0 warnings, 0 errors. |
| `./tools/dev check` | PASS: import, one composed build, C# main-scene smoke (`FISH_FOUNDATION_READY`, `FISH_SMOKE_OK`), scene save/reload automation, and expected missing-scene failure. |
| `bash -n tools/dev`; `./tools/dev run --display-mode=bogus` | Syntax valid; invalid wrapper option exits 2 before build. |
| Isolated `XDG_DATA_HOME` headless main-scene smoke | Fresh profile requested fullscreen; saved `windowed-fullscreen` was read; explicit fullscreen and windowed-fullscreen overrides took precedence; the saved file remained unchanged after override. Unsupported direct game argument was repaired to exit 2. |

Headless Godot reports its effective window mode as `Minimized`; that result does not qualify native fullscreen. The actual panel rendering, pointer/keyboard operation, focus loss/refocus, full-display layout and backend mode behavior are **NOT OBSERVED** here and remain P5 work. Source and headless checks demonstrate the configured input-clearing/rearm path, not a native transition. G1.4 and Phase 1 remain open. The existing `0.0.3` entry metadata and unresolved merge must be reconciled into a reviewable `0.1.0` baseline/history before a later runner continuation; this direct change set does not synthesize that checkpoint.
