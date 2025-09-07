using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;
using WPG.Runtime.Utilities.Logging;

namespace WPG.Runtime.Data
{
    public class LevelDataLoader
    {
        private const string LEVELS_FOLDER = "Levels";

        public async UniTask<LevelData> LoadLevelAsync(int levelNumber)
        {
            try
            {
                string fileName = $"Level_{levelNumber:D2}.json";
                string filePath = Path.Combine(Application.streamingAssetsPath, LEVELS_FOLDER, fileName);

                Log.Gameplay.Info($"Loading level data from: {filePath}");

#if UNITY_ANDROID && !UNITY_EDITOR
                // On Android, StreamingAssets are in a compressed archive
                var www = UnityEngine.Networking.UnityWebRequest.Get(filePath);
                await www.SendWebRequest();
                
                if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Log.Gameplay.Error($"Failed to load level {levelNumber}: {www.error}");
                    return null;
                }
                
                string jsonContent = www.downloadHandler.text;
#else
                if (!File.Exists(filePath))
                {
                    Log.Gameplay.Error($"Level file not found: {filePath}");
                    return null;
                }

                string jsonContent = await File.ReadAllTextAsync(filePath);
#endif

                LevelData levelData = JsonUtility.FromJson<LevelData>(jsonContent);
                
                if (levelData == null)
                {
                    Log.Gameplay.Error($"Failed to parse level data for level {levelNumber}");
                    return null;
                }

                Log.Gameplay.Info($"Successfully loaded level {levelNumber}: {levelData.levelName}");
                return levelData;
            }
            catch (System.Exception e)
            {
                Log.Gameplay.Error($"Exception while loading level {levelNumber}: {e.Message}");
                return null;
            }
        }

        public bool LevelExists(int levelNumber)
        {
            string fileName = $"Level_{levelNumber:D2}.json";
            string filePath = Path.Combine(Application.streamingAssetsPath, LEVELS_FOLDER, fileName);
            
#if UNITY_ANDROID && !UNITY_EDITOR
            // On Android we can't easily check file existence, so assume it exists
            // Real check would happen during loading
            return true;
#else
            return File.Exists(filePath);
#endif
        }
    }
}