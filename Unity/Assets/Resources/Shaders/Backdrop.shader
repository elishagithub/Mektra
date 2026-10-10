Shader "Mektra/Backdrop"
{
    Properties
    {
        _Top("Sky",Color)=(.4,.25,.15,1)
        _Bottom("Horizon",Color)=(.02,.1,.12,1)
        _Light("Backlight",Color)=(.6,.4,.2,1)
        _Landscape("Mountains",Float)=0
        _Night("Night",Float)=0
        _Dunes("Dunes",Float)=0
        _Travel("Parallax",Float)=0
        _Clock("Animation",Float)=0
        _Impact("Impact flash",Float)=0
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
            float4 _Top,_Bottom,_Light;
            float _Landscape,_Night,_Dunes,_Travel,_Clock,_Impact;
            struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
            v2f vert(appdata_base v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord;return o;}
            float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float4 frag(v2f i):SV_Target
            {
                float2 uv=i.uv;
                float sun=exp(-length((uv-float2(.27,.72))*float2(1,1.25))*5);
                float3 c=lerp(_Bottom.rgb,_Top.rgb,smoothstep(.05,1,uv.y));
                c+=_Light.rgb*sun*.65;
                // Layered ridgelines become soft dunes as the desert palette arrives.
                for(int layer=0;layer<3;layer++)
                {
                    float x=uv.x+_Travel*(.12+layer*.08);
                    float jagged=1-abs(frac(x*2+layer*.17)*2-1);
                    jagged+=abs(sin(x*17+layer))*.12;
                    float rolling=.55+.35*sin(x*5+layer*1.9);
                    float ridge=.14+(2-layer)*.11+lerp(jagged,rolling,_Dunes)*(.1+layer*.025);
                    float cover=(1-smoothstep(ridge-.003,ridge+.003,uv.y))*_Landscape;
                    float3 mountain=lerp(_Bottom.rgb,_Top.rgb,.48-layer*.14)*(.85-layer*.08);
                    c=lerp(c,mountain,cover*.8);
                }
                float haze=exp(-pow((uv.y-.39-sin(uv.x*3+_Clock*.06)*.025)*5,2));
                c+=_Light.rgb*haze*.09;
                float curtain=exp(-pow((uv.y-.73-sin(uv.x*6+_Clock*.12)*.055)*18,2));
                c+=float3(.03,.28,.17)*curtain*_Night*(.5+.5*sin(uv.x*13+_Clock*.09));
                float2 grid=uv*float2(34,68);float2 cell=floor(grid);float h=hash(cell);
                float star=exp(-dot(frac(grid)-.5,frac(grid)-.5)*500)*step(.97,h);
                c+=star*_Night*(.4+.25*sin(_Clock*.7+h*40))*float3(.7,.85,1);
                float vignette=1-smoothstep(.25,.8,length((uv-.5)*float2(.85,1)))*.35;
                c=c*vignette+float3(.22,.3,.4)*_Impact;
                return float4(c,1);
            }
            ENDCG
        }
    }
}
