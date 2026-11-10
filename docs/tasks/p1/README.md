# P1 — Settle core swimming

**State: Phase 1 COMPLETE by direct owner acceptance on 2026-11-09; G1.1–G1.4 PASS within recorded evidence limits.** [Closeout](closeout.md), [swimming acceptance](qualification/swimming-acceptance.md), and [accepted tuning](qualification/accepted-tuning.md) define the final source-bound baseline. P1–P9 were directly implemented; c1 controls were then implemented and observed in a bounded native session. The authored P10 iterative session was superseded by explicit owner acceptance of the current post-c1 candidate, and P11 evidence review/reconciliation was performed directly. Neither prompt is represented as an executed runner task. No `0.1.10` or `0.1.11` checkpoint was fabricated; root `0.2.0` is the deliberate P2 entry version.

## Scope and finish line

Deliver natural, nimble, easy-to-control swimming in mouse-only mode and keyboard fallback, with curved turning, pitch-based depth travel, quick acceleration, gradual slowing, the approved follow camera, and visible fish motion. The revised starter slots are Quaternius `Fish3` / `ClownFish` from the owner-provided Animated Fish Pack, Rock Face 01, Coast Sand 01 and Smithsonian *Heliopora coerulea*. The low-poly fish is a P1 test choice, not the final art/species decision. Establish fullscreen and windowed fullscreen in the first implementation task and use them throughout development.

The owner accepted the current core as sufficient for progression and deferred additional control/movement refinement until later gameplay work informs it. The authored seven-by-two and four-cell matrices were not retrospectively filled; [the closeout](closeout.md) distinguishes observed native paths from source checks and the owner decision. [Phase design](../../planning/p1/core-swimming.md), [swimming contract](../../systems/swimming-and-camera.md), [assets](../../art-and-assets.md), [validation](../../validation.md), and [G1.1–G1.4](../../roadmap.md#p1--swim-through-a-habitat) retain the development context.

The owner later selected **P2 underwater world** as the next roadmap Phase after accepted core swimming. Movement abilities remain approved future work but are no longer the immediate follow-up. Controller, free look, first person, survival systems, growth and production-world rendering remain outside this P1 stack. Local display/input preferences and a minimal pause/settings interface are included.

## Historical entry prerequisites and version plan

At original P1 authoring, the inspected published source was `5dc1c7c73b4f1f0b61579a76ea7e8cce86303613`, root **`0.0.0`**, without `game/` or `tools/dev`. This paragraph preserves the old entry context; [closeout](closeout.md) describes the actual current source.

The authored runner plan called for a **`0.1.0`** baseline before invocation. Direct P1 execution instead began from the recorded P0 state and did not invent that checkpoint. [Entry and resume](manual/entry-and-resume.md) retains the original procedure as historical task material.

| Order | Prompt | Assigned version | Validation / execution boundary |
| --- | --- | --- | --- |
| P1 | [Fullscreen play shell](P1-fullscreen-play-shell.txt) | `0.1.1` | T1 direct execution recorded in [implementation evidence](qualification/implementation.md); native P5 still pending. |
| P2 | [Starter asset acquisition](P2-starter-asset-acquisition.txt) | `0.1.2` | T1 direct acquisition passed with the owner-provided Quaternius pack and three environment sources; see implementation evidence. |
| P3 | [Godot asset preparation](P3-godot-asset-preparation.txt) | `0.1.3` | T1 import/loader and negative readiness checks passed by direct execution; see implementation evidence. |
| P4 | [Ocean movement course](P4-ocean-movement-course.txt) | `0.1.4` | Saved course, four-source composition and T2 checks passed by direct execution; see implementation evidence. |
| P5 | [Native display and assets](P5-native-display-and-assets.txt) | `0.1.5` | T3 native Linux/X11 checkpoint passed for the provisional shell and four assets by direct execution; [evidence and captures](qualification/native-display-and-assets.md). Coordinator checkpoint remains separate. |
| P6 | [Swim motor and keyboard](P6-swim-motor-and-keyboard.txt) | `0.1.6` | T1 motor/keyboard and scene checks passed by direct execution; [evidence](qualification/implementation.md#p6--shared-swim-motor-and-keyboard-fallback-direct-execution-2026-11-09). Native feel remains P10. |
| P7 | [Mouse mode and input lifecycle](P7-mouse-mode-and-input-lifecycle.txt) | `0.1.7` | T2 saved-scene input/mode/pause checks passed by direct execution; native comfort remains P10. |
| P8 | [Follow camera](P8-follow-camera.txt) | `0.1.8` | T1 saved-scene follow, obstruction, pause and resize checks passed by direct execution; native comfort remains P10. |
| P9 | [Fish body motion](P9-fish-body-motion.txt) | `0.1.9` | T1 real-scene Fish3 swim/glide/turn pose, loop, pause and isolation checks passed by direct execution; native deformation/feel remains P10. |
| P10 | [Owner tuning and dynamics lock](P10-owner-tuning-and-dynamics-lock.txt) | `0.1.10` planned only | Iterative requirement superseded by direct acceptance of the post-c1 candidate; no prompt execution or checkpoint claimed. |
| P11 | [Swimming closeout](P11-swimming-closeout.txt) | `0.1.11` planned only | Evidence-review/reconciliation responsibilities completed directly in [closeout](closeout.md); no runner checkpoint claimed. |

The numbered prompts are preserved as authored task history. P1–P9 have direct-execution records; P10/P11 were not run through the stack. P5 checked initial asset/display integration, later c1 added presets/rebinding, and the owner then accepted the current candidate for progression. The single final P1 disposition is [closeout](closeout.md).

## Historical execution and handoffs

The following commands describe the authored P1 stack and its original runner workflow. P1 is now closed through direct execution and owner acceptance; the next executable stack is [P2](../p2/README.md), starting at root `0.2.0`. Do not rerun P10/P11 merely to manufacture old checkpoints.

Validate the authored stack from the repository root:

```bash
npm run fish:phase:validate -- p1
```

After the entry prerequisites and baseline transition:

```bash
npm run fish:phase -- p1
```

The runner performs pending ordinary prompts and pauses before P5, P10 and P11 because their exact metadata is `- Browser required: yes.`. This means a native Godot/owner observation boundary. Browser access alone does not control the game. `--closeout` does not bypass those boundaries.

For P5 and P10, complete the actual manual work, review its evidence and changes, then let the authorized coordinating session create the exact version checkpoint before resuming. Child prompts do not self-commit. P11 always leaves its final changes reviewable. Do not manufacture a checkpoint for missing evidence or owner acceptance. A direct interactive execution can use the same briefs; record that mode and reconcile history before a later runner resume.

P2's original named asset instructions are superseded by [decisions C30–C31](../../decisions-and-questions.md) and [the current manifest](../../assets/p1-starter-manifest.md). The owner supplied Quaternius's Animated Fish Pack and selected its `Fish3` / `ClownFish` for provisional player art, with other pack models for world population. The earlier BlendSwap/Blender downloads remain inventoried separately. Do not bypass account or license restrictions or rewrite active prompt files under a runner.

## Evidence created during execution

| Planned record | Purpose |
| --- | --- |
| `qualification/implementation.md` | Actual source/commands/results for ordinary implementation tasks, with focused failures and repairs. |
| `docs/assets/p1-starter-manifest.md` | Public-safe identity, provenance, license/credit and acquisition/preparation/import/observation status for each selected asset. |
| `game/assets/README.md` | Verified installation and resource mapping, including ignored restricted files and missing-asset diagnostics. |
| `qualification/native-display-and-assets.md` | P5 native display, focus, layout, asset appearance and scene evidence. |
| `qualification/swimming-acceptance.md` | Historical P10 observations and direct final owner acceptance, with unobserved matrix cells explicit. |
| `qualification/accepted-tuning.md` | Final reproducible bindings/model/parameter/camera/animation profile bound to source, config and asset identity. |
| `closeout.md` | One final mapping of G1.1–G1.4 to source-bound evidence and the progression decision. |

These records are not prefilled as observations. Retain required evidence durably and exclude restricted asset payloads and local secrets. Document failures and subsequent repairs. Screenshots alone do not demonstrate working inputs or comfortable movement; the owner's judgment must come from actual play.

## Completion

The [final closeout](closeout.md) records all four Gate results, actual supporting evidence and qualification limits. [P2 underwater world](../p2/README.md) is READY TO RUN and replaces the course's normal-play presentation while retaining it as a regression fixture. Movement abilities, controller support and survival mechanics remain future work.
