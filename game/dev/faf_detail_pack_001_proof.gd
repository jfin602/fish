extends Node3D

# One isolated visual cluster; every detail remains outside the live habitat.
const STEMS := [
	"detail_001a_fan_shell", "detail_001b_ridge_shell",
	"detail_002a_coral_rubble", "detail_002b_reef_rubble",
	"detail_003a_slate_pebbles", "detail_003b_warm_pebbles",
	"detail_004a_encrusting_sponge", "detail_004b_cup_sponge",
]

@onready var camera: Camera3D = $Camera
@onready var environment_node: WorldEnvironment = $WorldEnvironment


func _ready() -> void:
	var environment := Environment.new()
	environment.background_mode = Environment.BG_COLOR
	environment.background_color = Color("#116d91")
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	environment.ambient_light_color = Color.WHITE
	environment.ambient_light_energy = 0.58
	environment_node.environment = environment
	var positions := [
		Vector3(-0.50, 0, 0.20), Vector3(-0.31, 0, -0.17),
		Vector3(-0.09, 0, 0.13), Vector3(0.06, 0, -0.19),
		Vector3(0.27, 0, 0.22), Vector3(0.46, 0, -0.16),
		Vector3(0.68, 0, 0.18), Vector3(0.79, 0, -0.20),
	]
	for index in STEMS.size():
		var packed: PackedScene = load("res://assets/faf/details/" + STEMS[index] + ".glb") as PackedScene
		if packed == null:
			push_error("FAF detail proof missing " + STEMS[index])
			get_tree().quit(1)
			return
		var detail: Node3D = packed.instantiate() as Node3D
		detail.position = positions[index]
		add_child(detail)
	camera.look_at(Vector3(0.14, 0, 0), Vector3.UP)
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--faf-capture="):
			_capture_and_quit.call_deferred(argument.trim_prefix("--faf-capture="))
			return


func _capture_and_quit(path: String) -> void:
	for _frame in 5:
		await get_tree().process_frame
	await RenderingServer.frame_post_draw
	var image: Image = get_viewport().get_texture().get_image()
	var error: Error = image.save_png(path)
	if error != OK:
		push_error("FAF detail proof capture failed: " + str(error))
		get_tree().quit(1)
		return
	print("FAF_DETAIL_PACK_001_NATIVE_CAPTURE ", path, " ", image.get_width(), "x", image.get_height())
	print("FAF_DETAIL_PACK_001_NATIVE_COST draw_calls=",
		Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME),
		" primitives=", Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME))
	get_tree().quit(0)
