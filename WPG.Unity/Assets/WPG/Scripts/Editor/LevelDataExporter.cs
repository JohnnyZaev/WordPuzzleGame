using System.IO;
using UnityEditor;
using UnityEngine;
using WPG.Runtime.Data;
using WPG.Runtime.Utilities.Logging;

namespace WPG.Editor
{
    public class LevelDataExporter : EditorWindow
    {
        private LevelConfiguration _selectedLevel;
        private string _exportPath = "Assets/StreamingAssets/Levels/";
        
        [MenuItem("WPG/Level Data Exporter")]
        public static void ShowWindow()
        {
            GetWindow<LevelDataExporter>("Level Data Exporter");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Level Data Exporter", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            _selectedLevel = EditorGUILayout.ObjectField("Level Configuration", _selectedLevel, typeof(LevelConfiguration), false) as LevelConfiguration;
            
            GUILayout.Space(10);
            
            _exportPath = EditorGUILayout.TextField("Export Path", _exportPath);
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Export Selected Level"))
            {
                if (_selectedLevel != null)
                {
                    ExportLevel(_selectedLevel);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please select a Level Configuration to export.", "OK");
                }
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("Export All Levels"))
            {
                ExportAllLevels();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Create StreamingAssets Folder"))
            {
                CreateStreamingAssetsFolder();
            }
        }
        
        private void ExportLevel(LevelConfiguration level)
        {
            try
            {
                CreateStreamingAssetsFolder();
                
                var levelData = new LevelDataJson
                {
                    levelNumber = level.LevelNumber,
                    levelName = level.LevelName,
                    targetWords = new string[level.TargetWords.Length],
                    letterClusters = new LetterClusterJson[level.LetterClusters.Length]
                };
                
                // Export target words
                for (int i = 0; i < level.TargetWords.Length; i++)
                {
                    if (level.TargetWords[i] != null)
                    {
                        levelData.targetWords[i] = level.TargetWords[i].Word;
                    }
                }
                
                // Export letter clusters
                for (int i = 0; i < level.LetterClusters.Length; i++)
                {
                    if (!string.IsNullOrEmpty(level.LetterClusters[i]))
                    {
                        levelData.letterClusters[i] = new LetterClusterJson
                        {
                            letters = level.LetterClusters[i],
                            frameColor = "#FFFFFFFF" // Default white color
                        };
                    }
                }
                
                string json = JsonUtility.ToJson(levelData, true);
                string fileName = $"Level_{level.LevelNumber:D2}.json";
                string fullPath = Path.Combine(_exportPath, fileName);
                
                File.WriteAllText(fullPath, json);
                
                AssetDatabase.Refresh();
                
                Log.Gameplay.Info($"Level {level.LevelNumber} exported to: {fullPath}");
                EditorUtility.DisplayDialog("Success", $"Level {level.LevelNumber} exported successfully!", "OK");
            }
            catch (System.Exception e)
            {
                Log.Gameplay.Error($"Failed to export level: {e.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to export level: {e.Message}", "OK");
            }
        }
        
        private void ExportAllLevels()
        {
            string[] guids = AssetDatabase.FindAssets("t:LevelConfiguration");
            int exportedCount = 0;
            
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                LevelConfiguration level = AssetDatabase.LoadAssetAtPath<LevelConfiguration>(assetPath);
                
                if (level != null)
                {
                    ExportLevel(level);
                    exportedCount++;
                }
            }
            
            if (exportedCount > 0)
            {
                EditorUtility.DisplayDialog("Success", $"Exported {exportedCount} levels successfully!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Info", "No Level Configuration assets found to export.", "OK");
            }
        }
        
        private void CreateStreamingAssetsFolder()
        {
            if (!Directory.Exists(_exportPath))
            {
                Directory.CreateDirectory(_exportPath);
                AssetDatabase.Refresh();
                Log.Gameplay.Info($"Created directory: {_exportPath}");
            }
        }
        
        private string ColorToHex(Color color)
        {
            return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
        }
    }
    
    [System.Serializable]
    public class LevelDataJson
    {
        public int levelNumber;
        public string levelName;
        public string[] targetWords;
        public LetterClusterJson[] letterClusters;
    }
    
    [System.Serializable]
    public class LetterClusterJson
    {
        public string letters;
        public string frameColor;
    }
}