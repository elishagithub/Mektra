using System.Collections.Generic;
using UnityEngine;
namespace Mektra
{
    // Three separate filaments follow the same reveal as the visible UI outline.
    public sealed class HolographicOutlines:MonoBehaviour
    {
        readonly Dictionary<string,LineRenderer[]> lines=new Dictionary<string,LineRenderer[]>();
        readonly HashSet<string> visible=new HashSet<string>();
        readonly List<string> obsolete=new List<string>();
        readonly List<Vector3> vertices=new List<Vector3>();
        Material material;
        Simulation current;
        void Awake(){material=new Material(Resources.Load<Shader>("Shaders/HolographicOutline"));}
        static Color Light(Color sky){float h,s,v;Color.RGBToHSV(sky,out h,out s,out v);return Color.HSVToRGB(h,Mathf.Clamp(s,.55f,.8f),1f);}
        public void Refresh(Simulation sim,double origin)
        {
            if(current!=sim){foreach(var group in lines.Values)foreach(var line in group)Destroy(line.gameObject);lines.Clear();current=sim;}
            visible.Clear();
            foreach(var platform in sim.platforms)
            {
                var shape=platform.data;
                if(platform.vanished||shape.bounds.maxY+platform.offset<sim.RenderCamera-160||shape.bounds.minY+platform.offset>sim.RenderCamera+sim.viewport+160)continue;
                if(visible.Count>=24)break;visible.Add(platform.key);
                if(!lines.TryGetValue(platform.key,out var group)){
                    group=new LineRenderer[3];
                    for(int part=0;part<3;part++){
                        var node=new GameObject("Holographic filament "+part);node.transform.SetParent(transform);
                        var line=node.AddComponent<LineRenderer>();line.sharedMaterial=material;line.useWorldSpace=true;line.widthMultiplier=12;line.numCapVertices=0;line.numCornerVertices=0;line.sortingOrder=2;line.textureMode=LineTextureMode.Stretch;group[part]=line;
                    }
                    lines[platform.key]=group;
                }
                WorldView.OutlineVisibility.TryGetValue(platform.key,out float reveal);
                int n=shape.outline.Length;if(n<3)continue;
                int top=0;for(int i=1;i<n;i++)if(shape.outline[i].y<shape.outline[top].y)top=i;
                var motion=platform.MotionAt(sim.RenderTime);
                for(int part=0;part<3;part++){
                    int first=part*n/3,last=(part+1)*n/3;double total=0;
                    for(int i=first;i<last;i++)total+=(shape.outline[(top+i+1)%n]-shape.outline[(top+i)%n]).Length;
                    double remaining=total*Mathf.Clamp01(reveal*1.15f-part*.075f);vertices.Clear();
                    for(int i=first;i<last&&remaining>0;i++){
                        var a=shape.outline[(top+i)%n];var b=shape.outline[(top+i+1)%n];double length=(b-a).Length;
                        if(vertices.Count==0)vertices.Add(Map(a,platform,origin,motion));
                        vertices.Add(Map(a+(b-a)*(System.Math.Min(length,remaining)/System.Math.Max(.001,length)),platform,origin,motion));remaining-=length;
                    }
                    var line=group[part];line.enabled=vertices.Count>1;if(!line.enabled)continue;
                    line.positionCount=vertices.Count;line.SetPositions(vertices.ToArray());line.Simplify(.5f);
                    // Raw sky hues are graded by the camera alongside the background.
                    var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Light(new Color(.09f,.065f,.16f)),0),new GradientColorKey(Light(new Color(.035f,.065f,.12f)),1)},new[]{new GradientAlphaKey(1f,0),new GradientAlphaKey(1f,1)});line.colorGradient=gradient;
                }
            }
            obsolete.Clear();foreach(var key in lines.Keys)if(!visible.Contains(key))obsolete.Add(key);
            foreach(var key in obsolete){foreach(var line in lines[key])Destroy(line.gameObject);lines.Remove(key);WorldView.OutlineVisibility.Remove(key);}
        }
        static Vector3 Map(Point p,PlatformInstance platform,double origin,Point motion)=>new Vector3((float)((platform.mirrored?390-p.x:p.x)+motion.x-195),(float)(origin-p.y-platform.offset-motion.y),0);
        void OnDestroy(){if(material!=null)Destroy(material);}
    }
}
