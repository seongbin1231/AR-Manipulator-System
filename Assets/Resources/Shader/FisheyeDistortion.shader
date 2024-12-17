Shader "Custom/FisheyeDistortion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}  // 메인 텍스처
        _Distortion ("Distortion Strength", Float) = 0.2 // 왜곡 강도
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

            sampler2D _MainTex; // 텍스처
            float _Distortion;  // 왜곡 강도

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

                // UV 좌표의 중심에서 거리 계산
                float2 center = float2(0.5, 0.5); // 화면 중심
                float2 offset = uv - center;
                float dist = length(offset);

                // 왜곡 계산
                float strength = 1.0 + _Distortion * dist * dist;
                uv = center + offset * strength;

                // 텍스처 샘플링
                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}
