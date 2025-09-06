using System;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace WPG.Runtime.Utilities.AddressablesController
{
    public interface IAddressablesController : IDisposable
    {
        public UniTask<T> LoadAssetByReferenceAsync<T>(AssetReference assetReference) where T : Object;
        public UniTask<T> LoadAssetByNameAsync<T>(string key) where T : Object;
        public void UnloadAssetReference(AssetReference assetRef);
        public void UnloadAllAssetReferences();
    }
}