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
	var surface: MeshInstance3D = effects.get_node("OverheadSurface")
	var shafts: MeshInstance3D = effects.get_node("SunShafts")
	var upper: CollisionShape3D = main.get_node("Habitat/HiddenLimits/Upper")
	var upper_box: BoxShape3D = upper.shape
	var motes: GPUParticles3D = effects.get_node("SuspendedMotes")
	var screen: CanvasLayer = atmosphere.get_node("WaterScreen")
	var rect: ColorRect = screen.get_node("ScreenTreatment")
	var shell: Node = main.get_node("PlayShell")
	if shell.find_child("GfxButton", true, false) == null or shell.find_child("GraphicsPanel", true, false) == null or shell.find_child("LightingPresetChoice", true, false) == null:
		fail("graphics controls or lighting preset UI missing")
		return
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
	if env.background_mode != Environment.BG_SKY or env.sky == null or not env.sky.sky_material is ShaderMaterial:
		fail("procedural sky is missing")
		return
	var sky_material: ShaderMaterial = env.sky.sky_material
	if sky_material.shader == null or sky_material.shader.resource_path != "res://shaders/underwater_sky.gdshader":
		fail("sky shader did not load")
		return
	if not surface.mesh is PlaneMesh or not surface.material_override is ShaderMaterial:
		fail("single mesh/material surface is missing")
		return
	var plane: PlaneMesh = surface.mesh
	var surface_material: ShaderMaterial = surface.material_override
	if surface_material.shader == null or surface_material.shader.resource_path != "res://shaders/water_surface.gdshader":
		fail("surface shader did not load")
		return
	var sun_direction: Vector3 = main.get_node("Sun").global_transform.basis.z.normalized()
	if (surface_material.get_shader_parameter("sun_direction") as Vector3).distance_to(sun_direction) > 0.001 or (sky_material.get_shader_parameter("sun_direction") as Vector3).distance_to(sun_direction) > 0.001:
		fail("surface/sky sun cue diverged from the directional light")
		return
	if plane.subdivide_width != 48 or plane.subdivide_depth != 48 or plane.size != Vector2(upper_box.size.x + 14.0, upper_box.size.z + 14.0):
		fail("surface mesh does not cover the shoreline with bounded wave detail")
		return
	if absf(surface_material.get_shader_parameter("surface_half_size") - plane.size.x * 0.5) > 0.001:
		fail("overhead surface edge fade is not anchored to its actual mesh")
		return
	var upper_top: float = upper.global_position.y + upper_box.size.y * 0.5
	if surface.global_position.y - 0.242 <= upper_top or surface.global_position.y > upper_top + 0.45 or surface.global_position.y < 9.0:
		fail("surface is not just above the saved upper collision")
		return
	if surface.get_child_count() != 0 or effects.get_child_count() != 4:
		fail("surface added physics or extra presentation geometry")
		return
	if not shafts.mesh is ArrayMesh or shafts.mesh.get_surface_count() != 1 or not shafts.material_override is ShaderMaterial or shafts.cast_shadow != GeometryInstance3D.SHADOW_CASTING_SETTING_OFF:
		fail("sun shafts must be one shadowless mesh/material")
		return
	var shaft_material: ShaderMaterial = shafts.material_override
	if shaft_material.shader == null or shaft_material.shader.resource_path != "res://shaders/sun_shafts.gdshader":
		fail("shaft shader did not load")
		return
	var surface_y: float = shaft_material.get_shader_parameter("water_surface_y")
	var deep_y: float = shaft_material.get_shader_parameter("water_deep_y")
	if not is_finite(surface_y) or not is_finite(deep_y) or absf(surface_y - atmosphere.SurfaceHeight) > 0.001 or absf(deep_y - atmosphere.DeepReferenceHeight) > 0.001 or deep_y >= surface_y:
		fail("shaft depth anchors do not match the water column")
		return
	var deep_strength: float = shaft_material.get_shader_parameter("deep_transmission")
	if not is_finite(deep_strength) or deep_strength <= 0.0 or deep_strength > 0.15 or not shaft_material.shader.code.contains("world_height"):
		fail("deep shaft contribution is not attenuated")
		return
	var shaft_vertices: PackedVector3Array = shafts.mesh.surface_get_arrays(0)[Mesh.ARRAY_VERTEX]
	if shaft_vertices.size() != 48 or atmosphere.get("ShaftCount") != 6:
		fail("shaft geometry is not eight bounded ribbons with six visible by default")
		return
	var ray: Vector3 = shaft_vertices[2] - shaft_vertices[1]
	if ray.normalized().dot(-sun_direction) < 0.999 or absf(shafts.to_global(shaft_vertices[0]).y - surface.global_position.y) > 0.15:
		fail("shaft rays diverge from sun/surface")
		return
	var lights := 0
	for node in main.find_children("*", "Light3D", true, false):
		lights += 1
	if lights != 1 or atmosphere.get("ScreenWarpPixels") > 0.2:
		fail("extra real light or excessive screen warp")
		return
	if motes.amount != 128 or motes.lifetime > 30.0 or motes.process_material == null or not motes.draw_pass_1 is SphereMesh or not motes.emitting:
		fail("GPU motes are absent or unbounded")
		return
	if motes.draw_pass_1.radial_segments > 6 or motes.draw_pass_1.rings > 3:
		fail("mote geometry exceeds low-detail budget")
		return
	var projections := 0
	var caustic: Decal = null
	for child in effects.get_children():
		if child is Decal:
			projections += 1
			caustic = child
			if child.texture_emission == null or not is_finite(child.emission_energy) or child.emission_energy <= 0.0 or child.emission_energy > 0.8 or child.size != Vector3(100, 22, 100) or absf(child.rotation.y) < 0.05 or child.lower_fade < 0.29:
				fail("caustic projection missing or unbounded")
				return
	if projections != 1 or caustic == null or shell_canvas == null:
		fail("caustics or UI layer missing")
		return
	if not is_finite(atmosphere.get("CausticDrift")) or atmosphere.get("CausticDrift") <= 0.0 or atmosphere.get("CausticDrift") > 0.5:
		fail("caustic drift is not finite and bounded")
		return
	var caustic_image: Image = caustic.texture_emission.get_image()
	if caustic_image == null or caustic_image.get_size() != Vector2i(256, 256) or caustic.texture_albedo != caustic.texture_emission:
		fail("generated caustic resource is absent or inconsistent")
		return
	var caustic_bytes: PackedByteArray = caustic_image.get_data()
	for ribbon in range(6):
		var floor_center: Vector3 = shafts.to_global((shaft_vertices[ribbon * 6 + 2] + shaft_vertices[ribbon * 6 + 5]) * 0.5)
		var decal_floor: Vector3 = caustic.to_local(floor_center)
		var sample_x := clampi(roundi((decal_floor.x / 100.0 + 0.5) * 256.0), 0, 255)
		var sample_y := clampi(roundi((decal_floor.z / 100.0 + 0.5) * 256.0), 0, 255)
		if caustic_image.get_pixel(sample_x, sample_y).a < 0.02 or decal_floor.y < -caustic.size.y * 0.5:
			fail("shaft ribbon does not land on a lit caustic ridge")
			return
	var dark_samples := 0
	var lit_samples := 0
	for sample_y in range(0, 256, 8):
		for sample_x in range(0, 256, 8):
			var alpha: float = caustic_image.get_pixel(sample_x, sample_y).a
			if alpha < 0.01:
				dark_samples += 1
			if alpha > 0.1:
				lit_samples += 1
	if dark_samples < 100 or lit_samples < 20:
		fail("generated texture lacks bounded gaps or lit detail")
		return
	var reef: Node3D = main.get_node("Habitat/Composition/ReefComposition")
	for section_name in ["Rocks", "Corals"]:
		for piece in reef.get_node(section_name).get_children():
			var position: Vector3 = caustic.to_local(piece.global_position)
			if absf(position.x) > 49.5 or absf(position.z) > 49.5:
				fail("prepared rock/coral anchor falls outside caustic footprint")
				return
	if caustic.to_local(Vector3(0, main.get_node("Habitat/Seabed").GetHeightAt(0.0, -21.0), -21)).y >= -caustic.size.y * 0.5:
		fail("deep shelf still receives projected caustics")
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
	if effects.visible or env.background_mode != Environment.BG_COLOR:
		fail("surface/sky remained visible with atmosphere off")
		return
	if screen.visible:
		fail("screen layer remained visible with atmosphere off")
		return
	atmosphere.SetEnabled(true)
	if not effects.visible or env.background_mode != Environment.BG_SKY:
		fail("surface/sky did not return with atmosphere on")
		return
	if fish.global_position != before_position or fish.velocity != before_velocity or camera.get_instance_id() != before_camera or fish.get("Tuning") != before_tuning:
		fail("presentation toggle changed authoritative fish/camera state")
		return
	root.size = Vector2i(960, 540)
	root.size_changed.emit()
	if rect.size != root.get_visible_rect().size:
		fail("screen treatment did not resize with viewport")
		return
	var caustic_before: Vector3 = caustic.position
	var shaft_position_before: Vector3 = shafts.position
	var surface_time_before: float = surface_material.get_shader_parameter("surface_time")
	var shaft_time_before: float = shaft_material.get_shader_parameter("shaft_time")
	var screen_material: ShaderMaterial = rect.material
	var screen_time_before: float = screen_material.get_shader_parameter("screen_time")
	paused = true
	await create_timer(0.15).timeout
	if caustic.position != caustic_before or shafts.position != shaft_position_before or surface_material.get_shader_parameter("surface_time") != surface_time_before or shaft_material.get_shader_parameter("shaft_time") != shaft_time_before or screen_material.get_shader_parameter("screen_time") != screen_time_before:
		fail("presentation time advanced while paused")
		return
	paused = false
	atmosphere.SetEnabled(false)
	caustic_before = caustic.position
	await process_frame
	await process_frame
	if effects.visible or caustic.position != caustic_before:
		fail("caustic drift continued while atmosphere was disabled")
		return
	atmosphere.SetEnabled(true)
	await process_frame
	if caustic.position == caustic_before or shafts.position.x != caustic.position.x or shafts.position.z != caustic.position.z or not is_finite(caustic.position.x) or not is_finite(caustic.position.z) or absf(caustic.position.x) > 1.8 or absf(caustic.position.z) > 1.8:
		fail("enabled caustic drift did not resume within bounds")
		return
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
	var regenerated: Decal = missing_atmosphere.get_node("WaterEffects/CausticProjection1")
	if regenerated.texture_emission == null or regenerated.texture_emission.get_image().get_data() != caustic_bytes:
		fail("generated caustic texture changed between scene instances")
		return
	print("ATMOSPHERE_CHECK_OK forward_plus=required fog=bounded surface=shoreline sky=sunlit shafts=6 ribbon_mesh_surfaces=1 shadowless caustics=1 deterministic=gaps drift=paired-pause-toggle motes=low-poly ui=above-screen warp=0.1px resize=responsive motor=unchanged")
	quit()
