using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CandyBeltSort
{
    public static class CandyBeltMenus
    {
        [MenuItem("Candy Belt Sort/Export Level Catalog JSON")]
        public static void ExportLevels()
        {
            var json = LevelCatalogExport.ToJson();
            var resources = "Assets/Resources/CandyBelt/levels.json";
            var store = "store/levels-preview.json";
            Directory.CreateDirectory("Assets/Resources/CandyBelt");
            Directory.CreateDirectory("store");
            File.WriteAllText(resources, json);
            File.WriteAllText(store, json);
            AssetDatabase.Refresh();
            var message = $"Exported {LevelCatalog.Count} levels to\n{resources}\nand {store}";
            if (Application.isBatchMode)
                Debug.Log(message);
            else
                EditorUtility.DisplayDialog("Candy Belt Sort", message, "OK");
        }

        [MenuItem("Candy Belt Sort/Validate Levels")]
        public static void ValidateLevels()
        {
            var result = LevelCatalog.ValidateAll();
            Debug.Log("[CandyBelt] " + result);
            if (!Application.isBatchMode)
                EditorUtility.DisplayDialog("Candy Belt Sort", result, "OK");
        }

        [MenuItem("Candy Belt Sort/Unlock All Levels (Play Mode Save)")]
        public static void UnlockAll()
        {
            PlayerPrefs.SetInt("cbs.highest", LevelCatalog.Count - 1);
            PlayerPrefs.Save();
            Debug.Log("Unlocked all Candy Belt Sort levels.");
        }

        [MenuItem("Candy Belt Sort/Reset Player Progress")]
        public static void ResetProgress()
        {
            PlayerPrefs.DeleteKey("cbs.highest");
            PlayerPrefs.DeleteKey("cbs.coins");
            PlayerPrefs.DeleteKey("cbs.streak");
            PlayerPrefs.DeleteKey("cbs.streakDay");
            PlayerPrefs.Save();
            Debug.Log("Candy Belt Sort progress reset.");
        }

        [MenuItem("Candy Belt Sort/AdMob Setup Help")]
        public static void AdMobHelp()
        {
            EditorUtility.DisplayDialog(
                "AdMob setup",
                "1. Install Google Mobile Ads Unity plugin (Package Manager git):\n" +
                "https://github.com/googleads/googleads-mobile-unity.git?path=source/plugin\n\n" +
                "2. Add scripting define GOOGLE_MOBILE_ADS to Android.\n\n" +
                "3. Replace test IDs in AdMobProvider and Plugins/Android/AndroidManifest.xml with your real App ID.\n\n" +
                "4. Keep ads 13+ — do not enroll in Designed for Families.\n\n" +
                "Editor and builds without the plugin use MockAdProvider.",
                "OK");
        }

        [MenuItem("Candy Belt Sort/Add GOOGLE_MOBILE_ADS Define (Android)")]
        public static void AddAdMobDefine()
        {
            AddDefine(BuildTargetGroup.Android, "GOOGLE_MOBILE_ADS");
            AddDefine(BuildTargetGroup.Standalone, "GOOGLE_MOBILE_ADS");
            Debug.Log("Added GOOGLE_MOBILE_ADS scripting define.");
        }

        static void AddDefine(BuildTargetGroup group, string symbol)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            if (defines.Contains(symbol)) return;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.IsNullOrEmpty(defines) ? symbol : defines + ";" + symbol);
        }
    }

    public static class LevelCatalogExport
    {
        public static string ToJson()
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"count\": {LevelCatalog.Count},");
            sb.AppendLine($"  \"worlds\": {WorldCatalog.WorldCount},");
            sb.AppendLine("  \"levels\": [");
            for (int i = 0; i < LevelCatalog.Count; i++)
            {
                var l = LevelCatalog.Get(i);
                sb.Append("    {");
                sb.Append($"\"index\": {l.Index}, ");
                sb.Append($"\"world\": {l.WorldIndex}, ");
                sb.Append($"\"worldName\": \"{l.WorldName}\", ");
                sb.Append($"\"levelInWorld\": {l.LevelInWorld + 1}, ");
                sb.Append($"\"colors\": {l.ColorCount}, ");
                sb.Append($"\"slots\": {l.OpenSlots}, ");
                sb.Append($"\"lanes\": {l.LaneCount}, ");
                sb.Append($"\"quota\": {l.QuotaBoxes}, ");
                sb.Append($"\"speed\": {l.BeltSpeed.ToString("0.000")}, ");
                sb.Append($"\"spawn\": {l.SpawnInterval.ToString("0.000")}, ");
                sb.Append($"\"frozen\": {l.FrozenChance.ToString("0.000")}, ");
                sb.Append($"\"locked\": {l.LockedBoxCount}, ");
                sb.Append($"\"hidden\": {l.HiddenChance.ToString("0.000")}, ");
                sb.Append($"\"bomb\": {l.BombChance.ToString("0.000")}, ");
                sb.Append($"\"seed\": {l.Seed}");
                sb.Append(i == LevelCatalog.Count - 1 ? "}\n" : "},\n");
            }
            sb.AppendLine("  ]");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
