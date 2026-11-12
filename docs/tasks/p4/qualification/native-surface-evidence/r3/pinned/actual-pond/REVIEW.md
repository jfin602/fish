# Actual Pond fixed-pose render review

15 captures completed, exit 0. 4.7.2-stable (official) with .NET capability, Vulkan/Forward+, llvmpipe (LLVM 19.1.7), 960 × 540. Each has 30 measured frames after 30 warmup frames. Source e93ecf5e6bc2d00d6738fce9766d98f17585a4ca plus only recorded probe capability guard repair.

## Image review

- Low bank/oblique bank show coherent inverted stump/root reflections that disappear in sky-only controls. Broad stump combing repaired in the isolated fixture is not visible in these current actual-Pond stills.
- Oblique water remains muted olive; low bank remains pale under the open, overcast Pond sky. These are improvements in reflection behavior, not parity with the dense tropical canopy references.
- The 0.50 m Bluegill remains readable from above; the 4 m fish is strongly attenuated. No emissive/sharp deep-fish appearance was seen in these poses.
- Underwater view retains its green surface and Bluegill silhouette. Whole-image deltas are listed below; they should not be generalized beyond fixed poses.
- Thin reeds and screen-edge/offscreen reflection gaps remain known SSR limitations. Stills do not establish temporal stability, motion feel, gameplay performance, or owner acceptance. This fixed-pose run does not observe motion. No additional rendering was performed after these stills; the Ocean check then ran separately.

## Frame intervals

Software-render diagnostic only; these values include scheduling/presentation and are not isolated GPU cost.

| Pose | Mode | Median ms | p95 ms | Draw calls |
|---|---|---:|---:|---:|
| low_bank | legacy | 125.320 | 140.862 | 71 |
| low_bank | candidate | 132.907 | 183.214 | 71 |
| low_bank | sky_only | 128.370 | 145.411 | 71 |
| oblique_bank | legacy | 130.736 | 163.905 | 74 |
| oblique_bank | candidate | 158.935 | 195.751 | 74 |
| oblique_bank | sky_only | 130.560 | 150.839 | 74 |
| fish_050cm | legacy | 124.839 | 148.258 | 83 |
| fish_050cm | candidate | 132.978 | 177.493 | 83 |
| fish_050cm | sky_only | 126.304 | 140.497 | 83 |
| fish_400cm | legacy | 123.487 | 142.409 | 83 |
| fish_400cm | candidate | 137.141 | 197.892 | 83 |
| fish_400cm | sky_only | 133.904 | 176.637 | 83 |
| underwater | legacy | 178.805 | 233.873 | 187 |
| underwater | candidate | 169.452 | 212.723 | 187 |
| underwater | sky_only | 162.544 | 175.615 | 187 |

## Underwater same-sky pixel checks

| Pair | Changed pixels | Maximum 8-bit channel delta | Mean absolute channel delta |
|---|---:|---:|---:|
| legacy_vs_candidate | 962 / 518400 | 1 | 0.00046393 |
| candidate_vs_sky_only | 4 / 518400 | 1 | 0.00000193 |
| legacy_vs_sky_only | 958 / 518400 | 1 | 0.00046200 |

Raw runtime log scan: 0 error lines and 0 warning lines; failures array empty. Full log retained. Source, probe, shader, report and image SHA-256 values are in [analysis.json](analysis.json).

## Limits and initial failure

- Initial run 1 failed before scene load because the probe assumed OS.has_feature("mono"). Scratch runtime queries show all tested mono/dotnet/C#/cs OS flags false in both binaries, whereas ClassDB.class_exists("CSharpScript") is true in pinned Mono and false in standard 4.6.3. The exact one-condition repair is preserved in [probe-capability-fix.diff](probe-capability-fix.diff); the successful 15-capture run uses it.
- This run compares old water material under the current Pond sky against candidate and sky-only control. It is not a complete checkpoint checkout A/B.
- Original approved ambientCG source selection was reconstructed from official versioned 2K-JPG packages; original historical byte parity is unverified. No substitutions were made.
- P4/P12 remain NOT GREEN. Owner GPU, movement, camera feel, world-switch acceptance and reference-match judgment remain outstanding.
