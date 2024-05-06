using UnityEngine;

namespace InternetShowdown.UI
{
    [ExecuteInEditMode]
    public abstract class ThemedElement : MonoBehaviour
    {
        public Theme theme;

        protected virtual void OnUpdate() { }

        protected virtual void OnValidate()
        {
            if (!theme) Debug.LogWarning("Missing theme");
        }

        public void Update()
        {
            if (!theme) return;
            OnUpdate();
        }
    }
}