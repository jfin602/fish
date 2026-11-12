extends SceneTree

# Native fixed-pose rendering evidence. Fish positions here are diagnostic only.
const SURFACE_Y := 10.390
const FISH_X := -19.64
const FISH_Z := 23.96
const WARMUP := 12
const SAMPLES := 90

func _initialize() -> void:
	call_deferred("_run")

func _median(values: Array[float]) -> float:
	values.sort()
	return values[values.size() / 2]

func _run() -> void:
	if RenderingServer.get_current_rendering_method() != "forward_plus":
		printerr("P12_R2_PROBE_FAILED Forward+ required")
		quit(1)
		return
	var output := ""
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--r2-output="):
			output = argument.trim_prefix("--r2-output=")
	if output.is_empty() or DirAccess.make_dir_recursive_absolute(output) != OK:
		printerr("P12_R2_PROBE_FAILED output directory required")
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
	var camera := Camera3D.new()
	camera.far = 150.0
	main.add_child(camera)
	camera.make_current()
	var depths := [0.1, 0.5, 1.0, 2.0, 3.0, 4.0, 5.0]
	for depth in depths:
		fish.global_position = Vector3(FISH_X, SURFACE_Y - depth, FISH_Z)
		camera.global_position = Vector3(FISH_X, 15.5, FISH_Z + 3.0)
		camera.look_at(Vector3(FISH_X, 8.5, FISH_Z), Vector3.UP)
		atmosphere.call("RefreshDepth", true, 0.0)
		await _capture(output, "fish_%03dcm" % roundi(depth * 100.0))
	var surface_material: ShaderMaterial = main.get_node(
		"UnderwaterAtmosphere/WaterEffects/OverheadSurface").material_override
	if surface_material.shader.code.contains("surface_daylight"):
		fish.global_position = Vector3(FISH_X, SURFACE_Y - 1.0, FISH_Z)
		camera.global_position = Vector3(FISH_X, 15.5, FISH_Z + 3.0)
		camera.look_at(Vector3(FISH_X, 8.5, FISH_Z), Vector3.UP)
		surface_material.set_shader_parameter("surface_daylight", 1.22)
		await _capture(output, "fish_100cm_brighter")
		fish.global_position = Vector3(FISH_X, SURFACE_Y - 4.0, FISH_Z)
		await _capture(output, "fish_400cm_brighter")
		surface_material.set_shader_parameter("surface_daylight", 1.0)
	# Oblique angle has a longer optical path than the fixed downward view.
	fish.global_position = Vector3(FISH_X, SURFACE_Y - 1.0, FISH_Z)
	camera.global_position = Vector3(FISH_X, 12.8, FISH_Z + 9.0)
	camera.look_at(Vector3(FISH_X, 8.5, FISH_Z), Vector3.UP)
	atmosphere.call("RefreshDepth", true, 0.0)
	await _capture(output, "fish_100cm_oblique")
	# Waterline and underwater views retain air/underwater separation.
	fish.global_position = Vector3(FISH_X, SURFACE_Y - 0.1, FISH_Z)
	camera.global_position = Vector3(FISH_X, 9.5, FISH_Z + 4.1)
	camera.look_at(Vector3(FISH_X, 9.8, FISH_Z), Vector3.UP)
	atmosphere.call("RefreshDepth", true, 0.0)
	await _capture(output, "underwater_near")
	camera.global_position = Vector3(FISH_X, 13.5, FISH_Z + 8.0)
	camera.look_at(Vector3(FISH_X, 9.5, FISH_Z), Vector3.UP)
	atmosphere.call("RefreshDepth", true, 0.0)
	await _capture(output, "overview")
	# Locate actual seed-71 terrain within one meter of the level waterline.
	var seabed: Node3D = main.get_node("Habitat/Seabed")
	var bank: Marker3D = main.get_node("Habitat/PondLandmarks/BankApproach")
	var shallow_point := Vector2.ZERO
	var shallow_score := INF
	var shallow_depth := 0.0
	for x in range(-60, 61, 2):
		for z in range(-60, 61, 2):
			var depth := SURFACE_Y - float(seabed.call("GetHeightAt", float(x), float(z)))
			if depth < 0.55 or depth > 0.95:
				continue
			var score := Vector2(float(x) - bank.global_position.x,
				float(z) - bank.global_position.z).length()
			if score < shallow_score:
				shallow_score = score
				shallow_point = Vector2(x, z)
				shallow_depth = depth
	if shallow_score < INF:
		camera.global_position = Vector3(shallow_point.x, SURFACE_Y + 5.0, shallow_point.y + 2.0)
		camera.look_at(Vector3(shallow_point.x, SURFACE_Y - 0.5, shallow_point.y), Vector3.UP)
		atmosphere.call("RefreshDepth", true, 0.0)
		print("P12_R2_SHALLOW terrain_depth=%.3f x=%.1f z=%.1f" % [shallow_depth, shallow_point.x, shallow_point.y])
		await _capture(output, "shallow_bottom")
	var shallow: Marker3D = main.get_node("Habitat/PondLandmarks/shallow")
	var direction := Vector2(shallow.global_position.x - bank.global_position.x,
		shallow.global_position.z - bank.global_position.z).normalized()
	camera.global_position = Vector3(bank.global_position.x + direction.x * 9.0,
		11.6, bank.global_position.z + direction.y * 9.0)
	camera.look_at(Vector3(bank.global_position.x, 11.6, bank.global_position.z), Vector3.UP)
	atmosphere.call("RefreshDepth", true, 0.0)
	await _capture(output, "shoreline_air")
	print("P12_R2_PROBE_DONE adapter=%s renderer=%s" % [
		RenderingServer.get_video_adapter_name(), RenderingServer.get_current_rendering_method()])
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
	frames.sort()
	var frame_p95 := frames[ceili(float(frames.size()) * 0.95) - 1]
	var image := root.get_texture().get_image()
	var path := output.path_join(label + ".png")
	var saved := image.save_png(path)
	if saved != OK:
		printerr("P12_R2_PROBE_FAILED save=%s error=%d" % [path, saved])
		quit(1)
		return
	print("P12_R2_SAMPLE view=%s frame_ms_median=%.2f frame_ms_p95=%.2f draws_median=%.0f primitives_median=%.0f image=%s" % [
		label, _median(frames), frame_p95, _median(draws), _median(primitives), path])
