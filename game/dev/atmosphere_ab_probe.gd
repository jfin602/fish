extends SceneTree

# Same-process Forward+ fixed-view A/B. Runs with a real display; no saved scene edits.
const WARMUP_FRAMES := 120
const SAMPLE_FRAMES := 180

func _initialize() -> void:
	call_deferred("_run")

func _median(values: Array[float]) -> float:
	values.sort()
	return values[floori(values.size() / 2.0)]

func _sample(label: String, enabled: bool, atmosphere: Node3D, camera: Camera3D, eye: Vector3, target: Vector3) -> Dictionary:
	atmosphere.SetEnabled(enabled)
	camera.global_position = eye
	camera.look_at(target, Vector3.UP)
	for frame in range(WARMUP_FRAMES):
		await process_frame
	var fps: Array[float] = []
	var frame_ms: Array[float] = []
	var draws: Array[float] = []
	var primitives: Array[float] = []
	var previous: int = Time.get_ticks_usec()
	for frame in range(SAMPLE_FRAMES):
		await process_frame
		var now: int = Time.get_ticks_usec()
		frame_ms.append(float(now - previous) / 1000.0)
		previous = now
		fps.append(Performance.get_monitor(Performance.TIME_FPS))
		draws.append(Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME))
		primitives.append(Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME))
	print("ATMOSPHERE_AB view=%s effects=%s fps_median=%.1f frame_ms_median=%.2f draws_median=%.0f primitives_median=%.0f samples=%d warmup=%d" % [label, "on" if enabled else "off", _median(fps), _median(frame_ms), _median(draws), _median(primitives), SAMPLE_FRAMES, WARMUP_FRAMES])
	if _median(draws) <= 0.0 or _median(primitives) <= 0.0:
		printerr("ATMOSPHERE_AB_FAILED rendering counters unavailable")
		quit(1)
		return {}
	return {"frame_ms": _median(frame_ms), "draws": _median(draws), "primitives": _median(primitives)}

func _compare(label: String, on: Dictionary, off: Dictionary) -> void:
	if on.is_empty() or off.is_empty():
		return
	var frame_cost: float = 100.0 * (on.frame_ms / off.frame_ms - 1.0)
	var draw_delta: float = on.draws - off.draws
	var primitive_cost: float = 100.0 * (on.primitives / off.primitives - 1.0)
	print("ATMOSPHERE_AB_DELTA view=%s frame_cost_pct=%.1f draw_delta=%.0f primitive_cost_pct=%.1f tune_candidate=%s" % [label, frame_cost, draw_delta, primitive_cost, str(frame_cost >= 10.0 or draw_delta > 2.0)])

func _run() -> void:
	if RenderingServer.get_current_rendering_method() != "forward_plus":
		printerr("ATMOSPHERE_AB_FAILED requires Forward+")
		quit(1)
		return
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	await process_frame
	var atmosphere: Node3D = main.get_node("UnderwaterAtmosphere")
	if RenderingServer.get_video_adapter_name().is_empty() or root.size.x < 640 or root.size.y < 360:
		printerr("ATMOSPHERE_AB_FAILED native adapter/window unavailable")
		quit(1)
		return
	var effects: Node3D = atmosphere.get_node("WaterEffects")
	var shafts: MeshInstance3D = effects.get_node("SunShafts")
	var surface: MeshInstance3D = effects.get_node("OverheadSurface")
	var camera := Camera3D.new()
	camera.name = "ABFixedView"
	camera.far = 100.0
	main.add_child(camera)
	camera.make_current()
	var routes: Node3D = main.get_node("Habitat/Composition/ReefComposition/Routes")
	var open: Vector3 = routes.get_node("OpenFar").global_position
	var upper: Vector3 = routes.get_node("UpperLandmark").global_position
	var plane: PlaneMesh = surface.mesh
	print("ATMOSPHERE_AB_DEVICE renderer=%s adapter=%s viewport=%s shafts=%d shaft_surfaces=%d surface_subdivisions=%dx%d caustics=1 motes=%d screen=1" % [RenderingServer.get_current_rendering_method(), RenderingServer.get_video_adapter_name(), root.size, atmosphere.get("ShaftCount"), shafts.mesh.get_surface_count(), plane.subdivide_width, plane.subdivide_depth, effects.get_node("SuspendedMotes").amount])
	var open_on: Dictionary = await _sample("open", true, atmosphere, camera, open + Vector3(0, 0.4, 3), open + Vector3(0, 0, -16))
	var open_off: Dictionary = await _sample("open", false, atmosphere, camera, open + Vector3(0, 0.4, 3), open + Vector3(0, 0, -16))
	var upper_on: Dictionary = await _sample("upper", true, atmosphere, camera, upper + Vector3(0, 0.3, 2), upper + Vector3(0, 5, -13))
	var upper_off: Dictionary = await _sample("upper", false, atmosphere, camera, upper + Vector3(0, 0.3, 2), upper + Vector3(0, 5, -13))
	if open_on.is_empty() or open_off.is_empty() or upper_on.is_empty() or upper_off.is_empty():
		return
	_compare("open", open_on, open_off)
	_compare("upper", upper_on, upper_off)
	print("ATMOSPHERE_AB_OK same_scene=1 views=2 modes=on,off surface=%s" % surface.name)
	quit()
