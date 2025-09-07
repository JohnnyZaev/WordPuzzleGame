using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using WPG.Runtime.Utilities.Logging;

namespace WPG.Runtime.Gameplay
{
    public class WordSlot : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly List<LetterClusterItem> _placedClusters = new();
        
        public int Index { get; }
        public string[] TargetWords { get; private set; } = Array.Empty<string>();
        
        public readonly ReactiveProperty<string> CurrentWord = new(string.Empty);
        public readonly ReactiveProperty<bool> IsCompleted = new();
        public readonly ReactiveProperty<List<LetterClusterItem>> PlacedClusters = new();
        
        public WordSlot(int index)
        {
            Index = index;
            
            // Subscribe to cluster changes to update
            PlacedClusters
                .Subscribe(_ => UpdateCurrentWord())
                .AddTo(_disposables);
        }
        
        public void SetTargetWords(string[] targetWords)
        {
            TargetWords = targetWords ?? Array.Empty<string>();
            UpdateCurrentWord();
        }
        
        public bool TryPlaceCluster(LetterClusterItem cluster)
        {
            if (cluster == null || cluster.IsUsed.Value)
            {
                return false;
            }
            
            // Check if adding this cluster would exceed word length
            int currentLength = _placedClusters.Sum(c => c.Letters.Length);
            if (currentLength + cluster.Letters.Length > 6) // Words are 6 letters
            {
                return false;
            }
            
            _placedClusters.Add(cluster);
            PlacedClusters.Value = new List<LetterClusterItem>(_placedClusters);
            return true;
        }
        
        public bool TryRemoveCluster(int clusterId)
        {
            var cluster = _placedClusters.FirstOrDefault(c => c.Id == clusterId);
            if (cluster == null)
            {
                return false;
            }
            
            _placedClusters.Remove(cluster);
            PlacedClusters.Value = new List<LetterClusterItem>(_placedClusters);
            return true;
        }
        
        public void Clear()
        {
            _placedClusters.Clear();
            PlacedClusters.Value = new List<LetterClusterItem>();
        }
        
        private void UpdateCurrentWord()
        {
            if (_placedClusters.Count == 0)
            {
                CurrentWord.Value = string.Empty;
                IsCompleted.Value = false;
                return;
            }
            
            // Concatenate all cluster letters
            string word = string.Concat(_placedClusters.Select(c => c.Letters));
            CurrentWord.Value = word.ToUpper();
            
            // Check if the current word matches any target word
            bool isCompleted = TargetWords != null && TargetWords.Any(target => target.ToUpper() == CurrentWord.Value);
            IsCompleted.Value = isCompleted;
            
            Log.Gameplay.Info($"WordSlot {Index}: CurrentWord='{CurrentWord.Value}', TargetWords=[{string.Join(",", TargetWords ?? Array.Empty<string>())}], IsCompleted={isCompleted}");
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
            CurrentWord?.Dispose();
            IsCompleted?.Dispose();
            PlacedClusters?.Dispose();
        }
    }
}