# Movement abilities

**Decision date:** 2026-11-09  
**Status:** Owner-selected next implementation focus after core controls are locked down. Deferred during core tuning; implementation and qualification are not claimed.

## Priority and source

First make normal swimming, turning, and slowing feel right. Once the owner locks down that core, **Movement abilities** is the next implementation focus. This sequence comes from the owner's [worksheet answer Q08](swim-controls-worksheet.md#q08--extra-movement-actions).

The [swimming contract](../../systems/swimming-and-camera.md) owns movement behavior, and [the roadmap](../../roadmap.md) owns Phase/Gate progress. The owner approved the seven core playtest criteria in worksheet Q10; [validation](../../validation.md#core-movement-acceptance) owns their execution and evidence requirements. The completed worksheet locks the design. Actual native playtesting and owner acceptance must lock the implemented core before this follow-up begins.

## Selected follow-up

| Ability | Intended experience |
| --- | --- |
| Short speed burst | A quick dart forward, faster than normal swimming. |
| Precision mode | Slower movement for carefully approaching something or navigating a tight space. |
| Quick evasive turn | A deliberate action that changes direction more sharply than an ordinary turn. |

These are the three selected follow-ups. Their order within the group remains open.

## Choices for the follow-up

Set the exact behavior, strength, duration, input bindings, and any costs or cooldowns when preparing this work. Preserve the mouse-only mode and keyboard fallback; controller support remains later work as recorded in worksheet Q02. Keyboard slowing is already a core control, not a new ability.

Use the accepted core as the playtest baseline. Each ability should have a distinct, understandable effect while keeping ordinary movement and camera behavior usable. Detailed implementation steps and qualification scenarios belong in the later scoped task work.
