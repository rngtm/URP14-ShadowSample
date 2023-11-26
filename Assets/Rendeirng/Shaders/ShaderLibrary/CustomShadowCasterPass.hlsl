#ifndef CUSTOM_SHADOW_CASTER_PASS_INCLUDED
#define CUSTOM_SHADOW_CASTER_PASS_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 
struct appdata
{
    float4 vertex : POSITION;
};

struct v2f
{
    float4 positionCS : SV_POSITION;
};

float4x4 _LightVP; // ライト用のViewProjection行列
float _ShadowBias; // 影のバイアス

v2f vert (appdata v)
{
    const float3 positionWS = TransformObjectToWorld(v.vertex);
    const float3 lightDir = _LightVP[2].xyz; // ライトの向き
    
    v2f o;
    o.positionCS = mul(_LightVP, float4(positionWS + lightDir * _ShadowBias, 1));
    return o;
}

half4 frag (v2f i) : SV_Target
{
    return 0;
}
#endif