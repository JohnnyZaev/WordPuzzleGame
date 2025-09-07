using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using WPG.Runtime.Utilities.Logging;

namespace WPG.Runtime.Utilities.SaveController
{
    public class SaveController : ISaveController
    {
        private readonly string _saveDirectory = Application.persistentDataPath;
        private const string FileExtension = ".json";

        public async UniTask SaveDataAsync<T>(T data, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
                
            try
            {
                string filePath = GetFilePath(fileName);
                string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
                
                // Use async file operations for better performance
                await File.WriteAllTextAsync(filePath, jsonData);
                
                Log.Bootstrap.Info($"[SaveController] Successfully saved data to: {filePath}");
            }
            catch (Exception ex)
            {
                Log.Bootstrap.Error($"[SaveController] Failed to save data to {fileName}: {ex.Message}");
                throw;
            }
        }
        
        public async UniTask<T> LoadDataAsync<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
                
            try
            {
                string filePath = GetFilePath(fileName);
                
                if (!File.Exists(filePath))
                {
                    Log.Bootstrap.Warning($"[SaveController] Save file not found: {filePath}");
                    return default(T);
                }
                
                string jsonData = await File.ReadAllTextAsync(filePath);
                T data = JsonConvert.DeserializeObject<T>(jsonData);
                
                Log.Bootstrap.Info($"[SaveController] Successfully loaded data from: {filePath}");
                return data;
            }
            catch (Exception ex)
            {
                Log.Bootstrap.Error($"[SaveController] Failed to load data from {fileName}: {ex.Message}");
                throw;
            }
        }
        
        public bool SaveFileExists(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;
                
            string filePath = GetFilePath(fileName);
            return File.Exists(filePath);
        }
        
        public bool DeleteSaveFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;
                
            try
            {
                string filePath = GetFilePath(fileName);
                
                if (!File.Exists(filePath))
                {
                    Log.Bootstrap.Warning($"[SaveController] Cannot delete file that doesn't exist: {filePath}");
                    return false;
                }
                
                File.Delete(filePath);
                Log.Bootstrap.Info($"[SaveController] Successfully deleted save file: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Bootstrap.Error($"[SaveController] Failed to delete save file {fileName}: {ex.Message}");
                return false;
            }
        }
        
        private string GetFilePath(string fileName)
        {
            return Path.Combine(_saveDirectory, fileName + FileExtension);
        }
        
        public void Dispose()
        {
        }
    }
}