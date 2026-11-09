# P1 movement course

`main.tscn` instances `movement_course.tscn` under `Habitat/MovementCourse`. The saved course contains markers, three solid obstacles and visible native target/landmark meshes. `StarterWorld.cs` checks the four local starter resources, installs Fish3 as the player visual, places two Rock Face 01 meshes and two *Heliopora coerulea* meshes at the saved course markers, and applies Coast Sand 01 to the Fish-authored floor. The selected source geometry and textures remain in ignored `assets/vendor-local/`; neither saved scene embeds them. Seven other Quaternius fish are stationary decoration. The P0 `FishController` and P1 `PlayShell` still own movement and pause/display behavior.

## Working scale and route

Coordinates are Godot metres, with the spawn at `(0, 0, 0)` and forward through the course toward negative Z. The floor and ceiling centers are Y `-4.2` and `4.2`; their surfaces are at approximately `-4.1` and `4.1`. The four invisible retaining walls are at X/Z `±9.5`, making a 19 m square habitat. The provisional controller clamps its center to X/Z `±8.3` and Y `±3`. The player collision is a sphere of radius `0.48` m (diameter `0.96` m). Imported Fish3 is visually scaled to `0.12`, about `0.95` m long, independently of that collision body. The camera remains the provisional child of the fish at `(0, 2.1, 6)`.

| Saved course element | Position | Use |
| --- | --- | --- |
| ApproachTarget | `(0, -1.5, -2.1)` | Nonblocking yellow target for approach and stopping practice. |
| GapEntry / GapCenter / GapExit | `(0.19, -3, -2.8)` / `(0.19, -3, -4.45)` / `(0.19, -3, -6.1)` | Fish-sized samples through the rocky passage. |
| RockWest / RockEast | `(-2.6, -4.1, -3.6)` / `(2.6, -4.1, -3.6)` | Two Rock Face 01 formations at scale `0.55`, with separate static box collision. Circle either rock or pass between them. |
| UpperLandmark / LowerLandmark | `(-5.2, 2.2, -6.4)` / `(5.2, -2.4, -6.3)` | Nonblocking cyan and pink depth markers. |
| CoralWest / CoralEast | `(-5.7, -4, -4.5)` / `(5.6, -4, -3.4)` | Decorative Smithsonian coral at scale `3.5` / `3.0`; no route-blocking collision. |
| CameraCheckPost | `(3, -2.05, 2.7)` | Visible 0.55 m radius, 4.1 m tall solid post near the spawn for later camera obstruction checks. |

Each rock box is `2.724 × 1.96 × 2.105` m, offset `(0.189, 0.962, -0.854)` m from its visual root. Headless mesh-bound inspection found all six corresponding box faces within `0.001` m of the imported visual bounds. The horizontal passage is `2.476` m wide between collision faces, leaving `1.516` m beyond the fish diameter. The rocks meet the seabed; the center route at Y `-3` is open. Native collision simplifies the detailed rock silhouette deliberately. The coral and target spheres do not block movement.

The reproducible reset is to quit the test session from the P1 pause panel and relaunch `./tools/dev run --display-mode=windowed-fullscreen` from the repository root. The saved main scene always starts at the spawn; no gameplay reset or persistent living-save system is present. For headless composition checks, use `./tools/dev check`, `./tools/dev inspect-scene game/scenes/movement_course.tscn`, and `./tools/dev inspect-course`. The latter checks real resource identities, shell presence, visual/marker placement, rock mesh/collision bounds and physics clearance at spawn, along the passage and at all four retaining walls. It does not establish native visibility, camera comfort or swimming feel. P5 should inspect both display modes and the route in a native window, including the rock passage, depth markers, coral and post.
