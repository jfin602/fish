# P1 — Settle core swimming

**State: P1 shell and P2 acquisition implemented by direct execution through root version `0.1.2`; A local populated-world prototype exists; P3/P4 qualification and native observation remain pending.** The controls design is approved. Later stack tasks remain unexecuted. The planned `0.1.0` Phase-entry checkpoint is absent from Git history; direct execution has not fabricated it. This stack must implement, tune and qualify swimming with the revised ocean asset set and fullscreen presentation before Phase 1 can pass.

## Scope and finish line

Deliver natural, nimble, easy-to-control swimming in mouse-only mode and keyboard fallback, with curved turning, pitch-based depth travel, quick acceleration, gradual slowing, the approved follow camera, and visible fish motion. The revised starter slots are Quaternius `Fish3` / `ClownFish` from the owner-provided Animated Fish Pack, Rock Face 01, Coast Sand 01 and Smithsonian *Heliopora coerulea*. The low-poly fish is a P1 test choice, not the final art/species decision. Establish fullscreen and windowed fullscreen in the first implementation task and use them throughout development.

By the end, the actual swimming dynamics must be settled: final bindings and tuning recorded, all seven owner-approved checks observed in both desktop modes, display/asset integration qualified, and the owner accepting the implemented feel. A prototype with unfinished core tuning cannot close P1. [Phase design](../../planning/p1/core-swimming.md), [swimming contract](../../systems/swimming-and-camera.md), [assets](../../art-and-assets.md), [validation](../../validation.md), and [G1.1–G1.4](../../roadmap.md#p1--swim-through-a-habitat) own the contracts.

Movement abilities is the selected follow-up after accepted core swimming. Controller, free look, first person, survival systems, growth, later asset expansion and production water are outside this stack. Local display/input preferences and a minimal pause/settings interface are included.

## Entry prerequisites and version plan

The inspected published source is `5dc1c7c73b4f1f0b61579a76ea7e8cce86303613`, with root version **`0.0.0`**, no `game/` and no `tools/dev`. The owner reports a local demo. Inspect and preserve the actual checkout and reconcile P0's implementation and native evidence before execution. These authoring changes do not update the root version or invent completed P0 work.

After actual P0 readiness is established, deliberately prepare the **`0.1.0`** baseline before invoking the runner. P1 cannot make that baseline change from inside its first child prompt because preflight happens first. Follow [entry and resume](manual/entry-and-resume.md), including dirty-work review and the exact manual-checkpoint procedure.

| Order | Prompt | Assigned version | Validation / execution boundary |
| --- | --- | --- | --- |
| P1 | [Fullscreen play shell](P1-fullscreen-play-shell.txt) | `0.1.1` | T1 direct execution recorded in [implementation evidence](qualification/implementation.md); native P5 still pending. |
| P2 | [Starter asset acquisition](P2-starter-asset-acquisition.txt) | `0.1.2` | T1 direct acquisition passed with the owner-provided Quaternius pack and three environment sources; see implementation evidence. |
| P3 | [Godot asset preparation](P3-godot-asset-preparation.txt) | `0.1.3` | Local import/loader prototype added; full preparation and qualification pending. |
| P4 | [Ocean movement course](P4-ocean-movement-course.txt) | `0.1.4` | Decorative population added; authored course/collision and T2 qualification pending. |
| P5 | [Native display and assets](P5-native-display-and-assets.txt) | `0.1.5` | T3; mandatory native/manual handoff. |
| P6 | [Swim motor and keyboard](P6-swim-motor-and-keyboard.txt) | `0.1.6` | T1; NOT EXECUTED. |
| P7 | [Mouse mode and input lifecycle](P7-mouse-mode-and-input-lifecycle.txt) | `0.1.7` | T2; NOT EXECUTED. |
| P8 | [Follow camera](P8-follow-camera.txt) | `0.1.8` | T1; NOT EXECUTED. |
| P9 | [Fish body motion](P9-fish-body-motion.txt) | `0.1.9` | T1; NOT EXECUTED. |
| P10 | [Owner tuning and dynamics lock](P10-owner-tuning-and-dynamics-lock.txt) | `0.1.10` | T3; mandatory owner/native handoff and iterative acceptance. |
| P11 | [Swimming closeout](P11-swimming-closeout.txt) | `0.1.11` | Manual final evidence review; leave changes reviewable. |

The numbered prompts are tasks within roadmap Phase 1. All are unexecuted at authoring. P5 checks initial asset/display integration using the current provisional controls; P10 settles the completed control modes and feel. P11 is the only final closeout.

## Execution and handoffs

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
| `qualification/swimming-acceptance.md` | P10 course, mode/display matrix, tuning iterations, actual owner feedback and final seven-criterion results. |
| `qualification/accepted-tuning.md` | Final reproducible bindings/model/parameter/camera/animation profile bound to source, config and asset identity. |
| `closeout.md` | One final mapping of G1.1–G1.4 to source-bound evidence and the progression decision. |

These records are not prefilled as observations. Retain required evidence durably and exclude restricted asset payloads and local secrets. Document failures and subsequent repairs. Screenshots alone do not demonstrate working inputs or comfortable movement; the owner's judgment must come from actual play.

## Completion

Every P1 Gate requires its actual evidence. If the owner still finds turning, slowing, depth control, camera or body motion unfinished, continue the scoped tuning/repair work and keep P1 incomplete. On success, the final docs identify the accepted dynamics and next Movement abilities scope. They do not treat controller support or survival mechanics as already implemented.
