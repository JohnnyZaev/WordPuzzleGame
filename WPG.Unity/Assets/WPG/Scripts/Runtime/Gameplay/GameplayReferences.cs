using UnityEngine;
using UnityEngine.AddressableAssets;
using WPG.Runtime.Gameplay.Views;

namespace WPG.Runtime.Gameplay
{
    [System.Serializable]
    public class GameplayReferences
    {
        [SerializeField] private AssetReference _gameplayView;
        [SerializeField] private AssetReference _victoryView;
        [SerializeField] private GameFieldView _gameFieldView;
        
        public GameFieldView GameFieldView => _gameFieldView;
    }
}