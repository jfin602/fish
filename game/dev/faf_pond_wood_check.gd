extends SceneTree

const MANIFEST := "res://../assets/faf/pond/wood/pond_wood_001_manifest.json"
const WRAPPERS := "res://scenes/faf_pond_wood/"


func _initialize() -> void:
	_run.call_deferred()


func _fail(message: String) -> void:
	printerr("FAF_POND_WOOD_CHECK_FAILED ",message)
	quit(1)


func _meshes(node: Node, out: Array[MeshInstance3D]) -> void:
	if node is MeshInstance3D: out.append(node)
	for child in node.get_children(): _meshes(child,out)


func _run() -> void:
	var file := FileAccess.open(ProjectSettings.globalize_path(MANIFEST),FileAccess.READ)
	if file == null:
		_fail("manifest missing");return
	var data: Dictionary = JSON.parse_string(file.get_as_text())
	var assets: Array = data.get("assets",[])
	if assets.size() != 12:
		_fail("inventory != 12");return
	var ids := {}
	var total_triangles := 0
	var floor := Node3D.new()
	root.add_child(floor)
	current_scene = floor
	for entry_variant in assets:
		var entry: Dictionary = entry_variant
		var id := String(entry["faf_id"])
		if ids.has(id) or entry.get("habitat") != "pond":
			_fail("duplicate ID or habitat missing " + id);return
		ids[id] = true
		var path := WRAPPERS + String(entry["stem"]) + ".tscn"
		var packed := load(path) as PackedScene
		if packed == null:
			_fail("wrapper missing " + path);return
		var piece := packed.instantiate() as Node3D
		floor.add_child(piece)
		await physics_frame
		if piece.get_meta("faf_id","") != id or piece.get_meta("faf_habitat","") != "pond":
			_fail("runtime metadata " + id);return
		var mesh_nodes: Array[MeshInstance3D] = []
		_meshes(piece,mesh_nodes)
		var triangles := 0
		var materials := 0
		var all_bounds := AABB()
		for mesh_node in mesh_nodes:
			if mesh_node.mesh == null: continue
			all_bounds = mesh_node.mesh.get_aabb()
			for surface in range(mesh_node.mesh.get_surface_count()):
				var arrays: Array = mesh_node.mesh.surface_get_arrays(surface)
				var indices: PackedInt32Array = arrays[Mesh.ARRAY_INDEX]
				triangles += indices.size()/3
				if mesh_node.mesh.surface_get_material(surface) != null: materials += 1
		if triangles != int(entry["triangle_count"]) or materials != int(entry["material_count"]):
			_fail("import geometry/material mismatch %s source=%d/%d imported=%d/%d" % [id,int(entry["triangle_count"]),int(entry["material_count"]),triangles,materials]);return
		var dimensions: Array = entry["dimensions_m"]
		var expected_bounds := Vector3(float(dimensions[0]),float(dimensions[2]),float(dimensions[1]))
		if all_bounds.size.distance_to(expected_bounds) > .015 or absf(all_bounds.position.y) > .4 or piece.scale != Vector3.ONE:
			_fail("import scale/orientation/contact pivot mismatch " + id);return
		var body: StaticBody3D = piece.get_node_or_null("SimpleWoodCollision")
		var family := String(entry["family"])
		if family == "cluster":
			if body != null:
				_fail("decorative twig collision " + id);return
		else:
			if body == null or body.collision_layer != 1 or body.collision_mask != 0:
				_fail("solid collision missing " + id);return
			var expected := (16 if id == "FAF-WOOD-002A" else 12) if family == "hollow_trunk" else (1+int(entry["generator_config"].get("forks",0)))
			if body.get_child_count() != expected:
				_fail("collision count %s got %d" % [id,body.get_child_count()]);return
			for shape_node in body.get_children():
				if not shape_node is CollisionShape3D or shape_node.shape == null or shape_node.disabled:
					_fail("collision child invalid " + id);return
				if family == "hollow_trunk" and not shape_node.shape is BoxShape3D:
					_fail("hollow has non-segmented collision " + id);return
		if family == "hollow_trunk":
			var radius := float(entry["generator_config"]["radius"])
			var inner := float(entry["generator_config"]["inner"])
			var length := float(entry["generator_config"]["length"])
			var sphere := SphereShape3D.new();sphere.radius = .48
			for step in range(21):
				var z := lerpf(-length*.60,length*.60,float(step)/20)
				var query := PhysicsShapeQueryParameters3D.new()
				query.shape = sphere
				query.transform = Transform3D(Basis.IDENTITY,Vector3(0,radius,z))
				query.collision_mask = 1
				if not floor.get_world_3d().direct_space_state.intersect_shape(query,1).is_empty():
					_fail("fish centerline blocked %s at %f" % [id,z]);return
			# Both the opening and the near wall must be physically honest.
			for contact in [Vector3(inner*.93-.12,radius,0),
				Vector3(0,radius+inner*.93-.12,0),
				Vector3(0,radius-inner*.93+.12,0)]:
				var side_query := PhysicsShapeQueryParameters3D.new()
				side_query.shape = sphere
				side_query.transform = Transform3D(Basis.IDENTITY,contact)
				side_query.collision_mask = 1
				if floor.get_world_3d().direct_space_state.intersect_shape(side_query,1).is_empty():
					_fail("hollow wall/floor/roof contact missing %s at %s" % [id,contact]);return
		total_triangles += triangles
		print("FAF_POND_WOOD_IMPORTED id=",id," triangles=",triangles," materials=",materials,
			" shapes=",0 if body == null else body.get_child_count()," bounds=",all_bounds.size)
		piece.queue_free()
		await physics_frame
	print("FAF_POND_WOOD_CHECK_OK assets=",assets.size()," unique_ids=",ids.size(),
		" imported_triangles=",total_triangles)
	quit(0)
