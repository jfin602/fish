shader_type spatial;
render_mode diffuse_burley, specular_schlick_ggx;

uniform sampler2D coast_color : source_color, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D shell_color : source_color, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D gravel_color : source_color, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D rock_color : source_color, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D coast_normal : hint_normal, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D shell_normal : hint_normal, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D gravel_normal : hint_normal, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D rock_normal : hint_normal, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D coast_roughness : filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D shell_roughness : filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D gravel_roughness : filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D rock_roughness : filter_linear_mipmap_anisotropic, repeat_enable;
uniform float water_surface_y = 10.0;
uniform float water_deep_y = -8.0;

varying vec3 world_position;

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    vec2 uv = UV;
    float shell = smoothstep(1.0, 5.0, world_position.x - 1.3 * sin(0.4 * world_position.z));
    float gravel = smoothstep(1.7, 5.3, world_position.z - 1.8 * sin(0.35 * world_position.x));
    float radius = length(world_position.xz);
    float rock = smoothstep(19.5, 25.0, radius - 1.4 * sin(0.31 * world_position.x + 0.19 * world_position.z));
    vec4 weights = vec4((1.0 - shell) * (1.0 - gravel), shell * (1.0 - gravel), gravel, 0.0) * (1.0 - rock);
    weights.w = rock;

    vec3 albedo = vec3(0.0);
    if (weights.x > 0.001) {
        albedo += texture(coast_color, uv).rgb * weights.x;
    }
    if (weights.y > 0.001) {
        albedo += texture(shell_color, uv).rgb * weights.y;
    }
    if (weights.z > 0.001) {
        albedo += texture(gravel_color, uv).rgb * weights.z;
    }
    if (weights.w > 0.001) {
        albedo += texture(rock_color, uv).rgb * weights.w;
    }
    // World height lets shelves in the same frame keep distinct water-column color.
    float depth = smoothstep(0.25, 1.0, clamp((water_surface_y - world_position.y) / max(water_surface_y - water_deep_y, 0.01), 0.0, 1.0));
    ALBEDO = albedo * mix(vec3(1.0), vec3(0.67, 0.80, 0.88), depth);
    if (weights.w > max(max(weights.x, weights.y), weights.z)) {
        NORMAL_MAP = texture(rock_normal, uv).rgb;
        ROUGHNESS = texture(rock_roughness, uv).r;
    } else if (weights.z > max(weights.x, weights.y)) {
        NORMAL_MAP = texture(gravel_normal, uv).rgb;
        ROUGHNESS = texture(gravel_roughness, uv).r;
    } else if (weights.y > weights.x) {
        NORMAL_MAP = texture(shell_normal, uv).rgb;
        ROUGHNESS = texture(shell_roughness, uv).r;
    } else {
        NORMAL_MAP = texture(coast_normal, uv).rgb;
        ROUGHNESS = texture(coast_roughness, uv).r;
    }
    NORMAL_MAP_DEPTH = 1.0;
}
