precision highp float;
uniform vec3 u_resolution; // Width & height of the shader
uniform float time; // Time elapsed
//uniform sampler2D u_texture;
uniform vec3 mouse; // camera rotation

// Constants
#define PI 3.1415925359
#define TWO_PI 6.2831852
#define MAX_STEPS 200 // Mar Raymarching steps
#define MAX_DIST 100. // Max Raymarching distance
#define SURF_DIST .01 // Surface distance
#define SAMPLE_DIST 0.05 // Sample distance
// Sphere locations
vec4 sphere1Pos = vec4(2.0, 0.6, 0.0, 1.0);
vec4 sphere2Pos = vec4(-2.0, 0.6, 0.0, 1.0);
vec4 sphere3Pos = vec4(0.0, 0.6, 2.0, 1.0);


float random(in vec2 x) 
{
  return fract(sin(dot(x, vec2(12.9898,54.233))) * 43758.5453123);
}

float noise(in vec2 x) 
{
  vec2 i = floor(x);
  vec2 f = fract(x);

  float tl = random(i); // top-left corner
  float tr = random(i + vec2(1.0, 0.0)); // top-right corner
  float bl = random(i + vec2(0.0, 1.0)); // bottom-left corner
  float br = random(i + vec2(1.0, 1.0)); // bottom-right corner
  vec2 u = smoothstep(0., 1., f);
  return mix(mix(tl, tr, u.x), mix(bl, br, u.x), u.y);
}

vec3 get_light() 
{
  //return vec3(6.*sin(0.3*time),5.,6.*cos(0.3*time));
    return vec3(6.0,5.0,2.0);
}

float get_plane_dist(in vec3 p) 
{
  return p.y + 0.5*noise(0.5*p.xz) + 0.05*noise(2.*p.xz) + 0.012*noise(4.*p.xz);
}

float get_sphere_dist(in vec3 p, in vec4 s) 
{ 
  return length(p - s.xyz) - s.w;
}

vec2 get_sphere_tex_coord(in vec3 p) 
{
  vec4 s = vec4(0, 1.2, 0, 1);
  vec3 r = p - s.xyz;
  float phi = atan(sqrt(dot(r.xz, r.xz)), r.y);
  float theta = atan(r.z, r.x);
  return fract(vec2(phi / PI, theta / TWO_PI));
}

float SDF(in vec3 p) 
{
  float sphere1Dist = get_sphere_dist(p, sphere1Pos);
  float sphere2Dist = get_sphere_dist(p, sphere2Pos);
  float sphere3Dist = get_sphere_dist(p, sphere3Pos);
  float planeDist = get_plane_dist(p);
  float d = min(min(sphere1Dist, sphere2Dist), min(sphere3Dist, planeDist));
  return d;
}

int get_object_id(in vec3 p) 
{
  if (get_sphere_dist(p, sphere1Pos) < 0.0) return 1;
  if (get_sphere_dist(p, sphere1Pos) < 0.0) return 2;
  if (get_sphere_dist(p, sphere1Pos) < 0.0) return 3;
  if (get_plane_dist(p) < 0.0) return 4;
  return 0;
}

vec3 get_color(in vec3 p) 
{
  if (get_sphere_dist(p, sphere1Pos) < 0.0) return vec3(0.5, 0.4, 1.0); // sphere 1 color blue
  if (get_sphere_dist(p, sphere2Pos) < 0.0) return vec3(1.0, 0.2, 0.2); // sphere 2 color red
  if (get_sphere_dist(p, sphere3Pos) < 0.0) return vec3(0.1, 0.9, 0.1); // sphere 3 color green
  if (get_plane_dist(p) < 0.0) return vec3(0.8, 0.55, 0.3); // ground color
  //if (get_sphere_dist(p, sphere1Pos) < 0.0) return texture2D(u_texture, get_sphere_tex_coord(p)).rgb; // sphere color
  //if (get_plane_dist(p) < 0.0) return texture2D(u_texture, fract(0.2*p.xz)).rgb; // ground color
  return vec3(0.);
}

float get_density(in vec3 p) 
{
  if (get_sphere_dist(p, sphere1Pos) < 0.0) return 0.2; // sphere 1 density
  if (get_sphere_dist(p, sphere2Pos) < 0.0) return 0.2; // sphere 2 density
  if (get_sphere_dist(p, sphere2Pos) < 0.0) return 0.2; // sphere 3 density
  if (get_plane_dist(p) < 0.0) return 10.0; // ground density
  return 0.;
}

float get_specular(in vec3 p) 
{
  if (get_sphere_dist(p, sphere1Pos) < 0.0) return 0.8; // sphere 1 specular
  if (get_sphere_dist(p, sphere2Pos) < 0.0) return 0.5; // sphere 2 specular
  if (get_sphere_dist(p, sphere3Pos) < 0.0) return 0.1; // sphere 3 specular
  if (get_plane_dist(p) < 0.0) return 0.05; // ground specular
  return 0.;
}

float ray_march(in vec3 ro, in vec3 rd) 
{
  float d = 0.;
  for (int i = 0; i < MAX_STEPS; ++i) 
  {
    vec3 p = ro + rd * d;
    float ds = SDF(p);
    if (ds < SURF_DIST) { return d;}
    d += ds;
    if (d > MAX_DIST){ return 1./0.;}
  }
  return 1./0.;
}

vec3 get_normal(in vec3 p) 
{ 
  vec2 e = vec2(.01,0); // Epsilon
  vec3 n = vec3(
    SDF(p+e.xyy) - SDF(p-e.xyy),
    SDF(p+e.yxy) - SDF(p-e.yxy),
    SDF(p+e.yyx) - SDF(p-e.yyx)
  );
  return normalize(n);
}

float compute_contrast(in vec3 p, in vec3 n) 
{
  // Directional light
  vec3 l = get_light(); // Light Position
  vec3 ld = normalize(l-p); // Light Vector
  float dif = dot(n,ld); // Diffuse light
  dif = clamp(dif,0.,1.); // Clamp so it doesnt go below 0
  // Shadows
  float d = ray_march(p + 10.*n*SURF_DIST, ld);
  if (d < length(l-p))
    dif *= 0.3;
  return dif;
}

float compute_reflection(in vec3 p, in vec3 n, in vec3 c, in float f) 
{
  vec3 l = get_light(); // Light Position
  vec3 ld = normalize(l-p); // Light Vector
  vec3 r = reflect(-ld, n); // reflected vector of sunlight
  float s = clamp(dot(r, normalize(c - p)), 0., 1.); // dot product between reflected light and camera vector
  return pow(s, 10.0) * f;
}

void main()
{
  vec3 color = vec3(0.);
  vec2 uv = gl_FragCoord.xy;
  // intrinsic parameter
  vec2 f = vec2(600.);
  vec2 c = 0.5 * u_resolution.xy;
  // normalized image plane
  vec2 xy = (uv - c) / f;
  // distortion
  vec2 k_d = vec2(0.3,0.0);
  vec2 p_d = vec2(0.0,0.0);
  float r2 = dot(xy, xy);
  xy = (1. + k_d.x*r2 + k_d.y*r2*r2) * xy + vec2(2.*p_d.x*xy.x*xy.y + p_d.y*(r2 + 2.*xy.x*xy.x), 2.*p_d.y*xy.x*xy.y + p_d.x*(r2 + 2.*xy.y*xy.y));
  // camera
  float theta = mouse.x / 100.0; // camera yaw
  float phi = mouse.y / 100.0; // camera pitch
  mat3 Ry = mat3(cos(theta), 0., -sin(theta), 0., 1., 0., sin(theta), 0., cos(theta));
  mat3 Rx = mat3(1., 0., 0., 0., cos(phi), sin(phi), 0., -sin(phi), cos(phi));
  mat3 R = Ry * Rx; // camera rotation
  float r = 6.0; // camera distance
  vec3 t = vec3(-r*cos(phi)*sin(theta), r*sin(phi), -r*cos(phi)*cos(theta)); // camera translation
  mat4 T = mat4(vec4(R[0], 0.0), vec4(R[1], 0.0), vec4(R[2], 0.0), vec4(t, 1.0));
  vec3 co = vec3(0.0,2.5,-2.0); // camera origin
  vec3 cd = normalize(vec3(xy,1)); // camera ray vector
  // ray
  vec3 ro = (T * vec4(co, 1.)).xyz; // ray origin w.r.t world
  vec3 rd = (T * vec4(cd, 0.)).xyz; // ray vector w.r.t world
  // find the surface point
  float d = ray_march(ro, rd);
  vec3 p = ro + rd * d;
  vec3 n = get_normal(p);
  color = get_color(p - n*SURF_DIST); // apply color and texture
  color *= compute_contrast(p, n); // apply lighting and shading
  // apply specular lighting
  color += compute_reflection(p, n, ro, get_specular(p-n*SURF_DIST));
  gl_FragColor = vec4(color,1.0);
}
