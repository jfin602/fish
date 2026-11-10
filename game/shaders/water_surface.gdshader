shader_type spatial;
render_mode unshaded, cull_front, blend_mix;

// The surface is viewed from below. Time is supplied by the atmosphere so pause freezes it.
uniform float surface_time = 0.0;
uniform vec3 sun_direction = vec3(0.0, 1.0, 0.0);
uniform float wave_height = 1.0;
uniform float wave_speed = 1.0;
uniform float wave_detail = 0.8;

varying vec3 surface_world;
varying vec3 surface_normal;

void vertex() {
    float t = surface_time * wave_speed;
    float a = VERTEX.x * 0.19 + VERTEX.z * 0.07 + t * 0.34;
    float b = VERTEX.x * -0.11 + VERTEX.z * 0.17 - t * 0.27;
    float c = VERTEX.x * 0.31 - VERTEX.z * 0.24 + t * 0.47;
    VERTEX.y += wave_height * (0.10 * sin(a) + 0.065 * sin(b) + wave_detail * 0.045 * sin(c));
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
    vec3 turquoise = mix(vec3(0.08, 0.42, 0.49), vec3(0.20, 0.66, 0.67),
        clamp(0.5 + 0.35 * ripple + 0.15 * wave_detail * detail, 0.0, 1.0));
    ALBEDO = turquoise + vec3(0.22, 0.44, 0.40) * fresnel
             + vec3(0.55, 0.66, 0.52) * halo + vec3(0.55, 0.58, 0.43) * glint;
    ALPHA = mix(0.62, 0.88, fresnel);
}
