extends SceneTree

# Forward+ side view of the actual Pike Encounter fish and animated Pond surface.
# Captures only to the caller's ignored artifact directory.

func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	if RenderingServer.get_current_rendering_method() != "forward_plus":
		printerr("WATERLINE_NATIVE_FAILED Forward+ required")
		quit(1)
		return
	var scene: Node3D = load("res://scenes/p5_pike_encounter.tscn").instantiate()
	root.add_child(scene)
	current_scene = scene
	await process_frame
	scene.set_physics_process(false) # Keep Pike AI out of this fixed camera comparison.
	var fish: CharacterBody3D = scene.get_node("Fish")
	var atmosphere: Node3D = scene.get_node("UnderwaterAtmosphere")
	var water: Node3D = fish.get_node("WaterReaction")
	water.set_physics_process(false)
	var sample_x := 0.0
	var sample_z := 0.0
	var height: float = atmosphere.call("GetSurfaceHeight", sample_x, sample_z)
	var radius: float = fish.get_node("Collision").shape.radius
	fish.global_position = Vector3(sample_x, height - radius * 0.35 - 0.002, sample_z)
	fish.global_basis = Basis.IDENTITY
	var camera := Camera3D.new()
	camera.fov = 43.0
	camera.near = 0.005
	camera.far = 120.0
	scene.add_child(camera)
	camera.global_position = fish.global_position + Vector3(1.15, 0.17, 0.15)
	camera.look_at(fish.global_position + Vector3(0, 0.04, 0))
	camera.make_current()
	var surface_marker := _marker(scene, Color(1, 0.2, 0.1), "SampledSurface")
	var limit_marker := _marker(scene, Color(1, 0.85, 0.1), "SwimLimit")
	var prefix := OS.get_environment("FISH_WATERLINE_CAPTURE")
	if prefix.is_empty():
		printerr("WATERLINE_NATIVE_FAILED capture path missing")
		quit(1)
		return
	for frame in 180:
		await physics_frame
		var current_height: float = atmosphere.call("GetSurfaceHeight", fish.global_position.x, fish.global_position.z)
		surface_marker.global_position = Vector3(fish.global_position.x,
			current_height, fish.global_position.z + 0.22)
		limit_marker.global_position = Vector3(fish.global_position.x,
			current_height - 0.35 * radius, fish.global_position.z + 0.22)
		if frame % 60 != 0:
			continue
		var position: Vector3 = fish.global_position
		water.call("SamplePondSurface", position, Vector3(0, 0, 1), 1.0, 0.3)
		await process_frame
		var sample_height: float = atmosphere.call("GetSurfaceHeight", position.x, position.z)
		var image: Image = root.get_viewport().get_texture().get_image()
		var path := "%s-%d.png" % [prefix, frame]
		image.save_png(path)
		var ring_count := 0
		for child in water.get_children():
			if child is MeshInstance3D and child.visible:
				ring_count += 1
		print("WATERLINE_NATIVE_SAMPLE frame=%d surface=%.4f fish=%.4f gap=%.4f rings=%d path=%s" % [
			frame, sample_height, position.y, sample_height - radius * 0.35 - position.y,
			ring_count, path])
	var started := Time.get_ticks_usec()
	var checksum := 0.0
	for i in 10000:
		checksum += float(atmosphere.call("GetSurfaceHeight", float(i % 29), float(i % 31)))
	print("WATERLINE_NATIVE_COST samples=10000 elapsed_ms=%.2f checksum=%.1f adapter=%s" % [
		float(Time.get_ticks_usec() - started) / 1000.0, checksum,
		RenderingServer.get_video_adapter_name()])
	quit()

func _marker(scene: Node3D, tint: Color, name: String) -> MeshInstance3D:
	var material := StandardMaterial3D.new()
	material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	material.albedo_color = tint
	material.no_depth_test = true
	var mesh := SphereMesh.new()
	mesh.radius = 0.018
	mesh.height = 0.036
	mesh.material = material
	var marker := MeshInstance3D.new()
	marker.name = name
	marker.mesh = mesh
	scene.add_child(marker)
	return marker
