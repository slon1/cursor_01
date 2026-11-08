using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Game.Config;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public static class SpriteManifestImporter
    {
        const string ManifestPath = "Docs/MANIFEST.csv";
        const string CatalogPath = "Assets/ScriptableObjects/SpriteCatalog.asset";
        const string SpriteRoot = "Assets/Sprites/";

        [MenuItem("Tools/Sprites/Import Manifest")]
        public static void ImportManifest()
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            var manifestFullPath = Path.Combine(projectRoot, ManifestPath);
            if (!File.Exists(manifestFullPath))
            {
                Debug.LogError("Манифест не найден: " + ManifestPath);
                return;
            }

            var rows = Csv.Parse(File.ReadAllText(manifestFullPath));
            if (rows.Count == 0 || !HeaderMatches(rows[0].Fields))
            {
                Debug.LogError("Манифест, строка 1: ожидаются колонки path, sprite_id, category, note.");
                return;
            }

            var catalog = LoadOrCreateCatalog();
            var entries = new List<SpriteCatalogEntry>();
            var levelOpen = new Sprite[SpriteCatalog.LevelCount];
            var levelLocked = new Sprite[SpriteCatalog.LevelCount];
            var openFilled = new bool[SpriteCatalog.LevelCount];
            var lockedFilled = new bool[SpriteCatalog.LevelCount];
            var seenIds = new Dictionary<string, int>();
            var seenPaths = new Dictionary<string, int>();
            var catalogIds = new HashSet<string>();
            var recorded = 0;
            var skipped = 0;

            for (var i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row.Fields.Count == 0 || (row.Fields.Count == 1 && row.Fields[0].Length == 0))
                    continue;

                if (row.Fields.Count < 3)
                {
                    Debug.LogError("Манифест, строка " + row.Line + ": меньше трёх колонок.");
                    continue;
                }

                var path = row.Fields[0].Trim();
                var spriteId = row.Fields[1].Trim();
                var category = row.Fields[2].Trim();
                var line = row.Line;

                if (seenIds.TryGetValue(spriteId, out var firstIdLine))
                {
                    Debug.LogError("Манифест, строка " + line + ": повторяющийся sprite_id " + spriteId + " (первый раз на строке " + firstIdLine + ").");
                    continue;
                }

                seenIds.Add(spriteId, line);

                if (seenPaths.TryGetValue(path, out var firstPathLine))
                {
                    Debug.LogError("Манифест, строка " + line + ": два id на один файл " + path + " (первый раз на строке " + firstPathLine + ").");
                    continue;
                }

                seenPaths.Add(path, line);

                if (category == "AppIcon" || category == "Preview")
                {
                    skipped++;
                    continue;
                }

                if (category == "LevelButton")
                {
                    if (TryAssignLevelButton(path, spriteId, line, levelOpen, levelLocked, openFilled, lockedFilled))
                        recorded++;
                    continue;
                }

                if (!Enum.TryParse(spriteId, false, out SpriteId id) || !Enum.IsDefined(typeof(SpriteId), id))
                {
                    Debug.LogError("Манифест, строка " + line + ": id " + spriteId + " отсутствует в enum SpriteId.");
                    continue;
                }

                catalogIds.Add(spriteId);
                if (!TryLoadSprite(path, line, out var sprite))
                    continue;

                entries.Add(new SpriteCatalogEntry { Id = id, Sprite = sprite });
                recorded++;
            }

            foreach (SpriteId id in Enum.GetValues(typeof(SpriteId)))
            {
                if (!catalogIds.Contains(id.ToString()))
                    Debug.LogError("SpriteId." + id + " отсутствует в " + ManifestPath + ".");
            }

            for (var level = 1; level <= SpriteCatalog.LevelCount; level++)
            {
                if (!openFilled[level - 1])
                    Debug.LogError("Манифест: пропущен подряд уровень " + level + " (Open) в LevelButton.");

                if (level > 1 && !lockedFilled[level - 1])
                    Debug.LogError("Манифест: пропущен подряд уровень " + level + " (Locked) в LevelButton.");
            }

            catalog.Entries = entries;
            catalog.LevelButtonOpen = levelOpen;
            catalog.LevelButtonLocked = levelLocked;
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("Импорт манифеста: записано " + recorded + " спрайтов, пропущено " + skipped + ".");
        }

        static bool TryAssignLevelButton(
            string path,
            string spriteId,
            int line,
            Sprite[] levelOpen,
            Sprite[] levelLocked,
            bool[] openFilled,
            bool[] lockedFilled)
        {
            if (!TryParseLevelButton(spriteId, out var level, out var locked))
            {
                Debug.LogError("Манифест, строка " + line + ": id " + spriteId + " не является кнопкой уровня Lvl_NN_Open или Lvl_NN_Locked.");
                return false;
            }

            if (level < 1 || level > SpriteCatalog.LevelCount)
            {
                Debug.LogError("Манифест, строка " + line + ": уровень " + level + " вне диапазона 1–10.");
                return false;
            }

            if (!TryLoadSprite(path, line, out var sprite))
                return false;

            var index = level - 1;
            if (locked)
            {
                levelLocked[index] = sprite;
                lockedFilled[index] = true;
            }
            else
            {
                levelOpen[index] = sprite;
                openFilled[index] = true;
            }

            return true;
        }

        static bool TryParseLevelButton(string spriteId, out int level, out bool locked)
        {
            level = 0;
            locked = false;
            if (!spriteId.StartsWith("Lvl_", StringComparison.Ordinal) || spriteId.Length < 11 || spriteId[6] != '_')
                return false;

            if (!int.TryParse(spriteId.Substring(4, 2), out level))
                return false;

            var suffix = spriteId.Substring(7);
            if (suffix == "Open")
                return true;

            if (suffix == "Locked")
            {
                locked = true;
                return true;
            }

            return false;
        }

        static bool TryLoadSprite(string relativePath, int line, out Sprite sprite)
        {
            sprite = null;
            var assetPath = SpriteRoot + relativePath.Replace('\\', '/');
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            var fullPath = Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
            {
                Debug.LogError("Манифест, строка " + line + ": файл не найден: " + assetPath);
                return false;
            }

            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null || importer.textureType != TextureImporterType.Sprite)
            {
                Debug.LogError("Манифест, строка " + line + ": файл не импортирован как Sprite: " + assetPath);
                return false;
            }

            if (importer.spriteImportMode == SpriteImportMode.Multiple)
            {
                var slices = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
                var count = 0;
                Sprite only = null;
                for (var i = 0; i < slices.Length; i++)
                {
                    if (slices[i] is not Sprite slice)
                        continue;

                    count++;
                    only = slice;
                }

                if (count != 1)
                {
                    Debug.LogError("Манифест, строка " + line + ": в файле " + Path.GetFileName(relativePath) + " несколько нарезанных спрайтов (" + count + ").");
                    return false;
                }

                sprite = only;
                return true;
            }

            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null)
            {
                Debug.LogError("Манифест, строка " + line + ": файл не импортирован как Sprite: " + assetPath);
                return false;
            }

            return true;
        }

        static SpriteCatalog LoadOrCreateCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<SpriteCatalog>(CatalogPath);
            if (catalog != null)
                return catalog;

            catalog = ScriptableObject.CreateInstance<SpriteCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
            return catalog;
        }

        static bool HeaderMatches(List<string> fields)
        {
            return fields.Count >= 4
                && fields[0].Trim() == "path"
                && fields[1].Trim() == "sprite_id"
                && fields[2].Trim() == "category"
                && fields[3].Trim() == "note";
        }

        sealed class CsvRow
        {
            public int Line;
            public List<string> Fields;
        }

        static class Csv
        {
            public static List<CsvRow> Parse(string text)
            {
                var rows = new List<CsvRow>();
                var field = new StringBuilder();
                var fields = new List<string>();
                var inQuotes = false;
                var line = 1;
                var rowLine = 1;

                for (var i = 0; i < text.Length; i++)
                {
                    var c = text[i];
                    if (inQuotes)
                    {
                        if (c == '"')
                        {
                            if (i + 1 < text.Length && text[i + 1] == '"')
                            {
                                field.Append('"');
                                i++;
                            }
                            else
                            {
                                inQuotes = false;
                            }
                        }
                        else
                        {
                            if (c == '\n')
                                line++;
                            field.Append(c);
                        }

                        continue;
                    }

                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        fields.Add(field.ToString());
                        field.Clear();
                    }
                    else if (c == '\n')
                    {
                        fields.Add(field.ToString());
                        field.Clear();
                        rows.Add(new CsvRow { Line = rowLine, Fields = fields });
                        fields = new List<string>();
                        line++;
                        rowLine = line;
                    }
                    else if (c != '\r')
                    {
                        field.Append(c);
                    }
                }

                if (field.Length > 0 || fields.Count > 0)
                {
                    fields.Add(field.ToString());
                    rows.Add(new CsvRow { Line = rowLine, Fields = fields });
                }

                return rows;
            }
        }
    }
}
