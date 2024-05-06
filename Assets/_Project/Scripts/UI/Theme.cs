using System;
using UnityEngine;

namespace InternetShowdown.UI
{
    [CreateAssetMenu(fileName = "Theme", menuName = "Theme", order = 0)]
    public class Theme : ScriptableObject
    {
        public Color background = new Color32(57, 63, 66, 255);
        public Color text = new Color32(243, 234, 233, 255);
        public Color primary = new Color32(205, 169, 168, 255);
        public Color secondary = new Color32(51, 107, 84, 255);
        public Color accent = new Color32(221, 213, 67, 255);

        [Space(9)]
        [Range(-1f, 1f)] public float hueShift = 0f;
        [Range(-1f, 1f)] public float saturationShift = 0f;
        [Range(-1f, 1f)] public float valueShift = 0.06f;

        public Color GetColor(ThemeColor color, int level)
        {
            var origColor = color switch
            {
                ThemeColor.Background => background,
                ThemeColor.Text => text,
                ThemeColor.Primary => primary,
                ThemeColor.Secondary => secondary,
                ThemeColor.Accent => accent,
                _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
            };

            Color.RGBToHSV(origColor, out var origHue, out var origSaturation, out var origValue);

            var newHue = origHue + hueShift * level;
            var newSaturation = origSaturation + saturationShift * level;
            var newValue = origValue + valueShift * level;

            return Color.HSVToRGB(newHue, newSaturation, newValue);
        }
    }

    public enum ThemeColor
    {
        Background,
        Text,
        Primary,
        Secondary,
        Accent
    }
}