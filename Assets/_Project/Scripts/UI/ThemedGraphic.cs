using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

namespace InternetShowdown.UI
{
    public class ThemedGraphic : ThemedElement
    {
        [Space(9)]
        public Graphic _target;
        public ThemeColor themeColor;
        [ShowIf(nameof(themeColor), ThemeColor.Custom), AllowNesting] public Color customColor = Color.white;
        [HideIf(nameof(themeColor), ThemeColor.Custom), AllowNesting] public int level;

        protected override void OnUpdate()
        {
            if (!_target)
            {
                if (TryGetComponent(out Graphic graphic)) _target = graphic;
                else Debug.LogWarning("Missing target graphic");
            }

            if (!_target) return;

            var targetColor = themeColor == ThemeColor.Custom ? customColor : theme.GetColor(themeColor, level);
            _target.color = targetColor;
        }
    }
}