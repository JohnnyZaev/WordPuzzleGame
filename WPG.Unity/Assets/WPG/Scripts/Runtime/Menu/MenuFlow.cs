using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using VContainer.Unity;
using WPG.Runtime.Bootstrap;
using WPG.Runtime.Persistent;
using WPG.Runtime.Utilities.AddressablesController;
using WPG.Runtime.Utilities.Logging;
using WPG.Runtime.Utilities.SaveController;
using Object = UnityEngine.Object;

namespace WPG.Runtime.Menu
{
    public class MenuFlow : IStartable, IDisposable
    {
        private readonly ILoadingController _loadingController;
        private readonly IAddressablesController _addressablesController;
        private readonly ISaveController _saveController;
        private readonly AssetReference _menuView;
        
        private GameObject _menuViewAsset;
        private MenuView _menuViewInstance;
        
        private MenuDTO _menuDTO;
        
        private readonly CompositeDisposable _disposables = new();

        public MenuFlow(ILoadingController loadingController,
            IAddressablesController addressablesController,
            AssetReference menuView,
            ISaveController saveController)
        {
            _loadingController = loadingController;
            _addressablesController = addressablesController;
            _menuView = menuView;
            _saveController = saveController;
        }

        public async void Start()
        {
            Log.Gameplay.Info("MenuFlow started");

            if (_saveController.SaveFileExists(RuntimeConstants.DTO.Menu))
            {
                _menuDTO = await _saveController.LoadDataAsync<MenuDTO>(RuntimeConstants.DTO.Menu);
            }
            else
            {
                _menuDTO = new MenuDTO();
                _menuDTO.CompletedLevels = 0;
                _menuDTO.CurrentLevel = 1;
                await _saveController.SaveDataAsync(_menuDTO, RuntimeConstants.DTO.Menu);
            }
            
            _loadingController.ReportLoadingProgress(2,3);
            
            _menuViewAsset = await _addressablesController.LoadAssetByReferenceAsync<GameObject>(_menuView);
            _menuViewInstance = Object.Instantiate(_menuViewAsset).GetComponent<MenuView>();
            _menuViewInstance._currentLevelText.text += _menuDTO.CurrentLevel.ToString();
            _menuViewInstance._completedLevelsText.text += _menuDTO.CompletedLevels.ToString();
            _menuViewInstance._playButton.OnClickAsObservable().ThrottleFirst(TimeSpan.MaxValue).Subscribe(_ => LoadGameplay()).AddTo(_disposables);
            _menuViewInstance.Show();
            _loadingController.FinishLoading().Forget();
        }

        private async void LoadGameplay()
        {
            _menuViewInstance.Hide().Forget();
            _loadingController.StartLoading();
            _loadingController.ReportLoadingProgress(0, 3);
            await SceneManager.LoadSceneAsync(RuntimeConstants.Scenes.Gameplay).ToUniTask();
            _loadingController.ReportLoadingProgress(1, 3);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}