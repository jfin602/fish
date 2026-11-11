extends Node3D

# Isolated Forward+ asset viewer. Every imported instance keeps scale one.
const SPECIES := [
	["001", "fish_001_yellow_tang"],
	["002", "fish_002_ocellaris_clownfish"],
	["003", "fish_003_blue_green_chromis"],
	["004", "fish_004_reef_butterflyfish"],
	["005", "fish_005_blacktip_reef_shark"],
]
const BONY_STAGES := ["egg", "larva", "juvenile", "subadult", "adult"]
const SHARK_STAGES := ["newborn_pup", "early_juvenile", "juvenile", "subadult", "adult"]

@onready var camera: Camera3D = $Camera
@onready var world_environment: WorldEnvironment = $WorldEnvironment
var species_index := 0
var stage_index := 2
var far_view := false
var current_asset: Node3D
var current_player: AnimationPlayer
var current_stem := ""
var current_size := 0.2
var caption: Label


func _ready() -> void:
	var environment := Environment.new()
	environment.background_mode = Environment.BG_COLOR
	environment.background_color = Color("#116c91")
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	environment.ambient_light_color = Color("#C6E5E4")
	environment.ambient_light_energy = 0.55
	environment.fog_enabled = true
	environment.fog_light_color = Color("#227F9D")
	environment.fog_density = 0.012
	world_environment.environment = environment
	var canvas := CanvasLayer.new()
	add_child(canvas)
	caption = Label.new()
	caption.position = Vector2(20, 18)
	caption.add_theme_font_size_override("font_size", 20)
	caption.mouse_filter = Control.MOUSE_FILTER_IGNORE
	canvas.add_child(caption)
	var requested_id := _argument("--faf-id=")
	for index in SPECIES.size():
		if SPECIES[index][0] == requested_id:
			species_index = index
	var requested_stage := _argument("--faf-stage=")
	var stages: Array = _stages()
	if stages.has(requested_stage):
		stage_index = stages.find(requested_stage)
	far_view = _argument("--faf-view=") == "far"
	_show_selected()
	var capture := _argument("--faf-capture=")
	if capture != "":
		_capture_and_quit.call_deferred(capture)


func _stages() -> Array:
	return SHARK_STAGES if species_index == 4 else BONY_STAGES


func _show_selected() -> void:
	if current_asset != null:
		current_asset.queue_free()
		current_asset = null
	current_player = null
	var stage: String = _stages()[stage_index]
	current_stem = SPECIES[species_index][1]
	if stage != "adult":
		current_stem += "__" + stage
	var packed: PackedScene = load("res://assets/faf/fish/" + current_stem + ".glb") as PackedScene
	if packed == null:
		push_error("Missing life-stage asset " + current_stem)
		get_tree().quit(1)
		return
	current_asset = packed.instantiate() as Node3D
	add_child(current_asset)
	current_asset.rotation.y = PI
	current_player = current_asset.find_child("AnimationPlayer", true, false) as AnimationPlayer
	if current_player != null:
		current_player.play(current_stem + "__Swim")
	var meshes: Array[MeshInstance3D] = []
	_collect_meshes(current_asset, meshes)
	var bounds := AABB()
	var first := true
	for mesh in meshes:
		bounds = mesh.get_aabb() if first else bounds.merge(mesh.get_aabb())
		first = false
	current_size = maxf(bounds.size.x, maxf(bounds.size.y, bounds.size.z))
	_update_camera()
	caption.text = "%s  |  %s  |  source scale 1:1\nQ/E species   A/D stage   1/2/3 Glide/Swim/SwimFast   V near/game distance" % [SPECIES[species_index][1].replace("_", " "), stage.replace("_", " ")]
	print("FAF_LIFE_STAGE_PROOF ", current_stem, " size=", current_size, " far=", far_view)


func _update_camera() -> void:
	var distance: float = maxf(1.8, current_size * 3.5) if far_view else maxf(.008, current_size * 2.55)
	camera.position = Vector3(distance, current_size * .20, 0.0)
	camera.look_at(Vector3.ZERO, Vector3.UP)


func _collect_meshes(node: Node, meshes: Array[MeshInstance3D]) -> void:
	if node is MeshInstance3D:
		meshes.append(node as MeshInstance3D)
	for child in node.get_children():
		_collect_meshes(child, meshes)


func _unhandled_key_input(event: InputEvent) -> void:
	if not event is InputEventKey or not event.pressed or event.echo:
		return
	match event.keycode:
		KEY_Q:
			species_index = posmod(species_index - 1, SPECIES.size())
			stage_index = 2
			_show_selected()
		KEY_E:
			species_index = (species_index + 1) % SPECIES.size()
			stage_index = 2
			_show_selected()
		KEY_A:
			stage_index = posmod(stage_index - 1, _stages().size())
			_show_selected()
		KEY_D:
			stage_index = (stage_index + 1) % _stages().size()
			_show_selected()
		KEY_V:
			far_view = not far_view
			_update_camera()
		KEY_1, KEY_2, KEY_3:
			if current_player != null:
				var role: String = str({KEY_1: "Glide", KEY_2: "Swim", KEY_3: "SwimFast"}[event.keycode])
				current_player.play(current_stem + "__" + role)


func _argument(prefix: String) -> String:
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with(prefix):
			return argument.trim_prefix(prefix)
	return ""


func _capture_and_quit(path: String) -> void:
	for _frame in 5:
		await get_tree().process_frame
	if current_player != null:
		current_player.seek(current_player.current_animation_length * .25, true)
	await RenderingServer.frame_post_draw
	var image: Image = get_viewport().get_texture().get_image()
	var error: Error = image.save_png(path)
	if error != OK:
		push_error("FAF capture failed: " + str(error))
		get_tree().quit(1)
		return
	print("FAF_LIFE_STAGE_CAPTURE ", current_stem, " ", path)
	get_tree().quit(0)
