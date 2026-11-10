extends SceneTree

# Native Forward+ evidence helper. It changes only the in-memory test scene.
func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	var out := ""
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--p3-capture-dir="):
			out = argument.trim_prefix("--p3-capture-dir=")
	if out.is_empty():
		push_error("P3_NATIVE_PROBE_FAILED missing capture directory")
		quit(2)
		return
	DirAccess.make_dir_recursive_absolute(out)
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	await create_timer(1.0).timeout
	var fish: CharacterBody3D = main.get_node("Fish")
	var seabed_visual: MeshInstance3D = main.get_node("Habitat/Seabed/Mesh")
	var terrain_visuals: Node3D = main.get_node("Habitat/Composition/ReefComposition/Terrain/Visuals")
	await _capture(out, "spawn-combined")
	seabed_visual.visible = false
	await _capture(out, "spawn-faf-only")
	seabed_visual.visible = true
	terrain_visuals.visible = false
	await _capture(out, "spawn-underlay-only")
	terrain_visuals.visible = true
	var shadow_states: Array[int] = []
	for mesh in terrain_visuals.find_children("*", "MeshInstance3D", true, false):
		shadow_states.append(mesh.cast_shadow)
		mesh.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	await _capture(out, "spawn-terrain-no-shadows")
	var shadow_index := 0
	for mesh in terrain_visuals.find_children("*", "MeshInstance3D", true, false):
		mesh.cast_shadow = shadow_states[shadow_index]
		shadow_index += 1
	for zone in ["DepthShallow", "DepthUpper", "DepthMiddle", "DepthLower", "DepthDeep", "TunnelEntry", "ShelterCenter", "CanyonCenter"]:
		var marker: Marker3D = main.get_node("Habitat/Composition/ReefComposition/Routes/" + zone) if zone != "CanyonCenter" else main.get_node("Habitat/Composition/ReefComposition/Geology/Routes/CanyonCenter")
		fish.global_position = marker.global_position
		fish.velocity = Vector3.ZERO
		await create_timer(0.6).timeout
		var fps: Array[float] = []
		var draws: Array[float] = []
		var primitives: Array[float] = []
		for i in 90:
			await process_frame
			fps.append(Performance.get_monitor(Performance.TIME_FPS))
			draws.append(Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME))
			primitives.append(Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME))
		fps.sort(); draws.sort(); primitives.sort()
		print("P3_NATIVE_SAMPLE zone=%s fps_min=%d fps_median=%d draws_median=%d primitives_median=%d frames=%d" % [zone, fps.front(), fps[45], draws[45], primitives[45], fps.size()])
		await _capture(out, zone)
	quit()

func _capture(out: String, name: String) -> void:
	await process_frame
	await RenderingServer.frame_post_draw
	var image: Image = root.get_texture().get_image()
	var err := image.save_png(out.path_join(name + ".png"))
	if err != OK:
		push_error("P3_NATIVE_PROBE_FAILED capture=%s error=%d" % [name, err])
	else:
		print("P3_NATIVE_CAPTURE name=%s size=%dx%d" % [name, image.get_width(), image.get_height()])
