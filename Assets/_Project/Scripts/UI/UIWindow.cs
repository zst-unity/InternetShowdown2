using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace InternetShowdown.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIWindow : MonoBehaviour
    {
        private TweenerCore<float, float, FloatOptions> _alphaTween;
        private TweenerCore<Vector3, Vector3, VectorOptions> _scaleTween;

        [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
        public bool IsVisible { get; private set; }

        private void OnValidate()
        {
            if (TryGetComponent(out CanvasGroup group)) CanvasGroup = group;
        }

        private void Awake()
        {
            Hide(false);
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
            if (tween && IsVisible == value) return;

            IsVisible = value;
            CanvasGroup.interactable = value;
            CanvasGroup.blocksRaycasts = value;

            if (tween)
            {
                _alphaTween.Kill();
                _alphaTween = CanvasGroup.DOFade(value ? 1f : 0f, 0.2f).SetEase(Ease.OutCirc);

                _scaleTween.Kill();
                _scaleTween = CanvasGroup.transform.DOScale(value ? 1f : 1.05f, 0.3f).SetEase(Ease.OutCirc);
            }
            else
            {
                CanvasGroup.alpha = value ? 1f : 0f;
                CanvasGroup.transform.localScale = Vector3.one * (value ? 1f : 1.1f);
            }
        }

        private void OnDestroy()
        {
            _alphaTween.Kill();
            _scaleTween.Kill();
        }
    }
}