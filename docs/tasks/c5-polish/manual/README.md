# C5 — M1–M8 manual execution briefs

**MANUAL ONLY** — copy one complete brief into Codex after opening the actual Fish checkout. These are deliberately outside the runner's root `.txt` scan, not automatic Phase/correction checkpoint prompts. [C5 task index](../README.md).

## Steps

1. [M1 — Bass player and Bluegill-only fish prey](M1-bass-player-and-bluegill-prey.txt)
2. [M2 — Shallow egg hatch and controllable Bass fry](M2-hatching-cutscene-and-tiny-bass.txt)
3. [M3 — Early Bass food and all-stage growth](M3-early-diet-and-growth.txt)
4. [M4 — Age-structured Bluegill schools in real Pond cover](M4-bluegill-schools-by-age-and-depth.txt)
5. [M5 — Independent Pike hunting and school-assisted evasion](M5-pike-multi-target-and-school-escape.txt)
6. [M6 — Dissipating underwater blood on genuine fish predation](M6-blood-cloud-on-real-fish-events.txt)
7. [M7 — C5 integration checks and owner-native launch readiness](M7-regression-and-native-playtest-preflight.txt)
8. [M8 — Launch Fish for the owner's full visual/playthrough qualification](M8-owner-native-launch-and-qualification.txt)

M1–M6 produce bounded implementation/check evidence; M7 prepares the integrated owner-native matrix and removes blockers. **M8 must finish by opening the actual Fish game in a user-visible desktop window with `./tools/dev run --display-mode=windowed-fullscreen`** after preflight. Keep the window open so the owner can play. No browser-only screenshot, headless trace or assistant inference qualifies that human play.

Start each next prompt only after reviewing previous output. Preserve unresolved P5 historical native gaps and root package `0.5.12`; don't self-commit/push or run `npm run fish:phase -- c5-polish`. Native Gate status stays NOT GREEN until actual owner evidence supports change.
