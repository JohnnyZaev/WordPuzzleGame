using System;
using System.Collections.Generic;
using R3;

namespace WPG.Runtime.Gameplay.ViewModels
{
    public class WordSlotViewModel : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly WordSlot _wordSlot;
        
        // Public reactive properties for UI binding
        public readonly ReactiveProperty<string> CurrentWord = new(string.Empty);
        public readonly ReactiveProperty<bool> IsCompleted = new();
        public readonly ReactiveProperty<List<LetterClusterItem>> PlacedClusters = new();
        
        // UI-specific properties
        public readonly ReactiveProperty<string> DisplayText = new(string.Empty);
        public readonly ReactiveProperty<float> CompletionProgress = new();
        
        public int Index => _wordSlot.Index;
        
        public WordSlotViewModel(WordSlot wordSlot)
        {
            _wordSlot = wordSlot;
            
            // Bind to word slot properties
            _wordSlot.CurrentWord
                .Subscribe(word => 
                {
                    CurrentWord.Value = word;
                    UpdateDisplayText();
                })
                .AddTo(_disposables);
                
            _wordSlot.IsCompleted
                .Subscribe(completed => IsCompleted.Value = completed)
                .AddTo(_disposables);
                
            _wordSlot.PlacedClusters
                .Subscribe(clusters => PlacedClusters.Value = clusters)
                .AddTo(_disposables);
            
            // Subscribe to completion changes to update progress
            IsCompleted
                .Subscribe(completed => CompletionProgress.Value = completed ? 1.0f : 0.0f)
                .AddTo(_disposables);
                
            UpdateDisplayText();
        }
        
        private void UpdateDisplayText()
        {
            var currentWord = CurrentWord.Value;
            if (string.IsNullOrEmpty(currentWord))
            {
                // Show empty slots for the target word length (6 letters)
                DisplayText.Value = "_ _ _ _ _ _";
            }
            else if (currentWord.Length < 6)
            {
                // Show current letters and empty slots for remaining
                var letters = currentWord.ToCharArray();
                var display = string.Empty;
                
                for (int i = 0; i < 6; i++)
                {
                    if (i < letters.Length)
                    {
                        display += letters[i];
                    }
                    else
                    {
                        display += "_";
                    }
                    
                    if (i < 5) display += " ";
                }
                
                DisplayText.Value = display;
            }
            else
            {
                // Show complete word with spaces
                var letters = currentWord.ToCharArray();
                DisplayText.Value = string.Join(" ", letters);
            }
        }
        
        public bool CanAcceptCluster(LetterClusterItem cluster)
        {
            if (cluster == null || cluster.IsUsed.Value)
            {
                return false;
            }
            
            // Check if adding this cluster would exceed word length
            int currentLength = CurrentWord.Value?.Length ?? 0;
            return currentLength + cluster.Letters.Length <= 6;
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
            CurrentWord?.Dispose();
            IsCompleted?.Dispose();
            PlacedClusters?.Dispose();
            DisplayText?.Dispose();
            CompletionProgress?.Dispose();
        }
    }
}