extends SceneTree

# Exercises Hybrid 2 through the saved shell, resolver, motor and camera rig.
func _initialize() -> void:
	call_deferred("_run")

func _require(ok: bool, reason: String) -> bool:
	if ok:
		return true
	printerr("HYBRID2_CAMERA_FAILED ", reason)
	quit(1)
	return false

func _tick(count: int = 1) -> void:
	for i in range(count):
		await physics_frame

func _motion(relative: Vector2) -> void:
	var event := InputEventMouseMotion.new()
	event.relative = relative
	root.push_input(event, true)

func _key(code: Key, down: bool) -> void:
	var event := InputEventKey.new()
	event.keycode = code
	event.physical_keycode = code
	event.pressed = down
	Input.parse_input_event(event)

func _button(code: MouseButton, down: bool) -> void:
	var event := InputEventMouseButton.new()
	event.button_index = code
	event.pressed = down
	root.push_input(event, true)

func _orbit(axis: Vector2, steps: int, camera: Camera3D) -> void:
	for i in range(steps):
		var prior := camera.global_position
		var prior_right := camera.global_basis.x
		_motion(axis * 75.0)
		await _tick()
		if not _require(camera.global_transform.is_finite() and
				camera.global_position.distance_to(prior) < 1.5 and
				camera.global_basis.x.dot(prior_right) > 0.5,
				"orbit pole crossing jumped or rolled at step %s" % i):
			return

func _run() -> void:
	var scene: Node3D = load("res://scenes/p1_course_fixture.tscn").instantiate()
	root.add_child(scene)
	current_scene = scene
	var fish: CharacterBody3D = scene.get_node("Fish")
	var rig: Node3D = scene.get_node("Fish/CameraRig")
	var camera: Camera3D = rig.get_node("Camera3D")
	var shell: Node = scene.get_node("PlayShell")
	var mode: OptionButton = shell.find_child("InputModeOption", true, false)
	await _tick(3)
	if not _require(mode.item_count == 5, "Hybrid 2 missing from selector"):
		return
	(shell.find_child("OptionsButton", true, false) as Button).pressed.emit()
	mode.select(3)
	mode.item_selected.emit(3)
	if not _require(fish.call("GetActivePresetId") == "builtin:hybrid2", "Hybrid 2 selection failed"):
		return
	(shell.find_child("ResumeButton", true, false) as Button).pressed.emit()
	await _tick(3)
	if not _require(not paused, "Hybrid 2 did not resume"):
		return
	# Keep the fish still in open water so a complete vertical orbit can be observed.
	fish.set_physics_process(false)
	fish.global_position = Vector3(0, 5, 0)
	rig.call("ResetFollow")
	await _tick(3)
	var fish_transform := fish.global_transform
	var fish_velocity := fish.velocity
	_motion(Vector2(300, 200)) # Capture warp is ignored.
	await _tick()
	if not _require(fish.call("GetLastYaw") == 0.0 and fish.call("GetLastPitch") == 0.0,
			"capture warp changed fish intent"):
		return
	var start := (camera.global_position - fish.global_position).normalized()
	await _orbit(Vector2(1, 0), 20, camera)
	await _tick(50)
	var half_yaw := (camera.global_position - fish.global_position).normalized()
	if not _require(Vector2(half_yaw.x, half_yaw.z).normalized().dot(
			Vector2(start.x, start.z).normalized()) < -0.9 and camera.global_transform.is_finite(),
			"horizontal orbit did not reach the opposite side: dot=%s start=%s half=%s armed=%s" % [
				half_yaw.dot(start), start, half_yaw, fish.call("GetInputArmed")]):
		return
	await _orbit(Vector2(1, 0), 20, camera)
	await _tick(50)
	var full_yaw := (camera.global_position - fish.global_position).normalized()
	if not _require(Vector2(full_yaw.x, full_yaw.z).normalized().dot(
			Vector2(start.x, start.z).normalized()) > 0.9,
			"horizontal orbit did not complete 360 degrees"):
		return
	var vertical_start := (camera.global_position - fish.global_position).normalized()
	await _orbit(Vector2(0, 1), 20, camera)
	await _tick(50)
	if not _require((camera.global_position - fish.global_position).normalized().dot(vertical_start) < -0.85 and
			camera.global_transform.is_finite(), "vertical orbit did not cross both poles"):
		return
	await _orbit(Vector2(0, 1), 20, camera)
	await _tick(180)
	var completed := (camera.global_position - fish.global_position).normalized()
	var still_eye := camera.global_position
	await _tick(60)
	if not _require(completed.dot(vertical_start) > 0.9 and
			camera.global_position.distance_to(still_eye) < 0.01 and
			fish.global_transform.is_equal_approx(fish_transform) and
			fish.velocity.is_equal_approx(fish_velocity),
			"vertical 360, non-recentering or fish isolation failed: dot=%s drift=%s fish=%s velocity=%s" % [
				completed.dot(vertical_start), camera.global_position.distance_to(still_eye),
				fish.global_transform.is_equal_approx(fish_transform), fish.velocity.is_equal_approx(fish_velocity)]):
		return
	# Pause and recapture must discard pending motion while keeping the fish untouched.
	_key(KEY_ESCAPE, true)
	await process_frame
	_key(KEY_ESCAPE, false)
	var paused_eye := camera.global_position
	_motion(Vector2(75, 75))
	await create_timer(0.05).timeout
	if not _require(paused and camera.global_position.is_equal_approx(paused_eye),
			"paused camera consumed queued motion"):
		return
	(shell.find_child("ResumeButton", true, false) as Button).pressed.emit()
	fish.set_physics_process(true)
	await _tick(3)
	fish.set_physics_process(false)
	_motion(Vector2(300, 300))
	await _tick(25)
	if not _require(camera.global_position.distance_to(paused_eye) < 0.02 and
			fish.global_transform.is_equal_approx(fish_transform),
			"recapture caused camera spike or fish movement: eye=%s fish=%s" % [
				camera.global_position.distance_to(paused_eye), fish.global_transform.is_equal_approx(fish_transform)]):
		return
	_motion(Vector2(75, 0))
	await _tick(30)
	if not _require(camera.global_position.distance_to(paused_eye) > 0.1,
			"fresh motion did not orbit after recapture"):
		return
	await _tick(180)
	var before_focus := camera.global_position
	shell.notification(Node.NOTIFICATION_APPLICATION_FOCUS_OUT)
	await process_frame
	if not _require(paused, "focus loss did not pause Hybrid 2"):
		return
	_motion(Vector2(75, 75))
	(shell.find_child("ResumeButton", true, false) as Button).pressed.emit()
	fish.set_physics_process(true)
	await _tick(3)
	fish.set_physics_process(false)
	_motion(Vector2(300, 300))
	await _tick(25)
	if not _require(camera.global_position.distance_to(before_focus) < 0.02 and
			fish.global_transform.is_equal_approx(fish_transform),
			"focus recapture caused a camera spike or fish movement"):
		return
	# A live preset switch clears the old orbit queue without writing fish state.
	var before_switch := fish.global_transform
	# Selection goes through the paused shell, as in normal use.
	_button(MOUSE_BUTTON_MIDDLE, true)
	await process_frame
	_button(MOUSE_BUTTON_MIDDLE, false)
	mode.select(0)
	mode.item_selected.emit(0)
	if not _require(fish.call("GetActivePresetId") == "builtin:keyboard" and
			fish.global_transform.is_equal_approx(before_switch), "switch changed fish state"):
		return
	print("HYBRID2_CAMERA_OK horizontal=360 vertical=360 poles fish-isolated pause-focus-recapture switch")
	quit()
