shader_type spatial;
render_mode unshaded, cull_disabled, blend_mix, fog_disabled;

// Pond-only candidate. Ocean retains water_surface.gdshader unchanged.
#include "res://shaders/pond_air_sky.gdshaderinc"

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
uniform sampler2D air_scene : hint_screen_texture, filter_linear;
uniform sampler2D air_depth : hint_depth_texture, filter_nearest;
// Fixed bounds: at most 34 reflection depth reads (initial + 24 coarse + 8
// refinements + final verification), in addition to refraction. No second
// camera, capture, history or scene render; owner-GPU cost still needs measuring.
uniform float reflection_strength : hint_range(0.0, 1.0) = 1.0;
uniform float reflection_scene_enabled : hint_range(0.0, 1.0) = 1.0;
uniform float reflection_max_distance : hint_range(8.0, 70.0) = 65.0;

varying vec3 surface_world;
varying vec3 surface_normal;
varying float shore_blend;

// Analytical derivatives of smooth noise: two cheap, directionally stretched
// fields make irregular wave slopes without equally spaced crossed sine bands.
vec2 pond_wave_gradient(vec2 point) {
    vec2 cell = floor(point);
    vec2 offset = fract(point);
    vec2 blend = offset * offset * (3.0 - 2.0 * offset);
    vec2 derivative = 6.0 * offset * (1.0 - offset);
    float a = pond_cloud_hash(cell);
    float b = pond_cloud_hash(cell + vec2(1.0, 0.0));
    float c = pond_cloud_hash(cell + vec2(0.0, 1.0));
    float d = pond_cloud_hash(cell + vec2(1.0, 1.0));
    return vec2(mix(b - a, d - c, blend.y), mix(c - a, d - b, blend.x)) * derivative;
}

// Broad, nonparallel slopes bend recognizable reflection silhouettes. A slow
// world-space amplitude field creates calmer and more rippled patches; it
// changes roughness, not turbidity. Existing low displacement stays put.
vec3 pond_air_normal(vec3 base_normal, vec2 point, float eye_distance, float time,
                     float shore) {
    float patch = 0.72 + 0.28 * sin(dot(point, vec2(0.047, 0.029))
        + 0.65 * sin(dot(point, vec2(-0.021, 0.039))));
    vec2 along = vec2(0.94, 0.34);
    vec2 across = vec2(-0.34, 0.94);
    vec2 large_point = vec2(dot(point, along) * 0.62, dot(point, across) * 0.27)
        + vec2(time * -0.16, time * 0.047);
    vec2 large = pond_wave_gradient(large_point);
    vec2 second_axis = vec2(0.68, -0.73);
    vec2 second_across = vec2(0.73, 0.68);
    vec2 small_point = vec2(dot(point, second_axis) * 1.37,
        dot(point, second_across) * 0.53) + vec2(time * 0.19, time * -0.071) + vec2(14.3, -6.7);
    vec2 small = pond_wave_gradient(small_point);
    vec2 slope = (along * large.x + across * large.y * 0.44) * 0.16
               + (second_axis * small.x + second_across * small.y * 0.39) * 0.055;
    float detail_fade = 1.0 - smoothstep(8.0, 32.0, eye_distance);
    slope += wave_detail * detail_fade * 0.022 * vec2(
        cos(dot(point, vec2(6.1, 2.7)) + time * 1.31),
        sin(dot(point, vec2(-3.8, 7.3)) - time * 1.17));
    float shore_damping = mix(0.20, 1.0, smoothstep(0.0, 1.0, shore));
    return normalize(base_normal + vec3(-slope.x, 0.0, -slope.y)
        * patch * shore_damping * (0.45 + 1.35 * wave_height));
}

// Forward+ uses reverse-Z [0,1]. Only reconstruct valid opaque samples.
vec3 pond_view_position(vec2 uv, float depth, mat4 inverse_projection) {
    vec4 point = inverse_projection * vec4(uv * 2.0 - 1.0, depth, 1.0);
    return point.xyz / point.w;
}

// RGB is the scene reflection, A is confidence. Missing/hidden/offscreen
// scenery becomes the matching air sky, never an edge-clamped image smear.
vec4 pond_scene_reflection(vec3 entry, vec3 direction, float water_y,
                           mat4 projection, mat4 inverse_projection,
                           mat4 inverse_view) {
    vec3 start = entry + direction * 0.10;
    float ray_length = clamp(reflection_max_distance, 8.0, 70.0);
    vec4 near_point = inverse_projection * vec4(0.0, 0.0, 1.0, 1.0);
    float near_z = near_point.z / near_point.w;
    if (direction.z > 0.0001)
        ray_length = min(ray_length, (near_z * 1.10 - start.z) / direction.z);
    if (ray_length <= 0.15) return vec4(0.0);
    vec3 end = start + direction * ray_length;
    vec4 start_clip = projection * vec4(start, 1.0);
    vec4 end_clip = projection * vec4(end, 1.0);
    if (start_clip.w <= 0.0001 || end_clip.w <= 0.0001) return vec4(0.0);
    float start_k = 1.0 / start_clip.w;
    float end_k = 1.0 / end_clip.w;
    vec3 start_q = start * start_k;
    vec3 end_q = end * end_k;
    vec2 start_uv = start_clip.xy * start_k * 0.5 + 0.5;
    vec2 delta_uv = (end_clip.xy * end_k * 0.5 + 0.5) - start_uv;
    if (any(lessThan(start_uv, vec2(0.002))) ||
        any(greaterThan(start_uv, vec2(0.998)))) return vec4(0.0);
    // Clip in screen space before distributing the finite sample budget.
    float limit = 1.0;
    if (delta_uv.x > 0.00001) limit = min(limit, (0.998 - start_uv.x) / delta_uv.x);
    if (delta_uv.x < -0.00001) limit = min(limit, (0.002 - start_uv.x) / delta_uv.x);
    if (delta_uv.y > 0.00001) limit = min(limit, (0.998 - start_uv.y) / delta_uv.y);
    if (delta_uv.y < -0.00001) limit = min(limit, (0.002 - start_uv.y) / delta_uv.y);
    if (limit <= 0.0001 || length(delta_uv * limit) < 0.0001) return vec4(0.0);
    float previous_fraction = 0.0;
    float previous_depth = texture(air_depth, start_uv).r;
    bool previous_valid = previous_depth > 0.000001;
    float previous_gap = previous_valid
        ? -start.z + pond_view_position(start_uv, previous_depth, inverse_projection).z : 0.0;
    for (int i = 1; i <= 24; i++) {
        float fraction = limit * float(i) / 24.0;
        vec2 uv = start_uv + delta_uv * fraction;
        float depth = texture(air_depth, uv).r;
        if (depth <= 0.000001) {
            previous_valid = false;
            previous_fraction = fraction;
            continue;
        }
        vec3 ray = mix(start_q, end_q, fraction) / mix(start_k, end_k, fraction);
        vec3 target = pond_view_position(uv, depth, inverse_projection);
        float gap = -ray.z + target.z;
        if (previous_valid && previous_gap < 0.0 && gap >= 0.0) {
            float low = previous_fraction;
            float high = fraction;
            // Eight refinements prevent the subpixel residual rejection that
            // striped broad stump reflections in the grazing-angle diagnostic.
            for (int j = 0; j < 8; j++) {
                float middle = (low + high) * 0.5;
                vec2 middle_uv = start_uv + delta_uv * middle;
                float middle_depth = texture(air_depth, middle_uv).r;
                vec3 middle_ray = mix(start_q, end_q, middle) / mix(start_k, end_k, middle);
                if (middle_depth <= 0.000001) { low = middle; }
                else {
                    vec3 middle_target = pond_view_position(middle_uv, middle_depth, inverse_projection);
                    if (-middle_ray.z + middle_target.z >= 0.0) high = middle;
                    else low = middle;
                }
            }
            vec2 hit_uv = start_uv + delta_uv * high;
            float hit_depth = texture(air_depth, hit_uv).r;
            if (hit_depth <= 0.000001) return vec4(0.0);
            vec3 hit_ray = mix(start_q, end_q, high) / mix(start_k, end_k, high);
            vec3 hit = pond_view_position(hit_uv, hit_depth, inverse_projection);
            float residual = abs(-hit_ray.z + hit.z);
            float thickness = clamp(0.08 + (-hit.z) * 0.006, 0.08, 0.45);
            vec3 hit_world = (inverse_view * vec4(hit, 1.0)).xyz;
            float travel = distance(start, hit);
            if (residual > thickness || hit_world.y < water_y + 0.015 ||
                travel < 0.20 || travel > reflection_max_distance) return vec4(0.0);
            vec2 border = min(hit_uv, vec2(1.0) - hit_uv);
            float edge_fade = smoothstep(0.005, 0.055, min(border.x, border.y));
            float range_fade = 1.0 - smoothstep(reflection_max_distance * 0.72,
                reflection_max_distance, travel);
            float confidence = edge_fade * range_fade *
                (1.0 - smoothstep(thickness * 0.5, thickness, residual));
            return vec4(texture(air_scene, hit_uv).rgb, confidence);
        }
        previous_fraction = fraction;
        previous_gap = gap;
        previous_valid = true;
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
    // Keep the inherited underside above unchanged. Only Pond's air-facing
    // branch has stronger, patchy normal ripples and scene reflections.
    if (FRONT_FACING && air_surface_enabled < 0.5) discard;
    if (FRONT_FACING) {
        float air_distance = distance(CAMERA_POSITION_WORLD, surface_world);
        vec3 air_normal = pond_air_normal(surface_normal, surface_world.xz,
            air_distance, t, shore_blend);
        vec3 view_normal = normalize(mat3(VIEW_MATRIX) * air_normal);
        vec3 entry_view = pond_view_position(SCREEN_UV, FRAGCOORD.z, INV_PROJECTION_MATRIX);
        vec3 flat_view_normal = normalize(mat3(VIEW_MATRIX) * vec3(0.0, 1.0, 0.0));
        vec2 bend = (view_normal.xy - flat_view_normal.xy) * 0.008 *
            smoothstep(0.0, 0.6, shore_blend);
        vec2 refracted_uv = clamp(SCREEN_UV + bend, vec2(0.002), vec2(0.998));
        float scene_depth = texture(air_depth, refracted_uv).r;
        vec3 target_view = scene_depth > 0.000001
            ? pond_view_position(refracted_uv, scene_depth, INV_PROJECTION_MATRIX) : entry_view;
        vec3 target_world = (INV_VIEW_MATRIX * vec4(target_view, 1.0)).xyz;
        // Reject foreground/dry refraction samples before compositing. Falling
        // back to the unbent ray prevents ripples cutting holes around reeds.
        if (scene_depth > 0.000001 && (target_world.y >= surface_world.y - 0.01 ||
            -target_view.z <= -entry_view.z + 0.01)) {
            refracted_uv = SCREEN_UV;
            scene_depth = texture(air_depth, refracted_uv).r;
            target_view = scene_depth > 0.000001
                ? pond_view_position(refracted_uv, scene_depth, INV_PROJECTION_MATRIX) : entry_view;
            target_world = (INV_VIEW_MATRIX * vec4(target_view, 1.0)).xyz;
        }
        bool opaque_water_target = scene_depth > 0.000001 &&
            target_world.y < surface_world.y - 0.01 && -target_view.z > -entry_view.z;
        float water_path = opaque_water_target ? distance(entry_view, target_view) : 30.0;
        // Preserve R2's dark water and continuous depth-based fish visibility.
        float transmission = exp(-surface_extinction * max(water_path, 0.0) /
            max(surface_daylight, 0.5));
        vec3 refracted = texture(air_scene, refracted_uv).rgb;
        vec3 water_tint = mix(surface_dark, surface_light, 0.45 + 0.20 * ripple);
        vec3 scattered = mix(water_scatter, water_tint, 0.03);
        vec3 transmitted_water = mix(refracted, scattered, 1.0 - transmission);
        transmitted_water = mix(transmitted_water, water_tint, 0.01);
        float facing = clamp(dot(air_normal, -ray_up), 0.0, 1.0);
        float reflectance = (0.0204 + 0.9796 * pow(1.0 - facing, 5.0)) * reflection_strength;
        vec3 reflected_direction = reflect(ray_up, air_normal);
        // Suppress self-occluded, below-horizon microfacets at grazing views
        // instead of replacing them with a fully reflective white sky stripe.
        reflectance *= smoothstep(-0.04, 0.03, reflected_direction.y);
        vec3 reflected = pond_air_sky(reflected_direction, sun_direction, overcast, surface_time);
        // Straight-down water needs little reflection and should keep the fish
        // legible. Avoid a depth march where its contribution is negligible.
        if (reflection_scene_enabled > 0.5 && reflectance > 0.035 && reflected_direction.y > 0.0) {
            vec4 scene_reflection = pond_scene_reflection(entry_view,
                normalize(mat3(VIEW_MATRIX) * reflected_direction), surface_world.y,
                PROJECTION_MATRIX, INV_PROJECTION_MATRIX, INV_VIEW_MATRIX);
            reflected = mix(reflected, scene_reflection.rgb, scene_reflection.a);
        }
        ALBEDO = mix(transmitted_water, reflected, reflectance);
        float air_fog = smoothstep(air_fog_begin, air_fog_end, air_distance);
        // Air haze belongs after the water/reflection composite. It does not
        // alter extinction or pretend a shadow is suspended sediment.
        ALBEDO = mix(ALBEDO, vec3(0.43, 0.48, 0.46), air_fog * 0.35);
        ALPHA = edge_fade * smoothstep(0.0, 0.55, shore_blend);
        if (scene_depth > 0.000001 && !opaque_water_target) ALPHA = 0.0;
    }
}
