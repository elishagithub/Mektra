Shader "Mektra/PlainParticle"
{
 Properties { _MainTex("Particle",2D)="white"{} _Tint("Tint",Color)=(1,1,1,1) }
 SubShader {
 Tags { "Queue"="Transparent" "RenderType"="Transparent" }
 Blend SrcAlpha OneMinusSrcAlpha
 ZWrite Off Cull Off
 Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
 struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
 sampler2D _MainTex;float4 _Tint;
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Tint;return o;}
 float4 frag(v2f i):SV_Target{return float4(i.color.rgb,i.color.a*tex2D(_MainTex,i.uv).a);}
 ENDCG
 }
 }
}
