# Pond lighting instability diagnosis

**Disposition: DIAGNOSIS COMPLETE — AWAITING OWNER REPAIR DECISION.** This is an observation and repair analysis, not a production change or a Phase/Gate qualification.

The repeatable broad dark/green band across Pond sediment is primarily the **camera-relative depth fog transition**. At the owner's requested pose, the adult Bluegill was placed in an actual collision with the Pond's `StaticBody3D` ground, and the ordinary `FollowCamera` was snapped to its in-game position, 2.0 m above and 3.8 m behind. The noon band remains when real Sun shadows, procedural sunlight pools, caustics, shafts, or the screen effect are disabled. Disabling depth fog removes the green-to-brown transition at the measured floor column while preserving local log, vegetation, and fish shadows. Volumetric fog and its Pond turbidity field add dark green attenuation; the basin's world-space color pattern adds quieter fixed sediment variation. The problem is their visible composition over a sloped floor, not evidence that the 28 m orthogonal shadow map casts the broad band.

The most useful side-by-side evidence is [ground-contact and isolation](evidence/01_ground_material.png), [controlled camera movement](evidence/03_camera.png), and the [fog-layer comparison](evidence/08_fog_stack.png). The [normal-input swim sequence](evidence/ground-motion.gif) shows how the band travels through the follow-camera view. Full-resolution PNGs and native logs are in ignored `artifacts/pond-lighting-diagnostics/`; compact, path-normalized manifests are in [evidence](evidence/).

The lowest-risk repair to evaluate is a Pond-only change to the depth-fog onset, curve, end distance, and/or fog color so that the camera-distance transition is less visible against sediment. Keep real cover shadows. Test the candidate during ground-contact swimming, other camera heights, twilight/night, and the Ocean return before considering it accepted. No repair has been implemented.

See [the detailed report](report.md) for the measurements, ablations, source trace, remaining unknowns, and three ranked repair options.
