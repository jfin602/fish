// The visible air sky and Pond reflection fallback describe the same sky.
// Inputs use the atmosphere's pause-aware clock, never shader TIME. Broad
// interpolated noise avoids the old crossed sine bands reflected on the water.
float pond_cloud_hash(vec2 point) {
    point = fract(point * vec2(123.34, 456.21));
    point += dot(point, point + 45.32);
    return fract(point.x * point.y);
}

float pond_cloud_noise(vec2 point) {
    vec2 cell = floor(point);
    vec2 offset = fract(point);
    offset = offset * offset * (3.0 - 2.0 * offset);
    return mix(mix(pond_cloud_hash(cell), pond_cloud_hash(cell + vec2(1.0, 0.0)), offset.x),
        mix(pond_cloud_hash(cell + vec2(0.0, 1.0)),
            pond_cloud_hash(cell + vec2(1.0, 1.0)), offset.x), offset.y);
}

vec3 pond_air_sky(vec3 direction, vec3 sun_direction, float overcast, float sky_time) {
    float height = smoothstep(0.0, 0.9, direction.y);
    float sun = max(dot(direction, normalize(sun_direction)), 0.0);
    vec3 air = mix(vec3(0.48, 0.52, 0.51), vec3(0.65, 0.68, 0.65), height)
             + (1.0 - 0.8 * overcast) * vec3(0.20, 0.18, 0.12) * pow(sun, 48.0);
    if (overcast <= 0.0) return air;
    vec2 cloud_position = direction.xz / (0.40 + max(direction.y, 0.0) * 0.80);
    vec2 drift = vec2(sky_time * 0.003, sky_time * -0.002);
    vec2 rotated = vec2(cloud_position.x * 0.80 - cloud_position.y * 0.60,
                        cloud_position.x * 0.60 + cloud_position.y * 0.80);
    float cloud = 0.60 * pond_cloud_noise(cloud_position * 1.2 + drift)
                + 0.28 * pond_cloud_noise(rotated * 2.7 - drift * 0.7 + vec2(8.2, -3.1))
                + 0.12 * pond_cloud_noise(cloud_position * 5.6 + drift + vec2(-5.7, 9.3));
    vec3 cloudy_air = mix(vec3(0.43, 0.48, 0.46), vec3(0.58, 0.63, 0.61), height);
    vec3 clouds = mix(vec3(0.32, 0.37, 0.36), vec3(0.72, 0.75, 0.71),
        smoothstep(0.23, 0.75, cloud));
    cloudy_air = mix(cloudy_air, clouds, smoothstep(0.015, 0.35, direction.y) * 0.62);
    // Overcast is Pond-only. Ocean's original overcast=0 air/underwater output
    // and sun remain unchanged, including when its air_mix is zero.
    return mix(air, cloudy_air, overcast);
}
