shader_type spatial;
render_mode unshaded, cull_disabled, depth_draw_never, blend_add;

// One six-quad mesh. UVs fade both ends and edges; vertex red varies phase.
uniform float shaft_time = 0.0;

void fragment() {
    float edge = pow(max(0.0, 1.0 - abs(UV.x * 2.0 - 1.0)), 1.8);
    float start_fade = smoothstep(0.0, 0.12, UV.y);
    float end_fade = 1.0 - smoothstep(0.66, 1.0, UV.y);
    float breakup = sin(UV.y * 17.0 + COLOR.r * 19.0 + shaft_time * 0.24)
                  * sin(UV.y * 9.0 - UV.x * 5.0 + COLOR.r * 31.0 - shaft_time * 0.17);
    float variation = 0.70 + 0.30 * breakup;
    ALBEDO = vec3(0.54, 0.79, 0.72);
    ALPHA = 0.075 * edge * start_fade * end_fade * variation;
}
