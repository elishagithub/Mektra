using UnityEngine;
namespace Mektra
{
    // A whole continuous outline is one mesh particle, so expansion never separates into dots.
    public sealed class ShapeOutlineRipple:MonoBehaviour
    {
        ParticleSystem particles;Mesh mesh;Material material;
        Simulation sim;double origin;
        static Color Hue(Color sky){float h,s,v;Color.RGBToHSV(sky,out h,out s,out v);return Color.HSVToRGB(h,Mathf.Clamp(s,.22f,.55f),.88f);}
        public void Initialize(Simulation simulation,double renderOrigin)
        {
            sim=simulation;origin=renderOrigin;var platform=sim.platform;var data=platform.data;var points=data.outline;int n=points.Length;
            var center=new Point((data.bounds.minX+data.bounds.maxX)*.5,(data.bounds.minY+data.bounds.maxY)*.5);
            var vertices=new Vector3[n*2];var uv=new Vector2[n*2];var colors=new Color[n*2];var indices=new int[n*6];
            for(int i=0;i<n;i++){
                var tangent=(points[(i+1)%n]-points[(i+n-1)%n]).Normal;var normal=new Point(-tangent.y,tangent.x);var local=points[i]-center;
                for(int side=0;side<2;side++){
                    var p=local+normal*(side==0?-4.5:4.5);vertices[i*2+side]=new Vector3((float)(p.x*(platform.mirrored?-1:1)),(float)-p.y,0);uv[i*2+side]=new Vector2(i/(float)n,side);
                    colors[i*2+side]=Color.white;
                }
                int a=i*2,b=((i+1)%n)*2,k=i*6;indices[k]=a;indices[k+1]=b;indices[k+2]=a+1;indices[k+3]=a+1;indices[k+4]=b;indices[k+5]=b+1;
            }
            mesh=new Mesh{name="Continuous shape ripple"};mesh.vertices=vertices;mesh.uv=uv;mesh.colors=colors;mesh.triangles=indices;mesh.RecalculateBounds();
            material=new Material(Resources.Load<Shader>("Shaders/HolographicOutline"));material.SetFloat("_Strength",.55f);
            particles=gameObject.AddComponent<ParticleSystem>();particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=particles.main;main.loop=false;main.playOnAwake=false;main.maxParticles=2;main.startSpeed=0;main.startSize=1;main.startLifetime=1.35f;main.simulationSpace=ParticleSystemSimulationSpace.Local;
            var emission=particles.emission;emission.enabled=false;var shape=particles.shape;shape.enabled=false;
            var size=particles.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,1),new Keyframe(1,1.42f)));
            var fade=particles.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(.75f,.35f),new GradientAlphaKey(0,1)});fade.color=gradient;
            var renderer=particles.GetComponent<ParticleSystemRenderer>();renderer.renderMode=ParticleSystemRenderMode.Mesh;renderer.mesh=mesh;renderer.sharedMaterial=material;renderer.sortingOrder=4;
            var motion=platform.MotionAt(sim.RenderTime);transform.position=new Vector3((float)((platform.mirrored?390-center.x:center.x)+motion.x-195),(float)(origin-center.y-platform.offset-motion.y),0);
            particles.Play();particles.Emit(new ParticleSystem.EmitParams{startLifetime=1.05f,startColor=Color.white},1);particles.Emit(new ParticleSystem.EmitParams{startLifetime=1.5f,startColor=new Color(1,1,1,.9f)},1);
        }
        void Update(){if(particles==null)return;double next=sim.platform.offset;if(next!=origin){transform.position+=new Vector3(0,(float)(next-origin),0);origin=next;}if(sim.phase==Phase.Paused){if(!particles.isPaused)particles.Pause();}else{if(particles.isPaused)particles.Play();if(!particles.IsAlive())Destroy(gameObject);}}
        void OnDestroy(){if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);}
    }
}
