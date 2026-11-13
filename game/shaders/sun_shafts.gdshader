shader_type spatial;
render_mode unshaded, cull_disabled, depth_draw_never, blend_add;

// One ribbon per sampled caustic ridge, all in one draw. The diagonal edge
// taper follows the light travel, and the bottom fades into the ground line.
uniform float shaft_time = 0.0;
uniform int shaft_count = 6;
uniform float shaft_brightness = 1.0;
uniform float water_surface_y = 10.0;
uniform float water_deep_y = -8.0;
uniform float deep_transmission = 0.12;
uniform vec3 shaft_color = vec3(0.54, 0.79, 0.72);
uniform vec3 sun_direction = vec3(0.0, 1.0, 0.0);
uniform float shaft_lean = 1.0;
varying float world_height;

void vertex() {
    world_height = (MODEL_MATRIX * vec4(VERTEX, 1.0)).y;
    float floor_y = mix(water_deep_y, water_surface_y, COLOR.b);
    float height_above_floor = (1.0 - UV.y) * max(water_surface_y - floor_y, 0.0);
    vec2 lean = sun_direction.xz / max(sun_direction.y, 0.32);
    VERTEX.xz += clamp(lean, vec2(-1.2), vec2(1.2)) * height_above_floor * shaft_lean;
}

void fragment() {
    float active = 1.0 - step(float(shaft_count) / 8.0, COLOR.g);
    float wander = 0.075 * sin(UV.y * 9.0 + COLOR.r * 31.0 + shaft_time * 0.12)
        + 0.035 * sin(UV.y * 19.0 - COLOR.r * 47.0 - shaft_time * 0.09);
    float diagonal = UV.x + 0.16 * (UV.y - 0.5) + wander;
    float edge = smoothstep(0.0, 0.32, diagonal) * (1.0 - smoothstep(0.68, 1.0, diagonal));
    float top_fade = smoothstep(0.0, 0.10, UV.y);
    float ground_fade = 1.0 - smoothstep(0.70, 1.0, UV.y);
    float broken = sin(UV.y * 16.0 + COLOR.r * 19.0 + shaft_time * 0.24)
                 * sin(UV.y * 7.0 - UV.x * 5.0 + COLOR.r * 31.0 - shaft_time * 0.17);
    float variation = smoothstep(-0.75, 0.5, broken) * 0.45 + 0.42;
    ALBEDO = shaft_color;
    float depth = smoothstep(0.0, 1.0, clamp((water_surface_y - world_height) / max(water_surface_y - water_deep_y, 0.01), 0.0, 1.0));
    ALPHA = active * shaft_brightness * 0.13 * edge * top_fade * ground_fade * variation * mix(1.0, deep_transmission, depth);
}
