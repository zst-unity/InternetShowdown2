using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace InternetShowdown.UI
{
    public class ThemedGraphic : ThemedElement
    {
        [field: Space(9)]
        [field: SerializeField] public Graphic Target { get; private set; }
        public ThemeColor themeColor;
        [ShowIf(nameof(themeColor), ThemeColor.Custom)] public Color customColor = Color.white;
        [HideIf(nameof(themeColor), ThemeColor.Custom)] public int level;
        [HideIf(nameof(themeColor), ThemeColor.Custom), Range(0f, 1f)] public float alpha = 1f;

        protected override void OnUpdate()
        {
            if (!Target)
            {
                if (TryGetComponent(out Graphic graphic)) Target = graphic;
                else Debug.LogWarning("Missing target graphic");
            }

            if (!Target) return;

            var targetColor = themeColor == ThemeColor.Custom ? customColor : theme.GetColor(themeColor, level, alpha);
            Target.color = targetColor;
        }
    }
}