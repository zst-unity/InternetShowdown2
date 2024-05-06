using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine.UI;
using System.Collections.Generic;
using NaughtyAttributes;
using System.Linq;

namespace InternetShowdown.UI
{
    public class ThemedButton : ThemedElement, IPointerEnterHandler, IPointerExitHandler
    {
        public List<GraphicProperties> normalProperties = new();
        public List<GraphicProperties> hoverProperties = new();

        protected override void OnUpdate()
        {
            ValidateProperties(normalProperties);
            ValidateProperties(hoverProperties);

            foreach (var prop in normalProperties)
            {
                if (!prop.target) continue;
                prop.target.color = prop.themeColor == ThemeColor.Custom ? prop.customColor : theme.GetColor(prop.themeColor, prop.level);
            }
        }

        private void ValidateProperties(List<GraphicProperties> properties)
        {
            foreach (var prop1 in properties)
            {
                if (!prop1.target) Debug.LogWarning($"Missing target graphic for properties");

                var count = properties.Count(prop2 => prop2.target == prop1.target);
                if (count > 1)
                {
                    Debug.LogWarning($"Found multiple properties for {prop1.target.name}");
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            TweenProperties(hoverProperties);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TweenProperties(normalProperties);
        }

        private void TweenProperties(List<GraphicProperties> properties)
        {
            foreach (var prop in properties)
            {
                if (!prop.target) continue;

                var targetColor = prop.themeColor == ThemeColor.Custom ? prop.customColor : theme.GetColor(prop.themeColor, prop.level);
                prop.colorTween.Kill();
                prop.colorTween = prop.target.DOColor(targetColor, prop.duration).SetEase(prop.ease);
            }
        }

        [Serializable]
        public class GraphicProperties
        {
            public Graphic target;

            [Header("Transition")]
            public float duration = 0.15f;
            public Ease ease = Ease.InOutSine;

            [Header("Properties")]
            public ThemeColor themeColor;
            [ShowIf(nameof(themeColor), ThemeColor.Custom), AllowNesting] public Color customColor = Color.white;
            [HideIf(nameof(themeColor), ThemeColor.Custom), AllowNesting] public int level;

            internal TweenerCore<Color, Color, ColorOptions> colorTween;
        }
    }
}