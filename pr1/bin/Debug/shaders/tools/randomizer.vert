#version 430
layout(local_size_x = 1, local_size_y = 1) in;
layout(rgba32f, binding = 0) uniform image2D img_output;
uniform vec4 brush_col;
uniform vec2 brush_pos1;
uniform vec2 brush_pos2;
uniform float brush_width;
uniform float seed;

float rand(vec2 co, float seed){
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * (abs(seed) + 12412.6664));
}
void main() {
  // base pixel
  // index in global work group
  ivec2 pixel_coords = ivec2(gl_GlobalInvocationID.xy);
  vec4 pix = vec4(1.0);
  if (rand(vec2(pixel_coords) / 100.0, seed) > 0.2) pix.rgb = vec3(0.0);
  imageStore(img_output, pixel_coords, pix);
}