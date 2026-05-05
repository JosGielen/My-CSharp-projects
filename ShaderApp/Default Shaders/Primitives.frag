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
const vec3 LIGHT_POS = vec3(5.0, 7.0, 5.0);


//2D rotation around the X, Y or Z axes
mat2 rot2D (float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    return mat2(c, -s, s, c);
}

//Signed Distance of p to a Cube of size b at the origin.
float sdBox(vec3 p, vec3 b)
{
    vec3 d = abs(p) - b;
    return min(max(d.x,max(d.y,d.z)),0.0) + length(max(d,0.0));
}

float sdSphere( vec3 p, float s )
{
    return length(p)-s;
}

float sdBoxFrame( vec3 p, vec3 b, float e )
{
       p = abs(p  )-b;
  vec3 q = abs(p+e)-e;

  return min(min(
      length(max(vec3(p.x,q.y,q.z),0.0))+min(max(p.x,max(q.y,q.z)),0.0),
      length(max(vec3(q.x,p.y,q.z),0.0))+min(max(q.x,max(p.y,q.z)),0.0)),
      length(max(vec3(q.x,q.y,p.z),0.0))+min(max(q.x,max(q.y,p.z)),0.0));
}

float sdEllipsoid( in vec3 p, in vec3 r ) // approximated
{
    float k0 = length(p/r);
    float k1 = length(p/(r*r));
    return k0*(k0-1.0)/k1;
}

float sdTorus( vec3 p, float r1, float r2 )
{
    return length( vec2(length(p.xz)-r1,p.y) )-r2;
}

float sdCappedTorus(in vec3 p, in vec2 sc, in float ra, in float rb)
{
    p.x = abs(p.x);
    float k = (sc.y*p.x>sc.x*p.y) ? dot(p.xy,sc) : length(p.xy);
    return sqrt( dot(p,p) + ra*ra - 2.0*ra*k ) - rb;
}

float sdHexPrism( vec3 p, in float r, float h )
{
    vec3 q = abs(p);
    const vec3 k = vec3(-0.8660254, 0.5, 0.57735);
    p = abs(p);
    p.xy -= 2.0*min(dot(k.xy, p.xy), 0.0)*k.xy;
    vec2 d = vec2(length(p.xy - vec2(clamp(p.x, -k.z*r, k.z*r), r))*sign(p.y - r), p.z-h );
    return min(max(d.x,d.y),0.0) + length(max(d,0.0));
}

float sdOctogonPrism( in vec3 p, in float r, float h )
{
  const vec3 k = vec3(-0.9238795325,   // sqrt(2+sqrt(2))/2 
                       0.3826834323,   // sqrt(2-sqrt(2))/2
                       0.4142135623 ); // sqrt(2)-1 
  // reflections
  p = abs(p);
  p.xy -= 2.0*min(dot(vec2( k.x,k.y),p.xy),0.0)*vec2( k.x,k.y);
  p.xy -= 2.0*min(dot(vec2(-k.x,k.y),p.xy),0.0)*vec2(-k.x,k.y);
  // polygon side
  p.xy -= vec2(clamp(p.x, -k.z*r, k.z*r), r);
  vec2 d = vec2( length(p.xy)*sign(p.y), p.z-h );
  return min(max(d.x,d.y),0.0) + length(max(d,0.0));
}

float sdCapsule( vec3 p, vec3 a, vec3 b, float r )
{
 vec3 pa = p-a, ba = b-a;
 float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
 return length( pa - ba*h ) - r;
}

float sdRoundCone( in vec3 p, in float r1, float r2, float h )
{
    vec2 q = vec2( length(p.xz), p.y );
    float b = (r1-r2)/h;
    float a = sqrt(1.0-b*b);
    float k = dot(q,vec2(-b,a));
    if( k < 0.0 ) return length(q) - r1;
    if( k > a*h ) return length(q-vec2(0.0,h)) - r2;
    return dot(q, vec2(a,b) ) - r1;
}

float sdRoundCone(vec3 p, vec3 a, vec3 b, float r1, float r2)
{
    // sampling independent computations (only depend on shape)
    vec3  ba = b - a;
    float l2 = dot(ba,ba);
    float rr = r1 - r2;
    float a2 = l2 - rr*rr;
    float il2 = 1.0/l2;
    // sampling dependant computations
    vec3 pa = p - a;
    float y = dot(pa,ba);
    float z = y - l2;
    float x2 = dot(pa*l2 - ba*y, pa*l2 - ba*y);
    float y2 = y*y*l2;
    float z2 = z*z*l2;
    // single square root!
    float k = sign(rr)*rr*rr*x2;
    if( sign(z)*a2*z2 > k )
    {
        return  sqrt(x2 + z2) * il2 - r2;
    }
    if( sign(y)*a2*y2 < k )
    {
        return  sqrt(x2 + y2) * il2 - r1;
    }
    return (sqrt(x2*a2*il2)+y*rr)*il2 - r1;
}

float sdTriPrism( vec3 p, float r, float h )
{
    const float k = sqrt(3.0);
    r *= 0.5*k;
    p.xy /= r;
    p.x = abs(p.x) - 1.0;
    p.y = p.y + 1.0/k;
    if( p.x+k*p.y>0.0 ) p.xy=vec2(p.x-k*p.y,-k*p.x-p.y)/2.0;
    p.x -= clamp( p.x, -2.0, 0.0 );
    float d1 = length(p.xy)*sign(-p.y)*r;
    float d2 = abs(p.z)-h;
    return length(max(vec2(d1,d2),0.0)) + min(max(d1,d2), 0.);
}

float sdCylinder( vec3 p, float r, float h )
{
    vec2 d = abs(vec2(length(p.xz),p.y)) - vec2(r, h);
    return min(max(d.x,d.y),0.0) + length(max(d,0.0));
}

float sdCylinder(vec3 p, vec3 a, vec3 b, float r)
{
    vec3 pa = p - a;
    vec3 ba = b - a;
    float baba = dot(ba,ba);
    float paba = dot(pa,ba);
    float x = length(pa*baba-ba*paba) - r*baba;
    float y = abs(paba-baba*0.5)-baba*0.5;
    float x2 = x*x;
    float y2 = y*y*baba;
    float d = (max(x,y)<0.0)?-min(x2,y2):(((x>0.0)?x2:0.0)+((y>0.0)?y2:0.0));
    return sign(d)*sqrt(abs(d))/baba;
}

float sdCone( in vec3 p, in float h, in float r )
{
    vec2 q = vec2( length(p.xz), p.y );
    vec2 k1 = vec2(0.0, h);
    vec2 k2 = vec2(-r,2.0*h);
    vec2 ca = vec2(q.x-min(q.x,(q.y < 0.0) ? r : 0.0), abs(q.y)-h);
    vec2 cb = q - k1 + k2*clamp( dot(k1-q,k2)/dot(k2, k2), 0.0, 1.0 );
    float s = (cb.x < 0.0 && ca.y < 0.0) ? -1.0 : 1.0;
    return s*sqrt( min(dot(ca, ca), dot(cb, cb)) );
}

float sdCappedCone( in vec3 p, in float h, in float r1, in float r2 )
{
    vec2 q = vec2( length(p.xz), p.y );
    vec2 k1 = vec2(r2,h);
    vec2 k2 = vec2(r2-r1,2.0*h);
    vec2 ca = vec2(q.x-min(q.x,(q.y < 0.0)?r1:r2), abs(q.y)-h);
    vec2 cb = q - k1 + k2*clamp( dot(k1-q,k2)/dot(k2, k2), 0.0, 1.0 );
    float s = (cb.x < 0.0 && ca.y < 0.0) ? -1.0 : 1.0;
    return s*sqrt( min(dot(ca, ca), dot(cb, cb)) );
}

float sdCappedCone(vec3 p, vec3 a, vec3 b, float ra, float rb)
{
    float rba  = rb-ra;
    float baba = dot(b-a,b-a);
    float papa = dot(p-a,p-a);
    float paba = dot(p-a,b-a)/baba;
    float x = sqrt( papa - paba*paba*baba );
    float cax = max(0.0,x-((paba<0.5)?ra:rb));
    float cay = abs(paba-0.5)-0.5;
    float k = rba*rba + baba;
    float f = clamp( (rba*(x-ra)+paba*baba)/k, 0.0, 1.0 );
    float cbx = x-ra - f*rba;
    float cby = paba - f;
    float s = (cbx < 0.0 && cay < 0.0) ? -1.0 : 1.0;
    return s*sqrt(min(cax*cax + cay*cay*baba, cbx*cbx + cby*cby*baba));
}

float sdOctahedron(vec3 p, float s)
{
    p = abs(p);
    float m = p.x + p.y + p.z - s;
    vec3 q;
    if( 3.0 * p.x < m )
    {
        q = p.xyz;
    }
    else if( 3.0 * p.y < m )
    {
        q = p.yzx;
    }
    else if( 3.0 * p.z < m ) 
    {
        q = p.zxy;
    }
    else
    {
        return m * 0.57735027;
    }
    float k = clamp(0.5 * (q.z - q.y + s), 0.0, s); 
    return length(vec3(q.x, q.y - s + k, q.z - k)); 
}

float sdPyramid( in vec3 p, in float r, float h )
{
    float m2 = h*h + 0.25;
    // symmetry
    p.xz = abs(p.xz);
    p.xz = (p.z>p.x) ? p.zx : p.xz;
    p.xz -= r;
    // project into face plane (2D)
    vec3 q = vec3( p.z, h*p.y - 0.5*p.x, h*p.x + 0.5*p.y);
    float s = max(-q.x,0.0);
    float t = clamp( (q.y-0.5*p.z)/(m2+0.25), 0.0, 1.0 );
    float a = m2*(q.x+s)*(q.x+s) + q.y*q.y;
    float b = m2*(q.x+0.5*t)*(q.x+0.5*t) + (q.y-m2*t)*(q.y-m2*t);
    float d2 = min(q.y,-q.x*m2-q.y*0.5) > 0.0 ? 0.0 : min(a,b);
    // recover 3D and scale, and add sign
    return sqrt( (d2+q.z*q.z)/m2 ) * sign(max(q.z,-p.y));;
}

// la,lb=semi axis, h=height, ra=corner
float sdRhombus(vec3 p, float la, float lb, float h, float ra)
{
    p = abs(p);
    
    
    vec2 b1 = vec2(la, lb);
    vec2 b2 = b1 - 2.0 * p.xz;
    float nd = b1.x * b2.x - b1.y * b2.y;

    float f = clamp(nd / dot(b1,b1), -1.0, 1.0 );
    vec2 q = vec2(length(p.xz-0.5*b1*vec2(1.0-f,1.0+f))*sign(p.x*b1.y+p.z*b1.x-b1.x*b1.y)-ra, p.y-h);
    return min(max(q.x,q.y),0.0) + length(max(q,0.0));
}

float sdHorseshoe( in vec3 p, in vec2 c, in float r, in float le, vec2 w )
{
    p.x = abs(p.x);
    float l = length(p.xy);
    p.xy = mat2(-c.x, c.y, c.y, c.x)*p.xy;
    p.xy = vec2((p.y>0.0 || p.x>0.0)?p.x:l*sign(-c.x), (p.x>0.0)?p.y:l );
    p.xy = vec2(p.x,abs(p.y-r))-vec2(le,0.0);
    vec2 q = vec2(length(max(p.xy,0.0)) + min(0.0,max(p.x,p.y)),p.z);
    vec2 d = abs(q) - w;
    return min(max(d.x,d.y),0.0) + length(max(d,0.0));
}

float sdU( in vec3 p, in float r, in float le, vec2 w )
{
    p.x = (p.y>0.0) ? abs(p.x) : length(p.xy);
    p.x = abs(p.x-r);
    p.y = p.y - le;
    float k = max(p.x,p.y);
    vec2 q = vec2( (k<0.0) ? -k : length(max(p.xy,0.0)), abs(p.z) ) - w;
    return length(max(q,0.0)) + min(max(q.x,q.y),0.0);
}

//-----------------------------------------------------------------------------

vec2 opU( vec2 d1, vec2 d2 )
{
 return (d1.x<d2.x) ? d1 : d2;
}

//Calculate a checkerboard pattern
float CheckerBoard( vec3 p)
{
    vec2 id = floor(p.xz * 5);
    return 0.6 + 0.4 * mod(id.x + id.y, 2.0);
}

//Returns the Distance of p to the scene and the scene part that contains p
vec2 GetDist(vec3 p)
{
    //float d = sdBox3D(p, vec3(0.5, 0.5, 0.5));  //Cube of size 0.5
    //float d = sdBoxFrame(p, vec3(0.5, 0.5, 0.5), 0.1);
    //float d = sdEllipsoid(p, vec3(0.5, 0.8, 0.3));
    //float d = sdTorus(p, 0.5, 0.2);
    //float d = sdCappedTorus(p, vec2(0.9, 0.0), 0.7, 0.2);
    //float d = sdHexPrism(p, 0.5, 0.1);
    //float d = sdOctogonPrism(p, 0.5, 0.1);
    //float d = sdCapsule(p, vec3(0.0, 0.0, 0.0), vec3(0.0, 1.0, 0.0), 0.3);
    //float d = sdRoundCone(p, 0.5, 0.2, 1.1);
    //float d = sdRoundCone(p, vec3(0.0, 0.0, 0.0), vec3(0.0, 1.1, 0.0), 0.5, 0.2);
    //float d = sdTriPrism(p, 0.7, 0.2);
    //float d = sdCylinder(p, 0.5, 0.7);
    //float d = sdCylinder(p, vec3(0.0, 0.0, 0.0), vec3(0.0, 1.0, 0.0), 0.3);
    //float d = sdCone(p, 1.0, 0.5);
    //float d = sdCappedCone(p, 0.7, 0.5, 0.2);
    //float d = sdCappedCone(p, vec3(0.0, 0.0, 0.0), vec3(0.0, 1.0, 0.0), 0.5, 0.2);
    //float d = sdOctahedron(p, 0.5);
    //float d = sdPyramid(p, 0.4, 0.8);
    //float d = sdRhombus(p, 0.25, 0.5, 0.06, 0.04);
    //float d = sdHorseshoe(p, vec2(cos(1.6),sin(1.6)), 0.4, 0.3, vec2(0.05,0.1));
    //float d = sdU(p, 0.5, 0.5, vec2(0.1, 0.1));
    //int item = BOX;
    //return vec2(d, item);  

    vec3 pos = p;
    vec2 res = vec2( pos.y, 0.0 );
    res = opU( res, vec2( sdSphere( pos-vec3(0.0,0.25, 2.5), 0.2 ), 26.9 ) );

    // bounding box
    if( sdBox( pos-vec3(-0.5,0.3,-1.0),vec3(0.35,0.3,3.5) )<res.x )
    {
        res = opU( res, vec2( sdCappedTorus((pos-vec3( -0.5,0.30, 2.0))*vec3(1,-1,1), vec2(0.866025,-0.5), 0.25, 0.05), 25.0) );
        res = opU( res, vec2( sdBoxFrame(    pos-vec3( -0.5,0.25, 1.0), vec3(0.3,0.25,0.2), 0.025 ), 16.9 ) );
        res = opU( res, vec2( sdCone(        pos-vec3( -0.5,0.25, 0.0), 0.3, 0.4), 55.0 ) );
        res = opU( res, vec2( sdCappedCone(  pos-vec3( -0.5,0.25,-1.0), 0.25, 0.25, 0.1 ), 13.67 ) );
        res = opU( res, vec2( sdRhombus(  (  pos-vec3( -0.5,0.30,-2.0)).xzy, 0.15, 0.25, 0.04, 0.08 ),17.0 ) );
    }

    // bounding box
    if( sdBox( pos-vec3(0.5,0.3,-1.0),vec3(0.35,0.3,3.5) )<res.x )
    {
        res = opU( res, vec2( sdTorus(      (pos-vec3( 0.5,0.30, 2.0)).xzy, 0.25, 0.05 ), 7.1 ) );
        res = opU( res, vec2( sdBox(         pos-vec3( 0.50,0.25, 1.0), vec3(0.3,0.25,0.1) ), 3.0 ) );
        res = opU( res, vec2( sdCapsule(     pos-vec3( 0.5,0.00, 0.0),vec3(-0.1,0.1,-0.1), vec3(0.2,0.4,0.2), 0.1  ), 31.9 ) );
        res = opU( res, vec2( sdCylinder(    pos-vec3( 0.5,0.25,-1.0), 0.15, 0.25 ), 8.0 ) );
        res = opU( res, vec2( sdHexPrism(    pos-vec3( 0.5,0.20,-2.0), 0.2, 0.05 ), 18.4 ) );
    }

    // bounding box
    if( sdBox( pos-vec3(-1.5,0.35,-1.0),vec3(0.35,0.35,3.5))<res.x )
    {
        res = opU( res, vec2( sdPyramid(    pos-vec3(-1.5,-0.6,-2.0), 0.5, 1.0 ), 13.56 ) );
        res = opU( res, vec2( sdOctahedron( pos-vec3(-1.5,0.30,-1.0), 0.35 ), 23.56 ) );
        res = opU( res, vec2( sdTriPrism(   pos-vec3(-1.5,0.15, 0.0), 0.3, 0.05 ),43.5 ) );
        res = opU( res, vec2( sdEllipsoid(  pos-vec3(-1.5,0.25, 1.0), vec3(0.2, 0.25, 0.05) ), 43.17 ) );
        res = opU( res, vec2( sdHorseshoe(  pos-vec3(-1.5,0.25, 2.0), vec2(cos(1.3),sin(1.3)), 0.2, 0.3, vec2(0.03,0.08) ), 11.5 ) );
    }

    // bounding box
    if( sdBox( pos-vec3(1.5,0.3,-1.0),vec3(0.35,0.3,3.5) )<res.x )
    {
        res = opU( res, vec2( sdOctogonPrism(pos-vec3( 1.5,0.2 ,-2.0), 0.2, 0.05), 51.8 ) );
        res = opU( res, vec2( sdCylinder(    pos-vec3( 1.5,0.14,-1.0), vec3(0.1,-0.1,0.0), vec3(-0.2,0.35,0.1), 0.08), 31.2 ) );
        res = opU( res, vec2( sdCappedCone(  pos-vec3( 1.5,0.09, 0.0), vec3(0.1,0.0,0.0), vec3(-0.2,0.40,0.1), 0.15, 0.05), 46.1 ) );
        res = opU( res, vec2( sdRoundCone(   pos-vec3( 1.5,0.15, 1.0), vec3(0.1,0.0,0.0), vec3(-0.1,0.35,0.1), 0.15, 0.05), 51.7 ) );
        res = opU( res, vec2( sdRoundCone(   pos-vec3( 1.5,0.20, 2.0), 0.2, 0.1, 0.3 ), 37.0 ) );
    }
    
    return res;
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
    float m = t.y;
    vec3 p = ro + rd * t.x;
    vec3 n = GetNormal(p);
    float light = LightReflection(p, ro, 1.0);
    if (t.x < MAX_DIST)
    {
        
        if (t.y == 0.0)
        {
            col += 1.5 * CheckerBoard(p) * (1 - 0.01 * t.x);
            col /= max(pow(t.x, 0.6), 1.5);
            // Shadows
            vec3 ld = normalize(LIGHT_POS - p); // Light Vector
            float d = RayMarch(p + 10.0 * n * SURF_DIST, ld).x;
            if (d < length(LIGHT_POS - p))
            col *= 0.4;
            return col;
        }
        else
        {
            col = 1.5 * vec3(light);
            col *= 0.4 + 0.3 * sin( m * 2.0 + vec3(0.0,1.0,2.0) );
        }
    }
    else
    {
        col = vec3(0.9-0.5*rd.y, 0.9-0.5*rd.y, 1.0); 
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
    vec3 ro = vec3(0.0, 2.0, 3.5);  //Rays origin = camera position
    ro.yz *= rot2D(Mouse.y/200.0);  //Vertical camera rotation
    ro.y = max(-0.9, ro.y);         //Prevent the camera to go below the ground
    ro.xz *= rot2D(-Mouse.x/100.0 + Time * 0.2); //Horizontal camera rotation
    vec3 rd = GetRayDir(uv, ro, vec3(0.0, 0.0, 0.0), Zoom); //Direction of the ray from the origin to the pixel
    //get the color
    vec3 col = Render(ro, rd);
    FragColor = vec4(col.x, col.y, col.z, 1.0);
}





