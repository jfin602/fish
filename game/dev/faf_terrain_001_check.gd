extends SceneTree

# Focused import proof for all six FAF terrain chunks; independent of live P2.
const MANIFEST_PATH := "res://../assets/faf/terrain/terrain_001_manifest.json"


func _initialize() -> void:
	var file := FileAccess.open(ProjectSettings.globalize_path(MANIFEST_PATH), FileAccess.READ)
	if file == null:
		_fail("manifest unavailable")
		return
	var manifest: Dictionary = JSON.parse_string(file.get_as_text()) as Dictionary
	if manifest.is_empty() or not manifest.has("assets"):
		_fail("manifest invalid")
		return
	var entries: Array = manifest["assets"]
	if entries.size() != 6:
		_fail("expected six terrain entries")
		return
	for record in entries:
		var entry: Dictionary = record
		var id: String = String(entry["faf_id"])
		var path: String = "res://" + String(entry["glb"]).trim_prefix("game/")
		var packed: PackedScene = load(path) as PackedScene
		if packed == null:
			_fail(id + " failed import")
			return
		var asset: Node3D = packed.instantiate() as Node3D
		root.add_child(asset)
		var meshes: Array[MeshInstance3D] = []
		_collect_meshes(asset, meshes)
		if meshes.size() != 1:
			_fail(id + " expected one mesh")
			return
		var node: MeshInstance3D = meshes[0]
		if asset.transform != Transform3D.IDENTITY or node.transform != Transform3D.IDENTITY:
			_fail(id + " non-identity import transform")
			return
		var mesh: Mesh = node.mesh
		var expected_materials: Array = entry["material_names"]
		if mesh.get_surface_count() != expected_materials.size():
			_fail(id + " material surface count mismatch")
			return
		var triangles := 0
		var upward_normals := 0
		for surface in mesh.get_surface_count():
			var material: Material = mesh.surface_get_material(surface)
			if material == null or not expected_materials.has(material.resource_name):
				_fail(id + " material mismatch")
				return
			var arrays: Array = mesh.surface_get_arrays(surface)
			var indices: PackedInt32Array = arrays[Mesh.ARRAY_INDEX]
			triangles += indices.size() / 3
			var normals: PackedVector3Array = arrays[Mesh.ARRAY_NORMAL]
			for normal in normals:
				if normal.y > 0.25:
					upward_normals += 1
		if triangles != int(entry["triangles"]) or upward_normals == 0:
			_fail(id + " triangle/normal mismatch " + str(triangles))
			return
		var size: Array = entry["dimensions_m"]
		var expected := Vector3(size[0], size[2], size[1])
		var bounds: AABB = node.get_aabb()
		if bounds.size.distance_to(expected) > 0.03 or absf(bounds.position.y + 0.09) > 0.02:
			_fail(id + " dimensions/anchor mismatch " + str(bounds))
			return
		print("FAF_TERRAIN_IMPORT ", id, " size=", bounds.size,
			" triangles=", triangles, " materials=", mesh.get_surface_count(),
			" upward_normals=", upward_normals)
		asset.queue_free()
	print("FAF_TERRAIN_001_OK")
	quit(0)


func _collect_meshes(node: Node, output: Array[MeshInstance3D]) -> void:
	if node is MeshInstance3D:
		output.append(node as MeshInstance3D)
	for child in node.get_children():
		_collect_meshes(child, output)


func _fail(message: String) -> void:
	printerr("FAF_TERRAIN_001_FAILED ", message)
	quit(1)
