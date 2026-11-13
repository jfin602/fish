shader_type sky;

uniform vec3 sun_direction = vec3(0.0, 1.0, 0.0);
uniform vec3 warm_direction = vec3(0.0, 1.0, 0.0);
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
uniform float moon_glow : hint_range(0.0, 1.0) = 0.0;
uniform float underwater_daylight : hint_range(0.0, 1.0) = 1.0;
uniform float night_weight : hint_range(0.0, 1.0) = 0.0;

float star_hash(vec2 cell) {
    vec3 value = fract(vec3(cell.x, cell.y, cell.x) * vec3(0.1031, 0.1030, 0.0973));
    value += dot(value, value.yzx + 33.33);
    return fract((value.x + value.y) * value.z);
}

float night_stars(vec3 direction) {
    vec2 grid = vec2(atan(direction.z, direction.x) * 0.15915494 + 0.5,
        acos(clamp(direction.y, -1.0, 1.0)) * 0.31830989) * vec2(520.0, 260.0);
    vec2 cell = floor(grid);
    float chosen = step(0.997, star_hash(cell));
    float point = 1.0 - smoothstep(0.07, 0.20, length(fract(grid) - vec2(0.5)));
    return chosen * point * (0.55 + 0.45 * star_hash(cell + vec2(19.0, 47.0)))
        * smoothstep(-0.04, 0.18, direction.y);
}

void sky() {
    float height = smoothstep(-0.18, 0.92, EYEDIR.y);
    vec3 horizon = mix(shallow_horizon, deep_horizon, water_depth);
    vec3 zenith = mix(shallow_zenith, deep_zenith, water_depth);
    float sun = max(dot(EYEDIR, normalize(sun_direction)), 0.0);
    COLOR = underwater_daylight * (mix(horizon, zenith, height)
          + sun_glow * (1.0 - 0.75 * overcast) * vec3(0.65, 0.59, 0.40) * pow(sun, 32.0)
          + sun_glow * (1.0 - 0.90 * overcast) * vec3(1.1, 1.02, 0.74) * pow(sun, 400.0));
    vec3 air = mix(air_horizon, air_zenith, height);
    float near_horizon = 1.0 - smoothstep(-0.08, 0.55, EYEDIR.y);
    float warm_sun = max(dot(EYEDIR, normalize(warm_direction)), 0.0);
    air += warm_horizon * warm_strength * near_horizon * pow(warm_sun, 10.0) * 0.42;
    // The directional light points at the moon below the horizon; twilight's
    // remaining sun glow must stay on the real solar direction.
    air += direct_color * sun_glow *
        (0.24 * pow(warm_sun, 48.0) + 1.15 * pow(warm_sun, 700.0));
    air += moon_glow * vec3(0.35, 0.39, 0.43) *
        (0.006 * pow(sun, 128.0) + 0.12 * pow(sun, 1100.0) + 0.80 * pow(sun, 8500.0));
    air += night_weight * night_stars(normalize(EYEDIR)) * vec3(0.34, 0.38, 0.41);
    COLOR = mix(COLOR, air, air_mix);
}
