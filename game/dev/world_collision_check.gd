extends SceneTree

func _initialize() -> void:
	call_deferred("_run")

func fail(reason: String) -> void:
	printerr("WORLD_COLLISION_FAILED ", reason)
	quit(1)

func mesh_bounds(node: Node, bounds: Array[AABB]) -> void:
	if node is MeshInstance3D and node.mesh != null and node.visible:
		bounds.append(node.global_transform * node.mesh.get_aabb())
	for child in node.get_children():
		mesh_bounds(child, bounds)

func mesh_points(node: Node, wrapper: Node3D, triangles: PackedVector3Array, points: PackedVector3Array) -> void:
	if node is MeshInstance3D and node.mesh != null and node.visible:
		var mesh_node: MeshInstance3D = node
		for surface in mesh_node.mesh.get_surface_count():
			if mesh_node.mesh.surface_get_primitive_type(surface) != Mesh.PRIMITIVE_TRIANGLES:
				continue
			var arrays := mesh_node.mesh.surface_get_arrays(surface)
			var vertices: PackedVector3Array = arrays[Mesh.ARRAY_VERTEX]
			var indices: PackedInt32Array = arrays[Mesh.ARRAY_INDEX] if arrays[Mesh.ARRAY_INDEX] != null else PackedInt32Array()
			for vertex in vertices:
				points.append(wrapper.to_local(mesh_node.to_global(vertex)))
			var count := indices.size() if not indices.is_empty() else vertices.size()
			for i in count:
				var index := indices[i] if not indices.is_empty() else i
				triangles.append(wrapper.to_local(mesh_node.to_global(vertices[index])))
	for child in node.get_children():
		mesh_points(child, wrapper, triangles, points)

func solids(node: Node, found: Array[StaticBody3D]) -> void:
	if node is StaticBody3D:
		found.append(node)
	for child in node.get_children():
		solids(child, found)

func all_shapes(node: Node, found: Array[CollisionShape3D], polygons: Array[CollisionPolygon3D]) -> void:
	if node is CollisionShape3D:
		found.append(node)
	if node is CollisionPolygon3D:
		polygons.append(node)
	for child in node.get_children():
		all_shapes(child, found, polygons)

func bounds_of(points: PackedVector3Array) -> AABB:
	var result := AABB(points[0], Vector3.ZERO)
	for point in points:
		result = result.expand(point)
	return result

func ray(space: PhysicsDirectSpaceState3D, start: Vector3, end: Vector3) -> Dictionary:
	var query := PhysicsRayQueryParameters3D.create(start, end)
	query.collision_mask = 1
	query.hit_back_faces = true
	return space.intersect_ray(query)

func _run() -> void:
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	for i in range(3):
		await physics_frame
	var geology: Node3D = main.get_node("Habitat/Composition/ReefComposition/Geology/Visuals")
	var all_solids: Array[StaticBody3D] = []
	solids(main, all_solids)
	if all_solids.size() != 41:
		fail("expected 38 geology pieces, draped terrain, seabed and limits; got " + str(all_solids.size()))
		return
	var shapes: Array[CollisionShape3D] = []
	var polygons: Array[CollisionPolygon3D] = []
	all_shapes(main, shapes, polygons)
	if shapes.size() != 46 or not polygons.is_empty():
		fail("unexpected shape/polygon inventory: " + str(shapes.size()) + "/" + str(polygons.size()))
		return
	for shape_node in shapes:
		if shape_node.shape == null or shape_node.disabled or not shape_node.get_parent() is PhysicsBody3D:
			fail("detached or disabled collision shape: " + str(shape_node.get_path()))
			return
	var resources := {}
	for wrapper in geology.get_children():
		var body: StaticBody3D = wrapper.get_node_or_null("Solid")
		if body == null or body.collision_layer != 1 or body.collision_mask != 0 or body.transform != Transform3D.IDENTITY:
			fail("solid owner/layer mismatch: " + wrapper.name)
			return
		var collision: CollisionShape3D = body.get_node_or_null("Shape")
		if collision == null or collision.disabled or collision.shape == null or collision.transform != Transform3D.IDENTITY:
			fail("shape missing, disabled or offset: " + wrapper.name)
			return
		var id: String = wrapper.get_meta("faf_id", "")
		var shape: Shape3D = collision.shape
		var vertices: PackedVector3Array
		if shape is ConvexPolygonShape3D:
			vertices = (shape as ConvexPolygonShape3D).points
		elif shape is ConcavePolygonShape3D:
			vertices = (shape as ConcavePolygonShape3D).data
		else:
			fail("wrong shape type: " + wrapper.name)
			return
		if vertices.is_empty() or shape.resource_path.is_empty():
			fail("unprepared shape: " + wrapper.name)
			return
		var expected_triangles := PackedVector3Array()
		var expected_points := PackedVector3Array()
		for child in wrapper.get_children():
			if child != body:
				mesh_points(child, wrapper, expected_triangles, expected_points)
		var expected: PackedVector3Array = expected_points if shape is ConvexPolygonShape3D else expected_triangles
		if vertices.size() != expected.size():
			fail("prepared shape/source vertex count differs: " + wrapper.name)
			return
		for index in vertices.size():
			if vertices[index].distance_to(expected[index]) > 0.001:
				fail("prepared shape/source vertex differs: " + wrapper.name + " index=" + str(index))
				return
		resources[shape.resource_path] = true
		var collider_bounds := collision.global_transform * bounds_of(vertices)
		var model_bounds: Array[AABB] = []
		mesh_bounds(wrapper, model_bounds)
		if model_bounds.is_empty():
			fail("no source mesh: " + wrapper.name)
			return
		var visual_bounds := model_bounds[0]
		for box in model_bounds.slice(1):
			visual_bounds = visual_bounds.merge(box)
		for axis in 3:
			if absf(collider_bounds.position[axis] - visual_bounds.position[axis]) > 0.05 or absf(collider_bounds.end[axis] - visual_bounds.end[axis]) > 0.05:
				fail("world bounds disagree on " + wrapper.name + " axis=" + str(axis) + " visual=" + str(visual_bounds) + " collision=" + str(collider_bounds))
				return
		if id.begins_with("FAF-ROCK") != (shape is ConvexPolygonShape3D):
			fail("wrong solid family: " + wrapper.name)
			return
	if resources.size() != 7:
		fail("unexpected prepared resource reuse count: " + str(resources.size()))
		return
	var terrain: Node3D = main.get_node("Habitat/Composition/ReefComposition/Terrain")
	var terrain_body: StaticBody3D = terrain.get_node_or_null("DrapedTerrainSolid")
	if terrain_body == null or terrain_body.collision_layer != 1 or terrain_body.collision_mask != 0 or terrain_body.transform != Transform3D.IDENTITY:
		fail("draped terrain body missing or offset")
		return
	var terrain_shape: CollisionShape3D = terrain_body.get_node_or_null("Shape")
	if terrain_shape == null or terrain_shape.disabled or terrain_shape.transform != Transform3D.IDENTITY or not terrain_shape.shape is ConcavePolygonShape3D or terrain_shape.shape.resource_path != "res://assets/faf/collision/faf_draped_terrain.res":
		fail("prepared draped terrain shape missing")
		return
	var expected_terrain := PackedVector3Array()
	var unused := PackedVector3Array()
	mesh_points(terrain.get_node("Visuals"), terrain, expected_terrain, unused)
	var actual_terrain: PackedVector3Array = (terrain_shape.shape as ConcavePolygonShape3D).data
	if expected_terrain.size() != actual_terrain.size():
		fail("draped terrain triangle count changed")
		return
	for index in actual_terrain.size():
		if actual_terrain[index].distance_to(expected_terrain[index]) > 0.001:
			fail("draped terrain/source triangle differs at " + str(index))
			return
	var fish: CharacterBody3D = main.get_node("Fish")
	var fish_shape: CollisionShape3D = fish.get_node("Collision")
	if fish.collision_layer != 1 or fish.collision_mask != 1 or not fish_shape.shape is SphereShape3D or absf((fish_shape.shape as SphereShape3D).radius - 0.48) > 0.001:
		fail("accepted fish collision envelope changed")
		return
	var seabed: StaticBody3D = main.get_node("Habitat/Seabed")
	if not seabed.get_node("Collision").shape is ConcavePolygonShape3D or seabed.get_node("Mesh").mesh == null:
		fail("mesh-aligned seabed collision missing")
		return
	var limits: StaticBody3D = main.get_node("Habitat/HiddenLimits")
	if limits.get_child_count() != 5:
		fail("hidden boundary count changed")
		return
	for name in ["West", "East", "North", "South", "Upper"]:
		var wall: CollisionShape3D = limits.get_node(name)
		if wall.disabled or not wall.shape is BoxShape3D:
			fail("boundary disabled: " + name)
			return
	var space := main.get_world_3d().direct_space_state
	var floor_hit := ray(space, Vector3.ZERO, Vector3(0, -15, 0))
	if floor_hit.get("collider") != seabed and floor_hit.get("collider") != terrain_body:
		fail("visible terrain/seabed floor ray missed")
		return
	for pair in [[Vector3(-35, 8.5, 0), Vector3(-40, 8.5, 0), limits],
		[Vector3(35, 8.5, 0), Vector3(40, 8.5, 0), limits],
		[Vector3(0, 8.5, -35), Vector3(0, 8.5, -40), limits],
		[Vector3(0, 8.5, 35), Vector3(0, 8.5, 40), limits],
		[Vector3(0, 8.5, 0), Vector3(0, 11, 0), limits]]:
		if ray(space, pair[0], pair[1]).get("collider") != pair[2]:
			fail("terrain or boundary ray missed: " + str(pair[0]))
			return
	print("WORLD_COLLISION_OK static=41 shapes=46 polygons=0 geology=38 terrain_triangles=58752 prepared_variants=8 p2_orphans=0 seabed=solid limits=5 fish_radius=0.48 geometry=aligned")
	quit()
