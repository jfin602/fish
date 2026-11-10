extends Node3D

# Native Forward+ visual fixture. Only this isolated scene instantiates FAF terrain.
const TERRAIN := [
	"res://assets/faf/terrain/terrain_001a_open_sand.glb",
	"res://assets/faf/terrain/terrain_001b_rolling.glb",
	"res://assets/faf/terrain/terrain_001c_reef_shelf.glb",
	"res://assets/faf/terrain/terrain_001d_slope.glb",
	"res://assets/faf/terrain/terrain_001e_drop_off.glb",
	"res://assets/faf/terrain/terrain_001f_rocky_floor.glb",
]

@onready var camera: Camera3D = $Camera
@onready var environment_node: WorldEnvironment = $WorldEnvironment


func _ready() -> void:
	var environment := Environment.new()
	environment.background_mode = Environment.BG_COLOR
	environment.background_color = Color("#126d92")
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	environment.ambient_light_color = Color.WHITE
	environment.ambient_light_energy = 0.35
	environment_node.environment = environment
	var positions := [
		Vector3(0, -0.04, 0), Vector3(-7.1, 0, 0.0),
		Vector3(-2.1, 0, -0.6), Vector3(2.2, 0, 0.7),
		Vector3(7.1, 0, 0.0), Vector3(0, 0, -6.4),
	]
	for i in TERRAIN.size():
		var packed: PackedScene = load(TERRAIN[i]) as PackedScene
		var chunk: Node3D = packed.instantiate() as Node3D
		chunk.name = "Terrain" + str(i)
		add_child(chunk)
		chunk.position = positions[i]
	_add_scale_context()
	camera.look_at(Vector3(0, 0, -0.4), Vector3.UP)
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--faf-capture="):
			_capture_and_quit.call_deferred(argument.trim_prefix("--faf-capture="))
			return


func _add_scale_context() -> void:
	var neighbors := [
		["res://assets/faf/rocks/rock_001a_reef_boulder.glb", Vector3(-3.6, 0, 1.15)],
		["res://assets/faf/coral/coral_001a_branching.glb", Vector3(-4.3, 0, 0.15)],
		["res://assets/faf/plants/plant_001a_sea_grass.glb", Vector3(3.8, 0, 1.15)],
	]
	for entry in neighbors:
		var packed: PackedScene = load(entry[0]) as PackedScene
		var prop: Node3D = packed.instantiate() as Node3D
		add_child(prop)
		prop.position = entry[1]


func _capture_and_quit(path: String) -> void:
	for _frame in 5:
		await get_tree().process_frame
	await RenderingServer.frame_post_draw
	var image: Image = get_viewport().get_texture().get_image()
	var error: Error = image.save_png(path)
	if error != OK:
		push_error("FAF terrain proof capture failed: " + str(error))
		get_tree().quit(1)
		return
	print("FAF_TERRAIN_001_NATIVE_CAPTURE ", path, " ", image.get_width(), "x", image.get_height(),
		" draw_calls=", Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME))
	get_tree().quit(0)
