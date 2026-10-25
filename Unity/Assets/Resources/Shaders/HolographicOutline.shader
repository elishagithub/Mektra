Shader "Mektra/HolographicOutline"
{
    Properties
    {
        _Strength("Halo intensity",Range(0,2))=.16
        _CoreIntensity("Filament emission",Range(0,5))=1.3
        _CoreWidth("Filament half width",Range(.01,.4))=.12
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        // Premultiplied additive energy; opacity is applied once in the fragment.
        Blend One One
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            float _Strength,_CoreIntensity,_CoreWidth;
            v2f vert(appdata v) { v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o; }
            float4 frag(v2f i):SV_Target
            {
                float d=abs(i.uv.y-.5)*2;
                float aa=max(fwidth(d),.006);
                float core=1-smoothstep(_CoreWidth-aa,_CoreWidth+aa,d);
                float inner=exp(-d*d*28);
                float outer=exp(-d*d*14)*(1-smoothstep(.72,1,d));
                float3 filament=lerp(i.color.rgb,float3(1,1,1),.8)*core*_CoreIntensity;
                float3 halo=i.color.rgb*(inner*.12+outer*_Strength);
                return float4((filament+halo)*i.color.a,0);
            }
            ENDCG
        }
    }
}
