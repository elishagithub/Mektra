using LeTai.Asset.TranslucentImage;
using UnityEngine;
using UnityEngine.Rendering;
namespace Mektra
{
    // Cache the vendor blur engine; forest textures only need processing once at startup.
    public static class ForestGlass
    {
        static ScalableBlur blur;
        static ScalableBlurConfig config;
        public static void Render(Texture source,RenderTexture target)
        {
            if(blur==null){
                config=ScriptableObject.CreateInstance<ScalableBlurConfig>();
                config.hideFlags=HideFlags.HideAndDontSave;
                config.Mode=ScalableBlurConfig.BlurMode.Balanced;
                config.Strength=8;
                blur=new ScalableBlur();blur.Init(config,true);
            }
            config.ReferenceResolution=new Vector2(target.width,target.height);
            var cmd=new CommandBuffer{name="Mektra Translucent Image forest glass"};
            var input=Shader.PropertyToID("MektraForestGlassInput");
            var desc=target.descriptor;desc.depthBufferBits=0;desc.msaaSamples=1;
            cmd.GetTemporaryRT(input,desc,FilterMode.Bilinear);cmd.Blit(source,input);
            int count=blur.GetScratchesCount(target.width,target.height);
            for(int i=0;i<count;i++){
                blur.GetNextScratchDescriptor(ref desc);
                int scratch=Shader.PropertyToID("MektraForestGlassScratch"+i);
                cmd.GetTemporaryRT(scratch,desc,FilterMode.Bilinear);blur.SetScratch(i,scratch);
            }
            try{
                blur.Blur(cmd,input,new Rect(0,0,1,1),new Rect(0,0,1,1),new BackgroundFill(),target);
                for(int i=0;i<count;i++)cmd.ReleaseTemporaryRT(Shader.PropertyToID("MektraForestGlassScratch"+i));
                cmd.ReleaseTemporaryRT(input);Graphics.ExecuteCommandBuffer(cmd);
            }finally{cmd.Release();}
        }
    }
}
