extends SceneTree

func _initialize() -> void:
	call_deferred("_run")

func _require(ok: bool, message: String) -> bool:
	if ok:
		return true
	printerr("WATER_REACTION_FAILED ", message)
	quit(1)
	return false

func _key(code: Key, down: bool) -> void:
	var event := InputEventKey.new()
	event.keycode = code
	event.physical_keycode = code
	event.pressed = down
	root.push_input(event, true)

func _near(a: Vector3, b: Vector3, tolerance: float = 0.001) -> bool:
	return a.distance_to(b) < tolerance

func _mapping(water: Node) -> bool:
	var rest: Vector3 = water.call("PreviewLevels", 0.0, 0.0, 0.0, 0.0)
	var glide: Vector3 = water.call("PreviewLevels", 1.0, 0.0, 0.0, 0.0)
	var cruise: Vector3 = water.call("PreviewLevels", 1.0, 1.0, 0.0, 0.0)
	var effort: Vector3 = water.call("PreviewLevels", 1.0, 1.0, 1.0, 0.0)
	var right: Vector3 = water.call("PreviewLevels", 1.0, 1.0, 0.0, 1.0)
	var left: Vector3 = water.call("PreviewLevels", 1.0, 1.0, 0.0, -1.0)
	if not _require(_near(rest, Vector3.ZERO) and _near(glide, Vector3(0.05, 0.05, 0.02)), "rest/glide map"):
		return false
	if not _require(_near(cruise, Vector3(0.31, 0.31, 0.16)) and _near(effort, Vector3(0.59, 0.59, 0.4)), "cruise/effort map"):
		return false
	if not _require(_near(right, Vector3(0.67, 0.31, 0.26)) and _near(left, Vector3(0.31, 0.67, 0.26)), "directional turn map"):
		return false
	for speed in [-2.0, 0.0, 0.4, 1.0, 3.0, NAN]:
		for turn in [-3.0, -1.0, 0.0, 1.0, 3.0, NAN]:
			var levels: Vector3 = water.call("PreviewLevels", speed, 4.0, 4.0, turn)
			if not _require(levels.is_finite() and levels.x >= 0 and levels.y >= 0 and levels.z >= 0 and levels.x <= 1 and levels.y <= 1 and levels.z <= 1, "mapping bound/finite"):
				return false
	return true

func _configuration(fish: CharacterBody3D, water: Node3D) -> bool:
	if not _require(water.get_parent() == fish and water.position == Vector3.ZERO and water.scale == Vector3.ONE, "effect is not anchored to fish root"):
		return false
	var visual: Node3D = fish.get_node("PlayerVisual")
	if not _require(absf(visual.scale.x - 0.12) < 0.0001 and absf(absf(visual.rotation.y) - PI) < 0.001, "Fish3 scale/orientation changed"):
		return false
	var left: GPUParticles3D = water.get_node("LeftWake")
	var right: GPUParticles3D = water.get_node("RightWake")
	var bubbles: GPUParticles3D = water.get_node("TailBubbles")
	if not _require(left.amount == 16 and right.amount == 16 and bubbles.amount == 12 and left.lifetime <= 0.55 and right.lifetime <= 0.55 and bubbles.lifetime <= 0.42, "emitter budget"):
		return false
	if not _require(left.trail_enabled and right.trail_enabled and left.draw_pass_1 is RibbonTrailMesh and right.draw_pass_1 is RibbonTrailMesh and bubbles.draw_pass_1 is QuadMesh, "Forward+ draw passes"):
		return false
	for emitter in [left, right, bubbles]:
		if not _require(not emitter.local_coords and emitter.process_material is ParticleProcessMaterial and emitter.amount_ratio == 0.0 and not emitter.emitting and emitter.preprocess == 0.0 and emitter.explosiveness == 0.0, "world-space/no-prewarm config"):
			return false
	return _require(left.position.x < 0 and right.position.x > 0 and left.position.z > 0.35 and right.position.z > 0.35 and bubbles.position.z > 0.35, "tail-space emission")

func _route(effect_enabled: bool, lifecycle: bool) -> Dictionary:
	var scene: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(scene)
	current_scene = scene
	var fish: CharacterBody3D = scene.get_node("Fish")
	var water: Node3D = scene.get_node("Fish/WaterReaction")
	var collider: CollisionShape3D = scene.get_node("Fish/Collision")
	var shape := collider.shape
	var tuning: Resource = fish.get("Tuning")
	if not _mapping(water) or not _configuration(fish, water):
		return {}
	var before_transform := fish.global_transform
	var before_velocity := fish.velocity
	var before_layer := fish.collision_layer
	var before_mask := fish.collision_mask
	var before_input: int = fish.call("GetHeldInputCount")
	water.call("SetEnabled", effect_enabled)
	water.call("Suspend")
	if not _require(fish.global_transform == before_transform and fish.velocity == before_velocity and collider.shape == shape and fish.collision_layer == before_layer and fish.collision_mask == before_mask and fish.get("Tuning") == tuning and fish.call("GetHeldInputCount") == before_input, "effect toggle/suspend mutated fish"):
		return {}
	fish.call("SetInputMode", "keyboard")
	for tick in range(4):
		await physics_frame
	_key(KEY_W, true)
	var armed := false
	for tick in range(15):
		await physics_frame
		if fish.call("GetLastThrust") > 0.5:
			armed = true
			break
	if not _require(armed, "thrust never armed"):
		return {}
	var trace: Array[Vector3] = []
	var velocities: Array[Vector3] = []
	var bases: Array[Basis] = []
	var inputs: Array[int] = []
	var steps: Array[int] = []
	var start_steps: int = fish.call("GetMotorStepCount")
	var cruise_ratio := 0.0
	for tick in range(45):
		if tick == 25:
			_key(KEY_D, true)
		if tick == 32:
			_key(KEY_UP, true)
		await physics_frame
		trace.append(fish.global_position)
		velocities.append(fish.velocity)
		bases.append(fish.global_basis)
		inputs.append(fish.call("GetHeldInputCount"))
		steps.append(int(fish.call("GetMotorStepCount")) - start_steps)
		if tick == 24:
			cruise_ratio = water.get_node("LeftWake").amount_ratio
		if not _require(collider.shape == shape and fish.collision_layer == before_layer and fish.collision_mask == before_mask and fish.get("Tuning") == tuning, "effect frame mutated collision or tuning"):
			return {}
	_key(KEY_D, false)
	_key(KEY_UP, false)
	_key(KEY_W, false)
	var left: GPUParticles3D = water.get_node("LeftWake")
	var right: GPUParticles3D = water.get_node("RightWake")
	if effect_enabled and not _require(cruise_ratio > 0.2 and cruise_ratio < 0.4, "ordinary cruise emitter ratio"):
		return {}
	if effect_enabled and not _require(left.amount_ratio > right.amount_ratio and left.amount_ratio > 0.2, "hard turn did not bias outside wake"):
		return {}
	if effect_enabled and not _require(left.global_basis.is_equal_approx(fish.global_basis) and right.global_basis.is_equal_approx(fish.global_basis), "emitters lost fish pitch/yaw frame"):
		return {}
	if not _require(absf(fish.rotation.x) > 0.1 and absf(fish.rotation.y) > 0.1, "trace did not exercise pitch and yaw"):
		return {}
	if lifecycle:
		for tick in range(12):
			await physics_frame
		if not _require(left.amount_ratio > 0 and left.amount_ratio < 0.06 and water.get_node("TailBubbles").amount_ratio < 0.025, "glide did not reduce wake"):
			return {}
		_key(KEY_ESCAPE, true)
		await process_frame
		_key(KEY_ESCAPE, false)
		await process_frame
		if not _require(paused and not left.emitting and left.amount_ratio == 0 and not right.emitting, "pause did not clear emitters"):
			return {}
		await create_timer(0.16, true).timeout
		if not _require(left.amount_ratio == 0 and right.amount_ratio == 0, "paused emitter accumulated backlog"):
			return {}
		_key(KEY_ESCAPE, true)
		await process_frame
		_key(KEY_ESCAPE, false)
		await process_frame
		if not _require(not paused and left.amount_ratio == 0 and not left.emitting, "resume emitted without rearm"):
			return {}
		for tick in range(3):
			await physics_frame
		water.notification(Node.NOTIFICATION_APPLICATION_FOCUS_OUT)
		if not _require(left.amount_ratio == 0 and not left.emitting, "focus loss did not clear"):
			return {}
	var weak_water: WeakRef = weakref(water)
	current_scene = null
	scene.queue_free()
	await process_frame
	if not _require(weak_water.get_ref() == null, "scene restart leaked water owner"):
		return {}
	return { "positions": trace, "velocities": velocities, "bases": bases, "inputs": inputs, "steps": steps }

func _run() -> void:
	if not _require(ProjectSettings.get_setting("rendering/renderer/rendering_method") == "forward_plus", "tracked renderer is not Forward+"):
		return
	var enabled: Dictionary = await _route(true, true)
	var disabled: Dictionary = await _route(false, false)
	if enabled.is_empty() or disabled.is_empty():
		return
	for i in range(enabled.positions.size()):
		if not _require(_near(enabled.positions[i], disabled.positions[i]) and _near(enabled.velocities[i], disabled.velocities[i]) and enabled.bases[i].is_equal_approx(disabled.bases[i]) and enabled.inputs[i] == disabled.inputs[i] and enabled.steps[i] == disabled.steps[i], "enabled/disabled authoritative drift at tick %d" % i):
			printerr("enabled=", enabled.positions[i], "/", enabled.velocities[i], "/", enabled.bases[i], "/", enabled.inputs[i], "/", enabled.steps[i], " disabled=", disabled.positions[i], "/", disabled.velocities[i], "/", disabled.bases[i], "/", disabled.inputs[i], "/", disabled.steps[i])
			return
	print("WATER_REACTION_OK 44_gpu_particles max_rate=%.1f/s equal_intent_ticks=%d pause_focus_cleanup=ok" % [32.0 / 0.55 + 12.0 / 0.42, enabled.positions.size()])
	quit()
