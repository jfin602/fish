extends SceneTree

func _initialize() -> void:
	call_deferred("_run")

func fail(message: String) -> void:
	printerr("ATMOSPHERE_CHECK_FAILED ", message)
	quit(1)

func _run() -> void:
	var config := ConfigFile.new()
	if config.load("res://project.godot") != OK:
		fail("tracked project settings could not load")
		return
	var saved_method: String = config.get_value("rendering", "renderer/rendering_method", "")
	var features: PackedStringArray = config.get_value("application", "config/features", PackedStringArray())
	if saved_method != "forward_plus" or not features.has("Forward Plus") or config.has_section_key("rendering", "renderer/rendering_method.mobile"):
		fail("tracked production renderer is not Forward+ only")
		return
	if RenderingServer.get_current_rendering_method() != "forward_plus":
		fail("production scene requires Forward+")
		return
	var packed: PackedScene = load("res://scenes/main.tscn")
	var main: Node3D = packed.instantiate()
	root.add_child(main)
	current_scene = main
	await process_frame
	var atmosphere: Node3D = main.get_node("UnderwaterAtmosphere")
	var env: Environment = main.get_node("WorldEnvironment").environment
	var fish: CharacterBody3D = main.get_node("Fish")
	var camera: Camera3D = main.get_node("Fish/CameraRig/Camera3D")
	var effects: Node3D = atmosphere.get_node("WaterEffects")
	var motes: GPUParticles3D = effects.get_node("SuspendedMotes")
	var screen: CanvasLayer = atmosphere.get_node("WaterScreen")
	var rect: ColorRect = screen.get_node("ScreenTreatment")
	var shell: Node = main.get_node("PlayShell")
	var shell_canvas: CanvasLayer = null
	for child in shell.get_children():
		if child is CanvasLayer:
			shell_canvas = child
	if not atmosphere.ParametersAreFinite() or env == null or not env.volumetric_fog_enabled or not env.fog_enabled:
		fail("finite Forward+ fog configuration missing")
		return
	if env.volumetric_fog_density <= 0.0 or env.volumetric_fog_density > 0.06 or env.volumetric_fog_length > 90.0:
		fail("fog exceeds bounds")
		return
	if motes.amount != 128 or motes.lifetime > 30.0 or motes.process_material == null or motes.draw_pass_1 == null or not motes.emitting:
		fail("GPU motes are absent or unbounded")
		return
	var projections := 0
	for child in effects.get_children():
		if child is Decal:
			projections += 1
			if child.texture_emission == null or child.emission_energy > 0.8 or child.size.x > 84.0:
				fail("caustic projection missing or unbounded")
				return
	if projections != 1 or shell_canvas == null:
		fail("caustics or UI layer missing")
		return
	var reef: Node3D = main.get_node("Habitat/Composition/ReefComposition")
	for section_name in ["Rocks", "Corals"]:
		for piece in reef.get_node(section_name).get_children():
			var position: Vector3 = piece.global_position
			if absf(position.x) > 42.0 or absf(position.z) > 42.0 or position.y < -10.0 or position.y > 14.0:
				fail("prepared rock/coral anchor falls outside caustic projection")
				return
	if screen.layer >= main.get_node("PrototypeHelp").layer or screen.layer >= shell_canvas.layer or rect.mouse_filter != Control.MOUSE_FILTER_IGNORE:
		fail("water screen can distort or intercept UI")
		return
	if rect.material == null or rect.material.shader == null or not ResourceLoader.exists("res://shaders/underwater_screen.gdshader"):
		fail("screen shader missing")
		return
	var before_position := fish.global_position
	var before_velocity := fish.velocity
	var before_camera := camera.get_instance_id()
	var before_tuning: Resource = fish.get("Tuning")
	atmosphere.SetEnabled(false)
	atmosphere.SetEnabled(true)
	if fish.global_position != before_position or fish.velocity != before_velocity or camera.get_instance_id() != before_camera or fish.get("Tuning") != before_tuning:
		fail("presentation toggle changed authoritative fish/camera state")
		return
	root.size = Vector2i(960, 540)
	root.size_changed.emit()
	if rect.size != root.get_visible_rect().size:
		fail("screen treatment did not resize with viewport")
		return
	var caustic_before: Vector3 = effects.get_node("CausticProjection1").position
	paused = true
	await create_timer(0.15).timeout
	if effects.get_node("CausticProjection1").position != caustic_before:
		fail("presentation time advanced while paused")
		return
	paused = false
	current_scene = null
	main.queue_free()
	await process_frame
	var missing: Node3D = packed.instantiate()
	missing.get_node("UnderwaterAtmosphere").set("ScreenShaderPath", "res://shaders/optional-missing.gdshader")
	root.add_child(missing)
	current_scene = missing
	await process_frame
	var missing_atmosphere: Node3D = missing.get_node("UnderwaterAtmosphere")
	if missing_atmosphere.get_node_or_null("WaterScreen") != null or not missing.get_node("WorldEnvironment").environment.volumetric_fog_enabled:
		fail("missing optional screen shader disabled required fog or left a broken layer")
		return
	print("ATMOSPHERE_CHECK_OK forward_plus=required fog=bounded caustics=1 motes=gpu ui=above-screen resize=responsive motor=unchanged")
	quit()
