using System;
using UnityEngine;
namespace Mektra
{
    // Independent articulated meshes, rendered into an alpha texture for gameplay and menus.
    public sealed class AvatarRig:IDisposable
    {
        static int serial;
        readonly GameObject root;readonly Transform body;readonly Transform[] feet=new Transform[4],eyes=new Transform[2],joints=new Transform[4],feathers=new Transform[2];
        readonly Camera camera;readonly Material shell,eye,glow;Material rim,ornament;readonly Vector3[] footHomes=new Vector3[4];
        public readonly RenderTexture Texture;
        double last=-1;bool disposed;readonly Color accent;
        public AvatarRig(Character character,int resolution=256)
        {
            accent=Tint(character.id);
            root=new GameObject("Mektra articulated avatar "+character.id);root.transform.position=new Vector3(10000+(serial++%1000)*12,0,0);
            shell=Material(character.id=="frost"?new Color(.68f,.76f,.82f):new Color(.008f,.014f,.018f),false);
            eye=Material(Color.Lerp(accent,Color.white,.6f),true);ornament=Material(accent,false);glow=new Material(Resources.Load<Shader>("Shaders/AvatarHalo"));glow.SetColor("_Color",new Color(accent.r,accent.g,accent.b,.6f));
            body=new GameObject("Body and face").transform;body.SetParent(root.transform,false);
            Part("Glossy body",body,Vector3.zero,new Vector3(1.55f,1.55f,1.5f),shell);
            for(int i=0;i<2;i++){
                eyes[i]=Part("Eye "+i,body,new Vector3(i==0?-.255f:.255f,.22f,-.716f),new Vector3(.275f,.31f,.13f),eye);
                Halo(body,eyes[i].localPosition+new Vector3(0,0,-.08f),.8f,glow);
            }
            for(int i=0;i<4;i++){
                float side=i%2==0?-1:1;footHomes[i]=new Vector3(side*(i<2?.70f:.49f),i<2?-.48f:-.77f,i<2?-.14f:.05f);
                feet[i]=Part("Articulated foot "+i,body,footHomes[i],new Vector3(.30f,.31f,.30f),shell);
                joints[i]=Part("Leg joint "+i,body,footHomes[i]*.82f,new Vector3(.19f,.30f,.19f),shell);
            }
            if(character.id=="ember"||character.id=="aurora"||character.id=="frost"||character.id=="bloom")for(int i=0;i<2;i++){
                var leaf=Part("Leaf antenna "+i,body,new Vector3(.43f+i*.14f,.88f+i*.1f,0),new Vector3(.12f,.40f,.05f),ornament);leaf.localRotation=Quaternion.Euler(0,0,-30-i*18);feathers[i]=leaf;
            }
            rim=new Material(glow);rim.SetColor("_Color",new Color(.12f,.8f,1,.28f));Halo(body,new Vector3(0,0,.9f),2.7f,rim);
            var cameraNode=new GameObject("Avatar alpha camera");cameraNode.transform.SetParent(root.transform,false);camera=cameraNode.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=1.6f;camera.transform.localPosition=new Vector3(0,.08f,-6);camera.transform.localRotation=Quaternion.identity;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.cullingMask=1<<30;camera.nearClipPlane=.1f;camera.farClipPlane=15;camera.allowHDR=false;camera.enabled=false;
            Texture=new RenderTexture(resolution,resolution,24,RenderTextureFormat.ARGB32){name="Articulated "+character.id,antiAliasing=4,filterMode=FilterMode.Bilinear};Texture.Create();camera.targetTexture=Texture;Render(0,false,false,0,true);
        }
        Material Material(Color colour,bool luminous){var m=new Material(Resources.Load<Shader>("Shaders/AvatarSurface"));m.SetColor("_Color",colour);m.SetColor("_Accent",new Color(.08f,.75f,1));m.SetFloat("_Unlit",luminous?1:0);return m;}
        static Transform Part(string name,Transform parent,Vector3 position,Vector3 scale,Material material){var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);go.name=name;go.layer=30;Kill(go.GetComponent<Collider>());go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;go.GetComponent<MeshRenderer>().sharedMaterial=material;return go.transform;}
        static void Halo(Transform parent,Vector3 position,float size,Material material){var go=GameObject.CreatePrimitive(PrimitiveType.Quad);go.name="Soft light bloom";go.layer=30;Kill(go.GetComponent<Collider>());go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=Vector3.one*size;go.GetComponent<MeshRenderer>().sharedMaterial=material;}
        public static Color Tint(string id)
        {
            switch(id){case "tide":return new Color(.05f,.8f,1);case "ember":return new Color(1,.55f,.08f);case "aurora":return new Color(.72f,.34f,1);case "frost":return new Color(.77f,.91f,1);case "volt":return new Color(.7f,1,.1f);case "nova":return new Color(1,.35f,.65f);case "bloom":return new Color(1,.6f,.8f);case "onyx":return new Color(.66f,.77f,1);case "comet":return new Color(.25f,1,.68f);default:return new Color(1,1,.55f);}
        }
        public void Render(double seconds,bool walking,bool flying,float happy,bool force=false,float look=0)
        {
            if(disposed||(!force&&seconds-last<1.0/45&&seconds>=last))return;last=seconds;float t=(float)seconds;
            float settle=last<0?1:1-Mathf.Exp(-16*Mathf.Clamp(Time.unscaledDeltaTime,.016f,.05f));
            float breath=1+Mathf.Sin(t*1.6f)*.018f;body.localScale=new Vector3(breath+(happy>0?.025f:0),1/breath,breath);body.localPosition=new Vector3(0,walking?Mathf.Abs(Mathf.Sin(t*9))*.025f:Mathf.Sin(t*1.6f)*.025f,0);body.localRotation=Quaternion.Euler(Mathf.Sin(t*.6f)*3,Mathf.Sin(t*.45f)*5+look*5, flying?-7:Mathf.Sin(t*.7f)*2);
            float blinkPhase=Mathf.Repeat(t+1.5f,4.7f);float blink=blinkPhase<.13f?Mathf.Max(.08f,Mathf.Abs(blinkPhase-.065f)/.065f):1;
            for(int i=0;i<2;i++){var scale=eyes[i].localScale;scale.y=.31f*blink*(happy>0?.85f:1);eyes[i].localScale=scale;var at=eyes[i].localPosition;at.x=(i==0?-.255f:.255f)+Mathf.Sin(t*.9f)*.015f+look*.035f;eyes[i].localPosition=at;}
            for(int i=0;i<4;i++){var home=footHomes[i];float step=walking?Mathf.Sin(t*11+i*Mathf.PI*.5f):Mathf.Sin(t*1.5f+i)*.12f;feet[i].localPosition=Vector3.Lerp(feet[i].localPosition,home+new Vector3(flying?-Mathf.Sign(home.x)*.10f:0,flying?.17f:Mathf.Max(0,step)*.12f,walking?step*.11f:0),force?1:settle);feet[i].localRotation=Quaternion.Euler(step*12,0,step*5);var anchor=home*.65f;var limb=feet[i].localPosition-anchor;joints[i].localPosition=(feet[i].localPosition+anchor)*.5f;joints[i].localRotation=Quaternion.FromToRotation(Vector3.up,limb);joints[i].localScale=new Vector3(.17f,limb.magnitude+.1f,.17f);}
            for(int i=0;i<feathers.Length;i++)if(feathers[i]!=null)feathers[i].localRotation=Quaternion.Euler(0,0,-30-i*18+Mathf.Sin(t*2+i)*5);
            camera.Render();
        }
        public float EyeOpenness=>eyes[0].localScale.y/.31f;
        public Vector3 FootPosition(int index)=>feet[index].localPosition;
        static void Kill(UnityEngine.Object item){if(Application.isPlaying)UnityEngine.Object.Destroy(item);else UnityEngine.Object.DestroyImmediate(item);}
        public void Dispose(){if(disposed)return;disposed=true;Texture.Release();Kill(Texture);Kill(shell);Kill(eye);Kill(glow);Kill(rim);Kill(ornament);Kill(root);}
    }
}
