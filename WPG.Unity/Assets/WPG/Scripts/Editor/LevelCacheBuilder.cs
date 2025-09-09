using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace WPG.Editor
{
    public class LevelCacheBuilder : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            GenerateLevelCache();
        }

        [MenuItem("WPG/Generate Level Cache")]
        public static void GenerateLevelCache()
        {
            var availableLevels = new List<int>();
            string levelsPath = Path.Combine(Application.dataPath, "StreamingAssets", "Levels");
            
            if (!Directory.Exists(levelsPath))
            {
                Debug.LogWarning("Levels directory not found at: " + levelsPath);
                return;
            }

            // Pattern to match Level_XX.json files
            var levelPattern = new Regex(@"Level_(\d+)\.json$", RegexOptions.IgnoreCase);
            
            string[] files = Directory.GetFiles(levelsPath, "*.json");
            
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                var match = levelPattern.Match(fileName);
                
                if (match.Success && int.TryParse(match.Groups[1].Value, out int levelNumber))
                {
                    availableLevels.Add(levelNumber);
                }
            }
            
            availableLevels.Sort();
            
            // Create the cache data
            var cacheData = new LevelCacheData { availableLevels = availableLevels.ToArray() };
            
            // Save to Resources folder
            string resourcesPath = Path.Combine(Application.dataPath, "WPG", "Resources");
            if (!Directory.Exists(resourcesPath))
            {
                Directory.CreateDirectory(resourcesPath);
            }
            
            string cacheFilePath = Path.Combine(resourcesPath, "LevelCache.json");
            string json = JsonUtility.ToJson(cacheData, true);
            File.WriteAllText(cacheFilePath, json);
            
            AssetDatabase.Refresh();
            
            Debug.Log($"Level cache generated with {availableLevels.Count} levels: [{string.Join(", ", availableLevels)}]");
        }
    }

    [System.Serializable]
    public class LevelCacheData
    {
        public int[] availableLevels;
    }
}