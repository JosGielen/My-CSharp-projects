#version 330 core

in vec3 Pos;          //Pixel position: range = (-1,-1) to (1,1)
uniform float time;
out vec4 FragColor;

vec3 palette(float t)
{
    vec3 a = vec3(0.5,0.5,0.5);
    vec3 b = vec3(0.5,0.5,0.5);
    vec3 c = vec3(1.0,1.0,1.0);
    vec3 d = vec3(0.263,0.416,0.557);
    return a + b*cos(6.28318*(c*t+d));
}

void main()
{
    vec3 p = Pos;
    vec3 finalColor = vec3(0.0);
    for (float i = 0.0; i < 4.0; i++)
    {
        p = fract(1.5 * p) - 0.5;
        float d = length(p) * exp(-length(Pos));
        vec3 color = palette(length(Pos) + 0.4 * (i + time));
        d = sin(d * 8.0 + time) / 8.0;
        d = abs(d);
        d = pow(0.005 / d, 1.2);
        vec3 u = vec3(d * color.x, d * color.y, d * color.z);
        finalColor += u;
    }
    FragColor = vec4(finalColor.x, finalColor.y, finalColor.z, 1.0);
}

