using Mirror;
using UnityEngine;

namespace InternetShowdown.Systems
{
    public abstract class LocalSystem<T> : MonoBehaviour where T : LocalSystem<T>
    {
        private static LocalSystem<T> _singleton;
        public static T Singleton => (T)_singleton;

        protected virtual void OnStartup() { }
        protected virtual void OnUpdate() { }

        private void Update()
        {
            OnUpdate();
        }

        private void Awake()
        {
            if (_singleton != null)
            {
                Destroy(gameObject);
            }
            else
            {
                _singleton = this;
                DontDestroyOnLoad(gameObject);
                OnStartup();
            }
        }
    }

    public abstract class NetworkSystem<T> : NetworkBehaviour where T : NetworkSystem<T>
    {
        private static NetworkSystem<T> _singleton;
        public static T Singleton => (T)_singleton;

#if UNITY_EDITOR
        private void Reset()
        {
            gameObject.tag = "NetworkSystem";
        }
#endif

        protected virtual void OnStartup() { }
        protected virtual void OnUpdate() { }

        private void Update()
        {
            OnUpdate();
        }

        private void Awake()
        {
            if (_singleton != null)
            {
                Destroy(gameObject);
            }
            else
            {
                _singleton = this;
                DontDestroyOnLoad(gameObject);
                OnStartup();
            }
        }
    }
}
