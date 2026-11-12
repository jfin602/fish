# R1 Forward+ benchmarks

Native 1920×1200, VSync disabled, uncapped, AMD Radeon 880M, fixed noon Pond Overcast. Each fixed row has 90 warmed-up frames. Frame time uses the elapsed presentation frame interval; this is not GPU-only time.

| View | Before median / p95 (ms) | After median / p95 (ms) | Draws before / after | Primitives before / after |
| --- | ---: | ---: | ---: | ---: |
| A1_spawn | 3.93 / 5.43 | 4.05 / 6.05 | 179 / 179 | 127,302 / 127,302 |
| A2_near | 4.05 / 6.17 | 4.06 / 5.20 | 206 / 206 | 129,978 / 129,978 |
| A3_log | 3.85 / 5.98 | 3.80 / 5.56 | 210 / 210 | 130,664 / 130,664 |
| A6_middle | 3.81 / 5.94 | 3.76 / 5.43 | 143 / 143 | 125,284 / 125,284 |
| A7_deep | 3.57 / 4.67 | 3.82 / 5.79 | 146 / 146 | 127,176 / 127,176 |
| A10_air | 5.29 / 9.15 | 5.31 / 6.73 | 205 / 205 | 126,522 / 126,522 |

Eight-second normal-control underwater route: **4.74 / 6.09 ms before → 4.73 / 6.16 ms after**; 202/202 median draws, 182,240/182,240 median primitives, 1480/1479 sampled frames. Both runs ended at (-10.63327, 7.79, -0.000227) and (-10.63327, 7.79, -0.000227) respectively.

Same candidate at A2 with local turbidity on/off: 4.39 / 5.61 ms vs 4.35 / 5.62 ms (median / p95), with 206 draws and 129,978 primitives in both. The 0.04 ms median difference is too small to establish a repeatable local-volume cost.

No GPU-only timestamp was available from this native probe. R8 texture payload remains 64×16×64 = 64 KiB, with one FogVolume and no new render pass. These timings include normal run-to-run noise; they do not show a material regression or a proven GPU saving.
