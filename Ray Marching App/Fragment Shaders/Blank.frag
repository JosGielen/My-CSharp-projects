#version 330 core

in vec3 Pos;  //Pixel position

uniform float Time;  //used for animation
uniform float Zoom;  //mouse wheel used to zoom in/out
uniform vec3 Mouse;  //mouse movement

out vec4 FragColor; //pixel color

//The Starting point
//------------------
void main()
{
    vec3 col = vec3(0.0);
    FragColor = vec4(col.x, col.y, col.z, 1.0);
}
