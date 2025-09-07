using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.SceneManagement;
using WPG.Runtime.Bootstrap;
using WPG.Runtime.Data;
using WPG.Runtime.Menu;
using WPG.Runtime.Persistent;
using WPG.Runtime.Utilities.Logging;
using WPG.Runtime.Utilities.SaveController;

namespace WPG.Runtime.Gameplay.ViewModels
{
    public class GameViewModel : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly LevelDataLoader _levelDataLoader;
        private readonly GameState _gameState;
        private readonly ISaveController _saveController;
        private readonly ILoadingController _loadingController;
        
        // Public reactive properties for UI binding
        public readonly ReactiveProperty<int> CurrentLevelNumber = new(1);
        public readonly ReactiveProperty<string> LevelName = new(string.Empty);
        public readonly ReactiveProperty<bool> IsLevelLoaded = new();
        public readonly ReactiveProperty<bool> IsLevelCompleted = new();
        public readonly ReactiveProperty<List<WordSlotViewModel>> WordSlots = new();
        public readonly ReactiveProperty<List<LetterClusterViewModel>> LetterClusters = new();
        
        // Victory screen properties
        public readonly ReactiveProperty<bool> ShowVictoryScreen = new();
        public readonly ReactiveProperty<List<string>> CompletedWords = new();
        
        // Commands
        public readonly ReactiveCommand<Unit> ReloadLevelCommand = new();
        public readonly ReactiveCommand<int> LoadLevelCommand = new();
        public readonly ReactiveCommand<Unit> GoToMainMenuCommand = new();
        public readonly ReactiveCommand<Unit> GoToNextLevelCommand = new();
        
        public GameViewModel(LevelDataLoader levelDataLoader, ISaveController saveController, ILoadingController loadingController)
        {
            _levelDataLoader = levelDataLoader;
            _saveController = saveController;
            _loadingController = loadingController;
            _gameState = new GameState();
            
            // Bind game state properties
            _gameState.IsLevelCompleted
                .Subscribe(completed => 
                {
                    IsLevelCompleted.Value = completed;
                    if (completed)
                    {
                        HandleLevelCompletion();
                    }
                })
                .AddTo(_disposables);
                
            // Subscribe to commands
            ReloadLevelCommand
                .Subscribe(_ => ReloadCurrentLevel())
                .AddTo(_disposables);
                
            LoadLevelCommand
                .Subscribe(LoadLevel)
                .AddTo(_disposables);
                
            GoToMainMenuCommand
                .Subscribe(_ => HandleGoToMainMenu())
                .AddTo(_disposables);
                
            GoToNextLevelCommand
                .Subscribe(_ => HandleGoToNextLevel())
                .AddTo(_disposables);
        }
        
        public async void LoadLevel(int levelNumber)
        {
            try
            {
                Log.Gameplay.Info($"Loading level {levelNumber}");
                IsLevelLoaded.Value = false;
                
                var levelData = await _levelDataLoader.LoadLevelAsync(levelNumber);
                if (levelData == null)
                {
                    Log.Gameplay.Error($"Failed to load level {levelNumber}");
                    return;
                }
                
                _gameState.LoadLevel(levelData);
                CurrentLevelNumber.Value = levelNumber;
                LevelName.Value = levelData.levelName;
                
                // Create ViewModels for word slots
                var wordSlotViewModels = new List<WordSlotViewModel>();
                for (int i = 0; i < _gameState.WordSlots.Length; i++)
                {
                    wordSlotViewModels.Add(new WordSlotViewModel(_gameState.WordSlots[i]));
                }
                WordSlots.Value = wordSlotViewModels;
                
                // Create ViewModels for letter clusters
                var clusterViewModels = new List<LetterClusterViewModel>();
                if (_gameState.AvailableClusters.Value != null)
                {
                    foreach (var cluster in _gameState.AvailableClusters.Value)
                    {
                        clusterViewModels.Add(new LetterClusterViewModel(cluster, this));
                    }
                }
                LetterClusters.Value = clusterViewModels;
                
                IsLevelLoaded.Value = true;
                Log.Gameplay.Info($"Level {levelNumber} loaded successfully");
            }
            catch (Exception e)
            {
                Log.Gameplay.Error($"Exception loading level {levelNumber}: {e.Message}");
            }
        }
        
        public void ReloadCurrentLevel()
        {
            if (CurrentLevelNumber.Value > 0)
            {
                _gameState.ResetLevel();
                LoadLevel(CurrentLevelNumber.Value);
            }
        }
        
        public bool TryPlaceCluster(int clusterId, int wordSlotIndex)
        {
            return _gameState.TryPlaceCluster(clusterId, wordSlotIndex);
        }
        
        public bool TryRemoveCluster(int clusterId)
        {
            return _gameState.TryRemoveCluster(clusterId);
        }
        
        public void InitializeFromMenuData(MenuDTO menuDTO)
        {
            if (menuDTO != null)
            {
                LoadLevel(menuDTO.CurrentLevel);
            }
        }
        
        private async UniTaskVoid HandleLevelCompletion()
        {
            // Use completion order from GameState instead of slot order
            var completedWords = new List<string>(_gameState.CompletedWordsInOrder);
            
            CompletedWords.Value = completedWords;
            ShowVictoryScreen.Value = true;
            
            // Update MenuDTO with level completion
            await UpdateMenuDTOOnLevelCompletion();
            
            Log.Gameplay.Info($"Level {CurrentLevelNumber.Value} completed with words in completion order: {string.Join(", ", completedWords)}");
        }
        
        private async System.Threading.Tasks.Task UpdateMenuDTOOnLevelCompletion()
        {
            try
            {
                // Load current MenuDTO
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
                
                // Increment completed levels
                menuDTO.CompletedLevels++;
                
                // Save updated MenuDTO
                await _saveController.SaveDataAsync(menuDTO, RuntimeConstants.DTO.Menu);
                
                Log.Gameplay.Info($"MenuDTO updated: CompletedLevels = {menuDTO.CompletedLevels}");
            }
            catch (Exception e)
            {
                Log.Gameplay.Error($"Failed to update MenuDTO on level completion: {e.Message}");
            }
        }
        
        private void HandleGoToMainMenu()
        {
            ShowVictoryScreen.Value = false;
            Log.Gameplay.Info("Going to Main Menu");

            _loadingController.StartLoading();
            SceneManager.LoadSceneAsync(RuntimeConstants.Scenes.Menu);
            _loadingController.ReportLoadingProgress(1, 3);
        }
        
        private async UniTaskVoid HandleGoToNextLevel()
        {
            ShowVictoryScreen.Value = false;
            int nextLevel = CurrentLevelNumber.Value + 1;
            
            // Check if the next level exists
            if (_levelDataLoader.LevelExists(nextLevel))
            {
                // The next level exists - advance to it
                await UpdateMenuDTOCurrentLevel(nextLevel);
                LoadLevel(nextLevel);
                Log.Gameplay.Info($"Loading next level: {nextLevel}");
            }
            else
            {
                // No more levels - reset to level 1
                await UpdateMenuDTOCurrentLevel(1);
                LoadLevel(1);
                Log.Gameplay.Info($"No more levels available, resetting to level 1");
            }
        }
        
        private async System.Threading.Tasks.Task UpdateMenuDTOCurrentLevel(int newCurrentLevel)
        {
            try
            {
                // Load current MenuDTO
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
                
                // Update current level
                menuDTO.CurrentLevel = newCurrentLevel;
                
                // Save updated MenuDTO
                await _saveController.SaveDataAsync(menuDTO, RuntimeConstants.DTO.Menu);
                
                Log.Gameplay.Info($"MenuDTO updated: CurrentLevel = {menuDTO.CurrentLevel}");
            }
            catch (Exception e)
            {
                Log.Gameplay.Error($"Failed to update MenuDTO current level: {e.Message}");
            }
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
            _gameState?.Dispose();
            
            // Dispose ViewModels
            if (WordSlots.Value != null)
            {
                foreach (var vm in WordSlots.Value)
                {
                    vm?.Dispose();
                }
            }
            
            if (LetterClusters.Value != null)
            {
                foreach (var vm in LetterClusters.Value)
                {
                    vm?.Dispose();
                }
            }
            
            CurrentLevelNumber?.Dispose();
            LevelName?.Dispose();
            IsLevelLoaded?.Dispose();
            IsLevelCompleted?.Dispose();
            WordSlots?.Dispose();
            LetterClusters?.Dispose();
            ShowVictoryScreen?.Dispose();
            CompletedWords?.Dispose();
            ReloadLevelCommand?.Dispose();
            LoadLevelCommand?.Dispose();
            GoToMainMenuCommand?.Dispose();
            GoToNextLevelCommand?.Dispose();
        }
    }
}