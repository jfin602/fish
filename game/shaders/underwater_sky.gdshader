shader_type sky;

uniform vec3 sun_direction = vec3(0.0, 1.0, 0.0);
uniform float water_depth : hint_range(0.0, 1.0) = 0.0;
uniform vec3 shallow_horizon = vec3(0.06, 0.28, 0.34);
uniform vec3 deep_horizon = vec3(0.025, 0.095, 0.18);
uniform vec3 shallow_zenith = vec3(0.39, 0.77, 0.87);
uniform vec3 deep_zenith = vec3(0.27, 0.57, 0.75);
uniform float overcast = 0.0;
uniform float air_mix : hint_range(0.0, 1.0) = 0.0;
uniform vec3 air_horizon = vec3(0.45, 0.70, 0.84);
uniform vec3 air_zenith = vec3(0.12, 0.45, 0.81);
uniform vec3 warm_horizon = vec3(1.0, 0.39, 0.16);
uniform vec3 direct_color = vec3(0.97, 0.98, 1.0);
uniform float warm_strength : hint_range(0.0, 1.0) = 0.0;
uniform float sun_glow : hint_range(0.0, 1.0) = 1.0;

void sky() {
    float height = smoothstep(-0.18, 0.92, EYEDIR.y);
    vec3 horizon = mix(shallow_horizon, deep_horizon, water_depth);
    vec3 zenith = mix(shallow_zenith, deep_zenith, water_depth);
    float sun = max(dot(EYEDIR, normalize(sun_direction)), 0.0);
    COLOR = mix(horizon, zenith, height)
          + sun_glow * (1.0 - 0.75 * overcast) * vec3(0.65, 0.59, 0.40) * pow(sun, 32.0)
          + sun_glow * (1.0 - 0.90 * overcast) * vec3(1.1, 1.02, 0.74) * pow(sun, 400.0);
    vec3 air = mix(air_horizon, air_zenith, height);
    float near_horizon = 1.0 - smoothstep(-0.08, 0.55, EYEDIR.y);
    air += warm_horizon * warm_strength * near_horizon * pow(sun, 10.0) * 0.42;
    air += direct_color * sun_glow *
        (0.24 * pow(sun, 48.0) + 1.15 * pow(sun, 700.0));
    COLOR = mix(COLOR, air, air_mix);
}
