extends Node3D

# Isolated native fixture; never instanced by the live P2 habitat.
@onready var camera: Camera3D = $Camera
@onready var environment_node: WorldEnvironment = $WorldEnvironment


func _ready() -> void:
	var environment := Environment.new()
	environment.background_mode = Environment.BG_COLOR
	environment.background_color = Color("#126d92")
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	environment.ambient_light_color = Color.WHITE
	environment.ambient_light_energy = 0.48
	environment_node.environment = environment
	camera.look_at(Vector3(0, 0.48, 0), Vector3.UP)
	for argument in OS.get_cmdline_user_args():
		if argument == "--faf-density":
			_add_density_instances()
		if argument.begins_with("--faf-capture="):
			_capture_and_quit.call_deferred(argument.trim_prefix("--faf-capture="))
			return


func _add_density_instances() -> void:
	var paths := [
		"res://assets/faf/plants/plant_001a_sea_grass.glb",
		"res://assets/faf/plants/plant_001b_sea_grass.glb",
		"res://assets/faf/plants/plant_001c_sea_grass.glb",
		"res://assets/faf/plants/plant_003a_reef_algae.glb",
	]
	for row in 3:
		for column in 6:
			var packed: PackedScene = load(paths[(row * 2 + column) % paths.size()]) as PackedScene
			var plant: Node3D = packed.instantiate() as Node3D
			add_child(plant)
			plant.position = Vector3((column - 2.5) * 0.97, 0, 1.25 - row * 0.72)
			plant.rotation.y = float((row * 7 + column * 3) % 11) * 0.39


func _capture_and_quit(path: String) -> void:
	for _frame in 5:
		await get_tree().process_frame
	await RenderingServer.frame_post_draw
	var image: Image = get_viewport().get_texture().get_image()
	var error: Error = image.save_png(path)
	if error != OK:
		push_error("FAF plant proof screenshot failed: " + str(error))
		get_tree().quit(1)
		return
	print("FAF_PLANT_PACK_001_NATIVE_CAPTURE ", path, " ", image.get_width(), "x", image.get_height())
	get_tree().quit(0)
