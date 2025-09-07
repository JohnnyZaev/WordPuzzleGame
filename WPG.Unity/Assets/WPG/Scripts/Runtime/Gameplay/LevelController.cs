using System;
using Cysharp.Threading.Tasks;
using WPG.Runtime.Bootstrap;
using WPG.Runtime.Data;
using WPG.Runtime.Gameplay.ViewModels;
using WPG.Runtime.Menu;
using WPG.Runtime.Persistent;
using WPG.Runtime.Utilities.Loading;
using WPG.Runtime.Utilities.Logging;
using WPG.Runtime.Utilities.SaveController;

namespace WPG.Runtime.Gameplay
{
    public class LevelController : ILoadUnit, IDisposable
    {
        private readonly ISaveController _saveController;
        private readonly LevelDataLoader _levelDataLoader;
        private readonly ILoadingController _loadingController;
        private GameViewModel _gameViewModel;
        
        public GameViewModel GameViewModel => _gameViewModel;

        public LevelController(ISaveController saveController, ILoadingController loadingController)
        {
            _saveController = saveController;
            _loadingController = loadingController;
            _levelDataLoader = new LevelDataLoader();
        }

        public async UniTask Load()
        {
            try
            {
                Log.Gameplay.Info("LevelController loading...");
                
                // Create game view model
                _gameViewModel = new GameViewModel(_levelDataLoader, _saveController, _loadingController);
                
                // Load menu data to get current level
                MenuDTO menuDTO;
                if (_saveController.SaveFileExists(RuntimeConstants.DTO.Menu))
                {
                    menuDTO = await _saveController.LoadDataAsync<MenuDTO>(RuntimeConstants.DTO.Menu);
                }
                else
                {
                    menuDTO = new MenuDTO
                    {
                        CurrentLevel = 1,
                        CompletedLevels = 0
                    };
                }
                
                // Initialize game with menu data
                _gameViewModel.InitializeFromMenuData(menuDTO);
                
                Log.Gameplay.Info("LevelController loaded successfully");
            }
            catch (Exception e)
            {
                Log.Gameplay.Error($"Failed to load LevelController: {e.Message}");
                throw;
            }
        }

        public void ReloadLevel()
        {
            _gameViewModel?.ReloadCurrentLevel();
        }
        
        public void LoadLevel(int levelNumber)
        {
            _gameViewModel?.LoadLevel(levelNumber);
        }

        public void Dispose()
        {
            _gameViewModel?.Dispose();
            Log.Gameplay.Info("LevelController disposed");
        }
    }
}