#version 330 core

in vec3 Pos;  //Pixel position

uniform float Time;  //used for animation
uniform float Zoom;  //mouse wheel used to zoom in/out
uniform vec3 Mouse;  //mouse movement

out vec4 FragColor; //pixel color

#define MAX_STEPS 100
#define MAX_DIST 100.
#define SURF_DIST .001

//Light Position
const vec3 LIGHT_POS = vec3(-3.0, 4.0, 3.0);

//Returns the Distance of p to the scene and the scene part that contains p
vec2 GetDist(vec3 p)
{   
    float d = 0;
	int mat = 1;
    return vec2(d, mat);  
}

//Returns the color and specular value of each part of the scene
vec4 GetMatColor(int a)
{
    if (a == 0) {return vec4(0.0, 0.0, 0.0, 0.0);}
    if (a == 1) {return vec4(1.0, 1.0, 1.0, 1.0);}
}

//Calculate the direction of a ray from the origin to the pixel coordinate uv 
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
        dist = GetDist(p);       //distance of the current position to the scene and name of the part hit
        totalDist += dist.x;             //total distance the ray has "marched"
        if(totalDist > MAX_DIST || abs(dist.x) < SURF_DIST) { break;}
    }
    return vec2(totalDist, dist.y);
}

//Calculate the normal at the point where the ray hits the scene
vec3 GetNormal(vec3 p) 
{
    vec2 e = vec2(.001, 0);
    vec3 n = GetDist(p).x - vec3(GetDist(p-e.xyy).x, GetDist(p-e.yxy).x, GetDist(p-e.yyx).x);
    return normalize(n);
}

//Light reflection at point p towards the camera (or) with reflectivity cr
float LightReflection(in vec3 p, in vec3 or, in float cr) 
{
    //Ambient light
    float ambient = 0.0;
    //Diffuse light reflection
    vec3 ld = normalize(LIGHT_POS - p); // Light Vector
    vec3 n = GetNormal(p);	// normal vector
    float diffuse = dot(n, normalize(ld));
    //Specular light reflection
    vec3 r = reflect(-ld, n); // reflected light vector
    float s = clamp(dot(r, normalize(or - p)), 0., 1.); // dot product between reflected light and camera vector
    float specular = pow(s, 15.0) * cr;
    //Shadows 
    float d = RayMarch(p + n * SURF_DIST * 2.0, ld).x;
    float shadow =  d < length(LIGHT_POS - p) ? 0.1 : 1.0;
    return (1.0 - ambient) * (shadow * diffuse + specular) + ambient;	
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
    //Get the base color  and light reflection of each part
    vec3 col = vec3(0.0);
    vec4 mat = vec4(0.0);
    vec2 t = RayMarch(ro, rd);
    vec3 p = ro + rd * t.x;
    if (t.x < MAX_DIST)
    {
        mat = GetMatColor(int(t.y));
		float light = LightReflection(p, ro, mat.w);
        col = vec3(light);
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
