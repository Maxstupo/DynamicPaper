//   __                                                                           _      _               
//  / _\  ___  ___  _ __    ___   _ __  __ _  _   _  _ __ ___    __ _  _ __  ___ | |__  (_) _ __    __ _ 
//  \ \  / __|/ _ \| '_ \  / _ \ | '__|/ _` || | | || '_ ` _ \  / _` || '__|/ __|| '_ \ | || '_ \  / _` |
//  _\ \| (__|  __/| | | ||  __/ | |  | (_| || |_| || | | | | || (_| || |  | (__ | | | || || | | || (_| |
//  \__/ \___|\___||_| |_| \___| |_|   \__,_| \__, ||_| |_| |_| \__,_||_|   \___||_| |_||_||_| |_| \__, |
//                                            |___/                                                |___/ 

float fMaterialSDF(vec3 samplePosWS, float dist, vec4 material)
{
    if(abs(material.x - kMatMountains) < 0.1)
    {
        vec2 uv = samplePosWS.xz * 0.0015;
       	float mountainNoise = noiseFbm(uv / material.y, iChannel2);
        
        dist -= mountainNoise * 5.0 * material.y;
    }
    else if(abs(material.x - kMatMapleBark) < 0.1)
    {
        float progressAlongBranch = material.w;
        float u = material.z/kPI;

        vec2 branchUv = vec2(u * 0.5, progressAlongBranch * 0.5);

        dist -= textureLod(iChannel2, branchUv, 0.0).r * 0.05; 
    }
    else if(abs(material.x - kMatPines) < 0.1)
    {
        float pineGroundDist = material.y;
        float mountainNoise = material.z;
        dist -= sin(pineGroundDist*kPI)*0.5*(1.0 - pineGroundDist/20.0);
    }
    return dist;
}

vec3 getNormalWS(vec3 p, float dt)
{
    vec3 normalWS = oz.yyy;
    for( int i = NON_CONST_ZERO; i<4; i++ )
    {
        vec3 e = 0.5773*(2.0*vec3((((i+3)>>1)&1),((i>>1)&1),(i&1))-1.0);
        vec3 samplePosWS = p + e * dt;
        vec4 mat;
        float dist = fSDF(samplePosWS, kRenderFilter, iChannel2, mat);
        normalWS += e*fMaterialSDF(samplePosWS, dist, mat);
    }
    return normalize(normalWS);    
}

float sampleShadowMap(vec3 p, float startOffset)
{
    float shadowMapRangeWS = 14.0;
    
    vec3 uv_depth = getShadowUvFromPosWS(p);
    
    vec2 shadow_depth = textureLod(iChannel1, uv_depth.xy, 0.0).xy;
    
    if(shadow_depth.y > (uv_depth.z + startOffset / kShadowMapRangeWS))
    {
        return shadow_depth.x;
    }
    else
    {
        return 1.0;
    }
}

float globalShadow(vec3 posWS, vec3 rayDirWS)
{    
    float softness = 0.01;
    
    // Far left
    float scale = 1.65;
    vec3 moutainPosWS = scale*vec3(300.0, -100.0, 1400.0);
    moutainPosWS += scale*vec3(-200.0, 100.0, 0.0);
    float mountainShadow = sphSoftShadow(posWS, rayDirWS, moutainPosWS, scale*600.0 * 0.85, softness);

    // A bit to the right
    scale = 2.7;
    moutainPosWS = scale*vec3(600.0, -100.0, 1000.0);
    moutainPosWS += scale*vec3(-50.0, 0.0, 0.0);
    mountainShadow *= sphSoftShadow(posWS, rayDirWS, moutainPosWS, scale*500.0 * 0.6, softness);

    scale = 4.45;
    moutainPosWS = scale*vec3(1000.0, -200.0, 900.0);
    mountainShadow *= sphSoftShadow(posWS, rayDirWS, moutainPosWS, scale*500.0 * 0.7, softness);

    return mountainShadow;;
   
}

float getShadow(vec3 p, vec3 sd)
{
	return sampleShadowMap(p, 0.1);
}

float cloudNoiseFbm(vec2 uv)
{
    float maxNoise = 0.0;
    float noise = 0.0;
    
    float amplitude = 1.0;
    float scale = 1.0;
    
    vec2 windOffset = oz.yy;
    
    for(int i = NON_CONST_ZERO; i < 7; ++i)
    {
        windOffset += s_time/scale * 0.0015 * kWindVelocityWS.xz;
        
        noise += amplitude * textureLod(iChannel2, uv*scale - windOffset, 0.0).r;
    	maxNoise += amplitude;
    	amplitude *= 0.5;
        scale *= 2.0;
    }
    
    return noise / maxNoise;
}

vec3 computeFinalLighting(float marchedDist, vec3 rayOriginWS, vec3 rayDirWS,
                          vec4 material)
{    
    vec3 endPointWS = rayOriginWS + rayDirWS * marchedDist;

    vec3 sceneColour = oz.yyy;
    
    if(marchedDist < kMaxDist)
    {
        float coneWidth = max(0.001, s_pixelConeWithAtUnitLength * (marchedDist - 10.0));
        float normalDt = coneWidth;
        vec3 normalWS = getNormalWS(endPointWS, normalDt);
        normalWS = fixNormalBackFacingness(rayDirWS, normalWS);
        
        vec3 worldShadowOffset = oz.yxy * 1000.0 * linearstep(0.1, 0.2, s_timeOfDay);
        float worldShadow = globalShadow(endPointWS + worldShadowOffset, s_dirToSunWS);
        float atmShadow = saturate(worldShadow + (s_dirToSunWS.y - 0.0615)*15.0);
     	float shadow = getShadow(endPointWS, s_dirToSunWS) * worldShadow;        
        
        vec3 albedo = oz.xyx;
        vec3 f0Reflectance = oz.xxx * 0.04;
        float roughness = 0.6;
        vec4 emissive = oz.yyyy;
        float ambientVis = 1.0;

        if(abs(material.x - kMatMapleLeaf) < 0.1
               || abs(material.x - kMatFallingMapleLeaf) < 0.1)
        {
            ambientVis = max(0.25, material.y);
            shadow *= material.y;
            
            float inside = material.z;
            float leafRand = floor(material.w) / 100.0;
            float tint = min(1.0, leafRand*leafRand*0.5 + inside);
                
            albedo = mix(vec3(0.5, 0.0075, 0.005), vec3(0.5, 0.15, 0.005), tint*tint);

            float stick = max(0.0, fract(material.w) - 0.75*inside);
			albedo = mix(albedo, vec3(0.2, 0.04, 0.005), stick);
            //Backlighting
            emissive.rgb = henyeyGreensteinPhase(dot(s_dirToSunWS, rayDirWS), 0.5)
                * shadow * albedo * albedo * s_sunColour * (1.0 - stick) * 4.0;
            //emissive.a = 1.0;
            vec2 uv = material.yz;

            roughness = 0.7 - stick*0.2;
        }
        else if(abs(material.x - kMatMapleBark) < 0.1)
        {
            float progressAlongBranch = material.w;
            ambientVis = max(0.25, material.y);
            float u = material.z;
            
            vec2 branchUv = vec2(u, progressAlongBranch);
            roughness = 0.6;

            albedo = vec3(0.2, 0.04, 0.005);
        }
        else if(abs(material.x - kMatGrass) < 0.1)
        {
            float normalisedHeight = material.y;
            ambientVis = 0.15 + 0.85*material.z;
            shadow *= min(1.0, material.z * 3.0);
            float grassRand = material.w;
            
            albedo = mix(vec3(0.005, 0.35, 0.015), vec3(0.1, 0.35, 0.015), 
                         saturate(normalisedHeight*normalisedHeight + (grassRand - 0.5)));

            //Backlighting
            emissive.rgb = henyeyGreensteinPhase(dot(s_dirToSunWS, rayDirWS), 0.5)
                * shadow * albedo * albedo * s_sunColour * 4.0 * normalisedHeight;
            
            roughness = 0.75;
        }
        else if(abs(material.x - kMatMountains) < 0.1)
        {
            float mountainNoise = material.z;
            float mountainScale = material.y;
            float detailNoise = noiseFbm(endPointWS.xz * (0.002 / mountainScale), iChannel2);
            float noise = (detailNoise * 0.5 + mountainNoise) / 1.5;
            float treeDist = material.w;
            
            albedo = 0.5*vec3(0.15, 0.025, 0.001);
            roughness = 0.85;
            
            float rocks = linearstep(1.15, 1.3, detailNoise * 0.3 + noise * 0.7);
            albedo = mix(albedo, oz.xxx * 0.1, rocks);
            roughness = mix(0.85, 0.5, rocks);
            
            float snowAmount = saturate((noise - 0.5)*2.0 + (endPointWS.y - 550.0)/300.0);
            albedo = mix(albedo, oz.xxx, snowAmount);
            roughness = mix(roughness, 1.0, snowAmount);
            
            ambientVis = (0.5 + 0.5*saturate(treeDist*0.1));
            shadow *= saturate(treeDist*0.1);
        }
        else if(abs(material.x - kMatPines) < 0.1)
        {
            float mountainNoise = material.z;
            float bottomToTop = material.y / 20.0;
            
            albedo = mix(0.5*vec3(0.005, 0.15, 0.01), 0.25*vec3(0.005, 0.1, 0.05), linearstep(1.0, 1.2, mountainNoise));
            ambientVis = (0.5 + 0.5*bottomToTop);
            shadow *= saturate(bottomToTop * 5.0);
            roughness = 0.85;
        }        
            
        //Lighting part
        {            
            vec3 reflectedRayDirWS = reflect(rayDirWS, normalWS);
            float rDotN = max(0.0001, dot(reflectedRayDirWS, normalWS));
            vec3 fresnelReflectance = roughFresnel(f0Reflectance, rDotN, roughness);

            vec3 diffuse = albedo * computeLighting(endPointWS, normalWS, normalWS, 1.0, ambientVis, shadow);
            vec3 specular = computeLighting(endPointWS, reflectedRayDirWS, normalWS, roughness, ambientVis, shadow);
            vec3 surfaceLighting = mix(diffuse, specular, fresnelReflectance);
            sceneColour = surfaceLighting * (1.0 - emissive.a) + emissive.rgb;
        }
        
        sceneColour = applyAtmosphere(sceneColour, rayDirWS, marchedDist, atmShadow);
    }
    else
    {
        vec3 skyColour = getSkyAndSun(rayDirWS);
    	sceneColour = skyColour;
        
        //Stars
        float scale = 3.0;
        vec3 stars = oz.yyy;
        for(uint i = NON_CONST_ZERO_U; i < 4u; ++i)
        {
            vec3 rd = rayDirWS;
            rd.y = max(0.0001, (rd.y + 0.5) / 1.5);
            vec2 uv = rd.xz/rd.y;
            uv.y += s_earthRotationRad;
            uv += oz.xx * float(i) * kGoldenRatio;
            
            vec2 id = pMod2(uv, oz.xx * 0.1 * scale) + oz.xx * float(i) * kGoldenRatio;

            vec2 rand = hash22(id * 73.157);
            uv += (rand - 0.5*oz.xx) * 0.1 * scale;
            
            float starHeat = hash12(id * 73.157);
            vec3 starColour = oz.xxx * 0.75 + 0.5 * hash32(id * 953.56);
			starHeat *= starHeat;
            float startFade = linearstep((0.5 + starHeat*0.5)*0.003/rd.y, 0.0, length(uv));
            stars += startFade * startFade * starColour * 0.04 * (0.5 + starHeat*starHeat) * scale * scale;
            
            scale *= 0.5;
        }
        
        vec3 skyTransmittance;
        getSky(rayDirWS, 0.0, skyTransmittance);
        stars = stars * skyTransmittance;
        
        float cloudPlaneInterDist = 4000.0/max(0.0001, rayDirWS.y + 0.13);
        vec3 cloudPosWS = rayOriginWS + rayDirWS * cloudPlaneInterDist;
        
        float rayDotSun = dot(s_dirToSunWS, rayDirWS);
        float earthShadow = smoothstep(-0.06, -0.04, s_dirToSunWS.y + (rayDotSun - 0.5)*0.035);
        
        vec2 cloudUv = fract(cloudPosWS.xz * 0.0000035); 
        vec2 cloudShadowUv = cloudUv + s_dirToSunWS.xz * 0.002; 
        
        float cloudNoise = cloudNoiseFbm(cloudUv);
        
        float phase = henyeyGreensteinPhase_schlick(dot(rayDirWS, s_dirToSunWS), 0.7);
        float cloudTransmittance = 1.0 - smoothstep(0.6, 0.7, cloudNoise);
        float cloudShadow = 1.0 - 0.9*smoothstep(0.5, 0.8, cloudNoiseFbm(cloudShadowUv));
        vec3 cloudInscatter = s_averageSkyColour + max(oz.xxx * 0.00035, s_cloudSunColour * earthShadow) *
            (1.0 + phase * 1.0 * kPI) * cloudShadow;
        
        cloudInscatter = applyAtmosphere(cloudInscatter, rayDirWS, cloudPlaneInterDist, 1.0);
        
        sceneColour += stars;
        sceneColour = sceneColour * cloudTransmittance + (1.0 - cloudTransmittance) * cloudInscatter;
    }
    
    return sceneColour;
}

float fogNoiseFbm(vec3 uvw)
{
    float maxNoise = 0.0;
    float noise = 0.0;
    
    float amplitude = 1.0;
    
    for(int i = NON_CONST_ZERO; i < 3; ++i)
    {
        noise += amplitude * textureLod(iChannel3, uvw.xy, 0.0).r;
    	maxNoise += amplitude;
    	amplitude *= 0.5;
        uvw *= 2.0;
    }
    
    return noise / maxNoise;
}

#define ITER 1024
float march(vec3 rayOriginWS, vec3 rayDirWS, float rand, out vec3 sceneColour, out float responsiveness)
{
    float phase = 4.0 * kPI * 
        (henyeyGreensteinPhase_schlick(dot(rayDirWS, s_dirToSunWS), 0.7) + 
        kIsotropicScatteringPhase);
        
    vec4 material;
    
    vec3 inscatter = oz.yyy;
    float transmittance = 1.0;
    
    float t = 0.001;
    float d;
    
    for(int i = NON_CONST_ZERO; i < ITER; ++i)
    {
        float coneWidth = max(0.001, s_pixelConeWithAtUnitLength * (t - 10.0));
        
        vec3 posWS = rayOriginWS + rayDirWS*t;
        d = fSDF(posWS, kRenderFilter, iChannel2, material);
        
        t += d;
        
        if(i >= ITER - 1)
        {
            t = kMaxDist;
        }              
        
        if(d < coneWidth || t >= kMaxDist)
        {
            break;
        }
        
        
        // Add some fog at the bottom of the tree, because shadowing from leaves looks cool !
#if 1
        vec3 fogVolumeCenterWs = kTreePosWS - oz.yxy * 99.0;
        float distToFogVolume = fCapsule(posWS, fogVolumeCenterWs, 
                                         fogVolumeCenterWs,
                                         100.0);
        float distToTree = length((kTreePosWS - s_dirToSunWS * oz.xyx * 5.0) - posWS);
        
        float densityBias = ( max(0.0, distToTree - 2.0) * 0.035 + 0.2 ) +
            linearstep(0.0, 10.0, distToFogVolume);
        
        if(densityBias < 1.0)
        {
            float fogNoise = fogNoiseFbm((posWS - oz.yxy * 0.75 * s_time) * 0.025);
            fogNoise = max(0.0, fogNoise - densityBias);
            fogNoise = saturate(fogNoise * 2.0);

            float scatteringCoef = fogNoise * 0.1;

            float stepTransmittance = exp(-scatteringCoef*d);

            posWS = rayOriginWS + rayDirWS*(t - d*rand);
            float shadow = sampleShadowMap(posWS, 0.0) * linearstep(0.0575, 0.1, s_dirToSunWS.y);;

                vec3 stepLighting =  s_sunColour * shadow * phase;
            stepLighting += s_averageSkyColour;

            inscatter += transmittance * (1.0 - stepTransmittance) * stepLighting;
            transmittance *= stepTransmittance;
        }
#endif
        
    }
      
    sceneColour = computeFinalLighting(t, rayOriginWS, rayDirWS, material);
    
    sceneColour = sceneColour * transmittance + inscatter;
    
    responsiveness = abs(material.x - kMatFallingMapleLeaf) < 0.1 ? 1.0f : 0.0f;
    
    return t;
}


vec3 applyBloom(vec3 colour, vec2 uv, vec2 subPixelJitter)
{
#if 1
    float totalWeight = 0.0;
    vec3 bloom = oz.yyy;
    
    float kernel = 1.0;
    //Super sample low mips to get less blocky bloom
    for(float xo = -kernel; xo < kernel + 0.1; xo += 0.5)
    {
        for(float yo = -kernel; yo < kernel + 0.1; yo += 0.5)
        {
            vec2 vo = vec2(xo, yo);
            float weight = (kernel*kernel*2.0) - dot(vo, vo);
            vo += 0.5 * (subPixelJitter);
            vec2 off = vo*(0.5/kernel)/iResolution.xy;
            
            if(weight > 0.0)
            {
                float maxBloom = 5.0;
                bloom += weight * min(maxBloom*oz.xxx, textureLod(iChannel0, uv + off*exp2(5.0), 5.0).rgb);totalWeight += weight;
                bloom += weight * min(maxBloom*oz.xxx, textureLod(iChannel0, uv + off*exp2(6.0), 6.0).rgb);totalWeight += weight;
                bloom += weight * min(maxBloom*oz.xxx, textureLod(iChannel0, uv + off*exp2(7.0), 7.0).rgb);totalWeight += weight;
            }
        }
    }

    bloom.rgb /= totalWeight;
    
    colour.rgb = colour.rgb * 0.8 + pow(bloom, oz.xxx*1.5) * 0.3;
#endif
    
    return colour;
}


#define TAA 1

void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
    vec2 subPixelJitter = fract(hash22(fragCoord)
                                + float(iFrame%256) * kGoldenRatio * oz.xx) - 0.5*oz.xx;
    
    vec2 uv = fragCoord.xy / iResolution.xy;
    float jitterAmount = float(TAA);
    
    vec2 uvJittered = (fragCoord.xy + jitterAmount * subPixelJitter) / iResolution.xy;
    
    float aspectRatio = iResolution.x/iResolution.y;
    vec2 uvSNorm = uvJittered * 2.0 - vec2(1.0);
    uvSNorm.x *= aspectRatio;
    
    vec2 mouseUNorm = iMouse.xy/iResolution.xy;
    vec2 mouseNorm = mouseUNorm*2.0 - vec2(1.0);
    
    vec3 rayOriginWS;
    
    // ---- Camera setup ---- //
    vec3 cameraForwardWS, cameraUpWS, cameraRightWS;
    computeCamera(iTime, mouseNorm, iMouse, iResolution.xy, rayOriginWS, cameraForwardWS, cameraUpWS, cameraRightWS);
    vec3  rayDirWS = normalize(uvSNorm.x*cameraRightWS + uvSNorm.y*cameraUpWS + kCameraPlaneDist*cameraForwardWS);
    
    initScene(rayOriginWS, iTime);
    
    // ---- TAA part ---- //  
    float prevTime = iTime - iTimeDelta;
    vec3 prevCameraPosWS, prevCameraForwardWS, prevCameraUpWS, prevCameraRightWS;
    computeCamera(prevTime, mouseNorm, iMouse, iResolution.xy, prevCameraPosWS, 
                  prevCameraForwardWS, prevCameraUpWS, prevCameraRightWS);
    
    // ---- Render scene ---- //
    float responsiveness;
    vec3 sceneColour;
    float tt = march(rayOriginWS, rayDirWS, hash12(fragCoord), sceneColour, responsiveness);
    
    float nightTimeStrength = linearstep(-0.15, -0.6, s_dirToSunWS.y);
    
    float exposure = 1.0 + nightTimeStrength*150.0;
    sceneColour *= exposure;
    
    sceneColour = applyBloom(sceneColour, uv, subPixelJitter);
        
    // ---- Flares ---- //
    {
        float sunVisibility = sampleShadowMap(rayOriginWS, 0.0) * globalShadow(rayOriginWS, s_dirToSunWS);
        float numApertureBlades = 8.0;

        vec2 sunUv = getScreenspaceUvFromRayDirectionWS(s_dirToSunWS,
                                                           cameraForwardWS, cameraUpWS, cameraRightWS, aspectRatio);
        vec2 sunUvSNorm = sunUv * 2.0 - vec2(1.0);
    	sunUvSNorm.x *= aspectRatio;
        
        vec2 sunUvDirSNorm = normalize(sunUvSNorm);
        
        float rDotL = dot(rayDirWS, s_dirToSunWS);
        
        vec2 uvToSunSNorm = sunUvSNorm - uvSNorm;
        float starBurst = 0.5 + 0.5*cos(1.5 * kPI + atan(uvToSunSNorm.x, uvToSunSNorm.y) * numApertureBlades);
        float startFade = linearstep(0.97, 1.0, rDotL);
        float starWidth = linearstep(0.5, 1.0, rDotL);
        starBurst = pow(starBurst, max(1.0, 500.0 - starWidth*starWidth * 501.0))*startFade;

        vec3 totalFlares = starBurst * 1.5 * linearstep(0.875, 1.0, rDotL*rDotL) * oz.xxx;

        vec2 flareCenterUvSNorm;
        
        flareCenterUvSNorm = sunUvSNorm * 2.0;
        totalFlares += 0.5 * vec3(0.1, 0.15, 0.01) * 
            linearstep(0.05, 0.0, sdOctogon(flareCenterUvSNorm - uvSNorm, 0.35));
        
        flareCenterUvSNorm = sunUvSNorm * 1.5;
        totalFlares += 0.65 * vec3(0.015, 0.12, 0.09) * 
            linearstep(0.03, 0.0, sdOctogon(flareCenterUvSNorm - uvSNorm, 0.2));
        
        flareCenterUvSNorm = sunUvSNorm * 0.25;
        totalFlares += 2.5 * vec3(0.1, 0.1, 0.1) * 
            linearstep(0.01, 0.0, sdOctogon(flareCenterUvSNorm - uvSNorm, 0.1));
        
        flareCenterUvSNorm = sunUvSNorm * 0.1;
        totalFlares += 4.0 * vec3(0.11, 0.11, 0.1) * 
            linearstep(0.02, 0.0, sdOctogon(flareCenterUvSNorm - uvSNorm, 0.05));
        
        flareCenterUvSNorm = sunUvSNorm * -0.12;
        totalFlares += 3.5 * vec3(0.15, 0.05, 0.025) * 
            linearstep(0.01, 0.0, sdOctogon(flareCenterUvSNorm - uvSNorm, 0.15));
        
        flareCenterUvSNorm = sunUvSNorm * -0.25;
        totalFlares += 0.8 * vec3(0.02, 0.15, 0.1) * 
            linearstep(0.02, 0.0, sdOctogon(flareCenterUvSNorm - uvSNorm, 0.25));
        
        //First coloured circle
        {
            flareCenterUvSNorm = sunUvSNorm * 0.5;

            float distToDisk = length(flareCenterUvSNorm - uvSNorm) - length(flareCenterUvSNorm * 2.0);
            float ci = smoothstep(0.2, 0.01, abs(distToDisk));
            float colourRamp = linearstep(-0.2, 0.2, distToDisk);
            vec2 uvToSunDir = normalize(uvToSunSNorm);
            vec3 repColour = wavelengthToRGB(300.0 + colourRamp * 500.0);

            totalFlares += 0.25 * repColour * ci * linearstep(0.75, 1.0, dot(sunUvDirSNorm, uvToSunDir));
        }
        
        //Second coloured circle
        {
            flareCenterUvSNorm = sunUvSNorm * 0.5;

            float distToDisk = length(flareCenterUvSNorm - uvSNorm) - length(flareCenterUvSNorm * 1.6);
            float ci = smoothstep(0.4, 0.0, abs(distToDisk));
            float colourRamp = linearstep(-0.8, 0.8, distToDisk);
			vec2 uvToSunDir = normalize(uvToSunSNorm);
            vec3 repColour = wavelengthToRGB(300.0 + colourRamp * 500.0);
            totalFlares += 0.25 * repColour * ci * linearstep(0.98, 1.0, dot(sunUvDirSNorm, uvToSunDir));
        }
        
        
        flareCenterUvSNorm = sunUvSNorm * -1.4;
        totalFlares += 0.8 * vec3(0.1, 0.15, 0.075) * 
            linearstep(0.015, 0.0, sdOctogon(flareCenterUvSNorm - uvSNorm, 0.17));
        
        
        flareCenterUvSNorm = sunUvSNorm * -2.0;
        totalFlares += 3.0 * vec3(0.1, 0.05, 0.02) * 
            linearstep(0.4, 0.0, sdOctogon(flareCenterUvSNorm - uvSNorm, 0.1));
        
        sceneColour += totalFlares * 0.2 * s_sunColour * sunVisibility * linearstep(3.2, 2.0, length(sunUvSNorm));
    }
    
    vec2 prevFrameUv = uv;
    vec4 prevData = texture(iChannel0, prevFrameUv);
    
    float defaultBlendToCurrent = (2.0/9.0)*max(1.0, iTimeDelta/0.066);
    float blendedResp = mix(prevData.a, responsiveness, defaultBlendToCurrent);
    float blendToCurrent = mix(defaultBlendToCurrent, 1.0, blendedResp);
    //Increase TAA at night to get stars trails
    blendToCurrent = min(1.0, mix(blendToCurrent, iTimeDelta, nightTimeStrength));
    
#if !TAA    
    blendToCurrent = 1.0;
#endif
    
    //Clamp to prevent the sun from leaving a trail
    sceneColour = min(oz.xxx * 3.0, sceneColour);
    sceneColour = max(oz.yyy, mix(prevData.rgb, sceneColour, blendToCurrent));
    
    fragColor = vec4(sceneColour, responsiveness);
}