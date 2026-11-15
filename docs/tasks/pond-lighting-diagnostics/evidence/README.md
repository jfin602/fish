# Evidence index

All contact sheets are resized from native 1920×1200 Forward+ PNG captures. The selected paired JPGs retain native resolution. The compact manifests preserve scene, pose, lighting, timing, and performance values while replacing host-specific absolute image paths with repository-relative artifact paths. [Native log excerpts](native-log-excerpts.txt) include setup, collision, sample, and completion markers. Complete PNG sequences, manifests, and logs are retained under ignored `artifacts/pond-lighting-diagnostics/` in the local workspace.

| File | Purpose |
| --- | --- |
| [01_ground_material.png](01_ground_material.png) | Verified ground contact and paired shadows/fog/material isolates |
| [02_stationary.png](02_stationary.png) | Normal 0, 5, 10 s fixed pose |
| [03_camera.png](03_camera.png) | 0–4 m controlled camera move with shadows and depth fog toggled |
| [04_motion.png](04_motion.png), [ground-motion.gif](ground-motion.gif) | Normal-input swim and follow camera |
| [05_daynight.png](05_daynight.png) | Noon, 17:00, 18:30, midnight |
| [06_ablation.png](06_ablation.png) | Shadow, pool, caustic, volume, and depth-fog matrix |
| [07_material_components.png](07_material_components.png) | Static basin shader component isolates |
| [08_fog_stack.png](08_fog_stack.png) | Global volume, Pond turbidity, depth fog, combined minimal |
| [09_pool_time.png](09_pool_time.png) | Normal, frozen, and disabled sunlight pools over ten seconds |

See [the report](../report.md) for sample coordinates, quantified differences, native conditions, and limits.
