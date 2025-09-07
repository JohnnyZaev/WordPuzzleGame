using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using WPG.Runtime.Data;
using WPG.Runtime.Utilities.Logging;

namespace WPG.Runtime.Gameplay
{
    public class GameState : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        
        // Level data
        public LevelData CurrentLevel { get; private set; }
        
        // Game field state - 4 words with 6 letters each
        public readonly WordSlot[] WordSlots = new WordSlot[4];
        
        // Available letter clusters
        public readonly ReactiveProperty<List<LetterClusterItem>> AvailableClusters = new();
        
        // Game status
        public readonly ReactiveProperty<bool> IsLevelCompleted = new();
        public readonly ReactiveProperty<bool> CanPlaceCluster = new(true);
        
        // Completion order tracking
        private readonly List<string> _completedWordsInOrder = new();
        public IReadOnlyList<string> CompletedWordsInOrder => _completedWordsInOrder;
        
        public GameState()
        {
            // Initialize word slots
            for (int i = 0; i < WordSlots.Length; i++)
            {
                WordSlots[i] = new WordSlot(i);
            }
            
            // Subscribe to word completion changes for both tracking and validation
            for (int i = 0; i < WordSlots.Length; i++)
            {
                int slotIndex = i; // Capture for closure
                WordSlots[i].IsCompleted
                    .Subscribe(isCompleted => OnWordCompletionChanged(WordSlots[slotIndex], isCompleted))
                    .AddTo(_disposables);
            }
        }
        
        public void LoadLevel(LevelData levelData)
        {
            CurrentLevel = levelData;
            
            // Clear all word slots first before loading a new level
            foreach (var slot in WordSlots)
            {
                slot.Clear();
            }
            
            // Clear completion order tracking
            _completedWordsInOrder.Clear();
            
            // Set target words for slots - each slot can accept any correct word
            for (int i = 0; i < WordSlots.Length; i++)
            {
                WordSlots[i].SetTargetWords(levelData.targetWords);
            }
            
            // Create available clusters
            var clusters = new List<LetterClusterItem>();
            for (int i = 0; i < levelData.letterClusters.Length; i++)
            {
                clusters.Add(new LetterClusterItem(i, levelData.letterClusters[i]));
            }
            
            AvailableClusters.Value = clusters;
            IsLevelCompleted.Value = false;
        }
        
        public bool TryPlaceCluster(int clusterId, int wordSlotIndex)
        {
            var cluster = AvailableClusters.Value?.FirstOrDefault(c => c.Id == clusterId);
            if (cluster == null || cluster.IsUsed.Value || wordSlotIndex < 0 || wordSlotIndex >= WordSlots.Length)
            {
                return false;
            }
            
            var wordSlot = WordSlots[wordSlotIndex];
            if (wordSlot.TryPlaceCluster(cluster))
            {
                cluster.SetUsed(wordSlotIndex);
                return true;
            }
            
            return false;
        }
        
        public bool TryRemoveCluster(int clusterId)
        {
            var cluster = AvailableClusters.Value?.FirstOrDefault(c => c.Id == clusterId);
            if (cluster == null || !cluster.IsUsed.Value)
            {
                return false;
            }
            
            var wordSlot = WordSlots[cluster.UsedInWordSlot.Value];
            if (wordSlot.TryRemoveCluster(clusterId))
            {
                cluster.SetUnused();
                return true;
            }
            
            return false;
        }
        
        private void OnWordCompletionChanged(WordSlot wordSlot, bool isCompleted)
        {
            string currentWord = wordSlot.CurrentWord.Value;
            
            if (isCompleted)
            {
                // Word became completed - add to completion order if not already there
                if (!string.IsNullOrEmpty(currentWord) && !_completedWordsInOrder.Contains(currentWord))
                {
                    _completedWordsInOrder.Add(currentWord);
                    Log.Gameplay.Info($"Word completed in order: {currentWord} (position {_completedWordsInOrder.Count})");
                }
            }
            else
            {
                // Word became incomplete - remove from completion order
                if (!string.IsNullOrEmpty(currentWord) && _completedWordsInOrder.Contains(currentWord))
                {
                    _completedWordsInOrder.Remove(currentWord);
                    Log.Gameplay.Info($"Word removed from completion order: {currentWord}");
                }
            }
            
            // Validate level completion after any word completion change
            ValidateCompletion();
        }
        
        private void ValidateCompletion()
        {
            Log.Gameplay.Info($"Validating completion for level {CurrentLevel?.levelNumber}");
            
            // Get current words from all slots
            string[] currentWords = WordSlots.Select(slot => slot.CurrentWord.Value).ToArray();
            
            // Debug logging for current state
            Log.Gameplay.Info($"Current words: [{string.Join(", ", currentWords)}]");
            
            if (CurrentLevel?.targetWords != null)
            {
                Log.Gameplay.Info($"Target words: [{string.Join(", ", CurrentLevel.targetWords)}]");
            }
            
            bool allTargetWordsFound = ValidateAllTargetWords(currentWords);
            
            Log.Gameplay.Info($"Validation results - Words found: {allTargetWordsFound}");
            Log.Gameplay.Info($"Completion order list has {_completedWordsInOrder.Count} words: [{string.Join(", ", _completedWordsInOrder)}]");
            
            bool wasCompleted = IsLevelCompleted.Value;
            bool isCompleted = allTargetWordsFound;
            
            Log.Gameplay.Info($"Level completion check - Was completed: {wasCompleted}, Is completed: {isCompleted}");
            
            if (isCompleted != wasCompleted)
            {
                IsLevelCompleted.Value = isCompleted;
                Log.Gameplay.Info(isCompleted
                    ? $"Level completed! All target words found."
                    : "Level completion status changed to incomplete");
            }
        }

        private bool ValidateAllTargetWords(string[] currentWords)
        {
            if (CurrentLevel?.targetWords == null || currentWords == null)
                return false;
            
            var targetWordsList = CurrentLevel.targetWords.ToList();
            var currentWordsList = currentWords.Where(w => !string.IsNullOrEmpty(w)).ToList();
            
            // Check if all target words are found in current words (regardless of position)
            foreach (var targetWord in targetWordsList)
            {
                if (!currentWordsList.Contains(targetWord.ToUpper()))
                    return false;
            }
            
            // Check if we have the correct number of words
            return currentWordsList.Count == targetWordsList.Count;
        }
        
        public void ResetLevel()
        {
            foreach (var slot in WordSlots)
            {
                slot.Clear();
            }
            
            // Clear completion order tracking
            _completedWordsInOrder.Clear();
            
            if (AvailableClusters.Value != null)
            {
                foreach (var cluster in AvailableClusters.Value)
                {
                    cluster.SetUnused();
                }
            }
            
            IsLevelCompleted.Value = false;
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
            
            foreach (var slot in WordSlots)
            {
                slot?.Dispose();
            }
            
            if (AvailableClusters.Value != null)
            {
                foreach (var cluster in AvailableClusters.Value)
                {
                    cluster?.Dispose();
                }
            }
            
            AvailableClusters?.Dispose();
            IsLevelCompleted?.Dispose();
            CanPlaceCluster?.Dispose();
        }
    }
}