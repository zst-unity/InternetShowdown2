using UnityEngine;

namespace InternetShowdown.Menu
{
    public class MenuEnter : MonoBehaviour
    {
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
        }
    }
}
