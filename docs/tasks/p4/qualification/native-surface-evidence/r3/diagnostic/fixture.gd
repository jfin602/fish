extends Node3D

var fish_root: String
var capture_root: String
var assets: Dictionary = {}
var water_material: ShaderMaterial
var camera: Camera3D
var environment: Environment
var variant: String = "baseline"

func _ready():
    fish_root = OS.get_environment("FISH_DIAGNOSTIC_GAME_ROOT").trim_suffix("/") + "/"
    capture_root = ProjectSettings.globalize_path("res://captures/")
    if fish_root == "/":
        push_error("Set FISH_DIAGNOSTIC_GAME_ROOT to the Fish game directory")
        get_tree().quit(1)
        return
    var args = OS.get_cmdline_user_args()
    var shader_path = "res://baseline/water_surface.gdshader"
    if args.size() > 0: variant = args[0]
    if args.size() > 1: shader_path = args[1]
    setup_environment()
    setup_banks()
    setup_water(shader_path)
    setup_fish()
    camera = Camera3D.new()
    camera.fov = 65.0
    camera.near = 0.05
    camera.far = 180.0
    add_child(camera)
    camera.current = true
    var shots = [
        ["grazing", Vector3(0.0, 0.6, 19.0), Vector3(0.0, 0.4, -15.0)],
        ["above", Vector3(4.0, 4.5, 17.0), Vector3(0.0, -0.1, -3.0)],
        ["fish", Vector3(1.6, 2.4, 8.0), Vector3(0.0, -0.5, 4.0)],
        ["below", Vector3(0.0, -0.8, 15.0), Vector3(0.0, 1.0, -8.0)]
    ]
    if variant.begins_with("refine") or variant.begins_with("flat"):
        shots = shots.slice(0, 2)
    DirAccess.make_dir_recursive_absolute(capture_root)
    for shot in shots:
        camera.position = shot[1]
        camera.look_at(shot[2])
        if shot[0] == "below":
            environment.sky.sky_material.set_shader_parameter("air_mix", 0.0)
            environment.fog_enabled = true
            environment.fog_density = 0.035
            environment.fog_light_color = Color(0.15, 0.22, 0.09)
            environment.ambient_light_color = Color(0.7, 0.75, 0.5)
        for frame in range(4):
            await get_tree().process_frame
        await RenderingServer.frame_post_draw
        var path = capture_root + variant + "-" + shot[0] + ".png"
        var result = get_viewport().get_texture().get_image().save_png(path)
        print("WATER_DIAGNOSTIC_CAPTURE captures/", path.get_file(), " status=", result)
    get_tree().quit()

func setup_environment():
    environment = Environment.new()
    environment.background_mode = Environment.BG_SKY
    var sky = Sky.new()
    var sky_material = ShaderMaterial.new()
    var sky_shader: Shader = load("res://baseline/underwater_sky.gdshader" if variant == "baseline" else "res://shaders/pond_sky.gdshader")
    sky_material.shader = sky_shader
    sky_material.set_shader_parameter("air_mix", 1.0)
    sky_material.set_shader_parameter("overcast", 1.0)
    sky_material.set_shader_parameter("sky_time", 2.5)
    sky_material.set_shader_parameter("sun_direction", Vector3(0.2,0.8,-0.3).normalized())
    sky.sky_material = sky_material
    sky.radiance_size = Sky.RADIANCE_SIZE_128
    environment.sky = sky
    environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
    environment.ambient_light_color = Color(0.75,0.77,0.7)
    environment.ambient_light_energy = 0.78
    environment.tonemap_mode = Environment.TONE_MAPPER_FILMIC
    environment.fog_enabled = false
    var we = WorldEnvironment.new()
    we.environment = environment
    add_child(we)
    var sun = DirectionalLight3D.new()
    sun.rotation_degrees = Vector3(-55,-30,0)
    sun.light_color = Color(0.88,0.87,0.76)
    sun.light_energy = 1.35
    sun.shadow_enabled = false
    add_child(sun)

func add_box(at: Vector3, size: Vector3, color: Color):
    var mesh = MeshInstance3D.new()
    var box = BoxMesh.new()
    box.size = size
    mesh.mesh = box
    var material = StandardMaterial3D.new()
    material.albedo_color = color
    material.roughness = 0.96
    mesh.material_override = material
    mesh.position = at
    add_child(mesh)

func add_asset(relative: String, at: Vector3, scale_amount: float, yaw: float):
    if not assets.has(relative):
        var document = GLTFDocument.new()
        var state = GLTFState.new()
        var error = document.append_from_file(fish_root + "assets/faf/pond/" + relative, state)
        if error != OK:
            push_error("ASSET_READ_FAILED " + relative)
            return
        var prototype = document.generate_scene(state)
        var packed = PackedScene.new()
        packed.pack(prototype)
        prototype.free()
        assets[relative] = packed
    var instance = assets[relative].instantiate()
    add_child(instance)
    instance.position = at
    instance.scale = Vector3.ONE * scale_amount
    instance.rotation.y = yaw

func setup_banks():
    add_box(Vector3(0,-4.25,-5),Vector3(27,0.5,70),Color(0.23,0.19,0.105))
    for side in [-1,1]:
        add_box(Vector3(side*13.6,-1.0,-5),Vector3(8,3,70),Color(0.16,0.135,0.075))
        for i in range(22):
            var z = 22.0 - i*2.4
            var x = side*(10.1 + sin(i*0.72)*0.65)
            add_asset("plants/plant_006b_cattail.glb",Vector3(x,-0.2,z),1.3+(i%3)*0.18,i*2.399)
            add_asset("plants/plant_005b_tall_grass.glb",Vector3(side*12.0,0.5,z-0.7),1.6,i*1.77)
            add_asset("plants/plant_007b_reed.glb",Vector3(x+side*0.6,-0.1,z-0.8),1.3,i*1.3)
            if i%4==0:
                add_asset("wood/wood_003a_rooted_leaning_trunk.glb",Vector3(side*14.0,0.5,z),1.0,i*2.4)
        add_asset("wood/wood_001b_forked_fallen_log.glb",Vector3(side*9.0,-0.25,0),1.0,side*0.5)
    add_asset("plants/plant_004c_lily_pad.glb",Vector3(-6,0,8),1.0,1.0)
    add_asset("plants/plant_004b_lily_pad.glb",Vector3(-7,0,6),1.0,2.0)
    add_asset("ground/detail_005b_algae_stone_upright.glb",Vector3(7,-0.5,8),1.3,0.3)

func setup_water(shader_path: String):
    var surface = MeshInstance3D.new()
    var plane = PlaneMesh.new()
    plane.size = Vector2(22,70)
    plane.subdivide_width = 36
    plane.subdivide_depth = 70
    surface.mesh = plane
    surface.position.z = -5.0
    water_material = ShaderMaterial.new()
    var shader: Shader = load(shader_path)
    water_material.shader = shader
    var params = {
        "surface_time":2.5,"sun_direction":Vector3(0.2,0.8,-0.3).normalized(),
        "wave_height":0.45,"wave_speed":0.65,"wave_detail":0.35,
        "camera_depth":0.0,"surface_half_size":200.0,
        "surface_dark":Color(0.13,0.28,0.12),"surface_light":Color(0.31,0.43,0.20),
        "overcast":1.0,"air_surface_enabled":1.0,"shore_mask_enabled":0.0,
        "air_fog_begin":27.0,"air_fog_end":105.0,
        "water_scatter":Color(0.055,0.085,0.035),"surface_extinction":0.65,"surface_daylight":1.0
    }
    for key in params: water_material.set_shader_parameter(key,params[key])
    if variant == "sky-only": water_material.set_shader_parameter("reflection_scene_enabled",0.0)
    if variant == "reflection-off": water_material.set_shader_parameter("reflection_strength",0.0)
    surface.material_override = water_material
    add_child(surface)

func setup_fish():
    add_asset("fish/fish_006_bluegill__adult.glb",Vector3(0.0,-0.4,4.0),1.0 / 0.309825,1.0)
    add_box(Vector3(-2.0,-1.2,5.0),Vector3(0.65,0.65,0.65),Color(0.9,0.03,0.02))
    add_box(Vector3(5.0,1.2,-5.0),Vector3(0.24,2.6,0.3),Color(0.8,0.52,0.06))
    add_box(Vector3(5.0,2.4,-5.0),Vector3(2.2,0.24,0.3),Color(0.8,0.52,0.06))
