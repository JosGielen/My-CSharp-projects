#version 330 core

in vec3 Pos;  //Pixel position

uniform float Time;  //used for animation
uniform float Zoom;  //mouse wheel used to zoom in/out
uniform vec3 Mouse;  //mouse movement

out vec4 FragColor; //pixel color

#define MAX_STEPS 100
#define MAX_DIST 100.
#define SURF_DIST .001
#define TAU 6.283185
#define PI 3.141592

//Scene parts enumeration
const int BALL = 1;
const int PLANE = 2;

//Material enumeration
const vec4 BALL_MAT = vec4(1.0, 1.0, 1.0, 1.0);
const vec4 PLANE_MAT = vec4(0.5, 0.5, 0.5, 0.05);

//Light Position
const vec3 LIGHT_POS = vec3(-5.0, 4.0, 0.0);

//Signed Distance of p to a sphere of radius r and center at location.
float sdSphere(vec3 p, vec3 location, float r)
{
    p = p - location;
    return length(p) - r;
}

//Returns the Distance of p to the scene and the scene part that contains p
vec2 GetDist(vec3 p)
{
    float ball = sdSphere(p, vec3(0.0, 0.0, 0.0), 1.0);  //Sphere with radius 1 at point (0,1,0).
    float plane = p.y + 1.0;   //Ground plane
    float d = min(plane, ball);
    int mat = 0;
    if(d == ball) {mat = BALL;}
    if(d == plane) {mat = PLANE;}
    return vec2(d, mat);  
}

//Returns the color and specular value of each part of the scene
vec4 GetMat(int a)
{
    if (a == BALL) {return BALL_MAT;}
    if (a == PLANE) {return PLANE_MAT;}
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

//2D rotation around the X, Y or Z axes
mat2 rot2D (float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    return mat2(c, -s, s, c);
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
    vec4 mat = GetMat(int(t.y));
    float light = LightReflection(p, ro, mat.w);
    if (t.x < MAX_DIST)
    {
        col = vec3(light);
        mat = GetMat(int(t.y));
        col *= mat.xyz;
    }
    return col;
}

//The Starting point
//------------------
void main()
{
    //Initialization
    vec3 ro = vec3(0.0, 0.25, 3.0);  //Rays origin = camera position
    ro.yz *= rot2D(Mouse.y/200.0);  //Vertical camera rotation
    ro.y = max(-0.9, ro.y);         //Prevent the camera to go below the ground
    ro.xz *= rot2D(-Mouse.x/100.0); //Horizontal camera rotation
    vec3 rd = GetRayDir(Pos.xy, ro, vec3(0.0, 0.0, 0.0), 0.1 * Zoom); //Direction of the ray from the origin to the pixel
    //get the color
    vec3 col = Render(ro, rd);
    FragColor = vec4(col.x, col.y, col.z, 1.0);
}
