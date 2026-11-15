# C5 Wake — natural surface wake correction

**Status: W1–W3 authored / NOT EXECUTED / NOT QUALIFIED.** [Approved S26 design](../../planning/c5-wake.md). Manual Codex work only; package version remains unchanged. Do not advance historical P2/P4/P5 Gate status from documentation.

The existing shared `WaterReaction` produces submerged tail trails/bubbles and a pool of eight colored Pond `SurfaceRing` meshes. Change **only the surface presentation** to body-originated diverging wave fronts, curved historical trails and gentle tail/bow turbulence, with stage-appropriate boost. Preserve the underwater effects and shared normal Pond/Pike Encounter source ownership.

## Execution order

| Prompt | Responsibility | Evidence and stop |
| --- | --- | --- |
| [W1 — Contact and world-space wake paths](manual/W1-body-contact-and-wake-paths.txt) | Derive real model/water anchors; bounded world history; debug-only paths | T2 deterministic source/scene and debug native images; stop for review |
| [W2 — Natural wake rendering](manual/W2-natural-wake-rendering.txt) | Replace circles; fade/propagate curved soft fronts with restrained highlights | T2 plus native A/B and performance; stop for review |
| [W3 — Growth, boost and owner qualification](manual/W3-growth-boost-native-qualification.txt) | Tune fry/adult stages, boost, water height and lifecycle; confirm Pond/Ocean isolation | T2 and real native T3; leave Pike Encounter running for owner playthrough |

[Assessment](prompt-assessment.md) · [Implementation plan](implementation-plan.md) · [V43](../../validation.md)

**Manual-only:** These `W*.txt` briefs are not Fish phase-runner prompts. Do not call `npm run fish:phase -- c5-wake` or `npm run fish:phase:validate -- c5-wake`; they intentionally do not follow the runner grammar. Do not commit/push from within manual prompts or bump the version. Each task stops at its explicit owner review boundary.

## Preflight and evidence

Read AGENTS.md, BOOT.md and docs/workflow.md; inspect current branch/HEAD and dirty working tree. Local dynamic-waterline corrections may be newer than the GitHub copy: review and preserve them. Confirm the Fish Godot project identity before native editor work and inspect exact `./tools/dev` command composition before running checks.

Create `qualification/` evidence **only after a real task runs**. Record source SHA and dirty identity, Godot/GPU/render backend, world/seed, stage/body dimensions, sampled wave phase/height, reproducible motion routes, screenshots/sequences, measured cost, PASS/FAIL/NOT OBSERVED and owner judgment. Historical failures and closeouts are not overwritten.
