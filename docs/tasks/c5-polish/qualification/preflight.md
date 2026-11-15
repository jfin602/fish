# C5 M7 — S25 implementation and integrated preflight

**2026-11-14 status: implementation and focused headless T2 checks PASS; editor MCP identity and native owner play remain NOT OBSERVED. Stop for owner review before M8.** This is an uncommitted manual task, not a Phase/Gate closeout. P5 G5.1–G5.3 remain NOT GREEN.

## Source and environment

- Actual checkout at entry: `/home/jfin/dev/fish`, `main` at `6242b02aa1b751bd1bb8a722023fccd56133c5a7`, one commit ahead of `origin/main`, clean before M7 edits. Root `package.json` version remains `0.5.12`. M1–M6 and caught-aftermath work were already present and were preserved. M7 changes are dirty and uncommitted; no push, reset, rebase or phase runner was used.
- Linux/X11, Godot `4.7.2.stable.mono`, Forward+, AMD Radeon 880M; fixture seed `5266` and baked Pond world `world-6c98f9e946b4ede8`. Ordinary Ocean smoke used seed `42`.
- Godot Editor MCP server `2026.11.04` was available, but its bridge at `ws://127.0.0.1:19080` stayed disconnected while another server owned that port. `./tools/dev editor` opened Fish with Forward+ but could not be verified through this task's MCP peer identity. No editor scene mutation or live MCP play claim is made. The editor was closed after this check; the project file's unrelated renderer rewrite was restored.

## Implemented behavior

- The encounter calls validated mouth feeding each physics tick; Space no longer requests a bite. The existing Enter/Space Skip Intro and Escape pause remain. Current Bass collider front and stage mouth radius bound food and Bluegill contact; the shell uses nearest points on its real visible mesh bounds. Facing and a solid-world ray reject rear, side, distant and obstructed targets. The authoritative FeedingLedger still commits one stable target ID, nutrition and event. Egg shell contact is automatic and worth zero nutrition; yolk is nonfeeding. The owner-approved eating cue is promoted to `assets/sfx/feeding_small.wav` and plays for valid small-food and Bluegill consumption, at different levels; the provisional strike take is no longer played. Bluegill consumption also plays a short visual fast-swim feeding cue, records one predation event, and emits one blood event. Invalid overlap has no sound, FX or nutrition.
- In the encounter, holding Space while swimming forward multiplies the real motor's target speed and acceleration. The FishController owns normalized energy, drain, recovery, exhaustion and re-arm; UI only reads energy. Release returns to normal acceleration with physical coasting, collision and waterline unchanged. Disarm, hatch, pause, focus loss and retry clear the held key. Space is reserved for boost in the encounter, including with an older custom control preset; normal preset bindings and ordinary Ocean/Pond behavior remain in place.
- Selected exposed defaults: speed multiplier `1.8`, full-reserve drain `2.5 s`, full recharge `5 s`, re-arm threshold `0.25` normalized energy. Native feel may warrant adjustment. The jaw radius is the existing stage-derived `max(0.012 m, 0.55 × collider radius)` with a `1.5 ×` contact allowance; a prey sphere contributes its physical radius. The old long-range BiteReach was removed.

## Checks actually run

| Check | Result and evidence |
| --- | --- |
| `./tools/dev import`, `./tools/dev build` | **PASS**; C# build had zero warnings/errors. The wrapper's composition was inspected first. |
| `inspect-auto-feed-boost` | **PASS**; no-button nymph and Bluegill contact, one nutrition/event/blood/sound/visual cue, repeated-contact rejection, rear/side/remote food rejection, real subadult speed `1.400 → 2.520 m/s`, finite exhaustion, recovery, fresh press, pause and focus cleanup. All five built-in presets activate boost; Adaptive Follow retains Left Shift free look. |
| `inspect-feeding`, `inspect-microfauna`, `inspect-growth`, `inspect-hatch` | **PASS** on final clean runs; yolk gate, auto larval zooplankton contact, nymphs, Bluegill diet and stages, wall blocking, shell zero nutrition, hatch/skip/retry and stage collider/camera clearance. One earlier microfauna run reported a shutdown resource leak after its success marker; the clean rerun passed. One earlier hatch run printed `HATCH_CHECK_OK` then hit the intermittent Godot .NET shutdown handle fatal; the clean rerun passed. These intermittent failures remain recorded, not erased. |
| `inspect-blood-fx`, `inspect-prey`, `inspect-bluegill-schools`, `inspect-pike-ai`, `inspect-pike-disturbance`, `inspect-pike-cover` | **PASS**; blood normal/off/low, prey cover and flight, three schools with 16 individual fish, Pike Bass/Bluegill choice and contact, real boosted disturbance, and three-zone cover. The first disturbance run **FAILed** because its historical slow juvenile motor created no movement cue; the test was updated to hold the genuine boost and then passed. |
| `inspect-control-presets`, `inspect-input-lifecycle` | **PASS**; five built-ins, schema 2 custom/migration recovery and normal input lifecycle. These checks do not substitute for native feel across every preset. |
| `inspect-world-isolation`, `inspect-world-select`, `inspect-pike-encounter`, `smoke-world` | **PASS**; three destinations, Ocean/Pond character selection and baked identity, no encounter entity leakage, main-scene motor smoke. |
| `git diff --check` | **PASS**. Documentation links and final dirty-file scope are checked at handoff. |

The owner's later SFX correction placed the approved eating cue at `assets/sfx/feeding_small.wav` under its canonical name and routed both valid feeding players through Godot's `game/assets/sfx/` link. The processed take remains in the candidate pack for provenance; the provisional strike sound is no longer played. A fresh headless import, zero-warning C# build and `inspect-auto-feed-boost` all passed after this correction. This does not add native play evidence.

Headless checks cannot establish Forward+ frame pacing or the visible bite, meter, camera and boost feel. The fixture has 47 cover instances, 16 Bluegill, three schools, and 128 motes; the added per-physics mouth scan visits bounded existing food/prey lists, but native GPU/CPU pacing is **NOT OBSERVED**. The intermittent .NET shutdown failure is a remaining test-stability risk.

## Native owner checklist for M8

- **V03–V05:** Hatch or Skip Intro into tiny Bass; swim with keyboard, mouse, hybrid and Adaptive Follow; confirm Left Shift free look, orbit/zoom, pause/focus/retry, camera/body clearance and readable boost meter.
- **V33–V35:** Cross shallow, middle and deep cover at slow and boosted speeds; compare Pike/Bluegill sensing around real wood and soft plants. Test stalking and escape without guaranteed safety or detection through solid cover.
- **V37–V38:** Feed from yolk interval through zooplankton, nymph, larval Bluegill and size-eligible juvenile Bluegill; confirm front mouth contact, no side/tail/wall/oversize feeding, visible swallow/sound/blood once, and real stage/camera growth. Touch the shed shell for zero nutrition.
- **V39–V41:** Observe age-structured Bluegill schools, Pike choice between Bass and a real Bluegill, caught aftermath and blood fade/reset. Revisit Ocean and Pond through World Select for normal passive behavior and character selection.
- **V42:** Hold/release/exhaust/recharge Space during pursuit and evasion, including after pause/focus/skip/retry; compare actual speed, disturbance, meter and native steering/camera feel. Record Forward+ frame pacing and any visible or audio defects. M8 requires the owner's native launch/playthrough; no M8 or Gate result is claimed here.
