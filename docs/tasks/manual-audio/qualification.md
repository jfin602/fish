# Manual audio pass — implementation evidence

Date: 2026-11-14. Starting branch/revision: `main` at `08810c654ec84709c320006c899047ce63ade95d`, clean before this task. The audio edits are uncommitted. Engine: Godot 4.7.2 .NET, Forward+, Linux/X11. Package remains `0.5.12`; no Roadmap Gate changed.

| Check | Result | Evidence |
| --- | --- | --- |
| Selected source | PASS | Owner-locked Pond deep Ogg `f7b5de45d36de0ceebb73ab03aa92aae4fe5cbcfe843965db5a356d39f73895a`; locked run 2 gentle/large/escape stereo masters match review-log hashes. Mono derivatives decode at 44.1 kHz, one channel. |
| Import and build | PASS | `./tools/dev import`; subsequent `./tools/dev build` produced Fish.dll with zero warnings/errors. |
| Focused audio | PASS | `./tools/dev inspect-audio`: one playing ambience node, correct buses, fish-owned 3D emitters, active camera listener, monotone continuous depth curve, smooth rise/fall, playback start count unchanged by depth samples, bounded swim/boost event gate. The check reloads Pike Encounter, confirms one fresh bed, returns to World Select, and confirms no bed there. |
| Scene and gameplay integration | PASS | `./tools/dev inspect-scene res://scenes/p5_pike_encounter.tscn`, `./tools/dev inspect-hatch`, `./tools/dev inspect-auto-feed-boost`: loaded scene, hatch/skip/retry, once-only feeding, growth and genuine boost checks passed. Other immediate-exit headless gameplay checks omit the audio presentation node; the dedicated audio check exercises it. |
| Source and docs | PASS | `git diff --check`; relevant links and paths resolve. |
| Desktop launch | PASS for launch only | `./tools/dev run --display-mode=windowed-fullscreen` launched Godot 4.7.2 .NET using Forward+ on AMD Radeon 880M, reached `FISH_WORLD_SELECT_READY entries=3 habitats=2`, and was left running for owner review. |
| Initial perceived audio quality | NOT OBSERVED at initial launch | The first implementation pass preceded the owner's listening feedback below. No sound-quality Gate was approved. |

The initial immediate-exit headless audio test exposed Godot mixer resources still in use at shutdown. The dedicated test now shuts the bed down and gives its audio thread one second before quitting. Unrelated instant-exit gameplay checks no longer instantiate this presentation node. The final focused audio, hatch and auto-feed/boost checks exited without the resource error.

The owner should choose Pike Encounter from World Select, listen near the surface and through repeated descents/ascents, then compare one swim start with sustained thrust, a valid boost, growth stages, pause, retry and World Select return. Exact source and tuning are in [runtime audio](../../systems/audio.md). Subjective observations remain for the owner to record.

## Owner mix feedback and correction — 2026-11-14

The owner reported that the ambient water sound is right as it is, eating is too loud, and swimming is a little too quiet. They also requested that swim effects stay independent of depth. The ambient track, its depth curve, buses and spatial emitter setup were left unchanged. Both eating cues were lowered 6 dB; all normal swim and boost stage gains were raised 3 dB. `UpdateDepth` still controls only the ambient player, and the focused audio check now asserts that a descent and ascent do not change either swim emitter's gain.

The revised `./tools/dev build` passed with zero warnings/errors, and `./tools/dev inspect-audio` passed the scene retry, World Select cleanup and depth-invariant emitter checks. The revised mix awaits the owner's native listening review.

## Owner cue timing and pitch correction — 2026-11-14

The owner asked for a swim cue on every fresh left or right mouse click and on presses of the steering controls, with slight pitch variation between plays. The old speed and idle gate was replaced by press-driven cues. Active thrust, slow, yaw and pitch bindings are included so built-in and custom steering keys or mouse buttons work. Held keys, key echo, releases and mouse movement do not retrigger. A genuine boost still plays once and takes priority over a simultaneous normal cue. Each normal or boost play now varies pitch by up to 5% around its base, with a minimum separation between adjacent pitches. Ambient water, eating levels, and the depth-independent swim gain were not retuned for this change.

`./tools/dev build` passed with zero warnings/errors. `./tools/dev inspect-audio` passed the control-binding, pitch-range, boost-priority and scene-lifecycle assertions. `git diff --check` passed. `./tools/dev run --display-mode=windowed-fullscreen` launched the revised game through World Select into Pike Encounter on the AMD Forward+ desktop; its Fish window was left open. Native sound quality remains for the owner to judge.

## Owner pitch, turn volume and overlap correction — 2026-11-14

The owner found the first pitch range barely perceptible and asked for overlapping cues rather than a new play cutting off the previous one. They also asked for volume variation on turning sounds. Pitch now varies up to 16% around each cue base, with at least about 5% between adjacent plays. Yaw/pitch presses vary gain by up to 3 dB around the stage level, with at least 1.5 dB between consecutive turns. Swim and boost use reusable pools of fish-owned 3D emitters: an idle voice is reused, and a new one is added if all current voices are still playing. Ambient depth response, base swim/boost gains and feeding levels were not changed.

`./tools/dev build` passed with zero warnings/errors. `./tools/dev inspect-audio` confirmed two turning cues and two boost cues can play simultaneously on distinct emitters, retaining each turning cue's original pitch and volume; pitch and turning-gain ranges passed. `git diff --check` passed, and `./tools/dev run --display-mode=windowed-fullscreen` relaunched the revised Fish window on the AMD Forward+ desktop. Native listening remains open.

## Owner cue length variation — 2026-11-14

The owner accepted the revised mix as nearly right and asked for length variation as well. Each swim and boost cue now selects 72–98% of its source duration independently of its pitch, with at least 8% difference between consecutive selections. A short per-voice fade prevents abrupt cutoffs. The ambient water track, feed levels, cue trigger mapping, pitch and turn-volume ranges remain as qualified above.

`./tools/dev build` passed with zero warnings/errors. `./tools/dev inspect-audio` passed length-range and fade checks, including one overlapping cue ending while the later cue continues. `git diff --check` passed. `./tools/dev run --display-mode=windowed-fullscreen` opened the revised Pike Encounter on the AMD Forward+ desktop for owner listening.

## Owner movement-driven turn correction — 2026-11-14

The owner found that cues felt more real when they followed the fish's actions rather than controls. Press-to-cue mapping was removed. Normal swim onset again waits for actual movement under effort; a click without movement is silent. Turn cues now follow measured yaw and pitch changes after collision-safe motor resolution. A blocked turn is silent, sustained rotation renews cues at a bounded cadence, and stronger angular rotation plays a longer share of the source (42–98%). The accepted pitch and turning-volume variation, independent overlapping voices, ordinary cue-length variation, quieter eating cues and unchanged ambient bed remain in place.

`./tools/dev build` passed with zero warnings/errors. `./tools/dev inspect-audio` passed movement, no-motion, turn threshold, sustained-turn cadence, boost priority, turn-strength duration, overlap, fade, depth isolation, scene retry and World Select cleanup checks. `./tools/dev run --display-mode=windowed-fullscreen` reached World Select on the AMD Forward+ desktop and then exited normally; no listening result was observed. Native listening of the revised feel remains for the owner.

## Owner turn sensitivity and variation correction — 2026-11-14

The owner reported that light mouse movements were firing turn cues constantly and that consecutive cues had lost their distinct pitch, gain and length. The turn gate now smooths measured rotation over about 0.12 seconds and requires at least 40% of the fish's maximum angular rate. It spaces repeat cues by 0.65–0.9 seconds. Each accepted turn retains independent pitch variation (±16%) and gain variation (±3 dB), and now selects a fresh strength-dependent duration with at least 8% source-length separation from the prior turn. Light qualifying turns use about 44–64% of the source; full-strength turns use about 74–94%. The ambient bed, feeding gain and depth-independent swim gain were not retuned.

`./tools/dev build` passed with zero warnings/errors. `./tools/dev inspect-audio` passed repeated light-turn silence, one-frame flick silence, deliberate-turn response, bounded repeat cadence, strength-dependent duration, same-strength length variation, adjacent pitch/gain separation, overlap and scene lifecycle checks. `git diff --check` passed. `./tools/dev run --display-mode=windowed-fullscreen` reached World Select on the AMD Forward+ desktop and remained open for owner review. Native listening remains open.

## Owner turn-speed gate — 2026-11-14

The owner requested turn sounds only while the fish moves at 40% of full speed or faster. The gate now compares the fish's post-physics speed with 40% of its current growth stage's normal maximum speed. It resets turn activity below that threshold and suppresses new turn cues even during strong sustained steering. A cue already playing may finish. Normal swim onset, boost, pitch, gain, length variation and ambience were left at their prior settings.

`./tools/dev build` passed with zero warnings/errors. `./tools/dev inspect-audio` passed sustained strong turns just below the threshold, the inclusive 40% boundary, falling below threshold during a turn, and the prior cue variation and scene-lifecycle checks. `git diff --check` passed. `./tools/dev run --display-mode=windowed-fullscreen` reached World Select on the AMD Forward+ desktop and remained open for owner review. Native listening of the revised speed gate remains open.
