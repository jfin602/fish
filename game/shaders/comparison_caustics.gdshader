shader_type spatial;
render_mode unshaded, blend_add, cull_disabled, depth_draw_never;

uniform vec3 tint : source_color = vec3(0.18, 0.58, 0.62);
uniform float strength : hint_range(0.0, 0.3) = 0.11;
uniform float scale = 5.0;
uniform float speed = 0.22;

void fragment() {
    vec2 p = UV * scale;
    float a = sin(p.x * 7.0 + sin(p.y * 4.0 + TIME * speed * 2.0));
    float b = sin(p.y * 8.0 - sin(p.x * 3.0 - TIME * speed * 1.6));
    float line = pow(max(0.0, 1.0 - abs(a + b) * 0.7), 10.0);
    ALBEDO = tint;
    ALPHA = line * strength;
}
