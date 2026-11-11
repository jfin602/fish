extends SceneTree

# Sends input through the saved scene, its C# fish, and its actual pause controls.
func _initialize() -> void:
	call_deferred("_run")

func _require(ok: bool, message: String) -> bool:
	if ok:
		return true
	printerr("INPUT_LIFECYCLE_FAILED ", message)
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
	Input.parse_input_event(event)

func _mouse_button(button: MouseButton, down: bool, at: Vector2 = Vector2.ZERO) -> void:
	var event := InputEventMouseButton.new()
	event.button_index = button
	event.pressed = down
	event.position = at
	event.global_position = at
	if at == Vector2.ZERO:
		Input.parse_input_event(event)
		root.push_input(event, true)
	else:
		root.push_input(event, true)

func _motion(relative: Vector2) -> void:
	var event := InputEventMouseMotion.new()
	event.relative = relative
	root.push_input(event, true)

func _run() -> void:
	var packed: PackedScene = load("res://scenes/main.tscn")
	var scene: Node3D = packed.instantiate()
	root.add_child(scene)
	current_scene = scene
	var fish: CharacterBody3D = scene.get_node("Fish")
	var camera: Camera3D = scene.get_node("Fish/CameraRig/Camera3D")
	var shell: Node = scene.get_node("PlayShell")
	await _tick(3)
	# The shell builds its controls in C#, so discover them by stable names.
	var mode: OptionButton = shell.find_child("InputModeOption", true, false)
	var resume: Button = shell.find_child("ResumeButton", true, false)
	var options: Button = shell.find_child("OptionsButton", true, false)
	var sensitivity: HSlider = shell.find_child("MouseSensitivitySlider", true, false)
	var invert: CheckButton = shell.find_child("InvertMouseYToggle", true, false)
	if not _require(mode != null and resume != null and options != null and
			sensitivity != null and invert != null,
			"pause controls absent from actual scene"):
		return
	if not _require(mode.selected == 0, "fresh profile did not start in keyboard mode"):
		return
	if not _require(options.visible, "mouse-only entry to options is absent"):
		return
	var keyboard_zoom_before := camera.global_position.distance_to(fish.global_position)
	_mouse_button(MOUSE_BUTTON_WHEEL_UP, true, Vector2(100, 100))
	_mouse_button(MOUSE_BUTTON_WHEEL_UP, false, Vector2(100, 100))
	await _tick(28)
	if not _require(camera.global_position.distance_to(fish.global_position) < keyboard_zoom_before - 0.2,
			"wheel zoom did not work in keyboard mode"):
		return
	await _tick(45)
	var keyboard_camera_center := camera.global_position
	var keyboard_fish_basis := fish.global_basis
	_key(KEY_LEFT, true)
	await _tick(18)
	_key(KEY_LEFT, false)
	if not _require(camera.global_position.distance_to(keyboard_camera_center) > 0.3 and
			fish.global_basis.is_equal_approx(keyboard_fish_basis),
			"bare Left did not pan the keyboard camera independently of the fish"):
		return
	await _tick(75)
	if not _require(camera.global_position.distance_to(keyboard_camera_center) < 0.12,
			"keyboard camera did not return after Left release"):
		return
	_key(KEY_SHIFT, true)
	_key(KEY_RIGHT, true)
	_key(KEY_UP, true)
	await _tick(20)
	_key(KEY_RIGHT, false)
	_key(KEY_UP, false)
	await _tick(75)
	var keyboard_held_camera := camera.global_position
	await _tick(20)
	if not _require(camera.global_position.distance_to(keyboard_held_camera) < 0.03 and
			keyboard_held_camera.distance_to(keyboard_camera_center) > 0.4 and
			fish.global_basis.is_equal_approx(keyboard_fish_basis),
			"Shift+arrows did not hold the camera without pitching the fish"):
		return
	_key(KEY_SHIFT, false)
	await _tick(90)
	if not _require(camera.global_position.distance_to(keyboard_camera_center) < 0.12,
			"keyboard camera did not return after Shift release"):
		return
	var options_point := options.get_global_rect().get_center()
	_mouse_button(MOUSE_BUTTON_LEFT, true, options_point)
	await process_frame
	_mouse_button(MOUSE_BUTTON_LEFT, false, options_point)
	await process_frame
	if not _require(paused and resume.has_focus(), "pointer Options did not open pause"):
		return
	var entry_resume := resume.get_global_rect().get_center()
	_mouse_button(MOUSE_BUTTON_LEFT, true, entry_resume)
	await process_frame
	_mouse_button(MOUSE_BUTTON_LEFT, false, entry_resume)
	await process_frame
	if not _require(not paused, "pointer could not resume from Options"):
		return
	await _tick(3)
	# Pointer activity is inert in keyboard mode; W still reaches the motor.
	_mouse_button(MOUSE_BUTTON_LEFT, true)
	_motion(Vector2(40, 20))
	await _tick(8)
	if not _require(fish.position.length() < 0.001, "inactive mouse moved keyboard fish"):
		return
	_mouse_button(MOUSE_BUTTON_LEFT, false)
	_key(KEY_W, true)
	await _tick(15)
	_key(KEY_W, false)
	if not _require(fish.position.z < -0.15, "keyboard thrust did not reach motor"):
		return
	_key(KEY_ESCAPE, true)
	await process_frame
	_key(KEY_ESCAPE, false)
	await process_frame
	if not _require(paused and resume.has_focus(), "keyboard pause did not focus Resume"):
		return
	var frozen := fish.global_position
	await create_timer(0.1).timeout
	if not _require(fish.global_position.distance_to(frozen) < 0.0001,
			"fish moved during pause"):
		return
	# Focused keyboard activation must resume, with the action consumed.
	_key(KEY_ENTER, true)
	await process_frame
	_key(KEY_ENTER, false)
	await process_frame
	if not _require(not paused, "Enter did not activate focused Resume"):
		return
	await _tick(3)
	if not _require(fish.global_position.distance_to(frozen) < 0.0001,
			"resume retained keyboard propulsion"):
		return
	# Set the mode via the real option signal, while the former device is held.
	_key(KEY_ESCAPE, true)
	await process_frame
	_key(KEY_ESCAPE, false)
	await process_frame
	_key(KEY_W, true)
	mode.select(1)
	mode.item_selected.emit(1)
	if not _require(mode.selected == 1, "mouse choice was not selected"):
		return
	sensitivity.value = 0.2
	invert.button_pressed = true
	_key(KEY_W, false)
	# Mouse users can activate Resume by pointer.
	var point := resume.get_global_rect().get_center()
	_mouse_button(MOUSE_BUTTON_LEFT, true, point)
	await process_frame
	_mouse_button(MOUSE_BUTTON_LEFT, false, point)
	await process_frame
	if not _require(not paused, "pointer Resume did not resume"):
		return
	await _tick(3)
	var start := fish.global_position
	# Capture warp is discarded. Later relative motion is consumed once.
	_motion(Vector2(250, 250))
	await _tick(1)
	if not _require(absf(fish.rotation.y) < 0.001 and absf(fish.rotation.x) < 0.001,
			"capture reset produced steering spike"):
		return
	_motion(Vector2(12, 8))
	await _tick(1)
	var yaw := fish.rotation.y
	var pitch := fish.rotation.x
	if not _require(yaw < -0.005 and pitch > 0.005,
			"relative mouse motion did not steer yaw and inverted pitch"):
		return
	await _tick(4)
	if not _require(absf(fish.rotation.y - yaw) < 0.0001 and absf(fish.rotation.x - pitch) < 0.0001,
			"mouse displacement applied more than once"):
		return
	await _tick(45) # Let the following view settle after the mouse yaw/pitch probe.
	_key(KEY_W, true)
	var centered_eye := camera.global_position
	await _tick(20)
	_key(KEY_W, false)
	if not _require(fish.global_position.distance_to(start) < 0.001,
			"inactive keyboard thrust moved mouse fish"):
		return
	if not _require(camera.global_position.y > centered_eye.y + 0.15,
			"mouse-mode W did not lift the camera view"):
		return
	await _tick(65)
	if not _require(camera.global_position.distance_to(centered_eye) < 0.13,
			"camera did not return after W release"):
		return
	_key(KEY_SHIFT, true)
	_key(KEY_A, true)
	_key(KEY_S, true)
	await _tick(22)
	_key(KEY_A, false)
	_key(KEY_S, false)
	await _tick(65)
	var held_eye := camera.global_position
	await _tick(20)
	if not _require(camera.global_position.distance_to(held_eye) < 0.03 and
			camera.global_position.distance_to(centered_eye) > 0.4 and
			fish.global_position.distance_to(start) < 0.001,
			"Shift did not hold the panned camera without moving the fish"):
		return
	_key(KEY_SHIFT, false)
	await _tick(90)
	if not _require(camera.global_position.distance_to(centered_eye) < 0.13,
			"releasing Shift did not return the camera to center: actual=%s baseline=%s" % [camera.global_position, centered_eye]):
		return
	var default_distance := camera.global_position.distance_to(fish.global_position)
	for i in 2:
		_mouse_button(MOUSE_BUTTON_WHEEL_UP, true, Vector2(100, 100))
		_mouse_button(MOUSE_BUTTON_WHEEL_UP, false, Vector2(100, 100))
	await _tick(35)
	var zoomed_distance := camera.global_position.distance_to(fish.global_position)
	if not _require(zoomed_distance < default_distance - 0.45 and
			fish.global_position.distance_to(start) < 0.001,
			"wheel-up zoom did not move the camera toward the fish without moving it"):
		return
	for i in 2:
		_mouse_button(MOUSE_BUTTON_WHEEL_DOWN, true, Vector2(100, 100))
		_mouse_button(MOUSE_BUTTON_WHEEL_DOWN, false, Vector2(100, 100))
	await _tick(75)
	if not _require(absf(camera.global_position.distance_to(fish.global_position) - default_distance) < 0.12,
			"wheel-down zoom did not restore the default camera distance: before=%.3f in=%.3f after=%.3f" % [default_distance, zoomed_distance, camera.global_position.distance_to(fish.global_position)]):
		return
	_mouse_button(MOUSE_BUTTON_LEFT, true)
	await _tick(18)
	_mouse_button(MOUSE_BUTTON_LEFT, false)
	if not _require(fish.global_position.distance_to(start) > 0.15,
			"mouse primary hold did not propel fish"):
		return
	var speed := fish.velocity.length()
	await _tick(8)
	if not _require(fish.velocity.length() < speed and fish.velocity.length() > 0.01,
			"primary release did not enter passive coast"):
		return
	_mouse_button(MOUSE_BUTTON_RIGHT, true)
	await _tick(1)
	if not _require(not paused, "right-button slowing opened the pause menu"):
		return
	await _tick(27)
	if not _require(fish.velocity.length() < 0.1 and not paused,
			"right-button slowing failed to reach the stop before reverse delay"):
		return
	var reverse_start := fish.global_position
	var backward := fish.global_transform.basis.z
	await _tick(40)
	if not _require(fish.velocity.dot(backward) > 0.2 and
			(fish.global_position - reverse_start).dot(backward) > 0.05 and not paused,
			"held right-button slow did not swim backward after the stop"):
		return
	_mouse_button(MOUSE_BUTTON_RIGHT, false)
	_mouse_button(MOUSE_BUTTON_MIDDLE, true)
	await process_frame
	_mouse_button(MOUSE_BUTTON_MIDDLE, false)
	await process_frame
	if not _require(paused and Input.mouse_mode == Input.MOUSE_MODE_VISIBLE,
			"middle button did not pause and release capture"):
		return
	# Switching back while primary is held must neutralize both adapters.
	_mouse_button(MOUSE_BUTTON_LEFT, true)
	mode.select(0)
	mode.item_selected.emit(0)
	resume.emit_signal("pressed")
	await _tick(3)
	var neutral := fish.global_position
	await _tick(5)
	if not _require(fish.global_position.distance_to(neutral) < 0.001,
			"mode switch leaked held propulsion"):
		return
	_mouse_button(MOUSE_BUTTON_LEFT, false)
	await _tick(3)
	# Focus loss pauses; focus return alone cannot resume.
	shell.notification(Node.NOTIFICATION_APPLICATION_FOCUS_OUT)
	await process_frame
	if not _require(paused and Input.mouse_mode == Input.MOUSE_MODE_VISIBLE,
			"focus loss did not pause and release pointer"):
		return
	shell.notification(Node.NOTIFICATION_APPLICATION_FOCUS_IN)
	await process_frame
	if not _require(paused, "focus return resumed without explicit action"):
		return
	var config := ConfigFile.new()
	if not _require(config.load("user://preferences.cfg") == OK and
			config.get_value("input", "mode") == "keyboard" and
			absf(config.get_value("input", "mouse_sensitivity") - 0.2) < 0.001 and
			config.get_value("input", "invert_mouse_y"),
			"local mode/sensitivity/inversion did not persist"):
		return
	print("INPUT_LIFECYCLE_OK keyboard-arrow-camera shift-hold mouse right-slow middle-pause camera-pan wheel-zoom focus capture rearm preferences")
	quit()
