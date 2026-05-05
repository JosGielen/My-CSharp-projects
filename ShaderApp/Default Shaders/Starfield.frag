#version 420 core
#extension GL_ARB_explicit_uniform_location : enable

in vec3 Pos;  //Pixel position
in vec2 texCoord;

layout(location = 2) uniform float Time;  //used for animation
layout(location = 3) uniform float Zoom;  //mouse wheel used to zoom in/out
layout(location = 4) uniform vec3 Mouse;  //mouse movement
layout(location = 5) uniform vec3 Resolution; //Size of the GLWpfControl

out vec4 FragColor; //pixel color

#define NUM_LAYERS 6

mat2 rot2D (float a)
{
    float s = sin(a);
    float c = cos(a);
    return mat2(c, -s, s, c);
}

float Star(vec2 uv, float flare)
{
    float d = length(uv);
    float m = 0.003 / (d * d);    
    float rays = max(0.0, 1 - abs(uv.x * uv.y * 1000));
    m += rays * 0.5 * flare;
    uv *= rot2D(3.1415926 / 4);
    rays = max(0.0, 1 - abs(uv.x * uv.y * 1000));
    m += rays * 0.3 * flare;
    m *= smoothstep(0.5, 0.2, d);
    return m;
}

float Hash21(vec2 p)
{
    p = fract(p * vec2(123.45, 678.91));
    p += dot(p, p + 47.39);
    return fract(p.x * p.y);
}

vec3 StarLayer(vec2 uv)
{
    vec3 col = vec3(0.0);
    vec2 gv = fract(uv) -0.5;
    vec2 id = floor(uv);
    for(int y = -1; y <= 1; y++)
    {
        for(int x = -1; x <= 1; x++)
        {
            vec2 offset = vec2(x, y);
            float n = Hash21(id + offset);  //Random between 0 and 1.
            float size = fract(n * 625.83);            
            float star = Star(gv - offset - vec2(n, fract(79 * n))+0.5, smoothstep(0.7, 1.0, size));
            vec3 color = sin(vec3(0.29, 0.33, 0.9)* fract(n * 5248.27) * 1.7 * 6.2831) * 0.5 + 0.5;
            color += vec3(0.7 - size, 0.0, 0.5 * size);
            col += star * size * color;
        }
    }
    return col;
}

void main()
{
    //Correct for the viewport size.
    vec2 uv = Pos.xy;
    uv.x *= Resolution.x / Resolution.y;
    uv += 0.01 * Mouse.xy;
    float t = Time * 0.1;
    vec3 col = vec3(0.0);
    for (float i = 0; i < 1; i += 1.0 / NUM_LAYERS)
    {
        float depth = fract(i + t);
        float scale = mix(10.0 , 0.5, depth);
        col += StarLayer(uv * scale + i * 371.55) * depth;
    }    
    FragColor = vec4(col, 1.0);
}








