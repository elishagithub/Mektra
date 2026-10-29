using System;
using UnityEngine;
namespace Mektra
{
    // Independent articulated meshes, rendered into an alpha texture for gameplay and menus.
    public sealed class AvatarRig:IDisposable
    {
        static int serial;
        readonly GameObject root;readonly Transform body;readonly Transform[] feet,eyes=new Transform[2],feathers=new Transform[2];
        readonly Camera camera;readonly Material shell,eye,glow;Material ornament;readonly Vector3[] footHomes;readonly bool spider;readonly Transform[] antennae=new Transform[2],wings=new Transform[2];
        public readonly RenderTexture Texture;
        public Vector3[] GroundFeet;
        double last=-1;bool disposed;readonly Color accent;
        public AvatarRig(Character character,int resolution=256)
        {
            accent=Color.Lerp(Tint(character.id),Color.white,.25f);
            spider=character.id=="mektra"||character.id=="onyx";
            int legs=FootCount(character.id);feet=new Transform[legs];footHomes=new Vector3[legs];
            root=new GameObject("Mektra articulated avatar "+character.id);root.transform.position=new Vector3(10000+(serial++%1000)*12,0,0);
            shell=Material(character.id=="frost"?new Color(.68f,.76f,.82f):Color.Lerp(new Color(.015f,.025f,.03f),accent,.12f),false);
            eye=Material(Color.Lerp(accent,Color.white,.6f),true);ornament=Material(accent,false);glow=new Material(Resources.Load<Shader>("Shaders/AvatarHalo"));glow.SetColor("_Color",new Color(accent.r,accent.g,accent.b,.6f));
            body=new GameObject("Body and face").transform;body.SetParent(root.transform,false);
            var bodyShape=character.id=="tide"||character.id=="comet"?new Vector3(1.30f,1.65f,1.4f):character.id=="ember"||character.id=="volt"?new Vector3(1.62f,1.40f,1.5f):new Vector3(1.55f,1.55f,1.5f);
            Part("Glossy bug body",body,Vector3.zero,bodyShape,shell);
            shell.SetColor("_Accent",accent);ornament.SetColor("_Accent",accent);
            for(int i=0;i<2;i++){
                float side=i==0?-1:1;
                antennae[i]=new GameObject("Animated antenna "+i).transform;antennae[i].SetParent(body,false);antennae[i].localPosition=new Vector3(side*.35f,.66f,0);
                Part("Antenna stem",antennae[i],new Vector3(side*.06f,.13f,0),new Vector3(.055f,.35f,.055f),shell);
                Part("Luminous antenna tip",antennae[i],new Vector3(side*.12f,.29f,0),Vector3.one*(spider?.10f:.14f),eye);
            }
            if(character.id=="aurora"||character.id=="frost"||character.id=="bloom"||character.id=="comet")for(int i=0;i<2;i++){
                float side=i==0?-1:1;wings[i]=Part("Soft wing "+i,body,new Vector3(side*.67f,.28f,.45f),new Vector3(.65f,character.id=="comet"?.35f:.95f,.09f),ornament);
            }
            if(character.id=="ember"||character.id=="volt")for(int i=0;i<2;i++)Part("Beetle shell plate "+i,body,new Vector3(i==0?-.37f:.37f,-.22f,-.51f),new Vector3(.66f,.75f,.30f),ornament);
            for(int i=0;i<2;i++){
                eyes[i]=Part("Eye "+i,body,new Vector3(i==0?-.255f:.255f,.22f,-.716f),new Vector3(.275f,.31f,.13f),eye);
                Halo(body,eyes[i].localPosition+new Vector3(0,0,-.08f),.8f,glow);
            }
            for(int i=0;i<feet.Length;i++){
                footHomes[i]=new Vector3(Mathf.Lerp(-.68f,.68f,(float)(i/2)/(feet.Length/2-1)),-.69f,i%2==0?-.38f:.16f);
                feet[i]=Part("Round walking foot "+i,root.transform,footHomes[i],Vector3.one*.36f,shell);
            }
            if(character.id=="ember"||character.id=="aurora"||character.id=="frost"||character.id=="bloom")for(int i=0;i<2;i++){
                var leaf=Part("Leaf antenna "+i,body,new Vector3(.43f+i*.14f,.88f+i*.1f,0),new Vector3(.12f,.40f,.05f),ornament);leaf.localRotation=Quaternion.Euler(0,0,-30-i*18);feathers[i]=leaf;
            }
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
            for(int i=0;i<feet.Length;i++){
                var home=footHomes[i];
                float phase=Mathf.Repeat(t*2.2f+(i%2)*.5f-(i/2)*.13f,1);
                float lift=walking&&phase>.62f?Mathf.Sin((phase-.62f)/.38f*Mathf.PI)*.16f:0;
                var target=GroundFeet!=null&&i<GroundFeet.Length&&!flying?GroundFeet[i]:home;
                if(flying)target=new Vector3(home.x*.72f,home.y+.28f,home.z);
                else target.y+=lift;
                feet[i].localPosition=Vector3.Lerp(feet[i].localPosition,target,force?1:settle);
                feet[i].localRotation=Quaternion.Euler(0,0,walking?phase*360:0);
            }
            for(int i=0;i<2;i++)antennae[i].localRotation=Quaternion.Euler(0,0,Mathf.Sin(t*2.2f+i)*9+(flying?(i==0?15:-15):look*8));
            for(int i=0;i<feathers.Length;i++)if(feathers[i]!=null)feathers[i].localRotation=Quaternion.Euler(0,0,-30-i*18+Mathf.Sin(t*2+i)*5);
            for(int i=0;i<2;i++)if(wings[i]!=null)wings[i].localRotation=Quaternion.Euler(0,Mathf.Sin(t*(flying?25:4)+i*Mathf.PI)*(flying?35:8),(i==0?25:-25));
            camera.Render();
        }
        public static int FootCount(string id)=>4;
        public int LegCount=>feet.Length;
        public float EyeOpenness=>eyes[0].localScale.y/.31f;
        public Vector3 FootPosition(int index)=>feet[index].localPosition;
        static void Kill(UnityEngine.Object item){if(Application.isPlaying)UnityEngine.Object.Destroy(item);else UnityEngine.Object.DestroyImmediate(item);}
        public void Dispose(){if(disposed)return;disposed=true;Texture.Release();Kill(Texture);Kill(shell);Kill(eye);Kill(glow);Kill(ornament);Kill(root);}
    }
}
