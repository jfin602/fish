shader_type sky;

uniform vec3 sun_direction = vec3(0.0, 1.0, 0.0);
uniform float water_depth : hint_range(0.0, 1.0) = 0.0;
uniform vec3 shallow_horizon = vec3(0.06, 0.28, 0.34);
uniform vec3 deep_horizon = vec3(0.025, 0.095, 0.18);
uniform vec3 shallow_zenith = vec3(0.39, 0.77, 0.87);
uniform vec3 deep_zenith = vec3(0.27, 0.57, 0.75);
uniform float overcast = 0.0;
uniform float air_mix : hint_range(0.0, 1.0) = 0.0;
uniform float sky_time = 0.0;

void sky() {
    float height = smoothstep(0.0, 0.9, EYEDIR.y);
    vec3 horizon = mix(shallow_horizon, deep_horizon, water_depth);
    vec3 zenith = mix(shallow_zenith, deep_zenith, water_depth);
    float sun = max(dot(EYEDIR, normalize(sun_direction)), 0.0);
    COLOR = mix(horizon, zenith, height)
          + (1.0 - 0.75 * overcast) * vec3(0.65, 0.59, 0.40) * pow(sun, 32.0)
          + (1.0 - 0.90 * overcast) * vec3(1.1, 1.02, 0.74) * pow(sun, 400.0);
    vec3 air = mix(vec3(0.48, 0.52, 0.51), vec3(0.65, 0.68, 0.65), height)
             + (1.0 - 0.8 * overcast) * vec3(0.20, 0.18, 0.12) * pow(sun, 48.0);
    float cloud = sin(EYEDIR.x * 19.0 + EYEDIR.z * 11.0 + sky_time * 0.015)
                * sin(EYEDIR.z * 27.0 - EYEDIR.x * 7.0 - sky_time * 0.009);
    cloud += 0.35 * sin(EYEDIR.x * 48.0 + EYEDIR.z * 32.0);
    air = mix(air, mix(vec3(0.29, 0.32, 0.32), vec3(0.72, 0.73, 0.68),
        smoothstep(-0.75, 0.85, cloud)), overcast * smoothstep(-0.1, 0.28, EYEDIR.y) * 0.72);
    COLOR = mix(COLOR, air, air_mix);
}
