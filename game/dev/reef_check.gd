extends SceneTree

const SOURCES := {
	"rock_face_01": "res://assets/vendor-local/rock_face_01/rock_face_01_2k.gltf",
	"rock_face_02": "res://assets/vendor-local/p2-environment/polyhaven/rock_face_02/rock_face_02_2k.gltf",
	"coastal_cliff_01": "res://assets/vendor-local/p2-environment/polyhaven/coastal_cliff_01/coastal_cliff_01_2k.gltf",
	"boulder_01": "res://assets/vendor-local/p2-environment/polyhaven/boulder_01/boulder_01_2k.gltf",
	"heliopora_coerulea": "res://assets/vendor-local/heliopora-coerulea.glb",
	"acropora_cervicornis": "res://assets/vendor-local/p2-environment/smithsonian/acropora-cervicornis/Acropora_cervicornis-150k-4096_std.glb",
	"acropora_palmata": "res://assets/vendor-local/p2-environment/smithsonian/acropora-palmata/usnm_3968-100k-2048_std.glb",
	"meandrina_meandrites": "res://assets/vendor-local/p2-environment/smithsonian/meandrina-meandrites/usnm_1174917-100k-2048_std.glb",
	"montipora_danae": "res://assets/vendor-local/p2-environment/smithsonian/montipora-danae/usnm_307-100k-2048_std.glb",
}

func _initialize() -> void:
	call_deferred("_run")

func fail(message: String) -> void:
	printerr("REEF_CHECK_FAILED ", message)
	quit(1)

func sphere_hits(space: PhysicsDirectSpaceState3D, center: Vector3, radius: float, fish: CharacterBody3D) -> Array[Dictionary]:
	var sphere := SphereShape3D.new()
	sphere.radius = radius
	var query := PhysicsShapeQueryParameters3D.new()
	query.shape = sphere
	query.transform = Transform3D(Basis.IDENTITY, center)
	query.collision_mask = 1
	query.exclude = [fish.get_rid()]
	return space.intersect_shape(query, 16)

func route_clear(space: PhysicsDirectSpaceState3D, fish: CharacterBody3D, points: Array[Vector3], label: String) -> bool:
	for segment in range(points.size() - 1):
		var count := ceili(points[segment].distance_to(points[segment + 1]) / 0.75)
		for step in range(count + 1):
			var at := points[segment].lerp(points[segment + 1], float(step) / count)
			var fish_hits := sphere_hits(space, at, 0.48, fish)
			if not fish_hits.is_empty():
				fail(label + " fish clearance blocked at " + str(at) + " by " + str(fish_hits[0].get("collider").name))
				return false
			if not sphere_hits(space, at + Vector3.UP * 2.0, 0.22, fish).is_empty():
				fail(label + " camera clearance blocked at " + str(at))
				return false
	return true

func ray_hit(space: PhysicsDirectSpaceState3D, from: Vector3, to: Vector3, fish: CharacterBody3D) -> Dictionary:
	var query := PhysicsRayQueryParameters3D.create(from, to)
	query.collision_mask = 1
	query.exclude = [fish.get_rid()]
	query.hit_back_faces = true
	return space.intersect_ray(query)

func collect_meshes(node: Node, found: Array[MeshInstance3D]) -> void:
	if node is MeshInstance3D:
		found.append(node as MeshInstance3D)
	for child in node.get_children():
		collect_meshes(child, found)

func marker(routes: Node3D, name: String) -> Vector3:
	var found: Marker3D = routes.get_node_or_null(name)
	if found == null:
		fail("missing route marker " + name + " among " + str(routes.get_children().map(func(child: Node) -> String: return child.name)))
		return Vector3.ZERO
	return found.global_position

func usable_margin(at: Vector3, reachable: Dictionary) -> float:
	return minf(minf(at.x - reachable["x-1"], reachable["x1"] - at.x),
		minf(at.z - reachable["z-1"], reachable["z1"] - at.z))

func _run() -> void:
	var packed: PackedScene = load("res://scenes/main.tscn")
	var main: Node3D = packed.instantiate()
	root.add_child(main)
	current_scene = main
	for i in range(3):
		await physics_frame
	var fish: CharacterBody3D = main.get_node("Fish")
	var habitat: Node3D = main.get_node("Habitat")
	var reef: Node3D = habitat.get_node("Composition/ReefComposition")
	var rocks: Node3D = reef.get_node("Rocks")
	var corals: Node3D = reef.get_node("Corals")
	var routes: Node3D = reef.get_node("Routes")
	var seabed: StaticBody3D = habitat.get_node("Seabed")
	var seabed_mesh: MeshInstance3D = habitat.get_node("Seabed/Mesh")
	if seabed_mesh.mesh.get_surface_count() != 1 or seabed_mesh.mesh.surface_get_array_len(0) < 3:
		fail("normal habitat lost its continuous seabed surface")
		return
	var seabed_material: ShaderMaterial = seabed_mesh.mesh.surface_get_material(0)
	if seabed_material == null or seabed_material.shader == null or seabed_material.shader.resource_path != "res://shaders/seabed_blend.gdshader":
		fail("normal habitat lost the blended prepared palette")
		return
	for prefix in ["coast", "shell", "gravel", "rock"]:
		for map_name in ["color", "normal", "roughness"]:
			if seabed_material.get_shader_parameter(prefix + "_" + map_name) == null:
				fail("prepared seabed map missing: " + prefix + " " + map_name)
				return
	if rocks.get_child_count() != 10 or corals.get_child_count() != 8 or habitat.get_node_or_null("MovementCourse") != null or habitat.get_node_or_null("CourseShelf") != null or habitat.get_node_or_null("Landmarks") != null:
		fail("normal reef counts or P1 course separation changed")
		return
	var counts := {}
	var triangles := 0
	for section in [rocks, corals]:
		for piece in section.get_children():
			var source_id: String = piece.get_meta("source_id", "")
			if not SOURCES.has(source_id):
				fail("unprepared source on " + piece.name)
				return
			counts[source_id] = counts.get(source_id, 0) + 1
			var visual: Node3D = piece.get_node("Visual/PreparedVisual")
			if visual.scene_file_path != SOURCES[source_id] or not visual.visible:
				fail("prepared source path or visibility changed on " + piece.name)
				return
			var meshes: Array[MeshInstance3D] = []
			collect_meshes(visual, meshes)
			if meshes.is_empty():
				fail("no imported mesh for " + piece.name)
				return
			var visual_bounds := AABB()
			var first := true
			for mesh in meshes:
				if mesh.mesh == null or not mesh.visible:
					fail("invisible or missing mesh on " + piece.name)
					return
				var world_box: AABB = mesh.global_transform * mesh.mesh.get_aabb()
				visual_bounds = world_box if first else visual_bounds.merge(world_box)
				first = false
				for surface in range(mesh.mesh.get_surface_count()):
					var arrays: Array = mesh.mesh.surface_get_arrays(surface)
					var indices: PackedInt32Array = arrays[Mesh.ARRAY_INDEX]
					var vertices: PackedVector3Array = arrays[Mesh.ARRAY_VERTEX]
					triangles += (indices.size() if not indices.is_empty() else vertices.size()) / 3
			if section == rocks:
				var shape: CollisionShape3D = piece.get_node("Collision")
				var box: BoxShape3D = shape.shape
				if piece.collision_layer != 1 or box == null:
					fail("rock collision missing on " + piece.name)
					return
				var collision_bounds: AABB = shape.global_transform * AABB(-box.size * 0.5, box.size)
				if not visual_bounds.intersects(collision_bounds):
					fail("rock visual and simplified collision disagree on " + piece.name)
					return
	for id in SOURCES:
		if counts.get(id, 0) < 1:
			fail("missing prepared normal-play model: " + id)
			return
	if counts["coastal_cliff_01"] != 1 or triangles > 1750000:
		fail("hero cliff or raw imported triangle count exceeds P4 bound: " + str(triangles))
		return
	var space := main.get_world_3d().direct_space_state
	if not sphere_hits(space, fish.global_position, 0.48, fish).is_empty() or not sphere_hits(space, Vector3(0, 2, 3.8), 0.22, fish).is_empty():
		fail("spawn or camera starts inside reef collision")
		return
	# Measure the authoritative fish's own normal-play clamp after a physics tick.
	# This catches markers that sit inside the mesh/hidden limits but outside play.
	var reachable := {}
	for axis in ["x", "z"]:
		for sign in [-1, 1]:
			fish.global_position = Vector3(sign * 35.0, 8.5, 0) if axis == "x" else Vector3(0, 8.5, sign * 35.0)
			await physics_frame
			var value: float = fish.global_position.x if axis == "x" else fish.global_position.z
			reachable[axis + str(sign)] = value
			if absf(value) < 23.0 or absf(value) > 30.0:
				fail("normal fish reach probe was obstructed or unclamped: " + axis + str(sign) + "=" + str(value))
				return
	fish.global_position = Vector3.ZERO
	fish.set_physics_process(false)
	if usable_margin(Vector3(reachable["x1"] + 0.5, 0, 0), reachable) >= 3.7 or \
		usable_margin(Vector3(0, 0, reachable["z-1"] - 0.5), reachable) >= 3.7:
		fail("reachability regression did not reject mandatory markers beyond normal fish travel")
		return
	if not route_clear(space, fish, [marker(routes, "OpenStart"), marker(routes, "OpenMid"), marker(routes, "OpenFar")], "open"):
		return
	if not route_clear(space, fish, [marker(routes, "TunnelEntry"), marker(routes, "TunnelCenter"), marker(routes, "TunnelExit")], "tunnel"):
		return
	if not route_clear(space, fish, [marker(routes, "ShelterEntry"), marker(routes, "ShelterCenter"), marker(routes, "ShelterExit")], "shelter"):
		return
	if not route_clear(space, fish, [marker(routes, "LowerLandmark"), marker(routes, "VerticalMid"), marker(routes, "UpperLandmark")], "vertical"):
		return
	for lane in [["DepthShallow", "DepthUpper", "DepthMiddle", "DepthLower", "DepthDeep"], ["AltShallow", "AltUpper", "AltMiddle", "AltLower", "AltDeep"]]:
		var points: Array[Vector3] = []
		var floors: Array[float] = []
		for name in lane:
			var at := marker(routes, name)
			var margin := usable_margin(at, reachable)
			if margin < 3.7:
				fail("mandatory marker outside usable normal-play region: " + name + " margin=" + str(margin) + " reach=" + str(reachable))
				return
			var floor_y: float = seabed.call("GetHeightAt", at.x, at.z)
			var clearance: float = routes.get_node(name).get_meta("height_above_floor", 2.0)
			if absf(at.y - floor_y - clearance) > 0.01:
				fail("depth route marker lost floor-relative clearance: " + name)
				return
			points.append(at)
			floors.append(floor_y)
		for i in range(4):
			if floors[i] - floors[i + 1] < 1.8:
				fail("depth route bands are not ordered: " + str(floors))
				return
		if not route_clear(space, fish, points, str(lane[0])):
			return
		fish.global_position = points[0]
		for target in points.slice(1):
			if fish.move_and_collide(target - fish.global_position) != null or fish.global_position.distance_to(target) > 0.05:
				fail("accepted fish collision body could not swim authored lane to " + str(target))
				return
		for target_index in range(points.size() - 2, -1, -1):
			var target: Vector3 = points[target_index]
			if fish.move_and_collide(target - fish.global_position) != null or fish.global_position.distance_to(target) > 0.05:
				fail("accepted fish collision body could not return along authored lane to " + str(target))
				return
		print("REEF_DEPTH_ROUTE lane=%s points=%s floors=%s" % [lane[0], points, floors])
	var tunnel := marker(routes, "TunnelCenter")
	var shelter := marker(routes, "ShelterCenter")
	if ray_hit(space, tunnel, tunnel + Vector3.UP * 5.0, fish).get("collider") != rocks.get_node("TunnelRoof"):
		fail("tunnel lacks a collidable roof over its clear centerline")
		return
	if ray_hit(space, tunnel, tunnel + Vector3.LEFT * 8.0, fish).get("collider") != rocks.get_node("TunnelWest") or ray_hit(space, tunnel, tunnel + Vector3.RIGHT * 8.0, fish).get("collider") != rocks.get_node("TunnelEast"):
		fail("tight tunnel no longer passes between collidable rock walls")
		return
	if ray_hit(space, shelter, shelter + Vector3.UP * 5.0, fish).get("collider") != rocks.get_node("ShelterRoof"):
		fail("shelter lacks a collidable overhang")
		return
	var upper_coral_meshes: Array[MeshInstance3D] = []
	collect_meshes(corals.get_node("HelioporaUpper/Visual/PreparedVisual"), upper_coral_meshes)
	var upper_coral_top := -INF
	for mesh in upper_coral_meshes:
		upper_coral_top = maxf(upper_coral_top, (mesh.global_transform * mesh.mesh.get_aabb()).end.y)
	if marker(routes, "UpperLandmark").y - marker(routes, "LowerLandmark").y < 4.0 or upper_coral_top < 4.0:
		fail("upper/lower landmark height separation or upper coral missing")
		return
	fish.global_position = marker(routes, "TunnelEntry")
	var tunnel_motion: Vector3 = marker(routes, "TunnelExit") - fish.global_position
	if fish.move_and_collide(tunnel_motion) != null or fish.global_position.distance_to(marker(routes, "TunnelExit")) > 0.05:
		fail("accepted fish root could not traverse saved tunnel centerline")
		return
	print("REEF_CHECK_OK rocks=10 corals=8 sources=9 seabed_materials=4 triangles=%d routes=open,tunnel,shelter,vertical,depth,alternate reach=%s marker_margin>=3.7 camera_probe=clear" % [triangles, reachable])
	quit()
