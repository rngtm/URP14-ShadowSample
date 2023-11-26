namespace Rendering.Scripts.Data
{
    /// <summary>
    /// 影の設定
    /// </summary>
    [System.Serializable]
    public class CustomShadowSettings
    {
        /// <summary>
        /// シャドウを描画するニアクリップ面
        /// </summary>
        public float near = 1f;
        
        /// <summary>
        /// シャドウを描画するファークリップ面
        /// </summary>
        public float far = 10f;
        
        /// <summary>
        /// シャドウを描画する範囲
        /// </summary>
        public float orthographicSize = 5f;
        
        /// <summary>
        /// 影用のバイアス (頂点をずらす量)
        /// </summary>
        public float shadowBias = 0f;
    }
}