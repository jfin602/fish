# Correction 2 P4 closeout — OWNER GREEN

Date: 2026-11-10. Root version `0.2.6`. Accepted/pushed lighting baseline: `d292aebd` (`lighting closeout owner green`).

The first native water/light candidate was promising but the owner requested a stronger moving surface/shoreline read, moving ground caustics, more caustic-aligned ribbon shafts, and a GFX panel with saved lighting presets. Those refinements were implemented and the earlier closeout correctly remained NOT GREEN while the revised look and its measured uncapped cost awaited the owner's decision.

The owner has now completed that decision, stated that the first correction stack is complete, and pushed the lighting updates. **Correction 2 is OWNER GREEN for its scoped progression target.** Preserve [implementation evidence](qualification/implementation.md) and [native evidence](qualification/native-water-light.md), including the uncapped performance measurements and the fact that they exceeded the earlier ~10% investigation guardrail. Owner acceptance closes this correction; it does **not** establish a final production performance budget.

| Correction checkpoint | Final disposition |
| --- | --- |
| Overhead surface / bright source above | OWNER ACCEPTED for progression |
| Organic moving caustics | OWNER ACCEPTED for progression |
| Caustic-aligned sun shafts | OWNER ACCEPTED for progression |
| Underwater color/readability and stable camera | OWNER ACCEPTED for progression |
| GFX panel and saved lighting presets | IMPLEMENTED / retained |
| Representative native performance | MEASURED; larger uncapped cost retained as known optimization evidence and accepted for progression |

This correction does **not** mark P2, G2.1–G2.3 or V18–V20 passed by itself. The owner subsequently identified missing depth-of-vision/water-column behavior and selected [c2-depth](../c2-depth/README.md) as the next bounded P2 correction. The [roadmap](../../roadmap.md) remains the Gate authority.
