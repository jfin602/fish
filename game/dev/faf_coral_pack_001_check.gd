extends SceneTree

# Isolated import check; reads the source-controlled FAF candidate manifest.
const MANIFEST_PATH := "res://../assets/faf/coral/coral_001_to_004_manifest.json"


func _initialize() -> void:
	var file := FileAccess.open(ProjectSettings.globalize_path(MANIFEST_PATH), FileAccess.READ)
	if file == null:
		_fail("manifest is unavailable")
		return
	var manifest: Dictionary = JSON.parse_string(file.get_as_text()) as Dictionary
	if manifest.is_empty() or not manifest.has("assets"):
		_fail("manifest is invalid")
		return
	var entries: Array = manifest["assets"]
	if entries.size() != 12:
		_fail("expected twelve records")
		return
	for record in entries:
		var entry: Dictionary = record
		var asset_path: String = "res://" + String(entry["glb"]).trim_prefix("game/")
		var packed: PackedScene = load(asset_path) as PackedScene
		if packed == null:
			_fail(String(entry["faf_id"]) + " failed import")
			return
		var asset: Node3D = packed.instantiate() as Node3D
		if asset == null:
			_fail(String(entry["faf_id"]) + " has no 3D root")
			return
		root.add_child(asset)
		var meshes: Array[MeshInstance3D] = []
		_collect_meshes(asset, meshes)
		if meshes.size() != 1:
			_fail(String(entry["faf_id"]) + " expected one mesh")
			return
		var mesh_node: MeshInstance3D = meshes[0]
		if asset.transform != Transform3D.IDENTITY or mesh_node.transform != Transform3D.IDENTITY:
			_fail(String(entry["faf_id"]) + " has non-identity transforms")
			return
		if mesh_node.mesh.get_surface_count() != 1:
			_fail(String(entry["faf_id"]) + " expected one material surface")
			return
		var material: Material = mesh_node.mesh.surface_get_material(0)
		var swatch: String = String(entry["palette"]).get_slice(".", 1)
		if material == null or material.resource_name != "mat__coral_" + swatch:
			_fail(String(entry["faf_id"]) + " material mismatch")
			return
		var source_size: Array = entry["dimensions_m"]
		var expected_size := Vector3(source_size[0], source_size[2], source_size[1])
		var bounds: AABB = mesh_node.get_aabb()
		if bounds.size.distance_to(expected_size) > 0.025 or absf(bounds.position.y) > 0.025:
			_fail(String(entry["faf_id"]) + " size/contact mismatch: " + str(bounds))
			return
		var arrays: Array = mesh_node.mesh.surface_get_arrays(0)
		var indices: PackedInt32Array = arrays[Mesh.ARRAY_INDEX]
		var triangles: int = indices.size() / 3
		if triangles != int(entry["triangles"]):
			_fail(String(entry["faf_id"]) + " triangle mismatch: " + str(triangles))
			return
		print("FAF_CORAL_IMPORT ", entry["faf_id"], " size=", bounds.size,
			" triangles=", triangles, " material=", material.resource_name)
		asset.queue_free()
	print("FAF_CORAL_PACK_001_OK")
	quit(0)


func _collect_meshes(node: Node, output: Array[MeshInstance3D]) -> void:
	if node is MeshInstance3D:
		output.append(node as MeshInstance3D)
	for child in node.get_children():
		_collect_meshes(child, output)


func _fail(message: String) -> void:
	printerr("FAF_CORAL_PACK_001_FAILED ", message)
	quit(1)
