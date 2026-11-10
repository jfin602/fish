extends Node3D

# Isolated owner checkpoint: no Fish player, input motor, collision, or P2 habitat.
# The saved scene contains all authored FAF placements for editor inspection.
const VIEWS := {
	"gameplay": [Vector3(0, 2.8, 4.6), Vector3(0, 1.05, -2.8), 75.0],
	"fish_close": [Vector3(0.72, 1.55, 1.80), Vector3(-0.15, 1.10, -0.25), 52.0],
	"arch": [Vector3(0.12, 1.95, 1.70), Vector3(0, 1.40, -4.45), 75.0],
	"west_reef": [Vector3(-6.78, 3.05, 2.36), Vector3(-6.05, 0.75, -3.65), 75.0],
	"east_reef": [Vector3(6.82, 3.05, 2.79), Vector3(6.39, 0.80, -3.70), 75.0],
	"overview": [Vector3(12.8, 8.9, 13.2), Vector3(0.0, 0.75, -2.4), 75.0],
	"surface": [Vector3(0.0, 1.3, -1.0), Vector3(0.0, 7.5, -0.3), 75.0],
}
const VIEW_KEYS := ["gameplay", "fish_close", "arch", "west_reef", "east_reef", "overview", "surface"]

@onready var camera: Camera3D = $ReviewCamera
@onready var atmosphere: Node3D = $UnderwaterAtmosphere
@onready var fish_group: Node3D = $FAFAssets/Fish


func _ready() -> void:
	var animation_count := 0
	for fish in fish_group.get_children():
		var player := fish.find_child("AnimationPlayer", true, false) as AnimationPlayer
		if player == null:
			push_error("FAF style proof: missing animation player in " + fish.name)
			get_tree().quit(1)
			return
		var stem := String(fish.name).substr(3)
		var swim := stem + "__Swim"
		if not player.has_animation(swim):
			push_error("FAF style proof: missing " + swim)
			get_tree().quit(1)
			return
		player.play(swim)
		player.seek(float(animation_count % 5) * 0.11, true)
		animation_count += 1
	print("FAF_STYLE_FISH_ANIMATIONS ", animation_count)
	# Existing Fish Forward+ water, sky, fog, caustic, shaft, mote, and screen stack.
	# The scene's Seabed/Upper nodes are non-rendering anchors, not P2 geometry.
	atmosphere.call("Initialize", self)
	_set_view("gameplay")
	var requested := _argument("--faf-view=")
	if requested != "":
		_set_view(requested)
	var capture_dir := _argument("--faf-capture-dir=")
	if capture_dir != "":
		_capture_views.call_deferred(capture_dir)


func _unhandled_key_input(event: InputEvent) -> void:
	if not event is InputEventKey or not event.pressed or event.echo:
		return
	if event.keycode >= KEY_1 and event.keycode <= KEY_7:
		_set_view(VIEW_KEYS[event.keycode - KEY_1])


func _set_view(name: String) -> void:
	if not VIEWS.has(name):
		push_error("FAF style proof: unknown view " + name)
		return
	camera.global_position = VIEWS[name][0]
	camera.look_at(VIEWS[name][1], Vector3.UP)
	camera.fov = VIEWS[name][2]
	print("FAF_STYLE_VIEW ", name, " camera=", camera.global_position)


func _argument(prefix: String) -> String:
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with(prefix):
			return argument.trim_prefix(prefix)
	return ""


func _capture_views(path: String) -> void:
	var error := DirAccess.make_dir_recursive_absolute(path)
	if error != OK:
		push_error("FAF style proof: cannot create evidence directory " + str(error))
		get_tree().quit(1)
		return
	for name in VIEW_KEYS:
		_set_view(name)
		for _frame in 18:
			await get_tree().process_frame
		await RenderingServer.frame_post_draw
		var image: Image = get_viewport().get_texture().get_image()
		var output := path.path_join(name + ".png")
		error = image.save_png(output)
		if error != OK:
			push_error("FAF style proof: failed image " + output)
			get_tree().quit(1)
			return
		print("FAF_STYLE_CAPTURE ", output, " ", image.get_width(), "x", image.get_height(),
			" draws=", Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME),
			" primitives=", Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME))
	get_tree().quit(0)
