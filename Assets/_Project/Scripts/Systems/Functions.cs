using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace InternetShowdown.Systems
{
    public class Functions : LocalSystem<Functions>
    {
        [SerializeField] private UniversalRendererData _rendererData;
        [SerializeField] private VolumeProfile _volumeProfile;
        [SerializeField] private UniversalRenderPipelineAsset _renderPipelineAsset;
        public static bool DodsterMode { get; private set; } = false;

        protected override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                DodsterMode = !DodsterMode;
                foreach (var rendererFeature in _rendererData.rendererFeatures)
                {
                    rendererFeature.SetActive(!DodsterMode);
                }

                foreach (var component in _volumeProfile.components)
                {
                    component.active = !DodsterMode;
                }

                _renderPipelineAsset.renderScale = DodsterMode ? 0.8f : 1f;
            }
        }
    }
}