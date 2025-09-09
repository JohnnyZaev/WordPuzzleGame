using System.IO;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using WPG.Runtime.Utilities.Logging;

namespace WPG.Runtime.Data
{
    [System.Serializable]
    public class LevelCacheData
    {
        public int[] availableLevels;
    }

    public class LevelDataLoader
    {
        private const string LEVELS_FOLDER = "Levels";
        private static LevelCacheData _levelCache;
        private static bool _levelCacheLoaded = false;

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

        private static void LoadLevelCache()
        {
            if (_levelCacheLoaded) return;

            try
            {
                var cacheResource = Resources.Load<TextAsset>("LevelCache");
                if (cacheResource != null)
                {
                    _levelCache = JsonUtility.FromJson<LevelCacheData>(cacheResource.text);
                    Log.Gameplay.Info($"Level cache loaded with {_levelCache?.availableLevels?.Length ?? 0} levels");
                }
                else
                {
                    Log.Gameplay.Warning("Level cache not found in Resources");
                    _levelCache = null;
                }
            }
            catch (System.Exception e)
            {
                Log.Gameplay.Error($"Failed to load level cache: {e.Message}");
                _levelCache = null;
            }

            _levelCacheLoaded = true;
        }

        public bool LevelExists(int levelNumber)
        {
            string fileName = $"Level_{levelNumber:D2}.json";
            string filePath = Path.Combine(Application.streamingAssetsPath, LEVELS_FOLDER, fileName);
            
#if UNITY_ANDROID && !UNITY_EDITOR
            // On Android, use cached level data instead of trying to check file existence
            LoadLevelCache();
            
            if (_levelCache?.availableLevels != null)
            {
                return _levelCache.availableLevels.Contains(levelNumber);
            }
            
            // Fallback to true if cache is not available (maintain backward compatibility)
            Log.Gameplay.Warning($"Level cache not available, falling back to assuming level {levelNumber} exists");
            return false;
#else
            return File.Exists(filePath);
#endif
        }
    }
}