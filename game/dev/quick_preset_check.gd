extends SceneTree

# Debug-only same-scene profile cycle through the real shell and C# fish.
func _initialize() -> void:
	call_deferred("_run")

func _require(ok: bool, reason: String) -> bool:
	if ok:
		return true
	printerr("QUICK_PRESET_FAILED ", reason)
	quit(1)
	return false

func _key(code: Key, down: bool) -> void:
	var event := InputEventKey.new()
	event.keycode = code
	event.physical_keycode = code
	event.pressed = down
	root.push_input(event, true)

func _button(shell: Node, name: String) -> Button:
	return shell.find_child(name, true, false) as Button

func _run() -> void:
	var scene: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(scene)
	current_scene = scene
	await physics_frame
	var shell: Node = scene.get_node("PlayShell")
	var fish: CharacterBody3D = scene.get_node("Fish")
	var camera: Camera3D = scene.get_node("Fish/CameraRig/Camera3D")
	var collision: CollisionShape3D = fish.get_node("Collision")
	var label: Label = shell.find_child("QuickPresetLabel", true, false)
	if not _require(OS.is_debug_build() and label != null, "debug cycle label unavailable"):
		return
	_button(shell, "OptionsButton").pressed.emit()
	_button(shell, "ControlPresetsButton").pressed.emit()
	_button(shell, "PresetCreateButton").pressed.emit()
	(shell.find_child("PresetNameEdit", true, false) as LineEdit).text = "P4 Route Test"
	_button(shell, "PresetConfirmButton").pressed.emit()
	_button(shell, "PresetBackButton").pressed.emit()
	_button(shell, "ResumeButton").pressed.emit()
	await physics_frame
	var choice: OptionButton = shell.find_child("InputModeOption", true, false)
	if not _require(choice.item_count == 4, "saved custom was absent from cycle"):
		return
	_key(KEY_W, true)
	for i in range(18):
		await physics_frame
	var speed_before := fish.velocity.length()
	var fish_identity := fish.get_instance_id()
	var camera_identity := camera.get_instance_id()
	var collision_identity := collision.get_instance_id()
	var position_before := fish.global_position
	var velocity_before := fish.velocity
	var camera_before := camera.global_transform
	var steps_before: int = fish.call("GetMotorStepCount")
	_key(KEY_F6, true)
	if not _require(fish.call("GetActivePresetId") == "builtin:mouse" and
			fish.get_instance_id() == fish_identity and camera.get_instance_id() == camera_identity and
			collision.get_instance_id() == collision_identity and
			fish.global_position.is_equal_approx(position_before) and
			fish.velocity.is_equal_approx(velocity_before) and
			camera.global_transform.is_equal_approx(camera_before),
			"live cycle changed physical scene state"):
		return
	_key(KEY_F6, false)
	await physics_frame
	if not _require(fish.call("GetMotorStepCount") == steps_before + 1 and
			fish.call("GetLastThrust") == 0.0 and fish.velocity.length() > 0.0 and
			fish.velocity.length() < speed_before and label.visible and
			label.text.contains("Mouse Only"), "cycle leaked input, doubled motor or hid label"):
		return
	_key(KEY_W, false)
	await physics_frame
	var expected := ["builtin:hybrid", "custom:", "builtin:keyboard", "builtin:mouse",
		"builtin:hybrid", "custom:", "builtin:keyboard"]
	for target in expected:
		_key(KEY_F6, true)
		var active: String = fish.call("GetActivePresetId")
		if not _require(active.begins_with(target), "unexpected cycle target %s for %s" % [active, target]):
			return
		_key(KEY_F6, false)
		await physics_frame
	if not _require(not paused and fish.get_instance_id() == fish_identity, "cycle restarted trial"):
		return
	_key(KEY_ESCAPE, true)
	await process_frame
	_key(KEY_ESCAPE, false)
	await process_frame
	var active_before_pause: String = fish.call("GetActivePresetId")
	_key(KEY_F6, true)
	_key(KEY_F6, false)
	if not _require(paused and fish.call("GetActivePresetId") == active_before_pause,
			"F6 changed a paused menu"):
		return
	var display: OptionButton = shell.find_child("DisplayModeOption", true, false)
	display.select(1)
	display.item_selected.emit(1)
	if not _require(paused and display.selected == 1, "display transition escaped pause"):
		return
	_key(KEY_W, true)
	_button(shell, "ResumeButton").pressed.emit()
	await physics_frame
	if not _require(not paused and fish.call("GetLastThrust") == 0.0,
			"first Resume frame used held input"):
		return
	_key(KEY_W, false)
	await physics_frame
	_key(KEY_W, true)
	await physics_frame
	if not _require(fish.call("GetLastThrust") == 1.0, "release/repress did not rearm"):
		return
	_key(KEY_W, false)
	shell.notification(Node.NOTIFICATION_APPLICATION_FOCUS_OUT)
	await process_frame
	if not _require(paused and Input.mouse_mode == Input.MOUSE_MODE_VISIBLE,
			"focus loss failed to restore menu pointer"):
		return
	_button(shell, "ControlPresetsButton").pressed.emit()
	var profiles: OptionButton = shell.find_child("PresetProfileOption", true, false)
	profiles.select(3)
	profiles.item_selected.emit(3)
	_button(shell, "Add_Thrust").pressed.emit()
	_key(KEY_F6, true)
	await process_frame
	_key(KEY_F6, false)
	await process_frame
	_button(shell, "PresetSaveButton").pressed.emit()
	_button(shell, "PresetApplyButton").pressed.emit()
	_button(shell, "PresetBackButton").pressed.emit()
	_button(shell, "ResumeButton").pressed.emit()
	await physics_frame
	var custom_id: String = fish.call("GetActivePresetId")
	_key(KEY_F6, false)
	await physics_frame
	_key(KEY_F6, true)
	await physics_frame
	if not _require(fish.call("GetActivePresetId") == custom_id and
			fish.call("GetLastThrust") == 1.0,
			"F6 stole a custom gameplay binding"):
		return
	_key(KEY_F6, false)
	print("QUICK_PRESET_OK cycle=4 repeated=8 state-preserved motor-once pause-resume display focus conflict")
	quit()
