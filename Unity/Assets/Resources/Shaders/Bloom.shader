Shader "Mektra/Bloom"
{
    Properties
    {
        _MainTex ("Scene", 2D) = "black" {}
        _BloomTex ("Glow", 2D) = "black" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        CGINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex,_BloomTex;
        float4 _MainTex_TexelSize;
        float2 _Direction;
        float _Strength;
        float4 threshold(v2f_img i):SV_Target {float3 c=tex2D(_MainTex,i.uv).rgb;float m=max(c.r,max(c.g,c.b));return float4(c*max(0,m-.65)/max(m,.001),1);}
        float4 blur(v2f_img i):SV_Target {float2 d=_Direction*_MainTex_TexelSize.xy;float3 c=tex2D(_MainTex,i.uv).rgb*.227027;c+=(tex2D(_MainTex,i.uv+d*1.384615).rgb+tex2D(_MainTex,i.uv-d*1.384615).rgb)*.316216;c+=(tex2D(_MainTex,i.uv+d*3.230769).rgb+tex2D(_MainTex,i.uv-d*3.230769).rgb)*.070270;return float4(c,1);}
        float4 combine(v2f_img i):SV_Target {float3 c=tex2D(_MainTex,i.uv).rgb+tex2D(_BloomTex,i.uv).rgb*_Strength;return float4(c/(1+c*.18),1);}
        ENDCG
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment threshold
            ENDCG }
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment blur
            ENDCG }
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment combine
            ENDCG }
    }
}
