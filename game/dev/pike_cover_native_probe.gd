extends SceneTree

# Read-only Forward+ fixture survey. Screenshots are local ignored artifacts.
var output := ""

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--pike-output="):
			output = argument.trim_prefix("--pike-output=")
	if output.is_empty() or DirAccess.make_dir_recursive_absolute(output) != OK:
		_fail("output directory unavailable")
		return
	if RenderingServer.get_current_rendering_method() != "forward_plus" or RenderingServer.get_video_adapter_name().is_empty():
		_fail("native Forward+ required")
		return
	if ProjectSettings.get_setting("application/config/name") != "Fish":
		_fail("wrong project")
		return
	var packed := load("res://scenes/p5_pike_encounter.tscn") as PackedScene
	if packed == null:
		_fail("fixture missing")
		return
	var fixture := packed.instantiate() as Node3D
	root.add_child(fixture)
	current_scene = fixture
	await process_frame
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
	var camera := Camera3D.new()
	camera.far = 90.0
	fixture.add_child(camera)
	camera.make_current()
	var views := {
		"nursery": [Vector3(4, 2.7, 28), Vector3(5, -.2, 16)],
		"refuge": [Vector3(1, 2.1, 10), Vector3(0, -1.8, -3)],
		"maze": [Vector3(0, .4, -9), Vector3(1, -3.1, -22)],
	}
	for label in views:
		camera.global_position = views[label][0]
		camera.look_at(views[label][1])
		for frame in range(32):
			await process_frame
		var image := root.get_texture().get_image()
		if image.save_png(output.path_join(label + ".png")) != OK:
			_fail("capture failed " + label)
			return
		print("PIKE_COVER_NATIVE_SAMPLE view=", label,
			" adapter=", RenderingServer.get_video_adapter_name(),
			" draws=", Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME),
			" primitives=", Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME),
			" image=", output.path_join(label + ".png"))
	print("PIKE_COVER_NATIVE_SURVEY_OK views=3 owner_gameplay=not_observed")
	quit()

func _fail(message: String) -> void:
	printerr("PIKE_COVER_NATIVE_FAILED ", message)
	quit(1)
