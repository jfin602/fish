shader_type sky;

uniform vec3 sun_direction = vec3(0.0, 1.0, 0.0);
uniform float water_depth : hint_range(0.0, 1.0) = 0.0;
uniform vec3 shallow_horizon = vec3(0.06, 0.28, 0.34);
uniform vec3 deep_horizon = vec3(0.025, 0.095, 0.18);
uniform vec3 shallow_zenith = vec3(0.39, 0.77, 0.87);
uniform vec3 deep_zenith = vec3(0.27, 0.57, 0.75);
uniform float overcast = 0.0;

void sky() {
    float height = smoothstep(0.0, 0.9, EYEDIR.y);
    vec3 horizon = mix(shallow_horizon, deep_horizon, water_depth);
    vec3 zenith = mix(shallow_zenith, deep_zenith, water_depth);
    float sun = max(dot(EYEDIR, normalize(sun_direction)), 0.0);
    COLOR = mix(horizon, zenith, height)
          + (1.0 - 0.75 * overcast) * vec3(0.65, 0.59, 0.40) * pow(sun, 32.0)
          + (1.0 - 0.90 * overcast) * vec3(1.1, 1.02, 0.74) * pow(sun, 400.0);
}
