# Correction 2 P4 closeout — NOT GREEN

Date: 2026-11-10. Root version `0.2.6`; `main` at `a2e7d46` plus uncommitted correction edits. No self-commit or push.

The owner played the first native water/light candidate and said it looked good, then requested more detailed moving waves and shoreline contact, shifting ground caustics, more caustic-aligned ribbon shafts, and a GFX panel with saved lighting presets. These refinements are implemented in the uncommitted revised candidate. [Implementation evidence](qualification/implementation.md#correction-2-p4--native-candidate-repair-after-owner-play) explains the source changes; [native evidence](qualification/native-water-light.md) records the actual window, UI and same-revision A/B observations.

**Decision:** keep Correction 2 **NOT GREEN**. The owner has not judged the revised surface, shaft, caustic, readability or motion-comfort result. The uncapped effects-on cost exceeds the ~10% guardrail in both sampled views despite the low-detail mote repair; the 60 FPS capped run does not settle headroom. A larger known cost requires an explicit owner decision with these numbers. The smallest remaining visual target is a revised open/tunnel/upper route playtest; the smallest performance target is further cost reduction or an explicit owner acceptance of the recorded cost. Preserve this record if a later repair changes the outcome.

| Owner checkpoint | State |
| --- | --- |
| Overhead surface and above-water sun | NOT OBSERVED on revised build |
| Volumetric/light-shaft look | NOT OBSERVED on revised build |
| Organic non-grid caustics | NOT OBSERVED on revised build |
| Underwater color/depth/readability | NOT OBSERVED as a full revised-route judgment |
| Motion comfort and stable camera | NOT OBSERVED on revised swim route |
| Representative native performance | NOT OBSERVED as owner acceptance; measured uncapped cost remains open |

The renderer is restored to explicit saved Forward+ in the uncommitted project diff. Focused build, structural, movement, native window and GFX interaction checks passed; the visual and performance limits above remain. [Roadmap](../../roadmap.md) remains the Gate authority: this correction does not mark P2, G2.1–G2.3 or V18–V20 passed.
