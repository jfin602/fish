extends SceneTree

func _initialize() -> void:
	call_deferred("_run")

func _key(code: Key, down: bool) -> void:
	var event := InputEventKey.new()
	event.keycode = code
	event.physical_keycode = code
	event.pressed = down
	Input.parse_input_event(event)

func _route(presentation_enabled: bool) -> Dictionary:
	var packed: PackedScene = load("res://scenes/main.tscn")
	var scene: Node3D = packed.instantiate()
	root.add_child(scene)
	current_scene = scene
	var fish: CharacterBody3D = scene.get_node("Fish")
	var atmosphere: Node3D = scene.get_node("UnderwaterAtmosphere")
	atmosphere.SetEnabled(presentation_enabled)
	fish.call("SetInputMode", "keyboard")
	for tick in range(4):
		await physics_frame
	_key(KEY_W, true)
	var thrust_started := false
	for tick in range(12):
		await physics_frame
		if fish.call("GetLastThrust") > 0.5:
			thrust_started = true
			break
	if not thrust_started:
		printerr("ATMOSPHERE_MOVEMENT_FAILED thrust input never armed")
		quit(1)
		return {}
	var initial := fish.global_position
	var initial_steps: int = fish.call("GetMotorStepCount")
	for tick in range(30):
		await physics_frame
	_key(KEY_W, false)
	var result := {
		"position": fish.global_position - initial,
		"velocity": fish.velocity,
		"steps": int(fish.call("GetMotorStepCount")) - initial_steps,
	}
	current_scene = null
	scene.queue_free()
	await process_frame
	return result

func _run() -> void:
	var enabled: Dictionary = await _route(true)
	var disabled: Dictionary = await _route(false)
	if enabled.is_empty() or disabled.is_empty():
		return
	var difference: float = (enabled.position as Vector3).distance_to(disabled.position as Vector3)
	var velocity_difference: float = (enabled.velocity as Vector3).distance_to(disabled.velocity as Vector3)
	if difference > 0.001 or velocity_difference > 0.001 or enabled.steps != disabled.steps:
		printerr("ATMOSPHERE_MOVEMENT_FAILED presentation changed authoritative route: ", enabled, " vs ", disabled)
		quit(1)
		return
	if (enabled.position as Vector3).z > -0.3:
		printerr("ATMOSPHERE_MOVEMENT_FAILED thrust did not move fish: ", enabled)
		quit(1)
		return
	print("ATMOSPHERE_MOVEMENT_OK equal-physics-route enabled=disabled steps=", enabled.steps)
	quit()
