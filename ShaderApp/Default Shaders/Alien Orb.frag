#version 420 core
#extension GL_ARB_explicit_uniform_location : enable

in vec3 Pos;  //Pixel position
in vec2 texCoord;

layout(location = 2) uniform float Time;  //used for animation
layout(location = 3) uniform float Zoom;  //mouse wheel used to zoom in/out
layout(location = 4) uniform vec3 Mouse;  //mouse movement
layout(location = 5) uniform vec3 Resolution; //Size of the GLWpfControl

out vec4 FragColor;

#define MAX_STEPS 100
#define MAX_DIST 100.
#define SURF_DIST .001
#define TAU 6.283185
#define PI 3.141592
#define S smoothstep

//Material enumeration
const int BALL = 1;
const int PLANE = 2;
const vec4 BALL_MAT = vec4(0.6, 0.2, 0.7, 1.0);  //grey mirror reflection
const vec4 PLANE_MAT = vec4(0.8, 0.6, 0.2, 0.1);    //brown little reflection

//Function Prototypes
vec3 GetRayDir(vec2 uv, vec3 p, vec3 l, float z);
vec2 RayMarch(vec3 ro, vec3 rd);
float compute_reflection(in vec3 p, in vec3 n, in vec3 or, in float cr);
vec3 GetNormal(vec3 p);
float sdSphere(vec3 p, float s);
float sdBallGyroid(vec3 p);
float smin(float a, float b, float k);
vec3 palette(float t);
mat2 rot2D (float angle);
mat3 rot3D (vec3 axis, float angle);
vec3 rot3D (vec3 p, vec3 axis, float angle);


//Returns the Distance of p to the scene and the scene part
vec2 GetDist(vec3 p)
{
    float ball = sdSphere(p, 1.0);  //Sphere with radius 1.
    ball = abs(ball) - 0.03;        //Make the sphere hollow with thickness 0.06.
    float g = sdBallGyroid(p);      //3D oscillating plane
    ball = smin(ball,g, -0.025);    //Smooth boolian intersection between the ball and the gyroid    
    float plane = p.y + 1.0;        //Ground plane
    float d = min(0.9 * plane, ball);
    int mat = 0;
    if(d == ball) {mat = BALL;}
    if(d == 0.9 * plane) {mat = PLANE;}
    return vec2(d, mat);  
}

//Returns the color and specular value of each part of the scene
vec4 GetMat(int a)
{
    if (a == BALL) {return BALL_MAT;}
    if (a == PLANE) {return PLANE_MAT;}
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
    vec3 r = reflect(rd, n);
    vec3 lightDir = -normalize(p); //Light at the center of the ball = origin
    float dif = dot(n, lightDir) * 0.5 + 0.5;
    float cd = length(p);  //Distance from the origin
    if (t.x < MAX_DIST)
    {
        col = vec3(dif);
        mat = GetMat(int(t.y));
        col *= mat.xyz;

        //make shadows on the plane
        if(cd > 1.03) //outside of the ball
        {
            float s = sdBallGyroid(-lightDir);
            float w = cd * 0.02;
            float shadow = smoothstep(-w, w, s);
            col *= shadow;
            col /= cd;
        }
        //Add a bright 2D light at the center
        float d = dot(Pos,Pos);
        float light = 0.003 / d * smoothstep(0.0, 0.5, t.x - 2.5); //reduce the light diameter and prevent it from shining through the ball
        col += light ;
    }
    col += compute_reflection(p, n, ro, 0.7 * mat.w);
    return col;
}

//Ray marching Example
//-------------------
void main()
{
    //Correct for the viewport size.
    vec2 uv = Pos.xy;
    uv.x *= Resolution.x / Resolution.y;
    //Initialization
    vec3 ro = vec3(0.0, 1.5, -3.0);  //Rays origin = camera position
    ro.yz *= rot2D(Mouse.y/200.0); //Vertical camera rotation
    ro.y = max(-0.9, ro.y);
    ro.xz *= rot2D(-Mouse.x/100.0);//Horizontal camera rotation
    vec3 rd = GetRayDir(uv, ro, vec3(0.0, 0.0, 0.0), 0.1 * Zoom); //Direction of the ray from the origin to the pixel
    //get the color
    vec3 col = Render(ro, rd);
    FragColor = vec4(col.x, col.y, col.z, 1.0);
}


//Functions implementations
//-------------------------

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

//Light reflection at point p normal n towards camera at or with reflectivity cr
float compute_reflection(in vec3 p, in vec3 n, in vec3 or, in float cr) 
{
  vec3 l = vec3(2.0,2.0,-5.0); // Light Position
  vec3 ld = normalize(l-p); // Light Vector
  vec3 r = reflect(-ld, n); // reflected light vector
  float s = clamp(dot(r, normalize(or - p)), 0., 1.); // dot product between reflected light and camera vector
  return pow(s, 10.0) * cr;
}

//Calculate the normal at the point where the ray hits the scene
vec3 GetNormal(vec3 p) 
{
    vec2 e = vec2(.001, 0);
    vec3 n = GetDist(p).x - vec3(GetDist(p-e.xyy).x, GetDist(p-e.yxy).x, GetDist(p-e.yyx).x);
    return normalize(n);
}

//Signed Distance of p to a sphere or radius s at the origin.
float sdSphere(vec3 p, float s)
{
    return length(p) - s;
}

//Signed distance function of a rotating gyroid
float sdBallGyroid(vec3 p)
{
    p.zy *= rot2D(0.5*Time);
    p *= 10.0;
    return abs(0.7 * dot(sin(p), cos(p.yzx)) / 10.0)-0.03;
}


//Smooth minimum function to morph objects together
//Negative k equals a smoothMax function
float smin(float a, float b, float k)
{
    float h = clamp(0.5 + 0.5*(b-a) / k, 0.0, 1.0);
    return mix(b, a, h) - k * h * (1.0 - h);
}

//Calculate a smooth color palette
vec3 palette(float t)
{
    vec3 a = vec3(0.5,0.5,0.5);
    vec3 b = vec3(0.5,0.5,0.5);
    vec3 c = vec3(1.0,1.0,1.0);
    vec3 d = vec3(0.263,0.416,0.557);
    return a + b*cos(6.28318*(c*t+d));
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



