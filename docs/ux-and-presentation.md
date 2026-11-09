# User experience and presentation

## Status

This document separates confirmed interface requirements from working presentation ideas. All game interfaces are currently planned and unqualified. [The decision register](decisions-and-questions.md) owns requirement status; [the roadmap](roadmap.md) owns progress.

## Confirmed perspective and continuity

Third person is the main experience in a 3D underwater world. The player sees the same fish grow from hatchling to large fish. Optional first person offers another view of that life. Camera changes must not reset survival state or encounters.

A living fish can be continued across sessions. Death ends that life permanently. The main menu contains a history section for all completed fish stories, and death presents a brief recounting of the life that just ended.

## Main menu

| Action | Intended behavior |
| --- | --- |
| Continue | Available for a valid living save; resumes that fish and its current situation. |
| New life | Starts a new hatchling under the explicit active-save replacement policy. |
| Life History | Opens the saved collection of completed-life stories, including a truthful empty state. |
| Settings | Provides the controls, display, audio, and accessibility options implemented for the current scope. |
| Quit | Leaves without advancing the world while closed. |

These labels are working UI wording. Continue, New life, and Life History must represent actual state. A technical save error should be explained and preserve data; it must not be presented as a dead fish or an empty history.

## During play

Prioritize the fish, nearby food, useful shelter, and threats. Size differences should be visually legible. If health, stamina, hunger, or growth meters are implemented, derive them from authoritative state and show information that helps the next decision.

The working camera is slightly above and behind the fish, with useful visibility in the vertical direction. Plants, rocks, depth changes, and lighting can provide spatial cues. Art style, water effects, camera distance, fog, and exact HUD treatment are open choices.

Predator approach and attack feedback should offer an understandable chance to react. Avoid relying only on color to distinguish edible targets, danger, or an unavailable action. An implemented burst, bite, or escape should have feedback that matches its actual outcome.

## Pause, saving, and recovery

Provide a clear way to pause and leave a session. Saving should communicate whether the operation succeeded. If it fails, retain the current life and explain the problem; do not falsely claim the player can safely resume a state that was not saved.

The game is paused while closed. Resume should restore the relevant danger state without surprise input carried from the menu. See [survival and runs](systems/survival-and-runs.md) for persistence rules.

## Death and recounting

The proposed sequence is a brief death transition, the fish's final identity/appearance, a short life account, and supporting statistics. Keep the cause factual and understandable. A skip action advances the presentation while retaining the same archived entry.

The archive should remain reachable whether the player starts another life immediately or returns later. Viewing a dead fish's story is a read-only reflection on that life, not a resume action.

[Life stories and history](systems/life-stories-and-history.md) owns event selection, factuality, retention, and idempotent archive behavior. A portrait, player-given name, narration, or animated timeline is optional until adopted for a task.

## Acceptance targets

- A first-time player can identify food, shelter, and an approaching threat in the native 3D view.
- Growth is visible and the camera remains usable at the tested sizes.
- Menu actions accurately reflect living, dead, missing, and unreadable-save states.
- The recounting can be skipped and later read in full from **Life History**.
- A player can browse all retained stories and return to the menu without changing the active run.
- Text, input focus, and any animations remain readable in the tested display conditions.

These targets require native observation where presentation or input is claimed. Headless checks and MCP connectivity alone are not proof of a usable interface.
