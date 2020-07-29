namespace Maxstupo.DynWallpaper.Graphics.ShaderToy {

    using OpenTK;

    public sealed class ShaderToyInput {
        public Vector3 iResolution;                             // viewport resolution (in pixels)
        public float iTime;                                     // shader playback time (in seconds)
        public float iGlobalTime;
        public Vector4 iMouse;                                  // mouse pixel coords. xy: current (if MLB down), zw: click
        public Vector4 iDate;                                   // (year, month, day, time in seconds)
        public float iSampleRate;                               // sound sample rate (i.e., 44100)
        public Vector3[] iChannelResolution = new Vector3[4];   // channel resolution (in pixels)
        public float[] iChannelTime = new float[4];             // channel playback time (in seconds)

        public int iFrame;       // shader playback frame
        public float iTimeDelta; // render time (in seconds)
        public float iFrameRate;

        public ShaderToyInput() {
            iResolution = new Vector3(1.0f, 1.0f, 1.0f);
            iTime = 0.0f;
            iGlobalTime = 0.0f;
            iMouse = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            iDate = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            iSampleRate = 44100 * 1.0f;


            iFrame = 0;
            iTimeDelta = 0.0f;
            iFrameRate = 0.0f;
        }

    }

}