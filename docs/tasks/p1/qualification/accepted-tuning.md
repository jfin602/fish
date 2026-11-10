# Accepted P1 baseline for P2 — 2026-11-09

The owner accepted the current post-c1 swimming/control candidate as sufficient for progression. This is a reproducible source baseline, not a claim that every personal `user://` preference selects the same profile. Source is clean `main` at `27c4debf0ac04f9cb2eff927c1e31c6b604c0b60`, root version `0.1.9` before this documentation and `0.2.0` entry-version change. No gameplay/configuration source is changed in this closeout; the dirty closeout consists of documentation and `package.json` only. Native and automated evidence boundaries are in [swimming acceptance](swimming-acceptance.md), [P5 display/assets](native-display-and-assets.md), and [c1 native controls](../../c1-bindings/qualification/native-controls.md).

## Selected controls

The fresh profile starts with `builtin:keyboard` / **Keyboard Only** (`ControlPresetStore.CurrentId` default). `user://control-presets.json` can restore a previously selected built-in or custom profile, and a legacy `user://preferences.cfg` can migrate its keyboard/mouse mode and mouse settings. The accepted built-in set is **Keyboard Only**, **Mouse Only**, and **Hybrid Swim**; there is no universal saved user selection in tracked source. The observed native c1 session also saved and restored custom profiles in an isolated test profile. `ControlPreset.cs` defines these immutable templates; saved custom snapshots can be edited in Pause → Control Presets.

| Profile | Thrust | Slow | Fish yaw/pitch | Pause/options |
| --- | --- | --- | --- | --- |
| Keyboard Only | W | S | A/D yaw; Up/Down pitch | Escape; visible Options button |
| Mouse Only | Left mouse hold | Right mouse hold | Captured mouse X yaw; Y pitch | Middle mouse or Escape; visible Options button |
| Hybrid Swim | W or left mouse | S or right mouse | A/D yaw, Up/Down pitch plus captured mouse X/Y | Middle mouse or Escape; visible Options button |

Built-in mouse X/Y are not inverted; sensitivity is `0.12°` per captured pixel, adjustable `0.04–0.40°/px`. Legacy preferences control built-in sensitivity and Y inversion; custom profiles store their own X/Y assignment, inversion and sensitivity. The mouse adapter rejects one-event motion above 300 px, caps queued motion at 80 px/axis/tick and ignores the first motion after capture. F6 cycles profiles in debug builds when no available profile binds F6; Pause selection is the normal path. Switching clears queued/held input and rearms after release without resetting the fish. Keyboard mode Left/Right arrows pan the camera; Shift + arrows hold/orbit it, while bare Up/Down pitch the fish. Mouse mode W/S and A/D pan the camera, with Shift holding the offset; these keys do not propel the fish. The wheel zooms in either mode.

## Authoritative movement and space

`game/resources/swim_tuning.tres`, attached to `Fish` in `game/scenes/main.tscn`, is the saved tuning used by `SwimMotor.Step`. Units are metres, seconds, degrees, and inverse seconds as applicable. Values below are the actual resource entries, not older prompt estimates.

| Field | Value | Unit / role |
| --- | ---: | --- |
| MaxSpeed | 4.0 | m/s |
| Acceleration | 12.0 | m/s² |
| CoastDeceleration | 1.6 | m/s² |
| BrakeDeceleration | 9.0 | m/s² |
| YawRateDegrees | 150.0 | °/s |
| PitchRateDegrees | 95.0 | °/s |
| PitchLimitDegrees | 55.0 | ° |
| VelocityAlignment | 4.5 | s⁻¹ |

`FishController` is the sole authoritative `CharacterBody3D`: floating motion, `MoveAndSlide`, fish yaw/pitch, speed and velocity. Slow wins over thrust and never reverses. Its `Fish/Collision` is a sphere of radius `0.48 m`, on layer/mask 1. Script swim bounds are X/Z `±24.9 m`, Y `±9 m`; the current box's enclosing static collision is X/Z `±28.5 m`, Y `±12.6 m`. `movement_course.tscn` is retained as a development/regression fixture. `StarterWorld` adds imported visual children; visual animation and camera never move the collider.

## Third-person camera

`FollowCamera.cs` reads the fish and uses `SwimTuning` values below. Offset is centered (`CameraShoulderOffset = 0`), with a steady world-up horizon and sphere-cast obstruction handling. Zoom starts at `3.8 m`; the wheel steps by `0.4 m` within `2.2–6.5 m`. `ResetZoom` exists in source but has no call site in the current runtime, so changing profiles does not claim to reset zoom.

| Field | Value | Unit |
| --- | ---: | --- |
| CameraDistance | 3.8 | m |
| CameraShoulderOffset | 0.0 | m |
| CameraHeight | 2.0 | m |
| CameraLookAhead | 0.8 | m |
| CameraFieldOfView | 68.0 | vertical degrees |
| CameraFollowResponse | 4.5 | s⁻¹ |
| CameraRecoveryResponse | 3.0 | s⁻¹ |
| CameraPanYawRateDegrees / CameraPanPitchRateDegrees | 90.0 / 65.0 | °/s |
| CameraPanYawLimitDegrees / CameraPanPitchLimitDegrees | 85.0 / 35.0 | ° |
| CameraPanReturnResponse | 5.0 | s⁻¹ |
| CameraZoomMinimumDistance / CameraZoomMaximumDistance / CameraZoomStep | 2.2 / 6.5 / 0.4 | m |

Source constants: camera collision probe radius `0.22 m`, obstacle clearance `0.12 m`, preferred minimum distance `0.75 m`, teleport reset threshold `2.5 m`, pitch-height fraction `0.35`, near clip `0.08 m`. These are in `FollowCamera.cs`, not separate resource fields.

## Fish visual motion and assets

`FishVisualMotion.cs` samples the imported `Armature|Swim` clip at the Root, Spine3 and Tail bones, blends its last 12% into the start, scales activity from effort/speed and adds visual-only turn bend/lean. Its saved resource fields are:

| Field | Value | Unit |
| --- | ---: | --- |
| SwimCadenceHz / GlideCadenceHz | 1.15 / 0.35 | Hz |
| SwimIntensity / GlideIntensity | 1.0 / 0.12 | weight |
| SwimActivityResponse / GlideEasingResponse | 7.0 / 3.0 | s⁻¹ |
| TurnBendDegrees / TurnLeanDegrees | 8.0 / 6.0 | ° |
| TurnResponse | 8.0 | s⁻¹ |

The provisional player is Quaternius Animated Fish Pack `Fish3` / `ClownFish`, visual scale `0.12` and Y facing correction `180°`. Two Rock Face 01 instances (`0.55` scale), Coast Sand 01 maps (floor UV 9×9, shelf 3×3), and two Smithsonian *Heliopora coerulea* instances (`3.5` and `3.0` scale) form the starter course. Seven other Quaternius fish are decorative. Exact ignored source/working paths, hashes, license and credit terms are in [the manifest](../../../assets/p1-starter-manifest.md) and [local installation guide](../../../../game/assets/README.md).

## Display, lifecycle and source identity

`game/project.godot` uses Compatibility rendering, a 1280×720 viewport, `canvas_items` stretch with `expand`, and fresh-profile mode `4` (Godot ExclusiveFullscreen request). `DisplayPreferences` offers Fullscreen (`Window.ModeEnum.ExclusiveFullscreen`) and Windowed fullscreen (`Window.ModeEnum.Fullscreen`), defaults to Fullscreen, and stores choices in `user://preferences.cfg`; a launch override does not save over the choice. X11 native evidence showed full-display borderless presentation for both requests, without proof of exclusive monitor ownership. Pause/focus loss clears input, releases pointer capture and requires release/repress to rearm. Controls persist separately in `user://control-presets.json`; neither file belongs to a fish save.

Source SHA-256 at accepted clean HEAD: `swim_tuning.tres` `6d97d70d7b280d2491d08378d1c3e54cdeca1ca14aa245055bec967943e8e2d7`; `main.tscn` `8461741933c12ced142ac26fc43cff6a391df7328d8c6e4ccd9872cc4434f403`; `project.godot` `5592eeaa48769682a2a0d90707a95582a27bdc1fa6d17eb0636f18574fded4ed`; `ControlPreset.cs` `0aa846e47cff062ee67f04e105e033289bc3ca69269cf3fbc1adf822c440952a`; `ControlPresetStore.cs` `226aa7e7521956c9b15d88a43089c8e7229239f2cd87e974591a1e83bbe44cd9`; `KeyboardSwimInput.cs` `6393dde433c5a04e2a8e4446f251149630bb3c180e98935bb85b829263f4dc80`; `MouseSwimInput.cs` `ab478a00e5006d60f1e0297a4f0745dad64bc7cc2c0e8cb5a9ebdb02ab8586b7`; `PlayShell.cs` `afb9d3c1dc10d42e98663ae7154cd8dd74ece338410c6f331d7859de1779da7f`; `DisplayPreferences.cs` `5a1bed15c0f6296c7f0b6275da094205133b54e0b661d3f08dcb57e4b9be39fd`; `SwimMotor.cs` `cbbf3d833bd961a1c002f1cda9a7b99b47bab3e8e9b6301d4b59ca3a50bb06d3`; `FishController.cs` `8bffbcdd85547b48a571445a0983b0324e63f9f926893502495c12ab0b0ce9da`; `FollowCamera.cs` `a77e8162f98b4b4a37aa1e60c962887c472fe1f7a369c364eac95a247000cc81`; `FishVisualMotion.cs` `56bc66eba4ae0788ad5a83377af9dceb689d79d82b96c32ed97cb45604445e7a`. Asset hashes remain in the manifest. P2 begins at root `0.2.0`, with its first authored prompt assigned `0.2.1`; no `0.1.10`/`0.1.11` checkpoints are fabricated.
