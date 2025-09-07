using VContainer.Unity;
using WPG.Runtime.Bootstrap;
using WPG.Runtime.Menu;
using WPG.Runtime.Persistent;
using WPG.Runtime.Utilities.Loading;
using WPG.Runtime.Utilities.Logging;
using WPG.Runtime.Utilities.SaveController;

namespace WPG.Runtime.Gameplay
{
    public class GameplayFlow : IStartable
    {
        private readonly ILoadingController _loadingController;
        private readonly LoadingService _loadingService;
        private readonly LevelController _levelController;
        private readonly GameplayReferences _gameplayReferences;
        private readonly ISaveController _saveController;
        
        private MenuDTO _menuDTO;

        public GameplayFlow(ILoadingController loadingController,
            LevelController levelController,
            GameplayReferences gameplayReferences,
            ISaveController saveController,
            LoadingService loadingService)
        {
            _loadingController = loadingController;
            _levelController = levelController;
            _saveController = saveController;
            _loadingService = loadingService;
            _gameplayReferences = gameplayReferences;
        }

        public async void Start()
        {
            Log.Gameplay.Info("GameplayFlow started");

            if (_saveController.SaveFileExists(RuntimeConstants.DTO.Menu))
            {
                _menuDTO = await _saveController.LoadDataAsync<MenuDTO>(RuntimeConstants.DTO.Menu);
            }
            else
            {
                _menuDTO = new MenuDTO
                {
                    CompletedLevels = 0,
                    CurrentLevel = 1
                };
                await _saveController.SaveDataAsync(_menuDTO, RuntimeConstants.DTO.Menu);
            }
            
            _loadingController.ReportLoadingProgress(2,3);

            await _loadingService.BeginLoading(_levelController);

            // Initialize GameFieldView with the loaded GameViewModel
            if (_gameplayReferences.GameFieldView != null && _levelController.GameViewModel != null)
            {
                _gameplayReferences.GameFieldView.Initialize(_levelController.GameViewModel);
                Log.Gameplay.Info("GameFieldView initialized with GameViewModel");
            }
            else
            {
                Log.Gameplay.Error("Failed to initialize GameFieldView - missing references");
            }

            await _loadingController.FinishLoading();
        }
    }
}