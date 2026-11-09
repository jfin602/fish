# Validation and evidence

This document defines scenarios. [The roadmap](roadmap.md) alone owns current Phase/Gate results. All game scenarios below are planned and unqualified in the starter; [repository-bootstrap evidence](tasks/repository-bootstrap/qualification.md) covers documentation and runner checks only.

## Evidence standard

For each claimed result, record the source revision and relevant dirty-state identity, platform/tool versions, scenario or seed, actual actions, expected and observed outcomes, artifacts, and limitations. A planned check is **NOT OBSERVED**. Use **PASS** and **FAIL** only for executed checks; **BLOCKED** identifies a specific missing dependency.

Headless compilation/smoke, editor inspection, runtime state, screenshots and live input prove different things. A static screenshot alone does not prove that input works; a headless state change does not prove camera readability. Use the smallest combination supporting the Gate. Preserve failures alongside later repairs.

## Foundation scenarios

| ID | Required observation | Gates |
| --- | --- | --- |
| B01 | Inspect actual .NET engine/SDK and coherent pins; import/build the actual Fish main scene; execute C# assertions within a bounded smoke; fail on missing marker, script errors or timeout. | G0.1 |
| B02 | Invoke CLI outside repository CWD; inspect real saved scene; create/change/save/reload/assert/remove a disposable scene; missing scene fails; no lingering fixture/cache is tracked. | G0.1 |
| B03 | Verify selected MCP versions, ignored local config, Fish bridge isolation and actual live peer path; inspect and save/reload a disposable real scene; retain Space Factory settings. | G0.2 |
| B04 | Open editor, play the real 3D scene, show horizontal/vertical input and release, inspect live state, capture running-game pixels, check output and close cleanly. | G0.3 |

P0 can record configuration success before a client reload, but B03/B04 remain unobserved until real tools and the actual Fish editor are connected. A TCP listener or addon file on disk is not a substitute.

## Gameplay scenarios

| ID | Scenario and required result | Gates |
| --- | --- | --- |
| V01 | Swim in horizontal and vertical directions; turn, release input and meet boundaries/cover; equivalent simulation conditions are independent of render/view changes. | G1.1 |
| V02 | Follow fish through shelter and open space; inspect depth/heading, occlusion, focus and viewport readability in the actual game. | G1.2 |
| V03 | Consume food/prey, repeat callbacks or competing contacts, reject ineligible prey, and account for one removal/reward per entity. | G2.1 |
| V04 | Cross growth thresholds while near geometry; verify continuous identity, visible/body changes, usable shelter changes and a food-chain reversal. | G2.1 |
| V05 | Observe a predator's detection/attack, a legitimate escape and a lethal encounter; each event's cause agrees with authoritative state. | G2.2 |
| V06 | Save during ordinary play and active danger; close/reopen; compare run ID, resources, consumed prey, position, time and pursuit state. Advance wall-clock time while closed without simulation progress. | G3.1 |
| V07 | Record terminal death, close/reopen and use supported recovery; dead run remains unavailable for Continue. Start new life with fresh identity and preserved history. | G3.2 |
| V08 | Inject interruption at each material death/archive persistence boundary and retry finalization; converge to one terminal outcome and one completed-life record with no resurrection. | G3.2 |
| V09 | Exercise unreadable/unsupported/partial active save and archive separately; report actual storage failure, preserve recoverable data, and do not invent an in-world death. | G3.1, G3.2 |
| V10 | Produce accounts for a brief life, eventful life and incomplete optional metadata; trace every claim to events/final state and keep supported species versus individual identity distinct. | G4.1 |
| V11 | Skip death presentation; open Life History from main menu; read same complete account, return, start another life, restart application and revisit without count/text changes. | G4.1, G4.2 |
| V12 | Use empty and representative large archives; reach every saved story, preserve navigation and prevent silent truncation or fictional filler entries. | G4.2 |
| V13 | Toggle first/third person at different sizes and near cover; state/identity/time remain unchanged by viewpoint and controls/readability remain usable. | G5.1 |
| V14 | Play feeding, escape and growth; suspend/resume across sessions; die; read true recap; revisit archive and begin a new hatchling with prior story intact. | G5.2 |
| V15 | Build/export/run each explicitly claimed desktop target; verify the complete named scope in that target environment and record configuration-only targets separately. | G5.2 |

These IDs are scenario references, not installed test commands. Write executable checks when the actual source exists and a concrete risk merits them. Avoid large suites that repeat the implementation.

## Requirement coverage

| Confirmed requirement | Planned evidence |
| --- | --- |
| C01–C04: 3D fish, growth/feeding/predation and camera choice | B04; V01–V05; V13. |
| C05–C07: one life across sessions, permanent death, no offline simulation | V06–V09; V14. |
| C08–C10: brief factual recounting and all stories in main-menu history | V08; V10–V12; V14. |
| C11–C13: reference workflow, Godot/MCP starter and honest setup boundary | Starter content/link/diff and runner checks; B01–B04 after p0. |

IDs refer to [the decision register](decisions-and-questions.md). A coverage mapping is not a passed result.

## Check selection

Use T1 content/link/diff review for documentation, the existing tests for runner changes, and focused C#/scene/rule checks for implementation. Use T2 for shared time, growth/body/camera, and persistence/event boundaries. Use T3 for native controls, threat readability, full life flow and platform claims. Inspect wrapper composition to avoid rebuilding the same unchanged candidate several times.

Automate difficult-to-observe invariants: one reward per consumed entity, no offline advancement, terminal recovery, archive identity, and narrative provenance. Observe actual play for movement feel, camera, fair warning, menu focus, recap pacing and archive readability. Extend validation only for a concrete unresolved risk or required Gate.

## Documentation review

Verify relative links and anchors, current-versus-planned wording, decision provenance, task/model/version grammar and source consistency. Ensure gameplay completion is never inferred from documentation, ported tests, package version or a prior project's evidence. Review the actual diff for unrelated changes and machine-specific secrets/paths before publication.
