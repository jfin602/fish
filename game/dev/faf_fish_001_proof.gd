extends Node3D

const GLIDE := "fish_001_yellow_tang__Glide"
const SWIM := "fish_001_yellow_tang__Swim"
const SWIM_FAST := "fish_001_yellow_tang__SwimFast"

@onready var fish: Node3D = $YellowTang
@onready var camera: Camera3D = $Camera
@onready var environment_node: WorldEnvironment = $WorldEnvironment

var player: AnimationPlayer


func _ready() -> void:
	var environment := Environment.new()
	environment.background_mode = Environment.BG_COLOR
	environment.background_color = Color("#116c91")
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	environment.ambient_light_color = Color.WHITE
	environment.ambient_light_energy = 0.5
	environment_node.environment = environment
	camera.look_at(Vector3.ZERO, Vector3.UP)
	player = fish.find_child("AnimationPlayer", true, false) as AnimationPlayer
	if player == null:
		push_error("FAF proof: imported AnimationPlayer missing")
		return
	player.play(SWIM)
	var capture_path := _capture_argument()
	if capture_path != "":
		_capture_and_quit.call_deferred(capture_path)


func _unhandled_key_input(event: InputEvent) -> void:
	if player == null or not event is InputEventKey or not event.pressed:
		return
	match event.keycode:
		KEY_1: player.play(GLIDE)
		KEY_2: player.play(SWIM)
		KEY_3: player.play(SWIM_FAST)


func _capture_argument() -> String:
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--faf-capture="):
			return argument.trim_prefix("--faf-capture=")
	return ""


func _capture_and_quit(path: String) -> void:
	for _frame in 5:
		await get_tree().process_frame
	player.seek(0.25, true)
	await RenderingServer.frame_post_draw
	var image: Image = get_viewport().get_texture().get_image()
	var error: Error = image.save_png(path)
	if error != OK:
		push_error("FAF proof screenshot failed: " + str(error))
		get_tree().quit(1)
		return
	print("FAF_FISH_001_NATIVE_CAPTURE ", path, " ", image.get_width(), "x", image.get_height())
	get_tree().quit(0)
