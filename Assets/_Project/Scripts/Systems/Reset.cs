using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace InternetShowdown.Systems
{
    // нужна чтобы сбрасывать поля ассетов при остановке плеймода (чтобы гит не видел изменения бесполезные)
    public class Reset : LocalSystem<Reset>
    {
        [SerializeField] private UniversalRendererData _rendererData;
        [SerializeField] private VolumeProfile _volumeProfile;
        [SerializeField] private UniversalRenderPipelineAsset _renderPipelineAsset;
        [SerializeField] private Material _speedlines;

        protected override void OnStop()
        {
            _speedlines.SetFloat("_Alpha", 0f);
            foreach (var rendererFeature in _rendererData.rendererFeatures)
            {
                rendererFeature.SetActive(true);
            }
            foreach (var component in _volumeProfile.components)
            {
                component.active = true;
            }
            _renderPipelineAsset.renderScale = 1f;
        }
    }
}