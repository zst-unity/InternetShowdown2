using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace InternetShowdown.Menu
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private UniversalRendererData _rendererData;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;

            var transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var transform in transforms)
            {
                if (transform.gameObject.CompareTag("NetworkSystem"))
                {
                    transform.gameObject.SetActive(true);
                }
            }

            foreach (var rendererFeature in _rendererData.rendererFeatures)
            {
                rendererFeature.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            foreach (var rendererFeature in _rendererData.rendererFeatures)
            {
                rendererFeature.SetActive(true);
            }
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
