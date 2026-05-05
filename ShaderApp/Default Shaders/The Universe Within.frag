#version 420 core
#extension GL_ARB_explicit_uniform_location : enable

in vec3 Pos;  //Pixel position
in vec2 texCoord;

layout(location = 2) uniform float Time;  //used for animation
layout(location = 3) uniform float Zoom;  //mouse wheel used to zoom in/out
layout(location = 4) uniform vec3 Mouse;  //mouse movement
layout(location = 5) uniform vec3 Resolution; //Size of the GLWpfControl

uniform samplerCube cubemap;
uniform sampler2D texture0;
uniform sampler2D texture1;
uniform sampler2D texture2;
uniform sampler2D texture3;

out vec4 FragColor; //pixel color

#define MAX_STEPS 100
#define MAX_DIST 100.
#define SURF_DIST .001
#define TAU 6.283185
#define PI 3.141592


float DistLine( vec2 p, vec2 a, vec2 b)
{
    vec2 pa = p - a;
    vec2 ba = b - a;
    float t = clamp(dot(pa,ba)/dot(ba,ba), 0.0, 1.0);
    return length(pa - ba * t);
}

float N21(vec2 p)
{
    p = fract(p * vec2(273.39,851.23));
    p += dot(p, p + 71.45);
    return fract(p.x * p.y);
}

vec2 N22(vec2 p)
{
    float n = N21(p + vec2(156.48,319.54));
    return vec2(n, N21(p + n));
}

vec2 GetPos (vec2 id, vec2 offset)
{
    vec2 n = N22(id + offset) * Time;
    return offset + sin(n) * 0.4;
}

float Line(vec2 p, vec2 a, vec2 b)
{
    float d = DistLine(p, a, b);
    float m = smoothstep(0.03, 0.01, d);
    float d2 = length(a - b);
    m *= smoothstep(1.2, 0.8, d2) * 0.5 + smoothstep(0.05, 0.03, abs(d2 - 0.75) );
    return m;
}

float Layer(vec2 uv)
{
    float m = 0.0;
    vec2 gv = fract(uv) -0.5;
    vec2 id = floor(uv);
    vec2 p[9];
    int i = 0;
    for (float y = -1; y <=1; y++)
    { 
        for (float x = -1; x <= 1; x++)
        {
            p[i++] = GetPos(id , vec2(x, y));
        }
    }
    float t = Time * 5.0;
    for (int i = 0; i < 9; i++)
    {
        m += Line(gv, p[4], p[i]);
        vec2 j = (p[i] - gv) * 20;
        float sparkle = 1 / dot(j, j);
        m += sparkle * (sin(t + fract(p[i].x) * 10) * 0.5 + 0.5);
    }
    m += Line(gv, p[1], p[3]);
    m += Line(gv, p[1], p[5]);
    m += Line(gv, p[7], p[3]);
    m += Line(gv, p[7], p[5]);
    return m;
}

void main()
{
    //Correct for the viewport size.
    vec2 uv = 0.5 * Pos.xy;
    uv.x *= Resolution.x / Resolution.y;
    float gradient = uv.y;
    float m = 0;
    float t = Time * 0.1;
    float s = sin(t);
    float c = cos(t);
    mat2 rot = mat2(c, -s, s, c);
    uv *= rot;
    vec2 ms = Mouse.xy / 100.0;
    ms *= rot;
    for (float i = 0.0; i <1.0; i += 1.0 / 4.0)
    {
        float z = fract(i + t);
        float size = mix(10.0, 0.5, z);
        float fade = smoothstep(0.0, 0.5, z) * smoothstep(1.0, 0.9, z);
        m += Layer(uv * size + i * 20 + ms) * fade;
    }
    vec3 base = sin(t * 5.0 * vec3(0.345, 0.456, 0.678)) * 0.4 + 0.6;
    vec3 col = m * base;
    col -= gradient * base;
    
    //if(gv.x > 0.48 || gv.y > 0.48) col = vec3(1.0, 0.0, 0.0);
    FragColor = vec4(col, 1.0);
}





