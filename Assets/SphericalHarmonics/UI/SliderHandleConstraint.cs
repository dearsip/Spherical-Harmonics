using UnityEngine;

namespace SphericalHarmonics.UI
{
    [DisallowMultipleComponent]
    internal sealed class SliderHandleConstraint : MonoBehaviour
    {
        private const float Width = 24f;
        private const float Height = 30f;

        private void LateUpdate()
        {
            RectTransform rect = (RectTransform)transform;
            Vector2 min = rect.anchorMin;
            Vector2 max = rect.anchorMax;
            min.y = .5f;
            max.y = .5f;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0f);
            rect.sizeDelta = new Vector2(Width, Height);
        }
    }
}
