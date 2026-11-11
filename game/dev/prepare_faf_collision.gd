@tool
extends SceneTree

# Run after importing FAF assets: godot --headless --path game --script res://dev/prepare_faf_collision.gd
# Shapes are source-local; the placement wrapper supplies the same transform to art and physics.
const SOURCES := {
	"FAF-TERRAIN-001A": "res://assets/faf/terrain/terrain_001a_open_sand.glb",
	"FAF-TERRAIN-001B": "res://assets/faf/terrain/terrain_001b_rolling.glb",
	"FAF-TERRAIN-001C": "res://assets/faf/terrain/terrain_001c_reef_shelf.glb",
	"FAF-TERRAIN-001D": "res://assets/faf/terrain/terrain_001d_slope.glb",
	"FAF-TERRAIN-001E": "res://assets/faf/terrain/terrain_001e_drop_off.glb",
	"FAF-TERRAIN-001F": "res://assets/faf/terrain/terrain_001f_rocky_floor.glb",
	"FAF-ROCK-001A": "res://assets/faf/rocks/rock_001a_reef_boulder.glb",
	"FAF-ROCK-001B": "res://assets/faf/rocks/rock_001b_reef_boulder.glb",
	"FAF-ROCK-001C": "res://assets/faf/rocks/rock_001c_reef_boulder.glb",
	"FAF-ROCK-001D": "res://assets/faf/rocks/rock_001d_reef_boulder.glb",
}
const OUTPUT := "res://assets/faf/collision/"

func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(OUTPUT))
	for id in SOURCES:
		var scene: PackedScene = load(SOURCES[id])
		if scene == null:
			printerr("COLLISION_PREP_FAILED missing source ", id)
			quit(1)
			return
		var model: Node3D = scene.instantiate()
		var triangles := PackedVector3Array()
		var points := PackedVector3Array()
		collect(model, Transform3D.IDENTITY, triangles, points)
		model.free()
		if triangles.is_empty():
			printerr("COLLISION_PREP_FAILED empty mesh ", id)
			quit(1)
			return
		var shape: Shape3D
		if id.begins_with("FAF-ROCK"):
			var hull := ConvexPolygonShape3D.new()
			hull.points = points
			shape = hull
		else:
			var surface := ConcavePolygonShape3D.new()
			surface.data = triangles
			surface.backface_collision = true
			shape = surface
		shape.resource_name = id + " imported-mesh collision"
		var path: String = OUTPUT + str(id).to_lower().replace("-", "_") + ".tres"
		if ResourceSaver.save(shape, path) != OK:
			printerr("COLLISION_PREP_FAILED save ", path)
			quit(1)
			return
		print("COLLISION_PREP_OK id=", id, " triangles=", triangles.size() / 3, " points=", points.size(), " path=", path)
	quit()

func collect(node: Node, parent_transform: Transform3D, triangles: PackedVector3Array, points: PackedVector3Array) -> void:
	var transform := parent_transform
	if node is Node3D:
		transform = parent_transform * (node as Node3D).transform
	if node is MeshInstance3D and node.mesh != null and node.visible:
		var mesh: Mesh = node.mesh
		for surface in mesh.get_surface_count():
			if mesh.surface_get_primitive_type(surface) != Mesh.PRIMITIVE_TRIANGLES:
				continue
			var arrays := mesh.surface_get_arrays(surface)
			var vertices: PackedVector3Array = arrays[Mesh.ARRAY_VERTEX]
			var indices: PackedInt32Array = arrays[Mesh.ARRAY_INDEX]
			for vertex in vertices:
				points.append(transform * vertex)
			var count := indices.size() if not indices.is_empty() else vertices.size()
			for i in range(count):
				var index := indices[i] if not indices.is_empty() else i
				triangles.append(transform * vertices[index])
	for child in node.get_children():
		collect(child, transform, triangles, points)
