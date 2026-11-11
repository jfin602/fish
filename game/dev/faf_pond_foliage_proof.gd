extends Node3D

# Isolated Forward+ native visual fixture. Never referenced by normal play.
const MANIFEST := "res://../assets/faf/pond/plants/pond_foliage_001_manifest.json"

@onready var camera: Camera3D = $Camera
@onready var environment_node: WorldEnvironment = $WorldEnvironment

var view := "underwater"
var family := "all"
var capture_path := ""
var surface_motion: Array[Node3D] = []
var elapsed := 0.0


func _ready() -> void:
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--faf-view="):
			view = argument.trim_prefix("--faf-view=")
		if argument.begins_with("--faf-family="):
			family = argument.trim_prefix("--faf-family=")
		if argument.begins_with("--faf-capture="):
			capture_path = argument.trim_prefix("--faf-capture=")
	_make_environment()
	_make_bed()
	_make_water()
	var file := FileAccess.open(ProjectSettings.globalize_path(MANIFEST), FileAccess.READ)
	if file == null:
		push_error("FAF Pond foliage manifest unavailable")
		get_tree().quit(1)
		return
	var data: Dictionary = JSON.parse_string(file.get_as_text()) as Dictionary
	var entries: Array = data.get("assets", [])
	if entries.size() != 13:
		push_error("FAF Pond foliage expected 13 assets")
		get_tree().quit(1)
		return
	var placed := 0
	for item in entries:
		var entry: Dictionary = item
		var kind := String(entry["family"])
		if family != "all" and family != kind:
			continue
		var path := "res://" + String(entry["glb"]).trim_prefix("game/")
		var packed := load(path) as PackedScene
		if packed == null:
			push_error("FAF Pond foliage failed: " + path)
			get_tree().quit(1)
			return
		var asset := packed.instantiate() as Node3D
		add_child(asset)
		asset.name = String(entry["faf_id"])
		var xyz := _placement(kind, String(entry["variant"]))
		asset.position = xyz
		asset.rotation.y = float(placed % 5) * 0.23
		if kind == "lily_pad":
			var pad := _find_pad(asset)
			if pad != null:
				surface_motion.append(pad)
		elif kind == "floating_muck":
			surface_motion.append(asset)
		placed += 1
	_position_camera()
	print("FAF_POND_FOLIAGE_PROOF_READY family=", family, " view=", view,
		" assets=", placed, " forward_plus=", ProjectSettings.get_setting("rendering/renderer/rendering_method"))
	if not capture_path.is_empty():
		_capture.call_deferred()


func _process(delta: float) -> void:
	# Fixture-only surface bob: lily stems and all mud roots remain fixed.
	elapsed += delta
	for i in range(surface_motion.size()):
		surface_motion[i].position.y = sin(elapsed * 0.65 + float(i) * 1.7) * 0.0025


func _find_pad(node: Node) -> Node3D:
	if node is Node3D and node.name.ends_with("__pad"):
		return node as Node3D
	for child in node.get_children():
		var found := _find_pad(child)
		if found != null:
			return found
	return null


func _make_environment() -> void:
	var env := Environment.new()
	env.background_mode = Environment.BG_COLOR
	env.background_color = Color("#2c3d30")
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color("#9aa581")
	env.ambient_light_energy = 0.34
	env.tonemap_mode = Environment.TONE_MAPPER_FILMIC
	env.fog_enabled = true
	env.fog_light_color = Color("#304d3e")
	env.fog_density = 0.021
	environment_node.environment = env


func _floor_y(x: float) -> float:
	var xs := [-7.0, -5.7, -4.5, -3.5, -1.0, 1.5, 4.5, 5.7, 7.0]
	var ys := [0.7, 0.28, -0.18, -0.72, -1.08, -1.48, -0.50, 0.18, 0.65]
	for i in range(xs.size() - 1):
		if x <= xs[i + 1]:
			return lerpf(ys[i], ys[i + 1], clampf((x - xs[i]) / (xs[i + 1] - xs[i]), 0, 1))
	return ys[-1]


func _make_bed() -> void:
	var xs := [-7.0, -5.7, -4.5, -3.5, -1.0, 1.5, 4.5, 5.7, 7.0]
	var zs := [-7.0, -3.0, 1.0, 7.0]
	var surface := SurfaceTool.new()
	surface.begin(Mesh.PRIMITIVE_TRIANGLES)
	for i in range(xs.size() - 1):
		for j in range(zs.size() - 1):
			var x0: float = xs[i]
			var x1: float = xs[i + 1]
			var z0: float = zs[j]
			var z1: float = zs[j + 1]
			var a := Vector3(x0, _floor_y(x0), z0)
			var b := Vector3(x1, _floor_y(x1), z0)
			var c := Vector3(x1, _floor_y(x1), z1)
			var d := Vector3(x0, _floor_y(x0), z1)
			for vertex in [a, b, c, a, c, d]:
				surface.add_vertex(vertex)
	surface.generate_normals()
	var bed := MeshInstance3D.new()
	bed.name = "AuthoredShallowMudBank"
	bed.mesh = surface.commit()
	var mud := StandardMaterial3D.new()
	mud.albedo_color = Color("#3d3828")
	mud.roughness = 1.0
	bed.material_override = mud
	add_child(bed)


func _make_water() -> void:
	var water := MeshInstance3D.new()
	water.name = "WaterlineReference"
	var plane := PlaneMesh.new()
	plane.size = Vector2(14, 14)
	water.mesh = plane
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color(0.16, 0.31, 0.26, 0.30)
	mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	mat.cull_mode = BaseMaterial3D.CULL_DISABLED
	water.material_override = mat
	water.position.y = -0.015
	add_child(water)


func _placement(kind: String, variant: String) -> Vector3:
	match kind:
		"lily_pad":
			match variant:
				"A": return Vector3(-3.5, 0, -2.7)
				"B": return Vector3(-1.0, 0, -2.0)
				_: return Vector3(1.5, 0, -2.6)
		"tall_grass":
			var x := -5.7 if variant == "A" else -5.4
			return Vector3(x, _floor_y(x), 1.5 if variant == "A" else -1.1)
		"cattail":
			var x := 4.5 if variant == "A" else 5.0
			return Vector3(x, _floor_y(x), -2.6 if variant == "A" else 1.5)
		"reed":
			var x := -4.5 if variant == "A" else -4.3
			return Vector3(x, _floor_y(x), -3.5 if variant == "A" else 3.1)
		"submerged_algae":
			var x := -2.5 if variant == "A" else 2.8
			return Vector3(x, _floor_y(x) + 0.01, 3.1)
		"floating_muck":
			return Vector3(-2.7 if variant == "A" else 3.0, 0, 0.4)
	return Vector3.ZERO


func _position_camera() -> void:
	var target_x := 0.0
	match family:
		"lily_pad": target_x = -1.0
		"tall_grass": target_x = -5.5
		"cattail": target_x = 4.8
		"reed": target_x = -4.4
		"submerged_algae": target_x = 0.1
		"floating_muck": target_x = 0.1
	var camera_x := target_x
	if family in ["tall_grass", "reed"]:
		camera_x += 2.5 # Look back at the left bank from open water.
	if family == "cattail":
		camera_x -= 2.5 # Look back at the right bank from open water.
	var focus_z := 3.1 if family == "submerged_algae" else -0.5
	var focus_y := -1.0 if family == "submerged_algae" else (-0.55 if view == "underwater" else 0.1)
	if family in ["tall_grass", "reed"]:
		focus_y = 0.5
	if family == "cattail":
		focus_y = 0.3
	if view == "above":
		camera.position = Vector3(camera_x, 5.8 if family == "all" else 3.6, 8.0 if family == "all" else 6.0)
	elif view == "waterline":
		camera.position = Vector3(camera_x, 0.16, 8.1 if family == "all" else 6.4)
	else:
		camera.position = Vector3(camera_x, -0.64, 6.2 if family == "all" else 6.6)
	camera.look_at(Vector3(target_x, focus_y, focus_z), Vector3.UP)
	camera.fov = 52 if family == "all" else (59 if family == "submerged_algae" else 46)


func _capture() -> void:
	for _i in 6:
		await get_tree().process_frame
	await RenderingServer.frame_post_draw
	var image := get_viewport().get_texture().get_image()
	var err := image.save_png(capture_path)
	if err != OK:
		push_error("FAF Pond foliage capture failed: " + str(err))
		get_tree().quit(1)
		return
	print("FAF_POND_FOLIAGE_CAPTURE ", capture_path, " ", image.get_width(), "x", image.get_height())
	get_tree().quit(0)
