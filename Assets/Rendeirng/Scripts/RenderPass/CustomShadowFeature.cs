using Rendering.Scripts.Component;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using ShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution;

namespace Rendering.Scripts.RenderPass
{
    public class CustomShadowFeature : ScriptableRendererFeature
    {
        /// <summary> レンダリングのタイミング </summary>
        [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        
        /// <summary> 背景のRenderingLayerMaskのインデックス </summary>
        private int bgLayerIndex = 1;

        /// <summary> キャラクターのRenderingLayerMaskのインデックス </summary>
        private int characterLayerIndex = 2;
        
        /// <summary>
        /// RTHandleの確保・解放を行うシステム
        /// 参考 : https://docs.unity3d.com/Packages/com.unity.render-pipelines.core@14.0/manual/rthandle-system-using.html
        /// </summary>
        private RTHandleSystem rtHandleSystem; 
        
        /// <summary> 背景用のシャドウマップテクスチャ </summary>
        private RTHandle bgDepthTexture;

        /// <summary> キャラクター用のシャドウマップテクスチャ </summary>
        private RTHandle characterDepthTexture;
        
        /// <summary> 背景用のシャドウマップを描画するレンダーパス </summary>
        private CustomShadowPass bgPass;

        /// <summary> キャラクター用のシャドウマップを描画するレンダーパス </summary>
        private CustomShadowPass characterPass;

        /// <summary> ライト </summary>
        private CustomLight _light;
        
        /// <summary> シャドウマップテクスチャ解像度 </summary>
        private const int shadowMapResolution = (int)ShadowResolution._256;
        
        /// <summary> シャドウマップテクスチャ ヨコのピクセル数 </summary>
        private const int shadowMapWidth = shadowMapResolution;
        
        /// <summary> シャドウマップテクスチャ タテのピクセル数 </summary>
        private const int shadowMapHeight = shadowMapResolution;
        
        static class ShaderPropertyId
        {
            public static readonly int BgShadowMapTexture = Shader.PropertyToID("_BgShadowMapTexture");
            public static readonly int CharacterShadowMapTexture = Shader.PropertyToID("_CharacterShadowMapTexture");
        }

        /// <inheritdoc/>
        public override void Create()
        {
            if (rtHandleSystem == null)
            {
                rtHandleSystem = new RTHandleSystem();
                rtHandleSystem.Initialize(shadowMapWidth, shadowMapHeight);
            }
            
            // シャドウマップテクスチャを作成
            characterDepthTexture ??= rtHandleSystem.Alloc(
                shadowMapWidth,
                shadowMapHeight,
                depthBufferBits: DepthBits.Depth16,
                isShadowMap: true);
            bgDepthTexture ??= rtHandleSystem.Alloc(
                shadowMapWidth,
                shadowMapHeight,
                depthBufferBits: DepthBits.Depth16,
                isShadowMap: true);

            // シャドウマップテクスチャをシェーダーに渡す
            Shader.SetGlobalTexture(ShaderPropertyId.CharacterShadowMapTexture, characterDepthTexture);
            Shader.SetGlobalTexture(ShaderPropertyId.BgShadowMapTexture, bgDepthTexture);
            
            // レンダーパスの作成
            bgPass ??= new CustomShadowPass();
            characterPass ??= new CustomShadowPass();
        }
        
        /// <inheritdoc/>
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            _light = CustomLight.Instance;
            if (_light == null)
            {
                return;
            }
            
            uint bgLayer = (uint)(1 << bgLayerIndex);
            uint characterLayer = (uint)(1 << characterLayerIndex);

            bgPass.Setup(renderPassEvent, _light, bgDepthTexture,  bgLayer);
            renderer.EnqueuePass(bgPass);

            characterPass.Setup(renderPassEvent, _light, characterDepthTexture, characterLayer);
            renderer.EnqueuePass(characterPass);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Debug.Log("Dispose");
            
            rtHandleSystem?.Release(characterDepthTexture);
            characterDepthTexture = null;
            
            rtHandleSystem?.Release(bgDepthTexture);
            bgDepthTexture = null;
            
            rtHandleSystem?.Dispose();
            rtHandleSystem = null;
        }
    }
}