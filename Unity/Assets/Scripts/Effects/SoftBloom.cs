using UnityEngine;
namespace Mektra
{
    // A small built-in render-pipeline bloom pass; no URP or sample packages.
    [RequireComponent(typeof(Camera))]
    public class SoftBloom : MonoBehaviour
    {
        Material bloom;
        void Awake(){GetComponent<Camera>().allowHDR=true;bloom=new Material(Resources.Load<Shader>("Shaders/Bloom"));}
        void OnRenderImage(RenderTexture source,RenderTexture destination)
        {
            if(bloom==null){Graphics.Blit(source,destination);return;}
            int width=Mathf.Max(1,source.width/2),height=Mathf.Max(1,source.height/2);
            var a=RenderTexture.GetTemporary(width,height,0,RenderTextureFormat.ARGBHalf);
            var b=RenderTexture.GetTemporary(width,height,0,RenderTextureFormat.ARGBHalf);
            Graphics.Blit(source,a,bloom,0);
            for(int i=0;i<3;i++)
            {
                bloom.SetVector("_Direction",new Vector4(1+i,0,0,0));Graphics.Blit(a,b,bloom,1);
                bloom.SetVector("_Direction",new Vector4(0,1+i,0,0));Graphics.Blit(b,a,bloom,1);
            }
            bloom.SetTexture("_BloomTex",a);bloom.SetFloat("_Strength",.8f);Graphics.Blit(source,destination,bloom,2);
            RenderTexture.ReleaseTemporary(a);RenderTexture.ReleaseTemporary(b);
        }
        void OnDestroy(){if(bloom!=null)Destroy(bloom);}
    }
}
