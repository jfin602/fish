extends SceneTree

# Native, same-process A/B of depth fog only. The whole atmosphere stays enabled.
const WARMUP_FRAMES := 120
const SAMPLE_FRAMES := 180
const ROUTES := "Habitat/Composition/ReefComposition/Routes"

func _initialize() -> void:
	call_deferred("_run")

func _median(values: Array[float]) -> float:
	values.sort()
	return values[values.size() / 2]

func _sample(label: String, bypass: bool, atmosphere: Node3D, camera: Camera3D, eye: Vector3, target: Vector3) -> Dictionary:
	camera.global_position = eye
	camera.look_at(target, Vector3.UP)
	atmosphere.call("SetVisibilityDiagnosticBypass", bypass)
	atmosphere.call("RefreshDepth", true)
	for i in WARMUP_FRAMES:
		await process_frame
	var frame_ms: Array[float] = []
	var draws: Array[float] = []
	var primitives: Array[float] = []
	var previous := Time.get_ticks_usec()
	for i in SAMPLE_FRAMES:
		await process_frame
		var now := Time.get_ticks_usec()
		frame_ms.append(float(now - previous) / 1000.0)
		previous = now
		draws.append(Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME))
		primitives.append(Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME))
	var result := {"frame_ms": _median(frame_ms), "draws": _median(draws), "primitives": _median(primitives)}
	print("VISIBILITY_AB view=%s mode=%s depth=%.3f base_m=%.1f effective_m=%.1f median_frame_ms=%.2f median_draws=%.0f median_primitives=%.0f samples=%d warmup=%d" % [label, "bypass" if bypass else "new", atmosphere.EffectiveDepth01, atmosphere.VisibilityRangeMeters, atmosphere.EffectiveUsefulRange, result.frame_ms, result.draws, result.primitives, SAMPLE_FRAMES, WARMUP_FRAMES])
	return result

func _run() -> void:
	if RenderingServer.get_current_rendering_method() != "forward_plus" or RenderingServer.get_video_adapter_name().is_empty() or root.size.x < 640 or root.size.y < 360:
		printerr("VISIBILITY_AB_FAILED native Forward+ adapter/window required")
		quit(1)
		return
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	await process_frame
	var atmosphere: Node3D = main.get_node("UnderwaterAtmosphere")
	var env: Environment = main.get_node("WorldEnvironment").environment
	var routes: Node3D = main.get_node(ROUTES)
	var camera := Camera3D.new()
	camera.name = "VisibilityFixedView"
	camera.far = 100.0
	main.add_child(camera)
	camera.make_current()
	print("VISIBILITY_AB_DEVICE renderer=%s adapter=%s resolution=%s scene=main.tscn preset=record_GFX_choice base_m=%.1f volumetric=%.3f height=%.3f" % [RenderingServer.get_current_rendering_method(), RenderingServer.get_video_adapter_name(), root.size, atmosphere.VisibilityRangeMeters, env.volumetric_fog_density, env.fog_height_density])
	for label in ["shallow", "deep"]:
		var marker: Marker3D = routes.get_node("DepthShallow" if label == "shallow" else "DepthDeep")
		var eye := marker.global_position + Vector3(0, 1.5, 3)
		var target := marker.global_position + Vector3(0, 1.5, -16)
		var bypass: Dictionary = await _sample(label, true, atmosphere, camera, eye, target)
		var treatment: Dictionary = await _sample(label, false, atmosphere, camera, eye, target)
		if bypass.draws <= 0 or bypass.primitives <= 0 or treatment.draws <= 0 or treatment.primitives <= 0:
			printerr("VISIBILITY_AB_FAILED rendering counters unavailable")
			quit(1)
			return
		print("VISIBILITY_AB_DELTA view=%s frame_pct=%.1f draw_delta=%.0f primitives_delta=%.0f" % [label, 100.0 * (treatment.frame_ms / bypass.frame_ms - 1.0), treatment.draws - bypass.draws, treatment.primitives - bypass.primitives])
	atmosphere.call("SetVisibilityDiagnosticBypass", false)
	print("VISIBILITY_AB_OK scene=main.tscn modes=bypass,new errors=see_engine_log")
	quit()
