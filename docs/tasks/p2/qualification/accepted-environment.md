# P2 environment identity — candidate pending acceptance

**Status: BLOCKED / NOT ACCEPTED.** This file records the exact P7 candidate for owner review; it is not a claim that V18–V20 or any G2 Gate passed. See [native observation and gaps](native-world.md). Root version is intentionally `0.2.6` until the owner accepts a fully observed candidate.

## Source and platform

- Base: `main` `ed38cb8b24558a78b6108bcc35aa97c067f9c5cd`, with uncommitted P7 source and evidence changes. Normal `run/main_scene` is `res://scenes/main.tscn` (SHA-256 `019681b0cb0509db4504b42c5ee4fdce078d73a51de961e6b0ba39610d1a1093`). Tracked desktop renderer is **Forward+**, `renderer/rendering_method="forward_plus"`; no runtime renderer override was used for final observations.
- Native observation: Linux X11 `:0`, AMD Radeon 880M / Mesa RADV Vulkan 1.4.318, 1920 × 1200 60 Hz, windowed fullscreen mode 3; exclusive fullscreen mode 4 was exercised in the pause panel. Godot `4.7.2.stable.mono.official.ed1daf0bf`, .NET SDK `10.0.403`. Other displays, GPUs and the editor error panel are unobserved.
- The P1 `res://resources/swim_tuning.tres` SHA-256 remains `6d97d70d7b280d2491d08378d1c3e54cdeca1ca14aa245055bec967943e8e2d7`. The accepted motor, camera and input source was not changed. Normal play uses imported Quaternius Fish3 under the P2 visual and retains the P1 course separately as `p1_course_fixture.tscn`.

## World and assets

- `res://scenes/p2_habitat.tscn` SHA-256 `ad2e2c9be6a7441ece279483c60ecbfe0e11a7e37f4bf54d9becffe5ee350f79` instances `res://scenes/p2_reef_composition.tscn` SHA-256 `9e5415cef2ec8c5ca2f8e636952c609280ff99119bf941bf1a33326a3b454eb4`. The normal habitat keeps one 84 × 84 m collidable seabed, five collision-only limits, 10 simplified-collision rock anchors, 8 coral anchors and open/tunnel/shelter/vertical route markers.
- `SeabedSurface.cs` SHA-256 `87924135ddf1bc835efafb8f8879cee22389a5ebc6aaa8049706c475b6b7122d` uses `seabed_blend.gdshader` SHA-256 `1aed79d2ba353a74ec8eccddf1591a58a5388502be0abac87184469a68559b52`. One surface blends Coast Sand 01, Ground060, Gravel041 and Rock063 color textures by world-space regional weights; the largest weight selects that set's normal and roughness maps. The shared surface replaces the square material-zone edges seen at P7 entry. Height/collision geometry is unchanged.
- Prepared runtime models: Rock Face 01, Rock Face 02, Coastal Cliff 01, Boulder 01, *Heliopora coerulea*, *Acropora cervicornis*, *A. palmata*, *Meandrina meandrites* and *Montipora danae*. The ignored [P2 source inventory](../../../../artifacts/assets/source/p2-environment/inventory.json) SHA-256 is `391099f0febe9ce04daa9be8d7614ae32b90f89d6c25f12f57aeda9cdee681d0`; all 34 runtime files across its 10 entries were present. No asset payload was moved into tracked Git source.

## Atmosphere and reaction parameters

- `UnderwaterAtmosphere.cs` SHA-256 `89c50049528e24a3f3a35e408770d0b7e2865adf3f8cd50255a483a00c8f643b` is unchanged from P5. Background `(0.035,0.17,0.22)`, ambient `(0.56,0.73,0.76)` at `0.42`, distance fog `0.0035`, lower fog `0.018` below `-1 m`, Forward+ volumetric fog density `0.024` at `64 m` length with albedo `(0.40,0.72,0.76)`, ambient inject `0.34`, anisotropy `0.18`. Top light pitch/yaw `(-1.05,-0.48)` radians at energy `1.25`. One bounded caustic projection at `0.45` energy and `0.10` drift, 128 world motes at 18 s lifetime, and screen treatment at `0.55 px` warp / `0.007` color variation. The stronger temporary fog trial was not saved.
- `WaterReaction.cs` SHA-256 `ce34142b6b9e40d21f0bf6ec73f72f4898c03736b3ff579414c84a53b1930c58` reads `FishController` presentation speed, effort and turn plus positive speed change; it writes only three GPU emitters. Mapping remains `wake=speed*(0.05+0.26*effort+0.28*acceleration)`, outside-turn addition `speed*0.36*abs(turn)`, bubbles `speed*(0.02+0.14*effort+0.24*acceleration+0.10*abs(turn))`, all clamped `0..1`.
- Left/right ribbons: local tail offsets `(-0.20,-0.04,+0.43)` and `(+0.20,-0.04,+0.43) m`, 16 particles each, `0.55 s` lifetime, `0.22 s` trail, `0.06 m` width, alpha `0.30`; tail bubbles: local `(0,+0.08,+0.43) m`, 12 particles, `0.42 s` lifetime, `0.025 m` sphere radius. Total cap 44 live slots and 86.8 emission slots/s. Emitters use world-space particles, no preprocess and no motor/collision authority. Pause/focus/restart clear them.

Final approval must be appended with the owner's actual judgment, accepted display/platform scope and any post-review repair identity. Do not infer that approval from the resource checks or captured frames.
