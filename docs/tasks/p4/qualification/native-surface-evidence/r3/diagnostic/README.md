# R3 Pond surface — rendered shader diagnostic

**Result: diagnostic shader rendering passed; native Fish qualification remains open.** This fixture renders the actual candidate shaders and tracked Pond FAF GLBs in a static test scene. Its bank boxes, yellow marker pole and submerged red cube are diagnostic geometry. It does not load the normal baked Pond or run Fish's .NET gameplay.

## What was observed

The final candidate produces recognizable, distorted reflections of the above-water marker, bank logs, reeds and stumps. Turning off scene reflections removes those silhouettes while retaining the sky and ripples. The close view retains a readable Bluegill and submerged red cube. The final Pond sky removes the old repeated vertical bands in this fixture.

| Fixed comparison | Evidence |
| --- | --- |
| Historical surface and sky | [Baseline above](captures/baseline-above.png) |
| Final candidate | [Above](captures/candidate-above.png), [grazing](captures/candidate-grazing.png), [fish](captures/candidate-fish.png) |
| Sky reflection without scene reflection | [Sky only](captures/sky-only-above.png) |
| Reflection disabled | [Reflection off](captures/reflection-off-above.png) |
| Old surface with new Pond sky | [Surface baseline](captures/surface-baseline-above.png) |

### Observed defect and focused repair

The first candidate with 4 binary intersection refinements produced conspicuous horizontal gaps in broad stump reflections at grazing angles. Increasing refinements to 6 repaired the nearest stumps but left a middle stump striped. Increasing to 8 removed the observed broad-stump combing. Coarse samples remained 24; thickness, scene, camera, normals and lighting stayed the same.

Compare [4 refinements](repair/refine4-grazing.png), [6 refinements](repair/refine6-grazing.png) and [8 refinements](repair/refine8-grazing.png). [The repair record](refinement-diagnostic.json) preserves the source hashes and controlled change. The final source uses 8 refinements, with at most **34 reflection depth reads** including the initial and final samples. Extra refinement reads occur only after a crossing is found; owner GPU cost has not been measured here.

### Underwater control

Every below-water view sets sky `air_mix=0` and the same diagnostic fog parameters. The final candidate, sky-only and reflection-off views are pixel-identical to the **old surface under the same new Pond sky**: maximum channel delta 0, changed pixels 0. The full historical baseline uses a different sky path and differs by at most 2/255 channel levels; it is not used to claim exact surface equivalence.

[Metadata](metadata.json) owns exact shader and image hashes, parameters, camera poses and pixel results. All five final run logs contain no engine errors or warnings. The original broad-stump failure is retained above rather than erased by the repair.

## Runtime and limits

Observed: Godot `4.6.3.stable.official.7d41c59c4` standard build; Forward+; Vulkan 1.4.305; Mesa `llvmpipe (LLVM 19.1.7, 256 bits)` software device; 960 × 540 viewport. The fixed shader time is 2.5. Bluegill uses the same `1 / 0.309825` normalization as its current adult catalog. Shadows are disabled in the fixture; all compared modes share that lighting setup.

No Fish or system tool pins changed. The repository's Godot 4.7.2 .NET and SDK 10.0.403 were unavailable in this execution environment. The scratch renderer used extracted Debian Mesa/Xvfb packages, which are intentionally not included in the repository evidence.

This establishes shader compilation and actual rendered behavior on the stated diagnostic engine. It does not establish reference-image parity, temporal stability during swimming, normal Pond lifecycle, waterline camera transitions, input/motor invariance, native visual acceptance, or Radeon 880M performance. Thin geometry can still produce missed screen-space details; off-screen scenery cannot be reflected by this pass. P4/P12 status is unchanged.

## Reproduce without modifying Fish

`prepare-diagnostic.py` copies the fixture and exact current shader sources into a new scratch project outside the Fish checkout. It verifies source hashes and obtains the two historical comparison shaders from Git revision `cc8cbb99de16f3b6138a046272721d986274a058`. It does not install tools or modify the source checkout.

From the Fish repository root:

```bash
python docs/tasks/p4/qualification/native-surface-evidence/r3/diagnostic/prepare-diagnostic.py --repo .
```

Optionally pass `--out` with an empty directory outside the checkout. The helper prints the `FISH_DIAGNOSTIC_GAME_ROOT` environment setting and five complete Godot commands. Run those commands with a graphical Forward+ capable Godot installation; prefer the project's pinned native engine when available. The fixture imports GLBs directly with `GLTFDocument`, without launching the Fish .NET project.

Each mode captures grazing, above, fish and below views into its scratch `captures/` directory, then exits:

| Mode | Surface | Sky | Reflection control |
| --- | --- | --- | --- |
| `baseline` | Historical | Historical | Historical behavior |
| `candidate` | Final Pond | New Pond | Scene and sky enabled |
| `sky-only` | Final Pond | New Pond | Scene contribution disabled |
| `reflection-off` | Final Pond | New Pond | Reflection strength zero |
| `surface-baseline` | Historical | New Pond | Historical surface under identical new sky |

The fixture source, project file and setup helper are retained here; shader snapshots, duplicated GLBs, runtime packages and generated import caches are excluded. The current shaders remain in `game/shaders/`, and GLBs remain in their existing asset directory. Historical log capture-path prefixes were normalized to this evidence's relative layout; engine output is otherwise unchanged. Newly captured final modes print relative paths directly.
