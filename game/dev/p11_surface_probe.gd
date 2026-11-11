extends SceneTree

# Fixed native Forward+ Pond views. This is a renderer sample, not owner art approval.
const WARMUP := 60
const SAMPLES := 90

func _initialize() -> void:
	call_deferred("_run")

func _median(values: Array[float]) -> float:
	values.sort()
	return values[values.size() / 2]

func _run() -> void:
	if RenderingServer.get_current_rendering_method() != "forward_plus" or RenderingServer.get_video_adapter_name().is_empty():
		printerr("P11_SURFACE_PROBE_FAILED native Forward+ adapter required")
		quit(1)
		return
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	await process_frame
	var bank: Marker3D = main.get_node("Habitat/PondLandmarks/BankApproach")
	var shallow: Marker3D = main.get_node("Habitat/PondLandmarks/shallow")
	var direction := Vector2(shallow.global_position.x - bank.global_position.x,
		shallow.global_position.z - bank.global_position.z).normalized()
	var camera := Camera3D.new()
	camera.far = 150.0
	main.add_child(camera)
	camera.make_current()
	main.get_node("PrototypeHelp").visible = false
	for view in ["air_bank", "waterline", "underwater_bank"]:
		var height := 11.6 if view == "air_bank" else (10.48 if view == "waterline" else 9.2)
		camera.global_position = Vector3(bank.global_position.x + direction.x * 9.0,
			height, bank.global_position.z + direction.y * 9.0)
		camera.look_at(Vector3(bank.global_position.x, 11.6,
			bank.global_position.z), Vector3.UP)
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
		print("P11_SURFACE_SAMPLE view=%s frame_ms_median=%.2f draws_median=%.0f primitives_median=%.0f samples=%d adapter=%s" % [
			view, _median(frames), _median(draws), _median(primitives), SAMPLES,
			RenderingServer.get_video_adapter_name()])
	quit()
