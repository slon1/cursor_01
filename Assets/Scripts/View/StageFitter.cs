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

            var pixel = _canvas != null ? _canvas.pixelRect : new Rect(0f, 0f, UnityEngine.Device.Screen.width, UnityEngine.Device.Screen.height);
            var factor = _canvas != null && _canvas.scaleFactor > 0.0001f ? _canvas.scaleFactor : 1f;
            var screen = new Vector2(UnityEngine.Device.Screen.width, UnityEngine.Device.Screen.height);
            var reported = UnityEngine.Device.Screen.safeArea;
            var safe = reported;
            if (screen.x > 1f && screen.y > 1f && (Mathf.Abs(screen.x - pixel.width) > 2f || Mathf.Abs(screen.y - pixel.height) > 2f))
            {
                var sx = pixel.width / screen.x;
                var sy = pixel.height / screen.y;
                safe = new Rect(reported.x * sx, reported.y * sy, reported.width * sx, reported.height * sy);
            }
            else
            {
                safe.position -= pixel.position;
            }

            var width = safe.width / factor;
            var height = safe.height / factor;
            if (width <= 1f || height <= 1f)
                return;

            var scale = Mathf.Min(width / StageWidth, height / StageHeight);
            var center = new Vector2(safe.center.x - pixel.width * 0.5f, safe.center.y - pixel.height * 0.5f);
            stage.anchorMin = stage.anchorMax = new Vector2(0.5f, 0.5f);
            stage.pivot = new Vector2(0.5f, 0.5f);
            stage.sizeDelta = new Vector2(StageWidth, StageHeight);
            stage.anchoredPosition = center / factor;
            stage.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
