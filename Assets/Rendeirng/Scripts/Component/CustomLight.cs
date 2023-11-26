using Rendering.Scripts.Data;

namespace Rendering.Scripts.Component
{
    using UnityEngine;

    [ExecuteAlways]
    public class CustomLight : MonoBehaviour
    {
        public static CustomLight Instance { get; private set; }

        /// <summary>
        /// ライト設定
        /// </summary>
        [SerializeField] private CustomShadowSettings settings = new CustomShadowSettings();
        
        /// <inheritdoc cref="settings"/>
        public CustomShadowSettings Settings => settings;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDrawGizmos()
        {
            var center = new Vector3(0, 0, (settings.far + settings.near) / 2f);
            var size = new Vector3(
                settings.orthographicSize,
                settings.orthographicSize,
                settings.far - settings.near);
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
