using System;
using UnityEngine;
namespace Mektra
{
    // All moving glow, trails and impact effects are native GPU particle systems.
    public class NativeEffects : MonoBehaviour
    {
        Camera gameCamera;
        HolographicOutlines outlines;
        ParticleSystem trail,starBurst,land,crash,dust,pointShimmer,playerShimmer,shockwave,lavaEmbers,shapeRipple,captureRing;
        Material glowMaterial,starMaterial,hologramMaterial,haloMaterial;
        GameObject halo,backdrop;
        ParallaxForest forest;
        ThemeJourney journey;
        Material skyMaterial;
        Texture2D monochrome;
        RenderTexture frostedMoon,frostedEmerald;
        static readonly RenderTexture[] menuThemes=new RenderTexture[3];
        public static Texture MenuBackdrop=>ThemeTexture(PlayerPrefs.GetInt("mektra.background.theme",0));
        public static Texture ThemeTexture(int index)=>menuThemes[Mathf.Clamp(index,0,2)];
        public static void RefreshMenuTheme(){}
        AmplifyColorEffect grading;int gradeVersion=-1;
        float impact;
        Color themeAccent;
        Simulation current;
        double sparkClock,emissionClock,pointClock,shimmerClock,lavaClock;
        Point previousTrail;
        double renderOrigin;
        readonly ParticleSystem.Particle[] rebaseBuffer=new ParticleSystem.Particle[900];
        readonly Color ice=new Color(.28f,.78f,1,1),gold=new Color(1,.72f,.25f,1);
        RenderTexture FrostForest(Texture2D source)
        {
            source.wrapMode=TextureWrapMode.Clamp;source.filterMode=FilterMode.Bilinear;
            const int width=384;int height=Mathf.RoundToInt(width*(float)source.height/source.width);
            var output=new RenderTexture(width,height,0,RenderTextureFormat.ARGB32){name=source.name+" frosted",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};output.Create();
            ForestGlass.Render(source,output);
            return output;
        }
        void OnDestroy()
        {
            if(frostedMoon!=null){frostedMoon.Release();Destroy(frostedMoon);}
            if(frostedEmerald!=null){frostedEmerald.Release();Destroy(frostedEmerald);}
            for(int i=0;i<menuThemes.Length;i++)if(menuThemes[i]!=null){menuThemes[i].Release();Destroy(menuThemes[i]);menuThemes[i]=null;}
        }
        void Awake()
        {
            gameCamera=Camera.main;
            if(gameCamera==null)gameCamera=FindFirstObjectByType<Camera>();
            gameCamera.orthographic=true;gameCamera.nearClipPlane=.1f;gameCamera.farClipPlane=200;
            gameCamera.transform.position=new Vector3(0,0,-100);gameCamera.allowHDR=true;
            // Plain theme: no post-processing glow.
            // The light-world shader supplies its own colour and HDR emission.
            // Remove grading from the camera so it cannot tint or flatten the glow.
            ColorPresetPreview.Initialize();
            grading=gameCamera.gameObject.AddComponent<AmplifyColorEffect>();CameraPlay.CurrentCamera=gameCamera;
            monochrome=new Texture2D(1024,32,TextureFormat.RGB24,false,true);monochrome.wrapMode=TextureWrapMode.Clamp;
            var monoPixels=new Color[1024*32];for(int blue=0;blue<32;blue++)for(int green=0;green<32;green++)for(int red=0;red<32;red++){float grey=(red*.2126f+green*.7152f+blue*.0722f)/31;monoPixels[green*1024+blue*32+red]=new Color(grey,grey,grey);}monochrome.SetPixels(monoPixels);monochrome.Apply();

            glowMaterial=new Material(Resources.Load<Shader>("Shaders/PlainParticle"));glowMaterial.mainTexture=ParticleTexture(false);
            hologramMaterial=new Material(glowMaterial);hologramMaterial.SetFloat("_Iridescence",.85f);
            glowMaterial.SetFloat("_Iridescence",.45f);
            starMaterial=new Material(glowMaterial);starMaterial.mainTexture=ParticleTexture(false);starMaterial.SetFloat("_Iridescence",.15f);
            pointShimmer=System("Holographic point halos",hologramMaterial,110,.35f,34,new Color(1,1,1,.16f));
            playerShimmer=System("Player motes",glowMaterial,40,.35f,2.4f,Color.white);
            playerShimmer.GetComponent<ParticleSystemRenderer>().enabled=false;
            trail=System("White particle trail",glowMaterial,240,.78f,19.435f,new Color(1,1,1,.5f));
            trail.GetComponent<ParticleSystemRenderer>().enabled=false;
            var size=trail.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,1),new Keyframe(.3f,.7f),new Keyframe(1,0)));
            starBurst=System("Collectible star fragments",starMaterial,180,.8f,12,Color.white);
            var spin=starBurst.rotationOverLifetime;spin.enabled=true;spin.z=new ParticleSystem.MinMaxCurve(-5f,5f);
            lavaEmbers=System("Lava embers",glowMaterial,60,1.1f,2,new Color(1,.35f,.06f,.75f));
            shapeRipple=System("Perfect shape outline ripples",glowMaterial,900,.85f,2,new Color(.5f,.85f,1,.65f));
            land=System("Landing sparks",glowMaterial,80,.32f,4,ice);
            crash=System("Mechanical explosion",glowMaterial,160,.7f,7,new Color(1,.35f,.35f,1));
            dust=System("Forest dust",glowMaterial,70,24,1.3f,new Color(.9f,.8f,.55f,.22f));
            var circleMaterial=new Material(glowMaterial);circleMaterial.mainTexture=CircleTexture();
            foreach(var ps in new[]{starBurst,land,crash})ConfigureRoundBurst(ps,circleMaterial);
            // Expanding rings provide a readable impact without covering the robot's face.
            var ringMaterial=new Material(hologramMaterial);ringMaterial.mainTexture=RingTexture();
            shockwave=System("Holographic impact rings",ringMaterial,24,.42f,12,new Color(.4f,.8f,1,.65f));
            var ringSize=shockwave.sizeOverLifetime;ringSize.enabled=true;
            ringSize.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.25f),new Keyframe(1,1)));
            captureRing=System("Firefly capture pulse",ringMaterial,12,.6f,84,new Color(.65f,1,.8f,.8f));
            var captureSize=captureRing.sizeOverLifetime;captureSize.enabled=true;
            captureSize.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.12f),new Keyframe(.4f,.75f),new Keyframe(1,1)));
            var fragments=starBurst.sizeOverLifetime;fragments.enabled=true;
            fragments.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.3f),new Keyframe(.12f,1),new Keyframe(.5f,.65f),new Keyframe(1,0)));
            var drag=starBurst.limitVelocityOverLifetime;drag.enabled=true;drag.limit=100;drag.dampen=.06f;
            haloMaterial=new Material(hologramMaterial);haloMaterial.SetColor("_Tint",new Color(.65f,.85f,1,.3f));
            halo=GameObject.CreatePrimitive(PrimitiveType.Quad);halo.name="Player soft halo";Destroy(halo.GetComponent<Collider>());halo.transform.SetParent(transform);halo.GetComponent<MeshRenderer>().material=haloMaterial;
            backdrop=GameObject.CreatePrimitive(PrimitiveType.Quad);backdrop.name="Forest atmosphere";Destroy(backdrop.GetComponent<Collider>());backdrop.transform.SetParent(transform);backdrop.transform.position=new Vector3(0,0,20);backdrop.GetComponent<MeshRenderer>().material=new Material(Resources.Load<Shader>("Shaders/Backdrop"));
            forest=gameObject.AddComponent<ParallaxForest>();
            skyMaterial=backdrop.GetComponent<MeshRenderer>().material;
            frostedMoon=FrostForest(Resources.Load<Texture2D>("Art/FireflyForest/moonlit-forest"));
            frostedEmerald=FrostForest(Resources.Load<Texture2D>("Art/FireflyForest/emerald-forest"));
            var themeMaterial=new Material(Resources.Load<Shader>("Shaders/ForestTheme"));
            for(int i=0;i<3;i++){menuThemes[i]=new RenderTexture(frostedMoon.width,frostedMoon.height,0){name="Menu forest theme "+i,filterMode=FilterMode.Bilinear};menuThemes[i].Create();themeMaterial.SetFloat("_Theme",i);var previous=RenderTexture.active;Graphics.Blit(frostedMoon,menuThemes[i],themeMaterial);RenderTexture.active=previous;}Destroy(themeMaterial);
            skyMaterial.SetTexture("_ForestA",frostedMoon);
            skyMaterial.SetTexture("_ForestB",frostedEmerald);
            outlines=gameObject.AddComponent<HolographicOutlines>();
            journey=gameObject.AddComponent<ThemeJourney>();journey.Initialize(glowMaterial);
        }
        ParticleSystem System(string name,Material material,int capacity,float life,float size,Color color)
        {
            var node=new GameObject(name);node.transform.SetParent(transform);var ps=node.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.loop=true;main.playOnAwake=false;main.maxParticles=capacity;main.simulationSpace=ParticleSystemSimulationSpace.World;main.startLifetime=life;main.startSize=size;main.startSpeed=0;main.startColor=color;
            var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.enabled=false;
            var fade=ps.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(.7f,.2f),new GradientAlphaKey(0,1)});fade.color=gradient;
            var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.material=material;renderer.renderMode=ParticleSystemRenderMode.Billboard;renderer.sortingOrder=3;ps.Play();return ps;
        }
        Texture2D ParticleTexture(bool star)
        {
            const int n=128;var t=new Texture2D(n,n,TextureFormat.RGBA32,false);t.name=star?"Mektra star glow":"Mektra soft particle";t.wrapMode=TextureWrapMode.Clamp;var pixels=new Color[n*n];
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                float px=(x+.5f)/n*2-1,py=(y+.5f)/n*2-1,r=Mathf.Sqrt(px*px+py*py);float alpha=Mathf.Pow(Mathf.Clamp01(1-r),2);
                if(star)
                {
                    // Supersample a true five-point silhouette: fragments stay legible as stars.
                    alpha=0;
                    for(int sy=0;sy<2;sy++)for(int sx=0;sx<2;sx++)
                        if(InsideStar(new Vector2(px+(sx-.5f)/n,py+(sy-.5f)/n)))alpha+=.25f;
                }
                pixels[y*n+x]=new Color(1,1,1,alpha);
            }
            t.SetPixels(pixels);t.Apply();return t;
        }
        static bool InsideStar(Vector2 point)
        {
            bool inside=false;
            for(int i=0,j=9;i<10;j=i++)
            {
                float a=Mathf.PI/2+i*Mathf.PI/5,b=Mathf.PI/2+j*Mathf.PI/5;
                var v=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(i%2==0?.9f:.38f);
                var w=new Vector2(Mathf.Cos(b),Mathf.Sin(b))*(j%2==0?.9f:.38f);
                if((v.y>point.y)!=(w.y>point.y)&&point.x<(w.x-v.x)*(point.y-v.y)/(w.y-v.y)+v.x)inside=!inside;
            }
            return inside;
        }
        Texture2D RingTexture()
        {
            const int n=64;var texture=new Texture2D(n,n,TextureFormat.RGBA32,false);
            texture.name="Mektra impact ring";texture.wrapMode=TextureWrapMode.Clamp;
            var pixels=new Color[n*n];
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                float r=new Vector2((x+.5f)/n*2-1,(y+.5f)/n*2-1).magnitude;
                pixels[y*n+x]=new Color(1,1,1,Mathf.Exp(-Mathf.Pow((r-.78f)/.045f,2)));
            }
            texture.SetPixels(pixels);texture.Apply();return texture;
        }
        void Ring(Point point,float diameter,Color color)
        {
            shockwave.Emit(new ParticleSystem.EmitParams{position=Position(point),startSize=diameter,startColor=color},1);
        }
        Vector3 Position(Point p)=>new Vector3((float)(p.x-195),(float)(renderOrigin-p.y),0);
        public void Refresh(Simulation sim)
        {
            bool changed=current!=sim;current=sim;
            if(gradeVersion!=ColorPresetPreview.Version){gradeVersion=ColorPresetPreview.Version;grading.LutTexture=null;grading.LutBlendTexture=Resources.Load<Texture2D>("ColorGrades/"+ColorPresetPreview.Options[ColorPresetPreview.CurrentIndex]);if(ColorPresetPreview.IsMonochrome)grading.LutBlendTexture=monochrome;grading.BlendAmount=grading.LutBlendTexture==null?0:1;}

            lavaEmbers.gameObject.SetActive(false);
            pointShimmer.gameObject.SetActive(false);playerShimmer.gameObject.SetActive(false);shockwave.gameObject.SetActive(false);
            double origin=sim.platform.offset;
            if(origin!=renderOrigin)
            {
                float delta=(float)(origin-renderOrigin);
                foreach(var ps in new[]{trail,starBurst,land,crash,pointShimmer,playerShimmer,shockwave,lavaEmbers,shapeRipple,captureRing})
                {
                    int count=ps.GetParticles(rebaseBuffer);
                    for(int i=0;i<count;i++)rebaseBuffer[i].position+=new Vector3(0,delta,0);
                    ps.SetParticles(rebaseBuffer,count);
                }
                renderOrigin=origin;
            }
            if(changed){trail.Clear();starBurst.Clear();land.Clear();crash.Clear();dust.Clear();pointShimmer.Clear();playerShimmer.Clear();shockwave.Clear();captureRing.Clear();lavaEmbers.Clear();shapeRipple.Clear();lavaClock=0;emissionClock=0;pointClock=0;shimmerClock=0;previousTrail=sim.RenderPosition;}
            gameCamera.orthographicSize=(float)sim.viewport/(2*GameConfig.ViewZoom);
            gameCamera.transform.position=new Vector3(0,(float)(renderOrigin-sim.RenderCamera-sim.viewport*(.7-.2/GameConfig.ViewZoom)),-100);
            backdrop.transform.position=new Vector3(0,gameCamera.transform.position.y,20);
            dust.GetComponent<ParticleSystemRenderer>().enabled=false;
            var dustMain=dust.main;dustMain.simulationSpace=ParticleSystemSimulationSpace.Local;
            dust.transform.position=new Vector3(0,gameCamera.transform.position.y,0);backdrop.transform.localScale=new Vector3(390,(float)sim.viewport,1);
            forest.Refresh(sim.RenderCamera,(float)sim.viewport,renderOrigin);
            var palette=journey.Refresh(sim,skyMaterial,forest,gameCamera.transform.position);
            themeAccent=palette.accent;
            skyMaterial.SetColor("_PlainTop",ColorPresetPreview.GradientTop);skyMaterial.SetFloat("_PlainInvert",ColorPresetPreview.IsInvert?1:0);
            outlines.enabled=false;
            skyMaterial.SetFloat("_BackgroundTheme",PlayerPrefs.GetInt("mektra.background.theme",0));
            skyMaterial.SetFloat("_Opening",1-palette.landscape*.72f);
            skyMaterial.SetFloat("_Travel",(float)(-(sim.startY-sim.bestY)*.00015));
            forest.SetTheme(1,new Color(.4f,.75f,.8f));skyMaterial.SetFloat("_Opening",0);
            if(sim.phase!=Phase.Paused)impact*=Mathf.Exp(-12*Mathf.Min(Time.unscaledDeltaTime,.05f));
            if(changed)impact=0;skyMaterial.SetFloat("_Impact",impact);
            bool alive=sim.phase!=Phase.Exploding&&sim.phase!=Phase.Over;
            halo.SetActive(false);halo.transform.position=Position(sim.RenderPosition)+new Vector3(0,0,1);halo.transform.localScale=Vector3.one*(sim.flying?44.1f:38.7f);
            if(sim.phase==Phase.Playing)
            {
                emissionClock+=Time.unscaledDeltaTime;
                var end=sim.RenderPosition;
                int count=Math.Max((int)(emissionClock*90),(int)Math.Ceiling((end-previousTrail).Length/3));
                emissionClock%=1.0/90;
                // Emit along the traveled segment, so a fast jump cannot leave gaps.
                for(int i=1;i<=count;i++)trail.Emit(new ParticleSystem.EmitParams{position=Position(previousTrail+(end-previousTrail)*(i/(double)count)),startColor=new Color(ColorPresetPreview.PlayerInk.r,ColorPresetPreview.PlayerInk.g,ColorPresetPreview.PlayerInk.b,.5f)},1);
                sparkClock+=Time.unscaledDeltaTime;
                if(sparkClock>=.24){sparkClock%=.24;var backward=(previousTrail-end).Normal;
                    land.Emit(new ParticleSystem.EmitParams{position=Position(end),velocity=new Vector3((float)backward.x*12+UnityEngine.Random.Range(-5f,5f),(float)-backward.y*12+UnityEngine.Random.Range(-5f,5f),0),startColor=Color.white,startSize=UnityEngine.Random.Range(.9f,1.6f),startLifetime=.38f},1);}
                previousTrail=end;
            }
            if(sim.LavaEnabled&&sim.phase==Phase.Playing&&sim.lava.Active(sim.RenderTime)&&sim.RenderLava<sim.RenderCamera+sim.viewport+30)
            {
                lavaClock+=Time.unscaledDeltaTime;
                while(lavaClock>=.08)
                {
                    lavaClock-=.08;
                    lavaEmbers.Emit(new ParticleSystem.EmitParams{position=Position(new Point(UnityEngine.Random.Range(8f,382f),sim.RenderLava+3)),velocity=new Vector3(UnityEngine.Random.Range(-8f,8f),UnityEngine.Random.Range(18f,40f),0),startSize=UnityEngine.Random.Range(1.5f,3.5f)},2);
                }
            }
            if(sim.phase==Phase.Paused){lavaEmbers.Pause();}else if(lavaEmbers.isPaused)lavaEmbers.Play();
            pointClock+=Time.unscaledDeltaTime;shimmerClock+=Time.unscaledDeltaTime;
            if(pointClock>=.07&&sim.phase!=Phase.Paused&&sim.phase!=Phase.Over&&sim.phase!=Phase.Exploding)
            {
                pointClock%=.07;int count=0;
                foreach(var s in sim.stars){if(s.collected||s.position.y<sim.RenderCamera-40||s.position.y>sim.RenderCamera+sim.viewport+40)continue;if(count++>=16)break;pointShimmer.Emit(new ParticleSystem.EmitParams{position=Position(s.At(sim.RenderTime)),startSize=8f+.8f*(float)Math.Sin(sim.time*2.4+s.position.y*.01)},1);}
            }
            if(alive&&sim.phase!=Phase.Paused&&shimmerClock>=.05)
            {
                shimmerClock%=.05;float a=UnityEngine.Random.Range(0,Mathf.PI*2);var dir=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0);
                playerShimmer.Emit(new ParticleSystem.EmitParams{position=Position(sim.RenderPosition)+dir*10.8f,velocity=dir*14},1);
            }
            if(sim.phase==Phase.Paused){trail.Pause();starBurst.Pause();land.Pause();crash.Pause();playerShimmer.Pause();pointShimmer.Pause();shockwave.Pause();captureRing.Pause();shapeRipple.Pause();}else{if(trail.isPaused)trail.Play();if(starBurst.isPaused)starBurst.Play();if(land.isPaused)land.Play();if(crash.isPaused)crash.Play();if(playerShimmer.isPaused)playerShimmer.Play();if(pointShimmer.isPaused)pointShimmer.Play();if(shockwave.isPaused)shockwave.Play();if(captureRing.isPaused)captureRing.Play();if(shapeRipple.isPaused)shapeRipple.Play();}
#if !MEKTRA_INVERT_PREVIEW
            if(dust.particleCount<45){for(int i=0;i<45-dust.particleCount;i++)dust.Emit(new ParticleSystem.EmitParams{position=new Vector3(UnityEngine.Random.Range(-195,195),UnityEngine.Random.Range(-(float)sim.viewport/2,(float)sim.viewport/2),10),velocity=new Vector3(UnityEngine.Random.Range(-1f,1f),-1.4f,0),startLifetime=UnityEngine.Random.Range(12,24)},1);}
#endif
            // Platforms are crisp UI vector fills; glow is reserved for collectibles and impacts.

        }
        public void GameEvent(string name,Simulation sim,Point origin)
        {
            current=sim;
            if(name=="perfect"){
                // The continuous white ripple is drawn with the shape outlines in WorldView.
            }
            else if(name=="magnet"){Ring(origin,110,new Color(1,1,1,.7f));Burst(land,origin,16,85,ice);}
            else if(name=="star"){
                // Contact is celebrated with glowing fragments, without an expanding circle.
                for(int i=0;i<12;i++)
                {
                    float angle=i*Mathf.PI*2/12+UnityEngine.Random.Range(-.08f,.08f);
                    float speed=UnityEngine.Random.Range(50f,85f);
                    var colour=i%3==0?Color.white:i%3==1?new Color(.85f,1,.45f):new Color(.4f,.95f,1);
                    starBurst.Emit(new ParticleSystem.EmitParams{position=Position(origin),velocity=new Vector3(Mathf.Cos(angle)*speed,Mathf.Sin(angle)*speed,0),startSize=UnityEngine.Random.Range(4f,8f),startLifetime=UnityEngine.Random.Range(.6f,.85f),rotation=0,startColor=colour},1);
                }
            }
            else if(name=="land"){Ring(sim.position,46,new Color(.4f,.85f,1,.4f));Burst(land,sim.position,12,65,ice);}
            else if(name=="jump"){
                for(int i=0;i<5;i++){float angle=i*Mathf.PI*2/5;land.Emit(new ParticleSystem.EmitParams{position=Position(origin),velocity=new Vector3(Mathf.Cos(angle)*30,Mathf.Sin(angle)*30,0),startColor=Color.white,startSize=UnityEngine.Random.Range(1f,2f),startLifetime=.35f},1);}
            }
            else if(name=="crash")
            {
                trail.Clear();impact=.5f;CameraPlay.Shockwave((float)(sim.position.x/390),.5f,.35f,.3f);
                Ring(sim.position,145,new Color(.5f,.8f,1,.85f));
                Ring(sim.position,92,new Color(1,.4f,.2f,.8f));
                Burst(crash,sim.position,42,210,new Color(1,.4f,.2f,1));
                Burst(land,sim.position,8,55,Color.white);
                // Wall sparks fan inward, keeping the blast visible inside the play area.
                float direction=sim.position.x<195?1:-1;
                for(int i=0;i<32;i++)
                {
                    float angle=UnityEngine.Random.Range(-1.3f,1.3f),speed=UnityEngine.Random.Range(75,260);
                    crash.Emit(new ParticleSystem.EmitParams{position=Position(sim.position),velocity=new Vector3(direction*Mathf.Cos(angle)*speed,Mathf.Sin(angle)*speed,0),startSize=UnityEngine.Random.Range(2,6),startLifetime=UnityEngine.Random.Range(.3f,.7f),startColor=BurstColor(i)},1);
                }
            }
        }
        static Color BurstColor(int i)=>i%3==0?Color.white:i%3==1?Color.black:new Color(.5f,.5f,.5f,1);
        Texture2D CircleTexture()
        {
            const int n=64;var texture=new Texture2D(n,n,TextureFormat.RGBA32,false);texture.wrapMode=TextureWrapMode.Clamp;var pixels=new Color[n*n];
            for(int y=0;y<n;y++)for(int x=0;x<n;x++){float radius=new Vector2((x+.5f)/n*2-1,(y+.5f)/n*2-1).magnitude;pixels[y*n+x]=new Color(1,1,1,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.94f,1f,radius)));}
            texture.SetPixels(pixels);texture.Apply();return texture;
        }
        void ConfigureRoundBurst(ParticleSystem ps,Material material)
        {
            ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=material;
            var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.3f),new Keyframe(.12f,1),new Keyframe(.5f,.65f),new Keyframe(1,0)));
            var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.speedModifier=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,1),new Keyframe(.3f,.45f),new Keyframe(1,.05f)));
            var fade=ps.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(.9f,.4f),new GradientAlphaKey(0,1)});fade.color=gradient;
        }
        void Burst(ParticleSystem ps,Point origin,int count,float speed,Color color)
        {
            var position=Position(origin);for(int i=0;i<count;i++){float a=i*Mathf.PI*2/count,v=speed*(.5f+(i%5)*.15f);ps.Emit(new ParticleSystem.EmitParams{position=position,velocity=new Vector3(Mathf.Cos(a)*v,Mathf.Sin(a)*v,0),startColor=BurstColor(i),startLifetime=.5f+(i%4)*.09f,startSize=2.5f+(i%4)*1.3f,rotation=0},1);}
        }
    }
}
