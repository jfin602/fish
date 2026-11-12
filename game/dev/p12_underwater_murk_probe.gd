extends SceneTree

# Native fixed-pose Pond atmosphere comparison. Optional trial settings affect this run only.
const WARMUP := 18
const SAMPLES := 90

func _initialize() -> void:
	call_deferred("_run")

func _median(values: Array[float]) -> float:
	values.sort()
	return values[values.size() / 2]

func _run() -> void:
	if RenderingServer.get_current_rendering_method() != "forward_plus":
		printerr("P12_MURK_PROBE_FAILED Forward+ required")
		quit(1)
		return
	var output := ""
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--murk-output="):
			output = argument.trim_prefix("--murk-output=")
	if output.is_empty() or DirAccess.make_dir_recursive_absolute(output) != OK:
		printerr("P12_MURK_PROBE_FAILED output directory required")
		quit(1)
		return
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	await process_frame
	paused = true
	main.get_node("PrototypeHelp").visible = false
	var fish: Node3D = main.get_node("Fish")
	var atmosphere: Node3D = main.get_node("UnderwaterAtmosphere")
	var environment: Environment = main.get_node("WorldEnvironment").environment
	var camera := Camera3D.new()
	camera.far = 150.0
	main.add_child(camera)
	camera.make_current()
	# Same fish and camera for every trial. Ground and cover start just behind it.
	fish.global_position = Vector3(-19.64, 8.79, 23.96)
	camera.global_position = Vector3(-19.64, 8.49, 28.06)
	camera.look_at(Vector3(-19.64, 8.25, 18.0), Vector3.UP)
	atmosphere.call("RefreshDepth", true, 0.0)
	var base_density := environment.fog_density
	var base_begin := environment.fog_depth_begin
	var base_end := environment.fog_depth_end
	var base_color := environment.fog_light_color
	print("P12_MURK_BASE density=%.3f begin=%.2f end=%.2f color=%s" % [
		base_density, base_begin, base_end, base_color])
	await _capture(output, "baseline_horizontal")
	for trial in [
		["medium_horizontal", 0.70, 3.0, 18.0, base_color],
		["strong_horizontal", 0.85, 3.0, 16.0, base_color]]:
		environment.fog_density = trial[1]
		environment.fog_depth_begin = trial[2]
		environment.fog_depth_end = trial[3]
		environment.fog_light_color = trial[4]
		await _capture(output, trial[0])
	var surface: ShaderMaterial = main.get_node(
		"UnderwaterAtmosphere/WaterEffects/OverheadSurface").material_override
	var base_dark: Vector3 = surface.get_shader_parameter("surface_dark")
	var base_light: Vector3 = surface.get_shader_parameter("surface_light")
	environment.fog_density = 0.70
	environment.fog_depth_begin = 3.0
	environment.fog_depth_end = 18.0
	surface.set_shader_parameter("surface_dark", Vector3(0.065, 0.12, 0.04))
	surface.set_shader_parameter("surface_light", Vector3(0.12, 0.20, 0.07))
	await _capture(output, "medium_dark_surface")
	surface.set_shader_parameter("surface_dark", base_dark)
	surface.set_shader_parameter("surface_light", base_light)
	# The surface comparison uses the shipped atmosphere; no trial leaks into it.
	environment.fog_density = base_density
	environment.fog_depth_begin = base_begin
	environment.fog_depth_end = base_end
	environment.fog_light_color = base_color
	var shallow: Marker3D = main.get_node("Habitat/PondLandmarks/shallow")
	for landmark_name in ["middle", "deep"]:
		var landmark: Marker3D = main.get_node("Habitat/PondLandmarks/" + landmark_name)
		var toward_shallow := (shallow.global_position - landmark.global_position).normalized()
		camera.global_position = landmark.global_position + Vector3(0.0, 0.5, 0.0)
		fish.global_position = landmark.global_position + toward_shallow * 4.0
		camera.look_at(fish.global_position, Vector3.UP)
		atmosphere.call("RefreshDepth", true, 0.0)
		print("P12_MURK_LANDMARK name=%s camera=%s fish=%s fog_end=%.2f" % [
			landmark_name, camera.global_position, fish.global_position, environment.fog_depth_end])
		await _capture(output, "production_" + landmark_name)
	fish.global_position = Vector3(-19.64, 8.79, 23.96)
	camera.global_position = Vector3(-19.64, 13.5, 31.96)
	camera.look_at(Vector3(-19.64, 9.5, 23.96), Vector3.UP)
	atmosphere.call("RefreshDepth", true, 0.0)
	await _capture(output, "air_overview")
	print("P12_MURK_PROBE_DONE adapter=%s" % RenderingServer.get_video_adapter_name())
	quit()

func _capture(output: String, label: String) -> void:
	for i in WARMUP:
		await process_frame
	var frames: Array[float] = []
	var draws: Array[float] = []
	var primitives: Array[float] = []
	var previous := Time.get_ticks_usec()
	for i in SAMPLES:
		await process_frame
		var now := Time.get_ticks_usec()
		frames.append(float(now - previous) / 1000.0)
		previous = now
		draws.append(Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME))
		primitives.append(Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME))
	var frame_p95 := frames.duplicate()
	frame_p95.sort()
	var p95: float = frame_p95[ceili(float(frame_p95.size()) * 0.95) - 1]
	var image := root.get_texture().get_image()
	var path := output.path_join(label + ".png")
	if image.save_png(path) != OK:
		printerr("P12_MURK_PROBE_FAILED save=%s" % path)
		quit(1)
		return
	print("P12_MURK_SAMPLE view=%s frame_ms_median=%.2f frame_ms_p95=%.2f draws_median=%.0f primitives_median=%.0f image=%s" % [
		label, _median(frames), p95, _median(draws), _median(primitives), path])
