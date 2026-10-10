Shader "Mektra/ParticleGlow"
{
    Properties { _Tint("Tint",Color)=(1,1,1,1)  _MainTex("Particle",2D)="white"{} _Iridescence("Holographic edge",Range(0,1))=0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha One
        ZWrite Off Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR; };
            struct v2f { float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR; };
            sampler2D _MainTex;float _Iridescence;float4 _Tint;
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Tint;return o;}
            float4 frag(v2f i):SV_Target
            {
                float a=tex2D(_MainTex,i.uv).a;
                float phase=i.uv.x*3+i.uv.y*2-_Time.y*.45;
                float t=.5+.5*sin(phase*6.28318);
                float3 hologram=lerp(float3(.15,.85,1),float3(.72,.35,1),t);
                float edge=smoothstep(.12,.46,length(i.uv-.5))*_Iridescence;
                float3 color=lerp(i.color.rgb,hologram,edge);
                float scan=lerp(1,.9+.1*sin(i.uv.y*90+_Time.y*8),_Iridescence);
                return float4(color*2.2,i.color.a*a*scan);
            }
            ENDCG
        }
    }
}
