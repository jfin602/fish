# P1 implementation plan

## Inspect and enter

Start with AGENTS/BOOT and [entry and resume](manual/entry-and-resume.md). The authoring snapshot contains only the published starter; inspect the actual game checkout and local changes before adopting source paths. Reuse compatible P0 implementation and record its actual foundation/native evidence. Missing P0 requirements remain prerequisite work. Enter at root version `0.1.0` deliberately; do not alter versions merely to satisfy a validator.

Inspect the real `tools/dev` commands and their composition. Use `doctor` for environment identity; use the composed `check` after changes that need its import/build/scene checks, without immediately repeating the same component builds. Use the actual native editor/MCP connection only after verifying the Fish project identity. Reference pins and renderer remain owned by [development](../../development.md) and [technical direction](../../technical-direction.md).

## Planned source ownership

These are proposed locations to map onto actual source, not files claimed to exist or a mandate to create empty classes.

| Boundary / likely paths | Responsibility |
| --- | --- |
| `game/project.godot`, actual main scene | Startup display mode, input actions, and the real P1 scene. |
| `game/scripts/app/`, `game/scenes/ui/` | Display choice, local preferences, pause/settings/focus lifecycle, accessible help. |
| `game/scripts/player/SwimMotor.cs`, tuning resource | One authoritative physics motor and reusable response parameters. |
| `game/scripts/input/` | Keyboard and mouse intent adapters plus explicit mode selection. |
| `game/scripts/presentation/` | Follow camera, obstacle adjustment and fish visual motion, independent of movement authority. |
| `game/scenes/p1/`, public-safe asset wrappers | Compact ocean course, start/landmark layout, authored collision and resource configuration. |
| `game/assets/`, ignored restricted subdirectory | Verified prepared resources; selected restricted vendor payloads remain untracked. |
| `game/dev/`, existing developer CLI | Small focused state/scene checks and a temporary display-mode launch override. |

Maintain one source of truth for settings/tuning. Avoid introducing survival state, a general ECS, a broad test framework or a full graphics-settings product in this phase.

## P1 — Fullscreen play shell

Make the normal game launch fill the display from the first P1 implementation change. Provide Fullscreen and Windowed fullscreen in a small pause/options surface with both pointer and keyboard navigation. A fresh profile defaults to Fullscreen; retain the chosen display/input preferences locally and keep developer overrides temporary. The [phase design](../../planning/p1/core-swimming.md#fullscreen-from-the-first-task) defines the provisional engine mapping and platform-equivalence caveat.

Add and document `./tools/dev run --display-mode=fullscreen` and `--display-mode=windowed-fullscreen`, or an equally explicit verified wrapper syntax. These are planned additions, not commands available in the inspected starter. Parse only supported values, forward game-owned arguments safely, and leave existing build/run composition intact. Treat native rendering as unobserved until P5. Do not claim an embedded editor viewport qualifies fullscreen.

Anchor/scale menu and control-help layout to the actual viewport. Establish deliberate pause/resume, focus-loss pause and input clearing. Do not add UI labels that claim unfinished input modes work; P7 introduces the completed mode selection. Store local settings separately from future living saves.

## P2 — Starter asset acquisition

Use the revised four starter slots from [art and assets](../../art-and-assets.md): Quaternius `Fish3` / `ClownFish` from the owner-provided Animated Fish Pack, Rock Face 01, Coast Sand 01 and Smithsonian *Heliopora coerulea*. The original prompt names are superseded by the owner's resumed P2 decisions. Inventory acquired originals, dependencies and source terms. Record exact source/creator/package/date/hash/license/credit data in a public-safe manifest. Distinguish research, acquisition, preparation, import and observation. The low-poly pack is an explicit P1 prototype choice; final realistic art and player species remain open.

Before putting any vendor files in the checkout, add and verify ignores for source archives and the local restricted payload directory. Restricted data must also stay out of tracked scenes/resources and generated packs. If access needs an owner-provided package, identify the precise link, archive and destination; preserve partial work rather than pretending a download occurred. No silent substitute or later-shortlist expansion.

## P3 — Godot asset preparation

Inspect Quaternius `Fish3` / `ClownFish` mesh/skeleton/`Swim` action, extract or convert through supported tools, rebuild suitable materials and document scale/orientation/root-motion treatment. Do not assume Blender or FBX clips transfer unchanged. Preserve the acquired source; required rig/material repairs belong to this task or a clearly bounded continuation. Other pack fish may be prepared for non-gameplay world population. Sea life pieces may be considered only after embedded third-party texture rights are resolved or those maps are excluded.

Prepare the rock, sand material and selected coral pieces with suitable measured scale/texture choices. Public-safe wrappers may load documented local resource paths dynamically. A fresh clone without required restricted resources must remain inspectable and report an actionable missing-asset condition; it cannot silently substitute a fish and pass selected-asset checks. Record actual local install/import mapping in `game/assets/README.md` and append command evidence.

## P4 — Ocean movement course

Compose a compact saved 3D scene using all four selected assets. Author seabed geometry for Coast Sand, rock obstacles/passage, coral placement, open water, a slowing target and upper/lower landmarks. Place a bounded number of other prepared Quaternius fish for visible world life, without adding ecology or predator behavior. Use deliberate simple collision shapes that agree with visible gaps and fish dimensions; visual triangles are not the collision policy. Keep start location, scale and route reproducible.

Use the existing provisional P0 controls to make the scene inspectable until P6/P7 replace the movement behavior. Preserve the fullscreen shell. Check saved-resource references, collision layers, bounds, spawn clearance and composition through the actual import/build/scene checks. Visible material quality and final movement remain unobserved until their native tasks.

## P5 — Native display and asset checkpoint

Pause for a real native-capable session. Inspect the correct Fish editor/runtime, start the main scene fullscreen, select windowed fullscreen, test launch overrides and saved-choice precedence, and inspect focus/capture/layout at actual display dimensions. Check pointer/keyboard access to the current panel; the final desktop swimming modes are qualified in P10.

Observe all four actual selected assets, selected fish materials/fin transparency/basic imported motion, obstacle/seabed/coral scale and visible route. Verify fresh-profile startup, saved-choice relaunch and temporary-override non-persistence using isolated or restored settings. Exercise the reversible missing-resource diagnostic in the native window, restore all required resources, and record representative scene frame timing/FPS and display/render settings. Record any concrete repair before progressing to detailed movement work. This early checkpoint cannot lock core dynamics or pass final G1.1/G1.2. Its source-bound evidence supports the eventual asset/display Gates, subject to later changes.

## P6 — Shared motor and keyboard

Implement one physics-time motor with speed demand, quick acceleration, slower coast, explicit keyboard slowing, curved turns, mild velocity/heading lag and pitched travel. Use the [initial mappings and model](../../planning/p1/core-swimming.md#shared-movement-and-input) as reversible defaults, then record actual values. No reverse command, strafing, automatic leveling or root-motion propulsion is added by implication.

Use small checks for acceleration/coast/brake relationships, finite bounded state, pitch travel, collisions and pause. Compare equivalent physics/input sequences across render cadence changes where the real harness permits it. Write checks for those behavioral risks, not one assertion per implementation line. Near-rest maneuverability and chosen pitch limits must remain tunable for P10.

## P7 — Mouse mode and input lifecycle

Connect relative mouse steering, held-primary propulsion and release coasting to the same motor. Secondary-button pause is the initial mouse-only menu route; it is not an active brake. Expose explicit mode selection with mouse-only and keyboard-only menu operation. Settings/help must show actual bindings.

Consume deltas once, clear stale movement on pause/focus/mode/display changes, and require fresh input after resumption. Prevent UI clicks from leaking into propulsion and keyboard fallback from depending on an external mouse. Validate lifecycle cases and equal motor behavior; native comfort remains P10.

## P8 — Follow camera

Implement behind/slightly-above framing, moderate distance, route visibility, smooth heading/depth following and nearby-obstacle adjustment. Follow the unbanked root while retaining the horizon. Exclude the fish from obstruction tests, handle compressed camera distance and recovery, and avoid snaps/singularities on near-vertical travel. Camera updates must not write movement state. Keep response values in the shared tuning profile and validate concrete obstruction/state risks.

## P9 — Fish body motion

Connect the prepared selected ocean fish's clips/bones to actual propulsion, speed and steering. Make tail/body motion visible during effort, easing in the glide, with bend/lean in turns. Contain imported root motion and keep the collision root authoritative. Document actual clip/skeleton mapping and state transitions; no feeding/attack/death behavior follows from those clips being present.

Keep restricted meshes/textures/animations in the established ignored path, including any edited or converted resource. Public code and independent configuration may describe how to drive them. Verify animation-state changes and resource boundaries; the actual visual result and comfort are part of P10.

## P10 — Tune and lock the dynamics

Use the same course for both input modes and iterate with actual owner feedback. Tune acceleration/coast/braking, turning/alignment, vertical/near-rest behavior, camera, then body motion, checking their integrated effect. Store the resulting numbers and mappings in the real tracked configuration. Keep a concise source-bound iteration log, preserving failures and repairs.

Run all seven accepted V01/V02 criteria independently for both modes and several minutes of free swimming with each. Recheck V16 selected assets and V17 fullscreen behavior on the final candidate, including the two-input-by-two-display-mode smoke matrix. Record display/backend/resolution scope and limitations; do not turn untested platforms into claims. Measure representative frame timing/FPS and visible stalls in the final four-asset course after motor/camera/animation integration, identifying hardware and render settings. Recheck affected missing-resource and saved-choice/override observations if later changes invalidated P5 evidence; do not invent a production performance budget.

Write `qualification/swimming-acceptance.md` and `qualification/accepted-tuning.md` only from actual observations. The latter must identify bindings, sensitivity, motor model and values, pitch/near-rest policy, camera and animation settings, source/configuration identity and imported-asset versions/hashes. The owner must explicitly accept the implemented feel. If a core behavior remains unsatisfactory, keep tuning or record a concrete unresolved gap and withhold the task version. Do not defer unfinished core work as optional polish.

## P11 — One final closeout

Map G1.1–G1.4 to the actual implementation, asset, display and acceptance records. Check that accepted evidence still describes the delivered gameplay/configuration/assets; rerun only checks invalidated by later changes. Documentation-only reconciliation does not require a new gameplay run.

Update the roadmap, task disposition, BOOT, MODULES, development instructions and relevant owning docs once. Replace remaining-core-tuning language with the actual accepted profile/evidence on success, preserving the worksheet's historical answers. Later controller/ability work stays separately open. P1 is complete only when every Gate passes; leave the closeout changes reviewable and route the next Phase to the already-authored P2 underwater-world stack.

## Evidence, versions and recovery

Use the [README evidence map](README.md#evidence-created-during-execution). Each record needs revision plus dirty-source identity when relevant, platform/tool versions, scenario, actions, expected/observed results, retained artifacts and limits. Distinguish imported from observed, headless from native, and assistant checks from actual owner judgments.

Tasks P1–P11 target `0.1.1` through `0.1.11`. Advance only after that task's required checks pass. Intermediate manual prompts need a reviewed exact version checkpoint before runner resume; final closeout does not self-commit. Missing access, dependencies or owner observations retain the previous successful version and partial work. Follow the existing runner rather than adding a new orchestration layer.
