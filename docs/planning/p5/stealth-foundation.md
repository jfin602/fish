# P5 — Stealth foundation: Pike Encounter

**Decision:** owner-approved scenario route and isolated scene (S23, 2026-11-12). **Status:** approved planning document only; no P5 implementation, executable prompt stack, or gameplay qualification is claimed. The [P5 roadmap](../../roadmap.md) still reads NOT STARTED. [Stealth and detection](../../systems/stealth-and-detection.md) owns the shared mechanics; [ecology and growth](../../systems/ecology-and-growth.md) owns feeding/growth. [Validation](../../validation.md) owns scenarios; the roadmap alone owns Gate dispositions.

## Outcome and guiding principle

Build the first small, playable naturalistic stealth-survival encounter: **M1 — Survive the Pike**. The player is a **Bluegill** in a dedicated freshwater test scene, facing one **Northern Pike**. The player can notice the hunter, move through exposed water or cover, become suspected and pursued, hide or evade, and watch the Pike investigate a last-known position and eventually disengage. The player must also be able to get caught after a convincingly exposed mistake.

> Every movement is a decision about exposure. Every hiding place is an opportunity to survive. Every feeding opportunity is a reason to take a risk.

M1 proves the stealth foundation before adding the later P5 feeding, nutrition, growth and prey-hunting loop. Avoid a scripted trigger or an NPC that always knows the player's location.

## Verified starting state and dependency

At planning review, GitHub `main` was `405116b7ff8aa9bb30cbebf9b86d9ee7322f893c`; root version `0.4.11`. **P4 P12/V32 remains FAIL / NOT GREEN**; P4 visual clarity, surface interaction and native-control issues remain active. This P5 plan does not finish, rewrite, advance or bypass P4 Gates.

The existing `game/scenes/world_select.tscn` and `game/scripts/WorldSelect.cs` display two habitat cards derived from `WorldCatalog.All` and route through `character_select.tscn`. `WorldCatalog.cs` defines only `ocean` and `pond` as playable habitats. Registered Pond assets include **Bluegill `FAF-FISH-006`** and **Northern Pike `FAF-FISH-010`**, including adult GLBs. `FishController.cs` owns authoritative swimming; `PlayShell.cs` owns in-play pause/return behavior. `PondFafEnvironment.cs` integrates authored logs/foliage; `PondTurbidityField.cs` currently serves presentation, not authoritative AI perception. `FafAmbientPondLife.cs` is presentation-only and must not silently become a predator system.

Before implementation, inspect actual current HEAD, dirty work, P4 native limitations, active scene routes, collider/camera clearance, and compatible asset exports. Reuse accepted behavior and preserve in-flight P4 work. Authoring P5 documents and a fixture does not authorize advancing the root package version to `0.5.0` or declaring P4 closed.

## World Select: three choices, only two habitats

Add a **third selectable card labeled “Pike Encounter”** to the existing World Select page. It is a **development encounter destination**, **not a third ecological world**, baked-world identity, native habitat, or selectable species.

| Card | Route | Fish choice |
| --- | --- | --- |
| Ocean | World Select → Character Select → ordinary Ocean | Existing habitat-compatible selection |
| Freshwater Pond | World Select → Character Select → ordinary Pond | Existing habitat-compatible selection |
| Pike Encounter | World Select → dedicated `res://scenes/p5_pike_encounter.tscn` | Automatically use Bluegill `FAF-FISH-006`; **skip Character Select** |

Preserve existing Ocean/Pond baked manifests, environment profiles, menus, normal ambient actors, shader/settings behavior and species compatibility. **Do not add `pike` to `WorldCatalog.All`**, infer a separate freshwater ecosystem, or inject encounter actors into normal Pond play.

The current `WorldSelect` dynamically sizes its two-card array and columns from `WorldCatalog.All.Count`. When implementing the third option, separate **menu destinations** from the authoritative two-world catalog, rather than expanding the habitat list as a shortcut. Keep keyboard and pointer selection, focus indication, disabled/loading/error states, accessible labels, and responsive layouts (including narrow viewport with three cards). The single Choose/Enter action can select an encounter destination, but Ocean/Pond must still follow their existing Character Select path.

Preflight the fixture scene and both Pond-native assets through existing exact-ID catalog/eligibility checks. On missing/invalid model, scene or incompatible species, display a useful error **on World Select**, leave the normal worlds accessible and **never silently substitute Ocean, a different species or primitive fish**. Use the existing species/habitat definitions for checks; test mode does not relax cross-habitat policy.

## Dedicated scene and lifecycle

The planned `p5_pike_encounter.tscn` is a **distinct saved Godot scene** with a bounded freshwater encounter area; it may reuse or instance existing Pond terrain/asset/lighting components where practical, but must not mutate the installed Pond bake or rely on changing the normal Pond scene in place. Do not copy the entire Pond/Ocean rendering engine, swim motor or character catalog. Retain the shared controls, current third-person camera, pause/input system and configured water appearance.

- **Player:** one correctly instantiated Bluegill, initially using its registered adult demo visual and inherited common FishController/collision/control contract; later P5 growth may alter body sizes through separate verified rules. Character Select is never shown for this route.
- **Predator:** one Northern Pike with separate authoritative perception/AI, fish animation and physically meaningful clearance/turning. Do not convert the presentation-only ambient population into actors without explicit ecology registration.
- **Cover:** an actual hollow trunk, a reachable soft vegetation zone, and exposed water linking them. Place a deterministic starting fish and Pike position so an initial safe observation and both caught/escaped paths are reachable; adjust or add fixture-specific geometry when real P4 assets do not provide usable clearance.
- **Player-visible interactions:** native movement and its existing camera presets, readable predator orientation/investigation/attack commitment, real hard/soft concealment, and no fake permanent numeric hidden meter by default.
- **Outcome:** “caught” and “escaped” are **ephemeral fixture outcomes**; include **Retry Encounter** and **Return to World Select**. Caught is **not** a permanent game death, not a save-abandonment action and does not create a Life History entry. Retry resets both actors, AI evidence/memory, timers, temporary signal events, UI, fish position/velocity and input state.
- **Exit:** pause/retry/back/focus-loss/scene swap clear held swim inputs, mouse capture, pause state, temporary predators, debug probes, fixture world/session refs and signals. No duplicate actors or stale pursuit on re-entry. World Select must continue to offer both ordinary habitats.

A simple in-scene diagnostic overlay for developers may expose awareness, sensed stimuli, last-known positions and active transition, but should be optional and not the production HUD. Any fixture setup may identify the underlying habitat as `pond` for native species eligibility **without creating a new world ID** or claiming an ordinary persistent Pond run. Keep normal save/resume, archival death and run identifiers out of this prototype.

## Shared stealth architecture (working design)

Build reusable C# systems with narrow responsibility, adding concrete types only when required by executed F1–F8 tasks:

1. **World/environment perception:** bounded 3D query over target direction/range, obstruction geometry, underwater clarity and plausible illumination. It must not depend on camera transform, renderer GFX sliders, or whether the player's display has fog enabled. P4's `PondTurbidityField` provides a candidate deterministic world-space input; gameplay should sample the underlying field and/or a shared authoritative parameterization rather than reading pixels or GPU fog.
2. **Hard and soft cover:** collision/actual occlusion versus partial concealment. A trunk can prevent entry for the Pike while a reed patch lowers evidence but cannot guarantee safety. Re-evaluate usable exit routes and actual body/camera clearance.
3. **Movement disturbance:** short-range, time-limited events derived from the authoritative fish motion, effort and turning. Existing water-reaction particles are presentation, not a perception sensor or a second swimming engine.
4. **Awareness and bounded memory:** evidence strength, suspicion, investigation and last-known position with finite decay. A predator can search after losing sight; it must not follow an occluded target's exact hidden position.
5. **Predator behavior:** ordinary activity, investigation, pursuit, readable attack commitment, searching and disengagement, with obstacle-aware steering and bounded movement/turn rates; not a compulsory linear state script. An initial capture can produce a temporary fixture result before actual health/death is introduced.
6. **Prey sensing later in P5:** reuse the same information/occlusion model for prey to notice a careless approach, flee, and take shelter. Do not require prey AI to finish M1's Pike-survival encounter.

Authoritative simulation uses the actual gameplay clock/physics; pause and focus changes must not advance awareness or movement. Prefer deterministic fixtures and readable inspection output over complex full-world behavior or unbounded per-frame raycasts. The fixture must run without changing camera/motor/world authority for the two existing habitats.

## Scoped work sequence

| Task | Goal | Proof tier | Stop condition |
| --- | --- | --- | --- |
| F1 — Scene and routing | Add third World Select **destination**, direct Bluegill entry, error/return/retry plumbing; keep Ocean/Pond unchanged | T2 | Choose/Back/retry and two original world routes isolate correctly |
| F2 — Cover fixture | Place actual log, vegetation and exposed crossing, size/clearance probes, Pike and Bluegill setup | T2 | Fish/camera traverse shelter; Pike can't clip through solid trunk |
| F3 — Environmental sight | World-space 3D vision/occlusion, sampled clarity and illumination approximation; diagnostics | T2 | Camera/GFX independence and repeatable contrast/occlusion results |
| F4 — Disturbance and cover | Hard/soft concealment and local authoritative movement cues | T2 | Slow vs. burst and hard vs. soft cover produce explainable signals |
| F5 — Pike perception/AI | Non-omniscient suspicion, investigate, pursue, strike, search/disengage and steering | T2 | Deterministic successful search and lost-contact paths |
| **F6 — Native M1 review** | **Manually play Survive the Pike; tune attack readability, hiding, evasion, predator behavior and frame pacing** | **T3/manual** | **Owner judges both hidden escape and exposed catch understandable; failures remain NOT GREEN** |
| F7 — Prey awareness | Apply the same perception rules to one test prey and its flight/cover response | T2 | Careful and careless approach differ through actual evidence |
| **F8 — Native integration review** | **Review the encounter, menu isolation, predator/prey behavior and performance together** | **T3/manual** | **Recorded V05, V33–V36 evidence and explicit owner findings** |

The table is a **planning breakdown**, not eight existing executable prompts or runner checkpoints. Later `/prompt-ass` → `/prompt-plan` → `/prompt-write` can turn it into finite P1-based task briefs with the runner's actual grammar. Manual owner checkpoints at F6/F8 must not be replaced by headless tests or a transcript claim.

## Qualification and relationship to the rest of P5

**First milestone M1:** in native play, a Bluegill enters the Pike Encounter directly from the third card; a Pike notices exposed movement, investigates, chases and can catch an exposed player; the same fish can use true shelter to break contact, be searched for, and escape as Pike memory expires. Behavior must be fair, comprehensible and not require magic cone UI. Scene reset and return are clean and repeatable. **M1 is not by itself G5.1, G5.2 or the whole P5 GREEN.**

- [V36](../../validation.md) verifies entry, no Character Select, fixed Bluegill/Pike identity, failure handling, exit/retry, cross-world isolation, focus/input and native layout.
- [V33](../../validation.md) covers visual/movement exposure and hard/soft concealment.
- [V34](../../validation.md) covers investigation, last-known-location search, escape, caught outcome and native predator readability.
- [V35](../../validation.md) later tests real prey awareness and stealth hunting.
- [V03/V04](../../validation.md) and G5.1 remain later P5 feeding/growth work, including nutrition once and life-stage consequences. V05 remains the underlying predator encounter standard. Full normal-life permanent death and durable persistence are P6/P7 work.

Keep isolated fixture proof distinct from qualifying gameplay across production habitats. A functioning encounter demonstrates the shared systems in one scene; integrating them into normal Pond/Ocean ecosystems and qualifying the full P5 loop need separately scoped work/evidence. Do not move P4 visual/open-water/surface qualifications into the M1 acceptance criteria.

## Invariants and deferrals

No third habitat; no fish species crossover; no new fish models or unauthorized FAF export; no replacement of baked worlds; no modifications to accepted P1 controls, water shaders or GFX presets as a stealth shortcut; no normal-world predator spawning as a side effect; no permanent death, saves, Life History, hunger economy, breeding, whole-ecosystem simulation, full life-stage progression or sophisticated audio/chemical sensing as hidden M1 scope.

At any future execution inspect branch/HEAD, dirty changes, current P4 and P5 task/evidence status, selected Godot/MCP project, actual available test commands, and active P4 correction work. Record implementation and native outcomes separately. **Do not bump root version or mark any Gate GREEN from writing this plan.**
