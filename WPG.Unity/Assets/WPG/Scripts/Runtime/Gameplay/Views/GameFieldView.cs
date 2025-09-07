using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using R3;
using TMPro;
using Cysharp.Threading.Tasks;
using WPG.Runtime.Gameplay.ViewModels;
using WPG.Runtime.Utilities.Logging;
using WPG.Runtime.UI.View;

namespace WPG.Runtime.Gameplay.Views
{
    public class GameFieldView : View
    {
        [Header("UI References")]
        [SerializeField] private Transform _wordSlotsContainer;
        [SerializeField] private Transform _letterClustersContainer;
        [SerializeField] private ScrollRect _clustersScrollRect;
        [SerializeField] private Button _reloadLevelButton;
        [SerializeField] private TMP_Text _levelNameText;
        [SerializeField] private TMP_Text _completionText;
        
        [Header("Prefabs")]
        [SerializeField] private WordSlotView _wordSlotPrefab;
        [SerializeField] private LetterClusterView _letterClusterPrefab;
        [SerializeField] private VictoryView _victoryView;
        
        private readonly CompositeDisposable _disposables = new();
        private readonly List<WordSlotView> _wordSlotViews = new();
        private readonly List<LetterClusterView> _letterClusterViews = new();
        
        private GameViewModel _gameViewModel;
        private VictoryViewModel _victoryViewModel;
        
        public void Initialize(GameViewModel gameViewModel)
        {
            _gameViewModel = gameViewModel;
            
            // Subscribe to ViewModel properties
            _gameViewModel.LevelName
                .Subscribe(levelName => 
                {
                    if (_levelNameText != null)
                    {
                        _levelNameText.text = levelName;
                    }
                })
                .AddTo(_disposables);
                
            _gameViewModel.IsLevelCompleted
                .Subscribe(completed => 
                {
                    if (_completionText != null)
                    {
                        _completionText.text = completed ? "Уровень завершен!" : "";
                        _completionText.gameObject.SetActive(completed);
                    }
                })
                .AddTo(_disposables);
                
            _gameViewModel.WordSlots
                .Subscribe(UpdateWordSlots)
                .AddTo(_disposables);
                
            _gameViewModel.LetterClusters
                .Subscribe(UpdateLetterClusters)
                .AddTo(_disposables);
                
            _gameViewModel.ShowVictoryScreen
                .Subscribe(HandleVictoryScreen)
                .AddTo(_disposables);
            
            // Subscribe to reload button
            if (_reloadLevelButton != null)
            {
                _reloadLevelButton.OnClickAsObservable()
                    .Subscribe(_ => _gameViewModel.ReloadLevelCommand.Execute(default))
                    .AddTo(_disposables);
            }
            
            // Initialize victory screen
            if (_victoryView != null)
            {
                _victoryViewModel = new VictoryViewModel();
                
                // Connect victory view model commands to a game view model
                _victoryViewModel.MainMenuCommand
                    .Subscribe(_ => _gameViewModel.GoToMainMenuCommand.Execute(Unit.Default))
                    .AddTo(_disposables);
                    
                _victoryViewModel.NextLevelCommand
                    .Subscribe(_ => _gameViewModel.GoToNextLevelCommand.Execute(Unit.Default))
                    .AddTo(_disposables);
                
                _victoryView.Initialize(_victoryViewModel);
                _victoryView.Hide(); // Initially hidden
            }
        }
        
        private void UpdateWordSlots(List<WordSlotViewModel> wordSlotViewModels)
        {
            // Clear existing views
            foreach (var view in _wordSlotViews)
            {
                if (view != null)
                {
                    view.Dispose();
                    DestroyImmediate(view.gameObject);
                }
            }
            _wordSlotViews.Clear();
            
            // Create new views
            if (wordSlotViewModels != null)
            {
                foreach (var viewModel in wordSlotViewModels)
                {
                    var view = Instantiate(_wordSlotPrefab, _wordSlotsContainer);
                    view.Initialize(viewModel);
                    _wordSlotViews.Add(view);
                }
            }
            
            Log.Gameplay.Info($"Updated word slots: {_wordSlotViews.Count} slots created");
        }
        
        private void UpdateLetterClusters(List<LetterClusterViewModel> clusterViewModels)
        {
            // Clear existing views
            foreach (var view in _letterClusterViews)
            {
                if (view != null)
                {
                    view.Dispose();
                    DestroyImmediate(view.gameObject);
                }
            }
            _letterClusterViews.Clear();
            
            // Create new views
            if (clusterViewModels != null)
            {
                foreach (var viewModel in clusterViewModels)
                {
                    var view = Instantiate(_letterClusterPrefab, _letterClustersContainer);
                    view.Initialize(viewModel, this);
                    _letterClusterViews.Add(view);
                }
            }
            
            Log.Gameplay.Info($"Updated letter clusters: {_letterClusterViews.Count} clusters created");
        }
        
        public int GetWordSlotIndexAtPosition(Vector2 screenPosition)
        {
            // Convert screen position to local position and find which word slot it overlaps
            for (int i = 0; i < _wordSlotViews.Count; i++)
            {
                var wordSlotView = _wordSlotViews[i];
                if (wordSlotView != null && wordSlotView.ContainsScreenPosition(screenPosition))
                {
                    return i;
                }
            }
            
            return -1; // No word slot found at this position
        }
        
        public Vector2 GetWordSlotPosition(int index)
        {
            if (index >= 0 && index < _wordSlotViews.Count && _wordSlotViews[index] != null)
            {
                return _wordSlotViews[index].GetCenterPosition();
            }
            
            return Vector2.zero;
        }
        
        public Vector2 GetClusterOriginPosition(int clusterId)
        {
            var clusterView = _letterClusterViews.Find(v => v.ClusterId == clusterId);
            return clusterView != null ? clusterView.GetOriginPosition() : Vector2.zero;
        }
        
        private void HandleVictoryScreen(bool show)
        {
            if (_victoryView == null || _victoryViewModel == null) return;
            
            if (show)
            {
                // Update victory screen data
                var completedWords = _gameViewModel.CompletedWords.Value ?? new List<string>();
                int currentLevel = _gameViewModel.CurrentLevelNumber.Value;
                bool hasNextLevel = currentLevel < 99; // Assume max 99 levels for now
                
                _victoryViewModel.SetVictoryData(completedWords, currentLevel, hasNextLevel);
                _victoryView.Show();
            }
            else
            {
                _victoryView.Hide();
            }
        }
        
        public override UniTask Show()
        {
            gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }
        
        public override UniTask Hide()
        {
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }
        
        private void OnDestroy()
        {
            Dispose();
        }
        
        public override void Dispose()
        {
            _disposables?.Dispose();
            
            foreach (var view in _wordSlotViews)
            {
                view?.Dispose();
            }
            _wordSlotViews.Clear();
            
            foreach (var view in _letterClusterViews)
            {
                view?.Dispose();
            }
            _letterClusterViews.Clear();
            
            _victoryViewModel?.Dispose();
            _victoryView?.Dispose();
        }
    }
}