@tool
extends SceneTree

# Regenerate after terrain layout, drape, or imported mesh changes.
# godot --headless --path game --script res://dev/prepare_faf_terrain_collision.gd -- --prepare-faf-terrain-collision
const OUTPUT := "res://assets/faf/collision/faf_draped_terrain.res"

func _initialize() -> void:
	call_deferred("_run")

func collect(node: Node, terrain: Node3D, points: PackedVector3Array) -> void:
	if node is MeshInstance3D and node.mesh != null and node.visible:
		var mesh_node: MeshInstance3D = node
		for surface in mesh_node.mesh.get_surface_count():
			if mesh_node.mesh.surface_get_primitive_type(surface) != Mesh.PRIMITIVE_TRIANGLES:
				continue
			var arrays := mesh_node.mesh.surface_get_arrays(surface)
			var vertices: PackedVector3Array = arrays[Mesh.ARRAY_VERTEX]
			var indices: PackedInt32Array = arrays[Mesh.ARRAY_INDEX] if arrays[Mesh.ARRAY_INDEX] != null else PackedInt32Array()
			var count := indices.size() if not indices.is_empty() else vertices.size()
			for i in count:
				var index := indices[i] if not indices.is_empty() else i
				points.append(terrain.to_local(mesh_node.to_global(vertices[index])))
	for child in node.get_children():
		collect(child, terrain, points)

func _run() -> void:
	if not OS.get_cmdline_user_args().has("--prepare-faf-terrain-collision"):
		printerr("TERRAIN_COLLISION_PREP_FAILED required preparation flag missing")
		quit(2)
		return
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	var terrain: Node3D = main.get_node("Habitat/Composition/ReefComposition/Terrain")
	var points := PackedVector3Array()
	collect(terrain.get_node("Visuals"), terrain, points)
	if points.size() < 100000 or points.size() % 3 != 0:
		printerr("TERRAIN_COLLISION_PREP_FAILED invalid draped triangles: ", points.size())
		quit(1)
		return
	var shape := ConcavePolygonShape3D.new()
	shape.data = points
	shape.backface_collision = true
	shape.resource_name = "FAF draped terrain collision"
	if ResourceSaver.save(shape, OUTPUT) != OK:
		printerr("TERRAIN_COLLISION_PREP_FAILED save ", OUTPUT)
		quit(1)
		return
	print("TERRAIN_COLLISION_PREP_OK triangles=", points.size() / 3, " path=", OUTPUT)
	quit()
