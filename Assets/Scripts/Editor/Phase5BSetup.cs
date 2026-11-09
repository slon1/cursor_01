using Game.Config;
using Game.View;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.EditorTools
{
    public static class Phase5BSetup
    {
        static readonly Vector2[] Nodes =
        {
            new Vector2(-1f, -818f),
            new Vector2(-227f, -531f),
            new Vector2(-134f, -315f),
            new Vector2(153f, -177f),
            new Vector2(-57f, 8f),
            new Vector2(235f, 135f),
            new Vector2(-16f, 281f),
            new Vector2(172f, 459f),
            new Vector2(-134f, 579f),
            new Vector2(107f, 723f)
        };

        public static void Run()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<SpriteCatalog>("Assets/ScriptableObjects/SpriteCatalog.asset");
            var heading = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Cinzel-Regular SDF.asset");
            var gold = new Color(0.910f, 0.769f, 0.408f, 1f);
            var cream = new Color(0.953f, 0.898f, 0.710f, 1f);
            BuildSelect(catalog, heading, gold, cream);
            BuildGamePopups(catalog, heading, gold);
            AssetDatabase.SaveAssets();
            Debug.Log("Phase 5B setup done.");
        }

        static void BuildSelect(SpriteCatalog catalog, TMP_FontAsset heading, Color gold, Color cream)
        {
            const string path = "Assets/Prefabs/Screens/SelectLevelScreen.prefab";
            var root = new GameObject("SelectLevelScreen", typeof(RectTransform), typeof(StageFitter), typeof(SelectLevelScreen));
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var stage = Child(root.transform, "Stage");
            stage.anchorMin = stage.anchorMax = new Vector2(0.5f, 0.5f);
            stage.sizeDelta = new Vector2(StageFitter.StageWidth, StageFitter.StageHeight);
            var fitter = root.GetComponent<StageFitter>();
            var fit = new SerializedObject(fitter);
            fit.FindProperty("stage").objectReferenceValue = stage;
            fit.ApplyModifiedPropertiesWithoutUndo();

            Stretch(stage, "Bg_SelectLevel", catalog.Get(SpriteId.Bg_SelectLevel), false);
            var glow = Image(stage, "Fx_LevelGlow", catalog.Get(SpriteId.Fx_LevelGlow), Vector2.zero, false);
            glow.gameObject.SetActive(false);
            var nodeButtons = new Button[Nodes.Length];
            for (var i = 0; i < Nodes.Length; i++)
            {
                var sprite = catalog.GetLevelButton(i + 1, false);
                nodeButtons[i] = Button(stage, "Node_" + (i + 1).ToString("00"), sprite, Nodes[i]);
            }

            var back = Button(stage, "Btn_Select_Back", catalog.Get(SpriteId.Btn_Select_Back), new Vector2(-391f, 951f));
            var frame = Image(stage, "Balance_Frame", catalog.Get(SpriteId.Balance_Frame), new Vector2(297f, 946f), false);
            var balance = Text(frame.transform, "Balance", heading, gold, new Vector2(62f, 0f), new Vector2(170f, 70f), 48f, TextAlignmentOptions.Center);

            var popup = new GameObject("Popup", typeof(RectTransform));
            popup.transform.SetParent(stage, false);
            var popupRect = popup.GetComponent<RectTransform>();
            popupRect.anchorMin = Vector2.zero;
            popupRect.anchorMax = Vector2.one;
            popupRect.offsetMin = Vector2.zero;
            popupRect.offsetMax = Vector2.zero;
            var dim = Button(popup.transform, "Dim", null, Vector2.zero);
            Stretch((RectTransform)dim.transform, dim.GetComponent<Image>(), new Color(0f, 0f, 0f, 0.6f));
            Image(popup.transform, "Popup_SelectLevel", catalog.Get(SpriteId.Popup_SelectLevel), Vector2.zero, true);
            var title = Text(popup.transform, "Title", heading, cream, new Vector2(0f, 220f), new Vector2(620f, 90f), 64f, TextAlignmentOptions.Center);
            var loot = Text(popup.transform, "Loot", heading, gold, new Vector2(150f, 40f), new Vector2(220f, 80f), 56f, TextAlignmentOptions.MidlineLeft);
            var close = Button(popup.transform, "Btn_SelectPopup_Close", catalog.Get(SpriteId.Btn_SelectPopup_Close), new Vector2(440f, 320f));
            var enter = Button(popup.transform, "Btn_SelectPopup_Enter", catalog.Get(SpriteId.Btn_SelectPopup_Enter), new Vector2(0f, -140f));
            popup.SetActive(false);

            var screen = root.GetComponent<SelectLevelScreen>();
            var serialized = new SerializedObject(screen);
            serialized.FindProperty("back").objectReferenceValue = back;
            serialized.FindProperty("balance").objectReferenceValue = balance;
            var nodes = serialized.FindProperty("nodes");
            nodes.arraySize = nodeButtons.Length;
            for (var i = 0; i < nodeButtons.Length; i++)
                nodes.GetArrayElementAtIndex(i).objectReferenceValue = nodeButtons[i];
            var positions = serialized.FindProperty("nodePositions");
            positions.arraySize = Nodes.Length;
            for (var i = 0; i < Nodes.Length; i++)
                positions.GetArrayElementAtIndex(i).vector2Value = Nodes[i];
            serialized.FindProperty("glow").objectReferenceValue = glow;
            serialized.FindProperty("popup").objectReferenceValue = popup;
            serialized.FindProperty("dim").objectReferenceValue = dim;
            serialized.FindProperty("title").objectReferenceValue = title;
            serialized.FindProperty("loot").objectReferenceValue = loot;
            serialized.FindProperty("closePopup").objectReferenceValue = close;
            serialized.FindProperty("enter").objectReferenceValue = enter;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        static void BuildGamePopups(SpriteCatalog catalog, TMP_FontAsset heading, Color gold)
        {
            const string path = "Assets/Prefabs/Screens/GameScreen.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            var presenter = root.GetComponent<GameScreenPresenter>();
            var serialized = new SerializedObject(presenter);
            var stage = (RectTransform)serialized.FindProperty("stage").objectReferenceValue;
            Remove(stage, "PauseOverlay");
            Remove(stage, "WinOverlay");
            Remove(stage, "LoseOverlay");

            var pause = Overlay(stage, "PauseOverlay");
            Dim(pause);
            Image(pause, "Popup_Pause_Frame", catalog.Get(SpriteId.Popup_Pause_Frame), Vector2.zero, true);
            var resume = Button(pause, "Btn_Pause_Resume", catalog.Get(SpriteId.Btn_Pause_Resume), new Vector2(0f, 8f));
            var restart = Button(pause, "Btn_Pause_Restart", catalog.Get(SpriteId.Btn_Pause_Restart), new Vector2(0f, -195f));
            var menu = Button(pause, "Btn_Pause_Menu", catalog.Get(SpriteId.Btn_Pause_Menu), new Vector2(0f, -366f));
            pause.gameObject.SetActive(false);

            var win = Overlay(stage, "WinOverlay");
            Stretch(win, "Popup_Win_Bg", catalog.Get(SpriteId.Popup_Win_Bg), true);
            var winLoot = Text(win, "WinLoot", heading, gold, new Vector2(100f, -250f), new Vector2(240f, 80f), 64f, TextAlignmentOptions.Center);
            var winTarget = Text(win, "WinTarget", heading, gold, new Vector2(170f, -440f), new Vector2(200f, 64f), 48f, TextAlignmentOptions.MidlineLeft);
            winTarget.rectTransform.pivot = new Vector2(0f, 0.5f);
            var winNext = Button(win, "Btn_Win_Next", catalog.Get(SpriteId.Btn_Win_Next), new Vector2(0f, -645f));
            var winSelect = Button(win, "Btn_Win_SelectLevel", catalog.Get(SpriteId.Btn_Win_SelectLevel), new Vector2(0f, -834f));

            var lose = Overlay(stage, "LoseOverlay");
            Stretch(lose, "Popup_Lose_Bg", catalog.Get(SpriteId.Popup_Lose_Bg), true);
            var loseLoot = Text(lose, "LoseLoot", heading, gold, new Vector2(100f, -250f), new Vector2(240f, 80f), 64f, TextAlignmentOptions.Center);
            var loseTarget = Text(lose, "LoseTarget", heading, gold, new Vector2(140f, -440f), new Vector2(200f, 64f), 48f, TextAlignmentOptions.MidlineLeft);
            loseTarget.rectTransform.pivot = new Vector2(0f, 0.5f);
            var loseRetry = Button(lose, "Btn_Lose_TryAgain", catalog.Get(SpriteId.Btn_Lose_TryAgain), new Vector2(0f, -645f));
            var loseSelect = Button(lose, "Btn_Lose_SelectLevel", catalog.Get(SpriteId.Btn_Lose_SelectLevel), new Vector2(0f, -834f));

            serialized.FindProperty("pauseOverlay").objectReferenceValue = pause.gameObject;
            serialized.FindProperty("resumeButton").objectReferenceValue = resume;
            serialized.FindProperty("restartButton").objectReferenceValue = restart;
            serialized.FindProperty("pauseMenuButton").objectReferenceValue = menu;
            serialized.FindProperty("winOverlay").objectReferenceValue = win.gameObject;
            serialized.FindProperty("winLoot").objectReferenceValue = winLoot;
            serialized.FindProperty("winTarget").objectReferenceValue = winTarget;
            serialized.FindProperty("winNext").objectReferenceValue = winNext;
            serialized.FindProperty("winSelect").objectReferenceValue = winSelect;
            serialized.FindProperty("loseOverlay").objectReferenceValue = lose.gameObject;
            serialized.FindProperty("loseLoot").objectReferenceValue = loseLoot;
            serialized.FindProperty("loseTarget").objectReferenceValue = loseTarget;
            serialized.FindProperty("loseRetry").objectReferenceValue = loseRetry;
            serialized.FindProperty("loseSelect").objectReferenceValue = loseSelect;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        static RectTransform Overlay(Transform parent, string name)
        {
            var rect = Child(parent, name);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        static void Dim(Transform parent)
        {
            var button = Button(parent, "Dim", null, Vector2.zero);
            Stretch((RectTransform)button.transform, button.GetComponent<Image>(), new Color(0f, 0f, 0f, 0.6f));
            button.onClick.RemoveAllListeners();
        }

        static void Remove(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
                Object.DestroyImmediate(child.gameObject);
        }

        static RectTransform Child(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        static void Stretch(Transform parent, string name, Sprite sprite, bool block)
        {
            var image = Image(parent, name, sprite, Vector2.zero, false);
            Stretch(image.rectTransform, image, Color.white);
            image.sprite = sprite;
            image.raycastTarget = block;
        }

        static void Stretch(RectTransform rect, Image image, Color color)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            image.color = color;
            image.raycastTarget = true;
        }

        static Image Image(Transform parent, string name, Sprite sprite, Vector2 pos, bool block)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = block;
            if (sprite != null)
                rect.sizeDelta = sprite.rect.size;
            return image;
        }

        static Button Button(Transform parent, string name, Sprite sprite, Vector2 pos)
        {
            var image = Image(parent, name, sprite, pos, true);
            var button = image.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = image;
            return button;
        }

        static TMP_Text Text(Transform parent, string name, TMP_FontAsset font, Color color, Vector2 pos, Vector2 size, float fontSize, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            var text = go.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.color = color;
            text.fontSize = fontSize;
            text.alignment = align;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            text.text = "0";
            return text;
        }
    }
}
