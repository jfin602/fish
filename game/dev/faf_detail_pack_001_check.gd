extends SceneTree

const MANIFEST_PATH := "res://../assets/faf/details/detail_001_to_004_manifest.json"


func _initialize() -> void:
	var file := FileAccess.open(ProjectSettings.globalize_path(MANIFEST_PATH), FileAccess.READ)
	if file == null:
		_fail("manifest unavailable")
		return
	var manifest: Dictionary = JSON.parse_string(file.get_as_text()) as Dictionary
	if manifest.is_empty() or not manifest.has("assets") or manifest["assets"].size() != 8:
		_fail("expected eight manifest records")
		return
	for record in manifest["assets"]:
		var entry: Dictionary = record
		var id: String = entry["faf_id"]
		var packed: PackedScene = load("res://" + String(entry["glb"]).trim_prefix("game/")) as PackedScene
		if packed == null:
			_fail(id + " failed GLB import")
			return
		var asset: Node3D = packed.instantiate() as Node3D
		root.add_child(asset)
		var meshes: Array[MeshInstance3D] = []
		_collect_meshes(asset, meshes)
		if meshes.size() != 1:
			_fail(id + " expected one mesh")
			return
		var mesh_node: MeshInstance3D = meshes[0]
		if asset.transform != Transform3D.IDENTITY or mesh_node.transform != Transform3D.IDENTITY:
			_fail(id + " nonidentity import transform")
			return
		if mesh_node.mesh.get_surface_count() != 1:
			_fail(id + " expected one material surface")
			return
		var material: Material = mesh_node.mesh.surface_get_material(0)
		if material == null or material.resource_name != String(entry["material_name"]):
			_fail(id + " material mismatch")
			return
		var dims: Array = entry["dimensions_m"]
		var expected_size := Vector3(dims[0], dims[2], dims[1])
		var bounds: AABB = mesh_node.get_aabb()
		if bounds.size.distance_to(expected_size) > 0.003 or absf(bounds.position.y) > 0.003:
			_fail(id + " size/contact mismatch: " + str(bounds))
			return
		var triangles: int = mesh_node.mesh.surface_get_array_index_len(0) / 3
		if triangles != int(entry["triangles"]):
			_fail(id + " triangle mismatch: " + str(triangles))
			return
		print("FAF_DETAIL_IMPORT ", id, " size=", bounds.size,
			" triangles=", triangles, " material=", material.resource_name)
		asset.queue_free()
	print("FAF_DETAIL_PACK_001_OK")
	quit(0)


func _collect_meshes(node: Node, output: Array[MeshInstance3D]) -> void:
	if node is MeshInstance3D:
		output.append(node as MeshInstance3D)
	for child in node.get_children():
		_collect_meshes(child, output)


func _fail(message: String) -> void:
	printerr("FAF_DETAIL_PACK_001_FAILED ", message)
	quit(1)
