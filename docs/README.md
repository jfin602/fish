# Documentation index

Start at [AGENTS.md](../AGENTS.md) and [BOOT.md](../BOOT.md), then follow only the routes needed for the task. P0 and P1 are complete; P2 world qualification remains open at its owner/native checkpoint. [The roadmap](roadmap.md) owns exact Phase/Gate status.

## Ownership

| Document | Owns |
| --- | --- |
| [Vision](vision.md) | Player fantasy, pillars, and scope boundaries. |
| [Game design](game-design.md) | Overall life loop and relationship between systems. |
| [Swimming and camera](systems/swimming-and-camera.md) | Approved core controls, movement feel, camera/body presentation and later movement integration. |
| [Swim controls worksheet](planning/p1/swim-controls-worksheet.md) | Historical owner discussion/provenance for the ten P1 control questions. |
| [P1 core swimming plan](planning/p1/core-swimming.md) | Selected-asset/fullscreen delivery, implementation starting points, tuning loop and P1 finish line. |
| [Movement abilities](planning/p1/movement-abilities.md) | Approved future burst/precision/evasive-turn group; immediate sequencing superseded by P2 worldbuilding. |
| [P2 underwater-world plan](planning/p2/underwater-world.md) | Natural reef habitat, depth-structured traversal, renderer lock, water-column atmosphere and movement-reactive presentation. |
| [P3 FAF reef-world plan](planning/p3/reef-world-rebuild.md) | Rebuild/dress the accepted varying-depth world with the complete original FAF environment library, caves/tunnels, depth-based density, and the five-fish Character Select/session-return flow. |
| [P3 procedural world generation](planning/p3/procedural-world-generation.md) | Deterministic finite world compiler: irregular basin, true 3D geology, terrain-aware placement, baked candidate/Base World lifecycle and generation-time target. |
| [P4 freshwater world and selection](planning/p4/freshwater-world.md) | Approved future Ocean/Pond World Select, distinct visual habitats, habitat-compatible species and shared loading/FAF requirements. |
| [Underwater visibility and discovery](systems/underwater-visibility.md) | Owner-selected distance/depth extinction, progressive reef reveal, near-field readability, tuning controls, and pending native acceptance. |
| [Ecology and growth](systems/ecology-and-growth.md) | Food, prey, predator interaction, growth and habitat use. |
| [Survival and runs](systems/survival-and-runs.md) | One life across sessions, pause, permanent death, saving and recovery. |
| [Life stories and history](systems/life-stories-and-history.md) | Meaningful event capture, factual recap and durable main-menu archive. |
| [UX and presentation](ux-and-presentation.md) | Player entry points, underwater presentation, feedback, menus, recap/history navigation. |
| [Art and assets](art-and-assets.md) | Historical source shortlist, provenance, licenses, imports and P2 environment-asset boundaries. |
| [Fish Asset Factory](fish-asset-factory/README.md) | Original art pipeline, owner-approved [visual contract](fish-asset-factory/art-style.md), palette, construction standards, IDs, budgets, Blender/Godot handoff and visual review. |
| [Tutorial](tutorial.md) | Proposed first-life teaching sequence and readable early risks. |
| [Technical direction](technical-direction.md) | Godot/C# architecture, renderer decision boundary and development integration. |
| [Development setup](development.md) | Reference pins, command availability, local setup and tool/platform verification. |
| [Module map](../MODULES.md) | Actual source responsibilities and planned boundaries. |
| [Roadmap](roadmap.md) | Single authoritative Phase/Gate status and evidence links. |
| [Validation](validation.md) | Scenario definitions and evidence standards; not a second status tracker. |
| [Decision register](decisions-and-questions.md) | Accepted decisions, working assumptions and unresolved choices. |
| [Decision records](decisions/README.md) | Conventions for detailed durable decision rationale. |
| [Glossary](glossary.md) | Shared product and workflow terms. |
| [Workflow](workflow.md) | Documentation, planning, implementation, validation and closeout process. |
| [Phase runner](phase-runner.md) | Exact executable prompt grammar and runner behavior. |
| [Planning](planning/README.md) | Phase-level designs and scoped implementation proposals. |
| [Task stacks](tasks/README.md) | Assessments, plans, executable prompts and evidence locations. |
| [Known issues](../known-issues.md) / [feature ideas](../feature-ideas.md) | Observed defects / proposed additions. |

## Current sequence

P0 and P1 are complete. P2 is **owner-closed for progression / NOT GREEN**: G2.1/G2.2 pass while G2.3/V20 remains an accepted open gap. P3 is **IN PROGRESS at `0.3.6`**, not GREEN: the initial C3 generated Ocean candidate failed native composition and alternate-seed routes. Complete manual [M1–M4 C3 world-generation repairs](tasks/c3-world-gen/manual/README.md) sequentially with visual review, then re-qualify the world and revisit [C3 visibility](tasks/c3-visibility/README.md).

**P4 is owner-selected but not started:** [Freshwater Pond and World Select](planning/p4/freshwater-world.md) preserves the Ocean and its five marine FAF fish as one world; adds Pond as another, with strict species eligibility; and reuses the compiler/Forward+/controls. Future gameplay is now **P5 feeding/growth, P6 saves/death, P7 Life History, P8 integrated qualification**. The previously selected Movement abilities remain a later enhancement. Planning does not bump version or establish qualification.

## Status language

- **Confirmed:** accepted by the owner.
- **Working design:** a proposed mechanism used to make a selected task concrete.
- **Implemented:** present in source, with the actual boundary described.
- **Observed/qualified:** supported by identified source, scenario and tool/platform evidence.
- **Open:** a consequential decision remains unresolved.

Update the owner of a changed fact and link it from related documents. Do not convert authored plans into passed Gates.
