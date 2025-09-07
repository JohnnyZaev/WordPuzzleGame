using UnityEngine;
using System.Collections.Generic;

namespace WPG.Runtime.Data
{
    [CreateAssetMenu(fileName = "LevelConfiguration", menuName = "WPG/Level Configuration", order = 0)]
    public class LevelConfiguration : ScriptableObject
    {
        [Header("Level Info")]
        [SerializeField] private int _levelNumber = 1;
        [SerializeField] private string _levelName;
        
        [Header("Words to Assemble")]
        [SerializeField] private WordData[] _targetWords = new WordData[4];
        
        [Header("Letter Clusters")]
        [SerializeField] private string[] _letterClusters;
        
        public int LevelNumber => _levelNumber;
        public string LevelName => _levelName;
        public WordData[] TargetWords => _targetWords;
        public string[] LetterClusters => _letterClusters;
        
        private void OnValidate()
        {
            if (_targetWords != null && _targetWords.Length != 4)
            {
                Debug.LogWarning($"Level '{name}' should have exactly 4 target words. Current count: {_targetWords.Length}");
            }
            
            if (_letterClusters != null && _letterClusters.Length == 0)
            {
                Debug.LogWarning($"Level '{name}' should have at least one letter cluster.");
            }
            
            if (_targetWords != null && _letterClusters != null)
            {
                ValidateClusterLettersMatchWords();
            }
        }
        
        private void ValidateClusterLettersMatchWords()
        {
            var allClusterLetters = new List<char>();
            foreach (var cluster in _letterClusters)
            {
                if (!string.IsNullOrEmpty(cluster))
                {
                    allClusterLetters.AddRange(cluster.ToCharArray());
                }
            }
            
            var allWordLetters = new List<char>();
            foreach (var wordData in _targetWords)
            {
                if (wordData != null && !string.IsNullOrEmpty(wordData.Word))
                {
                    allWordLetters.AddRange(wordData.Word.ToCharArray());
                }
            }
            
            if (allClusterLetters.Count != allWordLetters.Count)
            {
                Debug.LogWarning($"Level '{name}': Total cluster letters ({allClusterLetters.Count}) doesn't match total word letters ({allWordLetters.Count})");
            }
        }
    }
}