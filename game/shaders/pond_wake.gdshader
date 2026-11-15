shader_type spatial;
render_mode unshaded, cull_disabled, depth_draw_never, blend_mix, fog_disabled;

uniform vec3 surface_dark = vec3(0.08, 0.42, 0.49);
uniform vec3 surface_light = vec3(0.20, 0.66, 0.67);
uniform vec3 sun_direction = vec3(0.0, 1.0, 0.0);
uniform vec3 sun_color = vec3(1.0, 0.9, 0.7);
uniform float surface_illumination = 1.0;
uniform float night_weight = 0.0;
varying vec3 wake_world;

void vertex() {
    wake_world = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    float grain = sin(UV.y * 9.7 + wake_world.x * 6.2) *
        sin(UV.y * 15.1 - wake_world.z * 7.3);
    float broken = smoothstep(-0.55, 0.25, grain);
    vec3 view_ray = normalize(wake_world - CAMERA_POSITION_WORLD);
    float slope = (1.0 - abs(UV.x)) * COLOR.a * 0.16;
    vec3 wake_normal = normalize(vec3(slope * sign(UV.x), 1.0, grain * 0.035));
    float reflected = pow(max(dot(reflect(view_ray, wake_normal),
        normalize(sun_direction)), 0.0), 72.0);
    // The camera side is deliberate: strip winding changes around tight turns.
    // Air-facing crests read as soft white highlights; underwater they keep
    // the Pond's green water tint rather than becoming white foam.
    bool from_above = CAMERA_POSITION_WORLD.y >= wake_world.y;
    vec3 underwater_green = mix(surface_dark, surface_light, 0.28 + 0.06 * grain);
    vec3 above_white = mix(vec3(0.82, 0.89, 0.85), vec3(0.96, 0.98, 0.96),
        clamp(reflected * 2.0, 0.0, 1.0));
    ALBEDO = (from_above ? above_white : underwater_green) *
        surface_illumination * (from_above ? 0.88 : 0.76) +
        sun_color * reflected * surface_illumination * 0.22;
    // The Pond underside has a dense scattering pass; compensate with more
    // contrast while retaining the same tapered geometry and age fade.
    ALPHA = clamp(COLOR.a * broken * (from_above ? 0.22 : 1.8) *
        (0.85 + 0.15 * reflected) * mix(1.0, 0.30, night_weight), 0.0, 0.85);
}
