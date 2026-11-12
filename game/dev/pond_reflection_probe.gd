extends SceneTree

# Fixed native Forward+ positions for the experimental Pond water surface.
const SURFACE_Y := 10.390
const FISH_X := -19.64
const FISH_Z := 23.96
const WARMUP := 24
const SAMPLES := 36

func _initialize() -> void:
	call_deferred("_run")

func _median(values: Array[float]) -> float:
	values.sort()
	return values[values.size() / 2]

func _capture(output: String, label: String) -> void:
	for i in WARMUP:
		await process_frame
	var frames: Array[float] = []
	var draws: Array[float] = []
	var previous := Time.get_ticks_usec()
	for i in SAMPLES:
		await process_frame
		var now := Time.get_ticks_usec()
		frames.append(float(now - previous) / 1000.0)
		previous = now
		draws.append(Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME))
	var sorted := frames.duplicate()
	sorted.sort()
	var image := root.get_texture().get_image()
	var path := output.path_join(label + ".png")
	if image.save_png(path) != OK:
		printerr("POND_REFLECTION_PROBE_FAILED save=%s" % path)
		quit(1)
		return
	print("POND_REFLECTION_SAMPLE view=%s frame_ms_median=%.2f frame_ms_p95=%.2f draws_median=%.0f image=%s" % [
		label, _median(frames), sorted[ceili(float(sorted.size()) * 0.95) - 1],
		_median(draws), path])

func _run() -> void:
	if RenderingServer.get_current_rendering_method() != "forward_plus":
		printerr("POND_REFLECTION_PROBE_FAILED Forward+ required")
		quit(1)
		return
	var output := ""
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--pond-reflection-output="):
			output = argument.trim_prefix("--pond-reflection-output=")
	if output.is_empty() or DirAccess.make_dir_recursive_absolute(output) != OK:
		printerr("POND_REFLECTION_PROBE_FAILED output directory required")
		quit(1)
		return
	var uncapped := OS.get_cmdline_user_args().has("--pond-reflection-uncapped")
	if uncapped:
		DisplayServer.window_set_vsync_mode(DisplayServer.VSYNC_DISABLED)
		Engine.max_fps = 0
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	await process_frame
	paused = true
	main.get_node("PrototypeHelp").visible = false
	var fish: Node3D = main.get_node("Fish")
	var atmosphere: Node3D = main.get_node("UnderwaterAtmosphere")
	if OS.get_cmdline_user_args().has("--pond-reflection-off"):
		var surface: ShaderMaterial = main.get_node(
			"UnderwaterAtmosphere/WaterEffects/OverheadSurface").material_override
		surface.set_shader_parameter("reflection_strength", 0.0)
	var camera := Camera3D.new()
	camera.far = 150.0
	main.add_child(camera)
	camera.make_current()

	# Normal gameplay heights, followed by a longer, low reflected path.
	fish.global_position = Vector3(FISH_X, SURFACE_Y - 0.5, FISH_Z)
	camera.global_position = Vector3(FISH_X, SURFACE_Y + 1.2, FISH_Z + 3.0)
	camera.look_at(Vector3(FISH_X, SURFACE_Y - 0.8, FISH_Z), Vector3.UP)
	atmosphere.call("RefreshDepth", true, 2.0)
	await _capture(output, "near_fish_050cm")
	fish.global_position = Vector3(FISH_X, SURFACE_Y - 1.0, FISH_Z)
	await _capture(output, "near_fish_100cm")
	fish.global_position = Vector3(FISH_X, SURFACE_Y - 3.0, FISH_Z)
	await _capture(output, "near_fish_300cm")

	fish.global_position = Vector3(FISH_X, SURFACE_Y - 1.0, FISH_Z)
	camera.global_position = Vector3(FISH_X, SURFACE_Y + 0.65, FISH_Z + 9.0)
	camera.look_at(Vector3(FISH_X, SURFACE_Y + 0.20, FISH_Z - 12.0), Vector3.UP)
	atmosphere.call("RefreshDepth", true, 2.0)
	await _capture(output, "grazing_distance")

	var bank: Marker3D = main.get_node("Habitat/PondLandmarks/BankApproach")
	var shallow: Marker3D = main.get_node("Habitat/PondLandmarks/shallow")
	var toward := Vector2(shallow.global_position.x - bank.global_position.x,
		shallow.global_position.z - bank.global_position.z).normalized()
	camera.global_position = Vector3(bank.global_position.x + toward.x * 9.0,
		11.6, bank.global_position.z + toward.y * 9.0)
	camera.look_at(Vector3(bank.global_position.x, 11.6, bank.global_position.z), Vector3.UP)
	atmosphere.call("RefreshDepth", true, 2.0)
	await _capture(output, "shoreline")

	camera.global_position = Vector3(-56.0, SURFACE_Y + 1.3, 4.0)
	camera.look_at(Vector3(-56.0, SURFACE_Y - 0.5, 2.0), Vector3.UP)
	atmosphere.call("RefreshDepth", true, 2.0)
	await _capture(output, "shallow_mud")

	camera.global_position = Vector3(FISH_X, SURFACE_Y - 0.9, FISH_Z + 4.1)
	camera.look_at(Vector3(FISH_X, SURFACE_Y + 0.3, FISH_Z), Vector3.UP)
	atmosphere.call("RefreshDepth", true, 2.0)
	var material: ShaderMaterial = main.get_node(
		"UnderwaterAtmosphere/WaterEffects/OverheadSurface").material_override
	var frozen_time: float = material.get_shader_parameter("surface_time")
	await _capture(output, "underwater_up")
	if frozen_time != material.get_shader_parameter("surface_time"):
		printerr("POND_REFLECTION_PROBE_FAILED surface moved during pause")
		quit(1)
		return
	camera.global_position = Vector3(bank.global_position.x + toward.x * 9.0,
		11.6, bank.global_position.z + toward.y * 9.0)
	camera.look_at(Vector3(bank.global_position.x, 11.6, bank.global_position.z), Vector3.UP)
	atmosphere.call("RefreshDepth", true, 2.0)
	paused = false
	await _capture(output, "shoreline_motion")
	var moving_time: float = material.get_shader_parameter("surface_time")
	if moving_time <= frozen_time:
		printerr("POND_REFLECTION_PROBE_FAILED surface did not resume motion")
		quit(1)
		return
	print("POND_REFLECTION_TIME frozen=%.3f resumed=%.3f" % [frozen_time, moving_time])
	print("POND_REFLECTION_PROBE_DONE adapter=%s renderer=%s" % [
		RenderingServer.get_video_adapter_name(), RenderingServer.get_current_rendering_method()])
	quit()
