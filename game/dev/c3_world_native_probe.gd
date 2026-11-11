extends SceneTree

# Native review helper: samples the normal baked world without saving scene edits.
func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	var out := ""
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--c3-capture-dir="):
			out = argument.trim_prefix("--c3-capture-dir=")
	if out.is_empty():
		push_error("C3_NATIVE_PROBE_FAILED missing capture directory")
		quit(2)
		return
	DirAccess.make_dir_recursive_absolute(out)
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	await create_timer(1.0).timeout
	var fish: CharacterBody3D = main.get_node("Fish")
	var space := main.get_world_3d().direct_space_state
	var hidden_limits: StaticBody3D = main.get_node("Habitat/HiddenLimits")
	for sample in [
		["bank-east", Vector2(22, 0)], ["bank-west", Vector2(-22, 0)],
		["bank-north", Vector2(0, -22)], ["bank-south", Vector2(0, 22)],
	]:
		var position: Vector2 = sample[1]
		var query := PhysicsRayQueryParameters3D.create(Vector3(position.x, 30, position.y), Vector3(position.x, -30, position.y))
		query.exclude = [hidden_limits.get_rid()]
		var hit := space.intersect_ray(query)
		if hit.is_empty():
			push_error("C3_NATIVE_PROBE_FAILED no floor at " + str(sample[0]))
			continue
		var floor_y: float = hit.position.y
		fish.global_position = Vector3(position.x, minf(8.2, floor_y + 1.7), position.y)
		fish.rotation.y = atan2(-position.x, -position.y)
		fish.velocity = Vector3.ZERO
		await create_timer(1.0).timeout
		print("C3_NATIVE_SAMPLE zone=%s floor_y=%.2f fish=%s" % [sample[0], floor_y, fish.global_position])
		await _capture(out, sample[0])
	var base := "Habitat/Composition/ReefComposition/Geology/GeneratedRegions/"
	for region in ["cave-chamber", "west-swim-through", "east-swim-through", "stone-arch", "canyon-ridge"]:
		var marker: Marker3D = main.get_node(base + region + "/Centerline/Center")
		fish.global_position = marker.global_position
		fish.velocity = Vector3.ZERO
		await create_timer(1.0).timeout
		print("C3_NATIVE_SAMPLE zone=%s fish=%s" % [region, fish.global_position])
		await _capture(out, region)
	quit()

func _capture(out: String, name: String) -> void:
	await process_frame
	await RenderingServer.frame_post_draw
	var image: Image = root.get_texture().get_image()
	var err := image.save_png(out.path_join(name + ".png"))
	if err != OK:
		push_error("C3_NATIVE_PROBE_FAILED capture=%s error=%d" % [name, err])
	else:
		print("C3_NATIVE_CAPTURE name=%s size=%dx%d" % [name, image.get_width(), image.get_height()])
