#version 420 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;

out vec3 Pos;
out vec2 texCoord;

void main(void)
{
    texCoord = aTexCoord;
	Pos = aPosition;
    gl_Position = vec4(aPosition, 1.0);
}