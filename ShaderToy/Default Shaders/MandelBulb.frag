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

//Light Position
const vec3 LIGHT_POS = vec3(1.0, 2.0, 3.0);

//2D rotation around the X, Y or Z axes
mat2 rot2D (float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    return mat2(c, -s, s, c);
}

float sdPlane(vec3 p, float a) 
{
  return p.y + a;
}

//Signed Distance of p to a MandelBulb.
float sdMandel(vec3 p) 
{
	p.xyz = p.xzy;
    //p.xy *= rot2D(0.2*Time);
	vec3 z = p;
	vec3 dz=vec3(0.0);
	float power = 8.0;
	float r, theta, phi;
	float dr = 1.0;
	float t0 = 1.0;
	for(int i = 0; i < 12; ++i) 
    {
		r = length(z);
		if(r > 2.0) { break; }
		theta = atan(z.y / z.x);
        phi = asin(z.z / r) + Time * 0.05;
		dr = pow(r, power - 1.0) * dr * power + 1.0;
		r = pow(r, power);
		theta = theta * power;
		phi = phi * power;
		z = r * vec3(cos(theta) * cos(phi), sin(theta) * cos(phi), sin(phi)) + p;
		t0 = min(t0, r);
	}
	return 0.3 * log(r) * r / dr;
}

//Calculate a smooth color palette
vec3 palette(float t)
{
    vec3 a = vec3(0.5, 0.5, 0.5);
    vec3 b = vec3(0.5, 0.5, 0.5);
    vec3 c = vec3(1.0, 1.0, 1.0);
    vec3 d = vec3(0.0, 0.33, 0.67);
    return a + b * cos(TAU * (c * t + d));
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
    vec3 n = sdMandel(p) - vec3(sdMandel(p-e.xyy), sdMandel(p-e.yxy), sdMandel(p-e.yyx));
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
float RayMarch(vec3 ro, vec3 rd) 
{
    float dist = 0;
	float totalDist = 0.;  //Total distance travelled by the Ray.
    for(int i = 0; i < MAX_STEPS; i++) 
    {
    	vec3 p = ro + rd * totalDist;  //current position of the ray
        dist = sdMandel(p);       //distance of the current position to the scene and name of the part hit
        totalDist += dist;     //total distance the ray has "marched"
        if(totalDist > MAX_DIST || abs(dist) < SURF_DIST) { break;}
    }
    return totalDist;
}

//Calculate the color of the pixel 
vec3 Render(vec3 ro, vec3 rd)
{
    //Get the base color of each part
    vec3 col = vec3(0.0);
    float t = RayMarch(ro, rd);
    vec3 p = ro + rd * t;
    vec3 n = GetNormal(p);
    vec3 r = reflect(rd, n);  //Ray reflected at p
    float light = LightReflection(p, ro, 1.0);
    if (t < MAX_DIST)
    {
        col = vec3(light);
        float d = length(vec3(0.0) - p);
        col *= palette(1 - 2.0 * d);
    }
    //Add the reflections of the scene
    for (int i = 0; i < 1; i++)  //Increase loopcount for multiple reflections
    {
        ro = p + n * 3 * SURF_DIST;
        rd = r;
        float t = RayMarch(ro, rd); 
        if(t < MAX_DIST) 
        {
            p = ro + rd * t;
            n = GetNormal(p);
            r = reflect(rd, n);
            float d = length(vec3(0.0) - p);
            col *= palette(1 - 2.0 * d);
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
    vec3 ro = vec3(0.0, 0.5, 15.0);  //Rays origin = camera position
    ro.yz *= rot2D(Mouse.y/200.0);  //Vertical camera rotation
    ro.y = max(0.2, ro.y);         //Prevent the camera to go below the ground
    ro.xz *= rot2D(-Mouse.x/100.0); //Horizontal camera rotation
    vec3 rd = GetRayDir(uv, ro, vec3(0.0, 0.0, 0.0), Zoom); //Direction of the ray from the origin to the pixel
    //get the color
    vec3 col = Render(ro, rd);
    FragColor = vec4(col.x, col.y, col.z, 1.0);
}

