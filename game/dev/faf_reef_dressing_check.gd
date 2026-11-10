extends SceneTree

const ZONES := ["shallow", "upper", "middle", "lower", "deep"]
const EXPECTED := [128, 84, 54, 20, 9]

func _initialize() -> void:
	call_deferred("_run")

func fail(message: String) -> void:
	printerr("FAF_DRESSING_FAILED ", message)
	quit(1)

func distance_to_segment(point: Vector2, a: Vector2, b: Vector2) -> float:
	var ab := b - a
	var fraction := clampf((point - a).dot(ab) / ab.length_squared(), 0.0, 1.0)
	return point.distance_to(a + fraction * ab)

func marker(routes: Node3D, name: String) -> Vector2:
	var at := (routes.get_node(name) as Marker3D).global_position
	return Vector2(at.x, at.z)

func add_path(segments: Array[Dictionary], routes: Node3D, names: Array[String], radius: float) -> void:
	for i in range(names.size() - 1):
		segments.append({"a": marker(routes, names[i]), "b": marker(routes, names[i + 1]),
			"radius": radius, "name": names[i] + "-" + names[i + 1]})

func _run() -> void:
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	for i in range(3):
		await physics_frame
	var reef: Node3D = main.get_node("Habitat/Composition/ReefComposition")
	var dressing: Node3D = reef.get_node("Dressing")
	var visuals: Node3D = dressing.get_node("Visuals")
	var terrain: Node3D = reef.get_node("Terrain")
	var seabed: Node3D = main.get_node("Habitat/Seabed")
	var p2: Node3D = reef.get_node("Routes")
	var geo: Node3D = reef.get_node("Geology/Routes")
	var segments: Array[Dictionary] = []
	add_path(segments, p2, ["DepthShallow", "DepthUpper", "DepthMiddle", "DepthLower", "DepthDeep"], 1.5)
	add_path(segments, p2, ["AltShallow", "AltUpper", "AltMiddle", "AltLower", "AltDeep"], 1.5)
	add_path(segments, p2, ["TunnelEntry", "TunnelCenter", "TunnelExit"], 1.5)
	add_path(segments, p2, ["ShelterEntry", "ShelterCenter", "ShelterExit"], 1.5)
	add_path(segments, geo, ["CaveTurn", "CaveSide"], 2.0)
	segments.append({"a": marker(p2, "ShelterCenter"), "b": marker(geo, "CaveTurn"),
		"radius": 1.5, "name": "ShelterCenter-CaveTurn"})
	segments.append({"a": marker(geo, "CaveTurn"), "b": marker(p2, "ShelterExit"),
		"radius": 1.5, "name": "CaveTurn-ShelterExit"})
	add_path(segments, geo, ["CanyonEntry", "CanyonCenter", "CanyonExit"], 1.5)
	add_path(segments, geo, ["CanyonExit", "CanyonReturn"], 1.5)
	segments.append({"a": marker(geo, "CanyonReturn"), "b": marker(p2, "DepthDeep"),
		"radius": 1.5, "name": "CanyonReturn-DepthDeep"})
	var counts := [0, 0, 0, 0, 0]
	var ids := {}
	var positions_by_id := {}
	var clusters := {}
	var family_counts := {"CORAL": 0, "PLANT": 0, "DETAIL": 0}
	var max_edge_gap := 0.0
	if dressing.PlacementCount != 295 or dressing.ClusterCount != 28 or visuals.get_child_count() != 28:
		fail("bounded count or batch count mismatch")
		return
	for index in range(dressing.PlacementCount):
		var id: String = dressing.GetPlacementId(index)
		var zone: String = dressing.GetPlacementZone(index)
		var cluster: String = dressing.GetPlacementCluster(index)
		var at: Vector3 = dressing.GetPlacementPosition(index)
		var radius: float = dressing.GetPlacementRadius(index)
		var zone_index := ZONES.find(zone)
		if zone_index < 0 or radius < 0.04 or radius > 1.1:
			fail("invalid zone or asset footprint " + id)
			return
		counts[zone_index] += 1
		ids[id] = ids.get(id, 0) + 1
		if not positions_by_id.has(id):
			positions_by_id[id] = []
		positions_by_id[id].append(at)
		clusters[cluster] = true
		var family: String = id.split("-")[1]
		family_counts[family] += 1
		if zone == "deep" and family == "PLANT":
			fail("tall vegetation in deep geological zone")
			return
		var surface: float = terrain.GetVisibleHeightAt(at.x, at.z, seabed)
		if absf(at.y - surface - 0.015) > 0.012:
			fail("floating/buried centre " + id + " at " + str(at))
			return
		for offset in [Vector2(radius * 0.5, 0), Vector2(-radius * 0.5, 0),
			Vector2(0, radius * 0.5), Vector2(0, -radius * 0.5)]:
			var edge: float = terrain.GetVisibleHeightAt(at.x + offset.x, at.z + offset.y, seabed)
			max_edge_gap = maxf(max_edge_gap, absf(edge - surface))
			if absf(edge - surface) > 0.45:
				fail("floating/buried footprint " + id + " at " + str(at))
				return
		var p := Vector2(at.x, at.z)
		for segment in segments:
			if distance_to_segment(p, segment.a, segment.b) - radius < segment.radius:
				fail("route exclusion " + id + " at " + str(at) + " near " + segment.name)
				return
	for i in range(ZONES.size()):
		if counts[i] != EXPECTED[i]:
			fail("zone count " + ZONES[i] + " expected " + str(EXPECTED[i]) + " got " + str(counts[i]))
			return
	if ids.size() != 28 or clusters.size() != 28 or family_counts != {"CORAL": 126, "PLANT": 90, "DETAIL": 79}:
		fail("coverage/cluster/family count " + str(family_counts) + " ids=" + str(ids.size()))
		return
	for batch in visuals.get_children():
		var id: String = batch.get_meta("faf_id", "")
		if not batch is MultiMeshInstance3D or not ids.has(id) or batch.multimesh == null:
			fail("invalid visual batch " + id)
			return
		if batch.multimesh.instance_count != ids[id] or batch.get_meta("placement_count") != ids[id]:
			fail("batch transform count mismatch " + id)
			return
		if batch.multimesh.mesh == null or batch is CollisionObject3D:
			fail("missing mesh or unexpected physics " + id)
			return
		# Godot's headless dummy renderer returns identity for every MultiMesh
		# transform, including a one-box probe. Native P7 can read back positions.
		if DisplayServer.get_name() != "headless":
			var remaining: Array = positions_by_id[id].duplicate()
			for instance in range(batch.multimesh.instance_count):
				var actual: Vector3 = batch.multimesh.get_instance_transform(instance).origin
				var match_index := -1
				for candidate in range(remaining.size()):
					if actual.distance_to(remaining[candidate]) < 0.001:
						match_index = candidate
						break
				if match_index < 0:
					fail("untracked batch transform " + id + " " + str(actual))
					return
				remaining.remove_at(match_index)
	print("FAF_DRESSING_OK variants=%d clusters=%d placements=%d zones=%s families=%s max_footprint_delta=%.3f batches=%d transform_readback=%s" %
		[ids.size(), clusters.size(), dressing.PlacementCount, counts, family_counts, max_edge_gap,
			visuals.get_child_count(), "headless-unavailable" if DisplayServer.get_name() == "headless" else "verified"])
	quit()
