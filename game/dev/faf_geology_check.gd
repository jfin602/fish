extends SceneTree

const ROCKS := ["FAF-ROCK-001A", "FAF-ROCK-001B", "FAF-ROCK-001C", "FAF-ROCK-001D"]
const FISH_RADIUS := 0.48
const CAMERA_RADIUS := 0.22

func _initialize() -> void:
	call_deferred("_run")

func fail(message: String) -> void:
	printerr("FAF_GEOLOGY_FAILED ", message)
	quit(1)

func hit(space: PhysicsDirectSpaceState3D, at: Vector3, radius: float, fish: CharacterBody3D) -> Dictionary:
	var shape := SphereShape3D.new()
	shape.radius = radius
	var query := PhysicsShapeQueryParameters3D.new()
	query.shape = shape
	query.transform = Transform3D(Basis.IDENTITY, at)
	query.collision_mask = 1
	query.exclude = [fish.get_rid()]
	var found := space.intersect_shape(query, 1)
	return found[0] if not found.is_empty() else {}

func point(routes: Node3D, name: String) -> Vector3:
	return (routes.get_node(name) as Marker3D).global_position

func collect_boxes(node: Node, boxes: Array[Dictionary]) -> void:
	if node is MeshInstance3D and node.mesh != null:
		boxes.append({"box": node.global_transform * node.mesh.get_aabb(), "name": str(node.get_path())})
	for child in node.get_children():
		collect_boxes(child, boxes)

func box_distance(box: AABB, at: Vector3) -> float:
	var end := box.end
	var nearest := Vector3(clampf(at.x, box.position.x, end.x),
		clampf(at.y, box.position.y, end.y), clampf(at.z, box.position.z, end.z))
	return at.distance_to(nearest)

func clear_line(space: PhysicsDirectSpaceState3D, fish: CharacterBody3D, points: Array[Vector3], id: String) -> bool:
	for i in range(points.size() - 1):
		var count := ceili(points[i].distance_to(points[i + 1]) / 0.5)
		for step in range(count + 1):
			var at := points[i].lerp(points[i + 1], float(step) / count)
			var found := hit(space, at, FISH_RADIUS, fish)
			if not found.is_empty():
				fail(id + " fish blocked at " + str(at) + " by " + str(found.get("collider").name))
				return false
			found = hit(space, at + Vector3.UP * 2.0, CAMERA_RADIUS, fish)
			if not found.is_empty():
				fail(id + " camera blocked at " + str(at) + " by " + str(found.get("collider").name))
				return false
	fish.global_position = points[0]
	for destination in points.slice(1):
		if fish.move_and_collide(destination - fish.global_position) != null or fish.global_position.distance_to(destination) > 0.05:
			fail(id + " real fish collision sweep failed at " + str(destination))
			return false
	for j in range(points.size() - 2, -1, -1):
		var destination: Vector3 = points[j]
		if fish.move_and_collide(destination - fish.global_position) != null or fish.global_position.distance_to(destination) > 0.05:
			fail(id + " return sweep failed at " + str(destination))
			return false
	print("FAF_GEOLOGY_ROUTE id=%s points=%s" % [id, points])
	return true

func _run() -> void:
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	for i in range(3):
		await physics_frame
	var fish: CharacterBody3D = main.get_node("Fish")
	fish.set_physics_process(false)
	var reef: Node3D = main.get_node("Habitat/Composition/ReefComposition")
	var geology: Node3D = reef.get_node("Geology")
	var visuals: Node3D = geology.get_node("Visuals")
	var collision: Node3D = geology.get_node("Collision")
	var geo_routes: Node3D = geology.get_node("Routes")
	var p2_routes: Node3D = reef.get_node("Routes")
	var counts := {}
	var formations := {}
	if visuals.get_child_count() != geology.PieceCount or collision.get_child_count() != 0:
		fail("placement/collision count mismatch")
		return
	for wrapper in visuals.get_children():
		var id: String = wrapper.get_meta("faf_id", "")
		counts[id] = counts.get(id, 0) + 1
		formations[wrapper.get_meta("formation", "")] = true
		var solids := wrapper.find_children("*", "StaticBody3D", true, false)
		if solids.size() != 1 or solids[0].name != "Solid":
			fail("geology solid missing or duplicated: " + wrapper.name)
			return
		var body: StaticBody3D = solids[0]
		var solid_shape: CollisionShape3D = body.get_node_or_null("Shape")
		if body.get_parent() != wrapper or body.transform != Transform3D.IDENTITY or body.collision_layer != 1 or body.collision_mask != 0 or solid_shape == null or solid_shape.disabled or solid_shape.transform != Transform3D.IDENTITY:
			fail("geology solid transform/layer mismatch: " + wrapper.name)
			return
		if (id.begins_with("FAF-ROCK") and not solid_shape.shape is ConvexPolygonShape3D) or (id.begins_with("FAF-TERRAIN") and not solid_shape.shape is ConcavePolygonShape3D):
			fail("geology shape does not match asset family: " + wrapper.name)
			return
	for id in ROCKS:
		if counts.get(id, 0) < 5:
			fail("rock variant is not repeated: " + id + " count=" + str(counts.get(id, 0)))
			return
	for id in ["FAF-TERRAIN-001C", "FAF-TERRAIN-001E", "FAF-TERRAIN-001F"]:
		if counts.get(id, 0) < 2:
			fail("structural terrain absent: " + id)
			return
	if formations.size() < 6:
		fail("geological landmarks not distributed")
		return
	var space := main.get_world_3d().direct_space_state
	if not clear_line(space, fish, [point(p2_routes,"TunnelEntry"), point(p2_routes,"TunnelCenter"), point(p2_routes,"TunnelExit")], "GEO-WEST"):
		return
	if not clear_line(space, fish, [point(p2_routes,"ShelterEntry"), point(p2_routes,"ShelterCenter"), point(geo_routes,"CaveTurn"), point(geo_routes,"CaveSide"), point(geo_routes,"CaveTurn"), point(p2_routes,"ShelterExit")], "GEO-CAVE"):
		return
	if not clear_line(space, fish, [point(geo_routes,"CanyonEntry"), point(geo_routes,"CanyonCenter"), point(geo_routes,"CanyonExit")], "GEO-CANYON"):
		return
	if not clear_line(space, fish, [point(geo_routes,"CanyonExit"), point(geo_routes,"CanyonReturn"), point(p2_routes,"DepthDeep")], "GEO-RETURN"):
		return
	var turn := point(geo_routes,"CaveTurn")
	for angle in range(12):
		var at := turn + Vector3(cos(float(angle) * TAU / 12.0), 0, sin(float(angle) * TAU / 12.0)) * 1.25
		if not hit(space, at, FISH_RADIUS, fish).is_empty():
			fail("cave turn circle blocked at " + str(at))
			return
	var canyon := point(geo_routes,"CanyonCenter")
	for offset in [-2.5, -1.5, 0.0, 1.5, 2.5]:
		if not hit(space, canyon + Vector3(offset, 0, 0), FISH_RADIUS, fish).is_empty():
			fail("deep corridor lacks sampled 5 m fish clearance")
			return
	for name in ["DepthShallow", "DepthUpper", "DepthMiddle", "DepthLower", "DepthDeep", "AltShallow", "AltUpper", "AltMiddle", "AltLower", "AltDeep"]:
		if not hit(space, point(p2_routes,name), FISH_RADIUS, fish).is_empty():
			fail("P2 depth marker blocked: " + name)
			return
	var rig: Node3D = fish.get_node("CameraRig")
	fish.global_position = point(p2_routes, "TunnelCenter")
	fish.global_rotation = Vector3(0, PI / 2.0, 0)
	for i in range(20):
		await physics_frame
	var tunnel_camera_distance := rig.global_position.distance_to(fish.global_position)
	if not hit(space, rig.global_position, CAMERA_RADIUS, fish).is_empty():
		fail("camera probe entered tunnel obstruction")
		return
	fish.global_position = point(p2_routes, "OpenStart")
	fish.global_rotation = Vector3.ZERO
	for i in range(90):
		await physics_frame
	var open_camera_distance := rig.global_position.distance_to(fish.global_position)
	if open_camera_distance < 3.0:
		fail("camera did not recover in open water: tunnel=" + str(tunnel_camera_distance) + " open=" + str(open_camera_distance))
		return
	if not hit(space, rig.global_position, CAMERA_RADIUS, fish).is_empty():
		fail("camera recovered inside obstruction")
		return
	print("FAF_GEOLOGY_OK rock_counts=%s formations=%d corridor_sample_width=5 cave_turn_diameter=2.50 camera_tunnel=%.2f camera_recovery=%.2f collision=%d" % [counts, formations.size(), tunnel_camera_distance, rig.global_position.distance_to(fish.global_position), geology.PieceCount])
	quit()
