using UnityEngine;
using UnityEngine.UI;

namespace InternetShowdown.UI
{
    public class ThemedGraphic : ThemedElement
    {
        [Space(9)]
        public Graphic _target;
        public ThemeColor color;
        public int level;

        protected override void OnValidate()
        {
            if (!_target)
            {
                if (TryGetComponent(out Graphic graphic)) _target = graphic;
                else Debug.LogWarning("Missing target graphic");
            }
        }

        protected override void OnUpdate()
        {
            if (!_target) return;

            var targetColor = theme.GetColor(color, level);
            _target.color = targetColor;
        }
    }
}