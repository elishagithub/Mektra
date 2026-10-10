using System;
using UnityEngine;
namespace Mektra
{
    // Progress drives a continuous palette blend; pause and backtracking cannot flicker themes.
    public class ThemeJourney : MonoBehaviour
    {
        public struct Palette
        {
            public Color top,bottom,light,accent;
            public float forest,landscape,night,dunes,snow,rain;
            public Palette(string t,string b,string l,string a,float f,float mountains,float n,float d,float s,float r)
            {
                top=VectorArt.ColorOf(t);bottom=VectorArt.ColorOf(b);light=VectorArt.ColorOf(l);accent=VectorArt.ColorOf(a);
                forest=f;landscape=mountains;night=n;dunes=d;snow=s;rain=r;
            }
        }
        public static readonly string[] Names={"EMBER FOREST","ALPINE DAWN","VIOLET DUSK","AURORA NIGHT","STORMWOOD","GOLDEN DUNES"};
        static readonly Palette[] Themes={
            new Palette("#674627","#06242A","#B4813D","#69D8E8",1,0,0,0,0,0),
            new Palette("#A6747B","#294F66","#F4BDA0","#B2ECFF",.18f,1,0,0,1,0),
            new Palette("#593F78","#152A49","#E98B8E","#C0A0FF",.3f,1,.35f,0,.25f,0),
            new Palette("#112849","#073E44","#4C92B6","#77FFE0",.6f,.8f,1,0,.4f,0),
            new Palette("#263743","#101C2D","#61799D","#8CBFFF",.9f,.6f,.45f,0,0,1),
            new Palette("#A16852","#433342","#FFC48C","#FFD8A5",.08f,1,0,1,0,0)
        };
        public static Palette Sample(double meters)
        {
            double stage=Math.Max(0,meters)/180;
            int index=(int)(Math.Floor(stage)%Themes.Length);
            float u=Mathf.Clamp01(((float)(stage-Math.Floor(stage))-.66f)/.34f);
            u=u*u*u*(u*(u*6-15)+10); // Smootherstep has zero velocity and acceleration at both ends.
            var a=Themes[index];var b=Themes[(index+1)%Themes.Length];
            return new Palette{top=Color.Lerp(a.top,b.top,u),bottom=Color.Lerp(a.bottom,b.bottom,u),light=Color.Lerp(a.light,b.light,u),accent=Color.Lerp(a.accent,b.accent,u),forest=Mathf.Lerp(a.forest,b.forest,u),landscape=Mathf.Lerp(a.landscape,b.landscape,u),night=Mathf.Lerp(a.night,b.night,u),dunes=Mathf.Lerp(a.dunes,b.dunes,u),snow=Mathf.Lerp(a.snow,b.snow,u),rain=Mathf.Lerp(a.rain,b.rain,u)};
        }
        public static string Name(double meters)=>Names[(int)(Math.Floor(Math.Max(0,meters)/180)%Names.Length)];
        ParticleSystem weather;
        double clock;
        public void Initialize(Material particleMaterial)
        {
            var node=new GameObject("Bounded atmosphere weather");node.transform.SetParent(transform);
            weather=node.AddComponent<ParticleSystem>();weather.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=weather.main;main.loop=true;main.playOnAwake=false;main.maxParticles=100;main.startSpeed=0;main.simulationSpace=ParticleSystemSimulationSpace.Local;
            var emission=weather.emission;emission.enabled=false;var shape=weather.shape;shape.enabled=false;
            var renderer=weather.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=particleMaterial;renderer.sortingOrder=0;
            weather.Play();
        }
        public Palette Refresh(Simulation sim,Material sky,ParallaxForest forest,Vector3 camera)
        {
            var p=Sample(sim.ThemeDistance);
            sky.SetColor("_Top",p.top);sky.SetColor("_Bottom",p.bottom);sky.SetColor("_Light",p.light);
            sky.SetFloat("_Landscape",p.landscape);sky.SetFloat("_Night",p.night);sky.SetFloat("_Dunes",p.dunes);
            sky.SetFloat("_Travel",(float)((sim.RenderCamera*.000035)%100));sky.SetFloat("_Clock",(float)sim.time);
            forest.SetTheme(p.forest,p.accent);
            weather.transform.position=new Vector3(0,camera.y,8);
            if(sim.phase==Phase.Paused){weather.Pause();return p;}
            if(weather.isPaused)weather.Play();
            clock+=Math.Min(Time.unscaledDeltaTime,.05);
            float intensity=p.snow+p.rain;
            int count=Math.Min(5,(int)(clock*24*intensity));
            if(count>0)clock=0;
            for(int i=0;i<count;i++)
            {
                bool rain=UnityEngine.Random.value*intensity<p.rain;
                weather.Emit(new ParticleSystem.EmitParams{position=new Vector3(UnityEngine.Random.Range(-205,205), (float)sim.viewport/2+10,0),velocity=new Vector3(rain?-22:UnityEngine.Random.Range(-8,8),rain?-240:-24,0),startLifetime=rain?4:35,startSize=rain?2.4f:UnityEngine.Random.Range(2,4),startColor=new Color(.7f,.86f,1,rain?.25f:.4f)},1);
            }
            return p;
        }
    }
}
