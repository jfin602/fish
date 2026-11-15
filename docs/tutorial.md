# Opening experience

## Status

**Working design; unimplemented and unqualified.** The owner confirmed a hatchling start, feeding and growth, predator avoidance, saved continuation, permanent death, and a history of fish stories. A particular tutorial map, species, or scripted sequence has not been selected.

## P6 Living Pond opening (S27 approved; not implemented)

The first survival tutorial is an **ordinary Freshwater Pond Bass life** after egg hatch, not the separate Pike Encounter test. The hatchling should be able to eat initial nearby microfood without an instant unfair attack, but predator exposure begins early. Teach the small center-screen **view reticle**, and where needed the separate **actual fish-mouth heading indicator** when camera orbit/free-look differs; show genuine mouth-contact feeding, never automatic camera-center eating or magnetic target snapping.

A beginner can follow food among reeds, recognize adult Bluegill as a real danger to tiny Bass, use plants and hard cover to cross a feeding lane, and spot a frog or water snake near the surface. A failed open-water approach should have a readable cause and viable retreat. As food is consumed, encourage discovery of nearby vegetation/root/shore patches instead of endlessly circling the nest. Later instruction should arise naturally from becoming a Bluegill hunter, encountering larger competing Bass and following richer prey toward deeper Pike cover. Natural threats exist throughout; no locked depth/age corridors or mandatory scripted encounter.

P6 catch/retry is **session-local**. P7 later implements true save and permanent death, P8 archived Life History and P9 integrated acceptance. Preserve existing movement tutorial controls and keep all instructions reactive to actual player state.

## First minutes

Begin in a small, readable area with food close to cover. Give the player enough room to learn swimming, turning, and changing depth before demanding a difficult escape. Recognizable rocks and plants provide orientation and shelter.

The proposed opening teaches a few connected actions:

1. Swim to visible tiny food and observe a successful feeding result.
2. Reach nearby cover and learn which spaces the hatchling can use.
3. Notice a predator approach and escape using movement and shelter.
4. Grow enough to experience a visible capability change.
5. Catch the first suitable prey.

Recognize valid actions performed before a hint appears. Guidance should respond to actual player state, and it should not repeatedly interrupt later runs with instructions already understood.

## Surviving a session

Explain the difference between leaving and dying when the player first uses save-and-quit. A completed save preserves the living fish and its situation; the closed game does not continue predation or hunger.

The first real death introduces the recounting and its permanent history entry. Explain that **New life** begins another hatchling while **Life History** retains the fish that just died. Do not promise a checkpoint revival within that life.

## Prototype boundaries

The current core-controls playtest precedes the feeding-and-escape opening described above. Its accepted scenarios are owned by [validation](validation.md#core-movement-acceptance), using the [swimming contract](systems/swimming-and-camera.md). The tutorial sequence remains a later working design.

An initial movement sandbox may offer a development reset. Label it clearly as a test action and keep it separate from the eventual saved-life rules. A sandbox reset does not prove permanent death, save recovery, or history behavior.

The early gameplay prototype should get the player back to meaningful feeding and escape decisions promptly after failure. Exact growth speed, threat timing, and tutorial repetition are tuning choices. The [roadmap](roadmap.md) determines when those choices are tested.

## Acceptance targets

- The player can find and eat initial food using the visible controls.
- A shelter and a predator warning can be understood in native play.
- Feeding and growth feedback reflects actual state.
- The player can leave and later resume the same fish once persistence is implemented.
- The first death introduces a real archived story without implying respawn.

See [game design](game-design.md), [UX](ux-and-presentation.md), and [validation](validation.md) for the connected scope and evidence standards.
