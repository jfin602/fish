extends Node3D

# Isolated ambient insect review. No collision or normal-play scene references.
const ROOT := "res://assets/faf/pond/insects/"
const STEMS := ["insect_001a_green_darner_nymph","insect_001b_green_darner_nymph",
	"insect_002a_backswimmer","insect_002b_backswimmer"]
@onready var camera: Camera3D = $Camera
@onready var environment_node: WorldEnvironment = $WorldEnvironment
var selected := 0
var view := "three_quarter"
var distance_mode := "close"
var density := "sparse"
var capture_path := ""
var phase := .35
var instances: Array[Node3D] = []
var players: Array[AnimationPlayer] = []
var expected_triangles := 0
var label: Label

func _ready() -> void:
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--faf-id="):selected = clampi(int(arg.trim_prefix("--faf-id=")),0,3)
		if arg.begins_with("--faf-view="):view = arg.trim_prefix("--faf-view=")
		if arg.begins_with("--faf-distance="):distance_mode = arg.trim_prefix("--faf-distance=")
		if arg.begins_with("--faf-density="):density = arg.trim_prefix("--faf-density=")
		if arg.begins_with("--faf-capture="):capture_path = arg.trim_prefix("--faf-capture=")
		if arg.begins_with("--faf-phase="):phase = clampf(float(arg.trim_prefix("--faf-phase=")),0,1)
	_make_environment()
	_make_bank()
	_make_waterline()
	_make_context()
	_place_insects()
	_position_camera()
	_caption()
	print("FAF_INSECT_PROOF view=",view," distance=",distance_mode," density=",density,
		" instances=",instances.size()," expected_triangles=",expected_triangles,
		" renderer=",ProjectSettings.get_setting("rendering/renderer/rendering_method"))
	if capture_path != "":_capture.call_deferred()

func _make_environment() -> void:
	var env := Environment.new()
	env.background_mode = Environment.BG_COLOR
	env.background_color = Color("#273227")
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color("#a7a475")
	env.ambient_light_energy = .43
	env.tonemap_mode = Environment.TONE_MAPPER_FILMIC
	env.fog_enabled = true
	env.fog_light_color = Color("#374b36")
	env.fog_density = .035
	environment_node.environment = env

func _floor_height(x: float) -> float:
	return -.39 + .72*smoothstep(.9,2.8,x)

func _make_bank() -> void:
	var st := SurfaceTool.new();st.begin(Mesh.PRIMITIVE_TRIANGLES)
	for zi in range(14):
		for xi in range(20):
			var x: float = -2.6 + xi*.28
			var z: float = -1.9 + zi*.28
			var a := Vector3(x,_floor_height(x),z)
			var b := Vector3(x+.28,_floor_height(x+.28),z)
			var c := Vector3(x+.28,_floor_height(x+.28),z+.28)
			var d := Vector3(x,_floor_height(x),z+.28)
			for p in [a,d,c,a,c,b]:st.add_vertex(p)
	st.generate_normals()
	var ground := MeshInstance3D.new();ground.name="SingleShallowBank";ground.mesh=st.commit()
	var mud := StandardMaterial3D.new();mud.albedo_color=Color("#61583d");mud.roughness=1
	ground.material_override=mud;add_child(ground)

func _make_waterline() -> void:
	var surface := MeshInstance3D.new();surface.name="WaterlineReference"
	var plane := PlaneMesh.new();plane.size=Vector2(7,5);surface.mesh=plane
	var water := StandardMaterial3D.new();water.albedo_color=Color(.18,.32,.25,.12)
	water.transparency=BaseMaterial3D.TRANSPARENCY_ALPHA
	water.cull_mode=BaseMaterial3D.CULL_DISABLED
	water.shading_mode=BaseMaterial3D.SHADING_MODE_UNSHADED
	surface.material_override=water;surface.position.y=0;add_child(surface)

func _make_context() -> void:
	for info in [
		["res://assets/faf/pond/plants/plant_004a_lily_pad.glb",Vector3(-1.15,0,-.85),.8],
		["res://assets/faf/pond/plants/plant_007a_reed.glb",Vector3(1.50,_floor_height(1.50),-.85),.8],
		["res://assets/faf/pond/plants/plant_008a_submerged_algae.glb",Vector3(-1.4,-.38,.75),.8],
		["res://assets/faf/pond/wood/wood_001c_broken_fallen_log.glb",Vector3(-1.75,-.39,-1.3),.65]]:
		var packed := load(info[0]) as PackedScene
		if packed == null:continue
		var context := packed.instantiate() as Node3D
		context.name="ExistingPondContext";add_child(context)
		context.position=info[1];context.scale=Vector3.ONE*info[2]

func _insect_position(index: int, copy: int) -> Vector3:
	var x: float = -.20 + (index%2)*.34
	var z: float = (index/2)*.48 - .20
	if copy>0:
		x += .34*sin(copy*1.91+index*.72)
		z += .25*copy + .11*sin(copy*2.77+index)
	return Vector3(x,-.369 if index<2 else -.014,z)

func _place_insects() -> void:
	var copies := 1 if density=="sparse" else 5
	for index in range(4):
		var packed := load(ROOT+STEMS[index]+".glb") as PackedScene
		if packed==null:push_error("Missing insect GLB "+STEMS[index]);get_tree().quit(1);return
		for copy in range(copies):
			var model := packed.instantiate() as Node3D
			model.name="AmbientInsect_%d_%d" % [index,copy]
			add_child(model);model.position=_insect_position(index,copy)
			model.rotation.y=(copy%3-1)*.24
			if index>=2:
				model.rotation.z=PI
				model.rotation.x=-.25
			var player := model.find_child("AnimationPlayer",true,false) as AnimationPlayer
			if player == null:push_error("Missing insect animation player");get_tree().quit(1);return
			player.play(STEMS[index]+("__Crawl" if index<2 else "__Row"))
			instances.append(model);players.append(player)
			expected_triangles+=468 if index<2 else 396

func _selected_target() -> Vector3:
	return _insect_position(selected,0)

func _position_camera() -> void:
	var target := _selected_target()
	if density=="dense" and distance_mode=="game":target=Vector3(0,-.10,.18)
	camera.fov = 68.0 if distance_mode=="game" else 38.0
	var dist := 3.8 if distance_mode=="game" else .16 if selected<2 else .075
	match view:
		"above":camera.position=target+Vector3(.001,dist,dist*.30);camera.look_at(target,Vector3.FORWARD)
		"waterline":camera.position=target+Vector3((-1 if distance_mode=="game" else 1)*dist*.78,0.03-target.y,dist*.62);camera.look_at(target,Vector3.UP)
		"underwater":camera.position=target+Vector3((-1 if distance_mode=="game" else 1)*dist*.72,-.13-target.y,dist*.70);camera.look_at(target,Vector3.UP)
		_:camera.position=target+Vector3(dist*.72,dist*.45,dist*.70);camera.look_at(target,Vector3.UP)

func _caption() -> void:
	if label == null:
		var canvas := CanvasLayer.new();add_child(canvas)
		label = Label.new();canvas.add_child(label)
		label.position=Vector2(20,17);label.add_theme_font_size_override("font_size",19)
		label.mouse_filter=Control.MOUSE_FILTER_IGNORE
	label.text="Pond FAF insects  |  %s  |  %s  |  %s  |  %s\n1–4 asset  C view  V close/game  D sparse/dense" % [STEMS[selected],view,distance_mode,density]

func _unhandled_key_input(event: InputEvent) -> void:
	if not event is InputEventKey or not event.pressed or event.echo:return
	if event.keycode>=KEY_1 and event.keycode<=KEY_4:selected=event.keycode-KEY_1
	elif event.keycode==KEY_C:
		var views:=["three_quarter","above","waterline","underwater"]
		view=views[(views.find(view)+1)%views.size()]
	elif event.keycode==KEY_V:distance_mode="game" if distance_mode=="close" else "close"
	elif event.keycode==KEY_D:density="dense" if density=="sparse" else "sparse";_clear_insects();_place_insects()
	_position_camera();_caption()

func _clear_insects() -> void:
	for item in instances:item.queue_free()
	instances.clear();players.clear();expected_triangles=0

func _capture() -> void:
	for i in range(8):await get_tree().process_frame
	for player in players:
		var clip:=player.get_animation(player.current_animation)
		player.seek(clip.length*phase,true);player.advance(0)
	await RenderingServer.frame_post_draw
	print("FAF_INSECT_RENDER draw_calls=",Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME),
		" objects=",Performance.get_monitor(Performance.RENDER_TOTAL_OBJECTS_IN_FRAME),
		" primitives=",Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME))
	var err:=get_viewport().get_texture().get_image().save_png(capture_path)
	print("FAF_INSECT_CAPTURE ",capture_path," result=",err)
	get_tree().quit(0 if err==OK else 1)
