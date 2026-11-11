shader_type canvas_item;

// Runs over the rendered 3D view on layer 0. Interface CanvasLayers use layer 10.
uniform sampler2D screen_texture : hint_screen_texture, filter_linear;
uniform float warp_pixels : hint_range(0.0, 0.2) = 0.10;
uniform float color_variation : hint_range(0.0, 0.04) = 0.007;
uniform float screen_time = 0.0;
uniform float immersion : hint_range(0.0, 1.0) = 1.0;

void fragment() {
    vec2 wave = vec2(sin(SCREEN_UV.y * 38.0 + screen_time * 0.43),
                     cos(SCREEN_UV.x * 31.0 - screen_time * 0.36));
    vec2 uv = clamp(SCREEN_UV + wave * SCREEN_PIXEL_SIZE * warp_pixels * immersion,
                    SCREEN_PIXEL_SIZE, vec2(1.0) - SCREEN_PIXEL_SIZE);
    vec3 scene = texture(screen_texture, uv).rgb;
    float shimmer = sin(SCREEN_UV.x * 19.0 + SCREEN_UV.y * 11.0 + screen_time * 0.31);
    scene *= 1.0 + color_variation * immersion * shimmer;
    COLOR = vec4(scene, 1.0);
}
