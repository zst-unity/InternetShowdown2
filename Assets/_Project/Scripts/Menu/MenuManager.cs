using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using ZSToolkit.GlobalData;

namespace InternetShowdown.Menu
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private UniversalRendererData _rendererData;
        [SerializeField] private TMP_InputField _IPAddressInputField;
        [SerializeField] private TMP_InputField _PortInputField;

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

            _IPAddressInputField.text = GlobalData.Load("MenuData", "IPAddress", "localhost");
            _PortInputField.text = GlobalData.Load("MenuData", "Port", "7777");
        }

        private void OnDestroy()
        {
            foreach (var rendererFeature in _rendererData.rendererFeatures)
            {
                rendererFeature.SetActive(true);
            }
        }

        public void Host()
        {
            NetworkManager.singleton.StartHost();
        }

        public void Join()
        {
            NetworkManager.singleton.StartClient();
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void SetIPAddress(string text)
        {
            GlobalData.Save("MenuData", "IPAddress", text);
            NetworkManager.singleton.networkAddress = text;
        }

        public void SetPort(string text)
        {
            GlobalData.Save("MenuData", "Port", text);
            (NetworkManager.singleton.transport as PortTransport).Port = ushort.Parse(text);
        }
    }
}
