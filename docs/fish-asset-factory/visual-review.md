# Visual review

An asset is incomplete when Blender merely generates it. Use Blender MCP viewport screenshots as a core feedback loop, with controlled camera and lighting. Review side/profile, front, three-quarter and silhouette views at useful distances. For a rigged asset, capture deformation poses and loop playback, especially tail extremes, fins, head/eyes and turn poses. Record observations and corrections rather than labeling a file as finished from a script exit.

Ask: **“If this asset were placed into the accepted Fish reference reef, would it look like it belongs to the same world?”** Compare against the [repository style target](references/reef-style-target.png), its [reference record](references/README.md) and the [art-style contract](art-style.md): shape dialect, natural proportion, color balance, scale and readability. Avoid literal copying of a reference object.

After technical GLB validation, inspect the asset in a native Fish Godot view under the actual Forward+ lighting/fog, alongside nearby reef pieces and from the gameplay camera. Check materials, normals, transform, deformation, repetition and performance at representative density. Mark **style approved** only with recorded in-Godot evidence; keep `needs revision`, `not reviewed` and technical status separate. This review does not itself pass P2 or any Roadmap Gate.
