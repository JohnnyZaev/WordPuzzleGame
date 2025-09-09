using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using WPG.Runtime.Gameplay.Views;

namespace WPG.Runtime.Gameplay.ViewModels
{
    public class WordSlotViewModel : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly WordSlot _wordSlot;
        private readonly GameViewModel _gameViewModel;
        
        public readonly ReactiveProperty<bool> IsCompleted = new();
        
        public readonly ReactiveProperty<float> CompletionProgress = new();
        
        private readonly Dictionary<int, LetterClusterView> _clusterViewMap = new();

        private int Index => _wordSlot.Index;
        
        public List<LetterClusterView> GetPlacedClusterViews()
        {
            var placedClusters = _wordSlot.GetPlacedClusters();
            var clusterViews = new List<LetterClusterView>();
            
            foreach (var cluster in placedClusters)
            {
                if (_clusterViewMap.TryGetValue(cluster.Id, out var clusterView))
                {
                    clusterViews.Add(clusterView);
                }
            }
            
            return clusterViews;
        }

        private void RegisterClusterView(LetterClusterView clusterView)
        {
            if (clusterView != null)
            {
                _clusterViewMap[clusterView.ClusterId] = clusterView;
            }
        }
        
        public WordSlotViewModel(WordSlot wordSlot, GameViewModel gameViewModel)
        {
            _wordSlot = wordSlot;
            _gameViewModel = gameViewModel;
            
            _wordSlot.IsCompleted
                .Subscribe(completed => IsCompleted.Value = completed)
                .AddTo(_disposables);
            
            IsCompleted
                .Subscribe(CompletionProgressCheck)
                .AddTo(_disposables);
        }

        private void CompletionProgressCheck(bool completed)
        {
            CompletionProgress.Value = completed ? 1.0f : 0.0f;
        }

        public bool CanAcceptClusterView(LetterClusterView clusterView)
        {
            if (clusterView == null)
            {
                return false;
            }
            
            var currentViews = GetPlacedClusterViews();
            bool isAlreadyPlaced = currentViews.Contains(clusterView);
            
            if (isAlreadyPlaced)
            {
                return true;
            }
            
            var placedClusters = _wordSlot.GetPlacedClusters().ToList();
            int currentLetterCount = placedClusters.Sum(c => c.Letters.Length);
            
            return currentLetterCount < 6; // Conservative check - allow if not at maximum letters yet
        }
        
        public void PlaceClusterView(LetterClusterView clusterView)
        {
            if (clusterView == null) return;
            
            RegisterClusterView(clusterView);
            
            _gameViewModel.TryPlaceCluster(clusterView.ClusterId, Index);
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
            IsCompleted?.Dispose();
            CompletionProgress?.Dispose();
            _clusterViewMap.Clear();
        }
    }
}