//   __  _                 _                                           
//  / _\| |__    __ _   __| |  ___ __      __  _ __ ___    __ _  _ __  
//  \ \ | '_ \  / _` | / _` | / _ \\ \ /\ / / | '_ ` _ \  / _` || '_ \ 
//  _\ \| | | || (_| || (_| || (_) |\ V  V /  | | | | | || (_| || |_) |
//  \__/|_| |_| \__,_| \__,_| \___/  \_/\_/   |_| |_| |_| \__,_|| .__/ 
//                                                              |_|    


#define ITER_SHADOW 128
float marchShadow(vec3 rayOriginWS, vec3 rayDirWS, float t, float mt, out float lastDist)
{
    float d;
    float minVisibility = 1.0;
    lastDist = 100.0;
    
    vec4 material;
    
    for(int i = NON_CONST_ZERO; i < ITER_SHADOW && t < mt; ++i)
    {
        float coneWidth = max(0.00001, kTanSunRadius * t);
        
        vec3 posWS = rayOriginWS + rayDirWS*t;
        d = fSDF(posWS, kShadowMapFilter, iChannel2, material);
        
        float stepMinVis = (d) / max(0.0001, coneWidth * 0.5);
        if(stepMinVis <= minVisibility)
        {
            minVisibility = stepMinVis;
            lastDist = t;
        }
        
        t += max(0.01, d);           
        
        if(minVisibility < 0.01)
        {
            minVisibility = 0.0;
        }
    }
      
    return smoothstep(0.0, 1.0, minVisibility);
}

void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
    vec2 uv = fragCoord.xy / iResolution.xy;
    vec2 uvSNorm = uv * 2.0 - oz.xx;
    
    initScene(oz.xxx, iTime);
        
    vec3 shadowPlaneForwardWS = getShadowForwardDirWS(s_dirToSunWS);
    
	vec3 rayStartWS = uvSNormToShadowRayStartWS(uvSNorm, shadowPlaneForwardWS);
    
    float offsetFromShadowPlane = min(100.0, rayStartWS.y/shadowPlaneForwardWS.y);
    rayStartWS -= shadowPlaneForwardWS * offsetFromShadowPlane;
    
    float shadowDist = 0.0;
    float shadow = marchShadow(rayStartWS, shadowPlaneForwardWS, offsetFromShadowPlane - kShadowMapRangeWS, offsetFromShadowPlane + 100., shadowDist);
    
    shadowDist -= offsetFromShadowPlane;

    fragColor = vec4(shadow, shadowDist/kShadowMapRangeWS, 0.0, 1.0);
}
