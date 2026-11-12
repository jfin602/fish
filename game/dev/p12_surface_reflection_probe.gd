extends SceneTree

# Native renderer evidence only. Run the built .NET project with:
# --script res://dev/p12_surface_reflection_probe.gd -- --pond-surface-probe
# --surface-output=/absolute/empty/directory [--surface-samples=90]
# [--surface-source=<observed-commit-and-dirty-state>]
# Main.cs's --pond-surface-probe selects the real baked Pond and FAF-FISH-006.
# Fixed diagnostic poses and a paused tree isolate visual A/B comparisons. Their
# frame intervals include presentation/vsync; they are not GPU-only timings or a
# normal-play benchmark. Captures never establish owner approval or a Gate PASS.
const LEGACY_PATH := "res://shaders/water_surface.gdshader"
const CANDIDATE_PATH := "res://shaders/pond_water_surface.gdshader"
const SKY_INCLUDE_PATH := "res://shaders/pond_air_sky.gdshaderinc"
const MANIFEST_PATH := "res://worlds/base/pond-p4-71/manifest.json"
const WARMUP := 30
const FIXED_SURFACE_TIME := 12.0
const COMMON_SETTINGS := [
	"SurfaceWaveHeight", "SurfaceWaveSpeed", "SurfaceWaveDetail", "SunEnergy",
	"AmbientEnergy", "VolumetricDensity", "VisibilityRangeMeters", "CausticEnergy",
	"ShaftCount", "MoteCount", "ScreenWarpPixels",
]

var _output := ""
var _output_ready := false
var _samples := 90
var _source := "NOT_SUPPLIED"
var _report: Dictionary = {"status": "STARTING", "samples": [], "failures": []}
var _main: Node3D
var _fish: CharacterBody3D
var _atmosphere: Node3D
var _camera: Camera3D
var _original_camera: Camera3D
var _material: ShaderMaterial
var _sky_material: ShaderMaterial
var _original_sky_time: Variant
var _original_shader: Shader
var _legacy: Shader
var _candidate: Shader
var _original_uniforms: Dictionary = {}
var _original_fish_transform: Transform3D
var _original_velocity := Vector3.ZERO
var _original_paused := false
var _original_mouse_mode: int
var _shell: Node
var _original_shell_process: int
var _hidden_canvases: Array[Dictionary] = []
var _diagnostic_started := false
var _failed := false
var _capture_size := Vector2i.ZERO

func _initialize() -> void:
	call_deferred("_run")

func _arguments_are_valid() -> bool:
	var arguments := OS.get_cmdline_user_args()
	if not arguments.has("--pond-surface-probe"):
		return _fail("--pond-surface-probe is required; Main.cs selects Pond and FAF-FISH-006 through this flag")
	for argument in arguments:
		if argument.begins_with("--surface-output="):
			_output = argument.trim_prefix("--surface-output=")
		elif argument.begins_with("--surface-samples="):
			var value := argument.trim_prefix("--surface-samples=")
			if not value.is_valid_int() or int(value) < 30 or int(value) > 600:
				return _fail("--surface-samples must be an integer in 30..600")
			_samples = int(value)
		elif argument.begins_with("--surface-source="):
			_source = argument.trim_prefix("--surface-source=").replace("\n", " ").replace("\r", " ")
			if _source.is_empty() or _source.length() > 256:
				return _fail("--surface-source must be a nonempty label of at most 256 characters")
		elif argument.begins_with("--surface-"):
			return _fail("unknown probe argument: " + argument)
		elif argument.begins_with("--world") or argument.begins_with("--pond-generate") or argument.begins_with("--fish=") or argument == "--atmosphere=off":
			return _fail("conflicting world/render argument: " + argument)
		elif argument != "--pond-surface-probe" and not argument.begins_with("--display-mode="):
			return _fail("unexpected game argument for this bounded probe: " + argument)
	if OS.get_environment("FISH_POND_VISUAL_FIXTURE") == "1":
		return _fail("FISH_POND_VISUAL_FIXTURE must be unset; this probe requires the real Bluegill and tracked Pond bake")
	if _output.is_empty() or not _output.is_absolute_path() or _output.begins_with("res://") or _output.begins_with("user://"):
		return _fail("--surface-output requires an absolute filesystem directory")
	if DirAccess.dir_exists_absolute(_output) and (not DirAccess.get_files_at(_output).is_empty() or not DirAccess.get_directories_at(_output).is_empty()):
		return _fail("output directory must be empty; existing evidence will not be overwritten")
	if DirAccess.make_dir_recursive_absolute(_output) != OK:
		return _fail("cannot create output directory: " + _output)
	_output_ready = true
	return true

func _run() -> void:
	if not _arguments_are_valid():
		return
	var display := DisplayServer.get_name()
	var renderer := RenderingServer.get_current_rendering_method()
	var driver := RenderingServer.get_current_rendering_driver_name()
	var adapter := RenderingServer.get_video_adapter_name()
	if display in ["headless", "dummy"] or driver == "dummy" or adapter.is_empty() or renderer != "forward_plus":
		_fail("native Forward+ with a real render target required; display=%s renderer=%s driver=%s adapter=%s" % [display, renderer, driver, adapter])
		return
	if not OS.has_feature("mono"):
		_fail("the built Godot .NET project is required")
		return
	# Also terminates an awaited frame_post_draw if the window stops drawing.
	var timeout_seconds := maxf(180.0, 15.0 * (_samples + WARMUP) / 10.0)
	create_timer(timeout_seconds, true).timeout.connect(func() -> void:
		_fail("native comparison timed out after %.0f seconds" % timeout_seconds))
	_legacy = load(LEGACY_PATH) as Shader
	_candidate = load(CANDIDATE_PATH) as Shader
	if _legacy == null or _candidate == null:
		_fail("legacy or candidate shader missing")
		return
	var manifest: Variant = JSON.parse_string(FileAccess.get_file_as_string(MANIFEST_PATH))
	if not manifest is Dictionary or manifest.get("HabitatId", "") != "pond":
		_fail("tracked Pond manifest missing or invalid")
		return
	_report.merge({
		"source_supplied_by_caller": _source,
		"godot": Engine.get_version_info(), "display": display, "renderer": renderer,
		"driver": driver, "adapter": adapter, "os": OS.get_name(),
		"vsync_mode": DisplayServer.window_get_vsync_mode(), "engine_max_fps": Engine.max_fps,
		"world_id": manifest.get("WorldId"), "seed": manifest.get("Seed"), "fish_id": "FAF-FISH-006",
		"manifest_sha256": FileAccess.get_sha256(MANIFEST_PATH),
		"legacy_shader_sha256": FileAccess.get_sha256(LEGACY_PATH),
		"candidate_shader_sha256": FileAccess.get_sha256(CANDIDATE_PATH),
		"sky_include_sha256": FileAccess.get_sha256(SKY_INCLUDE_PATH),
		"legacy_scope": "Original water shader in the current scene and sky; this is not a full checkpoint-checkout comparison.",
		"samples_per_capture": _samples, "warmup_frames": WARMUP,
		"limitations": "Paused fixed poses; process/render interval includes vsync and scheduling, not isolated GPU time. No movement, camera-feel, offscreen-reflection completeness, world-switch, or owner visual qualification is claimed.",
	})
	var packed := load("res://scenes/main.tscn") as PackedScene
	if packed == null:
		_fail("main.tscn could not be loaded")
		return
	_main = packed.instantiate() as Node3D
	root.add_child(_main)
	current_scene = _main
	await process_frame
	_fish = _main.get_node_or_null("Fish") as CharacterBody3D
	_atmosphere = _main.get_node_or_null("UnderwaterAtmosphere") as Node3D
	var surface := _main.get_node_or_null("UnderwaterAtmosphere/WaterEffects/OverheadSurface") as MeshInstance3D
	var bank := _main.get_node_or_null("Habitat/PondLandmarks/BankApproach") as Marker3D
	var shallow := _main.get_node_or_null("Habitat/PondLandmarks/shallow") as Marker3D
	var seabed := _main.get_node_or_null("Habitat/Seabed") as Node3D
	if _fish == null or _atmosphere == null or surface == null or bank == null or shallow == null or seabed == null or not _fish.has_node("PlayerVisual"):
		_fail("actual Pond, Bluegill visual, atmosphere, surface, or baked landmarks unavailable")
		return
	_material = surface.material_override as ShaderMaterial
	var height_value: Variant = _atmosphere.get("SurfaceHeight")
	var core_value: Variant = seabed.get("DeepCore")
	if _material == null or _material.shader == null or typeof(height_value) != TYPE_FLOAT or typeof(core_value) != TYPE_VECTOR2:
		_fail("surface material or dynamic Pond height/core unavailable")
		return
	var surface_y := float(height_value)
	if not is_finite(surface_y) or absf(surface_y - surface.global_position.y) > 0.001 or not bool(_atmosphere.get("Enabled")):
		_fail("atmosphere is disabled or dynamic surface height disagrees with the mesh")
		return
	if _material.shader.resource_path != CANDIDATE_PATH:
		_fail("Pond is not using the candidate shader; build/import the current implementation first")
		return
	_original_paused = paused
	_original_mouse_mode = Input.mouse_mode
	_original_fish_transform = _fish.global_transform
	_original_velocity = _fish.velocity
	_original_camera = root.get_camera_3d()
	_original_shader = _material.shader
	for uniform in _original_shader.get_shader_uniform_list():
		var uniform_name: String = uniform["name"]
		_original_uniforms[uniform_name] = _material.get_shader_parameter(uniform_name)
	_diagnostic_started = true
	paused = true
	var world_environment := _main.get_node_or_null("WorldEnvironment") as WorldEnvironment
	if world_environment != null and world_environment.environment != null and world_environment.environment.sky != null:
		_sky_material = world_environment.environment.sky.sky_material as ShaderMaterial
		if _sky_material != null:
			_original_sky_time = _sky_material.get_shader_parameter("sky_time")
			_sky_material.set_shader_parameter("sky_time", FIXED_SURFACE_TIME)
	if _sky_material == null or _sky_material.shader == null or typeof(_original_sky_time) != TYPE_FLOAT:
		_fail("Pond sky shader and its pause-aware time uniform are required for a matched reflection comparison")
		return
	_report["sky_shader_path"] = _sky_material.shader.resource_path
	_report["sky_shader_sha256"] = FileAccess.get_sha256(_sky_material.shader.resource_path)
	Input.mouse_mode = Input.MOUSE_MODE_HIDDEN
	_fish.velocity = Vector3.ZERO
	_shell = _main.get_node_or_null("PlayShell")
	if _shell != null:
		_original_shell_process = _shell.process_mode
		_shell.process_mode = Node.PROCESS_MODE_DISABLED
		for child in _shell.get_children():
			if child is CanvasLayer:
				_hide_canvas(child)
	var help := _main.get_node_or_null("PrototypeHelp") as CanvasLayer
	if help != null:
		_hide_canvas(help)
	_camera = Camera3D.new()
	_camera.fov = 70.0
	_camera.near = 0.05
	_camera.far = 150.0
	_main.add_child(_camera)
	_camera.make_current()
	_capture_size = root.size
	var settings: Dictionary = {}
	for setting in COMMON_SETTINGS:
		settings[setting] = _atmosphere.get(setting)
	var uniforms_for_log: Dictionary = {}
	for uniform_name in _original_uniforms:
		uniforms_for_log[uniform_name] = var_to_str(_original_uniforms[uniform_name])
	_report.merge({"surface_y": surface_y, "viewport_size": [_capture_size.x, _capture_size.y],
		"atmosphere_settings": settings, "initial_surface_uniforms": uniforms_for_log,
		"fixed_surface_time": FIXED_SURFACE_TIME})
	print("P12_SURFACE_PROBE_CONTEXT ", JSON.stringify(_report))
	var direction := Vector3(shallow.global_position.x - bank.global_position.x, 0.0,
		shallow.global_position.z - bank.global_position.z).normalized()
	if direction.length_squared() < 0.9:
		_fail("bank/shallow markers do not define a usable direction")
		return
	var core: Vector2 = core_value
	var fish_point := seabed.to_global(Vector3(core.x, 0.0, core.y))
	fish_point.y = surface_y
	var floor_y := seabed.to_global(Vector3(core.x, float(seabed.call("GetHeightAt", core.x, core.y)), core.y)).y
	if not is_finite(floor_y) or surface_y - floor_y < 4.5:
		_fail("deep-core anchor does not provide clearance for the four-meter fish pose")
		return
	_report["fish_probe_floor_y"] = floor_y
	var bank_point := Vector3(bank.global_position.x, surface_y, bank.global_position.z)
	var poses := [
		{"name": "low_bank", "eye": bank_point + direction * 9.0 + Vector3.UP * 0.35,
			"target": bank_point + Vector3.UP * 0.9, "fish": fish_point - Vector3.UP * 0.5},
		{"name": "oblique_bank", "eye": bank_point + direction * 13.0 + Vector3.UP * 3.0,
			"target": bank_point + Vector3.UP * 0.1, "fish": fish_point - Vector3.UP * 0.5},
		{"name": "fish_050cm", "eye": fish_point + Vector3(0.0, 6.0, 3.0),
			"target": fish_point - Vector3.UP, "fish": fish_point - Vector3.UP * 0.5},
		{"name": "fish_400cm", "eye": fish_point + Vector3(0.0, 6.0, 3.0),
			"target": fish_point - Vector3.UP, "fish": fish_point - Vector3.UP * 4.0},
		{"name": "underwater", "eye": fish_point + Vector3(0.0, -1.8, 4.1),
			"target": fish_point - Vector3.UP * 0.15, "fish": fish_point - Vector3.UP * 0.5},
	]
	for pose in poses:
		_camera.global_position = pose["eye"]
		_camera.look_at(pose["target"], Vector3.UP)
		_fish.global_position = pose["fish"]
		_fish.rotation = Vector3.ZERO
		# Force the presentation blend to settle without advancing fish/simulation.
		# RefreshDepth(force, 0) alone does NOT settle the immersion interpolation.
		for step in 3:
			_atmosphere.call("RefreshDepth", true, 1.0)
		var pose_depth: Variant = _material.get_shader_parameter("camera_depth")
		for mode in ["legacy", "candidate", "sky_only"]:
			_material.shader = _legacy if mode == "legacy" else _candidate
			for uniform in _material.shader.get_shader_uniform_list():
				var uniform_name: String = uniform["name"]
				if _original_uniforms.has(uniform_name):
					_material.set_shader_parameter(uniform_name, _original_uniforms[uniform_name])
			_material.set_shader_parameter("camera_depth", pose_depth)
			_material.set_shader_parameter("surface_time", FIXED_SURFACE_TIME)
			if mode != "legacy":
				_material.set_shader_parameter("reflection_strength", 1.0)
				_material.set_shader_parameter("reflection_scene_enabled", 0.0 if mode == "sky_only" else 1.0)
			if not await _capture(pose, mode):
				return
	_report["status"] = "CAPTURED_REQUIRES_VISUAL_REVIEW"
	_report["pause_freeze"] = "PASS: surface/sky time and diagnostic fish/camera transforms remained fixed in every sample"
	_restore()
	if not _save_report():
		printerr("P12_SURFACE_PROBE_FAILED could not save comparison.json")
		quit(1)
		return
	print("P12_SURFACE_PROBE_DONE captures=%d report=%s visual_status=NOT_JUDGED" % [
		_report["samples"].size(), _output.path_join("comparison.json")])
	quit(0)

func _capture(pose: Dictionary, mode: String) -> bool:
	var fish_transform := _fish.global_transform
	var camera_transform := _camera.global_transform
	for frame in WARMUP:
		await process_frame
	var frames: Array[float] = []
	var draws: Array[float] = []
	var primitives: Array[float] = []
	await RenderingServer.frame_post_draw
	var previous := Time.get_ticks_usec()
	for sample in _samples:
		await RenderingServer.frame_post_draw
		var now := Time.get_ticks_usec()
		frames.append(float(now - previous) / 1000.0)
		previous = now
		draws.append(Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME))
		primitives.append(Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME))
		if not paused or absf(float(_material.get_shader_parameter("surface_time")) - FIXED_SURFACE_TIME) > 0.00001:
			return _fail("pause-aware surface time changed during capture")
		if absf(float(_sky_material.get_shader_parameter("sky_time")) - FIXED_SURFACE_TIME) > 0.00001:
			return _fail("pause-aware sky time changed during capture")
		if not _fish.global_transform.is_equal_approx(fish_transform) or not _camera.global_transform.is_equal_approx(camera_transform):
			return _fail("diagnostic fish or camera pose moved during capture")
		if root.size != _capture_size:
			return _fail("viewport resized during A/B capture; rerun at a fixed resolution")
	var pixels := root.get_texture().get_image()
	if pixels == null or pixels.is_empty():
		return _fail("rendered viewport image unavailable")
	var label: String = pose["name"] + "_" + mode
	var path := _output.path_join(label + ".png")
	var error := pixels.save_png(path)
	if error != OK:
		return _fail("PNG save failed: %s error=%d" % [path, error])
	var result := {"view": pose["name"], "mode": mode, "image": path,
		"image_size": [pixels.get_width(), pixels.get_height()],
		"eye": var_to_str(_camera.global_position), "target": var_to_str(pose["target"]),
		"fish_position": var_to_str(_fish.global_position), "fov": _camera.fov,
		"camera_immersion": _atmosphere.get("CameraImmersion01"),
		"frame_ms": _summary(frames), "draw_calls": _summary(draws),
		"primitives": _summary(primitives), "samples": _samples}
	_report["samples"].append(result)
	print("P12_SURFACE_SAMPLE ", JSON.stringify(result))
	return true

func _summary(values: Array[float]) -> Dictionary:
	values.sort()
	var middle := values.size() / 2
	var median := values[middle] if values.size() % 2 != 0 else (values[middle - 1] + values[middle]) * 0.5
	return {"median": median, "p95": values[ceili(values.size() * 0.95) - 1]}

func _hide_canvas(canvas: CanvasLayer) -> void:
	_hidden_canvases.append({"node": canvas, "visible": canvas.visible})
	canvas.visible = false

func _restore() -> void:
	if not _diagnostic_started:
		return
	_material.shader = _original_shader
	for uniform_name in _original_uniforms:
		_material.set_shader_parameter(uniform_name, _original_uniforms[uniform_name])
	if is_instance_valid(_sky_material):
		_sky_material.set_shader_parameter("sky_time", _original_sky_time)
	_fish.global_transform = _original_fish_transform
	_fish.velocity = _original_velocity
	if is_instance_valid(_original_camera):
		_original_camera.make_current()
	if is_instance_valid(_camera):
		_camera.queue_free()
	for entry in _hidden_canvases:
		if is_instance_valid(entry["node"]):
			entry["node"].visible = entry["visible"]
	if is_instance_valid(_shell):
		_shell.process_mode = _original_shell_process
	Input.mouse_mode = _original_mouse_mode
	for step in 3:
		_atmosphere.call("RefreshDepth", true, 1.0)
	paused = _original_paused
	_diagnostic_started = false

func _save_report() -> bool:
	if not _output_ready or _output.is_empty() or not DirAccess.dir_exists_absolute(_output):
		return false
	var file := FileAccess.open(_output.path_join("comparison.json"), FileAccess.WRITE)
	if file == null:
		return false
	file.store_string(JSON.stringify(_report, "\t") + "\n")
	file.flush()
	var error := file.get_error()
	file.close()
	return error == OK

func _fail(reason: String) -> bool:
	if not _failed:
		_failed = true
		_report["status"] = "FAIL"
		_report["failures"].append(reason)
		printerr("P12_SURFACE_PROBE_FAILED ", reason)
		_restore()
		# Invalid arguments must not overwrite a previous comparison report.
		if _output_ready:
			_save_report()
		quit(1)
	return false
