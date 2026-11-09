# Swim controls worksheet

**Created:** 2026-11-09  
**Last updated:** 2026-11-09  
**Status:** In progress; Q01–Q02 answered; Q03–Q04 partially answered; free look deferred.  
**Answers recorded:** 4 of 10 (2 answered; 2 partial).  
**Next question:** Q04 — Starting, coasting, and stopping.

## Purpose and scope

The owner wants to settle fish controls and movement feel before developing the survival mechanics. This worksheet captures those choices through ten questions, discussed and saved one at a time.

Third person is already the primary perspective; first person remains an optional view of the same fish. The questions focus on the primary swimming experience. Suggested examples below are possibilities to discuss, not selected defaults.

The owner has played a tech demo. Record their observations as owner feedback; do not infer unobserved behavior or implementation from the published starter documentation.

[Swimming and camera](../../systems/swimming-and-camera.md) owns the movement contract. This worksheet holds the discussion and answers for [P1](../../roadmap.md#p1--swim-through-a-habitat) and the controls decision Q02 in the [decision register](../../decisions-and-questions.md). An answer records design intent; implementation and native playtest evidence remain separate.

## Working through the worksheet

1. Ask the next question in conversation, with the optional examples when useful. The owner may answer freely, combine examples, or leave a detail open.
2. Save each answer to this file before moving on. Preserve the owner's words under **Owner answer** and date the entry. Put interpretation under **Decision notes**, clearly identifying any proposed interpretation.
3. Mark the question **Answered** when its main choice is clear, or **Partial** when a material choice remains open. Do not fill unanswered sections with assistant preferences. Record a deliberate deferral as **Deferred**.
4. Update the answer count and next-question pointer. Count questions with saved owner responses; a partial or deferred answer is still a recorded response. Ask one question at a time, and do not ask again about details the owner has already supplied.
5. Preserve dated revisions when an answer changes. Keep unresolved details visible. Once the answers define a coherent direction, reconcile the swimming contract and decision register from those answers; the roadmap continues to own implementation and qualification progress.

## Q01 — Desired swimming feel

**What should it feel like to control your fish, and what about the current demo would you keep or change to get there?**

Optional starting points: quick and nimble; smooth and flowing with some weight; heavy and deliberate. Describe a mix or a reference game if that expresses it better. Include anything in the demo that already feels right.

- **Status:** Answered.
- **Recorded:** 2026-11-09.
- **Owner answer:** Yes, your description is pretty spot on. Uh, I like the line movement that requires planning. Your fish should feel nimble and agile in the water, but it's still water after all. So there's slight fluid dynamics where you have to turn a little earlier than normal to make the turn in time and stuff like that. But at the end of the day, it shouldn't be hard to control the fish
- **Decision notes:** Summary of the owner's answer: the fish should feel nimble and agile, with slight water effects that reward anticipating a turn and starting it a little earlier. It should remain easy to control. This establishes the desired feel; the precise movement model remains open.
- **Open details:** Exact momentum, acceleration, stopping behavior, and turning response will be refined in Q04 and Q05, preserving the nimble, approachable feel.

## Q02 — Input device and comfort

**Which input device should we design and tune the swimming controls for first?**

Examples: keyboard and mouse; gamepad; both, with one named as the first playtest target. Include any essential preferences such as inverted look, sensitivity, hold versus toggle controls, or rebinding. Exact bindings can follow the movement choices.

- **Status:** Answered.
- **Recorded:** 2026-11-09.
- **Owner answer:** We should target keyboard and mouse first and support controller later on. I feel like controller might be the best way to get the most natural feel, but that really limits who can play the game. So both needs to be possible
- **Owner elaboration (2026-11-09):** Within the keyboard and mouse department, there should be two modes as well. So there, I, I think that we could have a particularly mouse-based game mode or control mode where the mouse controls direction and your movements of the mouse control like the direction the fish is going. And then holding one of the buttons of the mouse controls like speed or thrust or whatever. And you could do it all from a USB mouse. That would be pretty natural too, but not everybody has a USB mouse, which is why there would be a fallback keyboard option
- **Decision notes:** Summary of the owner's answers: target keyboard and mouse first, with two desktop control modes. A mouse-only mode uses mouse movement to control the fish's direction and a held mouse button to control swimming speed or thrust; the exact speed/thrust model is still open. A keyboard fallback supports players without an external mouse. Add controller support later, with both desktop controls and controller ultimately supported. The owner expects a controller, and potentially the mouse-only mode, to feel natural; keeping keyboard controls available broadens access.
- **Open details:** Mouse button assignment, exact speed/thrust behavior, keyboard bindings, camera/free-look behavior, comfort settings, and the controller implementation milestone remain open. The explicit steering and held-button choices are carried into Q03 and Q04.

## Q03 — Looking and steering

**How should looking around relate to the direction your fish faces and swims?**

Examples: the fish steers toward the view while swimming; looking and steering are independent; ordinary look steers the fish but a separate free-look action lets you inspect the surroundings. Consider looking behind or above while continuing along the same route.

- **Status:** Partial.
- **Recorded:** 2026-11-09; carried forward from the owner's Q02 elaboration.
- **Owner answer:** "the mouse controls direction and your movements of the mouse control like the direction the fish is going." The full statement is preserved in Q02.
- **Owner follow-up (2026-11-09):** Let's not worry about free look right now
- **Free look:** Deferred for the current controls work.
- **Decision notes:** Mouse movement remains the selected steering input for the mouse-only mode. The owner deferred free look for now. Q03 remains partially answered because the keyboard mode's looking/steering relationship is still open; address relevant directional-input and camera choices in Q05 and Q07 while continuing the worksheet.
- **Open details:** Keyboard looking/steering and camera follow behavior remain to be defined. Free look is deferred and can be revisited later.

## Q04 — Starting, coasting, and stopping

**What should happen when you start swimming, release the movement input, and try to stop near something?**

Describe ordinary movement: holding an input to propel the fish or setting a sustained speed; immediate or gradual acceleration; stopping promptly or coasting after release. Use approaching a small landmark as the example. Optional special actions are covered in Q08.

- **Status:** Partial.
- **Recorded:** 2026-11-09; carried forward from the owner's Q02 elaboration.
- **Owner answer:** "And then holding one of the buttons of the mouse controls like speed or thrust or whatever." The full statement is preserved in Q02.
- **Decision notes:** In mouse-only mode, holding a mouse button controls swimming speed or thrust. The owner has not yet chosen between those models. Preserve Q01's nimble feel and modest anticipation when refining the response.
- **Open details:** The mouse button, speed versus thrust behavior, acceleration while held, coasting or stopping after release, and the equivalent keyboard input remain open.

## Q05 — Turning and directional movement

**How should left, right, and backward inputs move the fish?**

Examples: turn its nose and swim in an arc; slide sideways while keeping its heading; swim backward; turn around to travel back the way it came. Describe the desired turning response at slow and fast speeds, including whether it can pivot while nearly still.

- **Status:** Unanswered.
- **Recorded:** —
- **Owner answer:** _Not answered yet._
- **Decision notes:** _None recorded._
- **Open details:** _None recorded._

## Q06 — Swimming upward and downward

**How would you like to control the fish's depth?**

Examples: look or aim up and down, then swim in that direction; use dedicated rise and dive inputs; combine both. Picture reaching a point directly above or below and then returning to level swimming.

- **Status:** Unanswered.
- **Recorded:** —
- **Owner answer:** _Not answered yet._
- **Decision notes:** _None recorded._
- **Open details:** _None recorded._

## Q07 — Third-person camera

**Where should the third-person camera sit, and how should it follow you through turns and around nearby obstacles?**

Consider fish size on screen, distance, height, visibility ahead, zoom, and how quickly the camera catches up. Describe a comfortable view while circling a rock or entering a narrow passage. Q03 owns whether looking changes swimming direction; this question owns framing and follow behavior.

- **Status:** Unanswered.
- **Recorded:** —
- **Owner answer:** _Not answered yet._
- **Decision notes:** _None recorded._
- **Open details:** _None recorded._

## Q08 — Extra movement actions

**Beyond ordinary swimming, which extra movement actions, if any, would make control more enjoyable?**

Possibilities include a short speed burst, an active brake, a slow precision mode, or a quick evasive turn. Keeping only ordinary swimming is also an option. Describe the intended feel and preferred input; stamina costs, survival balance, and resource systems can remain open.

- **Status:** Unanswered.
- **Recorded:** —
- **Owner answer:** _Not answered yet._
- **Decision notes:** _None recorded._
- **Open details:** _None recorded._

## Q09 — Body movement and camera stability

**How much should the fish visibly bend, pitch, and bank as it swims, and how much of that motion should the camera follow?**

Consider tail motion during effort and gliding, body lean during turns, and whether the camera keeps a stable horizon. Include any dislike of camera shake, bobbing, or rolling. Describe the desired sensation; detailed animation production can follow later.

- **Status:** Unanswered.
- **Recorded:** —
- **Owner answer:** _Not answered yet._
- **Decision notes:** _None recorded._
- **Open details:** _None recorded._

## Q10 — Ready-for-gameplay playtest

**What should you be able to do comfortably in a playtest before we consider the swimming controls ready for the survival mechanics?**

Examples: approach and stop near a landmark; weave between rocks; turn around predictably; reach a point above or below; pass through a gap; regain control cleanly after pausing; enjoy several minutes of free swimming. Identify the actions and feel that matter most to you, including any behavior that would still make the controls feel unfinished.

- **Status:** Unanswered.
- **Recorded:** —
- **Owner answer:** _Not answered yet._
- **Decision notes:** _None recorded._
- **Open details:** _None recorded._
