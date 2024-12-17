Shader "Custom/FisheyeDistortion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}  // ���� �ؽ�ó
        _Distortion ("Distortion Strength", Float) = 0.2 // �ְ� ����
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex; // �ؽ�ó
            float _Distortion;  // �ְ� ����

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // UV ��ǥ�� �߽ɿ��� �Ÿ� ���
                float2 center = float2(0.5, 0.5); // ȭ�� �߽�
                float2 offset = uv - center;
                float dist = length(offset);

                // �ְ� ���
                float strength = 1.0 + _Distortion * dist * dist;
                uv = center + offset * strength;

                // �ؽ�ó ���ø�
                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}
