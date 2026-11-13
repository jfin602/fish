extends SceneTree

# Matched native Forward+ night-optics capture. Does not save scenes or settings.
const WARMUP := 24
const SAMPLES := 90

var output := ""
var mode := "fixed"
var main: Node3D
var fish: Node3D
var camera: Camera3D
var follow: Camera3D
var atmosphere: Node3D
var environment: Environment
var clock: Node
var sun: DirectionalLight3D
var sky: ShaderMaterial
var surface: ShaderMaterial
var seabed: Node
var manifest: Dictionary = {}

func _initialize() -> void:
	call_deferred("_run")

func _fail(message: String) -> void:
	printerr("NIGHT_OPTICS_FAILED ", message)
	quit(1)

func _run() -> void:
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--night-output="):
			output = argument.trim_prefix("--night-output=")
		elif argument.begins_with("--night-mode="):
			mode = argument.trim_prefix("--night-mode=")
	if output.is_empty() or DirAccess.make_dir_recursive_absolute(output) != OK:
		_fail("output directory required")
		return
	if ProjectSettings.get_setting("application/config/name") != "Fish" or RenderingServer.get_current_rendering_method() != "forward_plus" or RenderingServer.get_video_adapter_name().is_empty():
		_fail("Fish native Forward+ required")
		return
	if not OS.is_debug_build() or not OS.get_cmdline_user_args().has("--pond-surface-probe") or not OS.get_cmdline_user_args().has("--environment-time-debug"):
		_fail("Pond time-debug fixture required")
		return
	DisplayServer.window_set_vsync_mode(DisplayServer.VSYNC_DISABLED)
	Engine.max_fps = 0
	main = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	await process_frame
	fish = main.get_node("Fish")
	follow = main.get_node("Fish/CameraRig/Camera3D")
	atmosphere = main.get_node("UnderwaterAtmosphere")
	environment = main.get_node("WorldEnvironment").environment
	clock = main.get_node("EnvironmentClock")
	sun = main.get_node("Sun")
	sky = environment.sky.sky_material
	surface = main.get_node("UnderwaterAtmosphere/WaterEffects/OverheadSurface").material_override
	seabed = main.get_node("Habitat/Seabed")
	main.get_node("PrototypeHelp").visible = false
	camera = Camera3D.new()
	camera.far = 150.0
	main.add_child(camera)
	camera.make_current()
	manifest = {"project":"Fish", "world":"pond-p4-71", "seed":71,
		"godot":Engine.get_version_info().string,"renderer":RenderingServer.get_current_rendering_method(),
		"adapter":RenderingServer.get_video_adapter_name(),"viewport":str(root.size),
		"vsync":"disabled","fps_cap":0,"mode":mode,"views":{}}
	print("NIGHT_OPTICS_SETUP ",JSON.stringify(manifest))
	if mode == "fixed":
		paused = true
		for item in [["shore",0.0],["fish",0.0],["log",0.0],["open",0.0],["up",0.0],
			["moon_shore",0.0],["moon_up",0.0],
			["shore",3.0],["shore",5.5],["shore",12.0],["fish",12.0],["shore",18.0]]:
			_pose(item[0])
			_set_hour(item[1])
			if item[0].begins_with("moon_"):
				camera.look_at(camera.global_position+sun.global_basis.z.normalized(),Vector3.UP)
				atmosphere.call("RefreshDepth",true,2.0)
			await _capture("%s_%04d" % [item[0],roundi(item[1]*100.0)], true)
		paused = false
	elif mode == "motion":
		await _motion()
	else:
		_fail("unknown mode " + mode)
		return
	var file := FileAccess.open(output.path_join("manifest-%s.json" % mode),FileAccess.WRITE)
	file.store_string(JSON.stringify(manifest,"  "))
	file.close()
	print("NIGHT_OPTICS_DONE mode=",mode)
	quit()

func _set_hour(hour: float) -> void:
	clock.call("SetTimeForDebug",hour)
	atmosphere.call("RefreshSolar",true)
	atmosphere.call("RefreshDepth",true,2.0)

func _pose(view: String) -> void:
	if view == "moon_shore": view = "shore"
	elif view == "moon_up": view = "up"
	var surface_y: float = atmosphere.SurfaceHeight
	if view == "shore":
		var bank: Marker3D = main.get_node("Habitat/PondLandmarks/BankApproach")
		var shallow: Marker3D = main.get_node("Habitat/PondLandmarks/shallow")
		var direction := Vector2(shallow.global_position.x-bank.global_position.x,
			shallow.global_position.z-bank.global_position.z).normalized()
		var eye_y: float = surface_y+0.65
		camera.global_position = Vector3(bank.global_position.x+direction.x*40.0,eye_y,
			bank.global_position.z+direction.y*40.0)
		camera.look_at(Vector3(bank.global_position.x,eye_y,bank.global_position.z),Vector3.UP)
	elif view == "fish":
		fish.global_position = Vector3(-19.64,surface_y-1.6,23.96)
		camera.global_position = fish.global_position+Vector3(0,0.45,3.2)
		camera.look_at(fish.global_position+Vector3(0,0,-2),Vector3.UP)
	elif view == "log":
		var hero: Node3D
		for child in main.get_node("Habitat/PondFafEnvironment").get_children():
			if child.name.begins_with("FAF_WOOD_002A_"):
				hero=child
		if hero == null:
			_fail("hollow log missing")
			return
		fish.global_position = hero.to_global(Vector3(0,2.52,6.2))
		camera.global_position = hero.to_global(Vector3(0,2.8,9.0))
		camera.look_at(hero.to_global(Vector3(0,2.4,0)),Vector3.UP)
	else:
		var site := Vector2(-19.64,23.96)
		var floor_y: float = seabed.call("GetHeightAt",site.x,site.y)
		fish.global_position = Vector3(site.x,maxf(floor_y+1.2,surface_y-3.0),site.y)
		camera.global_position = fish.global_position+Vector3(0,-0.1 if view == "up" else 0.6,3.2)
		camera.look_at(fish.global_position+Vector3(0,2.4 if view == "up" else 0.3,-4.0 if view == "up" else -14.0),Vector3.UP)
	atmosphere.call("RefreshDepth",true,2.0)

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
			frames.append(float(now-previous)/1000.0)
			previous=now
			draws.append(Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME))
			primitives.append(Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME))
	var path := output.path_join(label+".png")
	if root.get_texture().get_image().save_png(path) != OK:
		_fail("save " + path)
		return
	var row := {"image":path,"hour":clock.HourOfDay,"fish":str(fish.global_position),
		"camera":str(camera.global_position),"camera_basis":str(camera.global_basis),
		"moon_direction":str(sun.global_basis.z.normalized()),
		"moon_visibility":sky.get_shader_parameter("moon_glow"),
		"air_horizon":str(sky.get_shader_parameter("air_horizon")),
		"air_zenith":str(sky.get_shader_parameter("air_zenith")),
		"ambient_color":str(environment.ambient_light_color),
		"ambient_energy":environment.ambient_light_energy,
		"sun_energy":sun.light_energy,"fog_color":str(environment.fog_light_color),
		"fog_density":environment.fog_density,"fog_begin":environment.fog_depth_begin,
		"fog_end":environment.fog_depth_end,"fog_curve":environment.fog_depth_curve,
		"volume_color":str(environment.volumetric_fog_albedo),
		"volume_density":environment.volumetric_fog_density,
		"underwater_sky":sky.get_shader_parameter("underwater_daylight"),
		"surface_glow":str(surface.get_shader_parameter("night_surface_glow")),
		"surface_illumination":surface.get_shader_parameter("surface_illumination")}
	if benchmark:
		var ordered := frames.duplicate(); ordered.sort()
		row["median_ms"]=_median(frames)
		row["p95_ms"]=ordered[ceili(float(ordered.size())*0.95)-1]
		row["draws"]=_median(draws)
		row["primitives"]=_median(primitives)
		row["samples"]=SAMPLES
	manifest.views[label]=row
	print("NIGHT_OPTICS_FRAME ",JSON.stringify({"label":label,"median_ms":row.get("median_ms","na"),"p95_ms":row.get("p95_ms","na"),"draws":row.get("draws","na"),"fog":row.fog_color,"sky":row.air_horizon}))

func _key(code: Key, down: bool) -> void:
	var event := InputEventKey.new()
	event.physical_keycode=code
	event.pressed=down
	Input.parse_input_event(event)

func _motion() -> void:
	var spawn: Vector3 = main.get_node("Habitat/Spawn").global_position
	fish.global_position=Vector3(spawn.x,minf(spawn.y+0.4,atmosphere.SurfaceHeight-2.6),spawn.z)
	fish.call("SetInputMode","keyboard")
	follow.make_current()
	_set_hour(0.0)
	for i in 10: await process_frame
	_key(KEY_D,true)
	for i in 36: await physics_frame
	_key(KEY_D,false)
	_key(KEY_W,true)
	var beginning:=Time.get_ticks_usec()
	var previous:=beginning
	var next_photo:=0
	var frames: Array[float]=[]
	var draws: Array[float]=[]
	var primitives: Array[float]=[]
	var points: Array=[]
	while Time.get_ticks_usec()-beginning < 8000000:
		await process_frame
		var now:=Time.get_ticks_usec()
		frames.append(float(now-previous)/1000.0)
		previous=now
		draws.append(Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME))
		primitives.append(Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME))
		if now-beginning >= next_photo*1000000:
			var path:=output.path_join("route_%02d.png"%next_photo)
			root.get_texture().get_image().save_png(path)
			points.append({"second":float(now-beginning)/1000000.0,
				"fish":str(fish.global_position),"camera":str(follow.global_position),"image":path})
			next_photo+=1
	_key(KEY_W,false)
	paused=true
	var ordered:=frames.duplicate(); ordered.sort()
	manifest["motion"]={"duration_seconds":8,"samples":frames.size(),
		"median_ms":_median(frames),"p95_ms":ordered[ceili(float(ordered.size())*0.95)-1],
		"draws":_median(draws),"primitives":_median(primitives),
		"frames":points,"end":str(fish.global_position)}
	print("NIGHT_OPTICS_MOTION ",JSON.stringify(manifest.motion))
