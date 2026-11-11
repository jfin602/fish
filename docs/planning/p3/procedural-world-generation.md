# P3 procedural realistic world generation

**Decision date:** 2026-11-10  
**Status:** Owner-approved architecture, implemented through C3 P6; [P7 native qualification](../../tasks/c3-world-gen/closeout.md) failed visual naturalness and alternate-seed route completion. Human owner acceptance remains pending.
**Meaning of realistic:** geologically and compositionally believable landforms, support, routes and habitat placement inside Fish's existing **bright stylized low-poly reef naturalism**. This does not change the art style to photorealism.

## Outcome

Fish uses a **deterministic finite world compiler**, not a runtime random prop spawner.

A new world is compiled from:

```text
seed + generation settings + FAF asset catalog + generator version
```

into a finished baked package containing playable terrain/geology, collision, route data, environmental analysis and deterministic asset placements.

Generation may perform expensive analysis and cleanup once. Normal play loads the baked result and does **not** repeat basin generation, cave construction or placement search.

Good generated candidates can be explicitly promoted into curated **Base Worlds** for fast normal loading and shipping.

## Generation budget and lifecycle

The owner is willing to wait for materially better worlds when explicitly generating a new candidate.

- **Generation target:** complete a P3-sized candidate in **120 seconds or less** on the identified current development machine. This is an acceptance target to measure, not a fact already achieved.
- **Load path:** loading a baked candidate/Base World must skip procedural generation and be materially faster than generating it. Record actual load time rather than inventing a fixed threshold before measurement.
- Generation is deterministic for the same generator version, settings and seed.
- Generation exposes coarse progress stages so a long compile can report useful progress instead of appearing frozen.
- Runtime gameplay is not allowed to silently regenerate a baked world because a resource is missing or incompatible; report the problem.

## World compiler pipeline

### 1. Irregular basin

Generate one continuous open seabed basin.

The useful deepest region is near the world's center, with broad terrain generally rising toward the perimeter, but the world must not read as a mathematically perfect circle, concentric bowl, square room or parallel terrace stack.

Construct a deterministic low-frequency basin domain using:

- an irregular angular/perimeter radius;
- warped X/Z coordinates or equivalent broad domain distortion;
- a smooth center-to-perimeter elevation curve;
- large folds, shoulders, channels and ridges layered after the base basin;
- bounded small contour variation.

The deepest useful sample should remain in the central region, while local secondary depressions/ridges can break symmetry.

### 2. Natural perimeter

The terrain itself forms the understandable world boundary.

Approaching the playable edge should encounter rising shallow banks, reef ridges, cliff/geology, or terrain leaving the usable water column. The player should not perceive four rectangular walls or a perfect circular rim.

Fail-safe containment collision may remain **outside/behind** the natural landform, but normal play must not reach or recognize it as the world edge.

### 3. True 3D geological regions

A heightfield remains suitable for open seabed but is not the universal terrain representation.

Generate arbitrary 3D geological meshes for features that require more than one surface at the same X/Z:

- cave chambers;
- tunnels through ridges;
- natural arches;
- overhangs;
- enclosed swim-through passages;
- canyon walls where useful.

These regions include actual floor, walls and ceiling plus collision. They overlap/blend into the basin through broad geological transition/skirt regions rather than appearing as roof props balanced on boulders.

P3 does **not** require voxel terrain, marching cubes, destructible terrain or runtime CSG editing. Generation-time helpers may be used if their output is baked to ordinary production mesh/collision.

### 4. Route graph before dressing

Establish playable structure before decorative placement.

The compiler defines a deterministic route graph containing at minimum:

- safe spawn/open-water entry;
- shallow-to-middle-to-deep access;
- return route;
- alternate route;
- cave/tunnel traversal;
- useful open-water corridor(s);
- landmark approach points.

Route edges carry approximate clearance/protected volumes. Dressing treats these as exclusion or reduced-density areas rather than filling the world first and hoping routes remain open.

### 5. Environmental analysis

Derive deterministic placement fields from the finished terrain/geology, not from arbitrary XYZ coordinates.

Useful fields include:

- water depth/elevation band;
- surface normal and slope;
- broad curvature/concavity;
- distance to basin perimeter;
- distance to deep core;
- route distance / protected-route mask;
- cave/tunnel entrance distance;
- shelter/exposure approximation;
- nearby large-geology/reef anchor distance.

Open seabed may expose a cheap height/sample API, but arbitrary terrain placement must support actual mesh projection/raycast queries.

### 6. Habitat zones and cluster templates

Generate broad composition zones such as:

- shallow coral garden;
- sea-grass meadow;
- sandy clearing;
- rocky slope;
- cave entrance;
- sheltered reef pocket;
- drop-off reef;
- deep rubble field;
- canyon/rock wall;
- open swim lane.

Zones are deterministic and terrain-driven. They provide art direction without hard-coding every prop transform.

### 7. Natural local placement

**No normal world asset is placed from random XYZ alone.**

Each candidate placement uses:

1. habitat/cluster role;
2. terrain suitability;
3. actual surface projection;
4. local spacing;
5. route/camera exclusions;
6. support/intersection checks.

Use deterministic blue-noise/Poisson-like spacing or an equivalent non-grid distribution inside suitable regions.

Examples:

- sea grass favors shallower, flatter surfaces outside major lanes;
- broad seaweed prefers shallow/mid reef zones with clearance;
- fan coral can favor sloped/exposed surfaces;
- rounded/tube coral favors shelves and reef clusters;
- shells/pebbles favor sand and reef bases;
- rubble favors geological contacts/cave bases;
- large rocks and terrain accents must be visibly supported and partially embedded where appropriate.

FAF terrain A–F are **not the primary floor tiles**. The generated basin/geology owns playable ground. FAF terrain pieces may be used as localized embedded geological/surface features only when their footprint passes support tests and the result does not read as a floating patch.

### 8. Relaxation and repair

After first placement, run bounded cleanup passes.

Detect and repair/reject:

- floating or unsupported terrain/rocks;
- excessive burial;
- major prop intersections;
- blocked routes/cave mouths;
- obvious repeated orientation/spacing;
- extreme density spikes;
- camera-hostile chokepoints;
- decorative pieces outside the intended world surface;
- invalid support samples.

Prefer local move/remove/regenerate operations over restarting the entire seed.

### 9. World scoring

Produce deterministic diagnostics before a candidate is accepted for review.

Record at least:

- basin/perimeter shape statistics;
- deepest-region location;
- shallow/middle/deep coverage;
- route graph connectivity/clearance;
- cave/tunnel count and connectivity;
- environment variant coverage;
- vegetation/coral density by depth;
- support/intersection failures before/after repair;
- repetition/spacing metrics;
- mesh/triangle/material/instance counts;
- generation elapsed time.

A score can reject clearly invalid candidates automatically, but aesthetic owner review still decides whether a world is worth keeping.

## Baked world package

The exact Godot resource mix is implementation-owned, but a baked candidate must have an inspectable manifest similar to:

```text
worlds/<world-id>/
  world.json
  terrain.<baked-resource>
  terrain_collision.<baked-resource>
  geology.<baked-resource>
  geology_collision.<baked-resource>
  placements.<baked-resource>
  routes.<baked-resource>
  environment.<baked-resource>
```

The manifest records:

- world ID;
- seed;
- generator version;
- settings/schema version;
- source asset/catalog version or identities;
- generated resource paths;
- generation duration and machine context when available;
- deterministic content hashes/summary metrics.

Do not duplicate FAF source GLBs into every world package when shared references are sufficient.

## Candidate worlds and Base Worlds

Two lifecycle classes are distinct.

### Generated candidate

A development/user-generated candidate lives in ignored/user data until explicitly retained. It can be regenerated from its seed/version/settings while the matching generator version exists.

### Curated Base World

A world the owner has reviewed and chosen to retain may be **promoted** into the tracked/shipping Base World set.

Promotion:

- copies or saves only the required baked public-safe resources/manifest;
- keeps the source seed and generator version;
- never silently overwrites an existing Base World;
- produces a reviewable repository diff;
- does not imply the generator itself becomes part of every gameplay startup.

Initial Fish builds can ship curated Base Worlds even if player-facing random generation remains a later UX decision.

## Runtime queries

`GetHeightAt(x,z)` may remain useful for open seabed, but it is no longer the universal world contract.

World-facing code should use a boundary capable of supporting both open basin and arbitrary geological mesh, such as:

- sample/project to open seabed;
- raycast/project to actual terrain;
- retrieve surface normal;
- query water depth;
- query generated route/environment metadata.

Names are implementation details; the architectural requirement is that cave ceilings/walls and sloped placement do not have to pretend every X/Z has one terrain Y.

## Performance principles

Generation-time expense is acceptable when it buys better worlds, but runtime cost remains bounded.

- use deterministic local PRNG streams so one subsystem change does not unnecessarily reshuffle unrelated systems;
- cache/reuse intermediate grids/fields rather than raycasting every asset against the full scene repeatedly;
- batch surface queries spatially;
- use shared meshes/materials/MultiMesh where appropriate in baked output;
- keep decorative collision absent unless gameplay requires it;
- bake generated mesh/collision so normal load avoids generator work;
- instrument phase timings before optimizing blindly.

## Correction ordering

The [c3-world-gen](../../tasks/c3-world-gen/README.md) correction supersedes the old P3 assumption that procedural world generation is deferred.

At the owner's selected P3 checkpoint, run **c3-world-gen before c3-visibility**. Visibility/discovery tuning should be performed against the generated/baked world geometry rather than calibrated to the obsolete placed/test world.

## Deferred

This architecture does not include:

- infinite streaming worlds;
- runtime terrain destruction/editing;
- voxel sandbox terrain;
- marching-cubes world simulation;
- dynamic erosion simulation;
- procedural ecology/food/predator population;
- save-game persistence;
- network world sharing;
- automatic shipping of every generated candidate.

Those require later decisions.
