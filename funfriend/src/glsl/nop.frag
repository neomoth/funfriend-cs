#version 330 core

in vec2 TexCoord;
uniform sampler2D texture1;

out vec4 FragColor;

void main() {
	vec4 texColor = texture(texture1, TexCoord);

	if (texColor.a < 0.1) {
		discard;
	}

//	vec2 uv = TexCoord;
	
	FragColor = texColor;
//	FragColor = vec4(uv, 0.0, 1.0);
}