extends SceneTree

# Checks Godot's imported mesh data, rather than trusting Blender's manifest alone.
const MANIFEST := "res://../assets/faf/pond/plants/pond_foliage_001_manifest.json"


func _initialize() -> void:
	var file := FileAccess.open(ProjectSettings.globalize_path(MANIFEST), FileAccess.READ)
	if file == null:
		_fail("manifest unavailable")
		return
	var data: Dictionary = JSON.parse_string(file.get_as_text()) as Dictionary
	var entries: Array = data.get("assets", [])
	if entries.size() != 13:
		_fail("expected 13 assets")
		return
	var identities := {}
	for value in entries:
		var entry: Dictionary = value
		var faf_id := String(entry["faf_id"])
		if identities.has(faf_id) or entry.get("habitat") != "pond":
			_fail("duplicate or non-Pond identity: " + faf_id)
			return
		identities[faf_id] = true
		var path := "res://" + String(entry["glb"]).trim_prefix("game/")
		var packed := load(path) as PackedScene
		if packed == null:
			_fail("import failed: " + path)
			return
		var asset := packed.instantiate() as Node3D
		if asset == null:
			_fail("non-3D asset: " + faf_id)
			return
		root.add_child(asset)
		var meshes: Array[MeshInstance3D] = []
		_collect(asset, meshes)
		var expected_parts: Array = entry["components"]
		if meshes.size() != expected_parts.size():
			_fail("component count mismatch: " + faf_id)
			return
		var triangles := 0
		var materials := {}
		var all_bounds := AABB()
		var first := true
		var stem_found := false
		for node in meshes:
			if node.transform != Transform3D.IDENTITY:
				_fail("non-identity component transform: " + faf_id)
				return
			if node.mesh.get_surface_count() != 1:
				_fail("material surface mismatch: " + faf_id)
				return
			var material := node.mesh.surface_get_material(0)
			if material == null or not String(material.resource_name).begins_with("mat__pond_"):
				_fail("material mismatch: " + faf_id)
				return
			materials[material.resource_name] = true
			var arrays: Array = node.mesh.surface_get_arrays(0)
			var indices: PackedInt32Array = arrays[Mesh.ARRAY_INDEX]
			triangles += indices.size() / 3
			var uv: PackedVector2Array = arrays[Mesh.ARRAY_TEX_UV]
			if uv.is_empty():
				_fail("missing UV sway coordinate: " + faf_id)
				return
			var bounds := node.get_aabb()
			all_bounds = bounds if first else all_bounds.merge(bounds)
			first = false
			if node.name.ends_with("__stem"):
				stem_found = true
				var depth := float(entry["placement"]["mud_depth_m"])
				if absf(bounds.position.y + depth) > 0.025 or bounds.end.y < -0.05:
					_fail("lily stem misses surface/mud: " + faf_id + " " + str(bounds))
					return
		if triangles != int(entry["triangles"]) or materials.size() != int(entry["materials"]):
			_fail("cost mismatch: " + faf_id + " triangles=" + str(triangles))
			return
		var size: Array = entry["dimensions_m_blender_xyz"]
		var expected := Vector3(size[0], size[2], size[1])
		if all_bounds.size.distance_to(expected) > 0.035:
			_fail("dimensions mismatch: " + faf_id + " " + str(all_bounds.size))
			return
		if String(entry["family"]) == "lily_pad" and not stem_found:
			_fail("missing lily stem: " + faf_id)
			return
		print("FAF_POND_FOLIAGE_IMPORT ", faf_id, " components=", meshes.size(),
			" triangles=", triangles, " materials=", materials.size(), " size=", all_bounds.size)
		asset.queue_free()
	print("FAF_POND_FOLIAGE_OK assets=13 lily_stems=3")
	quit(0)


func _collect(node: Node, output: Array[MeshInstance3D]) -> void:
	if node is MeshInstance3D:
		output.append(node as MeshInstance3D)
	for child in node.get_children():
		_collect(child, output)


func _fail(message: String) -> void:
	printerr("FAF_POND_FOLIAGE_FAILED ", message)
	quit(1)
