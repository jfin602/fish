extends SceneTree

const MANIFEST := "res://../assets/faf/pond/ground/pond_ground_001_manifest.json"
const ROOT := "res://assets/faf/pond/ground/"
const MATERIALS := "res://assets/faf/pond/materials/"

func _initialize() -> void:
	call_deferred("_check")


func _check() -> void:
	var file := FileAccess.open(ProjectSettings.globalize_path(MANIFEST), FileAccess.READ)
	assert(file != null)
	var data: Dictionary = JSON.parse_string(file.get_as_text())
	var assets: Array = data["assets"]
	assert(assets.size() == 8)
	var ids := {}
	var total_triangles := 0
	for role in ["silt","mud","wet_bank","algae_stone"]:
		var shared := load(MATERIALS + "pond_%s.tres" % role) as StandardMaterial3D
		assert(shared != null)
		assert(shared.roughness >= .9)
	for entry in assets:
		var id: String = entry["faf_id"]
		assert(not ids.has(id))
		ids[id] = true
		assert(entry["habitat"] == "pond")
		assert(float(entry["dimensions_m"][2]) > 0)
		var path: String = ROOT + String(entry["stem"]) + ".glb"
		var packed := load(path) as PackedScene
		assert(packed != null)
		var root := packed.instantiate()
		var meshes := root.find_children("*","MeshInstance3D",true,false)
		assert(meshes.size() == 1)
		var item := meshes[0] as MeshInstance3D
		var triangles := 0
		var material_names := {}
		for surface in item.mesh.get_surface_count():
			var arrays := item.mesh.surface_get_arrays(surface)
			var indices: PackedInt32Array = arrays[Mesh.ARRAY_INDEX]
			var vertices: PackedVector3Array = arrays[Mesh.ARRAY_VERTEX]
			triangles += (indices.size() if indices.size() > 0 else vertices.size()) / 3
			var material := item.mesh.surface_get_material(surface)
			assert(material != null)
			material_names[material.resource_name] = true
		assert(triangles == int(entry["triangle_count"]))
		assert(material_names.size() == int(entry["material_count"]))
		assert(item.scale.is_equal_approx(Vector3.ONE))
		var size: Vector3 = item.mesh.get_aabb().size
		assert(size.x > 0 and size.y > 0 and size.z > 0)
		total_triangles += triangles
		print("FAF_POND_GROUND_IMPORT id=",id," triangles=",triangles,
			" materials=",material_names.size()," imported_mesh_aabb=",size)
		root.free()
	print("FAF_POND_GROUND_CHECK_OK assets=",ids.size()," unique_ids=",ids.size(),
		" source_triangles=",total_triangles," shared_materials=4")
	quit(0)
