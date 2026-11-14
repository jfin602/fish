# Fish base audio candidate pack

Generated locally on 2026-11-13 with Soundbox's installed `audiocpp-stable-audio-3-sfx` model. This is a revision 2 audition candidate, not an approved final sound set. The owner rejected the first effect set because most effects, including UI, sounded like a jackhammer. Its original sources and prompts are retained in [manifest-v1-rejected.json](manifest-v1-rejected.json); the revision 2 effects use shorter single-event generations. No gameplay, scene, sound trigger, audio bus, version, or Gate was changed for this pack.

## Contents and paths

- 28 requested sound types, 33 processed files. Second takes exist for `swim_gentle`, `swim_accelerate`, `feeding_small`, `predator_lunge`, and `ui_navigate`.
- Original, byte-identical Soundbox WAVs: `assets/audio-source/soundbox/run1/` and `assets/audio-source/soundbox/run2/`.
- Game-ready effects: `game/assets/audio/sfx/` (16-bit PCM WAV, 44.1 kHz stereo).
- Game-ready ambiences: `game/assets/audio/ambience/` (Ogg Vorbis, 44.1 kHz stereo).
- Full prompt, seed, Soundbox library ID, processing, duration, hashes, and per-file checks: [manifest.json](manifest.json).
- Review folders: [run1](run1/) has the first 27 effects and six ambiences; [run2](run2/) has the 27 regenerated effects. Each folder has individual playable files, a manifest, an audition Ogg, and a sample order with start times. The auditions have 400 ms silence between samples and do not contain spoken labels.
- [Take pick list](pick-list.md) links both runs by sound name and numbered variation.

## License

The [Stable Audio 3 Small SFX model card](https://huggingface.co/stabilityai/stable-audio-3-small-sfx) identifies the model-specific license as the Stability AI Community License. [Stability AI's licensing page](https://stability.ai/license) and [Stable Audio licensing guide](https://kb.stability.ai/knowledge-base/stable-audio-commerical-and-usage-licensing) describe commercial game use under the Community License for qualifying users below USD 1 million in annual revenue, including affiliates. Commercial users must register with Stability AI; use must comply with its Acceptable Use Policy. Above the threshold, a separate Enterprise license is needed. The model card also identifies [Gemma component terms](https://ai.google.dev/gemma/terms) for its text encoder. The owner must confirm these conditions before shipping. Soundbox's own software license does not replace the model license.

## Processing and validation

Revision 2 one-shot clips had low-energy edges trimmed where present, 3–20 ms fade boundaries, and attenuation only when their peaks exceeded -9 dBFS (-8 dBFS for surface sounds, -14 dBFS for UI). Underwater effects have a 1.8 kHz low-pass filter. Three contact/strike sounds were shortened to isolate a single event. Ambience clips use a 750 ms circular crossfade and at most -8 dBFS peak, without upward normalization. Original Soundbox sources remain unprocessed.

Automated checks passed: 33/33 current source copies match their Soundbox library files by SHA-256; 33/33 processed files decode at 44.1 kHz stereo, are non-silent, and remain below full-scale clipping. All six decoded ambience loop boundary jumps are below the 99th percentile of adjacent sample differences within their clips, and all six Godot ambience imports have looping enabled. Godot 4.7.2 .NET headless import passed for all 33 processed files. The initial import exposed Godot's rejection of the processed 24-bit WAV wrapper; the game-ready effects were changed to 16-bit PCM. Original Soundbox WAVs remain preserved.

The first effect set failed the owner's listening review. Revision 2 has passed technical checks; the owner said only specific revision 2 effects still need work and will select winners from the run folders. `pond_underwater_deep_01` remains quiet, and very short UI/contact clips may need gain or replacement in context. Generated ambience may contain identifiable events that technical checks cannot assess. Select assets by run and filename before integration and tune playback gain in context.

## Integration handoff

Review the audition and mark accepted takes. A later gameplay task can add buses, attenuation and spatial placement, trigger rules, ambience crossfades, and in-game balance. The source and processed paths are available now; no audio triggers are installed.
