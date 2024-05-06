using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine.UI;
using System.Collections.Generic;

namespace InternetShowdown.UI
{
    public class ThemedButton : ThemedElement, IPointerEnterHandler, IPointerExitHandler
    {
        public List<ThemedButtonTarget> _targets;
        private TweenerCore<Color, Color, ColorOptions> _colorTween;

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {

        }

        public void OnPointerExit(PointerEventData eventData)
        {

        }
    }

    [Serializable]
    public class ThemedButtonTarget
    {
        public Graphic _target;
        [Header("States")]
        public ThemedButtonState normalState = new();
        public ThemedButtonState hoverState = new();
    }

    [Serializable]
    public class ThemedButtonState
    {
        public ThemeColor color = ThemeColor.Primary;
        public int level = 0;
        public Vector2 scale = Vector2.one;
        public Vector2 offset = Vector2.zero;

        [Space(9)]
        public ThemedButtonTransition colorTransition = new();
        public ThemedButtonTransition scaleTransition = new();
        public ThemedButtonTransition offsetTransition = new();

        [Space(9)]
        public UnityEvent onEnter = new();
    }

    [Serializable]
    public class ThemedButtonTransition
    {
        public float duration = 0.2f;
        public Ease ease = Ease.InOutSine;
    }
}