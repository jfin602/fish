shader_type spatial;
render_mode unshaded, cull_disabled, blend_mix, fog_disabled;

// The surface is viewed from below. Time is supplied by the atmosphere so pause freezes it.
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
uniform vec3 air_fog_color = vec3(0.43, 0.48, 0.46);
uniform sampler2D air_scene : hint_screen_texture, filter_linear;
uniform sampler2D air_depth : hint_depth_texture, filter_nearest;

varying vec3 surface_world;
varying vec3 surface_normal;
varying float shore_blend;

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
    float upward = smoothstep(0.15, 0.65, ray_up.y);
    float deep_cue = camera_depth * upward;
    // Only the overhead interface keeps transmitted light through deep water.
    // Horizontal geometry still uses the unchanged water-column fog.
    ALBEDO = turquoise + mix(vec3(0.22, 0.44, 0.40), vec3(0.12, 0.26, 0.11), overcast) * fresnel
             + (1.0 - 0.7 * overcast) * (vec3(0.55, 0.66, 0.52) * halo + vec3(0.55, 0.58, 0.43) * glint)
             + deep_cue * (mix(vec3(0.09, 0.24, 0.25), vec3(0.08, 0.16, 0.06), overcast)
             + mix(vec3(0.26, 0.31, 0.20), vec3(0.20, 0.22, 0.10), overcast) * halo);
    float edge_distance = max(abs(surface_world.x), abs(surface_world.z));
    float edge_fade = 1.0 - smoothstep(surface_half_size - 16.0,
        surface_half_size - 2.0, edge_distance);
    ALPHA = mix(mix(0.62, 0.88, fresnel), 0.9, deep_cue * 0.65) * edge_fade * mix(1.0, smoothstep(0.0, 0.55, shore_blend), shore_mask_enabled);
    // The same plane is visible from air. Keep it translucent so a fish just
    // beneath the interface remains readable when the camera looks down.
    if (FRONT_FACING && air_surface_enabled < 0.5) discard;
    if (FRONT_FACING) {
        float air_fresnel = pow(1.0 - abs(dot(normal, ray_up)), 3.0);
        vec2 bend = vec2(normal.x, -normal.z) * 0.003;
        vec3 refracted = texture(air_scene, clamp(SCREEN_UV + bend,
            vec2(0.002), vec2(0.998))).rgb;
        float scene_depth = texture(air_depth, SCREEN_UV).r;
        vec4 scene_view = INV_PROJECTION_MATRIX * vec4(SCREEN_UV * 2.0 - 1.0, scene_depth, 1.0);
        vec4 surface_view = INV_PROJECTION_MATRIX * vec4(SCREEN_UV * 2.0 - 1.0, FRAGCOORD.z, 1.0);
        float water_path = scene_depth <= 0.0001 ? 30.0 :
            max(0.0, surface_view.z / surface_view.w - scene_view.z / scene_view.w);
        vec3 water_tint = mix(surface_dark, surface_light, 0.45 + 0.20 * ripple)
                        + vec3(0.13, 0.15, 0.11) * air_fresnel;
        ALBEDO = mix(refracted, water_tint, mix(0.16, 0.50, air_fresnel));
        float air_distance = distance(CAMERA_POSITION_WORLD, surface_world);
        float air_fog = smoothstep(air_fog_begin, air_fog_end, air_distance);
        ALBEDO = mix(ALBEDO, air_fog_color, air_fog * 0.94);
        // The depth buffer contains the opaque fish/terrain behind this transparent
        // interface. Short water paths transmit them; open/deep water keeps its sheen.
        float absorption = 1.0 - exp(-water_path * 0.22);
        float air_opacity = clamp(mix(0.14, 0.86, absorption) + 0.18 * air_fresnel, 0.14, 0.92);
        ALPHA = air_opacity * edge_fade * smoothstep(0.0, 0.55, shore_blend);
    }
}
