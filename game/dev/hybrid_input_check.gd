extends SceneTree

# Saved Fish scene, real PlayShell selection and C# resolver/motor.
func _initialize() -> void:
	call_deferred("_run")

func _require(ok: bool, message: String) -> bool:
	if ok:
		return true
	printerr("HYBRID_INPUT_FAILED ", message)
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

func _button(code: MouseButton, down: bool) -> void:
	var event := InputEventMouseButton.new()
	event.button_index = code
	event.pressed = down
	root.push_input(event, true)

func _motion(relative: Vector2) -> void:
	var event := InputEventMouseMotion.new()
	event.relative = relative
	root.push_input(event, true)

func _run() -> void:
	var scene: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(scene)
	current_scene = scene
	var fish: CharacterBody3D = scene.get_node("Fish")
	var shell: Node = scene.get_node("PlayShell")
	var mode: OptionButton = shell.find_child("InputModeOption", true, false)
	var display: OptionButton = shell.find_child("DisplayModeOption", true, false)
	var resume: Button = shell.find_child("ResumeButton", true, false)
	await _tick(3)
	if not _require(mode.item_count >= 3 and mode.selected == 0, "three presets unavailable"):
		return
	_key(KEY_W, true)
	await _tick(18)
	var moving_speed := fish.velocity.length()
	if not _require(moving_speed > 0.1, "keyboard fallback W failed"):
		return
	# A direct profile activation while swimming must retain the authoritative state.
	# The held W must then be released before the new profile can drive it.
	_key(KEY_ESCAPE, false)
	_key(KEY_W, false)
	_key(KEY_ESCAPE, true)
	await process_frame
	_key(KEY_ESCAPE, false)
	await process_frame
	var paused_position := fish.position
	await create_timer(0.05).timeout
	if not _require(paused and fish.position.is_equal_approx(paused_position), "pause did not freeze Fish"):
		return
	mode.select(2)
	mode.item_selected.emit(2)
	if not _require(fish.call("GetActivePresetId") == "builtin:hybrid", "hybrid selection failed"):
		return
	resume.emit_signal("pressed")
	await _tick(3)
	if not _require(not paused and fish.call("GetLastThrust") == 0.0, "resume leaked menu input"):
		return
	_motion(Vector2(350, 0)) # First capture motion and warp are ignored.
	await _tick()
	if not _require(absf(fish.call("GetLastYaw")) < 0.001, "capture motion steered"):
		return
	_key(KEY_W, true)
	await _tick(2)
	var motor_steps: int = fish.call("GetMotorStepCount")
	_motion(Vector2(12, -8))
	await _tick()
	if not _require(fish.call("GetLastThrust") == 1.0 and fish.call("GetLastYaw") > 0.0 and
			fish.call("GetLastPitch") > 0.0 and fish.call("GetMotorStepCount") == motor_steps + 1,
			"W and mouse did not combine in one motor step: thrust=%s yaw=%s pitch=%s steps=%s" % [fish.call("GetLastThrust"), fish.call("GetLastYaw"), fish.call("GetLastPitch"), fish.call("GetMotorStepCount")]):
		return
	var turn := fish.rotation.y
	var pitch := fish.rotation.x
	await _tick(2)
	if not _require(absf(fish.rotation.y - turn) < 0.001 and absf(fish.rotation.x - pitch) < 0.001,
			"relative mouse motion was applied twice"):
		return
	_button(MOUSE_BUTTON_LEFT, true)
	await _tick()
	if not _require(fish.call("GetLastThrust") == 1.0, "overlapping key/button doubled thrust"):
		return
	_key(KEY_W, false)
	await _tick()
	if not _require(fish.call("GetLastThrust") == 1.0, "mouse hold failed after W release"):
		return
	_button(MOUSE_BUTTON_LEFT, false)
	_key(KEY_A, true)
	_key(KEY_D, true)
	_key(KEY_UP, true)
	_key(KEY_DOWN, true)
	await _tick()
	if not _require(fish.call("GetLastYaw") == 0.0 and fish.call("GetLastPitch") == 0.0,
			"opposing keyboard steering did not cancel"):
		return
	_key(KEY_A, false)
	_key(KEY_D, false)
	_key(KEY_UP, false)
	_key(KEY_DOWN, false)
	await _tick()
	_key(KEY_D, true)
	await _tick(2)
	_motion(Vector2(-2, 0))
	await _tick()
	if not _require(fish.call("GetLastYaw") > 0.0 and fish.call("GetLastYaw") < 1.0,
			"opposing key and mouse yaw did not mix once: yaw=%s held=%s" % [fish.call("GetLastYaw"), fish.call("GetHeldInputCount")]):
		return
	_key(KEY_D, false)
	_key(KEY_D, true)
	_motion(Vector2(80, 80))
	await _tick()
	if not _require(absf(fish.call("GetLastYaw")) <= 1.0 and absf(fish.call("GetLastPitch")) <= 1.0,
			"mixed steering exceeded clamp"):
		return
	_key(KEY_D, false)
	_button(MOUSE_BUTTON_RIGHT, true)
	_key(KEY_S, true)
	await _tick()
	if not _require(fish.call("GetLastSlow") == 1.0, "overlapping slow inputs doubled"):
		return
	_button(MOUSE_BUTTON_RIGHT, false)
	_key(KEY_S, false)
	# Switching while held neutralizes controls without resetting the current velocity.
	_key(KEY_W, true)
	await _tick(8)
	var before_switch := fish.velocity.length()
	fish.call("SetInputMode", "mouse")
	fish.call("ResetMouseCapture")
	if not _require(absf(fish.velocity.length() - before_switch) < 0.001,
			"switch reset physical velocity"):
		return
	await _tick(2)
	if not _require(fish.call("GetLastThrust") == 0.0 and
			fish.velocity.length() > 0.0 and fish.velocity.length() < before_switch,
			"switch leaked held W or stopped neutral coasting"):
		return
	_key(KEY_W, false)
	await _tick(2)
	_button(MOUSE_BUTTON_LEFT, true)
	await _tick(4)
	if not _require(fish.call("GetLastThrust") == 1.0, "mouse-only button path failed: armed=%s held=%s mode=%s captured=%s" % [fish.call("GetInputArmed"), fish.call("GetHeldInputCount"), Input.mouse_mode, fish.call("GetActivePresetId")]):
		return
	_button(MOUSE_BUTTON_LEFT, false)
	_key(KEY_ESCAPE, true)
	await process_frame
	_key(KEY_ESCAPE, false)
	await process_frame
	if not _require(paused, "Escape recovery failed"):
		return
	display.select(1)
	display.item_selected.emit(1)
	var config := ConfigFile.new()
	if not _require(config.load("user://preferences.cfg") == OK and
			config.get_value("display", "mode") == "windowed-fullscreen" and paused,
			"display switch lost menu ownership or preference"):
		return
	shell.notification(Node.NOTIFICATION_APPLICATION_FOCUS_OUT)
	await process_frame
	if not _require(paused and Input.mouse_mode == Input.MOUSE_MODE_VISIBLE,
			"focus loss did not keep menu ownership"):
		return
	print("HYBRID_INPUT_OK keyboard mouse hybrid overlap cancel clamp capture switch pause focus display")
	quit()
