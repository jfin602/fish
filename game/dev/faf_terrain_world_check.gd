extends SceneTree

const IDS := ["FAF-TERRAIN-001A", "FAF-TERRAIN-001B", "FAF-TERRAIN-001C",
	"FAF-TERRAIN-001D", "FAF-TERRAIN-001E", "FAF-TERRAIN-001F"]
const SCENES := {
	"FAF-TERRAIN-001A": "res://assets/faf/terrain/terrain_001a_open_sand.glb",
	"FAF-TERRAIN-001B": "res://assets/faf/terrain/terrain_001b_rolling.glb",
	"FAF-TERRAIN-001C": "res://assets/faf/terrain/terrain_001c_reef_shelf.glb",
	"FAF-TERRAIN-001D": "res://assets/faf/terrain/terrain_001d_slope.glb",
	"FAF-TERRAIN-001E": "res://assets/faf/terrain/terrain_001e_drop_off.glb",
	"FAF-TERRAIN-001F": "res://assets/faf/terrain/terrain_001f_rocky_floor.glb",
}
const SURFACES := {
	"FAF-TERRAIN-001A": 1, "FAF-TERRAIN-001B": 1,
	"FAF-TERRAIN-001C": 3, "FAF-TERRAIN-001D": 2,
	"FAF-TERRAIN-001E": 3, "FAF-TERRAIN-001F": 3,
}
const LANES := [
	["DepthShallow", "DepthUpper", "DepthMiddle", "DepthLower", "DepthDeep"],
	["AltShallow", "AltUpper", "AltMiddle", "AltLower", "AltDeep"],
	["OpenStart", "OpenMid", "OpenFar"],
	["TunnelEntry", "TunnelCenter", "TunnelExit"],
	["ShelterEntry", "ShelterCenter", "ShelterExit"],
	["LowerLandmark", "VerticalMid", "UpperLandmark"],
]

func _initialize() -> void:
	call_deferred("_run")

func fail(message: String) -> void:
	printerr("FAF_TERRAIN_WORLD_FAILED ", message)
	quit(1)

func _run() -> void:
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	for i in range(3):
		await physics_frame
	var habitat: Node3D = main.get_node("Habitat")
	var seabed: StaticBody3D = habitat.get_node("Seabed")
	var terrain: Node3D = habitat.get_node("Composition/ReefComposition/Terrain")
	var visuals: Node3D = terrain.get_node("Visuals")
	var routes: Node3D = habitat.get_node("Composition/ReefComposition/Routes")
	var fish: CharacterBody3D = main.get_node("Fish")
	var camera: Camera3D = main.get_node("Fish/CameraRig/Camera3D")
	var atmosphere: Node3D = main.get_node("UnderwaterAtmosphere")
	if visuals.get_child_count() != 51 or terrain.ChunkCount != 51:
		fail("authored visual coverage count changed")
		return
	var counts := {}
	var zones := {}
	var yaw_count := 0
	for wrapper in visuals.get_children():
		var id: String = wrapper.get_meta("faf_id", "")
		if not IDS.has(id) or str(wrapper.get_meta("faf_scene_path", "")) != SCENES.get(id, ""):
			fail("untracked terrain identity on " + wrapper.name)
			return
		counts[id] = counts.get(id, 0) + 1
		zones[wrapper.get_meta("seam_zone", "")] = true
		if absf(wrapper.rotation.y) > 0.1:
			yaw_count += 1
		var found_mesh := false
		for imported in wrapper.get_children():
			if imported is CollisionObject3D:
				fail("FAF visual acquired collision")
				return
			for child in imported.get_children():
				if child is MeshInstance3D and child.mesh != null:
					found_mesh = true
					if child.mesh.get_surface_count() != SURFACES[id]:
						fail("FAF material-region count changed on " + wrapper.name)
						return
					for surface in range(child.mesh.get_surface_count()):
						if child.mesh.surface_get_material(surface) == null:
							fail("FAF material missing on " + wrapper.name)
							return
		if not found_mesh:
			fail("missing draped mesh on " + wrapper.name)
			return
	for id in IDS:
		if counts.get(id, 0) < 5:
			fail("variant missing meaningful coverage: " + id + "=" + str(counts.get(id, 0)))
			return
	var seam_zones: Node3D = terrain.get_node("SeamZones")
	if zones.size() < 15 or yaw_count < 40 or seam_zones.get_child_count() != 4:
		fail("placement variation or P3/P4 seam zones missing")
		return
	for marker in seam_zones.get_children():
		if absf(marker.global_position.y - seabed.GetHeightAt(marker.global_position.x, marker.global_position.z)) > 0.01:
			fail("seam zone is not floor anchored: " + marker.name)
			return
	var samples := 0
	var distinct := 0
	var max_delta := 0.0
	for lane in LANES:
		for segment in range(lane.size() - 1):
			var a: Vector3 = routes.get_node(lane[segment]).global_position
			var b: Vector3 = routes.get_node(lane[segment + 1]).global_position
			for step in range(6):
				var at := a.lerp(b, float(step) / 5.0)
				var floor_y: float = seabed.GetHeightAt(at.x, at.z)
				var visible_y: float = terrain.GetVisibleHeightAt(at.x, at.z, seabed)
				max_delta = maxf(max_delta, visible_y - floor_y)
				if visible_y < floor_y - 0.001 or visible_y - floor_y > 0.45:
					fail("visual/collision route floor mismatch at " + str(at) + " delta=" + str(visible_y - floor_y))
					return
				if visible_y - floor_y > 0.035:
					distinct += 1
				samples += 1
	if distinct < 8:
		fail("FAF geometry does not visibly participate along depth routes: " + str(distinct))
		return
	if not camera.current or not is_equal_approx((fish.get_node("Collision").shape as SphereShape3D).radius, 0.48) or fish.collision_mask != 1 or fish.collision_layer != 1:
		fail("fish/camera/collision identity changed")
		return
	if habitat.get_node("HiddenLimits").get_child_count() != 5 or habitat.get_node("Seabed/Collision").shape == null or atmosphere.SurfaceHeight <= atmosphere.DeepReferenceHeight:
		fail("P2 bounds or water anchors changed")
		return
	print("FAF_TERRAIN_WORLD_OK chunks=%d ids=%s seam_zones=%d route_samples=%d visible=%d max_delta=%.3f collision=P2 camera=shared" % [visuals.get_child_count(), counts, zones.size(), samples, distinct, max_delta])
	quit()
