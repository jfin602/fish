extends SceneTree

# Fixed Pond views at two debug-only solar times. Counts do not judge visual quality.
const WARMUP := 60
const SAMPLES := 90

func _initialize() -> void:
	call_deferred("_run")

func _median(values: Array[float]) -> float:
	values.sort()
	return values[values.size() / 2]

func _percentile(values: Array[float], fraction: float) -> float:
	values.sort()
	return values[mini(int(ceil(fraction * values.size())) - 1, values.size() - 1)]

func _run() -> void:
	if RenderingServer.get_current_rendering_method() != "forward_plus" or RenderingServer.get_video_adapter_name().is_empty():
		printerr("SOLAR_WATER_PROBE_FAILED native Forward+ adapter required")
		quit(1)
		return
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	await process_frame
	var clock: Node = main.get_node("EnvironmentClock")
	var atmosphere: Node3D = main.get_node("UnderwaterAtmosphere")
	var pond := OS.get_cmdline_user_args().has("--pond-surface-probe")
	var habitat := "pond" if pond else "ocean"
	var anchor: Vector3 = main.get_node("Fish").global_position
	var direction := Vector2(1.0, 1.0).normalized()
	if pond:
		var bank: Marker3D = main.get_node("Habitat/PondLandmarks/BankApproach")
		var shallow: Marker3D = main.get_node("Habitat/PondLandmarks/shallow")
		anchor = bank.global_position
		direction = Vector2(shallow.global_position.x - bank.global_position.x,
			shallow.global_position.z - bank.global_position.z).normalized()
	var camera := Camera3D.new()
	camera.far = 150.0
	main.add_child(camera)
	camera.make_current()
	main.get_node("PrototypeHelp").visible = false
	for view in ["waterline", "underwater_world"]:
		var height: float
		if pond:
			height = 10.48 if view == "waterline" else 9.2
		else:
			height = float(atmosphere.SurfaceHeight) + (0.8 if view == "waterline" else -1.5)
		var offset := 9.0 if pond else 5.0
		camera.global_position = Vector3(anchor.x + direction.x * offset,
			height, anchor.z + direction.y * offset)
		var look_y: float = 11.6 if pond else float(atmosphere.SurfaceHeight) - 1.0
		camera.look_at(Vector3(anchor.x, look_y, anchor.z), Vector3.UP)
		for hour in [12.0, 0.0]:
			clock.call("SetTimeForDebug", hour)
			atmosphere.call("RefreshSolar", true)
			atmosphere.call("RefreshDepth", true, 2.0)
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
			print("SOLAR_WATER_SAMPLE habitat=%s view=%s hour=%.0f median_ms=%.2f p95_ms=%.2f draws=%.0f primitives=%.0f samples=%d adapter=%s" % [
				habitat, view, hour, _median(frames), _percentile(frames, 0.95),
				_median(draws), _median(primitives), SAMPLES,
				RenderingServer.get_video_adapter_name()])
	quit()
