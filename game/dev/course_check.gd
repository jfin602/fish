extends SceneTree

func _initialize() -> void:
	call_deferred("verify_course")

func fail(message: String) -> void:
	printerr("COURSE_CHECK_FAILED ", message)
	quit(1)

func sphere_hits(space: PhysicsDirectSpaceState3D, point: Vector3, radius: float, fish: CharacterBody3D) -> Array[Dictionary]:
	var sphere := SphereShape3D.new()
	sphere.radius = radius
	var query := PhysicsShapeQueryParameters3D.new()
	query.shape = sphere
	query.transform = Transform3D(Basis.IDENTITY, point)
	query.collision_mask = 1
	query.exclude = [fish.get_rid()]
	return space.intersect_shape(query, 16)

func mesh_bounds(mesh: MeshInstance3D) -> AABB:
	var local: AABB = mesh.get_aabb()
	var low := Vector3(INF, INF, INF)
	var high := Vector3(-INF, -INF, -INF)
	for x in [0.0, 1.0]:
		for y in [0.0, 1.0]:
			for z in [0.0, 1.0]:
				var point: Vector3 = mesh.global_transform * (local.position + local.size * Vector3(x, y, z))
				low = low.min(point)
				high = high.max(point)
	return AABB(low, high - low)

func verify_course() -> void:
	var packed: PackedScene = load("res://scenes/p1_course_fixture.tscn")
	if packed == null:
		fail("main scene did not load")
		return
	var main: Node3D = packed.instantiate()
	root.add_child(main)
	await physics_frame
	var course: Node3D = main.get_node_or_null("Habitat/MovementCourse")
	var fish: CharacterBody3D = main.get_node_or_null("Fish")
	var shell: Node = main.get_node_or_null("PlayShell")
	var player: Node3D = main.get_node_or_null("Fish/PlayerVisual")
	var decor: Node3D = main.get_node_or_null("Habitat/StarterDecor")
	var ambient: Node3D = main.get_node_or_null("Habitat/AmbientFish")
	var sand: MeshInstance3D = main.get_node_or_null("Habitat/Floor/Mesh")
	var shelf: MeshInstance3D = main.get_node_or_null("Habitat/CourseShelf/Mesh")
	if course == null or fish == null or shell == null or player == null or decor == null or ambient == null or sand == null or shelf == null:
		fail("main scene, P1 shell or required P3 art is incomplete")
		return
	if not player.scene_file_path.ends_with("/quaternius/Fish3.fbx"):
		fail("player is not the selected Fish3 resource")
		return
	if decor.get_child_count() != 4 or ambient.get_child_count() != 7:
		fail("rock/coral or decorative fish population is incomplete")
		return
	for name in ["RockWest", "RockEast", "CoralWest", "CoralEast"]:
		var marker: Node3D = course.get_node_or_null(name)
		var visual: Node3D = decor.get_node_or_null(name)
		if marker == null or visual == null or marker.position != visual.position:
			fail("saved marker and visual disagree: " + name)
			return
		var expected := "/rock_face_01/rock_face_01_2k.gltf" if name.begins_with("Rock") else "/heliopora-coerulea.glb"
		if not visual.scene_file_path.ends_with(expected):
			fail("wrong resource at " + name)
			return
	var sand_material: StandardMaterial3D = sand.material_override
	if sand_material == null or sand_material.albedo_texture == null or not sand_material.albedo_texture.resource_path.ends_with("/coast_sand_01_Diffuse_2k.jpg"):
		fail("Coast Sand 01 is not on the seabed")
		return
	var shelf_material: StandardMaterial3D = shelf.material_override
	if shelf_material == null or shelf_material.albedo_texture != sand_material.albedo_texture or not is_equal_approx(shelf.global_position.y, -4.2) or shelf_material.uv1_scale != Vector3(3, 3, 1) or sand_material.uv1_scale != Vector3(9, 9, 1):
		fail("local course shelf or enlarged seabed lost the selected sand material/tiling")
		return
	var spawn: Marker3D = course.get_node("Spawn")
	var entry: Marker3D = course.get_node("GapEntry")
	var center: Marker3D = course.get_node("GapCenter")
	var exit_marker: Marker3D = course.get_node("GapExit")
	var upper: Marker3D = course.get_node("UpperLandmark")
	var lower: Marker3D = course.get_node("LowerLandmark")
	var west: StaticBody3D = course.get_node("RockWest")
	var east: StaticBody3D = course.get_node("RockEast")
	var post: StaticBody3D = course.get_node("CameraCheckPost")
	if fish.global_position != spawn.global_position or upper.position.y <= 0.5 or lower.position.y >= -1.0 or post.position.z <= spawn.position.z:
		fail("spawn, depth landmarks or camera-check geometry moved unexpectedly")
		return
	var fish_shape: SphereShape3D = fish.get_node("Collision").shape
	var west_collision: CollisionShape3D = west.get_node("Collision")
	var east_collision: CollisionShape3D = east.get_node("Collision")
	var west_shape: BoxShape3D = west_collision.shape
	var east_shape: BoxShape3D = east_collision.shape
	if fish_shape == null or west_shape == null or east_shape == null:
		fail("fish or rock collision shape missing")
		return
	var west_right: float = west.position.x + west_collision.position.x + west_shape.size.x * 0.5
	var east_left: float = east.position.x + east_collision.position.x - east_shape.size.x * 0.5
	var gap_width: float = east_left - west_right
	if gap_width < fish_shape.radius * 2.0 + 1.0 or center.position.x < west_right + fish_shape.radius or center.position.x > east_left - fish_shape.radius:
		fail("rock passage is too narrow for the fish")
		return
	for rock in [west, east]:
		var collision: CollisionShape3D = rock.get_node("Collision")
		var box: BoxShape3D = collision.shape
		var visual_mesh: MeshInstance3D = decor.get_node_or_null(rock.name + "/rock_face_01")
		if visual_mesh == null:
			fail("rock mesh missing at " + rock.name)
			return
		var visible_bounds := mesh_bounds(visual_mesh)
		var collision_bounds := AABB(rock.global_position + collision.position - box.size * 0.5, box.size)
		print("COURSE_ROCK_BOUNDS ", rock.name, " visual=", visible_bounds, " collision=", collision_bounds)
		for axis in [0, 1, 2]:
			if absf(visible_bounds.position[axis] - collision_bounds.position[axis]) > 0.25 or absf(visible_bounds.end[axis] - collision_bounds.end[axis]) > 0.25:
				fail("rock collision differs from visible bounds at " + rock.name)
				return
	var space := main.get_world_3d().direct_space_state
	for marker in [spawn, entry, center, exit_marker]:
		if not sphere_hits(space, marker.global_position, fish_shape.radius, fish).is_empty():
			fail("fish-sized clearance blocked at " + marker.name)
			return
	var blocked := false
	for hit in sphere_hits(space, west.global_position + west_collision.position, fish_shape.radius, fish):
		if hit["collider"] == west:
			blocked = true
	if not blocked:
		fail("rock does not block its visible formation")
		return
	var post_collision: CollisionShape3D = post.get_node_or_null("Collision")
	if post.get_node_or_null("Mesh") == null or post_collision == null or post_collision.shape == null or sphere_hits(space, post.global_position, fish_shape.radius, fish).is_empty():
		fail("camera-check post is not visible and blocking")
		return
	for point in [Vector3(-28.5, 0, 0), Vector3(28.5, 0, 0), Vector3(0, 0, -28.5), Vector3(0, 0, 28.5), Vector3(0, -12.6, 0), Vector3(0, 12.6, 0)]:
		if sphere_hits(space, point, fish_shape.radius, fish).is_empty():
			fail("habitat boundary is missing at " + str(point))
			return
	print("COURSE_CHECK_OK four_resources=loaded shell=present spawn=clear gap_width=", snappedf(gap_width, 0.001), " fish_diameter=", fish_shape.radius * 2.0, " gap_samples=3 rock=blocking bounds=6")
	quit()
