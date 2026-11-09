# P1 prompt assessment

## Request, source, and outcome

The owner requested `/docs-apply + /prompt-ass + /prompt-plan + /prompt-write p1`, directed P1 to use the selected assets, required swimming dynamics to be set by its end, and chose fullscreen development from the start with windowed fullscreen for testing.

Authoring source: `jfin602/fish` at `5dc1c7c73b4f1f0b61579a76ea7e8cce86303613`. Its ten-answer controls worksheet and seven playtest criteria are approved. The snapshot contains documentation and the Node runner, with root version `0.0.0`; `game/` and `tools/dev` are absent. The owner reports a local demo. Execution must inspect that checkout, preserve its work, and reconcile P0 evidence before choosing concrete source files. This assessment does not claim runtime inspection.

P1 finishes a comfortable, accepted swimming experience in mouse-only and keyboard modes, using the four starter assets in a compact ocean scene and the selected display modes. Implementation, native observations and owner acceptance are separate requirements. [Phase design](../../planning/p1/core-swimming.md), [P1 Gates](../../roadmap.md#p1--swim-through-a-habitat), and [the implementation plan](implementation-plan.md) provide the contracts and execution details.

## Decomposition and validation

| Prompt | Coherent outcome | Tier / manual boundary |
| --- | --- | --- |
| P1 | Fullscreen play shell, display choice, pause/settings and safe input focus boundary. | T1; code/configuration. |
| P2 | Actual acquisition and public-safe provenance for the four selected assets. | T1; account/file handoff only if a concrete dependency requires it. |
| P3 | Godot-ready selected resources, local installation path and import/rig/material mapping. | T1; import and resource checks. |
| P4 | Compact ocean movement course with deliberate scale, collisions and landmarks. | T2; scene/resource/collision integration. |
| P5 | Native display and selected-asset checkpoint before detailed movement work. | T3; mandatory interactive handoff. |
| P6 | One authoritative swimming motor and keyboard fallback. | T1; focused physics/input checks. |
| P7 | Mouse-only adapter, explicit mode selection and clean pause/refocus lifecycle. | T2; common-motor and input/UI integration. |
| P8 | Smooth third-person following, obstacle handling and steady horizon. | T1; focused camera logic; comfort still requires native play. |
| P9 | Selected-fish swim/glide motion and turn bend/lean independent of gameplay state. | T1; animation/state checks. |
| P10 | Owner-led tuning, repairs, final profile, and accepted swimming dynamics. | T3; mandatory native observation and actual owner judgment. |
| P11 | Single final evidence review and owning-document reconciliation. | T3 evidence review plus T1 docs; manual final closeout. |

Ordinary implementation tasks aim for roughly eight minutes, with a ten-minute soft ceiling and fifteen-minute ordinary budget. Acquisition/conversion and native owner tuning are dedicated integration work that can take longer because access, rig/material repair and subjective iteration cannot be certified by a fixed timer. Preserve partial work and exact remaining steps; split an unexpectedly large independent repair before execution resumes. Time targets never allow skipped evidence.

## Dependencies and preserved behavior

- Actual P0 foundation, its coherent .NET/Godot pins, developer CLI and isolated MCP connection must be available and evidenced. Missing foundation work routes to the existing p0 stack; do not recreate a working demo or fabricate checkpoint history.
- Inspect the actual command composition before using `tools/dev`; the published source provides no implementation to infer from.
- Retain the approved control model, one authoritative motor, physics/presentation separation, no underwater gravity fall, clear collision boundaries, and a stable horizon.
- Preserve the current renderer, Space Factory configuration, unrelated user work, original worksheet answers, and all later gameplay requirements.
- Retain the asset licensing boundary: restricted raw and converted files stay out of public Git, including embedded resources and accidental runner staging.
- Enter the runner only after an actual `0.1.0` baseline transition. Authoring these files does not change root version or qualify P0/P1.

## Material risks and responses

| Risk | Planned response |
| --- | --- |
| Missing or different local foundation | Inspect actual checkout and P0 evidence at entry; adapt paths to source, preserve work, identify missing prerequisite precisely. |
| Storefront access or incompatible asset archive | Acquire only the four selected assets; retain inventory and exact preparation state; surface a concrete missing package/access/conversion requirement. No silent substitution. |
| Restricted data reaches public Git | Ignore before copy/conversion; audit tracked scenes/resources for embedded vendor content and check staging before every affected checkpoint. |
| Listing claims mistaken for working assets | Inspect real skeletons/clips/materials, run the real imported scene, and keep acquired/prepared/imported/observed states distinct. |
| Divergent desktop controls | Two adapters drive one motor; explicit switching clears pending input; each mode has independent native evidence and complete menu access. |
| Excessive lag or unintended drift | Tune propulsion, heading response, velocity alignment, camera smoothing and animation separately, then evaluate their combined feel. |
| Animation moves physics twice | Visual child transforms only; remove or contain imported root motion, and verify collision-root state independently. |
| Fullscreen causes capture or layout regressions | Implement display shell first; observe actual native modes early; recheck focus, transitions and both input modes on the final candidate. |
| Superficial “finished” controls | Require all seven checks, a saved final profile, several minutes in each input mode and actual owner acceptance. Unfinished core feel blocks P1. |
| Runner advances despite failed prose | Withhold assigned version if required checks fail; record the gap and retain partial work. The runner does not infer a failed Gate from prose. |

## Done boundary and deferrals

P1 passes only with evidence for G1.1–G1.4 on the accepted source/configuration/assets. P5 is an early integration checkpoint, not final movement approval. P10 must tune and repair through actual acceptance; P11 may reconcile unchanged accepted evidence without repeating play solely for documentation edits. Missing or rejected owner acceptance prevents a successful phase closeout.

Movement abilities follows the accepted P1 core. Controller, free look, first person, feeding/predators, growth, survival persistence, stories, later animals/vegetation and production water remain outside this stack. A small local settings file is allowed for display/input preferences and is separate from living-save functionality.

## Authoring verification

Validate the completed direct prompt files with `npm run fish:phase:validate -- p1`, then inspect relative links, numbered versions, scope/diffs and the preservation of unobserved Gate results. This proves the authored stack's grammar and consistency only. Native and implementation evidence will be created during execution, not prefilled here.
