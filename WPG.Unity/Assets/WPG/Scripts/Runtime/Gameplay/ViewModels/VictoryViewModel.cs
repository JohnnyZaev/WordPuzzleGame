using System;
using System.Collections.Generic;
using R3;

namespace WPG.Runtime.Gameplay.ViewModels
{
    public class VictoryViewModel : IDisposable
    {
        // Public reactive properties for UI binding
        public readonly ReactiveProperty<List<string>> CompletedWords = new();
        public readonly ReactiveProperty<int> CurrentLevel = new(1);
        public readonly ReactiveProperty<bool> HasNextLevel = new(true);
        
        // Commands
        public readonly ReactiveCommand<Unit> MainMenuCommand = new();
        public readonly ReactiveCommand<Unit> NextLevelCommand = new();
        
        public void SetVictoryData(List<string> completedWords, int currentLevel, bool hasNextLevel)
        {
            CompletedWords.Value = completedWords ?? new List<string>();
            CurrentLevel.Value = currentLevel;
            HasNextLevel.Value = hasNextLevel;
        }
        
        public void Dispose()
        {
            CompletedWords?.Dispose();
            CurrentLevel?.Dispose();
            HasNextLevel?.Dispose();
            MainMenuCommand?.Dispose();
            NextLevelCommand?.Dispose();
        }
    }
}