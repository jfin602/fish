extends SceneTree

# Matched fixed-clock native Pond lighting evidence; no scene is saved.
const WARMUP := 24
const SAMPLES := 90
var output := ""
var mode := "fixed"
var hour := 12.0
var main: Node3D
var fish: Node3D
var camera: Camera3D
var atmosphere: Node3D
var environment: Environment
var clock: Node
var seabed: Node
var manifest: Dictionary = {}

func _initialize() -> void:
	call_deferred("_run")

func _fail(message: String) -> void:
	printerr("CLARITY_R3_FAILED ", message)
	quit(1)

func _run() -> void:
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--lighting-output="):
			output = argument.trim_prefix("--lighting-output=")
		elif argument.begins_with("--lighting-mode="):
			mode = argument.trim_prefix("--lighting-mode=")
		elif argument.begins_with("--lighting-hour="):
			hour = float(argument.trim_prefix("--lighting-hour="))
	if output.is_empty() or DirAccess.make_dir_recursive_absolute(output) != OK:
		_fail("output directory required")
		return
	if RenderingServer.get_current_rendering_method() != "forward_plus" or RenderingServer.get_video_adapter_name().is_empty():
		_fail("native Forward+ required")
		return
	if ProjectSettings.get_setting("application/config/name") != "Fish":
		_fail("wrong project")
		return
	if not OS.get_cmdline_user_args().has("--pond-surface-probe") or not OS.get_cmdline_user_args().has("--environment-time-debug"):
		_fail("Pond Bluegill selection and fixed time required")
		return
	DisplayServer.window_set_vsync_mode(DisplayServer.VSYNC_DISABLED)
	Engine.max_fps = 0
	main = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	await process_frame
	if not is_instance_valid(main) or main.get_node_or_null("Fish/PlayerVisual") == null:
		_fail("selectable fish missing")
		return
	fish = main.get_node("Fish")
	atmosphere = main.get_node("UnderwaterAtmosphere")
	environment = main.get_node("WorldEnvironment").environment
	clock = main.get_node("EnvironmentClock")
	seabed = main.get_node("Habitat/Seabed")
	main.get_node("PrototypeHelp").visible = false
	camera = Camera3D.new()
	camera.far = 150.0
	main.add_child(camera)
	camera.make_current()
	clock.call("SetTimeForDebug", hour)
	atmosphere.call("RefreshSolar", true)
	paused = true
	var sun: DirectionalLight3D = main.get_node("Sun")
	manifest = {
		"project": "Fish", "world": "pond-p4-71", "seed": 71,
		"character": "FAF-FISH-006 adult Bluegill", "time_hour": hour,
		"renderer": RenderingServer.get_current_rendering_method(),
		"adapter": RenderingServer.get_video_adapter_name(), "godot": Engine.get_version_info().string,
		"viewport": str(root.size), "vsync": "disabled", "fps_cap": 0,
		"lighting": "Pond Overcast", "sun_shadow_enabled": sun.shadow_enabled,
		"sun_energy": sun.light_energy, "sun_color": str(sun.light_color),
		"sun_basis": str(sun.global_basis), "ambient_energy": environment.ambient_light_energy,
		"views": {}
	}
	print("CLARITY_R3_SETUP ", JSON.stringify(manifest))
	if mode == "fixed":
		for view in ["A1_spawn", "A2_near", "A3_log", "A4_shallow", "A6_middle", "A7_deep", "A8_surface", "A9_shade", "A10_air"]:
			_pose(view)
			atmosphere.call("RefreshDepth", true, 2.0)
			await _capture(view, view in ["A2_near", "A3_log", "A4_shallow", "A6_middle", "A7_deep", "A10_air"])
	elif mode == "diagnostics":
		await _diagnostics()
	elif mode == "up":
		_pose("A11_up")
		atmosphere.call("RefreshDepth", true, 2.0)
		await _capture("A11_up", true)
	elif mode == "motion":
		await _motion()
	else:
		_fail("unknown mode " + mode)
		return
	var file := FileAccess.open(output.path_join("manifest.json"), FileAccess.WRITE)
	file.store_string(JSON.stringify(manifest, "  "))
	file.close()
	print("CLARITY_R3_DONE mode=%s views=%d" % [mode, manifest.views.size()])
	quit()

func _pose(view: String) -> void:
	var surface_y: float = atmosphere.SurfaceHeight
	var landmarks: Node3D = main.get_node("Habitat/PondLandmarks")
	var spawn: Vector3 = main.get_node("Habitat/Spawn").global_position
	var shallow: Vector3 = landmarks.get_node("shallow").global_position
	var middle: Vector3 = landmarks.get_node("middle").global_position
	var deep: Vector3 = landmarks.get_node("deep").global_position
	var hero: Node3D
	for child in main.get_node("Habitat/PondFafEnvironment").get_children():
		if child.name.begins_with("FAF_WOOD_002A_"): hero = child
	match view:
		"A1_spawn":
			fish.global_position = Vector3(spawn.x, minf(surface_y - 1.1, spawn.y + 0.4), spawn.z)
			camera.global_position = fish.global_position + Vector3(0, 0.65, 3.2)
			camera.look_at(fish.global_position + Vector3(0, 0.15, -2), Vector3.UP)
		"A2_near":
			fish.global_position = Vector3(-19.64, surface_y - 1.6, 23.96)
			camera.global_position = fish.global_position + Vector3(0, 0.45, 3.2)
			camera.look_at(fish.global_position + Vector3(0, 0.0, -2), Vector3.UP)
		"A3_log", "A9_shade":
			fish.global_position = hero.to_global(Vector3(0, 2.52, 6.2 if view == "A3_log" else 1.0))
			camera.global_position = hero.to_global(Vector3(0, 2.8, 9.0 if view == "A3_log" else 3.5))
			camera.look_at(hero.to_global(Vector3(0, 2.4, 0)), Vector3.UP)
		"A4_shallow":
			fish.global_position = Vector3(shallow.x, minf(surface_y - 0.8, shallow.y + 1.0), shallow.z)
			camera.global_position = fish.global_position + Vector3(0, 0.35, 3.0)
			camera.look_at(fish.global_position + Vector3(0, 0, -4), Vector3.UP)
		"A6_middle":
			fish.global_position = middle + (shallow - middle).normalized() * 3.5
			camera.global_position = middle + Vector3(0, 0.5, 0)
			camera.look_at(fish.global_position, Vector3.UP)
		"A7_deep":
			fish.global_position = deep + (middle - deep).normalized() * 3.5
			camera.global_position = deep + Vector3(0, 0.5, 0)
			camera.look_at(fish.global_position, Vector3.UP)
		"A8_surface":
			fish.global_position = Vector3(-19.64, surface_y - 2.0, 23.96)
			camera.global_position = fish.global_position + Vector3(0, -0.1, 3.2)
			camera.look_at(fish.global_position + Vector3(0, 2.4, -4), Vector3.UP)
		"A10_air":
			fish.global_position = Vector3(-19.64, surface_y - 0.5, 23.96)
			camera.global_position = Vector3(-19.64, surface_y + 1.2, 26.96)
			camera.look_at(Vector3(-19.64, surface_y - 0.8, 23.96), Vector3.UP)
		"A11_up":
			fish.global_position = Vector3(-19.64, surface_y - 1.6, 23.96)
			camera.global_position = fish.global_position + Vector3(0, 0.25, 2.6)
			camera.look_at(camera.global_position + Vector3(0, 5.0, -1.5), Vector3.UP)
	print("CLARITY_R3_POSE view=%s fish=%s camera=%s" % [view, fish.global_transform, camera.global_transform])

func _median(values: Array[float]) -> float:
	values.sort()
	return values[values.size() / 2]

func _capture(label: String, benchmark: bool) -> void:
	for i in WARMUP:
		await process_frame
	var frames: Array[float] = []
	var draws: Array[float] = []
	var primitives: Array[float] = []
	if benchmark:
		var previous := Time.get_ticks_usec()
		for i in SAMPLES:
			await process_frame
			var now := Time.get_ticks_usec()
			frames.append(float(now - previous) / 1000.0)
			previous = now
			draws.append(Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME))
			primitives.append(Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME))
	var path := output.path_join(label + ".png")
	if root.get_texture().get_image().save_png(path) != OK:
		_fail("save " + path)
		return
	var sun: DirectionalLight3D = main.get_node("Sun")
	var row := {
		"fish_transform": str(fish.global_transform), "camera_transform": str(camera.global_transform),
		"camera_position": str(camera.global_position), "camera_basis": str(camera.global_basis),
		"surface_y": atmosphere.SurfaceHeight, "fog_density": environment.fog_density,
		"volumetric_density": environment.volumetric_fog_density,
		"sun_energy": sun.light_energy, "sun_shadow_enabled": sun.shadow_enabled,
		"ambient_energy": environment.ambient_light_energy,
		"image": path, "width": root.get_texture().get_image().get_width(),
		"height": root.get_texture().get_image().get_height()
	}
	if benchmark:
		var ordered := frames.duplicate()
		ordered.sort()
		row["median_ms"] = _median(frames)
		row["p95_ms"] = ordered[ceili(float(ordered.size()) * 0.95) - 1]
		row["draws"] = _median(draws)
		row["primitives"] = _median(primitives)
		row["samples"] = SAMPLES
	manifest.views[label] = row
	print("CLARITY_R3_SAMPLE view=%s median=%s p95=%s draws=%s primitives=%s image=%s" % [label, row.get("median_ms", "na"), row.get("p95_ms", "na"), row.get("draws", "na"), row.get("primitives", "na"), path])

func _diagnostics() -> void:
	_pose("A3_log")
	atmosphere.call("RefreshDepth", true, 2.0)
	await _capture("D0_full", true)
	var sun: DirectionalLight3D = main.get_node("Sun")
	var base_sun: float = sun.light_energy
	sun.light_energy = 0.0
	await _capture("D1_direct_off", false)
	sun.light_energy = base_sun
	var base_ambient: float = environment.ambient_light_energy
	environment.ambient_light_energy = 0.0
	await _capture("D2_ambient_off", false)
	environment.ambient_light_energy = base_ambient
	var shaft: ShaderMaterial = atmosphere.get_node("WaterEffects/SunShafts").material_override
	var base_shaft: float = shaft.get_shader_parameter("shaft_brightness")
	shaft.set_shader_parameter("shaft_brightness", 0.0)
	await _capture("D3_shafts_off", false)
	shaft.set_shader_parameter("shaft_brightness", base_shaft)
	var caustic: Decal = atmosphere.get_node("WaterEffects/CausticProjection1")
	var base_caustic: float = caustic.emission_energy
	caustic.emission_energy = 0.0
	await _capture("D4_caustics_off", false)
	caustic.emission_energy = base_caustic
	sun.shadow_enabled = true
	await _capture("D5_shadows_on", true)
	var surface: MeshInstance3D = atmosphere.get_node("WaterEffects/OverheadSurface")
	surface.cast_shadow = 0
	sun.directional_shadow_mode = 0
	sun.directional_shadow_max_distance = 28.0
	await _capture("D7_one_split_no_surface_caster", true)
	sun.directional_shadow_max_distance = 16.0
	await _capture("D8_near_shadow_distance", true)
	sun.shadow_enabled = false
	await _capture("D6_restored", false)

func _key(code: Key, down: bool) -> void:
	var event := InputEventKey.new()
	event.physical_keycode = code
	event.pressed = down
	Input.parse_input_event(event)

func _motion() -> void:
	var spawn: Vector3 = main.get_node("Habitat/Spawn").global_position
	fish.global_position = Vector3(spawn.x, minf(spawn.y + 0.4, atmosphere.SurfaceHeight - 2.6), spawn.z)
	fish.call("SetInputMode", "keyboard")
	var follow: Camera3D = main.get_node("Fish/CameraRig/Camera3D")
	follow.make_current()
	paused = false
	clock.call("SetTimeForDebug", hour)
	for i in 10: await process_frame
	_key(KEY_D, true)
	for i in 36: await physics_frame
	_key(KEY_D, false)
	_key(KEY_W, true)
	var beginning := Time.get_ticks_usec()
	var previous := beginning
	var next_photo := 0
	var frames: Array[float] = []
	var draws: Array[float] = []
	var primitives: Array[float] = []
	var points: Array = []
	while Time.get_ticks_usec() - beginning < 8000000:
		await process_frame
		var now := Time.get_ticks_usec()
		frames.append(float(now - previous) / 1000.0)
		previous = now
		draws.append(Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME))
		primitives.append(Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME))
		if now - beginning >= next_photo * 1000000:
			var label := "M_%02d" % next_photo
			var path := output.path_join(label + ".png")
			if root.get_texture().get_image().save_png(path) != OK:
				_fail("motion image " + path)
				return
			points.append({"second": float(now - beginning) / 1000000.0,
				"fish": str(fish.global_transform), "camera": str(follow.global_transform),
				"thrust": fish.get("LastThrust"), "image": path})
			print("CLARITY_R3_MOTION second=%d fish=%s camera=%s thrust=%s" % [
				next_photo, fish.global_position, follow.global_position, fish.get("LastThrust")])
			next_photo += 1
	_key(KEY_W, false)
	paused = true
	var ordered := frames.duplicate(); ordered.sort()
	manifest["motion"] = {"duration_seconds": 8, "start": str(spawn), "end": str(fish.global_position),
		"preset": fish.call("GetActivePresetId"), "samples": frames.size(),
		"median_ms": _median(frames), "p95_ms": ordered[ceili(float(ordered.size()) * 0.95) - 1],
		"draws": _median(draws), "primitives": _median(primitives), "frames": points,
		"display": "1920x1200 exclusive fullscreen, VSync disabled, uncapped"}
	print("CLARITY_R3_MOTION_RESULT ", JSON.stringify(manifest.motion))
