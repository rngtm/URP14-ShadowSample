Shader "Unlit/BG"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _IsCharacter("Is Character", Int) = 0
        _ShadowColor ("Shadow Color", Color) = (1, 1, 1, 1)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
        
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "ShaderLibrary/CustomShadow.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _Color;
            half _IsCharacter;
            half4 _ShadowColor;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                float3 positionWS = TransformObjectToWorld(v.vertex);
                float3 positionVS = mul(UNITY_MATRIX_V, float4(positionWS, 1));
                float4 positionCS = mul(UNITY_MATRIX_P, float4(positionVS, 1));
                o.positionWS = positionWS;
                o.positionCS = positionCS;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 color = _Color;

                const half atten = SampleCustomShadow(i.positionWS);
                color.rgb = lerp(color.rgb * _ShadowColor.rgb, color.rgb, atten);
                
                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "CustomShadow" }
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "ShaderLibrary/CustomShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}
