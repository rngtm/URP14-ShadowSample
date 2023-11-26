Shader "Unlit/ShadowMapPreview"
{
    Properties
    {
        [Toggle(_USE_CHARACTER_SHADOW_MAP)] _UseCharacterShadowMap("Show Character Shadow Map", Int) = 1
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
            #pragma multi_compile _ _USE_CHARACTER_SHADOW_MAP
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _BgShadowMapTexture;
            sampler2D _CharacterShadowMapTexture;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                #if _USE_CHARACTER_SHADOW_MAP
                float depth = tex2D(_BgShadowMapTexture, i.uv).r;
                #else
                float depth = tex2D(_CharacterShadowMapTexture, i.uv).r;
                #endif
                
                #if UNITY_REVERSED_Z
                depth = 1.0 - depth; // near=0, far=1 となるように補正
                #endif
                
                return depth;
            }
            ENDHLSL
        }
    }
}
