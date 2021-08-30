#version 330

out vec4 outputColor;

in vec2 texCoord;

uniform float time;


void main()
{

    vec3 col = 0.5 + 0.5 * cos(vec3(time / 1.0) + texCoord.xyx + vec3(0.0, 2.0, 4.0));

    outputColor = vec4(col * 0.3, 1.0);
}