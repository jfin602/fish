extends SceneTree

# Deterministic input route; samples the rendered camera transform independently
# of physics frames. Run with --camera-fps=30, 60 or 120 and capture stdout.
var _samples: Array[Dictionary] = []
var _physics_steps := 0
var _fish: CharacterBody3D
var _failed := false

func _initialize() -> void:
	call_deferred("_run")

func _key(code: Key, down: bool) -> void:
	var event := InputEventKey.new()
	event.keycode = code
	event.physical_keycode = code
	event.pressed = down
	Input.parse_input_event(event)

func _on_physics_frame() -> void:
	_physics_steps += 1
	var time := _physics_steps / 60.0
	var y := 0.0
	var z := -2.0 * time
	var pitch := 0.0
	if time > 2.0:
		y = minf(1.6, (time - 2.0) * 0.8)
		pitch = 0.3 if time < 4.0 else 0.0
	if time > 4.0:
		z = -8.0 - (time - 4.0)
	_fish.global_transform = Transform3D(Basis(Vector3.RIGHT, pitch), Vector3(0, y, z))

func _sample(phase: String, fish: CharacterBody3D, rig: Node3D, frames: int) -> void:
	var camera: Camera3D = rig.get_node("Camera3D")
	var start := Time.get_ticks_usec()
	var prior_camera := rig.get_global_transform_interpolated().origin
	var prior_fish := fish.get_global_transform_interpolated().origin
	var prior_forward := -camera.global_basis.z
	var stationary_camera_frames := 0
	var moving_fish_frames := 0
	var largest_vertical_step := 0.0
	var vertical_travel := 0.0
	var prior_step := 0.0
	var largest_step_change := 0.0
	var largest_angle_degrees := 0.0
	var largest_horizon_roll := 0.0
	for i in range(frames):
		await process_frame
		var camera_position := rig.get_global_transform_interpolated().origin
		var fish_position := fish.get_global_transform_interpolated().origin
		var camera_step := camera_position.y - prior_camera.y
		var fish_step := fish_position.y - prior_fish.y
		if absf(camera_step) < 0.000001:
			stationary_camera_frames += 1
		if absf(fish_step) > 0.000001:
			moving_fish_frames += 1
		largest_vertical_step = maxf(largest_vertical_step, absf(camera_step))
		largest_step_change = maxf(largest_step_change, absf(camera_step - prior_step))
		var forward := -camera.global_basis.z
		largest_angle_degrees = maxf(largest_angle_degrees, rad_to_deg(prior_forward.angle_to(forward)))
		largest_horizon_roll = maxf(largest_horizon_roll, absf(camera.global_basis.x.y))
		vertical_travel += absf(camera_step)
		if i < 12 or absf(camera_step) > 0.02:
			print("CAMERA_FRAME phase=%s i=%d physics=%d fish=%s camera=%s camera_raw=%s camera_dy=%.6f" % [
				phase, i, _physics_steps, fish_position, camera_position, rig.global_position, camera_step])
		prior_camera = camera_position
		prior_fish = fish_position
		prior_step = camera_step
		prior_forward = forward
	print("CAMERA_METRIC phase=%s frames=%d elapsed_ms=%.1f physics=%d still_camera=%d moving_fish=%d max_dy=%.6f max_dy_change=%.6f vertical_travel=%.6f max_angle_deg=%.4f max_roll=%.6f" % [
		phase, frames, (Time.get_ticks_usec() - start) / 1000.0, _physics_steps,
		stationary_camera_frames, moving_fish_frames, largest_vertical_step,
		largest_step_change, vertical_travel, largest_angle_degrees, largest_horizon_roll])
	if phase == "ascent" and Engine.max_fps >= 60 and stationary_camera_frames > 5:
		_failed = true
	if phase == "coast" and largest_vertical_step > (0.05 if Engine.max_fps == 30 else 0.035 if Engine.max_fps == 60 else 0.025):
		_failed = true
	if largest_horizon_roll > 0.02 or largest_angle_degrees > 3.0:
		_failed = true
	if _failed:
		printerr("CAMERA_MOTION_FAILED phase=%s fps=%d" % [phase, Engine.max_fps])
		quit(1)

func _run() -> void:
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--camera-fps="):
			Engine.max_fps = int(arg.get_slice("=", 1))
	var scene: Node3D = load("res://scenes/p1_course_fixture.tscn").instantiate()
	root.add_child(scene)
	current_scene = scene
	var fish: CharacterBody3D = scene.get_node("Fish")
	var rig: Node3D = scene.get_node("Fish/CameraRig")
	fish.set_physics_process(false)
	for i in range(8):
		await physics_frame
	_fish = fish
	physics_frame.connect(_on_physics_frame)
	var frames_per_phase := Engine.max_fps * 2
	await _sample("level", fish, rig, frames_per_phase)
	if _failed: return
	await _sample("ascent", fish, rig, frames_per_phase)
	if _failed: return
	await _sample("coast", fish, rig, frames_per_phase)
	if _failed: return
	print("CAMERA_MOTION_OK fps=%d physics_ticks=60 phases=level,ascent,coast" % Engine.max_fps)
	quit()
