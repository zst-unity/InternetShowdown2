using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine.UI;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace InternetShowdown.UI
{
    public class ThemedInteractable : ThemedElement, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        public bool selectable;

        [Header("Parameters")]
        public Dictionary<Graphic, GraphicStateParameters> normalParameters = new();
        public Dictionary<Graphic, GraphicStateParameters> hoverParameters = new();
        public Dictionary<Graphic, GraphicStateParameters> pressedParameters = new();
        [ShowIf("selectable")] public Dictionary<Graphic, GraphicStateParameters> selectedParameters = new();

        [Header("Events")]
        public UnityEvent onPress = new();
        public UnityEvent onRelease = new();
        public UnityEvent onHoverEnter = new();
        public UnityEvent onHoverExit = new();
        [ShowIf("selectable")] public UnityEvent onSelect = new();
        [ShowIf("selectable")] public UnityEvent onDeselect = new();

        public ThemedInteractableState State { get; private set; }
        private bool _hovering;

        protected override void OnUpdate()
        {
            ValidateParameters(normalParameters);
            ValidateParameters(hoverParameters);

            if (State != ThemedInteractableState.Normal) return;
            foreach (var (target, parameters) in normalParameters)
            {
                if (!target) continue;
                target.color = parameters.themeColor == ThemeColor.Custom ? parameters.customColor : theme.GetColor(parameters.themeColor, parameters.level, parameters.alpha);
                target.rectTransform.localScale = parameters.scale;
            }
        }

        private void ValidateParameters(Dictionary<Graphic, GraphicStateParameters> parametersPairs)
        {
            foreach (var pair in parametersPairs)
            {
                if (!pair.Key) Debug.LogWarning($"Missing target graphic for properties");
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovering = true;

            if (State == ThemedInteractableState.Selected) return;
            State = ThemedInteractableState.Hovered;
            TweenProperties(hoverParameters);

            onHoverEnter.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;

            if (State == ThemedInteractableState.Selected) return;
            State = ThemedInteractableState.Normal;
            TweenProperties(normalParameters);

            onHoverExit.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(null, eventData);
            State = ThemedInteractableState.Pressed;
            TweenProperties(pressedParameters);

            onPress.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_hovering) return;

            if (eventData.button == PointerEventData.InputButton.Left && selectable)
            {
                EventSystem.current.SetSelectedGameObject(gameObject, eventData);
                return;
            }

            State = ThemedInteractableState.Hovered;
            TweenProperties(hoverParameters);

            onRelease.Invoke();
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (!selectable) return;

            State = ThemedInteractableState.Selected;
            TweenProperties(selectedParameters);

            onSelect.Invoke();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (!selectable) return;

            State = ThemedInteractableState.Normal;
            TweenProperties(normalParameters);

            onDeselect.Invoke();
        }

        private void TweenProperties(Dictionary<Graphic, GraphicStateParameters> parametersPairs)
        {
            foreach (var (target, parameters) in parametersPairs)
            {
                if (!target) continue;

                var targetColor = parameters.themeColor == ThemeColor.Custom ? parameters.customColor : theme.GetColor(parameters.themeColor, parameters.level, parameters.alpha);
                parameters.colorTween.Kill();
                parameters.colorTween = target.DOColor(targetColor, parameters.duration).SetEase(parameters.ease);

                parameters.scaleTween.Kill();
                parameters.scaleTween = target.rectTransform.DOScale(parameters.scale, parameters.duration).SetEase(parameters.ease);
            }
        }

        private void OnDestroy()
        {
            KillAllTweens(normalParameters);
            KillAllTweens(hoverParameters);
            KillAllTweens(pressedParameters);
            KillAllTweens(selectedParameters);
        }

        private void KillAllTweens(Dictionary<Graphic, GraphicStateParameters> parametersPairs)
        {
            foreach (var pair in parametersPairs)
            {
                pair.Value.colorTween?.Kill();
                pair.Value.scaleTween?.Kill();
            }
        }

        [Serializable]
        public class GraphicStateParameters
        {
            [Header("Transition")]
            public float duration = 0.1f;
            public Ease ease = Ease.InOutSine;

            [Header("Properties")]
            public ThemeColor themeColor;
            [ShowIf(nameof(themeColor), ThemeColor.Custom)] public Color customColor = Color.white;
            [HideIf(nameof(themeColor), ThemeColor.Custom)] public int level;
            [HideIf(nameof(themeColor), ThemeColor.Custom), Range(0f, 1f)] public float alpha = 1f;
            public Vector2 scale = Vector2.one;

            internal TweenerCore<Color, Color, ColorOptions> colorTween;
            internal TweenerCore<Vector3, Vector3, VectorOptions> scaleTween;
        }
    }

    public enum ThemedInteractableState
    {
        Normal,
        Hovered,
        Pressed,
        Selected
    }
}