extends SceneTree

# Native Forward+ collision review. Run with --debug-collisions and an isolated XDG_DATA_HOME.
const FISH_RADIUS := 0.48

func _initialize() -> void:
	call_deferred("_run")

func fail(reason: String) -> void:
	printerr("COLLISION_NATIVE_FAILED ", reason)
	quit(1)

func key(code: Key, down: bool) -> void:
	var event := InputEventKey.new()
	event.keycode = code
	event.physical_keycode = code
	event.pressed = down
	Input.parse_input_event(event)

func point(root_node: Node, name: String) -> Vector3:
	return (root_node.get_node(name) as Marker3D).global_position

func swim_path(fish: CharacterBody3D, path: Array[Vector3], label: String) -> bool:
	fish.global_position = path[0]
	for destination in path.slice(1):
		var motion: Vector3 = destination - fish.global_position
		var count := ceili(motion.length() / 0.25)
		for step in count:
			var contact := fish.move_and_collide(motion / count)
			if contact != null:
				fail(label + " hit " + str(contact.get_collider().name) + " at " + str(fish.global_position))
				return false
		if fish.global_position.distance_to(destination) > 0.05:
			fail(label + " failed to reach " + str(destination))
			return false
	return true

func capture(folder: String, label: String) -> void:
	for frame in 30:
		await process_frame
	await RenderingServer.frame_post_draw
	var image: Image = root.get_texture().get_image()
	var err := image.save_png(folder.path_join(label + ".png"))
	if err != OK:
		fail("capture failed " + label + " error=" + str(err))
	else:
		print("COLLISION_NATIVE_CAPTURE zone=", label, " size=", image.get_width(), "x", image.get_height())

func _run() -> void:
	var folder := ""
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--collision-capture-dir="):
			folder = argument.trim_prefix("--collision-capture-dir=")
	if folder.is_empty():
		fail("missing capture directory")
		return
	DirAccess.make_dir_recursive_absolute(folder)
	if RenderingServer.get_current_rendering_method() == "gl_compatibility":
		fail("Forward+ renderer required")
		return
	print("COLLISION_NATIVE_RENDERER method=", RenderingServer.get_current_rendering_method(), " driver=", RenderingServer.get_current_rendering_driver_name())
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	await create_timer(1.0).timeout
	var fish: CharacterBody3D = main.get_node("Fish")
	var p2: Node3D = main.get_node("Habitat/Composition/ReefComposition/Routes")
	var geo: Node3D = main.get_node("Habitat/Composition/ReefComposition/Geology/Routes")
	var before := fish.global_position
	await capture(folder, "spawn-debug")
	key(KEY_W, true)
	for frame in 240:
		await physics_frame
	key(KEY_W, false)
	var after := fish.global_position
	if after.z > -11.5:
		fail("real W input did not clear historical z=-10.47 stop: before=" + str(before) + " after=" + str(after))
		return
	print("COLLISION_NATIVE_INPUT action=W before=", before, " after=", after, " distance=", after.distance_to(before))
	await capture(folder, "open-water-after-input")
	fish.set_physics_process(false)
	var paths := {
		"west-tunnel": [point(p2,"TunnelEntry"), point(p2,"TunnelCenter"), point(p2,"TunnelExit")],
		"east-cave": [point(p2,"ShelterEntry"), point(p2,"ShelterCenter"), point(geo,"CaveTurn"), point(geo,"CaveSide"), point(geo,"CaveTurn"), point(p2,"ShelterExit")],
		"deep-corridor": [point(geo,"CanyonEntry"), point(geo,"CanyonCenter"), point(geo,"CanyonExit"), point(geo,"CanyonReturn")],
		"depth-descent": [point(p2,"DepthShallow"), point(p2,"DepthUpper"), point(p2,"DepthMiddle"), point(p2,"DepthLower"), point(p2,"DepthDeep")],
	}
	for label in paths:
		var route: Array = paths[label]
		var typed: Array[Vector3] = []
		for location in route:
			typed.append(location)
		if not swim_path(fish, typed, label):
			return
		print("COLLISION_NATIVE_ROUTE id=", label, " points=", typed.size(), " result=clear")
		fish.global_position = typed[1]
		await capture(folder, label + "-debug")
	var fps: Array[float] = []
	for frame in 90:
		await process_frame
		fps.append(Performance.get_monitor(Performance.TIME_FPS))
	fps.sort()
	print("COLLISION_NATIVE_PERF fps_min=", fps.front(), " fps_median=", fps[45], " frames=", fps.size())
	print("COLLISION_NATIVE_OK renderer=", RenderingServer.get_current_rendering_method(), " routes=4 input=W captures=6")
	quit()
