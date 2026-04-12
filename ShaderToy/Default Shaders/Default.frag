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

//Scene items enumeration
const int BOX = 1;

//Materials enumeration
const vec4 BOX_MAT = vec4(1.0, 1.0, 1.0, 1.0);

//Light Position
const vec3 LIGHT_POS = vec3(-5.0, 4.0, 0.0);


//2D rotation around the X, Y or Z axes
mat2 rot2D (float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    return mat2(c, -s, s, c);
}

//Signed Distance of p to a Cube of size b at the origin.
float sdBox3D(vec3 p, vec3 s)
{
    p.xz *= rot2D(Time * 0.5);
    p = abs(p) - s;
    return length(max(p, 0.0)) + min(max(p.x, max(p.y, p.z)), 0.0);
}

//Returns the Distance of p to the scene and the scene part that contains p
vec2 GetDist(vec3 p)
{
    float d = sdBox3D(p, vec3(0.5, 0.5, 0.5));  //Cube of size 0.5
    int item = BOX;
    return vec2(d, item);  
}

//Returns the color and specular value of each part of the scene
vec4 GetMat(int a)
{
    if (a == BOX) {return BOX_MAT;}
}

//Calculate the direction of a ray from the origin to point uv 
//when looking in direction look and with zoom factor zoom.
vec3 GetRayDir(vec2 p, vec3 orig, vec3 look, float zoom) 
{
    vec3 f = normalize(look - orig);
    vec3 r = normalize(cross(vec3(0.0, 1.0, 0.0), f));
    vec3 u = cross(f, r);
    vec3 c = f * zoom;
    vec3 i = c + p.x * r + p.y * u;
    return normalize(i);
}

//Calculate the normal at the point where the ray hits the scene
vec3 GetNormal(vec3 p) 
{
    vec2 e = vec2(.001, 0);
    vec3 n = GetDist(p).x - vec3(GetDist(p-e.xyy).x, GetDist(p-e.yxy).x, GetDist(p-e.yyx).x);
    return normalize(n);
}

//Light reflection at point p with normal n towards camera (or) with reflectivity cr
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

//The actual Ray Marching algorithm
vec2 RayMarch(vec3 ro, vec3 rd) 
{
    vec2 dist = vec2(0.0);
	float totalDist = 0.;  //Total distance travelled by the Ray.
    for(int i = 0; i < MAX_STEPS; i++) 
    {
    	vec3 p = ro + rd * totalDist;  //current position of the ray
        dist = GetDist(p);       //distance of the current position to the scene and name of the part hit
        totalDist += dist.x;     //total distance the ray has "marched"
        if(totalDist > MAX_DIST || abs(dist.x) < SURF_DIST) { break;}
    }
    return vec2(totalDist, dist.y);
}

//Calculate the color of the pixel 
vec3 Render(vec3 ro, vec3 rd)
{
    //Get the base color of each part
    vec3 col = vec3(0.0);
    vec2 t = RayMarch(ro, rd);
    vec3 p = ro + rd * t.x;
    vec3 n = GetNormal(p);
    vec3 r = reflect(n, rd);
    vec4 mat = GetMat(int(t.y));
    float light = LightReflection(p, ro, mat.w);
    if (t.x < MAX_DIST)
    {
        //col = vec3(light);
        //mat = GetMat(int(t.y));
        //col *= mat.xyz;

        col = 0.5 * n + 0.5;
    }
    return col;
}

//The Starting point
//------------------
void main()
{
    //Correct for the viewport size.
    vec2 uv = Pos.xy;
    uv.x *= Resolution.x / Resolution.y;
    //Initialization
    vec3 ro = vec3(0.0, 1.0, 2.0);  //Rays origin = camera position
    ro.yz *= rot2D(Mouse.y/200.0);  //Vertical camera rotation
    ro.y = max(-0.9, ro.y);         //Prevent the camera to go below the ground
    ro.xz *= rot2D(-Mouse.x/100.0); //Horizontal camera rotation
    vec3 rd = GetRayDir(uv, ro, vec3(0.0, 0.0, 0.0), Zoom); //Direction of the ray from the origin to the pixel
    //get the color
    vec3 col = Render(ro, rd);
    FragColor = vec4(col.x, col.y, col.z, 1.0);
}



