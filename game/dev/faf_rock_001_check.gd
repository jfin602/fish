extends SceneTree

# Isolated import proof for the four original FAF-ROCK-001 GLBs.
const EXPECTED := {
	"a": {"size": Vector3(1.56131, 0.88574, 1.29143), "triangles": 240},
	"b": {"size": Vector3(1.85, 0.57224, 2.035), "triangles": 186},
	"c": {"size": Vector3(1.22297, 1.39174, 0.975), "triangles": 248},
	"d": {"size": Vector3(1.46411, 1.03483, 1.04133), "triangles": 222},
}


func _initialize() -> void:
	for letter in ["a", "b", "c", "d"]:
		var path: String = "res://assets/faf/rocks/rock_001%s_reef_boulder.glb" % letter
		var packed: PackedScene = load(path) as PackedScene
		if packed == null:
			_fail(letter + " failed to import")
			return
		var asset: Node3D = packed.instantiate() as Node3D
		if asset == null:
			_fail(letter + " root is not Node3D")
			return
		root.add_child(asset)
		var meshes: Array[MeshInstance3D] = []
		_collect_meshes(asset, meshes)
		if meshes.size() != 1:
			_fail(letter + " expected one mesh, got " + str(meshes.size()))
			return
		var mesh_node: MeshInstance3D = meshes[0]
		if asset.transform != Transform3D.IDENTITY or mesh_node.transform != Transform3D.IDENTITY:
			_fail(letter + " has a non-identity import transform")
			return
		if mesh_node.mesh.get_surface_count() != 1:
			_fail(letter + " expected one material surface")
			return
		var material: Material = mesh_node.mesh.surface_get_material(0)
		if material == null or material.resource_name != "mat__rock_cool_slate":
			_fail(letter + " material did not import with FAF name")
			return
		var bounds: AABB = mesh_node.get_aabb()
		var expected_size: Vector3 = EXPECTED[letter]["size"]
		if bounds.size.distance_to(expected_size) > 0.02 or absf(bounds.position.y) > 0.02:
			_fail(letter + " dimensions/base mismatch: " + str(bounds))
			return
		var arrays: Array = mesh_node.mesh.surface_get_arrays(0)
		var indices: PackedInt32Array = arrays[Mesh.ARRAY_INDEX]
		var triangles: int = indices.size() / 3
		if triangles != EXPECTED[letter]["triangles"]:
			_fail(letter + " triangle count mismatch: " + str(triangles))
			return
		print("FAF_ROCK_001", letter.to_upper(), " root=", asset.name,
			" size=", bounds.size, " base_y=", bounds.position.y,
			" triangles=", triangles, " material=", material.resource_name)
		asset.queue_free()
	print("FAF_ROCK_001_OK")
	quit(0)


func _collect_meshes(node: Node, output: Array[MeshInstance3D]) -> void:
	if node is MeshInstance3D:
		output.append(node as MeshInstance3D)
	for child in node.get_children():
		_collect_meshes(child, output)


func _fail(message: String) -> void:
	printerr("FAF_ROCK_001_FAILED ", message)
	quit(1)
