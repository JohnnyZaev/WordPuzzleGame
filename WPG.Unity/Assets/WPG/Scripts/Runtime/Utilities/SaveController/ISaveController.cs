using System;
using Cysharp.Threading.Tasks;

namespace WPG.Runtime.Utilities.SaveController
{
    public interface ISaveController : IDisposable
    {
        /// <summary>
        /// Saves data as JSON to a file with the specified filename
        /// </summary>
        /// <param name="data">The data object to serialize and save</param>
        /// <param name="fileName">The name of the file (without extension)</param>
        /// <typeparam name="T">The type of data to save</typeparam>
        /// <returns>UniTask representing the save operation</returns>
        UniTask SaveDataAsync<T>(T data, string fileName);
        
        /// <summary>
        /// Loads data from JSON file with the specified filename
        /// </summary>
        /// <param name="fileName">The name of the file (without extension)</param>
        /// <typeparam name="T">The type of data to load</typeparam>
        /// <returns>UniTask with the loaded data or default if file doesn't exist</returns>
        UniTask<T> LoadDataAsync<T>(string fileName);
        
        /// <summary>
        /// Checks if a save file exists
        /// </summary>
        /// <param name="fileName">The name of the file (without extension)</param>
        /// <returns>True if file exists, false otherwise</returns>
        bool SaveFileExists(string fileName);
        
        /// <summary>
        /// Deletes a save file
        /// </summary>
        /// <param name="fileName">The name of the file (without extension)</param>
        /// <returns>True if file was deleted, false if file didn't exist</returns>
        bool DeleteSaveFile(string fileName);
    }
}