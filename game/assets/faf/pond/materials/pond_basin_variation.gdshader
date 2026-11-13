shader_type spatial;
render_mode cull_disabled, diffuse_burley, specular_disabled;

// Colour changes span several metres. There are no texture tiles or pixel noise.
uniform vec3 silt_color : source_color = vec3(0.210, 0.180, 0.120);
uniform vec3 mud_color : source_color = vec3(0.300, 0.240, 0.150);
uniform float water_surface_y = 10.39;
uniform vec3 sun_direction = vec3(0.0, 1.0, 0.0);
uniform float sunlight_energy = 0.0;
uniform float surface_time = 0.0;
varying vec3 world_point;

void vertex() {
	world_point = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
	float broad = sin(world_point.x * 0.20 + 0.4) * sin(world_point.z * 0.16 - 0.6);
	// Broad, skewed alluvial patches break up even mud values without a tiled map.
	float alluvial = smoothstep(-0.72, 0.72,
	    0.68 * sin(dot(world_point.xz, vec2(0.34, 0.11)) + 0.4)
	    + 0.32 * sin(dot(world_point.xz, vec2(-0.17, 0.29)) - 0.8));
	float shore = smoothstep(water_surface_y - 3.0, water_surface_y + 2.0, world_point.y);
	float mix_amount = clamp(0.34 + 0.37 * broad + 0.20 * shore
	    + 0.22 * (alluvial - 0.5), 0.08, 0.86);
	vec3 sediment = mix(silt_color, mud_color, mix_amount) * mix(0.76, 1.25, alluvial);
	float waterline = 1.0 - smoothstep(0.0, 0.85, abs(world_point.y - water_surface_y));
	vec3 damp = mix(sediment, vec3(0.115, 0.125, 0.073), waterline * 0.52);
	ALBEDO = mix(damp, vec3(0.17, 0.20, 0.105),
	    smoothstep(water_surface_y + 1.0, water_surface_y + 4.0, world_point.y) * 0.31);
	// Soft moving pools follow the sunlight's travel through the actual water
	// column. They fade on deep shelves and vanish with the shared solar clock.
	float water_depth = max(water_surface_y - world_point.y, 0.0);
	vec2 entry = world_point.xz + sun_direction.xz * water_depth / max(sun_direction.y, 0.3);
	float slow = surface_time * 0.12;
	float folds = sin(entry.x * 0.63 + entry.y * 0.17 + slow)
	    * sin(entry.y * 0.51 - entry.x * 0.13 - slow * 0.8);
	float detail = sin(entry.x * 1.17 - entry.y * 0.31 - slow * 1.4)
	    * sin(entry.y * 1.04 + entry.x * 0.27 + slow);
	float pool = smoothstep(-0.14, 0.61, 0.72 * folds + 0.28 * detail);
	float transmission = exp(-0.16 * water_depth);
	ALBEDO *= 1.0 + 0.52 * sunlight_energy * transmission * pool;
	ROUGHNESS = 0.96;
}
