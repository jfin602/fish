shader_type spatial;
render_mode unshaded, cull_disabled, depth_draw_never, blend_mix, fog_disabled;

uniform vec3 surface_dark = vec3(0.08, 0.42, 0.49);
uniform vec3 surface_light = vec3(0.20, 0.66, 0.67);
uniform vec3 sun_direction = vec3(0.0, 1.0, 0.0);
uniform vec3 sun_color = vec3(1.0, 0.9, 0.7);
uniform float surface_illumination = 1.0;
uniform float night_weight = 0.0;
uniform int repair_mode = 0;
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
    bool from_above = CAMERA_POSITION_WORLD.y >= wake_world.y && COLOR.r > 0.5;
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
    if (repair_mode == 1 || repair_mode == 3) {
        // The mesh carries a wider footprint; distribute the same disturbance
        // across it with no solid center or abrupt side boundary.
        float soft_breakup = smoothstep(-0.75, 0.45, grain);
        ALPHA *= (from_above ? 0.58 : 0.72) * (0.80 + 0.20 * soft_breakup);
    }
    if (repair_mode == 2 || repair_mode == 3) {
        // A small cross-front slope alters reflection and transmitted light.
        // These are local ribbon pixels, not a history loop in the water shader.
        vec3 tilted = normalize(vec3(0.34 * UV.x * (1.0 - abs(UV.x)), 1.0,
            0.055 * grain));
        vec3 flat_normal = vec3(0.0, 1.0, 0.0);
        vec3 light_dir = normalize(sun_direction);
        float slope_light = dot(tilted - flat_normal, light_dir);
        float slope_view = abs(dot(tilted, view_ray)) - abs(dot(flat_normal, view_ray));
        float tilt_glint = pow(max(dot(reflect(view_ray, tilted), light_dir), 0.0), 28.0);
        float flat_glint = pow(max(dot(reflect(view_ray, flat_normal), light_dir), 0.0), 28.0);
        float contrast = clamp(1.7 * slope_view + 1.2 * slope_light +
            0.55 * (tilt_glint - flat_glint), -0.38, 0.38);
        vec3 water_color = mix(surface_dark, surface_light, 0.48 + 0.08 * grain);
        ALBEDO = max(vec3(0.0), water_color * surface_illumination *
            (from_above ? 0.92 : 0.79) + contrast * surface_light *
            surface_illumination * (from_above ? 0.74 : 1.25));
        ALPHA = clamp(COLOR.a * broken * (from_above ? 0.47 : 1.25) *
            mix(1.0, 0.18, night_weight), 0.0, 0.68);
        if (repair_mode == 3) {
            ALPHA *= from_above ? 0.78 : 0.90;
        }
    }
}
