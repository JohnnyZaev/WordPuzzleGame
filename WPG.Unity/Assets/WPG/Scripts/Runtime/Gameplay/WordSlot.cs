using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using WPG.Runtime.Utilities.Logging;

namespace WPG.Runtime.Gameplay
{
    public class WordSlot : IDisposable
    {
        private readonly GameState _gameState;
        
        public int Index { get; }
        private string[] TargetWords { get; set; } = Array.Empty<string>();
        
        public readonly ReactiveProperty<string> CurrentWord = new(string.Empty);
        public readonly ReactiveProperty<bool> IsCompleted = new();
        
        public WordSlot(int index, GameState gameState)
        {
            Index = index;
            _gameState = gameState;
        }
        
        public void SetTargetWords(string[] targetWords)
        {
            TargetWords = targetWords ?? Array.Empty<string>();
            UpdateCurrentWord();
        }
        
        public IEnumerable<LetterClusterItem> GetPlacedClusters()
        {
            if (_gameState?.AvailableClusters.Value == null) return Enumerable.Empty<LetterClusterItem>();
            
            return _gameState.AvailableClusters.Value
                .Where(c => c.IsUsed.Value && c.UsedInWordSlot.Value == Index)
                .OrderBy(c => c.Id);
        }
        
        public bool TryPlaceCluster(LetterClusterItem cluster)
        {
            if (cluster == null || cluster.IsUsed.Value)
            {
                return false;
            }
            
            var placedClusters = GetPlacedClusters().ToList();
            int currentLength = placedClusters.Sum(c => c.Letters.Length);
            if (currentLength + cluster.Letters.Length > 6) // Words are 6 letters
            {
                return false;
            }
            
            cluster.SetUsed(Index);
            UpdateCurrentWord();
            return true;
        }
        
        public bool TryRemoveCluster(int clusterId)
        {
            var cluster = GetPlacedClusters().FirstOrDefault(c => c.Id == clusterId);
            if (cluster == null)
            {
                return false;
            }
            
            cluster.SetUnused();
            UpdateCurrentWord();
            return true;
        }
        
        public void Clear()
        {
            var placedClusters = GetPlacedClusters().ToList();
            foreach (var cluster in placedClusters)
            {
                cluster.SetUnused();
            }
            UpdateCurrentWord();
        }
        
        private void UpdateCurrentWord()
        {
            var placedClusters = GetPlacedClusters().ToList();
            if (placedClusters.Count == 0)
            {
                CurrentWord.Value = string.Empty;
                IsCompleted.Value = false;
                return;
            }
            
            string word = string.Concat(placedClusters.Select(c => c.Letters));
            CurrentWord.Value = word.ToUpper();
            
            bool isCompleted = TargetWords != null && TargetWords.Any(target => target.ToUpper() == CurrentWord.Value);
            IsCompleted.Value = isCompleted;
            
            Log.Gameplay.Info($"WordSlot {Index}: CurrentWord='{CurrentWord.Value}', TargetWords=[{string.Join(",", TargetWords ?? Array.Empty<string>())}], IsCompleted={isCompleted}");
        }
        
        public void Dispose()
        {
            CurrentWord?.Dispose();
            IsCompleted?.Dispose();
        }
    }
}