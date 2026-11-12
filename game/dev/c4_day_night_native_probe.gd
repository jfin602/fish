extends SceneTree

# Native Forward+ qualification capture. Debug time controls are required.
const SETTLE_FRAMES := 16
const HOURS_POND := [0.0, 6.0, 9.0, 12.0, 17.0, 18.0, 18.5]
const HOURS_OCEAN := [0.0, 9.0, 12.0, 17.0, 18.5]

var output_dir := ""
var main: Node3D
var clock: Node
var atmosphere: Node3D
var surface: ShaderMaterial
var shafts: ShaderMaterial
var sky: ShaderMaterial
var caustic: Decal
var environment: Environment
var sun: DirectionalLight3D
var fish: Node3D
var camera: Camera3D
var pond := false
var failed := false
var capture_prefix := ""
var initial_fish_position := Vector3.ZERO

func _initialize() -> void:
	call_deferred("_run")

func _fail(reason: String) -> void:
	failed = true
	printerr("C4_NATIVE_FAILED ", reason)
	quit(1)

func _run() -> void:
	var mode := "fixed"
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--c4-output="):
			output_dir = argument.trim_prefix("--c4-output=")
		if argument.begins_with("--c4-mode="):
			mode = argument.trim_prefix("--c4-mode=")
		if argument.begins_with("--c4-label="):
			capture_prefix = argument.trim_prefix("--c4-label=") + "_"
	pond = OS.get_cmdline_user_args().has("--pond-surface-probe")
	if output_dir.is_empty() or DirAccess.make_dir_recursive_absolute(output_dir) != OK:
		_fail("output directory required")
		return
	if not OS.is_debug_build() or not OS.get_cmdline_user_args().has("--environment-time-debug"):
		_fail("debug-only environment time control required")
		return
	if RenderingServer.get_current_rendering_method() != "forward_plus" or RenderingServer.get_video_adapter_name().is_empty():
		_fail("native Forward+ adapter required")
		return
	if ProjectSettings.get_setting("application/config/name") != "Fish":
		_fail("selected project is not Fish")
		return
	main = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	await process_frame
	clock = main.get_node("EnvironmentClock")
	atmosphere = main.get_node("UnderwaterAtmosphere")
	surface = main.get_node("UnderwaterAtmosphere/WaterEffects/OverheadSurface").material_override
	shafts = main.get_node("UnderwaterAtmosphere/WaterEffects/SunShafts").material_override
	caustic = main.get_node("UnderwaterAtmosphere/WaterEffects/CausticProjection1")
	environment = main.get_node("WorldEnvironment").environment
	sky = environment.sky.sky_material
	sun = main.get_node("Sun")
	fish = main.get_node("Fish")
	initial_fish_position = fish.global_position
	camera = Camera3D.new()
	camera.far = 150.0
	main.add_child(camera)
	camera.make_current()
	main.get_node("PrototypeHelp").visible = false
	print("C4_NATIVE_SETUP project=%s godot=%s renderer=%s adapter=%s viewport=%s window=%s mode=%s habitat=%s surface_y=%.3f fish=%s" % [
		ProjectSettings.get_setting("application/config/name"), Engine.get_version_info().string,
		RenderingServer.get_current_rendering_method(), RenderingServer.get_video_adapter_name(),
		ProjectSettings.get_setting("display/window/size/viewport_width"), root.size,
		mode, "pond" if pond else "ocean", atmosphere.SurfaceHeight, fish.name])
	if mode == "fixed":
		await _fixed_matrix()
	elif mode == "sweep":
		await _continuous_sweep()
	elif mode == "pause":
		await _pause_probe()
	elif mode == "focus":
		await _focus_probe()
	elif mode == "shore-repro":
		await _shore_repro()
	elif mode == "shore-sweep":
		await _continuous_sweep("far_shore")
	elif mode == "night":
		await _night_matrix()
	else:
		_fail("unknown mode " + mode)
		return
	if not failed:
		print("C4_NATIVE_DONE mode=%s habitat=%s" % [mode, "pond" if pond else "ocean"])
		quit()

func _set_hour(hour: float) -> void:
	clock.call("SetTimeForDebug", hour)
	atmosphere.call("RefreshSolar", true)
	atmosphere.call("RefreshDepth", true, 2.0)

func _pose(view: String) -> void:
	if pond:
		if view == "shore" or view == "far_shore":
			var bank: Marker3D = main.get_node("Habitat/PondLandmarks/BankApproach")
			var shallow: Marker3D = main.get_node("Habitat/PondLandmarks/shallow")
			var direction := Vector2(shallow.global_position.x - bank.global_position.x,
				shallow.global_position.z - bank.global_position.z).normalized()
			var distance := 40.0 if view == "far_shore" else 9.0
			var eye_y: float = atmosphere.SurfaceHeight + 0.65 if view == "far_shore" else 11.6
			camera.global_position = Vector3(bank.global_position.x + direction.x * distance,
				eye_y, bank.global_position.z + direction.y * distance)
			camera.look_at(Vector3(bank.global_position.x, eye_y, bank.global_position.z), Vector3.UP)
			print("C4_SHORE_AXIS habitat=pond view=%s bank=%s shallow=%s camera=%s forward=%s" % [
				view, bank.global_position, shallow.global_position, camera.global_position,
				-camera.global_basis.z])
		elif view == "fish":
			fish.global_position = Vector3(-19.64, 9.390, 23.96)
			camera.global_position = Vector3(-19.64, 15.5, 26.96)
			camera.look_at(Vector3(-19.64, 8.5, 23.96), Vector3.UP)
		else:
			fish.global_position = Vector3(-19.64, 8.790, 23.96)
			camera.global_position = Vector3(-19.64, 8.490, 28.06)
			camera.look_at(Vector3(-19.64, 8.25, 18.0), Vector3.UP)
	else:
		var anchor := fish.global_position
		var surface_y: float = atmosphere.SurfaceHeight
		if view == "shore":
			camera.global_position = Vector3(anchor.x + 5.0, surface_y + 1.2, anchor.z + 5.0)
			camera.look_at(Vector3(anchor.x, surface_y + 0.1, anchor.z), Vector3.UP)
		else:
			camera.global_position = Vector3(anchor.x + 3.0, surface_y - 2.0, anchor.z + 4.0)
			camera.look_at(Vector3(anchor.x, surface_y - 3.0, anchor.z - 5.0), Vector3.UP)
	atmosphere.call("RefreshDepth", true, 2.0)

func _fixed_matrix() -> void:
	paused = true
	var hours: Array = HOURS_POND if pond else HOURS_OCEAN
	for hour in hours:
		for view in ["shore", "underwater"]:
			_pose(view)
			_set_hour(hour)
			await _capture("%s_%s_%04d" % ["pond" if pond else "ocean", view, roundi(hour * 100.0)])
		if pond and (hour == 0.0 or hour == 12.0 or hour == 18.0):
			_pose("fish")
			_set_hour(hour)
			await _capture("pond_fish_%04d" % roundi(hour * 100.0))
	paused = false

func _night_pose(view: String) -> void:
	var seabed: Node = main.get_node("Habitat/Seabed")
	var surface_y: float = atmosphere.SurfaceHeight
	var site := Vector2(-19.64, 23.96) if pond else Vector2(initial_fish_position.x, initial_fish_position.z)
	if view == "deep":
		site = Vector2.ZERO
	var floor_y: float = seabed.call("GetHeightAt", site.x, site.y)
	var fish_y := surface_y - 0.85
	if view == "mid" or view == "open" or view == "up":
		fish_y = maxf(floor_y + 1.2, surface_y - 3.0)
	elif view == "cover":
		fish_y = maxf(floor_y + 1.2, surface_y - 1.6)
	elif view == "deep":
		fish_y = floor_y + 1.5
	fish.global_position = Vector3(site.x, fish_y, site.y)
	if view == "open":
		camera.global_position = fish.global_position + Vector3(0.0, 0.6, 3.2)
		camera.look_at(fish.global_position + Vector3(0.0, 0.3, -14.0), Vector3.UP)
	elif view == "up":
		camera.global_position = fish.global_position + Vector3(0.0, -0.1, 3.2)
		camera.look_at(fish.global_position + Vector3(0.0, 2.4, -4.0), Vector3.UP)
	else:
		camera.global_position = fish.global_position + Vector3(0.0, 0.7, 3.2)
		camera.look_at(fish.global_position + Vector3(0.0, 0.0, -1.0), Vector3.UP)
	print("C4_NIGHT_POSE habitat=%s view=%s fish_y=%.3f camera_y=%.3f floor_y=%.3f surface_y=%.3f" % [
		"pond" if pond else "ocean", view, fish_y, camera.global_position.y, floor_y, surface_y])
	atmosphere.call("RefreshDepth", true, 2.0)

func _night_matrix() -> void:
	paused = true
	for hour in [17.5, 20.0, 0.0, 5.0, 12.0]:
		var views := ["near", "mid", "cover", "open", "deep", "up"] if pond else ["near", "mid", "deep", "up"]
		if hour != 0.0 and hour != 12.0:
			views = ["near"]
		for view in views:
			_night_pose(view)
			_set_hour(hour)
			await _capture("%s_night_%s_%04d" % ["pond" if pond else "ocean", view, roundi(hour * 100.0)])
	paused = false

func _capture(label: String) -> void:
	for i in SETTLE_FRAMES:
		await process_frame
	var image := root.get_texture().get_image()
	var path := output_dir.path_join(capture_prefix + label + ".jpg")
	if image.save_jpg(path, 0.90) != OK:
		_fail("could not save " + path)
		return
	var sun_direction: Vector3 = sun.global_transform.basis.z.normalized()
	var surface_direction: Vector3 = surface.get_shader_parameter("sun_direction")
	var shaft_direction: Vector3 = shafts.get_shader_parameter("sun_direction")
	print("C4_NATIVE_FRAME image=%s hour=%.3f view=%s sun_dir=%s direction_error=%.5f shaft_error=%.5f sun_energy=%.3f ambient=%.3f sky_horizon=%s reflected_horizon=%s surface_daylight=%.3f surface_illumination=%.3f shaft_energy=%.3f caustic_energy=%.3f fog_density=%.3f fog_color=%s surface_time=%.3f" % [
		path, clock.HourOfDay, label, sun_direction,
		surface_direction.distance_to(sun_direction), shaft_direction.distance_to(sun_direction),
		sun.light_energy, environment.ambient_light_energy,
		sky.get_shader_parameter("air_horizon"), surface.get_shader_parameter("reflected_sky_horizon"),
		surface.get_shader_parameter("surface_daylight"), surface.get_shader_parameter("surface_illumination"),
		shafts.get_shader_parameter("shaft_brightness"), caustic.emission_energy,
		environment.fog_density, environment.fog_light_color, surface.get_shader_parameter("surface_time")])

func _shore_repro() -> void:
	paused = true
	_pose("far_shore")
	for hour in [0.0, 5.0, 5.5, 6.0, 12.0, 17.0, 17.5, 18.0, 18.5]:
		_set_hour(hour)
		await _capture("pond_far_shore_%04d" % roundi(hour * 100.0))
	paused = false

func _continuous_sweep(view: String = "shore") -> void:
	_pose(view)
	_set_hour(0.0)
	clock.call("SetAccelerationForDebug", 120.0)
	var surface_id := surface.get_instance_id()
	var sky_id := sky.get_instance_id()
	var previous_hour: float = clock.HourOfDay
	var previous_horizon: Vector3 = sky.get_shader_parameter("air_horizon")
	var wraps := 0
	var frames := 0
	var max_step := 0.0
	var max_horizon_step := 0.0
	var max_direction_error := 0.0
	while wraps == 0 and frames < 1200:
		await process_frame
		var hour: float = clock.HourOfDay
		var step: float = hour - previous_hour
		if step < 0.0:
			step += 24.0
			wraps += 1
		max_step = maxf(max_step, step)
		var horizon: Vector3 = sky.get_shader_parameter("air_horizon")
		max_horizon_step = maxf(max_horizon_step, horizon.distance_to(previous_horizon))
		var direction: Vector3 = sun.global_transform.basis.z.normalized()
		max_direction_error = maxf(max_direction_error,
			direction.distance_to(surface.get_shader_parameter("sun_direction")))
		max_direction_error = maxf(max_direction_error,
			direction.distance_to(shafts.get_shader_parameter("sun_direction")))
		if frames % 45 == 0:
			print("C4_SWEEP_SAMPLE frame=%d hour=%.3f sun_energy=%.3f sky_horizon=%s surface_light=%.3f shafts=%.3f" % [
				frames, hour, sun.light_energy, horizon,
				surface.get_shader_parameter("surface_illumination"),
				shafts.get_shader_parameter("shaft_brightness")])
		if frames % 30 == 0:
			var frame_path := output_dir.path_join(capture_prefix + "pond_sweep_%03d.jpg" % frames)
			if root.get_texture().get_image().save_jpg(frame_path, 0.85) != OK:
				_fail("could not save sweep frame " + frame_path)
				return
		previous_hour = hour
		previous_horizon = horizon
		frames += 1
	print("C4_SWEEP_RESULT frames=%d wraps=%d max_hour_step=%.4f max_horizon_step=%.4f max_direction_error=%.5f surface_id_stable=%s sky_id_stable=%s" % [
		frames, wraps, max_step, max_horizon_step, max_direction_error,
		surface_id == surface.get_instance_id(), sky_id == sky.get_instance_id()])
	if wraps != 1 or surface_id != surface.get_instance_id() or sky_id != sky.get_instance_id() or max_direction_error > 0.001:
		_fail("cycle did not complete with coherent solar resources")
		return

func _pause_probe() -> void:
	_pose("underwater")
	_set_hour(12.0)
	clock.call("SetAccelerationForDebug", 120.0)
	var shell := main.get_node("PlayShell")
	var options := shell.find_child("OptionsButton", true, false) as Button
	var resume := shell.find_child("ResumeButton", true, false) as Button
	if options == null or resume == null:
		_fail("PlayShell pause buttons not found")
		return
	for cycle in 3:
		for frame in 20:
			await process_frame
		var before_hour: float = clock.HourOfDay
		var before_surface: float = surface.get_shader_parameter("surface_time")
		options.emit_signal("pressed")
		for frame in 20:
			await process_frame
		var frozen_hour: float = clock.HourOfDay
		var frozen_surface: float = surface.get_shader_parameter("surface_time")
		var paused_ok := paused and absf(frozen_hour - before_hour) < 0.000001 and absf(frozen_surface - before_surface) < 0.000001
		resume.emit_signal("pressed")
		for frame in 20:
			await process_frame
		var resumed_hour: float = clock.HourOfDay
		var resumed_surface: float = surface.get_shader_parameter("surface_time")
		var resumed_ok := not paused and resumed_hour > frozen_hour and resumed_surface > frozen_surface
		print("C4_PAUSE cycle=%d frozen=%s resumed=%s before_hour=%.5f frozen_hour=%.5f resumed_hour=%.5f before_surface=%.5f frozen_surface=%.5f resumed_surface=%.5f" % [
			cycle, paused_ok, resumed_ok, before_hour, frozen_hour, resumed_hour,
			before_surface, frozen_surface, resumed_surface])
		if not paused_ok or not resumed_ok:
			_fail("PlayShell pause/resume did not freeze and resume solar water")
			return

func _focus_probe() -> void:
	root.mode = Window.MODE_WINDOWED
	root.size = Vector2i(1280, 720)
	_pose("underwater")
	_set_hour(12.0)
	clock.call("SetAccelerationForDebug", 120.0)
	print("C4_FOCUS_READY focused=%s" % root.has_focus())
	await create_timer(5.0).timeout
	var first_hour: float = clock.HourOfDay
	var first_surface: float = surface.get_shader_parameter("surface_time")
	var first_paused := paused
	await create_timer(2.0).timeout
	var second_hour: float = clock.HourOfDay
	var second_surface: float = surface.get_shader_parameter("surface_time")
	var frozen := first_paused and paused and absf(second_hour - first_hour) < 0.000001 and absf(second_surface - first_surface) < 0.000001
	print("C4_FOCUS_RESULT focused=%s paused=%s frozen=%s first_hour=%.5f second_hour=%.5f first_surface=%.5f second_surface=%.5f" % [
		root.has_focus(), paused, frozen, first_hour, second_hour, first_surface, second_surface])
	if not frozen:
		_fail("native focus-out did not freeze solar water")
