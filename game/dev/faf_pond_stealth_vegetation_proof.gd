extends Node3D

# Standalone Forward+ cover composition. This file is never instanced by Pond play.
const MANIFEST := "res://../assets/faf/pond/plants/pond_stealth_vegetation_001_manifest.json"
const WOOD := "res://scenes/faf_pond_wood/wood_004c_tangled_roots.tscn"
const WATER_Y := 2.9

@onready var camera: Camera3D = $ReviewCamera
@onready var environment_node: WorldEnvironment = $WorldEnvironment
var entries: Array = []
var view := "overview"
var capture_dir := ""
var check_only := false
var do_traverse := false
var counts := {"instances":0,"triangles":0,"materials":0}
var fish: CharacterBody3D


func _ready() -> void:
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--faf-view="): view = arg.trim_prefix("--faf-view=")
		if arg.begins_with("--faf-capture-dir="): capture_dir = arg.trim_prefix("--faf-capture-dir=")
		if arg == "--faf-check": check_only = true
		if arg == "--faf-traverse": do_traverse = true
	var file := FileAccess.open(ProjectSettings.globalize_path(MANIFEST),FileAccess.READ)
	if file == null:
		push_error("Pond cover manifest unavailable")
		get_tree().quit(1)
		return
	var manifest: Dictionary = JSON.parse_string(file.get_as_text())
	entries = manifest.get("assets",[])
	if entries.size() != 13:
		push_error("Expected thirteen vegetation variants")
		get_tree().quit(1)
		return
	_make_environment()
	_make_basin()
	_make_waterline()
	_make_wood()
	_layout.call_deferred()


func _make_environment() -> void:
	var env := Environment.new()
	env.background_mode = Environment.BG_COLOR
	env.background_color = Color("#25352b")
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color("#a1ae83")
	env.ambient_light_energy = .43
	env.tonemap_mode = Environment.TONE_MAPPER_FILMIC
	env.fog_enabled = true
	env.fog_light_color = Color("#3b563c")
	env.fog_density = .011
	environment_node.environment = env


func _height(x: float,z: float) -> float:
	# Left is a 3.4–3.6 m water column, right bank is about 1.2 m deep.
	return -.60 + 2.32*smoothstep(-17.0,16.0,x) + .045*sin(x*.27+z*.13)


func _make_basin() -> void:
	var tool := SurfaceTool.new()
	tool.begin(Mesh.PRIMITIVE_TRIANGLES)
	for zi in range(24):
		for xi in range(24):
			var x: float = -22.0 + xi*2.0
			var z: float = -22.0 + zi*2.0
			var a := Vector3(x,_height(x,z),z)
			var b := Vector3(x+2,_height(x+2,z),z)
			var c := Vector3(x+2,_height(x+2,z+2),z+2)
			var d := Vector3(x,_height(x,z+2),z+2)
			for point in [a,d,c,a,c,b]: tool.add_vertex(point)
	tool.generate_normals()
	var mesh := tool.commit()
	var floor := MeshInstance3D.new()
	floor.name = "GeneratedMudBasin"
	floor.mesh = mesh
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color("#514c34")
	mat.roughness = 1
	mat.cull_mode = BaseMaterial3D.CULL_DISABLED
	floor.material_override = mat
	add_child(floor)
	var body := StaticBody3D.new()
	body.name = "TerrainOnlyCollision"
	body.collision_layer = 1
	body.collision_mask = 0
	add_child(body)
	var shape := CollisionShape3D.new()
	shape.shape = mesh.create_trimesh_shape()
	body.add_child(shape)


func _make_waterline() -> void:
	var water := MeshInstance3D.new()
	water.name = "SceneOnlyWaterSurface"
	var plane := PlaneMesh.new()
	plane.size = Vector2(46,46)
	water.mesh = plane
	water.position.y = WATER_Y
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color(.18,.44,.40,.20)
	mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	mat.cull_mode = BaseMaterial3D.CULL_DISABLED
	water.material_override = mat
	add_child(water)


func _make_wood() -> void:
	var packed := load(WOOD) as PackedScene
	if packed == null:
		push_error("Existing Pond wood context unavailable")
		return
	var wood := packed.instantiate() as Node3D
	wood.name = "ExistingWoodContext"
	add_child(wood)
	wood.position = Vector3(9,_height(9,-15)-.08,-15)
	wood.rotation.y = -.26


func _place(index: int,x: float,z: float,yaw: float,scale_value: float) -> void:
	var entry: Dictionary = entries[index]
	var path := "res://" + String(entry["glb"]).trim_prefix("game/")
	var packed := load(path) as PackedScene
	assert(packed != null,path)
	var plant := packed.instantiate() as Node3D
	plant.name = "%s_%d" % [entry["faf_id"],counts["instances"]]
	add_child(plant)
	plant.position = Vector3(x,_height(x,z)-.015,z)
	plant.rotation.y = yaw
	plant.scale = Vector3.ONE*scale_value
	counts["instances"] += 1
	counts["triangles"] += int(entry["triangles"])
	counts["materials"] += int(entry["materials"])


func _layout() -> void:
	# Complete gallery, with enough space to judge every unique structural form.
	for i in entries.size():
		_place(i,-18.0+3.0*i,13.0,.11*i,1.0)
	# Bottom-rooted grass bed: 010C tops at about the y=2.9 waterline.
	for item in [[2,-16,-8,0,.96],[2,-14,-10,.24,1.01],[2,-12,-7,-.20,.94],
		[1,-10,-11,.42,1.02],[1,-9,-7,-.29,.92],[0,13,-8,.18,1.05],
		[0,15,-10,-.24,.91],[1,-2,-12,.29,.97]]:
		_place(item[0],item[1],item[2],item[3],item[4])
	# Weed/coontail soft cover overlaps the fish lane but contributes no collision.
	for item in [[3,-5,-7,.2,1],[4,-4,-9,-.27,.95],[5,-1,-12,.31,.9],
		[6,-4,-11,-.35,1.02],[7,-2,-8,.28,.96],[8,-1,-6,-.20,.89]]:
		_place(item[0],item[1],item[2],item[3],item[4])
	# Open broad leaves at the shallow transition; structural thicket beside wood.
	for item in [[9,11,-8,.18,1],[9,14,-12,-.27,.91],[10,5,-9,.22,.98],
		[11,7,-13,-.30,.95],[12,9,-10,.27,.94]]:
		_place(item[0],item[1],item[2],item[3],item[4])
	_set_view()
	var renderer: String = ProjectSettings.get_setting("rendering/renderer/rendering_method")
	print("POND_COVER_PROOF_READY assets=13 instances=",counts["instances"],
		" instanced_triangles=",counts["triangles"]," material_slots=",counts["materials"],
		" renderer=",renderer," grass_tall_water_y=",_height(-16,-8)+3.57745)
	if check_only:
		get_tree().quit(0 if renderer == "forward_plus" else 1)
	elif do_traverse:
		_traverse.call_deferred()
	elif capture_dir != "":
		_capture.call_deferred()


func _set_view() -> void:
	match view:
		"grass_short": camera.position=Vector3(14,2.1,-5); camera.look_at(Vector3(14,1.5,-9))
		"grass_medium": camera.position=Vector3(-7,1.7,-5); camera.look_at(Vector3(-9,.7,-9))
		"grass_tall": camera.position=Vector3(-18,1.2,-5); camera.look_at(Vector3(-15,1.0,-9))
		"grass_surface": camera.position=Vector3(-18,3.07,-4); camera.look_at(Vector3(-15,2.95,-8))
		"waterweed": camera.position=Vector3(-7,1.4,-5); camera.look_at(Vector3(-4,.7,-9))
		"coontail": camera.position=Vector3(-4,1.1,-5); camera.look_at(Vector3(-2,.3,-9))
		"pondweed": camera.position=Vector3(14,2.0,-5); camera.look_at(Vector3(11,1.5,-9))
		"thicket": camera.position=Vector3(5,2.0,-6); camera.look_at(Vector3(8,1.0,-12))
		"mixed": camera.position=Vector3(-7,1.7,-5); camera.look_at(Vector3(-2,1.0,-9))
		"gallery": camera.position=Vector3(0,14,28); camera.look_at(Vector3(0,.6,13))
		"fish":
			_make_fish(Vector3(-3,.65,3))
			(fish.get_node("CameraRig/Camera3D") as Camera3D).current = true
		_: camera.position=Vector3(-2,16,22); camera.look_at(Vector3(-2,.5,-3))


func _make_fish(start: Vector3) -> void:
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	fish = main.get_node("Fish").duplicate() as CharacterBody3D
	main.free()
	fish.name = "RealFishController"
	add_child(fish)
	fish.position = start


func _key(code: Key,down: bool) -> void:
	var event := InputEventKey.new()
	event.physical_keycode = code
	event.pressed = down
	Input.parse_input_event(event)


func _traverse() -> void:
	if fish == null: _make_fish(Vector3(-3,.65,7))
	(fish.get_node("CameraRig/Camera3D") as Camera3D).current = true
	for i in range(5): await get_tree().physics_frame
	var start_z := fish.global_position.z
	var min_camera_distance := 100.0
	var min_y := 100.0
	var camera_hits := 0
	var entered := false
	_key(KEY_W,true)
	for i in range(360):
		await get_tree().physics_frame
		entered = entered or fish.global_position.z < -6.0
		var rig := fish.get_node("CameraRig") as Node3D
		min_camera_distance = minf(min_camera_distance,rig.global_position.distance_to(fish.global_position))
		min_y = minf(min_y,fish.global_position.y)
		if i%12 == 0 and _camera_blocked(rig.global_position): camera_hits += 1
		if capture_dir != "" and entered and i%90 == 0:
			await _capture("pond_cover_traverse.png",false)
		if fish.global_position.z < -13: break
	_key(KEY_W,false)
	var passed := entered and fish.global_position.z < -11 and camera_hits == 0 and min_camera_distance > .7
	print("POND_COVER_TRAVERSAL passed=",passed," start_z=",start_z,
		" end_z=",fish.global_position.z," min_y=",min_y,
		" camera_hits=",camera_hits," min_camera_distance=",min_camera_distance)
	get_tree().quit(0 if passed else 1)


func _camera_blocked(at: Vector3) -> bool:
	var sphere := SphereShape3D.new()
	sphere.radius = .22
	var query := PhysicsShapeQueryParameters3D.new()
	query.shape = sphere
	query.transform = Transform3D(Basis.IDENTITY,at)
	query.collision_mask = 1
	query.exclude = [fish.get_rid()]
	return not get_world_3d().direct_space_state.intersect_shape(query,1).is_empty()


func _capture(filename: String = "",finish: bool = true) -> void:
	for i in range(8): await get_tree().process_frame
	await RenderingServer.frame_post_draw
	var path := capture_dir.path_join(filename if filename != "" else "pond_cover_%s.png" % view)
	DirAccess.make_dir_recursive_absolute(capture_dir)
	var result := get_viewport().get_texture().get_image().save_png(path)
	print("POND_COVER_CAPTURE path=",path," result=",result,
		" draw_calls=",Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME),
		" objects=",Performance.get_monitor(Performance.RENDER_TOTAL_OBJECTS_IN_FRAME),
		" primitives=",Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME))
	if finish: get_tree().quit(0 if result == OK else 1)
