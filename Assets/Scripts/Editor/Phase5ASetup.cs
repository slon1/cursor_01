using System.IO;
using System.Linq;
using Game.Config;
using Game.View;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace Game.EditorTools
{
    public static class Phase5ASetup
    {
        const string FontDir = "Assets/Fonts";
        const string CinzelPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/Cinzel/static/Cinzel-Regular.ttf";
        const string InterPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/Inter/static/Inter_18pt-Regular.ttf";
        const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789/,.:•✓- ";

        public static void Run()
        {
            Directory.CreateDirectory(FontDir);
            var heading = FontAsset("Cinzel-Regular SDF", CinzelPath);
            var body = FontAsset("Inter-18pt-Regular SDF", InterPath);
            var theme = Theme(heading, body);
            var menu = Screen("MenuScreen", typeof(MenuScreen), (stage, catalog) =>
            {
                FullBleed(stage, "Bg_Menu", catalog.Get(SpriteId.Bg_Menu));
                Place(stage, "Menu_Logo", catalog.Get(SpriteId.Menu_Logo), new Vector2(-1f, 741f), false);
                Place(stage, "Btn_Menu_Play", catalog.Get(SpriteId.Btn_Menu_Play), new Vector2(-1f, -384f), true);
                Place(stage, "Btn_Menu_Rules", catalog.Get(SpriteId.Btn_Menu_Rules), new Vector2(-2f, -586f), true);
                Place(stage, "Btn_Menu_Settings", catalog.Get(SpriteId.Btn_Menu_Settings), new Vector2(-2f, -771f), true);
                Label(stage, "HaulValue", heading, theme.Gold, new Vector2(127f, -946f));
            });
            var settings = Screen("SettingsScreen", typeof(SettingsScreen), (stage, catalog) =>
            {
                FullBleed(stage, "Bg_Settings", catalog.Get(SpriteId.Bg_Settings));
                Place(stage, "Btn_Settings_Back", catalog.Get(SpriteId.Btn_Settings_Back), new Vector2(-391f, 950f), true);
                Place(stage, "Switch_Music", catalog.Get(SpriteId.Switch_On), new Vector2(272f, 205f), true);
                Place(stage, "Switch_Sound", catalog.Get(SpriteId.Switch_On), new Vector2(272f, 12f), true);
                Place(stage, "Switch_Vibration", catalog.Get(SpriteId.Switch_On), new Vector2(272f, -190f), true);
                Place(stage, "Btn_Settings_Rules", catalog.Get(SpriteId.Btn_Settings_Rules), new Vector2(-2f, -410f), true);
            });
            var rules = Screen("RulesScreen", typeof(RulesScreen), (stage, catalog) =>
            {
                FullBleed(stage, "Bg_Rules", catalog.Get(SpriteId.Bg_Rules));
                Place(stage, "Rules_Frame", catalog.Get(SpriteId.Rules_Frame), new Vector2(-1f, 12f), false);
                Place(stage, "Btn_Rules_Back", catalog.Get(SpriteId.Btn_Rules_Back), new Vector2(-391f, 950f), true);
            });
            var select = Screen("SelectLevelScreen", null, (stage, catalog) => { });
            var game = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Screens/GameScreen.prefab");
            WireBoot(theme, menu, settings, rules, select, game);
            WireGameDev(theme);
            var scenes = EditorBuildSettings.scenes.Where(scene => scene.path == "Assets/Scenes/Boot.unity").ToArray();
            EditorBuildSettings.scenes = scenes;
            AssetDatabase.SaveAssets();
            Debug.Log("Phase 5A setup done.");
        }

        static TMP_FontAsset FontAsset(string name, string source)
        {
            var path = FontDir + "/" + name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (existing != null)
                return existing;

            var font = AssetDatabase.LoadAssetAtPath<Font>(source);
            var asset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
            asset.name = name;
            asset.TryAddCharacters(Characters);
            AssetDatabase.CreateAsset(asset, path);
            if (asset.atlasTextures != null && asset.atlasTextures.Length > 0 && asset.atlasTextures[0] != null)
            {
                asset.atlasTextures[0].name = name + " Atlas";
                AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
            }

            if (asset.material != null)
            {
                asset.material.name = name + " Material";
                AssetDatabase.AddObjectToAsset(asset.material, asset);
            }

            EditorUtility.SetDirty(asset);
            return asset;
        }

        static UiTheme Theme(TMP_FontAsset heading, TMP_FontAsset body)
        {
            const string path = "Assets/ScriptableObjects/UiTheme.asset";
            var theme = AssetDatabase.LoadAssetAtPath<UiTheme>(path);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<UiTheme>();
                AssetDatabase.CreateAsset(theme, path);
            }

            theme.Heading = heading;
            theme.Body = body;
            theme.Cream = new Color(0.953f, 0.898f, 0.710f, 1f);
            theme.Gold = new Color(0.910f, 0.769f, 0.408f, 1f);
            theme.Dim = new Color(0f, 0f, 0f, 0.6f);
            EditorUtility.SetDirty(theme);
            return theme;
        }

        static GameObject Screen(string name, System.Type presenter, System.Action<Transform, SpriteCatalog> build)
        {
            var path = "Assets/Prefabs/Screens/" + name + ".prefab";
            var catalog = AssetDatabase.LoadAssetAtPath<SpriteCatalog>("Assets/ScriptableObjects/SpriteCatalog.asset");
            var root = new GameObject(name, typeof(RectTransform), typeof(StageFitter));
            if (presenter != null)
                root.AddComponent(presenter);

            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var stageGo = new GameObject("Stage", typeof(RectTransform));
            stageGo.transform.SetParent(root.transform, false);
            var stage = stageGo.GetComponent<RectTransform>();
            stage.anchorMin = stage.anchorMax = new Vector2(0.5f, 0.5f);
            stage.pivot = new Vector2(0.5f, 0.5f);
            stage.sizeDelta = new Vector2(StageFitter.StageWidth, StageFitter.StageHeight);
            var fitter = root.GetComponent<StageFitter>();
            var serialized = new SerializedObject(fitter);
            serialized.FindProperty("stage").objectReferenceValue = stage;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            build(stage, catalog);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static void FullBleed(Transform stage, string name, Sprite sprite) => Paint(stage, name, sprite, Vector2.zero, false, true);
        static void Place(Transform stage, string name, Sprite sprite, Vector2 pos, bool button) => Paint(stage, name, sprite, pos, button, false);

        static void Paint(Transform stage, string name, Sprite sprite, Vector2 pos, bool button, bool stretch)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(stage, false);
            var rect = go.GetComponent<RectTransform>();
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = !stretch;
            if (stretch)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                image.raycastTarget = false;
            }
            else
            {
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = pos;
                rect.sizeDelta = sprite.rect.size;
                image.raycastTarget = button;
            }

            if (button)
            {
                var control = go.AddComponent<Button>();
                control.transition = Selectable.Transition.None;
            }
        }

        static void Label(Transform stage, string name, TMP_FontAsset font, Color color, Vector2 pos)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(stage, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(240f, 70f);
            var text = go.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.color = color;
            text.fontSize = 42f;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            text.text = "0";
        }

        static void WireBoot(UiTheme theme, GameObject menu, GameObject settings, GameObject rules, GameObject select, GameObject game)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity");
            var canvas = Object.FindAnyObjectByType<Canvas>();
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
            var manager = canvas.GetComponent<ScreenManager>();
            if (manager == null)
                manager = canvas.gameObject.AddComponent<ScreenManager>();

            var fadeGo = GameObject.Find("FadeOverlay");
            if (fadeGo == null)
            {
                fadeGo = new GameObject("FadeOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
                fadeGo.transform.SetParent(canvas.transform, false);
                var rect = fadeGo.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                var image = fadeGo.GetComponent<Image>();
                image.color = Color.black;
                image.raycastTarget = true;
                var group = fadeGo.GetComponent<CanvasGroup>();
                group.alpha = 0f;
                group.blocksRaycasts = false;
            }

            var bootstrap = canvas.GetComponent<AppBootstrap>();
            if (bootstrap == null)
                bootstrap = canvas.gameObject.AddComponent<AppBootstrap>();

            var screens = new SerializedObject(manager);
            screens.FindProperty("fade").objectReferenceValue = fadeGo.GetComponent<CanvasGroup>();
            screens.FindProperty("settings").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameSettingsConfig>("Assets/ScriptableObjects/GameSettingsConfig.asset");
            var list = screens.FindProperty("screens");
            list.arraySize = 5;
            Bind(list, 0, ScreenId.Menu, menu);
            Bind(list, 1, ScreenId.Settings, settings);
            Bind(list, 2, ScreenId.Rules, rules);
            Bind(list, 3, ScreenId.SelectLevel, select);
            Bind(list, 4, ScreenId.Game, game);
            screens.ApplyModifiedPropertiesWithoutUndo();

            var boot = new SerializedObject(bootstrap);
            boot.FindProperty("catalog").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SpriteCatalog>("Assets/ScriptableObjects/SpriteCatalog.asset");
            boot.FindProperty("levels").objectReferenceValue = AssetDatabase.LoadAssetAtPath<LevelSet>("Assets/ScriptableObjects/LevelSet.asset");
            boot.FindProperty("settings").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameSettingsConfig>("Assets/ScriptableObjects/GameSettingsConfig.asset");
            boot.FindProperty("theme").objectReferenceValue = theme;
            boot.FindProperty("screens").objectReferenceValue = manager;
            boot.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        static void Bind(SerializedProperty list, int index, ScreenId id, GameObject prefab)
        {
            var item = list.GetArrayElementAtIndex(index);
            item.FindPropertyRelative("Id").enumValueIndex = (int)id;
            item.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
        }

        static void WireGameDev(UiTheme theme)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/GameDev.unity");
            var bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            var serialized = new SerializedObject(bootstrap);
            var field = typeof(GameBootstrap).GetField("theme", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field.SetValue(bootstrap, theme);
            EditorUtility.SetDirty(bootstrap);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
