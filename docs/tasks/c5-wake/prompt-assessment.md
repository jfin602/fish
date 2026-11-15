# /prompt-ass — C5 wake

**Requested outcome:** Replace discrete circular Pond surface ripple markers with convincing body-generated, curved wave fronts while keeping fluid simulation and gameplay authority out of presentation.

## Existing source and dependencies

- The shared `game/scripts/WaterReaction.cs` owns underwater trails and bubbles, eight `SurfaceRing` meshes and splashes, including recent Bass-scale/boost tuning.
- `PlayerBodyGeometry` measures rest-pose mesh bounds; `FishController` and `PondGrowthState` own real body/movement/boost state.
- `UnderwaterAtmosphere`, Pond surface geometry and `water_surface.gdshader` own visible water. The latest owner-requested dynamic-waterline fix may be **unpushed**; do not accidentally substitute old fixed Y calculations.
- Preserve normal Pond and Pike Encounter effects; Ocean underwater visuals; owner camera smoothing, lighting, audio, feeding, predators and stage lifecycle.

## Work sequence

**W1 (T2):** inspect current source/waves, derive sensible head/side/tail anchors from the stage mesh/body contact, create fixed-capacity movement-history samples that preserve old world-space paths and depend on genuine displacement. Provide diagnostic-only paths and tests; leave production rings untouched.

**W2 (T2 + native comparison):** replace surface ring rendering with two thin, feathered, fading and outward-propagating fronts plus tiny coherent bow/tail detail. Use the same moving surface height and a natural normal/specular appearance rather than opaque-colored strips. Compare normal and boosted playback at consistent pose and time. Stop for visual review.

**W3 (T2 + owner T3):** tune all four playable Bass stages, changes of growth scale, speed/turn/boost, diving, pause/scene lifecycle and both Pond destinations. Measure native Forward+ performance, confirm Ocean isolation, and leave actual Pike Encounter running for owner.

## Risks and guardrails

Rest-pose contact approximations must not produce floating wake at sharp pitch or body scale changes. Ring-buffer density must not vary wildly by FPS. Old paths must not rotate with the fish or bridge teleports. Wake must not duplicate underlying displacement/waterline time, cause Z-fighting/shore lines, introduce hard painted V geometry or huge draw/mesh cost. No wake-driven fish forces or AI detection. Preserve historical evidence and stop on an unsupported renderer instead of hiding a defect with fog or camera changes.

## Acceptance evidence

W1: world-space contact/path debug screenshots and deterministic tests; W2: same-route before/after native overhead/oblique/underwater captures and cost; W3: stages/boost plus owner free-swim review. V43 supplements V20/V32; no Phase/Gate is qualified by planning documents or headless-only passes.
