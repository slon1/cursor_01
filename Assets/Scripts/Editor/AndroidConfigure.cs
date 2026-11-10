using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Game.EditorTools
{
    public static class AndroidConfigure
    {
        public const string ProductName = "Marit King";
        public const string PackageName = "com.slon1.maritking";
        public const string Version = "1.0.0";
        public const int VersionCode = 1;

        [MenuItem("Tools/Build/Configure Android")]
        public static void Apply()
        {
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = Version;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageName);
            PlayerSettings.Android.bundleVersionCode = VersionCode;
            // Unity 6000.5 поддерживает Android с API 26. API 24 редактор отвергает.
            if (PlayerSettings.Android.minSdkVersion != AndroidSdkVersions.AndroidApiLevel26)
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Low);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.Android.renderOutsideSafeArea = true;

            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
            EditorUserBuildSettings.buildAppBundle = true;
            SetAndroidProfile(CompressionType.Lz4HC, MobileTextureSubtarget.ASTC, true);
            AssetDatabase.SaveAssets();
            Debug.Log("Android настроен: " + ProductName + " " + PackageName + " " + Version);
        }

        public static void SetAndroidProfile(CompressionType compression, MobileTextureSubtarget textures, bool appBundle)
        {
            var guids = AssetDatabase.FindAssets("t:BuildProfile");
            for (var i = 0; i < guids.Length; i++)
            {
                var asset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (asset == null || asset.name.IndexOf("Android", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                var so = new SerializedObject(asset);
                var compressionSet = SetInt(so, "m_PlatformBuildProfile.m_CompressionType", (int)compression);
                var textureSet = SetInt(so, "m_PlatformBuildProfile.m_BuildSubtarget", (int)textures);
                var bundleSet = SetInt(so, "m_PlatformBuildProfile.m_BuildAppBundle", appBundle ? 1 : 0);
                var changed = compressionSet || textureSet || bundleSet;
                if (!compressionSet || !textureSet || !bundleSet)
                    changed |= Walk(so, compression, textures, appBundle);
                if (changed)
                {
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(asset);
                }
            }
        }

        static bool SetInt(SerializedObject so, string path, int value)
        {
            var property = so.FindProperty(path);
            if (property == null)
                return false;
            if (property.propertyType != SerializedPropertyType.Integer && property.propertyType != SerializedPropertyType.Enum)
                return false;
            if (property.intValue == value)
                return true;
            property.intValue = value;
            return true;
        }

        static bool Walk(SerializedObject so, CompressionType compression, MobileTextureSubtarget textures, bool appBundle)
        {
            var changed = false;
            var iterator = so.GetIterator();
            var enter = true;
            while (iterator.Next(enter))
            {
                enter = false;
                if (iterator.propertyType != SerializedPropertyType.Integer && iterator.propertyType != SerializedPropertyType.Enum)
                    continue;
                if (iterator.name == "m_CompressionType" && iterator.intValue != (int)compression)
                {
                    iterator.intValue = (int)compression;
                    changed = true;
                }
                else if (iterator.name == "m_BuildSubtarget" && iterator.intValue != (int)textures)
                {
                    iterator.intValue = (int)textures;
                    changed = true;
                }
                else if (iterator.name == "m_BuildAppBundle" && iterator.intValue != (appBundle ? 1 : 0))
                {
                    iterator.intValue = appBundle ? 1 : 0;
                    changed = true;
                }
            }

            return changed;
        }
    }
}
