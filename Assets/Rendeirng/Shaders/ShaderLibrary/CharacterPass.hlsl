#ifndef CHARACTER_PASS_INCLUDED
#define CHARACTER_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "ShaderLibrary/CustomShadow.hlsl"

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
};

CBUFFER_START(UnityPerMaterial)
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
float4 _ShadowColor;
CBUFFER_END

v2f vert (appdata v)
{
    v2f o;
    o.positionCS = TransformObjectToHClip(v.vertex);
    o.positionWS = TransformObjectToWorld(v.vertex);
    o.uv = v.uv;
    return o;
}

half4 frag (v2f i) : SV_Target
{
    half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
    half atten = SampleCustomShadow(i.positionWS);
    color.rgb = lerp(color.rgb * _ShadowColor.rgb, color.rgb, atten);
    return color;
}
#endif