//Texture:C:\Users\Joseph Gielen\Documents\Visual Studio 18\My Programs\C# Programs\ShaderToy.Net\ShaderToy.Net\bin\Debug\net10.0-windows\Textures\Metal.jpg
#version 420 core
#extension GL_ARB_explicit_uniform_location : enable

in vec3 Pos;  //Pixel position
in vec2 texCoord;

layout(location = 2) uniform float Time;  //used for animation
layout(location = 3) uniform float Zoom;  //mouse wheel used to zoom in/out
layout(location = 4) uniform vec3 Mouse;  //mouse movement
layout(location = 5) uniform vec3 Resolution; //Size of the GLWpfControl

uniform sampler2D texture0;

out vec4 FragColor; //pixel color

#define MAX_STEPS 100
#define MAX_DIST 100.
#define SURF_DIST .001
#define TAU 6.283185
#define PI 3.141592

//Light Position
const vec3 LIGHT_POS = vec3(1.0, 2.0, 3.0);

//2D rotation around the X, Y or Z axes
mat2 rot2D (float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    return mat2(c, -s, s, c);
}

//signed Distance of p to a infinite height box of size r (width, depth)
float sdBox(vec2 p, vec2 r)
{
    return length(max(abs(p)-r,0.0));
}

float sdCross(in vec3 p, in vec3 r) 
{
    p = abs(p);
    p.xz = (p.z > p.x) ? p.zx : p.xz;  //reflect p along the z=x line
    return length(max(abs(p.xyz)-r, 0.0));
}

float sdVStick(vec3 p, float l)
{
    p.y = max(p.y - l, 0.0);
    return length(p);
}

float smax(in float a, in float b, in float k)
{
    float h = max(k - abs(a - b), 0.0);
    return max(a,b) + (0.25 / k) * h * h ;
}

//Signed Distance of p to a sphere of radius r and center at location.
float sdSphere(vec3 p, float r)
{
    return length(p) - r;
}

vec2 rot45( vec2 v)
{
    return vec2(v.x-v.y,v.y+v.x)*0.707107;
}

vec4 sdGear(vec3 p, float t, float offset)
{
	//Rotation of the Gear
    float an = t * sign(p.y) + offset * 6.283185/24.0; 
    float co = cos(an);
	float si = sin(an);
    p.xz = mat2(co,-si,si,co)*p.xz;
	//Mirrors the gear (above and below the xz plane)
    p.y=abs(p.y);
    float d = 0.0;
	//mirror an infinite height box 12 times in a circle
    float an2 = 6.283185 / 12.0;
    float sector = round(atan(p.z, p.x) / an2);
    float angrot = sector * an2;
    vec3 q = p;
    q.xz = mat2(cos(angrot),-sin(angrot),
                sin(angrot), cos(angrot)) * q.xz;    
    d = sdBox( q.xz - vec2(0.17,0.0), vec2(0.04,0.02) ) - 0.005;
    //infinite height ring
    float d2 = abs(length(p.xz) - 0.155) - 0.018;
    d = min(d,d2);  //Join boxes and ring to create an infinite height gear
    //cut the gear  along a spherical shell of thickness +- 0.03
    float r = length(p) + 0.005 * length(texture(texture0, 7 * q.zy));  //Use a Texture as a bump map
    d = smax( d, abs(r - 0.5) - 0.03, 0.005 );
	//Make a cross inside the gear
    d2 = sdCross( p + vec3(0.0, -0.48, 0.0), vec3(0.15, 0.003, 0.003)) - 0.003;
    d = min(d,d2);
    //Add an axel between both gears
    d2 = length(vec3(p.x,max(p.y - 0.5, 0.0), p.z)) - 0.01;
    d = min(d, d2);
	return vec4(d, q);
}

vec4 GetDist(vec3 p)
{
    vec4 d1;
    vec4 d2;
    vec3 q = p;
	//mirror q along 45° planes to create 3 sets of gears at right angles
    if( abs(q.x)>abs(q.y) && abs(q.x)>abs(q.z) )
    {
        q = q.yxz*vec3(-1,1,1);
    }
    else if( abs(q.z)>abs(q.y) && abs(q.z)>abs(q.x) )
    {
        q = q.xzy*vec3(-1,1,1);
    }
    d1 = sdGear(q,Time, 0.0);
    // X : Rotate p 45° along the X-axis to create another set of gears
    vec3 qx = vec3(rot45(p.zy),p.x);
    if(abs(qx.x)>abs(qx.y))
    {
        qx=qx.zxy;
    }
    d2 = sdGear(qx,Time, 1.0);
    d1 = (d1.x < d2.x) ? d1 : d2;
    // Y : Rotate p 45° along the Y-axis to create another set of gears
	vec3 qy = vec3(rot45(p.xz),p.y);
    if(abs(qy.x)>abs(qy.y))
    {
        qy=qy.zxy;
    }
    d2 = sdGear(qy,Time, 1.0);
    d1 = (d1.x < d2.x) ? d1 : d2;
    // Z : Rotate p 45° along the Z-axis to create another set of gears   
	vec3 qz = vec3(rot45(p.yx),p.z); 
    if(abs(qz.x)>abs(qz.y))
    {
        qz=qz.zxy;
    }
    d2 = sdGear(qz,Time, 1.0);
    d1 = (d1.x < d2.x) ? d1 : d2;
	//Make the central connection hub.
    float f = sdSphere(p,0.1);
    d1 = (d1.x < f) ? d1 : vec4(f, d1.y, d1.z, d1.w);
    return d1;
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
    float spec = 2 * pow(s, 20.0) * cr;
    return dif + spec;
}

//The actual Ray Marching algorithm
vec4 RayMarch(vec3 ro, vec3 rd) 
{
    vec4 dist = vec4(0.0);
	float totalDist = 0.;  //Total distance travelled by the Ray.
    for(int i = 0; i < MAX_STEPS; i++) 
    {
    	vec3 p = ro + rd * totalDist;  //current position of the ray
        dist = GetDist(p);       //distance of the current position to the scene and name of the part hit
        totalDist += dist.x;     //total distance the ray has "marched"
        if(totalDist > MAX_DIST || abs(dist.x) < SURF_DIST) { break;}
    }
    return vec4(totalDist, dist.y, dist.z, dist.w);
}

//Calculate the color of the pixel 
vec3 Render(vec3 ro, vec3 rd)
{
    //Get the base color of each part
    vec3 col = vec3(0.0);
    vec4 t = RayMarch(ro, rd);
    vec3 p = ro + rd * t.x;
    float d = length(p);
    vec3 n = GetNormal(p);
    vec3 r = reflect(rd, n);  //Ray reflected at p
    float light = LightReflection(p, ro, 0.7);
    if (t.x < MAX_DIST)
    {
        if (d> 0.462)
        {
            col = 0.5 * vec3(light);
            col *= 2.0 * texture(texture0, 5 * vec2(t.y, t.z)).xyz;
            col *= 2.0 * texture(texture0, 5 * vec2(t.z, t.y)).xyz;
        }
        else
        {
             col = 0.4 * vec3(light);
        }
    }
    //Add the reflections of the scene
    for (int i = 0; i < 0; i++)  //Increase loopcount for multiple reflections
    {
        ro = p + n * 3 * SURF_DIST;
        rd = r;
        vec4 t = RayMarch(ro, rd); 
        if(t.x < MAX_DIST) 
        {
            p = ro + rd * t.x;
            n = GetNormal(p);
            r = reflect(rd, n);
            float d = length(vec3(0.0) - p);
        }
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
    vec3 ro = vec3(1.0, 0.5, 15.0);  //Rays origin = camera position
    ro.yz *= rot2D(Mouse.y/200.0);  //Vertical camera rotation
    ro.xz *= rot2D(-Mouse.x/100.0); //+ Time * 0.3); //Horizontal camera rotation
    vec3 rd = GetRayDir(uv, ro, vec3(0.0, 0.0, 0.0), Zoom); //Direction of the ray from the origin to the pixel
    //get the color
    vec3 col = Render(ro, rd);
    FragColor = vec4(col.x, col.y, col.z, 1.0);
}




