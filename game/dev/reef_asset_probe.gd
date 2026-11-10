extends SceneTree

const SOURCES := {
	"Rock Face 01": "res://assets/vendor-local/rock_face_01/rock_face_01_2k.gltf",
	"Rock Face 02": "res://assets/vendor-local/p2-environment/polyhaven/rock_face_02/rock_face_02_2k.gltf",
	"Coastal Cliff 01": "res://assets/vendor-local/p2-environment/polyhaven/coastal_cliff_01/coastal_cliff_01_2k.gltf",
	"Boulder 01": "res://assets/vendor-local/p2-environment/polyhaven/boulder_01/boulder_01_2k.gltf",
	"Heliopora": "res://assets/vendor-local/heliopora-coerulea.glb",
	"Cervicornis": "res://assets/vendor-local/p2-environment/smithsonian/acropora-cervicornis/Acropora_cervicornis-150k-4096_std.glb",
	"Palmata": "res://assets/vendor-local/p2-environment/smithsonian/acropora-palmata/usnm_3968-100k-2048_std.glb",
	"Meandrina": "res://assets/vendor-local/p2-environment/smithsonian/meandrina-meandrites/usnm_1174917-100k-2048_std.glb",
	"Montipora": "res://assets/vendor-local/p2-environment/smithsonian/montipora-danae/usnm_307-100k-2048_std.glb",
}

func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	for label in SOURCES:
		var packed: PackedScene = load(SOURCES[label])
		if packed == null:
			printerr("REEF_ASSET_PROBE_MISSING ", label)
			quit(1)
			return
		var scene: Node3D = packed.instantiate()
		root.add_child(scene)
		var meshes: Array[MeshInstance3D] = []
		_collect(scene, meshes)
		var bounds := AABB()
		var first := true
		var triangles := 0
		for node in meshes:
			if node.mesh == null:
				continue
			var local_bounds: AABB = scene.global_transform.affine_inverse() * node.global_transform * node.mesh.get_aabb()
			bounds = local_bounds if first else bounds.merge(local_bounds)
			first = false
			for surface in range(node.mesh.get_surface_count()):
				var arrays: Array = node.mesh.surface_get_arrays(surface)
				var indices: PackedInt32Array = arrays[Mesh.ARRAY_INDEX]
				var vertices: PackedVector3Array = arrays[Mesh.ARRAY_VERTEX]
				triangles += (indices.size() if not indices.is_empty() else vertices.size()) / 3
		print("REEF_ASSET_PROBE name=%s meshes=%d triangles=%d bounds=%s" % [label, meshes.size(), triangles, bounds])
		scene.queue_free()
	quit()

func _collect(node: Node, found: Array[MeshInstance3D]) -> void:
	if node is MeshInstance3D:
		found.append(node as MeshInstance3D)
	for child in node.get_children():
		_collect(child, found)
