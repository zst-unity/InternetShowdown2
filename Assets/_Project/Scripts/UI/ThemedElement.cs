using System;
using UnityEngine;
using UnityEngine.UI;

namespace InternetShowdown.UI
{
    [ExecuteInEditMode]
    public class ThemedElement : MonoBehaviour
    {
        [SerializeField] private Graphic _target;

        [Space(9)]
        public Theme theme;
        public ThemeColor color;
        public int level;

        private void OnValidate()
        {
            if (!_target)
            {
                if (TryGetComponent(out Graphic target))
                {
                    _target = target;
                }
                else throw new ArgumentException("Missing target");
            }
            if (!theme) throw new ArgumentException("Missing theme");
        }

        public void Update()
        {
            if (!theme || !_target) return;

            var targetColor = theme.GetColor(color, level);
            _target.color = targetColor;
        }
    }
}