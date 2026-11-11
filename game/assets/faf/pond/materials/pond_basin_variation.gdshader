shader_type spatial;
render_mode cull_disabled, diffuse_burley, specular_disabled;

// Colour changes span several metres. There are no texture tiles or pixel noise.
uniform vec3 silt_color : source_color = vec3(0.210, 0.180, 0.120);
uniform vec3 mud_color : source_color = vec3(0.300, 0.240, 0.150);
uniform float water_surface_y = 10.39;
varying vec3 world_point;

void vertex() {
	world_point = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
	float broad = sin(world_point.x * 0.20 + 0.4) * sin(world_point.z * 0.16 - 0.6);
	float shore = smoothstep(water_surface_y - 3.0, water_surface_y + 2.0, world_point.y);
	float mix_amount = clamp(0.34 + 0.37 * broad + 0.20 * shore, 0.08, 0.86);
	vec3 sediment = mix(silt_color, mud_color, mix_amount);
	float waterline = 1.0 - smoothstep(0.0, 0.85, abs(world_point.y - water_surface_y));
	vec3 damp = mix(sediment, vec3(0.115, 0.125, 0.073), waterline * 0.52);
	ALBEDO = mix(damp, vec3(0.17, 0.20, 0.105),
	    smoothstep(water_surface_y + 1.0, water_surface_y + 4.0, world_point.y) * 0.31);
	ROUGHNESS = 0.96;
}
