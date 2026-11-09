extends SceneTree

# Real FishController/CharacterBody3D integration, separate from the pure C# smoke checks.
func _initialize() -> void:
	call_deferred("_run")

func _new_course() -> Node3D:
	var packed: PackedScene = load("res://scenes/main.tscn")
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

func _run_schedule(fps: int) -> Vector3:
	Engine.max_fps = fps
	var scene := _new_course()
	var fish: CharacterBody3D = scene.get_node("Fish")
	await physics_frame
	Input.action_press("swim_thrust")
	for tick in range(90):
		await physics_frame
	Input.action_release("swim_thrust")
	var result := fish.global_position
	current_scene = null
	scene.queue_free()
	await process_frame
	return result

func _run() -> void:
	var low: Vector3 = await _run_schedule(30)
	var high: Vector3 = await _run_schedule(120)
	if not _require(low.distance_to(high) < 0.08, "render cadence changed equal-physics thrust position"):
		return
	if not _require(low.z < -3.5 and absf(low.x) < 0.02 and absf(low.y) < 0.02,
			"W did not propel nose-forward from spawn"):
		return

	Engine.max_fps = 0
	var scene := _new_course()
	var fish: CharacterBody3D = scene.get_node("Fish")
	await physics_frame
	Input.action_press("swim_thrust")
	for tick in range(30):
		await physics_frame
	Input.action_press("swim_slow")
	for tick in range(30):
		await physics_frame
	Input.action_release("swim_thrust")
	Input.action_release("swim_slow")
	if not _require(fish.velocity.length() < 0.02 and fish.global_position.z < -0.5,
			"S failed to slow W without reversing"):
		return
	fish.call("ClearAndDisarmInput")
	for tick in range(3):
		await physics_frame
	var yaw_before := fish.rotation.y
	Input.action_press("swim_yaw_left")
	Input.action_press("swim_yaw_right")
	Input.action_press("swim_pitch_up")
	Input.action_press("swim_pitch_down")
	for tick in range(12):
		await physics_frame
	Input.action_release("swim_yaw_left")
	Input.action_release("swim_pitch_up")
	Input.action_release("swim_pitch_down")
	if not _require(absf(fish.rotation.y - yaw_before) < 0.001 and absf(fish.rotation.x) < 0.001,
			"opposing steering keys failed to cancel"):
		return
	Input.action_press("swim_thrust")
	for tick in range(30):
		await physics_frame
	Input.action_release("swim_thrust")
	Input.action_release("swim_yaw_right")
	if not _require(fish.global_position.x > 0.2 and fish.rotation.y < -0.5,
			"D failed to turn the body and curve the route"):
		return
	current_scene = null
	scene.queue_free()
	await process_frame

	scene = _new_course()
	fish = scene.get_node("Fish")
	await physics_frame
	Input.action_press("swim_thrust")
	Input.action_press("swim_pitch_up")
	for tick in range(60):
		await physics_frame
	Input.action_release("swim_thrust")
	Input.action_release("swim_pitch_up")
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
	await physics_frame
	fish.position = Vector3(-2.6, -3, 0)
	Input.action_press("swim_thrust")
	for tick in range(120):
		await physics_frame
	Input.action_release("swim_thrust")
	var rock_z := fish.global_position.z
	if not _require(rock_z > -3.4 and rock_z < -2.5,
			"west rock failed to block the authoritative fish body"):
		return
	fish.position = Vector3(0.19, -3, 0)
	fish.call("ClearAndDisarmInput")
	for tick in range(3):
		await physics_frame
	Input.action_press("swim_thrust")
	for tick in range(120):
		await physics_frame
	Input.action_release("swim_thrust")
	var gap_z := fish.global_position.z
	if not _require(gap_z < -6.1 and gap_z >= -8.3,
			"fish failed to traverse the passage within bounds"):
		return
	Input.action_press("swim_thrust")
	for tick in range(90):
		await physics_frame
	Input.action_release("swim_thrust")
	if not _require(fish.global_position.z >= -8.3001 and fish.global_position.z <= -8.29,
			"fish exceeded the north habitat bound"):
		return
	print("MOTOR_SCENE_OK render_position_difference=%.4f rock_z=%.3f gap_z=%.3f bound_z=%.3f" % [
		low.distance_to(high), rock_z, gap_z, fish.global_position.z])
	quit()
