namespace Maxstupo.DynamicPaper.ShaderToy {

    using OpenTK;

    public sealed class RenderData {

        ///<summary>viewport resolution (in pixels)</summary>
        public Vector3 iResolution;

        ///<summary>shader playback time (in seconds)</summary>
        public float iTime;

        public float iGlobalTime;

        ///<summary>mouse pixel coords. xy: current (if MLB down), zw: click</summary>
        public Vector4 iMouse;

        ///<summary>(year, month, day, time in seconds)</summary>
        public Vector4 iDate;

        ///<summary>sound sample rate (i.e., 44100)</summary>
        public float iSampleRate;

        ///<summary>channel resolution (in pixels)</summary>
        public Vector3[] iChannelResolution = new Vector3[4];

        ///<summary>channel playback time (in seconds)</summary>
        public float[] iChannelTime = new float[4];

        ///<summary>shader playback frame</summary>
        public int iFrame;

        ///<summary>render time (in seconds)</summary>
        public float iTimeDelta;

        public float iFrameRate;

        public RenderData() {
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