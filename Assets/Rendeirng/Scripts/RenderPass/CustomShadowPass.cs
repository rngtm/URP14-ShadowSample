using Rendering.Scripts.Component;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Scripts.RenderPass
{
    /// <summary>
    /// シャドウマップを描画するScriptableRenderPass
    /// </summary>
    public class CustomShadowPass : ScriptableRenderPass
    {
        private static readonly ProfilingSampler sampler = new ProfilingSampler("Custom Shadow");
        private static readonly ShaderTagId shaderTagId = new ShaderTagId("CustomShadow");
        
        /// <summary>
        /// シャドウマップテクスチャ
        /// </summary>
        private RTHandle _depthAttachment;
        
        /// <summary>
        /// ライト
        /// </summary>
        private CustomLight _mainLight;
        
        /// <summary>
        /// レンダリング対象のRenderingLayerMask
        /// </summary>
        private uint _renderingLayerMask;

        static class ShaderPropertyId
        {
            public static readonly int LightPos = Shader.PropertyToID("_LightPos");
            public static readonly int LightVP = Shader.PropertyToID("_LightVP");
            public static readonly int ShadowBias = Shader.PropertyToID("_ShadowBias");
        }

        /// <summary>
        /// セットアップ処理
        /// </summary>
        /// <param name="renderPassEvent">Passの実行タイミング</param>
        /// <param name="light">ライト</param>
        /// <param name="depthAttachment">深度テクスチャ</param>
        /// <param name="renderingLayerMask">レンダリング対象とするrenderingLayerMask</param>
        public void Setup(RenderPassEvent renderPassEvent, CustomLight light, RTHandle depthAttachment, uint renderingLayerMask)
        {
            this.renderPassEvent = renderPassEvent;
            _mainLight = light;
            _depthAttachment = depthAttachment;
            _renderingLayerMask = renderingLayerMask;
        }

        /// <inheritdoc/>
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            ConfigureTarget(_depthAttachment);
            ConfigureClear(ClearFlag.All, Color.clear);
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, sampler))
            {
                // ビュー行列、プロジェクション行列を設定
                Matrix4x4 viewMatrix = GetViewMatrix(_mainLight);
                Matrix4x4 projMatrix = GetProjectionMatrix(_mainLight);
                cmd.SetGlobalMatrix(ShaderPropertyId.LightVP, projMatrix * viewMatrix);
                
                // 影用のパラメータを設定
                cmd.SetGlobalFloat(ShaderPropertyId.ShadowBias, _mainLight.Settings.shadowBias);
                cmd.SetGlobalVector(ShaderPropertyId.LightPos, this._mainLight.transform.position);

                // レンダラーを実行し、オブジェクトを描画する
                var cullResults = renderingData.cullResults;
                var drawingSettings = CreateDrawingSettings(shaderTagId, ref renderingData, SortingCriteria.CommonOpaque);
                var filterSettings = new FilteringSettings(RenderQueueRange.all)
                {
                    renderingLayerMask = _renderingLayerMask
                };
                context.DrawRenderers(cullResults, ref drawingSettings, ref filterSettings);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// <summary>
        /// ビュー行列を作成
        /// </summary>
        private static Matrix4x4 GetViewMatrix(CustomLight light)
        {
            Transform lightTransform = light.transform;
            Vector4 lightX = lightTransform.right; // ライトのX軸ベクトル
            Vector4 lightY = lightTransform.up; // ライトのY軸ベクトル
            Vector4 lightZ = lightTransform.forward; // ライトのZ軸ベクトル
            Vector4 lightW = lightTransform.position; // 平行移動成分
            lightW.w = 1f;

            // ワールド空間の座標をライト空間の座標に変換する行列　
            var worldToLight = new Matrix4x4(lightX, lightY, lightZ, lightW);

            // ライト空間の座標をワールド空間の座標に変換する行列 (ビュー行列)
            return worldToLight.inverse;
        }

        /// <summary>
        /// プロジェクション行列を作成
        /// </summary>
        private static Matrix4x4 GetProjectionMatrix(CustomLight light)
        {
            var settings = light.Settings;
            return GetProjectionMatrix(settings.orthographicSize, settings.near, settings.far);
        }

        /// <summary>
        /// プロジェクション行列の作成 (Orthographic)
        /// </summary>
        /// <param name="orthographicSize">タテヨコの大きさ</param>
        /// <param name="near">ニアクリップ面</param>
        /// <param name="far">ファークリップ面</param>
        private static Matrix4x4 GetProjectionMatrix(float orthographicSize, float near, float far)
        {
            Matrix4x4 projMatrix = Matrix4x4.identity;

            // カメラのビューボリューム サイズ
            float viewSizeX = orthographicSize / 2f; // 横のサイズ
            float viewSizeY = orthographicSize / 2f; // 縦のサイズ

            // プロジェクション行列　作成
            projMatrix.m00 = 1f / viewSizeX; // x: [xmin, xmax] -> [-1, 1]
            projMatrix.m11 = 1f / viewSizeY; // y: [ymin, ymax] -> [-1, 1]

            // テクスチャに対して描画するので、UVが反転しているかどうかを考慮
            if (SystemInfo.graphicsUVStartsAtTop) // UV.yが反転 : DirectX, Vulkan, Metal
            {
                projMatrix.m11 *= -1f;
            }

            // プラットフォームによって、深度バッファの向きが異なっている
            if (SystemInfo.usesReversedZBuffer) // 逆向きの深度 : DirectX, Metal, Vulkan
            {
                // z: [n,f] -> [1, 0] 
                projMatrix.m22 = -1f / (far - near);
                projMatrix.m23 = far / (far - near);
            }
            else // 従来の向き : OpenGLESなど
            {
                // z:[n,f] -> [0, 1]
                projMatrix.m22 = 1f / (far - near);
                projMatrix.m23 = -near / (far - near);
            }

            return projMatrix;
        }
    }
}