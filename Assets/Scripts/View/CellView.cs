using System;
using Game.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.View
{
    public sealed class CellView : MonoBehaviour, IPointerClickHandler
    {
        Image _background;
        Image _item;
        Image _highlight;

        public CellPos Pos { get; private set; }
        public Image ItemImage => _item;

        public event Action<CellPos> Clicked;

        public static CellView Create(Transform parent)
        {
            var root = new GameObject("Cell", typeof(RectTransform), typeof(Image), typeof(CellView));
            root.transform.SetParent(parent, false);
            var view = root.GetComponent<CellView>();
            view._background = root.GetComponent<Image>();
            view._background.raycastTarget = true;

            var itemObject = new GameObject("Item", typeof(RectTransform), typeof(Image));
            itemObject.transform.SetParent(root.transform, false);
            var itemRect = (RectTransform)itemObject.transform;
            itemRect.anchorMin = Vector2.zero;
            itemRect.anchorMax = Vector2.one;
            itemRect.offsetMin = new Vector2(8f, 8f);
            itemRect.offsetMax = new Vector2(-8f, -8f);
            view._item = itemObject.GetComponent<Image>();
            view._item.raycastTarget = false;
            view._item.preserveAspect = true;
            view._item.enabled = false;

            var highlightObject = new GameObject("Highlight", typeof(RectTransform), typeof(Image));
            highlightObject.transform.SetParent(root.transform, false);
            var highlightRect = (RectTransform)highlightObject.transform;
            highlightRect.anchorMin = Vector2.zero;
            highlightRect.anchorMax = Vector2.one;
            highlightRect.offsetMin = Vector2.zero;
            highlightRect.offsetMax = Vector2.zero;
            view._highlight = highlightObject.GetComponent<Image>();
            view._highlight.raycastTarget = false;
            view._highlight.enabled = false;
            return view;
        }

        public void Bind(CellPos pos) => Pos = pos;

        public void SetHighlight(Sprite sprite, float alpha)
        {
            _highlight.sprite = sprite;
            _highlight.enabled = sprite != null;
            _highlight.color = new Color(1f, 1f, 1f, alpha);
        }

        public void SetVisual(Sprite ground, Sprite item)
        {
            _background.sprite = ground;
            _item.sprite = item;
            _item.enabled = item != null;
            _item.color = Color.white;
            _item.transform.localScale = Vector3.one;
        }

        public void OnPointerClick(PointerEventData eventData) => Clicked?.Invoke(Pos);
    }
}
