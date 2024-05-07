using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace InternetShowdown.UI
{
    [ExecuteAlways]
    public class TextSizer : MonoBehaviour
    {
        public bool ResizeTextObject = true;
        public Vector2 Padding;
        public Vector2 MaxSize = new(1000, float.PositiveInfinity);
        public Vector2 MinSize;
        public Mode ControlAxes = Mode.Both;

        [Flags]
        public enum Mode
        {
            None = 0,
            Horizontal = 0x1,
            Vertical = 0x2,
            Both = Horizontal | Vertical
        }

        private RectTransform _rectTransform;

        protected virtual float MinX
        {
            get
            {
                if ((ControlAxes & Mode.Horizontal) != 0) return MinSize.x;
                return _rectTransform.rect.width - Padding.x;
            }
        }
        protected virtual float MinY
        {
            get
            {
                if ((ControlAxes & Mode.Vertical) != 0) return MinSize.y;
                return _rectTransform.rect.height - Padding.y;
            }
        }
        protected virtual float MaxX
        {
            get
            {
                if ((ControlAxes & Mode.Horizontal) != 0) return MaxSize.x;
                return _rectTransform.rect.width - Padding.x;
            }
        }
        protected virtual float MaxY
        {
            get
            {
                if ((ControlAxes & Mode.Vertical) != 0) return MaxSize.y;
                return _rectTransform.rect.height - Padding.y;
            }
        }

        protected virtual void UpdateElement()
        {
            var texts = GetComponentsInChildren<TMP_Text>().Where(text => text.gameObject != gameObject && text.transform.parent == transform).ToArray();

            var widestPreferredSize = Vector2.zero;
            foreach (var text in texts)
            {
                var preferredSize = text.GetPreferredValues(MaxX, MaxY);
                preferredSize.x = Mathf.Clamp(preferredSize.x, MinX, MaxX);
                preferredSize.y = Mathf.Clamp(preferredSize.y, MinY, MaxY);
                preferredSize += Padding;

                if (preferredSize.x > widestPreferredSize.x) widestPreferredSize.x = preferredSize.x;
                if (preferredSize.y > widestPreferredSize.y) widestPreferredSize.y = preferredSize.y;

                if ((ControlAxes & Mode.Horizontal) != 0)
                {
                    _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, widestPreferredSize.x);
                    if (ResizeTextObject)
                    {
                        text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
                    }
                }
                if ((ControlAxes & Mode.Vertical) != 0)
                {
                    _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, widestPreferredSize.y);
                    if (ResizeTextObject)
                    {
                        text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredSize.y);
                    }
                }
            }
        }

        private void OnEnable()
        {
            _rectTransform = GetComponent<RectTransform>();

#if UNITY_EDITOR
            ObjectChangeEvents.changesPublished += ChangesPublished;
#endif
        }

#if UNITY_EDITOR
        private void OnDisable()
        {
            ObjectChangeEvents.changesPublished -= ChangesPublished;
        }

        private void ChangesPublished(ref ObjectChangeEventStream stream)
        {
            for (int i = 0; i < stream.length; ++i)
            {
                UpdateElement();
            }
        }
#endif
    }
}