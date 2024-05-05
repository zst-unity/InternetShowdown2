using System;
using UnityEngine;

namespace InternetShowdown.UI
{
    [ExecuteAlways]
    public class Split : MonoBehaviour
    {
        [SerializeField] private RectTransform _first;
        [SerializeField] private RectTransform _second;
        [SerializeField] private SplitDirection _direction;
        [SerializeField, Range(0f, 1f)] private float _ratio = 0.5f;

        private void OnValidate()
        {
            if (!_first) throw new ArgumentException("Missing first element");
            if (!_second) throw new ArgumentException("Missing second element");
        }

        private void Update()
        {
            if (!_first || !_second) return;

            if (_direction == SplitDirection.Horizontal)
            {
                _first.pivot = new(0f, 0.5f);
                _second.pivot = new(1f, 0.5f);

                _first.anchorMin = new(0f, 0f);
                _first.anchorMax = new(_ratio, 1f);
                _first.offsetMin = new(_first.offsetMin.x, 0f);
                _first.offsetMax = new(_first.offsetMax.x, 0f);
                _first.sizeDelta = Vector2.zero;

                _second.anchorMin = new(_ratio, 0f);
                _second.anchorMax = new(1f, 1f);
                _second.offsetMin = new(_second.offsetMin.x, 0f);
                _second.offsetMax = new(_second.offsetMax.x, 0f);
                _second.sizeDelta = Vector2.zero;
            }
            else if (_direction == SplitDirection.Vertical)
            {
                _second.pivot = new(0.5f, 0f);
                _second.pivot = new(0.5f, 1f);

                _first.anchorMin = new(0f, 0f);
                _first.anchorMax = new(1f, _ratio);
                _first.offsetMin = new(0f, _first.offsetMax.y);
                _first.offsetMax = new(0f, _first.offsetMin.y);
                _first.sizeDelta = Vector2.zero;

                _second.anchorMin = new(0f, _ratio);
                _second.anchorMax = new(1f, 1f);
                _second.offsetMin = new(0f, _second.offsetMax.y);
                _second.offsetMax = new(0f, _second.offsetMin.y);
                _second.sizeDelta = Vector2.zero;
            }
        }
    }

    public enum SplitDirection
    {
        Horizontal,
        Vertical
    }
}