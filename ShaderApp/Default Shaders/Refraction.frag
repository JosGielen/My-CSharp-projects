//Cubemap:C:\Users\Joseph Gielen\Documents\Visual Studio 18\My Programs\C# Programs\ShaderToy.Net\ShaderToy.Net\bin\Debug\net10.0-windows\Textures\Chamberlains\1.png
//Cubemap:C:\Users\Joseph Gielen\Documents\Visual Studio 18\My Programs\C# Programs\ShaderToy.Net\ShaderToy.Net\bin\Debug\net10.0-windows\Textures\Chamberlains\2.png
//Cubemap:C:\Users\Joseph Gielen\Documents\Visual Studio 18\My Programs\C# Programs\ShaderToy.Net\ShaderToy.Net\bin\Debug\net10.0-windows\Textures\Chamberlains\3.png
//Cubemap:C:\Users\Joseph Gielen\Documents\Visual Studio 18\My Programs\C# Programs\ShaderToy.Net\ShaderToy.Net\bin\Debug\net10.0-windows\Textures\Chamberlains\4.png
//Cubemap:C:\Users\Joseph Gielen\Documents\Visual Studio 18\My Programs\C# Programs\ShaderToy.Net\ShaderToy.Net\bin\Debug\net10.0-windows\Textures\Chamberlains\5.png
//Cubemap:C:\Users\Joseph Gielen\Documents\Visual Studio 18\My Programs\C# Programs\ShaderToy.Net\ShaderToy.Net\bin\Debug\net10.0-windows\Textures\Chamberlains\6.png
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
    p.xz *= rot2D(Time * 0.3);
    float d = sdBox3D(p, vec3(1.0));  //Cube of size 1
    //Folding plane creation
    float c = cos(PI / 5.0);
    float s = sqrt(0.75 - c * c);
    vec3 n = vec3(-0.5, -c, s);
    //Folding The cube 3 times
    p = abs(p);
    p -= 2.0  * min(0.0, dot(p, n)) * n;
    p.xy = abs(p.xy);
    p -= 2.0  * min(0.0, dot(p, n)) * n;
    p.xy = abs(p.xy);
    p -= 2.0  * min(0.0, dot(p, n)) * n;
    //Bringing the object into view
    d = p.z - 1.0;
    return vec2(d, 0.0);  
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
    vec2 e = vec2(.005, 0);
    vec3 n = GetDist(p).x - vec3(GetDist(p-e.xyy).x, GetDist(p-e.yxy).x, GetDist(p-e.yyx).x);
    return normalize(n);
}

//The actual Ray Marching algorithm
//side = 1.0 outside the objects, -1.0 inside refracting objects
vec2 RayMarch(vec3 ro, vec3 rd, float side) 
{
    vec2 dist = vec2(0.0);
	float totalDist = 0.;  //Total distance travelled by the Ray
    for(int i = 0; i < MAX_STEPS; i++) 
    {
    	vec3 p = ro + rd * totalDist;  //current position of the ray
        dist = GetDist(p) * side;       //distance of the current position to the scene and name of the part hit
        totalDist += dist.x;     //total distance the ray has "marched"
        if(totalDist > MAX_DIST || abs(dist.x) < SURF_DIST) { break;}
    }
    return vec2(totalDist, dist.y);
}

//Calculate the color of the pixel 
vec3 Render(vec3 ro, vec3 rd)
{
    vec3 col = vec3(0.0);
    vec3 backcol = texture(cubemap, rd).rgb;
    vec3 reflcol = vec3(0.0);
    vec3 refrcol = vec3(0.0);
    float fresnel = 0.0;
    vec3 nHit = vec3(0.0);
    vec2 dOut = RayMarch(ro, rd, 1.0);  //Distance to the outside of the object
    if (dOut.x < MAX_DIST)
    {
        vec3 pHit = ro + rd * dOut.x; //Ray hits the outside of the object
        nHit = GetNormal(pHit);
        vec3 DirOut = reflect(rd, nHit);  //Reflection on the outside surface.
        reflcol = texture(cubemap, DirOut).rgb;
        //Refraction
        const float IOR = 1.81;  //Index of refraction
        const float Abb = 0.02;  //Chromatic abberation
        vec3 DirIn = refract(rd, nHit, 1 / IOR); //Ray direction inside the object
        vec3 pEnter = pHit - 3.0 * SURF_DIST * nHit;    //startpoint of the inside ray
        vec2 dIn = RayMarch(pEnter, DirIn, -1.0);   //Distance travelled inside the object
		vec3 pExit = pEnter + DirIn * dIn.x; //Inside Ray hits the edge of the object
        vec3 nExit = -GetNormal(pExit);
        //Red
        vec3 DirExit = refract(DirIn, nExit, IOR - Abb);
		if (dot(DirExit, DirExit) == 0)  //Total internal reflection (x, y and z are 0)
        { 
            DirExit = reflect(DirIn, nExit);
        }
		refrcol.r = texture(cubemap, DirExit).r;
        //Green
        DirExit = refract(DirIn, nExit, IOR);
		if (dot(DirExit, DirExit) == 0)  //Total internal reflection (x, y and z are 0)
        { 
            DirExit = reflect(DirIn, nExit);
        }
		refrcol.g = texture(cubemap, DirExit).g;
        //Blue
        DirExit = refract(DirIn, nExit, IOR + Abb);
		if (dot(DirExit, DirExit) == 0)  //Total internal reflection (x, y and z are 0)
        { 
            DirExit = reflect(DirIn, nExit);
        }
		refrcol.b = texture(cubemap, DirExit).b;
        //Absorption in the object
        float dens = 0.0;
        float optDist = exp(-dIn.x * dens);
        refrcol = optDist * refrcol;
        //Specular reflection
        fresnel = pow(1 + dot(rd, nHit),5.0);
        col = mix(refrcol, reflcol, fresnel);
        //Colored versions
        //col *= vec3(0.878, 0.0667, 0.373);  //Ruby
        //col *= vec3(0.059, 0.322, 0.729);   //Saphire
        //col *= vec3(0.314, 0.884, 0.471);   //Emerald
        //col = 0.5 * nHit + 0.5;             //Diffuse
    }
    else
    {
        col = backcol;
    }
    col = pow(col, vec3(0.8));  //Gamma correction
    return col ;
}

void main()
{
    //Correct for the viewport size.
    vec2 uv = Pos.xy;
    uv.x *= Resolution.x / Resolution.y;
    //Initialization
    vec3 ro = vec3(0.0, 0.25, -3.0);  //Rays origin = camera position
    ro.yz *= rot2D(Mouse.y/200.0);  //Vertical camera rotation
    ro.xz *= rot2D(-Mouse.x/100.0); //Horizontal camera rotation
    vec3 rd = GetRayDir(uv, ro, vec3(0.0, 0.0, 0.0), 0.1 * Zoom); //Direction of the ray from the origin to the pixel
    //get the color
    vec3 col = Render(ro, rd);
    FragColor = vec4(col.x, col.y, col.z, 1.0);
}






