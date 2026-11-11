shader_type spatial;
render_mode cull_disabled, diffuse_burley, specular_disabled;

// Colour changes span several metres. There are no texture tiles or pixel noise.
uniform vec3 silt_color : source_color = vec3(0.210, 0.180, 0.120);
uniform vec3 mud_color : source_color = vec3(0.300, 0.240, 0.150);
varying vec3 world_point;

void vertex() {
	world_point = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
	float broad = sin(world_point.x * 0.20 + 0.4) * sin(world_point.z * 0.16 - 0.6);
	float shore = smoothstep(-6.0, 3.0, world_point.y);
	float mix_amount = clamp(0.34 + 0.37 * broad + 0.20 * shore, 0.08, 0.86);
	ALBEDO = mix(silt_color, mud_color, mix_amount);
	ROUGHNESS = 0.96;
}
