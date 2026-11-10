extends SceneTree

# Source-bound check through the saved shell, fish input and camera rig.
func _initialize() -> void:
	call_deferred("_run")

func _require(ok: bool, reason: String) -> bool:
	if ok:
		return true
	printerr("ADAPTIVE_FOLLOW_FAILED ", reason)
	quit(1)
	return false

func _tick(count: int = 1) -> void:
	for i in range(count):
		await physics_frame

func _key(code: Key, down: bool) -> void:
	var event := InputEventKey.new()
	event.keycode = code
	event.physical_keycode = code
	event.pressed = down
	root.push_input(event, true)

func _motion(relative: Vector2) -> void:
	var event := InputEventMouseMotion.new()
	event.relative = relative
	root.push_input(event, true)

func _wheel(direction: MouseButton) -> void:
	var event := InputEventMouseButton.new()
	event.button_index = direction
	event.pressed = true
	root.push_input(event, true)

func _button(shell: Node, name: String) -> Button:
	return shell.find_child(name, true, false) as Button

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
	if not _require(mode.item_count == 5, "Adaptive Follow absent from selector"):
		return
	_button(shell, "OptionsButton").pressed.emit()
	mode.select(4)
	mode.item_selected.emit(4)
	if not _require(fish.call("GetActivePresetId") == "builtin:adaptive-follow", "selection failed"):
		return
	_button(shell, "ResumeButton").pressed.emit()
	await _tick(3)
	_motion(Vector2(300, 300)) # Capture warp.
	await _tick()
	_motion(Vector2(1, 0))
	await _tick()
	if not _require(absf(fish.call("GetLastYaw")) < 0.001, "1px noise steered fish"):
		return
	_motion(Vector2(25, -20))
	await _tick()
	if not _require(fish.call("GetLastYaw") > 0.2 and fish.call("GetLastPitch") > 0.2,
			"deliberate mouse steering lost"):
		return
	_key(KEY_A, true)
	_key(KEY_UP, true)
	await _tick()
	if not _require(fish.call("GetLastYaw") < 0 and fish.call("GetLastPitch") > 0,
			"keyboard precision steering lost yaw=%s pitch=%s held=%s" % [fish.call("GetLastYaw"), fish.call("GetLastPitch"), fish.call("GetHeldInputCount")]):
		return
	_key(KEY_A, false)
	_key(KEY_UP, false)
	_key(KEY_C, true)
	await _tick()
	_motion(Vector2(80, 40))
	await _tick()
	if not _require(fish.call("GetLastYaw") == 0.0 and fish.call("GetLastPitch") == 0.0,
			"free-look motion reached fish motor"):
		return
	_key(KEY_ESCAPE, true)
	await process_frame
	_key(KEY_ESCAPE, false)
	if not _require(paused, "C-held pause failed"):
		return
	_button(shell, "ResumeButton").pressed.emit()
	_key(KEY_C, false)
	await _tick(3)
	if not _require(fish.call("GetHeldInputCount") == 0 and
			fish.call("GetLastYaw") == 0.0 and fish.call("GetLastThrust") == 0.0,
			"C-held pause left input stuck"):
		return
	_key(KEY_C, true)
	await _tick()
	shell.notification(Node.NOTIFICATION_APPLICATION_FOCUS_OUT)
	await process_frame
	if not _require(paused, "C-held focus loss did not pause"):
		return
	_button(shell, "ResumeButton").pressed.emit()
	_key(KEY_C, false)
	await _tick(3)
	_key(KEY_A, true)
	await _tick()
	if not _require(fish.call("GetHeldInputCount") == 1 and fish.call("GetLastYaw") < 0,
			"focus recovery did not rearm fresh keyboard input"):
		return
	_key(KEY_A, false)
	await _tick()
	# Still fish and place it in open water for camera-only observation.
	fish.set_physics_process(false)
	fish.global_position = Vector3(0, 5, 0)
	rig.call("ResetFollow")
	await _tick(45)
	var original_rotation := fish.rotation
	fish.rotate_y(deg_to_rad(90.0))
	await _tick()
	var early_rear := fish.global_position - camera.global_position
	var turned_forward := -fish.global_basis.z
	if not _require(Vector2(early_rear.x, early_rear.z).normalized().dot(
			Vector2(turned_forward.x, turned_forward.z).normalized()) < 0.7,
			"camera did not lag a fish turn"):
		return
	await _tick(80)
	var settled_rear := fish.global_position - camera.global_position
	if not _require(Vector2(settled_rear.x, settled_rear.z).normalized().dot(
			Vector2(turned_forward.x, turned_forward.z).normalized()) > 0.9,
			"camera did not recenter after a fish turn"):
		return
	fish.rotation = original_rotation
	await _tick(80)
	var near_distance := camera.global_position.distance_to(fish.global_position)
	_wheel(MOUSE_BUTTON_WHEEL_DOWN)
	await _tick(45)
	if not _require(camera.global_position.distance_to(fish.global_position) > near_distance + 0.2,
			"Adaptive Follow wheel zoom lost"):
		return
	_wheel(MOUSE_BUTTON_WHEEL_UP)
	await _tick(45)
	var fish_transform := fish.global_transform
	var fish_velocity := fish.velocity
	var start_rear := (fish.global_position - camera.global_position).normalized()
	_key(KEY_C, true)
	await _tick()
	var entry_eye := camera.global_position
	_motion(Vector2(75, 0))
	await _tick()
	if not _require(camera.global_position.distance_to(entry_eye) < 1.5,
			"free-look entry jumped"):
		return
	for i in range(19):
		_motion(Vector2(75, 0))
		await _tick()
	await _tick(45)
	var half_rear := (fish.global_position - camera.global_position).normalized()
	if not _require(Vector2(start_rear.x, start_rear.z).normalized().dot(
			Vector2(half_rear.x, half_rear.z).normalized()) < -0.8 and
			fish.global_transform.is_equal_approx(fish_transform) and
			fish.velocity.is_equal_approx(fish_velocity),
			"C did not orbit around fish independently"):
		return
	var before_release := camera.global_position
	_key(KEY_C, false)
	await _tick()
	if not _require(camera.global_position.distance_to(before_release) < 1.5,
			"free-look release jumped"):
		return
	await _tick(100)
	var recentered := (fish.global_position - camera.global_position).normalized()
	if not _require(Vector2(start_rear.x, start_rear.z).normalized().dot(
			Vector2(recentered.x, recentered.z).normalized()) > 0.9,
			"camera did not return behind fish"):
		return
	_key(KEY_C, true)
	await _tick()
	_motion(Vector2(75, 0))
	await _tick()
	_key(KEY_ESCAPE, true)
	await process_frame
	_key(KEY_ESCAPE, false)
	var paused_eye := camera.global_position
	_motion(Vector2(75, 75))
	if not _require(paused and camera.global_position.is_equal_approx(paused_eye),
			"pause changed camera"):
		return
	_button(shell, "ResumeButton").pressed.emit()
	_key(KEY_C, false)
	await _tick(2)
	_motion(Vector2(300, 300))
	await _tick()
	if not _require(camera.global_position.distance_to(paused_eye) < 1.5,
			"recapture jumped or consumed queued motion"):
		return
	shell.notification(Node.NOTIFICATION_APPLICATION_FOCUS_OUT)
	await process_frame
	if not _require(paused, "focus loss did not pause"):
		return
	_button(shell, "ResumeButton").pressed.emit()
	await _tick(2)
	if not _require(fish.call("GetLastYaw") == 0.0 and fish.call("GetLastThrust") == 0.0,
			"focus recovery left swim input"):
		return
	_button(shell, "OptionsButton").pressed.emit()
	mode.select(3)
	mode.item_selected.emit(3)
	if not _require(fish.call("GetActivePresetId") == "builtin:hybrid2" and
			fish.global_transform.is_equal_approx(fish_transform),
			"switch to Hybrid 2 changed fish state"):
		return
	var switch_eye := camera.global_position
	_button(shell, "ResumeButton").pressed.emit()
	await _tick()
	if not _require(camera.global_position.distance_to(switch_eye) < 1.5,
			"preset switch jumped camera"):
		return
	print("ADAPTIVE_FOLLOW_OK steering filter keyboard free-look recenter pause focus switch")
	quit()
