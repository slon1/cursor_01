using Game.View;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.EditorTools
{
    public static class ScreenFieldWire
    {
        public static void WirePart0()
        {
            WireMenu();
            WireSettings();
            WireRules();
            AssetDatabase.SaveAssets();
            Debug.Log("Part 0 screen fields wired.");
        }

        static void WireMenu()
        {
            var root = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Screens/MenuScreen.prefab");
            var screen = root.GetComponent<MenuScreen>();
            var stage = root.transform.Find("Stage");
            var so = new SerializedObject(screen);
            so.FindProperty("play").objectReferenceValue = stage.Find("Btn_Menu_Play").GetComponent<Button>();
            so.FindProperty("rules").objectReferenceValue = stage.Find("Btn_Menu_Rules").GetComponent<Button>();
            so.FindProperty("settings").objectReferenceValue = stage.Find("Btn_Menu_Settings").GetComponent<Button>();
            var haul = stage.Find("HaulValue").GetComponent<TMP_Text>();
            haul.fontSize = 59f;
            so.FindProperty("haulValue").objectReferenceValue = haul;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Screens/MenuScreen.prefab");
            PrefabUtility.UnloadPrefabContents(root);
        }

        static void WireSettings()
        {
            var root = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Screens/SettingsScreen.prefab");
            var screen = root.GetComponent<SettingsScreen>();
            var stage = root.transform.Find("Stage");
            var so = new SerializedObject(screen);
            AssignButton(so, stage, "musicButton", "musicImage", "Switch_Music");
            AssignButton(so, stage, "soundButton", "soundImage", "Switch_Sound");
            AssignButton(so, stage, "vibrationButton", "vibrationImage", "Switch_Vibration");
            so.FindProperty("rules").objectReferenceValue = stage.Find("Btn_Settings_Rules").GetComponent<Button>();
            so.FindProperty("back").objectReferenceValue = stage.Find("Btn_Settings_Back").GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Screens/SettingsScreen.prefab");
            PrefabUtility.UnloadPrefabContents(root);
        }

        static void AssignButton(SerializedObject so, Transform stage, string button, string image, string name)
        {
            var child = stage.Find(name);
            so.FindProperty(button).objectReferenceValue = child.GetComponent<Button>();
            so.FindProperty(image).objectReferenceValue = child.GetComponent<Image>();
        }

        static void WireRules()
        {
            var root = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Screens/RulesScreen.prefab");
            var screen = root.GetComponent<RulesScreen>();
            var so = new SerializedObject(screen);
            so.FindProperty("back").objectReferenceValue = root.transform.Find("Stage/Btn_Rules_Back").GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Screens/RulesScreen.prefab");
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
