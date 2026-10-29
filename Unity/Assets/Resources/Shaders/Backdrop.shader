Shader "Mektra/Backdrop"
{
    Properties
    {
        _ForestA("Moonlit forest",2D)="black" {}
        _ForestB("Emerald forest",2D)="black" {}
        _Top("Sky",Color)=(.4,.25,.15,1)
        _Bottom("Horizon",Color)=(.02,.1,.12,1)
        _Light("Backlight",Color)=(.6,.4,.2,1)
        _Landscape("Mountains",Float)=0
        _Night("Night",Float)=0
        _Dunes("Dunes",Float)=0
        _Travel("Parallax",Float)=0
        _Clock("Animation",Float)=0
        _Impact("Impact flash",Float)=0
        _Opening("Opening celestial theme",Float)=0
        _Abstract("Glass abstract theme",Float)=0
    }
    SubShader
    {
        Tags { "Queue"="Background" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _ForestA,_ForestB;
            float _BackgroundTheme;
            float4 _Top,_Bottom,_Light;
            float _Plains,_Landscape,_Night,_Dunes,_Travel,_Clock,_Impact,_Opening,_Abstract;
            struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
            v2f vert(appdata_base v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord;return o;}
            float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float3 planet(float3 sky,float2 uv,float2 center,float radius,float3 tint)
            {
                float2 q=(uv-center)*float2(1,2.08);float d=length(q),edge=d-radius;
                float halo=exp(-max(edge,0)*75)*step(0,edge);
                sky+=tint*halo*.025;
                if(d<radius)
                {
                    float z=sqrt(saturate(1-dot(q,q)/(radius*radius)));
                    float light=saturate(dot(normalize(float3(q/radius,z)),normalize(float3(.45,.65,.35))));
                    float rim=pow(saturate(d/radius),20)*(.25+.75*saturate(q.y/radius+.5));
                    float3 body=tint*(.16+light*.42)+tint*rim*.24;
                    float depth=(1-smoothstep(radius-.008,radius,d));
                    sky=lerp(sky,body,.27*depth)+tint*pow(saturate(d/radius),45)*.035;
                }
                return sky;
            }
            float3 jupiter(float3 sky,float2 uv,float2 center,float radius)
            {
                float2 q=(uv-center)*float2(1,2.08);float d=length(q);
                sky=planet(sky,uv,center,radius,float3(.85,.57,.37));
                if(d<radius)
                {
                    float bands=.5+.5*sin(q.y/radius*34+sin(q.x/radius*5)*.65);
                    float fine=.5+.5*sin(q.y/radius*79+sin(q.x/radius*9));
                    float3 tint=lerp(float3(.43,.22,.16),float3(.98,.78,.51),bands*.75+fine*.15);
                    float spot=exp(-dot((q/radius-float2(.32,-.2))*float2(8,17),(q/radius-float2(.32,-.2))*float2(8,17)));
                    tint=lerp(tint,float3(.68,.22,.12),spot*.65);
                    float shade=.3+.45*sqrt(saturate(1-d*d/(radius*radius)));
                    sky=lerp(sky,tint*shade,.21);
                }
                return sky;
            }
            float drift(float start,float speed)
            {
                // Wrap only while fully outside the screen; each depth has its own pace.
                return frac((start+1+(_Travel-_Clock*.0015)*speed)/3)*3-1;
            }
            float3 celestial(float2 uv)
            {
                float2 q=(uv-float2(.27,.79))*float2(1,2.08);
                float warm=exp(-dot(q,q)*5.5);
                float mood=.15*(1-cos(_Travel*.65));
                float3 deep=lerp(float3(.005,.19,.36),float3(.15,.12,.4),mood);
                float3 horizon=lerp(float3(.035,.21,.29),float3(.025,.3,.34),mood);
                float3 c=lerp(_Bottom.rgb,_Top.rgb,smoothstep(0,1,uv.y));
                c=lerp(c,_Light.rgb*.55,warm*.45);
                c+=_Light.rgb*exp(-dot(q,q)*75)*.07;
                // A soft galactic ribbon moves behind the planets and never obscures the route.
                float galactic=uv.y-.24-uv.x*.44-sin(uv.x*6+_Travel*.25)*.026-_Travel*.018;
                float nebula=exp(-galactic*galactic*75);
                float filaments=.5+.5*sin(uv.x*28+sin(uv.y*21+_Travel*.13)*2);
                c+=nebula*(float3(.055,.12,.24)+float3(.11,.035,.16)*filaments)*.7;
                float2 grain=floor((uv+float2(0,_Travel*.04))*float2(120,245));
                float glint=hash(grain);float dots=exp(-dot(frac((uv+float2(0,_Travel*.04))*float2(120,245))-.5,frac((uv+float2(0,_Travel*.04))*float2(120,245))-.5)*180);
                c+=dots*step(.94,glint)*nebula*float3(.4,.55,.85)*.33;
                // Large cropped worlds frame the route, leaving the centre open.
                c=planet(c,uv,float2(-.11+.012*sin(_Travel*.7),drift(.65,.7)),.49,lerp(_Light.rgb,float3(.42,.33,.68),.3));
                c=planet(c,uv,float2(-.17+.01*sin(_Travel*.5),drift(.34,1.05)),.37,float3(.3,.47,.91));
                c=planet(c,uv,float2(1.13+.008*sin(_Travel*.6),drift(.46,.85)),.32,float3(.16,.65,1));
                // A distant world gradually enters between the larger foreground layers.
                c=planet(c,uv,float2(1.19,drift(1.65,.55)),.25,float3(.25,.48,.86));
                c=jupiter(c,uv,float2(-.13,drift(1.55,.9)),.36);
                c=planet(c,uv,float2(1.16,drift(2.3,1.12)),.39,float3(.24,.61,.9));
                c*=1+.035*sin(_Travel*.8);
                float orbit=length((uv-float2(.25,.72+sin(_Travel*.2)*.04))*float2(1,2.08));
                c+=float3(.35,.67,.79)*exp(-pow((orbit-.79)*350,2))*.13;
                c+=float3(.35,.67,.79)*exp(-pow((orbit-1.42)*350,2))*.09;
                float auroraLane=uv.y-.64-sin(uv.x*5+_Clock*.085)*.075;
                c+=float3(.035,.29,.21)*exp(-auroraLane*auroraLane*125)*_Night*(.45+.3*sin(uv.x*11+_Clock*.05));
                float2 grid=(uv-float2(0,_Clock*.0012))*float2(43,88);float2 cell=floor(grid);float seed=hash(cell);
                float sparkle=exp(-dot(frac(grid)-.5,frac(grid)-.5)*800)*step(.963,seed);
                float twinkle=smoothstep(.08,.9,.5+.5*sin(_Clock*(.18+seed*.12)+seed*70));
                c+=sparkle*lerp(float3(.35,.72,1),_Light.rgb,.25)*twinkle*.36;
                float luminance=dot(c,float3(.2126,.7152,.0722));
                return lerp(float3(luminance,luminance,luminance),c,.8);
            }
            float3 meteor(float2 uv)
            {
                // Sparse, staggered trails: long quiet gaps, soft entrance and exit.
                float cycle=floor(_Clock/38);
                float phase=frac(_Clock/38)*38;
                float seed=hash(float2(cycle,17));
                float life=saturate(phase/8);
                float visible=(1-smoothstep(6,8,phase))*smoothstep(0,1.5,phase);
                float2 head=float2(lerp(-.15,1.15,life),.82-seed*.2-life*.18);
                float2 tail=head-float2(.075,-.0104);
                float2 axis=head-tail;
                float along=saturate(dot(uv-tail,axis)/dot(axis,axis));
                float distance=length((uv-tail-axis*along)*float2(1,2.08));
                float streak=exp(-distance*distance*900000)*along*visible;
                return lerp(float3(.3,.56,.78),_Light.rgb,.25)*streak*.13;
            }
            float4 _PlainTop;float _PlainInvert;
            float3 abstractLight(float2 uv)
            {
                if(_Abstract>3.5)
                {
                    float3 night=_PlainTop.rgb*lerp(.5,1,saturate(uv.y));
                    return lerp(night,1-night,_PlainInvert);
                }
                float t=_Clock*.09;
                float3 c=lerp(float3(.004,.008,.025),float3(.016,.012,.04),uv.y);
                if(_Abstract<1.5 || _Abstract>2.5)
                {
                    for(int strand=0;strand<12;strand++)
                    {
                        float f=strand/11.0;
                        float curve=.5+.28*sin(uv.y*3.7-1.8+t*.35)+.022*sin(uv.y*9+t+f*2);
                        if(_Abstract>2.5)curve=.5+.23*sin((uv.y-.5)*5)*sin(t*.3+f*.4+1);
                        curve+=(f-.5)*.12;
                        float d=uv.x-curve;
                        float core=exp(-d*d*22000)*.12;
                        float bloom=exp(-d*d*180)*.024;
                        c+=lerp(float3(.01,.48,.85),float3(.62,.025,.39),f)*(core+bloom);
                    }
                }
                else
                {
                    for(int bar=0;bar<10;bar++)
                    {
                        float f=bar/9.0,x=f+.018*sin(t*.7+bar);
                        float d=uv.x-x;
                        float length=smoothstep(.02,.5,uv.y+.18*sin(t+bar));
                        c+=lerp(float3(.01,.4,.58),float3(.5,.02,.43),f)*(exp(-d*d*480)*.065+exp(-d*d*80000)*.07)*length;
                    }
                }
                return c;
            }
            float3 glassBackdrop(float2 uv)
            {
                if(_Abstract>3.5)return abstractLight(uv);
                // A softly refracting frosted sheet over animated light, below all gameplay UI.
                float2 warp=float2(sin(uv.y*6+_Clock*.035),cos(uv.x*7-_Clock*.025))*.006;
                float2 q=uv+warp+float2(_Travel*.05,0);
                float3 c=abstractLight(q)*.5;
                c+=(abstractLight(q+float2(.005,0))+abstractLight(q-float2(.005,0))+abstractLight(q+float2(0,.003))+abstractLight(q-float2(0,.003)))*.125;
                float sheen=exp(-pow((uv.x*.6+uv.y-.75+.035*sin(_Clock*.04))*12,2));
                c+=float3(.1,.18,.25)*sheen*.045;
                float2 grid=(uv-float2(_Clock*.0008,-_Clock*.007))*float2(24,44);
                float h=hash(floor(grid));float dust=exp(-dot(frac(grid)-.5,frac(grid)-.5)*220)*step(.96,h);
                c+=float3(.2,.45,.6)*dust*.18;
                return c*(1-smoothstep(.3,.85,length(uv-.5))*.3);
            }
            float4 frag(v2f i):SV_Target
            {
                float climb=max(0,-_Travel);
                float offset=.045+.23*(.5-.5*cos(climb*.27));
                float2 uv=float2(.012+i.uv.x*.976+sin(_Clock*.025)*.009,i.uv.y*.67+offset+.009*sin(_Clock*.018));
                float blend=.5-.5*cos(climb*.8);
                float3 woods=lerp(tex2D(_ForestA,uv).rgb,tex2D(_ForestB,uv+float2(0,.008*sin(_Clock*.035))).rgb,blend);
                float mist=pow(saturate(1-abs(i.uv.x-.5)*1.8),3);
                float breath=.5+.5*sin(i.uv.y*5+_Clock*.045);
                woods+=float3(.012,.042,.043)*mist*breath;
                // A restrained translucent blue film lifts deep foliage without washing out colour.
                float luminance=dot(woods,float3(.2126,.7152,.0722));
                woods=lerp(luminance.xxx,woods,.94);
                woods=lerp(woods,float3(.10,.20,.25),.08);
                float sheen=pow(saturate(1-abs(i.uv.x+i.uv.y*.25-.6)*1.6),4);
                woods+=float3(.025,.045,.05)*sheen;
                woods*=lerp(.84,1.10,mist);
                // Distant canopy light: broad soft shafts stay behind every gameplay object.
                float shafts=0;
                for(int ray=0;ray<3;ray++)
                {
                    float seed=hash(float2(ray,31));
                    float centre=.12+ray*.32+(1-i.uv.y)*.24+.012*sin(_Clock*.035+ray*2);
                    float width=.018+(1-i.uv.y)*.045;
                    float across=(i.uv.x-centre)/width;
                    shafts+=exp(-across*across*1.8)*smoothstep(.02,.65,i.uv.y)*(.72+.12*sin(_Clock*.09+seed*9));
                }
                woods+=float3(.16,.22,.23)*shafts*.19;
                // Sparse dust has no wings, collectible bloom or gameplay collision.
                // Two slow depth speeds respond gently to camera travel; wrapping fades at edges.
                float dustLight=0;
                for(int mote=0;mote<16;mote++)
                {
                    float seed=hash(float2(mote,77));
                    float depth=.5+.5*hash(float2(mote,19));
                    float x=.04+.92*hash(float2(mote,41))+.018*sin(_Clock*.11+seed*47);
                    float y=frac(seed+_Clock*(.002+depth*.002)-_Travel*.025*depth);
                    float2 delta=(i.uv-float2(x,y))*float2(1,2.08);
                    float radius=.0015+depth*.0015;
                    float fade=smoothstep(0,.08,y)*(1-smoothstep(.92,1,y));
                    dustLight+=exp(-dot(delta,delta)/(radius*radius))*fade*(.10+depth*.13);
                }
                woods+=float3(.78,.86,.83)*dustLight;
                if(_BackgroundTheme>.5&&_BackgroundTheme<1.5){float light=dot(woods,float3(.2126,.7152,.0722));woods=lerp(woods,float3(light*1.6,light*.66,light*.45),.85);}
                else if(_BackgroundTheme>1.5){float a=.5+.5*sin(i.uv.y*6+i.uv.x*3);woods*=lerp(float3(.8,.65,1.6),float3(.4,1.25,1.1),a);}
                return float4(woods,1);
            }
            ENDCG
        }
    }
}
