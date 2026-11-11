extends SceneTree

const FISH_RADIUS := 0.48
const CAMERA_RADIUS := 0.22

func _initialize() -> void:
	call_deferred("_run")

func fail(reason: String) -> void:
	printerr("GEOLOGY_CLEARANCE_FAILED ", reason)
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

func ray(space: PhysicsDirectSpaceState3D, from: Vector3, to: Vector3, fish: CharacterBody3D) -> Dictionary:
	var query := PhysicsRayQueryParameters3D.create(from, to, 1, [fish.get_rid()])
	return space.intersect_ray(query)

func clearance_radius(space: PhysicsDirectSpaceState3D, at: Vector3, accepted_radius: float, fish: CharacterBody3D) -> float:
	var low := accepted_radius
	var high := 2.5
	for step in range(8):
		var middle := (low + high) * 0.5
		if hit(space, at, middle, fish).is_empty():
			low = middle
		else:
			high = middle
	return low

func _run() -> void:
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	for i in range(3):
		await physics_frame
	var fish: CharacterBody3D = main.get_node("Fish")
	fish.set_physics_process(false)
	var rig: Node3D = fish.get_node("CameraRig")
	var generated: Node3D = main.get_node("Habitat/Composition/ReefComposition/Geology/GeneratedRegions")
	if generated.get_child_count() != 5:
		fail("expected five generated bodies")
		return
	var space := main.get_world_3d().direct_space_state
	var structure_count := 0
	var probe_count := 0
	for body in generated.get_children():
		var line: Node3D = body.get_node("Centerline")
		var points: Array[Vector3] = []
		for marker in line.get_children():
			points.append(marker.global_position)
		if points.size() != 16:
			fail(body.name + " route does not have 16 probes")
			return
		var min_fish_clearance := 2.5
		var min_camera_clearance := 2.5
		for at in points:
			min_fish_clearance = minf(min_fish_clearance, clearance_radius(space, at, FISH_RADIUS, fish))
			min_camera_clearance = minf(min_camera_clearance,
				clearance_radius(space, at + Vector3.UP * 2.0, CAMERA_RADIUS, fish))
		if min_fish_clearance < FISH_RADIUS + 0.1 or min_camera_clearance < CAMERA_RADIUS + 0.1:
			fail(body.name + " lacks measured clearance margin fish=" + str(min_fish_clearance) + " camera=" + str(min_camera_clearance))
			return
		for i in range(points.size() - 1):
			for substep in range(3):
				var at := points[i].lerp(points[i + 1], float(substep) / 3.0)
				var found := hit(space, at, FISH_RADIUS, fish)
				if not found.is_empty():
					fail(body.name + " fish blocked at " + str(at) + " by " + str(found.collider.name))
					return
				found = hit(space, at + Vector3.UP * 2.0, CAMERA_RADIUS, fish)
				if not found.is_empty():
					fail(body.name + " camera probe blocked at " + str(at) + " by " + str(found.collider.name))
					return
				probe_count += 1
		var center: Vector3 = points[points.size() / 2]
		var down := ray(space, center, center + Vector3.DOWN * 8.0, fish)
		if down.is_empty() or down.collider != body:
			fail(body.name + " generated floor is not first below center; hit=" + str(down.get("collider", "none")))
			return
		var tangent: Vector3 = (points[9] - points[7]).normalized()
		var side := Vector3(-tangent.z, 0.0, tangent.x).normalized()
		for direction in [side, -side]:
			var wall := ray(space, center, center + direction * 10.0, fish)
			if wall.is_empty() or wall.collider != body:
				fail(body.name + " generated wall missing at center")
				return
		if body.get_meta("geology_kind") != "canyon":
			var up := ray(space, center, center + Vector3.UP * 10.0, fish)
			if up.is_empty() or up.collider != body:
				fail(body.name + " generated ceiling missing at center")
				return
		fish.global_position = points[0]
		for i in range(1, points.size()):
			var destination: Vector3 = points[i]
			if fish.move_and_collide(destination - fish.global_position) != null or fish.global_position.distance_to(destination) > 0.06:
				fail(body.name + " fish sweep blocked at probe " + str(i))
				return
		for i in range(points.size() - 2, -1, -1):
			var destination: Vector3 = points[i]
			if fish.move_and_collide(destination - fish.global_position) != null or fish.global_position.distance_to(destination) > 0.06:
				fail(body.name + " fish return blocked at probe " + str(i))
				return
		fish.global_position = center
		fish.global_rotation = Vector3(0.0, atan2(-tangent.x, -tangent.z), 0.0)
		for i in range(20):
			await physics_frame
		var camera_distance := rig.global_position.distance_to(fish.global_position)
		if not hit(space, rig.global_position, CAMERA_RADIUS, fish).is_empty() or camera_distance < 0.75:
			fail(body.name + " live camera obstruction/retraction failed at " + str(camera_distance))
			return
		print("GEOLOGY_ROUTE id=%s probes=%d min_fish_clearance=%.2f min_camera_clearance=%.2f live_camera=%.2f" % [body.name, points.size(), min_fish_clearance, min_camera_clearance, camera_distance])
		structure_count += 1
	print("GEOLOGY_CLEARANCE_OK structures=%d clear_samples=%d fish_sweeps=out-and-back floor=generated walls=generated ceilings=generated" % [structure_count, probe_count])
	quit()
