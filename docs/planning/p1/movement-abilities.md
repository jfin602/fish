# Movement abilities

**Decision date:** 2026-11-09  
**Status:** Owner-selected future movement feature group. Its original immediate-post-core scheduling was superseded on 2026-11-09 by the P2 underwater-world decision; implementation and qualification are not claimed.

## Priority and source

First make normal swimming, turning, and slowing feel right. The owner originally selected **Movement abilities** as the next movement follow-up in [worksheet answer Q08](swim-controls-worksheet.md#q08--extra-movement-actions). A later owner decision selects [P2 worldbuilding](../p2/underwater-world.md) as the next roadmap Phase instead. This document preserves the accepted ability group without claiming a schedule.

The [swimming contract](../../systems/swimming-and-camera.md) owns movement behavior, and [the roadmap](../../roadmap.md) owns Phase/Gate progress. The owner approved the seven core playtest criteria in worksheet Q10; [validation](../../validation.md#core-movement-acceptance) owns their execution/evidence. Actual native playtesting and owner acceptance must still lock the implemented core before these abilities can safely build on it, whenever they are scheduled.

## S25 — C5 speed boost scheduling

On 2026-11-14 the owner promoted the burst to a **finite-energy hold-Spacebar swimming boost** for chase/escape, in place of player biting. The [revised C5 M7 brief](../../tasks/c5-polish/manual/M7-regression-and-native-playtest-preflight.txt) owns implementation and validation. This changes neither historical P1 qualification nor the deferral of precision mode and evasive turning.

## Selected follow-up

| Ability | Intended experience |
| --- | --- |
| Short speed burst | **S25 C5 current scope:** finite recoverable hold-Spacebar faster swim; not yet implemented. |
| Precision mode | Slower movement for carefully approaching something or navigating a tight space. |
| Quick evasive turn | A deliberate action that changes direction more sharply than an ordinary turn. |

These are the three originally selected abilities. Only speed boost is now scheduled in C5; precision/evasive turning remain future work.

## Choices for the follow-up

Set the exact behavior, strength, duration, input bindings, and any costs or cooldowns when preparing this work. Preserve the mouse-only mode and keyboard fallback; controller support remains later work as recorded in worksheet Q02. Keyboard slowing is already a core control, not a new ability.

Use the accepted core as the playtest baseline. Each ability should have a distinct, understandable effect while keeping ordinary movement and camera behavior usable. Detailed implementation steps and qualification scenarios belong in the later scoped task work.
