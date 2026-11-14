# MVP sound inventory

This is the named file list for the first complete Fish life: choose a habitat and fish, hatch, swim, hide, feed, grow, evade predators, save and continue, die, and view Life History. It also covers the Bass/Pike encounter used to prove that loop. Every row is one **sound role** to audition and eventually place in the game, not a claim that the current generated take is accepted. The current [run 1/run 2 pick list](../../docs/assets/audio-base-pack/pick-list.md) contains candidates for the rows marked **Candidate**.

Put approved, game-ready files in this folder using the exact filenames below. One approved take is enough per row. If a sound needs several variations, add `_02`, `_03`, etc. after the canonical name; the unsuffixed file remains the default. Ambience loops use `.ogg`; short effects use `.wav`. Do not copy a candidate into this folder merely because it passed technical checks—choose it by listening first.

## Habitat ambience — 8 loops

| Final filename | When it plays | Source |
| --- | --- | --- |
| `pond_underwater_shallows.ogg` | Pond, underwater in the shallows | Candidate |
| `pond_underwater_deep.ogg` | Pond, underwater in deeper water | Candidate |
| `pond_surface_day.ogg` | Pond, camera at or above the surface by day | Candidate |
| `pond_surface_night.ogg` | Pond, camera at or above the surface by night | Candidate |
| `ocean_underwater_reef.ogg` | Ocean, underwater around the reef and shallows | Candidate |
| `ocean_underwater_deep.ogg` | Ocean, underwater in deeper water | Candidate |
| `ocean_surface_day.ogg` | Ocean, camera at or above the surface by day | New |
| `ocean_surface_night.ogg` | Ocean, camera at or above the surface by night | New |

Blend these by habitat, depth, surface position, and time of day. Existing underwater loops can cover both day and night at different levels; the MVP does not need separate underwater night files.

## Swimming and surface — 7 effects

| Final filename | When it plays | Source |
| --- | --- | --- |
| `swim_gentle.wav` | Quiet ordinary swimming by a small or medium fish | Candidate |
| `swim_accelerate.wav` | A real increase in propulsion | Candidate |
| `swim_turn.wav` | A distinct hard turn, with a cooldown | Candidate |
| `swim_large.wav` | Heavier swimming once the same fish has grown large | Candidate |
| `escape_burst.wav` | A genuine limited Spacebar boost begins for pursuit or escape | Candidate |
| `surface_breach.wav` | Fish breaks the water surface | Candidate |
| `surface_reentry.wav` | Fish returns below the surface | Candidate |

Swimming beds should be subtle and follow actual speed and body size. A turn or acceleration must not start a new one-shot on every frame.

## Contact and water disturbance — 5 effects

| Final filename | When it plays | Source |
| --- | --- | --- |
| `vegetation_brush.wav` | Body brushes reeds, weeds, or other soft cover | Candidate |
| `wood_contact.wav` | Meaningful contact with a log or root | Candidate |
| `rock_contact.wav` | Meaningful contact with rock or hard ground | Candidate |
| `small_bubbles.wav` | Visible local bubble release | Candidate |
| `sediment_disturbance.wav` | Actual bed disturbance or silt plume | Candidate |

Contact sounds need impact thresholds and cooldowns so sustained overlap does not chatter. These are presentation effects; a sound does not create a predator sensing event by itself.

## Feeding and prey — 4 effects

| Final filename | When it plays | Source |
| --- | --- | --- |
| `feeding_small.wav` | Confirmed zooplankton or other tiny-food consumption | Candidate |
| `feeding_strike.wav` | A real automatic mouth-contact strike connects with an eligible fish | Candidate |
| `feeding_swallow.wav` | A valid fish prey target is consumed and nutrition is awarded | New |
| `prey_scatter.wav` | A real nearby prey group breaks formation or flees | Candidate |

The hit and swallow cues represent different outcomes. Never play a success cue for an invalid or duplicate bite. The S25 design removes manual bite/miss input; invalid contact needs no missed-bite sound. Existing audio takes are not deleted.

## Predator and injury — 6 effects

| Final filename | When it plays | Source |
| --- | --- | --- |
| `predator_nearby.wav` | Subtle threat presence the player could plausibly perceive | Candidate |
| `predator_attack_commit.wav` | A predator visibly/audibly commits to an attack | New |
| `predator_lunge.wav` | The actual lunge begins | Candidate |
| `predator_missed_strike.wav` | The committed attack misses | Candidate |
| `predator_contact.wav` | Predator physically catches or hits a fish | Candidate |
| `fish_hurt.wav` | The player fish takes real, nonterminal injury | New |

These cues follow actual actor state and contact. A distant or hidden predator must not be revealed by a cue that the fish could not reasonably perceive. `predator_contact` can play for an actual Pike hit on Bass or Bluegill; `fish_hurt` is reserved for damage to the player.

## Life milestones — 4 effects

| Final filename | When it plays | Source |
| --- | --- | --- |
| `hatch_egg_crack.wav` | The Bass egg actually opens in the skippable hatch sequence | New |
| `hatch_emerge.wav` | The same Bass enters its controllable fry life | New |
| `growth_stage_up.wav` | Authoritative growth advances the fish to a new physical stage | New |
| `life_death.wav` | A real terminal death is recorded | New |

Skipping the hatch presentation must not duplicate either hatch sound or the underlying life event. `life_death` does not play for a caught/escaped development-fixture reset, save error, or abandoned session. The factual recounting can use quiet ambience and normal menu sounds; it does not need a separate narration or music cue.

## Menus, settings, and saving — 6 effects

| Final filename | When it plays | Source |
| --- | --- | --- |
| `ui_navigate.wav` | Menu/list focus moves, including World Select and Life History | Candidate |
| `ui_confirm.wav` | A valid selection or button action is accepted | Candidate |
| `ui_cancel.wav` | Back, close, or cancel | Candidate |
| `ui_adjust.wav` | A setting is changed by one deliberate step | New |
| `ui_error.wav` | An unavailable action or technical operation fails | New |
| `ui_save_success.wav` | The living fish has been durably saved | New |

The ordinary UI trio covers Main Menu, World Select, Character Select, pause/settings, death/recount controls, and Life History. `ui_save_success` must wait for actual save completion; `ui_error` covers a failed save or load without implying a gameplay death. Avoid a sound on every pointer movement or slider frame.

## Scope and naming rules

- **40 canonical files:** 8 ambience loops and 32 short effects. The existing candidate pack covers 28 retained candidate roles; 12 listed roles need new source material. S25 retires the manual `feeding_bite_miss.wav` role.
- The exact filename stem is the event role. Current candidate names ending in `_01` or `_02` are audition take numbers; the chosen default take is renamed to the unsuffixed canonical filename when promoted here.
- The list is complete for the **defined MVP audio roles**, not for future weather, advanced senses, new species, new habitats, or a larger music score. No music track is required for this naturalistic MVP; ambience carries the continuous sound bed.
- Keep volume, distance, stereo position, looping, and transition rules in game integration. They are not additional files. Some events may be silent at very low intensity, but their named role remains available for meaningful instances.
- This inventory is a production target. No new file listed here is generated, approved, or wired into gameplay merely by adding this document.
