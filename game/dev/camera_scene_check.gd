extends SceneTree

# Exercises the saved camera rig against real course bodies without moving the fish via the view.
func _initialize() -> void:
	call_deferred("_run")

func _require(ok: bool, message: String) -> bool:
	if ok:
		return true
	printerr("CAMERA_SCENE_FAILED ", message)
	quit(1)
	return false

func _tick(count: int = 1) -> void:
	for i in range(count):
		await physics_frame

func _set_fish(fish: CharacterBody3D, place: Vector3, yaw: float, pitch: float) -> void:
	var orientation := Basis(Vector3.UP, yaw) * Basis(Vector3.RIGHT, pitch)
	fish.global_transform = Transform3D(orientation, place)

func _run() -> void:
	var scene: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(scene)
	current_scene = scene
	var fish: CharacterBody3D = scene.get_node("Fish")
	var rig: Node3D = scene.get_node("Fish/CameraRig")
	var camera: Camera3D = scene.get_node("Fish/CameraRig/Camera3D")
	var visual: Node3D = scene.get_node("Fish/PlayerVisual")
	var post: StaticBody3D = scene.get_node("Habitat/MovementCourse/CameraCheckPost")
	await _tick(3)
	if not _require(camera.current and rig.top_level and camera.fov == 68.0 and
			camera.keep_aspect == Camera3D.KEEP_HEIGHT and camera.near <= 0.1,
			"active camera profile or world-space rig missing"):
		return
	if not _require(camera.global_position.z > 3.3 and camera.global_position.y > 1.5 and
			absf(camera.global_position.x - fish.global_position.x) < 0.01,
			"spawn camera is not behind and above the fish"):
		return
	Input.action_press("swim_yaw_right")
	await _tick(8)
	Input.action_release("swim_yaw_right")
	var rear := Vector2(camera.global_position.x - fish.global_position.x,
		camera.global_position.z - fish.global_position.z).normalized()
	var desired_rear := Vector2(fish.global_basis.z.x, fish.global_basis.z.z).normalized()
	var turn_lag := absf(rear.angle_to(desired_rear))
	if not _require(turn_lag > 0.05 and turn_lag < 0.5,
			"camera did not lag slightly behind an actual motor turn"):
		return
	await _tick(45)
	rear = Vector2(camera.global_position.x - fish.global_position.x,
		camera.global_position.z - fish.global_position.z).normalized()
	if not _require(absf(rear.angle_to(desired_rear)) < 0.02,
			"camera did not return behind the fish after the turn"):
		return
	# Freeze only the motor in this fixture; camera still reads the real fish transform.
	fish.set_physics_process(false)
	for yaw in [0.0, 1.57, -2.4]:
		for pitch in [-0.96, 0.0, 0.96]:
			_set_fish(fish, Vector3(0, 0, 0), yaw, pitch)
			rig.call("ResetFollow")
			await _tick(2)
			if not _require(camera.global_transform.is_finite() and
					absf(camera.global_basis.x.y) < 0.001 and
					camera.global_basis.y.y > 0.1,
					"yaw/pitch produced invalid or rolled horizon"):
				return
	var fish_before := fish.global_transform
	visual.rotation.z = 0.55
	await _tick(2)
	if not _require(absf(camera.global_basis.x.y) < 0.001 and
			fish.global_transform.is_equal_approx(fish_before),
			"banked visual affected camera horizon or camera moved fish"):
		return
	visual.rotation.z = 0

	_set_fish(fish, Vector3(3, -2.05, 0), 0, 0)
	rig.call("ResetFollow")
	await _tick(3)
	var blocked_z := camera.global_position.z
	if not _require(blocked_z > 0.7 and blocked_z < 2.3 and
			fish.global_position.is_equal_approx(Vector3(3, -2.05, 0)),
			"post failed to retract camera without moving fish"):
		return
	_set_fish(fish, Vector3(3, -2.05, 1.5), 0, 0)
	rig.call("ResetFollow")
	await _tick(3)
	if not _require(camera.global_position.distance_to(fish.global_position) > 0.8 and
			absf(camera.global_position.x - 3.0) > 0.7 and
			camera.global_position.z < 2.8,
			"near post failed to find a body-clear shoulder view"):
		return
	var shoulder_eye := camera.global_position
	await _tick(10)
	if not _require(camera.global_position.distance_to(shoulder_eye) < 0.02,
			"near-post shoulder view oscillated while fish was still"):
		return
	_set_fish(fish, Vector3(3, -2.05, 0), 0, 0)
	rig.call("ResetFollow")
	await _tick(2)
	post.position.x += 5.0
	await _tick(2)
	var early_recovery := camera.global_position.z
	await _tick(90)
	var recovered_z := camera.global_position.z
	if not _require(early_recovery > blocked_z and early_recovery < 3.7 and
			recovered_z > 3.7 and recovered_z < 3.9,
			"camera failed smooth recovery after post cleared"):
		return
	post.position.x -= 5.0

	paused = true
	await create_timer(0.1).timeout
	var paused_eye := camera.global_position
	_set_fish(fish, Vector3(-4, 2, -4), 0, 0)
	await create_timer(0.1).timeout
	if not _require(camera.global_position.distance_to(paused_eye) < 0.0001,
			"camera moved while paused"):
		return
	paused = false
	await _tick(2)
	if not _require(camera.global_position.distance_to(Vector3(-4, 4, -0.2)) < 0.2,
			"resume retained stale interpolation after teleport"):
		return
	var previous_size := root.get_visible_rect().size
	_set_fish(fish, Vector3(-3.2, 2, -4), 0, 0)
	root.size = Vector2i(960, 540)
	await process_frame
	await _tick(2)
	if not _require(root.get_visible_rect().size != previous_size and
			camera.global_position.distance_to(Vector3(-3.2, 4, -0.2)) < 0.2 and
			camera.fov == 68.0,
			"viewport resize did not reset follow or preserve projection"):
		return
	print("CAMERA_SCENE_OK turn_lag=%.3f blocked_z=%.3f recovery_z=%.3f viewport=%s" % [
		turn_lag, blocked_z, recovered_z, root.get_visible_rect().size])
	quit()
