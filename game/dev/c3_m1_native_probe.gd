extends SceneTree

# Disposable Forward+ review capture of a baked M1 candidate. No scene is saved.
const SURFACE_Y := 10.39
const TARGETS := [
	["shore-shallow", 1.3],
	["shallow-reef", 4.0],
	["middle", 10.0],
	["deep-basin", 23.0],
	["trench", 37.0],
]

func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	var out := ""
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--c3-capture-dir="):
			out = argument.trim_prefix("--c3-capture-dir=")
	if out.is_empty():
		push_error("C3_M1_PROBE_FAILED capture directory missing")
		quit(2)
		return
	DirAccess.make_dir_recursive_absolute(out)
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	await create_timer(1.0).timeout
	var fish: CharacterBody3D = main.get_node("Fish")
	var space := main.get_world_3d().direct_space_state
	var limits: StaticBody3D = main.get_node("Habitat/HiddenLimits")
	var chosen := {}
	for target in TARGETS:
		var best := 10000.0
		var best_at := Vector2.ZERO
		var best_floor := 0.0
		for z in range(-160, 161, 5):
			for x in range(-160, 161, 5):
				var hit := _floor(space, limits, Vector2(x, z))
				if hit.is_empty():
					continue
				var depth: float = SURFACE_Y - hit.position.y
				if depth < 0.5 or depth > 45.0:
					continue
				var score: float = absf(depth - target[1]) + 0.001 * Vector2(x, z).length()
				if score < best:
					best = score
					best_at = Vector2(x, z)
					best_floor = hit.position.y
		if best > 1.0:
			push_error("C3_M1_PROBE_FAILED no %s sample, error=%.2f" % [target[0], best])
			quit(1)
			return
		chosen[target[0]] = [best_at, best_floor]
	for sample in TARGETS:
		var at: Vector2 = chosen[sample[0]][0]
		var floor_y: float = chosen[sample[0]][1]
		await _capture_pose(fish, out, sample[0], at, floor_y)
	for direction in [
		["east", Vector2.RIGHT], ["west", Vector2.LEFT],
		["north", Vector2.UP], ["south", Vector2.DOWN]
	]:
		var found := false
		var at := Vector2.ZERO
		var floor_y := 0.0
		for radius in range(60, 171, 2):
			var p: Vector2 = direction[1] * radius
			var hit := _floor(space, limits, p)
			if hit.is_empty():
				continue
			var depth: float = SURFACE_Y - hit.position.y
			if depth >= 1.2 and depth <= 4.5:
				at = p
				floor_y = hit.position.y
				found = true
		if not found:
			push_error("C3_M1_PROBE_FAILED no perimeter sample: " + direction[0])
			quit(1)
			return
		await _capture_pose(fish, out, "perimeter-" + direction[0], at, floor_y)
	# Debug overview removes water/fog and uses a temporary top-down camera;
	# it does not alter the baked package or gameplay scenes.
	main.get_node("UnderwaterAtmosphere/WaterEffects/OverheadSurface").visible = false
	for effect in main.get_node("UnderwaterAtmosphere/WaterEffects").get_children():
		if effect is Decal:
			effect.visible = false
	main.get_node("PrototypeHelp").visible = false
	var environment: WorldEnvironment = main.get_node("WorldEnvironment")
	environment.environment.fog_enabled = false
	environment.environment.volumetric_fog_enabled = false
	var overview := Camera3D.new()
	overview.name = "M1DebugOverview"
	overview.position = Vector3(0, 280, 0)
	overview.rotation.x = -PI / 2
	overview.projection = Camera3D.PROJECTION_ORTHOGONAL
	overview.size = 390
	overview.far = 600
	main.add_child(overview)
	overview.current = true
	await create_timer(1.0).timeout
	await RenderingServer.frame_post_draw
	var overview_path := out.path_join("debug-overview.png")
	var overview_image: Image = root.get_texture().get_image()
	var overview_err := overview_image.save_png(overview_path)
	if overview_err != OK:
		push_error("C3_M1_PROBE_FAILED overview save: %d" % overview_err)
	else:
		print("C3_M1_CAPTURE name=debug-overview file=%s" % overview_path)
	quit()

func _floor(space: PhysicsDirectSpaceState3D, limits: StaticBody3D, at: Vector2) -> Dictionary:
	var query := PhysicsRayQueryParameters3D.create(
		Vector3(at.x, 60, at.y), Vector3(at.x, -65, at.y))
	query.exclude = [limits.get_rid()]
	return space.intersect_ray(query)

func _capture_pose(fish: CharacterBody3D, out: String, name: String,
		at: Vector2, floor_y: float) -> void:
	var clearance := minf(1.6, maxf(0.58, SURFACE_Y - floor_y - 0.7))
	fish.global_position = Vector3(at.x, floor_y + clearance, at.y)
	fish.rotation.y = atan2(-at.x, -at.y)
	fish.velocity = Vector3.ZERO
	await create_timer(2.5).timeout
	await RenderingServer.frame_post_draw
	var image: Image = root.get_texture().get_image()
	var path := out.path_join(name + ".png")
	var err := image.save_png(path)
	if err != OK:
		push_error("C3_M1_PROBE_FAILED save %s: %d" % [path, err])
	else:
		print("C3_M1_CAPTURE name=%s at=%s depth=%.2f file=%s" %
			[name, at, SURFACE_Y - floor_y, path])
