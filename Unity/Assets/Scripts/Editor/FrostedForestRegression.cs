using System;
using UnityEngine;
namespace Mektra.Editor
{
    public static class FrostedForestRegression
    {
        public static void Run()
        {
            var shader=Shader.Find("Hidden/EfficientBlur");
            if(shader==null||!shader.isSupported)throw new Exception("Forest blur shader is unavailable");
            var source=new Texture2D(32,32,TextureFormat.RGBA32,false,true);var pixels=new Color[1024];
            for(int y=0;y<32;y++)for(int x=0;x<32;x++)pixels[y*32+x]=x<16?Color.black:Color.white;
            source.wrapMode=TextureWrapMode.Clamp;source.SetPixels(pixels);source.Apply();
            var b=RenderTexture.GetTemporary(32,32,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);
            b.wrapMode=TextureWrapMode.Clamp;
            var previous=RenderTexture.active;var readback=new Texture2D(32,32,TextureFormat.RGBA32,false,true);
            try{
                ForestGlass.Render(source,b);
                RenderTexture.active=b;readback.ReadPixels(new Rect(0,0,32,32),0,0);readback.Apply();
                float edge=readback.GetPixel(15,16).r;
                if(edge<=.1f||edge>=.9f||readback.GetPixel(0,16).r>.05f||readback.GetPixel(31,16).r<.95f)throw new Exception("Forest blur loses image contrast or does not soften edges: "+edge+" / "+readback.GetPixel(0,16).r+" / "+readback.GetPixel(31,16).r);
            }finally{RenderTexture.active=previous;RenderTexture.ReleaseTemporary(b);UnityEngine.Object.DestroyImmediate(source);UnityEngine.Object.DestroyImmediate(readback);}
            Debug.Log("MEKTRA_FROSTED_FOREST_PASS: Translucent Image GPU blur softens edges while preserving dark and light fields.");
        }
    }
}
