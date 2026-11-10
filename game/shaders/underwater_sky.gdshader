shader_type sky;

uniform vec3 sun_direction = vec3(0.0, 1.0, 0.0);

void sky() {
    float height = smoothstep(0.0, 0.9, EYEDIR.y);
    vec3 horizon = vec3(0.06, 0.28, 0.34);
    vec3 zenith = vec3(0.39, 0.77, 0.87);
    float sun = max(dot(EYEDIR, normalize(sun_direction)), 0.0);
    COLOR = mix(horizon, zenith, height)
          + vec3(0.65, 0.59, 0.40) * pow(sun, 32.0)
          + vec3(1.1, 1.02, 0.74) * pow(sun, 400.0);
}
