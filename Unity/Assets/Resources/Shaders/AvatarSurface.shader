Shader "Mektra/AvatarSurface"
{
 Properties {_Color("Body",Color)=(.012,.018,.022,1) _Accent("Rim",Color)=(.1,.8,1,1) _Unlit("Luminous",Float)=0}
 SubShader {Tags {"RenderType"="Opaque"} Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float4 _Color,_Accent;float _Unlit;
 struct v2f {float4 p:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;};
 v2f vert(appdata_base v){v2f o;o.p=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);return o;}
 float4 frag(v2f i):SV_Target{
 float3 n=normalize(i.normal),v=normalize(_WorldSpaceCameraPos-i.world),key=normalize(float3(-.45,.75,-.8)),rim=normalize(float3(.7,.6,.2));
 float facing=saturate(dot(n,v));float light=saturate(dot(n,key));
 float3 c=_Color.rgb*(.22+light*.78)+_Accent.rgb*pow(1-facing,2.2)*.85;
 c+=float3(.35,.65,.92)*pow(saturate(dot(n,key)),18)*.32;
 c+=float3(.65,.85,.96)*pow(saturate(dot(reflect(-key,n),v)),65)*.9;
 c+=_Accent.rgb*pow(saturate(dot(reflect(-rim,n),v)),40)*.5;
 return float4(lerp(c,_Color.rgb,_Unlit),1);}
 ENDCG}}
}
