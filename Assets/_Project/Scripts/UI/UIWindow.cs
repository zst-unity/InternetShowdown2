using UnityEngine;

namespace InternetShowdown.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIWindow : MonoBehaviour
    {
        [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
        public bool IsVisible { get; private set; }

        private void OnValidate()
        {
            if (TryGetComponent(out CanvasGroup group)) CanvasGroup = group;
        }

        private void Awake()
        {
            Set(false, false);
        }

        public void Show(bool tween = true)
        {
            Set(true, tween);
        }

        public void Hide(bool tween = true)
        {
            Set(false, tween);
        }

        private void Set(bool value, bool tween)
        {
            if (IsVisible == value) return;

            IsVisible = value;
            CanvasGroup.alpha = value ? 1f : 0f;
            CanvasGroup.interactable = value;
            CanvasGroup.blocksRaycasts = value;
        }
    }
}