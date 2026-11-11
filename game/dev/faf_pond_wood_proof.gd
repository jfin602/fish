extends Node3D

# Pond-only Forward+ review and real-controller traversal fixture.
const MANIFEST := "res://../assets/faf/pond/wood/pond_wood_001_manifest.json"
const WOOD_SCENE := "res://scenes/faf_pond_wood/"

@onready var overview: Camera3D = $OverviewCamera
@onready var world_environment: WorldEnvironment = $WorldEnvironment
var capture_dir := ""
var view := "gallery"
var do_traverse := false
var fish: CharacterBody3D
var hero: Node3D
var costs := {"triangles": 0, "instances": 0}


func _ready() -> void:
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--faf-view="): view = arg.trim_prefix("--faf-view=")
		if arg.begins_with("--faf-capture-dir="): capture_dir = arg.trim_prefix("--faf-capture-dir=")
		if arg == "--faf-traverse": do_traverse = true
	_make_environment()
	_make_ground()
	_make_water()
	var file := FileAccess.open(ProjectSettings.globalize_path(MANIFEST),FileAccess.READ)
	if file == null:
		push_error("Wood manifest unavailable")
		get_tree().quit(1)
		return
	var manifest: Dictionary = JSON.parse_string(file.get_as_text())
	var assets: Array = manifest.get("assets",[])
	if assets.size() != 12:
		push_error("Expected twelve wood assets")
		get_tree().quit(1)
		return
	for i in assets.size():
		var entry: Dictionary = assets[i]
		var piece := _place(entry,Vector3(12 + float(i % 4)*4.5,0,(float(i/4)-1)*7.5),0,1)
		piece.name = "Gallery_%s" % entry["faf_id"]
		if entry["faf_id"] == "FAF-WOOD-002A":
			hero = _place(entry,Vector3.ZERO,0,1)
			hero.name = "HeroPassage"
			costs["instances"] += 1
			costs["triangles"] += int(entry["triangle_count"])
		costs["instances"] += 1
		costs["triangles"] += int(entry["triangle_count"])
	# Authored mixed cover beyond the hero route; rotations/scales are bounded.
	for i in [1,2,6,8,9,10,11]:
		var entry: Dictionary = assets[i]
		var angle: float = [-0.18,0.26,-0.32,0.12,-0.22,0.29,-0.10][[1,2,6,8,9,10,11].find(i)]
		var scale_value: float = [0.93,1.08,1.0,0.88,1.12,0.96,1.04][[1,2,6,8,9,10,11].find(i)]
		_place(entry,Vector3(-10 + float(i % 3)*3.0,0,-5 + float(i/3)*3.2),angle,scale_value)
		costs["instances"] += 1
		costs["triangles"] += int(entry["triangle_count"])
	_setup_camera()
	print("FAF_POND_WOOD_PROOF_READY assets=12 instances=",costs["instances"],
		" instanced_triangles=",costs["triangles"]," renderer=",ProjectSettings.get_setting("rendering/renderer/rendering_method"))
	if do_traverse:
		_traverse.call_deferred()
	elif capture_dir != "":
		_capture.call_deferred("pond_wood_%s.png" % view)


func _place(entry: Dictionary, at: Vector3, angle: float, scale_value: float) -> Node3D:
	var path := WOOD_SCENE + String(entry["stem"]) + ".tscn"
	var packed := load(path) as PackedScene
	assert(packed != null,path)
	var instance := packed.instantiate() as Node3D
	add_child(instance)
	instance.position = at
	instance.rotation.y = angle
	instance.scale = Vector3.ONE * scale_value
	return instance


func _make_environment() -> void:
	var env := Environment.new()
	env.background_mode = Environment.BG_COLOR
	env.background_color = Color("#203b2b")
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color("#9aab81")
	env.ambient_light_energy = .55
	env.tonemap_mode = Environment.TONE_MAPPER_FILMIC
	env.fog_enabled = true
	env.fog_light_color = Color("#365340")
	env.fog_density = .013
	world_environment.environment = env


func _make_ground() -> void:
	var material := StandardMaterial3D.new()
	material.albedo_color = Color("#534a31")
	material.roughness = 1
	var floor := MeshInstance3D.new()
	floor.name = "SiltFloor"
	var mesh := PlaneMesh.new()
	mesh.size = Vector2(54,44)
	floor.mesh = mesh
	floor.material_override = material
	floor.position.y = -.06
	add_child(floor)
	var solid := StaticBody3D.new()
	solid.name = "SiltCollision"
	solid.collision_layer = 1
	solid.collision_mask = 0
	add_child(solid)
	var shape := CollisionShape3D.new()
	shape.shape = BoxShape3D.new()
	shape.shape.size = Vector3(54,.25,44)
	shape.position.y = -.19
	solid.add_child(shape)
	# A shallow raised bank remains away from the two-way test route.
	var bank := MeshInstance3D.new()
	bank.name = "SlopedMudBank"
	var bank_mesh := PrismMesh.new()
	bank_mesh.size = Vector3(6,1.6,20)
	bank.mesh = bank_mesh
	bank.material_override = material
	bank.position = Vector3(21,.45,0)
	add_child(bank)


func _make_water() -> void:
	var water := MeshInstance3D.new()
	water.name = "Waterline"
	var mesh := PlaneMesh.new()
	mesh.size = Vector2(54,44)
	water.mesh = mesh
	water.position.y = 5.4
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color(0.25,0.45,0.31,0.17)
	mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	mat.cull_mode = BaseMaterial3D.CULL_DISABLED
	water.material_override = mat
	add_child(water)
	var tunnel_light := OmniLight3D.new()
	tunnel_light.name = "FilteredInteriorLight"
	tunnel_light.position = Vector3(0,3.55,0)
	tunnel_light.light_color = Color("#b5a773")
	tunnel_light.light_energy = .9
	tunnel_light.omni_range = 7.5
	add_child(tunnel_light)


func _setup_camera() -> void:
	match view:
		"hero_entrance": overview.position = Vector3(4.5,4.1,10); overview.look_at(Vector3(0,2.3,2.5))
		"hero_inside": overview.position = Vector3(0,2.5,0); overview.look_at(Vector3(0,2.3,-8))
		"hero_exit": overview.position = Vector3(-4.5,4.3,-9); overview.look_at(Vector3(0,2.3,-2))
		"top": overview.position = Vector3(10,38,14); overview.look_at(Vector3(10,0,0))
		_: overview.position = Vector3(12,23,31); overview.look_at(Vector3(10,0,0))


func _make_fish(start: Vector3) -> void:
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	fish = main.get_node("Fish").duplicate() as CharacterBody3D
	fish.name = "Fish"
	main.free()
	add_child(fish)
	fish.position = start
	(fish.get_node("CameraRig/Camera3D") as Camera3D).current = true


func _key(code: Key, down: bool) -> void:
	var event := InputEventKey.new()
	event.physical_keycode = code
	event.pressed = down
	Input.parse_input_event(event)


func _traverse() -> void:
	_make_fish(Vector3(0,2.5,8.2))
	for i in range(4): await get_tree().physics_frame
	var first := await _pass(false)
	_key(KEY_W,false)
	fish.call("ClearAndDisarmInput")
	fish.position = Vector3(0,2.5,-8.2)
	for i in range(5): await get_tree().physics_frame
	# Rotate the real controller while stopped; the second pass then approaches
	# the opposite opening under normal thrust and follow-camera code.
	_key(KEY_D,true)
	for i in range(72): await get_tree().physics_frame
	_key(KEY_D,false)
	for i in range(4): await get_tree().physics_frame
	var second := await _pass(true)
	_key(KEY_W,false)
	var ok := first and second
	print("FAF_POND_WOOD_TRAVERSAL first=",first," return=",second,
		" fish=",fish.global_position," camera=",fish.get_node("CameraRig").global_position)
	get_tree().quit(0 if ok else 1)


func _pass(reverse: bool) -> bool:
	var entered := false
	var exited := false
	var min_distance := 100.0
	var camera_hits := 0
	var start_z := fish.position.z
	_key(KEY_W,true)
	for i in range(540):
		await get_tree().physics_frame
		var z := fish.position.z
		if absf(z) < 4.8: entered = true
		if entered and ((reverse and z > 5.8) or (not reverse and z < -5.8)):
			exited = true
			break
		var rig: Node3D = fish.get_node("CameraRig")
		min_distance = minf(min_distance,rig.global_position.distance_to(fish.global_position))
		if i % 12 == 0 and _camera_blocked(rig.global_position): camera_hits += 1
		if capture_dir != "" and entered and absf(z) < .13:
			await _capture("pond_wood_%s_inside.png" % ("return" if reverse else "first"))
	_key(KEY_W,false)
	print("FAF_POND_WOOD_PASS direction=",("return" if reverse else "first"),
		" start_z=",start_z," end_z=",fish.position.z," entered=",entered,
		" exited=",exited," min_camera_distance=",min_distance," camera_hits=",camera_hits)
	return entered and exited and camera_hits == 0 and min_distance > .70


func _camera_blocked(at: Vector3) -> bool:
	var sphere := SphereShape3D.new(); sphere.radius = .22
	var query := PhysicsShapeQueryParameters3D.new()
	query.shape = sphere
	query.transform = Transform3D(Basis.IDENTITY,at)
	query.collision_mask = 1
	query.exclude = [fish.get_rid()]
	return not get_world_3d().direct_space_state.intersect_shape(query,1).is_empty()


func _capture(filename: String) -> void:
	await RenderingServer.frame_post_draw
	DirAccess.make_dir_recursive_absolute(capture_dir)
	var path := capture_dir.path_join(filename)
	get_viewport().get_texture().get_image().save_png(path)
	print("FAF_POND_WOOD_CAPTURE=",path," rendered_primitives=",
		Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME))
