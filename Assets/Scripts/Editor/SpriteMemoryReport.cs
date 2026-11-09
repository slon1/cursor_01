using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    public static class SpriteMemoryReport
    {
        public struct Row
        {
            public string Path;
            public int Width;
            public int Height;
            public long BeforeBytes;
            public long AfterBytes;
            public string Format;
        }

        [MenuItem("Tools/Build/Report Sprite Memory")]
        public static void Log()
        {
            var rows = Collect();
            long before = 0;
            long after = 0;
            for (var i = 0; i < rows.Count; i++)
            {
                before += rows[i].BeforeBytes;
                after += rows[i].AfterBytes;
            }

            rows.Sort((a, b) => b.AfterBytes.CompareTo(a.AfterBytes));
            var text = new StringBuilder();
            text.Append("текстуры ").Append(rows.Count)
                .Append(" до ").Append(before)
                .Append(" после ").Append(after);
            var limit = Mathf.Min(10, rows.Count);
            for (var i = 0; i < limit; i++)
            {
                var row = rows[i];
                text.Append(" | ").Append(row.Path)
                    .Append(" ").Append(row.Width).Append("x").Append(row.Height)
                    .Append(" ").Append(row.Format)
                    .Append(" ").Append(row.AfterBytes);
            }

            Debug.Log(text.ToString());
        }

        public static List<Row> Collect()
        {
            var rows = new List<Row>();
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites" });
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                    continue;
                importer.GetSourceTextureWidthAndHeight(out var width, out var height);
                var settings = importer.GetPlatformTextureSettings("Android");
                var format = settings.overridden ? settings.format : TextureImporterFormat.Automatic;
                var max = settings.maxTextureSize > 0 ? settings.maxTextureSize : importer.maxTextureSize;
                Fit(width, height, max, out var fittedW, out var fittedH);
                rows.Add(new Row
                {
                    Path = path,
                    Width = width,
                    Height = height,
                    BeforeBytes = RgbaBytes(width, height),
                    AfterBytes = Estimate(fittedW, fittedH, format, Mathf.Max(width, height) >= 1000),
                    Format = format.ToString()
                });
            }

            return rows;
        }

        public static void Fit(int width, int height, int max, out int fittedW, out int fittedH)
        {
            var largest = Mathf.Max(width, height);
            if (largest <= max || largest == 0)
            {
                fittedW = width;
                fittedH = height;
                return;
            }

            var scale = max / (float)largest;
            fittedW = Mathf.Max(1, Mathf.RoundToInt(width * scale));
            fittedH = Mathf.Max(1, Mathf.RoundToInt(height * scale));
        }

        public static long RgbaBytes(int width, int height) => (long)width * height * 4;

        public static long Estimate(int width, int height, TextureImporterFormat format, bool large)
        {
            if (format == TextureImporterFormat.ASTC_6x6 || (format == TextureImporterFormat.Automatic && large))
                return Blocks(width, 6) * Blocks(height, 6) * 16L;
            if (format == TextureImporterFormat.ASTC_4x4 || format == TextureImporterFormat.Automatic)
                return Blocks(width, 4) * Blocks(height, 4) * 16L;
            return RgbaBytes(width, height);
        }

        static int Blocks(int pixels, int block) => (pixels + block - 1) / block;

        public static void ReimportSprites()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites" });
            AssetDatabase.StartAssetEditing();
            try
            {
                for (var i = 0; i < guids.Length; i++)
                    AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guids[i]), ImportAssetOptions.ForceUpdate);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        public static void WriteTable(string path)
        {
            var rows = Collect();
            rows.Sort((a, b) => b.AfterBytes.CompareTo(a.AfterBytes));
            var text = new StringBuilder();
            long before = 0;
            long after = 0;
            for (var i = 0; i < rows.Count; i++)
            {
                before += rows[i].BeforeBytes;
                after += rows[i].AfterBytes;
                text.Append(rows[i].Path).Append('\t')
                    .Append(rows[i].Width).Append('x').Append(rows[i].Height).Append('\t')
                    .Append(rows[i].Format).Append('\t')
                    .Append(rows[i].BeforeBytes).Append('\t')
                    .Append(rows[i].AfterBytes).Append('\n');
            }

            text.Insert(0, "total\t" + rows.Count + "\t" + before + "\t" + after + "\n");
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "Temp");
            File.WriteAllText(path, text.ToString());
        }
    }
}
