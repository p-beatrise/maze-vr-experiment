shader_type spatial;
render_mode unshaded, cull_front, depth_draw_never, blend_mul;

uniform vec4 filter_color : source_color = vec4(1.0, 1.0, 1.0, 0.0);

void fragment() {
    // Multiplicative tint: mix(white, color, alpha) multiplied over the scene.
    // alpha=0 → tint is (1,1,1) → multiply by 1 = no effect.
    // alpha=0.25, blue → tint=(0.75,0.75,1.0) → slightly blue scene.
    ALBEDO = mix(vec3(1.0), filter_color.rgb, filter_color.a);
    ALPHA  = 1.0;
    // Force depth to near plane so sphere renders on top of all geometry.
    DEPTH  = 0.0;
}
