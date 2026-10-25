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
            public float forest,landscape,night,dunes,snow,rain,plains;
            public Palette(string t,string b,string l,string a,float f,float mountains,float n,float d,float s,float r,float flat=0)
            {
                top=VectorArt.ColorOf(t);bottom=VectorArt.ColorOf(b);light=VectorArt.ColorOf(l);accent=VectorArt.ColorOf(a);
                forest=f;landscape=mountains;night=n;dunes=d;snow=s;rain=r;plains=flat;
            }
        }
        public static readonly string[] Names={"MOONLIT DUNES","BLUE HIGHLANDS","VIOLET NIGHT","AURORA NIGHT","MIDNIGHT PEAKS","EMBER DUNES","MOONLIT PLAINS"};
        static readonly Palette[] Themes={
            new Palette("#535879","#B38486","#E6C6A8","#69D8E8",0,1,1,1,0,0),
            new Palette("#405B7E","#8CA8B9","#D3DCE2","#7DBFF4",0,1,1,0,0,0),
            new Palette("#55506F","#A38EAC","#D4BEDB","#C0A0FF",0,1,1,0,0,0),
            new Palette("#13344C","#062D37","#4B9C9C","#77FFE0",0,.4f,1,0,0,0),
            new Palette("#182C50","#08162E","#6587B6","#8CBFFF",0,1,1,0,0,0),
            new Palette("#47283E","#181C39","#AD6651","#EDB28C",0,1,1,1,0,0),
            new Palette("#203D4D","#0B2033","#638EAB","#B0D2EA",0,1,1,1,0,0,1)
        };
        public static Palette Sample(double meters)
        {
            double stage=Math.Max(0,meters)/180;
            int index=(int)(Math.Floor(stage)%Themes.Length);
            float u=Mathf.Clamp01(((float)(stage-Math.Floor(stage))-.35f)/.65f);
            u=u*u*u*(u*(u*6-15)+10); // Smootherstep has zero velocity and acceleration at both ends.
            var a=Themes[index];var b=Themes[(index+1)%Themes.Length];
            return new Palette{top=Color.Lerp(a.top,b.top,u),bottom=Color.Lerp(a.bottom,b.bottom,u),light=Color.Lerp(a.light,b.light,u),accent=Color.Lerp(a.accent,b.accent,u),forest=Mathf.Lerp(a.forest,b.forest,u),landscape=Mathf.Lerp(a.landscape,b.landscape,u),night=Mathf.Lerp(a.night,b.night,u),dunes=Mathf.Lerp(a.dunes,b.dunes,u),snow=Mathf.Lerp(a.snow,b.snow,u),rain=Mathf.Lerp(a.rain,b.rain,u),plains=Mathf.Lerp(a.plains,b.plains,u)};
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
            var p=Sample((sim.startY-sim.bestY)/10);
#if MEKTRA_INVERT_PREVIEW
            // Landscape palette follows progress; LUT selection is independent.
            sky.SetFloat("_Abstract",ColorPresetPreview.AbstractMode);
#endif
            sky.SetColor("_Top",p.top);sky.SetColor("_Bottom",p.bottom);sky.SetColor("_Light",p.light);
            sky.SetFloat("_Landscape",p.landscape);sky.SetFloat("_Plains",p.plains);sky.SetFloat("_Night",p.night);sky.SetFloat("_Dunes",p.dunes);
            sky.SetFloat("_Travel",(float)((sim.RenderCamera*.000035)%100));sky.SetFloat("_Clock",(float)sim.time);
            forest.SetTheme(p.forest,p.accent);
            weather.transform.position=new Vector3(0,camera.y,8);
            if(sim.phase==Phase.Paused){weather.Pause();return p;}
            if(weather.isPaused)weather.Play();
            clock+=Math.Min(Time.unscaledDeltaTime,.05);
            #if MEKTRA_INVERT_PREVIEW
            float intensity=0; // Theme experiment: restrained, continuously falling luminous dust.
#else
            float intensity=0;
#endif
            int count=Math.Min(5,(int)(clock*24*intensity));
            if(count>0)clock=0;
            for(int i=0;i<count;i++)
            {
                bool rain=UnityEngine.Random.value*intensity<p.rain;
                weather.Emit(new ParticleSystem.EmitParams{position=new Vector3(UnityEngine.Random.Range(-205,205), (float)sim.viewport/2+10,0),velocity=new Vector3(rain?-22:UnityEngine.Random.Range(-12,12),rain?-160:UnityEngine.Random.Range(-52,-32),0),startLifetime=rain?7:24,startSize=rain?2.4f:UnityEngine.Random.Range(2,4),startColor=new Color(.7f,.86f,1,rain?.2f:.28f)},1);
            }
            return p;
        }
    }
}
