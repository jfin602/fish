extends SceneTree

# Native R4 sediment evidence. This script never saves a scene or preset.
const WARMUP := 24
const SAMPLES := 90
var output := ""
var mode := "fixed"
var main: Node3D
var fish: Node3D
var camera: Camera3D
var atmosphere: Node3D
var environment: Environment
var clock: Node
var seabed: Node
var volume: FogVolume
var material: FogMaterial
var slices: Array[Image]
var manifest: Dictionary = {}
var sites: Dictionary = {}

func _initialize() -> void:
	call_deferred("_run")

func _fail(message: String) -> void:
	printerr("CLARITY_R4_FAILED ", message)
	quit(1)

func _run() -> void:
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--sediment-output="):
			output = argument.trim_prefix("--sediment-output=")
		elif argument.begins_with("--sediment-mode="):
			mode = argument.trim_prefix("--sediment-mode=")
	if output.is_empty() or DirAccess.make_dir_recursive_absolute(output) != OK:
		_fail("output directory required")
		return
	if RenderingServer.get_current_rendering_method() != "forward_plus" or RenderingServer.get_video_adapter_name().is_empty():
		_fail("native Forward+ required")
		return
	if ProjectSettings.get_setting("application/config/name") != "Fish" or not OS.get_cmdline_user_args().has("--pond-surface-probe") or not OS.get_cmdline_user_args().has("--environment-time-debug"):
		_fail("wrong project or Pond fixture")
		return
	DisplayServer.window_set_vsync_mode(DisplayServer.VSYNC_DISABLED)
	Engine.max_fps = 0
	main = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	await process_frame
	fish = main.get_node("Fish")
	atmosphere = main.get_node("UnderwaterAtmosphere")
	environment = main.get_node("WorldEnvironment").environment
	clock = main.get_node("EnvironmentClock")
	seabed = main.get_node("Habitat/Seabed")
	volume = atmosphere.get_node("PondTurbidity")
	material = volume.material
	slices = material.density_texture.get_data()
	main.get_node("PrototypeHelp").visible = false
	camera = Camera3D.new()
	camera.far = 150.0
	main.add_child(camera)
	camera.make_current()
	clock.call("SetTimeForDebug", 12.0)
	atmosphere.call("RefreshSolar", true)
	paused = true
	manifest = {"project":"Fish", "world":"pond-p4-71", "seed":71,
		"time_hour":12.0, "renderer":RenderingServer.get_current_rendering_method(),
		"adapter":RenderingServer.get_video_adapter_name(), "godot":Engine.get_version_info().string,
		"viewport":str(root.size), "vsync":"disabled", "fps_cap":0,
		"volume_center":str(volume.global_position), "volume_size":str(volume.size),
		"volume_edge_fade":material.edge_fade, "volume_density":material.density,
		"texture_dimensions":[64,16,64], "texture_bytes":65536,
		"global_volume_density":environment.volumetric_fog_density, "views":{}}
	_scan_field()
	if mode == "fixed":
		for view in ["A1_spawn","A2_near","A3_log","A4_shallow","A5_silty","A6_middle","A7_deep","A9_shade","B1_clear","B2_murky","B3_clear_bed","B4_murky_bed","B5_transition","C1_clear","C2_murky","C3_clear_bed","C4_murky_bed"]:
			_pose(view)
			atmosphere.call("RefreshDepth", true, 2.0)
			await _capture(view, true)
	elif mode == "diagnostics":
		for view in ["A2_near","A3_log","B1_clear","B2_murky","B4_murky_bed","C1_clear","C2_murky"]:
			_pose(view)
			atmosphere.call("RefreshDepth", true, 2.0)
			var base: float = material.density
			for setting in ["full","off","low","high","restored"]:
				material.density = base * ({"full":1.0,"off":0.0,"low":0.5,"high":2.0,"restored":1.0}[setting])
				await _capture(view + "_" + setting, false)
			material.density = base
	elif mode == "motion":
		await _motion()
	else:
		_fail("unknown mode " + mode)
		return
	var file := FileAccess.open(output.path_join("manifest.json"), FileAccess.WRITE)
	file.store_string(JSON.stringify(manifest, "  "))
	file.close()
	print("CLARITY_R4_DONE mode=%s views=%d" % [mode, manifest.views.size()])
	quit()

func _sample_texture(p: Vector3) -> float:
	var local: Vector3 = p - volume.global_position
	var fx: float = (local.x / volume.size.x + 0.5) * 64.0 - 0.5
	var fy: float = (local.y / volume.size.y + 0.5) * 16.0 - 0.5
	var fz: float = (local.z / volume.size.z + 0.5) * 64.0 - 0.5
	if fx < 0.0 or fx > 63.0 or fy < 0.0 or fy > 15.0 or fz < 0.0 or fz > 63.0:
		return 0.0
	var x0 := int(floor(fx)); var y0 := int(floor(fy)); var z0 := int(floor(fz))
	var x1 := mini(x0 + 1, 63); var y1 := mini(y0 + 1, 15); var z1 := mini(z0 + 1, 63)
	var tx := fx - x0; var ty := fy - y0; var tz := fz - z0
	var lower: float = lerpf(lerpf(slices[z0].get_pixel(x0,y0).r,slices[z0].get_pixel(x1,y0).r,tx),lerpf(slices[z0].get_pixel(x0,y1).r,slices[z0].get_pixel(x1,y1).r,tx),ty)
	var upper: float = lerpf(lerpf(slices[z1].get_pixel(x0,y0).r,slices[z1].get_pixel(x1,y0).r,tx),lerpf(slices[z1].get_pixel(x0,y1).r,slices[z1].get_pixel(x1,y1).r,tx),ty)
	return lerpf(lower,upper,tz)

func _scan_field() -> void:
	var points: Array = []
	var map_rows: Array = []
	var surface: float = atmosphere.SurfaceHeight
	for z in range(-54,55,3):
		var row: Array = []
		for x in range(-54,55,3):
			var bed: float = seabed.call("GetHeightAt",float(x),float(z))
			var depth: float = surface - bed
			var value: float = _sample_texture(Vector3(x,surface-2.0,z))
			row.append(snappedf(value,0.0001))
			if depth >= 5.0 and depth <= 9.0 and value > 0.01:
				points.append({"x":x,"z":z,"bed":bed,"water_depth":depth,"density":value})
		map_rows.append(row)
	points.sort_custom(func(a: Dictionary,b: Dictionary) -> bool: return a.density < b.density)
	if points.size() < 2:
		_fail("no comparable-depth sample sites")
		return
	var low: Dictionary = points[0]
	var high: Dictionary = points[-1]
	# Pair by water depth to keep depth-driven light and fog comparable.
	var best_gap := -1.0
	for a in points:
		for b in points:
			if absf(a.water_depth-b.water_depth) > 0.5 or Vector2(a.x,a.z).distance_to(Vector2(b.x,b.z)) < 12.0:
				continue
			var gap: float = b.density-a.density
			if gap > best_gap:
				best_gap = gap; low = a; high = b
	# Pinned across baseline/candidate. The C pair is the candidate's extrema at matched depth.
	sites = {"clear":{"x":-30,"z":-27},"murky":{"x":3,"z":9},
		"transition":{"x":-13.5,"z":-9.0},
		"candidate_clear":{"x":18,"z":-12},"candidate_murky":{"x":-27,"z":-36}}
	manifest["field_scan"] = {"grid_origin_xz":[-54,-54],"step_m":3,"sample_y":surface-2.0,
		"normalized_texture_values":map_rows,"eligible_count":points.size(),"min":points[0].density,
		"max":points[-1].density,"paired_gap":best_gap,"extreme_clear":low,"extreme_murky":high,"sites":sites}
	print("CLARITY_R4_FIELD ",JSON.stringify({"eligible_count":points.size(),"min":points[0].density,"max":points[-1].density,"paired_gap":best_gap,"sites":sites}))

func _pose(view: String) -> void:
	var surface_y: float = atmosphere.SurfaceHeight
	var landmarks: Node3D = main.get_node("Habitat/PondLandmarks")
	var spawn: Vector3 = main.get_node("Habitat/Spawn").global_position
	var shallow: Vector3 = landmarks.get_node("shallow").global_position
	var middle: Vector3 = landmarks.get_node("middle").global_position
	var deep: Vector3 = landmarks.get_node("deep").global_position
	var hero: Node3D
	for child in main.get_node("Habitat/PondFafEnvironment").get_children():
		if child.name.begins_with("FAF_WOOD_002A_"): hero = child
	match view:
		"A1_spawn":
			fish.global_position = Vector3(spawn.x,minf(surface_y-1.1,spawn.y+0.4),spawn.z)
			camera.global_position = fish.global_position + Vector3(0,0.65,3.2)
			camera.look_at(fish.global_position+Vector3(0,0.15,-2),Vector3.UP)
		"A2_near":
			fish.global_position = Vector3(-19.64,surface_y-1.6,23.96)
			camera.global_position = fish.global_position+Vector3(0,0.45,3.2)
			camera.look_at(fish.global_position+Vector3(0,0,-2),Vector3.UP)
		"A3_log","A9_shade":
			fish.global_position = hero.to_global(Vector3(0,2.52,6.2 if view == "A3_log" else 1.0))
			camera.global_position = hero.to_global(Vector3(0,2.8,9.0 if view == "A3_log" else 3.5))
			camera.look_at(hero.to_global(Vector3(0,2.4,0)),Vector3.UP)
		"A4_shallow":
			fish.global_position = Vector3(shallow.x,minf(surface_y-0.8,shallow.y+1.0),shallow.z)
			camera.global_position = fish.global_position+Vector3(0,0.35,3.0)
			camera.look_at(fish.global_position+Vector3(0,0,-4),Vector3.UP)
		"A5_silty":
			var site := Vector2(shallow.x+4.0,shallow.z+4.0)
			var bed: float = seabed.call("GetHeightAt",site.x,site.y)
			fish.global_position = Vector3(site.x,bed+0.65,site.y)
			camera.global_position = fish.global_position+Vector3(0,0.4,3.0)
			camera.look_at(fish.global_position+Vector3(0,-0.2,-3),Vector3.UP)
		"A6_middle":
			fish.global_position = middle+(shallow-middle).normalized()*3.5
			camera.global_position = middle+Vector3(0,0.5,0)
			camera.look_at(fish.global_position,Vector3.UP)
		"A7_deep":
			fish.global_position = deep+(middle-deep).normalized()*3.5
			camera.global_position = deep+Vector3(0,0.5,0)
			camera.look_at(fish.global_position,Vector3.UP)
		"B1_clear","B2_murky","B3_clear_bed","B4_murky_bed","B5_transition","C1_clear","C2_murky","C3_clear_bed","C4_murky_bed":
			var key := "clear" if view in ["B1_clear","B3_clear_bed"] else "murky" if view in ["B2_murky","B4_murky_bed"] else "transition" if view == "B5_transition" else "candidate_clear" if view in ["C1_clear","C3_clear_bed"] else "candidate_murky"
			var site: Dictionary = sites[key]
			var bed: float = seabed.call("GetHeightAt",float(site.x),float(site.z))
			var y: float = bed+0.75 if view in ["B3_clear_bed","B4_murky_bed","C3_clear_bed","C4_murky_bed"] else surface_y-2.0
			fish.global_position = Vector3(site.x,y,site.z)
			camera.global_position = fish.global_position+Vector3(0,0.35,3.0)
			camera.look_at(fish.global_position+Vector3(0,0,-4),Vector3.UP)
	print("CLARITY_R4_POSE view=%s fish=%s camera=%s density=%s" % [view,fish.global_position,camera.global_position,_sample_texture(camera.global_position)])

func _median(values: Array[float]) -> float:
	values.sort()
	return values[values.size()/2]

func _capture(label: String, benchmark: bool) -> void:
	for i in WARMUP: await process_frame
	var frames: Array[float] = []
	var draws: Array[float] = []
	var primitives: Array[float] = []
	if benchmark:
		var previous := Time.get_ticks_usec()
		for i in SAMPLES:
			await process_frame
			var now := Time.get_ticks_usec()
			frames.append(float(now-previous)/1000.0); previous=now
			draws.append(Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME))
			primitives.append(Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME))
	var path := output.path_join(label+".png")
	if root.get_texture().get_image().save_png(path) != OK:
		_fail("save " + path); return
	var row := {"fish":str(fish.global_position),"camera":str(camera.global_position),
		"camera_basis":str(camera.global_basis),"camera_sample":_sample_texture(camera.global_position),
		"fish_sample":_sample_texture(fish.global_position),"local_density":material.density,
		"global_density":environment.volumetric_fog_density,"distance_fog":environment.fog_density,
		"image":path}
	if benchmark:
		var ordered := frames.duplicate(); ordered.sort()
		row["median_ms"]=_median(frames); row["p95_ms"]=ordered[ceili(float(ordered.size())*0.95)-1]
		row["draws"]=_median(draws); row["primitives"]=_median(primitives); row["samples"]=SAMPLES
	manifest.views[label]=row
	print("CLARITY_R4_SAMPLE view=%s median=%s p95=%s density=%s" % [label,row.get("median_ms","na"),row.get("p95_ms","na"),row.camera_sample])

func _key(code: Key, down: bool) -> void:
	var event := InputEventKey.new()
	event.physical_keycode=code; event.pressed=down
	Input.parse_input_event(event)

func _motion() -> void:
	# Real fish motor and follow camera, with the R2/R3 repeatable route.
	var spawn: Vector3 = main.get_node("Habitat/Spawn").global_position
	fish.global_position=Vector3(spawn.x,minf(spawn.y+0.4,atmosphere.SurfaceHeight-2.6),spawn.z)
	fish.call("SetInputMode","keyboard")
	var follow: Camera3D=main.get_node("Fish/CameraRig/Camera3D")
	follow.make_current(); paused=false
	clock.call("SetTimeForDebug",12.0)
	for i in 10: await process_frame
	_key(KEY_D,true)
	for i in 36: await physics_frame
	_key(KEY_D,false); _key(KEY_W,true)
	var beginning:=Time.get_ticks_usec(); var previous:=beginning; var next_photo:=0
	var frames: Array[float]=[]; var draws: Array[float]=[]; var primitives: Array[float]=[]; var points: Array=[]
	while Time.get_ticks_usec()-beginning < 8000000:
		await process_frame
		var now:=Time.get_ticks_usec()
		frames.append(float(now-previous)/1000.0); previous=now
		draws.append(Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME))
		primitives.append(Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME))
		if now-beginning >= next_photo*1000000:
			var path:=output.path_join("M_%02d.png"%next_photo)
			root.get_texture().get_image().save_png(path)
			points.append({"second":float(now-beginning)/1000000.0,"fish":str(fish.global_position),
				"camera":str(follow.global_position),"density":_sample_texture(follow.global_position),"image":path})
			next_photo+=1
	_key(KEY_W,false); paused=true
	var ordered:=frames.duplicate(); ordered.sort()
	manifest["motion"]={"duration_seconds":8,"samples":frames.size(),"median_ms":_median(frames),
		"p95_ms":ordered[ceili(float(ordered.size())*0.95)-1],"draws":_median(draws),
		"primitives":_median(primitives),"frames":points,"end":str(fish.global_position)}
	print("CLARITY_R4_MOTION ",JSON.stringify(manifest.motion))
