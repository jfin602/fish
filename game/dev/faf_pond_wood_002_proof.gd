extends Node3D

const MANIFEST := "res://../assets/faf/pond/wood/pond_wood_002_manifest.json"
@onready var camera: Camera3D=$ReviewCamera
@onready var environment_node: WorldEnvironment=$WorldEnvironment
var entries: Array=[]
var view := "overview"
var capture_dir := ""
var check_only := false
var traverse := false
var route_filter := ""
var route_runs := 0
var costs := {"instances":0,"triangles":0,"shapes":0}
var fish: CharacterBody3D
var test_piece: Node3D


func _ready() -> void:
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--faf-view="): view=arg.trim_prefix("--faf-view=")
		if arg.begins_with("--faf-capture-dir="): capture_dir=arg.trim_prefix("--faf-capture-dir=")
		if arg=="--faf-check": check_only=true
		if arg=="--faf-traverse": traverse=true
		if arg.begins_with("--faf-route="): route_filter=arg.trim_prefix("--faf-route=")
	if traverse and route_filter=="":
		push_error("Pass exactly one --faf-route per process")
		get_tree().quit(1)
		return
	var file := FileAccess.open(ProjectSettings.globalize_path(MANIFEST),FileAccess.READ)
	if file==null: push_error("wood-002 manifest missing");get_tree().quit(1);return
	entries=JSON.parse_string(file.get_as_text())["assets"]
	if entries.size()!=10: push_error("wood-002 inventory != 10");get_tree().quit(1);return
	_make_world()
	_layout.call_deferred()


func _make_world() -> void:
	var env := Environment.new()
	env.background_mode=Environment.BG_COLOR
	env.background_color=Color("#24392c")
	env.ambient_light_source=Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color=Color("#a9af84")
	env.ambient_light_energy=.52
	env.tonemap_mode=Environment.TONE_MAPPER_FILMIC
	env.fog_enabled=true
	env.fog_light_color=Color("#36513a")
	env.fog_density=.007
	environment_node.environment=env
	var mud := StandardMaterial3D.new()
	mud.albedo_color=Color("#554a32")
	mud.roughness=1
	var floor := MeshInstance3D.new()
	floor.name="FlatMud"
	var plane := PlaneMesh.new();plane.size=Vector2(110,110)
	floor.mesh=plane
	floor.material_override=mud
	floor.position.y=-.07
	add_child(floor)
	var body := StaticBody3D.new()
	body.name="MudCollision"
	body.collision_layer=1
	body.collision_mask=0
	add_child(body)
	var shape := CollisionShape3D.new()
	shape.shape=BoxShape3D.new();shape.shape.size=Vector3(110,.3,110)
	shape.position.y=-.23
	body.add_child(shape)
	var water := MeshInstance3D.new()
	water.name="SceneOnlyWater"
	var water_mesh := PlaneMesh.new();water_mesh.size=Vector2(110,110)
	water.mesh=water_mesh
	water.position.y=7.2
	var water_mat := StandardMaterial3D.new()
	water_mat.albedo_color=Color(.22,.42,.34,.12)
	water_mat.transparency=BaseMaterial3D.TRANSPARENCY_ALPHA
	water_mat.shading_mode=BaseMaterial3D.SHADING_MODE_UNSHADED
	water_mat.cull_mode=BaseMaterial3D.CULL_DISABLED
	water.material_override=water_mat
	add_child(water)
	_terrain_patch("MildBankSample",Vector2(45,10),mud,false)
	_terrain_patch("IrregularMudSample",Vector2(-45,10),mud,true)


func _patch_height(x: float,z: float,irregular: bool) -> float:
	var center_x := -45.0 if irregular else 45.0
	var fade := (1.0-smoothstep(4.0,6.4,absf(x-center_x))) * (1.0-smoothstep(8.0,12.0,absf(z-10.0)))
	var raised := 0.0
	if irregular:
		raised=.28+.16*sin(x*.62+z*.31)+.11*cos(z*.51-x*.22)
	else:
		raised=.26+.105*(x-40.0)+.025*sin(z*.30)
	return lerpf(-.07,raised,fade)


func _terrain_patch(name_value: String,center: Vector2,mud: StandardMaterial3D,irregular: bool) -> void:
	var tool := SurfaceTool.new()
	tool.begin(Mesh.PRIMITIVE_TRIANGLES)
	for zi in range(12):
		for xi in range(8):
			var x := center.x-6.4+float(xi)*1.6
			var z := center.y-12.0+float(zi)*2.0
			var a := Vector3(x,_patch_height(x,z,irregular),z)
			var b := Vector3(x+1.6,_patch_height(x+1.6,z,irregular),z)
			var c := Vector3(x+1.6,_patch_height(x+1.6,z+2.0,irregular),z+2.0)
			var d := Vector3(x,_patch_height(x,z+2.0,irregular),z+2.0)
			for point in [a,d,c,a,c,b]: tool.add_vertex(point)
	tool.generate_normals()
	var mesh := tool.commit()
	var patch := MeshInstance3D.new()
	patch.name=name_value
	patch.mesh=mesh
	var patch_mat := mud.duplicate() as StandardMaterial3D
	patch_mat.cull_mode=BaseMaterial3D.CULL_DISABLED
	patch.material_override=patch_mat
	add_child(patch)
	var body := StaticBody3D.new()
	body.name=name_value+"Collision"
	body.collision_layer=1
	body.collision_mask=0
	add_child(body)
	var shape := CollisionShape3D.new()
	shape.shape=mesh.create_trimesh_shape()
	body.add_child(shape)


func _place(entry: Dictionary,at: Vector3,yaw: float=0,scale_value: float=1) -> Node3D:
	var path := "res://scenes/faf_pond_wood_002/%s.tscn" % entry["stem"]
	var scene := load(path) as PackedScene
	assert(scene!=null,path)
	var piece := scene.instantiate() as Node3D
	add_child(piece)
	piece.position=at
	piece.rotation.y=yaw
	piece.scale=Vector3.ONE*scale_value
	costs["instances"]+=1
	costs["triangles"]+=int(entry["triangle_count"])
	costs["shapes"]+=int(entry["collision_count"])
	return piece


func _layout() -> void:
	for i in entries.size():
		var entry: Dictionary=entries[i]
		var x := -34.0+17.0*float(i/2)
		var z := 20.0 if i%2==0 else -20.0
		var piece := _place(entry,Vector3(x,0,z),.11*float(i%3),1.0)
		piece.name="Gallery_%s" % entry["faf_id"]
	# Two deliberately sparse encounter clusters preserve broad swim lanes.
	for item in [[1,-25,38,.19,.96],[2,-11,37,-.22,1.04],
		[5,10,38,.24,.93],[8,26,37,-.20,1.06]]:
		_place(entries[item[0]],Vector3(item[1],0,item[2]),item[3],item[4])
	_place(entries[1],Vector3(45,_patch_height(45,10,false)-.04,10),.08,.97)
	_place(entries[5],Vector3(-45,_patch_height(-45,10,true)-.08,10),-.13,1.0)
	for old_item in [["wood_001b_forked_fallen_log",-29.0,-39.0],
		["wood_002a_hero_hollow_trunk",31.0,-39.0]]:
		var old_path := "res://scenes/faf_pond_wood/%s.tscn" % old_item[0]
		var old_scene := load(old_path) as PackedScene
		assert(old_scene!=null,old_path)
		var context := old_scene.instantiate() as Node3D
		context.name="Existing_%s" % old_item[0]
		context.position=Vector3(old_item[1],0,old_item[2])
		add_child(context)
	_set_view()
	var renderer: String=ProjectSettings.get_setting("rendering/renderer/rendering_method")
	print("POND_WOOD_002_PROOF_READY assets=10 instances=",costs["instances"],
		" instanced_triangles=",costs["triangles"]," collision_shapes=",costs["shapes"],
		" renderer=",renderer)
	if check_only: get_tree().quit(0 if renderer=="forward_plus" else 1)
	elif traverse: _run_routes.call_deferred()
	elif capture_dir!="": _capture.call_deferred("pond_wood_002_%s.png" % view)


func _set_view() -> void:
	match view:
		"long": camera.position=Vector3(-30,7,37);camera.look_at(Vector3(-34,1.3,20))
		"fork": camera.position=Vector3(-13,8,37);camera.look_at(Vector3(-17,1.4,20))
		"root": camera.position=Vector3(5,8,37);camera.look_at(Vector3(0,1.4,20))
		"split": camera.position=Vector3(21,6,34);camera.look_at(Vector3(17,1.7,20))
		"crown": camera.position=Vector3(36,8,37);camera.look_at(Vector3(34,1.9,20))
		"end": camera.position=Vector3(17,2.0,12);camera.look_at(Vector3(17,1.9,20))
		"top": camera.position=Vector3(0,43,29);camera.look_at(Vector3(0,0,5))
		"bank": camera.position=Vector3(52,6,27);camera.look_at(Vector3(45,1.2,10))
		"irregular": camera.position=Vector3(-52,7,27);camera.look_at(Vector3(-45,1.2,10))
		"comparison": camera.position=Vector3(26,13,-5);camera.look_at(Vector3(25,1.5,-27))
		_: camera.position=Vector3(0,31,56);camera.look_at(Vector3(0,1,6))


func _make_fish(start: Vector3,yaw: float) -> void:
	var main: Node3D=load("res://scenes/main.tscn").instantiate()
	fish=main.get_node("Fish").duplicate() as CharacterBody3D
	main.free()
	fish.name="RealFishController"
	add_child(fish)
	fish.position=start
	fish.call("SetInputMode","keyboard")
	(fish.get_node("CameraRig/Camera3D") as Camera3D).current=true


func _key(code: Key,down: bool) -> void:
	var event := InputEventKey.new()
	event.physical_keycode=code
	event.pressed=down
	Input.parse_input_event(event)


func _probe_camera(at: Vector3) -> bool:
	var sphere := SphereShape3D.new();sphere.radius=.22
	var q := PhysicsShapeQueryParameters3D.new()
	q.shape=sphere
	q.transform=Transform3D(Basis.IDENTITY,at)
	q.collision_mask=1
	q.exclude=[fish.get_rid()]
	return not get_world_3d().direct_space_state.intersect_shape(q,1).is_empty()


func _route(index: int,label: String,start: Vector3,yaw: float,axis: String,target: float,frames: int=460) -> bool:
	if route_filter!="" and route_filter!=label: return true
	route_runs+=1
	if test_piece!=null: test_piece.queue_free();await get_tree().physics_frame
	test_piece=_place(entries[index],Vector3.ZERO)
	_make_fish(start,yaw)
	for i in range(5): await get_tree().physics_frame
	if absf(yaw)>.01:
		_key(KEY_D if yaw>0 else KEY_A,true)
		for i in range(roundi(absf(yaw)/PI*72.0)): await get_tree().physics_frame
		_key(KEY_D if yaw>0 else KEY_A,false)
		for i in range(3): await get_tree().physics_frame
	var hit_count := 0
	var min_camera := 100.0
	var end := start
	_key(KEY_W,true)
	for i in range(frames):
		await get_tree().physics_frame
		end=fish.global_position
		var rig := fish.get_node("CameraRig") as Node3D
		min_camera=minf(min_camera,rig.global_position.distance_to(end))
		if i%12==0 and _probe_camera(rig.global_position): hit_count+=1
		if capture_dir!="" and i==110 and index==6:
			await _capture("pond_wood_002_passage.png",false)
		if (axis=="-z" and end.z<target) or (axis=="+z" and end.z>target) or (axis=="-x" and end.x<target) or (axis=="+x" and end.x>target): break
	_key(KEY_W,false)
	var arrived := (axis=="-z" and end.z<target) or (axis=="+z" and end.z>target) or (axis=="-x" and end.x<target) or (axis=="+x" and end.x>target)
	var passed := arrived and hit_count==0 and min_camera>.65
	print("POND_WOOD_002_ROUTE ",label," pass=",passed," start=",start," end=",end,
		" camera_hits=",hit_count," min_camera=",min_camera)
	if route_filter!="": return passed
	fish.call("ClearAndDisarmInput")
	fish.collision_layer=0
	fish.collision_mask=0
	fish.position=Vector3(0,-50,0)
	fish.hide()
	fish.set_physics_process(false)
	fish.set_process(false)
	(fish.get_node("CameraRig") as Node3D).set_physics_process(false)
	await get_tree().physics_frame
	return passed


func _vertical_route(index: int,label: String,start: Vector3) -> bool:
	if route_filter!="" and route_filter!=label: return true
	route_runs+=1
	if test_piece!=null: test_piece.queue_free();await get_tree().physics_frame
	test_piece=_place(entries[index],Vector3.ZERO)
	_make_fish(start,0)
	for i in range(5): await get_tree().physics_frame
	var hit_count := 0
	var min_camera := 100.0
	var end := start
	_key(KEY_UP,true)
	_key(KEY_W,true)
	for i in range(300):
		await get_tree().physics_frame
		end=fish.global_position
		var rig := fish.get_node("CameraRig") as Node3D
		min_camera=minf(min_camera,rig.global_position.distance_to(end))
		if i%12==0 and _probe_camera(rig.global_position): hit_count+=1
		if end.y>4.2 and end.z<start.z-1.0: break
	_key(KEY_W,false)
	_key(KEY_UP,false)
	var passed := end.y>4.2 and end.z<start.z-1.0 and hit_count==0 and min_camera>.65
	print("POND_WOOD_002_ROUTE ",label," pass=",passed," start=",start," end=",end,
		" camera_hits=",hit_count," min_camera=",min_camera)
	return passed


func _run_routes() -> void:
	var results := []
	for i in [0,1]:
		var r := float(entries[i]["generator_config"]["radius"])
		var length := float(entries[i]["generator_config"]["length"])
		for side in [-1.0,1.0]:
			results.append(await _route(i,"long_%d_side_%d" % [i,int(side)],
				Vector3(side*(r+1.65),1.5,length*.5+2),0,"-z",-length*.5-1))
	for i in [2,3]:
		var spread := float(entries[i]["generator_config"]["spread"])
		results.append(await _route(i,"fork_%d_lateral" % i,
			Vector3(-spread-1.7,2.5 if i==3 else 1.55,-2.65),PI*.5,"+x",spread+1.2))
	for i in [4,5]:
		results.append(await _route(i,"root_%d_undercut" % i,
			Vector3(0,1.38,6.5),0,"-z",2.95))
	for i in [6,7]:
		var center := float(entries[i]["generator_config"]["outer"])
		var length := float(entries[i]["generator_config"]["length"])
		results.append(await _route(i,"split_%d_end" % i,
			Vector3(0,center,length*.5+2),0,"-z",-length*.5-1))
		results.append(await _route(i,"split_%d_side" % i,
			Vector3(center+2,center,0),-PI*.5,"-x",-.6))
	for i in [8,9]:
		results.append(await _route(i,"crown_%d_lateral" % i,
			Vector3(-6,3.4,-3.0),PI*.5,"+x",6))
		results.append(await _vertical_route(i,"crown_%d_vertical" % i,
			Vector3(-1.8,1.35,2.0)))
	var passed := not results.has(false) and route_runs==1
	print("POND_WOOD_002_TRAVERSAL passed=",passed," routes=",route_runs,
		" filter=",route_filter)
	get_tree().quit(0 if passed else 1)


func _capture(filename: String,finish: bool=true) -> void:
	for i in range(8): await get_tree().process_frame
	await RenderingServer.frame_post_draw
	DirAccess.make_dir_recursive_absolute(capture_dir)
	var path := capture_dir.path_join(filename)
	var result := get_viewport().get_texture().get_image().save_png(path)
	print("POND_WOOD_002_CAPTURE path=",path," result=",result,
		" draw_calls=",Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME),
		" primitives=",Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME))
	if finish: get_tree().quit(0 if result==OK else 1)
