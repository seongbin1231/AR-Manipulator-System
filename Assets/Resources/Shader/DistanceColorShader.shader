Shader "Custom/DistanceColorShader"
{
    Properties
    {
        _Transparency ("Transparency", Range(0,1)) = 0.3
        _MinDistance ("Min Distance", Float) = 0
        _MaxDistance ("Max Distance", Float) = 3
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float dist : TEXCOORD0;
            };

            float _Transparency;
            float _MinDistance;
            float _MaxDistance;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.dist = distance(_WorldSpaceCameraPos, worldPos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float t = saturate((i.dist - _MinDistance) / (_MaxDistance - _MinDistance));

                fixed4 col;
                if (t <= 0.5)
                {
                    // ���������� ���������
                    col = fixed4(1, t * 2, 0, _Transparency);
                }
                else
                {
                    // ��������� �ʷϻ�����
                    t = (t - 0.5) * 2;
                    col = fixed4(1 - t, 1, 0, _Transparency);
                }

                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
