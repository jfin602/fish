extends Node3D

# Isolated Pond Forward+ review fixture. No normal-play or World Select wiring.
const SPECIES := [
	["006", "fish_006_bluegill"],
	["007", "fish_007_largemouth_bass"],
	["008", "fish_008_channel_catfish"],
	["009", "fish_009_common_carp"],
	["010", "fish_010_northern_pike"],
]
const STAGES := ["egg", "larva", "juvenile", "subadult", "adult"]
const SWIM_ROLES := ["Idle", "Glide", "SwimSlow", "Swim", "SwimFast", "TurnLeft", "TurnRight"]
const EGG_ROLES := ["Idle", "Hatch"]
const VIEWS := ["side", "top", "three_quarter", "front"]

@onready var camera: Camera3D = $Camera
@onready var world_environment: WorldEnvironment = $WorldEnvironment
var species_index := 0
var stage_index := 4
var view_index := 0
var game_distance := false
var action_role := "Swim"
var current_asset: Node3D
var current_player: AnimationPlayer
var current_stem := ""
var current_size := 0.4
var caption: Label


func _ready() -> void:
	var environment := Environment.new()
	environment.background_mode = Environment.BG_COLOR
	environment.background_color = Color("#1B302B")
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	environment.ambient_light_color = Color("#D4D5AD")
	environment.ambient_light_energy = 0.8
	environment.fog_enabled = true
	environment.fog_light_color = Color("#29473E")
	environment.fog_density = 0.012
	world_environment.environment = environment
	var canvas := CanvasLayer.new()
	add_child(canvas)
	caption = Label.new()
	caption.position = Vector2(20, 18)
	caption.add_theme_font_size_override("font_size", 19)
	caption.mouse_filter = Control.MOUSE_FILTER_IGNORE
	canvas.add_child(caption)
	var requested_id := _argument("--faf-id=")
	for index in SPECIES.size():
		if SPECIES[index][0] == requested_id:
			species_index = index
	var requested_stage := _argument("--faf-stage=")
	if STAGES.has(requested_stage):
		stage_index = STAGES.find(requested_stage)
	var requested_view := _argument("--faf-view=")
	if VIEWS.has(requested_view):
		view_index = VIEWS.find(requested_view)
	game_distance = _argument("--faf-distance=") == "game"
	var requested_action := _argument("--faf-action=")
	if requested_action != "":
		action_role = requested_action
	_show_selected()
	var capture := _argument("--faf-capture=")
	if capture != "":
		_capture_and_quit.call_deferred(capture)


func _roles() -> Array:
	return EGG_ROLES if STAGES[stage_index] == "egg" else SWIM_ROLES


func _show_selected() -> void:
	if current_asset != null:
		current_asset.queue_free()
		current_asset = null
	current_player = null
	var stage: String = STAGES[stage_index]
	current_stem = SPECIES[species_index][1] + "__" + stage
	var packed: PackedScene = load("res://assets/faf/pond/fish/" + current_stem + ".glb") as PackedScene
	if packed == null:
		push_error("Missing Pond asset " + current_stem)
		get_tree().quit(1)
		return
	current_asset = packed.instantiate() as Node3D
	add_child(current_asset)
	current_asset.rotation.y = PI
	current_player = current_asset.find_child("AnimationPlayer", true, false) as AnimationPlayer
	if not _roles().has(action_role):
		action_role = "Idle" if stage == "egg" else "Swim"
	if current_player != null:
		current_player.play(current_stem + "__" + action_role)
	var meshes: Array[MeshInstance3D] = []
	_collect_meshes(current_asset, meshes)
	var bounds := AABB()
	var first := true
	for mesh in meshes:
		bounds = mesh.get_aabb() if first else bounds.merge(mesh.get_aabb())
		first = false
	current_size = maxf(bounds.size.x, maxf(bounds.size.y, bounds.size.z))
	_update_camera()
	_update_caption()
	print("FAF_POND_PROOF ", current_stem, " action=", action_role,
	      " view=", VIEWS[view_index], " game_distance=", game_distance,
	      " source_size=", current_size)


func _update_camera() -> void:
	# The current Fish third-person tuning uses 3.8 m. Pond growth camera
	# framing is not authored yet, so this is an honest fixed-distance check.
	var distance: float = 3.8 if game_distance else maxf(.008, current_size * 2.8)
	match VIEWS[view_index]:
		"top":
			camera.position = Vector3(0.001, distance, 0)
			camera.look_at(Vector3.ZERO, Vector3.FORWARD)
		"three_quarter":
			camera.position = Vector3(distance, distance * .38, -distance * .65)
			camera.look_at(Vector3.ZERO, Vector3.UP)
		"front":
			camera.position = Vector3(0, distance * .12, distance)
			camera.look_at(Vector3.ZERO, Vector3.UP)
		_:
			camera.position = Vector3(distance, distance * .16, 0)
			camera.look_at(Vector3.ZERO, Vector3.UP)


func _update_caption() -> void:
	caption.text = "%s  |  %s  |  %s  |  %s\nQ/E species   A/D stage   1–7 action   C side/top/three-quarter/front   V near/game" % [
		SPECIES[species_index][1].replace("_", " "), STAGES[stage_index], action_role,
		VIEWS[view_index] + (" game distance" if game_distance else " inspection")]


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
			_show_selected()
		KEY_E:
			species_index = (species_index + 1) % SPECIES.size()
			_show_selected()
		KEY_A:
			stage_index = posmod(stage_index - 1, STAGES.size())
			_show_selected()
		KEY_D:
			stage_index = (stage_index + 1) % STAGES.size()
			_show_selected()
		KEY_C:
			view_index = (view_index + 1) % VIEWS.size()
			_update_camera()
			_update_caption()
		KEY_V:
			game_distance = not game_distance
			_update_camera()
			_update_caption()
		KEY_1, KEY_2, KEY_3, KEY_4, KEY_5, KEY_6, KEY_7:
			var index: int = event.keycode - KEY_1
			var roles: Array = _roles()
			if index < roles.size() and current_player != null:
				action_role = str(roles[index])
				current_player.play(current_stem + "__" + action_role)
				_update_caption()


func _argument(prefix: String) -> String:
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with(prefix):
			return argument.trim_prefix(prefix)
	return ""


func _capture_and_quit(path: String) -> void:
	for _frame in 5:
		await get_tree().process_frame
	if current_player != null:
		var phase := .82 if action_role == "Hatch" else .5 if action_role.begins_with("Turn") else .25
		current_player.seek(current_player.current_animation_length * phase, true)
	await RenderingServer.frame_post_draw
	var image: Image = get_viewport().get_texture().get_image()
	var error: Error = image.save_png(path)
	if error != OK:
		push_error("Pond capture failed: " + str(error))
		get_tree().quit(1)
		return
	print("FAF_POND_CAPTURE ", current_stem, " ", path)
	get_tree().quit(0)
