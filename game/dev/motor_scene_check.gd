extends SceneTree

# Real FishController/CharacterBody3D integration, separate from the pure C# smoke checks.
func _initialize() -> void:
	call_deferred("_run")

func _new_course() -> Node3D:
	var packed: PackedScene = load("res://scenes/p1_course_fixture.tscn")
	var scene: Node3D = packed.instantiate()
	root.add_child(scene)
	current_scene = scene
	return scene

func _require(condition: bool, label: String) -> bool:
	if condition:
		return true
	printerr("MOTOR_SCENE_FAILED ", label)
	quit(1)
	return false

func _key(code: Key, down: bool) -> void:
	var event := InputEventKey.new()
	event.keycode = code
	event.physical_keycode = code
	event.pressed = down
	Input.parse_input_event(event)

func _run_schedule(fps: int) -> Vector3:
	Engine.max_fps = fps
	var scene := _new_course()
	var fish: CharacterBody3D = scene.get_node("Fish")
	for tick in range(3):
		await physics_frame
	_key(KEY_W, true)
	for tick in range(30):
		await physics_frame
	var start := fish.global_position
	for tick in range(90):
		await physics_frame
	_key(KEY_W, false)
	var result := fish.global_position - start
	current_scene = null
	scene.queue_free()
	await process_frame
	return result

func _run() -> void:
	var low: Vector3 = await _run_schedule(30)
	var high: Vector3 = await _run_schedule(120)
	if not _require(low.distance_to(high) < 0.08, "render cadence changed equal-physics thrust position: low=%s high=%s diff=%s" % [low, high, low.distance_to(high)]):
		return
	if not _require(low.z < -3.5 and absf(low.x) < 0.02 and absf(low.y) < 0.02,
			"W did not propel nose-forward from spawn: low=%s high=%s" % [low, high]):
		return

	Engine.max_fps = 0
	var rest_scene := _new_course()
	var rest_fish: CharacterBody3D = rest_scene.get_node("Fish")
	for tick in range(3):
		await physics_frame
	var rest_start := rest_fish.global_position
	_key(KEY_S, true)
	for tick in range(2):
		await physics_frame
	if not _require(rest_fish.velocity.z > 0.05 and
			rest_fish.global_position.z > rest_start.z + 0.001,
			"S from stationary did not start swimming backward immediately"):
		return
	_key(KEY_S, false)
	current_scene = null
	rest_scene.queue_free()
	await process_frame

	var scene := _new_course()
	var fish: CharacterBody3D = scene.get_node("Fish")
	for tick in range(3):
		await physics_frame
	_key(KEY_W, true)
	for tick in range(30):
		await physics_frame
	_key(KEY_S, true)
	for tick in range(60):
		await physics_frame
		if fish.velocity.length() < 0.01:
			break
	var stopped := fish.global_position
	for tick in range(14):
		await physics_frame
	if not _require(fish.velocity.length() < 0.02 and fish.global_position.distance_to(stopped) < 0.02,
			"held S did not pause for 250 ms after forward stop"):
		return
	for tick in range(16):
		await physics_frame
	if not _require(fish.velocity.z > 0.1 and fish.global_position.z > stopped.z + 0.05,
			"continued S did not swim backward after the 250 ms stop"):
		return
	_key(KEY_W, false)
	_key(KEY_S, false)
	if not _require(fish.global_position.z < -0.5,
			"S reverse path left the course spawn unexpectedly"):
		return
	fish.call("ClearAndDisarmInput")
	for tick in range(3):
		await physics_frame
	var yaw_before := fish.rotation.y
	_key(KEY_A, true)
	_key(KEY_D, true)
	_key(KEY_UP, true)
	_key(KEY_DOWN, true)
	for tick in range(12):
		await physics_frame
	_key(KEY_A, false)
	_key(KEY_UP, false)
	_key(KEY_DOWN, false)
	if not _require(absf(fish.rotation.y - yaw_before) < 0.001 and absf(fish.rotation.x) < 0.001,
			"opposing steering keys failed to cancel"):
		return
	_key(KEY_W, true)
	for tick in range(30):
		await physics_frame
	_key(KEY_W, false)
	_key(KEY_D, false)
	if not _require(fish.global_position.x > 0.2 and fish.rotation.y < -0.5,
			"D failed to turn the body and curve the route"):
		return
	current_scene = null
	scene.queue_free()
	await process_frame

	scene = _new_course()
	fish = scene.get_node("Fish")
	for tick in range(3):
		await physics_frame
	_key(KEY_W, true)
	_key(KEY_UP, true)
	for tick in range(60):
		await physics_frame
	_key(KEY_W, false)
	_key(KEY_UP, false)
	if not _require(fish.global_position.y > 0.4 and fish.rotation.x > 0.5,
			"Up did not pitch nose and produce upward travel"):
		return
	var before_pause := fish.global_position
	paused = true
	await create_timer(0.2).timeout
	if not _require(fish.global_position.distance_to(before_pause) < 0.0001,
			"paused scene advanced fish position"):
		return
	paused = false
	fish.call("ClearAndDisarmInput")
	for tick in range(3):
		await physics_frame
	if not _require(fish.global_position.distance_to(before_pause) < 0.0001,
			"disarmed resume retained motion"):
		return
	current_scene = null
	scene.queue_free()
	await process_frame

	scene = _new_course()
	fish = scene.get_node("Fish")
	for tick in range(3):
		await physics_frame
	fish.position = Vector3(-2.6, -3, 0)
	_key(KEY_W, true)
	for tick in range(120):
		await physics_frame
	_key(KEY_W, false)
	var rock_z := fish.global_position.z
	if not _require(rock_z > -3.4 and rock_z < -2.5,
			"west rock failed to block the authoritative fish body"):
		return
	fish.position = Vector3(0.19, -3, 0)
	fish.call("ClearAndDisarmInput")
	for tick in range(3):
		await physics_frame
	_key(KEY_W, true)
	for tick in range(120):
		await physics_frame
	_key(KEY_W, false)
	var gap_z := fish.global_position.z
	if not _require(gap_z < -6.1 and gap_z >= -36.7,
			"fish failed to traverse the passage within bounds"):
		return
	_key(KEY_W, true)
	for tick in range(360):
		await physics_frame
	_key(KEY_W, false)
	if not _require(fish.global_position.z >= -36.7001 and fish.global_position.z < -24.9,
			"fish failed to advance beyond the obsolete 24.9 m clamp or exceeded the north habitat bound: " + str(fish.global_position.z)):
		return
	print("MOTOR_SCENE_OK render_position_difference=%.4f rock_z=%.3f gap_z=%.3f bound_z=%.3f" % [
		low.distance_to(high), rock_z, gap_z, fish.global_position.z])
	quit()
