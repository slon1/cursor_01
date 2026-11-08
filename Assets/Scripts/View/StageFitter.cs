using UnityEngine;

namespace Game.View
{
    public sealed class StageFitter : MonoBehaviour
    {
        public const float StageWidth = 1080f;
        public const float StageHeight = 2160f;

        [SerializeField] RectTransform stage;

        Canvas _canvas;

        void OnEnable() => Fit();

        void OnRectTransformDimensionsChange() => Fit();

        void LateUpdate() => Fit();

        void Fit()
        {
            if (stage == null)
                return;

            if (_canvas == null)
                _canvas = GetComponentInParent<Canvas>();

            var width = ((RectTransform)transform).rect.width;
            var height = ((RectTransform)transform).rect.height;
            if (_canvas != null && _canvas.scaleFactor > 0.0001f)
            {
                var visible = _canvas.pixelRect.size / _canvas.scaleFactor;
                width = visible.x;
                height = visible.y;
            }

            if (width <= 1f || height <= 1f)
                return;

            var scale = Mathf.Min(width / StageWidth, height / StageHeight);
            stage.anchorMin = stage.anchorMax = new Vector2(0.5f, 0.5f);
            stage.pivot = new Vector2(0.5f, 0.5f);
            stage.sizeDelta = new Vector2(StageWidth, StageHeight);
            stage.anchoredPosition = Vector2.zero;
            stage.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
