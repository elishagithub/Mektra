Shader "Mektra/ForestTheme"
{
 Properties {_MainTex("Forest",2D)="black" {} _Theme("Theme",Float)=0}
 SubShader {Cull Off ZWrite Off ZTest Always Pass {CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;float _Theme;
 float4 frag(v2f_img i):SV_Target{float3 c=tex2D(_MainTex,i.uv).rgb;if(_Theme>.5&&_Theme<1.5){float l=dot(c,float3(.2126,.7152,.0722));c=lerp(c,float3(l*1.6,l*.66,l*.45),.85);}else if(_Theme>1.5){float a=.5+.5*sin(i.uv.y*6+i.uv.x*3);c*=lerp(float3(.8,.65,1.6),float3(.4,1.25,1.1),a);}return float4(c,1);}
 ENDCG}}
}
