# Game design

## Status and scope

This document organizes the Fish concept approved in the project discussion. [Decisions and open questions](decisions-and-questions.md) records provenance and decision status. Detailed mechanics below are working designs unless identified as confirmed. No gameplay system is implemented or qualified in this starter.

## Confirmed core

- Control a fish in a stylized 3D aquatic environment, primarily in third person. Ocean and freshwater Pond are separate supported habitats.
- Begin as a hatchling and survive long enough to grow into a large fish.
- Find food and eat suitable smaller fish while avoiding being eaten.
- Continue the same fish across multiple play sessions.
- Permanently end that life on death; begin the next run as a hatchling.
- Recount the dead fish's actual life briefly, then retain the story in a main-menu history section.
- Offer an optional first-person view as a secondary way to experience the same game.

## Multi-habitat world structure

The original reef/Ocean world remains an active playable option; P4 adds a separate Freshwater Pond, not a replacement Ocean skin. New Life uses **World Select → habitat-filtered Character Select → Play**. The current five FAF marine fish remain Ocean-only; future freshwater fish are Pond-only. Ordinary ambient and ecological populations respect the same habitat compatibility. The two habitats share movement/camera, procedural world generation and bake/load, but have separate environments, art, fog/light and content. [P4 plan](planning/p4/freshwater-world.md) owns the design.

World choice is a new-life choice, not an ability to transfer an existing fish between habitats. When persistence arrives, Continue restores the same living run, species, and world directly; a failed/missing world is a recovery issue, not a silent migration.

## Connected loops

### Immediate survival

Read the surroundings, choose a feeding opportunity, move out from shelter, feed or pursue, react to predators, and return to a viable position. Growth increases capability and changes which opportunities are useful.

### A fish's life

1. Begin a new life as a hatchling in a small starting habitat.
2. Learn movement, nearby cover, and accessible food.
3. Grow through successful feeding and survival.
4. Hunt suitable prey and reach more demanding areas.
5. Save and leave when desired; later continue the same living fish and current situation.
6. Keep surviving until a genuine gameplay death ends the life.
7. View the brief recounting and retain it in history.
8. Begin another life with a fresh fish.

The sequence is an organizing design, not a scripted campaign. A player can remain near familiar shelter or explore earlier when the rules permit. Intended life duration and a possible end objective remain open.

### Looking back

Open **Life History** from the main menu, browse completed lives, select a fish, and read its stored story and supporting record. Viewing an entry does not resume that dead fish. [Life stories and history](systems/life-stories-and-history.md) owns the archive rules.

## Growth and changing roles

The following stages describe an intended experience; they are not fixed level names or stat thresholds.

| Stage | Immediate problem | Changing opportunity |
| --- | --- | --- |
| Hatchling | Feed while staying close to usable shelter. | Narrow gaps and plants can protect the small body. |
| Small fish | Catch initial prey while watching for hunters. | More food becomes reachable; pursuit creates exposure. |
| Medium fish | Balance hunting against being hunted. | Familiar prey and predators change relevance as size grows. |
| Large fish | Catch useful meals and choose whether to enter greater danger. | The player can dominate some familiar territory while prey uses cover they no longer fit inside. |

The central early proof is a visible change in food-chain position. A species that initially threatens the player can become suitable prey after sufficient growth, if the selected prototype's size rules permit it. Growth must stay within one fish's identity; changing species is not part of the approved life progression.

## Controls and feedback

The core control direction is owner-approved in the [swimming contract](systems/swimming-and-camera.md), with the conversation preserved in the [completed worksheet](planning/p1/swim-controls-worksheet.md). First establish natural, nimble swimming that remains easy to control, using mouse steering and keyboard fallback. Controller support follows later. P10 owner feedback added a limited, self-returning camera pan in mouse mode and wheel zoom in either mode; a broader free-look system remains later work.

After actual playtesting and owner acceptance of the core, [P2 worldbuilding](planning/p2/underwater-world.md) establishes the technical underwater habitat/depth system. [P3 reef world rebuilding](planning/p3/reef-world-rebuild.md) then turns that varying-depth world into the first style-proving FAF reef. P3 also adds a Character Select page using all five FAF fish candidates—including the Blacktip Reef Shark—as visual-only player bodies under one shared movement/collision/camera contract, with an in-game return-to-select action that does not represent death or persistence. [Movement abilities](planning/p1/movement-abilities.md) remain accepted future movement work. Feeding belongs to P5 after P4 World Select/Freshwater Pond; tiny food collected by contact and an aimed bite for live prey remain working feeding designs.

Threats must remain legible above, below, and outside the camera's immediate view. Candidate predator behavior is approach, a readable attack commitment, pursuit, and disengagement. Small fish can use cover and turning; larger fish gain useful hunting capability. Numeric advantages and handling changes are tuning decisions.

## Survival across sessions

Saving suspends the current life. Returning preserves the fish's identity, growth, location, and relevant world and encounter state. The closed game does not simulate starvation or predation. A quit-and-resume cycle must not quietly reset a dangerous encounter or refill resources.

A real death is terminal for that run. Technical failures, unreadable save files, and interrupted writes must be reported or recovered as technical events rather than narrated as a fish's death. [Survival and runs](systems/survival-and-runs.md) defines those boundaries.

## First playable and integrated scope

[P1](tasks/p1/README.md) uses a compact 3D ocean test habitat and one controllable fish to settle the accepted core controls and camera in both desktop input modes. It integrates the four selected starter assets, develops fullscreen from the first task, and provides windowed fullscreen for testing. P1 ends with recorded tuning and actual owner acceptance of the dynamics.

[P2](tasks/p2/README.md) then replaces the visible test-course presentation with a natural reef-like habitat, locks a renderer from measured Compatibility/Forward+ testing, makes the world read convincingly underwater, and adds movement-reactive water presentation. The P1 course remains a regression fixture. [Art and assets](art-and-assets.md) owns source/provenance boundaries; the P2 slice does not settle the final species or production biome.

After the P3 ocean-world repair and qualification, P4 adds Freshwater Pond and World Select while preserving the Ocean. The P5 feeding-and-growth prototype then uses small food, one prey type, and one predator type. It should prove a readable escape, visible growth, and a meaningful change in what the player can eat. Specific content and quantities remain prototype choices.

The integrated slice adds saved continuation, permanent death, factual life events, the brief recounting, and the main-menu history archive. An empty history is a legitimate first-launch state. The optional first-person view can follow the main camera's basic qualification.

The current P3 correction is still underway at `0.3.6`; the P4 design is approved but not implemented. The original bootstrap task established the Godot project and development workflow. It does not authorize implementing every system described here. [The roadmap](roadmap.md) owns the Phase order and acceptance Gates.

## Deferred choices

Hunger pressure, diets, injury recovery, exact death thresholds, breeding, aging, advanced schooling, procedural habitats, and permanent unlocks need separate decisions when relevant. Permanent stat bonuses must not be assumed merely because a history archive exists. Fish naming and discovery records are compatible proposed additions, not requirements for the first playable movement test.
