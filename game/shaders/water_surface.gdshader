shader_type spatial;
render_mode unshaded, cull_front, blend_mix;

// The surface is viewed from below. Time is supplied by the atmosphere so pause freezes it.
uniform float surface_time = 0.0;
uniform vec3 sun_direction = vec3(0.0, 1.0, 0.0);

varying vec3 surface_world;
varying vec3 surface_normal;

void vertex() {
    float a = VERTEX.x * 0.19 + VERTEX.z * 0.07 + surface_time * 0.34;
    float b = VERTEX.x * -0.11 + VERTEX.z * 0.17 - surface_time * 0.27;
    VERTEX.y += 0.10 * sin(a) + 0.065 * sin(b);
    vec3 slope = vec3(0.10 * 0.19 * cos(a) - 0.065 * 0.11 * cos(b),
                      1.0,
                      0.10 * 0.07 * cos(a) + 0.065 * 0.17 * cos(b));
    surface_normal = normalize(mat3(MODEL_MATRIX) * normalize(vec3(-slope.x, 1.0, -slope.z)));
    surface_world = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    vec3 ray_up = normalize(surface_world - CAMERA_POSITION_WORLD);
    float ripple = sin(surface_world.x * 0.57 + surface_world.z * 0.23 + surface_time * 0.53)
                 * sin(surface_world.z * 0.43 - surface_world.x * 0.14 - surface_time * 0.39);
    vec3 normal = normalize(surface_normal + vec3(0.025 * ripple, 0.0, -0.02 * ripple));
    float fresnel = pow(1.0 - clamp(dot(normal, ray_up), 0.0, 1.0), 3.0);
    float sun = max(dot(ray_up, normalize(sun_direction)), 0.0);
    float halo = pow(sun, 18.0);
    float glint = pow(sun, 150.0) * (0.7 + 0.3 * ripple);
    vec3 turquoise = mix(vec3(0.08, 0.42, 0.49), vec3(0.20, 0.66, 0.67), 0.5 + 0.5 * ripple);
    ALBEDO = turquoise + vec3(0.22, 0.44, 0.40) * fresnel
             + vec3(0.55, 0.66, 0.52) * halo + vec3(0.55, 0.58, 0.43) * glint;
    ALPHA = mix(0.62, 0.88, fresnel);
}
