//   _____                 _         _   _                     _           _ 
//  /__   \_ __ ___  ___  (_)_ __   | |_| |__   ___  __      _(_)_ __   __| |
//    / /\/ '__/ _ \/ _ \ | | '_ \  | __| '_ \ / _ \ \ \ /\ / / | '_ \ / _` |
//   / /  | | |  __/  __/ | | | | | | |_| | | |  __/  \ V  V /| | | | | (_| |
//   \/   |_|  \___|\___| |_|_| |_|  \__|_| |_|\___|   \_/\_/ |_|_| |_|\__,_|
//                                                                                                       
//
//----------------------------------------------------------------------------------------
//     ___                                                    _  _    
//    / __\ _   _     /\/\    __ _  _   _  _ __  ___    __ _ (_)| | __
//   /__\//| | | |   /    \  / _` || | | || '__|/ _ \  / _` || || |/ /
//  / \/  \| |_| |  / /\/\ \| (_| || |_| || |  | (_) || (_| || ||   < 
//  \_____/ \__, |  \/    \/ \__,_| \__,_||_|   \___/  \__, ||_||_|\_\
//          |___/                                      |___/          
//
//----------------------------------------------------------------------------------------

//The day/night cyle takes around 1 a minute, so sit back and relax.

//Text ascii art using : http://patorjk.com/software/taag/

float tonemapOp(float v)
{
    v = pow(v, 2.0);
    v = v / (1.0 + v);
    return pow(v, 1.0/2.0) * 1.025;
}

vec3 tonemap(vec3 colour)
{
    float inputLuminance = max(0.0001, rbgToluminance(colour));
    vec3 normalisedColour = colour / inputLuminance;
    
    vec3 tonemapColour;
    tonemapColour.r = tonemapOp(colour.r);
    tonemapColour.g = tonemapOp(colour.g);
    tonemapColour.b = tonemapOp(colour.b);
    float tonemappedLuminance = tonemapOp(inputLuminance);
    
    tonemapColour = (tonemapColour / max(0.0001, rbgToluminance(tonemapColour)));
    
    return tonemappedLuminance * mix(normalisedColour, tonemapColour, min(1.0, 0.35*inputLuminance));
}

void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
	vec2 uv = fragCoord.xy / iResolution.xy;
      
	vec4 colour = texture(iChannel0, uv);
    
    //Vignette
    colour.rgb *= smoothstep(1.15, 0.3, length(uv - 0.5*oz.xx));
    
	//Tonemap
    float toeStrength = 1.25;
    colour.rgb = tonemap(colour.rgb * toeStrength); colour.rgb = pow(colour.rgb, toeStrength*oz.xxx);
    
    //Gamma
    colour = pow(colour, vec4(1.0/2.2));
    fragColor = colour;
    
    //Dithering
    fragColor += ((hash12(fragCoord)) - 0.5)*4.0/255.0;
}