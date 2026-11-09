# Documentation index

Start at [AGENTS.md](../AGENTS.md) and [BOOT.md](../BOOT.md), then follow only the routes needed for the task. This documentation describes a planned game and a working development runner; the Godot project is the next implementation step.

## Ownership

| Document | Owns |
| --- | --- |
| [Vision](vision.md) | Player fantasy, pillars, and scope boundaries. |
| [Game design](game-design.md) | Overall life loop and relationship between systems. |
| [Swimming and camera](systems/swimming-and-camera.md) | Approved core controls, movement feel, camera/body presentation, remaining tuning and later integration boundaries. |
| [Swim controls worksheet](planning/p1/swim-controls-worksheet.md) | Completed owner discussion and provenance for all ten controls answers. |
| [Movement abilities](planning/p1/movement-abilities.md) | Selected follow-up after actual core acceptance: burst, precision mode and evasive turn. |
| [Ecology and growth](systems/ecology-and-growth.md) | Food, prey, predator interaction, growth and habitat use. |
| [Survival and runs](systems/survival-and-runs.md) | One life across sessions, pause, permanent death, saving and recovery. |
| [Life stories and history](systems/life-stories-and-history.md) | Meaningful event capture, factual recap and durable main-menu archive. |
| [UX and presentation](ux-and-presentation.md) | Player entry points, feedback, menus, recap/history navigation. |
| [Art and assets](art-and-assets.md) | Approved realistic ocean direction, asset shortlist, starter order, licenses and import checks. |
| [Tutorial](tutorial.md) | Proposed first-life teaching sequence and readable early risks. |
| [Technical direction](technical-direction.md) | Godot/C# architecture and development integration boundaries. |
| [Development setup](development.md) | Reference pins, command availability, local setup and tool/platform verification. |
| [Module map](../MODULES.md) | Actual source responsibilities and clearly labeled planned modules. |
| [Roadmap](roadmap.md) | The single authoritative Phase/Gate status and evidence links. |
| [Validation](validation.md) | Scenario definitions and evidence standards; not a second status tracker. |
| [Decision register](decisions-and-questions.md) | Accepted decisions, working assumptions and unresolved choices. |
| [Decision records](decisions/README.md) | Conventions for detailed durable decision rationale. |
| [Glossary](glossary.md) | Shared product and workflow terms. |
| [Workflow](workflow.md) | Documentation, planning, implementation, validation and closeout process. |
| [Phase runner](phase-runner.md) | Exact executable prompt grammar and runner behavior. |
| [Planning](planning/README.md) | Phase-level designs and scoped implementation proposals. |
| [Task stacks](tasks/README.md) | Assessments, plans, executable prompts and evidence locations. |
| [Known issues](../known-issues.md) / [feature ideas](../feature-ideas.md) | Observed defects / proposed additions. |

## First task

[P0 foundation design](planning/p0/foundation.md) and [p0 execution stack](tasks/p0/README.md) establish the 3D Godot .NET project and Godot Editor MCP integration. The stack is authored, not executed. The [repository-bootstrap record](tasks/repository-bootstrap/qualification.md) covers only this starter and runner port.

## Approved core-controls design

The [completed worksheet](planning/p1/swim-controls-worksheet.md) was accepted in the final documentation review on 2026-11-09. Use the [swimming contract](systems/swimming-and-camera.md) for the design and [validation V01–V02](validation.md#core-movement-acceptance) for the seven accepted playtest criteria. Actual native acceptance of the implemented core is still required before Movement abilities.

## Status language

- **Confirmed:** accepted by the owner in the project conversation.
- **Working design:** a proposed mechanism used to make the next step concrete; it can change without rewriting confirmed goals.
- **Implemented:** present in source, with the actual boundary described.
- **Observed/qualified:** supported by identified source, scenario and tool/platform evidence.
- **Open:** a consequential decision remains unresolved.

Update the owner of a changed fact and link it from related documents. Do not copy implementation evidence into multiple status tables or convert authored plans into passed Gates.
