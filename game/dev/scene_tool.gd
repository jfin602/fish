extends SceneTree

func _initialize() -> void:
	call_deferred("execute")

func fail(message: String) -> void:
	printerr("SCENE_TOOL_FAILED ", message)
	quit(1)

func execute() -> void:
	var args: PackedStringArray = OS.get_cmdline_user_args()
	if args.is_empty():
		fail("expected inspect <res://scene> or automation-smoke")
		return
	if args[0] == "inspect" and args.size() == 2:
		var path: String = args[1]
		if not path.begins_with("res://") or not ResourceLoader.exists(path):
			fail("scene does not exist: " + path)
			return
		var packed: PackedScene = load(path)
		if packed == null:
			fail("scene failed to load: " + path)
			return
		var instance: Node = packed.instantiate()
		print("SCENE_OK ", path, " root=", instance.name, " children=", instance.get_child_count())
		instance.free()
		quit()
		return
	if args[0] == "automation-smoke" and args.size() == 1:
		var path := "res://.automation-fixtures/round_trip.tscn"
		DirAccess.make_dir_absolute(ProjectSettings.globalize_path("res://.automation-fixtures"))
		var root := Node3D.new()
		root.name = "RoundTrip"
		var marker := Node3D.new()
		marker.name = "Marker"
		marker.position = Vector3(1.25, 2.5, -3.75)
		root.add_child(marker)
		marker.owner = root
		var packed := PackedScene.new()
		if packed.pack(root) != OK or ResourceSaver.save(packed, path) != OK:
			root.free()
			fail("save failed")
			return
		root.free()
		var loaded: PackedScene = load(path)
		if loaded == null:
			DirAccess.remove_absolute(ProjectSettings.globalize_path(path))
			fail("reload failed")
			return
		var copy: Node3D = loaded.instantiate()
		var round_trip: Node3D = copy.get_node_or_null("Marker")
		var ok: bool = round_trip != null and round_trip.position == Vector3(1.25, 2.5, -3.75)
		copy.free()
		DirAccess.remove_absolute(ProjectSettings.globalize_path(path))
		DirAccess.remove_absolute(ProjectSettings.globalize_path("res://.automation-fixtures"))
		if not ok:
			fail("serialized marker did not survive reload")
			return
		print("SCENE_AUTOMATION_OK saved/reloaded Marker position=(1.25, 2.5, -3.75)")
		quit()
		return
	fail("invalid arguments")
