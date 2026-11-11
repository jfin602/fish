extends SceneTree

# Final M4 habitat review from baked seed-42 world; no scene is saved.
const TARGETS := [
	["shallow-garden", Vector2(71.0, 130.0), Vector2(0.0, -1.0)],
	["shallow-middle-transition", Vector2(72.0, 112.0), Vector2(-0.3, -1.0)],
	["wide-layering", Vector2(62.0, 129.0), Vector2(-0.7, -1.0)],
]

func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	var out := ""
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--c3-capture-dir="):
			out = argument.trim_prefix("--c3-capture-dir=")
	if out.is_empty():
		push_error("C3_M4_PROBE_FAILED capture directory missing")
		quit(2)
		return
	DirAccess.make_dir_recursive_absolute(out)
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	await create_timer(1.0).timeout
	var fish: CharacterBody3D = main.get_node("Fish")
	fish.set_physics_process(false)
	main.get_node("PrototypeHelp").visible = false
	var space := main.get_world_3d().direct_space_state
	var limits: StaticBody3D = main.get_node("Habitat/HiddenLimits")
	for target in TARGETS:
		var at: Vector2 = target[1]
		var query := PhysicsRayQueryParameters3D.create(Vector3(at.x, 60.0, at.y),
			Vector3(at.x, -65.0, at.y))
		query.exclude = [limits.get_rid()]
		var hit := space.intersect_ray(query)
		if hit.is_empty():
			push_error("C3_M4_PROBE_FAILED floor %s" % target[0])
			quit(1)
			return
		var floor_y: float = hit.position.y
		fish.global_position = Vector3(at.x, floor_y + 1.25, at.y)
		var facing: Vector2 = target[2]
		fish.rotation.y = atan2(-facing.x, -facing.y)
		fish.velocity = Vector3.ZERO
		await create_timer(2.0).timeout
		await RenderingServer.frame_post_draw
		var image: Image = root.get_texture().get_image()
		var path := out.path_join(target[0] + ".png")
		var error := image.save_png(path)
		if error != OK:
			push_error("C3_M4_PROBE_FAILED save %s error=%d" % [path, error])
			quit(1)
			return
		print("C3_M4_CAPTURE name=%s depth=%.2f fps=%.1f draw_calls=%.0f primitives=%.0f objects=%.0f path=%s" % [
			target[0], 10.39 - floor_y, Engine.get_frames_per_second(),
			Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME),
			Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME),
			Performance.get_monitor(Performance.RENDER_TOTAL_OBJECTS_IN_FRAME), path])
	quit()
