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

//Light Position
const vec3 LIGHT_POS = vec3(1.0, 2.0, 3.0);

const mat2 m = mat2( 0.80,  0.60, -0.60,  0.80 );

float hash( float n )
{
    return fract(sin(n)*43758.5453);
}

float noise( in vec2 x )
{
    vec2 i = floor(x);
    vec2 f = fract(x);

    f = f*f*(3.0-2.0*f);

    float n = i.x + i.y*57.0;

    return mix(mix( hash(n+ 0.0), hash(n+ 1.0),f.x),
               mix( hash(n+57.0), hash(n+58.0),f.x),f.y);
}

float fbm( vec2 p )
{
    float f = 0.0;
    f += 0.50000*noise( p ); p = m*p*2.02;
    f += 0.25000*noise( p ); p = m*p*2.03;
    f += 0.12500*noise( p ); p = m*p*2.01;
    f += 0.06250*noise( p ); p = m*p*2.04;
    f += 0.03125*noise( p );
    return f/0.984375;
}

float length2( vec2 p )
{
    vec2 q = p*p*p*p;
    return pow( q.x + q.y, 1.0/4.0 );
}

vec3 GetColor( vec2 p )
{
    p *= 1.7;
    // polar coordinates
    float r = length( p );
    float a = atan( p.y, p.x );
    // animate
    //r *= 1.0 + 0.2*clamp(1.0-r,0.0,1.0)*sin(4.0*Time);
    // iris (blue-green)
    vec3 color = vec3( 0.0, 0.3, 0.4 );
    float f = fbm( 5.0*p );
    color = mix( color, vec3(0.2,0.5,0.4), f );
    // yellow towards center
    color = mix( color, vec3(0.9,0.6,0.2), 1.0-smoothstep(0.2,0.6,r) );
    // darkening
    f = smoothstep( 0.4, 0.9, fbm( vec2(15.0*a,10.0*r) ) );
    color *= 1.0-0.5*f;
    // distort
    a += 0.05*fbm( 20.0*p );
    // cornea
    f = smoothstep( 0.3, 1.0, fbm( vec2(20.0*a,6.0*r) ) );
    color = mix( color, vec3(1.0,1.0,1.0), f );
    // edges
    color *= 1.0-0.25*smoothstep( 0.6,0.8,r );
    // shadow
    color *= vec3(0.8+0.2*cos(r*a));
    // crop
    f = smoothstep( 0.79, 0.82, r );
    color = mix( color, vec3(1.0), f );
     // pupil
    f = 1.0-smoothstep( 0.23, 0.21, r );
    color *= f;// = mix( color, vec3(0.0), f );

	return color;
}

//2D rotation around the X, Y or Z axes
mat2 rot2D (float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    return mat2(c, -s, s, c);
}

//Signed Distance of p to a sphere of radius r and center at location.
float sdSphere(vec3 p, vec3 location, float r)
{
    p = p - location;
    return length(p) - r;
}

//Returns the Distance of p to the scene and the scene part that contains p
float GetDist(vec3 p)
{
    float d = sdSphere(p, vec3(0.0, 0.0, 0.0), 1.0);  //Sphere with radius 1 at point (0,1,0).
    return d;  
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
    vec3 n = GetDist(p) - vec3(GetDist(p-e.xyy), GetDist(p-e.yxy), GetDist(p-e.yyx));
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
float RayMarch(vec3 ro, vec3 rd) 
{
    float dist = 0.0;
	float totalDist = 0.;  //Total distance travelled by the Ray.
    for(int i = 0; i < MAX_STEPS; i++) 
    {
    	vec3 p = ro + rd * totalDist;  //current position of the ray
        dist = GetDist(p);       //distance of the current position to the scene and name of the part hit
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
    float light = LightReflection(p, ro, 0.6);
    if (t < MAX_DIST)
    {
        col = vec3(light);
        col *= GetColor(p.xy);
    }
    return col;
}

//The Starting point
//------------------
void main()
{
    //Correct for the viewport size.
    vec2 uv = Pos.xy;
    uv.x = uv.x * Resolution.x / Resolution.y;
    uv.x = max(uv.x, -1.0);
    uv.x = min(uv.x, 1.0);
    //Initialization
    vec3 ro = vec3(0.0, 0.25, 3.0);  //Rays origin = camera position
    ro.yz *= rot2D(Mouse.y/200.0 - 0.3 * noise(vec2(0.73*Time, 1.06*Time)) + 0.2);  //Vertical camera rotation
    ro.y = max(-0.9, ro.y);         //Prevent the camera to go below the ground
    ro.xz *= rot2D(-Mouse.x/100.0 - 0.3 * noise(vec2(0.53*Time, 1.36*Time)) + 0.2); //Horizontal camera rotation
    vec2 q = uv;
    q.x = fract(q.x) - 0.5;
    vec3 rd = GetRayDir(q, ro, vec3(0.0, 0.0, 0.0), 0.1 * Zoom); //Direction of the ray from the origin to the pixel
    //get the color
    vec3 col = Render(ro, rd);
    FragColor = vec4(col.x, col.y, col.z, 1.0);
}





