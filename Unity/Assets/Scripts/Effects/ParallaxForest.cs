using System.Collections.Generic;
using UnityEngine;
namespace Mektra
{
    // Original procedural silhouettes: three quiet depths of sparse, distant branch silhouettes.
    // Decorative meshes never participate in gameplay collision.
    public class ParallaxForest : MonoBehaviour
    {
        const float TileHeight=1200;
        readonly List<Transform[]> layers=new List<Transform[]>();
        readonly float[] speeds={.018f,.042f,.075f};
        Material material;
        public void SetTheme(float density,Color accent)
        {
            material.SetColor("_Tint",new Color(accent.r*.65f+.35f,accent.g*.65f+.35f,accent.b*.65f+.35f,density));
        }
        void Awake()
        {
            material=new Material(Resources.Load<Shader>("Shaders/Forest"));
            for(int layer=0;layer<3;layer++)
            {
                var mesh=Build(layer);var tiles=new Transform[3];
                for(int tile=0;tile<3;tile++)
                {
                    var node=new GameObject("Forest depth "+layer+" tile "+tile);node.transform.SetParent(transform);
                    node.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=node.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.enabled=false;tiles[tile]=node.transform;
                }
                layers.Add(tiles);
            }
        }
        public void Refresh(double camera,float viewport,double origin)
        {
            for(int layer=0;layer<layers.Count;layer++)
            {
                float y=(float)((camera*speeds[layer]%TileHeight+TileHeight)%TileHeight)-TileHeight-viewport/2;
                for(int tile=0;tile<3;tile++)layers[layer][tile].position=new Vector3(0,y+tile*TileHeight+(float)(origin-camera-viewport/2),16-layer*3);
            }
        }
        Mesh Build(int layer)
        {
            var random=new System.Random(443+layer*89);var vertices=new List<Vector3>();var colors=new List<Color>();var triangles=new List<int>();
            Color color=layer==0?new Color(.25f,.35f,.34f,.12f):layer==1?new Color(.08f,.18f,.19f,.22f):new Color(.015f,.045f,.055f,.34f);
            System.Action<Vector2,Vector2,float,float> branch=(a,b,wa,wb)=>
            {
                var d=(b-a).normalized;var n=new Vector2(-d.y,d.x);int start=vertices.Count;
                foreach(var p in new[]{a+n*wa,b+n*wb,b-n*wb,a-n*wa}){vertices.Add(p);colors.Add(color);}
                triangles.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});
            };
            for(int side=-1;side<=1;side+=2)for(int tree=0;tree<3;tree++)
            {
                float rootX=side*(202+(float)random.NextDouble()*34),rootY=tree*400-50,height=300+(float)random.NextDouble()*180;
                Vector2 prior=new Vector2(rootX,rootY);
                for(int segment=1;segment<=10;segment++)
                {
                    float t=segment/10f;var next=new Vector2(rootX+Mathf.Sin(t*2.7f+tree)*14,rootY+t*height);
                    branch(prior,next,(12-layer*2)*(1-t+.1f),(10-layer*2)*(1-t+.1f));prior=next;
                    if(segment%4==0)
                    {
                        float reach=14+(float)random.NextDouble()*18;var tip=next+new Vector2(-side*reach,30+(float)random.NextDouble()*30);
                        branch(next,tip,1.8f,.35f);
                    }
                }
            }
            var mesh=new Mesh{name="Mektra original forest layer "+layer};mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();return mesh;
        }
    }
}
