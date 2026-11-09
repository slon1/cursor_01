using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Game.EditorTools
{
    public static class AndroidIcons
    {
        const string BackgroundPath = "Assets/Sprites/icon_background.png";
        const string ForegroundPath = "Assets/Sprites/icon_foreground.png";
        const string LegacyPath = "Assets/Sprites/icon_legacy.png";

        [MenuItem("Tools/Build/Apply Icons")]
        public static void Apply()
        {
            var background = LoadPng(BackgroundPath);
            var foreground = LoadPng(ForegroundPath);
            var composed = Compose(background, foreground);
            SavePng(LegacyPath, composed);
            Directory.CreateDirectory("Docs/reports/phase6c");
            File.WriteAllBytes("Docs/reports/phase6c/icon.png", composed.EncodeToPNG());
            AssetDatabase.ImportAsset(LegacyPath);

            var legacy = AssetDatabase.LoadAssetAtPath<Texture2D>(LegacyPath);
            var backgroundAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(BackgroundPath);
            var foregroundAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(ForegroundPath);
            Assign(Kind("Adaptive"), backgroundAsset, foregroundAsset);
            Assign(Kind("Legacy"), legacy);
            Assign(Kind("Round"), legacy);
            AssetDatabase.SaveAssets();
            Debug.Log("Иконки Android назначены.");
        }

        static void Assign(PlatformIconKind kind, params Texture2D[] textures)
        {
            var icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
            for (var i = 0; i < icons.Length; i++)
            {
                if (textures.Length == 1 || icons[i].layerCount < 2)
                    icons[i].SetTexture(textures[0], 0);
                else
                {
                    icons[i].SetTexture(textures[0], 0);
                    icons[i].SetTexture(textures[1], 1);
                }
            }

            PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
        }

        static PlatformIconKind Kind(string field)
        {
            var type = Type.GetType("UnityEditor.Android.AndroidPlatformIconKind, UnityEditor.Android.Extensions");
            if (type == null)
                throw new InvalidOperationException("Нет модуля Android.");
            return (PlatformIconKind)type.GetField(field).GetValue(null);
        }

        static Texture2D LoadPng(string path)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(File.ReadAllBytes(path));
            return texture;
        }

        static Texture2D Compose(Texture2D background, Texture2D foreground)
        {
            var size = Mathf.Max(background.width, foreground.width);
            var result = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            Blit(pixels, size, background);
            Blit(pixels, size, foreground);
            result.SetPixels32(pixels);
            result.Apply();
            return result;
        }

        static void Blit(Color32[] destination, int size, Texture2D source)
        {
            var pixels = source.GetPixels32();
            var x0 = (size - source.width) / 2;
            var y0 = (size - source.height) / 2;
            for (var y = 0; y < source.height; y++)
            {
                for (var x = 0; x < source.width; x++)
                {
                    var incoming = pixels[y * source.width + x];
                    var index = (y0 + y) * size + x0 + x;
                    destination[index] = Over(destination[index], incoming);
                }
            }
        }

        static Color32 Over(Color32 under, Color32 over)
        {
            var alpha = over.a / 255f;
            return new Color32(
                (byte)(over.r * alpha + under.r * (1f - alpha)),
                (byte)(over.g * alpha + under.g * (1f - alpha)),
                (byte)(over.b * alpha + under.b * (1f - alpha)),
                255);
        }

        static void SavePng(string assetPath, Texture2D texture)
        {
            File.WriteAllBytes(assetPath, texture.EncodeToPNG());
        }
    }
}
