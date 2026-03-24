//Made by Martijn Steinrucken aka The Art of Code/BigWings - 2021
//Modified by Jos Gielen 2026

#version 330 core

in vec3 Pos;  //Pixel position: range = (-1,-1) to (1,1)

uniform float Time;
uniform vec3 Resolution;
uniform float Zoom;
uniform vec3 Mouse;

out vec4 FragColor;

#define MAX_STEPS 100
#define MAX_DIST 100.
#define SURF_DIST .001
#define TAU 6.283185
#define PI 3.141592
#define S smoothstep

//Material enumeration
const int BASE = 1;
const int BARS = 2;
const int BALL = 3;
const vec4 BASE_MAT = vec4(0.35, 0.25, 0.0, 0.5);  //brown
const vec4 BARS_MAT = vec4(0.5, 0.5, 0.5, 0.6);    //grey
const vec4 BALL_MAT = vec4(0.7, 0.7, 0.7, 0.8);   //grey metallic mirror

//Light Position
const vec3 LIGHT_POS = vec3(1.0, 2.0, 3.0);

//Function Prototypes
//-------------------
vec3 palette(float t);
vec3 GetNormal(vec3 p);
vec3 GetRayDir(vec2 uv, vec3 p, vec3 l, float z);
vec2 RayMarch(vec3 ro, vec3 rd);
float sdSphere(vec3 p, float s);
float sdBox3D(vec3 p, vec3 b);
float sdBox2D(vec2 p, vec2 b);
float sdBall(vec3 p, float angle);
float sdLineSeg(vec3 p, vec3 start, vec3 end);
float smin(float a, float b, float k);
vec2 Min(vec2 a, vec2 b);
mat2 rot2D (float angle);
mat3 rot3D (vec3 axis, float angle);
vec3 rot3D (vec3 p, vec3 axis, float angle);

//Get the distance of p to the scene and the material of that part
vec2 GetDist(vec3 p)
{
    float base = sdBox3D(p, vec3(1.0, 0.1, 0.5)) -0.1;  //Cube Signed Distance Function with rounded edges
    float bars = length(vec2(sdBox2D(p.xy, vec2(0.8, 1.4)) - 0.15, abs(p.z)-0.4)) - 0.04;
    //movement of the balls
    float a = 0.8*sin(Time * 3); //balls move in a sinusoidal movement
    float a1 = min(0.0, a); //limit the movement of ball 1 to the positive half of the sin
    float a5 = max(0.0, a); //limit the movement of ball 5 to the negative half of the sin
    float b1 = sdBall(p - vec3(0.6, 0.5, 0.0), a1);
    float b2 = sdBall(p - vec3(0.3, 0.5, 0.0), (a + a1) * 0.03); 
    float b3 = sdBall(p - vec3(0.0, 0.5, 0.0), a * 0.03);
    float b4 = sdBall(p - vec3(-0.3, 0.5, 0.0), (a + a5) * 0.03);
    float b5 = sdBall(p - vec3(-0.6, 0.5, 0.0), a5);
    float balls = min(b1,min(b2,min(b3,min(b4, b5))));
    float d = min(base, bars);
    d = min(d, balls);
    d = max(d, -p.y); //cut off the bottom
    vec2 result = vec2(0.0);
    if (d == base) {result = vec2(d, BASE);}
    else if (d == bars) {result = vec2(d, BARS);}
    else if (d == balls) {result = vec2(d, BALL);}
    return result;
}

//Returns the color and specular value of each part of the scene
vec4 GetMat(int a)
{
    if (a == BASE) {return BASE_MAT;}
    if (a == BARS) {return BARS_MAT;}
    if (a == BALL) {return BALL_MAT;}
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
    //Get the base color of each part
    vec3 col = vec3(0.0);
    vec4 mat = vec4(0.0);
    vec2 t = RayMarch(ro, rd);
    vec3 p = ro + rd * t.x;
    vec3 n = GetNormal(p);
    vec3 r = reflect(rd, n);  //Ray reflected at p
    mat = GetMat(int(t.y));
    float light = LightReflection(p, ro, mat.w);
    if (t.x < MAX_DIST)
    {
        col = vec3(light);    
        col *= mat.xyz;
    }
    //No reflections for the bars
    if (t.y == BARS)
    {
        return col;
    }
    //Add the reflections for the rest of the scene
    for (int i = 0; i < 2; i++)  //Increase loopcount for multiple reflections
    {
        ro = p + n * 3 * SURF_DIST;
        rd = r;
        vec2 t = RayMarch(ro, rd); 
        if(t.x < MAX_DIST) 
        {
            p = ro + rd * t.x;
            n = GetNormal(p);
            r = reflect(rd, n);
            vec4 mat2 = GetMat(int(t.y));
            col *= mat.w * mat2.xyz;
        }
    }
    return col;
}


void main()
{
    //Initialization
    vec3 ro = vec3(-1.0, 2.5, 3.0);  //Rays origin = camera position
    ro.yz *= rot2D(Mouse.y/200.0); //Vertical camera rotation
    ro.xz *= rot2D(-Mouse.x/100.0);//Horizontal camera rotation
    vec3 rd = GetRayDir(Pos.xy, ro, vec3(0.0, 0.75, 0.0), 0.1 * Zoom); //Direction of the ray from the origin to the pixel
    //get the color
    vec3 col = Render(ro, rd);
    FragColor = vec4(col.x, col.y, col.z, 1.0);
}


//Functions implementations
//-------------------------
vec3 palette(float t)
{
    vec3 a = vec3(0.5,0.5,0.5);
    vec3 b = vec3(0.5,0.5,0.5);
    vec3 c = vec3(1.0,1.0,1.0);
    vec3 d = vec3(0.263,0.416,0.557);
    return a + b*cos(6.28318*(c*t+d));
}

//Calculate the normal at the point where the ray hits the scene
vec3 GetNormal(vec3 p) 
{
    vec2 e = vec2(.001, 0);
    vec3 n = GetDist(p).x - vec3(GetDist(p-e.xyy).x, GetDist(p-e.yxy).x, GetDist(p-e.yyx).x);
    return normalize(n);
}

//Calculate the direction of a ray from the origin to point uv 
//when looking in direction look and with zoom factor zoom.
vec3 GetRayDir(vec2 uv, vec3 orig, vec3 look, float zoom) 
{
    vec3 f = normalize(look - orig);
    vec3 r = normalize(cross(vec3(0.0, 1.0, 0.0), f));
    vec3 u = cross(f, r);
    vec3 c = f * zoom;
    vec3 i = c + uv.x * r + uv.y * u;
    return normalize(i);
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

//Signed Distance of p to a ball of radius r at position pos.
float sdBall(vec3 p, float angle)
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
    return d;
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

//Smooth minimum function to morph objects together
float smin(float a, float b, float k)
{
    float h = max(k - abs(a - b), 0.0) / k;
    return min(a, b) - h * h * h * k * (1.0 / 6.0);
}

vec2 Min(vec2 a, vec2 b)
{
    return a.x < b.x ? a : b;
}

//2D rotation around the X, Y or Z axes
mat2 rot2D (float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    return mat2(c, -s, s, c);
}

//General 3D rotation around a given axis
mat3 rot3D (vec3 axis, float angle)
{
    axis = normalize(axis);
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;

    return mat3(
    oc * axis.x * axis.x + c,
    oc * axis.x * axis.y - axis.z * s,
    oc * axis.z * axis.x + axis.y * s,
    oc * axis.x * axis.y + axis.z * s,
    oc * axis.y * axis.y + c,
    oc * axis.y * axis.z - axis.x * s,
    oc * axis.z * axis.x - axis.y * s,
    oc * axis.y * axis.z + axis.x * s,
    oc * axis.z * axis.z + c
    );
}

//3D rotation around an axis through a point
vec3 rot3D (vec3 p, vec3 axis, float angle)
{
    //Rodrigues rotation formula
    return mix(dot(axis, p) * axis, p, cos(angle)) + cross(axis, p) * sin(angle);
}
