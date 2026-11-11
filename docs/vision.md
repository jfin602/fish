# Vision

## The game

Fish is a 3D **naturalistic stealth-survival** game about living one fish's life, from a vulnerable hatchling to a large fish. Stealth is the primary gameplay identity, not an optional tactic: the player learns when to remain concealed, move between shelter, take risks to feed, stalk prey, and escape predators. Feeding and growth change the opportunities and dangers. Third person is the main perspective; an optional first-person view offers another way to experience the underwater world.

One fish's life is one run. That life can continue across many play sessions. Leaving the game preserves the living fish and its world; returning continues that life. Death permanently ends it, and the next run begins as a new hatchling. The world does not advance while the game is closed.

After death, a brief recounting tells that fish's actual life story. Its story remains in a history section of the main menu, where the player can revisit all previous lives.

These foundations come from owner decisions on 2026-11-09 through 2026-11-11, including S20's naturalistic stealth-survival direction. [The decision register](decisions-and-questions.md) separates confirmed design requirements from proposed rules. P0/P1 gameplay foundations are implemented; stealth AI, feeding and growth are not implemented or qualified.

## Guiding principle — owner-approved

> Every movement is a decision about exposure. Every hiding place is an opportunity to survive. Every feeding opportunity is a reason to take a risk.

Stealth survival with natural mechanics is Fish's main goal. The ecosystem should continue to behave as living creatures rather than as scripted enemies waiting for the player. Physical cover, movement, lighting, visibility and creature perception make the player's exposure consequential. The same basic interaction applies when a fish hides from a hunter and when it stalks prey. Use restrained, grounded feedback to make the rules legible; do not replace natural behavior with omniscient pursuit or arbitrary safe zones. [Stealth and detection](systems/stealth-and-detection.md) owns the proposed mechanical contract.

## Player fantasy

Begin small enough that a rock gap or a clump of plants can save your life. Learn where food is available and where hunters patrol. Venture farther, catch your first prey, and gradually become large enough to face species that once drove you into hiding.

The player should recognize their fish's history in familiar places: the shelter that kept it alive, the feeding ground where it grew, the narrow escape, and the deeper water it eventually dared to explore. Growing creates attachment because the same creature survives each session.

## Design priorities

### Stealth is the central skill

A meaningful encounter starts before combat: observe a hunter or prey, identify hiding places, judge exposure, move deliberately and react when noticed. Danger is often avoidable through good positioning and concealment; discovery does not remove every opportunity to evade. The underwater world must supply usable cover networks, not merely decorative assets.

### Growth changes the use of space

A small fish can fit into cover a large fish cannot. Increasing size changes what can be hunted, which routes are available, and how the player escapes danger. Familiar surroundings should remain useful as the fish changes.

### Feeding creates choices

Food offers growth, and reaching it can expose the player to danger. Safe shelter, accessible food, richer feeding grounds, and predator presence should create understandable decisions. Exact hunger, diet, and growth formulas remain open.

### Movement makes survival enjoyable

Swimming, turning, pursuing, and escaping are the immediate experience. Third-person controls should make the fish and nearby threats readable in three dimensions. The optional first-person camera observes the same fish and world.

### Failure has an understandable cause

Permanent death gives survival weight. Threat presentation should give players a chance to recognize danger and learn from an encounter. Death summaries must use the recorded cause and must distinguish a real gameplay death from a technical failure.

### A long life earns a satisfying payoff

Becoming large should provide a period of real capability in familiar territory. More dangerous areas can offer voluntary challenges. Full adult size, lifespan limits, and any final objective are unresolved design choices.

### Every life leaves an honest record

The recounting emphasizes actual turning points. A short life can have a short story; the game does not invent an escape, discovery, kill, or motive to make an entry more dramatic. Archived stories survive the end of the active run.

## Scope

The first playable work establishes natural, nimble, easy-to-control swimming and a comfortable third-person camera. After actual P1 playtesting/owner acceptance, **P2 builds the technical underwater world before ecology**: depth-structured traversal, convincing underwater atmosphere and movement-reactive water. **P3 then rebuilds and dresses that world with the complete original FAF reef library**, adding caves, tunnels, lush shallow reefs and sparser deep geology until the running game reaches the approved visual target. P4 completes its original Pond, World Select, cover-asset and traversal scope without new stealth-driven geometry requirements. P5 then assesses and improves the delivered cover network and begins the compact stealth-survival loop with sensing, concealment, investigation, hunting, escape, feeding and growth. The first integrated survival slice must also prove continuing a life across sessions, permanent death, a factual recounting, and revisiting that story from the main menu.

The owner originally selected realistic ocean creatures and environments on 2026-11-09; the later FAF low-poly naturalism contract supersedes that rendering direction. [Art and assets](art-and-assets.md) records the approved asset sources and preparation boundaries. P2 establishes a deliberate reef-like slice but does not settle the final player species, final production biome, larger procedural-generation strategy, number of simultaneous living saves, or cross-run unlocks. [Movement abilities](planning/p1/movement-abilities.md) remain approved future work. Multiplayer, breeding, detailed biological simulation, and online services are not part of the initial scope.

Use [game design](game-design.md) for the connected loop, [system documents](README.md) for specific rules, and [the roadmap](roadmap.md) for sequencing and evidence.
