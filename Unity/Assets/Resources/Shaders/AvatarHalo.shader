Shader "Mektra/AvatarHalo"
{
 Properties {_Color("Glow",Color)=(.1,.8,1,.3)}
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 Pass {CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 float4 _Color;
 float4 frag(v2f_img i):SV_Target {float r=length(i.uv-.5)*2;return float4(_Color.rgb,_Color.a*exp(-r*r*5)*saturate((1-r)*4));}
 ENDCG}}
}
