using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Game.EditorTools
{
    public static class AndroidPlayerBuild
    {
        const string BundlePath = "Builds/MaritKing.aab";
        const string ApkPath = "Builds/MaritKing-dev.apk";

        [MenuItem("Tools/Build/Build Android Bundle (Release)")]
        public static void BuildBundle() => BuildRelease();

        [MenuItem("Tools/Build/Build Android APK (Development)")]
        public static void BuildDevelopmentApk() => BuildDevelopment();

        public static BuildSummary BuildRelease()
        {
            if (!TryReadSigning(out var path, out var alias, out var storePass, out var keyPass, out var error))
            {
                Debug.LogError(error);
                return default;
            }

            var previousCustom = PlayerSettings.Android.useCustomKeystore;
            var previousPath = PlayerSettings.Android.keystoreName;
            var previousAlias = PlayerSettings.Android.keyaliasName;
            try
            {
                AndroidConfigure.Apply();
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = path;
                PlayerSettings.Android.keyaliasName = alias;
                PlayerSettings.Android.keystorePass = storePass;
                PlayerSettings.Android.keyaliasPass = keyPass;
                EditorUserBuildSettings.buildAppBundle = true;
                EditorUserBuildSettings.development = false;
                return Run(BundlePath, BuildOptions.CompressWithLz4HC);
            }
            finally
            {
                PlayerSettings.Android.keystorePass = string.Empty;
                PlayerSettings.Android.keyaliasPass = string.Empty;
                PlayerSettings.Android.keystoreName = previousPath;
                PlayerSettings.Android.keyaliasName = previousAlias;
                PlayerSettings.Android.useCustomKeystore = previousCustom;
            }
        }

        public static BuildSummary BuildDevelopment()
        {
            AndroidConfigure.Apply();
            PlayerSettings.Android.useCustomKeystore = false;
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.development = true;
            return Run(ApkPath, BuildOptions.Development | BuildOptions.CompressWithLz4);
        }

        static BuildSummary Run(string output, BuildOptions options)
        {
            Directory.CreateDirectory("Builds");
            var scenes = EditorBuildSettings.scenes;
            var enabled = new string[scenes.Length];
            var count = 0;
            for (var i = 0; i < scenes.Length; i++)
            {
                if (!scenes[i].enabled)
                    continue;
                enabled[count++] = scenes[i].path;
            }

            var list = new string[count];
            Array.Copy(enabled, list, count);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = list,
                locationPathName = output,
                target = BuildTarget.Android,
                options = options
            });
            var summary = report.summary;
            Debug.Log("Сборка " + output + " " + summary.result + " " + summary.totalSize + " байт, предупреждений " + Count(report, LogType.Warning) + ", ошибок " + Count(report, LogType.Error));
            return summary;
        }

        static int Count(BuildReport report, LogType type)
        {
            var count = 0;
            var steps = report.steps;
            for (var i = 0; i < steps.Length; i++)
            {
                var messages = steps[i].messages;
                for (var m = 0; m < messages.Length; m++)
                {
                    if (messages[m].type == type)
                        count++;
                }
            }

            return count;
        }

        public static bool TryReadSigning(out string path, out string alias, out string storePass, out string keyPass, out string error)
        {
            path = Environment.GetEnvironmentVariable("MK_KEYSTORE_PATH");
            alias = Environment.GetEnvironmentVariable("MK_KEY_ALIAS");
            storePass = Environment.GetEnvironmentVariable("MK_KEYSTORE_PASS");
            keyPass = Environment.GetEnvironmentVariable("MK_KEY_PASS");
            if (string.IsNullOrEmpty(path) && File.Exists("keystore.local.json"))
            {
                var file = JsonUtility.FromJson<KeystoreFile>(File.ReadAllText("keystore.local.json"));
                if (file != null)
                {
                    path = file.path;
                    alias = file.alias;
                    storePass = file.keystorePass;
                    keyPass = file.keyPass;
                }
            }

            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(alias) || string.IsNullOrEmpty(storePass) || string.IsNullOrEmpty(keyPass))
            {
                error = "Нет подписи релиза. Нужен keystore.local.json с полями path, alias, keystorePass, keyPass или переменные MK_KEYSTORE_PATH, MK_KEYSTORE_PASS, MK_KEY_ALIAS, MK_KEY_PASS.";
                path = alias = storePass = keyPass = null;
                return false;
            }

            if (!File.Exists(path))
            {
                error = "Файл keystore не найден.";
                path = alias = storePass = keyPass = null;
                return false;
            }

            error = null;
            return true;
        }

        [Serializable]
        class KeystoreFile
        {
            public string path;
            public string alias;
            public string keystorePass;
            public string keyPass;
        }
    }
}
