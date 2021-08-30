#version 330

out vec4 outputColor;

in vec2 texCoord;

uniform vec3 basecolor;
uniform sampler2D texture0;
uniform vec2 picker_pos;

float len(vec2 arg){
	return sqrt(arg.x * arg.x + arg.y * arg.y);
}
float normal(float init){
	return min(max(init, 0.0), 1.0);
}
void main()
{
	float sqrtotdbt = 0.707106781;
	vec2 AB = vec2(1.0, 0.0);
	vec2 AC = vec2(texCoord.x  - 0.5, texCoord.y - 0.5);
	float angle_cos = (AB.x * AC.x + AB.y * AC.y) / len(AC);
	float angle_sin = sqrt(1 - angle_cos * angle_cos);
	float r = normal(0.5 - 1 * angle_cos);
	float g = normal(0.5 - 1 * (angle_cos * sqrtotdbt - angle_sin * 0.5));
	if (len(AC) < 0.5){
		outputColor = vec4(r, g, 0.0, 1.0);
	}
	else{
		outputColor = vec4(0.0, 0.0, 0.0, 0.0);
	}
	
}