using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace InternetShowdown.UI
{
    public class ThemedButton : ThemedElement, IPointerEnterHandler, IPointerExitHandler
    {
        public Dictionary<Graphic, GraphicStateParameters> normalParameters = new();
        public Dictionary<Graphic, GraphicStateParameters> hoverParameters = new();

        public ThemedButtonState State { get; private set; }

        protected override void OnUpdate()
        {
            ValidateParameters(normalParameters);
            ValidateParameters(hoverParameters);

            if (State != ThemedButtonState.Normal) return;
            foreach (var (target, parameters) in normalParameters)
            {
                if (!target) continue;
                target.color = parameters.themeColor == ThemeColor.Custom ? parameters.customColor : theme.GetColor(parameters.themeColor, parameters.level);
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
            State = ThemedButtonState.Hovered;
            TweenProperties(hoverParameters);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            State = ThemedButtonState.Normal;
            TweenProperties(normalParameters);
        }

        private void TweenProperties(Dictionary<Graphic, GraphicStateParameters> parametersPairs)
        {
            foreach (var (target, parameters) in parametersPairs)
            {
                if (!target) continue;

                var targetColor = parameters.themeColor == ThemeColor.Custom ? parameters.customColor : theme.GetColor(parameters.themeColor, parameters.level);
                parameters.colorTween.Kill();
                parameters.colorTween = target.DOColor(targetColor, parameters.duration).SetEase(parameters.ease);

                parameters.scaleTween.Kill();
                parameters.scaleTween = target.rectTransform.DOScale(parameters.scale, parameters.duration).SetEase(parameters.ease);
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
            public Vector2 scale = Vector2.one;

            internal TweenerCore<Color, Color, ColorOptions> colorTween;
            internal TweenerCore<Vector3, Vector3, VectorOptions> scaleTween;
        }
    }

    public enum ThemedButtonState
    {
        Normal,
        Hovered,
    }
}