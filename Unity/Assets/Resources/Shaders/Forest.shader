Shader "Mektra/Forest"
{
    Properties { _Tint("Theme tint",Color)=(1,1,1,1) }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float4 _Tint;
            struct appdata {float4 vertex:POSITION;float4 color:COLOR;};
            struct v2f {float4 vertex:SV_POSITION;float4 color:COLOR;};
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color;return o;}
            float4 frag(v2f i):SV_Target{return i.color*_Tint;}
            ENDCG
        }
    }
}
