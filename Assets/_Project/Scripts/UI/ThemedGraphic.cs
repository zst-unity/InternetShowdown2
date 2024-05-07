using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace InternetShowdown.UI
{
    public class ThemedGraphic : ThemedElement
    {
        [Space(9)]
        public Graphic _target;
        public ThemeColor themeColor;
        [ShowIf(nameof(themeColor), ThemeColor.Custom)] public Color customColor = Color.white;
        [HideIf(nameof(themeColor), ThemeColor.Custom)] public int level;
        [HideIf(nameof(themeColor), ThemeColor.Custom), Range(0f, 1f)] public float alpha = 1f;

        protected override void OnUpdate()
        {
            if (!_target)
            {
                if (TryGetComponent(out Graphic graphic)) _target = graphic;
                else Debug.LogWarning("Missing target graphic");
            }

            if (!_target) return;

            var targetColor = themeColor == ThemeColor.Custom ? customColor : theme.GetColor(themeColor, level, alpha);
            _target.color = targetColor;
        }
    }
}