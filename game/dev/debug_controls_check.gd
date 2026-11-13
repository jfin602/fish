extends SceneTree

# Exercises the real Options controls and input routing in the saved game scene.
func _initialize() -> void:
	call_deferred("_run")

func _require(ok: bool, message: String) -> bool:
	if ok:
		return true
	printerr("DEBUG_CONTROLS_FAILED ", message)
	quit(1)
	return false

func _frames(count: int = 3) -> void:
	for i in range(count):
		await process_frame

func _key(code: Key) -> void:
	var event := InputEventKey.new()
	event.keycode = code
	event.physical_keycode = code
	event.pressed = true
	root.push_input(event, true)
	event = event.duplicate()
	event.pressed = false
	root.push_input(event, true)

func _wheel(button: MouseButton) -> void:
	var event := InputEventMouseButton.new()
	event.button_index = button
	event.pressed = true
	root.push_input(event, true)
	event = event.duplicate()
	event.pressed = false
	root.push_input(event, true)

func _files() -> PackedStringArray:
	var directory := DirAccess.open("user://screenshots")
	return directory.get_files() if directory != null else PackedStringArray()

func _run() -> void:
	var scene: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(scene)
	current_scene = scene
	await _frames()
	var shell: Node = scene.get_node("PlayShell")
	var clock: Node = scene.get_node("EnvironmentClock")
	var fish: CharacterBody3D = scene.get_node("Fish")
	var camera: Camera3D = scene.get_node("Fish/CameraRig/Camera3D")
	var sun: DirectionalLight3D = scene.get_node("Sun")
	var surface: MeshInstance3D = scene.get_node("UnderwaterAtmosphere/WaterEffects/OverheadSurface")
	var surface_material: ShaderMaterial = surface.material_override
	var options: Button = shell.find_child("OptionsButton", true, false)
	var debug_button: Button = shell.find_child("DebugControlsButton", true, false)
	var panel: PanelContainer = shell.find_child("DebugControlsPanel", true, false)
	var wheel_time: CheckButton = shell.find_child("MouseWheelTimeToggle", true, false)
	var screenshot: CheckButton = shell.find_child("BacktickScreenshotToggle", true, false)
	var back: Button = shell.find_child("DebugBackButton", true, false)
	var resume: Button = shell.find_child("ResumeButton", true, false)
	if not _require(options != null and debug_button != null and panel != null and
			wheel_time != null and screenshot != null and back != null and resume != null,
			"debug controls missing from Options"):
		return
	if not _require(not wheel_time.button_pressed and not screenshot.button_pressed and
			not panel.visible, "debug controls did not default off"):
		return
	var original_files := _files()
	_key(KEY_QUOTELEFT)
	await _frames()
	if not _require(_files().size() == original_files.size(), "backtick captured while toggle was off"):
		return
	options.emit_signal("pressed")
	debug_button.emit_signal("pressed")
	if not _require(panel.visible, "debug panel did not open"):
		return
	wheel_time.button_pressed = true
	if not _require(wheel_time.button_pressed and not screenshot.button_pressed,
			"time toggle changed screenshot toggle"):
		return
	back.emit_signal("pressed")
	resume.emit_signal("pressed")
	await _frames()
	var time_before: float = clock.call("GetHourOfDay")
	var sun_before := sun.global_basis.z
	var surface_before: Vector3 = surface_material.get_shader_parameter("sun_direction")
	var zoom_before: float = camera.global_position.distance_to(fish.global_position)
	_wheel(MOUSE_BUTTON_WHEEL_UP)
	await _frames()
	if not _require(absf(clock.call("GetHourOfDay") - time_before - 0.25) < 0.02,
			"wheel up did not advance 15 minutes"):
		return
	if not _require(absf(camera.global_position.distance_to(fish.global_position) - zoom_before) < 0.1,
			"time wheel also zoomed camera"):
		return
	if not _require(sun.global_basis.z.distance_to(sun_before) > 0.01 and
			surface_material.get_shader_parameter("sun_direction").distance_to(surface_before) > 0.01,
			"time wheel did not refresh sun and water presentation"):
		return
	var notice: Label = shell.find_child("DebugNotice", true, false)
	if not _require(notice.visible and ":" in notice.text, "time indicator missing"):
		return
	var text_field := LineEdit.new()
	text_field.focus_mode = Control.FOCUS_ALL
	shell.get_child(0).add_child(text_field)
	text_field.grab_focus()
	var focused_time: float = clock.call("GetHourOfDay")
	_wheel(MOUSE_BUTTON_WHEEL_UP)
	await _frames()
	if not _require(absf(clock.call("GetHourOfDay") - focused_time) < 0.02,
			"wheel scrubbed while a text field was focused"):
		return
	text_field.release_focus()
	text_field.queue_free()
	_wheel(MOUSE_BUTTON_WHEEL_DOWN)
	await _frames()
	if not _require(absf(clock.call("GetHourOfDay") - time_before) < 0.02,
			"wheel down did not reverse time"):
		return
	options.emit_signal("pressed")
	debug_button.emit_signal("pressed")
	wheel_time.button_pressed = false
	screenshot.button_pressed = true
	if not _require(not wheel_time.button_pressed and screenshot.button_pressed,
			"screenshot toggle changed time toggle"):
		return
	back.emit_signal("pressed")
	resume.emit_signal("pressed")
	await _frames()
	zoom_before = camera.global_position.distance_to(fish.global_position)
	_wheel(MOUSE_BUTTON_WHEEL_UP)
	await _frames(28)
	if not _require(camera.global_position.distance_to(fish.global_position) < zoom_before - 0.2,
			"normal wheel zoom did not return"):
		return
	_key(KEY_QUOTELEFT)
	await _frames(5)
	var first := _files()
	if not _require(first.size() == original_files.size() + 1,
			"enabled backtick did not save one screenshot"):
		return
	var new_name := ""
	for name in first:
		if not original_files.has(name):
			new_name = name
	var first_image := Image.load_from_file("user://screenshots/" + new_name)
	if not _require(first_image != null and first_image.get_width() == root.size.x and
			first_image.get_height() == root.size.y, "saved PNG has invalid viewport dimensions"):
		return
	_key(KEY_QUOTELEFT)
	await _frames(5)
	if not _require(_files().size() == original_files.size() + 2,
			"screenshot filename was not unique"):
		return
	text_field = LineEdit.new()
	text_field.focus_mode = Control.FOCUS_ALL
	shell.get_child(0).add_child(text_field)
	text_field.grab_focus()
	_key(KEY_QUOTELEFT)
	await _frames(5)
	if not _require(_files().size() == original_files.size() + 2,
			"backtick captured while a text field was focused"):
		return
	text_field.release_focus()
	text_field.queue_free()
	options.emit_signal("pressed")
	debug_button.emit_signal("pressed")
	screenshot.button_pressed = false
	back.emit_signal("pressed")
	resume.emit_signal("pressed")
	_key(KEY_QUOTELEFT)
	await _frames(5)
	if not _require(_files().size() == original_files.size() + 2,
			"backtick captured after toggle was disabled"):
		return
	print("DEBUG_CONTROLS_OK toggles=independent time=bidirectional zoom=restored screenshots=2")
	quit()
