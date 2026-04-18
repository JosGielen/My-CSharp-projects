//Cubemap:C:\Users\Joseph Gielen\Documents\Visual Studio 18\My Programs\C# Programs\ShaderToy.Net\ShaderToy.Net\bin\Debug\net10.0-windows\Textures\Chamberlains\1.png
//Cubemap:C:\Users\Joseph Gielen\Documents\Visual Studio 18\My Programs\C# Programs\ShaderToy.Net\ShaderToy.Net\bin\Debug\net10.0-windows\Textures\Chamberlains\2.png
//Cubemap:C:\Users\Joseph Gielen\Documents\Visual Studio 18\My Programs\C# Programs\ShaderToy.Net\ShaderToy.Net\bin\Debug\net10.0-windows\Textures\Chamberlains\3.png
//Cubemap:C:\Users\Joseph Gielen\Documents\Visual Studio 18\My Programs\C# Programs\ShaderToy.Net\ShaderToy.Net\bin\Debug\net10.0-windows\Textures\Chamberlains\4.png
//Cubemap:C:\Users\Joseph Gielen\Documents\Visual Studio 18\My Programs\C# Programs\ShaderToy.Net\ShaderToy.Net\bin\Debug\net10.0-windows\Textures\Chamberlains\5.png
//Cubemap:C:\Users\Joseph Gielen\Documents\Visual Studio 18\My Programs\C# Programs\ShaderToy.Net\ShaderToy.Net\bin\Debug\net10.0-windows\Textures\Chamberlains\6.png
//Texture:C:\Users\Joseph Gielen\Documents\Visual Studio 18\My Programs\C# Programs\ShaderToy.Net\ShaderToy.Net\bin\Debug\net10.0-windows\Textures\SeamlessTiles0003.jpg

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

out vec4 FragColor;

#define MAX_STEPS 100
#define MAX_DIST 100.
#define SURF_DIST .001
#define TAU 6.283185
#define PI 3.141592

//Material enumeration
const int BASE = 1;
const int BARS = 2;
const int BALL = 3;
const int LINE = 4;
const int TABLE = 5;
const vec4 BASE_MAT = vec4(0.5, 0.35, 0.1, 0.3);  //brown
const vec4 BARS_MAT = vec4(0.6, 0.6, 0.6, 0.5);    //grey
const vec4 BALL_MAT = vec4(0.4, 0.4, 0.4, 0.9);    //grey metallic mirror
const vec4 LINE_MAT = vec4(0.6, 0.5, 0.1, 0.4);    //Yellow
const vec4 TABLE_MAT = vec4(0.3, 0.3, 0.3, 1.0);   //White

//Light Position
const vec3 LIGHT_POS = vec3(1.0, 2.0, 3.0);

//2D rotation around the X, Y or Z axes
mat2 rot2D (float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    return mat2(c, -s, s, c);
}

vec2 Min(vec2 a, vec2 b)
{
    return a.x < b.x ? a : b;
}

//Signed Distance op p to a cylinder
float sdCylinder(vec3 p, float r)
{
    float d = length(p.xz) - (r - 0.1 * p.y);
    return d;
}

//Signed Distance of p to a sphere of radius r at the origin.
float sdSphere(vec3 p, float r)
{
    return length(p) - r;
}
 
//Signed Distance of p to a Cube of size b at the origin.
float sdBox3D(vec3 p, vec3 s)
{
    p = abs(p) - s;
    return length(max(p, 0.0)) + min(max(p.x, max(p.y, p.z)), 0.0);
}

//Signed Distance of p to a square of size b at the origin.
float sdBox2D(vec2 p, vec2 s)
{
    p = abs(p) - s;
    return length(max(p, 0.0)) + min(max(p.x, p.y), 0.0);
}

//Signed Distance of p to a Line segment between start and end
float sdLineSeg(vec3 p, vec3 a, vec3 b)
{
    vec3 ap = p - a;
    vec3 ab = b - a;
    float t = clamp(dot(ap, ab) / dot(ab, ab), 0.0, 1.0);
    vec3 c = a + ab * t;
    return length(p - c);
}

//Signed Distance of p to a ball of radius r at position pos.
vec2 sdBall(vec3 p, float angle)
{
    p.y -= 1.01;   //move the position to allow rotation around the line end (at the bar)
    p.xy *= rot2D(angle);
    p.y += 1.01;   //undo the position change to get the scene back into the correct place
    float ball = length(p) - 0.15;
    float ring = length(vec2(length(p.xy - vec2(0, 0.15)) - 0.03, p.z)) - 0.01;
    ball = min(ball, ring);
    p.z = abs(p.z);
    float line = sdLineSeg(p, vec3(0.0, 0.15, 0.0), vec3(0.0, 1.01, 0.4)) - 0.005;
    float d = min(ball, line);
    return vec2(d, d==ball ? BALL : LINE);
}

//Calculate the direction of a ray from the origin to point uv 
//when looking in direction look and with zoom factor zoom.
vec3 GetRayDir(vec2 uv, vec3 p, vec3 look, float zoom) 
{
    vec3 f = normalize(look - p);
    vec3 r = normalize(cross(vec3(0.0, 1.0, 0.0), f));
    vec3 u = cross(f, r);
    vec3 c = f * zoom;
    vec3 i = c + uv.x * r + uv.y * u;
    return normalize(i);
}

//Get the distance of p to the scene and the material of that part
vec2 GetDist(vec3 p)
{
    float base = sdBox3D(p, vec3(1.0, 0.1, 0.5)) -0.1;  //Cube Signed Distance Function with rounded edges
    float bars = length(vec2(sdBox2D(p.xy, vec2(0.8, 1.4)) - 0.15, abs(p.z)-0.4)) - 0.04;
    //movement of the balls
    float a = 0.8*sin(Time * 3); //balls move in a sinusoidal movement
    float a1 = min(0.0, a); //limit the movement of ball 1 to the positive half of the sin
    float a5 = max(0.0, a); //limit the movement of ball 5 to the negative half of the sin
    vec2 b1 = sdBall(p - vec3(0.6, 0.5, 0.0), a1);
    vec2 b2 = sdBall(p - vec3(0.3, 0.5, 0.0), (a + a1) * 0.03); 
    vec2 b3 = sdBall(p - vec3(0.0, 0.5, 0.0), a * 0.03);
    vec2 b4 = sdBall(p - vec3(-0.3, 0.5, 0.0), (a + a5) * 0.03);
    vec2 b5 = sdBall(p - vec3(-0.6, 0.5, 0.0), a5);
    vec2 balls = Min(b1, Min(b2, Min(b3, Min(b4, b5))));
    float d = min(base, bars);
    d = min(d, balls.x);
    base = max(base, -p.y);
    d = max(d, -p.y); // cut off the bottom
    float d2 = sdCylinder(p, 1.5);
    d2 = max(d2, p.y);
    d = min(d, d2);
    int mat = 0;
    if(d == base) { mat = BASE; }
    else if(d == bars) { mat = BARS; }
    else if(d == d2) { mat = TABLE; }
    else if(d == balls.x) { mat = int(balls.y); }
    return vec2(d, mat);
}

//Calculate the normal at the point where the ray hits the scene
vec3 GetNormal(vec3 p) 
{
    vec2 e = vec2(.001, 0);
    vec3 n = GetDist(p).x - vec3(GetDist(p-e.xyy).x, GetDist(p-e.yxy).x, GetDist(p-e.yyx).x);
    return normalize(n);
}

//The actual Ray Marching algorithm
vec2 RayMarch(vec3 ro, vec3 rd) 
{
    vec2 dist = vec2(0.0);
	float totalDist = 0.;  //Total distance travelled by the Ray.
    for(int i = 0; i < MAX_STEPS; i++) 
    {
    	vec3 p = ro + rd * totalDist;  //current position of the ray
        dist = GetDist(p);       //distance of the current position to the scene
        totalDist += dist.x;             //total distance the ray has "marched"
        if(totalDist > MAX_DIST || abs(dist.x) < SURF_DIST) { break;}
    }
    return vec2(totalDist, dist.y);
}

//Smooth minimum function to morph objects together
float smin(float a, float b, float k)
{
    float h = max(k - abs(a - b), 0.0) / k;
    return min(a, b) - h * h * h * k * (1.0 / 6.0);
}

//Returns the color and specular value of each part of the scene
vec4 GetMat(int a)
{
    if (a == BASE) { return BASE_MAT; }
    if (a == BARS) { return BARS_MAT; }
    if (a == BALL) { return BALL_MAT; }
    if (a == LINE) { return LINE_MAT; }
    if (a == TABLE) { return TABLE_MAT; }
}

//Light reflection at point p with reflectivity cr towards the camera (or)
float LightReflection(in vec3 p, in vec3 or, in float cr) 
{
    //Diffuse reflected light
    vec3 ld = normalize(LIGHT_POS - p); // Light Vector
    vec3 n = GetNormal(p);
    float dif = dot(n, normalize(LIGHT_POS)) * 0.5 + 0.5;
    //Specular reflected light
    vec3 r = reflect(-ld, n); // reflected light vector
    float s = clamp(dot(r, normalize(or - p)), 0., 1.); // dot product between reflected light and camera vector
    float spec = pow(s, 20.0) * cr;
    return dif + spec;
}

//Calculate the color of the pixel 
vec3 Render(vec3 ro, vec3 rd)
{
    vec3 col = texture(cubemap, rd).rgb;
    vec2 t = RayMarch(ro, rd);
    if (t.x < MAX_DIST)
    {
	 vec3 p = ro + rd * t.x;
        vec3 n = GetNormal(p);
        vec4 mat = GetMat(int(t.y));
        float light = LightReflection(p, ro, mat.w);
        vec3 r = reflect(rd, n);  //Ray reflected at p
        vec3 refl = texture(cubemap, r).rgb;
	 col = vec3(light);    
        //No reflections for the bars
        if (t.y == BARS || t.y == LINE)
        {
            return mat.xyz * col;
        }
        if (t.y == TABLE) { col = texture(texture0, p.xy).rgb; return col;}
        if (t.y == BASE) { col *= mat.xyz; col *= 1.5;}
        //Add the reflections for the rest of the scene
        for (int i = 0; i < 2; i++)  //Increase loopcount for multiple reflections
        {
            ro = p + n * 3 * SURF_DIST;
            rd = r;
            vec2 t = RayMarch(ro, rd); 
            if(t.x < MAX_DIST) //reflect the part
            {
                p = ro + rd * t.x;
                n = GetNormal(p);
                r = reflect(rd, n);
                vec4 mat2 = GetMat(int(t.y));
                col *= mat2.w * mat2.xyz;
            }
            else
            {
                col *= texture(cubemap, r).rgb; //reflect the cubemap
            }
        }
    }
    return 1.8 * col;
}

void main()
{
    //Correct for the viewport size.
    vec2 uv = Pos.xy;
    uv.x *= Resolution.x / Resolution.y;
    //Initialization
    vec3 ro = vec3(-1.0, 2.5, 3.0);  //Rays origin = camera position
    ro.yz *= rot2D(Mouse.y/200.0); //Vertical camera rotation
    ro.xz *= rot2D(-Mouse.x/100.0);//Horizontal camera rotation
    vec3 rd = GetRayDir(uv, ro, vec3(0.0, 0.75, 0.0), 0.25 * Zoom); //Direction of the ray from the origin to the pixel
    //get the color
    vec3 col = Render(ro, rd);
    FragColor = vec4(col.x, col.y, col.z, 1.0);
}
