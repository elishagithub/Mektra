Shader "Mektra/GlowLine"
{
    Properties { _Tint("Tint",Color)=(0.2,0.5,1,1) }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        Blend SrcAlpha One ZWrite Off Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION;float4 color:COLOR; };
            struct v2f { float4 vertex:SV_POSITION;float4 color:COLOR; };
            float4 _Tint;
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color;return o;}
            float4 frag(v2f i):SV_Target{return float4(i.color.rgb*_Tint.rgb*2.5,i.color.a*_Tint.a);}
            ENDCG
        }
    }
}
