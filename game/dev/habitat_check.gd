extends SceneTree

# P2 topology integration against the saved normal scene and the preserved P1 fixture.
func _initialize() -> void:
	call_deferred("_run")

func fail(message: String) -> void:
	printerr("HABITAT_CHECK_FAILED ", message)
	quit(1)

func tick(count: int = 1) -> void:
	for i in range(count):
		await physics_frame

func sphere_hits(space: PhysicsDirectSpaceState3D, at: Vector3, radius: float, fish: CharacterBody3D) -> Array[Dictionary]:
	var shape := SphereShape3D.new()
	shape.radius = radius
	var query := PhysicsShapeQueryParameters3D.new()
	query.shape = shape
	query.transform = Transform3D(Basis.IDENTITY, at)
	query.collision_mask = 1
	query.exclude = [fish.get_rid()]
	return space.intersect_shape(query, 16)

func seabed_hit(space: PhysicsDirectSpaceState3D, fish: CharacterBody3D, limits: StaticBody3D, x: float, z: float) -> Dictionary:
	var query := PhysicsRayQueryParameters3D.new()
	query.from = Vector3(x, 14, z)
	query.to = Vector3(x, -12, z)
	query.collision_mask = 1
	query.exclude = [fish.get_rid(), limits.get_rid()]
	query.hit_back_faces = true
	return space.intersect_ray(query)

func key(code: Key, down: bool) -> void:
	var event := InputEventKey.new()
	event.keycode = code
	event.physical_keycode = code
	event.pressed = down
	Input.parse_input_event(event)

func _run() -> void:
	var main_pack: PackedScene = load("res://scenes/main.tscn")
	var fixture_pack: PackedScene = load("res://scenes/p1_course_fixture.tscn")
	var habitat_pack: PackedScene = load("res://scenes/p2_habitat.tscn")
	if main_pack == null or fixture_pack == null or habitat_pack == null:
		fail("normal, habitat or P1 fixture scene missing")
		return
	var main: Node3D = main_pack.instantiate()
	root.add_child(main)
	current_scene = main
	await tick(3)
	var habitat: Node3D = main.get_node_or_null("Habitat")
	var fish: CharacterBody3D = main.get_node_or_null("Fish")
	var camera: Camera3D = main.get_node_or_null("Fish/CameraRig/Camera3D")
	var player: Node3D = main.get_node_or_null("Fish/PlayerVisual")
	var seabed: StaticBody3D = main.get_node_or_null("Habitat/Seabed")
	var limits: StaticBody3D = main.get_node_or_null("Habitat/HiddenLimits")
	var spawn: Marker3D = main.get_node_or_null("Habitat/Spawn")
	var composition: Node3D = main.get_node_or_null("Habitat/Composition")
	if habitat == null or fish == null or camera == null or player == null or seabed == null or limits == null or spawn == null or composition == null:
		fail("P2 normal scene missing fish, camera or habitat parts")
		return
	if not habitat.scene_file_path.ends_with("/p2_habitat.tscn") or main.get_node_or_null("Habitat/MovementCourse") != null or main.get_node_or_null("Habitat/Landmarks") != null or main.get_node_or_null("Habitat/CourseShelf") != null:
		fail("normal main still contains the P1 course or wrong habitat instance")
		return
	var fish_collision: CollisionShape3D = fish.get_node("Collision")
	var fish_shape: SphereShape3D = fish_collision.shape
	if not player.scene_file_path.ends_with("/quaternius/Fish3.fbx") or not camera.current or fish.collision_layer != 1 or fish.collision_mask != 1 or fish_shape == null or not is_equal_approx(fish_shape.radius, 0.48):
		fail("accepted Fish3, camera or fish collision changed: player=%s camera=%s layer=%s mask=%s radius=%s" % [player.scene_file_path, camera.current, fish.collision_layer, fish.collision_mask, fish_shape.radius if fish_shape != null else -1])
		return
	if fish.get("Tuning").resource_path != "res://resources/swim_tuning.tres" or not fish.global_position.is_equal_approx(spawn.global_position):
		fail("accepted tuning or safe spawn identity changed")
		return
	var visual: MeshInstance3D = seabed.get_node("Mesh")
	var collision: CollisionShape3D = seabed.get_node("Collision")
	var shape: ConcavePolygonShape3D = collision.shape
	if visual.mesh == null or visual.mesh.get_surface_count() != 1 or shape == null or not shape.backface_collision or seabed.collision_layer != 1 or limits.collision_layer != 1:
		fail("continuous blended mesh or two-sided collision missing")
		return
	var material: ShaderMaterial = visual.mesh.surface_get_material(0)
	if material == null or material.shader == null or material.shader.resource_path != "res://shaders/seabed_blend.gdshader":
		fail("continuous seabed blend shader missing")
		return
	for prefix in ["coast", "shell", "gravel", "rock"]:
		for map_name in ["color", "normal", "roughness"]:
			if material.get_shader_parameter(prefix + "_" + map_name) == null:
				fail("prepared material channel missing: " + prefix + " " + map_name)
				return
	if composition.get_child_count() != 1 or composition.get_node_or_null("ReefComposition") == null or limits.get_child_count() != 5:
		fail("P4 composition instance or hidden limit set changed")
		return
	for child in limits.get_children():
		if child is not CollisionShape3D or child.shape == null:
			fail("hidden limit is not collision only")
			return
	var space := main.get_world_3d().direct_space_state
	if not sphere_hits(space, spawn.global_position, 0.48, fish).is_empty() or not sphere_hits(space, Vector3(0, 2, 3.8), 0.22, fish).is_empty():
		fail("fish or camera spawn volume blocked")
		return
	var heights: Array[float] = []
	for point in [Vector2(0, 0), Vector2(8, 6), Vector2(-10, -8), Vector2(30, 0), Vector2(0, -30)]:
		var hit := seabed_hit(space, fish, limits, point.x, point.y)
		if hit.is_empty() or hit["collider"] != seabed:
			fail("seabed collision hole at " + str(point) + " hit=" + str(hit))
			return
		heights.append(hit["position"].y)
	if maxf(heights[3], heights[4]) - heights[0] < 3.0 or absf(heights[1] - heights[2]) < 0.15:
		fail("seabed has no meaningful continuous height variation: " + str(heights))
		return
	key(KEY_W, true)
	await tick(30)
	key(KEY_W, false)
	if fish.position.z > -0.5 or fish.velocity.length() < 0.1:
		fail("accepted fish motor did not move through open P2 spawn water")
		return
	fish.set_physics_process(false)
	fish.global_position = Vector3(0, 0, 0)
	var down := fish.move_and_collide(Vector3(0, -10, 0))
	if down == null or down.get_collider() != seabed or fish.global_position.y < -5.2:
		fail("fish escaped through seabed from above")
		return
	fish.global_position = Vector3(0, -9, 0)
	var underside := fish.move_and_collide(Vector3(0, 10, 0))
	if underside == null or underside.get_collider() != seabed or fish.global_position.y > -5.9:
		fail("seabed underside did not block upward passage")
		return
	fish.global_position = Vector3(0, 2, -27)
	var edge := fish.move_and_collide(Vector3(0, 0, -15))
	if edge == null or fish.global_position.z < -37.2:
		fail("basin rim did not block horizontal edge escape")
		return
	fish.global_position = Vector3(0, 8, 0)
	var upper := fish.move_and_collide(Vector3(0, 4, 0))
	if upper == null or upper.get_collider() != limits or fish.global_position.y > 9.0:
		fail("hidden upper limit did not contain the fish")
		return
	var fixture: Node3D = fixture_pack.instantiate()
	if fixture.get_node_or_null("Habitat/MovementCourse/CameraCheckPost") == null or fixture.get_node_or_null("Habitat/CourseShelf") == null:
		fail("P1 course fixture lost saved qualification geometry")
		return
	fixture.free()
	print("HABITAT_CHECK_OK scene=p2_habitat palette=4 spawn=clear seabed_samples=5 underside=blocked rim=blocked upper=blocked fixture=preserved")
	quit()
