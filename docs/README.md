# Documentation index

Start at [AGENTS.md](../AGENTS.md) and [BOOT.md](../BOOT.md), then follow only the routes needed for the task. P0 and P1 are complete. P2–P5 are owner-closed for progression / NOT GREEN with explicit remaining native evidence. The current package baseline is `0.6.0` for P6 development; C5 M1–M7 implementation is included, while owner-native qualification remains open. [The roadmap](roadmap.md) owns exact Phase/Gate status.

## Ownership

| Document | Owns |
| --- | --- |
| [Vision](vision.md) | Player fantasy, pillars, and scope boundaries. |
| [Game design](game-design.md) | Overall life loop and relationship between systems. |
| [Swimming and camera](systems/swimming-and-camera.md) | Approved core controls, movement feel, camera/body presentation and later movement integration. |
| [Runtime audio](systems/audio.md) | Pike Encounter Pond bed, spatial swim/boost cues, selected takes, tuning and pending owner listening. |
| [Swim controls worksheet](planning/p1/swim-controls-worksheet.md) | Historical owner discussion/provenance for the ten P1 control questions. |
| [P1 core swimming plan](planning/p1/core-swimming.md) | Selected-asset/fullscreen delivery, implementation starting points, tuning loop and P1 finish line. |
| [Movement abilities](planning/p1/movement-abilities.md) | Approved future burst/precision/evasive-turn group; immediate sequencing superseded by P2 worldbuilding. |
| [P2 underwater-world plan](planning/p2/underwater-world.md) | Natural reef habitat, depth-structured traversal, renderer lock, water-column atmosphere and movement-reactive presentation. |
| [P3 FAF reef-world plan](planning/p3/reef-world-rebuild.md) | Rebuild/dress the accepted varying-depth world with the complete original FAF environment library, caves/tunnels, depth-based density, and the five-fish Character Select/session-return flow. |
| [P3 procedural world generation](planning/p3/procedural-world-generation.md) | Deterministic finite world compiler: irregular basin, true 3D geology, terrain-aware placement, baked candidate/Base World lifecycle and generation-time target. |
| [P4 freshwater world and selection](planning/p4/freshwater-world.md) | Historical Ocean/Pond design; [P4 closeout](tasks/p4/closeout.md) preserves implementation and native gaps. |
| [P5 historical entry](tasks/p5/entry.md) | Earlier `0.5.0` baseline/progression record; superseded by executed P5 closeout for present state. |
| [Underwater visibility and discovery](systems/underwater-visibility.md) | Owner-selected distance/depth extinction, progressive reef reveal, near-field readability, tuning controls, and pending native acceptance. |
| [Day/night and weather](systems/day-night-and-weather.md) | Shared environmental time/solar cycle contract; weather is a later modifier layered on the day/night baseline. |
| [Stealth and detection](systems/stealth-and-detection.md) | Owner-approved naturalistic stealth core; physical shelter, exposure, sensory detection, investigation, escape, stealth hunting and P4/P5 boundary. |
| [P5 executed stealth foundation and closeout](tasks/p5/closeout.md) | Historical Bluegill player, Bass prey and Pike fixture implementation; owner-closed / NOT GREEN with native test debt. |
| [C5 Wake — natural surface waves](planning/c5-wake.md) | Approved S26 design for body-originated, world-space Pond wakes; [W1/W2 and refinements](tasks/c5-wake/README.md) implemented, W3 owner-native qualification open. |
| [C5 Bass lifecycle and ecology design](planning/c5-post-p5-ecology-lifecycle.md) | Historical/ongoing Bass player, egg hatch, Bluegill schools, Pike targeting and blood correction with M1–M7 focused evidence; M8 native review open. |
| [P6 Living Pond design](planning/p6/living-pond.md) | **S27 approved / NOT STARTED**: feeding reticle and mouth heading, nursery predators, frogs, snakes, competing Bass, living food, spatial exploration and full Bass survival in normal Freshwater Pond; V44–V51. |
| [P6 development baseline](tasks/p6/baseline.md) | Root `0.6.0` checkpoint, focused validation and outstanding native qualification; no P6 gameplay or Gate pass. |
| [P6 task stack](tasks/p6/README.md) | [Assessment](tasks/p6/prompt-assessment.md), [implementation plan](tasks/p6/implementation-plan.md) and thirteen execution briefs authored and grammar-validated; gameplay not started. |
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

P0 and P1 are complete. P2 is **owner-closed for progression / NOT GREEN**: G2.3/V20 remains an accepted open gap. P3 is also **owner-closed for progression / NOT GREEN**, with corrected Ocean `ocean-m4-42` at completed `0.3.6`, documented visual/native/visibility gaps and [direct closeout](tasks/p3/closeout.md). Do not convert accepted gaps into PASS.

**P4 and P5 are owner-closed for progression / NOT GREEN.** [P4 closeout](tasks/p4/closeout.md) retains underwater clarity, route and surface/camera qualification limits; [P5 closeout](tasks/p5/closeout.md) retains G5.1–G5.3 native evidence debt from its historical `0.5.12` package. The third World Select encounter is implemented, while normal Ocean/Pond remain separate test worlds. [C5 Bass correction](tasks/c5-polish/README.md) has M1–M7 implementation in the `0.6.0` baseline, with M8 owner-native review open. [C5 Wake](tasks/c5-wake/README.md) has W1/W2 and refinements implemented, while W3/V43 native review remains open. **P6 Living Pond** is the next approved but NOT STARTED gameplay phase; former P6 saves/permanent death are **P7**, Life History **P8** and integrated qualification **P9**, all future work. Preserve old V06–V15 IDs with their new Gate mapping. The normal Pond remains a passive demo in current source; P6 predator integration is design, not shipped behavior.

## Status language

- **Confirmed:** accepted by the owner.
- **Working design:** a proposed mechanism used to make a selected task concrete.
- **Implemented:** present in source, with the actual boundary described.
- **Observed/qualified:** supported by identified source, scenario and tool/platform evidence.
- **Open:** a consequential decision remains unresolved.

Update the owner of a changed fact and link it from related documents. Do not convert authored plans into passed Gates.
