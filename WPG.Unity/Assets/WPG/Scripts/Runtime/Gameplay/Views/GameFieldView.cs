using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using R3;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using VContainer;
using WPG.Runtime.Gameplay.ViewModels;
using WPG.Runtime.Utilities.Logging;
using WPG.Runtime.UI.View;
using WPG.Runtime.Utilities.AddressablesController;

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
        
        [Header("AssetReferences")]
        [SerializeField] private AssetReference _wordSlotPrefabReference;
        [SerializeField] private AssetReference _letterClusterPrefabReference;

        [SerializeField] private VictoryView _victoryView;
        
        private readonly CompositeDisposable _disposables = new();
        private readonly List<WordSlotView> _wordSlotViews = new();
        private readonly List<LetterClusterView> _letterClusterViews = new();
        
        private WordSlotView _wordSlotPrefab;
        private LetterClusterView _letterClusterPrefab;
        
        private GameViewModel _gameViewModel;
        private VictoryViewModel _victoryViewModel;
        
        private IAddressablesController _addressablesController;

        [Inject]
        private void Construct(IAddressablesController addressablesController)
        {
            _addressablesController = addressablesController;
        }
        
        public async void Initialize(GameViewModel gameViewModel)
        {
            _gameViewModel = gameViewModel;
            
            await LoadPrefabsAsync();
            
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
                        _completionText.text = completed ? "LevelCompleted!" : "";
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
            
            if (_reloadLevelButton != null)
            {
                _reloadLevelButton.OnClickAsObservable()
                    .Subscribe(_ => _gameViewModel.ReloadLevelCommand.Execute(default))
                    .AddTo(_disposables);
            }
            
            if (_victoryView != null)
            {
                _victoryViewModel = new VictoryViewModel();
                
                _victoryViewModel.MainMenuCommand
                    .Subscribe(_ => _gameViewModel.GoToMainMenuCommand.Execute(Unit.Default))
                    .AddTo(_disposables);
                    
                _victoryViewModel.NextLevelCommand
                    .Subscribe(_ => _gameViewModel.GoToNextLevelCommand.Execute(Unit.Default))
                    .AddTo(_disposables);
                
                _victoryView.Initialize(_victoryViewModel);
                _victoryView.Hide().Forget();
            }
        }
        
        private void UpdateWordSlots(List<WordSlotViewModel> wordSlotViewModels)
        {
            foreach (var view in _wordSlotViews)
            {
                if (view != null)
                {
                    view.Dispose();
                    DestroyImmediate(view.gameObject);
                }
            }
            _wordSlotViews.Clear();
            
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
            foreach (var view in _letterClusterViews)
            {
                if (view != null)
                {
                    view.Dispose();
                    DestroyImmediate(view.gameObject);
                }
            }
            _letterClusterViews.Clear();
            
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
        
        public List<WordSlotView> GetWordSlotViews()
        {
            return _wordSlotViews;
        }
        
        public Transform GetLetterClustersContainer()
        {
            return _letterClustersContainer;
        }
        
        private void HandleVictoryScreen(bool show)
        {
            if (_victoryView == null || _victoryViewModel == null) return;
            
            if (show)
            {
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
        
        private async UniTask LoadPrefabsAsync()
        {
            if (_wordSlotPrefabReference != null)
            {
                var addressable = await _addressablesController.LoadAssetByReferenceAsync<GameObject>(_wordSlotPrefabReference);
                _wordSlotPrefab = addressable.GetComponent<WordSlotView>();
            }
            
            if (_letterClusterPrefabReference != null)
            {
                var addressable = await _addressablesController.LoadAssetByReferenceAsync<GameObject>(_letterClusterPrefabReference);
                _letterClusterPrefab = addressable.GetComponent<LetterClusterView>();
            }
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
            
            
            Destroy(_wordSlotPrefab);
            Destroy(_letterClusterPrefab);
            _addressablesController.UnloadAssetReference(_wordSlotPrefabReference);
            _addressablesController.UnloadAssetReference( _letterClusterPrefabReference);
            
            _victoryViewModel?.Dispose();
            _victoryView?.Dispose();
        }
    }
}