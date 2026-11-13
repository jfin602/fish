extends SceneTree

const MANIFEST := "res://../assets/faf/pond/wood/pond_wood_002_manifest.json"


func _initialize() -> void:
	_run.call_deferred()


func _meshes(node: Node,out: Array[MeshInstance3D]) -> void:
	if node is MeshInstance3D: out.append(node)
	for child in node.get_children(): _meshes(child,out)


func _fail(message: String) -> void:
	printerr("POND_WOOD_002_GODOT_FAIL ",message)
	quit(1)


func _run() -> void:
	var file := FileAccess.open(ProjectSettings.globalize_path(MANIFEST),FileAccess.READ)
	if file == null: _fail("manifest missing");return
	var data: Dictionary = JSON.parse_string(file.get_as_text())
	var items: Array = data["assets"]
	if items.size()!=10: _fail("inventory != 10");return
	var scene := Node3D.new()
	root.add_child(scene)
	current_scene=scene
	var total := 0
	for entry_variant in items:
		var entry: Dictionary=entry_variant
		var id := String(entry["faf_id"])
		var path := "res://scenes/faf_pond_wood_002/%s.tscn" % entry["stem"]
		var packed := load(path) as PackedScene
		if packed == null: _fail("wrapper missing "+id);return
		var piece := packed.instantiate() as Node3D
		scene.add_child(piece)
		await physics_frame
		if piece.get_meta("faf_id","")!=id or piece.get_meta("faf_habitat","")!="pond":
			_fail("identity " + id);return
		var body := piece.get_node_or_null("AuthoredWoodCollision") as StaticBody3D
		if body==null or body.get_child_count()!=int(entry["collision_count"]):
			_fail("collision count "+id);return
		for child in body.get_children():
			if not child is CollisionShape3D or child.shape==null or child.disabled:
				_fail("bad shape "+id);return
		var meshes: Array[MeshInstance3D]=[]
		_meshes(piece,meshes)
		if meshes.size()!=1: _fail("mesh count "+id);return
		var mesh := meshes[0].mesh
		var tris := 0
		var mats := 0
		for surface in mesh.get_surface_count():
			var indices: PackedInt32Array=mesh.surface_get_arrays(surface)[Mesh.ARRAY_INDEX]
			tris+=indices.size()/3
			if mesh.surface_get_material(surface)!=null: mats+=1
		var dims: Array=entry["dimensions_m"]
		var expected := Vector3(float(dims[0]),float(dims[2]),float(dims[1]))
		if tris!=int(entry["triangle_count"]) or mats!=int(entry["material_count"]) or mesh.get_aabb().size.distance_to(expected)>.025:
			_fail("import cost/bounds "+id);return
		if entry["family"]=="split_hollow_log":
			var sphere := SphereShape3D.new();sphere.radius=.48
			var center := float(entry["generator_config"]["outer"])
			var length := float(entry["generator_config"]["length"])
			for j in range(13):
				var z := lerpf(-length*.55,length*.55,float(j)/12)
				if _blocked(scene,sphere,Vector3(0,center,z)):
					_fail("blocked hollow centerline %s z=%f" % [id,z]);return
			for x in [0.0,center*.6,center*1.2]:
				if _blocked(scene,sphere,Vector3(x,center,0)):
					_fail("blocked side entry %s x=%f" % [id,x]);return
		total+=tris
		print("POND_WOOD_002_IMPORT id=",id," tris=",tris," materials=",mats," shapes=",body.get_child_count())
		piece.queue_free()
		await physics_frame
	print("POND_WOOD_002_GODOT_OK assets=10 triangles=",total)
	quit(0)


func _blocked(scene: Node3D,sphere: SphereShape3D,at: Vector3) -> bool:
	var query := PhysicsShapeQueryParameters3D.new()
	query.shape=sphere
	query.transform=Transform3D(Basis.IDENTITY,at)
	query.collision_mask=1
	return not scene.get_world_3d().direct_space_state.intersect_shape(query,1).is_empty()
