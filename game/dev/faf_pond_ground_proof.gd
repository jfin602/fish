extends Node3D

# Isolated Forward+ Pond ground contact proof. The generated basin is one
# continuous mesh and the only playable floor; all details are small overlays.
const MANIFEST := "res://../assets/faf/pond/ground/pond_ground_001_manifest.json"
const ROOT := "res://assets/faf/pond/ground/"
const MATERIALS := "res://assets/faf/pond/materials/"
const WOOD := "res://scenes/faf_pond_wood/wood_002a_hero_hollow_trunk.tscn"

@onready var camera: Camera3D = $Camera
@onready var environment_node: WorldEnvironment = $Environment
var entries: Array = []
var placed := 0
var rejected_route := 0
var rejected_support := 0
var total_triangles := 0
var shared_material_bindings := 0
var view := "overview"
var capture_dir := ""
var check_only := false
var finished_heights: PackedFloat32Array = PackedFloat32Array()


func _ready() -> void:
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--faf-view="): view = arg.trim_prefix("--faf-view=")
		if arg.begins_with("--faf-capture-dir="): capture_dir = arg.trim_prefix("--faf-capture-dir=")
		if arg == "--faf-check": check_only = true
	var manifest_file := FileAccess.open(ProjectSettings.globalize_path(MANIFEST), FileAccess.READ)
	if manifest_file == null:
		push_error("Pond ground manifest unavailable")
		get_tree().quit(1)
		return
	var manifest: Dictionary = JSON.parse_string(manifest_file.get_as_text())
	entries = manifest.get("assets", [])
	if entries.size() != 8:
		push_error("Expected eight ground assets")
		get_tree().quit(1)
		return
	for role in ["silt", "mud", "wet_bank", "algae_stone"]:
		if load(MATERIALS + "pond_%s.tres" % role) == null:
			push_error("Missing Pond shared material: " + role)
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
	env.background_color = Color("#20291e")
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color("#b7a77e")
	env.ambient_light_energy = .35
	env.tonemap_mode = Environment.TONE_MAPPER_FILMIC
	env.fog_enabled = true
	env.fog_light_color = Color("#384735")
	env.fog_density = .008
	environment_node.environment = env


func _height(x: float, z: float) -> float:
	# A continuous shallow bank and two broad silt depressions, with no tile grid.
	var bank: float = 1.65 * smoothstep(6.0, 23.0, x)
	return -1.6 + bank + .18 * sin(x * .17 + z * .09) + .13 * sin(z * .14 - x * .07)


func _make_basin() -> void:
	var st := SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	for zi in range(30):
		for xi in range(34):
			var gx: float = -24.0 + xi * 1.5
			var gz: float = -21.0 + zi * 1.5
			finished_heights.append(_height(gx,gz))
	for zi in range(29):
		for xi in range(33):
			var x: float = -24.0 + xi * 1.5
			var z: float = -21.0 + zi * 1.5
			var a := Vector3(x, _height(x,z), z)
			var b := Vector3(x+1.5, _height(x+1.5,z), z)
			var c := Vector3(x+1.5, _height(x+1.5,z+1.5), z+1.5)
			var d := Vector3(x, _height(x,z+1.5), z+1.5)
			for vertex in [a,d,c,a,c,b]: st.add_vertex(vertex)
	st.generate_normals()
	var mesh := st.commit()
	var floor := MeshInstance3D.new()
	floor.name = "PrimaryGeneratedBasin"
	floor.mesh = mesh
	var shader_material := ShaderMaterial.new()
	shader_material.shader = load(MATERIALS + "pond_basin_variation.gdshader")
	floor.material_override = shader_material
	add_child(floor)
	var solid := StaticBody3D.new()
	solid.name = "FinishedTerrainCollision"
	solid.collision_layer = 1
	solid.collision_mask = 0
	add_child(solid)
	var collision := CollisionShape3D.new()
	collision.shape = mesh.create_trimesh_shape()
	solid.add_child(collision)
	# Explicitly count the floor separately from repeated detail triangles.
	print("FAF_POND_GROUND_BASIN triangles=", 33 * 29 * 2)


func _make_waterline() -> void:
	var plane := MeshInstance3D.new()
	plane.name = "Waterline"
	var mesh := PlaneMesh.new()
	mesh.size = Vector2(48,42)
	plane.mesh = mesh
	plane.position.y = .5
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color(.25,.38,.26,.045)
	mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	mat.cull_mode = BaseMaterial3D.CULL_DISABLED
	plane.material_override = mat
	add_child(plane)


func _make_wood() -> void:
	var packed := load(WOOD) as PackedScene
	if packed == null: return
	var wood := packed.instantiate() as Node3D
	wood.name = "ExistingHeroLogContext"
	add_child(wood)
	wood.position = Vector3(10,_height(10,-9)-.12,-9)


func _sample_surface(x: float, z: float) -> Dictionary:
	# Sample the exact triangle heights used for the finished visual/collision
	# mesh. Runtime integration may instead raycast the compiled Pond surface.
	var gx: float = (x + 24.0) / 1.5
	var gz: float = (z + 21.0) / 1.5
	if gx < 0 or gz < 0 or gx >= 33 or gz >= 29: return {}
	var ix := int(floor(gx))
	var iz := int(floor(gz))
	var tx: float = gx - ix
	var tz: float = gz - iz
	var a: float = finished_heights[iz*34+ix]
	var b: float = finished_heights[iz*34+ix+1]
	var c: float = finished_heights[(iz+1)*34+ix+1]
	var d: float = finished_heights[(iz+1)*34+ix]
	var h: float
	var normal: Vector3
	if tz >= tx:
		h = a*(1.0-tz) + d*(tz-tx) + c*tx
		normal = Vector3(0,d-a,1.5).cross(Vector3(1.5,c-a,1.5)).normalized()
	else:
		h = a*(1.0-tx) + c*tz + b*(tx-tz)
		normal = Vector3(1.5,c-a,1.5).cross(Vector3(1.5,b-a,0)).normalized()
	return {"position":Vector3(x,h,z),"normal":normal}


func _protected(x: float, z: float, radius: float) -> bool:
	# Center line is the fish route. Two circles guard the hero log mouths.
	if abs(x) < 2.4 + radius and z < 3.0 and z > -20.0: return true
	for mouth_z in [-14.0,-4.0]:
		if Vector2(x-10.0,z-mouth_z).length() < 3.7 + radius: return true
	return false


func _place(entry: Dictionary, x: float, z: float, yaw: float, scale_value: float) -> bool:
	var dims: Array = entry["dimensions_m"]
	var radius: float = max(float(dims[0]),float(dims[1])) * scale_value * .5
	if _protected(x,z,radius):
		rejected_route += 1
		return false
	var center := _sample_surface(x,z)
	if center.is_empty():
		rejected_support += 1
		return false
	var normal: Vector3 = center["normal"]
	var bank: bool = entry["family"] == "eroded_bank"
	if normal.y < (.67 if bank else .82):
		rejected_support += 1
		return false
	var half_x: float = float(dims[0]) * scale_value * .38
	var half_z: float = float(dims[1]) * scale_value * .38
	var max_delta := 0.0
	for offset in [Vector2(-half_x,-half_z),Vector2(half_x,-half_z),Vector2(-half_x,half_z),Vector2(half_x,half_z)]:
		var hit := _sample_surface(x+offset.x,z+offset.y)
		if hit.is_empty():
			rejected_support += 1
			return false
		max_delta = max(max_delta, abs(float(hit["position"].y) - float(center["position"].y)))
	if max_delta > (.70 if bank else .27):
		rejected_support += 1
		return false
	var path: String = ("res://scenes/faf_pond_ground/" if bank else ROOT) + String(entry["stem"]) + (".tscn" if bank else ".glb")
	var packed := load(path) as PackedScene
	if packed == null:
		push_error("Failed GLB import " + String(entry["stem"]))
		return false
	var instance := packed.instantiate() as Node3D
	instance.name = "Ground_%s_%d" % [entry["faf_id"],placed]
	_bind_shared_materials(instance)
	add_child(instance)
	var embed := .10 if bank else .018
	instance.position = center["position"] - normal * embed
	instance.rotation.y = yaw
	# Limit tilt so low debris does not float on one corner of the slope.
	instance.rotation.x = clamp(atan2(normal.z,normal.y),-.16,.16)
	instance.rotation.z = clamp(-atan2(normal.x,normal.y),-.16,.16)
	instance.scale = Vector3.ONE * scale_value
	placed += 1
	total_triangles += int(entry["triangle_count"])
	return true


func _bind_shared_materials(instance: Node3D) -> void:
	for child in instance.find_children("*","MeshInstance3D",true,false):
		var mesh_instance := child as MeshInstance3D
		for surface in mesh_instance.mesh.get_surface_count():
			var imported := mesh_instance.mesh.surface_get_material(surface)
			assert(imported != null)
			var role: String = String(imported.resource_name).trim_prefix("mat__pond_")
			var shared := load(MATERIALS + "pond_%s.tres" % role) as StandardMaterial3D
			assert(shared != null)
			mesh_instance.set_surface_override_material(surface, shared)
			shared_material_bindings += 1


func _layout() -> void:
	await get_tree().physics_frame
	# Exercise both protected spaces; these must be rejected before import.
	assert(not _place(entries[4],0,-10,0,1))
	assert(not _place(entries[6],10,-14,0,1))
	# Single-asset gallery, sparse bank patch, and denser sheltered pocket.
	for i in entries.size():
		_place(entries[i],-18.0 + i * 4.8,13.0, .15 * i, 1.0)
	var sparse := [Vector2(-17,-8),Vector2(-15,-10),Vector2(-18,-12),Vector2(-13,-7),Vector2(-14,-13)]
	for i in sparse.size():
		_place(entries[[2,4,6,5,7][i]],sparse[i].x,sparse[i].y,.45*i, .92 + .035*i)
	var dense := [Vector2(-11,-8),Vector2(-8,-10),Vector2(-12,-12),Vector2(-7,-13),Vector2(-15,-15),
		Vector2(-10,-15),Vector2(-6,-8),Vector2(-14,-5),Vector2(-8,-16),Vector2(-16,-11)]
	for i in dense.size():
		_place(entries[[0,2,4,6,3,5,7,1,4,6][i]],dense[i].x,dense[i].y,-.6+.23*i, .88 + .04*(i%5))
	var slope_group := [Vector2(18,5),Vector2(21,10),Vector2(17,-8),Vector2(20,-12),Vector2(16,14),Vector2(22,-1)]
	for i in slope_group.size():
		_place(entries[[0,1,2,3,4,6][i]],slope_group[i].x,slope_group[i].y,-.35+.18*i, .91+.03*i)
	_setup_camera()
	print("FAF_POND_GROUND_PROOF_READY imported=8 instances=",placed," detail_triangles=",total_triangles,
		" route_rejects=",rejected_route," support_rejects=",rejected_support,
		" shared_material_bindings=",shared_material_bindings,
		" renderer=",ProjectSettings.get_setting("rendering/renderer/rendering_method"))
	if check_only: get_tree().quit(0 if placed >= 20 else 1)
	elif capture_dir != "": _capture.call_deferred()


func _setup_camera() -> void:
	match view:
		"banks": camera.position = Vector3(-14,3.8,19); camera.look_at(Vector3(-15,-1,13))
		"details": camera.position = Vector3(9,3.7,19); camera.look_at(Vector3(8,-1,13))
		"cluster": camera.position = Vector3(-16,3.1,-2); camera.look_at(Vector3(-10,-1,-10))
		"slope": camera.position = Vector3(18,6,19); camera.look_at(Vector3(19,-.3,2))
		"above": camera.position = Vector3(-5,25,12); camera.look_at(Vector3(-3,-1,-2))
		"underwater": camera.position = Vector3(-21,-.15,-18); camera.look_at(Vector3(-9,-1,-9))
		"fish":
			# Use the real Fish camera node/FOV in a frozen presentation pose.
			var main: Node3D = load("res://scenes/main.tscn").instantiate()
			var fish: CharacterBody3D = main.get_node("Fish").duplicate() as CharacterBody3D
			main.free()
			fish.name = "ReviewFishCameraRig"
			fish.process_mode = Node.PROCESS_MODE_DISABLED
			add_child(fish)
			fish.global_position = Vector3.ZERO
			var fish_camera := fish.get_node("CameraRig/Camera3D") as Camera3D
			fish_camera.global_position = Vector3(-3,-.2,-18)
			fish_camera.look_at(Vector3(-10,-1,-10))
			fish_camera.current = true
		"gallery": camera.position = Vector3(-3,6,22); camera.look_at(Vector3(-1,-1,12))
		_: camera.position = Vector3(-10,14,21); camera.look_at(Vector3(-5,-1,-4))


func _capture() -> void:
	for i in range(8): await get_tree().process_frame
	var active := get_viewport().get_camera_3d()
	print("FAF_POND_GROUND_CAMERA view=",view," node=",active.get_path()," position=",active.global_position,
		" forward=",-active.global_basis.z)
	print("FAF_POND_GROUND_RENDER draw_calls=",Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME),
		" objects=",Performance.get_monitor(Performance.RENDER_TOTAL_OBJECTS_IN_FRAME),
		" primitives=",Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME))
	DirAccess.make_dir_recursive_absolute(capture_dir)
	var path: String = capture_dir.path_join("pond_ground_%s.png" % view)
	var error := get_viewport().get_texture().get_image().save_png(path)
	print("FAF_POND_GROUND_CAPTURE path=",path," result=",error)
	get_tree().quit(0 if error == OK else 1)
