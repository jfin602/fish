extends SceneTree

const FISH_RADIUS := 0.48
const CAMERA_RADIUS := 0.22
const CAMERA_OFFSET := Vector3.UP * 2.0

func _initialize() -> void:
	call_deferred("_run")

func _fail(reason: String) -> void:
	printerr("ROUTE_BAKED_FAILED ", reason)
	quit(1)

func _vec(values: Array) -> Vector3:
	return Vector3(float(values[0]), float(values[1]), float(values[2]))

func _run() -> void:
	var package := ""
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--world-package="):
			package = argument.trim_prefix("--world-package=")
	if package.is_empty():
		_fail("missing world package")
		return
	var data: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(package.path_join("world-data.json")))
	var manifest: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(package.path_join("manifest.json")))
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	for frame in range(3):
		await physics_frame
	var fish: CharacterBody3D = main.get_node("Fish")
	fish.set_physics_process(false)
	var nodes := {}
	for node in data["Nodes"]:
		nodes[node["Id"]] = _vec(node["Position"])
	if nodes.size() != 31 or data["Edges"].size() != 37:
		_fail("route inventory")
		return
	var connected := {"spawn": true}
	var pending := ["spawn"]
	while not pending.is_empty():
		var current: String = pending.pop_front()
		for edge in data["Edges"]:
			var next_id: String = edge["To"] if edge["From"] == current else edge["From"] if edge["To"] == current else ""
			if not next_id.is_empty() and not connected.has(next_id):
				connected[next_id] = true
				pending.append(next_id)
	if connected.size() != nodes.size():
		_fail("graph disconnected")
		return
	var space := main.get_world_3d().direct_space_state
	var shape := SphereShape3D.new()
	var query := PhysicsShapeQueryParameters3D.new()
	query.shape = shape
	query.collision_mask = 1
	query.exclude = [fish.get_rid()]
	var probes := 0
	for edge in data["Edges"]:
		var points: Array = edge["Points"]
		if _vec(points[0]).distance_to(nodes[edge["From"]]) > 0.001 or _vec(points[-1]).distance_to(nodes[edge["To"]]) > 0.001:
			_fail("edge endpoints " + edge["Id"])
			return
		if float(edge["ProtectedRadius"]) < FISH_RADIUS or float(edge["CameraProtectedRadius"]) < CAMERA_RADIUS:
			_fail("route reservation " + edge["Id"])
			return
		for segment in range(1, points.size()):
			var a := _vec(points[segment - 1])
			var b := _vec(points[segment])
			var steps := maxi(1, ceili(a.distance_to(b) / 0.3))
			for step in range(steps + 1):
				var at := a.lerp(b, float(step) / steps)
				for sample in [[at, FISH_RADIUS], [at + CAMERA_OFFSET, CAMERA_RADIUS]]:
					shape.radius = sample[1]
					query.transform = Transform3D(Basis.IDENTITY, sample[0])
					var hits := space.intersect_shape(query, 1)
					if not hits.is_empty():
						_fail("blocked %s at %s by %s" % [edge["Id"], sample[0], hits[0].collider.get_path()])
						return
				probes += 1
	var base := "Habitat/Composition/ReefComposition/Geology/GeneratedRegions/"
	for region in ["west-swim-through", "east-swim-through", "cave-chamber", "stone-arch", "canyon-ridge"]:
		var line: Node3D = main.get_node(base + region + "/Centerline")
		var route: Dictionary
		for edge in data["Edges"]:
			if edge["Id"] == region:
				route = edge
		if route.is_empty() or line.get_child_count() != route["Points"].size():
			_fail("missing passage " + region)
			return
		for i in range(line.get_child_count()):
			if line.get_child(i).global_position.distance_to(_vec(route["Points"][i])) > 0.001:
				_fail("passage centerline mismatch " + region)
				return
	print("ROUTE_BAKED_OK seed=%d nodes=%d edges=%d clear_probes=%d geology=5 connected=true" % [manifest["Seed"], nodes.size(), data["Edges"].size(), probes])
	quit()
