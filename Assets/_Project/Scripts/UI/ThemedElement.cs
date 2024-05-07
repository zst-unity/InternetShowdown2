using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace InternetShowdown.UI
{
    [ExecuteAlways]
    public abstract class ThemedElement : SerializedMonoBehaviour
    {
        public Theme theme;

        protected virtual void OnUpdate() { }

        private void OnValidate()
        {
            if (!theme)
            {
                Debug.LogWarning("Missing theme");
                return;
            }
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            ObjectChangeEvents.changesPublished += ChangesPublished;
        }

        private void OnDisable()
        {
            ObjectChangeEvents.changesPublished -= ChangesPublished;
        }

        private void ChangesPublished(ref ObjectChangeEventStream stream)
        {
            for (int i = 0; i < stream.length; ++i)
            {
                UpdateElement();
            }
        }
#endif

        public void UpdateElement()
        {
            if (!theme) return;
            OnUpdate();
        }
    }
}