#version 420 core
#extension GL_ARB_explicit_uniform_location : enable

in vec3 Pos;  //Pixel position
in vec2 texCoord;

layout(location = 2) uniform float Time;  //used for animation
layout(location = 3) uniform float Zoom;  //mouse wheel used to zoom in/out
layout(location = 4) uniform vec3 Mouse;  //mouse movement
layout(location = 5) uniform vec3 Resolution; //Size of the GLWpfControl

out vec4 FragColor; //pixel color

#define MAX_STEPS 100
#define MAX_DIST 100.
#define SURF_DIST .001
#define TAU 6.283185
#define PI 3.141592

//Scene parts enumeration
const int BALL1 = 1;
const int BALL2 = 2;
const int BALL3 = 3;
const int BALL4 = 4;
const int PLANE = 5;

//Material enumeration
const vec4 BALL1_MAT = vec4(1.0, 1.0, 1.0, 0.8);  //silver mirror
const vec4 BALL2_MAT = vec4(1.0, 0.0, 0.0, 0.0); //red matt
const vec4 BALL3_MAT = vec4(1.0, 1.0, 0.0, 0.0); //yellow matt
const vec4 BALL4_MAT = vec4(0.0, 1.0, 0.0, 0.0); //green matt
const vec4 PLANE_MAT = vec4(0.7, 0.7, 0.7, 0.05); //Grey checkered
const vec3 SKY_MAT = vec3(0.6, 0.6, 0.9);  //Light blue

//Light Position
const vec3 LIGHT_POS = vec3(-15.0, 27.0, 0.0);

//Signed Distance of p to a sphere of radius r and center at location.
float sdSphere(vec3 p, vec3 location, float r)
{
    p = p - location;
    return length(p) - r;
}

//Calculate a checkerboard pattern
float CheckerBoard( vec3 p)
{
    vec2 id = floor(p.xz);
    return mod(id.x + id.y, 2.0);
}

//Returns the Distance of p to the scene and the scene part that contains p
vec2 GetDist(vec3 p)
{
    float ball1 = sdSphere(p, vec3(0.0, 3.0, 0.0), 4.0);  //Sphere with radius 4 at point (0,0).
    float ball2 = sdSphere(p, vec3(-4.0, 0.0, 7.0), 1.0);  //Sphere with radius 1 at point (2,7).
    float ball3 = sdSphere(p, vec3(4.0, -0.3, 8.0), 0.7);  //Sphere with radius 1 at point (4,9).
    float ball4 = sdSphere(p, vec3(0.0, 0.0, 6.0), 1.0);  //Sphere with radius 1 at point (0,6).
    float plane = p.y + 1.0;   //Ground plane
    float d = min(plane, min(ball1, min(ball2, min(ball3, ball4))));
    int mat = 0;
    if(d == ball1) {mat = BALL1;}
    if(d == ball2) {mat = BALL2;}
    if(d == ball3) {mat = BALL3;}
    if(d == ball4) {mat = BALL4;}
    if(d == plane) {mat = PLANE;}
    return vec2(d, mat);  
}

//Returns the color and specular value of each part of the scene
vec4 GetMat(int a)
{
    if (a == BALL1) {return BALL1_MAT;}
    if (a == BALL2) {return BALL2_MAT;}
    if (a == BALL3) {return BALL3_MAT;}
    if (a == BALL4) {return BALL4_MAT;}
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

//Calculate the color of the pixel 
vec3 Render(vec3 ro, vec3 rd)
{
    //Get the base color of each part
    vec3 col = SKY_MAT;
    vec4 mat = vec4(0.0);
    vec2 t = RayMarch(ro, rd);
    vec3 p = ro + rd * t.x;
    vec3 n = GetNormal(p);
    float cd = length(p);
    vec3 r = reflect(rd, n);  //Ray reflected at p
    mat = GetMat(int(t.y));
    float light = LightReflection(p, ro, mat.w);
    if (t.x < MAX_DIST)
    {
        col = vec3(light);
        mat = GetMat(int(t.y));
        col *= mat.xyz;
        //No reflections on the ground plane
        if (t.y == PLANE)
        {
            col += CheckerBoard(p);
            col /= max(pow(t.x, 0.5), 0.6);
            // Shadows
            vec3 ld = normalize(LIGHT_POS - p); // Light Vector
            float d = RayMarch(p + 10.0 * n * SURF_DIST, ld).x;
            if (d < length(LIGHT_POS - p))
            col *= 0.4;
            return col;
        }
        else if (t.y == BALL2 || t.y == BALL3 || t.y == BALL4)
        {
            return col;
        }
        //Add the reflections for the rest of the scene
        for (int i = 0; i < 1; i++)  //Increase loopcount for multiple reflections
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
                if (t.y == PLANE)
                {
                    col += mat.w * CheckerBoard(p);
                    col /= (2 * max(pow(t.x, 0.5), 1.0));
                    // Shadows
                    vec3 ld = normalize(LIGHT_POS - p); // Light Vector
                    float d = RayMarch(p + 10.0 * n * SURF_DIST, ld).x;
                    if (d < length(LIGHT_POS - p))
                    col *= 0.4;
                }
            }
            else
            {
            col *= 1.5 * SKY_MAT;
            }
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
    uv.x = Pos.x * Resolution.x / Resolution.y;
    //Initialization
    vec3 ro = vec3(0.0, 5.0, 15.0);  //Rays origin = camera position
    ro.yz *= rot2D(Mouse.y/200.0);  //Vertical camera rotation
    ro.y = max(-0.9, ro.y);         //Prevent the camera to go below the ground
    ro.xz *= rot2D(-Mouse.x/100.0); //Horizontal camera rotation
    vec3 rd = GetRayDir(uv, ro, vec3(0.0, 0.0, 0.0), 0.1 * Zoom); //Direction of the ray from the origin to the pixel
    //get the color
    vec3 col = Render(ro, rd);
    FragColor = vec4(col.x, col.y, col.z, 1.0);
}
