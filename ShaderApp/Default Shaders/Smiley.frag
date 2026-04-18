#version 420 core
#extension GL_ARB_explicit_uniform_location : enable

in vec3 Pos;  //Pixel position
in vec2 texCoord;

layout(location = 2) uniform float Zoom;  //mouse wheel used to zoom in/out
layout(location = 3) uniform vec3 Resolution; //Size of the GLWpfControl

out vec4 FragColor; //pixel color

//The Starting point
//------------------

float circle(vec2 p, vec2 loc, float radius, float blur)
{
    float d = length(p - loc);
    d = smoothstep(radius + blur, radius - blur, d);
    return d;
}

void main()
{
    //Correct for the viewport size.
    vec2 p = Pos.xy;
    p.x *= Resolution.x / Resolution.y;
    p = p / (Zoom / 5.0);
    float c = 0;
    float face = circle(p, vec2(0.0, 0.0), 0.3, 0.005);
    float leftEye = circle(p, vec2(-0.14, 0.15), 0.05, 0.001);
    float rightEye = circle(p, vec2(0.14, 0.15), 0.05, 0.001);
    float mouth1 = circle(p, vec2(0.0, -0.05), 0.18, 0.001);
    float mouth2 = circle(p, vec2(0.0, +0.05), 0.2, 0.001);
    c = clamp(face  - 2 * leftEye  - 2 * rightEye - mouth1 + mouth2, 0.0, 1.0);
    vec3 col = vec3(0.8 * c, 0.6 * c, 0.2 * c);
    FragColor = vec4(col.x, col.y, col.z, 1.0);
}

