extends SceneTree

# Forward+ review of the five baked M2 geological passages. No saved scene edits.
func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	var out := ""
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--c3-capture-dir="):
			out = argument.trim_prefix("--c3-capture-dir=")
	if out.is_empty():
		push_error("C3_M2_PROBE_FAILED capture directory missing")
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
	var base := "Habitat/Composition/ReefComposition/Geology/GeneratedRegions/"
	for region in ["west-swim-through", "east-swim-through", "cave-chamber", "stone-arch", "canyon-ridge"]:
		var line: Node3D = main.get_node(base + region + "/Centerline")
		var entry: Vector3 = line.get_node("Entry").global_position
		var center: Vector3 = line.get_node("Center").global_position
		var exit_at: Vector3 = line.get_node("Exit").global_position
		var inward := (center - entry).normalized()
		var reverse := (center - exit_at).normalized()
		for sample in [
			["far", entry - inward * 14.0, inward],
			["medium", entry - inward * 7.0, inward],
			["entrance", entry - inward * 1.8, inward],
			["inside", center, (exit_at - center).normalized()],
			["reverse", exit_at - reverse * 7.0, reverse],
		]:
			fish.global_position = sample[1]
			fish.rotation.y = atan2(-sample[2].x, -sample[2].z)
			fish.velocity = Vector3.ZERO
			await create_timer(0.55).timeout
			await RenderingServer.frame_post_draw
			var image: Image = root.get_texture().get_image()
			var filename: String = region + "-" + sample[0] + ".png"
			var error := image.save_png(out.path_join(filename))
			if error != OK:
				push_error("C3_M2_PROBE_FAILED save %s: %d" % [filename, error])
				quit(1)
				return
			print("C3_M2_CAPTURE name=%s fish=%s size=%dx%d" % [filename, fish.global_position,
				image.get_width(), image.get_height()])
	quit()
