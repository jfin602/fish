extends SceneTree

const MANIFEST := "res://../assets/faf/pond/plants/pond_stealth_vegetation_001_manifest.json"


func _initialize() -> void:
	var file := FileAccess.open(ProjectSettings.globalize_path(MANIFEST), FileAccess.READ)
	if file == null:
		_fail("manifest unavailable")
		return
	var manifest: Dictionary = JSON.parse_string(file.get_as_text())
	var entries: Array = manifest.get("assets", [])
	if entries.size() != 13:
		_fail("expected 13 assets")
		return
	var identities := {}
	var total_tris := 0
	for item in entries:
		var entry: Dictionary = item
		var faf_id := String(entry["faf_id"])
		if identities.has(faf_id) or entry["habitat"] != "pond":
			_fail("identity collision " + faf_id)
			return
		identities[faf_id] = true
		var resource := "res://" + String(entry["glb"]).trim_prefix("game/")
		var packed := load(resource) as PackedScene
		if packed == null:
			_fail("import failed " + resource)
			return
		var asset := packed.instantiate() as Node3D
		if asset == null:
			_fail("not a 3D asset " + faf_id)
			return
		root.add_child(asset)
		var meshes: Array[MeshInstance3D] = []
		_collect(asset, meshes)
		if meshes.size() != (entry["components"] as Array).size() or _has_collision(asset):
			_fail("components/collision mismatch " + faf_id)
			return
		var combined := AABB()
		var first := true
		var tris := 0
		var mats := {}
		for mesh_node in meshes:
			if mesh_node.transform != Transform3D.IDENTITY or mesh_node.mesh.get_surface_count() != 1:
				_fail("mesh transform/surface " + faf_id)
				return
			var surface := mesh_node.mesh.surface_get_arrays(0)
			var indices: PackedInt32Array = surface[Mesh.ARRAY_INDEX]
			var uv: PackedVector2Array = surface[Mesh.ARRAY_TEX_UV]
			if uv.is_empty():
				_fail("missing sway UV " + faf_id)
				return
			tris += indices.size() / 3
			var mat := mesh_node.mesh.surface_get_material(0)
			if mat == null or not String(mat.resource_name).begins_with("mat__pond_cover_"):
				_fail("material " + faf_id)
				return
			mats[mat.resource_name] = true
			var box := mesh_node.get_aabb()
			combined = box if first else combined.merge(box)
			first = false
		if tris != int(entry["triangles"]) or mats.size() != int(entry["materials"]):
			_fail("cost mismatch " + faf_id + " triangles=" + str(tris))
			return
		var dims: Array = entry["dimensions_m_blender_xyz"]
		var expected := Vector3(dims[0], dims[2], dims[1])
		if combined.size.distance_to(expected) > .04 or absf(combined.position.y) > .04:
			_fail("bounds/root mismatch " + faf_id + " " + str(combined))
			return
		total_tris += tris
		print("POND_COVER_IMPORT ",faf_id," triangles=",tris," materials=",mats.size()," size=",combined.size)
		asset.queue_free()
	print("POND_COVER_GODOT_OK assets=13 triangles=",total_tris)
	quit(0)


func _collect(node: Node, output: Array[MeshInstance3D]) -> void:
	if node is MeshInstance3D:
		output.append(node as MeshInstance3D)
	for child in node.get_children():
		_collect(child, output)


func _has_collision(node: Node) -> bool:
	if node is CollisionObject3D or node is CollisionShape3D:
		return true
	for child in node.get_children():
		if _has_collision(child):
			return true
	return false


func _fail(message: String) -> void:
	printerr("POND_COVER_GODOT_FAILED ",message)
	quit(1)
