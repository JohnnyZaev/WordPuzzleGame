using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using WPG.Runtime.Utilities.Logging;

namespace WPG.Runtime.Utilities.AddressablesController
{
    /// <summary>
    ///     The AddressablesController class is responsible for loading and unloading addressable assets in a Unity project.
    ///     It provides methods to load assets by reference or by name
    ///     and to unload assets either individually or all at once.
    /// </summary>
    public sealed class AddressablesController : IAddressablesController
    {
        private Dictionary<string, AsyncOperationHandle> _assetReferenceToHandleDictionary = new();

        /// <summary>
        ///     Asynchronously loads an asset from an AssetReference. It will return an asset even if it already was loaded.
        /// </summary>
        /// <typeparam name="T">The type of the asset to be loaded.</typeparam>
        /// <param name="assetReference">The AssetReference to load the asset from.</param>
        /// <returns>
        ///     A UniTask representing the asynchronous loading operation,
        ///     with the asset of type T as the result.
        /// </returns>
        public async UniTask<T> LoadAssetByReferenceAsync<T>(AssetReference assetReference) where T : Object
        {
            if (!_assetReferenceToHandleDictionary.TryGetValue(assetReference.AssetGUID, out var asyncOperationHandle))
            {
                asyncOperationHandle = assetReference.LoadAssetAsync<T>();
                _assetReferenceToHandleDictionary.Add(assetReference.AssetGUID, asyncOperationHandle);
            }

            var isOperationInProgress = asyncOperationHandle.IsValid() && !asyncOperationHandle.IsDone;
            if (isOperationInProgress)
            {
                await asyncOperationHandle.Task.AsUniTask();
            }

            var isOperationSuccessful = asyncOperationHandle.IsValid() &&
                                        asyncOperationHandle.Status == AsyncOperationStatus.Succeeded;
            return isOperationSuccessful ? (T)asyncOperationHandle.Result : null;
        }

        /// <summary>
        ///     Asynchronously loads an asset by its name.  It will return an asset even if it already was loaded.
        /// </summary>
        /// <typeparam name="T">The type of the asset to be loaded.</typeparam>
        /// <param name="key">The name key of the asset to be loaded.</param>
        /// <returns>
        ///     A UniTask representing the asynchronous loading operation,
        ///     with the asset of type T as the result.
        /// </returns>
        public UniTask<T> LoadAssetByNameAsync<T>(string key) where T : Object
        {
            return LoadAssetByReferenceAsync<T>(new AssetReference(key));
        }

        /// <summary>
        ///     Unloads an asset previously loaded using its AssetReference.
        /// </summary>
        /// <param name="assetRef">The AssetReference of the asset to be unloaded.</param>
        public void UnloadAssetReference(AssetReference assetRef)
        {
            if (assetRef == null)
            {
                Log.Bootstrap.Warning(
                    "[AddressablesService] UnloadAssetReference: trying to unload a null asset reference");
            }
            else if (_assetReferenceToHandleDictionary != null &&
                     _assetReferenceToHandleDictionary.TryGetValue(assetRef.AssetGUID, out var asyncOp))
            {
                if (asyncOp.IsValid())
                {
                    Addressables.Release(asyncOp);
                }

                _assetReferenceToHandleDictionary.Remove(assetRef.AssetGUID);
            }
        }

        /// <summary>
        ///     Unloads all currently loaded asset references and releases their resources.
        /// </summary>
        public void UnloadAllAssetReferences()
        {
            if (_assetReferenceToHandleDictionary != null)
            {
                foreach (var keyValueAssetReference in _assetReferenceToHandleDictionary)
                    if (keyValueAssetReference.Value.IsValid())
                    {
                        Addressables.Release(keyValueAssetReference.Value);
                    }

                _assetReferenceToHandleDictionary.Clear();
            }
        }

        public void Dispose()
        {
            UnloadAllAssetReferences();
            _assetReferenceToHandleDictionary = null;
        }
    }
}