extends Node3D

const SPECIES := {
	"002": ["fish_002_ocellaris_clownfish", 0.154],
	"003": ["fish_003_blue_green_chromis", 0.145],
	"004": ["fish_004_reef_butterflyfish", 0.201],
	"005": ["fish_005_blacktip_reef_shark", 1.57],
}

@onready var camera: Camera3D = $Camera
@onready var environment_node: WorldEnvironment = $WorldEnvironment
var player: AnimationPlayer
var stem: String


func _ready() -> void:
	var id := _argument("--faf-id=")
	if not SPECIES.has(id):
		push_error("FAF proof needs --faf-id=002/003/004/005")
		get_tree().quit(1)
		return
	stem = SPECIES[id][0]
	var length: float = SPECIES[id][1]
	var packed: PackedScene = load("res://assets/faf/fish/" + stem + ".glb") as PackedScene
	if packed == null:
		push_error("FAF proof GLB missing: " + stem)
		get_tree().quit(1)
		return
	var fish: Node3D = packed.instantiate() as Node3D
	add_child(fish)
	fish.rotation.y = PI
	var environment := Environment.new()
	environment.background_mode = Environment.BG_COLOR
	environment.background_color = Color("#116c91")
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	environment.ambient_light_color = Color.WHITE
	environment.ambient_light_energy = 0.55
	environment_node.environment = environment
	camera.position = Vector3(length * 2.35, length * 0.32, length * 0.28)
	camera.look_at(Vector3.ZERO, Vector3.UP)
	player = fish.find_child("AnimationPlayer", true, false) as AnimationPlayer
	if player == null:
		push_error("FAF proof AnimationPlayer missing: " + stem)
		get_tree().quit(1)
		return
	player.play(stem + "__Swim")
	var capture := _argument("--faf-capture=")
	if capture != "":
		_capture_and_quit.call_deferred(capture)


func _unhandled_key_input(event: InputEvent) -> void:
	if player == null or not event is InputEventKey or not event.pressed:
		return
	match event.keycode:
		KEY_1: player.play(stem + "__Glide")
		KEY_2: player.play(stem + "__Swim")
		KEY_3: player.play(stem + "__SwimFast")


func _argument(prefix: String) -> String:
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with(prefix):
			return argument.trim_prefix(prefix)
	return ""


func _capture_and_quit(path: String) -> void:
	for _frame in 5:
		await get_tree().process_frame
	player.seek(player.current_animation_length * 0.25, true)
	await RenderingServer.frame_post_draw
	var image: Image = get_viewport().get_texture().get_image()
	var error: Error = image.save_png(path)
	if error != OK:
		push_error("FAF proof capture failed: " + str(error))
		get_tree().quit(1)
		return
	print("FAF_REEF_FISH_NATIVE_CAPTURE ", stem, " ", path, " ", image.get_width(), "x", image.get_height())
	get_tree().quit(0)
