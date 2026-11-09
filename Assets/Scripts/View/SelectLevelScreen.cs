using System.Globalization;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Config;
using Game.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.View
{
    public sealed class SelectLevelScreen : MonoBehaviour, IAppScreen
    {
        [SerializeField] Button back;
        [SerializeField] TMP_Text balance;
        [SerializeField] Button[] nodes;
        [SerializeField] Vector2[] nodePositions;
        [SerializeField] Image glow;
        [SerializeField] GameObject popup;
        [SerializeField] Button dim;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text loot;
        [SerializeField] Button closePopup;
        [SerializeField] Button enter;

        AppContext _app;
        int _selected;

        public void Open(AppContext app, object args)
        {
            _app = app;
            back.onClick.AddListener(() => app.Screens.BackAsync().Forget());
            dim.onClick.AddListener(ClosePopup);
            closePopup.onClick.AddListener(ClosePopup);
            enter.onClick.AddListener(() => app.Screens.ShowAsync(ScreenId.Game, app.Levels.Get(_selected)).Forget());
            popup.SetActive(false);
            glow.gameObject.SetActive(false);
            glow.sprite = app.Catalog.Get(SpriteId.Fx_LevelGlow);

            balance.font = app.Theme.Heading;
            balance.color = app.Theme.Gold;
            balance.text = app.Progress.BalancePoints.ToString("N0", CultureInfo.InvariantCulture);
            title.font = app.Theme.Heading;
            title.color = app.Theme.Cream;
            loot.font = app.Theme.Heading;
            loot.color = app.Theme.Gold;

            for (var i = 0; i < nodes.Length; i++)
            {
                var level = i + 1;
                var rect = (RectTransform)nodes[i].transform;
                if (nodePositions != null && i < nodePositions.Length)
                    rect.anchoredPosition = nodePositions[i];

                var open = RunFlow.NodeOpen(app.Progress, app.Levels.Has(level), level);
                nodes[i].image.sprite = app.Catalog.GetLevelButton(level, !open);
                var captured = level;
                nodes[i].onClick.AddListener(() => OnNode(captured));
            }
        }

        void OnNode(int level)
        {
            var open = RunFlow.NodeOpen(_app.Progress, _app.Levels.Has(level), level);
            var rect = (RectTransform)nodes[level - 1].transform;
            if (!open)
            {
                rect.DOShakeAnchorPos(_app.Settings.PopupDuration, new Vector2(14f, 0f), 14);
                return;
            }

            _selected = level;
            title.text = "VAULT " + level.ToString("00", CultureInfo.InvariantCulture);
            var config = _app.Levels.Get(level);
            loot.text = (config.MinCoins * _app.Settings.GoldPointsPerCoin).ToString(CultureInfo.InvariantCulture);
            popup.SetActive(true);
            glow.transform.SetParent(rect.parent, false);
            glow.rectTransform.anchoredPosition = rect.anchoredPosition;
            glow.rectTransform.sizeDelta = rect.sizeDelta * 1.45f;
            var index = rect.GetSiblingIndex();
            glow.transform.SetSiblingIndex(index);
            rect.SetSiblingIndex(glow.transform.GetSiblingIndex() + 1);
            glow.gameObject.SetActive(true);
            glow.transform.DOKill();
            glow.transform.localScale = Vector3.one;
            glow.transform.DOScale(1.08f, _app.Settings.PopupDuration).SetLoops(-1, LoopType.Yoyo);
        }

        void ClosePopup()
        {
            popup.SetActive(false);
            glow.transform.DOKill();
            glow.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (glow != null)
                glow.transform.DOKill();
        }
    }
}
