using System;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using VContainer.Unity;
using WPG.Runtime.Persistent;
using WPG.Runtime.Utilities.Loading;

namespace WPG.Runtime.Bootstrap
{
    public sealed class BootstrapFlow : IStartable, IDisposable
    {
        private readonly LoadingService _loadingService;
        private readonly ILoadingController _loadingController;

        public BootstrapFlow(LoadingService loadingService, ILoadingController loadingController)
        {
            _loadingService = loadingService;
            _loadingController = loadingController;
        }

        public async void Start()
        {
            await _loadingService.BeginLoading(_loadingController);
            
            _loadingController.StartLoading();
            _loadingController.ReportLoadingProgress(0, 3);
            
            if (SceneManager.GetActiveScene().buildIndex == RuntimeConstants.Scenes.Bootstrap)
            {
                await SceneManager.LoadSceneAsync(RuntimeConstants.Scenes.Menu).ToUniTask();
            }
            
            _loadingController.ReportLoadingProgress(1, 3);
        }

        public void Dispose()
        {
            
        }
    }
}