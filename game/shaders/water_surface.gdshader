shader_type spatial;
render_mode unshaded, cull_disabled, blend_mix, fog_disabled;

// Time is supplied by the atmosphere so pause freezes the waves.
uniform float surface_time = 0.0;
uniform vec3 sun_direction = vec3(0.0, 1.0, 0.0);
uniform float wave_height = 1.0;
uniform float wave_speed = 1.0;
uniform float wave_detail = 0.8;
uniform float camera_depth = 0.0;
uniform float surface_half_size = 45.5;
uniform vec3 surface_dark = vec3(0.08, 0.42, 0.49);
uniform vec3 surface_light = vec3(0.20, 0.66, 0.67);
uniform float overcast = 0.0;
uniform float air_surface_enabled = 0.0;
uniform float shore_mask_enabled = 0.0;
uniform float air_fog_begin = 27.0;
uniform float air_fog_end = 105.0;
uniform vec3 water_scatter = vec3(0.22, 0.28, 0.105);
uniform float surface_extinction = 0.0;
uniform float surface_daylight = 1.0;
uniform float surface_illumination = 1.0;
uniform vec3 night_surface_glow = vec3(0.0);
uniform float reflection_sun_energy = 1.0;
uniform vec3 reflection_sun_color = vec3(1.0, 0.9, 0.7);
uniform float reflection_strength = 1.0;
uniform float reflection_distortion = 1.0;
uniform float ripple_strength = 1.0;
uniform float fine_normal_strength = 1.0;
uniform vec3 reflected_sky_horizon = vec3(0.48, 0.52, 0.51);
uniform vec3 reflected_sky_zenith = vec3(0.65, 0.68, 0.65);
uniform sampler2D air_scene : hint_screen_texture, filter_linear;
uniform sampler2D air_depth : hint_depth_texture, filter_nearest;

varying vec3 surface_world;
varying vec3 surface_normal;
varying float shore_blend;

vec3 pond_sky(vec3 direction) {
    float height = smoothstep(0.0, 0.9, direction.y);
    vec3 base = mix(reflected_sky_horizon, reflected_sky_zenith, height);
    float solar = max(dot(direction, normalize(sun_direction)), 0.0);
    return base + reflection_sun_color * reflection_sun_energy *
        (0.24 * pow(solar, 28.0) + 0.46 * pow(solar, 180.0));
}

vec4 pond_screen_reflection(vec3 origin, vec3 direction, float water_y,
    mat4 view_matrix, mat4 projection_matrix, mat4 inv_projection_matrix,
    mat4 inv_view_matrix) {
    // Only opaque, above-water pixels can be recovered from the screen buffer.
    // Twelve bounded taps avoid a second camera and full-size render target.
    float travel = 0.38;
    for (int i = 0; i < 12; i++) {
        vec3 point_view = (view_matrix * vec4(origin + direction * travel, 1.0)).xyz;
        vec4 clip = projection_matrix * vec4(point_view, 1.0);
        if (clip.w <= 0.0) break;
        vec2 uv = clip.xy / clip.w * 0.5 + 0.5;
        if (min(min(uv.x, uv.y), min(1.0 - uv.x, 1.0 - uv.y)) <= 0.015) break;
        float depth = texture(air_depth, uv).r;
        if (depth <= 0.0001) {
            travel *= 1.48;
            continue;
        }
        vec4 hit_view = inv_projection_matrix * vec4(uv * 2.0 - 1.0, depth, 1.0);
        hit_view /= hit_view.w;
        float behind = hit_view.z - point_view.z;
        if (behind < 0.0 || behind > 2.8) {
            travel *= 1.48;
            continue;
        }
        vec3 hit_world = (inv_view_matrix * hit_view).xyz;
        if (hit_world.y <= water_y + 0.08) {
            travel *= 1.48;
            continue;
        }
        float edge = smoothstep(0.015, 0.10,
            min(min(uv.x, uv.y), min(1.0 - uv.x, 1.0 - uv.y)));
        float confidence = edge * (1.0 - smoothstep(16.0, 35.0, travel));
        return vec4(texture(air_scene, uv).rgb, confidence);
    }
    return vec4(0.0);
}

void vertex() {
    float t = surface_time * wave_speed;
    float a = VERTEX.x * 0.19 + VERTEX.z * 0.07 + t * 0.34;
    float b = VERTEX.x * -0.11 + VERTEX.z * 0.17 - t * 0.27;
    float c = VERTEX.x * 0.31 - VERTEX.z * 0.24 + t * 0.47;
    shore_blend = mix(1.0, COLOR.r, shore_mask_enabled);
    VERTEX.y += shore_blend * wave_height * (0.10 * sin(a) + 0.065 * sin(b) + wave_detail * 0.045 * sin(c));
    vec3 slope = vec3(wave_height * (0.019 * cos(a) - 0.00715 * cos(b) + wave_detail * 0.01395 * cos(c)),
                      1.0,
                      wave_height * (0.007 * cos(a) + 0.01105 * cos(b) - wave_detail * 0.0108 * cos(c)));
    surface_normal = normalize(mat3(MODEL_MATRIX) * normalize(vec3(-slope.x, 1.0, -slope.z)));
    surface_world = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    vec3 ray_up = normalize(surface_world - CAMERA_POSITION_WORLD);
    float t = surface_time * wave_speed;
    float ripple = sin(surface_world.x * 0.57 + surface_world.z * 0.23 + t * 0.53)
                 * sin(surface_world.z * 0.43 - surface_world.x * 0.14 - t * 0.39);
    float detail = sin(surface_world.x * 1.17 - surface_world.z * 0.68 - t * 0.61)
                 * sin(surface_world.z * 0.94 + surface_world.x * 0.32 + t * 0.44);
    vec3 normal = normalize(surface_normal + vec3(0.025 * ripple + 0.018 * wave_detail * detail,
        0.0, -0.02 * ripple + 0.016 * wave_detail * detail));
    float fresnel = pow(1.0 - clamp(dot(normal, ray_up), 0.0, 1.0), 3.0);
    float sun = max(dot(ray_up, normalize(sun_direction)), 0.0);
    float halo = pow(sun, 18.0);
    float glint = pow(sun, 150.0) * (0.7 + 0.3 * ripple);
    vec3 turquoise = mix(surface_dark, surface_light,
        clamp(0.5 + 0.35 * ripple + 0.15 * wave_detail * detail, 0.0, 1.0));
    // Pond's underside scatters muddy olive light instead of reading as a bright pane.
    turquoise = mix(turquoise, water_scatter, overcast * 0.85);
    float upward = smoothstep(0.15, 0.65, ray_up.y);
    float deep_cue = camera_depth * upward;
    // Only the overhead interface keeps transmitted light through deep water.
    // Horizontal geometry uses the independently tuned water-column fog.
    ALBEDO = surface_illumination * (turquoise + mix(vec3(0.22, 0.44, 0.40), vec3(0.025, 0.055, 0.020), overcast) * fresnel
             + reflection_sun_color * reflection_sun_energy * (1.0 - 0.95 * overcast) * (vec3(0.55, 0.66, 0.52) * halo + vec3(0.55, 0.58, 0.43) * glint)
             + deep_cue * (mix(vec3(0.09, 0.24, 0.25), vec3(0.08, 0.16, 0.06), overcast)
             + mix(vec3(0.26, 0.31, 0.20), vec3(0.20, 0.22, 0.10), overcast) * halo));
    // A faint scattered night-sky cue belongs to the overhead interface only.
    ALBEDO += night_surface_glow * upward * (1.0 - 0.55 * camera_depth)
        * (0.78 + 0.22 * ripple);
    float edge_distance = max(abs(surface_world.x), abs(surface_world.z));
    float edge_fade = 1.0 - smoothstep(surface_half_size - 16.0,
        surface_half_size - 2.0, edge_distance);
    ALPHA = mix(mix(0.62, 0.88, fresnel), 0.9, deep_cue * 0.65) * edge_fade * mix(1.0, smoothstep(0.0, 0.55, shore_blend), shore_mask_enabled);
    // The air side explicitly composes shallow transmission from opaque
    // color/depth, so a fish just beneath the interface remains readable.
    if (FRONT_FACING && air_surface_enabled < 0.5) discard;
    if (FRONT_FACING) {
        vec2 broad = vec2(0.026, 0.016) * cos(surface_world.x * 0.71 + surface_world.z * 0.24 + t * 0.63)
            + vec2(-0.019, 0.034) * cos(surface_world.x * -0.34 + surface_world.z * 1.21 - t * 0.41);
        vec2 fine = vec2(0.019, -0.029) * cos(surface_world.x * 1.92 - surface_world.z * 0.83 + t * 0.97)
            + vec2(-0.014, -0.018) * cos(surface_world.x * -2.83 - surface_world.z * 1.52 - t * 1.37)
            + vec2(0.012, 0.007) * cos(surface_world.x * 4.17 + surface_world.z * 2.21 + t * 1.61);
        vec2 slopes = (broad * ripple_strength + fine * fine_normal_strength) * shore_blend;
        vec3 air_normal = normalize(surface_normal + vec3(slopes.x, 0.0, slopes.y));
        float facing = clamp(abs(dot(air_normal, ray_up)), 0.0, 1.0);
        float air_fresnel = 0.025 + 0.975 * pow(1.0 - facing, 4.0);
        vec2 bend = vec2(normal.x, -normal.z) * 0.003;
        vec2 refracted_uv = clamp(SCREEN_UV + bend, vec2(0.002), vec2(0.998));
        vec3 refracted = texture(air_scene, refracted_uv).rgb;
        float scene_depth = texture(air_depth, refracted_uv).r;
        vec4 scene_view = INV_PROJECTION_MATRIX * vec4(refracted_uv * 2.0 - 1.0, scene_depth, 1.0);
        vec4 surface_view = INV_PROJECTION_MATRIX * vec4(SCREEN_UV * 2.0 - 1.0, FRAGCOORD.z, 1.0);
        vec3 entry_view = surface_view.xyz / surface_view.w;
        vec3 target_view = scene_view.xyz / scene_view.w;
        vec3 target_world = (INV_VIEW_MATRIX * vec4(target_view, 1.0)).xyz;
        bool opaque_water_target = scene_depth > 0.0001 && target_world.y < surface_world.y - 0.01;
        float water_path = opaque_water_target ? distance(entry_view, target_view) : 30.0;
        float transmission = exp(-surface_extinction * max(water_path, 0.0) /
            max(surface_daylight, 0.5));
        // A dry bank or overhanging branch must not be colored as submerged scenery.
        if (scene_depth > 0.0001 && !opaque_water_target) transmission = 1.0;
        vec3 water_tint = mix(surface_dark, surface_light, 0.45 + 0.20 * ripple);
        vec3 scattered = mix(water_scatter, water_tint, 0.03) * surface_illumination;
        vec3 transmitted = mix(scattered, refracted, transmission);
        vec3 reflected_direction = reflect(ray_up, air_normal);
        float reflect_mix = clamp(air_fresnel * reflection_strength, 0.0, 0.92);
        vec3 reflected = pond_sky(reflected_direction);
        if (reflect_mix > 0.06) {
            vec4 scene_reflection = pond_screen_reflection(surface_world + air_normal * 0.06,
                normalize(mix(reflected_direction, reflect(ray_up, surface_normal),
                    1.0 - reflection_distortion)), surface_world.y,
                VIEW_MATRIX, PROJECTION_MATRIX, INV_PROJECTION_MATRIX, INV_VIEW_MATRIX);
            reflected = mix(reflected, scene_reflection.rgb, scene_reflection.a);
        }
        // Overcast Pond light is subdued; preserve dark bank silhouettes while
        // letting open sky remain brighter than the absorbed water body.
        reflected = pow(max(reflected, vec3(0.0)), vec3(2.2))
            * vec3(0.72, 0.77, 0.57) + water_scatter * (0.07 * surface_illumination);
        ALBEDO = mix(transmitted, reflected, reflect_mix);
        float air_distance = distance(CAMERA_POSITION_WORLD, surface_world);
        float air_fog = smoothstep(air_fog_begin, air_fog_end, air_distance);
        ALBEDO = mix(ALBEDO, mix(water_scatter, reflected, 0.40), air_fog * 0.35);
        ALPHA = edge_fade * smoothstep(0.0, 0.75, shore_blend);
        if (scene_depth > 0.0001 && !opaque_water_target) ALPHA = 0.0;
    }
}
