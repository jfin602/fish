shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear;
uniform float strength : hint_range(0.0, 0.2) = 0.07;

void fragment() {
    vec3 scene = texture(screen_texture, SCREEN_UV).rgb;
    COLOR = vec4(mix(scene, vec3(0.055, 0.27, 0.31), strength), 1.0);
}
