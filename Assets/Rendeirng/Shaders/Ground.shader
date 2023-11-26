Shader "Unlit/Ground"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ShadowColor ("Shadow Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "ShaderLibrary/CustomShadow.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positioCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
            half4 _ShadowColor;
            CBUFFER_END
            
            v2f vert (appdata v)
            {
                v2f o;
                o.positioCS = TransformObjectToHClip(v.vertex);
                o.positionWS = TransformObjectToWorld(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half atten = SampleCustomShadow(i.positionWS);

                half4 color = 1;
                color.rgb = lerp(color.rgb * _ShadowColor.rgb, color.rgb, atten);
                return color;
                
                // return saturate(atten) * (1.0 - outVolume);
            }
            ENDHLSL
        }
    }
}
