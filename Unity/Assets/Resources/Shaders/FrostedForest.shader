Shader "Mektra/FrostedForest"
{
    Properties { _MainTex("Forest",2D)="black" {} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _Direction;
            fixed4 frag(v2f_img i):SV_Target
            {
                float2 step=_Direction.xy*2.0;
                float3 colour=tex2D(_MainTex,i.uv).rgb*.227027;
                colour+=tex2D(_MainTex,i.uv+step*1.384615).rgb*.316216;
                colour+=tex2D(_MainTex,i.uv-step*1.384615).rgb*.316216;
                colour+=tex2D(_MainTex,i.uv+step*3.230769).rgb*.070270;
                colour+=tex2D(_MainTex,i.uv-step*3.230769).rgb*.070270;
                return float4(colour,1);
            }
            ENDCG
        }
    }
}
