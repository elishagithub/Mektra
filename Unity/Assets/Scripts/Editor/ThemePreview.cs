using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace Mektra.Editor
{
    public static class ThemePreview
    {
        // Render authored shader scenery offscreen for visual review, without starting a run.
        public static void Render()
        {
            var cameraNode=new GameObject("Theme review camera");var camera=cameraNode.AddComponent<Camera>();
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
            camera.orthographic=true;camera.orthographicSize=408;camera.transform.position=new Vector3(0,-408,-100);camera.farClipPlane=200;camera.allowHDR=false;
            var sky=GameObject.CreatePrimitive(PrimitiveType.Quad);sky.transform.position=new Vector3(0,-408,20);sky.transform.localScale=new Vector3(390,816,1);
            var material=new Material(Resources.Load<Shader>("Shaders/Backdrop"));sky.GetComponent<MeshRenderer>().sharedMaterial=material;
            var trees=new GameObject("Theme review forest").AddComponent<ParallaxForest>();
            typeof(ParallaxForest).GetMethod("Awake",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(trees,null);
            trees.Refresh(0,816,0);
            Directory.CreateDirectory("ThemePreviews");
            var target=new RenderTexture(390,816,24);camera.targetTexture=target;
            for(int i=0;i<6;i++)
            {
                var p=ThemeJourney.Sample(i*180);
                material.SetColor("_Top",p.top);material.SetColor("_Bottom",p.bottom);material.SetColor("_Light",p.light);
                material.SetFloat("_Landscape",p.landscape);material.SetFloat("_Night",p.night);material.SetFloat("_Dunes",p.dunes);material.SetFloat("_Clock",12);
                trees.SetTheme(p.forest,p.accent);camera.Render();RenderTexture.active=target;
                var image=new Texture2D(390,816,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,390,816),0,0);image.Apply();
                File.WriteAllBytes("ThemePreviews/"+ThemeJourney.Names[i]+".png",image.EncodeToPNG());Object.DestroyImmediate(image);
            }
            RenderTexture.active=null;camera.targetTexture=null;target.Release();Object.DestroyImmediate(target);
            Object.DestroyImmediate(cameraNode);Object.DestroyImmediate(sky);Object.DestroyImmediate(trees.gameObject);
            Debug.Log("MEKTRA_THEME_PREVIEWS_SUCCESS");
        }
    }
}
