using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    public sealed class SpriteAndroidPostprocessor : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Sprites/"))
                return;

            var importer = (TextureImporter)assetImporter;
            importer.isReadable = false;
            importer.mipmapEnabled = false;

            importer.GetSourceTextureWidthAndHeight(out var width, out var height);
            var settings = importer.GetPlatformTextureSettings("Android");
            settings.overridden = true;
            settings.maxTextureSize = 2048;
            settings.format = Mathf.Max(width, height) >= 1000
                ? TextureImporterFormat.ASTC_6x6
                : TextureImporterFormat.ASTC_4x4;
            settings.compressionQuality = 50;
            importer.SetPlatformTextureSettings(settings);
        }
    }
}
