extends Node3D

# Native Forward+ visual fixture. No path from normal play references this scene.
const CREATURES := [
	["001", "creature_001_northern_leopard_frog", "Northern leopard frog", "IdleShore"],
	["002", "creature_002_northern_water_snake", "Northern water snake", "SurfaceSwim"],
	["003", "creature_003_common_snapping_turtle", "Common snapping turtle", "Swim"],
]
const VIEWS := ["side", "top", "three_quarter"]

@onready var camera: Camera3D = $Camera
@onready var world_environment: WorldEnvironment = $WorldEnvironment
var selected := 0
var view := "three_quarter"
var distance_mode := "close"
var action := ""
var capture_path := ""
var creature: Node3D
var player: AnimationPlayer
var label: Label
var elapsed := 0.0


func _ready() -> void:
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--faf-id="):
			for i in CREATURES.size():
				if CREATURES[i][0] == argument.trim_prefix("--faf-id="):
					selected = i
		if argument.begins_with("--faf-view="):
			view = argument.trim_prefix("--faf-view=")
		if argument.begins_with("--faf-distance="):
			distance_mode = argument.trim_prefix("--faf-distance=")
		if argument.begins_with("--faf-action="):
			action = argument.trim_prefix("--faf-action=")
		if argument.begins_with("--faf-capture="):
			capture_path = argument.trim_prefix("--faf-capture=")
	_make_environment()
	_make_bank()
	_make_waterline()
	_make_caption()
	_show_selected()
	if capture_path != "":
		_capture.call_deferred()


func _make_environment() -> void:
	var env := Environment.new()
	env.background_mode = Environment.BG_COLOR
	env.background_color = Color("#26382c")
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color("#9aa88b")
	env.ambient_light_energy = .43
	env.tonemap_mode = Environment.TONE_MAPPER_FILMIC
	env.fog_enabled = true
	env.fog_light_color = Color("#344f40")
	env.fog_density = .038
	world_environment.environment = env


func _bank_height(x: float) -> float:
	return -.58 + .82 * clampf((-x + 1.55) / 3.1, 0, 1)


func _make_bank() -> void:
	var xs := [-3.5, -2.4, -1.6, -.9, .0, 1.3, 3.5]
	var zs := [-5.0, -2.0, 1.4, 5.0]
	var surface := SurfaceTool.new()
	surface.begin(Mesh.PRIMITIVE_TRIANGLES)
	for i in range(xs.size()-1):
		for j in range(zs.size()-1):
			var a := Vector3(xs[i],_bank_height(xs[i]),zs[j])
			var b := Vector3(xs[i+1],_bank_height(xs[i+1]),zs[j])
			var c := Vector3(xs[i+1],_bank_height(xs[i+1]),zs[j+1])
			var d := Vector3(xs[i],_bank_height(xs[i]),zs[j+1])
			for p in [a,b,c,a,c,d]:surface.add_vertex(p)
	surface.generate_normals()
	var bank := MeshInstance3D.new()
	bank.name = "ShallowBankMud"
	bank.mesh = surface.commit()
	var mud := StandardMaterial3D.new()
	mud.albedo_color = Color("#564d36")
	mud.roughness = 1
	bank.material_override = mud
	add_child(bank)
	# A dark submerged base makes the waterline and animal clear at both distances.
	var deep := MeshInstance3D.new()
	deep.name = "PondBottom"
	var plane := PlaneMesh.new();plane.size = Vector2(20,20)
	deep.mesh = plane
	deep.position.y = -.62
	var deep_mud := StandardMaterial3D.new()
	deep_mud.albedo_color = Color("#33392c")
	deep.material_override = deep_mud
	add_child(deep)


func _make_waterline() -> void:
	var surface := MeshInstance3D.new()
	surface.name = "WaterlineReference"
	var plane := PlaneMesh.new();plane.size = Vector2(20,20)
	surface.mesh = plane
	var water := StandardMaterial3D.new()
	water.albedo_color = Color(.16,.32,.27,.23)
	water.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	water.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	water.cull_mode = BaseMaterial3D.CULL_DISABLED
	surface.material_override = water
	surface.position.y = 0
	add_child(surface)


func _make_caption() -> void:
	var canvas := CanvasLayer.new()
	add_child(canvas)
	label = Label.new()
	label.position = Vector2(22,18)
	label.add_theme_font_size_override("font_size",20)
	label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	canvas.add_child(label)


func _show_selected() -> void:
	if creature != null:
		creature.queue_free()
	var stem: String = CREATURES[selected][1]
	var packed := load("res://assets/faf/pond/creatures/"+stem+".glb") as PackedScene
	if packed == null:
		push_error("Missing Pond creature " + stem)
		get_tree().quit(1)
		return
	creature = packed.instantiate() as Node3D
	add_child(creature)
	player = creature.find_child("AnimationPlayer",true,false) as AnimationPlayer
	if action == "" or not player.get_animation_list().has(stem+"__"+action):
		action = CREATURES[selected][3]
	match selected:
		0:
			creature.position = Vector3(-1.75,.34,0) if action in ["IdleShore","Hop",""] else Vector3(.15,-.025,0)
		1:
			creature.position = Vector3(-1.75,.33,0) if action == "ShoreCrawl" else Vector3(1.30,-.28,0) if action in ["UnderwaterSwim","Dive"] else Vector3(.15,-.025,0)
		2:
			creature.position = Vector3(1.15,-.37,0) if action in ["IdleBottom","BottomWalk",""] else Vector3(1.30,-.29,0) if action == "Dive" else Vector3(.15,-.045,0)
	player.play(stem+"__"+action)
	_position_camera()
	_update_caption()
	print("FAF_POND_CREATURE_PROOF ",stem," action=",action," view=",view,
		" distance=",distance_mode," placement=",creature.position,
		" renderer=",ProjectSettings.get_setting("rendering/renderer/rendering_method"))


func _position_camera() -> void:
	var size: float = [.25,.97,.75][selected]
	var dist := 3.8 if distance_mode == "game" else maxf(size*2.5,.58)
	var target := creature.position
	match view:
		"top":
			camera.position = target + Vector3(.001,dist,0)
			camera.look_at(target,Vector3.FORWARD)
		"side":
			camera.position = target + Vector3(dist,dist*.16,0)
			camera.look_at(target,Vector3.UP)
		_:
			camera.position = target + Vector3(dist*.78,dist*.44,dist*.72)
			camera.look_at(target,Vector3.UP)


func _update_caption() -> void:
	label.text = "%s  |  %s  |  %s  |  %s\n1–3 animal  C view  V near/game  J/K action" % [
		CREATURES[selected][2],action,view,distance_mode]


func _unhandled_key_input(event: InputEvent) -> void:
	if not event is InputEventKey or not event.pressed or event.echo:return
	if event.keycode >= KEY_1 and event.keycode <= KEY_3:
		selected = event.keycode - KEY_1
		action = ""
		_show_selected()
	elif event.keycode == KEY_C:
		view = VIEWS[(VIEWS.find(view)+1)%VIEWS.size()]
		_position_camera();_update_caption()
	elif event.keycode == KEY_V:
		distance_mode = "game" if distance_mode == "close" else "close"
		_position_camera();_update_caption()
	elif event.keycode == KEY_J or event.keycode == KEY_K:
		var stem: String = CREATURES[selected][1]
		var names := player.get_animation_list()
		var idx := names.find(stem+"__"+action)
		idx = posmod(idx + (-1 if event.keycode == KEY_J else 1),names.size())
		action = String(names[idx]).trim_prefix(stem+"__")
		player.play(stem+"__"+action)
		_update_caption()


func _capture() -> void:
	for i in range(5):await get_tree().process_frame
	var clip := player.get_animation(player.current_animation)
	player.seek(clip.length * (.70 if action in ["Hop","Dive"] else .38),true)
	player.advance(0)
	await RenderingServer.frame_post_draw
	var err := get_viewport().get_texture().get_image().save_png(capture_path)
	if err != OK:
		push_error("Pond creature screenshot failed "+str(err))
		get_tree().quit(1)
		return
	print("FAF_POND_CREATURE_CAPTURE ",capture_path)
	get_tree().quit(0)
