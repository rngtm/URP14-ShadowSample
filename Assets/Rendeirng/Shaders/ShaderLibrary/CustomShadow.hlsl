#ifndef CUSTOM_SHADOW_HLSL_INCLUDED
#define CUSTOM_SHADOW_HLSL_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

TEXTURE2D(_CharacterShadowMapTexture);
SAMPLER(sampler_CharacterShadowMapTexture);

TEXTURE2D(_BgShadowMapTexture);
SAMPLER(sampler_BgShadowMapTexture);

float4x4 _LightVP; // ライト用のViewProjection行列
float3 _LightPos; // ライト位置

half SampleCustomShadow(float3 positionWS)
{
    // ワールド空間の座標をクリップ空間に変換
    float3 shadowCoord = mul(_LightVP, positionWS - _LightPos);

    // 範囲[-1,1]を範囲[0,1]に変換 
    shadowCoord.xy = (shadowCoord.xy * 0.5 + 0.5);
    
    // 影のレンダリング範囲内なら1.0
    half inVolume = step(shadowCoord.x, 1);
    inVolume = min(inVolume, min(step(shadowCoord.x, 1), step(0, shadowCoord.x)));
    inVolume = min(inVolume, min(step(shadowCoord.y, 1), step(0, shadowCoord.y)));

    // プラットフォームによっては、テクスチャのUVのyが反転しているので、その補正を入れる
    #if UNITY_UV_STARTS_AT_TOP 
    shadowCoord.y = 1 - shadowCoord.y;
    #endif
    shadowCoord.xy = saturate(shadowCoord.xy);

    // 頂点座標から深度値を取り出す
    float depth = shadowCoord.z; 

    // シャドウマップから深度値を取り出す
    #ifdef CHARACTER_PASS
        float shadowMapDepth = SAMPLE_TEXTURE2D(_BgShadowMapTexture, sampler_BgShadowMapTexture, shadowCoord.xy).r;
    #else
        float shadowMapDepth = max(
            SAMPLE_TEXTURE2D(_CharacterShadowMapTexture, sampler_CharacterShadowMapTexture, shadowCoord.xy).r,
            SAMPLE_TEXTURE2D(_BgShadowMapTexture, sampler_BgShadowMapTexture, shadowCoord.xy).r);
    #endif

    // プラットフォームによって、深度値の向きが異なっているため、その補正を入れる
    #if UNITY_REVERSED_Z
    depth = -depth; // near=0, far=1 となるように補正
    shadowMapDepth = 1.0 - shadowMapDepth; // near=0, far=1 となるように補正
    #endif

    // シャドウマップよりも深度が大きければ、影に入っていると判定する(atten=0)。　影に入っていなければatten=1
    half shadowAttenuation = step(depth, shadowMapDepth);

    // 影のレンダリング範囲外を0にする
    shadowAttenuation = inVolume > 0 ? shadowAttenuation : 1;
    return shadowAttenuation;
}

#endif
