#version 330 core

in vec3 aPosition;

uniform mat4 Model;
uniform mat4 View;
uniform mat4 Projection;

out vec3 Pos;

void main()
{
    Pos = vec3(Model * vec4(aPosition, 1.0));
    gl_Position = Projection * View * vec4(Pos, 1.0);
}