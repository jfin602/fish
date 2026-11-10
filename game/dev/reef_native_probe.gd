extends SceneTree

# Short native Forward+ sample of the saved P4 scene; no performance target is asserted.
func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	await create_timer(1.0).timeout
	var fps: Array[float] = []
	var draws: Array[float] = []
	var primitives: Array[float] = []
	for i in range(120):
		await process_frame
		fps.append(Performance.get_monitor(Performance.TIME_FPS))
		draws.append(Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME))
		primitives.append(Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME))
	fps.sort()
	draws.sort()
	primitives.sort()
	print("REEF_NATIVE_SAMPLE fps_min=%d fps_median=%d fps_max=%d draw_calls_median=%d primitives_median=%d frames=%d" % [fps.front(), fps[floori(fps.size() / 2.0)], fps.back(), draws[floori(draws.size() / 2.0)], primitives[floori(primitives.size() / 2.0)], fps.size()])
	quit()
