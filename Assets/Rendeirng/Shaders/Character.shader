Shader "Unlit/Character"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        _ShadowColor ("Shadow Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Pass
        {
            Tags { "LightMode" = "SRPDefaultUnlit" }
            
            HLSLPROGRAM
            #define CHARACTER_PASS
            
            #pragma vertex vert
            #pragma fragment frag
            #include "ShaderLibrary/CharacterPass.hlsl"
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
